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
        public const float Speed = 300.0f;
        public const float JumpVelocity = -500.0f;
        public const float CoyoteTimeMax = 0.15f;

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
        public bool IsDying { get; private set; }

        public override void _Ready()
        {
            _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        }

        public override void _PhysicsProcess(double delta) 
        {
            UpdateSpriteDirection();
            UpdateCoyoteTime((float)delta);
        }

        /// <summary>Reproduce una animación. Ignora si el jugador está muriendo.</summary>
        public void SetAnimation(string animationName)
        {
            if (!IsDying && _animatedSprite != null)
                _animatedSprite.Play(animationName);
        }

        /// <summary>Maneja el daño recibido por el jugador.</summary>
        public async Task HitAsync()
        {
            try
            {
                IsDying = true;
                if (_animatedSprite != null)
                {
                    _animatedSprite.Play("hit");
                    await ToSignal(_animatedSprite, AnimatedSprite2D.SignalName.AnimationFinished);
                }
            }
            catch (Exception)
            {
                // Asegurar recarga incluso si hay error
            }
            finally
            {
                GetTree().CallDeferred(SceneTree.MethodName.ReloadCurrentScene);
            }
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

        private void UpdateCoyoteTime(float delta)
        {
            _coyoteTimeCounter = IsOnFloor() 
                ? CoyoteTimeMax 
                : Math.Max(0f, _coyoteTimeCounter - delta);
        }
    }
}
