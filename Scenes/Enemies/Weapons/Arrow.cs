using ForceOfHell.Scripts.MainCharacter;
using Godot;
using System;
using System.Threading.Tasks;

public partial class Arrow : RigidBody2D
{
	private AnimatedSprite2D _animatedSprite;

	private Area2D _arrowDamage;
	public string animation = string.Empty;
	private Vector2 _direction = Vector2.Zero;

	[Export] private CollisionShape2D _collisionShape;
	public override void _Ready()
	{
		_animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");	
	}

	public void OnArrowDamageBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			_ = player.HitAsync();
			QueueFree();
		}
		else if (body is TileMapLayer)
		{
			QueueFree();
		}
	}

	public void Configure(int direction, float speed)
	{
		AnimatedSprite2D animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		if (animatedSprite != null)
		{
			animatedSprite.FlipH = direction < 0;
		}
		LinearVelocity = new Vector2(direction * speed, 0f);
	}

	public void Configure(Vector2 direction, string animation)
		{
			this.animation = animation;
			_direction = direction == Vector2.Zero ? Vector2.Zero : direction.Normalized();

			if (_direction != Vector2.Zero)
				Rotation = _direction.Angle();
		}
}