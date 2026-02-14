using Godot;
using System.Threading.Tasks;
using PlayerType = ForceOfHell.Scripts.MainCharacter.Player;

namespace ForceOfHell.Scripts.StateMachine.MovementStateMachine.States
{
    /// <summary>
    /// Estado de movimiento para el estado Idle del jugador.
    /// </summary>
    public partial class IdleMovementState : State
    {
        private PlayerType _player;

        /// <summary>
        /// Initializes the player character by retrieving the first node in the 'MainCharacter' group and ensures it is
        /// ready for interaction.
        /// </summary>
        /// <remarks>If the player character is not ready, the method awaits a signal indicating that the
        /// player is ready before proceeding. This ensures that subsequent operations on the player character are safe
        /// and reliable.</remarks>
        /// <returns>A task that represents the asynchronous operation of preparing the player character for interaction.</returns>
        public override async Task Ready()
        {
            _player = (PlayerType)GetTree().GetFirstNodeInGroup("MainCharacter");
            if (!_player.IsNodeReady())
                await ToSignal(_player, "ready");
        }

        /// <summary>
        /// Transitions the player character into the idle movement state, resetting horizontal velocity and updating
        /// the animation to idle.
        /// </summary>
        /// <remarks>Call this method when the player should appear stationary and not respond to movement
        /// input. The player's horizontal movement is halted, and the idle animation is displayed. This ensures the
        /// character does not slide or move unintentionally while idle.</remarks>
        public override void Enter()
        {
            _player.SetAnimation("idle");
            _player.Velocity = _player.Velocity with { X = 0 };
            _player.MoveAndSlide();
        }

        /// <summary>
        /// Updates the player's movement state based on input and current conditions.
        /// </summary>
        /// <remarks>This method transitions the player to different movement states such as climbing,
        /// jumping, falling, or running based on player input and whether the player is on the ground.</remarks>
        /// <param name="delta">The time elapsed since the last update, in seconds, used to calculate movement changes.</param>
        public override void Update(double delta)
        {
            if (_player.CanClimb && (Input.IsActionPressed("move_up") || Input.IsActionPressed("move_down")))
            {
                stateMachine.TransitionTo("ClimbMovementState");
                return;
            }

            if (!_player.IsOnFloor())
            {
                stateMachine.TransitionTo(_player.Velocity.Y < 0
                    ? "JumpingMovementState"
                    : "FallingMovementState");
                return;
            }

            if (Input.IsActionPressed("move_left") ||
                Input.IsActionPressed("move_right") ||
                Mathf.Abs(_player.Velocity.X) > 0)
            {
                stateMachine.TransitionTo("RunningMovementState");
            }
        }

        /// <summary>
        /// Handles input events and transitions the player to the jumping movement state when the jump action is
        /// pressed and the player is on the floor.
        /// </summary>
        /// <remarks>This method only initiates a jump if the player is currently on the floor and the
        /// jump action is pressed. It is typically called in response to user input during the idle movement
        /// state.</remarks>
        /// <param name="ev">The input event containing information about the player's actions, such as button presses or movement
        /// commands.</param>
        public override void HandleInput(InputEvent ev)
        {
            if (ev.IsActionPressed("jump") && _player.IsOnFloor())
                stateMachine.TransitionTo("JumpingMovementState");
        }
    }
}
