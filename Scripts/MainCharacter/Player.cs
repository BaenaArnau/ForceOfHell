using Godot;
using System;

namespace ForceOfHell.Scripts.MainCharacter
{
	/// <summary>
	/// Clase que representa al jugador.
	/// </summary>
	public partial class Player : CharacterBody2D
	{
		/// <summary>Velocidad horizontal configurada del jugador (px/s).</summary>
		public const float Speed = 300.0f;

		/// <summary>Velocidad vertical aplicada al iniciar un salto (negativa = hacia arriba).</summary>
		public const float JumpVelocity = -500.0f;

		/// <summary>Duración máxima del salto coyote en segundos.</summary>
		public const float CoyoteTimeMax = 0.15f;

		/// <summary>Tiempo restante para salto coyote (lectura pública, modificación privada).</summary>
		private float _coyoteTimeCounter;

		/// <summary>Tiempo restante para salto coyote (solo lectura pública).</summary>
		public float CoyoteTimeRemaining => _coyoteTimeCounter;

		[Signal] public delegate void InJumpingEventHandler();

		/// <summary>
		/// Compatibilidad: propiedad con el nombre antiguo usada por estados externos.
		/// Permite lectura pública y asignación interna.
		/// </summary>
		public float CoyoteTimeCounter { get => _coyoteTimeCounter; internal set => _coyoteTimeCounter = value; }

		/// <summary>
		/// Compatibilidad: exponer AnimatedSprite2D como propiedad pública para estados que lo usan directamente.
		/// </summary>
		public AnimatedSprite2D animatedSprite
		{
			get
			{
				if (_animatedSpriteNode is AnimatedSprite2D aspr)
					return aspr;
				return GetNode<AnimatedSprite2D>("AnimatedSprite2D");
			}
		}

		/// <summary>Bool para indicar que el jugador está en proceso de muerte (solo lectura externa).</summary>
		public bool IsDying { get; private set; }

		/// <summary> Sprite animado del jugador para controlar las animaciones (privado).</summary>
		private Node _animatedSpriteNode;

		/// <summary>
		/// Método llamado al iniciar el nodo.
		/// </summary>
		public override void _Ready()
		{
			_animatedSpriteNode = GetNodeOrNull("AnimatedSprite2D");
		}

		/// <summary>Actualización de física del jugador. Mantener ligera para lógica central.</summary>
		/// <param name="delta">Tiempo en segundos desde el último paso de física.</param>
		public override void _PhysicsProcess(double delta) 
		{
			if (Velocity.X < 0f)
			{
				if (_animatedSpriteNode is AnimatedSprite2D aspr)
					aspr.FlipH = true;
			}
			else if (Velocity.X > 0f)
			{
				if (_animatedSpriteNode is AnimatedSprite2D aspr2)
					aspr2.FlipH = false;
			}

			if (IsOnFloor())
				_coyoteTimeCounter = CoyoteTimeMax;
			else
			{
				_coyoteTimeCounter -= (float)delta;
				if (_coyoteTimeCounter < 0f)
					_coyoteTimeCounter = 0f;
			}
		}

		/// <summary>
		/// Reproduce una animación en el <see cref="AnimatedSprite2D"/> hijo.
		/// Método auxiliar usado por los estados para cambiar la animación.
		/// </summary>
		/// <param name="animationName">Nombre de la animación a reproducir.</param>
		public void SetAnimation(string animationName)
		{
			if (IsDying)
				return;
			
			if (_animatedSpriteNode is AnimatedSprite2D aspr)
				aspr.Play(animationName);

		}

		/// <summary>
		/// Método llamado cuando el jugador recibe daño.
		/// </summary>
		public async System.Threading.Tasks.Task HitAsync()
		{
			try
			{
				IsDying = true;
				if (_animatedSpriteNode is AnimatedSprite2D aspr)
				{
					aspr.Play("hit");
					await ToSignal(aspr, "animation_finished");
				}
				GetTree().CallDeferred("reload_current_scene");
			}
			catch (Exception)
			{
				GetTree().CallDeferred("reload_current_scene");
			}
		}
	}
}
