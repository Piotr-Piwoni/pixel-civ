using System;
using PixelCiv.Managers;
using Sirenix.OdinInspector;
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
	private SpriteRenderer _Renderer;


	private void Awake()
	{
		_Renderer = GetComponent<SpriteRenderer>();
	}

	private void Update()
	{
		// TODO: Create proper tile based movement.
		if (!_IsMoving) return;

		Vector3 targetWorld = GameManager.Instance.Grid.CellToWorld(DestinationCell);

		// Move towards target.
		transform.position = Vector3.MoveTowards(transform.position, targetWorld,
												 MOVE_SPEED * Time.deltaTime);

		if (!(Vector3.Distance(transform.position, targetWorld) < 0.01f)) return;
		_IsMoving = false;
		CellPosition = DestinationCell;
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

	public void Move(Vector3Int targetCell)
	{
		DestinationCell = targetCell;
		_IsMoving = true;
	}

	private void SetColour()
	{
		if (_Renderer)
			_Renderer.color = Colour;
	}
}
}
