namespace primitive.Core
{
    public class StateModel
    {
        public WorkerModel Worker { get; set; }
        public IShape Shape { get; set; }
        public int Alpha { get; set; }
        public bool MutateAlpha { get; set; }

        private double score;
        public double Score
        {
            get
            {
                if (score < 0)
                {
                    score = Worker.GetScore(Shape, Alpha);
                }
                return score;
            }
        }

        public StateModel() { }

        public StateModel(WorkerModel worker, IShape shape, int alpha, bool mutateAlpha = false, double score = -1)
        {
            Worker = worker;
            Shape = shape;
            if (alpha == 0)
            {
                alpha = 128;
                mutateAlpha = true;
            }
            Alpha = alpha;
            MutateAlpha = mutateAlpha;
            this.score = score;
        }

        public StateModel DoMove()
        {
            var rnd = Worker.Rnd;
            var oldState = Copy();
            Shape.Mutate();
            if (MutateAlpha)
            {
                Alpha = (Alpha + rnd.Next(21) - 10).Clamp(1, 255);
            }
            score = -1;
            return oldState;
        }

        public void UndoMove(StateModel undo)
        {
            Shape = undo.Shape;
            Alpha = undo.Alpha;
            score = undo.Score;
        }

        public StateModel Copy()
        {
            return new StateModel(Worker, Shape.Copy(), Alpha, MutateAlpha, Score);
        }

        public void HillClimb(int maxAge)
        {
            var bestScore = Score;
            int step = 0;
            for (int age = 0; age < maxAge; age++)
            {
                var undo = DoMove();
                var score = Score;
                if (score >= bestScore)
                {
                    UndoMove(undo);
                }
                else
                {
                    bestScore = score;
                    age = -1;
                }
                step++;
            }
        }
    }
}
