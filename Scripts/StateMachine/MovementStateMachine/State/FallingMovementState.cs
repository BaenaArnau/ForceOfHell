using Godot;
using System.Threading.Tasks;
using PlayerType = ForceOfHell.Scripts.MainCharacter.Player;

namespace ForceOfHell.Scripts.StateMachine.MovementStateMachine.State
{
    /// <summary>
    /// Estado de movimiento para la caída del jugador.
    /// </summary>
    public partial class FallingMovementState : Scripts.StateMachine.State
    {
        private const float LandingVelocityThreshold = 250f;

        private PlayerType _player;
        private bool _landingSoundPlayed;
        private float _lastVerticalVelocity;

        public override async Task Ready()
        {
            _player = (PlayerType)GetTree().GetFirstNodeInGroup("MainCharacter");
            if (!_player.IsNodeReady())
                await ToSignal(_player, "ready");
        }
        
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
                float move = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
                Vector2 velocity = _player.Velocity + _player.GetGravity() * (float)delta;
                velocity.X = Mathf.Abs(move) > 0f ? move * PlayerType.Speed : 0f;

                _lastVerticalVelocity = velocity.Y;
                _landingSoundPlayed = false;

                _player.Velocity = velocity;
                _player.MoveAndSlide();
                return;
            }

            TryPlayLandingSound();
            stateMachine.TransitionTo(Mathf.Abs(_player.Velocity.X) > 0.1f 
                ? "RunningMovementState" 
                : "IdleMovementState");
        }

        /// <summary>Procesa eventos de entrada mientras se está en Falling.</summary>
        /// <param name="ev">Evento de entrada recibido.</param>
        public override void HandleInput(InputEvent ev)
        {
            if (ev.IsActionPressed("jump") && _player.CoyoteTimeCounter > 0f)
                stateMachine.TransitionTo("JumpingMovementState");
        }

        private void TryPlayLandingSound()
        {
            if (_landingSoundPlayed)
                return;

            _landingSoundPlayed = true;
            // Aquí debería ir tu lógica de reproducción de sonido
            // if (_lastVerticalVelocity > LandingVelocityThreshold)
            //     _player.PlayLandingSound();
        }
    }
}
