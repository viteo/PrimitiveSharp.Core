namespace primitive.Core
{
    public interface IAnnealable
    {
        double Energy();
        object DoMove();
        void UndoMove(object state);
        IAnnealable Copy();
    }

    public class State : IAnnealable
    {
        public Worker Worker { get; set; }
        public IShape Shape { get; set; }
        public int Alpha { get; set; }
        public bool MutateAlpha { get; set; }
        public double Score { get; set; }

        public State(){}

        public State(Worker worker, IShape shape, int alpha, bool mutateAlpha = false, double score = -1)
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
                Alpha = Util.Clamp(Alpha + rnd.Next(21) - 10, 1, 255);
            }
            Score = -1;
            return oldState;
        }

        public void UndoMove(object undo)
        {
            if (undo is State oldState)
            {
                Shape = oldState.Shape;
                Alpha = oldState.Alpha;
                Score = oldState.Score;
            }
        }

        public IAnnealable Copy()
        {
            return new State(Worker,Shape.Copy(),Alpha,MutateAlpha,Score);
        }
    }
}
