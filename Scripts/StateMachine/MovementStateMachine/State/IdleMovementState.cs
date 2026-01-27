using Godot;
using System.Threading.Tasks;
using PlayerType = ForceOfHell.Scripts.MainCharacter.Player;

namespace ForceOfHell.Scripts.StateMachine.MovementStateMachine.State
{
    /// <summary>
    /// Estado de movimiento para el estado Idle del jugador.
    /// </summary>
    public partial class IdleMovementState : Scripts.StateMachine.State
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
            _player.SetAnimation("idle");
            _player.Velocity = _player.Velocity with { X = 0 };
            _player.MoveAndSlide();
        }

        public override void Update(double delta)
        {
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

        public override void HandleInput(InputEvent ev)
        {
            if (ev.IsActionPressed("jump") && _player.IsOnFloor())
                stateMachine.TransitionTo("JumpingMovementState");
        }
    }
}
