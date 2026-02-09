using Godot;
using System;
using ForceOfHell.Scripts.MainCharacter;

namespace ForceOfHell.Scripts.Objects
{
	public partial class Door : Area2D
	{
		/// <summary>
		/// Gets or sets the AnimatedSprite2D instance associated with this component.
		/// </summary>
		[Export] public AnimatedSprite2D _animatedSprite2D;

		/// <summary>
		/// Handles the event when a body enters the associated area or trigger.
		/// </summary>
		/// <param name="body">The node representing the body that has entered. Typically used to determine if the entering body is of a specific
		/// type.</param>
		private void OnBodyEntered(Node body)
		{
			if (body is Player)
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
			if (body is Player)
				_animatedSprite2D.Frame = 0;
		}
	}
}
