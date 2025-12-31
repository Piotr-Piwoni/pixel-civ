using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace PixelCiv
{
[ExecuteInEditMode]
public class Spawner : MonoBehaviour
{
	public SpawnerTag SpawnerTag;

	[SerializeField]
	private GameObject _SpawnablePrefab;


	private void Awake()
	{
		SpawnObject();
	}

	/// <summary>
	///     Spawns the provided object.
	/// </summary>
	/// <param name="spawnObject">The object to spawn.</param>
	/// <param name="facingSame">
	///     Should the object be facing the same direction as the
	///     spawner.
	/// </param>
	public void Spawn(Transform spawnObject, bool facingSame = false)
	{
		spawnObject.position = transform.position;
		if (facingSame)
			spawnObject.rotation = transform.rotation;
	}

	[Button("Spawn")]
	private void SpawnObject()
	{
		if (!_SpawnablePrefab)
		{
			#if UNITY_EDITOR
			if (!EditorApplication.isPlaying)
				Debug.LogWarning($"{name} is missing a spawnable prefab!");
			#endif
			return;
		}

		Spawn(_SpawnablePrefab.transform, true);
	}
}

public enum SpawnerTag
{
	Player = 0,
	Player2 = 1,
	Player3 = 2,
	Player4 = 3,
	Npc = 4,
	Enemy = 5
}
}