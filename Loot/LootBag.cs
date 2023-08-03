using Godot;

using SadChromaLib.Utils.Random;

namespace SadChromaLib.Specialisations.Inventory.Loot;

[GlobalClass]
public sealed partial class LootBag : Resource
{
	[Export]
	private LootEntry[] _lootItems;

	private WeightedBag<StringName> _bag;

	#region Main Functions

	/// <summary>
	/// Picks a random item from this bag's loot pool
	/// </summary>
	/// <returns></returns>
	public StringName GetLoot()
	{
		InitialiseBag();
		return _bag.Pick();
	}

	/// <summary>
	/// Gives a specified item bag a random item based off this bag's loot pool
	/// </summary>
	/// <param name="itemBag">The item bag to receive the items</param>
	/// <param name="minAmount">The minimum amount to give</param>
	/// <param name="maxAmount">The maximum amount to give</param>
	/// <param name="rolls">The number of items to give the item bag</param>
	public void GiveLoot(ItemBag itemBag, int minAmount, int maxAmount, int rolls = 1)
	{
		InitialiseBag();

		int roll = RandomUtils.BasicInt() % rolls;

		for (int i = 0; i < roll; ++ i) {
			StringName itemId = _bag.Pick();
			int count = (int) RandomUtils.RangeLong(minAmount, maxAmount);

			itemBag.GiveItem(itemId, count);
		}
	}

	#endregion

	private void InitialiseBag()
	{
		if (_bag != null)
			return;

		_bag = new(_lootItems);
	}
}
