using Godot;
using Godot.Collections;

using SadChromaLib.Persistence;

namespace SadChromaLib.Specialisations.Inventory.Crafting;

using SerialisedData = Dictionary<StringName, Variant>;

public sealed partial class CraftingRegistry : ISerialisableComponent
{
	private static StringName KeyUnlockedRecipes => "unlockedRecipes";

	public SerialisedData Serialise()
	{
		Array<StringName> recipeIds = new();
		recipeIds.AddRange(_unlockedRecipes);

		return new() {
			[KeyUnlockedRecipes] = recipeIds
		};
	}

	public void Deserialise(SerialisedData data)
	{
		Array<StringName> recipeIds = (Array<StringName>) data[KeyUnlockedRecipes];
		_unlockedRecipes.Clear();

		for (int i = 0; i < recipeIds.Count; ++ i) {
			_unlockedRecipes.Add(recipeIds[i]);
		}
	}
}