using Godot;

namespace Simulation.Scenes;


public partial class Player : Entity
{
    protected override void MovePhysics(float delta)
    {
        Move(delta, Input.GetAxis("-x","x"), Input.GetAxis("y", "-y"));
    }
}
