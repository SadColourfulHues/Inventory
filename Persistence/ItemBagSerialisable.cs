using Godot;
using Godot.Collections;

using SadChromaLib.Persistence;

namespace SadChromaLib.Specialisations.Inventory;

using SerialisedData = Dictionary<StringName, Variant>;

public sealed partial class ItemBag : ISerialisable
{
	private static StringName KeyItemId => "id";
	private static StringName KeyItemCount => "qty";

	public string SerialisableGetFilename()
		=> $"bag_{_persistentFilename}.json";

	public void SerialisableWrite(FileAccess fileRef)
	{
		Array<SerialisedData> data = new();
		data.Resize(MaxCapacity);

		for (int i = 0; i < MaxCapacity; ++ i) {
			if (_items[i] == null) {
				data[i] = null;
				continue;
			}

			data[i] = new() {
				[KeyItemId] = _items[i].ItemId,
				[KeyItemCount] = _items[i].Count
			};
		}

		fileRef.StoreString(Json.Stringify(data));
		fileRef.Close();
	}

	public void SerialisableRead(string serialisedData)
	{
		Array<SerialisedData> data = (Array<SerialisedData>) Json.ParseString(serialisedData);

		if (data.Count != MaxCapacity) {
			GD.PrintErr("ItemBag: max capacity mistmatch in serialised data.");
			return;
		}

		for (int i = 0; i < data.Count; ++ i) {
			if (data[i] == null ||
				data[i].Count < 1)
			{
				_items[i] = null;
				continue;
			}

			SerialisedData itemData = data[i];

			_items[i] = new() {
				ItemId = (StringName) itemData[KeyItemId],
				Count = (int) itemData[KeyItemCount]
			};
		}
	}
}