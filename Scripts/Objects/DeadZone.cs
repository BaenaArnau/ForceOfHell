using Godot;
using System;
using ForceOfHell.Scripts.MainCharacter;


namespace ForceOfHell.Scripts.Objects
{
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
        }
    }
}
