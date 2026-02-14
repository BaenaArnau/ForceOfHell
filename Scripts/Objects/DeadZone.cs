using ForceOfHell.Scripts.Enemies;
using ForceOfHell.Scripts.MainCharacter;
using Godot;


namespace ForceOfHell.Scripts.Objects
{
    /// <summary>
    /// Represents an area within the scene that eliminates player characters and certain enemies upon entry.
    /// </summary>
    /// <remarks>This class is typically used to define zones that trigger elimination behavior for players
    /// and specific enemy types when they enter. After eliminating a player, the area removes itself from the scene. It
    /// is commonly connected to area triggers in game scenes to enforce boundaries or hazards.</remarks>
    public partial class DeadZone : Area2D
    {
        /// <summary>
        /// Handles the event when a body enters the associated area and triggers the appropriate response if the body
        /// is a player.
        /// </summary>
        /// <remarks>This method is typically connected to an area or trigger in the scene. If the
        /// entering body is a player, the player will be eliminated and the area will remove itself from the
        /// scene.</remarks>
        /// <param name="body">The node that has entered the area. If this node is a player, the player will be affected accordingly.</param>
        public void OnBodyEntered(Node body)
        {
            if (body is Player player)
            {
                _ = player.Die();
                QueueFree();
            }
            if (body is Balrog b)
                b.QueueFree();

            if (body is Skeleton s)
                s.QueueFree();

            if (body is Goblin g)
                g.QueueFree();
        }
    }
}
