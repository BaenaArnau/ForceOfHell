using ForceOfHell.Scripts.MainCharacter;
using Godot;

namespace ForceOfHell.Scripts.Objects
{
    /// <summary>
    /// Represents a mana potion area that restores mana to a player upon entry.
    /// </summary>
    /// <remarks>The ManaPotion class inherits from Area2D and provides a bobbing visual effect. When a player
    /// enters the area, a predefined amount of mana is restored to the player, up to their maximum mana, and the potion
    /// is removed from the scene. This class is typically used as a collectible or power-up in gameplay
    /// scenarios.</remarks>
    public partial class ManaPotion : Area2D
    {
        private const float BobAmplitude = 6f;
        private const float BobSpeed = 2.5f;

        private Vector2 _basePosition;
        private float _bobTime;
        private int manaRegenerayion = 20;

        /// <summary>
        /// Initializes the node's state when it is added to the scene.
        /// </summary>
        /// <remarks>This method is called when the node is ready and can be used to set up initial values
        /// or states. It is typically overridden to perform any necessary setup before the node starts
        /// processing.</remarks>
        public override void _Ready()
        {
            _basePosition = Position;
            _bobTime = 0f;
        }

        /// <summary>
        /// Updates the object's position to create a vertical bobbing effect based on the elapsed time since the last
        /// frame.
        /// </summary>
        /// <remarks>The bobbing speed and amplitude are determined by the BobSpeed and BobAmplitude
        /// properties. This method should be called every frame to maintain smooth motion.</remarks>
        /// <param name="delta">The time, in seconds, that has passed since the previous frame. This value influences the calculation of the
        /// bobbing motion.</param>
        public override void _Process(double delta)
        {
            _bobTime += (float)delta * BobSpeed;
            var offsetY = Mathf.Sin(_bobTime) * BobAmplitude;
            Position = _basePosition + new Vector2(0f, offsetY);
        }

        /// <summary>
        /// Handles the event when a body enters the associated area and restores mana to the player if applicable.
        /// </summary>
        /// <remarks>This method is typically connected to an area or trigger in the scene. If the entering node is a
        /// player, the player's mana is increased by a predefined amount, up to the maximum allowed. The area is then removed
        /// from the scene.</remarks>
        /// <param name="body">The node that has entered the area. If the node is a player, its mana will be increased.</param>
        private void OnBodyEntered(Node body)
        {
            if (body is Player player)
            {
                player.manaActual += manaRegenerayion;
                if (player.manaActual > player.GetManaMax())
                    player.manaActual = player.GetManaMax();
                QueueFree();
            }
        }
    }
}
