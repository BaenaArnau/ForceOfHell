using ForceOfHell.Scripts.Objects;
using ForceOfHell.Scripts.Weapons.Bullet;
using Godot;
using System;
using System.Threading.Tasks;

namespace ForceOfHell.Scripts.MainCharacter
{
    /// <summary>
    /// Clase que representa al jugador.
    /// </summary>
    public partial class Player : CharacterBody2D
    {
        public const float Speed = 200.0f;
        public const float JumpVelocity = -350.0f;
        public const float CoyoteTimeMax = 0.15f;

        // Ajustes de posicionamiento del arma según la dirección.
        private const float WeaponOffsetLeft = 0f;
        private const float WeaponOffsetRight = 13f;

        public Weapons.Weapons equip_weapon;
        internal const int mana = 100;
        internal const int health = 100;

        public int manaActual = mana;
        public int healthActual = health;

        // Nodo de animación del arma, asignado desde el editor.
        [Export] private AnimatedSprite2D animatedWeapon;

        [Export] private Area2D area2D;

        // Escena de la bala, asignada desde el editor.
        [Export] public PackedScene CargarBullet { get; set; }

        // Eventos de señal para acciones del jugador.
        [Signal] public delegate void InJumpingEventHandler();

        private AnimatedSprite2D _animatedSprite;
        private float _coyoteTimeCounter;

        /// <summary>Tiempo restante para salto coyote (acceso público, modificación interna).</summary>
        public float CoyoteTimeCounter
        {
            get => _coyoteTimeCounter;
            internal set => _coyoteTimeCounter = value;
        }

        /// <summary>Indica si el jugador está en proceso de muerte.</summary>
        public bool IsDying { get; set; }
        public bool CanClimb { get; set; }

        /// <summary>Ancla horizontal de la escalera actual.</summary>
        public float ClimbAnchorX { get; set; }

        /// <summary>
        /// Initializes the node when it enters the scene tree. Sets up the main weapon and sprite components, and configures
        /// the monitoring state of the area.
        /// </summary>
        public override void _Ready()
        {
            // Inicializa arma y sprite principal.
            equip_weapon = new Weapons.Weapons();
            _animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
            equip_weapon.SetWeapon(0);
            area2D.Monitoring = false;

        }

        /// <summary>
        /// Handles per-frame physics processing, including updating sprite direction, managing coyote time, updating weapon
        /// cooldowns, and processing attacks based on available mana.
        /// </summary>
        /// <param name="delta">The elapsed time, in seconds, since the previous physics frame. Used to update time-dependent logic.</param>
        public override void _PhysicsProcess(double delta)
        {
            // Bloquea toda lógica si el jugador está muriendo.
            if (IsDying)
                return;

            UpdateSpriteDirection();
            UpdateCoyoteTime((float)delta);
            equip_weapon?.UpdateCooldown((float)delta);
            if ((equip_weapon._fireCooldown > 0f && equip_weapon._fireCooldown != equip_weapon.FireRate) && equip_weapon.IsMeelee)
                area2D.Monitoring = true;
            else
                area2D.Monitoring = false;

            if ((manaActual - equip_weapon.Cost) >= 0)
                attack();
            else
                GD.Print("No tienes suficiente mana para usar el arma.");
        }

        /// <summary>Reproduce una animación. Ignora si el jugador está muriendo.</summary>
        public void SetAnimation(string animationName)
        {
            if (!IsDying && _animatedSprite != null)
                _animatedSprite.Play(animationName);
        }

        /// <summary>Maneja el daño recibido por el jugador.</summary>
        public async Task HitAsync(int damange)
        {
            try
            {
                if ((health - damange) <= 0)
                {
                    healthActual = 0;
                    await Die();
                }
                else
                {
                    healthActual -= damange;
                    if (_animatedSprite != null)
                        _animatedSprite.Play("hit");
                    else
                        GD.PushError("[Player] No se ha asignado el nodo de animación.");
                }
            }
            catch (Exception)
            {
                // Asegurar recarga incluso si hay error.
            }
            finally
            {
                GetTree().CallDeferred(SceneTree.MethodName.ReloadCurrentScene);
            }
        }

        /// <summary>
        /// Inicia la secuencia de muerte: bloquea acciones, reproduce animación y recarga la escena.
        /// </summary>
        public async Task Die()
        {
            if (IsDying)
                return;

            IsDying = true;
            Velocity = Vector2.Zero;
            MoveAndSlide();

            try
            {
                if (_animatedSprite != null)
                {
                    _animatedSprite.Play("death");
                    await ToSignal(_animatedSprite, AnimatedSprite2D.SignalName.AnimationFinished);
                }
            }
            catch (Exception)
            {
                GetTree().CallDeferred(SceneTree.MethodName.ReloadCurrentScene);
            }
            finally
            {
                GetTree().CallDeferred(SceneTree.MethodName.ReloadCurrentScene);
            }
        }

