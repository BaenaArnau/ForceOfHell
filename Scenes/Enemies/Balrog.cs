using ForceOfHell.Scripts.MainCharacter;
using Godot;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

public partial class Balrog : CharacterBody2D
{
	private AnimatedSprite2D _animatedSprite;
	private Area2D _weaponCollision; 
	private Area2D _chaseArea;
	private Player _player;
	private Area2D _damageArea;
	private Vector2 _initialPosition;
	public bool BalrogActivate = false;
	private bool _attackedThisFrame = false;
	public bool TakeDamage = false;
	public override void _Ready()
	{
		_animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");	
		_weaponCollision = GetNodeOrNull<Area2D>("WeaponCollision");
		_damageArea = GetNodeOrNull<Area2D>("DamageArea");
		_player = GetNodeOrNull<Player>("../Character");
		_initialPosition = GlobalPosition;
	}

	public override void _PhysicsProcess(double delta)
	{
		Velocity += GetGravity() * (float)delta;	

		
		
		if (BalrogActivate)
		{
			if (_animatedSprite.Animation == "Attack" && _animatedSprite.Frame == 5)
			{
				if (!_attackedThisFrame)
				{
					_attackedThisFrame = true;
					Godot.Collections.Array<Node2D> bodies = _weaponCollision.GetOverlappingBodies();
					foreach (Node2D body in bodies)			
					{
						if (body is Player player)				
						{
							_ = player.HitAsync();	
						}
					} 
					_animatedSprite.Play("Idle");
				}
			}
			else
			{	
				_attackedThisFrame = false; 
			}

			if (_animatedSprite.Animation != "Attack")
			{
				_animatedSprite.Play("Walk");
				Velocity = Velocity with { X = (_player.GlobalPosition.X < GlobalPosition.X ? -1 : 1) * 100f };
			}
		}
		else
		{
	
			if ((_initialPosition - GlobalPosition).Length() > 5f)
			{
				Velocity = Velocity with { X = (_initialPosition.X < GlobalPosition.X ? -1 : 1) * 100f };
				_animatedSprite.Play("Walk");
			}
			else
			{
				Velocity = Velocity with { X = 0f };
				_animatedSprite.Play("Idle");
			}
		}
		UpdateSpriteDirection();
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
				< 0f => false,
				> 0f => true,
				_ => _animatedSprite.FlipH
			};
		}
	public void OnWeaponCollisionAreaBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			GD.Print("Player hit by Goblin_1 weapon");
			_animatedSprite.Play("Attack");
			Velocity = Velocity with { X = 0f };
			
		}
	}	
}
