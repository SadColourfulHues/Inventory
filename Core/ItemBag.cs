using System.Diagnostics;
using Godot;

using System;
using System.Runtime.CompilerServices;

namespace SadChromaLib.Specialisations.Inventory;

/// <summary>
/// An object that handles item storage and organisation.
/// </summary>
[GlobalClass]
public sealed partial class ItemBag: Resource
{
	public delegate void ItemsChangedCallback(ItemBag bagRef);
	public event ItemsChangedCallback OnItemsChanged;

	[Export]
	private ItemRegistry _registry;

	/// <summary>
	/// The maximum number of items in this item bag.
	/// </summary>
	[Export]
	public int MaxCapacity = 24;

	[Export]
	string _persistentFilename = "bag";

	readonly ItemEntry[] _items;

	public ItemBag() {
		_items = new ItemEntry[MaxCapacity];
	}

	public ItemBag(ItemRegistry registry, int capacity)
	{
		_items = new ItemEntry[MaxCapacity];

		_registry = registry;
		MaxCapacity = capacity;
	}

	#region Operations

	/// <summary>
	/// Removes all items from the item bag.
	/// </summary>
	public void Clear()
	{
		for (int i = 0; i < MaxCapacity; ++ i) {
			_items[i] = null;
		}

		EmitChangedSignal();
	}

	/// <summary>
	/// Copies items from another item bag and overwrites this bag's current content.
	/// </summary>
	/// <param name="source">The bag to copy from.</param>
	public void Copy(ItemBag source)
	{
		Debug.Assert(
			condition: source.MaxCapacity == MaxCapacity,
			message: "ItemBag.Copy: size mismatch"
		);

		ReadOnlySpan<ItemEntry> other = source._items.AsSpan();

		for (int i = 0; i < MaxCapacity; ++ i) {
			if (other[i] is null) {
				_items[i] = null;
				continue;
			}

			_items[i] = new() {
				ItemId = other[i].ItemId,
				Count = other[i].Count
			};
		}

		EmitChangedSignal();
	}

	/// <summary>
	/// Transfers an item from this item bag to another.
	/// </summary>
	/// <param name="slot">The slot of the item to transfer.</param>
	/// <param name="other">The bag to transfer the item to.</param>
	/// <param name="removeOnTransfer">Whether or not to remove the item on completion.</param>
	/// <returns></returns>
	public bool Transfer(int slot, ItemBag other, bool removeOnTransfer = true)
	{
		ItemEntry item = _items[slot];

		if (item is null)
			return false;

		ItemDefinition itemDef = _registry.GetDefinition(item.ItemId);

		if (itemDef is null)
			return false;

		int otherItemSlot = other.GetSlotForItemId(item.ItemId);
		int itemCountInOther = (
			otherItemSlot == -1
			? 0
			: other._items[otherItemSlot]?.Count ?? 0
		);

		// Both bags filled scenario
		if (item.Count == itemDef.MaxAmount &&
			itemCountInOther == itemDef.MaxAmount)
		{
			return false;
		}

		// Account for item overflow
		int totalCount = itemCountInOther + item.Count;

		if (totalCount <= itemDef.MaxAmount) {
			int? openSlot = other.FindOpenSlot();

			if (openSlot is null)
				return false;

			other._items[openSlot.Value] = item;

			if (removeOnTransfer) {
				_items[slot] = null;
			}
		}
		else {
			int overflow = totalCount % itemDef.MaxAmount;

			other._items[otherItemSlot].Count = itemDef.MaxAmount;
			_items[slot].Count = overflow;
		}

		EmitChangedSignal();
		other.EmitChangedSignal();

		return true;
	}

	/// <summary>
	/// Transfers the contents of this item bag to another,
	/// </summary>
	/// <param name="other">The item bag to receive the items.</param>
	/// <param name="filter">A method used to exclude certain items from the transfer.</param>
	public void Transfer(ItemBag other, Func<string, bool> filter = null)
	{
		for (int i = 0; i < MaxCapacity; ++ i) {
			if (_items[i] is null ||
				(filter?.Invoke(_items[i].ItemId) ?? false))
			{
				continue;
			}

			int? slotInOther = other.GetSlotForItemId(_items[i].ItemId);

			if (slotInOther.Value == -1) {
				slotInOther = other.FindOpenSlot();

				if (slotInOther is null)
					return;

				other._items[slotInOther.Value] = _items[i];
				_items[i] = null;
			}
			else {
				ItemDefinition itemDef = _registry.GetDefinition(_items[i].ItemId);

				if (_items[i].Count == itemDef.MaxAmount &&
					other._items[slotInOther.Value].Count == itemDef.MaxAmount)
				{
					continue;
				}

				if (itemDef is null)
					continue;

				ItemEntry otherItem = other._items[slotInOther.Value];

				int totalAmount = otherItem.Count + _items[i].Count;

				if (totalAmount > itemDef.MaxAmount) {
					int overflow = totalAmount % itemDef.MaxAmount;

					other._items[slotInOther.Value].Count = itemDef.MaxAmount;
					_items[i].Count = overflow;

					if (overflow > 0) {
						continue;
					}

					_items[i] = null;
				}
				else {
					other._items[slotInOther.Value].Count = totalAmount;
					_items[i] = null;
				}
			}
		}

		EmitChangedSignal();
		other.EmitChangedSignal();
	}

