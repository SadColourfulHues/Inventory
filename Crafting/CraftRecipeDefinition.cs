using Godot;
using System;

namespace SadChromaLib.Specialisations.Inventory.Crafting;

/// <summary>
/// A resource representing a crafting recipe.
/// </summary>
[GlobalClass]
public sealed partial class CraftRecipeDefinition : Resource
{
	[Export]
	public string CraftId;

	[Export]
	public string ResultingItemId;

	[Export]
	public CraftRecipeIngredient[] Ingredients;

	[Export]
	public int CraftAmount = 1;

	[Export]
	public bool AvailableImmediately = true;

	/// <summary>
	/// Returns true if an item bag has all the ingredients needed to craft this recipe
	/// </summary>
	/// <param name="bag">The item bag to check.</param>
	/// <param name="craftAmount">The amount of items to craft.</param>
	/// <returns></returns>
	public bool HasIngredients(ItemBag bag, int craftAmount = 1)
	{
		ReadOnlySpan<CraftRecipeIngredient> ingredients = Ingredients;

		for (int i = 0; i < ingredients.Length; ++ i) {
			int requiredAmount = ingredients[i].Count * craftAmount;

			if (bag.HasItem(ingredients[i].ItemId, requiredAmount))
				continue;

			return false;
		}

		return true;
	}

	/// <summary>
	/// Performs the necessary transactions to finalise the crafting process
	/// </summary>
	/// <param name="bag">The item bag to use.</param>
	/// <param name="craftAmount">The amount of items to craft.</param>
	public void Craft(ItemBag bag, int craftAmount = 1)
	{
		ReadOnlySpan<CraftRecipeIngredient> ingredients = Ingredients;

		for (int i = 0; i < ingredients.Length; ++ i) {
			int requiredAmount = ingredients[i].Count * craftAmount;
			bag.TakeItem(ingredients[i].ItemId, requiredAmount);
		}

		bag.GiveItem(ResultingItemId, craftAmount * CraftAmount);
	}
}
