using Godot;
using System.Threading.Tasks;
using PlayerType = ForceOfHell.Scripts.MainCharacter.Player;

namespace ForceOfHell.Scripts.StateMachine.MovementStateMachine.States
{
    /// <summary>
    /// Estado de movimiento para la carrera del jugador.
    /// </summary>
    public partial class RunningMovementState : State
    {
        private PlayerType _player;

        /// <summary>
        /// Prepares the main character for interaction by ensuring it is ready within the game environment.
        /// </summary>
        /// <remarks>If the main character is not yet ready, this method waits for the 'ready' signal
        /// before proceeding. This ensures that subsequent operations involving the main character are performed only
        /// when it is fully initialized.</remarks>
        /// <returns>A task that represents the asynchronous operation of waiting for the main character to become ready.</returns>
        public override async Task Ready()
        {
            _player = (PlayerType)GetTree().GetFirstNodeInGroup("MainCharacter");
            if (!_player.IsNodeReady())
                await ToSignal(_player, "ready");
        }

        /// <summary>
        /// Transitions the player character into the running state by triggering the running animation.
        /// </summary>
        /// <remarks>Call this method when the player should begin running. Ensure that the player's
        /// current state allows for animation changes before invoking this method to avoid unexpected
        /// behavior.</remarks>
        public override void Enter() => _player.SetAnimation("run");

        /// <summary>
        /// Updates the player's movement state based on input and current conditions.
        /// </summary>
        /// <remarks>This method transitions the player's state between climbing, jumping, falling, and
        /// idle based on player input and whether the player is on the ground.</remarks>
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

            bool isMoving = Input.IsActionPressed("move_left") || Input.IsActionPressed("move_right");
            if (Mathf.Abs(_player.Velocity.X) < 0.1f && !isMoving)
                stateMachine.TransitionTo("IdleMovementState");
        }

        /// <summary>
        /// Updates the player's physics state based on input actions for movement.
        /// </summary>
        /// <remarks>This method modifies the player's velocity based on the input strength for moving
        /// left or right. It ensures that the player moves at a speed defined by the PlayerType, and calls MoveAndSlide
        /// to apply the movement.</remarks>
        /// <param name="delta">The time elapsed since the last frame, used to calculate movement adjustments.</param>
        public override void UpdatePhysics(double delta)
        {
            float move = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
            _player.Velocity = _player.Velocity with { X = Mathf.Abs(move) > 0f ? move * PlayerType.Speed : 0 };
            _player.MoveAndSlide();
        }

        /// <summary>
        /// Handles input events and transitions the player to a new movement state based on the received input.
        /// </summary>
        /// <remarks>If the jump action is pressed while the player is on the floor, this method
        /// transitions the state machine to the jumping movement state.</remarks>
        /// <param name="ev">The input event containing information about the action being performed, such as whether the jump action is
        /// pressed.</param>
        public override void HandleInput(InputEvent ev)
        {
            if (ev.IsActionPressed("jump") && _player.IsOnFloor())
                stateMachine.TransitionTo("JumpingMovementState");
        }
    }
}
