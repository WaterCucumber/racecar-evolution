using System;

namespace Simulation.Data;

[Serializable]
public class NeuralNetworkData
{
    public int InputSize { get; set; }
    public int HiddenSize { get; set; }
    public int OutputSize { get; set; }

    public double[][] WeightsIH { get; set; }
    public double[][] WeightsHO { get; set; }
    public double[] BiasH { get; set; }
    public double[] BiasO { get; set; }

    public int Generation { get; set; } = 0;
}
