using System;

namespace primitive.Core
{
    public static class Optimize
    {
        public static IAnnealable HillClimb(IAnnealable state, int maxAge)
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
