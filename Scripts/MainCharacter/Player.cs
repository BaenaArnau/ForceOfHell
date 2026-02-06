using ForceOfHell.Scripts.Weapons;
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
		public const float Speed = 300.0f;
		public const float JumpVelocity = -500.0f;
		public const float CoyoteTimeMax = 0.15f;

		// Ajustes de posicionamiento del arma según la dirección.
		private const float WeaponOffsetLeft = 0f;
		private const float WeaponOffsetRight = 13f;

		private Weapons.Weapons equip_weapon;
		private int mana = 100;
		private int health = 100;

		[Export] private AnimatedSprite2D animatedWeapon;
		[Export] public PackedScene CargarBullet { get; set; }

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
			// Inicializa arma y sprite principal.
			equip_weapon = new Weapons.Weapons();
			_animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
			equip_weapon.SetWeapon(0);
		}

		public override void _PhysicsProcess(double delta)
		{
			UpdateSpriteDirection();
			UpdateCoyoteTime((float)delta);
			equip_weapon?.UpdateCooldown((float)delta);
			attack();
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

				// Reproduce la animación de daño y espera a que termine.
				if (_animatedSprite != null)
				{
					_animatedSprite.Play("hit");
					await ToSignal(_animatedSprite, AnimatedSprite2D.SignalName.AnimationFinished);
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
				animatedWeapon.Position = new Vector2(xOffset, animatedWeapon.Position.Y);
			}
		}

		private void UpdateCoyoteTime(float delta)
		{
			// Rellena el coyote time cuando está en el suelo, o lo reduce en el aire.
			_coyoteTimeCounter = IsOnFloor()
				? CoyoteTimeMax
				: Math.Max(0f, _coyoteTimeCounter - delta);
		}

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

		public void attack()
		{
			// Salida temprana para minimizar trabajo si no se puede disparar.
			if (equip_weapon == null || !equip_weapon.attack() || CargarBullet == null || _animatedSprite == null)
				return;

			var bulletInstance = (Bullet)CargarBullet.Instantiate();
			bulletInstance.GlobalPosition = _animatedSprite.GlobalPosition;
			bulletInstance.Configure(equip_weapon.direction, equip_weapon.Bullet);
			GetTree().CurrentScene?.AddChild(bulletInstance);
		}
	}
}
