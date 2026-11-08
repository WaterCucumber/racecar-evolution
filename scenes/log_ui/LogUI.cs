using Godot;

namespace Simulation.Scenes;


public partial class LogUI : CanvasLayer
{
    [Export] private Label _generation;
    [Export] private Label _difficulty;
    [Export] private Label _bestFitness;
    [Export] private Label _mutationRate;
    [Export] private Label _plato;
    [Export] private Label _agents;
    [Export] private Label _population;

    [Export] private EvolutionManager _manager;


    public override void _Ready()
    {
        _manager.EpochPass += OnEpochPass;
        _manager.EpochStarted += OnEpochStarted;
        _manager.AgentsChanged += OnAgentsChanged;
        _population.Text = $"Population: {EvolutionManager.Population}";
        Update();
    }

    private void OnAgentsChanged(int amount)
    {
        _agents.Text = $"Agents: {amount, 2}";
    }

    private void OnEpochStarted()
    {
        _difficulty.Text = $"Difficulty: {_manager.GetDifficulty(),2:F2}";
    }

    private void OnEpochPass()
    {
        _generation.Text = $"Generation: {_manager.GetGeneration(),3}";
        _bestFitness.Text = $"Best Fitness: {_manager.GetBestFitness():F2} pt.";
        _mutationRate.Text = $"Mutation Rate: {_manager.GetMutationRate()}";
        _plato.Text = $"Plato: {_manager.GetPlato(), 2}";
    }

    private void Update()
    {
        _generation.Text = $"Generation: {_manager.GetGeneration(),3}";
        _bestFitness.Text = $"Best Fitness: {_manager.GetBestFitness():F2} pt.";
        _mutationRate.Text = $"Mutation Rate: {_manager.GetMutationRate()}";
        _plato.Text = $"Plato: {_manager.GetPlato(), 2}";
        _agents.Text = $"Agents: {_manager.GetAgents(), 2}";
        _difficulty.Text = $"Difficulty: {_manager.GetDifficulty(),2:F2}";
    }
}
