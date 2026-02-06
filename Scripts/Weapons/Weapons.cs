using ForceOfHell.Scripts.MainCharacter;
using Godot;
using System;
using System.Diagnostics;

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
		public string Name { get; set; } = string.Empty;

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
		public string? Bullet { get; set; }

		/// <summary>Dirección del disparo calculada desde la entrada.</summary>
		public Vector2 direction;

		/// <summary>Tiempo restante hasta permitir otro disparo.</summary>
		private float _fireCooldown;

		/// <summary>Inicializa el arma por defecto.</summary>
		public override void _Ready()
		{
			this.SetWeapon(0);
		}

		/// <summary>Actualización por frame (no usada actualmente).</summary>
		public override void _Process(double delta)
		{

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
		/// Intenta disparar y devuelve si el disparo es válido.
		/// </summary>
		public bool attack()
		{
			if (IsMeelee)
			{
				direction = Vector2.Zero;
				return false;
			}

			if (_fireCooldown > 0f)
			{
				direction = Vector2.Zero;
				return false;
			}

			direction = GetAimDirection();
			if (direction == Vector2.Zero)
				return false;

			_fireCooldown = Math.Max(0.01f, FireRate);
			return true;
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
