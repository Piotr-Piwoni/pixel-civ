using PixelCiv.Systems;
using PixelCiv.Utilities;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelCiv.Managers
{
public class GameManager : PersistentSingleton<GameManager>
{
	[TabGroup("", "Info", SdfIconType.QuestionSquareFill,
				 TextColor = "lightblue"), ShowInInspector, ReadOnly,]
	public Camera Camera { get; private set; }
	[TabGroup("", "Info"), ShowInInspector, ReadOnly,]
	public CinemachineCamera CinemachineCam { get; private set; }
	[TabGroup("", "Info"), ShowInInspector, ReadOnly,]
	public GameObject Player { get; private set; }
	[TabGroup("", "Info"), ShowInInspector, ReadOnly,
	 PropertyOrder(-1f),]
	public GameState CurrentState { get; private set; } = GameState.Playing;

	[SerializeField,
	 TabGroup("", "Settings", SdfIconType.GearFill, TextColor = "yellow"),]
	private GameObject _PlayerPrefab;
	[SerializeField, TabGroup("", "Settings"),]
	private bool _SpawnPlayer;
	[SerializeField, TabGroup("", "Settings"),]
	private AudioClip _MusicClip;

	private bool _DelayPlayerInit;
	private GameObject _ActorGroup;
	private GameState _PreviousState;
	private Spawner _PlayerSpawner;


	protected override void Awake()
	{
		base.Awake();
		_PreviousState = CurrentState;

		// If the actor group was not found create it.
		_ActorGroup = GameObject.FindGameObjectWithTag("ActorGroup");
		if (!_ActorGroup)
			_ActorGroup = new GameObject("Actor Group")
			{
				tag = "ActorGroup",
			};

		// Get the player if it already exists, otherwise create one if possible.
		Player = GameObject.FindGameObjectWithTag("Player");
		if (!Player && _PlayerPrefab && _SpawnPlayer)
			_DelayPlayerInit = true;
		// If there is already a player, ensure it's in the correct group.
		else
		{
			Player.transform.SetParent(_ActorGroup.transform);
			HandlePlayerInit();
		}
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
		if (_DelayPlayerInit && InputManager.Instance)
			HandlePlayerInit();

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

	/// Handle Player initialization.
	private void HandlePlayerInit()
	{
		// Get the player if it already exists, otherwise create one if possible.
		if (!Player)
		{
			Player = Instantiate(_PlayerPrefab, _ActorGroup.transform);
			_DelayPlayerInit = false;
		}

		// Try to obtain the player spawner.
		Spawner[] spawners = FindObjectsByType<Spawner>(FindObjectsSortMode.None);
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
			Debug.Log("<color=yellow>Player spawner was not found in the scene. Using default position.</color>");
	}


	public enum GameState
	{
		MainMenu = 0,
		Playing = 1,
		Talking = 2,
		Pause = 3,
		Menu = 4,
	}
}
}
