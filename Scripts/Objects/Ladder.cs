using ForceOfHell.Scripts.MainCharacter;
using Godot;
using System;

public partial class Ladder : Area2D
{
	[Export] Player _player;

	/// <summary>
	/// Enables climbing for the player when the specified body enters the trigger area.
	/// </summary>
	/// <param name="body">The node that has entered the trigger area. If the node is a player, climbing is enabled for that player.</param>
	public void OnBodyEntered(Node body)
	{
		if (body is Player player)
		{
			player.CanClimb = true;
			player.ClimbAnchorX = GlobalPosition.X;
		}
	}

	/// <summary>
	/// Handles logic when a body exits the associated area or trigger.
	/// </summary>
	/// <param name="body">The node representing the body that has exited. If the body is a player, climbing will be disabled for that player.</param>
	public void OnBodyExited(Node body)
	{
		if (body is Player player)
		{
			player.CanClimb = false;
		}
	}
}
