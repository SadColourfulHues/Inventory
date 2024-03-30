using Godot;

namespace SadChromaLib.Specialisations.Inventory.Crafting;

/// <summary>
/// A resource definining an ingredient in a crafting recipe.
/// </summary>
[GlobalClass]
public sealed partial class CraftRecipeIngredient: Resource
{
	[Export]
	public string ItemId;

	[Export]
	public int Count = 1;
}