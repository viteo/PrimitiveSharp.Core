using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace primitive
{
    public class Model
    {
        public int Sw { get; set; }
        public int Sh { get; set; }
        public double Scale { get; set; }
        public Color Background { get; set; }
        public Bitmap Target { get; set; }
        public Bitmap Current { get; set; }
        public Bitmap ContextImage { get; set; }
        public Graphics Context { get; set; }
        public SolidBrush Brush { get; set; }
        public double Score { get; set; }
        public List<IShape> Shapes { get; set; }
        public List<Color> Colors { get; set; }
        public List<double> Scores { get; set; }
        public List<Worker> Workers { get; set; }

        public Model(Image target, Color background, int size, int numWorkers)
        {
            var w = target.Width;
            var h = target.Height;
            double aspect = (double)w / (double)h;
            int sw = 0, sh = 0;
            double scale = 0;
            if (aspect >= 1)
            {
                sw = size;
                sh = (int)((double)size / aspect);
                scale = (double)size / (double)w;
            }
            else
            {
                sw = (int)((double)size * aspect);
                sh = size;
                scale = (double)size / (double)h;
            }

            Sw = sw;
            Sh = sh;
            Scale = scale;
            Background = background;
            Target = Util.ImageToRgba(target);
            Current = Util.UniformRgba(target.Width, target.Height, background);
            Score = Core.DifferenceFull(Target, Current);
            ContextImage = new Bitmap(Sw, Sh);
            (Context, Brush) = NewContext(ContextImage);

            Shapes = new List<IShape>();
            Colors = new List<Color>();
            Scores = new List<double>();
            Workers = new List<Worker>();

            for (int i = 0; i < numWorkers; i++)
            {
                var worker = new Worker(Target);
                Workers.Add(worker);
            }
        }

        private (Graphics, SolidBrush) NewContext(Bitmap im)
        {
            Graphics context = Graphics.FromImage(im);
            context.ScaleTransform((float)Scale, (float)Scale);
            context.TranslateTransform(0.5f, 0.5f);
            SolidBrush brush = new SolidBrush(Background);
            context.FillRectangle(brush, 0, 0, im.Width, im.Height);
            return (context, brush);
        }

        public List<Bitmap> Frames(double scoreDelta)
        {
            List<Bitmap> result = new List<Bitmap>();
            Bitmap im = new Bitmap(Sw, Sh);
            Graphics dc;
            SolidBrush dcBrush;
            (dc, dcBrush) = NewContext(im);
            result.Add(Util.ImageToRgba(im));
            double previous = 10;
            for (int i = 0; i < Shapes.Count; i++)
            {
                Color c = Colors[i];
                dcBrush.Color = c;
                Shapes[i].Draw(dc, dcBrush, Scale);
                var score = Scores[i];
                var delta = previous - score;
                if (delta >= scoreDelta)
                {
                    previous = score;
                    result.Add(Util.ImageToRgba(im));
                }

            }
            return result;
        }

        public string SVG()
        {
            Color bg = Background;
            List<string> lines = new List<string>();
            lines.Add(String.Format("<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" width=\"{0}\" height=\"{1}\">", Sw, Sh));
            lines.Add(String.Format("<rect x=\"0\" y=\"0\" width=\"{0}\" height=\"{1}\" fill=\"#{2:X}{3:X}{4:X}\" />", Sw, Sh, bg.R, bg.G, bg.B));
            lines.Add(String.Format("<g transform=\"scale({0}) translate(0.5 0.5)\">", Scale));
            for (int i = 0; i < Shapes.Count; i++)
            {
                Color c = Colors[i];
                string attrs = "fill=\"#{0:X}{1:X}{2:X}\" fill-opacity=\"{3}\"";
                attrs = String.Format(attrs, c.R, c.G, c.B, (double)c.A / 255);
                lines.Add(Shapes[i].SVG(attrs));
            }
            lines.Add("</g>");
            lines.Add("</svg>");
            lines.Add("\n");
            return String.Join('\n', lines);
        }

        public void Add(IShape shape, int alpha)
        {
            var before = Util.CopyRgba(Current);
            var lines = shape.Rasterize();
            var color = Core.ComputeColor(Target, Current, lines, alpha);
            Core.DrawLines(Current, color, lines);
            var score = Core.DifferencePartial(Target, before, Current, Score, lines);

            Score = score;
            Shapes.Add(shape);
            Colors.Add(color);
            Scores.Add(score);

            Brush.Color = color;
            shape.Draw(Context, Brush, Scale);
        }

        public int Step(ShapeType shapeType, int alpha, int repeat)
        {
            var state = runWorkers(shapeType, alpha, 1000, 100, 16);
            //state = Optimize.HillClimb(state, 1000) as State;
            Add(state.Shape, state.Alpha);

            for (int i = 0; i < repeat; i++)
            {
                state.Worker.Init(Current, Score);
                var a = state.Energy();
                state = Optimize.HillClimb(state, 100) as State;
                var b = state.Energy();
                if (a == b)
                    break;
                Add(state.Shape, state.Alpha);
            }

            //foreach (var w in Workers)
            //    Workers[0].Heatmap.AddHeatmap(w.Heatmap);
            //Util.SavePNG("heatmap.png", Workers[0].Heatmap.Image(0.5));

            var counter = 0;
            foreach (var worker in Workers)
                counter += worker.Counter;
            return counter;
        }

        private State runWorker(Worker worker, ShapeType t, int a, int n, int age, int m)
        {
            return worker.BestHillClimbState(t, a, n, age, m);
        }

        private State runWorkers(ShapeType t, int a, int n, int age, int m)
        {
            var wn = Workers.Count;
            var ch = new List<State>(wn);
            var wm = m / wn;
            if (m % wn != 0)
                wm++;

            var parameters = new List<(Worker,ShapeType,int,int,int,int)>();
            var results = new List<State>();
            var tasks = new List<Task>();
            for (int i = 0; i < wn; i++)
            {
                var worker = Workers[i];
                worker.Init(Current, Score);
                parameters.Add((worker, t, a, n, age, wm));
            }

            #region Sequential
            foreach(var parameter in parameters)
            {
                results.Add(runWorker(parameter.Item1, parameter.Item2, parameter.Item3, parameter.Item4, parameter.Item5, parameter.Item6));
            }
            #endregion

            #region Parallel
            //parameters.ForEach((parameter) =>
            //{
            //    var task = Task.Factory.StartNew(() => { return runWorker(parameter.Item1,parameter.Item2,parameter.Item3,parameter.Item4,parameter.Item5,parameter.Item6); })
            //    .ContinueWith((result) => 
            //    {
            //        results.Add(result.Result);

            //    });
            //    tasks.Add(task);
            //});
            //Task.WaitAll(tasks.ToArray());
            #endregion

            double bestEnergy = double.MaxValue;
            State bestState = new State();
            foreach (var res in results)
            {
                double energy = res.Energy();
                if (energy < bestEnergy)
                {
                    bestEnergy = energy;
                    bestState = res;
                }
            }
            return bestState;
        }
    }
}
