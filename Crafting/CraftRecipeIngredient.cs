using Godot;

namespace SadChromaLib.Specialisations.Inventory.Crafting;

/// <summary>
/// A resource definining an ingredient in a crafting recipe.
/// </summary>
[GlobalClass]
public sealed partial class CraftRecipeIngredient: Resource
{
	[Export]
	public StringName ItemId;

	[Export]
	public int Count = 1;
}