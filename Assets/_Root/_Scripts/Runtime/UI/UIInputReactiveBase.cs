using PROJECTNAME.Managers;
using UnityEngine;
using DeviceType = PROJECTNAME.Managers.DeviceType;

namespace PROJECTNAME.UI
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