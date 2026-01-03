using System;
using PixelCiv.Managers;
using PixelCiv.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using DeviceType = PixelCiv.Managers.DeviceType;

namespace PixelCiv
{
[HideMonoScript]
public class PlayerController : MonoBehaviour
{
	[SerializeField, Header("Movement Settings"),
	 Tooltip("Speed of the player movement"),]
	private float _MoveSpeed = 5f;
	[SerializeField, Tooltip("Speed multiplier applied when the player is sprinting."),]
	private float _SprintMult = 1.5f;

	private Camera _Camera;
	[ShowInInspector, ReadOnly,]
	private Guid _SelectedUnit;


	private void Awake()
	{
		InputManager.Instance.SetPlayerInput(GetComponent<PlayerInput>());
		_Camera = Camera.main;
	}

	private void Update()
	{
		Vector2 moveInput = InputManager.Instance.MoveInput;
		MoveCharacter(moveInput);
		MoveUnit();
	}

	private void OnEnable()
	{
		InputManager.Instance.OnInteractionPressed += OnSelect;
		InputManager.Instance.OnDeviceChanged += HandleDeviceChanged;
	}

	private void OnDisable()
	{
		if (!InputManager.Instance) return;
		InputManager.Instance.OnInteractionPressed -= OnSelect;
		InputManager.Instance.OnDeviceChanged -= HandleDeviceChanged;
	}

	public void SetSpeed(float newSpeed)
	{
		_MoveSpeed = newSpeed;
	}

	private void HandleDeviceChanged(DeviceType deviceType)
	{
		switch (deviceType)
		{
		case DeviceType.KeyboardMouse:
			break;
		case DeviceType.Gamepad:
			break;
		case DeviceType.Unknown:
			throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, null);
		}
	}

	private void MoveCharacter(Vector2 move)
	{
		if (!_Camera)
			return;

		float speed = _MoveSpeed;
		if (InputManager.Instance.IsSprinting)
			speed *= _SprintMult;

		Vector3 movement = move * (speed * Time.deltaTime);
		transform.Translate(movement, Space.World);
	}

	private void MoveUnit()
	{
		if (_SelectedUnit == Guid.Empty || !Mouse.current.rightButton.wasPressedThisFrame)
			return;

		HexCoords mouseHexCoords =
				Utils.GetMouseHexCoords(_Camera, GameManager.Instance.Grid);
		Hex targetHex = GameManager.Instance.HexMap.Find(mouseHexCoords);
		if (targetHex == null) return;

		UnitManager.Instance.Move(_SelectedUnit, targetHex.Coordinates);
		_SelectedUnit = Guid.Empty;
		Debug.Log($"{nameof(_SelectedUnit)} set to: {_SelectedUnit}");
	}

	private void OnSelect()
	{
		if (!GameManager.Instance) return;

		HexCoords mouseHexCoords =
				Utils.GetMouseHexCoords(_Camera, GameManager.Instance.Grid);
		Hex selectedHex = GameManager.Instance.HexMap.Find(mouseHexCoords);
		if (selectedHex == null) return;

		_SelectedUnit = selectedHex.UnitID;
		Debug.Log($"{nameof(_SelectedUnit)} set to: {_SelectedUnit}");
		Debug.Log("--- Hex ---\n" +
				  $"Cell Coord: {selectedHex.Coordinates.Offset}\n" +
				  $"Axial Coord: {selectedHex.Coordinates.Axial}\n" +
				  $"Unit ID: {selectedHex.UnitID}\n" +
				  $"Building: {selectedHex.Building}");
	}
}
}
