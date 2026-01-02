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

	private readonly List<Unit> _Units = new();


	/// <summary>
	///     Create a new Unit object.
	/// </summary>
	/// <param name="position">The Unit's spawning hex position.</param>
	/// <param name="colour">Optional colour to set the sprite tint to.</param>
	/// <returns>The created Unit object.</returns>
	public Unit CreateUnit(HexCoords position, Color? colour = null)
	{
		if (_Units.Any(unit => unit.Position == position))
		{
			Debug.Log($"A unit already exists on the specified tile{position.Offset}.");
			return null;
		}

		var unit = Instantiate(_UnitPrefab, transform).GetComponent<Unit>();
		unit.Initialize(Guid.NewGuid(), colour ?? Color.white, position);
		unit.OnMovementCompleted += CompleteMoveOrder;
		_Units.Add(unit);
		return unit;
	}

	/// <summary>
	///     Move a desired Unit towards a specified hex cell.
	/// </summary>
	/// <param name="id">The GUID of the Unit to be moved.</param>
	/// <param name="targetCoords">The target's hex position.</param>
	public void Move(Guid id, HexCoords targetCoords)
	{
		Unit unit = _Units.Find(u => u.ID == id);
		if (!unit) return;

		// Validate target tile.
		Hex targetHex = GameManager.Instance.HexMap.Find(targetCoords);
		if (targetHex == null || targetHex.UnitID != Guid.Empty) return;

		// Clear current hex occupancy.
		Hex currentHex = GameManager.Instance.HexMap.Find(unit.Position);
		if (currentHex != null)
			currentHex.UnitID = Guid.Empty;

		// Compute path.
		List<HexCoords> path = FindPath(unit.Position, targetCoords);
		unit.SetPath(path);
		unit.OnMovementCompleted += CompleteMoveOrder;
	}

	private void CompleteMoveOrder(Guid id)
	{
		Unit unit = _Units.Find(u => u.ID == id);
		if (!unit) return;

		Hex hex = GameManager.Instance.HexMap.Find(unit.Position);
		if (hex != null)
			hex.UnitID = unit.ID;
	}

	/// <summary>
	///     A* pathing calculation to a desired position.
	/// </summary>
	/// <param name="start">The starting hex position.</param>
	/// <param name="goal">The destination hex position.</param>
	/// <returns>The path containing hex positions.</returns>
	private List<HexCoords> FindPath(HexCoords start, HexCoords goal)
	{
		var frontier = new List<HexCoords> { start, };
		var traveledPath = new Dictionary<HexCoords, HexCoords>();
		var movementCost = new Dictionary<HexCoords, int> { [start] = 0, };
		var totalCostToGoal = new Dictionary<HexCoords, int>
				{ [start] = movementCost[start] + start.DistanceTo(goal), };

		// Loop until the no more tiles are left to check.
		while (frontier.Count > 0)
		{
			// Get the current tile based on the shorest distance to goal.
			HexCoords currentTile = frontier
									.OrderBy(n => totalCostToGoal.GetValueOrDefault(
													 n, int.MaxValue))
									.First();

			frontier.Remove(currentTile);
			if (currentTile == goal)
				return ReconstructPath(traveledPath, currentTile);

			// Check each neighbour of the current tile.
			foreach (HexCoords neighbour in Hex.GetNeighbours(currentTile))
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
				totalCostToGoal[neighbour] = assumedMoveCost + neighbour.DistanceTo(goal);
				if (!frontier.Contains(neighbour))
					frontier.Add(neighbour);
			}
		}

		// In case no valid path was found.
		Debug.LogWarning("No valid path was found!");
		return new List<HexCoords>();
	}

	private List<HexCoords> ReconstructPath(Dictionary<HexCoords, HexCoords> cameFrom,
			HexCoords current)
	{
		var path = new List<HexCoords>();
		// Continue until the current tile is equal to itself.
		while (true)
		{
			path.Add(current);
			if (!cameFrom.TryGetValue(current, out current))
				break;
		}

		path.Reverse();
		return path;
	}
}
}
