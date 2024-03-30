using System;
using System.Diagnostics;

using SadChromaLib.Persistence;

namespace SadChromaLib.Specialisations.Inventory;

public sealed partial class ItemBag: ISerialisable
{
	public string SerialisableGetFilename()
		=> $"bag_{_persistentFilename}";

	public void SerialisableWrite(PersistenceWriter writer)
	{
		ReadOnlySpan<ItemEntry> items = _items.AsSpan();
		writer.Write(MaxCapacity);

		for (int i = 0; i < items.Length; ++i) {
			if (items[i] is null) {
				writer.Write(false);
				continue;
			}

			writer.Write(true);
			writer.Write(items[i].ItemId);
			writer.Write(items[i].Count);
		}
	}

	public void SerialisableRead(PersistenceReader reader)
	{
		int capacity = reader.ReadInt();

		Debug.Assert(
			condition: capacity <= MaxCapacity,
			message: "ItemBag: Deserialisation failed => capacity mismatch"
		);

		for (int i = 0; i < capacity; ++ i) {
			if (!reader.ReadBool()) {
				_items[i] = null;
				continue;
			}

			_items[i] = new() {
				ItemId = reader.ReadString(),
				Count = reader.ReadInt()
			};
		}

		EmitSignal(SignalName.Changed);
		EmitChanged();
	}
}