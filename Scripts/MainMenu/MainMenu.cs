using Godot;

namespace ForceOfHell.Scripts.MainMenu
{
    /// <summary>
    /// Represents the main menu of the application, providing options to start the game or exit the application.
    /// </summary>
    /// <remarks>This class handles user interactions with the main menu, including starting the game and
    /// quitting the application. Ensure that the scene files referenced in the methods are correctly set up to avoid
    /// runtime errors.</remarks>
    public partial class MainMenu : Node
    {
        /// <summary>
        /// Handles the event triggered when the play button is pressed, initiating the transition to the first level scene.
        /// </summary>
        /// <remarks>This method prints a message to the output console and changes the current scene to the specified
        /// first level scene file. Ensure that the scene file path is correct and that the scene is properly set up to avoid
        /// runtime errors.</remarks>
        public void _on_play_pressed()
        {
            GD.Print("Start Button Pressed");
            GetTree().ChangeSceneToFile("res://Scenes/FirstLevel/first_level.tscn");
        }

        /// <summary>
        /// Handles the exit button press event and terminates the application.
        /// </summary>
        /// <remarks>This method prints a message to the output console before quitting the application.
        /// It should be called in response to user interaction with the exit button in the main menu.</remarks>
        public void _on_exit_pressed()
        {
            GD.Print("Quit Button Pressed");
            GetTree().Quit();
        }
    }
}

