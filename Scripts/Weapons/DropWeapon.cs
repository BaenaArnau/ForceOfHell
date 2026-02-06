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

		private void OnInfoAreaExited(Node body)
		{
			if (body is Player)
				_popup?.Hide();
		}

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

		private void OnBodyEntered(Node body)
		{
			if (body is Player player)
			{
				player.SetAnimation("PickUp");
				player.ChangeWeapon(Weapon.Id);
				QueueFree();
			}
		}
	}
}