	/// <summary>
	/// Refills existing items from another bag.
	/// </summary>
	/// <param name="source">The bag to take items from.</param>
	/// <param name="filter">A method used to exclude certain items from the transfer.</param>
	public void Restock(ItemBag source, Func<string, bool> filter = null)
	{
		for (int i = 0; i < MaxCapacity; ++ i) {
			if (_items[i] is null)
				continue;

			ItemDefinition itemDef = _registry.GetDefinition(_items[i].ItemId);
			int slotInSource = source.GetSlotForItemId(itemDef.ItemId);

			if (itemDef is null || slotInSource == -1)
				continue;

			if (filter?.Invoke(itemDef.ItemId) ?? false)
				continue;

			ItemEntry itemInSource = source._items[slotInSource];

			int wantAmount = itemDef.MaxAmount - _items[i].Count;
			int takeAmount = Math.Min(wantAmount, itemInSource.Count);

			int count = itemInSource.Count - takeAmount;

			if (count < 1) {
				source._items[slotInSource] = null;
			}
			else {
				source._items[slotInSource].Count = count;
			}

			_items[i].Count = Math.Min(itemDef.MaxAmount, _items[i].Count + takeAmount);
		}

		EmitChangedSignal();
		source.EmitChangedSignal();
	}

	/// <summary>
	/// Adds an item with a specified ID to the bag.
	/// </summary>
	/// <param name="itemId">The ID of the item to give.</param>
	/// <param name="count">The amount of items to give. (Must be positive!)</param>
	/// <returns></returns>
	public bool GiveItem(string itemId, int count)
	{
		ItemDefinition itemDef = _registry.GetDefinition(itemId);

		#if TOOLS
		AssertOperation(itemDef, count);
		#endif

		ItemEntry entry = FindItem(itemId).Item1;

		// Update existing item stack value
		if (entry != null) {
			if (entry.Count >= itemDef.MaxAmount)
				return false;

			entry.Count = Math.Min(itemDef.MaxAmount, entry.Count + count);

			EmitChangedSignal();
			return true;
		}

		// Check if the bag can fit a new item stack
		int slot = FindOpenSlot() ?? -1;

		if (slot == -1)
			return false;

		entry = new() {
			ItemId = itemId,
			Count = Math.Min(itemDef.MaxAmount, count)
		};

		_items[slot] = entry;

		EmitChangedSignal();
		return true;
	}

	/// <summary>
	/// Removes a set amount of items from the bag.
	/// </summary>
	/// <param name="itemId">The ID of the item to remove.</param>
	/// <param name="count">The amount of items to take. (Must be positive!)</param>
	/// <returns></returns>
	public bool TakeItem(string itemId, int count)
	{
		ItemDefinition itemDef = _registry.GetDefinition(itemId);

		#if TOOLS
		AssertOperation(itemDef, count);
		#endif

		(ItemEntry, int) slotInfo = FindItem(itemId);

		ItemEntry entry = slotInfo.Item1;
		int entryIdx = slotInfo.Item2;

		// You can't taketh what doesn't existeth
		// ... it's 12:00 in the morning, okay?
		if (entry is null || entry.Count < count)
			return false;

		// Automatically remove empty item entries from the bag
		entry.Count = Math.Max(0, entry.Count - count);

		if (entry.Count == 0) {
			_items[entryIdx] = null;
		}

		EmitChangedSignal();
		return true;
	}

	/// <summary>
	/// Gives or takes away an item from the item bag.
	/// </summary>
	/// <param name="id">The item's associated ID.</param>
	/// <param name="count">The amount to give/take.</param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool GiveOrTakeItem(string id, int count)
	{
		int actualCount = Math.Abs(count);

		return count < 0
			? TakeItem(id, actualCount)
			: GiveItem(id, actualCount);
	}

