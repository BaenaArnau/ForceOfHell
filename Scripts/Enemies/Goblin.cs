using ForceOfHell.Scripts.MainCharacter;
using Godot;
using System;
using System.Threading.Tasks;

namespace ForceOfHell.Scripts.Enemies
{
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
        public bool TakeDamage = false;
        public override void _Ready()
        {
            _animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
            _weaponCollision = GetNodeOrNull<Area2D>("WeaponCollision");
            _detectionArea = GetNodeOrNull<Area2D>("DetectionArea");
            _damageArea = GetNodeOrNull<Area2D>("DamageArea");
        }

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
                        {
                            _ = player.HitAsync(15);
                        }
                    }
                    _animatedSprite.Play("Idle");
                }
            }
            else
            {
                _attackedThisFrame = false;
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
                        {
                            Velocity = Velocity with { Y = JumpVelocity };
                        }
                    }
                }
            }

            MoveAndSlide();
        }

        private async Task DamageDealt()
        {
            TakeDamage = true;
            await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
            TakeDamage = false;
        }

        private void ReceiveDamage()
        {
            _ = DamageDealt();
        }


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

        public void OnDamageAreaBodyEntered(Node2D body)
        {
            if (body is Player player)
            {
                GD.Print("Player hit Goblin_1");
                ReceiveDamage();
            }
        }

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

        public void OnPlayerJumped()
        {
            if (IsOnVisionRange)
            {
                Velocity = Velocity with { Y = JumpVelocity };
            }
        }

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