using Godot;

using SadChromaLib.Utils.Random;

namespace SadChromaLib.Specialisations.Inventory.Loot;

[GlobalClass]
public sealed partial class LootEntry : Resource, IWeightedObject<string>
{
	[Export]
	public string ItemId;

	[Export(PropertyHint.Range, "0.0,200.0")]
	public float DropChance = 50f;

	private float? _bagWeight;

	#region Weighted Object

	public string GetValue()
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