using Godot;
using System;
using ForceOfHell.Scripts.StateMachine.MovementStateMachine;

namespace ForceOfHell.Scripts.StateMachine
{
    /// <summary>
    /// Clase base para todos los estados de la máquina de movimiento.
    /// Contiene los métodos ciclo de vida que deben implementar los estados concretos.
    /// </summary>
    public partial class State : Node
    {
        /// <summary>
        /// Referencia a la máquina de estados que contiene este estado.
        /// Se asigna desde <see cref="MovementStateMachine"/> en tiempo de preparación.
        /// Expuesta como lectura pública pero solo asignable internamente por la máquina.
        /// </summary>
        private MovementStateMachine.MovementStateMachine _stateMachine;

        /// <summary>
        /// Propiedad pública para acceder a la máquina de estados. El setter es interno.
        /// </summary>
        public MovementStateMachine.MovementStateMachine StateMachine
        {
            get => _stateMachine;
            internal set
            {
                _stateMachine = value;
                stateMachine = value;
            }
        }

        /// <summary>
        /// Campo protegido mantenido por compatibilidad. Los estados existentes usan `stateMachine`.
        /// </summary>
        protected MovementStateMachine.MovementStateMachine stateMachine;

        /// <summary>Se invoca cuando el estado empieza (se entra en él).</summary>
        public virtual void Enter() { }

        /// <summary>Se invoca cuando el estado termina (se sale de él).</summary>
        public virtual void Exit() { }

        /// <summary>Se invoca una sola vez cuando se inicializa el nodo/estado.
        /// Versión asíncrona para permitir esperas (por ejemplo, espera a otros nodos).</summary>
        public new virtual System.Threading.Tasks.Task Ready() { return System.Threading.Tasks.Task.CompletedTask; }

        /// <summary>Actualización por frame (non-physics). Delta en segundos.</summary>
        /// <param name="delta">Tiempo en segundos desde el último frame.</param>
        public virtual void Update(double delta) { }

        /// <summary>Actualización en el paso de física (_PhysicsProcess). Delta en segundos.</summary>
        /// <param name="delta">Tiempo en segundos desde la última física.</param>
        public virtual void UpdatePhysics(double delta) { }

        /// <summary>Procesa eventos de entrada no manejados (unhandled input).</summary>
        /// <param name="ev">Evento de entrada recibido.</param>
        public virtual void HandleInput(InputEvent ev) { }
    }
}
