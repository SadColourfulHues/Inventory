using Godot;

using System;
using System.Collections.Generic;

namespace SadChromaLib.Specialisations.Inventory.Crafting;

[GlobalClass]
public sealed partial class CraftingRegistry: Resource
{
	[Export]
	private CraftRecipeDefinition[] _recipes;

	private readonly HashSet<string> _unlockedRecipes;

	public CraftingRegistry()
	{
		_unlockedRecipes = new();
	}

	/// <summary>
	/// Returns a crafting recipe definition from a given item ID
	/// </summary>
	/// <param name="outputId">The associated item ID</param>
	/// <returns></returns>
	public CraftRecipeDefinition GetDefinition(string outputId)
	{
		ReadOnlySpan<CraftRecipeDefinition> recipes = _recipes;

		for (int i = 0; i < recipes.Length; ++ i) {
			if (recipes[i].ResultingItemId != outputId)
				continue;

			return recipes[i];
		}

		return null;
	}

	public CraftRecipeDefinition[] GetUnlockedRecipes()
	{
		ReadOnlySpan<CraftRecipeDefinition> recipes = _recipes;

		int recipeCount = 0;
		CraftRecipeDefinition[] unlocked = new CraftRecipeDefinition[_recipes.Length];

		for (int i = 0; i < _recipes.Length; ++ i) {
			if (!recipes[i].AvailableImmediately &&
				!_unlockedRecipes.Contains(recipes[i].CraftId))
			{
				continue;
			}

			unlocked[recipeCount] = recipes[i];
			recipeCount ++;
		}

		if (recipeCount == 0)
			return null;

		return unlocked.AsSpan()[..recipeCount]
			.ToArray();
	}

	public void UnlockRecipe(string recipeId)
	{
		_unlockedRecipes.Add(recipeId);
	}

	public void UnlockRecipe(CraftRecipeDefinition recipe)
	{
		UnlockRecipe(recipe.CraftId);
	}

	public void UnlockAllRecipes()
	{
		ReadOnlySpan<CraftRecipeDefinition> recipes = _recipes;

		for (int i = 0; i < recipes.Length; ++ i) {
			_unlockedRecipes.Add(recipes[i].CraftId);
		}
	}
}
