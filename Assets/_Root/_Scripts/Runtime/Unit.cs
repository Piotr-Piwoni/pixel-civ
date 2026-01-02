using System;
using System.Collections.Generic;
using PixelCiv.Managers;
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
	[ShowInInspector, ReadOnly,]
	public Guid ID { get; private set; }
	[ShowInInspector, ReadOnly,]
	public Vector3Int CellPosition { get; private set; }
	[ShowInInspector, ReadOnly,]
	public Vector3Int DestinationCell { get; private set; }

	private bool _IsMoving;
	private Queue<Vector3Int> _Path = new();
	private SpriteRenderer _Renderer;


	private void Awake()
	{
		_Renderer = GetComponent<SpriteRenderer>();
	}

	private void Update()
	{
		if (!_IsMoving || _Path.Count == 0) return;

		DestinationCell = _Path.Peek();
		Vector3 worldPos = GameManager.Instance.Grid.CellToWorld(DestinationCell);
		// Move towards target.
		transform.position = Vector3.MoveTowards(transform.position, worldPos,
												 MOVE_SPEED * Time.deltaTime);

		// If within margin, set the cell position.
		if (!(Vector3.Distance(transform.position, worldPos) < 0.01f)) return;
		CellPosition = _Path.Dequeue();

		if (_Path.Count != 0) return;
		_IsMoving = false;
		OnMovementCompleted?.Invoke(ID);
	}

	public void Initialize(Guid id, Color colour, Vector3Int cellPosition)
	{
		ID = id;
		Colour = colour;
		CellPosition = cellPosition;

		transform.position = GameManager.Instance.Grid.GetCellCenterWorld(CellPosition);
		SetColour();
	}

	public void SetPath(List<Vector3Int> path)
	{
		if (path.IsNullOrEmpty()) return;

		_Path = new Queue<Vector3Int>(path);
		_IsMoving = true;
	}

	private void SetColour()
	{
		if (_Renderer)
			_Renderer.color = Colour;
	}
}
}
