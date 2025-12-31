using System;
using System.Collections.Generic;
using System.Linq;
using PixelCiv.Utilities;
using UnityEngine;

namespace PixelCiv.Managers
{
public class UnitManager : Singleton<UnitManager>
{
	[SerializeField]
	private GameObject _UnitPrefab;

	private readonly Dictionary<Guid, Vector3Int> _MoveOrders = new();
	private readonly List<Unit> _Units = new();


	public Unit CreateUnit(Vector3Int position, Color? colour = null)
	{
		if (_Units.Any(unit => unit.CellPosition == position))
		{
			Debug.Log($"A unit already exists on the specified tile{position}.");
			return null;
		}

		var unit = Instantiate(_UnitPrefab, transform).GetComponent<Unit>();
		unit.Initialize(Guid.NewGuid(), colour ?? Color.white, position);
		unit.OnMovementCompleted += CompleteMoveOrder;
		_Units.Add(unit);
		return unit;
	}

	public Unit CreateUnit(Vector2Int axial, Color? colour = null)
	{
		return CreateUnit(Hex.AxialToOffset(axial), colour);
	}

	public void Move(Guid id, Vector3Int targetCell)
	{
		_MoveOrders.Add(id, targetCell);

		Unit unit = _Units.Find(u => u.ID == id);
		Hex hex = GameManager.Instance.HexMap.Find(unit.CellPosition);
		if (hex != null)
			hex.UnitID = Guid.Empty;

		unit.Move(targetCell);
	}

	private void CompleteMoveOrder(Guid id)
	{
		_MoveOrders.Remove(id);

		Unit unit = _Units.Find(u => u.ID == id);
		Hex hex = GameManager.Instance.HexMap.Find(unit.CellPosition);
		if (hex != null)
			hex.UnitID = unit.ID;
	}
}
}
