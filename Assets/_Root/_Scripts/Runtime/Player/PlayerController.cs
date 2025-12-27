using System;
using PROJECTNAME.Managers;
using Unity.Cinemachine;
using UnityEngine;
using DeviceType = PROJECTNAME.Managers.DeviceType;

namespace Demos
{
public class PlayerController : MonoBehaviour
{
	[Header("Movement Settings"), SerializeField,
	 Tooltip("Speed of the player movement")]
	private float _MoveSpeed = 5f;

	private CinemachineCamera _Camera;
	private float _CameraPitch;
	private float _CameraYaw;
	private float _LookSensitivity = 0.35f;
	private Transform _CameraTransform;


	private void Awake()
	{
		// Grab Cinemachine camera from children.
		_Camera = GetComponentInChildren<CinemachineCamera>();
		if (_Camera)
			_CameraTransform = _Camera.transform;
		else
			Debug.LogError("No Cinemachine Camera found as a child!");
	}

	private void Update()
	{
		Vector2 moveInput = InputManager.Instance.MoveInput;
		MoveCharacter(moveInput);

		Vector2 lookInput = InputManager.Instance.LookInput;
		LookAround(lookInput);
	}

	private void OnEnable()
	{
		InputManager.Instance.OnMovePressed += HandleMovement;
		InputManager.Instance.OnJumpPressed += HandleJump;
		InputManager.Instance.OnAttackPressed += HandleAttack;
		InputManager.Instance.OnInteractionPressed += HandleInteraction;
		InputManager.Instance.OnDeviceChanged += HandleDeviceChanged;
	}

	private void OnDisable()
	{
		if (!InputManager.Instance) return;

		InputManager.Instance.OnMovePressed -= HandleMovement;
		InputManager.Instance.OnJumpPressed -= HandleJump;
		InputManager.Instance.OnAttackPressed -= HandleAttack;
		InputManager.Instance.OnInteractionPressed -= HandleInteraction;
		InputManager.Instance.OnDeviceChanged -= HandleDeviceChanged;
	}

	public void SetSpeed(float newSpeed)
	{
		_MoveSpeed = newSpeed;
	}

	private void HandleAttack()
	{
		Debug.Log("Attack Pressed!");
	}

	private void HandleDeviceChanged(DeviceType deviceType)
	{
		switch (deviceType)
		{
			case DeviceType.KeyboardMouse:
				_LookSensitivity = 0.35f;
				break;
			case DeviceType.Gamepad:
				_LookSensitivity = 1f;
				break;
			case DeviceType.Unknown:
				throw new ArgumentOutOfRangeException(nameof(deviceType),
					deviceType, null);
		}
	}

	private void HandleInteraction()
	{
		Debug.Log("Interaction Pressed!");
	}

	private void HandleJump()
	{
		Debug.Log("Jump Pressed!");
	}

	private void HandleMovement()
	{
		Debug.Log("Movement Input Detected!");
	}

	private void LookAround(Vector2 look)
	{
		if (!_CameraTransform) return;

		_CameraYaw += look.x * _LookSensitivity;
		_CameraPitch -= look.y * _LookSensitivity;
		// Limit pitch to avoid flipping.
		_CameraPitch = Mathf.Clamp(_CameraPitch, -80f, 80f);

		_CameraTransform.rotation =
			Quaternion.Euler(_CameraPitch, _CameraYaw, 0f);
	}

	private void MoveCharacter(Vector2 move)
	{
		if (!_CameraTransform) return;

		// Get camera forward and right.
		Vector3 forward = _CameraTransform.forward;
		forward.y = 0;
		forward.Normalize();

		Vector3 right = _CameraTransform.right;
		right.y = 0;
		right.Normalize();

		Vector3 movement = (forward * move.y + right * move.x) *
		                   (_MoveSpeed * Time.deltaTime);
		transform.Translate(movement, Space.World);
	}
}
}