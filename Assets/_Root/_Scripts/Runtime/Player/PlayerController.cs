using System;
using PixelCiv.Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using DeviceType = PixelCiv.Managers.DeviceType;

namespace PixelCiv
{
public class PlayerController : MonoBehaviour
{
	[SerializeField, Header("Movement Settings"), Tooltip("Speed of the player movement"),]
	private float _MoveSpeed = 5f;

	private Camera _Camera;


	private void Awake()
	{
		InputManager.Instance.SetPlayerInput(GetComponent<PlayerInput>());
		_Camera = Camera.main;
	}

	private void Update()
	{
		Vector2 moveInput = InputManager.Instance.MoveInput;
		MoveCharacter(moveInput);
	}

	private void OnEnable()
	{
		InputManager.Instance.OnInteractionPressed += HandleInteraction;
		InputManager.Instance.OnDeviceChanged += HandleDeviceChanged;
	}

	private void OnDisable()
	{
		if (!InputManager.Instance) return;

		InputManager.Instance.OnInteractionPressed -= HandleInteraction;
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

	private void HandleInteraction()
	{
		Debug.Log("Interaction Pressed!");
	}

	private void MoveCharacter(Vector2 move)
	{
		if (!_Camera) return;
		Vector3 movement = move * (_MoveSpeed * Time.deltaTime);
		transform.Translate(movement, Space.World);
	}
}
}
