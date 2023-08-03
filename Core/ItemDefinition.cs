using Godot;

namespace SadChromaLib.Specialisations.Inventory;

/// <summary>
/// A resource describing an item
/// </summary>
[GlobalClass]
public sealed partial class ItemDefinition: Resource
{
	[Export]
	public StringName ItemId;

	[Export]
	public string DisplayName;

	[Export(PropertyHint.File)]
	public string IconPath;

	[Export]
	public int MaxAmount = 99;

	[Export]
	public int BasePrice = 1;

	[Export]
	public bool IsUseable;

	[Export]
	public StringName UseableItemTag;

	[Export(PropertyHint.MultilineText)]
	public string ItemDescription;

	public Texture2D GetIcon()
	{
		return ResourceLoader.Load<Texture2D>(IconPath);
	}
}