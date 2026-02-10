using ForceOfHell.Scripts.MainCharacter;
using Godot;
using System;

public partial class ManaPotion : Area2D
{
	private const float BobAmplitude = 6f;
	private const float BobSpeed = 2.5f;

	private Vector2 _basePosition;
	private float _bobTime;
	private int manaRegenerayion = 20;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_basePosition = Position;
		_bobTime = 0f;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_bobTime += (float)delta * BobSpeed;
		var offsetY = Mathf.Sin(_bobTime) * BobAmplitude;
		Position = _basePosition + new Vector2(0f, offsetY);
	}

	/// <summary>
	/// Handles the event when a body enters the associated area and restores mana to the player if applicable.
	/// </summary>
	/// <remarks>This method is typically connected to an area or trigger in the scene. If the entering node is a
	/// player, the player's mana is increased by a predefined amount, up to the maximum allowed. The area is then removed
	/// from the scene.</remarks>
	/// <param name="body">The node that has entered the area. If the node is a player, its mana will be increased.</param>
	private void OnBodyEntered(Node body)
	{
		if (body is Player player)
		{
			player.manaActual += manaRegenerayion;
			if (player.manaActual > player.GetManaMax())
				player.manaActual = player.GetManaMax();
			QueueFree();
		}
	}
}
