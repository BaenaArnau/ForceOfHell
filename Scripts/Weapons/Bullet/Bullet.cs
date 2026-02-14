using ForceOfHell.Scripts.Enemies;
using ForceOfHell.Scripts.Objects;
using Godot;

namespace ForceOfHell.Scripts.Weapons.Bullet
{
    /// <summary>
    /// Bala que se desplaza en una dirección fija y se destruye al chocar con el terreno.
    /// </summary>
    public partial class Bullet : Area2D
    {
        /// <summary>Velocidad lineal de la bala.</summary>
        private const float Speed = 450f;

        /// <summary>Dirección normalizada del movimiento.</summary>
        private Vector2 _direction = Vector2.Zero;

        /// <summary>Nombre de la animación a reproducir.</summary>
        public string animation = string.Empty;

        /// <summary>Sprite animado de la bala.</summary>
        [Export] private AnimatedSprite2D _animatedSprite;

        /// <summary>Forma de colisión de la bala.</summary>
        [Export] private CollisionShape2D _collisionShape;

        public float damage;

        /// <summary>Inicializa la animación si existe.</summary>
        public override void _Ready()
        {
            if (!string.IsNullOrWhiteSpace(animation))
                _animatedSprite.Play(animation);
        }

        /// <summary>Mueve la bala cada frame.</summary>
        public override void _Process(double delta)
        {
            if (_direction == Vector2.Zero)
                return;

            Translate(_direction * Speed * (float)delta);
        }

        /// <summary>
        /// Configura la dirección y la animación, y rota la bala según el movimiento.
        /// </summary>
        public void Configure(Vector2 direction, string animation, float damege)
        {
            this.damage = damege;

            this.animation = animation;
            _direction = direction == Vector2.Zero ? Vector2.Zero : direction.Normalized();

            if (_direction != Vector2.Zero)
                Rotation = _direction.Angle();
        }

        /// <summary>Elimina la bala al chocar con el terreno.</summary>
        private void OnBodyEntered(Node body)
        {
            HandleCollision(body);
        }

        /// <summary>
        /// Handles logic when another Area2D enters the monitored area.
        /// </summary>
        /// <param name="area">The Area2D instance that has entered the area. Cannot be null.</param>
        private void OnAreaEntered(Area2D area)
        {
            HandleCollision(area);
        }

        /// <summary>
        /// Handles a collision with the specified node, applying damage if applicable and removing the node from the
        /// scene.
        /// </summary>
        /// <remarks>If the node belongs to the 'Terreno' group, it is immediately removed from the scene.
        /// For other node types, damage is applied before removal. The method ensures that the collision effects are
        /// handled according to the type of node encountered.</remarks>
        /// <param name="node">The node involved in the collision. This may represent terrain, boxes, Balrogs, Skeletons, or Goblins.</param>
        private void HandleCollision(Node node)
        {
            if (node.IsInGroup("Terreno"))
            {
                QueueFree();
                return;
            }

            if (node is Box b)
            {
                b.TakeDamage(false);
                QueueFree();
            }

            if (node is Balrog balrog)
            {
                _ = balrog.DamageDealt(damage);
                QueueFree();
            }

            if (node is Skeleton s)
            {
                _ = s.DamageDealt(damage);
                QueueFree();
            }

            if (node is Goblin g)
            {
                _ = g.DamageDealt(damage);
                QueueFree();
            }
        }
    }
}
