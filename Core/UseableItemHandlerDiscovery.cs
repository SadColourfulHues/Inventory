using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SadChromaLib.Specialisations.Inventory;

/// <summary>
/// An organiser responsible for finding and running useable item handlers.
/// </summary>
public sealed partial class UseableItemHandlerDiscovery
{
	private readonly Dictionary<string, IUseableItemHandler> _handlers;

	public UseableItemHandlerDiscovery()
	{
		_handlers = new();
		LoadHandlerClasses();
	}

	/// <summary>
	/// Attempts to trigger the associated item handler's 'ItemUse' method.
	/// </summary>
	/// <param name="useableTag">The item's unique useability handler tag.</param>
	/// <param name="itemId">The item's identifier.</param>
	/// <param name="owner">A node referencing the user of said item.</param>
	/// <param name="bag">A reference to the item bag holding the item.</param>
	/// <returns></returns>
	public (bool useSuccess, bool closeOnUse)
	ExecHandlerForType(string useableTag, string itemId, Node owner, ItemBag bag)
	{
		if (!_handlers.ContainsKey(useableTag))
			return (false, false);

		IUseableItemHandler handler = _handlers[useableTag];

		if (!handler.ItemIsUseable( owner, itemId, bag))
			return (false, false);

		_handlers[useableTag].ItemUse(owner, itemId, bag);
		return (true, _handlers[useableTag].ItemCloseOnUse(itemId));
	}

	private void LoadHandlerClasses()
	{
		Assembly assembly = Assembly.GetExecutingAssembly();
		ReadOnlySpan<Type> types = assembly.GetTypes();

		for (int i = 0; i < types.Length; ++ i) {
			BindItemUseabilityTag handlerAttribute = types[i].GetCustomAttribute<BindItemUseabilityTag>();

			if (handlerAttribute == null)
				continue;

			object handlerObject = Activator.CreateInstance(types[i]);

			if (handlerObject is not IUseableItemHandler useableItem)
				continue;

			_handlers.Add(
				handlerAttribute.Tag,
				value: useableItem
			);
		}
	}
}