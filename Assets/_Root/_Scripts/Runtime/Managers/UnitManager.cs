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


	/// <summary>
	///     Create a new Unit object.
	/// </summary>
	/// <param name="position">The Unit's spawning position in offset coordinates.</param>
	/// <param name="colour">Optional colour to set the sprite tint to.</param>
	/// <returns></returns>
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

	/// <summary>
	///     Move a desired Unit towards a specified cell.
	/// </summary>
	/// <param name="id">The GUID of the Unit to be moved.</param>
	/// <param name="targetCell">The target cell's position in offset coordinates.</param>
	public void Move(Guid id, Vector3Int targetCell)
	{
		Unit unit = _Units.Find(u => u.ID == id);
		if (!unit) return;

		// Validate target tile.
		Hex targetHex = GameManager.Instance.HexMap.Find(targetCell);
		if (targetHex == null || targetHex.UnitID != Guid.Empty) return;

		// Clear current hex occupancy.
		Hex currentHex = GameManager.Instance.HexMap.Find(unit.CellPosition);
		if (currentHex != null)
			currentHex.UnitID = Guid.Empty;

		// Compute path.
		List<Vector3Int> path = FindPath(Hex.OffsetToAxial(unit.CellPosition),
										 Hex.OffsetToAxial(targetCell));
		unit.SetPath(path);
		unit.OnMovementCompleted += CompleteMoveOrder;
	}

	public void Move(Guid id, Vector2Int targetCell)
	{
		Move(id, Hex.AxialToOffset(targetCell));
	}

	private void CompleteMoveOrder(Guid id)
	{
		Unit unit = _Units.Find(u => u.ID == id);
		if (!unit) return;

		Hex hex = GameManager.Instance.HexMap.Find(unit.CellPosition);
		if (hex != null)
			hex.UnitID = unit.ID;

		_MoveOrders.Remove(id);
	}

	/// <summary>
	///     A* pathing calculation to a desired position.
	/// </summary>
	/// <param name="start">The starting cell position, in axial coordinates.</param>
	/// <param name="goal">The destination cell position, in axial coordinates.</param>
	/// <returns></returns>
	private List<Vector3Int> FindPath(Vector2Int start, Vector2Int goal)
	{
		var frontier = new List<Vector2Int> { start, };
		var traveledPath = new Dictionary<Vector2Int, Vector2Int>();
		var movementCost = new Dictionary<Vector2Int, int> { [start] = 0, };
		var totalCostToGoal = new Dictionary<Vector2Int, int>
				{ [start] = movementCost[start] + Hex.Distance(start, goal), };

		// Loop until the no more tiles are left to check.
		while (frontier.Count > 0)
		{
			// Get the current tile based on the shorest distance to goal.
			Vector2Int currentTile = frontier
									 .OrderBy(n => totalCostToGoal.GetValueOrDefault(
													  n, int.MaxValue)).First();

			frontier.Remove(currentTile);
			if (currentTile == goal)
				return ReconstructPath(traveledPath, currentTile);

			// Check each neighbour of the current tile.
			foreach (Vector2Int neighbour in Hex.GetNeighbours(currentTile))
			{
				// Ignore invalid tiles.
				Hex neighbourTile = GameManager.Instance.HexMap.Find(neighbour);
				if (neighbourTile == null || neighbourTile.UnitID != Guid.Empty ||
					!neighbourTile.Visuals)
					continue;

				// Calculate the assumed movement cost from the current tile to it's neighbour.
				// Skip neighbour if we already know its movement cost.
				const int TILE_MOVE_COST = 1;
				int assumedMoveCost = movementCost[currentTile] + TILE_MOVE_COST;
				if (movementCost.ContainsKey(neighbour) &&
					assumedMoveCost >= movementCost[neighbour])
					continue;

				// Add to path.
				traveledPath[neighbour] = currentTile;
				movementCost[neighbour] = assumedMoveCost;
				totalCostToGoal[neighbour] =
						assumedMoveCost + Hex.Distance(neighbour, goal);
				if (!frontier.Contains(neighbour))
					frontier.Add(neighbour);
			}
		}

		// In case no valid path was found.
		Debug.LogWarning("No valid path was found!");
		return new List<Vector3Int>();
	}

	private List<Vector3Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom,
			Vector2Int current)
	{
		var path = new List<Vector2Int>();
		// Continue until the current tile is equal to itself.
		while (true)
		{
			path.Add(current);
			if (!cameFrom.TryGetValue(current, out current))
				break;
		}

		path.Reverse();
		return path.Select(Hex.AxialToOffset).ToList();
	}
}
}
