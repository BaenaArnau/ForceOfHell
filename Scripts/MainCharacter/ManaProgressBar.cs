using Godot;

namespace ForceOfHell.Scripts.MainCharacter
{
    /// <summary>
    /// Represents a progress bar that visually indicates the player's current mana level.
    /// </summary>
    /// <remarks>This class updates its displayed value based on the player's actual mana, reflecting changes
    /// in real-time during gameplay.</remarks>
    public partial class ManaProgressBar : ProgressBar
    {
        [Export] public Player _player;

        /// <summary>
        /// Processes the current frame, updating the mana value based on the player's current mana.
        /// </summary>
        /// <param name="delta">The time elapsed, in seconds, since the last frame. Typically used to perform time-based updates.</param>
        public override void _Process(double delta)
        {
            Value = _player.manaActual * 100 / 100;
        }
    }
}