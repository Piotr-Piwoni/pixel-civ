using System;
using System.Collections.Generic;
using System.Linq;
using PixelCiv.Managers;
using PixelCiv.Utilities;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace PixelCiv
{
public class Unit : MonoBehaviour
{
	public event Action<Guid> OnMovementCompleted;

	private const float MOVE_SPEED = 5f;

	[ShowInInspector, ReadOnly,]
	public Color Colour { get; private set; }
	[ShowInInspector]
	public Guid ID { get; private set; }
	[ShowInInspector, ReadOnly,]
	public HexCoords NextHexMove { get; private set; }
	[ShowInInspector, ReadOnly,]
	public HexCoords Position { get; private set; }

	private bool _IsMoving;

	// Debugging fields.
	#if UNITY_EDITOR
	[ShowInInspector, ReadOnly,]
	private HexCoords _Destination;
	#endif

	private Queue<HexCoords> _Path = new();
	private SpriteRenderer _Renderer;


	private void Awake()
	{
		_Renderer = GetComponent<SpriteRenderer>();
	}

	private void Update()
	{
		if (!_IsMoving || _Path.Count == 0) return;

		NextHexMove = _Path.Peek();
		Vector3 worldPos = GameManager.Instance.Grid.CellToWorld(NextHexMove.Offset);
		// Move towards target.
		transform.position = Vector3.MoveTowards(transform.position, worldPos,
												 MOVE_SPEED * Time.deltaTime);

		// If within margin, set the cell position.
		if (!(Vector3.Distance(transform.position, worldPos) < 0.01f)) return;
		Position = _Path.Dequeue();

		// Reset fields related to movement and invoke the movement completed event.
		if (_Path.Count != 0) return;
		_IsMoving = false;
		NextHexMove = HexCoords.Zero;
		OnMovementCompleted?.Invoke(ID);

		#if UNITY_EDITOR
		_Destination = HexCoords.Zero;
		#endif
	}

	public void Initialize(Guid id, Color colour, HexCoords position)
	{
		ID = id;
		Colour = colour;
		Position = position;

		transform.position =
				GameManager.Instance.Grid.GetCellCenterWorld(Position.Offset);
		SetColour();
	}

	public void SetPath(List<HexCoords> path)
	{
		if (path.IsNullOrEmpty()) return;

		_Path = new Queue<HexCoords>(path);
		_IsMoving = true;

		#if UNITY_EDITOR
		_Destination = _Path.Last();
		#endif
	}

	private void SetColour()
	{
		if (_Renderer)
			_Renderer.color = Colour;
	}
}
}
