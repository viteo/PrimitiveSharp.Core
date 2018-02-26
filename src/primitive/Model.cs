using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.Primitives;

namespace primitive
{
    public class Model
    {
        public int Sw { get; set; }
        public int Sh { get; set; }
        public double Scale { get; set; }
        public Rgba32 Background { get; set; }
        public Image<Rgba32> Target { get; set; }
        public Image<Rgba32> Current { get; set; }
        public Image<Rgba32> Result;
        public double Score { get; set; }
        public List<IShape> Shapes { get; set; }
        public List<Rgba32> Colors { get; set; }
        public List<double> Scores { get; set; }
        public List<Worker> Workers { get; set; }

        public Model(Image<Rgba32> target, Rgba32 background, int outSize, int numWorkers)
        {
            var w = target.Width;
            var h = target.Height;
            double aspect = (double)w / (double)h;
            int sw = 0, sh = 0;
            double scale = 0;
            if (aspect >= 1)
            {
                sw = outSize;
                sh = (int)((double)outSize / aspect);
                scale = (double)outSize / (double)w;
            }
            else
            {
                sw = (int)((double)outSize * aspect);
                sh = outSize;
                scale = (double)outSize / (double)h;
            }

            Sw = sw;
            Sh = sh;
            Scale = scale;
            Background = background;
            Target = target;//.Clone();
            Current = Util.UniformRgba(target.Width, target.Height, background);

            Matrix3x2 scalematrix = Matrix3x2.CreateScale((float)Scale);
            Matrix3x2 translatematrix = Matrix3x2.CreateTranslation(0.5f, 0.5f);
            Result = new Image<Rgba32>(Sw, Sh);
            Result.Mutate(r => r.Transform(scalematrix*translatematrix));

            Score = Core.DifferenceFull(Target, Current);
            Shapes = new List<IShape>();
            Colors = new List<Rgba32>();
            Scores = new List<double>();
            Workers = new List<Worker>();

            for (int i = 0; i < numWorkers; i++)
            {
                var worker = new Worker(Target);
                Workers.Add(worker);
            }
        }

        public List<Image<Rgba32>> Frames(double scoreDelta)
        {
            List<Image<Rgba32>> result = new List<Image<Rgba32>>();
            Image<Rgba32> im = Util.UniformRgba(Sw, Sh, Background);
            result.Add(im.Clone());
            double previous = 10;
            for (int i = 0; i < Shapes.Count; i++)
            {
                Rgba32 c = Colors[i];
                Shapes[i].Draw(im, c, Scale);
                var score = Scores[i];
                var delta = previous - score;
                if (delta >= scoreDelta)
                {
                    previous = score;
                    result.Add(im.Clone());
                }

            }
            return result;
        }

        public string SVG()
        {
            Rgba32 bg = Background;
            List<string> lines = new List<string>();
            lines.Add(String.Format("<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" width=\"{0}\" height=\"{1}\">", Sw, Sh));
            lines.Add(String.Format("<rect x=\"0\" y=\"0\" width=\"{0}\" height=\"{1}\" fill=\"#{2:X}{3:X}{4:X}\" />", Sw, Sh, bg.R, bg.G, bg.B));
            lines.Add(String.Format("<g transform=\"scale({0}) translate(0.5 0.5)\">", Scale));
            for (int i = 0; i < Shapes.Count; i++)
            {
                Rgba32 c = Colors[i];
                string attrs = "fill=\"#{0:X}{1:X}{2:X}\" fill-opacity=\"{3}\"";
                attrs = String.Format(attrs, c.R, c.G, c.B, (double)c.A / 255);
                lines.Add(Shapes[i].SVG(attrs));
            }
            lines.Add("</g>");
            lines.Add("</svg>");
            lines.Add("\n");
            return String.Join("\n", lines);
        }

        public void Add(IShape shape, int alpha)
        {
            var before = Current.Clone();
            var lines = shape.Rasterize();
            var color = Core.ComputeColor(Target, Current, lines, alpha);
            Core.DrawLines(Current, color, lines);
            var score = Core.DifferencePartial(Target, before, Current, Score, lines);

            Score = score;
            Shapes.Add(shape);
            Colors.Add(color);
            Scores.Add(score);

            shape.Draw(Result, color, Scale);
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

            var parameters = new List<(Worker, ShapeType, int, int, int, int)>();
            var results = new List<State>();
            var tasks = new List<Task>();
            for (int i = 0; i < wn; i++)
            {
                var worker = Workers[i];
                worker.Init(Current, Score);
                parameters.Add((worker, t, a, n, age, wm));
            }

            #region Sequential
            //foreach(var parameter in parameters)
            //{
            //    results.Add(runWorker(parameter.Item1, parameter.Item2, parameter.Item3, parameter.Item4, parameter.Item5, parameter.Item6));
            //}
            #endregion

            #region Parallel
            parameters.ForEach((parameter) =>
            {
                var task = Task.Factory.StartNew(() => { return runWorker(parameter.Item1, parameter.Item2, parameter.Item3, parameter.Item4, parameter.Item5, parameter.Item6); })
                .ContinueWith((result) =>
                {
                    results.Add(result.Result);

                });
                tasks.Add(task);
            });
            Task.WaitAll(tasks.ToArray());
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
