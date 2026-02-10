using ForceOfHell.Scripts.MainCharacter;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ForceOfHell.Scripts.Weapons
{
	public partial class DropWeapon : Area2D
	{
		[Export] private NodePath weaponSpritePath;
		[Export] private NodePath infoAreaPath;
		[Export] private NodePath popupPath;

		private readonly RandomNumberGenerator _rng = new();
		private Weapons _weaponSprite = default!;
		private Area2D? _infoArea;
		private PopupPanel? _popup;
		private Label? _nameLabel;
		private Label? _damageLabel;
		private Label? _fireRateLabel;
		private Label? _manaCostLabel;
		private Label? _descriptionLabel;

		private const float PopupOffsetY = 16f;
		private const float BobAmplitude = 6f;
		private const float BobSpeed = 2.5f;

		private Vector2 _basePosition;
		private float _bobTime;

		public Weapons Weapon { get; private set; } = default!;

		public override void _Ready()
		{
			_weaponSprite = GetNode<Weapons>(weaponSpritePath);
			_infoArea = GetNodeOrNull<Area2D>(infoAreaPath);
			_popup = GetNodeOrNull<PopupPanel>(popupPath);

			if (_popup != null)
			{
				_nameLabel = _popup.GetNode<Label>("VBoxContainer/Name");
				_damageLabel = _popup.GetNode<Label>("VBoxContainer/HBoxContainer3/HBoxContainer/DamageNum");
				_fireRateLabel = _popup.GetNode<Label>("VBoxContainer/HBoxContainer3/HBoxContainer2/FireRateNum");
				_manaCostLabel = _popup.GetNode<Label>("VBoxContainer/HBoxContainer3/HBoxContainer3/ManaCostNum");
				_descriptionLabel = _popup.GetNode<Label>("VBoxContainer/Description");

				_popup.Visible = false;
				_popup.Size = (Vector2I)_popup.GetContentsMinimumSize();
				_popup.Unfocusable = true;
				_popup.GuiDisableInput = true;
			}

			AssignRandomWeapon();

			if (_infoArea != null)
			{
				_infoArea.Connect(Area2D.SignalName.BodyEntered, new Callable(this, nameof(OnInfoAreaEntered)));
				_infoArea.Connect(Area2D.SignalName.BodyExited, new Callable(this, nameof(OnInfoAreaExited)));
			}

			_basePosition = Position;
			_bobTime = 0f;
		}

		public override void _Process(double delta)
		{
			_bobTime += (float)delta * BobSpeed;
			var offsetY = Mathf.Sin(_bobTime) * BobAmplitude;
			Position = _basePosition + new Vector2(0f, offsetY);
		}

		/// <summary>
		/// Selects a weapon at random from the available weapons and assigns it to the current instance.
		/// </summary>
		/// <remarks>If no weapons are available, the method logs an error and does not assign a weapon. The selection
		/// is uniformly random among all available weapons.</remarks>
		private void AssignRandomWeapon()
		{
			var weapons = ReadWeapons.Weapons;
			var count = weapons.Count;
			if (count == 0)
			{
				GD.PushError("[DropWeapon] El diccionario de armas está vacío.");
				return;
			}

			var targetIndex = _rng.RandiRange(0, count - 1);
			var index = 0;

			foreach (var pair in weapons)
			{
				if (index++ != targetIndex)
					continue;

				Weapon = pair.Value;
				_weaponSprite.SetWeapon(pair.Key);
				break;
			}
		}

		/// <summary>
		/// Handles the event when a node enters the information area, displaying weapon details in the popup if the node is a
		/// player.
		/// </summary>
		/// <remarks>If the popup is not available or the entering node is not a player, no action is taken.</remarks>
		/// <param name="body">The node that has entered the information area. Only nodes representing the player will trigger the display of
		/// weapon information.</param>
		private void OnInfoAreaEntered(Node body)
		{
			if (body is not Player || _popup == null)
				return;

			_nameLabel!.Text = Weapon.Name;
			_damageLabel!.Text = Weapon.Damage.ToString("0.##");
			_fireRateLabel!.Text = Weapon.FireRate.ToString("0.##");
			_manaCostLabel!.Text = Weapon.Cost.ToString("0.##");
			_descriptionLabel!.Text = Weapon.Description;

			PositionPopupAboveWeapon();
		}

		/// <summary>
		/// Handles the event when a node exits the information area.
		/// </summary>
		/// <param name="body">The node that has exited the information area. If the node is a player, the information popup will be hidden.</param>
		private void OnInfoAreaExited(Node body)
		{
			if (body is Player)
				_popup?.Hide();
		}

		/// <summary>
		/// Positions the popup UI element directly above the weapon sprite on the screen.
		/// </summary>
		/// <remarks>This method calculates the popup's position based on the weapon sprite's current screen
		/// coordinates and displays the popup above it with a vertical offset. If the popup is not initialized, the method
		/// does nothing.</remarks>
		private void PositionPopupAboveWeapon()
		{
			if (_popup == null)
				return;

			var screenPos = _weaponSprite.GetGlobalTransformWithCanvas().Origin;
			var size = _popup.Size;
			var popupPos = new Vector2I(
				(int)(screenPos.X - size.X * 0.5f),
				(int)(screenPos.Y - size.Y - PopupOffsetY)
			);

			_popup.Popup(new Rect2I(popupPos, size));
		}

		/// <summary>
		/// Handles the event when a body enters the trigger area, granting mana and updating the player's weapon if
		/// applicable.
		/// </summary>
		/// <remarks>This method is typically connected to a physics or area trigger event. It only affects nodes of
		/// type Player and performs no action for other node types.</remarks>
		/// <param name="body">The node that has entered the trigger area. If the node is a player with a matching equipped weapon, mana is
		/// granted and the weapon is updated.</param>
		private void OnBodyEntered(Node body)
		{
			if (body is Player player)
			{
				if (player.equip_weapon.Id == Weapon.Id)
				{
					player.manaActual += 50;
					if (player.manaActual > player.GetManaMax())
						player.manaActual = player.GetManaMax();
				}
			   
				player.SetAnimation("PickUp");
				player.ChangeWeapon(Weapon.Id);
				QueueFree();
			}
		}
	}
}
