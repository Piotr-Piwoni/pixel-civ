using Sirenix.OdinInspector;
using UnityEngine;

namespace PixelCiv.Scriptable_Objects
{
[CreateAssetMenu(fileName = "UnitStats", menuName = "Game/UnitStats", order = 0),
 HideMonoScript,]
public class UnitStats : ScriptableObject
{
	public float MoveSpeed => _MoveSpeed;
	public float TurnSpeed => _TurnSpeed;
	public int AttackPower => _AttackPower;
	public int Defence => _Defence;
	public int Range => _Range;
	public UnitType Type => _Type;

	[SerializeField]
	private UnitType _Type = UnitType.Footman;
	[SerializeField]
	private float _MoveSpeed = 5f;
	[SerializeField]
	private float _TurnSpeed = 15f;
	[SerializeField]
	private int _AttackPower = 1;
	[SerializeField]
	private int _Defence = 1;
	[SerializeField, Unit(Units.Meter, "Hex"),]
	private int _Range = 1;
}
}
