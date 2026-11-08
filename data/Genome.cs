using System;
using Godot;
using Godot.Collections;

namespace Simulation.Data;

[GlobalClass]
public partial class Genome : Resource
{
    [Export] public Dictionary<PropertyType, float> Property = new()
    {
        { PropertyType.Color, 0xFFFFFF }
    };
    public NeuralNetwork Brain { get; private set; }
    private static readonly Random _random = new();


    public Genome()
    {
        Initialize();
    }
    public Genome(NeuralNetwork neuralNetwork)
    {
        Brain = neuralNetwork;
    } 

    public void Initialize()
    {
        Brain = new(8, 6, 2);
        
        int color = (int)Property[PropertyType.Color];
        byte r = (byte)(((color >> 16) & 0xFF) * _random.NextDouble()); 
        byte g = (byte)(((color >> 8) & 0xFF) * _random.NextDouble());  
        byte b = (byte)((color & 0xFF) * _random.NextDouble());  
        Property[PropertyType.Color] = (r << 16) + (g << 8) + b;
    }

    public Genome Clone()
    {
        return new()
        {
            Property = Property.Duplicate(),
            Brain = Brain.Clone()
        };
    }

    public void Mutate(double rate)
    {
        foreach (var keyValuePair in Property)
        {
            Property[keyValuePair.Key] *= _random.NextSingle() * 0.5f + 0.5f;
        }
        Property[PropertyType.Color] *= 1.5f;
        Brain.Mutate(rate);
        Brain.Data.Generation++;
    }

    public Genome BlendCrossover(Genome anotherGen, double alpha = 0.5)
    {
        Genome child = new();

        foreach (var keyValuePair in Property)
        {
            child.Property[keyValuePair.Key] = (Property[keyValuePair.Key] + anotherGen.Property[keyValuePair.Key]) / 2;
        }

        // Weight input->hidden blend
        BlendMatrix(Brain.Data.WeightsIH, anotherGen.Brain.Data.WeightsIH, child.Brain.Data.WeightsIH, alpha);

        // Weight hidden->output blend
        BlendMatrix(Brain.Data.WeightsHO, anotherGen.Brain.Data.WeightsHO, child.Brain.Data.WeightsHO, alpha);

        // Bias blend
        BlendVector(Brain.Data.BiasH, anotherGen.Brain.Data.BiasH, child.Brain.Data.BiasH, alpha);
        BlendVector(Brain.Data.BiasO, anotherGen.Brain.Data.BiasO, child.Brain.Data.BiasO, alpha);

        return child;
    }


    private static void BlendMatrix(double[][] m1, double[][] m2, double[][] dst, double alpha)
    {
        for (int i = 0; i < m1.Length; i++)
        {
            for (int j = 0; j < m1[i].Length; j++)
            {
                dst[i][j] = GetBlendValue(m1[i][j], m2[i][j], alpha);
            }
        }
    }

    private static void BlendVector(double[] v1, double[] v2, double[] dst, double alpha)
    {
        for (int i = 0; i < v1.Length; i++)
        {
            dst[i] = GetBlendValue(v1[i], v2[i], alpha);
        }
    }

    private static double GetBlendValue(double w1, double w2, double alpha)
    {
        double min = Math.Min(w1, w2);
        double max = Math.Max(w1, w2);
        double range = max - min;

        // Expand range by +/- range * alpha
        double lowerBound = min - range * alpha;
        double upperBound = max + range * alpha;

        return lowerBound + _random.NextDouble() * (upperBound - lowerBound);
    }
}


public enum PropertyType
{
    Color
}

