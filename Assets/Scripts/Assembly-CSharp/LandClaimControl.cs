using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LandClaimControl : MonoBehaviour, OrderedStart
{
	public enum land_screen_context_t
	{
		none = 0,
		open_active = 1,
		open_expired = 2
	}

	public static LandClaimControl Instance;

	public land_screen_context_t land_screen_context;

	public string curr_land_claim_player_0 = "";

	public string curr_land_claim_player_1 = "";

	public string curr_land_claim_player_2 = "";

	public static int three_day_land_claim_add_seconds;

	public static int three_day_land_claim_add_days = 3;

	public static int eight_day_land_claim_add_seconds;

	public static int eight_day_land_claim_add_days = 8;

	public static int admin_land_claim_add_seconds;

	public static int admin_land_claim_add_days = 32000;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
	}

	public void GetLandClaimPlayers(ChunkData chunk_data, string land_claim_str)
	{
		foreach (KeyValuePair<string, LandClaimChunkTimer> item in chunk_data.land_claim_chunk_timers_)
		{
			if (item.Value.land_claim_str == land_claim_str)
			{
				curr_land_claim_player_0 = item.Value.land_claim_user0;
				curr_land_claim_player_1 = item.Value.land_claim_user1;
				curr_land_claim_player_2 = item.Value.land_claim_user2;
			}
		}
	}

	private void RedrawLandClaimPlayers()
	{
		WindowPrefabsControl.Instance.GetObject("LandClaim", "player1").transform.Find("text").GetComponent<Text>().text = curr_land_claim_player_0 + " (owner)";
		if (curr_land_claim_player_1 == "")
		{
			WindowPrefabsControl.Instance.GetObject("LandClaim", "add2").gameObject.SetActive(true);
			WindowPrefabsControl.Instance.GetObject("LandClaim", "player2").gameObject.SetActive(false);
		}
		else
		{
			WindowPrefabsControl.Instance.GetObject("LandClaim", "add2").gameObject.SetActive(false);
			WindowPrefabsControl.Instance.GetObject("LandClaim", "player2").gameObject.SetActive(true);
			WindowPrefabsControl.Instance.GetObject("LandClaim", "player2").transform.Find("text").GetComponent<Text>().text = curr_land_claim_player_1;
			WindowPrefabsControl.Instance.GetObject("LandClaim", "player2").transform.Find("delete").gameObject.SetActive(IsCurrLandClaimMine());
		}
		if (curr_land_claim_player_2 == "")
		{
			WindowPrefabsControl.Instance.GetObject("LandClaim", "add3").gameObject.SetActive(true);
			WindowPrefabsControl.Instance.GetObject("LandClaim", "player3").gameObject.SetActive(false);
		}
		else
		{
			WindowPrefabsControl.Instance.GetObject("LandClaim", "add3").gameObject.SetActive(false);
			WindowPrefabsControl.Instance.GetObject("LandClaim", "player3").gameObject.SetActive(true);
			WindowPrefabsControl.Instance.GetObject("LandClaim", "player3").transform.Find("text").GetComponent<Text>().text = curr_land_claim_player_2;
			WindowPrefabsControl.Instance.GetObject("LandClaim", "player3").transform.Find("delete").gameObject.SetActive(IsCurrLandClaimMine());
		}
	}

	public void PressLandClaimDeleteUser(int slot)
	{
		if (IsCurrLandClaimMine() && land_screen_context == land_screen_context_t.open_active)
		{
			switch (slot)
			{
			case 1:
				WindowPrefabsControl.Instance.GetObject("LandClaim", "add2").GetComponent<InputField>().SetTextWithoutNotify("");
				break;
			case 2:
				WindowPrefabsControl.Instance.GetObject("LandClaim", "add3").GetComponent<InputField>().SetTextWithoutNotify("");
				break;
			}
			ModifyCurrent(slot, "");
		}
	}

	public void FinishedTypingSlot2()
	{
		if (WindowPrefabsControl.Instance.GetScreen("LandClaim") == null)
		{
			return;
		}
		string text = WindowPrefabsControl.Instance.GetObject("LandClaim", "add2").GetComponent<InputField>().text;
		WindowPrefabsControl.Instance.GetObject("LandClaim", "add2").GetComponent<InputField>().SetTextWithoutNotify("");
		if (!IsCurrLandClaimMine())
		{
			PopupControl.Instance.ShowMessage("Cannot edit\nThis land claim is owned by <color=#17d8ff>" + curr_land_claim_player_0 + "</color>");
		}
		else if (land_screen_context == land_screen_context_t.open_active)
		{
			ModifyCurrent(1, text);
		}
	}

	public void FinishedTypingSlot3()
	{
		if (WindowPrefabsControl.Instance.GetScreen("LandClaim") == null)
		{
			return;
		}
		string text = WindowPrefabsControl.Instance.GetObject("LandClaim", "add3").GetComponent<InputField>().text;
		WindowPrefabsControl.Instance.GetObject("LandClaim", "add3").GetComponent<InputField>().SetTextWithoutNotify("");
		if (!IsCurrLandClaimMine())
		{
			PopupControl.Instance.ShowMessage("Cannot edit\nThis land claim is owned by <color=#17d8ff>" + curr_land_claim_player_0 + "</color>");
		}
		else if (land_screen_context == land_screen_context_t.open_active)
		{
			ModifyCurrent(2, text);
		}
	}

	public void ModifyCurrent(int slot, string new_username)
	{
		string player_zone = ChunkControl.Instance.player_zone;
		int interacting_element_chunkX = GameController.Instance.interacting_element_chunkX;
		int interacting_element_chunkZ = GameController.Instance.interacting_element_chunkZ;
		int interacting_element_innerX = GameController.Instance.interacting_element_innerX;
		int interacting_element_innerZ = GameController.Instance.interacting_element_innerZ;
		string[] array = new string[9];
		for (int i = 0; i < 9; i++)
		{
			array[i] = ConstructionControl.GenerateCacheKey();
		}
		ModifyLandClaimTimer(player_zone, interacting_element_chunkX, interacting_element_chunkZ, interacting_element_innerX, interacting_element_innerZ, slot, new_username, array);
		GameServerSender.Instance.SendChangeLandClaimUser(player_zone, interacting_element_chunkX, interacting_element_chunkZ, interacting_element_innerX, interacting_element_innerZ, slot, new_username, array);
		ChunkData chunkData = ChunkControl.Instance.GetChunkData(ChunkControl.Instance.GetChunkString(player_zone, interacting_element_chunkX, interacting_element_chunkZ));
		GetLandClaimPlayers(chunkData, player_zone + "," + interacting_element_chunkX + "," + interacting_element_chunkZ + "," + interacting_element_innerX + "," + interacting_element_innerZ);
		RedrawLandClaimPlayers();
	}

	public bool IsFarEnoughAwayFromEnemyLandClaims(string zone, int chunkX, int chunkZ, string username_lower)
	{
		if (!GameServerConnector.Instance.FullyInGame())
		{
			return true;
		}
		if (GameServerConnector.Instance.is_host || GameServerConnector.Instance.is_moderator)
		{
			return true;
		}
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				string chunkString = ChunkControl.Instance.GetChunkString(zone, chunkX + i, chunkZ + j);
				ChunkData chunkData = null;
				bool flag = false;
				if (ChunkControl.Instance.IsChunkFullyLoadedOrMidload(chunkString))
				{
					chunkData = ChunkControl.Instance.GetChunkData(chunkString);
				}
				else if (GameServerConnector.Instance.is_host && PlayerData.Instance.GetSlotShort("exists", ChunkControl.GetChunkFilename(chunkString), chunkString) == 1)
				{
					chunkData = ChunkControl.Instance.HostGetChunk(zone, chunkX + i, chunkZ + j);
					flag = true;
				}
				if (chunkData == null)
				{
					continue;
				}
				bool flag2 = false;
				bool flag3 = false;
				foreach (KeyValuePair<string, LandClaimChunkTimer> item in chunkData.land_claim_chunk_timers_)
				{
					flag2 = true;
					if (item.Value.land_claim_user0.ToLower() == username_lower || item.Value.land_claim_user1.ToLower() == username_lower || item.Value.land_claim_user2.ToLower() == username_lower)
					{
						flag3 = true;
					}
				}
				if (flag)
				{
					chunkData.SaveLandClaimChunkTimersToDisk(chunkString);
				}
				if (flag2 && !flag3)
				{
					return false;
				}
			}
		}
		return true;
	}

	public void ModifyLandClaimTimer(string zone, int chunkX, int chunkZ, int innerX, int innerZ, int user_index, string new_username, string[] mp_cache_keys)
	{
		string land_claim_str = zone + "," + chunkX + "," + chunkZ + "," + innerX + "," + innerZ;
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				string chunkString = ChunkControl.Instance.GetChunkString(zone, chunkX + i, chunkZ + j);
				string chunkFilename = ChunkControl.GetChunkFilename(chunkString);
				if (ChunkControl.Instance.IsChunkFullyLoadedOrMidload(chunkString))
				{
					ChunkData chunkData = ChunkControl.Instance.GetChunkData(chunkString);
					chunkData.ModifyLandClaimTimer(land_claim_str, user_index, new_username);
					chunkData.mp_cache_key = mp_cache_keys[(i + 1) * 3 + (j + 1)];
				}
				else if (GameServerConnector.Instance.ShouldSaveLocally() && PlayerData.Instance.GetSlotShort("exists", chunkFilename, chunkString) == 1)
				{
					ChunkData chunkData2 = ChunkControl.Instance.HostGetChunk(zone, chunkX + i, chunkZ + j);
					chunkData2.ModifyLandClaimTimer(land_claim_str, user_index, new_username);
					chunkData2.SaveLandClaimChunkTimersToDisk(chunkString);
				}
			}
		}
	}

	private Dictionary<string, LandClaimChunkTimer> GetLandClaimChunkTimersAt(string zone, int chunkX, int chunkZ)
	{
		ChunkData chunkData = ChunkControl.Instance.GetChunkData(ChunkControl.Instance.GetChunkString(zone, chunkX, chunkZ));
		if (chunkData == null)
		{
			return null;
		}
		if (zone == "overworld")
		{
			return chunkData.land_claim_chunk_timers_;
		}
		return ZoneDataControl.Instance.curr_zonedata.outdoor_land_claim_chunk_timers;
	}

	public bool AllowedToBuild(string zone, int chunkX, int chunkZ)
	{
		if (!GameServerConnector.Instance.FullyInGame())
		{
			return true;
		}
		if (GameServerConnector.Instance.is_host)
		{
			return true;
		}
		if (GameServerConnector.Instance.is_moderator)
		{
			return true;
		}
		Dictionary<string, LandClaimChunkTimer> landClaimChunkTimersAt = GetLandClaimChunkTimersAt(zone, chunkX, chunkZ);
		if (landClaimChunkTimersAt == null)
		{
			return false;
		}
		string globalString = PlayerData.Instance.GetGlobalString("username_lower");
		string text = "";
		foreach (KeyValuePair<string, LandClaimChunkTimer> item in landClaimChunkTimersAt)
		{
			if (item.Value.land_claim_user0.ToLower() == globalString || item.Value.land_claim_user1.ToLower() == globalString || item.Value.land_claim_user2.ToLower() == globalString)
			{
				return true;
			}
			text = item.Value.land_claim_user0;
		}
		if (text == "")
		{
			return true;
		}
		if (text == "ME")
		{
			text = GameServerConnector.Instance.server_name.Replace("(private)", "");
		}
		PopupControl.Instance.ShowMessage("<color=#aaaaaa>You cannot build here.</color>\n<color=#fff75c>" + text + "</color> built a <color=#63d8ff>Land Claim</color> nearby, and owns this land.");
		return false;
	}

	public bool LandOwnedByMe(string zone, int chunkX, int chunkZ)
	{
		if (!GameServerConnector.Instance.FullyInGame())
		{
			return !GameServerConnector.Instance.MidConnect();
		}
		if (GameServerConnector.Instance.is_host)
		{
			return true;
		}
		Dictionary<string, LandClaimChunkTimer> landClaimChunkTimersAt = GetLandClaimChunkTimersAt(zone, chunkX, chunkZ);
		if (landClaimChunkTimersAt == null)
		{
			return false;
		}
		string globalString = PlayerData.Instance.GetGlobalString("username_lower");
		foreach (KeyValuePair<string, LandClaimChunkTimer> item in landClaimChunkTimersAt)
		{
			if (item.Value.land_claim_user0.ToLower() == globalString || item.Value.land_claim_user1.ToLower() == globalString || item.Value.land_claim_user2.ToLower() == globalString)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsCurrLandClaimMine()
	{
		if (!GameServerConnector.Instance.FullyInGame())
		{
			if (GameServerConnector.Instance.MidConnect())
			{
				return false;
			}
		}
		else if (!GameServerConnector.Instance.is_host)
		{
			return curr_land_claim_player_0.ToLower() == PlayerData.Instance.GetGlobalString("username_punctuated");
		}
		if (curr_land_claim_player_0 == "ME")
		{
			return true;
		}
		return curr_land_claim_player_0.ToLower() == PlayerData.Instance.GetGlobalString("username_lower");
	}

	public void OpenLandClaimScreen(string land_claim_item_name, string zone, int chunkX, int chunkZ, int innerX, int innerZ)
	{
		string chunkString = ChunkControl.Instance.GetChunkString(zone, chunkX, chunkZ);
		if (!ChunkControl.Instance.IsChunkFullyLoadedOrMidload(chunkString))
		{
			return;
		}
		WindowControl.Instance.OpenMiniwindow(WindowControl.miniwindow_type_t.land_claim);
		WindowControl.Instance.HideMiniwindowHeaders();
		WindowPrefabsControl.Instance.CreateScreen("LandClaim", WindowPrefabsControl.build_into_t.mini_window);
		WindowPrefabsControl.Instance.GetObject("LandClaim", "header-img").GetComponent<ItemSprite>().RedrawBasic(new InventoryItem(land_claim_item_name), 1);
		if (land_claim_item_name == "Old Land Claim")
		{
			land_screen_context = land_screen_context_t.open_expired;
			WindowPrefabsControl.Instance.GetTextLegacy("LandClaim", "range-text").text = "-";
			WindowPrefabsControl.Instance.GetTextLegacy("LandClaim", "expire-text").text = "<color=#FF5A6A>EXPIRED</color>";
			WindowPrefabsControl.Instance.GetObject("LandClaim", "player1").gameObject.SetActive(false);
			WindowPrefabsControl.Instance.GetObject("LandClaim", "add1").gameObject.SetActive(true);
			WindowPrefabsControl.Instance.GetObject("LandClaim", "add1").transform.Find("Placeholder").gameObject.SetActive(false);
			WindowPrefabsControl.Instance.GetObject("LandClaim", "add2").transform.Find("Placeholder").gameObject.SetActive(false);
			WindowPrefabsControl.Instance.GetObject("LandClaim", "add3").transform.Find("Placeholder").gameObject.SetActive(false);
			WindowPrefabsControl.Instance.GetObject("LandClaim", "add1").GetComponent<InputField>().interactable = false;
			WindowPrefabsControl.Instance.GetObject("LandClaim", "add2").GetComponent<InputField>().interactable = false;
			WindowPrefabsControl.Instance.GetObject("LandClaim", "add3").GetComponent<InputField>().interactable = false;
		}
		else
		{
			land_screen_context = land_screen_context_t.open_active;
			GetLandClaimPlayers(ChunkControl.Instance.GetChunkData(chunkString), zone + "," + chunkX + "," + chunkZ + "," + innerX + "," + innerZ);
			RedrawLandClaimPlayers();
		}
	}

	public void OnClose()
	{
		if (land_screen_context != land_screen_context_t.none)
		{
			GameServerSender.Instance.SendReleaseInteractingObject();
		}
		WindowPrefabsControl.Instance.DestroyScreen("LandClaim");
		land_screen_context = land_screen_context_t.none;
	}

	public void RedrawTimeRemaining()
	{
		Text textLegacy = WindowPrefabsControl.Instance.GetTextLegacy("LandClaim", "expire-text");
		if (!GameController.Instance.interacting_element_item.HasActiveOrExpiredRespawn("landclaim_spawn"))
		{
			textLegacy.text = "???";
			return;
		}
		DateTime respawnDateTime = GameController.Instance.interacting_element_item.GetRespawnDateTime("landclaim_spawn");
		DateTime utcNow = DateTime.UtcNow;
		int num = (int)(respawnDateTime - utcNow).TotalDays;
		int num2 = (int)(respawnDateTime - utcNow).TotalHours - num * 24;
		int num3 = (int)(respawnDateTime - utcNow).TotalMinutes - num * 1440 - num2 * 60;
		int num4 = (int)(respawnDateTime - utcNow).TotalSeconds - num * 86400 - num2 * 3600 - num3 * 60;
		if (num > 1000)
		{
			textLegacy.text = "NEVER";
		}
		else if (num >= 1)
		{
			textLegacy.text = num + " days, " + num2 + " hours";
		}
		else if (num2 > 0)
		{
			textLegacy.text = num2 + " hours, " + num3 + " mins";
		}
		else if (num3 > 0)
		{
			textLegacy.text = num3 + " mins, " + num4 + " seconds";
		}
		else if (num4 >= 1)
		{
			textLegacy.text = num4 + " seconds";
		}
		else
		{
			textLegacy.text = "0 seconds";
			WindowControl.Instance.CloseMiniwindow(true);
		}
	}

	public void AddLandClaimsToNearbyChunks(string zone, int chunkX, int chunkZ, int innerX, int innerZ, InventoryItem landclaim_item, string builder_player)
	{
		DateTime when_to_respawn = DateTime.UtcNow;
		if (landclaim_item.item_name == "3-day Land Claim")
		{
			when_to_respawn = when_to_respawn.AddSeconds(three_day_land_claim_add_seconds);
			when_to_respawn = when_to_respawn.AddDays(three_day_land_claim_add_days);
		}
		else if (landclaim_item.item_name == "8-day Land Claim")
		{
			when_to_respawn = when_to_respawn.AddSeconds(eight_day_land_claim_add_seconds);
			when_to_respawn = when_to_respawn.AddDays(eight_day_land_claim_add_days);
		}
		else if (landclaim_item.item_name == "Admin Land Claim")
		{
			when_to_respawn = when_to_respawn.AddSeconds(admin_land_claim_add_seconds);
			when_to_respawn = when_to_respawn.AddDays(admin_land_claim_add_days);
		}
		string land_claim_str = zone + "," + chunkX + "," + chunkZ + "," + innerX + "," + innerZ;
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				string chunkString = ChunkControl.Instance.GetChunkString(zone, chunkX + i, chunkZ + j);
				string chunkFilename = ChunkControl.GetChunkFilename(chunkString);
				if (ChunkControl.Instance.IsChunkFullyLoadedOrMidload(chunkString))
				{
					ChunkControl.Instance.GetChunkData(chunkString).AddLandClaimChunkTimer(land_claim_str, builder_player, "", "", chunkString, when_to_respawn, false);
				}
				else if (GameServerConnector.Instance.ShouldSaveLocally() && PlayerData.Instance.GetSlotShort("exists", chunkFilename, chunkString) == 1)
				{
					ChunkData chunkData = ChunkControl.Instance.HostGetChunk(zone, chunkX + i, chunkZ + j);
					chunkData.AddLandClaimChunkTimer(land_claim_str, builder_player, "", "", chunkString, when_to_respawn, false);
					chunkData.SaveLandClaimChunkTimersToDisk(chunkString);
				}
			}
		}
	}

	public void RemoveLandClaimsFromNearbyChunks(string zone, int chunkX, int chunkZ, int innerX, int innerZ)
	{
		string land_claim_str = zone + "," + chunkX + "," + chunkZ + "," + innerX + "," + innerZ;
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				string chunkString = ChunkControl.Instance.GetChunkString(zone, chunkX + i, chunkZ + j);
				string chunkFilename = ChunkControl.GetChunkFilename(chunkString);
				if (ChunkControl.Instance.IsChunkFullyLoadedOrMidload(chunkString))
				{
					ChunkControl.Instance.GetChunkData(chunkString).RemoveLandClaimChunkTimers(land_claim_str);
				}
				else if (GameServerConnector.Instance.ShouldSaveLocally() && PlayerData.Instance.GetSlotShort("exists", chunkFilename, chunkString) == 1)
				{
					ChunkData chunkData = ChunkControl.Instance.HostGetChunk(zone, chunkX + i, chunkZ + j);
					chunkData.RemoveLandClaimChunkTimers(land_claim_str);
					chunkData.SaveLandClaimChunkTimersToDisk(chunkString);
				}
			}
		}
	}
}
