using UnityEngine;

namespace PixelCiv.Scriptable_Objects
{
[CreateAssetMenu(fileName = "GridSettings", menuName = "Game/GridSettings", order = 0)]
public class GridSettings : ScriptableObject
{
	public Vector3 CellSize = Vector3.one;
	public Vector3 CellGap = Vector3.zero;
	public GridLayout.CellLayout CellLayout = GridLayout.CellLayout.Rectangle;
	public GridLayout.CellSwizzle CellSwizzle = GridLayout.CellSwizzle.XYZ;
}
}
