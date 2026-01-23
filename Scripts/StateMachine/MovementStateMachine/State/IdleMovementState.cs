using Godot;
using System;
using PlayerType = ForceOfHell.Scripts.MainCharacter.Player;

namespace ForceOfHell.Scripts.StateMachine.MovementStateMachine.State
{
    /// <summary>
    /// Estado de movimiento para el estado Idle del jugador.
    /// </summary>
    public partial class IdleMovementState : ForceOfHell.Scripts.StateMachine.State
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
        
        /// <summary>
        /// Al entrar en el estado Idle: establece la animación y detiene la velocidad horizontal.
        /// También resetea el doble salto porque estamos en suelo.
        /// </summary>
        public override void Enter()
        {
            _player.SetAnimation("idle");
            _player.Velocity = new Vector2(0, _player.Velocity.Y);
            _player.MoveAndSlide();
        }

        /// <summary>Actualización por frame del estado Idle: decide transiciones en base a input/physics.</summary>
        /// <param name="delta">Delta en segundos desde el último frame.</param>
        public override void Update(double delta)
        {
            if (!_player.IsOnFloor())
            {
                if (_player.Velocity.Y < 0)
                    stateMachine.TransitionTo("JumpingMovementState");
                else
                    stateMachine.TransitionTo("FallingMovementState");
            }
            
            if (Input.IsActionPressed("move_left") || Input.IsActionPressed("move_right"))
            {
                stateMachine.TransitionTo("RunningMovementState");
                return;
            }

            if (Mathf.Abs(_player.Velocity.X) > 0)
                stateMachine.TransitionTo("RunningMovementState");
        }

        /// <summary>Procesa eventos de entrada no manejados cuando estamos en Idle.</summary>
        /// <param name="ev">Evento de entrada recibido.</param>
        public override void HandleInput(InputEvent ev)
        {
            if (ev.IsActionPressed("move_left") || ev.IsActionPressed("move_right"))
                stateMachine.TransitionTo("RunningMovementState");
            if (ev.IsActionPressed("jump") && _player.IsOnFloor())
                stateMachine.TransitionTo("JumpingMovementState");
        }
    }
}
