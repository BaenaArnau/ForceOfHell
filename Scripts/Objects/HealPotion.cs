using ForceOfHell.Scripts.MainCharacter;
using Godot;

namespace ForceOfHell.Scripts.Objects
{
    /// <summary>
    /// Represents a healing potion that restores health to players when they enter its area of effect.
    /// </summary>
    /// <remarks>The healing potion animates with a bobbing motion and applies a fixed amount of health
    /// restoration to players who enter its area. The healing is capped at the player's maximum health, and the potion
    /// is removed from the scene after use.</remarks>
    public partial class HealPotion : Area2D
    {
        private const float BobAmplitude = 6f;
        private const float BobSpeed = 2.5f;

        private Vector2 _basePosition;
        private float _bobTime;
        private int healRegenerayion = 20;

        /// <summary>
        /// Initializes the node's state when it is added to the scene.
        /// </summary>
        /// <remarks>This method sets the initial position of the node and resets the bobbing time to
        /// zero. It is called automatically when the node is ready.</remarks>
        public override void _Ready()
        {
            _basePosition = Position;
            _bobTime = 0f;
        }

        /// <summary>
        /// Updates the object's position to create a vertical bobbing effect based on the elapsed time since the last
        /// frame.
        /// </summary>
        /// <remarks>The bobbing effect is determined by the BobSpeed and BobAmplitude properties. Adjust
        /// these properties to control the speed and height of the motion. This method is typically called every frame
        /// to animate the object smoothly.</remarks>
        /// <param name="delta">The time, in seconds, since the previous frame. This value influences the calculation of the bobbing motion.</param>
        public override void _Process(double delta)
        {
            _bobTime += (float)delta * BobSpeed;
            var offsetY = Mathf.Sin(_bobTime) * BobAmplitude;
            Position = _basePosition + new Vector2(0f, offsetY);
        }

        /// <summary>
        /// Handles the event when a body enters the area and applies healing to the player if applicable.
        /// </summary>
        /// <remarks>This method is typically connected to an area or trigger's body entered signal. If the entering
        /// node is a player, the player's health is increased by the configured amount, up to the maximum allowed. The healing
        /// area is then removed from the scene.</remarks>
        /// <param name="body">The node that has entered the area. If the node is a player, healing is applied.</param>
        private void OnBodyEntered(Node body)
        {
            if (body is Player player)
            {
                player.healthActual += healRegenerayion;
                if (player.healthActual > player.GetHealthMax())
                    player.healthActual = player.GetHealthMax();
                QueueFree();
            }
        }
    }
}
