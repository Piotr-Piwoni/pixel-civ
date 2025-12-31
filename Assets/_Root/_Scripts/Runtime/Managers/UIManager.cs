using System;
using PixelCiv.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;

namespace PixelCiv.Managers
{
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

		Vector2Int capitalAxial = Hex.OffsetToAxial(
				GameManager.Instance.PlayerCapitalPosition);

		const int SEARCH_RANGE = 1;
		for (var radius = 1; radius <= SEARCH_RANGE; radius++)
			foreach (Vector2Int hexAxial in Hex.GetRing(capitalAxial, radius))
			{
				Hex hex = GameManager.Instance.HexMap.Find(hexAxial);
				if (hex == null || !hex.Visuals) continue;

				Unit unit = UnitManager.Instance.CreateUnit(hexAxial, Color.blue);
				// Exit on successful unit creation.
				if (!unit) continue;
				hex.UnitID = unit.ID;
				return;
			}
	}
}
}
