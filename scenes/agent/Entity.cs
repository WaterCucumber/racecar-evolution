using System;
using Godot;
using Simulation.Components;
using Simulation.Data;
using Simulation.Util;

namespace Simulation.Scenes;


public abstract partial class Entity : CharacterBody2D
{
    protected const float MaxSpeed = 400;
    private const float Acceleration = 200;

    public event Action Stopped;

    [Export] private Area2D _collisionCheckArea;
    private Line2D _path;

    public AgentScoringComponent Scoring { get; private set; }
    public Checkpoints Checkpoints { get; private set; }

    protected Genome _genome;

    protected float _targetSpeed;
    private float _pathPointPlaceTime;
    private bool _isStopped;


    public Genome GetGenome() => _genome;
    public void Reset()
    {
        Visible = true;
        Rotation = default;
        Velocity = default;
        _pathPointPlaceTime = default;
        _targetSpeed = default;
        _path.Points = default;
        _isStopped = default;
    }


    public void Initialize(Genome genome, Checkpoints checkpoints)
    {
        Checkpoints = checkpoints;
        _genome = genome;

        // Initialize scoring
        Scoring = new(this, _collisionCheckArea);
        Scoring.Died += Stop;
        Scoring.Finished += Stop;

        Init();

        // Visuals
        int color = (int)_genome.Property[PropertyType.Color];
        byte r = (byte)((color >> 16) & 0xFF); 
        byte g = (byte)((color >> 8) & 0xFF);  
        byte b = (byte)(color & 0xFF);  
        Modulate = Color.Color8(r, g, b);
        _path.Modulate = Modulate;
    }

    protected virtual void Init() {}
    protected abstract void MovePhysics(float delta);


    public override void _Ready()
    {
        _path = new()
        {
            Width = 1,
            GlobalPosition = Vector2.Zero,
            ZIndex = -10
        };
        GetParent().CallDeferred(Node.MethodName.AddChild, _path);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_genome == null || _isStopped) return;
        QueueRedraw();

        // Move physics
        MovePhysics((float)delta);

        Scoring.Update(delta);

        // Visual
        _pathPointPlaceTime += (float)delta;
        if (_pathPointPlaceTime > 0.3f)
        {
            _pathPointPlaceTime = 0;
            _path.AddPoint(Position);
        }
    }

    protected void Move(float delta, float xSpeed, float ySpeed)
    {
        _targetSpeed += xSpeed * delta;
        float speed = _targetSpeed * MaxSpeed;

        float deltaRotation = ySpeed * Mathf.Pi * delta;
        Rotation += deltaRotation;

        Vector2 targetVelocity = speed * Vector2.FromAngle(Rotation);
        Velocity = Velocity.MoveToward(targetVelocity, delta * Acceleration);
        MoveAndSlide();
    }

    private void Stop()
    {
        if(_isStopped) return;
        if(this is Player) GD.Print($"Your score: {Scoring.GetFitness()}");
        _isStopped = true;
        Hide();
        Velocity = Vector2.Zero;
        Stopped?.Invoke();
    }
}

