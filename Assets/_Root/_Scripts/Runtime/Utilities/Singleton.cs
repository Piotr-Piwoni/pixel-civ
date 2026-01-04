using PixelCiv.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

// Inspired and taken from a Tarodev video
// - Unity Architecture for Noobs - Game Structure
// URL: https://www.youtube.com/watch?v=tE1qH8OxO2Y

namespace PixelCiv.Utilities
{
/// <summary>
///     A static instance is similar to a singleton, but instead of destroying
///     a new instance, it overrides the current instance.
///     Great for resetting object state.
/// </summary>
/// <typeparam name="T">The class to make a static instance.</typeparam>
public abstract class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour
{
	public static T Instance { get; private set; }

	protected virtual void Awake()
	{
		if (!Instance)
			Instance = this as T;
		else if (Instance != this)
			Destroy(gameObject);
	}

	protected virtual void OnApplicationQuit()
	{
		Instance = null;
		Destroy(gameObject);
	}
}

/// <summary>
///     A basic singleton. It will destroy any new versions created, leaving the
///     original instance intact.
/// </summary>
/// <typeparam name="T">The class to make a singleton.</typeparam>
public abstract class Singleton<T> : StaticInstance<T> where T : MonoBehaviour
{
	protected override void Awake()
	{
		if (Instance)
		{
			Destroy(gameObject);
			return;
		}

		base.Awake();
	}
}

/// <summary>
///     A persistent version of the singleton. This will survive through scene
///     loads.
/// </summary>
/// <typeparam name="T">The class to make persistent.</typeparam>
public abstract class PersistentSingleton<T> : Singleton<T>, ISceneChangeHandler
	where T : MonoBehaviour
{
	protected override void Awake()
	{
		base.Awake();
		if (Instance != this) return;
		transform.SetParent(null); //< Make this a root object if not already.
		DontDestroyOnLoad(gameObject);
	}

	public virtual void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneChange;
	}

	public virtual void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneChange;
	}

	public abstract void OnSceneChange(Scene scene, LoadSceneMode mode);
}
}