	/// <summary>
	/// Returns true if the item bag contains the specified item.
	/// </summary>
	/// <param name="itemId">The ID of the item to check for.</param>
	/// <param name="count">The minimum amount of items.</param>
	/// <returns></returns>
	public bool HasItem(string itemId, int count = 1)
	{
		ItemDefinition itemDef = _registry.GetDefinition(itemId);
		ItemEntry entry = FindItem(itemId).Item1;

		Debug.Assert(itemDef is not null, "Invalid itemID!");

		return entry is not null && entry.Count >= count;
	}

	/// <summary>
	/// Overrides the item bag's registry. (Typically used for testing and/or debugging.)
	/// </summary>
	/// <param name="registry"></param>
	public void SetRegistryOverride(ItemRegistry registry) {
		_registry = registry;
	}

	#endregion

	#region Slots Helper

	/// <summary>
	/// Returns an ItemEntry at the specified slot.
	/// </summary>
	/// <param name="slot">The item slot.</param>
	/// <returns></returns>
	public ItemEntry GetItemAtSlot(int slot)
	{
		Debug.Assert(
			condition: slot >= 0 && slot < MaxCapacity,
			message: "Attempted to use 'GetItemAt' with an out-of-bounds index"
		);

		return _items[slot];
	}

	/// <summary>
	/// Returns an ItemEntry with the specified ID.
	/// </summary>
	/// <param name="itemId">The item ID.</param>
	/// <returns></returns>
	public ItemEntry GetItemWithId(string itemId)
	{
		int slot = GetSlotForItemId(itemId);

		if (slot == -1)
			return null;

		return GetItemAtSlot(slot);
	}

	/// <summary>
	/// Returns the slot of an item entry with the given ID.
	/// </summary>
	/// <param name="itemId"></param>
	/// <returns></returns>
	public int GetSlotForItemId(string itemId)
	{
		for (int i = 0; i < MaxCapacity; ++ i) {
			if (_items[i] is null ||
				_items[i].ItemId != itemId)
			{
				continue;
			}

			return i;
		}

		return -1;
	}

	/// <summary>
	/// Removes an item at the specified slot.
	/// </summary>
	/// <param name="slot"></param>
	public void ClearSlot(int slot)
	{
		Debug.Assert(
			condition: slot >= 0 && slot < MaxCapacity,
			message: "ItemBag: Attempted to clear an invalid slot."
		);

		_items[slot] = null;
		EmitChangedSignal();
	}

	/// <summary>
	/// Swaps the items at the specified slots.
	/// </summary>
	/// <param name="a">Item A</param>
	/// <param name="b">Item B</param>
	public void SwapSlots(int a, int b)
	{
		Debug.Assert(
			condition: a >= 0 && a < MaxCapacity && b >= 0 && b < MaxCapacity,
			message: "ItemBag: Attempted to swap invalid indexes."
		);

		(_items[a], _items[b]) = (_items[b], _items[a]);
		EmitChangedSignal();
	}

	/// <summary>
	/// Sorts the item bag's contents.
	/// </summary>
	/// <param name="sortMode"></param>
	public void Sort(ItemEntryComparer.Mode sortMode = ItemEntryComparer.Mode.AtoZ)
	{
		ItemEntryComparer comparer = new(_registry, sortMode);
		Array.Sort<ItemEntry>(_items, comparer);

		EmitChangedSignal();
	}

	/// <summary>
	/// Whether or not the item bag could hold more new items.
	/// </summary>
	/// <returns></returns>
	public bool IsFull() {
		return FindOpenSlot() is null;
	}

	private int? FindOpenSlot()
	{
		ReadOnlySpan<ItemEntry> items = _items;

		for (int i = 0; i < MaxCapacity; ++ i) {
			if (items[i] is not null)
				continue;

			return i;
		}

		return null;
	}

	private (ItemEntry, int) FindItem(string itemId)
	{
		for (int i = 0; i < MaxCapacity; ++ i) {
			if (_items[i] is null ||
				_items[i].ItemId != itemId)
			{
				continue;
			}

			return (_items[i], i);
		}

		return (null, -1);
	}

	#endregion

	#region Helpers

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void EmitChangedSignal()
	{
		OnItemsChanged?.Invoke(this);
		EmitChanged();
	}

	#if TOOLS
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void AssertOperation(ItemDefinition itemDef, int count)
	{
		Debug.Assert(itemDef is not null, "Invalid itemID!");
		Debug.Assert(count >= 0, "Tried to use a negative count in an unsigned operation.");
	}
	#endif

	#endregion
}