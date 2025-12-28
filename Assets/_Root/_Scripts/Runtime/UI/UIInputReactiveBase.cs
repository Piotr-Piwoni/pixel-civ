using PixelCiv.Managers;
using UnityEngine;
using DeviceType = PixelCiv.Managers.DeviceType;

namespace PixelCiv.UI
{
public abstract class UIInputReactiveBase : MonoBehaviour
{
	protected virtual void Start()
	{
		RegisterToUIManager();
	}

	public abstract void HandleDeviceChange(DeviceType deviceType);

	protected void RegisterToUIManager()
	{
		UIManager.Instance.AddReactiveUI(this);
	}
}
}