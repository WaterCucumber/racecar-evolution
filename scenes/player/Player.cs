using Godot;

namespace Simulation.Scenes;


public partial class Player : Entity
{
    public override void _Ready()
    {
        Initialize(null, null);
    }

    protected override void MovePhysics(float delta)
    {
        float oldRot = Rotation;
        Move(delta, Input.GetAxis("-x","x"), Input.GetAxis("y", "-y"));
        GetInputs(delta, oldRot);
    }

    private void GetInputs(float delta, float oldRotation)
    {
        float angularVelocity = delta > 0 ? (Rotation - oldRotation) / delta : 0f; 
        float maxPossibleAngularVelocity = MaxSpeed * Mathf.Tan(MaxAngularVelocity) * RotationAcceleration;
        var r = (angularVelocity + maxPossibleAngularVelocity) / 2 / maxPossibleAngularVelocity;  
        GD.Print($"Result: {r}\tAV: {Mathf.RadToDeg(angularVelocity)}");
    }
}
