using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace primitive.Core
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
            Result = Util.UniformRgba(Sw, Sh, Background);

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

        // run algorithm
        public void RunModel()
        {
            Logger.WriteLine(1, "{0}: t={1:G3}, score={2:G6}", 0, 0.0, Score);
            var start = DateTime.Now;
            int frame = 0;

            Logger.WriteLine(1, "count={0}, mode={1}, alpha={2}, repeat={3}", Parameters.Nprimitives, Parameters.Mode, Parameters.Alpha, Parameters.Repeat);
            for (int i = 0; i < Parameters.Nprimitives; i++)
            {
                frame++;
                // find optimal shape and add it to the model
                var t = DateTime.Now;
                var n = Step((ShapeType)Parameters.Mode, Parameters.Alpha, Parameters.Repeat);
                var nps = Util.NumberString((double)n / (DateTime.Now - t).TotalSeconds);
                var elapsed = (DateTime.Now - start).TotalSeconds;
                Logger.WriteLine(1, "{0:00}: t={1:G3}, score={2:G6}, n={3}, n/s={4}", frame, elapsed, Score, n, nps);
            }
        }

        public List<Image<Rgba32>> GetFrames(bool saveFrames, int Nth = 1)
        {
            if (!saveFrames)
                return new List<Image<Rgba32>> { Result };
            Image<Rgba32> im = Util.UniformRgba(Sw, Sh, Background);
            var result = new List<Image<Rgba32>>();
            for (int i = 0; i < Shapes.Count; i++)
            {
                if (i % Nth != 0)
                    continue;
                Rgba32 c = Colors[i];
                Shapes[i].Draw(im, c, Scale);
                result.Add(im.Clone());
            }
            return result;
        }

        public Image<Rgba32> GetFrames(double scoreDelta)
        {
            Image<Rgba32> im = Util.UniformRgba(Sw, Sh, Background);
            Image<Rgba32> result = Util.UniformRgba(Sw, Sh, Background);
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
            int frame = 0;
            if (!saveFrames)
                frame = Shapes.Count - 1;
            for (; frame < Shapes.Count; frame += Nth)
            {
                Rgba32 bg = Background;
                var fillA = Colors[0].A;
                var vw = Sw / (int)Scale;
                var vh = Sh / (int)Scale;
                List<string> lines = new List<string>();
                lines.Add(String.Format(
                    $"<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" viewBox=\"0 0 {vw} {vh}\">"));
                lines.Add(String.Format(
                    $"<rect width=\"100%\" height=\"100%\" fill=\"#{bg.R:X2}{bg.G:X2}{bg.B:X2}\" />"));
                lines.Add(String.Format($"<g fill-opacity=\"{(double)fillA / 255}\">"));
                for (int i = 0; i < frame; i++)
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
