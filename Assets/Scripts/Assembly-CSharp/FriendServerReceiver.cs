using System.Collections.Generic;
using UnityEngine;

public class FriendServerReceiver : MonoBehaviour, OrderedStart
{
	public enum friend_command
	{
		none = 0,
		want_to_connect_as_player = 1,
		want_to_connect_as_selfConnector = 2,
		want_to_connect_as_clusterDispatcher = 3,
		want_to_connect_as_gameServerCluster = 4,
		want_to_connect_as_adminDashboard = 5,
		bad_version = 6,
		mathproblem_for_player_connect = 7,
		please_signal_intent_as_player = 8,
		player_intent_granted = 9,
		attempt_register_username = 10,
		attempt_player_login = 11,
		account_was_deleted = 12,
		login_succeed_gameDispatcher = 13,
		login_succeed_gameServerCluster = 14,
		ping = 15,
		try_add_friend = 16,
		notif_someone_added_you = 17,
		accept_friend_request = 18,
		notif_someone_accept_your_req = 19,
		deny_friend_request = 20,
		notif_someone_denied_your_req = 21,
		notif_friend_logged_in = 22,
		notif_friend_logged_out = 23,
		remove_friend = 24,
		notif_friend_removed_you = 25,
		private_chat_message = 26,
		notif_server_launched = 27,
		notif_server_lost = 28,
		request_public_server_list = 29,
		player_clicked_join_public_server = 30,
		request_public_server_icon = 31,
		please_ping_dispatchers = 32,
		dispatcher_ping = 33,
		launch_new_private_server = 34,
		private_server_setup_failure = 35,
		player_intends_to_join_server = 36,
		server_join_info = 37,
		player_try_login_server = 38,
		invite_friend = 39,
		got_invite = 40,
		accept_invite = 41,
		you_cannot_join_my_world_any_more = 42,
		you_may_join_my_world_now = 43,
		update_world_string = 44,
		ask_to_join_player = 45,
		submit_report = 46,
		warning = 47,
		kick_player = 48,
		mute_player = 49,
		admin_delete_obj = 50,
		receive_gems = 52,
		accept_gems = 53,
		request_server_stat = 54,
		receive_trophy = 55,
		accept_trophy = 56,
		auto_mute_player_48h = 57,
		rename_player = 58,
		create_new_world = 59,
		new_world_created = 60,
		account_templocked = 62
	}

	public static FriendServerReceiver Instance;

	public static int report_find_user_max_per_page;

	public List<RecentlySeenPlayer> recently_seen_players;

	public List<Trophy> trophies;

	public List<Friend> friends;

	public List<ServerInfo> public_server_list;

	public int total_unread;

	public List<string> requesting_server_icons;

	public Dictionary<string, Sprite> cached_server_icons;

	public int give_gems_on_open;

	public byte show_warning_on_open;

	public Connection connection => null;

	public FriendServerConnector connector => null;

	public FriendServerSender sender => null;

	public FriendServerInterface interface_ => null;

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

	public void AddToRecentlySeenPlayers(string username_lower, string username_punctuated, string chat)
	{
	}

	public int NumFriendsOnline()
	{
		return 0;
	}

	public string FirstFriendOnline()
	{
		return null;
	}

	public Friend GetFriendByUsername(string username)
	{
		return null;
	}

	private void UnpackWorldString(Friend friend, Packet incoming)
	{
	}

	public void ShowReceiveGems(int amount)
	{
	}

	public void OnReceive(Packet incoming)
	{
	}

	public void ShowWarning(byte warning_type)
	{
	}
}
