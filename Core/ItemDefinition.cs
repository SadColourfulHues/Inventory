using Godot;

namespace SadChromaLib.Specialisations.Inventory;

/// <summary>
/// A resource describing an item
/// </summary>
[GlobalClass]
#if TOOLS
[Tool]
#endif
public sealed partial class ItemDefinition: Resource
{
	[Export]
	public string ItemId;

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
	public string UseableItemTag;

	[Export(PropertyHint.MultilineText)]
	public string ItemDescription;

	public Texture2D GetIcon()
	{
		return ResourceLoader.Load<Texture2D>(IconPath);
	}

	#if TOOLS
	public override void _ValidateProperty(Godot.Collections.Dictionary _)
	{
		if (!Engine.IsEditorHint())
			return;

		ResourceName = $"{DisplayName ?? "Item Name"} ({ItemId ?? "<invalid id>"})";
		EmitChanged();
	}
	#endif
}