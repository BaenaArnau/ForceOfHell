using Godot;
using System.Threading.Tasks;
using ForceOfHell.Scripts.StateMachine.MovementStateMachine;
using PlayerType = ForceOfHell.Scripts.MainCharacter.Player;

namespace ForceOfHell.Scripts.StateMachine.MovementStateMachine.States
{
    /// <summary>
    /// Estado de movimiento para la carrera del jugador.
    /// </summary>
    public partial class RunningMovementState : State
    {
        private PlayerType _player;

        public override async Task Ready()
        {
            _player = (PlayerType)GetTree().GetFirstNodeInGroup("MainCharacter");
            if (!_player.IsNodeReady())
                await ToSignal(_player, "ready");
        }

        public override void Enter() => _player.SetAnimation("run");
        
        public override void Update(double delta)
        {
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

        public override void UpdatePhysics(double delta)
        {
            float move = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
            _player.Velocity = _player.Velocity with { X = Mathf.Abs(move) > 0f ? move * PlayerType.Speed : 0 };
            _player.MoveAndSlide();
        }

        public override void HandleInput(InputEvent ev)
        {
            if (ev.IsActionPressed("jump") && _player.IsOnFloor())
                stateMachine.TransitionTo("JumpingMovementState");
        }
    }
}
