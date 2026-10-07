using System;
using System.Globalization;
using UnityEngine.SceneManagement;
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

	public static int report_find_user_max_per_page = 8;

	public List<RecentlySeenPlayer> recently_seen_players = new List<RecentlySeenPlayer>();

	public List<Trophy> trophies = new List<Trophy>();

	public List<Friend> friends = new List<Friend>();

	public List<ServerInfo> public_server_list = new List<ServerInfo>();

	public int total_unread;

	public List<string> requesting_server_icons = new List<string>();

	public Dictionary<string, Sprite> cached_server_icons = new Dictionary<string, Sprite>();

	public int give_gems_on_open;

	public byte show_warning_on_open;

	public Connection connection => FriendServerConnector.Instance.friend_server_connection;

	public FriendServerConnector connector => FriendServerConnector.Instance;

	public FriendServerSender sender => FriendServerSender.Instance;

	public FriendServerInterface interface_ => FriendServerInterface.Instance;

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
		if (interface_.curr_screen == FriendServerInterface.friend_window_screen.report_category || interface_.curr_screen == FriendServerInterface.friend_window_screen.report_find_user) return;
		int index = -1;
		for (int i = 0; i < recently_seen_players.Count; i++)
		{
			if (recently_seen_players[i].username_lower == username_lower)
			{
				index = i;
				break;
			}
		}
		if (index != -1)
		{
			RecentlySeenPlayer player = recently_seen_players[index];
			recently_seen_players.RemoveAt(index);
			recently_seen_players.Insert(0, player);
			if (!Startup.StringNullOrWhitespace(chat))
			{
				player.game_chats.Insert(0, chat);
				if (player.game_chats.Count >= 20) player.game_chats.RemoveAt(player.game_chats.Count - 1);
			}
		}
		else
		{
			if (recently_seen_players.Count >= report_find_user_max_per_page * 8) recently_seen_players.RemoveAt(recently_seen_players.Count - 1);
			RecentlySeenPlayer player = new RecentlySeenPlayer { username_lower = username_lower, username_punctuated = username_punctuated };
			if (recently_seen_players.Count == 0) recently_seen_players.Add(player);
			else recently_seen_players.Insert(0, player);
			if (!Startup.StringNullOrWhitespace(chat)) player.game_chats.Add(chat);
		}
	}

	public int NumFriendsOnline()
	{
		int count = 0;
		foreach (Friend friend in friends) if (friend.status_t == Friend.status.online) count++;
		return count;
	}

	public string FirstFriendOnline()
	{
		foreach (Friend friend in friends) if (friend.status_t == Friend.status.online) return friend.username_punctuated;
		return "???";
	}

	public Friend GetFriendByUsername(string username)
	{
		for (int i = 0; i < friends.Count; i++) if (friends[i].username_lower == username) return friends[i];
		return null;
	}

	private void UnpackWorldString(Friend friend, Packet incoming)
	{
		friend.world_string_type = (Friend.world_string_type_t)incoming.GetByte();
		friend.world_string_server_name = incoming.GetString();
		friend.world_string_n_online = incoming.GetShort();
	}

	public void ShowReceiveGems(int amount)
	{
		PopupControl.Instance.on_yes_pressed = delegate
		{
			PlayerData.Instance.SetGlobalShort("GEMS", (short)(PlayerData.Instance.GetGlobalShort("GEMS") + amount));
			Packet outgoing = new Packet();
			outgoing.PutByte(53);
			connection.Send(outgoing);
		};
		PopupControl.Instance.ShowYesNo("The Hybrid Animals Team has sent you <color=#54f1ff>" + amount + " gems</color>.\nAccept gems?", "Yes", "No", PopupControl.context.yesno_ACTION);
	}

	public void OnReceive(Packet incoming)
	{
		byte command = incoming.GetByte();
		bool in_game = SceneManager.GetActiveScene().name == "Game";
		switch ((friend_command)command)
		{
		case friend_command.bad_version:
		case friend_command.account_templocked:
			if (in_game && WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.new_friends_list && interface_.curr_screen == FriendServerInterface.friend_window_screen.connecting_screen)
			{
				interface_.ShowFailedToConnect(command == 6 ? "UPDATE AVAILABLE\n<color=#26ccff>You must update your game to play Online!</color>" : "ERROR 403\n<color=#26ccff>Something went wrong when logging in. Please try again in 10 minutes.</color>", command == 6);
			}
			connector.Disconnect();
			break;
		case friend_command.mathproblem_for_player_connect:
			MathProblem problem = new MathProblem();
			problem.Unpack(incoming);
			sender.SendMathSolution(problem.Solve());
			break;
		case friend_command.please_signal_intent_as_player:
			sender.SignalIntent((byte)(Startup.StringNullOrEmpty(PlayerData.Instance.GetGlobalString("username_lower")) ? 2 : 1));
			break;
		case friend_command.player_intent_granted:
			byte intent = incoming.GetByte();
			if (intent == 1) sender.SendAttemptLogin();
			else if (intent == 2) connector.TryGotoRegisterScreen();
			break;
		case friend_command.account_was_deleted:
			connector.TryGotoRegisterScreen();
			break;
		case friend_command.attempt_register_username:
			if (!in_game || WindowControl.Instance.curr_miniwindow != WindowControl.miniwindow_type_t.new_friends_list || interface_.curr_screen != FriendServerInterface.friend_window_screen.register_attempt)
			{
				connector.Disconnect();
				break;
			}
			byte register_result = incoming.GetByte();
			if (register_result == 1)
			{
				string lower = incoming.GetString();
				string punctuated = incoming.GetString();
				string code = incoming.GetString();
				PopupControl.Instance.ShowMessage("<color=#bbbbbb>Success!</color>\nYour username is now <color=#00ff00>" + punctuated + "</color>");
				PlayerData.Instance.SetGlobalString("username_lower", lower);
				PlayerData.Instance.SetGlobalString("username_punctuated", punctuated);
				PlayerData.Instance.SetGlobalString("rand_code", code);
				interface_.ChangeFriendScreen(FriendServerInterface.friend_window_screen.connecting_screen);
				sender.SendAttemptLogin();
			}
			else if (register_result >= 2 && register_result <= 4)
			{
				string prefix = register_result == 3 ? "<color=#ff0000>WARNING</color>\n'" : "<color=#bbbbbb>Username unavailable</color>\n'";
				string suffix = register_result == 2 ? "' is already taken. Please try a different username." : register_result == 3 ? "' contains an inappropriate word. Swearing is prohibited in this game." : "' contains letters that are not supported. Please try a different username.";
				PopupControl.Instance.ShowMessage(prefix + incoming.GetString() + suffix);
				interface_.ChangeFriendScreen(FriendServerInterface.friend_window_screen.register_screen);
			}
			break;
		case friend_command.attempt_player_login:
			byte login_result = incoming.GetByte();
			if (login_result != 1 && login_result != 2) return;
			connector.fully_logged_in = true;
			if (login_result == 1) connector.StartPinging();
			friends.Clear();
			int count = incoming.GetShort();
			for (int i = 0; i < count; i++)
			{
				string lower = incoming.GetString();
				string punctuated = incoming.GetString();
				bool online = incoming.GetByte() == 1;
				Friend friend = new Friend(lower, online ? Friend.status.online : Friend.status.offline, punctuated);
				friends.Add(friend);
				if (online) UnpackWorldString(friend, incoming);
				else if (incoming.GetByte() == 1 && DateTime.TryParseExact(incoming.GetString(), "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime date)) friend.set_last_online(date);
			}
			count = incoming.GetShort();
			for (int i = 0; i < count; i++) friends.Add(new Friend(incoming.GetString(), Friend.status.req_sent, incoming.GetString()));
			count = incoming.GetShort();
			for (int i = 0; i < count; i++) friends.Add(new Friend(incoming.GetString(), Friend.status.req_received, incoming.GetString()));
			count = incoming.GetShort();
			if (count > 0)
			{
				List<ToPing> pings = new List<ToPing>();
				for (int i = 0; i < count; i++) pings.Add(new ToPing(incoming.GetString(), incoming.GetString(), incoming.GetShort()));
				PingController.Instance.PingMany(pings, delegate { }, this);
			}
			give_gems_on_open = incoming.GetShort();
			show_warning_on_open = incoming.GetByte();
			int incorrect = incoming.GetShort();
			trophies.Clear();
			count = incoming.GetShort();
			for (int i = 0; i < count; i++) trophies.Add(new Trophy(incoming.GetString(), incoming.GetString(), incoming.GetString(), incoming.GetString(), incoming.GetString(), incoming.GetString()));
			Debug.Log("Login Succeed! (" + incorrect + " incorrect logins)");
			connector.CheckIfAutoLoginShouldBeDisabled();
			PlayerData.Instance.SetGlobalShort("n_wasted_autologins", (short)(PlayerData.Instance.GetGlobalShort("n_wasted_autologins") + 1));
			if (in_game)
			{
				if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.new_friends_list) interface_.ChangeFriendScreen(FriendServerInterface.friend_window_screen.friend_list);
				sender.UpdateWorldString();
			}
			break;
		case friend_command.ping:
			connector.last_server_ping = DateTime.UtcNow;
			break;
		case friend_command.try_add_friend:
			sender.EndTimeout();
			byte add_result = incoming.GetByte();
			string add_name = incoming.GetString();
			if (add_result == 0)
			{
				string lower = incoming.GetString();
				PopupControl.Instance.ShowMessage("Friend request sent to <color=#4dccff>" + add_name + "!</color>");
				friends.Add(new Friend(lower, Friend.status.req_sent, add_name));
				connector.CheckIfAutoLoginShouldBeDisabled();
				if (in_game && WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.new_friends_list) interface_.ChangeFriendScreen(FriendServerInterface.friend_window_screen.friend_list);
			}
			else
			{
				string message = "";
				switch (add_result)
				{
					case 1: message = "<color=#bbbbbb>Username invalid</color>\n'" + add_name + "' does not exist."; break;
					case 2: message = "<color=#bbbbbb>Cannot Add</color>\n'" + add_name + "' is already your friend!"; break;
					case 3: message = "<color=#bbbbbb>Cannot Add</color>\nYou already sent a request to'" + add_name + "'"; break;
					case 4: message = "<color=#bbbbbb>Cannot Add</color>\n'" + add_name + "' already sent you a request"; break;
					case 5: message = "<color=#bbbbbb>Too many friends</color>\nYou cannot have more than 30 friends at a time (our servers can't handle it!)"; break;
					case 6: message = "<color=#bbbbbb>Too many friends</color>\n'" + add_name + "' already has 30 friends, which is the maximum!"; break;
					case 7: message = "<color=#bbbbbb>Why</color>"; break;
				}
				PopupControl.Instance.ShowMessage(message);
				if (in_game && WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.new_friends_list) interface_.ChangeFriendScreen(FriendServerInterface.friend_window_screen.add_friend);
			}
			break;
		case friend_command.notif_someone_added_you:
			Friend added = new Friend(incoming.GetString(), Friend.status.req_received, incoming.GetString());
			friends.Add(added);
			added.set_last_online(DateTime.UtcNow);
			if (in_game)
			{
				GameplayGUIControl.Instance.ShowNotif("<color=#80d9ff>" + added.username_punctuated + "</color> sent you a friend request   ", interface_.icon_got_friend_req, new OnNotifClick(OnNotifClick.type.friends_list_general));
				if (interface_.curr_screen == FriendServerInterface.friend_window_screen.friend_list) interface_.RedrawFriendsList();
			}
			break;
		case friend_command.accept_friend_request:
			sender.EndTimeout();
			Friend accepted = GetFriendByUsername(incoming.GetString());
			if (incoming.GetByte() == 1)
			{
				accepted.status_t = Friend.status.online;
				UnpackWorldString(accepted, incoming);
			}
			else
			{
				accepted.status_t = Friend.status.offline;
				if (incoming.GetByte() == 1 && DateTime.TryParseExact(incoming.GetString(), "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime date)) accepted.set_last_online(date);
			}
			connector.CheckIfAutoLoginShouldBeDisabled();
			if (in_game && WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.new_friends_list) interface_.ChangeFriendScreen(FriendServerInterface.friend_window_screen.friend_list);
			break;
		case friend_command.notif_someone_accept_your_req:
			Friend other_accepted = GetFriendByUsername(incoming.GetString());
			other_accepted.username_punctuated = incoming.GetString();
			if (incoming.GetByte() != 1)
			{
				other_accepted.status_t = Friend.status.offline;
				return;
			}
			other_accepted.status_t = Friend.status.online;
			UnpackWorldString(other_accepted, incoming);
			if (in_game)
			{
				GameplayGUIControl.Instance.ShowNotif("<color=#fffd91>" + other_accepted.username_punctuated + "</color> accepted your friend request   ", interface_.icon_accepted_friend_req, new OnNotifClick(OnNotifClick.type.friends_list_general));
				if (interface_.curr_screen == FriendServerInterface.friend_window_screen.friend_list) interface_.RedrawFriendsList();
			}
			break;
		case friend_command.deny_friend_request:
		case friend_command.notif_someone_denied_your_req:
		case friend_command.remove_friend:
			if (command == 20 || command == 24) sender.EndTimeout();
			friends.RemoveAt(friends.IndexOf(GetFriendByUsername(incoming.GetString())));
			if (command != 20) connector.CheckIfAutoLoginShouldBeDisabled();
			if (in_game)
			{
				if (command == 21)
				{
					if (interface_.curr_screen == FriendServerInterface.friend_window_screen.friend_list) interface_.RedrawFriendsList();
				}
				else if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.new_friends_list) interface_.ChangeFriendScreen(FriendServerInterface.friend_window_screen.friend_list);
			}
			break;
		case friend_command.notif_friend_logged_in:
			Friend online_friend = GetFriendByUsername(incoming.GetString());
			online_friend.status_t = Friend.status.online;
			UnpackWorldString(online_friend, incoming);
			if (in_game)
			{
				GameplayGUIControl.Instance.ShowNotif("<color=#00ff00>" + online_friend.username_punctuated + "</color> is now online    ", interface_.icon_friend_login, new OnNotifClick(OnNotifClick.type.friends_list_general));
				if (interface_.curr_screen == FriendServerInterface.friend_window_screen.friend_list) interface_.RedrawFriendsList();
			}
			break;
		case friend_command.notif_friend_logged_out:
			Friend offline_friend = GetFriendByUsername(incoming.GetString());
			offline_friend.status_t = Friend.status.offline;
			offline_friend.set_last_online(DateTime.UtcNow);
			total_unread -= offline_friend.chat.n_unread;
			offline_friend.chat.n_unread = 0;
			offline_friend.chat.entries.Clear();
			if (in_game)
			{
				interface_.RedrawGlobalNotificationCounter();
				if (interface_.curr_screen == FriendServerInterface.friend_window_screen.friend_list) interface_.RedrawFriendsList();
				else if (interface_.curr_screen == FriendServerInterface.friend_window_screen.friend_chat) interface_.ChangeFriendScreen(FriendServerInterface.friend_window_screen.friend_list);
			}
			break;
		case friend_command.notif_friend_removed_you:
			string removed_name = incoming.GetString();
			Friend removed = GetFriendByUsername(removed_name);
			if (removed == null) return;
			friends.RemoveAt(friends.IndexOf(removed));
			connector.CheckIfAutoLoginShouldBeDisabled();
			if (in_game)
			{
				if (interface_.curr_screen == FriendServerInterface.friend_window_screen.friend_list) interface_.RedrawFriendsList();
				else if (interface_.curr_screen == FriendServerInterface.friend_window_screen.friend_chat && interface_.last_friend_clicked == removed_name) interface_.ChangeFriendScreen(FriendServerInterface.friend_window_screen.friend_list);
			}
			break;
		case friend_command.private_chat_message:
		case friend_command.got_invite:
		case friend_command.ask_to_join_player:
			string chat_user = incoming.GetString();
			string message_text = "";
			byte type = 0;
			string server_name = "";
			if (command == 26) message_text = incoming.GetString();
			else
			{
				type = incoming.GetByte();
				if (type == 1 || type == 2) server_name = incoming.GetString();
			}
			Friend chatting = GetFriendByUsername(chat_user);
			if (chatting == null) return;
			chat_log log;
			if (command == 26) log = new chat_log("<color=#abebff>" + chatting.username_punctuated + ":</color> " + message_text, message_text, null, false, null);
			else
			{
				bool invite = command == 40;
				if (invite) chatting.chat.DisableOldInvites();
				else chatting.chat.DisableOldJoins();
				Dictionary<string, string> data = new Dictionary<string, string>();
				data.Add("text", invite ? "JOIN" : "ALLOW");
				data.Add("action", invite ? "accept_invite" : "accept_other_join");
				data.Add("type", type.ToString());
				if (type == 1 || type == 2) data.Add("server_name", server_name);
				data.Add("username", chat_user);
				log = new chat_log((invite ? "<color=#21bcff>" : "<color=#30ff8d>") + chatting.username_punctuated + (invite ? " invited you to play!</color>" : " wishes to join your game</color>"), invite ? "[they invited me]" : "[they want to join me]", invite ? interface_.icon_invite : interface_.icon_want_to_join, false, data);
			}
			chatting.chat.AddLog(log);
			if (in_game) interface_.FriendChatReceived(chatting, log);
			else
			{
				chatting.chat.n_unread++;
				total_unread++;
			}
			break;
		case friend_command.notif_server_launched:
			if (in_game)
			{
				byte launched = incoming.GetByte();
				if (launched == 1) PopupControl.Instance.ShowConnecting("Authenticating");
				else if (launched == 0) PopupControl.Instance.ShowConnecting("Setting up private server");
			}
			break;
		case friend_command.request_public_server_list:
			sender.EndTimeout();
			List<ServerInfo> servers = new List<ServerInfo>();
			int server_count = incoming.GetByte();
			for (int i = 0; i < server_count; i++) servers.Add(new ServerInfo(incoming.GetString(), incoming.GetString(), incoming.GetString(), incoming.GetString(), incoming.GetString(), incoming.GetShort(), incoming.GetShort(), incoming.GetString()));
			public_server_list.Clear();
			foreach (ServerInfo server in servers)
			{
				int index = -1;
				for (int i = 0; i < public_server_list.Count; i++)
				{
					if (public_server_list[i].n_online <= server.n_online) { index = i; break; }
				}
				if (index == -1) public_server_list.Add(server);
				else public_server_list.Insert(index, server);
			}
			if (in_game && interface_.curr_screen == FriendServerInterface.friend_window_screen.connecting_screen) interface_.ChangeFriendScreen(FriendServerInterface.friend_window_screen.public_server_list);
			break;
		case friend_command.player_clicked_join_public_server:
			sender.EndTimeout();
			byte join_result = incoming.GetByte();
			if (join_result != 0 && join_result != 1) return;
			PopupControl.Instance.ShowMessage(join_result == 1 ? "Could not join server\nServer full!" : "Could not join server\nServer no longer exists!");
			if (in_game && interface_.curr_screen == FriendServerInterface.friend_window_screen.connecting_screen) interface_.ChangeFriendScreen(FriendServerInterface.friend_window_screen.public_server_list);
			break;
		case friend_command.request_public_server_icon:
			string icon_name = incoming.GetString();
			bool has_icon = incoming.GetByte() == 1;
			byte[] bytes = null;
			if (has_icon)
			{
				bytes = new byte[incoming.GetShort()];
				for (int i = 0; i < bytes.Length; i++) bytes[i] = incoming.GetByte();
			}
			requesting_server_icons.Remove(icon_name);
			Sprite icon = null;
			if (has_icon)
			{
				Texture2D texture = new Texture2D(32, 32);
				texture.LoadImage(bytes);
				icon = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
			}
			cached_server_icons.Add(icon_name, icon);
			if (in_game) interface_.GotServerIcon(icon_name);
			break;
		case friend_command.please_ping_dispatchers:
			if (in_game)
			{
				byte joining = incoming.GetByte();
				WindowControl.Instance.CloseAllWindows();
				PopupControl.Instance.ShowConnecting(joining == 1 ? "Player is joining" : "Joining game");
				int ping_count = incoming.GetShort();
				if (ping_count < 1) return;
				List<ToPing> pings = new List<ToPing>();
				for (int i = 0; i < ping_count; i++) pings.Add(new ToPing(incoming.GetString(), incoming.GetString(), incoming.GetShort()));
				PingController.Instance.PingMany(pings, sender.SendDispatcherPingResult, this);
			}
			break;
		case friend_command.private_server_setup_failure:
			if (in_game) PopupControl.Instance.ShowMessage("Couldn't set up match\nThe servers are down for maintenance. Sorry!");
			break;
		case friend_command.server_join_info:
			if (!in_game) { sender.EndTimeout(); return; }
			string name = incoming.GetString();
			string join_code = incoming.GetString();
			string ip = incoming.GetString();
			string ip_type = incoming.GetString();
			int port = incoming.GetShort();
			incoming.GetByte();
			string owner = name.Replace("(private)", "");
			PopupControl.Instance.ShowConnecting(PlayerData.Instance.GetGlobalString("username_punctuated") == owner ? "Connecting to private server" : "Connecting to " + owner + "'s World");
			GameServerConnector.Instance.ConnectToGameServer(ip, ip_type, port, join_code);
			break;
		case friend_command.invite_friend:
			sender.EndTimeout();
			if (in_game)
			{
				PopupControl.Instance.ShowMessage("Invite sent!");
				if (interface_.curr_screen == FriendServerInterface.friend_window_screen.connecting_screen) interface_.ChangeFriendScreen(FriendServerInterface.friend_window_screen.friend_list);
			}
			break;
		case friend_command.accept_invite:
			string invite_user = incoming.GetString();
			if (in_game) sender.SendYouMayJoinMyWorldNow(invite_user, 0, "");
			else sender.SendAcceptInviteFailed(invite_user, 0);
			break;
		case friend_command.you_cannot_join_my_world_any_more:
			string failed_user = incoming.GetString();
			byte failure = incoming.GetByte();
			if (failure == 0 || failure == 1) PopupControl.Instance.ShowMessage("Could not join!\n" + failed_user + (failure == 1 ? " is no longer online" : " went to the Main Menu"));
			break;
		case friend_command.you_may_join_my_world_now:
			if (in_game) PopupControl.Instance.HideAll();
			break;
		case friend_command.update_world_string:
			string world_user = incoming.GetString();
			Friend world_friend = GetFriendByUsername(world_user);
			if (world_friend == null) return;
			UnpackWorldString(world_friend, incoming);
			if (in_game)
			{
				if (interface_.curr_screen == FriendServerInterface.friend_window_screen.friend_list) interface_.RedrawFriendsList();
				else if (interface_.curr_screen == FriendServerInterface.friend_window_screen.friend_chat && interface_.last_friend_clicked == world_user) interface_.RedrawChat(world_friend.chat);
			}
			break;
		case friend_command.submit_report:
			PopupControl.Instance.ShowMessage("Report submitted!\nWe will review your report as soon as possible. Thanks for reporting");
			break;
		case friend_command.warning:
			byte warning = incoming.GetByte();
			if (warning != 0) ShowWarning(warning);
			break;
		case friend_command.receive_gems:
			ShowReceiveGems(incoming.GetShort());
			break;
		case friend_command.receive_trophy:
			trophies.Add(new Trophy(incoming.GetString(), incoming.GetString(), incoming.GetString(), incoming.GetString(), incoming.GetString(), incoming.GetString()));
			if (in_game)
			{
				if (interface_.curr_screen == FriendServerInterface.friend_window_screen.friend_list) interface_.RedrawFriendsList();
				interface_.ShowNewGiftsNotif();
			}
			break;
		}
	}

	public void ShowWarning(byte warning_type)
	{
		string reason = "";
		switch (warning_type)
		{
			case 2: reason = "Harrassment"; break;
			case 3: reason = "Asking for or giving out personal information"; break;
			case 4: reason = "Swearing or Inappropriate Speech"; break;
			case 5: reason = "Sharing Websites or Promoting Products"; break;
			case 6: reason = "Unacceptable Behaviour"; break;
			case 7: reason = "Placing Inappropriate Objects"; break;
		}
		PopupControl.Instance.ShowMessage("<color=#ff0000>WARNING</color>\nYou have been reported for <color=#ffbe26>" + reason + "</color>.\n\nThis is your only warning. If you do not stop,\n<color=#ff0000>YOU WILL BE PERMANENTLY BANNED</color>");
	}
}
