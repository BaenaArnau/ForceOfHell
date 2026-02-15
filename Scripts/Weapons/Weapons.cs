using Godot;
using System;

namespace ForceOfHell.Scripts.Weapons
{
    /// <summary>
    /// Controla la lógica del arma: datos, cooldown y dirección de disparo.
    /// </summary>
    public partial class Weapons : AnimatedSprite2D
    {
        /// <summary>Constructor por defecto.</summary>
        public Weapons() { }

        /// <summary>Identificador único del arma.</summary>
        public int Id { get; set; }

        /// <summary>Nombre interno del arma y de su animación.</summary>
        public new string Name { get; set; } = string.Empty;

        /// <summary>Daño base del arma.</summary>
        public float Damage { get; set; }

        /// <summary>Coste de mana/energía al usarla.</summary>
        public int Cost { get; set; }

        /// <summary>Cadencia de disparo (tiempo mínimo entre disparos).</summary>
        public float FireRate { get; set; }

        /// <summary>Descripción del arma.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Indica si el arma es cuerpo a cuerpo.</summary>
        public bool IsMeelee { get; set; }

        /// <summary>Nombre del recurso de bala asociado.</summary>
#nullable enable
        public string? Bullet { get; set; }

        /// <summary>Dirección del disparo calculada desde la entrada.</summary>
        public Vector2 direction;

        /// <summary>Tiempo restante hasta permitir otro disparo.</summary>
        public float _fireCooldown;

        /// <summary>Ángulo de retroceso en grados al disparar.</summary>
        private const float RecoilAngleDeg = 15f;

        /// <summary>Duración del movimiento de retroceso (ida).</summary>
        private const float RecoilKickDuration = 0.05f;

        /// <summary>Duración del retorno a la posición original.</summary>
        private const float RecoilReturnDuration = 0.1f;

        /// <summary>Tween activo del retroceso, para cancelarlo si se lanza otro.</summary>
#nullable enable
        private Tween? _recoilTween;

        /// <summary>Inicializa el arma por defecto.</summary>
        public override void _Ready()
        {
            this.SetWeapon(0);
        }

        /// <summary>Configura las propiedades del arma desde los datos.</summary>
        public void SetWeapon(int referencia)
        {
            var data = ReadWeapons.Weapons[referencia];

            this.Id = data.Id;
            this.Name = data.Name;
            this.Damage = data.Damage;
            this.Cost = data.Cost;
            this.FireRate = data.FireRate;
            this.Description = data.Description;
            this.IsMeelee = data.IsMeelee;
            this.Bullet = data.Bullet;

            _fireCooldown = 0f;
            Play(this.Name);
        }

        /// <summary>Reduce el cooldown de disparo.</summary>
        public void UpdateCooldown(float delta)
        {
            _fireCooldown = Math.Max(0f, _fireCooldown - delta);
        }

        /// <summary>
        /// Intenta atacar. Devuelve true si el ataque es válido
        /// (tanto para melee como para armas a distancia).
        /// </summary>
        public bool attack()
        {
            if (_fireCooldown > 0f)
            {
                direction = Vector2.Zero;
                return false;
            }

            if (IsMeelee)
            {
                // Melee no necesita dirección de disparo, pero sí input.
                direction = Vector2.Zero;
                bool hasInput = Input.IsActionPressed("shot_right")
                    || Input.IsActionPressed("shot_left")
                    || Input.IsActionPressed("shot_up")
                    || Input.IsActionPressed("shot_down");

                if (!hasInput)
                    return false;

                _fireCooldown = Math.Max(0.01f, FireRate);
                return true;
            }

            // Arma a distancia: requiere dirección válida.
            direction = GetAimDirection();
            if (direction == Vector2.Zero)
                return false;

            _fireCooldown = Math.Max(0.01f, FireRate);
            return true;
        }

        /// <summary>
        /// Aplica el efecto de retroceso (recoil) estilo Nuclear Throne
        /// sobre el nodo visual del arma a distancia.
        /// </summary>
        /// <param name="weaponNode">Nodo del arma en la escena al que aplicar la rotación.</param>
        /// <param name="isFlipped">Si el sprite está volteado (mirando a la izquierda).</param>
        public void ApplyRecoil(Node weaponNode, bool isFlipped)
        {
            // Cancela el tween anterior si aún está activo.
            if (_recoilTween != null && _recoilTween.IsValid())
                _recoilTween.Kill();

            float recoilRad = Mathf.DegToRad(isFlipped ? RecoilAngleDeg : -RecoilAngleDeg);

            _recoilTween = weaponNode.CreateTween();
            _recoilTween
                .TweenProperty(weaponNode, "rotation", recoilRad, RecoilKickDuration)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Quad);
            _recoilTween
                .TweenProperty(weaponNode, "rotation", 0f, RecoilReturnDuration)
                .SetEase(Tween.EaseType.In)
                .SetTrans(Tween.TransitionType.Quad);
        }

        /// <summary>Obtiene la dirección de disparo según las teclas de flecha.</summary>
        private static Vector2 GetAimDirection()
        {
            var dir = Vector2.Zero;

            if (Input.IsActionPressed("shot_right")) dir.X += 1;
            if (Input.IsActionPressed("shot_left")) dir.X -= 1;
            if (Input.IsActionPressed("shot_down")) dir.Y += 1;
            if (Input.IsActionPressed("shot_up")) dir.Y -= 1;

            return dir == Vector2.Zero ? Vector2.Zero : dir.Normalized();
        }
    }
}
