using System;
using System.Collections.Generic;
using PixelCiv.Utilities;
using PixelCiv.Utilities.Types;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using DeviceType = PixelCiv.Utilities.Types.DeviceType;

namespace PixelCiv.Managers
{
[HideMonoScript]
public class InputManager : PersistentSingleton<InputManager>
{
	public event Action OnInteractionPressed;
	public event Action OnJumpPressed;
	public event Action OnMovePressed;
	public event Action<DeviceType> OnDeviceChanged;

	public bool IsSprinting { get; private set; }
	[TabGroup("", "Info", SdfIconType.QuestionSquareFill, TextColor = "lightblue"),
	 ShowInInspector, ReadOnly,]
	public DeviceType CurrentDeviceType { get; private set; } = DeviceType.Unknown;
	public Vector2 LookInput { get; private set; } = Vector2.zero;
	public Vector2 MoveInput { get; private set; } = Vector2.zero;

	[SerializeField, TabGroup("", "Info"), ReadOnly,]
	private PlayerInput _PlayerInput;
	[SerializeField, TabGroup("", "Settings", SdfIconType.GearFill, TextColor = "yellow"),
	 FoldoutGroup("/Settings/Input References"),]
	private InputActionReference _MoveAction;
	[SerializeField, FoldoutGroup("/Settings/Input References"),]
	private InputActionReference _LookAction;
	[SerializeField, FoldoutGroup("/Settings/Input References"),]
	private InputActionReference _JumpAction;
	[SerializeField, FoldoutGroup("/Settings/Input References"),]
	private InputActionReference _InteractionAction;
	[SerializeField, FoldoutGroup("/Settings/Input References"),]
	private InputActionReference _SprintAction;

	[SerializeField, FoldoutGroup("/Settings/Action Maps"),]
	private string _GameplayActionMap = "Gameplay";
	[SerializeField, FoldoutGroup("/Settings/Action Maps"),]
	private string _UIActionMap = "UI";

	private Action<InputAction.CallbackContext> _InteractionCallback;
	private Action<InputAction.CallbackContext> _JumpCallback;
	private Dictionary<ActionMap, string> _ActionMapDictionary;


	protected override void Awake()
	{
		base.Awake();
		InitializeActionMaps();
	}

	public override void OnEnable()
	{
		base.OnEnable();
		BindInput();

		if (!_PlayerInput) return;
		_PlayerInput.onControlsChanged += OnControlsChanged;
		UpdateCurrentDeviceType(_PlayerInput.currentControlScheme);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		UnbindInput();

		if (_PlayerInput)
			_PlayerInput.onControlsChanged -= OnControlsChanged;
	}

	public override void OnSceneChange(Scene scene, LoadSceneMode mode) { }

	public void SetPlayerInput(PlayerInput playerInput)
	{
		if (!playerInput)
			return;

		_PlayerInput = playerInput;
		_PlayerInput.onControlsChanged += OnControlsChanged;
		UpdateCurrentDeviceType(_PlayerInput.currentControlScheme);
	}

	/// <summary>
	///     Switch the currently in-use action map to a new one.
	/// </summary>
	/// <param name="actionMap">The map to switch to.</param>
	public void SwitchActionMap(ActionMap actionMap)
	{
		if (!_PlayerInput)
		{
			Debug.LogWarning("The Input Manager has no Player Input!");
			return;
		}

		if (_ActionMapDictionary.TryGetValue(actionMap, out string actionMapName))
			_PlayerInput.SwitchCurrentActionMap(actionMapName);
		else
			Debug.LogError($"No action map found for \"{actionMap}\"");
	}

	private void BindInput()
	{
		_MoveAction.action.performed += OnMovePerformed;
		_MoveAction.action.canceled += OnMoveCanceled;
		_LookAction.action.performed += OnLookPerformed;
		_LookAction.action.canceled += OnLookCanceled;
		_SprintAction.action.performed += OnSprintPressed;
		_SprintAction.action.canceled += OnSprintCanceled;

		_JumpCallback = _ => OnJumpPressed?.Invoke();
		_JumpAction.action.performed += _JumpCallback;
		_InteractionCallback = _ => OnInteractionPressed?.Invoke();
		_InteractionAction.action.performed += _InteractionCallback;

		EnableAllActions();
	}

	private void DisableAllActions()
	{
		_MoveAction.action.Disable();
		_JumpAction.action.Disable();
		_InteractionAction.action.Disable();
		_SprintAction.action.Disable();
	}

	private void EnableAllActions()
	{
		_MoveAction.action.Enable();
		_JumpAction.action.Enable();
		_InteractionAction.action.Enable();
		_SprintAction.action.Enable();
	}

	/// Initialize a dictionary that links the ActionMap enum with their
	/// string counterpart.
	private void InitializeActionMaps()
	{
		_ActionMapDictionary = new Dictionary<ActionMap, string>
		{
				{ ActionMap.Gameplay, _GameplayActionMap },
				{ ActionMap.UI, _UIActionMap },
		};
	}

	/// Handles changing the control scheme.
	private void OnControlsChanged(PlayerInput input)
	{
		if (input.currentControlScheme == null) return;
		UpdateCurrentDeviceType(input.currentControlScheme);
	}

	private void OnLookCanceled(InputAction.CallbackContext context)
	{
		LookInput = Vector2.zero;
	}

	private void OnLookPerformed(InputAction.CallbackContext context)
	{
		LookInput = context.ReadValue<Vector2>();
	}

	private void OnMoveCanceled(InputAction.CallbackContext context)
	{
		MoveInput = Vector2.zero;
	}

	private void OnMovePerformed(InputAction.CallbackContext context)
	{
		OnMovePressed?.Invoke();
		MoveInput = context.ReadValue<Vector2>();
	}

	private void OnSprintCanceled(InputAction.CallbackContext context)
	{
		IsSprinting = false;
	}

	private void OnSprintPressed(InputAction.CallbackContext context)
	{
		IsSprinting = true;
	}

	private void UnbindInput()
	{
		_MoveAction.action.performed -= OnMovePerformed;
		_MoveAction.action.canceled -= OnMoveCanceled;
		_LookAction.action.performed -= OnLookPerformed;
		_LookAction.action.canceled -= OnLookCanceled;
		_SprintAction.action.performed -= OnSprintPressed;
		_SprintAction.action.canceled -= OnSprintCanceled;

		_JumpAction.action.performed -= _JumpCallback;
		_InteractionAction.action.performed -= _InteractionCallback;

		if (_PlayerInput)
			_PlayerInput.onControlsChanged -= OnControlsChanged;

		DisableAllActions();
	}

	/// Internal function used to convert the "PlayerInput.currentControlScheme"
	/// from string to DeviceType.
	private void UpdateCurrentDeviceType(string controlScheme)
	{
		DeviceType newDevice = controlScheme switch
		{
				"Keyboard&Mouse" => DeviceType.KeyboardMouse,
				"Gamepad" => DeviceType.Gamepad,
				_ => DeviceType.Unknown,
		};

		if (newDevice == CurrentDeviceType) return;
		CurrentDeviceType = newDevice;
		Debug.Log($"Device Changed: <color=red>{CurrentDeviceType}</color>");
		OnDeviceChanged?.Invoke(CurrentDeviceType);
	}
}
}
