using Godot;

namespace ForceOfHell.Scripts.MainCharacter
{
    /// <summary>
    /// Represents a progress bar control that displays the player's current health as a percentage.
    /// </summary>
    /// <remarks>This control is designed to visually indicate the player's health status in the game. The
    /// value is updated based on the player's health percentage, which should range from 0 to 100. Use this control to
    /// provide real-time feedback on health changes during gameplay.</remarks>
    public partial class HealthProgressBar : ProgressBar
    {
        [Export] public Player _player;

        /// <summary>
        /// Updates the health progress bar value based on the player's current health percentage.
        /// </summary>
        /// <remarks>This method assigns the player's health percentage to the progress bar's value. The
        /// health is expected to be a value between 0 and 100, representing the player's current health
        /// state.</remarks>
        /// <param name="delta">The time elapsed, in seconds, since the last frame. Used to synchronize health updates with the game loop.</param>
        public override void _Process(double delta)
        {
            Value = _player.healthActual * 100 / 100;
        }
    }
}
