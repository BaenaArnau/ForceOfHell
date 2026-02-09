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
