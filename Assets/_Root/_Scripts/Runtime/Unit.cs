using System;
using PixelCiv.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PixelCiv
{
public class Unit : MonoBehaviour
{
	[ShowInInspector, ReadOnly,]
	public Color Colour { get; private set; }
	[ShowInInspector, ReadOnly,]
	public Guid ID { get; private set; }
	[ShowInInspector, ReadOnly,]
	public Vector3Int CellPosition { get; private set; }
	[ShowInInspector, ReadOnly,]
	public Vector3Int DestinationCell { get; private set; }

	private SpriteRenderer _Renderer;


	private void Awake()
	{
		_Renderer = GetComponent<SpriteRenderer>();
	}

	public void Initialize(Guid id, Color colour, Vector3Int cellPosition)
	{
		ID = id;
		Colour = colour;
		CellPosition = cellPosition;

		transform.position = GameManager.Instance.Grid.GetCellCenterWorld(CellPosition);
		SetColour();
	}

	public void SetTarget(Vector3Int targetCellPosition)
	{
		DestinationCell = targetCellPosition;
	}

	private void SetColour()
	{
		if (_Renderer)
			_Renderer.color = Colour;
	}
}
}
