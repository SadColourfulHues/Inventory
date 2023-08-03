using Godot;

using SadChromaLib.Utils.Random;

namespace SadChromaLib.Specialisations.Inventory.Loot;

[GlobalClass]
public sealed partial class LootEntry : Resource, IWeightedObject<StringName>
{
	[Export]
	public StringName ItemId;

	[Export(PropertyHint.Range, "0.0,200.0")]
	public float DropChance = 50f;

	private float? _bagWeight;

	#region Weighted Object

	public StringName GetValue()
	{
		return ItemId;
	}

	public float GetWeight()
	{
		return _bagWeight ?? DropChance;
	}

	public void UpdateWeight(float newWeight)
	{
		_bagWeight = newWeight;
	}

	#endregion
}