using Godot;
using System.Threading.Tasks;
using PlayerType = ForceOfHell.Scripts.MainCharacter.Player;

namespace ForceOfHell.Scripts.StateMachine.MovementStateMachine.States
{
	/// <summary>
	/// Estado de movimiento para subir/bajar escaleras.
	/// </summary>
	public partial class ClimbMovementState : State
	{
		private const float ClimbSpeed = 120f;
		private PlayerType _player;

		public override async Task Ready()
		{
			_player = (PlayerType)GetTree().GetFirstNodeInGroup("MainCharacter");
			if (!_player.IsNodeReady())
				await ToSignal(_player, "ready");
		}

		public override void Enter()
		{
			SnapToLadder();
			_player.Velocity = Vector2.Zero;
			_player.MoveAndSlide();
			_player.SetAnimation("climb");
		}

		public override void Update(double delta)
		{
			if (!_player.CanClimb)
			{
				stateMachine.TransitionTo(_player.IsOnFloor()
					? (Mathf.Abs(_player.Velocity.X) > 0.1f ? "RunningMovementState" : "IdleMovementState")
					: "FallingMovementState");
			}
		}

		public override void UpdatePhysics(double delta)
		{
			SnapToLadder();

			float moveY = Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up");
			_player.Velocity = new Vector2(0f, moveY * ClimbSpeed);
			_player.MoveAndSlide();

			_player.SetAnimation(Mathf.Abs(moveY) > 0.01f ? "climb" : "idle");
		}

		public override void HandleInput(InputEvent ev)
		{
			if (ev.IsActionPressed("jump"))
			{
				_player.Velocity = _player.Velocity with { Y = PlayerType.JumpVelocity };
				stateMachine.TransitionTo("JumpingMovementState");
			}
		}

		private void SnapToLadder()
		{
			var pos = _player.GlobalPosition;
			pos.X = _player.ClimbAnchorX;
			_player.GlobalPosition = pos;
		}
	}
}