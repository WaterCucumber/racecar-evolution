using System;
using System.Linq;

namespace Simulation.Data;


public class NeuralNetwork
{
    public NeuralNetworkData Data { get; private set; }

    private static readonly Random _random = new();

    public NeuralNetwork(int inputSize, int hiddenSize, int outputSize)
    {
        Data = new()
        {
            InputSize = inputSize,
            HiddenSize = hiddenSize,
            OutputSize = outputSize,
            WeightsIH = InitMatrix(inputSize, hiddenSize),
            WeightsHO = InitMatrix(hiddenSize, outputSize),
            BiasH = InitVector(hiddenSize),
            BiasO = InitVector(outputSize)
        };
    }


    public NeuralNetwork Clone()
    {
        var copy = new NeuralNetwork(Data.InputSize, Data.HiddenSize, Data.OutputSize);
        ForWeights(Data.WeightsIH, copy.Data.WeightsIH);
        ForWeights(Data.WeightsHO, copy.Data.WeightsHO);
        Array.Copy(Data.BiasH, copy.Data.BiasH, Data.BiasH.Length);
        Array.Copy(Data.BiasO, copy.Data.BiasO, Data.BiasO.Length);
        return copy;
    }

    public double[] FeedForward(double[] input)
    {
        // Hidden layer
        var hidden = ProceedLayer(Data.HiddenSize, Data.InputSize, Data.BiasH, input, Data.WeightsIH);

        // Output layer
        var output = ProceedLayer(Data.OutputSize, Data.HiddenSize, Data.BiasO, hidden, Data.WeightsHO);
        return output;
    }

    public void Mutate(double rate)
    {
        MutateMatrix(Data.WeightsIH, rate);
        MutateMatrix(Data.WeightsHO, rate);
        MutateVector(Data.BiasH, rate);
        MutateVector(Data.BiasO, rate);
    }

    #region Export / Import
    public NeuralNetworkData ExportData()
    {
        return new NeuralNetworkData
        {
            InputSize = Data.InputSize,
            HiddenSize = Data.HiddenSize,
            OutputSize = Data.OutputSize,
            WeightsIH = Data.WeightsIH,
            WeightsHO = Data.WeightsHO,
            BiasH = Data.BiasH,
            BiasO = Data.BiasO
        };
    }

    public void ImportData(NeuralNetworkData data)
    {
        if (data.InputSize != Data.InputSize || data.HiddenSize != Data.HiddenSize || data.OutputSize != Data.OutputSize)
        {
            throw new Exception("Import error: Architecture of loaded neural network is diffirent!");
        }

        Data.WeightsIH = data.WeightsIH;
        Data.WeightsHO = data.WeightsHO;
        Data.BiasH = data.BiasH;
        Data.BiasO = data.BiasO;
    }
    #endregion


    #region Initialization Util Methods
    private static double[][] InitMatrix(int rows, int cols)
    {
        // Xavier Initialization
        double xavierRange = XavierRange(rows); 
        var m = new double[rows][];
        for (int i = 0; i < rows; i++)
        {
            m[i] = new double[cols];
            for (int j = 0; j < cols; j++)
                m[i][j] = xavierRange * (_random.NextDouble() * 2 - 1);
        }
        return m;
    }
    private static double[] InitVector(int size) => [.. Enumerable.Range(0, size).Select(_ => _random.NextDouble() * 2 - 1)];
    private static double XavierRange(int rows) => 1.0 / Math.Sqrt(rows);
    #endregion
    
    #region Clone Util Method
    private static void ForWeights(double[][] src, double[][] dst)
    {
        for (int i = 0; i < src.Length; i++)
            Array.Copy(src[i], dst[i], src[i].Length);
    }
    #endregion
    
    #region Feed Forward Util Methods
    private static double[] ProceedLayer(int layerSize, int previousLayerSize, double[] bias, double[] input, double[][] weights)
    {
        var output = new double[layerSize];
        for (int i = 0; i < layerSize; i++)
        {
            double sum = bias[i];
            for (int j = 0; j < previousLayerSize; j++)
            {
                sum += input[j] * weights[j][i];
            }
            output[i] = Tanh(sum);
        }
        return output;
    }
    private static double Tanh(double x) => Math.Tanh(x);
    #endregion
    
    #region Mutation Util Methods
    private static void MutateMatrix(double[][] m, double rate)
    {
        double xavierRange = XavierRange(m.Length); 
        for (int i = 0; i < m.Length; i++)
        {
            for (int j = 0; j < m[i].Length; j++)
            {
                if (_random.NextDouble() < rate)
                {
                    if(_random.NextDouble() < 0.15d)
                    {
                        m[i][j] = (_random.NextDouble() * 2 - 1) * xavierRange;
                    }
                    else
                    {
                        m[i][j] += (_random.NextDouble() * 2 - 1) * 0.5;
                    }
                }
            }
        }
    }
    private static void MutateVector(double[] v, double rate)
    {
        for (int i = 0; i < v.Length; i++)
        {
            if (_random.NextDouble() < rate)
            {
                if(_random.NextDouble() < 0.15d)
                {
                    v[i] = _random.NextDouble() * 4 - 2;
                }
                else
                {
                    v[i] += (_random.NextDouble() * 2 - 1) * 0.5;
                }
            }
        }
    }
    #endregion
}