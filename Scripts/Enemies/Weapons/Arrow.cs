using ForceOfHell.Scripts.MainCharacter;
using Godot;

namespace ForceOfHell.Scripts.Enemies.Weapons
{
    /// <summary>
    /// Represents an arrow projectile in a 2D physics environment that can interact with players and tile map layers.
    /// </summary>
    /// <remarks>The Arrow class inherits from RigidBody2D and uses an AnimatedSprite2D for its visual
    /// representation. It can be configured with direction and speed, and handles collision events to apply damage to
    /// Player instances or remove itself upon contact with TileMapLayer nodes. Use the Configure methods to set the
    /// arrow's movement and animation before launching.</remarks>
    public partial class Arrow : RigidBody2D
    {
        private AnimatedSprite2D _animatedSprite;

        private Area2D _arrowDamage;
        public string animation = string.Empty;
        private Vector2 _direction = Vector2.Zero;

        [Export] private CollisionShape2D _collisionShape;

        /// <summary>
        /// Initializes the node when it enters the scene tree and prepares it for use.
        /// </summary>
        /// <remarks>This method is called automatically by the engine when the node is added to the
        /// scene. It retrieves the child node named "AnimatedSprite2D" as an AnimatedSprite2D instance. Ensure that the
        /// scene hierarchy includes an AnimatedSprite2D child to avoid null references.</remarks>
        public override void _Ready()
        {
            _animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        }

        /// <summary>
        /// Handles the event when an arrow collides with a body, applying damage to a player if applicable and removing
        /// the arrow from the scene.
        /// </summary>
        /// <remarks>If the collided body is a player, this method applies a fixed amount of damage. In
        /// all cases, the arrow instance is removed from the scene after the collision.</remarks>
        /// <param name="body">The Node2D instance representing the body that the arrow has collided with. This can be a player character
        /// or a tile map layer.</param>
        public void OnArrowDamageBodyEntered(Node2D body)
        {
            if (body is Player player)
            {
                _ = player.HitAsync(10);
                QueueFree();
            }
            else if (body is TileMapLayer)
                QueueFree();
        }

        /// <summary>
        /// Configures the movement direction and speed of the animated sprite.
        /// </summary>
        /// <remarks>If an animated sprite is present, its horizontal flip state is set based on the
        /// direction to ensure correct visual orientation.</remarks>
        /// <param name="direction">The direction of movement. A negative value moves the sprite to the left; a positive value moves it to the
        /// right.</param>
        /// <param name="speed">The speed at which the sprite moves in the specified direction. Must be a non-negative value.</param>
        public void Configure(int direction, float speed)
        {
            AnimatedSprite2D animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
            if (animatedSprite != null)
                animatedSprite.FlipH = direction < 0;

            LinearVelocity = new Vector2(direction * speed, 0f);
        }

        /// <summary>
        /// Configures the object's movement direction and applies the specified animation.
        /// </summary>
        /// <remarks>If the direction vector is not zero, the object's rotation is updated to align with
        /// the specified direction.</remarks>
        /// <param name="direction">The direction vector that determines the object's orientation. If the vector is zero, the direction remains
        /// unchanged.</param>
        /// <param name="animation">The name of the animation to apply to the object during movement.</param>
        public void Configure(Vector2 direction, string animation)
        {
            this.animation = animation;
            _direction = direction == Vector2.Zero ? Vector2.Zero : direction.Normalized();

            if (_direction != Vector2.Zero)
                Rotation = _direction.Angle();
        }
    }
}