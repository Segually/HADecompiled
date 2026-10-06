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
		List<string> list = new List<string>();
		string[] files = System.IO.Directory.GetFiles(Application.dataPath + "/SYNCHRONOUS/TextFiles/" + DevBuildControl.quest_scenics_folder_);
		foreach (string path in files)
		{
			string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(path);
			string text = "";
			string[] array = System.IO.File.ReadAllLines(path);
			foreach (string text2 in array)
			{
				if (Startup.StringNullOrWhitespace(text2))
				{
					continue;
				}
				if (text == "")
				{
					if (text2[0] == '[')
					{
						int num = text2.IndexOf("=");
						int num2 = text2.IndexOf("(");
						if (InventoryUtils.UsesShackId(text2.Substring(num + 2, num2 - num - 3)))
						{
							text = fileNameWithoutExtension + " " + text2;
						}
					}
				}
				else if (text2.Contains("shack_id = *long* "))
				{
					int num3 = text2.IndexOf("shack_id = *long* ") + "shack_id = *long* ".Length;
					list.Add(text2.Substring(num3, text2.Length - num3) + " = " + text);
				}
			}
		}
		Startup.WriteOnlyIfChanged(System.IO.Path.Combine(Application.dataPath + "/SYNCHRONOUS/TextFiles/" + DevBuildControl.quest_scenics_folder_, "(Auto Gen) npc_home_data.txt"), list.ToArray());
	}

	public List<string> GetZoneTrail(string start_zone_name)
	{
		List<string> list = new List<string>();
		ZoneData zoneData = LoadZoneDataFromDisk(start_zone_name);
		for (int i = -15; zoneData.outer_item_zone != "overworld"; i++)
		{
			string outer_item_zone = zoneData.outer_item_zone;
			list.Add(outer_item_zone);
			zoneData = LoadZoneDataFromDisk(outer_item_zone);
			if (i == -1)
			{
				break;
			}
		}
		return list;
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
		string text = zone_name + "-zonedata";
		string zoneDataFilename = ChunkControl.GetZoneDataFilename(text);
		PlayerData.Instance.SetSlotShort("outer_item_rot", new_rot, zoneDataFilename, text);
		new_item.SaveToDisk(zoneDataFilename, "zone_item", text);
	}

	public void InitialAdjustCave(string zone_name, InventoryItem zone_house_item)
	{
		string text = zone_name + "-zonedata";
		string zoneDataFilename = ChunkControl.GetZoneDataFilename(text);
		switch (PlayerData.Instance.GetSlotShort("interior_model_innerX", zoneDataFilename, text))
		{
		case 9:
			PlayerData.Instance.SetSlotShort("interior_model_innerX", 8, zoneDataFilename, text);
			break;
		case 0:
			PlayerData.Instance.SetSlotShort("interior_model_innerX", 1, zoneDataFilename, text);
			break;
		}
		switch (PlayerData.Instance.GetSlotShort("interior_model_innerZ", zoneDataFilename, text))
		{
		case 9:
			PlayerData.Instance.SetSlotShort("interior_model_innerZ", 8, zoneDataFilename, text);
			break;
		case 0:
			PlayerData.Instance.SetSlotShort("interior_model_innerZ", 1, zoneDataFilename, text);
			break;
		}
		short slotShort = PlayerData.Instance.GetSlotShort("interior_model_chunkX", zoneDataFilename, text);
		short slotShort2 = PlayerData.Instance.GetSlotShort("interior_model_chunkZ", zoneDataFilename, text);
		ChunkGeneratorCaves.GenerateNewCaveSystem(zone_name, slotShort, slotShort2, zone_house_item).SaveToDisk(zone_name);
	}

	public bool PlayerZoneExists(string zone_name)
	{
		string text = zone_name + "-zonedata";
		return InventoryItem.LoadFromDisk("zone_item", ChunkControl.GetZoneDataFilename(text), text).item_name != "";
	}

	public void CreatePlayerZone(string new_zone_name, InventoryItem house_item, int interior_model_chunkX, int interior_model_chunkZ, int interior_model_innerX, int interior_model_innerZ, string outer_item_zone, int outer_item_rot)
	{
		new ZoneData(new_zone_name, house_item, outer_item_rot, outer_item_zone, interior_model_chunkX, interior_model_chunkZ, interior_model_innerX, interior_model_innerZ).SaveToDisk(new_zone_name);
	}

	public void SetInteriorRotation(int rot)
	{
		GameObject gameObject;
		switch (rot)
		{
		case 0:
			gameObject = curr_interior.GetComponent<HouseInteriorModel>().rotation0_doorpos;
			break;
		case 1:
			gameObject = curr_interior.GetComponent<HouseInteriorModel>().rotation1_doorpos;
			break;
		case 2:
			gameObject = curr_interior.GetComponent<HouseInteriorModel>().rotation2_doorpos;
			break;
		case 3:
			gameObject = curr_interior.GetComponent<HouseInteriorModel>().rotation3_doorpos;
			break;
		default:
			gameObject = null;
			break;
		}
		GameObject door_model = curr_interior.GetComponent<HouseInteriorModel>().door_model;
		door_model.transform.position = gameObject.transform.position;
		door_model.transform.rotation = gameObject.transform.rotation;
	}

	public void CreateInteriorModel(ZoneData zone_data, Action on_interior_model_complete)
	{
		float num = (float)zone_data.interior_model_chunkX * 10f + (float)zone_data.interior_model_innerX + 0.5f;
		float num2 = (float)zone_data.interior_model_chunkZ * 10f + (float)zone_data.interior_model_innerZ + 0.5f;
		Vector3 model_origin_pos = new Vector3(num - 1.8f, 0f, num2 + 1.6f);
		if (InventoryUtils.IsHouseObject(zone_data.house_item.item_name))
		{
			string text;
			switch (InventoryUtils.GetBuildingType(zone_data.house_item.item_name))
			{
			case InventoryUtils.building_type.shack:
				text = "SHACK_INTERIOR";
				break;
			case InventoryUtils.building_type.mansion:
				text = "MANSION_INTERIOR";
				break;
			case InventoryUtils.building_type.castle:
				text = "CASTLE_INTERIOR";
				break;
			case InventoryUtils.building_type.underground_room:
				text = "UNDERGROUND_INTERIOR";
				break;
			case InventoryUtils.building_type.upstairs_room:
				text = "UPSTAIRS_INTERIOR";
				break;
			case InventoryUtils.building_type.tent:
				text = "TENT_INTERIOR";
				break;
			case InventoryUtils.building_type.igloo:
				text = "IGLOO_INTERIOR";
				break;
			case InventoryUtils.building_type.windmill:
				text = "WINDMILL_INTERIOR";
				break;
			case InventoryUtils.building_type.warehouse:
				text = "WAREHOUSE_INTERIOR";
				break;
			default:
				text = "";
				break;
			}
			MakePlaneBlack();
			if (text != "")
			{
				ResourceControl.Instance.AsyncInstantiateHouseInterior(text, delegate(GameObject new_interior)
				{
					curr_interior = new_interior;
					if (curr_interior != null)
					{
						curr_interior.GetComponent<HouseInteriorModel>().exit_interactable.GetComponent<Interactable>().InitExit();
						curr_interior.transform.position = model_origin_pos;
						SetInteriorRotation(zone_data.outer_item_rot);
						ColorizerControl.Instance.ColorizeCurrentShackInterior();
					}
					on_interior_model_complete?.Invoke();
				});
				return;
			}
		}
		else if (InventoryUtils.IsCaveObject(zone_data.house_item.item_name))
		{
			if (zone_data.house_item.GetString("quest_miniworld") != "true")
			{
				curr_cave_exit = UnityEngine.Object.Instantiate(generic_cave_exit);
				curr_cave_exit.transform.position = new Vector3(num, 0f, num2);
				curr_cave_exit.GetComponent<Interactable>().InitExit();
			}
			MakePlaneBlack();
		}
		else if (InventoryUtils.IsHeavenDimension(zone_data.house_item.item_name))
		{
			curr_cave_exit = UnityEngine.Object.Instantiate(clouds_exit);
			curr_cave_exit.transform.position = new Vector3(num, 0f, num2);
			curr_cave_exit.GetComponent<Interactable>().InitExit();
		}
		else if (!InventoryUtils.IsPureDimension(zone_data.house_item.item_name) && !(zone_data.house_item.item_name == "Pocket World Basement") && InventoryUtils.IsHellDimension(zone_data.house_item.item_name))
		{
			curr_cave_exit = UnityEngine.Object.Instantiate(generic_cave_exit);
			curr_cave_exit.transform.position = new Vector3(num, 0f, num2);
			curr_cave_exit.GetComponent<Interactable>().InitExit();
		}
		on_interior_model_complete?.Invoke();
	}

	private void MakePlaneBlack()
	{
		GameController.Instance.breeder_floor_plane.SetActive(true);
		GameController.Instance.breeder_floor_plane.transform.position = new Vector3(GameController.Instance.prev_player_pos.x, 0f, GameController.Instance.prev_player_pos.z);
		Texture2D texture2D = new Texture2D(2, 2);
		Color[] array = new Color[4];
		for (int i = 0; i < 4; i++)
		{
			array[i] = new Color(0.09f, 0.09f, 0.09f, 1f);
		}
		texture2D.SetPixels(0, 0, 2, 2, array);
		texture2D.Apply();
		GameController.Instance.breeder_floor_plane.GetComponent<Renderer>().material.mainTexture = texture2D;
	}

	private Vector3 GetCurrDoorwayPosition(ZoneData zone_data)
	{
		if (InventoryUtils.IsHouseObject(zone_data.house_item.item_name))
		{
			if (curr_interior != null && curr_interior.GetComponent<HouseInteriorModel>() != null && curr_interior.GetComponent<HouseInteriorModel>().exit_interactable != null)
			{
				return curr_interior.GetComponent<HouseInteriorModel>().exit_interactable.transform.position;
			}
		}
		else if (InventoryUtils.IsCaveObject(zone_data.house_item.item_name) || InventoryUtils.IsHellDimension(zone_data.house_item.item_name))
		{
			if (curr_cave_exit != null)
			{
				return curr_cave_exit.transform.position;
			}
		}
		else if (InventoryUtils.IsHeavenDimension(zone_data.house_item.item_name) && curr_cave_exit != null)
		{
			return curr_cave_exit.transform.position + Vector3.right * 1.5f;
		}
		return Vector3.zero;
	}

	public Vector3 GetCurrExitPosition(ZoneData zone_data)
	{
		int entrance_chunkX = zone_data.house_item.GetShort("outer_item_chunkX");
		int entrance_chunkZ = zone_data.house_item.GetShort("outer_item_chunkZ");
		int entrance_innerX = zone_data.house_item.GetShort("outer_item_innerX");
		int entrance_innerZ = zone_data.house_item.GetShort("outer_item_innerZ");
		if (entrance_chunkZ == 0 && entrance_chunkX == 0 && entrance_innerZ == 0 && entrance_innerX == 0)
		{
			Debug.Log("EXIT DOES NOT EXIST");
			entrance_innerX = 0;
		}
		else
		{
			DetermineExtrancePosition(zone_data.house_item.item_name, zone_data.outer_item_rot, ref entrance_chunkX, ref entrance_chunkZ, ref entrance_innerX, ref entrance_innerZ);
		}
		return new Vector3((float)(entrance_innerX + entrance_chunkX * 10) + 0.5f, 0f, (float)(entrance_innerZ + entrance_chunkZ * 10) + 0.5f);
	}

	public void UpdateZoneItemOnChangedOutside()
	{
		if (InventoryUtils.IsHouseObject(Instance.curr_zonedata.house_item.item_name) && curr_interior != null)
		{
			SetInteriorRotation(Instance.curr_zonedata.outer_item_rot);
			ColorizerControl.Instance.ColorizeCurrentShackInterior();
		}
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
		if (item_name == "Upstairs Room")
		{
			switch (item_rot)
			{
			case 0:
				entrance_innerX--;
				if (entrance_innerX < 0)
				{
					entrance_innerX += 10;
					entrance_chunkX--;
				}
				break;
			case 1:
				entrance_innerZ++;
				if (entrance_innerZ >= 10)
				{
					entrance_innerZ -= 10;
					entrance_chunkZ++;
				}
				break;
			case 2:
				entrance_innerX++;
				if (entrance_innerX >= 10)
				{
					entrance_innerX -= 10;
					entrance_chunkX++;
				}
				break;
			case 3:
				entrance_innerZ--;
				if (entrance_innerZ < 0)
				{
					entrance_innerZ += 10;
					entrance_chunkZ--;
				}
				break;
			}
			return;
		}
		if (InventoryUtils.IsHeavenDimension(item_name) && !InventoryUtils.IsCaveObject(item_name))
		{
			entrance_innerX++;
			if (entrance_innerX >= 10)
			{
				entrance_innerX -= 10;
				entrance_chunkX++;
			}
			return;
		}
		if (!InventoryUtils.IsCaveObject(item_name))
		{
			if (item_name == "Underground Room")
			{
				return;
			}
			item_rot = item_rot switch
			{
				0 => 3,
				1 => 0,
				2 => 1,
				3 => 2,
				_ => -1,
			};
		}
		switch (item_rot)
		{
		case 0:
			entrance_innerZ -= 2;
			if (entrance_innerZ < 0)
			{
				entrance_innerZ += 10;
				entrance_chunkZ--;
			}
			break;
		case 1:
			entrance_innerX -= 2;
			if (entrance_innerX < 0)
			{
				entrance_innerX += 10;
				entrance_chunkX--;
			}
			break;
		case 2:
			entrance_innerZ += 2;
			if (entrance_innerZ >= 10)
			{
				entrance_innerZ -= 10;
				entrance_chunkZ++;
			}
			break;
		case 3:
			entrance_innerX += 2;
			if (entrance_innerX >= 10)
			{
				entrance_innerX -= 10;
				entrance_chunkX++;
			}
			break;
		}
	}
}
