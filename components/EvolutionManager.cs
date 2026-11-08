using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Simulation.Components;
using Simulation.Data;
using Simulation.Util;

namespace Simulation.Scenes;


public partial class EvolutionManager : Node
{
    public const int Population = 100;
    private const float DefaultMinRadius = 400;
    private const float MinimumMinRadius = 200;
    private const float DefaultMaxRadius = 500;
    private const float MaximumMaxRadius = 700;
    private const float DefaultNumPoints = 6;
    private const float MaximumNumPoints = 22;
    private const float RaduisRase = 4f;
    private const float NumPointsRase = 1/5f;
    private const double DefaultMutationRate = 0.15;
    private const double PlatoMutationMultiply = 0.01;
    private const double PlatoMutationRate = 0.4;

    private static readonly PackedScene _agent = GD.Load<PackedScene>(@"res://scenes/agent/agent.tscn");
    private static readonly Random _random = new();
    private static EvolutionManager _instance;

    public event Action<int> AgentsChanged;
    public event Action EpochPass;
    public event Action EpochStarted;
    
    [Export] private TrackCreator _trackCreator;
    [Export] private string _saveFile = "best_brain.json";
    [Export] private string _loadFile = "best_car_brain.json";

    private Agent[] _population;
    private Agent[] _previousElite;
    private int _agentsProceeding;

    private int _generationIndex;
    private float _previousFitness, _bestFitness;
    private float _minR = DefaultMinRadius, _maxR = DefaultMaxRadius, _numPoints = DefaultNumPoints;
    private int _platoAmount;
    private double _mutationRate = DefaultMutationRate;

    private bool _started = false;
    private Checkpoints _currentCheckpoints;


    public static EvolutionManager GetInstance() => _instance;

    public float GetDifficulty() => _maxR / _minR * ((int)_numPoints) / (DefaultMaxRadius / DefaultMinRadius * DefaultNumPoints);
    public int GetGeneration() => _generationIndex;
    public float GetBestFitness() => _previousFitness;
    public double GetMutationRate() => _mutationRate;
    public int GetPlato() => _platoAmount;
    public int GetAgents() => _agentsProceeding;

