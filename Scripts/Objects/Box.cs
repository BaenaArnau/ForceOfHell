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

		private int boxHealth = 4;
		private bool _isDestroyed;

		/// <summary>
		/// Called every frame to update the node's state. Handles destruction logic when the box's health reaches zero or
		/// below.
		/// </summary>
		/// <remarks>This method is typically called by the engine once per frame. Override this method to implement
		/// per-frame logic for the node.</remarks>
		/// <param name="delta">The elapsed time, in seconds, since the previous frame. Used to synchronize updates with the frame rate.</param>
		public override void _Process(double delta)
		{
			if (boxHealth <= 0 && !_isDestroyed)
			{
				_isDestroyed = true;
				_animatedSprite2D.Play();
				RandomizeDrop();
			}
		}

		/// <summary>
		/// Applies damage to the box, reducing its health based on the type of attack.
		/// </summary>
		/// <param name="IsMelee">true to apply melee damage, which reduces health by 2; false to apply non-melee damage, which reduces health by 1.</param>
		public void TakeDamage(bool IsMelee)
		{
			if (IsMelee) 
			{
				boxHealth -= 2;
				return;
			}
			boxHealth--;
		}

		/// <summary>
		/// Randomly generates and drops an item at the current position based on predefined probabilities.
		/// </summary>
		/// <remarks>This method selects an item type to drop using random chance and delegates item creation to the
		/// first level. The method frees the current object after attempting to drop an item. If the current scene is not
		/// available, no item is dropped and the object is freed immediately.</remarks>
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
