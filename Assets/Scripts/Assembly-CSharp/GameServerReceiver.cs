using System.Collections.Generic;
using UnityEngine;

public class GameServerReceiver : MonoBehaviour, OrderedStart
{
	public enum game_command
	{
		none = 0,
		ping = 1,
		login_succeed = 2,
		initial_player_data = 3,
		request_initial_unique_ids_from_host = 4,
		initial_game_data = 5,
		chat_message = 6,
		notif_other_player_status = 7,
		notif_A_killed_B = 8,
		notif_guard_died = 9,
		request_zone_data = 10,
		got_zone_data = 11,
		request_chunk = 12,
		got_chunk = 13,
		move_at = 17,
		update_many_nearby = 18,
		update_one_nearby = 19,
		change_zone = 20,
		start_teleport = 21,
		end_teleport = 22,
		resync_daynight = 23,
		change_equipment = 24,
		change_parent_creatures = 25,
		open_basket = 26,
		got_basket_contents = 27,
		req_basket_contents = 28,
		generate_your_own_goldchest = 29,
		close_basket = 30,
		build_object = 32,
		remove_object = 33,
		replace_object = 34,
		modify_land_claim_user = 35,
		modify_land_claims_outside = 36,
		your_zone_data_changed = 37,
		request_zone_trail = 38,
		claim_object = 39,
		release_object = 40,
		request_more_unique_ids = 41,
		got_more_unique_ids = 42,
		used_unique_id = 43,
		delete_furniture_file = 44,
		real_time_note_press = 45,
		request_page_of_teleporters = 46,
		got_page_of_teleporters = 47,
		took_teleporter_screenshot = 48,
		request_teleporter_screenshot = 49,
		got_teleporter_screenshot = 50,
		finished_editing_teleporter = 51,
		tele_search_NEW = 52,
		try_challenge_minigame_owner = 53,
		challenge_minigame_response = 54,
		begin_multiplayer_minigame = 55,
		minigame_leave = 56,
		pool_update_cue_position = 57,
		pool_SHOOT = 58,
		pool_sync_ready = 59,
		pool_place_white_ball = 60,
		pool_play_again = 61,
		sit_in_chair = 62,
		try_claim_new_mobs = 63,
		player_deloaded_mob = 64,
		stream_mob_positions = 65,
		request_mob_data = 66,
		got_mob_data = 67,
		try_inherit_mob = 68,
		succeed_inherit_mob = 69,
		attack_animation = 70,
		was_hit = 71,
		mob_die = 72,
		update_creature_stats = 74,
		increase_hp = 75,
		show_exp_receive = 76,
		companion_equip = 78,
		companion_rename = 79,
		companion_destroy = 80,
		apply_new_perk_effect = 81,
		launch_projectile_perk = 82,
		perk_quick_tag = 83,
		pre_applied_perks = 84,
		create_perk_drop = 85,
		respawn = 86,
		back_to_breeder = 87,
		update_synced_target_list = 88,
		created_local_mob = 89,
		bandit_flag_destroyed = 90
	}

	public static GameServerReceiver Instance;

	public int max_companions = 2;

	public Dictionary<string, Sprite> cached_teleporter_textures = new Dictionary<string, Sprite>();

	public Dictionary<string, List<int>> unique_ids_given_away = new Dictionary<string, List<int>>();

	private List<string> full_bandit_camps_sent_to_server = new List<string>();

	public GameObject report_object_button;

	public List<string> disabled_perks = new List<string>();

	public bool waiting_on_initial_zone_data;

	public Connection connection => GameServerConnector.Instance.game_server_connection;

	public GameServerConnector connector => GameServerConnector.Instance;

	public GameServerSender sender => GameServerSender.Instance;

	public GameServerInterface interface_ => GameServerInterface.Instance;

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

	private void ClearPreviousMap()
	{
		MusicBoxControl.Instance.ClearLoadedSongs();
		cached_teleporter_textures.Clear();
		CompanionController.Instance.DestroyTempCompanions();
		QuestControl.Instance.RevertQuestIfNecessary();
		AudioControl.Instance.EndBattleMusic();
		CompanionController.Instance.max_personal_companions_right_now = 0;
		CompanionController.Instance.RecreateAllCompanions();
		MobControl.Instance.ClearAllCreatures(true, false, false);
		interface_.nearby_players.Clear();
		if (GameController.Instance.player != null)
		{
			GameplayGUIControl.Instance.end_sit_button.SetActive(false);
			GameController.Instance.player.GetComponent<SharedCreature>().EndSittingInChair();
			GameController.Instance.player.GetComponent<PerkReceiver>().ClearAllDurationEffects();
		}
		sender.chunks_mid_request.Clear();
		sender.cached_chunks.Clear();
		ChunkControl.Instance.DestroyAllTerrain();
		BanditCampsControl.Instance.loaded_bandit_camp_instances.Clear();
		BanditCampsControl.Instance.loaded_bandit_camp_maps.Clear();
		full_bandit_camps_sent_to_server.Clear();
		ConstructionControl.Instance.online_unique_ids_.Clear();
		waiting_on_initial_zone_data = true;
		ChunkControl.Instance.ClearCachedFollowObj();
		RemovePlayer();
		BreedControl.Instance.LoadEverythingFromDisk(GameController.Instance.prev_player_pos);
	}

	private void RemovePlayer()
	{
		if (GameController.Instance.player == null) return;
		GameController.Instance.player.GetComponent<Combatant>().Delete();
		GameController.Instance.player.GetComponent<SharedCreature>().Delete();
		Object.Destroy(GameController.Instance.player);
	}

