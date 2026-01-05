using System;
using System.Collections.Generic;
using System.Linq;
using PixelCiv.Components;
using PixelCiv.Systems;
using PixelCiv.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelCiv.Managers
{
[HideMonoScript,]
public class GameManager : PersistentSingleton<GameManager>
{
	[ShowInInspector, ReadOnly,]
	public Camera Camera { get; private set; }
	[ShowInInspector, ReadOnly,]
	public GameObject Player { get; private set; }
	[ShowInInspector, ReadOnly,]
	public GameState CurrentState { get; private set; } = GameState.Playing;
	public Grid Grid => _Grid;
	[ShowInInspector, ReadOnly,]
	public List<Civilization> Civilizations { get; private set; } = new();
	public Transform ActorsGroup { get; private set; }
	public Transform ManagersGroup { get; private set; }

	public HexMap HexMap = new();

	[SerializeField]
	private GameObject _PlayerPrefab;
	[SerializeField]
	private AudioClip _MusicClip;
	[SerializeField]
	private GameObject _UnitManagerPrefab;
	[SerializeField]
	private Grid _Grid;
	[SerializeField, Min(1),]
	private int _NumberOfPlayers = 1;

	private bool _DelayPlayerInit;
	private GameState _PreviousState;
	private Spawner _PlayerSpawner;


	protected override void Awake()
	{
		base.Awake();
		_PreviousState = CurrentState;

		// Reserve space.
		Civilizations.Capacity = 10;

		// Init players.
		for (var i = 0; i < _NumberOfPlayers; i++)
		{
			// Always make sure that at least one civilization is the players.
			Civilization civ;
			civ = i == 0 ? new Civilization(isPlayer: true) : new Civilization();
			Civilizations.Add(civ);
		}

		GetGroups();
	}

	private void Start()
	{
		// Create the Unit Manager.
		Instantiate(_UnitManagerPrefab, ManagersGroup);

		TryGetPlayer();

		// If a music clip is provided, play the music.
		if (_MusicClip)
			AudioSystem.Instance.PlayMusic(_MusicClip);
	}

	private void Update()
	{
		if (_DelayPlayerInit && InputManager.Instance)
			HandlePlayerInit();

		if (!Player) return;
		// Handle game functionality differently based on current state.
		switch (CurrentState)
		{
		case GameState.MainMenu:
			// Logic for when the game is in the Main Menu.
			break;
		case GameState.Playing:
		{
			break;
		}
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

	public Civilization GetPlayerCivilization()
	{
		return Civilizations.Find(n => n.IsPlayer);
	}

	public static Civilization GetUnitCivilization(Guid id)
	{
		return Instance.Civilizations.FirstOrDefault(civ => civ.Units.Contains(id));
	}

	public override void OnSceneChange(Scene scene, LoadSceneMode mode) { }

	private void GetGroups()
	{
		// If the Actors group was not found create it.
		ActorsGroup = GameObject.FindGameObjectWithTag("ActorGroup")?.transform;
		if (!ActorsGroup)
			ActorsGroup = new GameObject("Actors") { tag = "ActorGroup", }.transform;

		// If the Managers group was not found create it.
		ManagersGroup = GameObject.FindGameObjectWithTag("ManagersGroup")?.transform;
		if (!ManagersGroup)
			ManagersGroup = new GameObject("Managers & Systems")
					{ tag = "ManagersGroup", }.transform;
	}

	private void HandlePlayerInit()
	{
		// Attempt to create a player if possible.
		if (!Player)
		{
			Player = Instantiate(_PlayerPrefab, ActorsGroup.transform);
			_DelayPlayerInit = false;
		}

		// Try to obtain the player spawner.
		Spawner[] spawners = FindObjectsByType<Spawner>(FindObjectsSortMode.None);
		foreach (Spawner spawner in spawners)
		{
			if (spawner.SpawnerTag != SpawnerTag.Player) continue;
			_PlayerSpawner = spawner;
			break;
		}

		// If spawner found, spawn the player there.
		if (_PlayerSpawner)
			_PlayerSpawner.Spawn(Player.transform);
		else
			Debug.Log("<color=yellow>Player spawner was not found in the scene. " +
					  "Using default position.</color>");
	}

	private void TryGetPlayer()
	{
		// Try to get existing player.
		Player = GameObject.FindGameObjectWithTag("Player");
		if (Player)
		{
			Player.transform.SetParent(ActorsGroup.transform);
			HandlePlayerInit();
			return;
		}

		// If a player doesn't already exist, create one.
		// Defer creating the player based on if the Input Manager is initialized.
		if (!_PlayerPrefab) return;
		if (!InputManager.Instance)
			_DelayPlayerInit = true;
		else
			HandlePlayerInit();
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
