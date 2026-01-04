using System;
using System.Collections.Generic;
using PixelCiv.Components;
using PixelCiv.Utilities;
using PixelCiv.Utilities.Hex;
using PixelCiv.Utilities.Types;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;
using DeviceType = PixelCiv.Utilities.Types.DeviceType;

namespace PixelCiv.Managers
{
[HideMonoScript]
public class UIManager : Singleton<UIManager>
{
	[SerializeField,
	 TabGroup("", "Info", SdfIconType.QuestionSquareFill, TextColor = "lightblue"),
	 ReadOnly,]
	private UIDocument _Document;
	[SerializeField,
	 TabGroup("", "Settings", SdfIconType.GearFill, TextColor = "yellow"),]
	private VisualTreeAsset _StartingUI;


	protected override void Awake()
	{
		base.Awake();

		// Attempt to find a UI Document in the scene.
		_Document = FindAnyObjectByType<UIDocument>(FindObjectsInactive.Include);
		if (_Document)
		{
			_Document.gameObject.SetActive(true);
			_Document.visualTreeAsset = _StartingUI;
		}
		else
			Debug.Log("No UIDocument component was found in the scene.");
	}

	private void Start()
	{
		var spawnUnitsButton = _Document.rootVisualElement.Q<Button>("SpawnUnitsButton");
		spawnUnitsButton.clicked += SpawnUnitsButtonOnClicked;
	}

	private void OnEnable()
	{
		InputManager.Instance.OnDeviceChanged += OnDeviceChanged;
	}

	private void OnDisable()
	{
		if (!InputManager.Instance) return;
		InputManager.Instance.OnDeviceChanged -= OnDeviceChanged;
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
			throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, null);
		}
	}

	private void SpawnUnitsButtonOnClicked()
	{
		if (!UnitManager.Instance)
			return;

		HexCoords capitalCoords = GameManager.Instance.PlayerCapitalPosition;

		const int SEARCH_RANGE = 3;
		List<HexCoords> area = Hex.GetSpiral(capitalCoords, SEARCH_RANGE);
		foreach (HexCoords hexCoords in area)
		{
			Hex hex = GameManager.Instance.HexMap.Find(hexCoords);
			if (hex is not { Type: TileType.Grassland, }) continue;

			Unit unit = UnitManager.Instance.CreateUnit(UnitType.Footman, hexCoords,
														Color.blue);
			// Exit on successful unit creation.
			if (!unit) continue;
			hex.UnitID = unit.ID;
			return;
		}
	}
}
}
