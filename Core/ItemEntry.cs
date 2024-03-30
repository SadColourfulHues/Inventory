namespace SadChromaLib.Specialisations.Inventory;

/// <summary>
/// An object representing an item slot in a bag.
/// </summary>
public sealed class ItemEntry
{
	public string ItemId;
	public int Count;

	public bool IsItemType(ItemDefinition itemDef)
	{
		return itemDef.ItemId == ItemId;
	}
}