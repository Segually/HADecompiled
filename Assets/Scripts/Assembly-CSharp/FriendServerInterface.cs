using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FriendServerInterface : MonoBehaviour, OrderedStart
{
	public enum friend_window_screen
	{
		none = 0,
		connecting_screen = 1,
		register_screen = 2,
		register_attempt = 3,
		friend_list = 4,
		add_friend = 5,
		friend_chat = 6,
		public_server_list = 7,
		public_server_join = 8,
		report_category = 9,
		report_find_user = 10,
		report_finalize = 11,
		report_doubleCheck_user = 12,
		report_doubleCheck_user_self = 13,
		gifts = 14
	}

	public static FriendServerInterface Instance;

	public GameObject chat_log_prefab;

	public Sprite icon_got_friend_req;

	public Sprite icon_accepted_friend_req;

	public Sprite icon_friend_login;

	public Sprite icon_invite;

	public Sprite icon_want_to_join;

	public GameObject scaled_parent;

	public Canvas canvas;

	private List<GameObject> chat_log_objects = new List<GameObject>();

	public Dictionary<string, string> report_data = new Dictionary<string, string>();

	public friend_window_screen curr_screen;

	private int report_list_tab;

	private int report_find_user_page;

	private List<GameObject> instantiated_gifts_nibs = new List<GameObject>();

	private List<GameObject> instantiated_friends_nibs = new List<GameObject>();

	private bool hover_visible;

	private GameObject friend_nib_selected;

	private float friend_nib_selected_prev_text_alpha;

	private float friend_nib_selected_prev_bg_alpha;

	private Color friend_nib_selected_prev_bg_col;

	public string last_friend_clicked = "";

	private DateTime last_pub_server_request;

	public GameObject gift_nib_prefab;

	public GameObject friend_nib_prefab;

	public int curr_pub_server_page;

	public GameObject global_notifications_obj;

	private ServerInfo server_join_viewing;

	public Sprite default_server_icon;

	public Connection connection => FriendServerConnector.Instance.friend_server_connection;

	public FriendServerConnector connector => FriendServerConnector.Instance;

	public FriendServerSender sender => FriendServerSender.Instance;

	public FriendServerReceiver receiver => FriendServerReceiver.Instance;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
		last_pub_server_request = DateTime.UtcNow.AddHours(-1.0);
		RedrawGlobalNotificationCounter();
		if (connection.GetStatus() == Connection.connection_status.connected && connector.fully_logged_in) sender.UpdateWorldString();
	}

	public void PressYes13()
	{
		PlayerData.Instance.SetGlobalShort("13_plus", 1);
		ChangeFriendScreen(friend_window_screen.register_screen);
	}

	public void PressNo13()
	{
		PopupControl.Instance.ShowMessage("You must be 13 or older to play in Online Mode");
	}

	public void ShowNumFriendsOnline()
	{
		if (connection.GetStatus() != Connection.connection_status.connected) return;
		int count = receiver.NumFriendsOnline();
		if (count == 0) return;
		string message = count == 1 ? receiver.FirstFriendOnline() + "</color> is online    " : count + (count < 10 ? "</color> friends online  " : "</color> friends online   ");
		GameplayGUIControl.Instance.ShowNotif("<color=#00ff00>" + message, Instance.icon_friend_login, new OnNotifClick(OnNotifClick.type.friends_list_general));
	}

	public void ShowNewGiftsNotif()
	{
		if (connection.GetStatus() != Connection.connection_status.connected || receiver.trophies.Count < 1) return;
		GameplayGUIControl.Instance.ShowNotif(receiver.trophies.Count == 1 ? "<i><color=#ffeb3b>You have a new gift</color></i>" : "<i><color=#ffeb3b>You have new gifts</color></i>", null, new OnNotifClick(OnNotifClick.type.new_gift));
	}

	public void PressedFriendsButton()
	{
		PlayerData.Instance.SetGlobalShort("n_wasted_autologins", 0);
		Connection.connection_status status = connection.GetStatus();
		if (status == Connection.connection_status.not_connected) connector.ConnectToFriendServer();
		else if (status != Connection.connection_status.connecting && status != Connection.connection_status.connected) return;
		OpenFriendScreen(status == Connection.connection_status.connected && connector.fully_logged_in && !sender.WaitingOnRequest() ? friend_window_screen.friend_list : friend_window_screen.connecting_screen);
	}

	public void SetChatMaxScroll(ChatCollection chat)
	{
		WindowPrefabsControl.Instance.GetScreen("CHAT").GetComponent<Scrollable>().SetScrollAreaMaxY(chat.entries.Count <= 3 ? 70f : ChatCollection.chatlogspacing * (chat.entries.Count - 3));
	}

	public void CreateChatLogObj(int i, chat_log log, bool window_already_open)
	{
		GameObject obj = Instantiate(chat_log_prefab);
		obj.transform.SetParent(WindowPrefabsControl.Instance.GetObject("CHAT", "LogParent").transform);
		obj.transform.localScale = Vector3.one;
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localPosition = new Vector3(0f, 255f - ChatCollection.chatlogspacing * i, 0f);
		if (window_already_open) obj.transform.SetAsFirstSibling();
		else obj.transform.SetAsLastSibling();
		if (log.img != null && log.accept_or_deny_data == null)
		{
			obj.transform.Find("img").GetComponent<Image>().sprite = log.img;
			if (!log.has_bg) Destroy(obj.transform.Find("bg").gameObject);
		}
		else
		{
			Destroy(obj.transform.Find("img").gameObject);
			Destroy(obj.transform.Find("bg").gameObject);
		}
		if (log.accept_or_deny_data == null) Destroy(obj.transform.Find("accept").gameObject);
		else
		{
			Dictionary<string, string> data = log.accept_or_deny_data;
			byte type = byte.Parse(data["type"], Startup.parse_culture);
			string server = type == 1 || type == 2 ? data["server_name"] : "";
			Friend friend = receiver.GetFriendByUsername(data["username"]);
			bool valid = false;
			if (friend != null)
			{
				if (data["action"] == "accept_invite")
				{
					if (type == 0) valid = friend.world_string_type == Friend.world_string_type_t.in_self_personal_alone || friend.world_string_type == Friend.world_string_type_t.in_self_personal_w_others;
					else if (type == 1) valid = friend.world_string_type == Friend.world_string_type_t.in_other_personal && friend.world_string_server_name.Replace("(private)", "") == server;
					else if (type == 2) valid = friend.world_string_type == Friend.world_string_type_t.in_pub && friend.world_string_server_name == server;
				}
				else if (data["action"] == "accept_other_join")
				{
					if (type == 0) valid = GameServerConnector.Instance.FullyInGame() ? GameServerConnector.Instance.is_host : !GameServerConnector.Instance.MidConnect();
					else if (type == 1) valid = GameServerConnector.Instance.FullyInGame() && !GameServerConnector.Instance.is_host && GameServerConnector.Instance.server_name.Replace("(private)", "") == server;
				}
				if (GameServerConnector.Instance.FullyInGame() && GameServerConnector.Instance.server_name == friend.world_string_server_name) valid = false;
			}
			if (valid) obj.transform.Find("accept").Find("Text").GetComponent<Text>().text = data["text"];
			else Destroy(obj.transform.Find("accept").gameObject);
		}
		Text text = obj.transform.Find("Text").GetComponent<Text>();
		text.text = log.text;
		RectTransform rect = (RectTransform)obj.transform;
		rect.sizeDelta = new Vector2(Mathf.Max(100f, text.preferredWidth + 80f), rect.sizeDelta.y);
		obj.GetComponent<ChatLogObject>().corresponding_chat_log = i;
		chat_log_objects.Add(obj);
	}

	public void PressChatLogButton(int id)
	{
		if (curr_screen != friend_window_screen.friend_chat) return;
		chat_log log = receiver.GetFriendByUsername(last_friend_clicked).chat.entries[id];
		if (log.accept_or_deny_data == null) return;
		Dictionary<string, string> data = log.accept_or_deny_data;
		string action = data["action"];
		byte type = byte.Parse(data["type"], Startup.parse_culture);
		string username = data["username"];
		string server = type == 1 || type == 2 ? data["server_name"] : "";
		if (action == "accept_invite")
		{
			if (type == 0)
			{
				sender.SendAcceptInvite(username);
				PopupControl.Instance.ShowConnecting("Accepting invite");
			}
			else if (type == 1)
			{
				Friend host = receiver.GetFriendByUsername(server.ToLower());
				PopupControl.Instance.ShowMessage(host == null ? "Cannot join\nYou are not friends with <color=#76e8bb>" + server + "</color>.\n\n<color=#bbbbbb>(Accepting invites to join friends-of-friends will work in the future! It's not coded yet)</color>" : "Please ask <color=#76e8bb>" + server + "</color> directly for an invite.\n\n<color=#bbbbbb>(Accepting invites to join friends-of-friends will work in the future! It's not coded yet)</color>");
			}
			else if (type == 2)
			{
				sender.TryJoinPublicServer(server);
				PopupControl.Instance.ShowConnecting("Accepting invite");
			}
			WindowControl.Instance.CloseMiniwindow(true);
			receiver.GetFriendByUsername(last_friend_clicked).chat.DisableOldInvites();
		}
		else if (action == "accept_other_join")
		{
			if (type == 0)
			{
				sender.SendYouMayJoinMyWorldNow(username, 0, "");
				PopupControl.Instance.ShowConnecting("Allowing friend to join");
			}
			WindowControl.Instance.CloseMiniwindow(true);
			receiver.GetFriendByUsername(last_friend_clicked).chat.DisableOldJoins();
		}
	}

	public void FriendChatReceived(Friend friend, chat_log new_log)
	{
		if (curr_screen == friend_window_screen.friend_chat && last_friend_clicked == friend.username_lower)
		{
			RedrawChat(friend.chat);
			return;
		}
		friend.chat.n_unread++;
		receiver.total_unread++;
		RedrawGlobalNotificationCounter();
		if (curr_screen == friend_window_screen.friend_list)
		{
			RedrawFriendsList();
			return;
		}
		OnNotifClick click = new OnNotifClick(OnNotifClick.type.certain_friend);
		click.data.Add("friend_username_lower", friend.username_lower);
		GameplayGUIControl.Instance.ShowNotif("<i><color=#fffc66>" + new_log.text + "</color></i>", new_log.img, click);
	}

	public void RedrawGlobalNotificationCounter()
	{
		if (receiver.total_unread < 1)
		{
			global_notifications_obj.SetActive(false);
			return;
		}
		global_notifications_obj.SetActive(true);
		global_notifications_obj.transform.Find("Text").GetComponent<Text>().text = receiver.total_unread.ToString();
	}

	public void OpenFriendScreen(friend_window_screen new_screen)
	{
		WindowControl.Instance.OpenMiniwindow(WindowControl.miniwindow_type_t.new_friends_list);
		ChangeFriendScreen(new_screen);
	}

	public void DirectOpenFriend(string friend_username_lower)
	{
		WindowControl.Instance.OpenMiniwindow(WindowControl.miniwindow_type_t.new_friends_list);
		last_friend_clicked = friend_username_lower;
		OpenFriendChat();
	}

	public void PressGiftNotif()
	{
		WindowControl.Instance.OpenMiniwindow(WindowControl.miniwindow_type_t.new_friends_list);
		ChangeFriendScreen(friend_window_screen.gifts);
	}

	private void OpenFriendChat()
	{
		Friend friend = receiver.GetFriendByUsername(last_friend_clicked);
		if (friend == null || friend.status_t != Friend.status.online) return;
		ChangeFriendScreen(friend_window_screen.friend_chat);
		RedrawChat(friend.chat);
		receiver.total_unread -= friend.chat.n_unread;
		friend.chat.n_unread = 0;
		RedrawGlobalNotificationCounter();
	}

	public void PressReport()
	{
		ChangeFriendScreen(friend_window_screen.report_category);
	}

	public void PressAddFriend()
	{
		ChangeFriendScreen(friend_window_screen.add_friend);
	}

	public void PressCancelAddFriend()
	{
		ChangeFriendScreen(friend_window_screen.friend_list);
	}

	public void PressAcceptAddFriend()
	{
		string username = WindowPrefabsControl.Instance.GetTextLegacy("FRIENDS-add_friend", "username-text").text;
		if (UsernameInvalid(username)) return;
		FriendServerSender.ChangeConnectingText("Adding friend");
		ChangeFriendScreen(friend_window_screen.connecting_screen);
		sender.TryAddFriend(username);
	}

	public void PressReportCategory(int index)
	{
		switch (index)
		{
			case 0: report_data.Add("category", "hacking"); break;
			case 1: report_data.Add("category", "swearing or inappropriate"); break;
			case 2: report_data.Add("category", "personal info"); break;
			case 3: report_data.Add("category", "bad username"); break;
			case 4: report_data.Add("category", "websites or products"); break;
			case 5: report_data.Add("category", "harassment"); break;
			case 6: report_data.Add("category", "other"); break;
		}
		ChangeFriendScreen(friend_window_screen.report_find_user);
	}

	public void PressReportDoubleCheckCategory(int index)
	{
		switch (index)
		{
			case 0: report_data.Add("doubleCheck_category", "hacking"); break;
			case 1: report_data.Add("doubleCheck_category", "swearing or inappropriate"); break;
			case 2: report_data.Add("doubleCheck_category", "personal info"); break;
			case 3: report_data.Add("doubleCheck_category", "bad username"); break;
			case 4: report_data.Add("doubleCheck_category", "websites or products"); break;
			case 5: report_data.Add("doubleCheck_category", "harassment"); break;
			case 6: report_data.Add("doubleCheck_category", "other"); break;
		}
		ChangeFriendScreen(friend_window_screen.report_doubleCheck_user_self);
	}

	public void PressUserReportSend()
	{
		report_data.Add("doubleCheck_own_username", WindowPrefabsControl.Instance.GetObject("FRIENDS-report-doubleCheck-user-self", "username_input").GetComponent<InputField>().text);
		sender.SendSubmitReport();
		PopupControl.Instance.ShowConnecting("Submitting report");
		WindowControl.Instance.CloseMiniwindow(true);
	}

	public void ChangeFriendScreen(friend_window_screen new_screen)
	{
		WindowPrefabsControl windows = WindowPrefabsControl.Instance;
		WindowControl window = WindowControl.Instance;
		string[] screens = { null, "FRIENDS-loading", "FRIENDS-register", "FRIENDS-loading", "FRIENDS-friends_list", "FRIENDS-add_friend", "CHAT", "FRIENDS-pub_serv_list", "FRIENDS-join_pub_serv", "FRIENDS-report-category", "FRIENDS-report-find-user", "FRIENDS-report-finalize_player", "FRIENDS-report-doubleCheck-user", "FRIENDS-report-doubleCheck-user-self", "FRIENDS-gifts" };
		if (curr_screen != friend_window_screen.none) windows.DestroyScreen(screens[(int)curr_screen]);
		if (curr_screen == friend_window_screen.register_screen) windows.DestroyScreen("FRIENDS-13 plus");
		if (curr_screen == friend_window_screen.friend_list) hover_visible = false;
		switch (new_screen)
		{
			case friend_window_screen.connecting_screen:
			case friend_window_screen.register_attempt:
				window.HideMiniwindowHeaders();
				windows.CreateScreen("FRIENDS-loading", WindowPrefabsControl.build_into_t.mini_window);
				windows.GetTextLegacy("FRIENDS-loading", "ConnectText").text = FriendServerSender.connecting_string;
				break;
			case friend_window_screen.register_screen:
				window.HideMiniwindowHeaders();
				windows.CreateScreen(PlayerData.Instance.GetGlobalShort("13_plus") == 0 ? "FRIENDS-13 plus" : "FRIENDS-register", WindowPrefabsControl.build_into_t.mini_window);
				break;
			case friend_window_screen.friend_list:
				window.HideMiniwindowHeaders();
				windows.CreateScreen("FRIENDS-friends_list", WindowPrefabsControl.build_into_t.mini_window);
				windows.GetTextMeshPro("FRIENDS-friends_list", "header").text = TranslationControl.Instance.TranslateGeneral("FRIEND LIST", "GUI");
				windows.GetTextLegacy("FRIENDS-friends_list", "add").text = TranslationControl.Instance.TranslateGeneral("ADD FRIEND", "GUI");
				windows.GetTextLegacy("FRIENDS-friends_list", "have no friends").text = TranslationControl.Instance.TranslateGeneral("Add your friends to play with them online!", "GUI");
				window.miniwindow_header_L.text = "FRIENDS";
				window.miniwindow_header_R.text = "PUBLIC SERVERS";
				window.VisuallySelectLeftMiniwindowTab();
				RedrawFriendsList();
				if (receiver.give_gems_on_open != 0)
				{
					receiver.ShowReceiveGems(receiver.give_gems_on_open);
					receiver.give_gems_on_open = 0;
				}
				else if (receiver.show_warning_on_open != 0)
				{
					receiver.ShowWarning(receiver.show_warning_on_open);
					receiver.show_warning_on_open = 0;
				}
				break;
			case friend_window_screen.add_friend:
			case friend_window_screen.friend_chat:
				window.HideMiniwindowHeaders();
				windows.CreateScreen(screens[(int)new_screen], WindowPrefabsControl.build_into_t.mini_window);
				break;
			case friend_window_screen.public_server_list:
				window.ShowMiniwindowHeaders();
				window.miniwindow_header_L.text = "FRIENDS";
				window.miniwindow_header_R.text = "PUBLIC SERVERS";
				window.VisuallySelectRightMiniwindowTab();
				windows.CreateScreen("FRIENDS-pub_serv_list", WindowPrefabsControl.build_into_t.mini_window);
				RedrawPublicServerList();
				break;
			case friend_window_screen.public_server_join:
				window.HideMiniwindowHeaders();
				windows.CreateScreen("FRIENDS-join_pub_serv", WindowPrefabsControl.build_into_t.mini_window);
				RedrawJoinPubServScreen();
				break;
			case friend_window_screen.report_category:
				window.ColorizeMiniwindow("companions");
				window.HideMiniwindowHeaders();
				windows.CreateScreen(screens[(int)new_screen], WindowPrefabsControl.build_into_t.mini_window);
				report_data.Clear();
				break;
			case friend_window_screen.report_find_user:
				windows.CreateScreen(screens[(int)new_screen], WindowPrefabsControl.build_into_t.mini_window);
				report_list_tab = 0;
				report_find_user_page = 0;
				RedrawRecentUsersList();
				windows.GetTextLegacy("FRIENDS-report-find-user", "header").text = report_data.ContainsKey("category") && report_data["category"] == "other" ? "USER TO REPORT (OPTIONAL)" : "USER TO REPORT";
				break;
			case friend_window_screen.report_finalize:
				windows.CreateScreen(screens[(int)new_screen], WindowPrefabsControl.build_into_t.mini_window);
				InputField reason = windows.GetObject("FRIENDS-report-finalize_player", "reason_input").GetComponent<InputField>();
				InputField user = windows.GetObject("FRIENDS-report-finalize_player", "user_input").GetComponent<InputField>();
				string text = "???";
				if (report_data.ContainsKey("category"))
				{
					switch (report_data["category"])
					{
						case "hacking": text = "Hacking"; break;
						case "swearing or inappropriate": text = "Swearing or inappropriate topics"; break;
						case "personal info": text = "Asking for or sharing person information"; break;
						case "bad username": text = "Inappropriate Username"; break;
						case "websites or products": text = "Sharing websites or promoting products"; break;
						case "harassment": text = "Harassment"; break;
						case "other":
							text = "";
							reason.interactable = true;
							if (!report_data.ContainsKey("report_username_custom") && !report_data.ContainsKey("report_username_lower")) user.interactable = true;
							break;
					}
				}
				reason.SetTextWithoutNotify(text);
				user.SetTextWithoutNotify(report_data.ContainsKey("report_username_custom") ? report_data["report_username_custom"] : report_data.ContainsKey("report_username_lower") ? report_data["report_username_lower"] : "");
				break;
			case friend_window_screen.report_doubleCheck_user:
			case friend_window_screen.report_doubleCheck_user_self:
				windows.CreateScreen(screens[(int)new_screen], WindowPrefabsControl.build_into_t.mini_window);
				break;
			case friend_window_screen.gifts:
				window.HideMiniwindowHeaders();
				windows.CreateScreen("FRIENDS-gifts", WindowPrefabsControl.build_into_t.mini_window);
				RedrawGiftsScreen();
				break;
		}
		curr_screen = new_screen;
	}

	public void PressGifts()
	{
		ChangeFriendScreen(friend_window_screen.gifts);
	}

	public void PressSubmitReportPlayer()
	{
		string notes = WindowPrefabsControl.Instance.GetObject("FRIENDS-report-finalize_player", "additional_input").GetComponent<InputField>().text;
		if (Startup.StringNullOrWhitespace(notes))
		{
			PopupControl.Instance.ShowMessage("You must enter a description!");
			return;
		}
		report_data.Add("additional_notes", notes);
		ChangeFriendScreen(friend_window_screen.report_doubleCheck_user);
	}

	public void PressNextOnReportUsername()
	{
		if (!report_data.ContainsKey("category")) return;
		if (report_data["category"] != "other" && !report_data.ContainsKey("report_username_lower") && !report_data.ContainsKey("report_username_custom"))
		{
			PopupControl.Instance.ShowMessage("You must enter a username!");
			return;
		}
		ChangeFriendScreen(friend_window_screen.report_finalize);
	}

	public void PressReportListTab(int tab)
	{
		report_list_tab = tab;
		report_find_user_page = 0;
		RedrawRecentUsersList();
	}

	public void PressRecentNib(int i)
	{
		int index = i + report_find_user_page * 8;
		string lower = null;
		string punctuated = null;
		if (report_list_tab == 1 && index < receiver.friends.Count)
		{
			lower = receiver.friends[index].username_lower;
			punctuated = receiver.friends[index].username_punctuated;
		}
		else if (report_list_tab == 0 && index < receiver.recently_seen_players.Count)
		{
			lower = receiver.recently_seen_players[index].username_lower;
			punctuated = receiver.recently_seen_players[index].username_punctuated;
		}
		if (lower != null)
		{
			WindowPrefabsControl.Instance.GetObject("FRIENDS-report-find-user", "input").GetComponent<InputField>().SetTextWithoutNotify(punctuated);
			if (report_data.ContainsKey("report_username_lower")) report_data["report_username_lower"] = lower;
			else report_data.Add("report_username_lower", lower);
		}
		if (report_data.ContainsKey("report_username_custom")) report_data.Remove("report_username_custom");
	}

	public void OnEditReportUsernameInput()
	{
		if (report_data.ContainsKey("report_username_lower")) report_data.Remove("report_username_lower");
		string username = WindowPrefabsControl.Instance.GetObject("FRIENDS-report-find-user", "input").GetComponent<InputField>().text;
		if (report_data.ContainsKey("report_username_custom")) report_data["report_username_custom"] = username;
		else report_data.Add("report_username_custom", username);
	}

	private void RedrawRecentUsersList()
	{
		WindowPrefabsControl windows = WindowPrefabsControl.Instance;
		Color selected = new Color(0.87f, 0.85f, 0.44f, 1f);
		Color unselected = new Color(0.5f, 0.48f, 0.27f, 1f);
		windows.GetImage("FRIENDS-report-find-user", "tab_recents").color = report_list_tab == 0 ? selected : unselected;
		windows.GetImage("FRIENDS-report-find-user", "tab_friends").color = report_list_tab == 1 ? selected : unselected;
		if (report_list_tab == 0 || report_list_tab == 1)
		{
			for (int i = 0; i < 8; i++)
			{
				GameObject obj = windows.GetObject("FRIENDS-report-find-user", "user" + i);
				int index = i + report_find_user_page * 8;
				int count = report_list_tab == 1 ? receiver.friends.Count : receiver.recently_seen_players.Count;
				obj.SetActive(index < count);
				if (index < count) obj.transform.Find("Text").GetComponent<Text>().text = report_list_tab == 1 ? receiver.friends[i].username_punctuated : receiver.recently_seen_players[index].username_punctuated;
			}
		}
		windows.GetObject("FRIENDS-report-find-user", "page_switcher_L").GetComponent<CanvasGroup>().alpha = report_find_user_page == 0 ? 0.25f : 1f;
		windows.GetObject("FRIENDS-report-find-user", "page_switcher_R").GetComponent<CanvasGroup>().alpha = NextFindUserPageExists() ? 1f : 0.25f;
	}

	private bool NextFindUserPageExists()
	{
		int count;
		if (report_list_tab == 1) count = receiver.friends.Count;
		else if (report_list_tab == 0) count = receiver.recently_seen_players.Count;
		else return false;
		return count > FriendServerReceiver.report_find_user_max_per_page && FriendServerReceiver.report_find_user_max_per_page * (report_find_user_page + 1) < count;
	}

	public void PressChangeRecentUserPage(int dir)
	{
		if (dir == 1 && !NextFindUserPageExists()) return;
		if (dir == -1 && report_find_user_page == 0) return;
		report_find_user_page += dir;
		RedrawRecentUsersList();
	}

	public void RedrawJoinPubServScreen()
	{
		WindowPrefabsControl windows = WindowPrefabsControl.Instance;
		Text title = windows.GetTextLegacy("FRIENDS-join_pub_serv", "server title");
		Text description = windows.GetTextLegacy("FRIENDS-join_pub_serv", "description");
		Image icon = windows.GetImage("FRIENDS-join_pub_serv", "icon");
		title.text = server_join_viewing.server_name;
		string text = "No description";
		if (server_join_viewing.server_description1 != "")
		{
			text = server_join_viewing.server_description1;
			if (server_join_viewing.server_description2 != "")
			{
				text += "\n" + server_join_viewing.server_description2;
				if (server_join_viewing.server_description3 != "")
				{
					text += "\n" + server_join_viewing.server_description3;
					if (server_join_viewing.server_description4 != "") text += "\n" + server_join_viewing.server_description4;
				}
			}
		}
		description.text = text;
		icon.sprite = receiver.cached_server_icons.ContainsKey(server_join_viewing.server_name) && receiver.cached_server_icons[server_join_viewing.server_name] != null ? receiver.cached_server_icons[server_join_viewing.server_name] : default_server_icon;
		icon.transform.parent.localPosition += Vector3.left * (title.preferredWidth * 0.5f + 80f);
	}

	public void OnCloseFriendScreen()
	{
		if (curr_screen == friend_window_screen.register_screen || curr_screen == friend_window_screen.register_attempt) connector.Disconnect();
		ChangeFriendScreen(friend_window_screen.none);
	}

	public void RedrawGiftsScreen()
	{
		foreach (GameObject obj in instantiated_gifts_nibs) Destroy(obj);
		instantiated_gifts_nibs.Clear();
		for (int i = 0; i < receiver.trophies.Count; i++) CreateGiftNib(i, receiver.trophies[i]);
		WindowPrefabsControl.Instance.GetScreen("FRIENDS-gifts").GetComponent<Scrollable>().SetScrollAreaMaxY(receiver.trophies.Count < 4 ? 50f : receiver.trophies.Count * 114f - 342f);
	}

	public void RedrawFriendsList()
	{
		WindowPrefabsControl.Instance.GetTextLegacy("FRIENDS-friends_list", "my username").text = "<color=#cccccc>Logged in as: </color>" + PlayerData.Instance.GetGlobalString("username_punctuated");
		foreach (GameObject obj in instantiated_friends_nibs) Destroy(obj);
		instantiated_friends_nibs.Clear();
		WindowPrefabsControl.Instance.GetObject("FRIENDS-friends_list", "have no friends").SetActive(receiver.friends.Count == 0);
		int index = 0;
		foreach (Friend.status status in new[] { Friend.status.req_received, Friend.status.online, Friend.status.req_sent, Friend.status.offline })
		{
			foreach (Friend friend in receiver.friends) if (friend.status_t == status) CreateFriendNib(status, index++, friend);
		}
		WindowPrefabsControl.Instance.GetScreen("FRIENDS-friends_list").GetComponent<Scrollable>().SetScrollAreaMaxY(receiver.friends.Count < 4 ? 50f : receiver.friends.Count * 114f - 342f);
		WindowPrefabsControl.Instance.GetObject("FRIENDS-friends_list", "gifts_button").SetActive(receiver.trophies.Count > 0);
	}

	private void HideHover()
	{
		hover_visible = false;
		Animation animation = WindowPrefabsControl.Instance.GetObject("FRIENDS-friends_list", "hover").GetComponent<Animation>();
		animation.Stop();
		animation.Play("hover hide");
	}

	private void DeselectCurrSelectedFriendNib()
	{
		friend_nib_selected.transform.localScale = Vector3.one;
		friend_nib_selected.transform.Find("background").GetComponent<CanvasGroup>().alpha = friend_nib_selected_prev_bg_alpha;
		friend_nib_selected.transform.Find("background").GetComponent<Image>().color = friend_nib_selected_prev_bg_col;
		friend_nib_selected.transform.Find("friend name").GetComponent<CanvasGroup>().alpha = friend_nib_selected_prev_text_alpha;
		friend_nib_selected = null;
	}

	public void OnClickAway()
	{
		if (!PopupControl.Instance.popup_open)
		{
			if (friend_nib_selected != null) DeselectCurrSelectedFriendNib();
			if (hover_visible) HideHover();
		}
	}

	private void Update()
	{
		if (curr_screen == friend_window_screen.friend_list && GamepadInput.Instance.GetMouseButtonDown()) OnClickAway();
	}

	public void ClickFriendNib(int visual_index)
	{
		PopupControl.Instance.SetButtonWasPressed();
		if (visual_index < 0 || visual_index >= instantiated_friends_nibs.Count) return;
		GameObject nib = instantiated_friends_nibs[visual_index];
		if (nib == null) return;
		if (friend_nib_selected != null) DeselectCurrSelectedFriendNib();
		if (hover_visible) HideHover();
		Friend friend = receiver.GetFriendByUsername(nib.GetComponent<FriendNib>().friend_username_lower);
		if (friend.status_t == Friend.status.req_received) return;
		Text header = WindowPrefabsControl.Instance.GetTextLegacy("FRIENDS-friends_list", "hover header text");
		Image background = WindowPrefabsControl.Instance.GetImage("FRIENDS-friends_list", "hover bg");
		GameObject[] to_hide = new GameObject[3];
		for (int i = 0; i < 3; i++) to_hide[i] = WindowPrefabsControl.Instance.GetObject("FRIENDS-friends_list", "to hide " + i);
		GameObject hover = WindowPrefabsControl.Instance.GetObject("FRIENDS-friends_list", "hover");
		Transform parent = hover.transform.parent;
		hover.transform.SetParent(nib.transform.parent);
		hover.transform.localPosition = nib.transform.localPosition + Vector3.right * 300f;
		hover.transform.SetParent(parent);
		friend_nib_selected = nib;
		nib.transform.localScale = Vector3.one * 1.06f;
		CanvasGroup bg_group = nib.transform.Find("background").GetComponent<CanvasGroup>();
		Image bg_image = nib.transform.Find("background").GetComponent<Image>();
		CanvasGroup text_group = nib.transform.Find("friend name").GetComponent<CanvasGroup>();
		friend_nib_selected_prev_bg_alpha = bg_group.alpha;
		friend_nib_selected_prev_bg_col = bg_image.color;
		friend_nib_selected_prev_text_alpha = text_group.alpha;
		bg_group.alpha = 1f;
		bg_image.color = new Color(0.52f, 0.75f, 0.86f, 1f);
		text_group.alpha = friend.status_t == Friend.status.offline ? 0.8f : 1f;
		nib.transform.SetAsLastSibling();
		Text notifs = WindowPrefabsControl.Instance.GetTextLegacy("FRIENDS-friends_list", "notifs_text");
		notifs.transform.parent.gameObject.SetActive(friend.chat.n_unread >= 1);
		if (friend.chat.n_unread >= 1) notifs.text = friend.chat.n_unread.ToString();
		hover_visible = true;
		hover.GetComponent<Animation>().Stop();
		hover.GetComponent<Animation>().Play("hover show");
		header.text = friend.username_punctuated;
		bool show = friend.status_t != Friend.status.offline && friend.status_t != Friend.status.req_sent;
		foreach (GameObject obj in to_hide) obj.SetActive(show);
		RectTransform rect = (RectTransform)background.transform;
		rect.sizeDelta = new Vector2(rect.sizeDelta.x, show ? 561f : 190f);
		last_friend_clicked = friend.username_lower;
	}

	public void PressPubServerTab()
	{
		curr_pub_server_page = 0;
		if ((DateTime.UtcNow - last_pub_server_request).TotalSeconds <= 8.0) ChangeFriendScreen(friend_window_screen.public_server_list);
		else
		{
			FriendServerSender.ChangeConnectingText("Loading Public Servers");
			ChangeFriendScreen(friend_window_screen.connecting_screen);
			sender.RequestPublicServerList();
			last_pub_server_request = DateTime.UtcNow;
		}
	}

	public void PressBackOnJoinPublicServer()
	{
		ChangeFriendScreen(friend_window_screen.public_server_list);
	}

	public void PressJoinPublicServer()
	{
		if (GameServerConnector.Instance.server_name == server_join_viewing.server_name)
		{
			PopupControl.Instance.ShowMessage("You are already connected to that server!");
			return;
		}
		FriendServerSender.ChangeConnectingText("Connecting");
		ChangeFriendScreen(friend_window_screen.connecting_screen);
		sender.TryJoinPublicServer(server_join_viewing.server_name);
	}

	public void PressAcceptFriend(string friend_username_lower)
	{
		FriendServerSender.ChangeConnectingText("Accepting Friend Request");
		ChangeFriendScreen(friend_window_screen.connecting_screen);
		sender.AcceptFriendRequest(receiver.GetFriendByUsername(friend_username_lower).username_lower);
	}

	public void PressDeclineFriend(string friend_username_lower)
	{
		FriendServerSender.ChangeConnectingText("Declining Friend Request");
		ChangeFriendScreen(friend_window_screen.connecting_screen);
		sender.DeclineFriendRequest(receiver.GetFriendByUsername(friend_username_lower).username_lower);
	}

	public void PressInviteFriend()
	{
		Friend friend = receiver.GetFriendByUsername(last_friend_clicked);
		if (friend == null) return;
		Connection game = GameServerConnector.Instance.game_server_connection;
		if (game != null && (game.GetStatus() == Connection.connection_status.connecting || game.GetStatus() == Connection.connection_status.connected) && friend.world_string_server_name == GameServerConnector.Instance.server_name)
		{
			PopupControl.Instance.ShowMessage("Could not invite!\nPlayer is already connected to that server");
			return;
		}
		FriendServerSender.ChangeConnectingText("Sending Invite");
		ChangeFriendScreen(friend_window_screen.connecting_screen);
		sender.InviteFriend(friend.username_lower);
		friend.chat.AddLog(new chat_log("<i><color=#dddddd>You invited " + friend.username_lower + " to play</color></i>", "", null, false, null));
	}

	public void PressJoinFriend()
	{
		Friend friend = receiver.GetFriendByUsername(last_friend_clicked);
		if (friend == null || friend.status_t != Friend.status.online) return;
		Connection game = GameServerConnector.Instance.game_server_connection;
		if (game != null && (game.GetStatus() == Connection.connection_status.connecting || game.GetStatus() == Connection.connection_status.connected) && GameServerConnector.Instance.server_name == friend.world_string_server_name)
		{
			PopupControl.Instance.ShowMessage("Cannot join\nYou are already connected to that server!");
			return;
		}
		switch (friend.world_string_type)
		{
			case Friend.world_string_type_t.in_main_menu:
				PopupControl.Instance.ShowMessage("Cannot join\n" + friend.username_punctuated + " is at the Main Menu!");
				break;
			case Friend.world_string_type_t.in_self_personal_alone:
			case Friend.world_string_type_t.in_self_personal_w_others:
				AskToJoinFriend(friend);
				break;
			case Friend.world_string_type_t.in_other_personal:
				string owner = friend.world_string_server_name.Replace("(private)", "");
				Friend host = receiver.GetFriendByUsername(owner.ToLower());
				if (host != null) AskToJoinFriend(host);
				else PopupControl.Instance.ShowMessage("Cannot join\nYou are not friends with <color=#76e8bb>" + owner + "</color>.\n\n<color=#bbbbbb>(Joining friends-of-friends will work in the future! It's not coded yet)</color>");
				break;
			case Friend.world_string_type_t.in_pub:
				PopupControl.Instance.ShowConnecting("Joining <color=#76e8bb>" + friend.username_punctuated + "</color>");
				sender.TryJoinPublicServer(friend.world_string_server_name);
				break;
		}
	}

	private void AskToJoinFriend(Friend friend)
	{
		PopupControl.Instance.ShowMessage("Asked <color=#76e8bb>" + friend.username_punctuated + "</color> if you are allowed to join");
		sender.AskToJoinPlayer(friend.username_lower, 0, "");
		friend.chat.AddLog(new chat_log("<i><color=#dddddd>Asked " + friend.username_lower + " if you can join</color></i>", "", null, false, null));
	}

	public void PressSendChat()
	{
		InputField input = WindowPrefabsControl.Instance.GetObject("CHAT", "TextInput").GetComponent<InputField>();
		string message = input.text;
		if (Startup.StringNullOrWhitespace(message)) return;
		chat_log log = new chat_log("<color=#baffba>" + PlayerData.Instance.GetGlobalString("username_punctuated") + ":</color> " + message, "", null, false, null);
		if (curr_screen == friend_window_screen.friend_chat)
		{
			sender.SendPrivateMessage(last_friend_clicked, message);
			Friend friend = receiver.GetFriendByUsername(last_friend_clicked);
			friend.chat.AddLog(log);
			FriendChatReceived(friend, log);
		}
		else if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.chat) GameServerSender.Instance.SendGameChat(message);
		input.SetTextWithoutNotify("");
	}

	public void PressMessageFriend()
	{
		OpenFriendChat();
	}

	public void RedrawChat(ChatCollection chat)
	{
		foreach (GameObject obj in chat_log_objects) Destroy(obj);
		chat_log_objects.Clear();
		int i = 0;
		foreach (chat_log log in chat.entries) CreateChatLogObj(i++, log, false);
		SetChatMaxScroll(chat);
	}

	public void PressRemoveFriend()
	{
		Friend friend = receiver.GetFriendByUsername(last_friend_clicked);
		if (friend == null) return;
		string friend_to_remove = friend.username_lower;
		PopupControl.Instance.on_yes_pressed = delegate
		{
			FriendServerSender.ChangeConnectingText("Removing Friend");
			ChangeFriendScreen(friend_window_screen.connecting_screen);
			sender.SendRemoveFriend(friend_to_remove);
		};
		PopupControl.Instance.ShowYesNo("Are you sure you want to remove <color=#ff5e45>" + friend.username_punctuated + "?</color>", "Yes", "No", PopupControl.context.yesno_ACTION);
	}

	public bool UsernameInvalid(string username_lower)
	{
		if (username_lower.Length <= 3) PopupControl.Instance.ShowMessage("<color=#bbbbbb>Username too short!</color>\nUsername must be more than 3 letters long.");
		else if (username_lower.Length >= 16) PopupControl.Instance.ShowMessage("<color=#bbbbbb>Username too long!</color>\nUsername must be less than 16 letters.");
		else if (username_lower.Contains(" ")) PopupControl.Instance.ShowMessage("<color=#bbbbbb>Username invalid</color>\nUsername cannot contain spacebar.");
		else return false;
		return true;
	}

	private void CreateGiftNib(int i, Trophy trophy)
	{
		GameObject obj = Instantiate(gift_nib_prefab);
		obj.GetComponent<GiftNib>().index = i;
		obj.transform.SetParent(WindowPrefabsControl.Instance.GetScreen("FRIENDS-gifts").GetComponent<Scrollable>().scrollwheel_parent.transform);
		obj.transform.localScale = Vector3.one;
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localPosition = new Vector3(0f, 243f - 114f * i, 0f);
		obj.transform.Find("gift name").GetComponent<Text>().text = "x1 " + trophy.trophy_name + "\n<color=#cccccc>from " + trophy.trophy_from + "</color>";
		obj.transform.Find("item icon").GetComponent<ItemSprite>().RedrawBasic(GenerateTrophy(trophy), 1);
		instantiated_gifts_nibs.Add(obj);
	}

	public InventoryItem GenerateTrophy(Trophy trophy)
	{
		ExtraInventoryData extra = new ExtraInventoryData();
		extra.SetString("trophy_name", trophy.trophy_name);
		extra.SetString("trophy_reason", trophy.trophy_reason);
		extra.SetString("paint", trophy.trophy_paint);
		extra.SetString("trophy_from", trophy.trophy_from);
		extra.SetString("trophy_for", PlayerData.Instance.GetGlobalString("username_punctuated"));
		extra.SetString("trophy_date", trophy.trophy_date);
		extra.SetString("trophy_VALIDATOR", GetTrophyValidatorString(trophy.trophy_name, trophy.trophy_reason, trophy.trophy_paint, trophy.trophy_from, PlayerData.Instance.GetGlobalString("username_punctuated"), trophy.trophy_date));
		return new InventoryItem("Trophy", extra);
	}

	public string GetTrophyValidatorString(string trophy_name, string trophy_reason, string trophy_paint, string trophy_from, string trophy_for, string trophy_date)
	{
		int sum = 0;
		foreach (string str in new List<string> { trophy_name, trophy_reason, trophy_paint, trophy_from, trophy_for, trophy_date })
		{
			for (int i = 0; i < str.Length; i++) sum += str[i];
		}
		List<char> chars = new List<char>();
		string digits = sum.ToString();
		for (int i = 0; i < digits.Length; i++) chars.Add((char)(int.Parse(digits[i].ToString(), Startup.parse_culture) + 97));
		return new string(chars.ToArray());
	}

	private void CreateFriendNib(Friend.status status_t, int i, Friend friend)
	{
		GameObject obj = Instantiate(friend_nib_prefab);
		obj.GetComponent<FriendNib>().friend_username_lower = friend.username_lower;
		obj.GetComponent<FriendNib>().visual_index = i;
		obj.transform.SetParent(WindowPrefabsControl.Instance.GetScreen("FRIENDS-friends_list").GetComponent<Scrollable>().scrollwheel_parent.transform);
		obj.transform.localScale = Vector3.one;
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localPosition = new Vector3(0f, 200f - 114f * i, 0f);
		Text name = obj.transform.Find("friend name").GetComponent<Text>();
		CanvasGroup name_alpha = obj.transform.Find("friend name").GetComponent<CanvasGroup>();
		CanvasGroup status_alpha = obj.transform.Find("status-bg").GetComponent<CanvasGroup>();
		Image status_color = obj.transform.Find("status-bg").Find("status-col").GetComponent<Image>();
		CanvasGroup background = obj.transform.Find("background").GetComponent<CanvasGroup>();
		string world = "";
		switch (status_t)
		{
			case Friend.status.offline:
				name.text = friend.username_punctuated + " (OFFLINE)";
				name_alpha.alpha = 0.33f;
				status_alpha.alpha = 0.8f;
				status_color.color = new Color(0.74f, 0.74f, 0.74f, 1f);
				background.alpha = 0.5f;
				break;
			case Friend.status.online:
				name.text = friend.username_punctuated + " <color=#00ff00>(ONLINE)</color>";
				name_alpha.alpha = 1f;
				status_alpha.alpha = 1f;
				status_color.color = Color.green;
				background.alpha = 1f;
				obj.transform.Find("notifications").gameObject.SetActive(friend.chat.n_unread >= 1);
				if (friend.chat.n_unread >= 1) obj.transform.Find("notifications").Find("Text").GetComponent<Text>().text = friend.chat.n_unread.ToString();
				switch (friend.world_string_type)
				{
					case Friend.world_string_type_t.in_main_menu: world = "in Main Menu"; break;
					case Friend.world_string_type_t.in_self_personal_alone: world = "in Personal World"; break;
					case Friend.world_string_type_t.in_self_personal_w_others:
						world = "in Personal World\n<size=35><color=#00ff00>with " + friend.world_string_n_online + (friend.world_string_n_online == 1 ? " other player</color></size>" : " other players</color></size>");
						break;
					case Friend.world_string_type_t.in_other_personal:
						world = "in <color=#4de6ff>" + friend.world_string_server_name.Replace("(private)", "") + "'s</color> World\n<size=35><color=#00ff00>with " + friend.world_string_n_online + (friend.world_string_n_online == 1 ? " other player</color></size>" : " other players</color></size>");
						break;
					case Friend.world_string_type_t.in_pub: world = "in <color=#ffe46b>" + friend.world_string_server_name + "</color>"; break;
				}
				obj.transform.Find("world string mask").Find("world string").GetComponent<Text>().text = world;
				instantiated_friends_nibs.Add(obj);
				return;
			case Friend.status.req_sent:
				name.text = friend.username_punctuated + " <color=#1FD1FF>(REQUEST SENT)</color>";
				name_alpha.alpha = 1f;
				status_alpha.alpha = 1f;
				status_color.color = new Color(0.12f, 0.82f, 1f, 1f);
				background.alpha = 0.5f;
				break;
			case Friend.status.req_received:
				name.text = friend.username_punctuated + " <color=#FFF994>added you!</color>";
				name_alpha.alpha = 1f;
				status_alpha.alpha = 1f;
				status_color.color = new Color(1f, 250f / 255f, 148f / 255f, 1f);
				background.alpha = 1f;
				obj.transform.Find("accept decline buttons").gameObject.SetActive(true);
				break;
		}
		obj.transform.Find("notifications").gameObject.SetActive(false);
		if (status_t == Friend.status.offline && friend.last_online_set)
		{
			TimeSpan elapsed = DateTime.UtcNow - friend.last_online;
			int hours = (int)elapsed.TotalHours;
			if (hours < 1)
			{
				int minutes = (int)elapsed.TotalMinutes;
				world = minutes < 1 ? "Last online: just now" : minutes == 1 ? "Last online: 1 minute ago" : "Last online: " + minutes + " minutes ago";
			}
			else if (hours < 24) world = hours == 1 ? "Last online: 1 hour ago" : "Last online: " + hours + " hours ago";
			else
			{
				int days = (int)elapsed.TotalDays;
				if (days < 7) world = days == 1 ? "Last online: 1 day ago" : "Last online: " + days + " days ago";
				else if (days < 14) world = "Last online: 1 week ago";
				else if (days < 21) world = "Last online: 2 weeks ago";
				else if (days < 28) world = "Last online: 3 weeks ago";
				else if (days < 56) world = "Last online: 1 month ago";
			}
			world = "<size=37><color=#7F9FAD>" + world + "</color></size>";
		}
		obj.transform.Find("world string mask").Find("world string").GetComponent<Text>().text = world;
		instantiated_friends_nibs.Add(obj);
	}

	public void PressPubServerPageR()
	{
		if (curr_pub_server_page >= MaxPubServerPage()) return;
		curr_pub_server_page++;
		RedrawPublicServerList();
	}

	public void PressPubServerPageL()
	{
		if (curr_pub_server_page <= 0) return;
		curr_pub_server_page--;
		RedrawPublicServerList();
	}

	public void PressPubServNib(int index)
	{
		server_join_viewing = GetServerFromPage(curr_pub_server_page, index);
		ChangeFriendScreen(friend_window_screen.public_server_join);
	}

	public int MaxPubServerPage()
	{
		return receiver.public_server_list.Count == 0 ? 0 : (receiver.public_server_list.Count - 1) / 6;
	}

	public int NumServersOnPage(int page)
	{
		if (receiver.public_server_list.Count < 7) return page == 0 ? receiver.public_server_list.Count : 0;
		return Math.Max(0, Math.Min(6, receiver.public_server_list.Count - page * 6));
	}

	public ServerInfo GetServerFromPage(int page, int index_on_page)
	{
		return receiver.public_server_list[index_on_page + page * 6];
	}

	public void GotServerIcon(string server_name)
	{
		if (curr_screen != friend_window_screen.public_server_list) return;
		for (int i = 0; i < NumServersOnPage(curr_pub_server_page); i++)
		{
			if (GetServerFromPage(curr_pub_server_page, i).server_name != server_name) continue;
			DrawServerIcon(server_name, WindowPrefabsControl.Instance.GetObject("FRIENDS-pub_serv_list", "nib-" + i));
			return;
		}
	}

	private void DrawServerIcon(string server_name, GameObject the_nib)
	{
		GameObject spin = the_nib.transform.Find("spin").gameObject;
		GameObject icon = the_nib.transform.Find("img").gameObject;
		if (receiver.cached_server_icons.ContainsKey(server_name))
		{
			spin.SetActive(false);
			icon.SetActive(true);
			icon.GetComponent<Image>().sprite = receiver.cached_server_icons[server_name] != null ? receiver.cached_server_icons[server_name] : default_server_icon;
		}
		else
		{
			spin.SetActive(true);
			icon.SetActive(false);
			if (!receiver.requesting_server_icons.Contains(server_name)) sender.RequestServerIcon(server_name);
		}
	}

	public void RedrawPublicServerList()
	{
		WindowPrefabsControl windows = WindowPrefabsControl.Instance;
		windows.GetObject("FRIENDS-pub_serv_list", "page_R").GetComponent<CanvasGroup>().alpha = curr_pub_server_page < MaxPubServerPage() ? 1f : 0.25f;
		windows.GetObject("FRIENDS-pub_serv_list", "page_L").GetComponent<CanvasGroup>().alpha = curr_pub_server_page > 0 ? 1f : 0.25f;
		for (int i = 0; i < 6; i++) windows.GetObject("FRIENDS-pub_serv_list", "nib-" + i).SetActive(false);
		windows.GetObject("FRIENDS-pub_serv_list", "none online").SetActive(receiver.public_server_list.Count == 0);
		for (int i = 0; i < NumServersOnPage(curr_pub_server_page); i++)
		{
			GameObject nib = windows.GetObject("FRIENDS-pub_serv_list", "nib-" + i);
			ServerInfo server = GetServerFromPage(curr_pub_server_page, i);
			nib.SetActive(true);
			nib.transform.Find("title").GetComponent<Text>().text = server.server_name;
			nib.transform.Find("num online").GetComponent<Text>().text = (server.n_online >= server.max_players ? "(FULL) " : "") + server.n_online + " players";
			nib.GetComponent<CanvasGroup>().alpha = 1f;
			DrawServerIcon(server.server_name, nib);
			nib.transform.Find("locked").gameObject.SetActive(false);
			nib.transform.Find("type").GetComponent<Text>().text = server.server_game_mode == "Open World PVP" ? "Open World <color=#ff0000>PVP</color>" : server.server_game_mode;
		}
	}

	public void PressRegister()
	{
		string username = WindowPrefabsControl.Instance.GetTextLegacy("FRIENDS-register", "username-text").text;
		if (UsernameInvalid(username)) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(10);
		outgoing.PutString(username);
		connection.Send(outgoing);
		ChangeFriendScreen(friend_window_screen.register_attempt);
		FriendServerSender.ChangeConnectingText("Checking Username");
	}

	public void PressUpdateButton()
	{
		WindowControl.Instance.CloseMiniwindow(true);
		PopupControl.Instance.GotoGamePage();
	}

	public void ShowFailedToConnect(string message, bool show_update_button)
	{
		if (WindowPrefabsControl.Instance.GetScreen("FRIENDS-loading") == null) return;
		WindowPrefabsControl.Instance.GetObject("FRIENDS-loading", "Connecting").SetActive(false);
		WindowPrefabsControl.Instance.GetObject("FRIENDS-loading", "Retry").SetActive(true);
		WindowPrefabsControl.Instance.GetTextLegacy("FRIENDS-loading", "FailureText").text = message;
		WindowPrefabsControl.Instance.GetObject("FRIENDS-loading", "UpdateButton").SetActive(show_update_button);
	}
}