	public void OnReceive(Packet incoming)
	{
		if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Game")
		{
			if (connector.game_server_connection != null) connector.game_server_connection.Disconnect();
			return;
		}
		switch ((game_command)incoming.GetByte())
		{
			case (game_command)1:
			{
				connector.last_server_ping = System.DateTime.UtcNow;
				return;
			}
			case (game_command)2:
			{
				FriendServerSender.Instance.EndTimeout();
				connector.server_name = incoming.GetString();
				connector.is_host = incoming.GetByte() == 1;
				incoming.GetByte();
				connector.pvp_enabled = false;
				sender.packet_validator_code = incoming.GetString();
				sender.packet_validator_total_variation = incoming.GetShort();
				List<string> joined = new List<string>();
				connector.n_others_in_game = incoming.GetShort();
				if (connector.is_host && connector.n_others_in_game != 0)
					for (int i = 0; i < connector.n_others_in_game; i++) joined.Add(incoming.GetString());
				FriendServerSender.Instance.UpdateWorldString();
				connector.completely_logged_in = true;
				connector.StartPinging();
				PopupControl.Instance.HideAll();
				if (!connector.is_host)
				{
					chat_log log = new chat_log("Welcome to <color=#4acfff>" + connector.server_name.Replace("(private)", "") + "'s</color> world", "", FriendServerInterface.Instance.default_server_icon, true, null);
					interface_.game_chat.AddLog(log);
					interface_.GameChatReceived(log);
				}
				if (connector.is_host)
					foreach (string user in joined) interface_.ShowPlayerLogInOrOut(user, 1);
				if (!connector.is_host) ClearPreviousMap();
				else unique_ids_given_away.Clear();
				sender.SendInitialPlayerData();
				return;
			}
			case (game_command)4:
			{
				string user = incoming.GetString();
				Packet outgoing = new Packet();
				outgoing.PutByte(4);
				outgoing.PutString(user);
				for (int i = 0; i < 25; i++)
				{
					int id = ConstructionControl.Instance.GetNewUniqueId(true);
					outgoing.PutLong(id);
					if (!unique_ids_given_away.ContainsKey(user)) unique_ids_given_away.Add(user, new List<int>());
					unique_ids_given_away[user].Add(id);
				}
				connection.Send(outgoing, Connection.priority.SUPER_HIGH);
				return;
			}
			case (game_command)5:
			{
				int count = incoming.GetShort();
				for (int i = 0; i < count; i++) ConstructionControl.Instance.online_unique_ids_.Add(incoming.GetLong());
				ReceiveDaynight(incoming);
				disabled_perks.Clear();
				count = incoming.GetShort();
				for (int i = 0; i < count; i++) disabled_perks.Add(incoming.GetString());
				connector.is_moderator = incoming.GetByte() == 1;
				CompanionController.Instance.max_personal_companions_right_now = incoming.GetByte();
				CompanionController.Instance.RecreateAllCompanions();
				foreach (ActiveCompanion companion in CompanionController.Instance.active_companions) sender.SendCreatedLocalMob(companion.combat_name);
				GameplayGUIControl.Instance.HideGameplayGui();
				GameplayGUIControl.Instance.curr_GUI = (GameplayGUIControl.GUI_layout_t)0;
				GameplayGUIControl.Instance.ShowGameplayGui();
				if (incoming.GetByte() == 0) sender.RequestZoneData(GameController.Instance.GetSavedPlayerZoneOnServer(connector.server_name), (ZoneDataControl.change_zone_type)3, GameController.Instance.GetSavedPlayerPositionOnServer(connector.server_name));
				connector.pvp_enabled = incoming.GetByte() == 1;
				incoming.GetByte();
				return;
			}
			case (game_command)6:
			{
				string user = incoming.GetString();
				string punctuated = incoming.GetString();
				string message = incoming.GetString();
				byte friendsOnly = incoming.GetByte();
				if (friendsOnly == 1 && user != PlayerData.Instance.GetGlobalString("username_lower"))
				{
					bool found = false;
					foreach (Friend friend in FriendServerReceiver.Instance.friends)
						if (friend.username_lower == user) { found = true; break; }
					if (!found) return;
				}
				chat_log log = new chat_log("<color=#abebff>" + punctuated + ":</color> " + message, message, null, false, null);
				interface_.game_chat.AddLog(log);
				interface_.GameChatReceived(log);
				FriendServerReceiver.Instance.AddToRecentlySeenPlayers(user, punctuated, message);
				return;
			}
			case (game_command)7:
			{
				string user = incoming.GetString();
				string punctuated = incoming.GetString();
				byte state = incoming.GetByte();
				interface_.ShowPlayerLogInOrOut(punctuated, state);
				if (state == 1) connector.n_others_in_game++;
				else if (state == 0)
				{
					connector.n_others_in_game--;
					if (connector.is_host && unique_ids_given_away.ContainsKey(user))
					{
						ConstructionControl.Instance.RecycleUniqueIds(unique_ids_given_away[user]);
						unique_ids_given_away.Remove(user);
					}
				}
				FriendServerSender.Instance.UpdateWorldString();
				return;
			}
			case (game_command)8:
			{
				string killer = incoming.GetString();
				string victim = incoming.GetString();
				string username = PlayerData.Instance.GetGlobalString("username_lower");
				if (killer == username) killer = "You";
				if (victim == username) victim = "you";
				chat_log log = new chat_log("<color=#f2cb88>" + killer + "</color><color=#ff5252> killed " + victim + "</color>", "", CompanionController.Instance.companion_died_ico, false, null);
				interface_.game_chat.AddLog(log);
				interface_.GameChatReceived(log);
				return;
			}
			case (game_command)9:
			{
				string mob = incoming.GetString();
				string owner = incoming.GetString();
				CompanionController.Instance.OnGuardDie(mob, owner);
				return;
			}
			case (game_command)10:
			{
				string user = incoming.GetString();
				string zone = incoming.GetString();
				byte type = incoming.GetByte();
				Vector3 position = type == 2 || type == 3 ? UnpackPosition(incoming) : Vector3.zero;
				Packet outgoing = new Packet();
				outgoing.PutByte(11);
				outgoing.PutString(user);
				outgoing.PutString(zone);
				outgoing.PutByte(type);
				if (type == 2 || type == 3) sender.PackPosition(outgoing, position);
				ZoneData data = ZoneDataControl.Instance.LoadZoneDataFromDisk(zone);
				data.PackForWeb(outgoing);
				data.ClearOutdoorLandClaims();
				List<string> trail = ZoneDataControl.Instance.GetZoneTrail(zone);
				outgoing.PutShort(trail.Count);
				foreach (string name in trail) outgoing.PutString(name);
				connection.Send(outgoing);
				return;
			}
			case (game_command)11:
			{
				sender.StopZoneDataTimeout();
				byte result = incoming.GetByte();
				byte mapChange = incoming.GetByte();
				waiting_on_initial_zone_data = false;
				if (result == 1) interface_.ProcessIncomingZoneData(incoming, mapChange == 1);
				else interface_.UnknownZoneGotoSpawn(true, mapChange == 1);
				return;
			}
			case (game_command)12:
			{
				string user = incoming.GetString();
				string zone = incoming.GetString();
				int x = incoming.GetShort();
				int z = incoming.GetShort();
				string key = ChunkControl.Instance.GetChunkString(zone, x, z);
				bool loaded = ChunkControl.Instance.IsChunkFullyLoadedOrMidload(key);
				ChunkData data = loaded ? ChunkControl.Instance.GetChunkData(key) : ChunkControl.Instance.HostGetChunk(zone, x, z);
				List<string> camps = data.DetermineBanditCampsWithinChunk(ZoneDataControl.Instance.LoadZoneDataFromDisk(zone).house_item);
				List<string> sendCamps = new List<string>();
				foreach (string camp in camps)
				{
					if (!full_bandit_camps_sent_to_server.Contains(camp))
					{
						sendCamps.Add(camp);
						full_bandit_camps_sent_to_server.Add(camp);
					}
				}
				Packet outgoing = new Packet();
				outgoing.PutByte(13);
				outgoing.PutString(user);
				data.PackForWeb(outgoing);
				outgoing.PutByte((byte)sendCamps.Count);
				foreach (string camp in sendCamps) BanditCampsControl.Instance.GetBanditCampInstanceByName(camp).PackForWeb(outgoing);
				connection.Send(outgoing);
				if (!loaded) data.SaveLandClaimChunkTimersToDisk(key);
				return;
			}
			case (game_command)13:
			{
				string zone = incoming.GetString();
				int x = incoming.GetShort();
				int z = incoming.GetShort();
				string key = ChunkControl.Instance.GetChunkString(zone, x, z);
				byte type = incoming.GetByte();
				string cache = incoming.GetString();
				ChunkData data = null;
				bool reload = false;
				if (type == 1)
				{
					if (sender.cached_chunks.ContainsKey(key) && sender.cached_chunks[key].mp_cache_key == cache) data = sender.cached_chunks[key];
					else
					{
						ChunkControl.Instance.ChangeChunkStatus(key, Chunk.status_t.pls_load_and_build);
						reload = true;
					}
				}
				else if (type == 0)
				{
					data = new ChunkData();
					data.UnpackFromWeb(incoming);
					data.mp_cache_key = cache;
					if (data.mp_chunk_size > 75)
					{
						if (!sender.cached_chunks.ContainsKey(key))
						{
							if (sender.cached_chunks.Count > 40)
							{
								string oldest = "";
								double age = 0;
								foreach (KeyValuePair<string, ChunkData> entry in sender.cached_chunks)
								{
									double seconds = (System.DateTime.UtcNow - entry.Value.mp_cache_last_used).TotalSeconds;
									if (seconds > age) { oldest = entry.Key; age = seconds; }
								}
								if (oldest != "") sender.cached_chunks.Remove(oldest);
							}
							sender.cached_chunks.Add(key, data);
						}
						else sender.cached_chunks[key] = data;
					}
				}
				int count = incoming.GetShort();
				for (int i = 0; i < count; i++)
				{
					BanditCampInstance camp = BanditCampInstance.UnpackFromWeb(incoming);
					if (!BanditCampsControl.Instance.loaded_bandit_camp_instances.ContainsKey(camp.instance_name))
						BanditCampsControl.Instance.loaded_bandit_camp_instances.Add(camp.instance_name, camp);
				}
				sender.chunks_mid_request.Remove(key);
				if (reload || !ChunkControl.Instance.ChunkExists(key)) return;
				data.mp_cache_last_used = System.DateTime.UtcNow;
				ChunkControl.Instance.GetChunk(key).chunk_data = data;
				ChunkControl.Instance.ChangeChunkStatus(key, Chunk.status_t.MP_got_data);
				return;
			}
			case (game_command)17:
			{
				string user = incoming.GetString();
				Vector3 position = UnpackPosition(incoming);
				Vector3 target = UnpackPosition(incoming);
				Quaternion rotation = UnpackRotation(incoming);
				if (!interface_.nearby_players.ContainsKey(user)) return;
				OnlinePlayer player = interface_.nearby_players[user];
				if (player.obj == null) return;
				SharedCreature creature = player.obj.GetComponent<SharedCreature>();
				if (creature.snapped_to_chair_obj || creature.teleport_particle != null) return;
				if (player.snap_player_to_realpos_counter == 0)
				{
					interface_.CreateMovementSmoother(player.obj, position, target);
					player.snap_player_to_realpos_counter = 3;
				}
				player.snap_player_to_realpos_counter--;
				creature.SetMoveTo(target);
				creature.SnapSpotterRotation(rotation);
				return;
			}
			case (game_command)18:
			{
				int count = incoming.GetShort();
				for (int i = 0; i < count; i++) interface_.NewPlayerNearby(incoming);
				count = incoming.GetShort();
				for (int i = 0; i < count; i++) interface_.NearbyPlayerWentAway(incoming);
				return;
			}
			case (game_command)19:
			{
				byte type = incoming.GetByte();
				if (type == 0) interface_.NearbyPlayerWentAway(incoming);
				else if (type == 1) interface_.NewPlayerNearby(incoming);
				return;
			}
			case (game_command)21:
			{
				interface_.StartTeleportPlayer(incoming.GetString());
				return;
			}
			case (game_command)22:
			{
				string user = incoming.GetString();
				interface_.EndTeleportPlayer(user, UnpackPosition(incoming));
				return;
			}
			case (game_command)23:
			{
				ReceiveDaynight(incoming);
				return;
			}
			case (game_command)24:
			{
				string user = incoming.GetString();
				byte type = incoming.GetByte();
				interface_.PlayerChangeEquip(user, type, InventoryItem.UnpackFromWeb(incoming));
				return;
			}
			case (game_command)25:
			{
				string user = incoming.GetString();
				List<string> parents = new List<string>();
				int count = incoming.GetShort();
				for (int i = 0; i < count; i++) parents.Add(incoming.GetString());
				interface_.OtherPlayerChangeCreatures(user, parents);
				return;
			}
			case (game_command)27:
			{
				PopupControl.Instance.HideAll();
				int id = incoming.GetLong();
				BasketContents contents = new BasketContents(incoming);
				inventory_ctr.Instance.SucceedOpenWorldContainer(id, contents);
				return;
			}
			case (game_command)28:
			{
				string user = incoming.GetString();
				int id = incoming.GetLong();
				BasketContents contents = new BasketContents();
				contents.LoadFromDiskAsContainer(id);
				Packet outgoing = new Packet();
				outgoing.PutByte(27);
				outgoing.PutString(user);
				outgoing.PutLong(id);
				contents.Pack(outgoing);
				connection.Send(outgoing);
				return;
			}
			case (game_command)29:
			{
				PopupControl.Instance.HideAll();
				int id = incoming.GetLong();
				BasketContents contents = LootControl.Instance.GenerateLootChest(GameController.Instance.interacting_element_item);
				inventory_ctr.Instance.AddChestRespawnAndRedraw();
				inventory_ctr.Instance.SucceedOpenWorldContainer(id, contents);
				return;
			}
			case (game_command)30:
			{
				int id = incoming.GetLong();
				BasketContents contents = new BasketContents(incoming);
				incoming.GetString();
				contents.SaveToAllAsContainer(id);
				return;
			}
			case (game_command)32:
			{
				InventoryItem item = InventoryItem.UnpackFromWeb(incoming);
				byte rotation = incoming.GetByte();
				string zone = incoming.GetString();
				int x = incoming.GetShort();
				int z = incoming.GetShort();
				int innerX = incoming.GetShort();
				int innerZ = incoming.GetShort();
				string builder = incoming.GetString();
				string cache = incoming.GetString();
				string key = ChunkControl.Instance.GetChunkString(zone, x, z);
				if (!ChunkControl.Instance.IsChunkFullyLoadedOrMidload(key) && !connector.is_host)
				{
					if (item.item_name == "3-day Land Claim" || item.item_name == "8-day Land Claim" || item.item_name == "Admin Land Claim")
						LandClaimControl.Instance.AddLandClaimsToNearbyChunks(zone, x, z, innerX, innerZ, item, builder);
					return;
				}
				ConstructionControl.Instance.PlayerBuildAt(item, zone, x, z, innerX, innerZ, rotation, ConstructionControl.build_context_t.on_other_build_new, builder, cache);
				return;
			}
			case (game_command)33:
			{
				string zone = incoming.GetString();
				int x = incoming.GetShort();
				int z = incoming.GetShort();
				int innerX = incoming.GetShort();
				int innerZ = incoming.GetShort();
				byte rotation = incoming.GetByte();
				InventoryItem item = InventoryItem.UnpackFromWeb(incoming);
				ChunkElement element = new ChunkElement(item, rotation);
				string cache = incoming.GetString();
				string key = ChunkControl.Instance.GetChunkString(zone, x, z);
				if (!ChunkControl.Instance.IsChunkFullyLoadedOrMidload(key) && !connector.is_host)
				{
					if (item.item_name == "3-day Land Claim" || item.item_name == "8-day Land Claim" || item.item_name == "Admin Land Claim")
						LandClaimControl.Instance.RemoveLandClaimsFromNearbyChunks(zone, x, z, innerX, innerZ);
					return;
				}
				ConstructionControl.Instance.PlayerRemoveAt(element, zone, x, z, innerX, innerZ, ConstructionControl.remove_context.other_remove, cache);
				return;
			}
			case (game_command)34:
			{
				InventoryItem item = InventoryItem.UnpackFromWeb(incoming);
				InventoryItem oldItem = InventoryItem.UnpackFromWeb(incoming);
				byte rotation = incoming.GetByte();
				string zone = incoming.GetString();
				int x = incoming.GetShort();
				int z = incoming.GetShort();
				int innerX = incoming.GetShort();
				int innerZ = incoming.GetShort();
				string cache = incoming.GetString();
				if (oldItem.item_name == "Music Box") MusicBoxControl.Instance.remove_song(zone + "," + x + "," + z + "," + innerX + "," + innerZ);
				if (ChunkControl.Instance.IsChunkFullyLoadedOrMidload(ChunkControl.Instance.GetChunkString(zone, x, z)) || connector.is_host)
					ConstructionControl.Instance.PlayerReplaceAt(item, oldItem, rotation, zone, x, z, innerX, innerZ, false, cache);
				return;
			}
			case (game_command)35:
			{
				string zone = incoming.GetString();
				int x = incoming.GetShort();
				int z = incoming.GetShort();
				int innerX = incoming.GetShort();
				int innerZ = incoming.GetShort();
				int index = incoming.GetByte();
				string user = incoming.GetString();
				string[] keys = new string[9];
				for (int i = 0; i < 9; i++) keys[i] = incoming.GetString();
				LandClaimControl.Instance.ModifyLandClaimTimer(zone, x, z, innerX, innerZ, index, user, keys);
				return;
			}
			case (game_command)36:
			{
				byte type = incoming.GetByte();
				string key = incoming.GetString();
				Dictionary<string, LandClaimChunkTimer> timers = ZoneDataControl.Instance.curr_zonedata.outdoor_land_claim_chunk_timers;
				if (type == 2) timers.Remove(key);
				else if (type == 1)
				{
					byte index = incoming.GetByte();
					string user = incoming.GetString();
					if (!timers.ContainsKey(key)) return;
					if (index == 1) timers[key].land_claim_user1 = user;
					else if (index == 2) timers[key].land_claim_user2 = user;
				}
				else if (type == 0)
				{
					int seconds = incoming.GetShort();
					int minutes = incoming.GetShort();
					int hours = incoming.GetShort();
					int days = incoming.GetShort();
					string user = incoming.GetString();
					System.DateTime expire = System.DateTime.UtcNow.AddSeconds(seconds).AddMinutes(minutes).AddHours(hours).AddDays(days);
					timers[key] = ChunkData.CreateLandClaimChunkTimer(key, user, "", "", expire);
				}
				return;
			}
			case (game_command)37:
			{
				ZoneDataControl.Instance.curr_zonedata.ClearOutdoorLandClaims();
				ZoneDataControl.Instance.curr_zonedata = ZoneData.UnpackFromWeb(incoming, ChunkControl.Instance.player_zone);
				ZoneDataControl.Instance.UpdateZoneItemOnChangedOutside();
				return;
			}
			case (game_command)38:
			{
				int count = incoming.GetShort();
				List<string> trail = new List<string>();
				string user = incoming.GetString();
				for (int i = 0; i < count; i++)
				{
					trail.Add(user);
					user = incoming.GetString();
				}
				string zone = incoming.GetString();
				byte type = incoming.GetByte();
				Vector3 position = type == 2 || type == 3 ? UnpackPosition(incoming) : Vector3.zero;
				Packet outgoing = new Packet();
				outgoing.PutByte(38);
				outgoing.PutShort(count);
				foreach (string name in trail)
				{
					outgoing.PutString(name);
					ZoneData data = ZoneDataControl.Instance.LoadZoneDataFromDisk(name);
					data.PackForWeb(outgoing);
					data.ClearOutdoorLandClaims();
				}
				outgoing.PutString(user);
				outgoing.PutString(zone);
				outgoing.PutByte(type);
				if (type == 2 || type == 3) sender.PackPosition(outgoing, position);
				connection.Send(outgoing);
				return;
			}
			case (game_command)39:
			{
				string user = incoming.GetString();
				string obj = incoming.GetString();
				if (interface_.nearby_players.ContainsKey(user)) interface_.nearby_players[user].currently_using = obj;
				return;
			}
			case (game_command)40:
			{
				string user = incoming.GetString();
				if (interface_.nearby_players.ContainsKey(user)) interface_.nearby_players[user].currently_using = "";
				return;
			}
			case (game_command)41:
			{
				string user = incoming.GetString();
				Packet outgoing = new Packet();
				outgoing.PutByte(42);
				outgoing.PutString(user);
				outgoing.PutShort(10);
				for (int i = 0; i < 10; i++)
				{
					int id = ConstructionControl.Instance.GetNewUniqueId(true);
					outgoing.PutLong(id);
					if (!unique_ids_given_away.ContainsKey(user)) unique_ids_given_away.Add(user, new List<int>());
					unique_ids_given_away[user].Add(id);
				}
				connection.Send(outgoing);
				Debug.Log("sent:10 more unique ids");
				return;
			}
			case (game_command)42:
			{
				int count = incoming.GetShort();
				for (int i = 0; i < count; i++) ConstructionControl.Instance.online_unique_ids_.Add(incoming.GetLong());
				sender.requesting_unique_ids = false;
				return;
			}
			case (game_command)43:
			{
				string user = incoming.GetString();
				int id = incoming.GetLong();
				if (unique_ids_given_away.ContainsKey(user)) unique_ids_given_away[user].Remove(id);
				Debug.Log("[unique id used up!]");
				return;
			}
			case (game_command)45:
			{
				string user = incoming.GetString();
				byte type = incoming.GetByte();
				int octave = incoming.GetShort();
				int key = incoming.GetShort();
				int instrument = incoming.GetShort();
				if (type == 0) MusicBoxControl.Instance.remove_online_finger_note(user + "," + (octave + key) + "," + instrument);
				else if (type == 1) MusicBoxControl.Instance.online_finger_pressed(user, octave, key, instrument);
				return;
			}
			case (game_command)46:
			{
				string user = incoming.GetString();
				byte type = incoming.GetByte();
				if (type == 0) sender.PackPageOfTeleporters(user, incoming.GetShort());
				else if (type == 1)
				{
					string zone = incoming.GetString();
					int x = incoming.GetShort();
					int z = incoming.GetShort();
					int ix = incoming.GetShort();
					int iz = incoming.GetShort();
					int id = CustomTeleporterControl.Instance.GetCustomTeleId(zone, x, z, ix, iz);
					sender.PackPageOfTeleporters(user, CustomTeleporterControl.Instance.FindCorrespondingTelePage(id));
				}
				return;
			}
			case (game_command)47:
			{
				PopupControl.Instance.HideAll();
				inventory_ctr.Instance.crafting_Tab.SetActive(true);
				inventory_ctr.Instance.LayOutCraftingTab((inventory_ctr.background_strip_layout)0);
				int page = incoming.GetShort();
				byte search = incoming.GetByte();
				byte more = incoming.GetByte();
				for (int i = 0; i < 3; i++)
				{
					OnlineTeleporter tele = null;
					if (incoming.GetByte() == 1)
					{
						tele = new OnlineTeleporter();
						tele.title = incoming.GetString();
						tele.description = incoming.GetString();
						tele.tele_str = incoming.GetString();
						tele.to_zone = incoming.GetString();
						tele.to_chunkX = incoming.GetShort();
						tele.to_chunkZ = incoming.GetShort();
						tele.to_innerX = incoming.GetShort();
						tele.to_innerZ = incoming.GetShort();
						tele.built_by = incoming.GetString();
					}
					if (i == 0) CustomTeleporterControl.Instance.teleporter_L = tele;
					else if (i == 1) CustomTeleporterControl.Instance.teleporter_mid = tele;
					else CustomTeleporterControl.Instance.teleporter_R = tele;
					inventory_ctr.Instance.instantiated_crafting_slots[i].gameObject.SetActive(tele != null);
					if (tele != null) CustomTeleporterControl.Instance.DrawOnlineTeleporterSlot(i, tele);
				}
				if (search == 1) CustomTeleporterControl.Instance.search_page = page;
				else inventory_ctr.Instance.craft_PAGE = page;
				inventory_ctr.Instance.crafting_page_left.SetActive(page != 0);
				inventory_ctr.Instance.crafting_page_right.SetActive(more == 1);
				if (CustomTeleporterControl.Instance.in_search_screen && CustomTeleporterControl.Instance.teleporter_L == null && CustomTeleporterControl.Instance.teleporter_mid == null && CustomTeleporterControl.Instance.teleporter_R == null && page == 0)
					PopupControl.Instance.ShowMessage("0 results found", PopupControl.context.message);
				return;
			}
			case (game_command)48:
			{
				string zone = incoming.GetString();
				int x = incoming.GetShort();
				int z = incoming.GetShort();
				int ix = incoming.GetShort();
				int iz = incoming.GetShort();
				int length = incoming.GetLong();
				byte[] data = new byte[length];
				for (int i = 0; i < length; i++) data[i] = incoming.GetByte();
				int id = CustomTeleporterControl.Instance.GetCustomTeleId(zone, x, z, ix, iz);
				if (id != -1) System.IO.File.WriteAllBytes(System.IO.Path.Combine(Startup.persistentDataPath + System.IO.Path.DirectorySeparatorChar + PlayerData.Instance.GetCurrentSlotFolder(), "tele_graphic_" + id), data);
				return;
			}
			case (game_command)49:
			{
				string user = incoming.GetString();
				string zone = incoming.GetString();
				int x = incoming.GetShort();
				int z = incoming.GetShort();
				int ix = incoming.GetShort();
				int iz = incoming.GetShort();
				int id = CustomTeleporterControl.Instance.GetCustomTeleId(zone, x, z, ix, iz);
				if (id == -1) return;
				string path = System.IO.Path.Combine(Startup.persistentDataPath + System.IO.Path.DirectorySeparatorChar + PlayerData.Instance.GetCurrentSlotFolder(), "tele_graphic_" + id);
				if (!System.IO.File.Exists(path)) return;
				Packet outgoing = new Packet();
				outgoing.PutByte(50);
				outgoing.PutString(user);
				outgoing.PutString(zone);
				outgoing.PutShort(x);
				outgoing.PutShort(z);
				outgoing.PutShort(ix);
				outgoing.PutShort(iz);
				byte[] data = System.IO.File.ReadAllBytes(path);
				outgoing.PutLong(data.Length);
				foreach (byte value in data) outgoing.PutByte(value);
				connection.Send(outgoing, (Connection.priority)3);
				return;
			}
			case (game_command)50:
			{
				string teleStr = incoming.GetString();
				int length = incoming.GetLong();
				byte[] data = new byte[length];
				for (int i = 0; i < length; i++) data[i] = incoming.GetByte();
				Texture2D texture = new Texture2D(150, 150);
				texture.LoadImage(data);
				Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
				if (cached_teleporter_textures.ContainsKey(teleStr)) cached_teleporter_textures[teleStr] = sprite;
				else cached_teleporter_textures.Add(teleStr, sprite);
				if ((int)WindowControl.Instance.curr_miniwindow != 3) return;
				int index;
				if (CustomTeleporterControl.Instance.teleporter_L.tele_str == teleStr) index = 0;
				else if (CustomTeleporterControl.Instance.teleporter_mid.tele_str == teleStr) index = 1;
				else if (CustomTeleporterControl.Instance.teleporter_R.tele_str == teleStr) index = 2;
				else return;
				inventory_ctr.Instance.instantiated_crafting_slots[index].graphic.sprite = cached_teleporter_textures[teleStr];
				return;
			}
			case (game_command)51:
			{
				string title = incoming.GetString();
				string description = incoming.GetString();
				string zone = incoming.GetString();
				int x = incoming.GetShort();
				int z = incoming.GetShort();
				int ix = incoming.GetShort();
				int iz = incoming.GetShort();
				int id = CustomTeleporterControl.Instance.GetCustomTeleId(zone, x, z, ix, iz);
				if (id == -1) return;
				PlayerData.Instance.SetSlotString("teleporter_" + id + "_name", title, (PlayerData.filename_t)4);
				PlayerData.Instance.SetSlotString("teleporter_" + id + "_desc", description, (PlayerData.filename_t)4);
				return;
			}
			case (game_command)53:
			{
				string user = incoming.GetString();
				byte type = incoming.GetByte();
				if (type < 2)
				{
					if (MinigameMenu.Instance == null) return;
					int menu = (int)MinigameMenu.Instance.curr_menu;
					sender.SendMinigameResponse((byte)(menu == 6 ? 2 : menu == 5 ? 1 : menu == 3 ? 3 : 0), user, type);
				}
				else if (type == 2)
				{
					if (TradingTableControl.Instance == null) return;
					sender.SendMinigameResponse((byte)(TradingTableControl.Instance.other_player_has_joined ? 2 : 3), user, 2);
				}
				return;
			}
			case (game_command)54:
			{
				byte response = incoming.GetByte();
				string user = incoming.GetString();
				byte type = incoming.GetByte();
				if (type < 2)
				{
					switch (response)
					{
						case 0:
							PopupControl.Instance.ShowMessage("<color=#43de4f>" + user + "</color> is setting up a game.\nPlease wait.", PopupControl.context.message);
							return;
						case 1:
							PopupControl.Instance.ShowMessage("<color=#43de4f>" + user + "</color> is currently playing against GameGuy.", PopupControl.context.message);
							return;
						case 2:
							PopupControl.Instance.ShowMessage("<color=#43de4f>" + user + "</color> is currently playing against someone else.", PopupControl.context.message);
							return;
						case 3:
							PopupControl.Instance.on_yes_pressed = () =>
							{
								PopupControl.Instance.ShowConnecting("Joining Match");
								sender.SendBeginMinigame(user, 3, type, null);
							};
							PopupControl.Instance.ShowYesNo("Play against <color=#43de4f>" + user + "?</color>", "Yes", "No", PopupControl.context.yesno_ACTION);
							return;
					}
				}
				else if (type == 2)
				{
					if (response == 2) PopupControl.Instance.ShowMessage("<color=#43de4f>" + user + "</color> is currently trading with someone else.", PopupControl.context.message);
					else if (response == 3)
					{
						PopupControl.Instance.on_yes_pressed = () =>
						{
							PopupControl.Instance.ShowConnecting("Joining Trading Table");
							sender.SendBeginMinigame(user, 3, type, null);
						};
						PopupControl.Instance.ShowYesNo("Trade with <color=#43de4f>" + user + "?</color>", "Yes", "No", PopupControl.context.yesno_ACTION);
					}
				}
				return;
			}
			case (game_command)55:
			{
				string user = incoming.GetString();
				byte response = incoming.GetByte();
				byte type = incoming.GetByte();
				if (response == 0) PopupControl.Instance.ShowMessage("Could not start game\n<color=#43de4f>" + user + "</color> left!", PopupControl.context.message);
				else if (response == 1) PopupControl.Instance.ShowMessage("Could not join\n<color=#43de4f>" + user + "</color> already started a game!", PopupControl.context.message);
				else if (response == 2 && type == 0)
				{
					int[] layout = new int[14];
					for (int i = 0; i < 14; i++) layout[i] = incoming.GetByte();
					PopupControl.Instance.HideAll();
					GameController.Instance.OpenPoolTable(user, layout);
				}
				else if (response == 3)
				{
					if (MinigameMenu.Instance == null) sender.SendBeginMinigame(user, 0, type, null);
					else if ((int)MinigameMenu.Instance.curr_menu != 3) sender.SendBeginMinigame(user, 1, type, null);
					else if (type == 0)
					{
						int[] layout = new int[14];
						for (int i = 0; i < 14; i++) layout[i] = incoming.GetByte();
						if (PoolGameControl.Instance != null)
						{
							PoolGameControl.Instance.curr_opponent = user;
							PoolGameControl.Instance.ArrangeBalls(layout);
							PoolGameControl.Instance.StartMpGame(true);
						}
						sender.SendBeginMinigame(user, 2, 0, layout);
					}
				}
				return;
			}
			case (game_command)56:
			{
				if (PoolGameControl.Instance != null)
				{
					PoolGameControl.Instance.other_player_ready = false;
					WindowControl.Instance.PressClose();
					PopupControl.Instance.ShowMessage("Other player left", (PopupControl.context)1);
				}
				return;
			}
			case (game_command)57:
			{
				if (PoolGameControl.Instance != null) PoolGameControl.Instance.TryUpdateCuePosition(incoming.GetLong() / 100f);
				return;
			}
			case (game_command)58:
			{
				if (PoolGameControl.Instance == null) return;
				float angle = incoming.GetLong() / 100f;
				float power = incoming.GetShort() / 100f;
				int count = incoming.GetLong();
				byte[] bytes = new byte[count];
				for (int i = 0; i < count; i++) bytes[i] = incoming.GetByte();
				PoolGameRecording recording = new PoolGameRecording();
				recording.unpack_from_web(bytes);
				PoolGameControl.Instance.MP_next_deg = angle;
				PoolGameControl.Instance.MP_next_power = power;
				PoolGameControl.Instance.MP_next_recording = recording;
				if ((int)PoolGameControl.Instance.GamePhase == 18) PoolGameControl.Instance.ShowMpRecording();
				return;
			}
			case (game_command)59:
			{
				if (PoolGameControl.Instance != null) PoolGameControl.Instance.OnOtherPlayerReady();
				return;
			}
			case (game_command)60:
			{
				if (PoolGameControl.Instance != null)
				{
					int x = incoming.GetLong();
					int y = incoming.GetLong();
					PoolGameControl.Instance.PlaceWhiteBallAt(new Vector2(x, y));
				}
				return;
			}
			case (game_command)61:
			{
				int[] layout = new int[14];
				for (int i = 0; i < 14; i++) layout[i] = incoming.GetByte();
				if (PoolGameControl.Instance != null)
				{
					PoolGameControl.Instance.ArrangeBalls(layout);
					PoolGameControl.Instance.RestartMpGame();
				}
				return;
			}
			case (game_command)62:
			{
				string user = incoming.GetString();
				string chair = incoming.GetString();
				GameObject obj = interface_.GetPlayerByUsername(user);
				if (obj != null)
				{
					if (chair == "") obj.GetComponent<SharedCreature>().EndSittingInChair();
					else obj.GetComponent<SharedCreature>().TrySitInChairObj(chair);
				}
				return;
			}
			case (game_command)63:
			{
				int count = incoming.GetByte();
				for (int i = 0; i < count; i++)
				{
					string id = incoming.GetString();
					byte succeeded = incoming.GetByte();
					if (succeeded == 1 && sender.mobs_I_am_trying_to_claim_awaiting_response.ContainsKey(id) && !MobControl.Instance.active_combatants.ContainsKey(id))
					{
						CreatureStruct creature = sender.mobs_I_am_trying_to_claim_awaiting_response[id];
						Vector3 position = new Vector3(creature.original_element_chunkX * 10 + creature.original_element_innerX + creature.spawn_offset_x + 0.5f, 1.5f, creature.original_element_chunkZ * 10 + creature.original_element_innerZ + creature.spawn_offset_z + 0.5f);
						MobControl.Instance.SpawnLocalMob(creature, position);
					}
					sender.mobs_I_am_trying_to_claim_awaiting_response.Remove(id);
				}
				return;
			}
			case (game_command)64:
			{
				string id = incoming.GetString();
				if (!MobControl.Instance.active_combatants.ContainsKey(id)) return;
				GameObject obj = MobControl.Instance.active_combatants[id];
				string key = ChunkControl.Instance.GetChunkString(obj.transform.position);
				SharedCreature creature = obj.GetComponent<SharedCreature>();
				if ((int)creature.brain_type != 9 && (int)creature.brain_type != 6 && ChunkControl.Instance.IsChunkFullyLoadedOrMidload(key))
				{
					Packet outgoing = new Packet();
					outgoing.PutByte(68);
					outgoing.PutString(id);
					connection.Send(outgoing);
				}
				else Object.Destroy(obj);
				return;
			}
			case (game_command)65:
			{
				List<string> requested = new List<string>();
				string user = incoming.GetString();
				bool smooth = false;
				if (interface_.nearby_players.ContainsKey(user))
				{
					OnlinePlayer owner = interface_.nearby_players[user];
					smooth = owner.snap_mobs_to_realpos_counter == 0;
					if (smooth) owner.snap_mobs_to_realpos_counter = 5;
					owner.snap_mobs_to_realpos_counter--;
				}
				int count = incoming.GetByte();
				for (int i = 0; i < count; i++)
				{
					string id = incoming.GetString();
					Vector3 position = UnpackPosition(incoming);
					Vector3 target = UnpackPosition(incoming);
					Quaternion rotation = UnpackRotation(incoming);
					if (!MobControl.Instance.active_combatants.ContainsKey(id))
					{
						if (!sender.other_players_mobs_that_I_inquired_about.Contains(id))
						{
							requested.Add(id);
							sender.other_players_mobs_that_I_inquired_about.Add(id);
						}
					}
					else
					{
						GameObject obj = MobControl.Instance.active_combatants[id];
						if (obj != null)
						{
							if (smooth) interface_.CreateMovementSmoother(obj, position, target);
							obj.GetComponent<SharedCreature>().SetMoveTo(target);
							obj.GetComponent<SharedCreature>().SnapSpotterRotation(rotation);
						}
					}
				}
				if (requested.Count == 0) return;
				Packet outgoing = new Packet();
				outgoing.PutByte(66);
				outgoing.PutString(user);
				outgoing.PutByte((byte)requested.Count);
				foreach (string id in requested) outgoing.PutString(id);
				connection.Send(outgoing);
				return;
			}
			case (game_command)66:
			{
				string user = incoming.GetString();
				Packet outgoing = new Packet();
				outgoing.PutByte(67);
				outgoing.PutString(user);
				int count = incoming.GetByte();
				outgoing.PutByte((byte)count);
				for (int i = 0; i < count; i++)
				{
					string id = incoming.GetString();
					outgoing.PutString(id);
					if (!MobControl.Instance.active_combatants.ContainsKey(id))
					{
						outgoing.PutByte(0);
						continue;
					}
					GameObject obj = MobControl.Instance.active_combatants[id];
					SharedCreature creature = obj.GetComponent<SharedCreature>();
					if (!MobControl.Instance.my_claimed_creatures.Contains(id) && (!creature.is_local_mob || (int)creature.brain_type != 6))
					{
						outgoing.PutByte(0);
						continue;
					}
					outgoing.PutByte(1);
					MobControl.Instance.ObjToCreatureStruct(obj).Pack(outgoing);
					outgoing.PutString(id);
					sender.PackPosition(outgoing, obj.transform.position);
				}
				connection.Send(outgoing);
				return;
			}
			case (game_command)67:
			{
				string user = incoming.GetString();
				int count = incoming.GetByte();
				for (int i = 0; i < count; i++)
				{
					string id = incoming.GetString();
					byte succeeded = incoming.GetByte();
					if (succeeded == 1)
					{
						CreatureStruct creature = CreatureStruct.PacketToCreatureStruct(incoming);
						string combatId = incoming.GetString();
						Vector3 position = UnpackPosition(incoming);
						if (interface_.nearby_players.ContainsKey(user)) MobControl.Instance.SpawnNetMob(creature, position, combatId);
					}
					sender.other_players_mobs_that_I_inquired_about.Remove(id);
				}
				return;
			}
			case (game_command)69:
			{
				string id = incoming.GetString();
				if (!MobControl.Instance.active_combatants.ContainsKey(id)) return;
				GameObject obj = MobControl.Instance.active_combatants[id];
				if (obj == null) return;
				Vector3 position = obj.transform.position;
				Quaternion rotation = obj.transform.rotation;
				CreatureStruct creature = MobControl.Instance.ObjToCreatureStruct(obj);
				obj.GetComponent<SharedCreature>().Delete();
				obj.GetComponent<Combatant>().Delete();
				Object.Destroy(obj);
				MobControl.Instance.SpawnLocalMob(creature, position).transform.rotation = rotation;
				return;
			}
			case (game_command)70:
			{
				string id = incoming.GetString();
				if (MobControl.Instance.active_combatants.ContainsKey(id)) MobControl.Instance.active_combatants[id].GetComponent<SharedCreature>().VisuallyAttack();
				return;
			}
			case (game_command)71:
			{
				string id = incoming.GetString();
				int damage = incoming.GetLong();
				int fakeDamage = incoming.GetLong();
				Combatant.hit_col color = (Combatant.hit_col)incoming.GetByte();
				bool missed = incoming.GetByte() == 1;
				bool dodged = incoming.GetByte() == 1;
				string attackerId = incoming.GetString();
				if (id == PlayerData.Instance.GetGlobalString("username_lower")) id = "LOCAL";
				if (!MobControl.Instance.active_combatants.ContainsKey(id)) return;
				GameObject attacker = MobControl.Instance.active_combatants.ContainsKey(attackerId) ? MobControl.Instance.active_combatants[attackerId] : null;
				MobControl.Instance.active_combatants[id].GetComponent<Combatant>().WasHit(damage, attacker, missed, dodged, color, false, fakeDamage);
				return;
			}
			case (game_command)72:
			{
				string id = incoming.GetString();
				float delay = incoming.GetShort() / 10f;
				float splatDelay = incoming.GetShort() / 10f;
				string zone = incoming.GetString();
				int x = incoming.GetShort();
				int z = incoming.GetShort();
				int ix = incoming.GetShort();
				int iz = incoming.GetShort();
				int respawn = incoming.GetShort();
				string killerId = incoming.GetString();
				byte darksword = incoming.GetByte();
				byte aether = incoming.GetByte();
				InventoryItem original = InventoryItem.UnpackFromWeb(incoming);
				if (killerId == PlayerData.Instance.GetGlobalString("username_lower")) killerId = "LOCAL";
				if (!MobControl.Instance.active_combatants.ContainsKey(id))
				{
					if (connector.is_host && (zone != "" || x != 0 || z != 0 || ix != 0 || iz != 0))
					{
						QuestControl.Instance.TryNoteQuestMobKilled(zone, x, z, ix, iz, original, id);
						QuestControl.Instance.CheckIfAllQuestMobsKilled(zone, x, z, ix, iz, original, id);
					}
					return;
				}
				GameObject killer = MobControl.Instance.active_combatants.ContainsKey(killerId) ? MobControl.Instance.active_combatants[killerId] : null;
				Combatant combatant = MobControl.Instance.active_combatants[id].GetComponent<Combatant>();
				if (darksword == 1) combatant.BeginDarkswordParticles(PerkControl.Instance.prefab_darksword_kill);
				else if (aether == 1) combatant.BeginDarkswordParticles(PerkControl.Instance.prefab_aether_banish);
				combatant.Die(killer, delay, splatDelay, respawn);
				return;
			}
			case (game_command)74:
			{
				string id = incoming.GetString();
				int level = incoming.GetLong();
				int maxHp = incoming.GetLong();
				int hp = incoming.GetLong();
				int regen = incoming.GetLong();
				if (!MobControl.Instance.active_combatants.ContainsKey(id)) return;
				GameObject obj = MobControl.Instance.active_combatants[id];
				SharedCreature creature = obj.GetComponent<SharedCreature>();
				int oldLevel = creature.level;
				creature.level = level;
				obj.GetComponent<Combatant>().HP_max = maxHp;
				obj.GetComponent<Combatant>().hp = hp;
				creature.hp_regen = regen;
				if (level == oldLevel) return;
				creature.ShowLevelupParticles(true, 0f);
				if (!interface_.nearby_players.ContainsKey(id)) creature.RedrawLevelText();
				else if (interface_.nearby_players[id].obj != null) interface_.nearby_players[id].obj.GetComponent<SharedCreature>().RedrawMultiplayerOverhead(id, level);
				return;
			}
			case (game_command)75:
			{
				string id = incoming.GetString();
				int amount = incoming.GetLong();
				if (id == PlayerData.Instance.GetGlobalString("username_lower")) id = "LOCAL";
				if (MobControl.Instance.active_combatants.ContainsKey(id)) MobControl.Instance.active_combatants[id].GetComponent<Combatant>().IncreaseHp(amount);
				return;
			}
			case (game_command)76:
			{
				string text = incoming.GetString();
				GameController.Instance.showOverheadNotif(text, UnpackPosition(incoming), false, false);
				return;
			}
			case (game_command)78:
			{
				string id = incoming.GetString();
				InventoryItem hat = InventoryItem.UnpackFromWeb(incoming);
				InventoryItem body = InventoryItem.UnpackFromWeb(incoming);
				InventoryItem hand = InventoryItem.UnpackFromWeb(incoming);
				if (!MobControl.Instance.active_combatants.ContainsKey(id)) return;
				GameObject obj = MobControl.Instance.active_combatants[id];
				if (obj == null || obj.GetComponent<Combatant>().mob_type != Combatant.TYPE_T.creature) return;
				SharedCreature creature = obj.GetComponent<SharedCreature>();
				if (creature.is_local_mob || (int)creature.brain_type != 6) return;
				creature.hat_ = hat;
				creature.body_ = body;
				creature.hand_ = hand;
				creature.OnEquipmentChanged();
				return;
			}
			case (game_command)79:
			{
				string id = incoming.GetString();
				string name = incoming.GetString();
				if (!MobControl.Instance.active_combatants.ContainsKey(id)) return;
				GameObject obj = MobControl.Instance.active_combatants[id];
				if (obj == null || (int)obj.GetComponent<Combatant>().mob_type != 3) return;
				SharedCreature creature = obj.gameObject.GetComponent<SharedCreature>();
				if (creature.is_local_mob || (int)creature.brain_type != 6) return;
				List<string> parents = creature.myCreatureModel.original.creatures_that_made_me;
				creature.AssignOverheadName(name, MobControl.Instance.GetOverheadNameColor(parents[0] + parents[1]));
				if (creature.levelDisplay != null)
				{
					GameController.Instance.possible_destroy.Remove(creature.levelDisplay);
					Object.Destroy(creature.levelDisplay);
				}
				creature.RedrawLevelDisplay();
				return;
			}
			case (game_command)80:
			{
				string id = incoming.GetString();
				if (!MobControl.Instance.active_combatants.ContainsKey(id)) return;
				GameObject obj = MobControl.Instance.active_combatants[id];
				if (obj == null || obj.GetComponent<Combatant>().mob_type != Combatant.TYPE_T.creature) return;
				SharedCreature creature = obj.GetComponent<SharedCreature>();
				if (!creature.is_local_mob && (int)creature.brain_type == 6) Object.Destroy(obj);
				return;
			}
			case (game_command)81:
			{
				string caster = incoming.GetString();
				int casterLevel = incoming.GetLong();
				string target = incoming.GetString();
				PerkData data = new PerkData();
				data.UnpackFromWeb(incoming);
				int level = incoming.GetShort();
				string effect = incoming.GetString();
				bool reapply = incoming.GetByte() == 1;
				if (target == PlayerData.Instance.GetGlobalString("username_lower")) target = "LOCAL";
				GameObject obj = MobControl.Instance.active_combatants.ContainsKey(target) ? MobControl.Instance.active_combatants[target] : null;
				if (obj != null) obj.GetComponent<PerkReceiver>().ApplyPerkEffect(reapply, effect, data, level, caster, casterLevel, false);
				return;
			}
			case (game_command)82:
			{
				Debug.Log("Received: launch projectile");
				PerkData data = new PerkData();
				data.UnpackFromWeb(incoming);
				int level = incoming.GetShort();
				string target = incoming.GetString();
				string caster = incoming.GetString();
				int casterLevel = incoming.GetLong();
				Vector3 end = UnpackPosition(incoming);
				Vector3 start = UnpackPosition(incoming);
				if (target == PlayerData.Instance.GetGlobalString("username_lower")) target = "LOCAL";
				PerkControl.Instance.LaunchProjectile(data, level, target, caster, casterLevel, end, start);
				return;
			}
			case (game_command)83:
			{
				string id = incoming.GetString();
				byte active = incoming.GetByte();
				if (MobControl.Instance.active_combatants.ContainsKey(id)) MobControl.Instance.active_combatants[id].GetComponent<SharedCreature>().isQuickTagging = active == 1;
				return;
			}
			case (game_command)84:
			{
				string id = incoming.GetString();
				int count = incoming.GetShort();
				for (int i = 0; i < count; i++)
				{
					string caster = incoming.GetString();
					int casterLevel = incoming.GetLong();
					PerkData data = new PerkData();
					data.UnpackFromWeb(incoming);
					int level = incoming.GetShort();
					string effect = incoming.GetString();
					int duration = incoming.GetShort();
					incoming.GetShort();
					if (caster == PlayerData.Instance.GetGlobalString("username_lower")) caster = "LOCAL";
					if (MobControl.Instance.active_combatants.ContainsKey(id))
						MobControl.Instance.active_combatants[id].GetComponent<PerkReceiver>().InitializeDurationTimer(duration, effect, data, level, caster, casterLevel, true);
				}
				return;
			}
			case (game_command)85:
			{
				Vector3 position = UnpackPosition(incoming);
				string effect = incoming.GetString();
				PerkData data = new PerkData();
				data.UnpackFromWeb(incoming);
				int level = incoming.GetShort();
				string caster = incoming.GetString();
				int casterLevel = incoming.GetLong();
				Debug.Log("Got: Create Drop");
				PerkControl.Instance.CreateDrop(position, effect, data, level, caster, casterLevel, false);
				return;
			}
			case (game_command)86:
			{
				string user = incoming.GetString();
				OnlinePlayerData data = new OnlinePlayerData();
				data.Unpack(incoming);
				if (!interface_.nearby_players.ContainsKey(user) || MobControl.Instance.active_combatants.ContainsKey(user)) return;
				MobControl.Instance.SpawnOtherPlayer(interface_.nearby_players[user], data);
				return;
			}
			case (game_command)88:
			{
				string id = incoming.GetString();
				int count = incoming.GetByte();
				List<string> ids = new List<string>();
				for (int i = 0; i < count; i++) ids.Add(incoming.GetString());
				if (MobControl.Instance.active_combatants.ContainsKey(id)) MobControl.Instance.active_combatants[id].GetComponent<SharedCreature>().synced_target_ids = ids;
				return;
			}
			case (game_command)90:
			{
				string name = incoming.GetString();
				if (BanditCampsControl.Instance.loaded_bandit_camp_instances.ContainsKey(name))
				{
					BanditCampInstance camp = BanditCampsControl.Instance.loaded_bandit_camp_instances[name];
					camp.flag_destroyed = true;
					if (connector.is_host) camp.SaveToDisk();
				}
				return;
			}
		}
	}

