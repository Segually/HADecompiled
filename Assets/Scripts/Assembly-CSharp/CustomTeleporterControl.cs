using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomTeleporterControl : MonoBehaviour, OrderedStart
{
	public enum tele_type
	{
		hardcoded = 0,
		custom_local = 1,
		custom_network = 2
	}

	[Serializable]
	public struct teleport_location
	{
		public string name;

		public string name_POR;

		public string name_RUS;

		public string name_IND;

		public string name_SPN;

		public string name_TAI;

		public string description;

		public string description_POR;

		public string description_RUS;

		public string description_IND;

		public string description_SPN;

		public string description_TAI;

		public int min_noobia_version;

		public Sprite sprite;

		public string to_zone;

		public int to_chunkX;

		public int to_chunkZ;

		public int to_innerX;

		public int to_innerZ;
	}

	public static CustomTeleporterControl Instance;

	public bool disable_teleport_button;

	public GameObject tele_search_button;

	public OnlineTeleporter teleporter_L;

	public OnlineTeleporter teleporter_mid;

	public OnlineTeleporter teleporter_R;

	public Color[] possible_colors;

	public bool save_window_open;

	public Texture2D custom_tele_mask;

	public Texture2D custom_tele_overlay;

	private RenderTexture teleporter_screenshot_render_texture;

	private Texture2D teleporter_screenshot_texture2D;

	private const int resWidth = 150;

	private Vector3 take_screenshot_pos;

	private int screenshot_timer;

	private bool relay_screenshot;

	public bool in_search_screen;

	public int search_page;

	public string click_teleport_zone_to = "";

	public int click_teleport_to_chunkX;

	public int click_teleport_to_chunkZ;

	public int click_teleport_to_innerX;

	public int click_teleport_to_innerZ;

	public GameObject prefab_teleport;

	private List<Texture2D> temp_teleporter_textures = new List<Texture2D>();

	public int curr_hardcoded_teleporters_page;

	public teleport_location[] hardcoded_teleports;

	public void Start_0()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	public void Start_1()
	{
		teleporter_screenshot_render_texture = new RenderTexture(150, 150, 16);
		teleporter_screenshot_texture2D = new Texture2D(150, 150, TextureFormat.RGB24, false);
	}

	private void OnDestroy()
	{
		if (teleporter_screenshot_render_texture != null)
		{
			teleporter_screenshot_render_texture.Release();
			UnityEngine.Object.Destroy(teleporter_screenshot_render_texture);
		}
		if (teleporter_screenshot_texture2D != null)
		{
			UnityEngine.Object.Destroy(teleporter_screenshot_texture2D);
		}
	}

	public int GetNumberOfActiveTeleporters()
	{
		short slotShort = PlayerData.Instance.GetSlotShort("n_teleporters", PlayerData.filename_t.teleporters);
		int num = 0;
		for (int i = 0; i < slotShort; i++)
		{
			if (PlayerData.Instance.GetSlotShort("teleporter_" + i + "_deleted", PlayerData.filename_t.teleporters) == 0)
			{
				num++;
			}
		}
		return num;
	}

	public void OnLeftMiniwindowTabPressed(int hard_coded_skip_to_page)
	{
		WindowPrefabsControl.Instance.DestroyScreen("Teleport-nonebuilt");
		inventory_ctr.Instance.curr_crafting_list = null;
		inventory_ctr.Instance.goto_crafting_tab(hard_coded_skip_to_page);
	}

	public void OnRightMiniwindowTabPressed()
	{
		if (GameServerConnector.Instance.FullyInGame() && !GameServerConnector.Instance.is_host)
		{
			inventory_ctr.Instance.crafting_Tab.SetActive(true);
			inventory_ctr.Instance.LayOutCraftingTab(inventory_ctr.background_strip_layout.normal);
			PopupControl.Instance.ShowConnecting("Loading teleporters");
			GameServerSender.Instance.RequestPageOfTeleportersByPageNumber(0, false);
		}
		else if (GetNumberOfActiveTeleporters() == 0)
		{
			inventory_ctr.Instance.HideCraftingTab();
			WindowPrefabsControl.Instance.CreateScreen("Teleport-nonebuilt", WindowPrefabsControl.build_into_t.mini_window);
			WindowPrefabsControl.Instance.GetObject("Teleport-nonebuilt", "tele_spr").GetComponent<ItemSprite>().RedrawBasicIgnorePremium(new InventoryItem("Teleporter"), 1);
		}
		else
		{
			inventory_ctr.Instance.curr_crafting_list = null;
			inventory_ctr.Instance.goto_crafting_tab(0);
		}
	}

	public void PressEditTeleColor(int index)
	{
		ExtraInventoryData extraDataCopy = GameController.Instance.interacting_element_item.GetExtraDataCopy();
		extraDataCopy.SetShort("col_id", index);
		ConstructionControl.Instance.PlayerReplaceInteracting(new InventoryItem("Teleporter", extraDataCopy), true);
		WindowPrefabsControl.Instance.GetObject("Teleport-save", "edit-tele-col-selector").transform.localPosition = WindowPrefabsControl.Instance.GetObject("Teleport-save", "edit-tele-col-" + index).transform.localPosition;
		TakeScreenshot(GameController.Instance.interacting_element_chunkX, GameController.Instance.interacting_element_chunkZ, GameController.Instance.interacting_element_innerX, GameController.Instance.interacting_element_innerZ, false);
	}

	public void VisuallyTeleportMainPlayer(string tele_str)
	{
		WindowControl.Instance.CloseMiniwindow(false);
		if (!(GameController.Instance.player == null))
		{
			if (GameController.Instance.player.GetComponent<SharedCreature>().snapped_to_chair_obj)
			{
				GameplayGUIControl.Instance.end_sit_button.SetActive(false);
			}
			VisuallyTeleportSomeone(GameController.Instance.player);
			AudioControl.Instance.PlayTeleportSfx();
			if (GameServerConnector.Instance.FullyInGame())
			{
				GameServerSender.Instance.SendStartTeleport(tele_str);
			}
			GameplayGUIControl.Instance.HideGameplayGui();
			GameController.Instance.PAUSE_GAME();
		}
	}

	public void ColorizeTeleporter_(GameObject tele_obj, int colorID)
	{
		if (tele_obj == null)
		{
			return;
		}
		tele_obj.transform.Find("green-glow").GetComponent<MeshRenderer>().material.color = possible_colors[colorID];
		if (tele_obj.transform.Find("upward-stream") != null)
		{
			tele_obj.transform.Find("upward-stream").GetComponent<ParticleSystemRenderer>().material.color = (possible_colors[colorID] - Color.white) * 0.2f + Color.white;
		}
		if (tele_obj.transform.Find("swirls") != null)
		{
			tele_obj.transform.Find("swirls").GetComponent<ParticleSystemRenderer>().material.color = (possible_colors[colorID] - Color.white) * 0.2f + Color.white;
		}
	}

	public int GetHardcodedTeleId(string chunkStr, int innerX, int innerZ)
	{
		for (int i = 0; i < hardcoded_teleports.Length; i++)
		{
			teleport_location teleport_location = hardcoded_teleports[i];
			string chunkString = ChunkControl.Instance.GetChunkString(teleport_location.to_zone, teleport_location.to_chunkX, teleport_location.to_chunkZ);
			if (teleport_location.to_innerZ == innerZ && teleport_location.to_innerX == innerX && chunkString == chunkStr)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetCustomTeleId(string zone, int chunkX, int chunkZ, int innerX, int innerZ)
	{
		short slotShort = PlayerData.Instance.GetSlotShort("n_teleporters", PlayerData.filename_t.teleporters);
		for (int i = 0; i < slotShort; i++)
		{
			string text = "teleporter_" + i;
			string slotString = PlayerData.Instance.GetSlotString(text + "_to_zone", PlayerData.filename_t.teleporters);
			short slotShort2 = PlayerData.Instance.GetSlotShort(text + "_to_chunkX", PlayerData.filename_t.teleporters);
			short slotShort3 = PlayerData.Instance.GetSlotShort(text + "_to_chunkZ", PlayerData.filename_t.teleporters);
			short slotShort4 = PlayerData.Instance.GetSlotShort(text + "_to_innerX", PlayerData.filename_t.teleporters);
			short slotShort5 = PlayerData.Instance.GetSlotShort(text + "_to_innerZ", PlayerData.filename_t.teleporters);
			if (innerZ == slotShort5 && slotShort4 == innerX && slotShort3 == chunkZ && slotShort2 == chunkX && zone == slotString)
			{
				return i;
			}
		}
		return -1;
	}

	public void GotoCorrespondingTelePage(int id)
	{
		inventory_ctr.Instance.jump_to_page(FindCorrespondingTelePage(id));
	}

	public int FindCorrespondingTelePage(int id)
	{
		short slotShort = PlayerData.Instance.GetSlotShort("n_teleporters", PlayerData.filename_t.teleporters);
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < slotShort; i++)
		{
			if (PlayerData.Instance.GetSlotShort("teleporter_" + i + "_deleted", PlayerData.filename_t.teleporters) != 1)
			{
				if (i == id)
				{
					return num2;
				}
				if (num == 2)
				{
					num2++;
				}
				num = ((num != 2) ? (num + 1) : 0);
			}
		}
		return 0;
	}

	public Texture2D GetCustomTeleTexture(int tele_id)
	{
		string path = Startup.persistentDataPath + System.IO.Path.DirectorySeparatorChar + PlayerData.Instance.GetCurrentSlotFolder();
		Texture2D texture2D = null;
		if (System.IO.File.Exists(System.IO.Path.Combine(path, "tele_graphic_" + tele_id)))
		{
			byte[] data = System.IO.File.ReadAllBytes(System.IO.Path.Combine(path, "tele_graphic_" + tele_id));
			texture2D = new Texture2D(150, 150);
			texture2D.LoadImage(data);
		}
		return texture2D;
	}

	public string GetNewTeleporterName()
	{
		if (!GameServerConnector.Instance.FullyInGame())
		{
			if (GameServerConnector.Instance.MidConnect())
			{
				return "Error";
			}
		}
		else if (!GameServerConnector.Instance.is_host)
		{
			return PlayerData.Instance.GetGlobalString("username_punctuated") + "'s Teleporter";
		}
		return "Teleporter" + Instance.GetNewTeleporterId();
	}

	public void PressAcceptEditTeleporter()
	{
		save_window_open = false;
		ExtraInventoryData extraDataCopy = GameController.Instance.interacting_element_item.GetExtraDataCopy();
		extraDataCopy.SetShort("set_up", 1);
		extraDataCopy.SetString("teleporter_name", "");
		ConstructionControl.Instance.PlayerReplaceInteracting(new InventoryItem("Teleporter", extraDataCopy), true);
		if (GameServerConnector.Instance.FullyInGame() && !GameServerConnector.Instance.is_host)
		{
			string player_zone = ChunkControl.Instance.player_zone;
			InputField component = WindowPrefabsControl.Instance.GetObject("Teleport-save", "teleport-save-title").GetComponent<InputField>();
			InputField component2 = WindowPrefabsControl.Instance.GetObject("Teleport-save", "teleport-save-desc").GetComponent<InputField>();
			GameServerSender.Instance.SendFinishedEditingTeleporter(component.text, component2.text, player_zone, GameController.Instance.interacting_element_chunkX, GameController.Instance.interacting_element_chunkZ, GameController.Instance.interacting_element_innerX, GameController.Instance.interacting_element_innerZ);
			TakeScreenshot(GameController.Instance.interacting_element_chunkX, GameController.Instance.interacting_element_chunkZ, GameController.Instance.interacting_element_innerX, GameController.Instance.interacting_element_innerZ, true);
		}
		else if (!GameServerConnector.Instance.MidConnect())
		{
			InputField component3 = WindowPrefabsControl.Instance.GetObject("Teleport-save", "teleport-save-title").GetComponent<InputField>();
			InputField component4 = WindowPrefabsControl.Instance.GetObject("Teleport-save", "teleport-save-desc").GetComponent<InputField>();
			int customTeleId = GetCustomTeleId(ChunkControl.Instance.player_zone, GameController.Instance.interacting_element_chunkX, GameController.Instance.interacting_element_chunkZ, GameController.Instance.interacting_element_innerX, GameController.Instance.interacting_element_innerZ);
			PlayerData.Instance.SetSlotString("teleporter_" + customTeleId + "_name", component3.text, PlayerData.filename_t.teleporters);
			PlayerData.Instance.SetSlotString("teleporter_" + customTeleId + "_desc", component4.text, PlayerData.filename_t.teleporters);
		}
		WindowPrefabsControl.Instance.DestroyScreen("Teleport-save");
		if (!GameServerConnector.Instance.FullyInGame())
		{
			WindowControl.Instance.ShowMiniwindowHeaders();
			WindowControl.Instance.VisuallySelectRightMiniwindowTab();
		}
		else
		{
			bool is_host = GameServerConnector.Instance.is_host;
			WindowControl.Instance.ShowMiniwindowHeaders();
			WindowControl.Instance.VisuallySelectRightMiniwindowTab();
			if (!is_host)
			{
				GameServerSender.Instance.RequestPageOfTeleportersByTeleStr(ChunkControl.Instance.player_zone, GameController.Instance.interacting_element_chunkX, GameController.Instance.interacting_element_chunkZ, GameController.Instance.interacting_element_innerX, GameController.Instance.interacting_element_innerZ);
				PopupControl.Instance.ShowConnecting("Saving teleporter");
				return;
			}
		}
		OnRightMiniwindowTabPressed();
		GotoCorrespondingTelePage(GetCustomTeleId(ChunkControl.Instance.player_zone, GameController.Instance.interacting_element_chunkX, GameController.Instance.interacting_element_chunkZ, GameController.Instance.interacting_element_innerX, GameController.Instance.interacting_element_innerZ));
	}

	public void TryReEnableTeleporterButtonIfOutOfBounds()
	{
		if (ShouldDisableTeleporterButton())
		{
			GameplayGUIControl.Instance.DisableTeleporterButton();
		}
		else
		{
			GameplayGUIControl.Instance.EnableTeleporterButton();
		}
	}

	public bool ShouldDisableTeleporterButton()
	{
		if (GameController.Instance.player == null)
		{
			return false;
		}
		if (ChunkControl.Instance.GetCaveFloorModelBelow(GameController.Instance.player) == 0)
		{
			return false;
		}
		if (ChunkControl.Instance.GetCaveExitFloorModel() == 0)
		{
			return false;
		}
		return !(ChunkControl.Instance.player_zone == "shack9072");
	}

	public void PressTeleport()
	{
		PopupControl.Instance.SetButtonWasPressed();
		if (disable_teleport_button)
		{
			PopupControl.Instance.ShowMessage("You cannot teleport while underground!");
		}
		else if (!GameServerConnector.Instance.FullyInGame() || !GameServerReceiver.Instance.waiting_on_initial_zone_data)
		{
			OpenTeleportWindow(-1);
		}
	}

	public void OpenTeleportWindow(int skip_to_hard_code_page)
	{
		WindowControl.Instance.OpenMiniwindow(WindowControl.miniwindow_type_t.teleport);
		WindowControl.Instance.VisuallySelectLeftMiniwindowTab();
		if (skip_to_hard_code_page == -1)
		{
			skip_to_hard_code_page = curr_hardcoded_teleporters_page;
		}
		else
		{
			curr_hardcoded_teleporters_page = skip_to_hard_code_page;
		}
		OnLeftMiniwindowTabPressed(skip_to_hard_code_page);
	}

	private void LateUpdate()
	{
		if (screenshot_timer == 0)
		{
			return;
		}
		screenshot_timer--;
		if (screenshot_timer != 0)
		{
			return;
		}
		Vector3 position = Camera.main.transform.parent.position;
		Quaternion rotation = Camera.main.transform.parent.rotation;
		GameController.Instance.player.SetActive(false);
		GameController.Instance.targeted_circle_graphic.SetActive(false);
		Camera.main.transform.parent.position = take_screenshot_pos + new Vector3(2f, 4f, -2f);
		Camera.main.transform.parent.LookAt(take_screenshot_pos);
		RenderTexture renderTexture = teleporter_screenshot_render_texture;
		Camera.main.targetTexture = renderTexture;
		Texture2D texture2D = teleporter_screenshot_texture2D;
		Camera.main.Render();
		RenderTexture.active = renderTexture;
		texture2D.ReadPixels(new Rect(0f, 0f, 150f, 150f), 0, 0);
		texture2D.Apply();
		Color[] pixels = custom_tele_mask.GetPixels();
		Color[] pixels2 = custom_tele_overlay.GetPixels();
		Color[] pixels3 = texture2D.GetPixels();
		Color[] array = new Color[pixels3.Length];
		for (int i = 0; i < pixels3.Length; i++)
		{
			if (pixels[i] != Color.black)
			{
				array[i] = pixels3[i];
			}
			else
			{
				array[i] = Color.clear;
			}
			array[i] = Color.Lerp(array[i], pixels2[i], pixels2[i].a);
		}
		Texture2D texture2D2 = new Texture2D(150, 150, TextureFormat.ARGB32, false);
		texture2D2.filterMode = FilterMode.Point;
		texture2D2.SetPixels(array);
		texture2D2.Apply();
		Camera.main.targetTexture = null;
		RenderTexture.active = null;
		Camera.main.transform.parent.position = position;
		Camera.main.transform.parent.rotation = rotation;
		GameController.Instance.player.SetActive(true);
		GameController.Instance.targeted_circle_graphic.SetActive(true);
		byte[] array2 = texture2D2.EncodeToPNG();
		string player_zone = ChunkControl.Instance.player_zone;
		Vector3 chunkCoords = ChunkControl.Instance.GetChunkCoords(take_screenshot_pos);
		Vector3 inner = ChunkControl.Instance.GetInner(take_screenshot_pos);
		int num = (int)chunkCoords.x;
		int num2 = (int)chunkCoords.z;
		int num3 = (int)inner.x;
		int num4 = (int)inner.z;
		string key = player_zone + "," + num + "," + num2 + "," + num3 + "," + num4;
		if (!GameServerConnector.Instance.ShouldSaveLocally())
		{
			if (relay_screenshot)
			{
				GameServerSender.Instance.SendTeleporterScreenshot(player_zone, num, num2, num3, num4, array2);
			}
		}
		else
		{
			int customTeleId = GetCustomTeleId(player_zone, num, num2, num3, num4);
			System.IO.File.WriteAllBytes(System.IO.Path.Combine(Startup.persistentDataPath + System.IO.Path.DirectorySeparatorChar + PlayerData.Instance.GetCurrentSlotFolder(), "tele_graphic_" + customTeleId), array2);
		}
		relay_screenshot = false;
		Sprite sprite = Sprite.Create(texture2D2, new Rect(0f, 0f, texture2D2.width, texture2D2.height), new Vector2(0.5f, 0.5f));
		if (save_window_open)
		{
			WindowPrefabsControl.Instance.GetImage("Teleport-save", "teleport-graphic").sprite = sprite;
		}
		if (GameServerReceiver.Instance.cached_teleporter_textures.ContainsKey(key))
		{
			GameServerReceiver.Instance.cached_teleporter_textures[key] = sprite;
		}
		screenshot_timer = 0;
	}

	public void DeleteTeleporter(string zone, int chunkX, int chunkZ, int innerX, int innerZ)
	{
		int customTeleId = GetCustomTeleId(zone, chunkX, chunkZ, innerX, innerZ);
		if (customTeleId != -1)
		{
			PlayerData.Instance.SetSlotShort("teleporter_" + customTeleId + "_deleted", 1, PlayerData.filename_t.teleporters);
		}
	}

	public int GetNewTeleporterId()
	{
		int slotShort = PlayerData.Instance.GetSlotShort("n_teleporters", PlayerData.filename_t.teleporters);
		for (int i = 0; i < slotShort; i++)
		{
			if (PlayerData.Instance.GetSlotShort("teleporter_" + i + "_deleted", PlayerData.filename_t.teleporters) == 1)
			{
				if (i == -1)
				{
					return slotShort;
				}
				return i;
			}
		}
		return slotShort;
	}

	public void CreateNewTeleporter(string zone, int chunkX, int chunkZ, int innerX, int innerZ, string title)
	{
		short slotShort = PlayerData.Instance.GetSlotShort("n_teleporters", PlayerData.filename_t.teleporters);
		int newTeleporterId = GetNewTeleporterId();
		if (newTeleporterId == slotShort)
		{
			PlayerData.Instance.SetSlotShort("n_teleporters", slotShort + 1, PlayerData.filename_t.teleporters);
		}
		PlayerData.Instance.SetSlotShort("teleporter_" + newTeleporterId + "_deleted", 0, PlayerData.filename_t.teleporters);
		string text = "teleporter_" + newTeleporterId;
		PlayerData.Instance.SetSlotString(text + "_name", title, PlayerData.filename_t.teleporters);
		PlayerData.Instance.SetSlotString(text + "_desc", "Add description", PlayerData.filename_t.teleporters);
		PlayerData.Instance.SetSlotShort(text + "_to_chunkX", chunkX, PlayerData.filename_t.teleporters);
		PlayerData.Instance.SetSlotShort(text + "_to_chunkZ", chunkZ, PlayerData.filename_t.teleporters);
		PlayerData.Instance.SetSlotShort(text + "_to_innerX", innerX, PlayerData.filename_t.teleporters);
		PlayerData.Instance.SetSlotShort(text + "_to_innerZ", innerZ, PlayerData.filename_t.teleporters);
		PlayerData.Instance.SetSlotString(text + "_to_zone", zone, PlayerData.filename_t.teleporters);
	}

	public void TakeScreenshot(int chunkX, int chunkZ, int innerX, int innerZ, bool relay)
	{
		screenshot_timer = 3;
		take_screenshot_pos = new Vector3((float)chunkX * 10f + (float)innerX + 0.5f, 0f, (float)chunkZ * 10f + (float)innerZ + 0.5f);
		relay_screenshot = relay;
	}

	public void PressSearchForTeleporter()
	{
		inventory_ctr.Instance.HideCraftingTab();
		WindowControl.Instance.HideMiniwindowHeaders();
		WindowPrefabsControl.Instance.CreateScreen("Teleport-search", WindowPrefabsControl.build_into_t.mini_window);
	}

	public void PressBackOnTeleSearch()
	{
		WindowPrefabsControl.Instance.DestroyScreen("Teleport-search");
	}

	public void PressBeginSearch()
	{
		in_search_screen = true;
		PopupControl.Instance.ShowConnecting("Searching");
		GameServerSender.Instance.SendNewTeleSearch(WindowPrefabsControl.Instance.GetTextLegacy("Teleport-search", "username-text").text);
		tele_search_button.SetActive(false);
		UnityEngine.Object.Destroy(WindowPrefabsControl.Instance.GetObject("Teleport-search", "destroy0"));
		UnityEngine.Object.Destroy(WindowPrefabsControl.Instance.GetObject("Teleport-search", "destroy1"));
		UnityEngine.Object.Destroy(WindowPrefabsControl.Instance.GetObject("Teleport-search", "destroy2"));
		UnityEngine.Object.Destroy(WindowPrefabsControl.Instance.GetObject("Teleport-search", "destroy3"));
		UnityEngine.Object.Destroy(WindowPrefabsControl.Instance.GetObject("Teleport-search", "destroy4"));
		WindowPrefabsControl.Instance.GetTextLegacy("Teleport-search", "rename").text = "Search Results";
	}

	public void ShowTeleporterSetup(GameObject obj, string title, string desc, int colID, bool mod_tele)
	{
		save_window_open = true;
		WindowControl.Instance.OpenMiniwindow(WindowControl.miniwindow_type_t.teleport);
		WindowControl.Instance.HideMiniwindowHeaders();
		WindowPrefabsControl.Instance.CreateScreen("Teleport-save", WindowPrefabsControl.build_into_t.mini_window);
		InputField component = WindowPrefabsControl.Instance.GetObject("Teleport-save", "teleport-save-title").GetComponent<InputField>();
		InputField component2 = WindowPrefabsControl.Instance.GetObject("Teleport-save", "teleport-save-desc").GetComponent<InputField>();
		component.SetTextWithoutNotify(title);
		component2.SetTextWithoutNotify(desc);
		GameObject @object = WindowPrefabsControl.Instance.GetObject("Teleport-save", "edit-tele-col-" + colID);
		WindowPrefabsControl.Instance.GetObject("Teleport-save", "edit-tele-col-selector").transform.localPosition = @object.transform.localPosition;
		for (int i = 0; i < possible_colors.Length; i++)
		{
			WindowPrefabsControl.Instance.GetObject("Teleport-save", "edit-tele-col-" + i).GetComponent<Image>().color = possible_colors[i];
		}
		TakeScreenshot(GameController.Instance.interacting_element_chunkX, GameController.Instance.interacting_element_chunkZ, GameController.Instance.interacting_element_innerX, GameController.Instance.interacting_element_innerZ, false);
	}

	public Vector3 ClickedTeleposToVec3()
	{
		return new Vector3((float)(click_teleport_to_innerX + click_teleport_to_chunkX * 10) + 0.5f, 0f, (float)(click_teleport_to_innerZ + click_teleport_to_chunkZ * 10) + 0.5f);
	}

	public void DelayedTeleportTransition(int tele_index_clicked, tele_type tele_type_t, float delay = 2.5f)
	{
		StartCoroutine(TeleportTransition(tele_index_clicked, tele_type_t, delay));
	}

	private IEnumerator TeleportTransition(int tele_index_clicked, tele_type tele_type_t, float delay)
	{
		switch (tele_type_t)
		{
		case tele_type.custom_local:
		{
			short slotShort = PlayerData.Instance.GetSlotShort("n_teleporters", PlayerData.filename_t.teleporters);
			int num = -1;
			for (int i = 0; i < slotShort; i++)
			{
				if (PlayerData.Instance.GetSlotShort("teleporter_" + i + "_deleted", PlayerData.filename_t.teleporters) == 0)
				{
					num++;
				}
				if (num == tele_index_clicked)
				{
					int num2 = i;
					click_teleport_zone_to = PlayerData.Instance.GetSlotString("teleporter_" + num2 + "_to_zone", PlayerData.filename_t.teleporters);
					click_teleport_to_chunkX = PlayerData.Instance.GetSlotShort("teleporter_" + num2 + "_to_chunkX", PlayerData.filename_t.teleporters);
					click_teleport_to_chunkZ = PlayerData.Instance.GetSlotShort("teleporter_" + num2 + "_to_chunkZ", PlayerData.filename_t.teleporters);
					click_teleport_to_innerX = PlayerData.Instance.GetSlotShort("teleporter_" + num2 + "_to_innerX", PlayerData.filename_t.teleporters);
					click_teleport_to_innerZ = PlayerData.Instance.GetSlotShort("teleporter_" + num2 + "_to_innerZ", PlayerData.filename_t.teleporters);
					break;
				}
			}
			break;
		}
		case tele_type.hardcoded:
			if (tele_index_clicked == -1)
			{
				click_teleport_zone_to = "overworld";
				Vector3 position = BreedControl.Instance.campos_result.transform.position;
				Vector3 chunkCoords = ChunkControl.Instance.GetChunkCoords(position);
				Vector3 inner = ChunkControl.Instance.GetInner(position);
				click_teleport_to_chunkX = (int)chunkCoords.x;
				click_teleport_to_chunkZ = (int)chunkCoords.z;
				click_teleport_to_innerX = (int)inner.x;
				click_teleport_to_innerZ = (int)inner.z;
			}
			else if (tele_index_clicked == -999)
			{
				click_teleport_zone_to = "overworld";
				click_teleport_to_innerX = 5;
				click_teleport_to_innerZ = 5;
				click_teleport_to_chunkX = 100;
				click_teleport_to_chunkZ = -100;
			}
			else
			{
				click_teleport_zone_to = hardcoded_teleports[tele_index_clicked].to_zone;
				click_teleport_to_chunkX = hardcoded_teleports[tele_index_clicked].to_chunkX;
				click_teleport_to_chunkZ = hardcoded_teleports[tele_index_clicked].to_chunkZ;
				click_teleport_to_innerX = hardcoded_teleports[tele_index_clicked].to_innerX;
				click_teleport_to_innerZ = hardcoded_teleports[tele_index_clicked].to_innerZ;
			}
			break;
		}
		yield return new WaitForSeconds(delay);
		TransitionControl.Instance.BeginTeleportTransition();
		QuestControl.Instance.RevertQuestIfNecessary();
	}

	public void VisuallyTeleportSomeone(GameObject to_teleport)
	{
		if (to_teleport.GetComponent<SharedCreature>().teleport_particle != null)
		{
			EndTeleportAnimation(to_teleport);
		}
		if (to_teleport.GetComponent<SharedCreature>().snapped_to_chair_obj)
		{
			to_teleport.GetComponent<SharedCreature>().EndSittingInChair();
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab_teleport);
		to_teleport.GetComponent<SharedCreature>().teleport_particle = gameObject;
		gameObject.transform.position = new Vector3(to_teleport.transform.position.x, 0f, to_teleport.transform.position.z);
		if (to_teleport.GetComponent<Rigidbody>() != null)
		{
			to_teleport.GetComponent<Rigidbody>().isKinematic = true;
		}
		to_teleport.transform.SetParent(gameObject.transform.Find("player goes here"));
		to_teleport.transform.localPosition = Vector3.zero;
	}

	public void EndTeleportAnimation(GameObject other_player)
	{
		if (!(other_player.GetComponent<SharedCreature>().teleport_particle == null))
		{
			other_player.transform.SetParent(null);
			other_player.GetComponent<SharedCreature>().UpdateSize();
			UnityEngine.Object.Destroy(other_player.GetComponent<SharedCreature>().teleport_particle);
		}
	}

	public void InteractWithTeleporter(GameObject obj, string chunkStr, string zone, int chunkX, int chunkZ, int innerX, int innerZ)
	{
		int hardcodedTeleId = GetHardcodedTeleId(chunkStr, innerX, innerZ);
		if (hardcodedTeleId == -1)
		{
			if (GameController.Instance.interacting_element_item.GetShort("set_up") == 0)
			{
				ShowTeleporterSetup(obj, GameController.Instance.interacting_element_item.GetString("teleporter_name"), "Add description", GameController.Instance.interacting_element_item.GetShort("col_id"), false);
				return;
			}
			if (GameServerConnector.Instance.FullyInGame() && !GameServerConnector.Instance.is_host)
			{
				WindowControl.Instance.OpenMiniwindow(WindowControl.miniwindow_type_t.teleport);
				WindowControl.Instance.VisuallySelectRightMiniwindowTab();
				PopupControl.Instance.ShowConnecting("Loading teleporter");
				GameServerSender.Instance.RequestPageOfTeleportersByTeleStr(zone, chunkX, chunkZ, innerX, innerZ);
				return;
			}
			int customTeleId = GetCustomTeleId(zone, chunkX, chunkZ, innerX, innerZ);
			if (customTeleId != -1)
			{
				WindowControl.Instance.OpenMiniwindow(WindowControl.miniwindow_type_t.teleport);
				WindowControl.Instance.VisuallySelectRightMiniwindowTab();
				OnRightMiniwindowTabPressed();
				GotoCorrespondingTelePage(customTeleId);
			}
			return;
		}
		int num = hardcodedTeleId / 3;
		if (hardcodedTeleId == 0 || PlayerData.Instance.GetSlotShort("discovered_" + hardcoded_teleports[hardcodedTeleId].name, PlayerData.filename_t.general) == 1)
		{
			OpenTeleportWindow(num);
			return;
		}
		OnNotifClick onNotifClick = new OnNotifClick(OnNotifClick.type.teleporters);
		onNotifClick.data.Add("skip_to_hard_code_page", num.ToString() ?? "");
		GameplayGUIControl.Instance.ShowNotif("Activated teleporter!   ", new InventoryItem("Teleporter"), 1, onNotifClick);
		PlayerData.Instance.SetSlotShort("discovered_" + Instance.hardcoded_teleports[hardcodedTeleId].name, 1, PlayerData.filename_t.general);
		ColorizeTeleporter_(ChunkControl.Instance.GetObjectAt("Teleporter", zone, chunkX, chunkZ, innerX, innerZ), 0);
	}

	public bool IsHardcodedTeleporterDiscovered(int hard_coded_index)
	{
		if (PlayerData.Instance.GetGlobalShort("dev_mode") == 1)
		{
			return true;
		}
		if (hardcoded_teleports[hard_coded_index].name == "Noobia")
		{
			return true;
		}
		return PlayerData.Instance.GetSlotShort("discovered_" + hardcoded_teleports[hard_coded_index].name, PlayerData.filename_t.general) == 1;
	}

	public void DrawHardcodedTeleporterSlot(int slot_id, int index)
	{
		bool flag = IsHardcodedTeleporterDiscovered(index);
		string title_str = "";
		string desc = "";
		switch (TranslationControl.Instance.use_language)
		{
		case TranslationControl.languages.English:
			title_str = hardcoded_teleports[index].name;
			desc = hardcoded_teleports[index].description;
			break;
		case TranslationControl.languages.Russian:
			title_str = hardcoded_teleports[index].name_RUS;
			desc = hardcoded_teleports[index].description_RUS;
			break;
		case TranslationControl.languages.Portuguese:
			title_str = hardcoded_teleports[index].name_POR;
			desc = hardcoded_teleports[index].description_POR;
			break;
		case TranslationControl.languages.Indonesian:
			title_str = hardcoded_teleports[index].name_IND;
			desc = hardcoded_teleports[index].description_IND;
			break;
		case TranslationControl.languages.Spanish:
			title_str = hardcoded_teleports[index].name_SPN;
			desc = hardcoded_teleports[index].description_SPN;
			break;
		case TranslationControl.languages.Thai:
			title_str = hardcoded_teleports[index].name_TAI;
			desc = hardcoded_teleports[index].description_TAI;
			break;
		}
		inventory_ctr.Instance.instantiated_crafting_slots[slot_id].LayOutCraftingSlot(title_str, desc, 1f, true, flag, false, false, false, CraftingSlot.req_placement.disable, CraftingSlot.text_area_layout.full, CraftingSlot.slots_positioning.full_size, false);
		Image graphic = inventory_ctr.Instance.instantiated_crafting_slots[slot_id].graphic;
		if (flag)
		{
			graphic.sprite = hardcoded_teleports[index].sprite;
		}
		else
		{
			graphic.sprite = DevBuildControl.Instance.spr_teleport_unknown;
		}
	}

	public void DrawCustomTeleporterSlot(int slot_id, int index)
	{
		string title_str = "";
		string desc = "";
		Sprite sprite = null;
		short slotShort = PlayerData.Instance.GetSlotShort("n_teleporters", PlayerData.filename_t.teleporters);
		int num = -1;
		for (int i = 0; i < slotShort; i++)
		{
			if (PlayerData.Instance.GetSlotShort("teleporter_" + i + "_deleted", PlayerData.filename_t.teleporters) == 0)
			{
				num++;
			}
			if (num == index)
			{
				string text = "teleporter_" + i;
				title_str = PlayerData.Instance.GetSlotString(text + "_name", PlayerData.filename_t.teleporters);
				desc = PlayerData.Instance.GetSlotString(text + "_desc", PlayerData.filename_t.teleporters);
				Texture2D customTeleTexture = GetCustomTeleTexture(i);
				sprite = Sprite.Create(customTeleTexture, new Rect(0f, 0f, customTeleTexture.width, customTeleTexture.height), new Vector2(0.5f, 0.5f));
				temp_teleporter_textures.Add(customTeleTexture);
				if (temp_teleporter_textures.Count >= 4)
				{
					UnityEngine.Object.Destroy(temp_teleporter_textures[0]);
					temp_teleporter_textures.RemoveAt(0);
				}
				break;
			}
		}
		inventory_ctr.Instance.instantiated_crafting_slots[slot_id].LayOutCraftingSlot(title_str, desc, 1f, true, true, false, false, false, CraftingSlot.req_placement.disable, CraftingSlot.text_area_layout.full, CraftingSlot.slots_positioning.full_size, false);
		Image graphic = inventory_ctr.Instance.instantiated_crafting_slots[slot_id].graphic;
		if (sprite != null)
		{
			graphic.sprite = sprite;
		}
		else
		{
			graphic.sprite = DevBuildControl.Instance.spr_teleport_unknown;
		}
	}

	public void DrawOnlineTeleporterSlot(int slot_id, OnlineTeleporter teleporter)
	{
		inventory_ctr.Instance.instantiated_crafting_slots[slot_id].LayOutCraftingSlot(teleporter.title, teleporter.description, 1f, true, true, false, false, false, CraftingSlot.req_placement.disable, CraftingSlot.text_area_layout.full, CraftingSlot.slots_positioning.full_size, true);
		Image graphic = inventory_ctr.Instance.instantiated_crafting_slots[slot_id].graphic;
		if (!GameServerReceiver.Instance.cached_teleporter_textures.ContainsKey(teleporter.tele_str))
		{
			graphic.sprite = DevBuildControl.Instance.spr_teleport_unknown;
			GameServerSender.Instance.RequestTeleporterScreenshot(teleporter);
		}
		else
		{
			graphic.sprite = GameServerReceiver.Instance.cached_teleporter_textures[teleporter.tele_str];
		}
	}
}
