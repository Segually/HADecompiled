using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	}

	private void OnDestroy()
	{
	}

	public int GetNumberOfActiveTeleporters()
	{
		return 0;
	}

	public void OnLeftMiniwindowTabPressed(int hard_coded_skip_to_page)
	{
	}

	public void OnRightMiniwindowTabPressed()
	{
	}

	public void PressEditTeleColor(int index)
	{
	}

	public void VisuallyTeleportMainPlayer(string tele_str)
	{
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
		return 0;
	}

	public void GotoCorrespondingTelePage(int id)
	{
	}

	public int FindCorrespondingTelePage(int id)
	{
		return 0;
	}

	public Texture2D GetCustomTeleTexture(int tele_id)
	{
		return null;
	}

	public string GetNewTeleporterName()
	{
		return null;
	}

	public void PressAcceptEditTeleporter()
	{
	}

	public void TryReEnableTeleporterButtonIfOutOfBounds()
	{
	}

	public bool ShouldDisableTeleporterButton()
	{
		return false;
	}

	public void PressTeleport()
	{
	}

	public void OpenTeleportWindow(int skip_to_hard_code_page)
	{
	}

	private void LateUpdate()
	{
	}

	public void DeleteTeleporter(string zone, int chunkX, int chunkZ, int innerX, int innerZ)
	{
	}

	public int GetNewTeleporterId()
	{
		return 0;
	}

	public void CreateNewTeleporter(string zone, int chunkX, int chunkZ, int innerX, int innerZ, string title)
	{
	}

	public void TakeScreenshot(int chunkX, int chunkZ, int innerX, int innerZ, bool relay)
	{
	}

	public void PressSearchForTeleporter()
	{
	}

	public void PressBackOnTeleSearch()
	{
	}

	public void PressBeginSearch()
	{
	}

	public void ShowTeleporterSetup(GameObject obj, string title, string desc, int colID, bool mod_tele)
	{
	}

	public Vector3 ClickedTeleposToVec3()
	{
		return default(Vector3);
	}

	public void DelayedTeleportTransition(int tele_index_clicked, tele_type tele_type_t, float delay = 2.5f)
	{
	}

	private IEnumerator TeleportTransition(int tele_index_clicked, tele_type tele_type_t, float delay)
	{
		return null;
	}

	public void VisuallyTeleportSomeone(GameObject to_teleport)
	{
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
	}

	public bool IsHardcodedTeleporterDiscovered(int hard_coded_index)
	{
		return false;
	}

	public void DrawHardcodedTeleporterSlot(int slot_id, int index)
	{
	}

	public void DrawCustomTeleporterSlot(int slot_id, int index)
	{
	}

	public void DrawOnlineTeleporterSlot(int slot_id, OnlineTeleporter teleporter)
	{
	}
}
