using Godot;
using System.Collections.Generic;

namespace SadChromaLib.Specialisations.Inventory;

/// <summary>
/// A helper class that implements an object comparer for ItemBag's sorting mechanism
/// </summary>
public sealed partial class ItemEntryComparer: IComparer<ItemEntry>
{
	private ItemRegistry _registryRef;
	private Mode _sortMode;

	public ItemEntryComparer(ItemRegistry registry, Mode sortMode)
	{
		_registryRef = registry;
		_sortMode = sortMode;
	}

	public int Compare(ItemEntry x, ItemEntry y)
	{
		if (x == null && y != null)
			return 1;
		else if (x != null && y == null)
			return -1;
		else if (x == null && y == null)
			return 0;

		ItemDefinition aDef = _registryRef.GetDefinition(x.ItemId);
		ItemDefinition bDef = _registryRef.GetDefinition(y.ItemId);

		switch (_sortMode) {
			case Mode.AtoZ:
				return aDef.DisplayName.CompareTo(bDef.DisplayName);

			case Mode.Count:
				return y.Count.CompareTo(x.Count);

			case Mode.Price:
				float priceA = aDef.BasePrice * x.Count;
				float priceB = bDef.BasePrice * y.Count;

				return priceB.CompareTo(priceA);

			case Mode.Hue:
				float a = GetHueScore(aDef.GetIcon(), 1);
				float b = GetHueScore(bDef.GetIcon(), 1);

				return a.CompareTo(b);

			default:
				return 0;
		}
	}

	private static Color GetAverageColour(Texture2D texture, int stepSize)
	{
		Image image = texture.GetImage();
		float samples = 0.0f;
		float averageR = 0.0f;
		float averageG = 0.0f;
		float averageB = 0.0f;

		for (int y = 0, h = image.GetHeight(); y < h; y += stepSize) {
			for (int x = 0, w = image.GetWidth(); x < w; x += stepSize) {
				Color colour = image.GetPixel(x, y);

				averageR += colour.R;
				averageG += colour.G;
				averageB += colour.B;

				samples ++;
			}
		}

		return new(
			r: averageR / samples,
			g: averageG / samples,
			b: averageB / samples,
			a: 1.0f
		);
	}

	private static float GetHueScore(Texture2D texture, int stepSize)
	{
		Color colour = GetAverageColour(texture, stepSize);
		return (1.3f * colour.H) + ((1f - colour.S) * (1f - colour.V));
	}

	public enum Mode
	{
		AtoZ,
		Hue,
		Count,
		Price
	}
}