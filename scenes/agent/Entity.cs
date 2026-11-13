using System;
using Godot;
using Simulation.Components;
using Simulation.Data;
using Simulation.Util;

namespace Simulation.Scenes;


public abstract partial class Entity : CharacterBody2D
{
    protected const float MaxAngularVelocity = 20 * Mathf.Pi / 180;
    protected const float MaxSpeed = 400;
    protected const float RotationAcceleration = 0.05f;
    private const float Acceleration = 200;

    public event Action Stopped;

    [Export] private Area2D _collisionCheckArea;
    private Line2D _path;

    public AgentScoringComponent Scoring { get; private set; }
    public Checkpoints Checkpoints { get; private set; }

    protected Genome _genome;

    protected float _targetSpeed;
    private float _pathPointPlaceTime;
    private bool _isStopped = true;


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
        _isStopped = false;

        // Initialize scoring
        if(Checkpoints != null)
        {
            Scoring = new(this, _collisionCheckArea);
            Scoring.Died += Stop;
            Scoring.Finished += Stop;
        }

        Init();

        // Visuals
        if(_genome != null)
        {
            int color = (int)_genome.Property[PropertyType.Color];
            byte r = (byte)((color >> 16) & 0xFF); 
            byte g = (byte)((color >> 8) & 0xFF);  
            byte b = (byte)(color & 0xFF);  
            Modulate = Color.Color8(r, g, b);
            _path.Modulate = Modulate;
        }
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
        if (_isStopped) return;
        QueueRedraw();

        // Move physics
        MovePhysics((float)delta);

        Scoring?.Update(delta);

        // Visual
        _pathPointPlaceTime += (float)delta;
        if (_pathPointPlaceTime > 0.3f)
        {
            _pathPointPlaceTime = 0;
            _path?.AddPoint(Position);
        }
    }

    protected void Move(float delta, float xSpeed, float ySpeed)
    {
        _targetSpeed = (float)xSpeed * MaxSpeed;

        Vector2 localForward = Vector2.FromAngle(Rotation);
        Velocity = Velocity.MoveToward(localForward * _targetSpeed, (float)delta * Acceleration);

        float currentForwardSpeed = Velocity.Dot(localForward);
        float wheelAngleRadians = (float)ySpeed * MaxAngularVelocity;
        float deltaRotation = currentForwardSpeed * Mathf.Tan(wheelAngleRadians) * RotationAcceleration * (float)delta;

        Rotation += deltaRotation;
        MoveAndSlide();
    }

    private void Stop()
    {
        if(_isStopped) return;
        _isStopped = true;
        Hide();
        Velocity = Vector2.Zero;
        Stopped?.Invoke();
    }
}

