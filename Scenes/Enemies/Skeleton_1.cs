using ForceOfHell.Scripts.MainCharacter;
using Godot;
using System;
using System.Threading.Tasks;

public partial class Skeleton_1 : CharacterBody2D
{
	private AnimatedSprite2D _animatedSprite;
	private Area2D _detectionArea;
	private Area2D _attackArea;
	private Player _player;
	private Area2D _damageArea;
	public bool IsOnVisionRange { get; private set; }
	public bool IsOnAttackRange { get; private set; }
	public const float JumpVelocity = -400f;
	private bool _attackedThisFrame = false;
	[Export] public PackedScene Arrow { get; set; } 
	
	public bool TakeDamage = false;
	public override void _Ready()
	{
		_animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");	
		_detectionArea = GetNodeOrNull<Area2D>("DetectionArea");
		_attackArea = GetNodeOrNull<Area2D>("AttackArea");
		_damageArea = GetNodeOrNull<Area2D>("DamageArea");
	}

    public override void _Process(double delta)
    {
        if (_animatedSprite.Animation == "Attack" && _animatedSprite.Frame == 1)
        {
			if (!_attackedThisFrame)
			{
				_attackedThisFrame = true;
			}
		}
		else
		{
			_attackedThisFrame = false;
		}
    }

	public override void _PhysicsProcess(double delta)
	{
		Velocity += GetGravity() * (float)delta;		

		UpdateSpriteDirection();

		if (IsOnAttackRange && _animatedSprite.Animation != "Attack")
		{
			_animatedSprite.Play("Attack");
			Velocity = Velocity with { X = 0f }; 

			Arrow arrowInstance = Arrow.Instantiate<Arrow>();
			arrowInstance.Position = GlobalPosition + new Vector2(_animatedSprite.FlipH ? -30f : 30f, 0f);
			arrowInstance.Configure(_animatedSprite.FlipH ? -1 : 1, 800f);
			GetParent().AddChild(arrowInstance);
			GD.Print("Skeleton_1 Attacked");
		}

		if (IsOnVisionRange && _animatedSprite.Animation != "Attack")
		{
			_animatedSprite.Play("Walk");
			Velocity = Velocity with { X = (_player.GlobalPosition.X < GlobalPosition.X ? -1 : 1) * 100f };

			for (int i = 0; IsOnFloor() && i < GetSlideCollisionCount(); i++)
			{
				KinematicCollision2D collision = GetSlideCollision(i);
				if (collision.GetCollider() is TileMapLayer tileMap)
				{
					Vector2 collisionNormal = collision.GetNormal();
					if (Math.Abs(collisionNormal.X) > 0.5f)
					{
						Velocity = Velocity with { Y = JumpVelocity };
					}
				}
			}
		}

		MoveAndSlide();
	}

	private async Task DamageDealt()
	{
        TakeDamage = true;
		await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
		TakeDamage = false;
	}

	private void ReceiveDamage()
    {
        _ = DamageDealt();
    }

	public void OnDamageAreaBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			GD.Print("Player hit Goblin_1");
			ReceiveDamage();
		}
	}

	private void UpdateSpriteDirection()
		{
			if (_animatedSprite == null) 
				return;

			_animatedSprite.FlipH = Velocity.X switch
			{
				< 0f => true,
				> 0f => false,
				_ => _animatedSprite.FlipH
			};
		}

	public void OnAreaDetectionBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			GD.Print("Player detected by Goblin_1");
			IsOnVisionRange = true;
			_player = player;
			_player.InJumping += OnPlayerJumped;
		}
	}

	public void OnPlayerJumped()
	{
		if (IsOnVisionRange)
		{
			Velocity = Velocity with { Y = JumpVelocity };
		}
	}

	public void OnAreaDetectionBodyExited(Node2D body)
	{
		if (body is Player player)
		{
			IsOnVisionRange = false;
			GD.Print("Player left Skeleton_1 detection area");
			_animatedSprite.Play("Idle");
			Velocity = Velocity with { X = 0f };
		}
	}

	public void OnAttackAreaEntered(Node2D body)
    {
        if (body is Player player)
        {
            IsOnAttackRange = true;
        }
    }
	
	public void OnAttackAreaExited(Node2D body)
	{
		if (body is Player player)
		{
			IsOnAttackRange = false;
		}
	}

	public void OnAnimationFinished()
    {
        if (_animatedSprite.Animation == "Attack")
		{
			_animatedSprite.Play("Idle");
		}
    }
}