using System;
using UnityEngine;
using UnityEngine.UI;
using DeviceType = PixelCiv.Managers.DeviceType;

namespace PixelCiv.UI
{
public class IconSwap : UIInputReactiveBase
{
	[SerializeField]
	private Sprite _KeyboardMouseIcon;
	[SerializeField]
	private Sprite _GamepadIcon;

	private Image _Image;


	private void Awake()
	{
		_Image = GetComponent<Image>();
	}

	public override void HandleDeviceChange(DeviceType deviceType)
	{
		switch (deviceType)
		{
		case DeviceType.KeyboardMouse:
			if (!_KeyboardMouseIcon)
			{
				Debug.LogWarning($"{name} has no {nameof(_KeyboardMouseIcon)} assigned.");
				break;
			}

			_Image.sprite = _KeyboardMouseIcon;
			break;
		case DeviceType.Gamepad:
			if (!_GamepadIcon)
			{
				Debug.LogWarning($"{name} has no {nameof(_GamepadIcon)} assigned.");
				break;
			}

			_Image.sprite = _GamepadIcon;
			break;
		case DeviceType.Unknown:
			throw new ArgumentOutOfRangeException(nameof(deviceType),
												deviceType, null);
		}
	}
}
}