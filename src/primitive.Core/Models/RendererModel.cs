using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace primitive.Core
{
    public class RendererModel
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Nprimitives { get; set; }
        public ShapeType Mode { get; set; }
        public byte Alpha { get; set; }
        public int Repeat { get; set; }
        public double Scale { get; set; }
        public Rgba32 Background { get; set; }
        public int WorkersCount { get; set; }
        public Image<Rgba32> Input { get; set; }
        public Image<Rgba32> Current { get; set; }
        public Image<Rgba32> Result;
        public double Score { get; set; }
        private List<IShape> Shapes { get; set; }
        private List<Rgba32> Colors { get; set; }
        private List<double> Scores { get; set; }
        private List<Worker> Workers { get; set; }

        public RendererModel(Image<Rgba32> input, ParametersModel parameters)
        {
            Resize(input, parameters.CanvasResize);
            double aspect = input.Width / (double)input.Height;
            if (aspect >= 1)
            {
                Width = parameters.RenderSize;
                Height = (int)(parameters.RenderSize / aspect);
                Scale = parameters.RenderSize / (double)input.Width;
            }
            else
            {
                Width = (int)(parameters.RenderSize * aspect);
                Height = parameters.RenderSize;
                Scale = parameters.RenderSize / (double)input.Height;
            }
            Nprimitives = parameters.Nprimitives;
            Mode = parameters.Mode;
            Alpha = parameters.Alpha;
            Repeat = parameters.Repeat;            
            Background = parameters.Background;
            WorkersCount = parameters.WorkersCount;
            Input = input;//.Clone();
            Current = Util.UniformRgba(input.Width, input.Height, Background);
            Result = Util.UniformRgba(Width, Height, Background);

            Score = Core.DifferenceFull(Input, Current);
            Shapes = new List<IShape>();
            Colors = new List<Rgba32>();
            Scores = new List<double>();
            Workers = new List<Worker>();

            for (int i = 0; i < WorkersCount; i++)
            {
                var worker = new Worker(Input);
                Workers.Add(worker);
            }
        }

        // run algorithm
        public void RunRenderer()
        {
            Logger.WriteLine(1, "{0}: t={1:G3}, score={2:G6}", 0, 0.0, Score);
            var start = DateTime.Now;
            int frame = 0;

            Logger.WriteLine(1, "count={0}, mode={1}, alpha={2}, repeat={3}", Nprimitives, Mode, Alpha, Repeat);
            for (int i = 0; i < Nprimitives; i++)
            {
                frame++;
                // find optimal shape and add it to the model
                var t = DateTime.Now;
                var n = Step(Mode, Alpha, Repeat);
                var nps = Util.NumberString((double)n / (DateTime.Now - t).TotalSeconds);
                var elapsed = (DateTime.Now - start).TotalSeconds;
                Logger.WriteLine(1, "{0:00}: t={1:G3}, score={2:G6}, n={3}, n/s={4}", frame, elapsed, Score, n, nps);
            }
        }

        public Image<Rgba32> GetFrames(bool saveFrames, int Nth = 1)
        {
            if (!saveFrames)
                return Result;
            Image<Rgba32> im = Util.UniformRgba(Width, Height, Background);
            Image<Rgba32> result = Util.UniformRgba(Width, Height, Background);
            for (int i = 0; i < Shapes.Count; i++)
            {
                Rgba32 c = Colors[i];
                Shapes[i].Draw(im, c, Scale);
                if (i % Nth == 0)
                    result.Frames.AddFrame(im.Frames[0]);
            }
            return result;
        }

        public Image<Rgba32> GetFrames(double scoreDelta)
        {
            Image<Rgba32> im = Util.UniformRgba(Width, Height, Background);
            Image<Rgba32> result = Util.UniformRgba(Width, Height, Background);
            result.Frames.AddFrame(im.Frames[0]);
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
                    result.Frames.AddFrame(im.Frames[0]);
                }
            }
            return result;
        }

        public List<string> GetSVG(bool saveFrames, int Nth = 1)
        {
            List<string> result = new List<string>();
            int frameNum = 0;
            if (!saveFrames)
                frameNum = Shapes.Count - 1;
            for (; frameNum < Shapes.Count; frameNum += Nth)
            {
                Rgba32 bg = Background;
                var fillA = Colors[0].A;
                var vw = Width / (int)Scale;
                var vh = Height / (int)Scale;
                List<string> lines = new List<string>();
                lines.Add(String.Format(
                    $"<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" viewBox=\"0 0 {vw} {vh}\">"));
                lines.Add(String.Format(
                    $"<rect width=\"100%\" height=\"100%\" fill=\"#{bg.R:X2}{bg.G:X2}{bg.B:X2}\" />"));
                lines.Add(String.Format($"<g fill-opacity=\"{(double)fillA / 255}\">"));
                for (int i = 0; i < frameNum; i++)
                {
                    List<string> attrs = new List<string>();
                    Rgba32 c = Colors[i];
                    attrs.Add(String.Format($"fill=\"#{c.R:X2}{c.G:X2}{c.B:X2}\""));
                    if (c.A != fillA)
                    {
                        attrs.Add(String.Format($"fill-opacity=\"{(double)c.A / 255}\""));
                    }
                    lines.Add(Shapes[i].SVG(String.Join(" ", attrs)));
                }
                lines.Add("</g>");
                lines.Add("</svg>");
                lines.Add("\n");
                result.Add(String.Join("\n", lines));
            }
            return result;
        }

        private void Add(IShape shape, int alpha)
        {
            var before = Current.Clone();
            var lines = shape.Rasterize();
            var color = Core.ComputeColor(Input, Current, lines, alpha);
            Core.DrawLines(Current, color, lines);
            var score = Core.DifferencePartial(Input, before, Current, Score, lines);

            Score = score;
            Shapes.Add(shape);
            Colors.Add(color);
            Scores.Add(score);

            shape.Draw(Result, color, Scale);
        }

        private int Step(ShapeType shapeType, int alpha, int repeat)
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
            //foreach (var parameter in parameters)
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

        private void Resize(Image<Rgba32> image, int canvasSize)
        {
            int width, height;
            if (canvasSize >= image.Width && canvasSize >= image.Height)
                return;
            if (image.Width > image.Height)
            {
                width = canvasSize;
                height = Convert.ToInt32(image.Height * canvasSize / (double)image.Width);
            }
            else
            {
                width = Convert.ToInt32(image.Width * canvasSize / (double)image.Height);
                height = canvasSize;
            }
            image.Mutate(im => im.Resize(width, height));
        }
    }
}
