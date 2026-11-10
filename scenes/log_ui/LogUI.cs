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
    private static LogUI _instance;

    public Label Generation => _generation;
    public Label Difficulty => _difficulty;
    public Label BestFitness => _bestFitness;
    public Label MutationRate => _mutationRate;
    public Label Plato => _plato;
    public Label Agents => _agents;
    public Label Population => _population;


    public static LogUI GetInstance() => _instance;
    public static void Send(Label l, string value)
    {
        l.Text = value;
    }
    public static void Toggle(Label l, bool value)
    {
        l.Visible = value;
    }

    public override void _Ready()
    {
        _instance = this;
        Population.Text = $"Population: {EvolutionManager.Population}";
    }
}
