using ForceOfHell.Scripts.MainCharacter;
using Godot;
using System;
using System.Threading.Tasks;

namespace ForceOfHell.Scripts.FirstLevel
{
	public partial class first_level : Node
	{
		[Export] public PackedScene _weapon;
		[Export] public PackedScene _manaPotion;
		[Export] public PackedScene _healPotion;
		[Export] private Player _player;
		[Export] private Balrog _balrog;

		/// <summary>
		/// Generates and adds a new item to the scene at the specified position based on the provided item name.
		/// </summary>
		/// <remarks>If an unsupported item name is provided, an error is logged and no item is generated.</remarks>
		/// <param name="position">The position, in 2D coordinates, where the item will be placed in the scene.</param>
		/// <param name="name">The name of the item to generate. Supported values are "weapon", "manaPotion", and "healPotion". The comparison is
		/// case-sensitive.</param>
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
		
		public void OnBossArenaBodyEntered(Node2D body)
		{
			if (body is Player player)
			{
				GD.Print("Player entered the boss arena");
				_balrog.BalrogActivate = true;
				// Aquí puedes agregar la lógica para iniciar el combate con el jefe
			}
		}

		public void OnBossArenaBodyExited(Node2D body)
		{
			if (body is Player player)
			{
				GD.Print("Player exited the boss arena");
				_balrog.BalrogActivate = false;
				// Aquí puedes agregar la lógica para finalizar el combate con el jefe
			}
		}
	}
}
