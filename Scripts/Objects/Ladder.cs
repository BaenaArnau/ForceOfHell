using ForceOfHell.Scripts.MainCharacter;
using Godot;
using System;

public partial class Ladder : Area2D
{
	[Export] Player _player;

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
			player.CanClimb = true;
			player.ClimbAnchorX = GlobalPosition.X;
		}
	}

	public void OnBodyExited(Node body)
	{
		if (body is Player player)
		{
			player.CanClimb = false;
			player.ClimbAnchorX = 0f;
		}
	}
}
