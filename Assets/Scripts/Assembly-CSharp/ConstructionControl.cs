using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConstructionControl : MonoBehaviour, OrderedStart
{
	public enum build_context_t
	{
		on_regular_load = 0,
		on_self_build_new = 1,
		on_other_build_new = 2
	}

	public enum remove_context
	{
		self_remove = 0,
		other_remove = 1
	}

	public enum button_state
	{
		none = 0,
		DONE_USING_TOOL = 1,
		BUILD_NEW_OBJ = 2,
		CAST_PROJECTILE_AT_ENEMY = 3,
		CAST_PROJECTILE_AT_ALLY = 4,
		PICKING_CAST_CUSTOM_LOCATION = 5,
		MODIFY_OBJECT = 6,
		COMPANION_ATTACK = 7,
		COMPANION_MOVE = 8
	}

	public enum usage_context_t
	{
		world_object = 0,
		on_mouseObj_or_storeModel = 1,
		on_item_screenshot = 2
	}

	public enum object_geometry
	{
		undefined = 0,
		_1_by_1 = 1,
		wallshape = 2,
		_xplus1 = 3,
		rugshape = 4,
		shackshape = 5,
		_3_by_3 = 6,
		_2_by_2 = 7,
		_5_by_5 = 8
	}

	public static ConstructionControl Instance;

	public GameObject click_to_place;

	public Text click_to_place_txt;

	public Text click_to_place_BUTTON_text;

	public GameObject DONE_placing_button;

	public GameObject prefab_rotate_button;

	public Color col_checkmark_allowed;

	public Color col_checkmark_not_allowed;

	public Material mat_build_allowed;

	public Material mat_build_not_allowed;

	public Material mat_build_allowed_CUTOUT;

	public Material mat_build_not_allowed_CUTOUT;

	private int mouse_rot;

	public GameObject mouse_obj;

	public List<int> online_unique_ids_ = new List<int>();

	private bool creating_mouse_obj;

	public GameObject button_rotate_furniture;

	public button_state done_button_context;

	private object_geometry mouse_obj_geometry;

	private string edit_navpost_col;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
	}

	public void ClickAcceptBuild()
	{
	}

	private void TryAcceptBuild()
	{
	}

	public void RecycleUniqueIds(List<int> unique_ids)
	{
	}

	public int GetNewUniqueId(bool only_use_local_unique_ids = false)
	{
		if ((GameServerConnector.Instance.FullyInGame() ? GameServerConnector.Instance.is_host : true) || only_use_local_unique_ids)
		{
			short slotShort = PlayerData.Instance.GetSlotShort("n_recycled_unique_ids", PlayerData.filename_t.general);
			int num = slotShort - 1;
			if (slotShort < 1)
			{
				int slotLong = PlayerData.Instance.GetSlotLong("unique_id_iterator_new", PlayerData.filename_t.general);
				int value = ((slotLong != ChunkControl.dedicated_quest_range_start - 1) ? (slotLong + 1) : (ChunkControl.dedicated_quest_range_start + 1000));
				PlayerData.Instance.SetSlotLong("unique_id_iterator_new", value, PlayerData.filename_t.general);
				return slotLong;
			}
			int slotLong2 = PlayerData.Instance.GetSlotLong("recycled_unique_id_" + num, PlayerData.filename_t.general);
			PlayerData.Instance.SetSlotShort("n_recycled_unique_ids", num, PlayerData.filename_t.general);
			return slotLong2;
		}
		if (online_unique_ids_.Count < 2)
		{
			GameServerConnector.Instance.disconnected_string = "Error\nUniqueIDs exhausted";
			GameController.Instance.GoToMenu(true);
			int result = online_unique_ids_[0];
			online_unique_ids_.Clear();
			return result;
		}
		int num2 = online_unique_ids_[0];
		online_unique_ids_.RemoveAt(0);
		GameServerSender.Instance.SendUsedUniqueId(num2);
		if (online_unique_ids_.Count < 16 && !GameServerSender.Instance.requesting_unique_ids)
		{
			GameServerSender.Instance.RequestMoreUniqueIds();
			GameServerSender.Instance.requesting_unique_ids = true;
		}
		return num2;
	}

	public bool AutoReplaceAt(InventoryItem new_item, InventoryItem old_element_item, int old_element_rot, string zone, int chunkX, int chunkZ, int innerX, int innerZ, Action<GameObject> on_complete = null)
	{
		return false;
	}

	private void MouseObjPositionChanged(Vector3 rounded_clickedAt, bool on_enter_build_mode)
	{
	}

	public void RedrawStringLights(GameObject instance, InventoryItem item)
	{
		short @short = item.GetShort("model1_chunkX");
		short short2 = item.GetShort("model1_chunkZ");
		short short3 = item.GetShort("model1_innerX");
		short short4 = item.GetShort("model1_innerZ");
		GameObject gameObject = instance.transform.Find("model1").gameObject;
		GameObject gameObject2 = instance.transform.Find("model2").gameObject;
		LineRenderer component = instance.GetComponent<LineRenderer>();
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < instance.transform.childCount; i++)
		{
			Transform child = instance.transform.GetChild(i);
			if (child.gameObject.name.Contains("bulb") && child.gameObject.name != "bulb1" && child.gameObject.name != "bulb2" && child.gameObject.name != "bulb3")
			{
				list.Add(child.gameObject);
			}
		}
		foreach (GameObject item2 in list)
		{
			UnityEngine.Object.Destroy(item2);
		}
		if (@short == 0 && short2 == 0 && short3 == 0 && short4 == 0)
		{
			component.enabled = false;
			gameObject.transform.localPosition = Vector3.zero;
			return;
		}
		component.enabled = true;
		gameObject.transform.SetParent(null);
		gameObject2.transform.SetParent(null);
		gameObject.transform.position = new Vector3((float)(short3 + @short * 10) + 0.5f, 0f, (float)(short4 + short2 * 10) + 0.5f);
		float num = Vector3.Distance(gameObject.transform.position, gameObject2.transform.position) * 2f;
		int num2 = (int)num;
		if (num2 < 6)
		{
			num2 = 5;
		}
		component.positionCount = num2;
		int num3 = ((item.item_name == "String Lights") ? 1 : ((item.item_name == "Holiday Lights") ? 3 : ((item.item_name == "Blue String Lights" || item.item_name == "Red String Lights") ? 2 : 0)));
		int num4 = 0;
		for (int j = 0; j < num2; j++)
		{
			Vector3 position = gameObject.transform.position;
			Vector3 position2 = gameObject2.transform.position;
			float num5 = (float)j / (float)(num2 - 1);
			float y = ((num5 - 0.5f) * (num5 - 0.5f) + 0.75f) * 2.5f;
			float num6 = Mathf.Clamp01(num5);
			Vector3 vector = new Vector3(position.x + num6 * (position2.x - position.x), y, position.z + num6 * (position2.z - position.z));
			component.SetPosition(j, vector);
			if (j != 0 && num2 - 1 != j)
			{
				int num7 = 0;
				if (num4 + 1 != num3)
				{
					num7 = num4 + 1;
				}
				GameObject obj = UnityEngine.Object.Instantiate(instance.transform.Find("bulb" + (num7 + 1)).gameObject);
				obj.transform.position = vector;
				obj.transform.SetParent(instance.transform);
				obj.SetActive(true);
				num4 = num7;
			}
		}
		gameObject.transform.SetParent(instance.transform);
		gameObject2.transform.SetParent(instance.transform);
	}

	private bool AllowedToPlace(Vector3 rounded_clickedAt, InventoryItem item)
	{
		return false;
	}

	private void Update()
	{
	}

	private void ConvertToGlow(Transform T, bool allowed)
	{
	}

	private void CreateMouseObj(bool on_modify_position, byte start_rot, Vector3 start_pos)
	{
	}

	public void EnterBuildMode(InventoryItem item, Vector3 mouse_obj_start_pos, byte start_rot, bool on_modify_position)
	{
	}

	public void RotateMouseObj()
	{
	}

	private void FixedUpdate()
	{
	}

	public void GrabFurniture(ChunkElement element, string zone, int item_chunkX, int item_chunkZ, int item_innerX, int item_innerZ, int mouse_chunkX, int mouse_chunkZ, int mouse_innerX, int mouse_innerZ)
	{
	}

	public void DonePlacing(bool manual_press, bool unpause_game)
	{
	}

	public void EndDelete()
	{
	}

	public void EnterToolMode(InventoryItem tool)
	{
	}

	public void UseToolClick(Vector3 clickedAt)
	{
	}

	private bool HasToolUseResultByStatus(ToolUseResult.status looking_for, List<ToolUseResult> error_codes, ref ToolUseResult print)
	{
		return false;
	}

	public void ShowDoneButton(string str, button_state context)
	{
	}

	public void DeleteMouseObject()
	{
	}

	public void SnapPainting(GameObject instance, int chunkX, int chunkZ, int innerX, int innerZ)
	{
		Transform parent = instance.transform.parent;
		instance.transform.SetParent(null);
		instance.transform.rotation = Quaternion.identity;
		instance.transform.SetParent(parent);
		int num = 0;
		if (ChunkControl.Instance.player_zone != "overworld")
		{
			List<Vector3> list = new List<Vector3>();
			List<Vector3> list2 = new List<Vector3>();
			List<Vector3> list3 = new List<Vector3>();
			List<Vector3> list4 = new List<Vector3>();
			ZoneData curr_zonedata = ZoneDataControl.Instance.curr_zonedata;
			switch (InventoryUtils.GetBuildingType(curr_zonedata.house_item.item_name))
			{
			case InventoryUtils.building_type.shack:
			case InventoryUtils.building_type.igloo:
				list = InventoryUtils.GetShackLeftWalls();
				list2 = InventoryUtils.GetShackRightWalls();
				list3 = InventoryUtils.GetShackForwardWalls();
				list4 = InventoryUtils.GetShackBackWalls();
				break;
			case InventoryUtils.building_type.mansion:
				list = InventoryUtils.GetMansionLeftWalls();
				list2 = InventoryUtils.GetMansionRightWalls();
				list3 = InventoryUtils.GetMansionForwardWalls();
				list4 = InventoryUtils.GetMansionBackWalls();
				break;
			case InventoryUtils.building_type.castle:
				list = InventoryUtils.GetCastleLeftWalls();
				list2 = InventoryUtils.GetCastleRightWalls();
				list3 = InventoryUtils.GetCastleForwardWalls();
				list4 = InventoryUtils.GetCastleBackWalls();
				break;
			case InventoryUtils.building_type.underground_room:
			case InventoryUtils.building_type.upstairs_room:
			case InventoryUtils.building_type.tent:
				list = InventoryUtils.GetUndergroundLeftWalls();
				list2 = InventoryUtils.GetUndergroundRightWalls();
				list3 = InventoryUtils.GetUndergroundForwardWalls();
				list4 = InventoryUtils.GetUndergroundBackWalls();
				break;
			case InventoryUtils.building_type.windmill:
				list = InventoryUtils.GetWindmillLeftWalls();
				list2 = InventoryUtils.GetWindmillRightWalls();
				list3 = InventoryUtils.GetWindmillForwardWalls();
				list4 = InventoryUtils.GetWindmillBackWalls();
				break;
			case InventoryUtils.building_type.warehouse:
				list = InventoryUtils.GetWarehouseLeftWalls();
				list2 = InventoryUtils.GetWarehouseRightWalls();
				list3 = InventoryUtils.GetWarehouseForwardWalls();
				list4 = InventoryUtils.GetWarehouseBackWalls();
				break;
			}
			float num2 = (float)(innerX + chunkX * 10) + 0.5f - ((float)(curr_zonedata.interior_model_innerX + curr_zonedata.interior_model_chunkX * 10) + 0.5f);
			float num3 = (float)(innerZ + chunkZ * 10) + 0.5f - ((float)(curr_zonedata.interior_model_innerZ + curr_zonedata.interior_model_chunkZ * 10) + 0.5f);
			Vector3 vector = new Vector3(num2, 0f, num3);
			foreach (Vector3 item in list)
			{
				if (Vector3.Distance(item, vector) < 0.1f)
				{
					num = 1;
					break;
				}
			}
			if (num == 0)
			{
				foreach (Vector3 item2 in list2)
				{
					if (Vector3.Distance(item2, vector) < 0.1f)
					{
						num = 2;
						break;
					}
				}
			}
			if (num == 0)
			{
				foreach (Vector3 item3 in list3)
				{
					if (Vector3.Distance(item3, vector) < 0.1f)
					{
						num = 3;
						break;
					}
				}
			}
			if (num == 0)
			{
				foreach (Vector3 item4 in list4)
				{
					if (Vector3.Distance(item4, vector) < 0.1f)
					{
						num = 4;
						break;
					}
				}
			}
		}
		if (num == 0)
		{
			if (CheckForCustomBuiltHangableWall(chunkX, chunkZ, innerX, innerZ, -1, 0))
			{
				num = 1;
			}
			else if (CheckForCustomBuiltHangableWall(chunkX, chunkZ, innerX, innerZ, 1, 0))
			{
				num = 2;
			}
			else if (CheckForCustomBuiltHangableWall(chunkX, chunkZ, innerX, innerZ, 0, 1))
			{
				num = 3;
			}
			else if (CheckForCustomBuiltHangableWall(chunkX, chunkZ, innerX, innerZ, 0, -1))
			{
				num = 4;
			}
		}
		Vector3 localPosition;
		Quaternion localRotation;
		Vector3 localPosition2;
		switch (num)
		{
		case 1:
			localPosition = new Vector3(-0.475f, 0.75f, 0f);
			localRotation = Quaternion.Euler(-180f, -90f, 90f);
			localPosition2 = new Vector3(-0.5f, 0f, 0f);
			break;
		case 2:
			localPosition = new Vector3(0.475f, 0.75f, 0f);
			localRotation = Quaternion.Euler(-180f, 90f, 90f);
			localPosition2 = new Vector3(0.5f, 0f, 0f);
			break;
		case 3:
			localPosition = new Vector3(0f, 0.75f, 0.475f);
			localRotation = Quaternion.Euler(-180f, 0f, 90f);
			localPosition2 = new Vector3(0f, 0f, 0.5f);
			break;
		case 4:
			localPosition = new Vector3(0f, 0.75f, -0.475f);
			localRotation = Quaternion.Euler(-180f, -180f, 90f);
			localPosition2 = new Vector3(0f, 0f, -0.5f);
			break;
		default:
			localPosition = Vector3.zero;
			localRotation = Quaternion.Euler(-90f, 0f, 0f);
			localPosition2 = new Vector3(0f, 0f, 0f);
			break;
		}
		instance.transform.Find("models").localPosition = localPosition;
		instance.transform.Find("models").localRotation = localRotation;
		instance.transform.Find("Interactable").localPosition = localPosition2;
	}

	private bool CheckForCustomBuiltHangableWall(int chunkX, int chunkZ, int innerX, int innerZ, int modX, int modZ)
	{
		return false;
	}

	public void AdjustBuildableInstance(GameObject instance, InventoryItem item, usage_context_t adjust_context, Action adjusted_callback = null)
	{
		AdjustBuildableInstance(instance, item, adjust_context, "", -1, -1, -1, -1, build_context_t.on_regular_load, null, null, null, 0, adjusted_callback);
	}

	public void AdjustBuildableInstance(GameObject instance, InventoryItem item, usage_context_t usage_context, string zone, int chunkX, int chunkZ, int innerX, int innerZ, build_context_t build_context, Chunk chunk, ChunkObj chunkObj, ChunkData chunk_data, int rot, Action adjusted_callback = null)
	{
		if (instance == null)
		{
			return;
		}
		string chunkString = ChunkControl.Instance.GetChunkString(zone, chunkX, chunkZ);
		bool flag = false;
		if (item.item_name.Contains("Mob - ") && !(item.item_name == "Mob - Tiny") && !(item.item_name == "Mob - Normal") && !(item.item_name == "Mob - Big") && !(item.item_name == "Mob - Giant"))
		{
			CreatureStruct creatureStructFromMobFile = MobControl.Instance.GetCreatureStructFromMobFile(MobControl.Instance.ItemToMobFileName(item), chunk_data, innerX, innerZ, item, rot);
			if (usage_context == usage_context_t.world_object && !DevBuildControl.Instance.view_dev_objects)
			{
				MobControl.Instance.TryInstantiateMob(creatureStructFromMobFile, item, rot, chunkObj);
				goto IL_tail;
			}
		}
		switch (item.item_name)
		{
		case "String Lights":
		case "Holiday Lights":
		case "Red String Lights":
		case "Blue String Lights":
			if (usage_context == usage_context_t.world_object)
			{
				RedrawStringLights(instance, item);
				goto IL_tail;
			}
			goto IL_6d4;
		case "Beehive":
		case "Spiderhive":
			if (usage_context == usage_context_t.world_object)
			{
				if (item.GetString("tag") == "natural" && !item.HasActiveRespawn("mob_spawn"))
				{
					MobControl.Instance.TryInstantiateMobMiniCluster((item.item_name == "Beehive") ? "wasp" : "spider", chunk_data, innerX, innerZ, item, rot, chunkObj);
				}
				goto IL_tail;
			}
			goto IL_6d4;
		case "Cave Chest":
		case "Loot Chest":
		case "Loot Basket":
		case "Titanium Chest":
		case "Boss Chest":
		case "Gold Chest":
		{
			if (usage_context != usage_context_t.world_object)
			{
				goto IL_6d4;
			}
			bool flag2;
			if (!item.HasActiveRespawn("loot_spawn"))
			{
				if (item.GetString("bandit_camp_instance") == "")
				{
					goto IL_tail;
				}
				BanditCampInstance banditCampInstanceByName = BanditCampsControl.Instance.GetBanditCampInstanceByName(item.GetString("bandit_camp_instance"));
				flag2 = false;
				flag = false;
				if (!banditCampInstanceByName.flag_destroyed)
				{
					goto IL_700;
				}
			}
			else
			{
				flag2 = true;
			}
			if (item.item_name == "Gold Chest" || item.item_name == "Loot Basket" || item.item_name == "Titanium Chest" || item.item_name == "Loot Chest" || item.item_name == "Boss Chest")
			{
				UnityEngine.Object.Destroy(instance.transform.Find("Particle System").gameObject);
			}
			if (item.item_name == "Gold Chest" || item.item_name == "Cave Chest")
			{
				instance.transform.Find("lid").localPosition = new Vector3(-0.335f, 0.869f, 0.038f);
				instance.transform.Find("lid").localRotation = Quaternion.Euler(-220f, 90f, 0f);
			}
			else if (item.item_name == "Titanium Chest" || item.item_name == "Loot Chest")
			{
				instance.transform.Find("lid").localPosition = new Vector3(0.045f, 0.588f, 0.038f);
				instance.transform.Find("lid").localRotation = Quaternion.Euler(-196f, 90f, 0f);
			}
			else if (item.item_name == "Boss Chest")
			{
				instance.transform.Find("lid").localPosition = new Vector3(0.12f, 1.77f, 0.54f);
				instance.transform.Find("lid").localRotation = Quaternion.Euler(-220f, 90f, 0f);
			}
			if (flag2)
			{
				chunkObj.AddRespawnWatcher(innerX, innerZ, item, rot, "loot_spawn");
			}
			goto IL_tail;
		}
		case "Metal Lamp Post":
		case "Red Torch":
		case "Stone Lantern":
		case "Torch":
		case "Campfire":
		case "Lamp Post":
		case "Fireplace":
		case "Fancy Torch":
		case "Blue Torch":
		{
			if (usage_context != usage_context_t.world_object)
			{
				goto IL_6d4;
			}
			GameObject gameObject = instance.transform.Find("Glow").gameObject;
			if (!GraphicsControl.Instance.ShowLightingEffects())
			{
				UnityEngine.Object.Destroy(gameObject);
			}
			else
			{
				GameController.Instance.torch_glows.Add(gameObject);
			}
			if (item.item_name != "Fireplace")
			{
				goto IL_tail;
			}
			instance.GetComponent<AudioSource>().volume = AudioControl.Instance.general_sfx_volume * 0.25f;
			instance.GetComponent<AudioSource>().Play();
			goto IL_tail;
		}
		case "Grandfather Clock":
			if (usage_context != usage_context_t.world_object)
			{
				goto IL_6d4;
			}
			instance.GetComponent<AudioSource>().volume = AudioControl.Instance.general_sfx_volume;
			instance.GetComponent<AudioSource>().Play();
			goto IL_tail;
		case "Mob - Normal":
		case "Mob - Big":
		case "Mob - Tiny":
		case "Mob - Giant":
		{
			if (usage_context != usage_context_t.world_object)
			{
				goto IL_6d4;
			}
			if (!DevBuildControl.Instance.view_dev_objects)
			{
				float size = 1f;
				float level_mod = 1f;
				if (item.item_name == "Mob - Big")
				{
					size = 2f;
					level_mod = 1.25f;
				}
				else if (item.item_name == "Mob - Tiny")
				{
					size = 0.5f;
					level_mod = 0.75f;
				}
				else if (item.item_name == "Mob - Giant")
				{
					size = 3f;
					level_mod = 1.5f;
				}
				MobControl.Instance.TryInstantiateMob(CreatureStruct.GenerateNewWildMob(chunk_data, innerX, innerZ, size, level_mod, item, rot), item, rot, chunkObj);
			}
			goto IL_tail;
		}
		case "Bonsai Tree":
			if (usage_context < usage_context_t.on_item_screenshot)
			{
				short @short = item.GetShort("bonsai_age");
				if (@short < 101)
				{
					goto IL_default;
				}
				float bonsaiScale = InventoryUtils.GetBonsaiScale(@short);
				instance.transform.localScale = Vector3.one * bonsaiScale;
				instance.GetComponent<Interactable>().circle_size = bonsaiScale - 1f - 0.3f + 1f;
			}
			goto IL_6d4;
		case "Companion":
		{
			if (item.GetString("companion_mode") == "guard")
			{
				if (usage_context == usage_context_t.world_object)
				{
					string @string = item.GetString("creature_A");
					string string2 = item.GetString("creature_B");
					string text = item.GetString("npc_display_name");
					int @long = item.GetLong("level");
					InventoryItem hat;
					InventoryItem body;
					InventoryItem hand;
					if (item.GetString("tag") != "dev_obj")
					{
						ItemCountPair[] itemListFromItem = ChunkControl.Instance.GetItemListFromItem("pockets", item);
						hat = itemListFromItem[3].item;
						body = itemListFromItem[8].item;
						hand = itemListFromItem[13].item;
					}
					else
					{
						text = TranslationControl.Instance.TranslateGeneral(text, "CompanionsEtc");
						ExtraInventoryData extraInventoryData = new ExtraInventoryData();
						extraInventoryData.SetString("paint", item.GetString("hat_paint"));
						hat = new InventoryItem(item.GetString("hat"), extraInventoryData);
						ExtraInventoryData extraInventoryData2 = new ExtraInventoryData();
						extraInventoryData2.SetString("paint", item.GetString("armor_paint"));
						body = new InventoryItem(item.GetString("body"), extraInventoryData2);
						ExtraInventoryData extraInventoryData3 = new ExtraInventoryData();
						extraInventoryData3.SetString("paint", item.GetString("hand_paint"));
						hand = new InventoryItem(item.GetString("wep"), extraInventoryData3);
					}
					MobControl.Instance.TryInstantiateMob(CreatureStruct.GenerateGuardMob(chunk_data, innerX, innerZ, @string, string2, hat, body, hand, text, @long, item, rot), item, rot, chunkObj);
					goto IL_tail;
				}
				goto IL_callback;
			}
			CreateMannequin(instance.transform.Find("creature-go-here"), item);
			string string3 = item.GetString("companion_mode");
			if (string3 == "wait")
			{
				if (usage_context == usage_context_t.world_object)
				{
					TryMakeCompanionSitOnChair(chunkObj, chunk_data, innerX, innerZ, item, instance);
				}
			}
			else if (string3 == "merchant" && usage_context == usage_context_t.world_object)
			{
				TryMakeCompanionSitOnChair(chunkObj, chunk_data, innerX, innerZ, item, instance);
			}
			if (string3 != "wait")
			{
				if (usage_context != usage_context_t.world_object || string3 != "merchant")
				{
					goto IL_6d4;
				}
				instance.GetComponent<Interactable>().AssignOverheadIcon(DevBuildControl.Instance.overhead_logos[1]);
				goto IL_tail;
			}
			if (usage_context != usage_context_t.world_object)
			{
				goto IL_callback;
			}
			int num = CompanionController.WaitIconIdToStateIconId(item.GetShort("npc_icon"));
			instance.GetComponent<Interactable>().AssignOverheadIcon(DevBuildControl.Instance.overhead_logos[num]);
			goto IL_tail;
		}
		case "DEBUG-npc":
		{
			if (usage_context != usage_context_t.world_object)
			{
				goto IL_6d4;
			}
			CreateMannequin(instance.transform.Find("creature-go-here"), item);
			TryMakeCompanionSitOnChair(chunkObj, chunk_data, innerX, innerZ, item, instance);
			int iconId = GetIconId(item.GetString("npc_icon"));
			int iconId2 = GetIconId(item.GetString("npc_icon2"));
			Sprite set_icon = DevBuildControl.Instance.overhead_logos[iconId];
			Sprite set_icon_ = ((iconId2 != 0) ? DevBuildControl.Instance.overhead_logos[iconId2] : null);
			instance.GetComponent<Interactable>().AssignOverheadIcon(set_icon, set_icon_);
			goto IL_tail;
		}
		case "Magic Bean":
			if ((build_context == build_context_t.on_self_build_new || build_context == build_context_t.on_other_build_new) && usage_context == usage_context_t.world_object)
			{
				instance.transform.Find("animated model").GetComponent<Animation>().Play();
				goto IL_tail;
			}
			goto IL_6d4;
		case "3-day Land Claim":
		case "8-day Land Claim":
		case "Admin Land Claim":
			if (usage_context != usage_context_t.world_object)
			{
				goto IL_6d4;
			}
			if (build_context == build_context_t.on_self_build_new || build_context == build_context_t.on_other_build_new)
			{
				GameObject original;
				if (item.item_name == "3-day Land Claim")
				{
					original = inventory_ctr.Instance.land_claim_particle_prefab;
				}
				else
				{
					if (!(item.item_name == "8-day Land Claim") && !(item.item_name == "Admin Land Claim"))
					{
						throw new NullReferenceException();
					}
					original = inventory_ctr.Instance.land_claim_particle_prefab_2;
				}
				UnityEngine.Object.Instantiate(original).transform.position = new Vector3((float)(innerX + chunkX * 10) + 0.5f, 0f, (float)(innerZ + chunkZ * 10) + 0.5f);
				AudioControl.Instance.PlayPitch(GameController.Instance.sfx_landclaim, 1f, 1f);
				if (item.item_name == "3-day Land Claim")
				{
					GameplayGUIControl.Instance.ShowNotif("<color=#7ad9ff>Land Claimed!</color>", item, 1, new OnNotifClick(OnNotifClick.type.none));
				}
				else if (item.item_name == "8-day Land Claim")
				{
					GameplayGUIControl.Instance.ShowNotif("<color=#80ff8e>Land Claimed!</color>", item, 1, new OnNotifClick(OnNotifClick.type.none));
				}
				else if (item.item_name == "Admin Land Claim")
				{
					GameplayGUIControl.Instance.ShowNotif("<color=#fffd7a>Land Claimed!</color>", item, 1, new OnNotifClick(OnNotifClick.type.none));
				}
			}
			chunkObj.AddRespawnWatcher(innerX, innerZ, item, rot, "landclaim_spawn");
			goto IL_tail;
		case "Painting":
		{
			if (usage_context == usage_context_t.world_object)
			{
				SnapPainting(instance, chunkX, chunkZ, innerX, innerZ);
			}
			AnimatedPainting component = instance.GetComponent<AnimatedPainting>();
			component.frame_1_mesh.enabled = true;
			component.frame_2_mesh.enabled = false;
			if (item.GetShort("dev_painting_id") == 0)
			{
				component.AssignTexture1(inventory_ctr.Instance.LoadPaintingFrame(item, "n_bytes_frame_1", "b1-"));
				if (usage_context != usage_context_t.world_object)
				{
					goto IL_callback;
				}
				component.AssignTexture2(inventory_ctr.Instance.LoadPaintingFrame(item, "n_bytes_frame_2", "b2-"));
				component.speed = item.GetShort("painting_speed");
				component.Animate();
				goto IL_tail;
			}
			short short2 = item.GetShort("dev_painting_id");
			string painting_filename = DevBuildControl.Instance.NPC_paintings[short2].painting_filename;
			short speed = DevBuildControl.Instance.NPC_paintings[short2].speed;
			ResourceControl.Instance.AssignPainting(painting_filename + "0", component.frame_1_mesh, adjusted_callback);
			if (usage_context != usage_context_t.world_object)
			{
				return;
			}
			ResourceControl.Instance.AssignPainting(painting_filename + "1", component.frame_2_mesh);
			component.speed = speed;
			component.Animate();
			flag = true;
			goto IL_700;
		}
		case "Teleporter":
		{
			int hardcodedTeleId = CustomTeleporterControl.Instance.GetHardcodedTeleId(chunkString, innerX, innerZ);
			int colorID;
			if (hardcodedTeleId == -1)
			{
				colorID = item.GetShort("col_id");
			}
			else
			{
				string name = CustomTeleporterControl.Instance.hardcoded_teleports[hardcodedTeleId].name;
				colorID = ((name != "Noobia" && PlayerData.Instance.GetSlotShort("discovered_" + name, PlayerData.filename_t.general) == 0) ? 5 : 0);
			}
			CustomTeleporterControl.Instance.ColorizeTeleporter_(instance, colorID);
			goto IL_default;
		}
		case "Gravestone":
			if (usage_context != usage_context_t.world_object)
			{
				goto IL_6d4;
			}
			if (item.GetShort("has_ghost") == 1)
			{
				InventoryItem companion_item = item.LoadSubItem("ghost");
				int spawn_offset_x = 0;
				int spawn_offset_z = 0;
				switch (rot)
				{
				case 0:
					spawn_offset_x = 1;
					spawn_offset_z = 0;
					break;
				case 1:
					spawn_offset_x = 0;
					spawn_offset_z = -1;
					break;
				case 2:
					spawn_offset_x = -1;
					spawn_offset_z = 0;
					break;
				case 3:
					spawn_offset_x = 0;
					spawn_offset_z = 1;
					break;
				}
				MobControl.Instance.TryInstantiateMob(CreatureStruct.GenerateCompanionGhost(companion_item, item, chunk_data, rot, innerX, innerZ, spawn_offset_x, spawn_offset_z), item, rot, chunkObj);
			}
			goto IL_tail;
		case "Large Weapon Display":
			DecorateLargeWeaponDisplay(item, instance, adjusted_callback);
			flag = true;
			goto IL_6d4;
		case "Weapon Display":
			DecorateWeaponDisplay(item, instance, adjusted_callback);
			flag = true;
			goto IL_6d4;
		case "Armor Display":
		case "Custom Statue":
		{
			Transform transform = instance.transform.Find("creature-go-here");
			CreateMannequin(transform, item, adjusted_callback);
			if (usage_context == usage_context_t.on_item_screenshot)
			{
				transform.GetChild(0).gameObject.GetComponent<LiteModel>().AnimateAll();
				return;
			}
			flag = true;
			goto IL_6d4;
		}
		case "Boss Spawner - Yandeon":
		case "Boss Spawner - Shindeon":
			if (usage_context != usage_context_t.world_object)
			{
				goto IL_6d4;
			}
			MobControl.Instance.TryInstantiateMob(MobControl.Instance.GetCreatureStructFromMobFile(MobControl.Instance.ItemToMobFileName(item), chunk_data, innerX, innerZ, item, rot), item, rot, chunkObj);
			goto IL_tail;
		case "Creature Nest":
			if (usage_context != usage_context_t.world_object)
			{
				goto IL_6d4;
			}
			instance.transform.Find("egg").gameObject.GetComponent<MeshRenderer>().material.color = MobControl.Instance.GetOverheadNameColor(chunk_data.biome_mobA + chunk_data.biome_mobA);
			if (ZoneDataControl.Instance.curr_zonedata.house_item.GetString("quest_miniworld") != "true")
			{
				MobControl.Instance.TryInstantiateMob(CreatureStruct.GenerateNestMob(chunk_data, innerX, innerZ, item, rot), item, rot, chunkObj);
			}
			goto IL_tail;
		}
		IL_default:
		flag = false;
		IL_6d4:
		if (usage_context != usage_context_t.world_object)
		{
			if (adjusted_callback != null && !flag)
			{
				adjusted_callback();
			}
			return;
		}
		goto IL_700;
		IL_callback:
		if (adjusted_callback != null)
		{
			adjusted_callback();
		}
		return;
		IL_tail:
		flag = false;
		IL_700:
		bool itemBool = inventory_ctr.Instance.GetItemBool(item.item_name, "is_flooring_obj");
		bool itemBool2 = inventory_ctr.Instance.GetItemBool(item.item_name, "is_wall_obj");
		if (itemBool || itemBool2)
		{
			if (build_context == build_context_t.on_self_build_new || build_context == build_context_t.on_other_build_new)
			{
				if (itemBool)
				{
					ModularObjectControl.Instance.ModularChangedAt(innerX, innerZ, ModularObjectControl.type.PATHWAYS, zone, chunkX, chunkZ);
				}
				else if (itemBool2)
				{
					ModularObjectControl.Instance.ModularChangedAt(innerX, innerZ, ModularObjectControl.type.WALLS, zone, chunkX, chunkZ);
				}
			}
			UnityEngine.Object.Destroy(instance);
			if (adjusted_callback != null && !flag)
			{
				adjusted_callback();
			}
			return;
		}
		if ((InventoryUtils.IsChairObject(item.item_name) || InventoryUtils.IsBedObject(item.item_name)) && build_context == build_context_t.on_regular_load)
		{
			foreach (ChunkElement item2 in chunk_data.GetElementsAt(innerX, innerZ))
			{
				if (item2.item.item_name == "DEBUG-npc" || (item2.item.item_name == "Companion" && item2.item.GetString("companion_mode") != "guard"))
				{
					GameObject buildableInstanceByItem = chunkObj.GetBuildableInstanceByItem(innerX, innerZ, item2.item);
					if (buildableInstanceByItem != null)
					{
						TryMakeCompanionSitOnChair(chunkObj, chunk_data, innerX, innerZ, item2.item, buildableInstanceByItem, instance);
					}
				}
			}
			if (GameServerConnector.Instance.FullyInGame())
			{
				string text2 = zone + "," + chunkX + "," + chunkZ + "," + innerX + "," + innerZ;
				foreach (KeyValuePair<string, OnlinePlayer> nearby_player in GameServerInterface.Instance.nearby_players)
				{
					if (nearby_player.Value.sitting_in_chair == text2 && nearby_player.Value.obj != null)
					{
						nearby_player.Value.obj.GetComponent<SharedCreature>().TrySitInChairObj(text2);
					}
				}
			}
		}
		if (adjusted_callback != null && !flag)
		{
			adjusted_callback();
		}
	}

	public void PlayerBuildAt(InventoryItem build_item, string zone, int chunkX, int chunkZ, int innerX, int innerZ, int build_rot, build_context_t build_context, string builder_player, string mp_cache_key)
	{
	}

	public static string GenerateCacheKey()
	{
		return null;
	}

	public void PlayerRemoveAt(ChunkElement remove_element, string zone, int chunkX, int chunkZ, int innerX, int innerZ, remove_context remove_context_t, string mp_cache_key)
	{
	}

	private int GetIconId(string icon_name)
	{
		if (icon_name == "Merchant")
		{
			return 1;
		}
		if (icon_name == "Happy_face")
		{
			return 2;
		}
		if (icon_name == "Info_giver")
		{
			return 3;
		}
		if (icon_name == "Quest")
		{
			return 4;
		}
		return 0;
	}

	public void PlayerReplaceAt(InventoryItem new_item, InventoryItem old_element_item, int old_element_rot, string zone, int chunkX, int chunkZ, int innerX, int innerZ, bool send, string mp_cache_key)
	{
	}

	public void AsyncCreateBuildableInstance(InventoryItem item, int innerX, int innerZ, int rot, build_context_t build_context, Chunk chunk, ChunkData chunk_data, ChunkObj chunkObj, Action<GameObject> on_complete = null)
	{
		ChunkControl.Instance.GetChunkString(chunk_data.zone, chunk_data.X, chunk_data.Z);
		Vector3 position = new Vector3((float)(innerX + chunk_data.X * 10) + 0.5f, 0f, (float)(innerZ + chunk_data.Z * 10) + 0.5f);
		chunkObj.FillBuildableGeometry(item, position, rot);
		GameObject check_not_null = chunkObj.parent_obj;
		Action<GameObject> action = delegate(GameObject instance)
		{
			string text = "";
			if (check_not_null == null)
			{
				UnityEngine.Object.Destroy(instance);
				return;
			}
			if (!chunkObj.DoesBuildableInstanceAlreadyExist(innerX, innerZ, item))
			{
				Combatant combatant = instance.GetComponent<Combatant>();
				bool flag = false;
				if (combatant == null && instance.transform.Find("Combatant") != null)
				{
					combatant = instance.transform.Find("Combatant").GetComponent<Combatant>();
					flag = true;
				}
				if (combatant != null)
				{
					if (item.HasActiveRespawn("mob_spawn"))
					{
						chunkObj.AddRespawnWatcher(innerX, innerZ, item, rot, "mob_spawn");
						goto IL_destroy;
					}
					if (MobControl.Instance.active_combatants.ContainsKey(chunk_data.zone + "," + chunk_data.X + "," + chunk_data.Z + "," + innerX + "," + innerZ))
					{
						Debug.Log("Error: Bug averted for 'Combatants' (not great solution) - " + item.item_name);
						goto IL_destroy;
					}
				}
				Collectible collectible = instance.GetComponent<Collectible>();
				if (collectible == null && instance.transform.Find("Collectible") != null)
				{
					collectible = instance.transform.Find("Collectible").GetComponent<Collectible>();
				}
				if (collectible != null && collectible.on_pickup == Collectible.pickup_type_t.destroy)
				{
					if (item.HasActiveRespawn("collect_spawn"))
					{
						chunkObj.AddRespawnWatcher(innerX, innerZ, item, rot, "collect_spawn");
						goto IL_destroy;
					}
					if (ChunkControl.Instance.active_interactibles.ContainsKey(chunk_data.zone + "," + chunk_data.X + "," + chunk_data.Z + "," + innerX + "," + innerZ))
					{
						Debug.Log("Error: Bug averted for 'Collectibles' (not great solution) - " + item.item_name);
						goto IL_destroy;
					}
				}
				instance.transform.position = position;
				instance.transform.rotation = Quaternion.identity;
				instance.transform.Rotate(Vector3.up, (float)rot * 90f);
				instance.transform.SetParent(chunkObj.parent_obj.transform);
				if (collectible != null)
				{
					collectible.InitAsHarvestable(chunk_data.zone, chunk_data.X, chunk_data.Z, innerX, innerZ, item, rot, chunkObj);
				}
				Interactable interactable = instance.GetComponent<Interactable>();
				if (interactable == null && instance.transform.Find("Interactable") != null)
				{
					interactable = instance.transform.Find("Interactable").GetComponent<Interactable>();
				}
				if (interactable != null)
				{
					interactable.corresponding_item = item;
					interactable.temp_rot = rot;
					if (interactable.icon_spr != null || interactable.icon2_spr != null)
					{
						interactable.RedrawOverheadIcon();
					}
					interactable.CustomStart(chunk_data, innerX, innerZ, item.item_name);
					foreach (Transform item2 in instance.transform)
					{
						if (item2.name == "Interactable Redirect")
						{
							item2.GetComponent<Interactable>().InitRedirect();
						}
					}
				}
				if (combatant != null)
				{
					combatant.Init();
					combatant.SetOrigin(chunk_data.zone, chunk_data.X, chunk_data.Z, innerX, innerZ);
					combatant.original_element_item = item;
					combatant.original_element_rot = rot;
					combatant.combat_name = chunk_data.zone + "," + chunk_data.X + "," + chunk_data.Z + "," + innerX + "," + innerZ;
					combatant.hp = combatant.HP_max;
					if (flag)
					{
						combatant.destroy_parent = true;
					}
					MobControl.Instance.active_combatants.Add(combatant.combat_name, combatant.gameObject);
				}
				PaintableObject component = instance.GetComponent<PaintableObject>();
				if (component != null)
				{
					string final_paint = inventory_ctr.Instance.GetPaintFromItemOrUseDefault(item);
					BanditCampsControl.Instance.ModifyIfBanditPaint(ref final_paint, item.item_name, item.GetString("bandit_camp_instance"));
					component.Colorize(final_paint, inventory_ctr.Instance.GetStampFromItem(item), inventory_ctr.Instance.GetLayoutItemFromItem(item.item_name), item.item_name);
				}
				AdjustBuildableInstance(instance, item, usage_context_t.world_object, chunk_data.zone, chunk_data.X, chunk_data.Z, innerX, innerZ, build_context, chunk, chunkObj, chunk_data, rot);
				chunkObj.AddBuildableInstance(instance, item, chunk_data.X, chunk_data.Z, innerX, innerZ, (byte)rot);
				on_complete?.Invoke(instance);
				return;
			}
			if (item.item_name != "DEBUG-1x1-fillempty")
			{
				text = "BUILDABLE INSTANCE ALREADY CREATED [" + item.item_name + "], DISCARDING DUPLICATE";
			}
			IL_destroy:
			if (text != "")
			{
				Debug.Log(text);
			}
			UnityEngine.Object.Destroy(instance);
			on_complete?.Invoke(instance);
		};
		if (!ResourceControl.ValidWorldModel(item))
		{
			action(new GameObject("Empty prefab"));
		}
		else
		{
			ResourceControl.Instance.AsyncInstantiateWorldObjectPrefab(item, chunk, action);
		}
	}

	public void DecorateWeaponDisplay(InventoryItem display_item, GameObject parent, Action on_weapon_added = null)
	{
	}

	private Vector3 ParseVector3(string str, Vector3 default_)
	{
		return default(Vector3);
	}

	private Quaternion ParseQuaternion(string str)
	{
		return default(Quaternion);
	}

	public void PaintDisplayWeapon(GameObject weapon_instance, InventoryItem item_weapon)
	{
	}

	public void PositionDisplayWeapon(string input_wep, GameObject obj, bool flip)
	{
	}

	public void DecorateLargeWeaponDisplay(InventoryItem display_item, GameObject parent, Action on_weapons_added = null)
	{
	}

	public List<OccupiedSpace> GetObjectWorldGeometry(InventoryItem item, Vector3 origin, int rot)
	{
		string item_name = item.item_name;
		List<OccupiedSpace> list = new List<OccupiedSpace>();
		string itemLayer = GetItemLayer(item_name);
		if (InventoryUtils.IsStringItem(item_name))
		{
			list.Add(new OccupiedSpace(origin, origin, itemLayer, new ChunkElement(item, rot)));
			short @short = item.GetShort("model1_chunkX");
			short short2 = item.GetShort("model1_chunkZ");
			short short3 = item.GetShort("model1_innerX");
			short short4 = item.GetShort("model1_innerZ");
			list.Add(new OccupiedSpace(new Vector3((float)(@short * 10 + short3) + 0.5f, 0f, (float)(short2 * 10 + short4) + 0.5f), origin, itemLayer, new ChunkElement(item, rot)));
			return list;
		}
		foreach (Vector3 item2 in GetObjectLocalGeometry(item_name, rot))
		{
			list.Add(new OccupiedSpace(origin + item2, origin, itemLayer, new ChunkElement(item, rot)));
		}
		return list;
	}

	public List<Vector3> GetObjectLocalGeometry(string item_name, int rot)
	{
		object_geometry itemGeometry = GetItemGeometry(item_name);
		List<Vector3> list = new List<Vector3>();
		switch (itemGeometry)
		{
		case object_geometry.wallshape:
			list.Add(Vector3.zero);
			switch (rot)
			{
			case 0:
			case 2:
				list.Add(Vector3.left);
				list.Add(Vector3.right);
				break;
			case 1:
			case 3:
				list.Add(Vector3.forward);
				list.Add(Vector3.back);
				break;
			}
			break;
		case object_geometry._xplus1:
			list.Add(Vector3.zero);
			switch (rot)
			{
			case 0:
				list.Add(Vector3.right);
				break;
			case 1:
				list.Add(Vector3.back);
				break;
			case 2:
				list.Add(Vector3.left);
				break;
			case 3:
				list.Add(Vector3.forward);
				break;
			}
			break;
		case object_geometry.rugshape:
			list.Add(Vector3.zero);
			switch (rot)
			{
			case 0:
				list.Add(Vector3.left);
				list.Add(Vector3.right);
				list.Add(Vector3.forward);
				list.Add(new Vector3(1f, 0f, 1f));
				list.Add(new Vector3(-1f, 0f, 1f));
				break;
			case 1:
				list.Add(Vector3.forward);
				list.Add(Vector3.right);
				list.Add(Vector3.back);
				list.Add(new Vector3(1f, 0f, 1f));
				list.Add(new Vector3(1f, 0f, -1f));
				break;
			case 2:
				list.Add(Vector3.left);
				list.Add(Vector3.right);
				list.Add(Vector3.back);
				list.Add(new Vector3(1f, 0f, -1f));
				list.Add(new Vector3(-1f, 0f, -1f));
				break;
			case 3:
				list.Add(Vector3.forward);
				list.Add(Vector3.left);
				list.Add(Vector3.back);
				list.Add(new Vector3(-1f, 0f, 1f));
				list.Add(new Vector3(-1f, 0f, -1f));
				break;
			}
			break;
		case object_geometry.shackshape:
		case object_geometry._3_by_3:
			list.Add(Vector3.zero);
			list.Add(Vector3.left);
			list.Add(Vector3.right);
			list.Add(Vector3.forward);
			list.Add(Vector3.back);
			list.Add(new Vector3(1f, 0f, 1f));
			list.Add(new Vector3(-1f, 0f, -1f));
			list.Add(new Vector3(-1f, 0f, 1f));
			list.Add(new Vector3(1f, 0f, -1f));
			if (itemGeometry != object_geometry.shackshape)
			{
				break;
			}
			switch (rot)
			{
			case 0:
				list.Add(Vector3.right * 2f);
				break;
			case 1:
				list.Add(Vector3.back * 2f);
				break;
			case 2:
				list.Add(Vector3.left * 2f);
				break;
			case 3:
				list.Add(Vector3.forward * 2f);
				break;
			}
			break;
		case object_geometry._2_by_2:
			list.Add(Vector3.zero);
			switch (rot)
			{
			case 0:
				list.Add(Vector3.right);
				list.Add(Vector3.forward);
				list.Add(new Vector3(1f, 0f, 1f));
				break;
			case 1:
				list.Add(Vector3.right);
				list.Add(Vector3.back);
				list.Add(new Vector3(1f, 0f, -1f));
				break;
			case 2:
				list.Add(Vector3.left);
				list.Add(Vector3.back);
				list.Add(new Vector3(-1f, 0f, -1f));
				break;
			case 3:
				list.Add(Vector3.left);
				list.Add(Vector3.forward);
				list.Add(new Vector3(-1f, 0f, 1f));
				break;
			}
			break;
		case object_geometry._5_by_5:
			list.Add(Vector3.zero);
			list.Add(Vector3.left);
			list.Add(Vector3.right);
			list.Add(Vector3.forward);
			list.Add(Vector3.back);
			list.Add(new Vector3(1f, 0f, 1f));
			list.Add(new Vector3(-1f, 0f, -1f));
			list.Add(new Vector3(-1f, 0f, 1f));
			list.Add(new Vector3(1f, 0f, -1f));
			list.Add(Vector3.left * 2f);
			list.Add(Vector3.right * 2f);
			list.Add(Vector3.forward * 2f);
			list.Add(Vector3.back * 2f);
			list.Add(new Vector3(2f, 0f, 2f));
			list.Add(new Vector3(-2f, 0f, -2f));
			list.Add(new Vector3(-2f, 0f, 2f));
			list.Add(new Vector3(2f, 0f, -2f));
			list.Add(new Vector3(1f, 0f, 2f));
			list.Add(new Vector3(1f, 0f, -2f));
			list.Add(new Vector3(-1f, 0f, 2f));
			list.Add(new Vector3(-1f, 0f, -2f));
			list.Add(new Vector3(2f, 0f, 1f));
			list.Add(new Vector3(-2f, 0f, 1f));
			list.Add(new Vector3(2f, 0f, -1f));
			list.Add(new Vector3(-2f, 0f, -1f));
			break;
		default:
			list.Add(Vector3.zero);
			break;
		}
		return list;
	}

	private void ChainReplace(InventoryItem old_item_no_stamp, InventoryItem new_item_no_stamp, string zone, int chunkX, int chunkZ, int innerX, int innerZ, int modX, int modZ, List<string> explored)
	{
	}

	private ToolUseResult ProcessToolClick(string chunkStr, string zone, int chunkX, int chunkZ, int innerX, int innerZ, ChunkElement element, Vector3 clickedAt)
	{
		return null;
	}

	public void DeleteUnnecessaryComponents(InventoryItem item, GameObject new_obj)
	{
	}

	private void DeleteUnnecessaryComponentsRecursive(Transform T)
	{
	}

	public void CreateMannequin(Transform parent_obj, InventoryItem mannequin_item, Action on_mannequin_created = null)
	{
	}

	private Vector3 SnapMousePositionToObjectOrigins(Vector3 original_position, InventoryItem item_placing)
	{
		return default(Vector3);
	}

	public bool TryMakeCompanionSitOnChair(ChunkObj chunkObj, ChunkData chunk_data, int x, int z, InventoryItem companion_item, GameObject override_companionObj = null, GameObject override_chairObj = null)
	{
		return false;
	}

	public void PlayerReplaceInteracting(InventoryItem new_item, bool send)
	{
	}

	public void ClickStatuePropertiesChangeAnimal1(int dir)
	{
	}

	public void ClickStatuePropertiesChangeAnimal2(int dir)
	{
	}

	public void ClickStatuePropertiesAccept()
	{
	}

	public void ClickStatueAdvancedProperties()
	{
	}

	private void RefreshAdvancedPropertiesCreatures()
	{
	}

	public void PressNavpostColor(int index)
	{
	}

	public void PressNavpostAccept()
	{
	}

	public string FormatNavpostString(string text, string col)
	{
		return null;
	}

	public object_geometry GetItemGeometry(string item_name)
	{
		switch (ResourceControl.Instance.GetStringFromItemFile(item_name, "World_geometry"))
		{
		case "1_by_1":
			return object_geometry._1_by_1;
		case "2_by_2":
			return object_geometry._2_by_2;
		case "Shack_shape":
			return object_geometry.shackshape;
		case "x_plus_1":
			return object_geometry._xplus1;
		case "5_by_5":
			return object_geometry._5_by_5;
		case "Wall_shape":
			return object_geometry.wallshape;
		case "3_by_3":
			return object_geometry._3_by_3;
		case "Rug_shape":
			return object_geometry.rugshape;
		default:
			return object_geometry.undefined;
		}
	}

	public string GetItemLayer(string item_name)
	{
		string stringFromItemFile = ResourceControl.Instance.GetStringFromItemFile(item_name, "Layer");
		if (!(stringFromItemFile == ""))
		{
			return stringFromItemFile;
		}
		return "normal";
	}
}
