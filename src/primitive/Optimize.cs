using System;
using System.Collections.Generic;

namespace primitive
{
    public interface IAnnealable
    {
        double Energy();
        object DoMove();
        void UndoMove(object state);
        IAnnealable Copy();
    }

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

        public static double PreAnneal(IAnnealable state, int iterations)
        {
            state = state.Copy();
            var previous = state.Energy();
            double total = 0;
            for (int i = 0; i < iterations; i++)
            {
                state.DoMove();
                var energy = state.Energy();
                total += Math.Abs(energy - previous);
                previous = energy;
            }
            return total / (double) iterations;
        }

        public static IAnnealable Anneal(IAnnealable state, double maxTemp, double minTemp, int steps)
        {
            var factor = -Math.Log(maxTemp / minTemp);
            state = state.Copy();
            var bestState = state.Copy();
            var bestEnergy = state.Energy();
            var previousEnergy = bestEnergy;
            for (int step = 0; step < steps; step++)
            {
                var pct = (double) step / (double) (steps - 1);
                var temp = maxTemp * Math.Exp(factor * pct);
                var undo = state.DoMove();
                var energy = state.Energy();
                var change = energy - previousEnergy;
                if(change > 0 && Math.Exp(-change/temp) < PrimitiveCS.Rand.NextDouble())
                    state.UndoMove(undo);
                else
                {
                    previousEnergy = energy;
                    if (energy < bestEnergy)
                    {
                        //Console.WriteLine("step: {0} of {1} {2}%, temp: {3:G3}, energy: {4:G6}",step,steps,pct,temp,energy);
                        bestEnergy = energy;
                        bestState = state.Copy();
                    }
                }
            }
            return bestState;
        }
    }
}
