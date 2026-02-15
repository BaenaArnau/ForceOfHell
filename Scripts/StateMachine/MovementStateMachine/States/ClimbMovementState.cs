using Godot;
using System;
using System.Threading.Tasks;
using PlayerType = ForceOfHell.Scripts.MainCharacter.Player;

namespace ForceOfHell.Scripts.StateMachine.MovementStateMachine.States
{
    /// <summary>
    /// Represents the climbing movement state of the player, managing interactions and physics during climbing.
    /// </summary>
    /// <remarks>This state handles the player's transition into climbing, updates movement based on input,
    /// and ensures proper alignment with climbing ladders. It also manages state transitions based on climbing
    /// conditions, such as initiating climbing, jumping, or returning to idle or running states.</remarks>
    public partial class ClimbMovementState : State
    {
        private const float ClimbSpeed = 120f;
        private PlayerType _player;

        /// <summary>
        /// Prepares the main character for interaction by ensuring it is fully initialized and ready within the scene.
        /// </summary>
        /// <remarks>Call this method before attempting to interact with the main character to guarantee
        /// it is properly initialized. The method waits asynchronously if the main character node is not yet ready,
        /// ensuring safe access and interaction.</remarks>
        /// <returns>A task that completes when the main character node is ready for interaction.</returns>
        public override async Task Ready()
        {
            _player = (PlayerType)GetTree().GetFirstNodeInGroup("MainCharacter");
            if (!_player.IsNodeReady())
                await ToSignal(_player, "ready");
        }

        /// <summary>
        /// Sets the player's state to begin climbing by aligning the player with the ladder and preparing for vertical
        /// movement.
        /// </summary>
        /// <remarks>Call this method when the player interacts with a ladder to initiate climbing. The
        /// player's velocity is reset and their position is adjusted to ensure proper alignment with the ladder before
        /// movement begins.</remarks>
        public override void Enter()
        {
            SnapToLadder();
            _player.Velocity = Vector2.Zero;
            _player.MoveAndSlide();
            //_player.SetAnimation("climb");
        }

        /// <summary>
        /// Updates the player's movement state based on the current conditions and player capabilities.
        /// </summary>
        /// <remarks>If the player is not able to climb and is on the floor, the method transitions the
        /// state machine to either 'RunningMovementState' or 'IdleMovementState' based on the player's horizontal
        /// velocity.</remarks>
        /// <param name="delta">The time elapsed since the last update, used to calculate movement changes.</param>
        public override void Update(double delta)
        {
            if (!_player.CanClimb)
            {
                if (_player.IsOnFloor())
                {
                    stateMachine.TransitionTo(Mathf.Abs(_player.Velocity.X) > 0.1f
                        ? "RunningMovementState"
                        : "IdleMovementState");
                }

                return;
            }
        }

        /// <summary>
        /// Updates the player's physics state during climbing, adjusting movement and animation based on input and
        /// climbing conditions.
        /// </summary>
        /// <remarks>If climbing is not permitted, the player's velocity is reset and sliding occurs. When
        /// climbing is allowed, vertical movement is determined by input, and the appropriate climbing or idle
        /// animation is set based on movement. This method should be called once per physics frame to ensure responsive
        /// climbing behavior.</remarks>
        /// <param name="delta">The time elapsed since the last frame, in seconds. Used to calculate movement adjustments for smooth physics
        /// updates.</param>
        public override void UpdatePhysics(double delta)
        {
            if (!_player.CanClimb)
            {
                _player.Velocity = Vector2.Zero;
                _player.MoveAndSlide();
                return;
            }

            SnapToLadder();

            float moveY = Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up");
            _player.Velocity = new Vector2(0f, moveY * ClimbSpeed);
            _player.MoveAndSlide();

            try
            {
                _player.SetAnimation(Mathf.Abs(moveY) > 0.01f ? "climb" : "idle");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Handles input events to initiate a jump action when the appropriate conditions are met.
        /// </summary>
        /// <remarks>Transitions the player to the 'JumpingMovementState' if the jump action is pressed
        /// and neither the move up nor move down actions are active. This ensures that jumping only occurs when the
        /// player is not climbing.</remarks>
        /// <param name="ev">The input event containing information about the player's current actions. Must not be null.</param>
        public override void HandleInput(InputEvent ev)
        {
            if (ev.IsActionPressed("jump") &&
                !Input.IsActionPressed("move_up") &&
                !Input.IsActionPressed("move_down"))
            {
                _player.Velocity = _player.Velocity with { Y = PlayerType.JumpVelocity };
                stateMachine.TransitionTo("JumpingMovementState");
            }
        }

        /// <summary>
        /// Aligns the player's horizontal position to the nearest climbing ladder anchor point to facilitate proper
        /// ladder interaction.
        /// </summary>
        /// <remarks>This method adjusts the player's global position to ensure they are correctly
        /// positioned for climbing. It is typically called when the player is near a ladder to enable smooth
        /// transitions onto the ladder.</remarks>
        private void SnapToLadder()
        {
            var pos = _player.GlobalPosition;
            pos.X = _player.ClimbAnchorX;
            _player.GlobalPosition = pos;
        }
    }
}