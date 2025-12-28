using System;
using System.Collections.Generic;
using PixelCiv.UI;
using PixelCiv.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PixelCiv.Managers
{
public class UIManager : Singleton<UIManager>
{
	[SerializeField,
	 TabGroup("", "Settings", SdfIconType.GearFill, TextColor = "yellow"),
	 FoldoutGroup("/Settings/Prefabs")]
	private GameObject _CrosshairCanvasPrefab;

	private readonly List<UIInputReactiveBase> _ReactiveUIs = new();

	[TabGroup("", "Info", SdfIconType.QuestionSquareFill,
		 TextColor = "lightblue"), ShowInInspector, ReadOnly]
	private Canvas _CrosshairCanvas;


	protected override void Awake()
	{
		base.Awake();

		// If the player exists and the prefab to the crosshair canvas was provide, spawn it.
		if (GameManager.Instance.Player && _CrosshairCanvasPrefab)
		{
			_CrosshairCanvas = Instantiate(_CrosshairCanvasPrefab, transform)
				.GetComponent<Canvas>();
		}
	}

	private void OnEnable()
	{
		InputManager.Instance.OnDeviceChanged += OnDeviceChanged;
	}

	private void OnDisable()
	{
		if (!InputManager.Instance)
			return;

		InputManager.Instance.OnDeviceChanged -= OnDeviceChanged;
	}

	public void AddReactiveUI(UIInputReactiveBase uiInputReactiveBase)
	{
		if (!_ReactiveUIs.Contains(uiInputReactiveBase))
			_ReactiveUIs.Add(uiInputReactiveBase);
	}

	private void OnDeviceChanged(DeviceType deviceType)
	{
		switch (deviceType)
		{
			case DeviceType.KeyboardMouse:
				Debug.Log("Showing Keyboard & Mouse UI.");
				break;
			case DeviceType.Gamepad:
				Debug.Log("Showing Gamepad UI.");
				break;
			case DeviceType.Unknown:
				throw new ArgumentOutOfRangeException(nameof(deviceType),
					deviceType, null);
		}

		foreach (UIInputReactiveBase uiInputReactiveBase in _ReactiveUIs)
			uiInputReactiveBase.HandleDeviceChange(deviceType);
	}
}
}