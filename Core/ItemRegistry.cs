using Godot;

namespace SadChromaLib.Specialisations.Inventory;

[GlobalClass]
public sealed partial class ItemRegistry: Resource
{
	[Export]
	ItemDefinition[] _definitions;

	public ItemRegistry()
	{
	}

	public ItemRegistry(ItemDefinition[] definitions)
	{
		_definitions = definitions;
	}

	public bool IsValid(string id)
	{
		System.ReadOnlySpan<ItemDefinition> definitions = _definitions;

		for (int i = 0; i < definitions.Length; ++ i) {
			if (_definitions[i].ItemId != id) {
				continue;
			}

			return true;
		}

		return false;
	}

	public ItemDefinition GetDefinition(string id)
	{
		System.ReadOnlySpan<ItemDefinition> definitions = _definitions;

		for (int i = 0; i < definitions.Length; ++ i) {
			if (_definitions[i].ItemId != id) {
				continue;
			}

			return _definitions[i];
		}

		return null;
	}

	public ItemDefinition[] GetDefinitionList()
	{
		return _definitions;
	}
}