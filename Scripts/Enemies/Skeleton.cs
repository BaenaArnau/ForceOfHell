using ForceOfHell.Scripts.Enemies.Weapons;
using ForceOfHell.Scripts.MainCharacter;
using Godot;
using System;
using System.Threading.Tasks;

namespace ForceOfHell.Scripts.Enemies
{
    /// <summary>
    /// Represents a skeleton enemy character in a 2D game that can detect, pursue, and attack the player.
    /// </summary>
    /// <remarks>The Skeleton class inherits from CharacterBody2D and provides functionality for vision and
    /// attack range detection, health management, and interaction with player actions. It uses areas to determine when
    /// the player is within vision or attack range and responds accordingly. The class is designed to be used within a
    /// Godot game environment, supporting movement, attack animations, and projectile instantiation.</remarks>
    public partial class Skeleton : CharacterBody2D
    {
        private AnimatedSprite2D _animatedSprite;
        private Area2D _detectionArea;
        private Area2D _attackArea;
        private Player _player;
        private Area2D _damageArea;
        public bool IsOnVisionRange { get; private set; }
        public bool IsOnAttackRange { get; private set; }
        public const float JumpVelocity = -400f;
        private bool _attackedThisFrame = false;
        [Export] public PackedScene Arrow { get; set; }
        private const int Health = 5;
        public float CurrentHealth { get; set; } = Health;
        public bool TakeDamage = false;

        /// <summary>
        /// Initializes the node and retrieves references to its child nodes for animation and detection functionality.
        /// </summary>
        /// <remarks>This method is called when the node is added to the scene. It ensures that the
        /// required child nodes for animated sprite and area detection are available for subsequent interactions. If
        /// any child node is missing, the corresponding reference will be null.</remarks>
        public override void _Ready()
        {
            _animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
            _detectionArea = GetNodeOrNull<Area2D>("DetectionArea");
            _attackArea = GetNodeOrNull<Area2D>("AttackArea");
            _damageArea = GetNodeOrNull<Area2D>("DamageArea");
        }

        /// <summary>
        /// Processes the current animation frame and updates the attack state based on the elapsed time.
        /// </summary>
        /// <remarks>Sets the internal attack state when the animation is 'Attack' and the frame is 1.
        /// Resets the state if the animation changes or the frame is not 1.</remarks>
        /// <param name="delta">The time, in seconds, that has elapsed since the previous frame. Used to synchronize animation and state
        /// updates.</param>
        public override void _Process(double delta)
        {
            if (_animatedSprite.Animation == "Attack" && _animatedSprite.Frame == 1)
            {
                if (!_attackedThisFrame)
                    _attackedThisFrame = true;
            }
            else
                _attackedThisFrame = false;
        }

        /// <summary>
        /// Updates the physics state of the skeleton enemy based on the elapsed time, applying gravity, handling attack
        /// and movement animations, and managing interactions with the environment.
        /// </summary>
        /// <remarks>This method manages the skeleton's behavior when within attack and vision ranges,
        /// triggering appropriate animations and instantiating projectiles as needed. It also handles collision
        /// responses to ensure correct movement and jumping mechanics.</remarks>
        /// <param name="delta">The time elapsed since the last frame, in seconds, used to calculate movement and physics updates.</param>
        public override void _PhysicsProcess(double delta)
        {
            Velocity += GetGravity() * (float)delta;

            UpdateSpriteDirection();

            if (IsOnAttackRange && _animatedSprite.Animation != "Attack")
            {
                _animatedSprite.Play("Attack");
                Velocity = Velocity with { X = 0f };

                Arrow arrowInstance = Arrow.Instantiate<Arrow>();
                arrowInstance.Position = GlobalPosition + new Vector2(_animatedSprite.FlipH ? -30f : 30f, 0f);
                arrowInstance.Configure(_animatedSprite.FlipH ? -1 : 1, 800f);
                GetParent().AddChild(arrowInstance);
                GD.Print("Skeleton_1 Attacked");
            }

            if (IsOnVisionRange && _animatedSprite.Animation != "Attack")
            {
                _animatedSprite.Play("Walk");
                Velocity = Velocity with { X = (_player.GlobalPosition.X < GlobalPosition.X ? -1 : 1) * 100f };

                for (int i = 0; IsOnFloor() && i < GetSlideCollisionCount(); i++)
                {
                    KinematicCollision2D collision = GetSlideCollision(i);
                    if (collision.GetCollider() is TileMapLayer tileMap)
                    {
                        Vector2 collisionNormal = collision.GetNormal();
                        if (Math.Abs(collisionNormal.X) > 0.5f)
                            Velocity = Velocity with { Y = JumpVelocity };
                    }
                }
            }

            MoveAndSlide();
        }

        /// <summary>
        /// Processes the specified amount of damage, reducing the current health accordingly and marking the object for
        /// freeing if health is depleted.
        /// </summary>
        /// <remarks>If the resulting health is less than or equal to zero after applying damage, the
        /// current health is set to zero and the object is queued for freeing.</remarks>
        /// <param name="damage">The amount of damage to apply to the current health. Must be a positive value.</param>
        /// <returns></returns>
        internal async Task DamageDealt(float damage)
        {
            if ((CurrentHealth - damage) <= 0)
            {
                CurrentHealth = 0;
                QueueFree();
            }
            else
                CurrentHealth -= damage;
        }

