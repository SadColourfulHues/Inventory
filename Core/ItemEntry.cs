using Godot;

namespace SadChromaLib.Specialisations.Inventory;

/// <summary>
/// An object representing an item slot in a bag.
/// </summary>
[GlobalClass]
public sealed partial class ItemEntry : RefCounted
{
	public StringName ItemId;
	public int Count;

	public bool IsItemType(ItemDefinition itemDef)
	{
		return itemDef.ItemId == ItemId;
	}
}