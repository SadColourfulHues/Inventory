using Godot;
using System;

namespace SadChromaLib.Specialisations.Inventory;

/// <summary>
/// An interface that defines a basic useable item handler
/// </summary>
public interface IUseableItemHandler
{
	bool ItemCloseOnUse(StringName itemId);
	bool ItemIsUseable(Node user, StringName itemId, ItemBag bag);
	void ItemUse(Node user, StringName itemId, ItemBag bag);
}

/// <summary>
/// An attribute that binds a useable item tag to a handler. (This is required to make the class visible to UseableItemHandlerDiscovery.)
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class BindItemUseabilityTag : Attribute
{
	public StringName Tag;

	public BindItemUseabilityTag(string tag)
	{
		Tag = tag;
	}
}