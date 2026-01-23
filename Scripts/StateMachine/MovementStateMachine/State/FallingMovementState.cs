using Godot;
using System;
using PlayerType = ForceOfHell.Scripts.MainCharacter.Player;

namespace ForceOfHell.Scripts.StateMachine.MovementStateMachine.State
{
	/// <summary>
	/// Estado de movimiento para la caída del jugador.
	/// </summary>
	public partial class FallingMovementState : ForceOfHell.Scripts.StateMachine.State
	{
		private const float LandingVelocityThreshold = 250f;

		/// <summary>
		/// Referencia al jugador.
		/// </summary>
		private PlayerType _player;
		private bool _landingSoundPlayed;
		private float _lastVerticalVelocity;

		/// <summary>
		/// Método llamado al iniciar el nodo.
		/// </summary>
		public override async System.Threading.Tasks.Task Ready()
		{
			_player = (PlayerType)GetTree().GetFirstNodeInGroup("MainCharacter");
			if (!_player.IsNodeReady())
				await ToSignal(_player, "ready");
		}
		
		/// <summary>Al entrar en Falling: reproducir animación de caída.</summary>
		public override void Enter()
		{
			_landingSoundPlayed = false;
			_lastVerticalVelocity = 0f;
			_player.SetAnimation("fall");
		}

		public override void Exit()
		{
			_landingSoundPlayed = false;
			_lastVerticalVelocity = 0f;
		}

		/// <summary>Actualización por frame en Falling: transiciones al aterrizar o doble salto.</summary>
		/// <param name="delta">Delta en segundos.</param>
		public override void Update(double delta)
		{
			if (_player.IsOnFloor() && _player.Velocity.X == 0)
				stateMachine.TransitionTo("IdleMovementState");
		}

		/// <summary>Update de física en Falling: aplica gravedad y control horizontal inmediato en aire.</summary>
		/// <param name="delta">Delta en segundos.</param>
		public override void UpdatePhysics(double delta)
		{
			if (!_player.IsOnFloor())
			{
				Vector2 velocity = _player.Velocity;
				velocity += _player.GetGravity() * (float)delta;
				float move = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
				if (Mathf.Abs(move) > 0f)
					velocity.X = move * PlayerType.Speed;
				else
					velocity.X = 0f;

				_lastVerticalVelocity = velocity.Y;
				_landingSoundPlayed = false;

				_player.Velocity = velocity;
				_player.MoveAndSlide();
			}

			if (_player.IsOnFloor())
			{
				TryPlayLandingSound();
				if (Mathf.Abs(_player.Velocity.X) > 0.1f)
					stateMachine.TransitionTo("RunningMovementState");
				else
					stateMachine.TransitionTo("IdleMovementState");
			}
		}

		/// <summary>Procesa eventos de entrada mientras se está en Falling.</summary>
		/// <param name="ev">Evento de entrada recibido.</param>
		public override void HandleInput(InputEvent ev)
		{
			if (ev.IsActionPressed("jump") && _player.CoyoteTimeCounter > 0f)
			{
				stateMachine.TransitionTo("JumpingMovementState");
				return;
			}
		}

		private void TryPlayLandingSound()
		{
			if (_landingSoundPlayed)
				return;

			_landingSoundPlayed = true;
		}
	}
}