	public Dictionary<string, string> EncodeObjectIntoReportData(string zone, int chunkX, int chunkZ, int innerX, int innerZ, InventoryItem item)
	{
		Dictionary<string, string> result = new Dictionary<string, string>();
		switch (item.item_name)
		{
			case "Painting": result.Add("category", "report_object_painting"); break;
			case "Companion": result.Add("category", "report_object_companion"); break;
			case "Custom Statue": result.Add("category", "report_object_statue"); break;
			case "Sign": result.Add("category", "report_object_sign"); break;
			case "Merchant Sign": result.Add("category", "report_object_merchant_sign"); break;
		}
		result.Add("zone", zone);
		result.Add("chunkX", chunkX.ToString());
		result.Add("chunkZ", chunkZ.ToString());
		result.Add("innerX", innerX.ToString());
		result.Add("innerZ", innerZ.ToString());
		Dictionary<string, short> shorts = new Dictionary<string, short>();
		Dictionary<string, string> strings = new Dictionary<string, string>();
		Dictionary<string, int> longs = new Dictionary<string, int>();
		foreach (KeyValuePair<string, object> entry in item.GetRawData())
		{
			if (entry.Value.GetType() == typeof(short))
			{
				short value = (short)entry.Value;
				if (value != 0) shorts[entry.Key] = value;
			}
			else if (entry.Value.GetType() == typeof(string))
			{
				string value = (string)entry.Value;
				if (!Startup.StringNullOrEmpty(value)) strings[entry.Key] = value;
			}
			else if (entry.Value.GetType() == typeof(int))
			{
				int value = (int)entry.Value;
				if (value != 0) longs[entry.Key] = value;
			}
		}
		result.Add("n_shorts", shorts.Count.ToString());
		int index = 0;
		foreach (KeyValuePair<string, short> entry in shorts)
		{
			result.Add("short" + index + "_key", entry.Key);
			result.Add("short" + index + "_val", entry.Value.ToString());
			index++;
		}
		result.Add("n_strings", strings.Count.ToString());
		index = 0;
		foreach (KeyValuePair<string, string> entry in strings)
		{
			result.Add("string" + index + "_key", entry.Key);
			result.Add("string" + index + "_val", entry.Value);
			index++;
		}
		result.Add("n_longs", longs.Count.ToString());
		index = 0;
		foreach (KeyValuePair<string, int> entry in longs)
		{
			result.Add("long" + index + "_key", entry.Key);
			result.Add("long" + index + "_val", entry.Value.ToString());
			index++;
		}
		return result;
	}