        /// <summary>
        /// Updates the horizontal flip state of the sprite and its associated weapon based on the current horizontal
        /// velocity.
        /// </summary>
        /// <remarks>This method ensures that the sprite and weapon visually face the correct direction according to
        /// movement. If the direction has not changed since the last update, no changes are made. The weapon's position is
        /// also adjusted to match the new orientation.</remarks>
        private void UpdateSpriteDirection()
        {
            if (_animatedSprite == null)
                return;

            var currentFlip = _animatedSprite.FlipH;
            var nextFlip = Velocity.X switch
            {
                < 0f => true,
                > 0f => false,
                _ => currentFlip
            };

            // Evita asignaciones repetidas si no cambia la dirección.
            if (currentFlip == nextFlip)
                return;

            _animatedSprite.FlipH = nextFlip;

            // Sincroniza el arma con la dirección del sprite principal.
            if (animatedWeapon != null)
            {
                animatedWeapon.FlipH = nextFlip;
                var xOffset = nextFlip ? WeaponOffsetLeft : WeaponOffsetRight;
                animatedWeapon.Position = new Vector2((xOffset - 6), animatedWeapon.Position.Y);
                area2D.Position = new Vector2((nextFlip ? -14 : 14), area2D.Position.Y);
            }
        }

        /// <summary>
        /// Updates the coyote time counter based on whether the character is on the ground or in the air.
        /// </summary>
        /// <remarks>Coyote time allows the character to jump for a brief period after leaving the ground, improving
        /// responsiveness in platforming controls.</remarks>
        /// <param name="delta">The elapsed time, in seconds, since the last update. Must be non-negative.</param>
        private void UpdateCoyoteTime(float delta)
        {
            // Rellena el coyote time cuando está en el suelo, o lo reduce en el aire.
            _coyoteTimeCounter = IsOnFloor()
                ? CoyoteTimeMax
                : Math.Max(0f, _coyoteTimeCounter - delta);
        }

        /// <summary>
        /// Changes the currently equipped weapon to the weapon specified by the given identifier.
        /// </summary>
        /// <remarks>If no weapon is currently assigned or the animated weapon node is missing, an error is logged and
        /// the weapon is not changed.</remarks>
        /// <param name="id">The identifier of the weapon to equip. Must correspond to a valid weapon available to the player.</param>
        public void ChangeWeapon(int id)
        {
            if (equip_weapon == null)
            {
                GD.PushError("[Player] No se ha asignado el arma.");
                return;
            }

            equip_weapon.SetWeapon(id);

            // Actualiza la animación del arma si existe.
            if (animatedWeapon != null)
                animatedWeapon.SetAnimation(equip_weapon.Name);
            else
                GD.PushError("[Player] No se ha asignado el nodo de arma.");
        }

        /// <summary>
        /// Performs an attack using the currently equipped weapon. Melee weapons apply a visual swing,
        /// ranged weapons apply recoil and spawn a bullet. Flips sprite to face the attack direction.
        /// </summary>
        public void attack()
        {
            // Salida temprana para minimizar trabajo si no se puede atacar.
            if (equip_weapon == null || !equip_weapon.attack() || CargarBullet == null || _animatedSprite == null)
                return;

            animatedWeapon.Play(equip_weapon.Name);
            manaActual -= equip_weapon.Cost;

            // Aplica el retroceso visual del arma (estilo Nuclear Throne).
            if (animatedWeapon != null)
                equip_weapon.ApplyRecoil(animatedWeapon, _animatedSprite.FlipH);

            // Si el arma es cuerpo a cuerpo, no se instancia una bala.
            if (equip_weapon.IsMeelee)
            {
                return;
            }

            var bulletInstance = (Bullet)CargarBullet.Instantiate();
            bulletInstance.GlobalPosition = _animatedSprite.GlobalPosition;
            bulletInstance.Configure(equip_weapon.direction, equip_weapon.Bullet);
            GetTree().CurrentScene?.AddChild(bulletInstance);
        }

        /// <summary>
        /// Gets the maximum mana value for the current instance.
        /// </summary>
        /// <returns>The maximum amount of mana available. The value is always non-negative.</returns>
        public int GetManaMax()
        {
            return mana;
        }

        /// <summary>
        /// Gets the maximum health value for the current instance.
        /// </summary>
        /// <returns>The maximum health as an integer. The value represents the upper limit of health for this instance.</returns>
        public int GetHealthMax()
        {
            return health;
        }

        /// <summary>
        /// Handles logic when the specified area enters the current object.
        /// </summary>
        /// <param name="area">The area that has entered and triggered the event. Cannot be null.</param>
        private void OnAreaEntered(Area2D area)
        {
            HandleCollision(area);
        }

        /// <summary>
        /// Handles the event when a body enters the node's collision area.
        /// </summary>
        /// <param name="body">The node representing the body that has entered the collision area. Cannot be null.</param>
        private void OnBodyEntered(Node body)
        {
            HandleCollision(body);
        }

        /// <summary>
        /// Handles collision logic for the specified node, applying damage if the node is a box.
        /// </summary>
        /// <param name="node">The node involved in the collision. If the node is a box, damage will be applied.</param>
        private void HandleCollision(Node node)
        {
            if (node is Box b)
            {
                b.TakeDamage(true);
            }
        }
    }
}
