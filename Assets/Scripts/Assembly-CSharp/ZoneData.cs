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
		InventoryItem item = InventoryItem.UnpackFromWeb(incoming);
		int rotation = incoming.GetByte();
		int chunkX = incoming.GetShort();
		int chunkZ = incoming.GetShort();
		int innerX = incoming.GetShort();
		int innerZ = incoming.GetShort();
		string outerZone = incoming.GetString();
		Dictionary<string, LandClaimChunkTimer> timers = new Dictionary<string, LandClaimChunkTimer>();
		int count = incoming.GetShort();
		for (int i = 0; i < count; i++)
		{
			string key = incoming.GetString();
			int second = incoming.GetShort();
			int minute = incoming.GetShort();
			int hour = incoming.GetShort();
			int day = incoming.GetShort();
			int month = incoming.GetShort();
			int year = incoming.GetShort();
			string user0 = incoming.GetString();
			string user1 = incoming.GetString();
			string user2 = incoming.GetString();
			LandClaimChunkTimer timer = ChunkData.CreateLandClaimChunkTimer(key, user0, user1, user2, new System.DateTime(year, month, day, hour, minute, second));
			timers[timer.land_claim_str] = timer;
		}
		ZoneData result = new ZoneData(zone_name, item, rotation, outerZone, chunkX, chunkZ, innerX, innerZ);
		result.outdoor_land_claim_chunk_timers = timers;
		return result;
	}

	public void CalculateOutdoorLandClaims()
	{
		string text = outer_item_zone;
		short @short = house_item.GetShort("outer_item_chunkX");
		short short2 = house_item.GetShort("outer_item_chunkZ");
		if (text != "overworld")
		{
			int num = -15;
			do
			{
				ZoneData zoneData = ZoneDataControl.Instance.LoadZoneDataFromDisk(text);
				text = zoneData.outer_item_zone;
				@short = zoneData.house_item.GetShort("outer_item_chunkX");
				short2 = zoneData.house_item.GetShort("outer_item_chunkZ");
				if (num++ == -1)
				{
					return;
				}
			}
			while (text != "overworld");
		}
		string chunkString = ChunkControl.Instance.GetChunkString(text, @short, short2);
		bool flag = ChunkControl.Instance.IsChunkFullyLoadedOrMidload(chunkString);
		ChunkData chunkData = (flag ? ChunkControl.Instance.GetChunkData(chunkString) : ChunkControl.Instance.HostGetChunk(text, @short, short2));
		outdoor_land_claim_chunk_timers = chunkData.GetAllLandClaimTimers();
		if (!flag)
		{
			chunkData.SaveLandClaimChunkTimersToDisk(chunkString);
		}
	}

	public void ClearOutdoorLandClaims()
	{
		outdoor_land_claim_chunk_timers.Clear();
	}

	public static bool IsNpcHome(string zone_key)
	{
		bool file_exists = false;
		if (Startup.StringNullOrWhitespace(zone_key) || zone_key == "overworld")
		{
			return false;
		}
		file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines(System.IO.Path.Combine(DevBuildControl.quest_scenics_folder_, "(Auto Gen) npc_home_data"), ref file_exists);
		if (!file_exists)
		{
			return false;
		}
		foreach (string item in textFileLines)
		{
			if (!Startup.StringNullOrWhitespace(item) && zone_key == "shack" + item.Substring(0, item.IndexOf(" = ")))
			{
				return true;
			}
		}
		return false;
	}

	public static void GetNpcHomeData(string npc_home_data_file, string zone_key, ref InventoryItem house_item, ref int interior_model_chunkX, ref int interior_model_chunkZ, ref int interior_model_innerX, ref int interior_model_innerZ, ref int outer_item_rot, ref string outer_item_zone)
	{
		bool file_exists = false;
		bool file_exists2 = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines(npc_home_data_file, ref file_exists2);
		if (!file_exists2)
		{
			return;
		}
		foreach (string item in textFileLines)
		{
			if (Startup.StringNullOrWhitespace(item))
			{
				continue;
			}
			int num = item.IndexOf(" = ");
			if (!("shack" + item.Substring(0, num) == zone_key))
			{
				continue;
			}
			int num2 = item.IndexOf(')');
			string text = item.Substring(num + 3, num2 - num - 2);
			string text2 = item.Substring(num2 + 2, item.Length - (num2 + 2));
			int num3 = text2.IndexOf(" = ");
			int num4 = text2.IndexOf(" (");
			string new_item_name = text2.Substring(num3 + 3, num4 - (num3 + 3));
			if (DevBuildControl.Instance.debug_bandit_camp_data != null && new_item_name == "BIOME_CAVE_ENTRANCE")
			{
				new_item_name = inventory_ctr.BiomeIdToCaveEntrance(DevBuildControl.Instance.debug_bandit_camp_data.biome_id);
			}
			int num5 = -1;
			int num6 = -1;
			for (int i = 0; i < text.Length; i++)
			{
				if (num5 == -1)
				{
					if (text[i] == ',')
					{
						num5 = i;
					}
				}
				else if (num6 == -1 && text[i] == ',')
				{
					num6 = i;
				}
			}
			interior_model_chunkX = int.Parse(text.Substring(num5 + 1, num6 - (num5 + 1)), Startup.parse_culture);
			interior_model_chunkZ = int.Parse(text.Substring(num6 + 2, text.IndexOf(')') - (num6 + 2)), Startup.parse_culture);
			interior_model_innerX = int.Parse(text2.Substring(1, 1), Startup.parse_culture);
			interior_model_innerZ = int.Parse(text2.Substring(3, 1), Startup.parse_culture);
			int num7 = text.IndexOf('(');
			outer_item_zone = text.Substring(num7 + 1, text.IndexOf(',') - (num7 + 1));
			outer_item_rot = int.Parse(text2.Substring(text2.IndexOf("(rot ") + 5, 1), Startup.parse_culture);
			file_exists = false;
			List<string> textFileLines2 = ResourceControl.Instance.GetTextFileLines(DevBuildControl.quest_scenics_folder_ + "/" + text, ref file_exists);
			if (!file_exists)
			{
				break;
			}
			ExtraInventoryData extraInventoryData = new ExtraInventoryData();
			bool flag = false;
			foreach (string item2 in textFileLines2)
			{
				if (Startup.StringNullOrWhitespace(item2))
				{
					continue;
				}
				if (!flag)
				{
					flag = item2.Contains(text2);
					continue;
				}
				if (item2[0] == '[')
				{
					break;
				}
				int num8 = -1;
				int num9 = -1;
				for (int j = 0; j < item2.Length; j++)
				{
					if (num8 == -1)
					{
						if (char.IsLetterOrDigit(item2[j]))
						{
							num8 = j;
						}
					}
					else if (num9 == -1 && item2[j] == '=')
					{
						num9 = j;
					}
				}
				string key = item2.Substring(num8, num9 - num8 - 1);
				string text3 = item2.Substring(num9 + 2, item2.Length - (num9 + 2));
				if (text3.Contains("*string*"))
				{
					extraInventoryData.SetString(key, text3.Replace("*string* ", ""));
				}
				else if (text3.Contains("*short*"))
				{
					extraInventoryData.SetShort(key, short.Parse(text3.Replace("*short* ", ""), Startup.parse_culture));
				}
				else if (text3.Contains("*long*"))
				{
					extraInventoryData.SetLong(key, int.Parse(text3.Replace("*long* ", ""), Startup.parse_culture));
				}
			}
			extraInventoryData.SetShort("depth", 0);
			extraInventoryData.SetShort("outer_item_chunkX", interior_model_chunkX);
			extraInventoryData.SetShort("outer_item_chunkZ", interior_model_chunkZ);
			extraInventoryData.SetShort("outer_item_innerX", interior_model_innerX);
			extraInventoryData.SetShort("outer_item_innerZ", interior_model_innerZ);
			if (DevBuildControl.Instance.debug_bandit_camp_data != null)
			{
				extraInventoryData.SetString("bandit_camp_instance", "debug_instance");
			}
			house_item = new InventoryItem(new_item_name, extraInventoryData);
			break;
		}
	}

	public void SaveToDisk(string zone_key)
	{
		string text = zone_key + "-zonedata";
		string zoneDataFilename = ChunkControl.GetZoneDataFilename(text);
		house_item.SaveToDisk(zoneDataFilename, "zone_item", text);
		PlayerData.Instance.SetSlotShort("outer_item_rot", outer_item_rot, zoneDataFilename, text);
		PlayerData.Instance.SetSlotShort("interior_model_chunkX", interior_model_chunkX, zoneDataFilename, text);
		PlayerData.Instance.SetSlotShort("interior_model_chunkZ", interior_model_chunkZ, zoneDataFilename, text);
		PlayerData.Instance.SetSlotShort("interior_model_innerX", interior_model_innerX, zoneDataFilename, text);
		PlayerData.Instance.SetSlotShort("interior_model_innerZ", interior_model_innerZ, zoneDataFilename, text);
		PlayerData.Instance.SetSlotString("outer_item_zone", outer_item_zone, zoneDataFilename, text);
		if (InventoryUtils.IsCaveObject(house_item.item_name))
		{
			ZoneDataControl.Instance.InitialAdjustCave(zone_key, house_item);
		}
	}

	public void PackForWeb(Packet outgoing)
	{
		house_item.PackForWeb(outgoing);
		outgoing.PutByte((byte)outer_item_rot);
		outgoing.PutShort(interior_model_chunkX);
		outgoing.PutShort(interior_model_chunkZ);
		outgoing.PutShort(interior_model_innerX);
		outgoing.PutShort(interior_model_innerZ);
		outgoing.PutString(outer_item_zone);
		outgoing.PutShort(outdoor_land_claim_chunk_timers.Count);
		foreach (LandClaimChunkTimer timer in outdoor_land_claim_chunk_timers.Values)
		{
			outgoing.PutString(timer.land_claim_str);
			outgoing.PutShort(timer.when_to_expire.Second);
			outgoing.PutShort(timer.when_to_expire.Minute);
			outgoing.PutShort(timer.when_to_expire.Hour);
			outgoing.PutShort(timer.when_to_expire.Day);
			outgoing.PutShort(timer.when_to_expire.Month);
			outgoing.PutShort(timer.when_to_expire.Year);
			outgoing.PutString(timer.land_claim_user0);
			outgoing.PutString(timer.land_claim_user1);
			outgoing.PutString(timer.land_claim_user2);
		}
	}
}
