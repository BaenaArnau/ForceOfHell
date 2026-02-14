using ForceOfHell.Scripts.Enemies;
using ForceOfHell.Scripts.MainCharacter;
using Godot;

namespace ForceOfHell.Scripts.Objects
{
    /// <summary>
    /// Represents a door area that detects when bodies enter or exit and provides visual feedback using an associated
    /// AnimatedSprite2D instance.
    /// </summary>
    /// <remarks>The Door class is designed to be used in interactive environments where visual state changes
    /// are triggered by bodies entering or exiting its area. It can be connected to signals for area entry and exit,
    /// allowing for dynamic animation updates based on player or object interactions. The AnimatedSprite2D property
    /// should be assigned to enable visual feedback.</remarks>
    public partial class Door : Area2D
    {
        /// <summary>
        /// Gets or sets the AnimatedSprite2D instance associated with this component.
        /// </summary>
        [Export] public AnimatedSprite2D _animatedSprite2D;

        /// <summary>
        /// Initializes the node when it is added to the scene and sets the animated sprite to its first frame.
        /// </summary>
        /// <remarks>This method is called automatically by the engine when the node enters the scene
        /// tree. It is typically used to perform setup tasks before the node is displayed, such as initializing
        /// properties or preparing child nodes.</remarks>
        public override void _Ready()
        {
            _animatedSprite2D.Frame = 0;
        }

        /// <summary>
        /// Handles the event when a body enters the associated area or trigger.
        /// </summary>
        /// <param name="node">The node representing the body that has entered. Typically used to determine if the entering body is of a specific
        /// type.</param>
        private void OnBodyEntered(Node node)
        {
            if (node is Balrog balrog)
                _animatedSprite2D.Frame = 1;

            if (node is Skeleton s)
                _animatedSprite2D.Frame = 1;

            if (node is Goblin g)
                _animatedSprite2D.Frame = 1;

            if (node is Player p)
                _animatedSprite2D.Frame = 1;
        }

        /// <summary>
        /// Handles logic when a body exits the associated area or trigger.
        /// </summary>
        /// <remarks>This method is intended to be connected to an exit signal, such as when a physics body leaves an
        /// area. If the exiting body is a player, the animation frame is reset. This can be used to update visual state or
        /// trigger additional logic when specific objects leave the area.</remarks>
        /// <param name="body">The node representing the body that has exited. Typically used to determine if the exiting body is of a specific
        /// type.</param>
        private void OnBodyExited(Node body)
        {
            _animatedSprite2D.Frame = 0;
        }
    }
}
