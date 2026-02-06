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
			if (body.IsInGroup("Terreno"))
			{
				QueueFree();
			}
		}
	}
}
