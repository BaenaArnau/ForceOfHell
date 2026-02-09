using Godot;
using System;

namespace ForceOfHell.Scripts.FirstLevel
{
	public partial class first_level : Node
	{
		[Export] public PackedScene _weapon;
		[Export] public PackedScene _manaPotion;
		[Export] public PackedScene _healPotion;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}

		public void GenerateItem(Vector2 position, string name)
		{
			if (name == "weapon")
			{
				GD.Print("Drop: Arma");
				var weaponInstance = _weapon.Instantiate() as Node2D;
				weaponInstance.Position = position;
				AddChild(weaponInstance);
			}
			else if (name == "manaPotion")
			{
				GD.Print("Drop: Poción de mana");
				var manaPotionInstance = _manaPotion.Instantiate() as Node2D;
				manaPotionInstance.Position = position;
				AddChild(manaPotionInstance);
			}
			else if (name == "healPotion")
			{
				GD.Print("Drop: Poción de vida");
				var healPotionInstance = _healPotion.Instantiate() as Node2D;
				healPotionInstance.Position = position;
				AddChild(healPotionInstance);
			}
			else
				GD.PushError("El Objeto no existe");
		}
	}
}