    private void SetDifficulty(float v)
    {
        if(v < 1) return;

        float a = 4 / 5f * DefaultMinRadius;
        float b = DefaultNumPoints * (DefaultMinRadius * RaduisRase + RaduisRase * v * DefaultMaxRadius) + DefaultMinRadius * DefaultMaxRadius * NumPointsRase;
        float c = DefaultMaxRadius * DefaultNumPoints * DefaultMinRadius * (1 - v) ;
        
        float d = Mathf.Sqrt(b*b-4*a*c);
        int i = (int)((-b+d)/a/2);

        _minR = DefaultMinRadius - i * RaduisRase;
        _maxR = DefaultMaxRadius + i * RaduisRase;
        _numPoints = DefaultNumPoints + i * NumPointsRase;
        
        _maxR = Mathf.Clamp(_maxR, DefaultMaxRadius, MaximumMaxRadius);
        _minR = Mathf.Clamp(_minR, MinimumMinRadius, DefaultMinRadius);
        _numPoints = Mathf.Clamp(_numPoints, DefaultNumPoints, MaximumNumPoints);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("start") && !_started)
        {
            _started = true;
            Start();
        }
    }

    public override void _Ready()
    {
        _instance = this;
        SetDifficulty(9.4f);
    }


    public async void Start()
    {
        InstantiateAgents(Population);
        var gens = GetStartGenomes();
        await SetupEpoch(gens);
    }

    private Genome[] GetStartGenomes()
    {
        var gens = new Genome[_population.Length];
        if (_loadFile != "")
        {
            gens[0] = new();
            bool hasSavedBrain = SaveManager.LoadBrain(gens[0].Brain, _loadFile);

            if (hasSavedBrain)
            {
                for (int i = 1; i < _population.Length; i++)
                {
                    gens[i] = gens[0].Clone();
                    gens[i].Mutate(0.2);
                }
            }
        }
        return gens;
    }

    private void InstantiateAgents(int amount)
    {
        _population = new Agent[amount];
        for (var i = 0; i < amount; i++)
        {
            var agent = _agent.Instantiate<Agent>();
            agent.Stopped += OnStopped;
            _trackCreator.AddChild(agent);
            _population[i] = agent;
        }
    }

    
    private async Task SetupEpoch(Genome[] gens)
    {
        var checkpoints = await _trackCreator.CreateTrackAsync((int)_numPoints, _minR, _maxR);
        InitializeAgents(gens, checkpoints);
        _currentCheckpoints = checkpoints;
        EpochStarted?.Invoke();
    }

    private void InitializeAgents(Genome[] gens, Checkpoints checkpoints)
    {
        _agentsProceeding = _population.Length;
        AgentsChanged?.Invoke(_agentsProceeding);
        for (var i = 0; i < _population.Length; i++)
        {
            var agent = _population[i];
            agent.GlobalPosition = checkpoints.GetSpawnPoint();

            agent.Initialize(gens[i] ?? new(), checkpoints);
        }
    }

    private Genome SelectParent(float topParentsPct = 0.3f)
    {
        Agent[] a = new Agent[3];
        int topParents = (int)(_population.Length * topParentsPct);

        for (var i = 0; i < a.Length; i++)
        {
            a[i] = _population[_random.Next(topParents)];
        }

        return a.MaxBy(a => a.Scoring.GetFitness()).GetGenome();
    }

    private Genome[] EvolvePopulation()
    {
        var pop = new Genome[_population.Length];

        int eliteLength = Mathf.Min(3, _population.Length);
        for (var i = 0; i < eliteLength; i++)
        {
            pop[i] = _population[i].GetGenome();
        }
        
        for (var i = eliteLength; i < pop.Length; i++)
        {
            var p1 = SelectParent();
            var p2 = SelectParent();

            var child = p1.BlendCrossover(p2);
            child.Mutate(_mutationRate);
            pop[i] = child;
        }
        return pop;
    }

    private void FinishEpoch(Checkpoints checkpoints)
    {
        _population = [.. _population.OrderByDescending(a => a.Scoring.GetFitness())];
        if(_saveFile != "") SaveManager.SaveBrain(_population[0].GetGenome().Brain, _saveFile);
        
        float bestFitness = _population[0].Scoring.GetFitness();
        float checkpointsFitness = checkpoints.GetCheckpoints() * AgentScoringComponent.CheckpointFitness;
        GD.Print($"Generation {_generationIndex,2} Best Fitness: {bestFitness}");

        if(bestFitness - checkpointsFitness < 1.01f)
        {
            _platoAmount++;
            GD.Print($"\t{_platoAmount} Plato defined");

            if(_platoAmount > 5)
            {
                _mutationRate = Mathf.Min(_mutationRate + (_platoAmount - 5) * PlatoMutationMultiply, PlatoMutationRate);
                GD.Print($"\tToo many platos. Using special ({_mutationRate}) mutation rate");
                
                if(bestFitness < _previousFitness)
                {
                    _maxR -= RaduisRase;
                    _minR += RaduisRase;
                    _numPoints -= NumPointsRase;
                }
            }
        }
        else
        {
            _platoAmount = 0;
            _mutationRate = DefaultMutationRate;
            GD.Print($"\tSuccess. Default ({DefaultMutationRate}) mutation rate");
            _bestFitness = bestFitness;

            _maxR += RaduisRase;
            _minR -= RaduisRase;
            _numPoints += NumPointsRase;
        }
        _maxR = Mathf.Clamp(_maxR, DefaultMaxRadius, MaximumMaxRadius);
        _minR = Mathf.Clamp(_minR, MinimumMinRadius, DefaultMinRadius);
        _numPoints = Mathf.Clamp(_numPoints, DefaultNumPoints, MaximumNumPoints);
        _previousFitness = bestFitness;
        

        GD.Print();
        _generationIndex++;
        EpochPass?.Invoke();
    }
    private async void NewEpoch()
    {
        FinishEpoch(_currentCheckpoints);
        var gens = EvolvePopulation();
        await SetupEpoch(gens);
    }

    private async void OnStopped()
    {
        _agentsProceeding--;
        AgentsChanged?.Invoke(_agentsProceeding);
        if(_agentsProceeding == 0)
        {
            NewEpoch();
        }
    }
}
