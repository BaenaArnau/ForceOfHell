using ForceOfHell.Scripts.Weapons.Bullet;
using ForceOfHell.Scripts.FirstLevel;
using Godot;
using System;

namespace ForceOfHell.Scripts.Objects
{
	public partial class Box : RigidBody2D
	{
		[Export] public AnimatedSprite2D _animatedSprite2D;
		[Export] public CollisionShape2D _collisionShape2D;
		[Export] public first_level _firstLevel;

		private int boxHealth = 3;
		private bool _isDestroyed;

		public override void _Process(double delta)
		{
			if (boxHealth <= 0 && !_isDestroyed)
			{
				_isDestroyed = true;
				_animatedSprite2D.Play();
				RandomizeDrop();
			}
		}

		public void TakeDamage()
		{
			boxHealth--;
		}

		private void RandomizeDrop()
		{
			var sceneRoot = GetTree().CurrentScene;
			if (sceneRoot == null)
			{
				GD.PushError("[Box] CurrentScene es null, no se puede soltar el drop.");
				QueueFree();
				return;
			}

			Random random = new Random();
			int dropChance = random.Next(0, 100);

			if (dropChance < 25)
			{
				_firstLevel.GenerateItem(GlobalPosition, "weapon");
			}
			else if (dropChance < 50)
			{
				_firstLevel.GenerateItem(GlobalPosition, "manaPotion");
			}
			else if (dropChance < 75)
			{
				_firstLevel.GenerateItem(GlobalPosition, "healPotion");
			}
			QueueFree();
		}
	}
}
