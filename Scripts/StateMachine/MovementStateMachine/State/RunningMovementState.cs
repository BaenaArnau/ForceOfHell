using Godot;
using System;
using PlayerType = ForceOfHell.Scripts.MainCharacter.Player;

namespace ForceOfHell.Scripts.StateMachine.MovementStateMachine.State
{
	/// <summary>
	/// Estado de movimiento para la carrera del jugador.
	/// </summary>
	public partial class RunningMovementState : ForceOfHell.Scripts.StateMachine.State
	{
		/// <summary>
		/// Referencia al jugador.
		/// </summary>
		private PlayerType _player;

		/// <summary>
		/// Método llamado al iniciar el nodo.
		/// </summary>
		public override async System.Threading.Tasks.Task Ready()
		{
			_player = (PlayerType)GetTree().GetFirstNodeInGroup("MainCharacter");
			if (!_player.IsNodeReady())
				await ToSignal(_player, "ready");
		}

		/// <summary>Al entrar en Running: reproducir animación de carrera y preparar estado.</summary>
		public override void Enter()
		{
			_player.SetAnimation("run");
		}
		
		/// <summary>Actualización por frame del estado Running: controla transiciones por physics o input.</summary>
		/// <param name="delta">Delta en segundos.</param>
		public override void Update(double delta)
		{
			if (!_player.IsOnFloor())
			{
				if (_player.Velocity.Y < 0)
					stateMachine.TransitionTo("JumpingMovementState");
				else 
					stateMachine.TransitionTo("FallingMovementState");

				return;
			}
			if (Mathf.Abs((float)_player.Velocity.X) < 0.1f)
			{
				bool inputPressed = Input.IsActionPressed("move_left") || Input.IsActionPressed("move_right");
				if (!inputPressed)
					stateMachine.TransitionTo("IdleMovementState");
			}
		}

		/// <summary>Update de física en Running: aplica la velocidad horizontal en función del input.</summary>
		/// <param name="delta">Delta en segundos.</param>
		public override void UpdatePhysics(double delta)
		{
			float move = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
			if (Mathf.Abs(move) > 0f)
				_player.Velocity = new Vector2(move * PlayerType.Speed, _player.Velocity.Y);
			else
				_player.Velocity = new Vector2(0, _player.Velocity.Y);
			_player.MoveAndSlide();
		}

		/// <summary>Procesa input no manejado en Running (ej. pulsación de salto).</summary>
		/// <param name="ev">Evento de entrada recibido.</param>
		public override void HandleInput(InputEvent ev)
		{
			if (ev.IsActionPressed("jump") && _player.IsOnFloor())
				stateMachine.TransitionTo("JumpingMovementState");
		}
	}
}
