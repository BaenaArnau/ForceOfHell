using ForceOfHell.Scripts.Objects;
using Godot;
using System;

namespace ForceOfHell.Scripts.Weapons.Bullet
{
	/// <summary>
	/// Bala que se desplaza en una dirección fija y se destruye al chocar con el terreno.
	/// </summary>
	public partial class Bullet : Area2D
	{
		/// <summary>Velocidad lineal de la bala.</summary>
		private const float Speed = 450f;

		/// <summary>Dirección normalizada del movimiento.</summary>
		private Vector2 _direction = Vector2.Zero;

		/// <summary>Nombre de la animación a reproducir.</summary>
		public string animation = string.Empty;

		/// <summary>Sprite animado de la bala.</summary>
		[Export] private AnimatedSprite2D _animatedSprite;

		/// <summary>Forma de colisión de la bala.</summary>
		[Export] private CollisionShape2D _collisionShape;

		/// <summary>Inicializa la animación si existe.</summary>
		public override void _Ready()
		{
			if (!string.IsNullOrWhiteSpace(animation))
				_animatedSprite.Play(animation);
		}

		/// <summary>Mueve la bala cada frame.</summary>
		public override void _Process(double delta)
		{
			if (_direction == Vector2.Zero)
				return;

			Translate(_direction * Speed * (float)delta);
		}

		/// <summary>
		/// Configura la dirección y la animación, y rota la bala según el movimiento.
		/// </summary>
		public void Configure(Vector2 direction, string animation)
		{
			this.animation = animation;
			_direction = direction == Vector2.Zero ? Vector2.Zero : direction.Normalized();

			if (_direction != Vector2.Zero)
				Rotation = _direction.Angle();
		}

		/// <summary>Elimina la bala al chocar con el terreno.</summary>
		private void OnBodyEntered(Node body)
		{
			HandleCollision(body);
		}

		/// <summary>
		/// Handles logic when another Area2D enters the monitored area.
		/// </summary>
		/// <param name="area">The Area2D instance that has entered the area. Cannot be null.</param>
		private void OnAreaEntered(Area2D area)
		{
			HandleCollision(area);
		}

		/// <summary>
		/// Handles collision logic when the current object interacts with the specified node.
		/// </summary>
		/// <remarks>If the collided node belongs to the "Terreno" group, the current object is removed from the
		/// scene. If the node is a Box, it receives damage and the current object is then removed.</remarks>
		/// <param name="node">The node that the current object has collided with. Must not be null.</param>
		private void HandleCollision(Node node)
		{
			if (node.IsInGroup("Terreno"))
			{
				QueueFree();
				return;
			}

			if (node is Box b)
			{
				b.TakeDamage(false);
				QueueFree();
			}
		}
	}
}