        /// <summary>
        /// Handles the event when a body enters the damage area. If the body is a player, triggers a notification
        /// indicating player interaction.
        /// </summary>
        /// <remarks>This method is intended to be used in collision detection scenarios to identify when
        /// a player enters a designated damage area. It can be extended to implement additional logic for handling
        /// player interactions.</remarks>
        /// <param name="body">The body that has entered the damage area. Typically expected to be a Node2D instance representing a game
        /// entity, such as a player.</param>
        public void OnDamageAreaBodyEntered(Node2D body)
        {
            if (body is Player player)
                GD.Print("Player hit Goblin_1");
        }

        /// <summary>
        /// Updates the horizontal flip state of the animated sprite based on the current velocity.
        /// </summary>
        /// <remarks>This method checks the X component of the velocity to determine if the sprite should
        /// be flipped horizontally. If the velocity is negative, the sprite is flipped; if positive, it is not. If the
        /// velocity is zero, the current flip state remains unchanged.</remarks>
        private void UpdateSpriteDirection()
        {
            if (_animatedSprite == null)
                return;

            _animatedSprite.FlipH = Velocity.X switch
            {
                < 0f => true,
                > 0f => false,
                _ => _animatedSprite.FlipH
            };
        }

        /// <summary>
        /// Handles the event when a body enters the detection area and responds to player entities by updating vision
        /// state and subscribing to player actions.
        /// </summary>
        /// <remarks>This method sets the IsOnVisionRange property to <see langword="true"/> and
        /// subscribes to the player's jumping event. Ensure that the body parameter is not null and is of the correct
        /// type to avoid runtime errors.</remarks>
        /// <param name="body">The node that has entered the detection area. Must be of type Player to trigger vision state changes and
        /// event subscriptions.</param>
        public void OnAreaDetectionBodyEntered(Node2D body)
        {
            if (body is Player player)
            {
                GD.Print("Player detected by Goblin_1");
                IsOnVisionRange = true;
                _player = player;
                _player.InJumping += OnPlayerJumped;
            }
        }

        /// <summary>
        /// Handles the event when the player jumps, updating the player's vertical velocity to reflect the jump action
        /// if the player is within the vision range.
        /// </summary>
        /// <remarks>This method sets the player's vertical velocity to the predefined jump velocity only
        /// when the player is detected within the vision range. Use this method to synchronize enemy behavior with
        /// player jump actions in scenarios where vision-based interaction is required.</remarks>
        public void OnPlayerJumped()
        {
            if (IsOnVisionRange)
                Velocity = Velocity with { Y = JumpVelocity };
        }

        /// <summary>
        /// Handles the event when a body exits the detection area, updating the skeleton's state if the exiting body is
        /// a player.
        /// </summary>
        /// <remarks>When a player leaves the detection area, the skeleton resets its vision range state,
        /// plays the idle animation, and stops horizontal movement. This method is typically used in area detection
        /// event handling to manage enemy behavior in response to player movement.</remarks>
        /// <param name="body">The node representing the body that exited the detection area. If the body is a player, the skeleton's
        /// vision state is updated.</param>
        public void OnAreaDetectionBodyExited(Node2D body)
        {
            if (body is Player player)
            {
                IsOnVisionRange = false;
                GD.Print("Player left Skeleton_1 detection area");
                _animatedSprite.Play("Idle");
                Velocity = Velocity with { X = 0f };
            }
        }

        /// <summary>
        /// Handles the event when an entity enters the attack area and updates the attack range status if the entity is
        /// a player.
        /// </summary>
        /// <remarks>Sets the IsOnAttackRange property to <see langword="true"/> when a Player enters the
        /// attack area. Ensure that the body parameter is of the correct type to avoid unintended behavior.</remarks>
        /// <param name="body">The entity that has entered the attack area. Must be a Player instance to trigger the attack range update.</param>
        public void OnAttackAreaEntered(Node2D body)
        {
            if (body is Player player)
                IsOnAttackRange = true;
        }

        /// <summary>
        /// Handles the event when an entity exits the attack area, updating the attack range status if the entity is a
        /// player.
        /// </summary>
        /// <param name="body">The Node2D object representing the entity that exited the attack area. Must be a Player instance to affect
        /// the attack range status.</param>
        public void OnAttackAreaExited(Node2D body)
        {
            if (body is Player player)
                IsOnAttackRange = false;
        }

        /// <summary>
        /// Handles the completion of an animation by transitioning the animated sprite to the idle state if the current
        /// animation is "Attack".
        /// </summary>
        /// <remarks>This method should be called when an animation finishes playing to ensure that the
        /// sprite returns to a neutral state after performing an action. It is typically used in animation event
        /// callbacks to maintain consistent character behavior.</remarks>
        public void OnAnimationFinished()
        {
            if (_animatedSprite.Animation == "Attack")
                _animatedSprite.Play("Idle");
        }
    }
}