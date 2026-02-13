using Godot;
using System;

public partial class MainMenu : Node
{
		public void _on_play_pressed()
		{
			GD.Print("Start Button Pressed");
			GetTree().ChangeSceneToFile("res://Scenes/FirstLevel/first_level.tscn");
		}

		public void _on_exit_pressed()
		{
			GD.Print("Quit Button Pressed");
			GetTree().Quit();
		}
}

