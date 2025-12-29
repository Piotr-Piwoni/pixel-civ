using System;
using PixelCiv.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;

namespace PixelCiv.Managers
{
public class UIManager : Singleton<UIManager>
{
	[SerializeField, TabGroup("", "Info", SdfIconType.QuestionSquareFill, TextColor = "lightblue"), ReadOnly,]
	private UIDocument _Document;
	[SerializeField, TabGroup("", "Settings", SdfIconType.GearFill, TextColor = "yellow"),]
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
		if (!InputManager.Instance)
			return;

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
			throw new ArgumentOutOfRangeException(nameof(deviceType),
												  deviceType, null);
		}
	}

	private void SpawnUnitsButtonOnClicked()
	{
		if (!UnitManager.Instance)
			return;

		Vector3Int capitalPos = GameManager.Instance.PlayerCapitalPosition;
		const int SEARCH_RANGE = 1;
		for (int y = -SEARCH_RANGE; y < SEARCH_RANGE; y++)
		for (int x = -SEARCH_RANGE; x < SEARCH_RANGE; x++)
		{
			// Skip center tile.
			if (x == 0 && y == 0) continue;
			bool created = UnitManager.Instance.CreateUnit(capitalPos + new Vector3Int(x, y, 0), Color.blue);
			// Exit on successful tile creation.
			if (created) return;
		}
	}
}
}
