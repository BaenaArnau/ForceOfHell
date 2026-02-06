using System.Collections.Generic;
using Godot;

namespace ForceOfHell.Scripts.StateMachine.MovementStateMachine
{
	/// <summary>
	/// Máquina de estados para el movimiento del jugador.
	/// </summary>
	public partial class MovementStateMachine : Node
	{
		/// <summary>Ruta (NodePath) al estado inicial dentro de este nodo.</summary>
		[Export] public NodePath initialState;

		/// <summary>Mapa de estados indexado por el <c>Node.Name</c> de cada estado.</summary>
		private Dictionary<string, State > _states;

		/// <summary>Estado actualmente activo en la máquina.</summary>
		private State _current_state;

		/// <summary>Referencia al jugador para verificar si está muriendo.</summary>
		private ForceOfHell.Scripts.MainCharacter.Player _player;

		/// <summary>Inicialización de la máquina de estados: carga estados hijos y establece el estado inicial.</summary>
		public override void _Ready()
		{
			_states = new Dictionary<string, State>();
			foreach (Node node in GetChildren())
			{
				if (node is State s)
				{
					_states[node.Name] = s;
					s.StateMachine = this;
					s.Ready();
					s.Exit();
				}
			}

			_current_state = GetNode<State>(initialState);
			_current_state.Enter();

			var parent = GetParent();
			if (parent is ForceOfHell.Scripts.MainCharacter.Player player)
				_player = player;
		}

		/// <summary>Procesamiento por frame: delega al estado actual.</summary>
		/// <param name="delta">Delta en segundos desde el último frame.</param>
		public override void _Process(double delta)
		{
			if (_player != null && _player.IsDying)
				return;
			
			_current_state.Update(delta);
		}

		/// <summary>Procesamiento de física: delega al estado actual.</summary>
		/// <param name="delta">Delta en segundos desde el último paso de física.</param>
		public override void _PhysicsProcess(double delta)
		{
			if (_player != null && _player.IsDying)
				return;
			
			_current_state.UpdatePhysics(delta);
		}

		/// <summary>Procesamiento de eventos de entrada: delega al estado actual.</summary>
		/// <param name="event">Evento de entrada.</param>
		/// <returns>Si el evento fue procesado.</returns>
		public override void _UnhandledInput(InputEvent @event)
		{
			if (_player != null && _player.IsDying)
				return;
			
			_current_state.HandleInput(@event);
		}

		/// <summary>
		/// Pide la transición a otro estado por su clave (nombre del nodo).
		/// Si la clave no existe o ya estamos en ese estado la petición se ignora.
		/// </summary>
		/// <param name="key">Nombre del nodo-estado destino dentro de este nodo.</param>
		public void TransitionTo(string key)
		{
			if (!_states.ContainsKey(key) || _current_state == _states[key])
				return;

			_current_state.Exit();
			_current_state = _states[key];
			_current_state.Enter();
		}
	}
}