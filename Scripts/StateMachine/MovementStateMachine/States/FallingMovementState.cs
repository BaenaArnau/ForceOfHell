using Godot;
using System.Threading.Tasks;
using PlayerType = ForceOfHell.Scripts.MainCharacter.Player;

namespace ForceOfHell.Scripts.StateMachine.MovementStateMachine.States
{
    /// <summary>
    /// Estado de movimiento para la caída del jugador.
    /// </summary>
    public partial class FallingMovementState : State
    {
        private const float LandingVelocityThreshold = 250f;

        private PlayerType _player;
        private bool _landingSoundPlayed;
        private float _lastVerticalVelocity;

        /// <summary>
        /// Prepares the main character for interaction by ensuring it is ready within the game environment.
        /// </summary>
        /// <remarks>If the main character is not yet ready, this method asynchronously waits for the
        /// 'ready' signal before proceeding. This ensures that subsequent operations involving the main character are
        /// performed only when it is fully initialized.</remarks>
        /// <returns>A task that represents the asynchronous operation of waiting for the main character to become ready.</returns>
        public override async Task Ready()
        {
            _player = (PlayerType)GetTree().GetFirstNodeInGroup("MainCharacter");
            if (!_player.IsNodeReady())
                await ToSignal(_player, "ready");
        }

        /// <summary>
        /// Transitions the player into the falling movement state and initializes relevant state variables and
        /// animation.
        /// </summary>
        /// <remarks>This method resets the landing sound flag and vertical velocity, and sets the
        /// player's animation to indicate a falling state. It should be called when the player begins to fall, ensuring
        /// that the state is properly initialized for subsequent movement and collision handling.</remarks>
        public override void Enter()
        {
            _landingSoundPlayed = false;
            _lastVerticalVelocity = 0f;
            _player.SetAnimation("fall");
        }

        /// <summary>
        /// Resets the state of the movement when exiting the falling state, ensuring that landing-related flags and
        /// velocities are cleared.
        /// </summary>
        /// <remarks>Call this method when transitioning out of the falling movement state to prevent
        /// unintended landing effects and to prepare for subsequent state changes.</remarks>
        public override void Exit()
        {
            _landingSoundPlayed = false;
            _lastVerticalVelocity = 0f;
        }

        /// <summary>Actualización por frame en Falling: transiciones al aterrizar o doble salto.</summary>
        /// <param name="delta">Delta en segundos.</param>
        public override void Update(double delta)
        {
            if (_player.CanClimb && (Input.IsActionPressed("move_up") || Input.IsActionPressed("move_down")))
            {
                stateMachine.TransitionTo("ClimbMovementState");
                return;
            }

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

        /// <summary>
        /// Attempts to play the landing sound if it has not already been played.
        /// </summary>
        /// <remarks>This method checks if the landing sound has already been played to prevent it from
        /// being triggered multiple times. It is intended to be called when a landing event occurs.</remarks>
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
