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

	public int max_companions;

	public Dictionary<string, Sprite> cached_teleporter_textures;

	public Dictionary<string, List<int>> unique_ids_given_away;

	private List<string> full_bandit_camps_sent_to_server;

	public GameObject report_object_button;

	public List<string> disabled_perks;

	public bool waiting_on_initial_zone_data;

	public Connection connection => null;

	public GameServerConnector connector => null;

	public GameServerSender sender => null;

	public GameServerInterface interface_ => null;

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
	}

	private void RemovePlayer()
	{
	}

	public void OnReceive(Packet incoming)
	{
	}

	public Dictionary<string, string> EncodeObjectIntoReportData(string zone, int chunkX, int chunkZ, int innerX, int innerZ, InventoryItem item)
	{
		return null;
	}

	public void ShowReportObjectButton(string report_str, Dictionary<string, string> report_data)
	{
	}

	public void HideReportObjectButton()
	{
		report_object_button.SetActive(false);
	}

	public void ClickReportObjectButton()
	{
	}

	public void DrawReportObjectScreen()
	{
	}

	private void ReceiveDaynight(Packet incoming)
	{
	}

	public Vector3 UnpackPosition(Packet incoming)
	{
		return default(Vector3);
	}

	public Quaternion UnpackRotation(Packet incoming)
	{
		return default(Quaternion);
	}
}
