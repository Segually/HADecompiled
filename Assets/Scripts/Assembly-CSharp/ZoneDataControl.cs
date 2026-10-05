using System;
using System.Collections.Generic;
using UnityEngine;

public class ZoneDataControl : MonoBehaviour, OrderedStart
{
	public enum change_zone_type
	{
		place_at_entrance = 0,
		place_at_exit = 1,
		custom_position = 2,
		custom_position_no_transition = 3
	}

	public static ZoneDataControl Instance;

	private ZoneData curr_zonedata_cache;

	public GameObject curr_interior;

	public GameObject curr_cave_exit;

	public GameObject generic_cave_exit;

	public GameObject clouds_exit;

	public Color hell_col;

	public ZoneData curr_zonedata
	{
		get
		{
			return curr_zonedata_cache;
		}
		set
		{
			curr_zonedata_cache = value;
		}
	}

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
		curr_zonedata_cache = LoadOverworld();
	}

	public static void GenerateNPCShackData()
	{
	}

	public List<string> GetZoneTrail(string start_zone_name)
	{
		return null;
	}

	public ZoneData LoadOverworld()
	{
		return new ZoneData("overworld", new InventoryItem(""), 0, "", 0, 0, 0, 0);
	}

	public ZoneData LoadZoneDataFromDisk(string zone_name)
	{
		ZoneData zoneData = new ZoneData(zone_name, new InventoryItem(""), 0, "", 0, 0, 0, 0);
		if (zone_name == "overworld" || zone_name == "")
		{
			return LoadOverworld();
		}
		if (!ZoneData.IsNpcHome(zone_name))
		{
			string text = zone_name + "-zonedata";
			string zoneDataFilename = ChunkControl.GetZoneDataFilename(text);
			InventoryItem inventoryItem = InventoryItem.LoadFromDisk("zone_item", zoneDataFilename, text);
			if (inventoryItem.item_name == "")
			{
				return LoadOverworld();
			}
			zoneData.house_item = inventoryItem;
			zoneData.interior_model_chunkX = PlayerData.Instance.GetSlotShort("interior_model_chunkX", zoneDataFilename, text);
			zoneData.interior_model_chunkZ = PlayerData.Instance.GetSlotShort("interior_model_chunkZ", zoneDataFilename, text);
			zoneData.interior_model_innerX = PlayerData.Instance.GetSlotShort("interior_model_innerX", zoneDataFilename, text);
			zoneData.interior_model_innerZ = PlayerData.Instance.GetSlotShort("interior_model_innerZ", zoneDataFilename, text);
			zoneData.outer_item_rot = PlayerData.Instance.GetSlotShort("outer_item_rot", zoneDataFilename, text);
			zoneData.outer_item_zone = PlayerData.Instance.GetSlotString("outer_item_zone", zoneDataFilename, text);
		}
		else
		{
			InventoryItem house_item = new InventoryItem("");
			string outer_item_zone = "";
			int interior_model_chunkX = 0;
			int interior_model_chunkZ = 0;
			int interior_model_innerX = 0;
			int interior_model_innerZ = 0;
			int outer_item_rot = 0;
			ZoneData.GetNpcHomeData(System.IO.Path.Combine(DevBuildControl.quest_scenics_folder_, "(Auto Gen) npc_home_data"), zone_name, ref house_item, ref interior_model_chunkX, ref interior_model_chunkZ, ref interior_model_innerX, ref interior_model_innerZ, ref outer_item_rot, ref outer_item_zone);
			zoneData.house_item = house_item;
			zoneData.interior_model_chunkX = interior_model_chunkX;
			zoneData.interior_model_chunkZ = interior_model_chunkZ;
			zoneData.interior_model_innerX = interior_model_innerX;
			zoneData.interior_model_innerZ = interior_model_innerZ;
			zoneData.outer_item_rot = outer_item_rot;
			zoneData.outer_item_zone = outer_item_zone;
		}
		zoneData.CalculateOutdoorLandClaims();
		return zoneData;
	}

	public void ModifyPlayerZone(string zone_name, int new_rot, InventoryItem new_item)
	{
	}

	public void InitialAdjustCave(string zone_name, InventoryItem zone_house_item)
	{
	}

	public bool PlayerZoneExists(string zone_name)
	{
		return false;
	}

	public void CreatePlayerZone(string new_zone_name, InventoryItem house_item, int interior_model_chunkX, int interior_model_chunkZ, int interior_model_innerX, int interior_model_innerZ, string outer_item_zone, int outer_item_rot)
	{
	}

	public void SetInteriorRotation(int rot)
	{
	}

	public void CreateInteriorModel(ZoneData zone_data, Action on_interior_model_complete)
	{
	}

	private void MakePlaneBlack()
	{
	}

	private Vector3 GetCurrDoorwayPosition(ZoneData zone_data)
	{
		return default(Vector3);
	}

	public Vector3 GetCurrExitPosition(ZoneData zone_data)
	{
		return default(Vector3);
	}

	public void UpdateZoneItemOnChangedOutside()
	{
	}

	public void ChangeZone(ZoneData new_zone_data, change_zone_type type, Action on_zone_change_complete, bool send, bool on_map_change, bool clear_mobs = true)
	{
		ChangeZone(new_zone_data, type, Vector3.zero, on_zone_change_complete, send, on_map_change, clear_mobs);
	}

	public void ChangeZone(ZoneData new_zone_data, change_zone_type type, Vector3 custom_position, Action on_zone_change_complete, bool send, bool on_map_change, bool clear_mobs = true)
	{
		if (GameServerConnector.Instance.FullyInGame() && !GameServerConnector.Instance.is_host)
		{
			GameServerConnector.Instance.slow_load_chunks_begin = DateTime.UtcNow;
		}
		if (curr_interior != null)
		{
			UnityEngine.Object.Destroy(curr_interior);
		}
		if (curr_cave_exit != null)
		{
			UnityEngine.Object.Destroy(curr_cave_exit);
		}
		GameController.Instance.breeder_floor_plane.SetActive(false);
		ZoneData prev_zonedata = curr_zonedata;
		if (prev_zonedata != null)
		{
			prev_zonedata.ClearOutdoorLandClaims();
		}
		curr_zonedata = new_zone_data;
		ChunkControl.Instance.player_zone = new_zone_data.zone_name;
		Action action = delegate
		{
			Vector3 vector = type switch
			{
				change_zone_type.place_at_exit => GetCurrExitPosition(prev_zonedata),
				change_zone_type.place_at_entrance => GetCurrDoorwayPosition(curr_zonedata),
				_ => custom_position,
			};
			vector += Vector3.up;
			if (GameController.Instance.player != null)
			{
				CustomTeleporterControl.Instance.EndTeleportAnimation(GameController.Instance.player);
				if (GameServerConnector.Instance.FullyInGame())
				{
					GameServerSender.Instance.SendEndTeleport(vector);
				}
				GameController.Instance.player.GetComponent<Rigidbody>().isKinematic = false;
				GameController.Instance.player.layer = 0;
				GameController.Instance.player.transform.position = vector;
				GameController.Instance.player.GetComponent<SharedCreature>().SetMoveTo(vector);
			}
			GameController.Instance.prev_player_pos = vector;
			CompanionController.Instance.MoveCompanionsToPlayerPosition();
			if (clear_mobs)
			{
				MobControl.Instance.ClearAllCreatures(true, true, true);
			}
			ChunkControl.Instance.UpdateTerrain(true);
			GameplayGUIControl.Instance.UpdateDistanceDisplay();
			ChunkControl.Instance.SavePlayerLocation();
			if (send && GameServerConnector.Instance.FullyInGame())
			{
				GameServerSender.Instance.SendChangeZone(new_zone_data.zone_name, vector, on_map_change);
			}
			if (new_zone_data.zone_name == "overworld")
			{
				GameController.Instance.SetLightAngleToOverworld();
				GameplayGUIControl.Instance.EnableTeleporterButton();
				AudioControl.Instance.SetMusicPitch(1f);
				ChunkControl.Instance.view_zoom = 1f;
			}
			else if (InventoryUtils.IsHouseObject(curr_zonedata.house_item.item_name))
			{
				GameController.Instance.SetLightAngleToIndoors();
				GameplayGUIControl.Instance.EnableTeleporterButton();
				AudioControl.Instance.SetMusicPitch(1f);
				if (curr_zonedata.house_item.item_name == "Warehouse")
				{
					ChunkControl.Instance.view_zoom = 0.73f;
				}
				else
				{
					ChunkControl.Instance.view_zoom = 0.67f;
				}
			}
			else if (InventoryUtils.IsCaveObject(curr_zonedata.house_item.item_name))
			{
				GameController.Instance.SetLightAngleToIndoors();
				if (CustomTeleporterControl.Instance.ShouldDisableTeleporterButton())
				{
					GameplayGUIControl.Instance.DisableTeleporterButton();
				}
				AudioControl.Instance.SetMusicPitch(0.6f);
				ChunkControl.Instance.view_zoom = 1f;
				ChunkControl.cave_define correspondingCave = ChunkControl.Instance.GetCorrespondingCave(curr_zonedata.house_item.item_name);
				RenderSettings.ambientLight = correspondingCave.color;
				GameController.Instance.directional_light.color = correspondingCave.color;
			}
			else if (InventoryUtils.IsHeavenDimension(curr_zonedata.house_item.item_name) || InventoryUtils.IsPureDimension(curr_zonedata.house_item.item_name) || curr_zonedata.house_item.item_name == "Pocket World Basement")
			{
				if (curr_zonedata.house_item.item_name != "Pocket World Basement")
				{
					GameController.Instance.SetLightAngleToOverworld();
				}
				else
				{
					GameController.Instance.SetLightAngleToIndoors();
				}
				GameplayGUIControl.Instance.EnableTeleporterButton();
				AudioControl.Instance.SetMusicPitch(1f);
				ChunkControl.Instance.view_zoom = 1f;
			}
			else if (InventoryUtils.IsHellDimension(curr_zonedata.house_item.item_name))
			{
				GameController.Instance.SetLightAngleToIndoors();
				GameplayGUIControl.Instance.EnableTeleporterButton();
				AudioControl.Instance.SetMusicPitch(0.6f);
				ChunkControl.Instance.view_zoom = 1f;
				RenderSettings.ambientLight = hell_col;
				GameController.Instance.directional_light.color = hell_col;
			}
			if (InventoryUtils.IsHeavenDimension(curr_zonedata.house_item.item_name))
			{
				GameController.Instance.no_fall_thru_floor.transform.localScale = Vector3.one * 50f;
			}
			else
			{
				GameController.Instance.no_fall_thru_floor.transform.localScale = Vector3.one * 6f;
			}
			GameController.Instance.GiveAllOverheads();
			GameController.Instance.EvalDaynight();
			on_zone_change_complete?.Invoke();
		};
		if (new_zone_data.zone_name == "overworld")
		{
			action();
		}
		else
		{
			CreateInteriorModel(curr_zonedata, action);
		}
	}

	private void DetermineExtrancePosition(string item_name, int item_rot, ref int entrance_chunkX, ref int entrance_chunkZ, ref int entrance_innerX, ref int entrance_innerZ)
	{
	}
}
