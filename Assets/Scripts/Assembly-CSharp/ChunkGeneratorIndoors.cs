using System.Collections.Generic;

public class ChunkGeneratorIndoors
{
	public static void GenerateUnexplored(ChunkData chunk_data, int X, int Z, string zone, InventoryItem house_item, int origin_chunkX, int origin_chunkZ, int origin_innerX, int origin_innerZ, Dictionary<string, ZoneData> auto_built_zones)
	{
		if (!DevBuildControl.Instance.IsDevPlacedShackZone(zone))
		{
			if (InventoryUtils.GetBuildingType(house_item.item_name) == InventoryUtils.building_type.castle)
			{
				chunk_data.TryAutoBuildAt("Upstairs Room", -14, 2, 3, origin_chunkX, origin_chunkZ, origin_innerX, origin_innerZ, zone, auto_built_zones);
				chunk_data.TryAutoBuildAt("Underground Room", -13, 9, 0, origin_chunkX, origin_chunkZ, origin_innerX, origin_innerZ, zone, auto_built_zones);
			}
			else if (InventoryUtils.GetBuildingType(house_item.item_name) == InventoryUtils.building_type.windmill)
			{
				chunk_data.TryAutoBuildAt("Upstairs Room", -9, 6, 2, origin_chunkX, origin_chunkZ, origin_innerX, origin_innerZ, zone, auto_built_zones);
			}
		}
	}
}