	public void ShowReportObjectButton(string report_str, Dictionary<string, string> report_data)
	{
		FriendServerInterface.Instance.report_data.Clear();
		FriendServerInterface.Instance.report_data = report_data;
		report_object_button.SetActive(true);
		report_object_button.transform.Find("Text").GetComponent<UnityEngine.UI.Text>().text = report_str;
	}

	public void HideReportObjectButton()
	{
		report_object_button.SetActive(false);
	}

	public void ClickReportObjectButton()
	{
		if ((int)WindowControl.Instance.curr_window == 1) DialogueControl.Instance.CloseWithIntentionOfMiniwindow(true);
		else if ((int)PopupControl.Instance.curr_popup == 15) PopupControl.Instance.PressOkay();
		WindowControl.Instance.OpenMiniwindow((WindowControl.miniwindow_type_t)15);
		DrawReportObjectScreen();
	}

	public void DrawReportObjectScreen()
	{
		WindowPrefabsControl.Instance.CreateScreen("FRIENDS-report-finalize_obj", WindowPrefabsControl.build_into_t.mini_window, WindowPrefabsControl.set_transform_t.over_top);
		UnityEngine.UI.InputField input = WindowPrefabsControl.Instance.GetObject("FRIENDS-report-finalize_obj", "object_input").GetComponent<UnityEngine.UI.InputField>();
		string value = "";
		Dictionary<string, string> data = FriendServerInterface.Instance.report_data;
		if (data.ContainsKey("n_strings"))
		{
			int count = int.Parse(data["n_strings"], Startup.parse_culture);
			for (int i = 0; i < count; i++)
			{
				if (data["string" + i + "_key"] == "item_id")
				{
					value = data["string" + i + "_val"];
					break;
				}
			}
		}
		input.SetTextWithoutNotify(value);
	}

	private void ReceiveDaynight(Packet incoming)
	{
		if (connector.is_host) return;
		int value = incoming.GetShort();
		if ((int)BreedControl.Instance.state_t != 0) return;
		GameController.time_of_day = value / 1000f;
		GameController.Instance.EvalDaynight();
	}

	public Vector3 UnpackPosition(Packet incoming)
	{
		int chunkX = incoming.GetShort();
		int chunkZ = incoming.GetShort();
		int innerX = incoming.GetShort();
		int innerZ = incoming.GetShort();
		Vector3 position = new Vector3(chunkX * 10f + innerX / 10f, 0f, chunkZ * 10f + innerZ / 10f);
		if (position != Vector3.zero) position += Vector3.up * SharedCreature.H;
		return position;
	}

	public Quaternion UnpackRotation(Packet incoming)
	{
		return new Quaternion(incoming.GetShort() / 100f, incoming.GetShort() / 100f, incoming.GetShort() / 100f, incoming.GetShort() / 100f);
	}
}
