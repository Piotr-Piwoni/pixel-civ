using System.Collections.Generic;
using PROJECTNAME.Interfaces;
using UnityEngine.SceneManagement;

namespace PROJECTNAME.Utilities
{
public class PersistentSystems : PersistentSingleton<PersistentSystems>
{
	private readonly List<ISceneChangeHandler> _SceneChangeHandlers = new();

	protected override void Awake()
	{
		base.Awake();
		// Remove itself from the scene change handlers list.
		if (_SceneChangeHandlers.Contains(this))
			_SceneChangeHandlers.Remove(this);
	}

	public override void OnSceneChange(Scene scene, LoadSceneMode mode)
	{
		// Call OnSceneChange() for all registered systems.
		foreach (ISceneChangeHandler handler in _SceneChangeHandlers)
			handler.OnSceneChange(scene, mode);
	}

	/// Register a system that implements ISceneChangeHandler.
	public void RegisterSystem(ISceneChangeHandler handler)
	{
		if (!_SceneChangeHandlers.Contains(handler))
			_SceneChangeHandlers.Add(handler);
	}

	/// Remove a system that implements ISceneChangeHandler.
	public void RemoveSystem(ISceneChangeHandler handler)
	{
		if (_SceneChangeHandlers.Contains(handler))
			_SceneChangeHandlers.Remove(handler);
	}
}
}