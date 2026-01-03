using PixelCiv.Scriptable_Objects;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PixelCiv.Components
{
[RequireComponent(typeof(Grid)), HideMonoScript,]
public class HexGrid : MonoBehaviour
{
	[ShowInInspector, ReadOnly,]
	public Grid Grid { get; private set; }

	[SerializeField, OnValueChanged(nameof(UpdateSettings)),]
	private GridSettings _Settings;


	private void Awake()
	{
		Grid = GetComponent<Grid>();
	}

	#if UNITY_EDITOR
	private void Reset()
	{
		Grid = GetComponent<Grid>();
	}

	private void UpdateSettings()
	{
		if (!_Settings || !Grid) return;

		// Update Grid Settings.
		Grid.cellSize = _Settings.CellSize;
		Grid.cellGap = _Settings.CellGap;
		Grid.cellLayout = _Settings.CellLayout;
		Grid.cellSwizzle = _Settings.CellSwizzle;
	}
	#endif
}
}
