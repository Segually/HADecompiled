using System;
using System.Collections.Generic;
using UnityEngine;

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

	private List<GameObject> chat_log_objects;

	public Dictionary<string, string> report_data;

	public friend_window_screen curr_screen;

	private int report_list_tab;

	private int report_find_user_page;

	private List<GameObject> instantiated_gifts_nibs;

	private List<GameObject> instantiated_friends_nibs;

	private bool hover_visible;

	private GameObject friend_nib_selected;

	private float friend_nib_selected_prev_text_alpha;

	private float friend_nib_selected_prev_bg_alpha;

	private Color friend_nib_selected_prev_bg_col;

	public string last_friend_clicked;

	private DateTime last_pub_server_request;

	public GameObject gift_nib_prefab;

	public GameObject friend_nib_prefab;

	public int curr_pub_server_page;

	public GameObject global_notifications_obj;

	private ServerInfo server_join_viewing;

	public Sprite default_server_icon;

	public Connection connection => null;

	public FriendServerConnector connector => null;

	public FriendServerSender sender => null;

	public FriendServerReceiver receiver => null;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
	}

	public void PressYes13()
	{
	}

	public void PressNo13()
	{
	}

	public void ShowNumFriendsOnline()
	{
	}

	public void ShowNewGiftsNotif()
	{
	}

	public void PressedFriendsButton()
	{
	}

	public void SetChatMaxScroll(ChatCollection chat)
	{
	}

	public void CreateChatLogObj(int i, chat_log log, bool window_already_open)
	{
	}

	public void PressChatLogButton(int id)
	{
	}

	public void FriendChatReceived(Friend friend, chat_log new_log)
	{
	}

	public void RedrawGlobalNotificationCounter()
	{
	}

	public void OpenFriendScreen(friend_window_screen new_screen)
	{
	}

	public void DirectOpenFriend(string friend_username_lower)
	{
	}

	public void PressGiftNotif()
	{
	}

	private void OpenFriendChat()
	{
	}

	public void PressReport()
	{
	}

	public void PressAddFriend()
	{
	}

	public void PressCancelAddFriend()
	{
	}

	public void PressAcceptAddFriend()
	{
	}

	public void PressReportCategory(int index)
	{
	}

	public void PressReportDoubleCheckCategory(int index)
	{
	}

	public void PressUserReportSend()
	{
	}

	public void ChangeFriendScreen(friend_window_screen new_screen)
	{
	}

	public void PressGifts()
	{
	}

	public void PressSubmitReportPlayer()
	{
	}

	public void PressNextOnReportUsername()
	{
	}

	public void PressReportListTab(int tab)
	{
	}

	public void PressRecentNib(int i)
	{
	}

	public void OnEditReportUsernameInput()
	{
	}

	private void RedrawRecentUsersList()
	{
	}

	private bool NextFindUserPageExists()
	{
		return false;
	}

	public void PressChangeRecentUserPage(int dir)
	{
	}

	public void RedrawJoinPubServScreen()
	{
	}

	public void OnCloseFriendScreen()
	{
	}

	public void RedrawGiftsScreen()
	{
	}

	public void RedrawFriendsList()
	{
	}

	private void HideHover()
	{
	}

	private void DeselectCurrSelectedFriendNib()
	{
	}

	public void OnClickAway()
	{
	}

	private void Update()
	{
	}

	public void ClickFriendNib(int visual_index)
	{
	}

	public void PressPubServerTab()
	{
	}

	public void PressBackOnJoinPublicServer()
	{
	}

	public void PressJoinPublicServer()
	{
	}

	public void PressAcceptFriend(string friend_username_lower)
	{
	}

	public void PressDeclineFriend(string friend_username_lower)
	{
	}

	public void PressInviteFriend()
	{
	}

	public void PressJoinFriend()
	{
	}

	private void AskToJoinFriend(Friend friend)
	{
	}

	public void PressSendChat()
	{
	}

	public void PressMessageFriend()
	{
	}

	public void RedrawChat(ChatCollection chat)
	{
	}

	public void PressRemoveFriend()
	{
	}

	public bool UsernameInvalid(string username_lower)
	{
		return false;
	}

	private void CreateGiftNib(int i, Trophy trophy)
	{
	}

	public InventoryItem GenerateTrophy(Trophy trophy)
	{
		return null;
	}

	public string GetTrophyValidatorString(string trophy_name, string trophy_reason, string trophy_paint, string trophy_from, string trophy_for, string trophy_date)
	{
		return null;
	}

	private void CreateFriendNib(Friend.status status_t, int i, Friend friend)
	{
	}

	public void PressPubServerPageR()
	{
	}

	public void PressPubServerPageL()
	{
	}

	public void PressPubServNib(int index)
	{
	}

	public int MaxPubServerPage()
	{
		return 0;
	}

	public int NumServersOnPage(int page)
	{
		return 0;
	}

	public ServerInfo GetServerFromPage(int page, int index_on_page)
	{
		return null;
	}

	public void GotServerIcon(string server_name)
	{
	}

	private void DrawServerIcon(string server_name, GameObject the_nib)
	{
	}

	public void RedrawPublicServerList()
	{
	}

	public void PressRegister()
	{
	}

	public void PressUpdateButton()
	{
	}

	public void ShowFailedToConnect(string message, bool show_update_button)
	{
	}
}
