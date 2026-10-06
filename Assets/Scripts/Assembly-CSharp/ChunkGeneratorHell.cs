using System.Collections.Generic;
using UnityEngine;

public class ChunkGeneratorHell
{
	private static string[] possible_mobs = { "bat", "devil", "dragon", "dwarf", "gecko", "politician", "raptor", "scorpion", "snake", "trex" };

	public static void GenerateUnexplored(ChunkData chunk_data, int X, int Z, string zone, InventoryItem house_item, int origin_chunkX, int origin_chunkZ, int origin_innerX, int origin_innerZ, Dictionary<string, ZoneData> auto_built_zones)
	{
		chunk_data.biome_mobA = possible_mobs[Random.Range(0, possible_mobs.Length)];
		chunk_data.biome_mobB = possible_mobs[Random.Range(0, possible_mobs.Length)];
		if (!DevBuildControl.Instance.IsDevPlacedShackZone(zone)) TryAutoBuildStartArea(house_item, chunk_data, origin_chunkX, origin_chunkZ, origin_innerX, origin_innerZ, zone, auto_built_zones);
		else Debug.Log("IS DEV BUILT HOUSE");
		int variant = Random.Range(1, 6);
		Color[] pixels = ResourceControl.Instance.LoadImageSynchronously("Lava Variants/Variant" + variant).GetPixels();
		int pixel = 0;
		for (int x = 9; x >= 0; x--)
		{
			for (int z = 9; z >= 0; z--)
			{
				if (chunk_data.EmptyAt(x, z) && pixels[pixel] == Color.black) chunk_data.AddElement(x, z, new ChunkElement("Lava"));
				pixel++;
			}
		}
		float value = Random.value;
		for (int i = 0; i < 2; i++)
		{
			TryPlaceObjectRandomly(chunk_data, (double)value < 0.5 ? "Small Black Stalagmite" : "Large Black Stalagmite");
			value = Random.value;
		}
		if ((double)value < 0.07) TryPlaceObjectRandomly(chunk_data, "Old Torch");
		if ((double)Random.value < 0.2)
		{
			int x = -1;
			int z = -1;
			for (int attempt = 0; attempt < 5; attempt++)
			{
				x = Random.Range(0, 10);
				z = Random.Range(0, 10);
				if (chunk_data.GetElementsAt(x, z).Count == 0) break;
				x = -1;
				z = -1;
			}
			if (x != -1 && z != -1)
			{
				chunk_data.AddElement(x, z, new ChunkElement("Magmite Vein", Random.Range(0, 4)));
				int additional = (double)Random.value < 0.5 ? 1 : 2;
				for (int i = 0; i < additional; i++)
				{
					for (int attempt = 0; attempt < 5; attempt++)
					{
						int dx = Random.Range(-2, 2);
						int dz = Random.Range(-2, 2);
						int nextX = x + dx;
						int nextZ = z + dz;
						if (nextX < 0 || nextZ < 0 || nextX >= 10 || nextZ >= 10 || chunk_data.GetElementsAt(nextX, nextZ).Count != 0) continue;
						chunk_data.AddElement(nextX, nextZ, new ChunkElement("Magmite Vein", Random.Range(0, 4)));
						x = nextX;
						z = nextZ;
						break;
					}
				}
			}
		}
		if ((double)Random.value >= 0.12) return;
		value = Random.value;
		string mob;
		if ((double)value < 0.2) mob = "Mob - Normal";
		else if ((double)value < 0.4)
		{
			mob = "Mob - Normal";
			TryPlaceObjectRandomly(chunk_data, mob);
		}
		else if ((double)value < 0.6)
		{
			TryPlaceObjectRandomly(chunk_data, "Mob - Giant");
			return;
		}
		else if ((double)value < 0.8) mob = "Mob - Vengeful Spirit";
		else
		{
			TryPlaceObjectRandomly(chunk_data, "Mob - Bandit");
			mob = "Mob - Bandit Elite";
		}
		TryPlaceObjectRandomly(chunk_data, mob);
		TryPlaceObjectRandomly(chunk_data, mob);
	}

	private static void TryPlaceObjectRandomly(ChunkData chunk_data, string object_id)
	{
		for (int attempt = 0; attempt < 5; attempt++)
		{
			int x = Random.Range(0, 10);
			int z = Random.Range(0, 10);
			if (chunk_data.GetElementsAt(x, z).Count != 0) continue;
			chunk_data.AddElement(x, z, new ChunkElement(object_id, Random.Range(0, 4)));
			return;
		}
	}

	private static void TryAutoBuildStartArea(InventoryItem house_item, ChunkData chunk_data, int origin_chunkX, int origin_chunkZ, int origin_innerX, int origin_innerZ, string zone, Dictionary<string, ZoneData> auto_built_zones)
	{
		for (int x = -2; x <= 2; x++)
		{
			for (int z = -2; z <= 2; z++) chunk_data.TryAutoBuildAt("DEBUG-1x1-fillempty", x, z, 0, origin_chunkX, origin_chunkZ, origin_innerX, origin_innerZ, zone, auto_built_zones);
		}
		if (house_item.GetShort("dont_build_start_area") != 0) return;
		foreach (Vector2 position in new List<Vector2> { new Vector2(-4, -2), new Vector2(-4, 0), new Vector2(-4, 2) })
		{
			chunk_data.TryAutoBuildAt("Sky Chest", (int)position.x, (int)position.y, 0, origin_chunkX, origin_chunkZ, origin_innerX, origin_innerZ, zone, auto_built_zones);
		}
		List<Vector2> paths = new List<Vector2>
		{
			new Vector2(1, 1), new Vector2(1, 0), new Vector2(1, -1),
			new Vector2(0, 1), new Vector2(0, -1),
			new Vector2(-1, 1), new Vector2(-1, 0), new Vector2(-1, -1), new Vector2(-2, 0)
		};
		for (int z = -2; z <= 2; z++)
		{
			paths.Add(new Vector2(-3, z));
			paths.Add(new Vector2(-4, z));
		}
		foreach (Vector2 position in paths)
		{
			chunk_data.TryAutoBuildAt("Stone Bricks", (int)position.x, (int)position.y, 0, origin_chunkX, origin_chunkZ, origin_innerX, origin_innerZ, zone, auto_built_zones);
		}
	}
}
