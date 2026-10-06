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

	private string edit_navpost_col = "white";

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
	}

	public void ClickAcceptBuild()
	{
		PopupControl.Instance.SetButtonWasPressed();
		TryAcceptBuild();
	}

	private void TryAcceptBuild()
	{
		if (mouse_obj == null)
		{
			return;
		}
		Vector3 position = mouse_obj.transform.position;
		if (!AllowedToPlace(position, inventory_ctr.Instance.ITEM_USING))
		{
			if (inventory_ctr.Instance.ITEM_USING.item_name == "Companion")
			{
				PopupControl.Instance.ShowMessage("Your companion cannot stand there.");
			}
			else
			{
				PopupControl.Instance.ShowMessage("You cannot build that there.");
			}
			return;
		}
		string player_zone = ChunkControl.Instance.player_zone;
		string chunkString = ChunkControl.Instance.GetChunkString(position);
		Vector3 chunkCoords = ChunkControl.Instance.GetChunkCoords(position);
		int num = (int)chunkCoords.x;
		int num2 = (int)chunkCoords.z;
		Vector3 inner = ChunkControl.Instance.GetInner(position);
		int num3 = (int)inner.x;
		int num4 = (int)inner.z;
		Chunk chunk = ChunkControl.Instance.GetChunk(chunkString);
		if (chunk == null)
		{
			return;
		}
		ChunkData chunk_data = chunk.chunk_data;
		if (chunk_data == null || !LandClaimControl.Instance.AllowedToBuild(player_zone, num, num2))
		{
			return;
		}
		if (inventory_ctr.Instance.ITEM_USING.item_name == "Companion")
		{
			if (ChunkControl.Instance.GetNumItemsInSurroundingArea("Companion") > 9)
			{
				PopupControl.Instance.ShowMessage("There are too many companions in this area!\n<color=#999999>(The game will start lagging if you add more)</color>\n\nPlease try putting it further away...");
				return;
			}
		}
		else if (inventory_ctr.Instance.ITEM_USING.item_name == "Painting")
		{
			if (ChunkControl.Instance.GetNumItemsInSurroundingArea("Painting") > 6)
			{
				PopupControl.Instance.ShowMessage("There are too many paintings in this area!\n<color=#999999>(The game will start lagging if you add more)</color>\n\nPlease try putting it further away...");
				return;
			}
		}
		else if (InventoryUtils.IsStringItem(inventory_ctr.Instance.ITEM_USING.item_name))
		{
			short num5 = inventory_ctr.Instance.ITEM_USING.GetShort("model1_innerX");
			short num6 = inventory_ctr.Instance.ITEM_USING.GetShort("model1_innerZ");
			short num7 = inventory_ctr.Instance.ITEM_USING.GetShort("model1_chunkX");
			short num8 = inventory_ctr.Instance.ITEM_USING.GetShort("model1_chunkZ");
			if (num6 == 0 && num5 == 0 && num7 == 0 && num8 == 0)
			{
				ExtraInventoryData extraDataCopy = inventory_ctr.Instance.ITEM_USING.GetExtraDataCopy();
				extraDataCopy.SetShort("model1_innerX", num3);
				extraDataCopy.SetShort("model1_innerZ", num4);
				extraDataCopy.SetShort("model1_chunkX", num);
				extraDataCopy.SetShort("model1_chunkZ", num2);
				inventory_ctr.Instance.ITEM_USING = new InventoryItem(inventory_ctr.Instance.ITEM_USING.item_name, extraDataCopy);
				MouseObjPositionChanged(mouse_obj.transform.position, false);
				return;
			}
		}
		else if (inventory_ctr.Instance.ITEM_USING.item_name == "3-day Land Claim" || inventory_ctr.Instance.ITEM_USING.item_name == "8-day Land Claim" || inventory_ctr.Instance.ITEM_USING.item_name == "Admin Land Claim")
		{
			if (player_zone != "overworld")
			{
				PopupControl.Instance.ShowMessage("You can only build this outdoors");
				return;
			}
			if (!LandClaimControl.Instance.IsFarEnoughAwayFromEnemyLandClaims(player_zone, num, num2, PlayerData.Instance.GetGlobalString("username_lower")))
			{
				PopupControl.Instance.ShowMessage("This is too close to other Land Claims\n(the edges would overlap!)\n<color=#f5b042>Try building further away.</color>");
				return;
			}
		}
		inventory_ctr.Instance.ITEM_USING = inventory_ctr.Instance.FinalizeItemBeforePutDown(inventory_ctr.Instance.ITEM_USING, chunk_data, num3, num4);
		PlayerBuildAt(inventory_ctr.Instance.ITEM_USING, ChunkControl.Instance.player_zone, num, num2, num3, num4, mouse_rot, build_context_t.on_self_build_new, "ME", GenerateCacheKey());
		if (InventoryUtils.IsHouseObject(inventory_ctr.Instance.ITEM_USING.item_name))
		{
			switch (InventoryUtils.GetBuildingType(inventory_ctr.Instance.ITEM_USING.item_name))
			{
			case InventoryUtils.building_type.castle:
				AchievesControl.Instance.UnlockAchievement("Your Royal Highness");
				break;
			case InventoryUtils.building_type.mansion:
				AchievesControl.Instance.UnlockAchievement("Living With Style!");
				break;
			case InventoryUtils.building_type.shack:
				AchievesControl.Instance.UnlockAchievement("Home Sweet Home");
				break;
			}
		}
		else if (inventory_ctr.Instance.ITEM_USING.item_name == "Companion")
		{
			CompanionController.Instance.DestroyActiveCompanion(CompanionController.Instance.GetCurrSelectedCompanion());
		}
		if (done_button_context == button_state.MODIFY_OBJECT)
		{
			DonePlacing(false, false);
			EnterToolMode(new InventoryItem("Builder Tools"));
		}
		else if (done_button_context == button_state.BUILD_NEW_OBJ)
		{
			if ((inventory_ctr.Instance.GetItemBool(inventory_ctr.Instance.ITEM_USING.item_name, "is_flooring_obj") || inventory_ctr.Instance.GetItemBool(inventory_ctr.Instance.ITEM_USING.item_name, "is_wall_obj")) && inventory_ctr.Instance.HasItem(inventory_ctr.Instance.ITEM_USING))
			{
				inventory_ctr.Instance.GrabNext(inventory_ctr.Instance.ITEM_USING);
				MouseObjPositionChanged(SnapMousePositionToObjectOrigins(mouse_obj.transform.position, inventory_ctr.Instance.ITEM_USING), false);
				click_to_place_BUTTON_text.text = "DONE";
			}
			else
			{
				DonePlacing(false, true);
			}
		}
	}

	public void RecycleUniqueIds(List<int> unique_ids)
	{
		foreach (int unique_id in unique_ids)
		{
			int num = PlayerData.Instance.GetSlotShort("n_recycled_unique_ids", PlayerData.filename_t.general);
			PlayerData.Instance.SetSlotLong("recycled_unique_id_" + num, unique_id, PlayerData.filename_t.general);
			PlayerData.Instance.SetSlotShort("n_recycled_unique_ids", num + 1, PlayerData.filename_t.general);
		}
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
		string chunkString = ChunkControl.Instance.GetChunkString(zone, chunkX, chunkZ);
		ChunkObj chunkObj = ChunkControl.Instance.GetChunkObj(chunkString);
		ChunkData chunkData = ChunkControl.Instance.GetChunkData(chunkString);
		if (chunkObj == null || chunkData == null)
		{
			return false;
		}
		chunkObj.ReplaceElementItemInstance(chunkData.X, chunkData.Z, innerX, innerZ, new_item, old_element_item, old_element_rot, chunkData, on_complete);
		chunkData.ReplaceElementItem(innerX, innerZ, new_item, old_element_item, old_element_rot);
		if (GameServerConnector.Instance.ShouldSaveLocally())
		{
			chunkData.SaveWholeChunkToDisk(chunkString);
		}
		return true;
	}

	private void MouseObjPositionChanged(Vector3 rounded_clickedAt, bool on_enter_build_mode)
	{
		bool flag = AllowedToPlace(rounded_clickedAt, inventory_ctr.Instance.ITEM_USING);
		Image component = button_rotate_furniture.transform.Find("accept").Find("bg").GetComponent<Image>();
		if (flag)
		{
			component.color = col_checkmark_allowed;
			button_rotate_furniture.transform.Find("accept").Find("bg").GetComponent<CanvasGroup>().alpha = 1f;
		}
		else
		{
			component.color = col_checkmark_not_allowed;
			button_rotate_furniture.transform.Find("accept").Find("bg").GetComponent<CanvasGroup>().alpha = 0.4f;
		}
		ConvertToGlow(mouse_obj.transform, flag);
		if (!on_enter_build_mode && button_rotate_furniture != null)
		{
			button_rotate_furniture.transform.Find("accept").gameObject.SetActive(true);
		}
		string chunkString = ChunkControl.Instance.GetChunkString(rounded_clickedAt);
		Vector3 chunkCoords = ChunkControl.Instance.GetChunkCoords(rounded_clickedAt);
		int chunkX = (int)chunkCoords.x;
		int chunkZ = (int)chunkCoords.z;
		Vector3 inner = ChunkControl.Instance.GetInner(rounded_clickedAt);
		int num = (int)inner.x;
		int num2 = (int)inner.z;
		mouse_obj.transform.rotation = Quaternion.Euler(0f, mouse_rot * 90, 0f);
		if (inventory_ctr.Instance.ITEM_USING.item_name == "Companion")
		{
			if (inventory_ctr.Instance.ITEM_USING.GetString("companion_mode") != "guard" && (!ChunkControl.Instance.IsChunkFullyLoadedOrMidload(chunkString) || !TryMakeCompanionSitOnChair(ChunkControl.Instance.GetChunkObj(chunkString), ChunkControl.Instance.GetChunkData(chunkString), num, num2, inventory_ctr.Instance.ITEM_USING, mouse_obj)))
			{
				mouse_obj.transform.Find("creature-go-here").GetChild(0).GetComponent<LiteModel>().StartAnimation(0);
			}
		}
		else if (inventory_ctr.Instance.ITEM_USING.item_name == "Painting")
		{
			SnapPainting(mouse_obj, chunkX, chunkZ, num, num2);
		}
		else if (InventoryUtils.IsStringItem(inventory_ctr.Instance.ITEM_USING.item_name))
		{
			RedrawStringLights(mouse_obj, inventory_ctr.Instance.ITEM_USING);
		}
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
		string item_name = item.item_name;
		string chunkString = ChunkControl.Instance.GetChunkString(rounded_clickedAt);
		ChunkControl.Instance.GetChunkCoords(rounded_clickedAt);
		ChunkControl.Instance.GetInner(rounded_clickedAt);
		ChunkData chunkData = ChunkControl.Instance.GetChunkData(chunkString);
		string itemLayer = GetItemLayer(item_name);
		foreach (OccupiedSpace item2 in ChunkControl.Instance.GetBuildablesThatOverlapThisSpace(rounded_clickedAt))
		{
			if (item_name == "Companion")
			{
				if (InventoryUtils.IsChairObject(item2.element.item.item_name) || InventoryUtils.IsBedObject(item2.element.item.item_name))
				{
					continue;
				}
			}
			else if (InventoryUtils.IsStringItem(item_name) && InventoryUtils.IsStringItem(item2.element.item.item_name))
			{
				continue;
			}
			if (!InventoryUtils.IsSimpleMob(item2.element.item.item_name) && ((itemLayer == "sub_flooring" && item2.layer == "sub_flooring") || (itemLayer == "flooring" && item2.layer == "flooring") || (itemLayer == "normal" && item2.layer == "normal")))
			{
				return false;
			}
		}
		bool flag = InventoryUtils.IsStringItem(item_name);
		if (flag)
		{
			short num = item.GetShort("model1_chunkX");
			short num2 = item.GetShort("model1_chunkZ");
			short num3 = item.GetShort("model1_innerX");
			short num4 = item.GetShort("model1_innerZ");
			if (rounded_clickedAt == new Vector3((float)(num * 10 + num3) + 0.5f, 0f, (float)(num2 * 10 + num4) + 0.5f))
			{
				return false;
			}
		}
		if (InventoryUtils.IsCaveObject(ZoneDataControl.Instance.curr_zonedata.house_item.item_name) && ZoneDataControl.Instance.curr_cave_exit != null && Vector3.Distance(ZoneDataControl.Instance.curr_cave_exit.transform.position, rounded_clickedAt) < ChunkControl.dist_empty_around_cave_ladder)
		{
			return false;
		}
		if (ChunkControl.Instance.player_zone != "overworld")
		{
			if (InventoryUtils.IsHouseObject(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
			{
				List<Vector3> list;
				switch (InventoryUtils.GetBuildingType(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
				{
				case InventoryUtils.building_type.shack:
				case InventoryUtils.building_type.igloo:
					list = InventoryUtils.GetShackBuildArea();
					break;
				case InventoryUtils.building_type.mansion:
					list = InventoryUtils.GetMansionBuildArea();
					break;
				case InventoryUtils.building_type.castle:
					list = InventoryUtils.GetCastleBuildArea();
					break;
				case InventoryUtils.building_type.underground_room:
				case InventoryUtils.building_type.upstairs_room:
				case InventoryUtils.building_type.tent:
					list = InventoryUtils.GetUndergroundBuildArea();
					break;
				case InventoryUtils.building_type.windmill:
					list = InventoryUtils.GetWindmillBuildArea();
					break;
				case InventoryUtils.building_type.warehouse:
					list = InventoryUtils.GetWarehouseBuildArea();
					break;
				default:
					list = null;
					break;
				}
				ZoneData curr_zonedata = ZoneDataControl.Instance.curr_zonedata;
				if (!list.Contains(rounded_clickedAt - new Vector3((float)(curr_zonedata.interior_model_chunkX * 10 + curr_zonedata.interior_model_innerX) + 0.5f, 0f, (float)(curr_zonedata.interior_model_chunkZ * 10 + curr_zonedata.interior_model_innerZ) + 0.5f)))
				{
					return false;
				}
			}
			else if (flag && chunkData.floor_model_id == 0)
			{
				return false;
			}
		}
		return Vector3.Distance(GameController.Instance.player.transform.position, rounded_clickedAt) > 1f;
	}

	private void Update()
	{
		if (!inventory_ctr.Instance.PLACING_OBJECT_OR_USING_TOOL)
		{
			return;
		}
		if (inventory_ctr.Instance.GetItemType(inventory_ctr.Instance.ITEM_USING) == inventory_ctr.inv_type_t.tool)
		{
			if (GamepadInput.Instance.GetMouseButtonDown() && !PopupControl.Instance.GetButtonWasPressed())
			{
				Ray ray = Camera.main.ScreenPointToRay(GamepadInput.Instance.GetMousePosition());
				if (GameController.Instance.plane.Raycast(ray, out var enter))
				{
					UseToolClick(ChunkControl.Instance.GetRoundedClick(ray.GetPoint(enter)) + new Vector3(0.5f, 0f, 0.5f));
				}
			}
		}
		else if (inventory_ctr.Instance.GetItemType(inventory_ctr.Instance.ITEM_USING) == inventory_ctr.inv_type_t.place_in_world && GamepadInput.Instance.GetMouseButton() && !PopupControl.Instance.GetButtonWasPressed())
		{
			Ray ray2 = Camera.main.ScreenPointToRay(GamepadInput.Instance.GetMousePosition());
			if (!GameController.Instance.plane.Raycast(ray2, out var enter2))
			{
				return;
			}
			Vector3 vector = SnapMousePositionToObjectOrigins(ChunkControl.Instance.GetRoundedClick(ray2.GetPoint(enter2)) + new Vector3(0.5f, 0f, 0.5f), inventory_ctr.Instance.ITEM_USING);
			if (mouse_obj != null)
			{
				Vector3 position = mouse_obj.transform.position;
				mouse_obj.transform.position = vector;
				if (position != mouse_obj.transform.position)
				{
					MouseObjPositionChanged(vector, false);
				}
			}
			else
			{
				CreateMouseObj(false, (byte)mouse_rot, vector);
			}
		}
	}

	private void ConvertToGlow(Transform T, bool allowed)
	{
		Component[] components = T.GetComponents<Component>();
		foreach (Component component in components)
		{
			if (!(component.GetType() == typeof(MeshRenderer)))
			{
				continue;
			}
			MeshRenderer meshRenderer = (MeshRenderer)component;
			Material material = new Material((T.tag == "MouseObjCutout") ? (allowed ? mat_build_allowed_CUTOUT : mat_build_not_allowed_CUTOUT) : (allowed ? mat_build_allowed : mat_build_not_allowed));
			material.mainTexture = meshRenderer.material.mainTexture;
			Material[] array = new Material[meshRenderer.materials.Length];
			for (int j = 0; j < array.Length; j++)
			{
				array[j] = material;
			}
			meshRenderer.materials = array;
		}
		foreach (Transform item in T)
		{
			ConvertToGlow(item, allowed);
		}
	}

	private void CreateMouseObj(bool on_modify_position, byte start_rot, Vector3 start_pos)
	{
		if (creating_mouse_obj)
		{
			return;
		}
		creating_mouse_obj = true;
		mouse_rot = start_rot;
		mouse_obj_geometry = GetItemGeometry(inventory_ctr.Instance.ITEM_USING.item_name);
		Action<GameObject> on_mouse_obj_ready = delegate(GameObject new_obj)
		{
			mouse_obj = new_obj;
			mouse_obj.name = "Mouse Obj";
			DeleteUnnecessaryComponents(inventory_ctr.Instance.ITEM_USING, new_obj);
			AdjustBuildableInstance(new_obj, inventory_ctr.Instance.ITEM_USING, usage_context_t.on_mouseObj_or_storeModel);
			new_obj.transform.Rotate(Vector3.up, mouse_rot * 90);
			new_obj.transform.position = start_pos;
			MouseObjPositionChanged(start_pos, true);
			creating_mouse_obj = false;
		};
		bool itemBool = inventory_ctr.Instance.GetItemBool(inventory_ctr.Instance.ITEM_USING.item_name, "is_wall_obj");
		bool itemBool2 = inventory_ctr.Instance.GetItemBool(inventory_ctr.Instance.ITEM_USING.item_name, "is_flooring_obj");
		if (!itemBool2 && !itemBool && !ResourceControl.ValidWorldModel(inventory_ctr.Instance.ITEM_USING))
		{
			on_mouse_obj_ready(new GameObject("Empty prefab"));
		}
		else if (itemBool)
		{
			GameObject parent = new GameObject();
			parent.transform.localScale = Vector3.one;
			parent.transform.localRotation = Quaternion.identity;
			parent.transform.localPosition = Vector3.zero;
			ModularObjectControl.Instance.AsyncLoadModularModel("Wall-Models/" + inventory_ctr.Instance.ITEM_USING.item_name + "/0_prefab", delegate(GameObject new_mesh)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(new_mesh);
				gameObject.transform.SetParent(parent.transform);
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
				gameObject.transform.localScale = Vector3.one * 0.5f;
				gameObject.SetActive(true);
				on_mouse_obj_ready(parent);
			});
		}
		else if (itemBool2)
		{
			GameObject prefab = null;
			GameObject parent2 = new GameObject();
			parent2.transform.localScale = Vector3.one;
			parent2.transform.localRotation = Quaternion.identity;
			parent2.transform.localPosition = Vector3.zero;
			GameObject mesh1 = null;
			int load_ops = 2;
			Action all_loaded = delegate
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(prefab);
				gameObject2.SetActive(true);
				gameObject2.transform.SetParent(parent2.transform);
				gameObject2.transform.localScale = Vector3.one;
				gameObject2.transform.localRotation = Quaternion.identity;
				gameObject2.transform.localPosition = new Vector3(-0.5f, 0f, -0.5f);
				PartiallyGeneratedModularModel new_model = new PartiallyGeneratedModularModel(inventory_ctr.Instance.ITEM_USING, ModularObjectControl.segment.undefined, "", "", -1, -1, -1);
				ModularObjectControl.Instance.AddVertices(mesh1, 0, 0, 0, 1, 0, new_model);
				ModularObjectControl.Instance.CreateMesh(gameObject2, new_model, false);
				on_mouse_obj_ready(parent2);
			};
			ModularObjectControl.Instance.AsyncLoadModularModel("Pathway-Prefabs/" + inventory_ctr.Instance.ITEM_USING.item_name, delegate(GameObject loaded_prefab)
			{
				prefab = loaded_prefab;
				load_ops--;
				if (load_ops == 0)
				{
					all_loaded();
				}
			});
			ModularObjectControl.Instance.AsyncLoadModularModel(ResourceControl.Instance.GetStringFromItemFile(inventory_ctr.Instance.ITEM_USING.item_name, "flooring_model") + "/1_prefab", delegate(GameObject new_mesh)
			{
				mesh1 = new_mesh;
				load_ops--;
				if (load_ops == 0)
				{
					all_loaded();
				}
			});
		}
		else
		{
			ResourceControl.Instance.AsyncInstantiateWorldObjectPrefab(inventory_ctr.Instance.ITEM_USING, null, on_mouse_obj_ready);
		}
	}

	public void EnterBuildMode(InventoryItem item, Vector3 mouse_obj_start_pos, byte start_rot, bool on_modify_position)
	{
		if (mouse_obj_start_pos == Vector3.zero)
		{
			mouse_obj_start_pos = SnapMousePositionToObjectOrigins(new Vector3((float)((int)GameController.Instance.prev_player_pos.x - 4) + 0.5f, 0f, (float)((int)GameController.Instance.prev_player_pos.z - 1) + 0.5f), item);
		}
		inventory_ctr.Instance.ITEM_USING = inventory_ctr.Instance.AdjustMouseItemData(item);
		inventory_ctr.Instance.PLACING_OBJECT_OR_USING_TOOL = true;
		click_to_place.SetActive(true);
		if (on_modify_position)
		{
			ShowDoneButton("CANCEL", button_state.MODIFY_OBJECT);
		}
		else
		{
			if (item.item_name == "Companion")
			{
				click_to_place_txt.text = "Pick a spot for " + CompanionController.Instance.GetCurrSelectedCompanion().companion_name.ToUpper() + " to stand";
			}
			else
			{
				click_to_place_txt.text = "Pick a spot to put your " + item.item_name;
			}
			ShowDoneButton("CANCEL", button_state.BUILD_NEW_OBJ);
		}
		button_rotate_furniture = UnityEngine.Object.Instantiate(prefab_rotate_button);
		button_rotate_furniture.transform.SetParent(MobControl.Instance.gameObject.transform);
		button_rotate_furniture.transform.SetAsFirstSibling();
		button_rotate_furniture.transform.localPosition = Vector3.zero;
		button_rotate_furniture.transform.localScale = Vector3.one;
		button_rotate_furniture.transform.localRotation = Quaternion.identity;
		Transform transform = button_rotate_furniture.transform;
		((RectTransform)button_rotate_furniture.transform).anchorMax = Vector2.one * 10f;
		((RectTransform)transform).anchorMin = Vector2.one * 10f;
		button_rotate_furniture.transform.Find("delete").gameObject.SetActive(done_button_context == button_state.MODIFY_OBJECT);
		GameObject gameObject = button_rotate_furniture.transform.Find("accept").gameObject;
		if (on_modify_position)
		{
			gameObject.SetActive(false);
			Vector3 localPosition = button_rotate_furniture.transform.Find("accept").localPosition;
			button_rotate_furniture.transform.Find("accept").localPosition = button_rotate_furniture.transform.Find("delete").localPosition;
			button_rotate_furniture.transform.Find("delete").localPosition = localPosition;
		}
		else
		{
			gameObject.SetActive(true);
		}
		if (mouse_obj != null)
		{
			UnityEngine.Object.Destroy(mouse_obj);
		}
		CreateMouseObj(false, start_rot, mouse_obj_start_pos);
	}

	public void RotateMouseObj()
	{
		if (InventoryUtils.IsStringItem(inventory_ctr.Instance.ITEM_USING.item_name) || inventory_ctr.Instance.ITEM_USING.item_name == "Painting")
		{
			return;
		}
		if (mouse_obj != null)
		{
			mouse_obj.transform.Rotate(Vector3.up, 90f);
		}
		mouse_rot = ((mouse_rot + 1 < 4) ? (mouse_rot + 1) : 0);
	}

	private void FixedUpdate()
	{
		if (button_rotate_furniture != null && mouse_obj != null)
		{
			float num;
			switch (mouse_obj_geometry)
			{
			case object_geometry.undefined:
			case object_geometry._1_by_1:
				num = 0.9f;
				break;
			case object_geometry._xplus1:
			case object_geometry.rugshape:
				num = 1.05f;
				break;
			default:
				num = 1.8f;
				break;
			}
			MobControl.Instance.SnapOverhead((RectTransform)button_rotate_furniture.transform, mouse_obj.transform.position + new Vector3(num, 0f, num) * ChunkControl.Instance.view_zoom);
		}
	}

	public void GrabFurniture(ChunkElement element, string zone, int item_chunkX, int item_chunkZ, int item_innerX, int item_innerZ, int mouse_chunkX, int mouse_chunkZ, int mouse_innerX, int mouse_innerZ)
	{
		DonePlacing(true, false);
		InventoryItem inventoryItem = element.item;
		PlayerRemoveAt(element, zone, item_chunkX, item_chunkZ, item_innerX, item_innerZ, remove_context.self_remove, GenerateCacheKey());
		if (inventoryItem.item_name == "3-day Land Claim" || inventoryItem.item_name == "8-day Land Claim")
		{
			inventoryItem = new InventoryItem("Old Land Claim", new ExtraInventoryData());
		}
		else if (InventoryUtils.IsStringItem(inventoryItem.item_name))
		{
			if (item_chunkX != mouse_chunkX || item_chunkZ != mouse_chunkZ || mouse_innerX != item_innerX || mouse_innerZ != item_innerZ)
			{
				ExtraInventoryData extraInventoryData = new ExtraInventoryData();
				extraInventoryData.SetShort("model1_innerX", item_innerX);
				extraInventoryData.SetShort("model1_innerZ", item_innerZ);
				extraInventoryData.SetShort("model1_chunkX", item_chunkX);
				extraInventoryData.SetShort("model1_chunkZ", item_chunkZ);
				item_chunkX = inventoryItem.GetShort("model1_chunkX");
				item_chunkZ = inventoryItem.GetShort("model1_chunkZ");
				item_innerX = inventoryItem.GetShort("model1_innerX");
				item_innerZ = inventoryItem.GetShort("model1_innerZ");
				inventoryItem = new InventoryItem(inventoryItem.item_name, extraInventoryData);
			}
		}
		else if (inventoryItem.item_name == "Teleporter")
		{
			ExtraInventoryData extraDataCopy = inventoryItem.GetExtraDataCopy();
			extraDataCopy.SetShort("set_up", 0);
			inventoryItem = new InventoryItem(inventoryItem.item_name, extraDataCopy);
		}
		EnterBuildMode(inventoryItem, new Vector3((float)(item_innerX + item_chunkX * 10) + 0.5f, 0f, (float)(item_innerZ + item_chunkZ * 10) + 0.5f), (byte)element.rot, true);
	}

	public void DonePlacing(bool manual_press, bool unpause_game)
	{
		if (done_button_context == button_state.none)
		{
			return;
		}
		PopupControl.Instance.SetButtonWasPressed();
		DONE_placing_button.SetActive(false);
		if (unpause_game)
		{
			GameController.Instance.GiveAllOverheads();
			GameplayGUIControl.Instance.ShowGameplayGui();
			GameController.Instance.UNPAUSE_GAME();
		}
		switch (done_button_context)
		{
		case button_state.DONE_USING_TOOL:
			click_to_place.SetActive(false);
			inventory_ctr.Instance.PLACING_OBJECT_OR_USING_TOOL = false;
			if (manual_press)
			{
				if (InventoryUtils.IsPaintbrush(inventory_ctr.Instance.ITEM_USING.item_name) || InventoryUtils.IsStamp(inventory_ctr.Instance.ITEM_USING.item_name))
				{
					inventory_ctr.Instance.GiveItem(inventory_ctr.Instance.ITEM_USING, 1, "", false);
				}
				else if (inventory_ctr.Instance.ITEM_USING.item_name == "Paint Thinner")
				{
					inventory_ctr.Instance.GiveItem("Paint Thinner", 1, "", false);
				}
				else if (inventory_ctr.Instance.ITEM_USING.item_name == "Lock")
				{
					inventory_ctr.Instance.GiveItem("Lock", 1, "", false);
				}
			}
			break;
		case button_state.BUILD_NEW_OBJ:
		case button_state.MODIFY_OBJECT:
			if (mouse_obj != null)
			{
				UnityEngine.Object.Destroy(mouse_obj);
			}
			if (button_rotate_furniture != null)
			{
				UnityEngine.Object.Destroy(button_rotate_furniture);
			}
			click_to_place.SetActive(false);
			inventory_ctr.Instance.PLACING_OBJECT_OR_USING_TOOL = false;
			if (manual_press && !(inventory_ctr.Instance.ITEM_USING.item_name == "Companion"))
			{
				if (InventoryUtils.IsStringItem(inventory_ctr.Instance.ITEM_USING.item_name))
				{
					inventory_ctr.Instance.GiveItem(inventory_ctr.Instance.ITEM_USING.item_name, 1, "", false);
				}
				else
				{
					inventory_ctr.Instance.GiveItem(inventory_ctr.Instance.ITEM_USING, 1, "", false);
				}
			}
			break;
		case button_state.CAST_PROJECTILE_AT_ENEMY:
			click_to_place.SetActive(false);
			GameController.Instance.casting_projectile_at_enemy = false;
			break;
		case button_state.CAST_PROJECTILE_AT_ALLY:
			click_to_place.SetActive(false);
			GameController.Instance.casting_projectile_at_ally = false;
			break;
		case button_state.PICKING_CAST_CUSTOM_LOCATION:
			click_to_place.SetActive(false);
			GameController.Instance.picking_cast_custom_location = false;
			break;
		case button_state.COMPANION_ATTACK:
			click_to_place.SetActive(false);
			GameController.Instance.is_picking_companion_target = false;
			break;
		case button_state.COMPANION_MOVE:
			click_to_place.SetActive(false);
			GameController.Instance.is_picking_companion_walk_location = false;
			break;
		}
		done_button_context = button_state.none;
	}

	public void EndDelete()
	{
		inventory_ctr.Instance.DropExtraItemDataOnPickup(inventory_ctr.Instance.ITEM_USING);
		DonePlacing(false, false);
		EnterToolMode(new InventoryItem("Builder Tools"));
	}

	public void EnterToolMode(InventoryItem tool)
	{
		inventory_ctr.Instance.ITEM_USING = tool;
		inventory_ctr.Instance.PLACING_OBJECT_OR_USING_TOOL = true;
		click_to_place.SetActive(true);
		ShowDoneButton("DONE", button_state.DONE_USING_TOOL);
	}

	public void UseToolClick(Vector3 clickedAt)
	{
		Vector3 roundedClick = ChunkControl.Instance.GetRoundedClick(clickedAt);
		string player_zone = ChunkControl.Instance.player_zone;
		List<ToolUseResult> list = new List<ToolUseResult>();
		foreach (OccupiedSpace item in ChunkControl.Instance.GetBuildablesThatOverlapThisSpace(roundedClick + new Vector3(0.5f, 0f, 0.5f)))
		{
			string chunkString = ChunkControl.Instance.GetChunkString(item.origin);
			Vector3 inner = ChunkControl.Instance.GetInner(item.origin);
			int innerX = (int)inner.x;
			int innerZ = (int)inner.z;
			Vector3 chunkCoords = ChunkControl.Instance.GetChunkCoords(item.origin);
			ToolUseResult toolUseResult = ProcessToolClick(chunkString, player_zone, (int)chunkCoords.x, (int)chunkCoords.z, innerX, innerZ, item.element, clickedAt);
			if (toolUseResult.status_ == ToolUseResult.status.success)
			{
				return;
			}
			list.Add(toolUseResult);
		}
		ToolUseResult print = null;
		if (HasToolUseResultByStatus(ToolUseResult.status.error_land_claimed, list, ref print))
		{
			PopupControl.Instance.ShowMessage("Cannot modify object\nThis land claim is owned by <color=#17d8ff>" + print.extra_data + "</color>");
		}
		else if (HasToolUseResultByStatus(ToolUseResult.status.error_someone_using, list, ref print))
		{
			PopupControl.Instance.ShowMessage("Cannot modify object\nSomeone is using that!");
		}
		else if (HasToolUseResultByStatus(ToolUseResult.status.error_dev_obj, list, ref print))
		{
			PopupControl.Instance.ShowMessage(TranslationControl.Instance.TranslateGeneral("Cannot modify object - it's owned by the NPCs nearby! Try again on objects further in the wild", "GUI"));
		}
		else if (HasToolUseResultByStatus(ToolUseResult.status.error_bandit_camp, list, ref print))
		{
			PopupControl.Instance.ShowMessage(TranslationControl.Instance.TranslateGeneral("Cannot modify object. First you must find and destroy their flag!", "GUI"), PopupControl.context.message, new InventoryItem("Flagpole"));
		}
		else if (HasToolUseResultByStatus(ToolUseResult.status.error_need_tool, list, ref print))
		{
			PopupControl.Instance.ShowMessage(TranslationControl.Instance.TranslateGeneral("That isn't a furniture object. You need a TOOL_NAME to modify that!", "GUI").Replace("TOOL_NAME", print.extra_data), PopupControl.context.message, new InventoryItem(print.extra_data));
		}
		else
		{
			HasToolUseResultByStatus(ToolUseResult.status.error_not_editable_at_all, list, ref print);
		}
	}

	private bool HasToolUseResultByStatus(ToolUseResult.status looking_for, List<ToolUseResult> error_codes, ref ToolUseResult print)
	{
		foreach (ToolUseResult error_code in error_codes)
		{
			if (error_code.status_ == looking_for)
			{
				print = error_code;
				return true;
			}
		}
		return false;
	}

	public void ShowDoneButton(string str, button_state context)
	{
		DONE_placing_button.SetActive(context != button_state.MODIFY_OBJECT);
		click_to_place_BUTTON_text.text = str;
		done_button_context = context;
	}

	public void DeleteMouseObject()
	{
		if (!inventory_ctr.Instance.CanReceiveItem(inventory_ctr.Instance.ITEM_USING, 1))
		{
			PopupControl.Instance.ShowMessage("Can't take item - your inventory is full!");
		}
		else if (InventoryUtils.IsHouseObject(inventory_ctr.Instance.ITEM_USING.item_name))
		{
			if (inventory_ctr.Instance.ITEM_USING.item_name == "Underground Room" || inventory_ctr.Instance.ITEM_USING.item_name == "Upstairs Room")
			{
				PopupControl.Instance.on_yes_pressed = delegate
				{
					inventory_ctr.Instance.DropExtraItemDataOnPickup(inventory_ctr.Instance.ITEM_USING);
					DonePlacing(false, false);
					EnterToolMode(new InventoryItem("Builder Tools"));
				};
				PopupControl.Instance.ShowYesNo("Are you sure you want to pick up this room?\n<color=#ff392b>EVERYTHING INSIDE WILL DISAPPEAR!</color>", "Yes", "No", PopupControl.context.yesno_ACTION);
			}
			else
			{
				PopupControl.Instance.on_yes_pressed = delegate
				{
					inventory_ctr.Instance.DropExtraItemDataOnPickup(inventory_ctr.Instance.ITEM_USING);
					DonePlacing(false, false);
					EnterToolMode(new InventoryItem("Builder Tools"));
				};
				PopupControl.Instance.ShowYesNo("Are you sure you want to pick up this house?\n<color=#ff392b>EVERYTHING INSIDE WILL DISAPPEAR!</color>", "Yes", "No", PopupControl.context.yesno_ACTION);
			}
		}
		else if (InventoryUtils.IsHellDimension(inventory_ctr.Instance.ITEM_USING.item_name))
		{
			PopupControl.Instance.on_yes_pressed = delegate
			{
				inventory_ctr.Instance.DropExtraItemDataOnPickup(inventory_ctr.Instance.ITEM_USING);
				DonePlacing(false, false);
				EnterToolMode(new InventoryItem("Builder Tools"));
			};
			PopupControl.Instance.ShowYesNo("Are you sure you want to pick up this?\n<color=#ff392b>EVERYTHING INSIDE WILL DISAPPEAR!</color>", "Yes", "No", PopupControl.context.yesno_ACTION);
		}
		else if (InventoryUtils.UsesBasketId(inventory_ctr.Instance.ITEM_USING))
		{
			PopupControl.Instance.on_yes_pressed = delegate
			{
				inventory_ctr.Instance.DropExtraItemDataOnPickup(inventory_ctr.Instance.ITEM_USING);
				DonePlacing(false, false);
				EnterToolMode(new InventoryItem("Builder Tools"));
			};
			PopupControl.Instance.ShowYesNo("Are you sure you want to pick up this " + inventory_ctr.Instance.ITEM_USING.item_name + "?\n<color=#ff392b>EVERYTHING INSIDE WILL DISAPPEAR!</color>", "Yes", "No", PopupControl.context.yesno_ACTION);
		}
		else if (inventory_ctr.Instance.ITEM_USING.item_name == "Vending Machine")
		{
			PopupControl.Instance.on_yes_pressed = delegate
			{
				inventory_ctr.Instance.DropExtraItemDataOnPickup(inventory_ctr.Instance.ITEM_USING);
				DonePlacing(false, false);
				EnterToolMode(new InventoryItem("Builder Tools"));
			};
			PopupControl.Instance.ShowYesNo("Are you sure you want to pick up this Vending Machine?\n<color=#ff392b>EVERYTHING INSIDE WILL DISAPPEAR!</color>", "Yes", "No", PopupControl.context.yesno_ACTION);
		}
		else
		{
			EndDelete();
		}
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
		int num = innerX + modX;
		int num2 = innerZ + modZ;
		if (num >= 10)
		{
			num -= 10;
			chunkX++;
		}
		else if (num < 0)
		{
			num += 10;
			chunkX--;
		}
		if (num2 >= 10)
		{
			num2 -= 10;
			chunkZ++;
		}
		else if (num2 < 0)
		{
			num2 += 10;
			chunkZ--;
		}
		ChunkData chunkData = ChunkControl.Instance.GetChunkData(ChunkControl.Instance.GetChunkString(ChunkControl.Instance.player_zone, chunkX, chunkZ));
		if (chunkData == null)
		{
			return false;
		}
		foreach (ChunkElement item in chunkData.GetElementsAt(num, num2))
		{
			if (item.item.item_name == "Palisade Wall" || item.item.item_name == "Tall Stone Wall")
			{
				return true;
			}
		}
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
		string chunkString = ChunkControl.Instance.GetChunkString(zone, chunkX, chunkZ);
		if (build_context == build_context_t.on_self_build_new && builder_player == "ME" && GameServerConnector.Instance.FullyInGame() && !GameServerConnector.Instance.is_host)
		{
			builder_player = PlayerData.Instance.GetGlobalString("username_punctuated");
		}
		if (build_item.item_name == "3-day Land Claim" || build_item.item_name == "8-day Land Claim" || build_item.item_name == "Admin Land Claim")
		{
			LandClaimControl.Instance.AddLandClaimsToNearbyChunks(zone, chunkX, chunkZ, innerX, innerZ, build_item, builder_player);
		}
		else if (build_item.item_name == "Teleporter")
		{
			string title = build_item.GetString("teleporter_name");
			if (GameServerConnector.Instance.ShouldSaveLocally())
			{
				CustomTeleporterControl.Instance.CreateNewTeleporter(zone, chunkX, chunkZ, innerX, innerZ, title);
			}
			if (build_context == build_context_t.on_self_build_new)
			{
				CustomTeleporterControl.Instance.TakeScreenshot(chunkX, chunkZ, innerX, innerZ, true);
			}
		}
		else if (InventoryUtils.UsesShackId(build_item.item_name) && GameServerConnector.Instance.ShouldSaveLocally())
		{
			int @long = build_item.GetLong("shack_id");
			if (ZoneDataControl.Instance.PlayerZoneExists("shack" + @long))
			{
				ZoneDataControl.Instance.ModifyPlayerZone("shack" + @long, build_rot, build_item);
			}
			else
			{
				ZoneDataControl.Instance.CreatePlayerZone("shack" + @long, build_item, chunkX, chunkZ, innerX, innerZ, zone, (byte)build_rot);
			}
		}
		ChunkData chunkData = ChunkControl.Instance.GetChunkData(chunkString);
		if (chunkData == null && GameServerConnector.Instance.ShouldSaveLocally())
		{
			chunkData = ChunkControl.Instance.HostGetChunk(zone, chunkX, chunkZ);
		}
		if (chunkData != null)
		{
			chunkData.AddElement(innerX, innerZ, new ChunkElement(build_item, build_rot));
			chunkData.mp_cache_key = mp_cache_key;
		}
		if (GameServerConnector.Instance.ShouldSaveLocally())
		{
			chunkData.SaveWholeChunkToDisk(chunkString);
		}
		if (ChunkControl.Instance.IsChunkFullyLoadedOrMidload(chunkString))
		{
			Chunk chunk = ChunkControl.Instance.GetChunk(chunkString);
			AsyncCreateBuildableInstance(build_item, innerX, innerZ, build_rot, build_context, chunk, chunk.chunk_data, chunk.chunk_obj);
		}
		if (build_context == build_context_t.on_self_build_new)
		{
			GameServerSender.Instance.SendBuildFurniture(build_item, (byte)build_rot, ChunkControl.Instance.player_zone, chunkX, chunkZ, innerX, innerZ, mp_cache_key, "");
		}
	}

	public static string GenerateCacheKey()
	{
		string text = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789";
		string text2 = "";
		for (int i = 0; i < 8; i++)
		{
			text2 += text[UnityEngine.Random.Range(0, text.Length)];
		}
		return text2;
	}

	public void PlayerRemoveAt(ChunkElement remove_element, string zone, int chunkX, int chunkZ, int innerX, int innerZ, remove_context remove_context_t, string mp_cache_key)
	{
		string chunkString = ChunkControl.Instance.GetChunkString(zone, chunkX, chunkZ);
		string item_name = remove_element.item.item_name;
		if (item_name == "3-day Land Claim" || item_name == "8-day Land Claim" || item_name == "Admin Land Claim")
		{
			LandClaimControl.Instance.RemoveLandClaimsFromNearbyChunks(zone, chunkX, chunkZ, innerX, innerZ);
			if (remove_context_t == remove_context.self_remove)
			{
				GameplayGUIControl.Instance.ShowNotif("<color=#aaaaaa>Land unclaimed</color>", new InventoryItem("Old Land Claim"), 1, new OnNotifClick(OnNotifClick.type.none));
			}
		}
		if (item_name == "Companion" && remove_element.item.GetString("companion_mode") == "guard")
		{
			string key = ChunkControl.Instance.player_zone + "," + chunkX + "," + chunkZ + "," + innerX + "," + innerZ;
			if (MobControl.Instance.active_combatants.ContainsKey(key) && !MobControl.Instance.active_combatants[key].GetComponent<Combatant>().is_dead)
			{
				UnityEngine.Object.Destroy(MobControl.Instance.active_combatants[key]);
				MobControl.Instance.active_combatants.Remove(key);
			}
		}
		ChunkData chunkData = ChunkControl.Instance.GetChunkData(chunkString);
		if (chunkData == null && GameServerConnector.Instance.ShouldSaveLocally())
		{
			chunkData = ChunkControl.Instance.HostGetChunk(zone, chunkX, chunkZ);
		}
		if (chunkData != null)
		{
			chunkData.RemoveElement(innerX, innerZ, remove_element);
			chunkData.mp_cache_key = mp_cache_key;
		}
		if (GameServerConnector.Instance.ShouldSaveLocally())
		{
			if (item_name == "Teleporter")
			{
				CustomTeleporterControl.Instance.DeleteTeleporter(zone, chunkX, chunkZ, innerX, innerZ);
			}
			chunkData.SaveWholeChunkToDisk(chunkString);
		}
		if (ChunkControl.Instance.IsChunkFullyLoadedOrMidload(chunkString))
		{
			ChunkControl.Instance.GetChunkObj(chunkString).DestroyBuildableInstance(chunkX, chunkZ, innerX, innerZ, remove_element.item, remove_element.rot);
			if (item_name == "Music Box")
			{
				MusicBoxControl.Instance.remove_song(zone + "," + chunkX + "," + chunkZ + "," + innerX + "," + innerZ);
			}
			else if (inventory_ctr.Instance.GetItemBool(item_name, "is_flooring_obj"))
			{
				ModularObjectControl.Instance.ModularChangedAt(innerX, innerZ, ModularObjectControl.type.PATHWAYS, zone, chunkX, chunkZ);
			}
			else if (inventory_ctr.Instance.GetItemBool(item_name, "is_wall_obj"))
			{
				ModularObjectControl.Instance.ModularChangedAt(innerX, innerZ, ModularObjectControl.type.WALLS, zone, chunkX, chunkZ);
			}
			else if ((item_name == "Companion") ? (remove_element.item.GetString("companion_mode") != "guard") : (item_name == "DEBUG-npc" || InventoryUtils.IsChairObject(item_name) || InventoryUtils.IsBedObject(item_name)))
			{
				ChunkControl.Instance.RedrawAtSquare(chunkString, innerX, innerZ);
			}
		}
		if (remove_context_t == remove_context.self_remove)
		{
			GameServerSender.Instance.SendRemoveObject(zone, chunkX, chunkZ, innerX, innerZ, remove_element, mp_cache_key, "");
		}
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
		string chunkString = ChunkControl.Instance.GetChunkString(zone, chunkX, chunkZ);
		if (InventoryUtils.UsesShackId(old_element_item.item_name))
		{
			string text = "shack" + old_element_item.GetLong("shack_id");
			if (GameServerConnector.Instance.ShouldSaveLocally())
			{
				text += "-zonedata";
				string zoneDataFilename = ChunkControl.GetZoneDataFilename(text);
				new_item.SaveToDisk(zoneDataFilename, "zone_item", text);
			}
		}
		ChunkData chunkData = ChunkControl.Instance.GetChunkData(chunkString);
		if (chunkData == null && GameServerConnector.Instance.ShouldSaveLocally())
		{
			chunkData = ChunkControl.Instance.HostGetChunk(zone, chunkX, chunkZ);
		}
		if (chunkData != null)
		{
			chunkData.ReplaceElementItem(innerX, innerZ, new_item, old_element_item, old_element_rot);
			chunkData.mp_cache_key = mp_cache_key;
		}
		if (GameServerConnector.Instance.ShouldSaveLocally())
		{
			chunkData.SaveWholeChunkToDisk(chunkString);
		}
		if (ChunkControl.Instance.IsChunkFullyLoadedOrMidload(chunkString))
		{
			ChunkControl.Instance.GetChunk(chunkString);
			ChunkControl.Instance.GetChunkObj(chunkString).ReplaceElementItemInstance(chunkData.X, chunkData.Z, innerX, innerZ, new_item, old_element_item, old_element_rot, chunkData);
			bool itemBool = inventory_ctr.Instance.GetItemBool(old_element_item.item_name, "is_wall_obj");
			bool itemBool2 = inventory_ctr.Instance.GetItemBool(old_element_item.item_name, "is_flooring_obj");
			if (itemBool)
			{
				ModularObjectControl.Instance.ModularChangedAt(innerX, innerZ, ModularObjectControl.type.WALLS, zone, chunkX, chunkZ);
			}
			else if (itemBool2)
			{
				ModularObjectControl.Instance.ModularChangedAt(innerX, innerZ, ModularObjectControl.type.PATHWAYS, zone, chunkX, chunkZ);
			}
		}
		if (send)
		{
			GameServerSender.Instance.SendReplaceBuildable(new_item, old_element_item, old_element_rot, zone, chunkX, chunkZ, innerX, innerZ, mp_cache_key, "");
		}
		if (GameController.Instance.interacting_element_item == old_element_item && GameController.Instance.interacting_element_rot == old_element_rot && GameController.Instance.interacting_element_chunkX == chunkX && GameController.Instance.interacting_element_chunkZ == chunkZ && GameController.Instance.interacting_element_innerX == innerX && GameController.Instance.interacting_element_innerZ == innerZ && ChunkControl.Instance.player_zone == zone)
		{
			GameController.Instance.interacting_element_item = new_item;
		}
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
		InventoryItem item_weapon = display_item.LoadSubItem("wep");
		if (item_weapon.item_name == "")
		{
			string text = display_item.GetString("wep");
			if (text != "")
			{
				item_weapon = new InventoryItem(text);
			}
		}
		if (item_weapon.item_name != "" && inventory_ctr.Instance.GetItemType(item_weapon) == inventory_ctr.inv_type_t.holdable)
		{
			GameObject check_not_null = parent.gameObject;
			ResourceControl.Instance.AsyncInstantiateEquipment(inventory_ctr.Instance.GetItemWorldObjPath(item_weapon.item_name), delegate(GameObject weapon_instance)
			{
				if (check_not_null == null)
				{
					UnityEngine.Object.Destroy(weapon_instance);
				}
				else
				{
					weapon_instance.transform.SetParent(parent.transform.Find("holder0").transform);
					PaintDisplayWeapon(weapon_instance, item_weapon);
					PositionDisplayWeapon(item_weapon.item_name, weapon_instance, flip: false);
					on_weapon_added?.Invoke();
				}
			});
		}
		else
		{
			on_weapon_added?.Invoke();
		}
	}

	private Vector3 ParseVector3(string str, Vector3 default_)
	{
		if (string.IsNullOrWhiteSpace(str))
		{
			return default_;
		}
		string[] array = str.Split(',');
		if (array.Length != 3)
		{
			return default_;
		}
		return new Vector3(float.Parse(array[0].Trim(), Startup.parse_culture), float.Parse(array[1].Trim(), Startup.parse_culture), float.Parse(array[2].Trim(), Startup.parse_culture));
	}

	private Quaternion ParseQuaternion(string str)
	{
		if (string.IsNullOrWhiteSpace(str))
		{
			return Quaternion.identity;
		}
		string[] array = str.Split(',');
		if (array.Length != 3)
		{
			return Quaternion.identity;
		}
		return Quaternion.Euler(float.Parse(array[0].Trim(), Startup.parse_culture), float.Parse(array[1].Trim(), Startup.parse_culture), float.Parse(array[2].Trim(), Startup.parse_culture));
	}

	public void PaintDisplayWeapon(GameObject weapon_instance, InventoryItem item_weapon)
	{
		if (weapon_instance.GetComponent<PaintableObject>() != null)
		{
			string paintFromItemOrUseDefault = inventory_ctr.Instance.GetPaintFromItemOrUseDefault(item_weapon);
			string stampFromItem = inventory_ctr.Instance.GetStampFromItem(item_weapon);
			weapon_instance.GetComponent<PaintableObject>().Colorize(paintFromItemOrUseDefault, stampFromItem, inventory_ctr.Instance.GetLayoutItemFromItem(item_weapon.item_name), item_weapon.item_name, process_particles: true);
		}
	}

	public void PositionDisplayWeapon(string input_wep, GameObject obj, bool flip)
	{
		string stringFromItemFile = ResourceControl.Instance.GetStringFromItemFile(input_wep, "weapon_display_copy");
		string item_name = (string.IsNullOrWhiteSpace(stringFromItemFile) ? input_wep : stringFromItemFile);
		string stringFromItemFile2 = ResourceControl.Instance.GetStringFromItemFile(item_name, "weapon_display_localPosition");
		string stringFromItemFile3 = ResourceControl.Instance.GetStringFromItemFile(item_name, "weapon_display_localRotation");
		string stringFromItemFile4 = ResourceControl.Instance.GetStringFromItemFile(item_name, "weapon_display_localScale");
		obj.transform.localPosition = ParseVector3(stringFromItemFile2, Vector3.zero);
		obj.transform.localRotation = ParseQuaternion(stringFromItemFile3);
		obj.transform.localScale = ParseVector3(stringFromItemFile4, Vector3.one);
		if (ResourceControl.Instance.GetStringFromItemFile(input_wep, "dual_wield") == "true")
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(obj.transform.parent.gameObject);
			gameObject.transform.position = obj.transform.parent.position;
			gameObject.transform.rotation = obj.transform.parent.rotation;
			gameObject.transform.localScale = obj.transform.parent.localScale;
			gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y, 0f - gameObject.transform.localScale.z);
			gameObject.transform.SetParent(obj.transform.parent);
		}
	}

	public void DecorateLargeWeaponDisplay(InventoryItem display_item, GameObject parent, Action on_weapons_added = null)
	{
		InventoryItem item_weapon1 = display_item.LoadSubItem("wep");
		if (item_weapon1.item_name == "")
		{
			string text = display_item.GetString("wep");
			if (text != "")
			{
				item_weapon1 = new InventoryItem(text);
			}
		}
		InventoryItem item_weapon2 = display_item.LoadSubItem("wep2");
		if (item_weapon2.item_name == "")
		{
			string text2 = display_item.GetString("wep2");
			if (text2 != "")
			{
				item_weapon2 = new InventoryItem(text2);
			}
		}
		bool wep1_complete = false;
		bool wep2_complete = false;
		Action on_complete = delegate
		{
			if (wep1_complete && wep2_complete)
			{
				on_weapons_added?.Invoke();
			}
		};
		if (item_weapon1.item_name != "" && inventory_ctr.Instance.GetItemType(item_weapon1) == inventory_ctr.inv_type_t.holdable)
		{
			GameObject check_not_null = parent.gameObject;
			ResourceControl.Instance.AsyncInstantiateEquipment(inventory_ctr.Instance.GetItemWorldObjPath(item_weapon1.item_name), delegate(GameObject weapon1_instance)
			{
				if (check_not_null == null)
				{
					UnityEngine.Object.Destroy(weapon1_instance);
				}
				else
				{
					weapon1_instance.transform.SetParent(parent.transform.Find("holder0").transform);
					PaintDisplayWeapon(weapon1_instance, item_weapon1);
					PositionDisplayWeapon(item_weapon1.item_name, weapon1_instance, flip: true);
					wep1_complete = true;
					on_complete();
				}
			});
		}
		else
		{
			wep1_complete = true;
			on_complete();
		}
		if (item_weapon2.item_name != "" && inventory_ctr.Instance.GetItemType(item_weapon2) == inventory_ctr.inv_type_t.holdable)
		{
			GameObject check_not_null2 = parent.gameObject;
			ResourceControl.Instance.AsyncInstantiateEquipment(inventory_ctr.Instance.GetItemWorldObjPath(item_weapon2.item_name), delegate(GameObject weapon2_instance)
			{
				if (check_not_null2 == null)
				{
					UnityEngine.Object.Destroy(weapon2_instance);
				}
				else
				{
					weapon2_instance.transform.SetParent(parent.transform.Find("holder1").transform);
					PaintDisplayWeapon(weapon2_instance, item_weapon2);
					PositionDisplayWeapon(item_weapon2.item_name, weapon2_instance, flip: true);
					wep2_complete = true;
					on_complete();
				}
			});
		}
		else
		{
			wep2_complete = true;
			on_complete();
		}
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
		int num = innerX + modX;
		int num2 = innerZ + modZ;
		if (num >= 10)
		{
			num -= 10;
			chunkX++;
		}
		else if (num < 0)
		{
			chunkX--;
			num += 10;
		}
		if (num2 >= 10)
		{
			num2 -= 10;
			chunkZ++;
		}
		else if (num2 < 0)
		{
			chunkZ--;
			num2 += 10;
		}
		string chunkString = ChunkControl.Instance.GetChunkString(zone, chunkX, chunkZ);
		if (!ChunkControl.Instance.IsChunkFullyLoadedOrMidload(chunkString))
		{
			return;
		}
		string item = chunkString + "," + num + "," + num2;
		if (explored.Contains(item))
		{
			return;
		}
		explored.Add(item);
		foreach (ChunkElement item2 in ChunkControl.Instance.GetChunkData(chunkString).GetElementsAt(num, num2))
		{
			if (item2.item.item_name == old_item_no_stamp.item_name)
			{
				ExtraInventoryData extraDataCopy = item2.item.GetExtraDataCopy();
				extraDataCopy.SetString("stamp", "");
				if (new InventoryItem(item2.item.item_name, extraDataCopy) == old_item_no_stamp)
				{
					string @string = item2.item.GetString("stamp");
					ExtraInventoryData extraDataCopy2 = new_item_no_stamp.GetExtraDataCopy();
					extraDataCopy2.SetString("stamp", @string);
					PlayerReplaceAt(new InventoryItem(new_item_no_stamp.item_name, extraDataCopy2), item2.item, item2.rot, zone, chunkX, chunkZ, num, num2, true, GenerateCacheKey());
					ChainReplace(old_item_no_stamp, new_item_no_stamp, zone, chunkX, chunkZ, num, num2, 0, 1, explored);
					ChainReplace(old_item_no_stamp, new_item_no_stamp, zone, chunkX, chunkZ, num, num2, 1, 0, explored);
					ChainReplace(old_item_no_stamp, new_item_no_stamp, zone, chunkX, chunkZ, num, num2, 0, -1, explored);
					ChainReplace(old_item_no_stamp, new_item_no_stamp, zone, chunkX, chunkZ, num, num2, -1, 0, explored);
					break;
				}
			}
		}
	}

	private ToolUseResult ProcessToolClick(string chunkStr, string zone, int chunkX, int chunkZ, int innerX, int innerZ, ChunkElement element, Vector3 clickedAt)
	{
		if (!ChunkControl.Instance.IsChunkFullyLoadedOrMidload(chunkStr))
		{
			return new ToolUseResult(ToolUseResult.status.error_unknown);
		}
		ChunkData chunkData = ChunkControl.Instance.GetChunkData(chunkStr);
		ChunkControl.Instance.GetChunkObj(chunkStr);
		if (!LandClaimControl.Instance.AllowedToBuild(zone, chunkX, chunkZ))
		{
			return new ToolUseResult(ToolUseResult.status.error_unknown);
		}
		if (element.item.GetString("bandit_camp_instance") != "" && !BanditCampsControl.Instance.GetBanditCampInstanceByName(element.item.GetString("bandit_camp_instance")).flag_destroyed)
		{
			return new ToolUseResult(ToolUseResult.status.error_bandit_camp);
		}
		if (element.item.GetString("tag") == "dev_obj")
		{
			return new ToolUseResult(ToolUseResult.status.error_dev_obj);
		}
		if (element.item.GetString("tag") == "auto_built_immovable")
		{
			return new ToolUseResult(ToolUseResult.status.error_not_editable_at_all);
		}
		string item_name = element.item.item_name;
		if (InventoryUtils.IsPaintbrush(inventory_ctr.Instance.ITEM_USING.item_name))
		{
			if (!inventory_ctr.Instance.IsItemPaintable(element.item.item_name))
			{
				return new ToolUseResult(ToolUseResult.status.error_unknown);
			}
			if (!inventory_ctr.Instance.GetItemBool(element.item.item_name, "is_wall_obj") && !inventory_ctr.Instance.GetItemBool(element.item.item_name, "is_flooring_obj"))
			{
				ExtraInventoryData extraDataCopy = element.item.GetExtraDataCopy();
				extraDataCopy.SetString("paint", inventory_ctr.Instance.ITEM_USING.item_name);
				PlayerReplaceAt(new InventoryItem(element.item.item_name, extraDataCopy), element.item, element.rot, zone, chunkX, chunkZ, innerX, innerZ, true, GenerateCacheKey());
			}
			else
			{
				List<string> explored = new List<string>();
				ExtraInventoryData extraDataCopy2 = element.item.GetExtraDataCopy();
				extraDataCopy2.SetString("paint", inventory_ctr.Instance.ITEM_USING.item_name);
				extraDataCopy2.SetString("stamp", "");
				InventoryItem new_item_no_stamp = new InventoryItem(element.item.item_name, extraDataCopy2);
				ExtraInventoryData extraDataCopy3 = element.item.GetExtraDataCopy();
				extraDataCopy3.SetString("stamp", "");
				ChainReplace(new InventoryItem(element.item.item_name, extraDataCopy3), new_item_no_stamp, zone, chunkX, chunkZ, innerX, innerZ, 0, 0, explored);
			}
			DonePlacing(false, true);
			return new ToolUseResult(ToolUseResult.status.success);
		}
		if (InventoryUtils.IsStamp(inventory_ctr.Instance.ITEM_USING.item_name))
		{
			if (!inventory_ctr.Instance.IsItemPaintable(element.item.item_name))
			{
				return new ToolUseResult(ToolUseResult.status.error_unknown);
			}
			ExtraInventoryData extraDataCopy4 = element.item.GetExtraDataCopy();
			extraDataCopy4.SetString("stamp", inventory_ctr.Instance.ITEM_USING.item_name);
			PlayerReplaceAt(new InventoryItem(element.item.item_name, extraDataCopy4), element.item, element.rot, zone, chunkX, chunkZ, innerX, innerZ, true, GenerateCacheKey());
			DonePlacing(false, true);
			return new ToolUseResult(ToolUseResult.status.success);
		}
		if (inventory_ctr.Instance.ITEM_USING.item_name == "Paint Thinner")
		{
			if (inventory_ctr.Instance.IsItemPaintable(element.item.item_name))
			{
				string @string = element.item.GetString("paint");
				string string2 = element.item.GetString("stamp");
				if (!Startup.StringNullOrWhitespace(@string) || !Startup.StringNullOrWhitespace(string2))
				{
					ExtraInventoryData extraDataCopy5 = element.item.GetExtraDataCopy();
					extraDataCopy5.SetString("paint", "");
					extraDataCopy5.SetString("stamp", "");
					InventoryItem inventoryItem = new InventoryItem(element.item.item_name, extraDataCopy5);
					if (!inventory_ctr.Instance.GetItemBool(element.item.item_name, "is_wall_obj") && !inventory_ctr.Instance.GetItemBool(element.item.item_name, "is_flooring_obj"))
					{
						if (!Startup.StringNullOrWhitespace(@string))
						{
							inventory_ctr.Instance.GiveItem(@string, 1, "");
						}
						if (!Startup.StringNullOrWhitespace(string2))
						{
							inventory_ctr.Instance.GiveItem(string2, 1, "");
						}
						PlayerReplaceAt(inventoryItem, element.item, element.rot, zone, chunkX, chunkZ, innerX, innerZ, true, GenerateCacheKey());
					}
					else
					{
						List<string> explored2 = new List<string>();
						ExtraInventoryData extraDataCopy6 = element.item.GetExtraDataCopy();
						extraDataCopy6.SetString("stamp", "");
						ChainReplace(new InventoryItem(element.item.item_name, extraDataCopy6), inventoryItem, zone, chunkX, chunkZ, innerX, innerZ, 0, 0, explored2);
					}
					DonePlacing(false, true);
					return new ToolUseResult(ToolUseResult.status.success);
				}
			}
			return new ToolUseResult(ToolUseResult.status.error_unknown);
		}
		if (inventory_ctr.Instance.ITEM_USING.item_name == "Lock")
		{
			bool flag;
			if (InventoryUtils.IsHouseObject(item_name))
			{
				flag = false;
			}
			else
			{
				if (InventoryUtils.UsesBasketId(element.item))
				{
					if (item_name == "Crate" || item_name == "Double Crate" || item_name == "Trading Table")
					{
						PopupControl.Instance.ShowMessage(item_name + "s cannot be locked.");
						return new ToolUseResult(ToolUseResult.status.success);
					}
				}
				else if (item_name != "Music Box")
				{
					return new ToolUseResult(ToolUseResult.status.error_unknown);
				}
				flag = true;
			}
			if (element.item.GetString("password") != "")
			{
				PopupControl.Instance.ShowMessage("This " + item_name + " is already locked!");
			}
			else
			{
				GameController.Instance.NoteInteractingElement(chunkX, chunkZ, innerX, innerZ, element.item, element.rot);
				DonePlacing(false, false);
				LockControl.Instance.OpenLockScreen("Create a password for your " + item_name, LockControl.lock_context.create_password);
				if (flag)
				{
					GameServerSender.Instance.SendClaimObject(zone + "," + chunkX + "," + chunkZ + "," + innerX + "," + innerZ);
				}
			}
			return new ToolUseResult(ToolUseResult.status.success);
		}
		if (inventory_ctr.Instance.ITEM_USING.item_name == "Builder Tools")
		{
			if (!Startup.StringNullOrWhitespace(inventory_ctr.Instance.GetItemToolRequiredToMove(item_name)) && !GameServerConnector.Instance.is_moderator)
			{
				return new ToolUseResult(ToolUseResult.status.error_need_tool, inventory_ctr.Instance.GetItemToolRequiredToMove(item_name));
			}
			if (GameServerConnector.Instance.FullyInGame() && !GameServerConnector.Instance.is_moderator)
			{
				if (GameServerInterface.Instance.AnyoneUsing(zone + "," + chunkX + "," + chunkZ + "," + innerX + "," + innerZ))
				{
					return new ToolUseResult(ToolUseResult.status.error_someone_using);
				}
				if ((element.item.item_name == "3-day Land Claim" || element.item.item_name == "8-day Land Claim" || element.item.item_name == "Admin Land Claim") && !GameServerConnector.Instance.is_host)
				{
					string text = "";
					string key = zone + "," + chunkX + "," + chunkZ + "," + innerX + "," + innerZ;
					if (chunkData.land_claim_chunk_timers_.ContainsKey(key))
					{
						text = chunkData.land_claim_chunk_timers_[key].land_claim_user0;
					}
					if (PlayerData.Instance.GetGlobalString("username_lower") != text.ToLower())
					{
						return new ToolUseResult(ToolUseResult.status.error_land_claimed, text);
					}
				}
			}
			Vector3 inner = ChunkControl.Instance.GetInner(clickedAt);
			Vector3 chunkCoords = ChunkControl.Instance.GetChunkCoords(clickedAt);
			GrabFurniture(element, zone, chunkX, chunkZ, innerX, innerZ, (int)chunkCoords.x, (int)chunkCoords.z, (int)inner.x, (int)inner.z);
			return new ToolUseResult(ToolUseResult.status.success);
		}
		if (inventory_ctr.Instance.ITEM_USING.item_name == "Drill")
		{
			string string3 = element.item.GetString("tag");
			if (string3 == "dev_obj" || string3 == "auto_built_immovable")
			{
				return new ToolUseResult(ToolUseResult.status.error_unknown);
			}
			PopupControl.Instance.on_yes_pressed = delegate
			{
				PlayerRemoveAt(element, zone, chunkX, chunkZ, innerX, innerZ, remove_context.self_remove, GenerateCacheKey());
			};
			PopupControl.Instance.ShowYesNo("Are you sure you want to destroy this <color=#00bbff>" + element.item.item_name + "</color>?", "Yes", "No", PopupControl.context.yesno_ACTION);
			return new ToolUseResult(ToolUseResult.status.success);
		}
		if (!(inventory_ctr.Instance.ITEM_USING.item_name == "Shovel") && !(inventory_ctr.Instance.ITEM_USING.item_name == "Magma Shovel") && !(inventory_ctr.Instance.ITEM_USING.item_name == "Titanium Shovel"))
		{
			return new ToolUseResult(ToolUseResult.status.error_unknown);
		}
		InventoryItem inventoryItem2 = null;
		switch (inventory_ctr.Instance.GetItemToolRequiredToMove(item_name))
		{
		case "Shovel":
			if (inventory_ctr.Instance.ITEM_USING.item_name == "Shovel")
			{
				inventoryItem2 = element.item;
				break;
			}
			goto case "Titanium Shovel";
		case "Titanium Shovel":
			if (inventory_ctr.Instance.ITEM_USING.item_name == "Titanium Shovel")
			{
				inventoryItem2 = element.item;
				break;
			}
			goto case "Magma Shovel";
		case "Magma Shovel":
			if (inventory_ctr.Instance.ITEM_USING.item_name == "Magma Shovel")
			{
				inventoryItem2 = element.item;
			}
			break;
		}
		if (inventoryItem2 == null)
		{
			return new ToolUseResult(ToolUseResult.status.error_unknown);
		}
		if (!inventory_ctr.Instance.CanReceiveItem(inventoryItem2, 1))
		{
			GameController.Instance.showOverheadNotif(TranslationControl.Instance.TranslateGeneral("Inventory Full!", "GUI"), GameController.Instance.player.transform.position, false, true);
		}
		else
		{
			inventory_ctr.Instance.DropExtraItemDataOnPickup(new InventoryItem(inventoryItem2.item_name, inventoryItem2.GetExtraDataCopy()));
			PlayerRemoveAt(element, zone, chunkX, chunkZ, innerX, innerZ, remove_context.self_remove, GenerateCacheKey());
		}
		return new ToolUseResult(ToolUseResult.status.success);
	}

	public void DeleteUnnecessaryComponents(InventoryItem item, GameObject new_obj)
	{
		string item_name = item.item_name;
		if (item_name == "Armor Display" || item_name == "Custom Statue")
		{
			new_obj.GetComponent<Interactable>().deleted = true;
			UnityEngine.Object.Destroy(new_obj.GetComponent<Interactable>());
			new_obj.transform.Find("collider").gameObject.SetActive(false);
		}
		else if (item_name == "Companion" || item_name == "Bonsai Tree")
		{
			new_obj.GetComponent<Interactable>().deleted = true;
			UnityEngine.Object.Destroy(new_obj.GetComponent<Interactable>());
		}
		else if (item_name == "3-day Land Claim" || item_name == "8-day Land Claim" || item_name == "Admin Land Claim")
		{
			new_obj.GetComponent<Interactable>().deleted = true;
			UnityEngine.Object.Destroy(new_obj.GetComponent<Interactable>());
			UnityEngine.Object.Destroy(new_obj.transform.Find("Quad").gameObject);
		}
		else if (item_name == "Painting")
		{
			new_obj.transform.Find("Interactable").GetComponent<Interactable>().deleted = true;
			UnityEngine.Object.Destroy(new_obj.transform.Find("Interactable").GetComponent<Interactable>());
		}
		else if (!InventoryUtils.IsStringItem(item_name))
		{
			DeleteUnnecessaryComponentsRecursive(new_obj.transform);
		}
	}

	private void DeleteUnnecessaryComponentsRecursive(Transform T)
	{
		Component[] components = T.GetComponents<Component>();
		for (int i = 0; i < components.Length; i++)
		{
			Component component = components[i];
			if (!(component.GetType() == typeof(Transform)) && !(component.GetType() == typeof(MeshFilter)) && !(component.GetType() == typeof(ParticleSystem)) && !(component.GetType() == typeof(ParticleSystemRenderer)) && !(component.GetType() == typeof(MeshRenderer)) && !(component.GetType() == typeof(UnityEngine.Rendering.SortingGroup)) && !(component.GetType() == typeof(Spin)))
			{
				if (component.GetType() == typeof(Collectible))
				{
					((Collectible)component).deleted = true;
				}
				else if (component.GetType() == typeof(Interactable))
				{
					((Interactable)component).deleted = true;
				}
				else if (component.GetType() == typeof(Collectible))
				{
					((Collectible)component).deleted = true;
				}
				else if (component.GetType() == typeof(Combatant))
				{
					((Combatant)component).is_dead = true;
				}
				UnityEngine.Object.Destroy(components[i]);
			}
		}
		foreach (Transform item in T)
		{
			DeleteUnnecessaryComponentsRecursive(item);
		}
	}

	public void CreateMannequin(Transform parent_obj, InventoryItem mannequin_item, Action on_mannequin_created = null)
	{
		string item_name = mannequin_item.item_name;
		string @string = mannequin_item.GetString("creature_A");
		string string2 = mannequin_item.GetString("creature_B");
		List<InventoryItem> list = new List<InventoryItem>();
		InventoryItem inventoryItem = new InventoryItem("");
		InventoryItem inventoryItem2 = new InventoryItem("");
		InventoryItem inventoryItem3 = new InventoryItem("");
		if (mannequin_item.GetString("tag") == "dev_obj")
		{
			ExtraInventoryData extraInventoryData = new ExtraInventoryData();
			extraInventoryData.SetString("paint", mannequin_item.GetString("hat_paint"));
			inventoryItem = new InventoryItem(mannequin_item.GetString("hat"), extraInventoryData);
			ExtraInventoryData extraInventoryData2 = new ExtraInventoryData();
			extraInventoryData2.SetString("paint", mannequin_item.GetString("armor_paint"));
			inventoryItem2 = new InventoryItem(mannequin_item.GetString("body"), extraInventoryData2);
			ExtraInventoryData extraInventoryData3 = new ExtraInventoryData();
			extraInventoryData3.SetString("paint", mannequin_item.GetString("hand_paint"));
			inventoryItem3 = new InventoryItem(mannequin_item.GetString("wep"), extraInventoryData3);
		}
		else if (mannequin_item.item_name == "Armor Display" || mannequin_item.item_name == "Custom Statue")
		{
			inventoryItem = mannequin_item.LoadSubItem("hat");
			inventoryItem2 = mannequin_item.LoadSubItem("body");
			inventoryItem3 = mannequin_item.LoadSubItem("wep");
		}
		else if (mannequin_item.item_name == "Companion")
		{
			ItemCountPair[] itemListFromItem = ChunkControl.Instance.GetItemListFromItem("pockets", mannequin_item);
			inventoryItem = itemListFromItem[3].item;
			inventoryItem2 = itemListFromItem[8].item;
			inventoryItem3 = itemListFromItem[13].item;
		}
		if (inventoryItem.item_name != "")
		{
			list.Add(inventoryItem);
		}
		if (inventoryItem2.item_name != "")
		{
			list.Add(inventoryItem2);
		}
		if (inventoryItem3.item_name != "")
		{
			list.Add(inventoryItem3);
		}
		bool hat_ready = false;
		bool armor_ready = false;
		bool weapon_ready = false;
		bool scale_applied = false;
		bool hybrid_model_ready = false;
		Action on_any_part_ready = delegate
		{
			if (hat_ready && armor_ready && weapon_ready && scale_applied && hybrid_model_ready)
			{
				on_mannequin_created?.Invoke();
			}
		};
		GameObject hybridLite = CreatureMorpher.Instance.GetHybridLite(new List<string> { @string, string2 }, delegate
		{
			hybrid_model_ready = true;
			on_any_part_ready();
		});
		hybridLite.transform.SetParent(parent_obj);
		hybridLite.transform.localPosition = Vector3.zero;
		hybridLite.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
		hybridLite.GetComponent<LiteModel>().StopAnimation();
		InventoryItem inventoryItem4 = new InventoryItem("");
		InventoryItem inventoryItem5 = new InventoryItem("");
		InventoryItem inventoryItem6 = new InventoryItem("");
		foreach (InventoryItem item in list)
		{
			if (!(item.item_name != ""))
			{
				continue;
			}
			switch (inventory_ctr.Instance.GetItemType(item))
			{
			case inventory_ctr.inv_type_t.holdable:
				if ((item_name == "Custom Statue" || item_name == "Companion" || item_name == "DEBUG-npc") && inventoryItem6.item_name == "")
				{
					inventoryItem6 = item;
				}
				break;
			case inventory_ctr.inv_type_t.armor:
				if (inventoryItem5.item_name == "")
				{
					inventoryItem5 = item;
				}
				break;
			case inventory_ctr.inv_type_t.helmet:
				if (inventoryItem4.item_name == "")
				{
					inventoryItem4 = item;
				}
				break;
			}
		}
		string text = "";
		if (item_name == "Armor Display" || item_name == "Companion")
		{
			text = InventoryUtils.GetEquipmentSkinMat(inventoryItem4.item_name, inventoryItem5.item_name);
		}
		Material material;
		Material material2;
		if (text != "")
		{
			material = MobControl.Instance.GetSkinMaterialByName(text);
			material2 = material;
		}
		else if (item_name == "DEBUG-npc")
		{
			material = MobControl.Instance.GetSkinMaterialByName(mannequin_item.GetString("npc_material"));
			material2 = material;
		}
		else if (item_name == "Armor Display")
		{
			material = parent_obj.parent.GetComponent<PaintableObject>().target_meshes[1].renderer.material;
			material2 = null;
		}
		else if (item_name == "Custom Statue")
		{
			material = parent_obj.parent.GetComponent<PaintableObject>().target_meshes[1].renderer.material;
			material2 = parent_obj.parent.GetComponent<PaintableObject>().target_meshes[1].renderer.material;
		}
		else
		{
			material = null;
			material2 = null;
		}
		if (material != null)
		{
			hybridLite.GetComponent<LiteModel>().ApplySpecialMaterial(material);
		}
		foreach (InventoryItem item2 in list)
		{
			if (!(item2.item_name != ""))
			{
				continue;
			}
			switch (inventory_ctr.Instance.GetItemType(item2))
			{
			case inventory_ctr.inv_type_t.armor:
				if (inventoryItem5.item_name != "")
				{
					hybridLite.GetComponent<LiteModel>().ApplyArmor(item2, material2, delegate
					{
						armor_ready = true;
						on_any_part_ready();
					});
				}
				break;
			case inventory_ctr.inv_type_t.helmet:
				if (inventoryItem4.item_name != "")
				{
					hybridLite.GetComponent<LiteModel>().ApplyHat(item2, material2, delegate
					{
						hat_ready = true;
						on_any_part_ready();
					});
				}
				break;
			case inventory_ctr.inv_type_t.holdable:
				if ((item_name == "Custom Statue" || item_name == "Companion" || item_name == "DEBUG-npc") && inventoryItem6.item_name != "")
				{
					hybridLite.GetComponent<LiteModel>().ApplyWeapon(item2, material2, delegate
					{
						weapon_ready = true;
						on_any_part_ready();
					});
				}
				break;
			}
		}
		if (inventoryItem4.item_name == "")
		{
			hat_ready = true;
		}
		if (inventoryItem5.item_name == "")
		{
			armor_ready = true;
		}
		if (inventoryItem6.item_name == "")
		{
			weapon_ready = true;
		}
		float num = 1f;
		if (item_name == "DEBUG-npc")
		{
			if (mannequin_item.GetShort("npc_scale_x10") != 0)
			{
				num = (float)mannequin_item.GetShort("npc_scale_x10") / 10f;
			}
		}
		else if (!(item_name == "Armor Display"))
		{
			if (item_name == "Custom Statue")
			{
				num = 0.75f;
			}
			else if (item_name == "Companion")
			{
				num = InventoryUtils.GetEquipmentScale(inventoryItem4.item_name, inventoryItem5.item_name);
			}
		}
		hybridLite.transform.localScale = num * Vector3.one;
		if (item_name == "Companion" || item_name == "DEBUG-npc")
		{
			hybridLite.GetComponent<LiteModel>().StartAnimation(0);
			Interactable component = parent_obj.transform.parent.GetComponent<Interactable>();
			component.overhead_model_snap = hybridLite.GetComponent<LiteModel>();
			component.interaction_distance = num;
			component.circle_size = num - 1f - 0.1f + 1f;
		}
		scale_applied = true;
		on_any_part_ready();
	}

	private Vector3 SnapMousePositionToObjectOrigins(Vector3 original_position, InventoryItem item_placing)
	{
		if (item_placing.item_name == "Companion" && item_placing.GetString("companion_mode") != "guard")
		{
			foreach (OccupiedSpace item in ChunkControl.Instance.GetBuildablesThatOverlapThisSpace(original_position))
			{
				string chunkString = ChunkControl.Instance.GetChunkString(item.origin);
				if (!ChunkControl.Instance.IsChunkFullyLoadedOrMidload(chunkString))
				{
					continue;
				}
				Vector3 inner = ChunkControl.Instance.GetInner(item.origin);
				foreach (ChunkElement item2 in ChunkControl.Instance.GetChunkData(chunkString).GetElementsAt((int)inner.x, (int)inner.z))
				{
					if (InventoryUtils.IsChairObject(item2.item.item_name) || InventoryUtils.IsBedObject(item2.item.item_name))
					{
						return item.origin;
					}
				}
			}
		}
		return original_position;
	}

	public bool TryMakeCompanionSitOnChair(ChunkObj chunkObj, ChunkData chunk_data, int x, int z, InventoryItem companion_item, GameObject override_companionObj = null, GameObject override_chairObj = null)
	{
		if (chunkObj == null)
		{
			return false;
		}
		if (override_companionObj == null)
		{
			override_companionObj = chunkObj.GetBuildableInstanceByItem(x, z, companion_item);
		}
		if (override_companionObj == null)
		{
			return false;
		}
		GameObject gameObject = override_companionObj.transform.Find("creature-go-here").gameObject;
		InventoryItem inventoryItem = null;
		InventoryItem inventoryItem2 = null;
		foreach (ChunkElement item in chunk_data.GetElementsAt(x, z))
		{
			string item_name = item.item.item_name;
			if (InventoryUtils.IsChairObject(item_name))
			{
				inventoryItem = item.item;
				break;
			}
			if (InventoryUtils.IsBedObject(item_name))
			{
				inventoryItem2 = item.item;
				break;
			}
		}
		if (inventoryItem == null && inventoryItem2 == null)
		{
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			return false;
		}
		if (override_chairObj == null)
		{
			if (inventoryItem != null)
			{
				override_chairObj = chunkObj.GetBuildableInstanceByItem(x, z, inventoryItem);
			}
			else if (inventoryItem2 != null)
			{
				override_chairObj = chunkObj.GetBuildableInstanceByItem(x, z, inventoryItem2);
			}
		}
		if (override_chairObj == null)
		{
			return false;
		}
		GameObject gameObject2 = override_companionObj.transform.Find("creature-go-here").gameObject;
		gameObject2.transform.SetParent(null);
		Transform transform = override_chairObj.transform.Find("target");
		gameObject2.transform.position = transform.position;
		gameObject2.transform.rotation = transform.rotation;
		gameObject2.transform.Rotate(Vector3.up, 180f);
		if (inventoryItem != null)
		{
			gameObject2.transform.GetChild(0).GetComponent<LiteModel>().StartAnimation(4);
		}
		else if (inventoryItem2 != null)
		{
			gameObject2.transform.GetChild(0).GetComponent<LiteModel>().StartAnimation(5);
		}
		gameObject2.transform.SetParent(override_companionObj.transform);
		return true;
	}

	public void PlayerReplaceInteracting(InventoryItem new_item, bool send)
	{
		PlayerReplaceAt(new_item, GameController.Instance.interacting_element_item, GameController.Instance.interacting_element_rot, ChunkControl.Instance.player_zone, GameController.Instance.interacting_element_chunkX, GameController.Instance.interacting_element_chunkZ, GameController.Instance.interacting_element_innerX, GameController.Instance.interacting_element_innerZ, send, GenerateCacheKey());
	}

	public void ClickStatuePropertiesChangeAnimal1(int dir)
	{
		InventoryItem interacting_element_item = GameController.Instance.interacting_element_item;
		int num = CreatureMorpher.Instance.GetCreatureIndex_(interacting_element_item.GetString("creature_A")) + dir;
		int numCreatures = CreatureMorpher.Instance.GetNumCreatures();
		int index = num;
		if (num >= numCreatures)
		{
			index = 0;
		}
		if (num < 0)
		{
			index = numCreatures - 1;
		}
		ExtraInventoryData extraDataCopy = interacting_element_item.GetExtraDataCopy();
		extraDataCopy.SetString("creature_A", CreatureMorpher.Instance.GetCreatureName_(index));
		PlayerReplaceInteracting(new InventoryItem(interacting_element_item.item_name, extraDataCopy), true);
		RefreshAdvancedPropertiesCreatures();
	}

	public void ClickStatuePropertiesChangeAnimal2(int dir)
	{
		InventoryItem interacting_element_item = GameController.Instance.interacting_element_item;
		int num = CreatureMorpher.Instance.GetCreatureIndex_(interacting_element_item.GetString("creature_B")) + dir;
		int numCreatures = CreatureMorpher.Instance.GetNumCreatures();
		int index = num;
		if (num >= numCreatures)
		{
			index = 0;
		}
		if (num < 0)
		{
			index = numCreatures - 1;
		}
		ExtraInventoryData extraDataCopy = interacting_element_item.GetExtraDataCopy();
		extraDataCopy.SetString("creature_B", CreatureMorpher.Instance.GetCreatureName_(index));
		PlayerReplaceInteracting(new InventoryItem(interacting_element_item.item_name, extraDataCopy), true);
		RefreshAdvancedPropertiesCreatures();
	}

	public void ClickStatuePropertiesAccept()
	{
		inventory_ctr.Instance.ShowInventoryTab(true);
		if (!(GameController.Instance.interacting_element_item.item_name == "Armor Display") && GameController.Instance.interacting_element_item.item_name == "Custom Statue")
		{
			InventoryItem interacting_element_item = GameController.Instance.interacting_element_item;
			string text = WindowPrefabsControl.Instance.GetObject("INVENTORY-statue advanced", "message_input_1").transform.Find("Text").GetComponent<Text>().text;
			string text2 = WindowPrefabsControl.Instance.GetObject("INVENTORY-statue advanced", "message_input_2").transform.Find("Text").GetComponent<Text>().text;
			ExtraInventoryData extraDataCopy = interacting_element_item.GetExtraDataCopy();
			extraDataCopy.SetString("statue_message1", text);
			extraDataCopy.SetString("statue_message2", text2);
			PlayerReplaceInteracting(new InventoryItem(interacting_element_item.item_name, extraDataCopy), true);
		}
		WindowPrefabsControl.Instance.DestroyScreen("INVENTORY-statue advanced");
	}

	public void ClickStatueAdvancedProperties()
	{
		inventory_ctr.Instance.HideInventoryTab(true);
		WindowPrefabsControl.Instance.CreateScreen("INVENTORY-statue advanced", WindowPrefabsControl.build_into_t.mini_window);
		string text = GameController.Instance.interacting_element_item.GetString("statue_message1");
		string text2 = GameController.Instance.interacting_element_item.GetString("statue_message2");
		if (Startup.StringNullOrWhitespace(text) && Startup.StringNullOrWhitespace(text2))
		{
			text = TranslationControl.Instance.TranslateGeneral(CompanionController.default_statue_message1, "CompanionsEtc");
			text2 = TranslationControl.Instance.TranslateGeneral(CompanionController.default_statue_message2, "CompanionsEtc");
		}
		WindowPrefabsControl.Instance.GetObject("INVENTORY-statue advanced", "message_input_1").GetComponent<InputField>().SetTextWithoutNotify(text);
		WindowPrefabsControl.Instance.GetObject("INVENTORY-statue advanced", "message_input_2").GetComponent<InputField>().SetTextWithoutNotify(text2);
		if (GameController.Instance.interacting_element_item.item_name == "Armor Display")
		{
			WindowPrefabsControl.Instance.GetObject("INVENTORY-statue advanced", "message_strip").SetActive(false);
			GameObject @object = WindowPrefabsControl.Instance.GetObject("INVENTORY-statue advanced", "creature_strip");
			Vector3 localPosition = @object.transform.localPosition;
			@object.transform.localPosition = new Vector3(0f, localPosition.y, 0f);
		}
		RefreshAdvancedPropertiesCreatures();
	}

	private void RefreshAdvancedPropertiesCreatures()
	{
		string @string = GameController.Instance.interacting_element_item.GetString("creature_A");
		string string2 = GameController.Instance.interacting_element_item.GetString("creature_B");
		WindowPrefabsControl.Instance.GetTextLegacy("INVENTORY-statue advanced", "creature1_name").text = @string;
		WindowPrefabsControl.Instance.GetTextLegacy("INVENTORY-statue advanced", "creature2_name").text = string2;
		ResourceControl.Instance.AssignCreatureSprite(@string, WindowPrefabsControl.Instance.GetImage("INVENTORY-statue advanced", "creature1_img"));
		ResourceControl.Instance.AssignCreatureSprite(string2, WindowPrefabsControl.Instance.GetImage("INVENTORY-statue advanced", "creature2_img"));
	}

	public void PressNavpostColor(int index)
	{
		GameObject @object = WindowPrefabsControl.Instance.GetObject("NAV POST", "col" + index);
		switch (index)
		{
		case 0:
			edit_navpost_col = "white";
			break;
		case 1:
			edit_navpost_col = "red";
			break;
		case 2:
			edit_navpost_col = "orange";
			break;
		case 3:
			edit_navpost_col = "yellow";
			break;
		case 4:
			edit_navpost_col = "green";
			break;
		case 5:
			edit_navpost_col = "cyan";
			break;
		case 6:
			edit_navpost_col = "blue";
			break;
		case 7:
			edit_navpost_col = "purple";
			break;
		case 8:
			edit_navpost_col = "pink";
			break;
		}
		WindowPrefabsControl.Instance.GetObject("NAV POST", "col_selector").transform.localPosition = @object.transform.localPosition;
	}

	public void PressNavpostAccept()
	{
		string text = WindowPrefabsControl.Instance.GetTextLegacy("NAV POST", "navpost_input").text;
		if (Startup.StringNullOrEmpty(text))
		{
			PopupControl.Instance.ShowMessage("Enter a town name!");
			return;
		}
		ExtraInventoryData extraDataCopy = GameController.Instance.interacting_element_item.GetExtraDataCopy();
		extraDataCopy.SetString("sign_text", text);
		extraDataCopy.SetString("text_col", edit_navpost_col);
		PlayerReplaceInteracting(new InventoryItem(GameController.Instance.interacting_element_item.item_name, extraDataCopy), true);
		GameplayGUIControl.Instance.ShowNotif(FormatNavpostString(text, edit_navpost_col), new InventoryItem("Navpost"), 1, new OnNotifClick(OnNotifClick.type.none));
		WindowControl.Instance.CloseMiniwindow(true);
	}

	public string FormatNavpostString(string text, string col)
	{
		switch (col)
		{
		case "green":
			return "Town of <color=#74ff4d>" + text + "</color>";
		case "yellow":
			return "Town of <color=#fff64d>" + text + "</color>";
		case "pink":
			return "Town of <color=#ef5eff>" + text + "</color>";
		case "red":
			return "Town of <color=#FF6666>" + text + "</color>";
		case "orange":
			return "Town of <color=#ffc354>" + text + "</color>";
		case "cyan":
			return "Town of <color=#4dffff>" + text + "</color>";
		case "blue":
			return "Town of <color=#4da3ff>" + text + "</color>";
		case "purple":
			return "Town of <color=#a270ff>" + text + "</color>";
		case "white":
			return "Town of <color=#ffffff>" + text + "</color>";
		default:
			return text;
		}
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
