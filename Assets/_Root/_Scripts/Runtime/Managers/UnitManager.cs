using System;
using System.Collections.Generic;
using System.Linq;
using PixelCiv.Utilities;
using UnityEngine;

namespace PixelCiv.Managers
{
public class UnitManager : Singleton<UnitManager>
{
	public event Action<Guid> OnMovementCompleted;

	[SerializeField]
	private GameObject _UnitPrefab;
	private readonly Dictionary<Guid, Vector3Int> _MoveOrders = new();

	private readonly List<Unit> _Units = new();


	private void OnEnable()
	{
		OnMovementCompleted += CompleteMoveOrder;
	}

	private void OnDisable()
	{
		OnMovementCompleted -= CompleteMoveOrder;
	}

	public Unit CreateUnit(Vector3Int position, Color? colour = null)
	{
		if (_Units.Any(unit => unit.CellPosition == position))
		{
			Debug.Log($"A unit already exists on the specified tile{position}.");
			return null;
		}

		var unit = Instantiate(_UnitPrefab, transform).GetComponent<Unit>();
		unit.Initialize(Guid.NewGuid(), colour ?? Color.white, position);
		_Units.Add(unit);
		return unit;
	}

	public Unit CreateUnit(Vector2Int axial, Color? colour = null)
	{
		return CreateUnit(Hex.AxialToOffset(axial), colour);
	}

	private void CompleteMoveOrder(Guid id)
	{
		_MoveOrders.Remove(id);

		Unit unit = _Units.Find(u => u.ID == id);
		if (!unit) return;

		Hex hex = GameManager.Instance.HexMap.Find(unit.CellPosition);
		if (hex != null)
			hex.Unit = unit.ID;
	}
}
}
