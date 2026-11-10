using System;
using System.Linq;
using Godot;
using Simulation.Data;
using Simulation.Scenes;

namespace Simulation.Util;


public static class PopulationEvolutionist
{
    private static readonly Random _random = new();

    public static Genome[] GetStartGenomes(int population, string loadFile)
    {
        var gens = new Genome[population];
        if (loadFile != "")
        {
            gens[0] = new();
            bool hasSavedBrain = SaveManager.LoadBrain(gens[0].Brain, loadFile);

            if (hasSavedBrain)
            {
                for (int i = 1; i < population; i++)
                {
                    gens[i] = gens[0].Clone();
                    gens[i].Mutate(0.2);
                }
            }
        }
        return gens;
    }

    public static Genome[] EvolvePopulation(Agent[] population, double mutationRate)
    {
        var result = new Genome[population.Length];

        int eliteLength = Mathf.Min(3, population.Length);
        for (var i = 0; i < eliteLength; i++)
        {
            result[i] = population[i].GetGenome();
        }
        
        for (var i = eliteLength; i < result.Length; i++)
        {
            var p1 = SelectParent(population).GetGenome();
            var p2 = SelectParent(population).GetGenome();

            var child = p1.BlendCrossover(p2);
            child.Mutate(mutationRate);
            result[i] = child;
        }
        return result;
    }

    private static Agent SelectParent(Agent[] population, float topParentsPct = 0.3f)
    {
        Agent[] a = new Agent[3];
        int topParents = (int)(population.Length * topParentsPct);

        for (var i = 0; i < a.Length; i++)
        {
            a[i] = population[_random.Next(topParents)];
        }

        return a.MaxBy(a => a.Scoring.GetFitness());
    }

}
