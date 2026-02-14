using Godot;
using System.Threading.Tasks;
using PlayerType = ForceOfHell.Scripts.MainCharacter.Player;

namespace ForceOfHell.Scripts.StateMachine.MovementStateMachine.States
{
    /// <summary>
    /// Estado de movimiento para el salto del jugador.
    /// </summary>
    public partial class JumpingMovementState : State
    {
        private PlayerType _player;

        /// <summary>
        /// Prepares the main character for interaction by ensuring it is ready within the game environment.
        /// </summary>
        /// <remarks>If the main character is not yet ready, this method waits for the 'ready' signal
        /// before proceeding. This ensures that subsequent interactions with the main character occur only after it is
        /// fully initialized.</remarks>
        /// <returns>A task that represents the asynchronous operation of waiting for the main character to become ready.</returns>
        public override async Task Ready()
        {
            _player = (PlayerType)GetTree().GetFirstNodeInGroup("MainCharacter");
            if (!_player.IsNodeReady())
                await ToSignal(_player, "ready");
        }

        /// <summary>
        /// Transitions the player into the jumping state, initializing the jump animation and velocity.
        /// </summary>
        /// <remarks>Call this method when the player is intended to begin a jump. This ensures the
        /// correct animation, velocity, and state signaling are applied for consistent gameplay and physics
        /// behavior.</remarks>
        public override void Enter()
        {
            _player.SetAnimation("jump");
            _player.CoyoteTimeCounter = 0f;
            _player.Velocity = _player.Velocity with { Y = PlayerType.JumpVelocity };
            _player.MoveAndSlide();
            _player.EmitSignal(nameof(PlayerType.InJumping));
        }

        /// <summary>
        /// Updates the player's movement state based on the current input and velocity.
        /// </summary>
        /// <remarks>This method transitions the player's state to climbing, falling, running, or idle
        /// based on the player's current conditions and input actions.</remarks>
        /// <param name="delta">The time elapsed since the last update, in seconds, used to calculate movement changes.</param>
        public override void Update(double delta)
        {
            if (_player.CanClimb && (Input.IsActionPressed("move_up") || Input.IsActionPressed("move_down")))
            {
                stateMachine.TransitionTo("ClimbMovementState");
                return;
            }

            if (_player.Velocity.Y >= 0)
            {
                stateMachine.TransitionTo("FallingMovementState");
                return;
            }

            if (_player.IsOnFloor())
            {
                stateMachine.TransitionTo(Mathf.Abs(_player.Velocity.X) > 0.1f
                    ? "RunningMovementState"
                    : "IdleMovementState");
            }
        }

        /// <summary>
        /// Updates the player's physics state based on the elapsed time since the last frame.
        /// </summary>
        /// <remarks>This method adjusts the player's velocity based on input actions for moving left and
        /// right, and applies gravity if the player is not on the floor. It ensures that the player's movement is
        /// smooth and responsive to user input.</remarks>
        /// <param name="delta">The time, in seconds, since the last update. Used to calculate the player's velocity and movement.</param>
        public override void UpdatePhysics(double delta)
        {
            if (_player.IsOnFloor())
                return;

            float move = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
            Vector2 velocity = _player.Velocity + _player.GetGravity() * (float)delta;
            velocity.X = Mathf.Abs(move) > 0f ? move * PlayerType.Speed : 0f;

            _player.Velocity = velocity;
            _player.MoveAndSlide();
        }
    }
}
