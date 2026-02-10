using ForceOfHell.Scripts.MainCharacter;
using Godot;
using System;

public partial class HealPotion : Area2D
{
	private const float BobAmplitude = 6f;
	private const float BobSpeed = 2.5f;

	private Vector2 _basePosition;
	private float _bobTime;
	private int healRegenerayion = 20;

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
	/// Handles the event when a body enters the area and applies healing to the player if applicable.
	/// </summary>
	/// <remarks>This method is typically connected to an area or trigger's body entered signal. If the entering
	/// node is a player, the player's health is increased by the configured amount, up to the maximum allowed. The healing
	/// area is then removed from the scene.</remarks>
	/// <param name="body">The node that has entered the area. If the node is a player, healing is applied.</param>
	private void OnBodyEntered(Node body)
	{
		if (body is Player player)
		{
			player.healthActual += healRegenerayion;
			if (player.healthActual > player.GetHealthMax())
				player.healthActual = player.GetHealthMax();
			QueueFree();
		}
	}
}
