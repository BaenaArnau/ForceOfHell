using ForceOfHell.Scripts.MainCharacter;
using Godot;
using System;
using System.Threading.Tasks;

namespace ForceOfHell.Scripts.Enemies
{
    /// <summary>
    /// Represents a Goblin enemy character that can detect, chase, and attack players within its vision range. Provides
    /// health management and interaction logic for combat scenarios.
    /// </summary>
    /// <remarks>The Goblin responds to player presence by moving toward and attacking the player when
    /// detected. It manages its own health and is removed from the game when health reaches zero. The Goblin's behavior
    /// includes jumping, sprite animation updates, and event-driven responses to player actions. This class is intended
    /// for use in enemy AI and player interaction systems within a 2D game environment.</remarks>
    public partial class Goblin : CharacterBody2D
    {
        private AnimatedSprite2D _animatedSprite;
        private Area2D _weaponCollision;
        private Area2D _detectionArea;
        private Area2D _chaseArea;
        private Player _player;
        private Area2D _damageArea;
        public bool IsOnVisionRange { get; private set; }
        private bool _attackedThisFrame = false;
        public const float JumpVelocity = -400f;
        private const int Health = 10;
        public float CurrentHealth { get; set; } = Health;

        /// <summary>
        /// Initializes the node and retrieves references to child nodes required for animation and collision handling.
        /// </summary>
        /// <remarks>This method is called when the node enters the scene tree. It ensures that references
        /// to the animated sprite and collision areas are available for subsequent interactions and logic.</remarks>
        public override void _Ready()
        {
            _animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
            _weaponCollision = GetNodeOrNull<Area2D>("WeaponCollision");
            _detectionArea = GetNodeOrNull<Area2D>("DetectionArea");
            _damageArea = GetNodeOrNull<Area2D>("DamageArea");
        }

        /// <summary>
        /// Updates the physics state of the goblin character based on the elapsed time since the last frame. Handles
        /// movement, gravity, attack logic, and animation transitions.
        /// </summary>
        /// <remarks>This method manages the goblin's velocity, attack interactions with the player, and
        /// animation changes according to its current state. It also checks for collisions with tile maps to trigger
        /// jumping and adjusts movement direction based on the player's position. Callers should ensure that this
        /// method is invoked regularly within the physics processing loop to maintain consistent character
        /// behavior.</remarks>
        /// <param name="delta">The time, in seconds, that has elapsed since the previous frame. Used to calculate movement and apply
        /// gravity.</param>
        public override void _PhysicsProcess(double delta)
        {
            Velocity += GetGravity() * (float)delta;

            UpdateSpriteDirection();
            if (_animatedSprite.Animation == "Attack" && _animatedSprite.Frame == 4)
            {
                if (!_attackedThisFrame)
                {
                    _attackedThisFrame = true;
                    Godot.Collections.Array<Node2D> bodies = _weaponCollision.GetOverlappingBodies();
                    foreach (Node2D body in bodies)
                    {
                        if (body is Player player)
                            _ = player.HitAsync(15);
                    }
                    _animatedSprite.Play("Idle");
                }
            }
            else
                _attackedThisFrame = false;

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
        /// Processes the specified amount of damage and updates the current health accordingly. If the damage reduces
        /// health to zero or below, the object is scheduled for removal.
        /// </summary>
        /// <remarks>If the damage equals or exceeds the current health, the health is set to zero and the
        /// object is marked for freeing. Otherwise, the current health is reduced by the specified amount.</remarks>
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
        /// Updates the horizontal orientation of the animated sprite based on the current velocity.
        /// </summary>
        /// <remarks>This method sets the sprite's horizontal flip state according to the X component of
        /// the velocity. If the velocity is negative, the sprite is flipped horizontally; if positive, it is not. When
        /// the velocity is zero, the existing flip state is preserved. This ensures the sprite visually faces the
        /// direction of movement.</remarks>
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
        /// Handles the event when a body enters the damage area. Processes interactions with player entities that enter
        /// the area.
        /// </summary>
        /// <remarks>This method is typically used to detect and respond to player collisions with the
        /// enemy's damage area. Only Player instances are processed; other node types are ignored.</remarks>
        /// <param name="body">The node that has entered the damage area. Must be of type Node2D. If the node is a Player, the method will
        /// process the interaction.</param>
        public void OnDamageAreaBodyEntered(Node2D body)
        {
            if (body is Player player)
            {
                GD.Print("Player hit Goblin_1");
            }
        }

        /// <summary>
        /// Handles the event when a body enters the detection area and responds to player entities by updating vision
        /// state and subscribing to player actions.
        /// </summary>
        /// <remarks>When a Player enters the detection area, this method sets the goblin's vision range
        /// state to active and subscribes to the player's jumping event. Ensure that the provided body is not null and
        /// is of the correct type to avoid runtime errors.</remarks>
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
        /// Updates the player's vertical velocity to initiate a jump when the player is within the vision range.
        /// </summary>
        /// <remarks>This method should be called when the player performs a jump action. The jump effect
        /// is only applied if the player is currently within the vision range, ensuring that the vertical velocity is
        /// set to the defined jump velocity. Use this method to synchronize enemy behavior with player actions in
        /// vision-dependent scenarios.</remarks>
        public void OnPlayerJumped()
        {
            if (IsOnVisionRange)
                Velocity = Velocity with { Y = JumpVelocity };
        }

        /// <summary>
        /// Handles the event when a body exits the detection area. If the exiting body is a player, updates the
        /// entity's state to reflect that the player is no longer within vision range.
        /// </summary>
        /// <remarks>When a player leaves the detection area, the entity resets its vision range flag,
        /// plays the idle animation, and stops horizontal movement. This method is typically used to manage enemy
        /// behavior in response to player proximity.</remarks>
        /// <param name="body">The node representing the body that has exited the detection area. If the body is a player, the method will
        /// update the entity's vision state and animation.</param>
        public void OnAreaDetectionBodyExited(Node2D body)
        {
            if (body is Player player)
            {
                IsOnVisionRange = false;
                GD.Print("Player left Goblin_1 detection area");
                _animatedSprite.Play("Idle");
                Velocity = Velocity with { X = 0f };
            }
        }

        /// <summary>
        /// Handles the event when a body enters the weapon's collision area, triggering an attack animation if the body
        /// is a player.
        /// </summary>
        /// <remarks>When a player enters the collision area, the method initiates the attack animation
        /// and resets the weapon's horizontal velocity. This is typically used to respond to player collisions in enemy
        /// attack logic.</remarks>
        /// <param name="body">The node representing the body that entered the collision area. Must be of type Player to trigger the attack
        /// animation.</param>
        public void OnWeaponCollisionAreaBodyEntered(Node2D body)
        {
            if (body is Player player)
            {
                GD.Print("Player hit by Goblin_1 weapon");
                _animatedSprite.Play("Attack");
                Velocity = Velocity with { X = 0f };

            }
        }
    }
}