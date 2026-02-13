using ForceOfHell.Scripts.MainCharacter;
using Godot;
using System;
using System.Threading.Tasks;

public partial class first_level : Node
{

	[Export] private Player _player;
	[Export] private Balrog _balrog;
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
