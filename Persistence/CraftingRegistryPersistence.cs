using Godot;
using SadChromaLib.Persistence;

namespace SadChromaLib.Specialisations.Inventory.Crafting;

public sealed partial class CraftingRegistry: ISerialisableComponent
{
	public void Serialise(PersistenceWriter writer)
	{
		writer.Write(_unlockedRecipes);
	}

	public void Deserialise(PersistenceReader reader)
	{
		reader.ReadStringSet(_unlockedRecipes);
	}
}