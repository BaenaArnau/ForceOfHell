using Godot;

namespace ForceOfHell.Scripts.MainCharacter
{
    /// <summary>
    /// Represents a 2D camera that follows the player and adjusts its position based on the player's movement.
    /// </summary>
    /// <remarks>This class inherits from Camera2D and provides functionality to smooth the camera's position
    /// based on the player's velocity. It includes methods for controlling drag and smoothing speed, which are called
    /// during the game loop.</remarks>
    public partial class Camera : Camera2D
    {
        /// <summary>
        /// Exporta el nodo Player para acceder a sus propiedades.
        /// </summary>
        [Export] private Player player;

        /// <summary>
        /// Velocidad de arrastre objetivo hacia la izquierda.
        /// </summary>
        private float targetDragLeft = 0f;

        /// <summary>
        /// Velocidad de arrastre objetivo hacia la derecha.
        /// </summary>
        private float targetDragRight = 0f;

        /// <summary>
        /// Velocidad de suavizado horizontal.
        /// </summary>
        private float smoothSpeedHorizontal = 5f;

        /// <summary>
        /// Velocidad de suavizado vertical.
        /// </summary>
        private float smoothSpeedVertical = 8f;

        /// <summary>
        /// Método que se llama cuando el nodo está listo.
        /// </summary>
        public override void _Ready()
        {
            PositionSmoothingEnabled = false;
            PositionSmoothingSpeed = smoothSpeedHorizontal;
        }

        /// <summary>
        /// Método que se llama en cada frame para actualizar la cámara.
        /// </summary>
        public override void _Process(double delta)
        {
            PositionSmoothingEnabled = true;
            controlDrag();
            smoothingSpeed(delta);
        }

        /// <summary>
        /// Método que se llama para suavizar la velocidad de la cámara.
        /// </summary>
        private void smoothingSpeed(double delta)
        {
            DragLeftMargin = Mathf.Lerp(DragLeftMargin, targetDragLeft, (float)delta * 5f);
            DragRightMargin = Mathf.Lerp(DragRightMargin, targetDragRight, (float)delta * 5f);

            float deltaY = player.GlobalPosition.Y - GlobalPosition.Y;

            if (Mathf.Abs(deltaY) > 0.1f)
                PositionSmoothingSpeed = smoothSpeedVertical;
            else
                PositionSmoothingSpeed = smoothSpeedHorizontal;
        }

        /// <summary>
        /// Controla el arrastre de la cámara según la velocidad del jugador.
        /// </summary>
        private void controlDrag()
        {
            if (player.Velocity.X > 10)
            {
                targetDragLeft = 0.6f;
                targetDragRight = -1.0f;
            }
            else if (player.Velocity.X < -10)
            {
                targetDragLeft = -1.0f;
                targetDragRight = 0.6f;
            }
            else
            {
                targetDragLeft = 0f;
                targetDragRight = 0f;
            }
        }
    }
}

