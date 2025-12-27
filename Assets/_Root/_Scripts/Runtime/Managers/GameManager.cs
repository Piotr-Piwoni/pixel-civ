using PROJECTNAME.Systems;
using PROJECTNAME.Utilities;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PROJECTNAME.Managers
{
public class GameManager : PersistentSingleton<GameManager>
{
	[TabGroup("", "Info", SdfIconType.QuestionSquareFill,
		 TextColor = "lightblue"), ShowInInspector, ReadOnly]
	public Camera Camera { get; private set; }
	[TabGroup("", "Info"), ShowInInspector, ReadOnly]
	public CinemachineCamera CinemachineCam { get; private set; }
	[TabGroup("", "Info"), ShowInInspector, ReadOnly]
	public GameObject Player { get; private set; }
	[TabGroup("", "Info"), ShowInInspector, ReadOnly,
	 PropertyOrder(-1f)]
	public GameState CurrentState { get; private set; } = GameState.Playing;

	[SerializeField,
	 TabGroup("", "Settings", SdfIconType.GearFill, TextColor = "yellow")]
	private GameObject _PlayerPrefab;
	[SerializeField, TabGroup("", "Settings")]
	private AudioClip _MusicClip;

	private GameState _PreviousState;
	private Spawner _PlayerSpawner;


	protected override void Awake()
	{
		base.Awake();
		_PreviousState = CurrentState;

		HandlePlayerInit();
		GetCamera();
	}

	private void Start()
	{
		// Hide the cursor on game start.
		Cursor.lockState = CursorLockMode.Locked;

		// If a music clip is provided, play the music.
		if (_MusicClip)
			AudioSystem.Instance.PlayMusic(_MusicClip);
	}

	private void Update()
	{
		// Handle game functionality differently based on current state.
		switch (CurrentState)
		{
			case GameState.MainMenu:
				// Logic for when the game is in the Main Menu.
				break;
			case GameState.Playing:
				// Logic for when the game is actually playing.
				break;
			case GameState.Talking:
				// Logic for when talking occurs in the game.
				break;
			case GameState.Pause:
				// Logic for when the game is paused.
				break;
			case GameState.Menu:
				// Logic for when the game is in a UI menu.
				break;
		}
	}

	public void ChangeState(GameState newState)
	{
		_PreviousState = CurrentState;
		CurrentState = newState;
	}

	public override void OnSceneChange(Scene scene, LoadSceneMode mode)
	{
	}

	/// Try to obtain the camera in the scene.
	private void GetCamera()
	{
		// Locate the main camera.
		var cameraObjs = FindObjectsByType<Camera>(
			FindObjectsInactive.Include,
			FindObjectsSortMode.None);

		foreach (Camera camObj in cameraObjs)
			if (camObj.CompareTag("MainCamera"))
				Camera = camObj;

		if (!Camera)
		{
			Debug.LogError("No Camera found in the scene!");
			return;
		}

		// Try to get the CinemachineCamera component from the camera's parent.
		CinemachineCam = Camera.GetComponentInParent<CinemachineCamera>(true);
		if (!CinemachineCam)
		{
			Debug.LogWarning("A Cinemachine Camera was not found in the " +
			                 "scene or is not the parent object of the Camera.");
		}

		// If there's a player, move the cameras to the player, otherwise move
		// them to the Game Manager.
		if (CinemachineCam)
		{
			if (CinemachineCam.transform.parent == Player?.transform)
				return;
			CinemachineCam.transform.SetParent(Player
				? Player.transform
				: transform);
			return;
		}

		if (!Camera || Camera.transform.parent == Player?.transform)
			return;
		Camera.transform.SetParent(Player ? Player.transform : transform);
	}


	/// Handle Player initialization.
	private void HandlePlayerInit()
	{
		// Get the player if it already exists, otherwise create one if possible.
		Player = GameObject.FindGameObjectWithTag("Player");
		if (!Player && _PlayerPrefab)
			Player = Instantiate(_PlayerPrefab);

		// Try to obtain the player spawner.
		var spawners = FindObjectsByType<Spawner>(
			FindObjectsSortMode.None);
		foreach (Spawner spawner in spawners)
		{
			if (spawner.SpawnerTag != SpawnerTag.Player)
				continue;
			_PlayerSpawner = spawner;
			break;
		}

		// If spawner found, spawn the player there.
		if (_PlayerSpawner)
			_PlayerSpawner.Spawn(Player.transform, true);
		else
		{
			Debug.Log("<color=yellow>Player spawner was not found " +
			          "in the scene.</color>");
		}
	}

	public enum GameState
	{
		MainMenu = 0,
		Playing = 1,
		Talking = 2,
		Pause = 3,
		Menu = 4
	}
}
}