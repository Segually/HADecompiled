using System.Collections.Generic;

public class ZoneData
{
	public string zone_name;

	public InventoryItem house_item;

	public int interior_model_chunkX;

	public int interior_model_chunkZ;

	public int interior_model_innerX;

	public int interior_model_innerZ;

	public int outer_item_rot;

	public string outer_item_zone = "";

	public Dictionary<string, LandClaimChunkTimer> outdoor_land_claim_chunk_timers = new Dictionary<string, LandClaimChunkTimer>();

	public ZoneData(string zone_name, InventoryItem house_item, int outer_item_rot, string outer_item_zone, int interior_model_chunkX, int interior_model_chunkZ, int interior_model_innerX, int interior_model_innerZ)
	{
		this.zone_name = zone_name;
		this.house_item = house_item;
		this.outer_item_rot = outer_item_rot;
		this.outer_item_zone = outer_item_zone;
		this.interior_model_chunkX = interior_model_chunkX;
		this.interior_model_chunkZ = interior_model_chunkZ;
		this.interior_model_innerX = interior_model_innerX;
		this.interior_model_innerZ = interior_model_innerZ;
	}

	public static ZoneData UnpackFromWeb(Packet incoming, string zone_name)
	{
		return null;
	}

	public void CalculateOutdoorLandClaims()
	{
	}

	public void ClearOutdoorLandClaims()
	{
		outdoor_land_claim_chunk_timers.Clear();
	}

	public static bool IsNpcHome(string zone_key)
	{
		return false;
	}

	public static void GetNpcHomeData(string npc_home_data_file, string zone_key, ref InventoryItem house_item, ref int interior_model_chunkX, ref int interior_model_chunkZ, ref int interior_model_innerX, ref int interior_model_innerZ, ref int outer_item_rot, ref string outer_item_zone)
	{
	}

	public void SaveToDisk(string zone_key)
	{
	}

	public void PackForWeb(Packet outgoing)
	{
	}
}
