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

        public override async Task Ready()
        {
            _player = (PlayerType)GetTree().GetFirstNodeInGroup("MainCharacter");
            if (!_player.IsNodeReady())
                await ToSignal(_player, "ready");
        }

        public override void Enter()
        {
            _player.SetAnimation("jump");
            _player.CoyoteTimeCounter = 0f;
            _player.Velocity = _player.Velocity with { Y = PlayerType.JumpVelocity };
            _player.MoveAndSlide();
            _player.EmitSignal(nameof(PlayerType.InJumping));
        }

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
