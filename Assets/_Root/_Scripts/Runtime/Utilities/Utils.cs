using UnityEngine;
using UnityEngine.InputSystem;

namespace PixelCiv.Utilities
{
public class Utils
{
	public static HexCoords GetMouseHexCoords(Camera camera, Grid grid)
	{
		Mouse mouse = Mouse.current;
		Vector3 mouseWorldPos = camera.ScreenToWorldPoint(mouse.position.ReadValue());
		Vector3Int mouseCellPos = grid.WorldToCell(mouseWorldPos);
		return new HexCoords(HexCoords.OffsetToAxial(mouseCellPos));
	}

	public static HexCoords GetMouseHexCoords(Grid grid)
	{
		return GetMouseHexCoords(Camera.main, grid);
	}
}
}
