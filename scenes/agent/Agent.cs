using Godot;

namespace Simulation.Scenes;


public partial class Agent : Entity
{
    private const float MiddleRayLength = 800;
    private const float SideRayLength = 50;
    private const int TotalRays = 5; // Change With Carefulness

    private Vector2[] _rays;
    private double[] _output;


    protected override void Init()
    {
        _targetSpeed = default;

        // Create rays
        if(_rays == null || _rays.Length < TotalRays)
        {
            int halfRays = TotalRays / 2;
            _rays = new Vector2[TotalRays];
            for (var i = 0; i < _rays.Length; i++)
            {
                float angle = 0.5f * Mathf.Pi * i / TotalRays; // PI/2 (for less angle, we need offset)
                Vector2 direction = Vector2.FromAngle(angle + Mathf.Pi * 7 / 4); // 3PI/2 (for right direction) + PI/4 (offset)

                float pct = 1 - Mathf.Abs(halfRays - i) / (float)halfRays; // Side = 0, Middle = 1
                float length = SideRayLength + (MiddleRayLength - SideRayLength) * pct;
                _rays[i] = direction * length;
            }
        }

        // Get first output
        var inputs = GetInputs(0, Rotation);
        _output = _genome.Brain.FeedForward(inputs);
    }

    protected override void MovePhysics(float delta)
    {
        float oldRotation = Rotation;
        Move((float)delta, (float)_output[0], (float)_output[1]);

        var inputs = GetInputs((float)delta, oldRotation);
        _output = _genome.Brain.FeedForward(inputs);
    }


/*
    public override void _Draw()
    {
        for (var i = 0; i < _rays.Length; i++)
        {
            var ray = _rays[i];
            DrawLine(Vector2.Zero, ray, Modulate * 0.5f);
        }
    }*/

    /// <returns>[<all rays>, forward, lateral, angular]</returns>
    private double[] GetInputs(float delta, float oldRotation)
    {
        var space = GetWorld2D().DirectSpaceState;

        Vector2 localForward = Vector2.FromAngle(Rotation);
        float forwardSpeed = Velocity.Dot(localForward);

        float angularVelocity = delta > 0 ? (Rotation - oldRotation) / delta : 0f;

        Vector2 localRight = Vector2.FromAngle(Rotation + Mathf.Pi / 2f);
        float lateralVelocity = Velocity.Dot(localRight); 

        double[] inputs = new double[_rays.Length + 3];

        // Set rays
        for (var i = 0; i < _rays.Length; i++)
        {
            var query = PhysicsRayQueryParameters2D.Create(GlobalPosition, _rays[i].Rotated(Rotation) + GlobalPosition);
            query.CollisionMask = 0b01;
            query.Exclude = [GetRid()];
            
            var result = space.IntersectRay(query);
            if(result.Count > 0 && (Vector2)result["position"] is Vector2 pos)
            {
                double distancePercent = GlobalPosition.DistanceTo(pos) / _rays[i].Length();
                inputs[i] = distancePercent;
            }
        }

        inputs[_rays.Length] = forwardSpeed / MaxSpeed;  
        inputs[_rays.Length + 1] = lateralVelocity / (MaxSpeed * 0.5);  
        inputs[_rays.Length + 2] = angularVelocity / Mathf.Pi;  
        return inputs;
    }
}
