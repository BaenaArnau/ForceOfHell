using Godot;

namespace ForceOfHell.Scripts.MainCharacter
{
    public partial class ManaProgressBar : ProgressBar
    {
        [Export] public Player _player;
        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            Value = _player.manaActual * 100 / 100;
        }
    }
}