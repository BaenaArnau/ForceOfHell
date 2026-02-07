using Godot;
using System;
using ForceOfHell.Scripts.MainCharacter;

public partial class DeadZone : Area2D
{
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

    public void OnBodyEntered(Node body)
    {
        if (body is Player player)
        {
            _ = player.Die();
            QueueFree();
        }
    }
}
