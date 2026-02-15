using ForceOfHell.Scripts.MainCharacter;
using Godot;
using System.Threading.Tasks;

namespace ForceOfHell.Scripts.Enemies
{
    /// <summary>
    /// Represents the Balrog enemy character, which interacts with the player through chase, attack, and damage
    /// mechanics within the game scene.
    /// </summary>
    /// <remarks>The Balrog is activated when a player enters its chase area and can deal damage when the
    /// player is within its weapon collision area. It manages its health and transitions between idle, walking, and
    /// attacking states based on player interactions. The class provides methods for handling collision events,
    /// processing damage, and updating animation and movement in response to game events.</remarks>
    public partial class Balrog : CharacterBody2D
    {
        private AnimatedSprite2D _animatedSprite;
        private Area2D _weaponCollision;
        private Area2D _chaseArea;
        [Export] private Player _player;
        private Area2D _damageArea;
        private Vector2 _initialPosition;
        public bool BalrogActivate = false;
        private bool _attackedThisFrame = false;
        public bool TakeDamage = false;
        private const int Health = 50;
        public float CurrentHealth { get; set; } = Health;

        /// <summary>
        /// Initializes the node and retrieves references to child nodes required for animation and collision handling.
        /// </summary>
        /// <remarks>This method is called when the node enters the scene tree. It sets up references to
        /// the animated sprite and collision areas, which are essential for the enemy's visual representation and
        /// interaction with other objects. Ensure that the child nodes 'AnimatedSprite2D', 'WeaponCollision', and
        /// 'DamageArea' exist in the scene to avoid null references.</remarks>
        public override void _Ready()
        {
            _animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
            _weaponCollision = GetNodeOrNull<Area2D>("WeaponCollision");
            _damageArea = GetNodeOrNull<Area2D>("DamageArea");
            _initialPosition = GlobalPosition;
        }
        /// <summary>
        /// Updates the physics state of the Balrog character based on the elapsed time since the last frame. Handles
        /// movement, gravity, animation transitions, and player interactions during attack sequences.
        /// </summary>
        /// <remarks>This method manages the Balrog's behavior, including switching between idle, walking,
        /// and attacking animations. When the Balrog is activated and in an attack state, it interacts with the player
        /// by triggering a hit. The method also ensures the character moves toward its initial position when not
        /// activated.</remarks>
        /// <param name="delta">The time elapsed, in seconds, since the previous frame. Used to calculate movement and apply gravity.</param>
        public override void _PhysicsProcess(double delta)
        {
            Velocity += GetGravity() * (float)delta;

            if (BalrogActivate)
            {
                if (_animatedSprite.Animation == "Attack" && _animatedSprite.Frame == 5)
                {
                    if (!_attackedThisFrame)
                    {
                        _attackedThisFrame = true;
                        Godot.Collections.Array<Node2D> bodies = _weaponCollision.GetOverlappingBodies();
                        foreach (Node2D body in bodies)
                        {
                            if (body is Player player)
                                _ = player.HitAsync(50);
                        }
                        _animatedSprite.Play("Idle");
                    }
                }
                else
                    _attackedThisFrame = false;

                if (_animatedSprite.Animation != "Attack")
                {
                    _animatedSprite.Play("Walk");
                    Velocity = Velocity with { X = (_player.GlobalPosition.X < GlobalPosition.X ? -1 : 1) * 100f };
                }
            }
            else
            {

                if ((_initialPosition - GlobalPosition).Length() > 5f)
                {
                    Velocity = Velocity with { X = (_initialPosition.X < GlobalPosition.X ? -1 : 1) * 100f };
                    _animatedSprite.Play("Walk");
                }
                else
                {
                    Velocity = Velocity with { X = 0f };
                    _animatedSprite.Play("Idle");
                }
            }
            UpdateSpriteDirection();
            MoveAndSlide();
        }

        /// <summary>
        /// Processes the specified amount of damage to the entity, updating its health and handling death conditions as
        /// necessary.
        /// </summary>
        /// <remarks>If the damage reduces the entity's health to zero or below, the scene is changed to
        /// the main menu and the entity is removed from the scene. The method temporarily sets the TakeDamage flag to
        /// <see langword="true"/> while processing the damage.</remarks>
        /// <param name="damage">The amount of damage to apply to the entity's current health. Must be a positive value.</param>
        /// <returns>A task that represents the asynchronous operation of processing damage.</returns>
        internal async Task DamageDealt(float damage)
        {
            TakeDamage = true;
            if ((CurrentHealth - damage) <= 0)
            {
                CurrentHealth = 0;
                GetTree().ChangeSceneToFile("res://Scenes/MainMenu/main_menu.tscn");
                QueueFree();
            }
            else
                CurrentHealth -= damage;

            await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
            TakeDamage = false;
        }

        /// <summary>
        /// Handles the event when a body enters the damage area and applies damage if the body is a player being
        /// monitored.
        /// </summary>
        /// <remarks>This method checks whether the entered body is a Player and whether the player's area
        /// is actively monitored before applying damage. Ensure that the player's weapon is equipped for damage to be
        /// dealt.</remarks>
        /// <param name="body">The Node2D instance representing the body that has entered the damage area. Must be a Player with a
        /// monitored area to receive damage.</param>
        public void OnDamageAreaBodyEntered(Node2D body)
        {
            if (body is Player p)
            {
                if (p.area2D.Monitoring)
                    _ = DamageDealt(p.equip_weapon.Damage);
            }
        }

        /// <summary>
        /// Updates the horizontal orientation of the animated sprite based on the current velocity.
        /// </summary>
        /// <remarks>This method sets the sprite's horizontal flip state according to the X component of
        /// the velocity. If the animated sprite is not assigned, no changes are made.</remarks>
        private void UpdateSpriteDirection()
        {
            if (_animatedSprite == null)
                return;

            _animatedSprite.FlipH = Velocity.X switch
            {
                < 0f => false,
                > 0f => true,
                _ => _animatedSprite.FlipH
            };
        }

        /// <summary>
        /// Handles the event when a body enters the chase area. Activates the Balrog if the body is a player.
        /// </summary>
        /// <remarks>This method sets the BalrogActivate flag to <see langword="true"/> and stores a
        /// reference to the player when a player enters the chase area. Other node types are ignored.</remarks>
        /// <param name="body">The node that has entered the chase area. If the node is a player, the Balrog will be activated.</param>
        public void OnChaseAreaBodyEntered(Node2D body)
        {
            if (body is Player player)
            {
                BalrogActivate = true;
                _player = player;
            }
        }

        /// <summary>
        /// Handles the event when a body exits the chase area. Deactivates the Balrog if the exiting body is a player.
        /// </summary>
        /// <param name="body">The node representing the body that has exited the chase area. If this node is of type Player, the Balrog
        /// will be deactivated.</param>
        public void OnChaseAreaBodyExited(Node2D body)
        {
            if (body is Player)
            {
                BalrogActivate = false;
            }
        }

        /// <summary>
        /// Handles the event when a body enters the weapon's collision area, triggering an attack animation if the body
        /// is a player.
        /// </summary>
        /// <remarks>This method is typically used to detect player collisions with enemy weapons and
        /// initiate attack logic. The weapon's velocity is reset when a player is detected.</remarks>
        /// <param name="body">The body that entered the collision area. Must be a Node2D instance; if the body is a Player, the attack
        /// animation is initiated.</param>
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