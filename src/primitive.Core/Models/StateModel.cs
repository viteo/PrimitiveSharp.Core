namespace primitive.Core
{
    public class StateModel
    {
        public WorkerModel Worker { get; set; }
        public IShape Shape { get; set; }
        public int Alpha { get; set; }
        public bool MutateAlpha { get; set; }
        public double Score { get; set; }

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
            Score = score;
        }

        public double Energy()
        {
            if (Score < 0)
            {
                Score = Worker.Energy(Shape, Alpha);
            }
            return Score;
        }

        public object DoMove()
        {
            var rnd = Worker.Rnd;
            var oldState = Copy();
            Shape.Mutate();
            if (MutateAlpha)
            {
                Alpha = (Alpha + rnd.Next(21) - 10).Clamp(1, 255);
            }
            Score = -1;
            return oldState;
        }

        public void UndoMove(object undo)
        {
            if (undo is StateModel oldState)
            {
                Shape = oldState.Shape;
                Alpha = oldState.Alpha;
                Score = oldState.Score;
            }
        }

        public StateModel Copy()
        {
            return new StateModel(Worker, Shape.Copy(), Alpha, MutateAlpha, Score);
        }

        public static StateModel HillClimb(StateModel state, int maxAge)
        {
            state = state.Copy();
            var bestState = state.Copy();
            var bestEnergy = state.Energy();
            int step = 0;
            for (int age = 0; age < maxAge; age++)
            {
                var undo = state.DoMove();
                var energy = state.Energy();
                if (energy >= bestEnergy)
                {
                    state.UndoMove(undo);
                }
                else
                {
                    //Console.WriteLine("step: {0}, energy: {1:G6}", step, energy);
                    bestEnergy = energy;
                    bestState = state.Copy();
                    age = -1;
                }
                step++;
            }
            return bestState;
        }
    }
}
