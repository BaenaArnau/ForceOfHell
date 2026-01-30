using Godot;
using System;

namespace ForceOfHell.Scripts.Weapons
{
    public partial class Weapons : AnimatedSprite2D
    {
        public Weapons() { }
        public int Id { get; set;  }
        public string Name { get; set; } = string.Empty;
        public float Damage { get; set; }
        public int Cost { get; set; }
        public float FireRate { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsMeelee { get; set; }
        public string? Bullet { get; set; }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            this.SetWeapon(0);
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }

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

            Play(this.Name);
        }
    }
}
