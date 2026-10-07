using System;

public class Friend
{
	public enum world_string_type_t
	{
		in_main_menu = 0,
		in_self_personal_alone = 1,
		in_self_personal_w_others = 2,
		in_other_personal = 3,
		in_pub = 4
	}

	public enum status
	{
		offline = 0,
		online = 1,
		req_sent = 2,
		req_received = 3
	}

	public string username_lower;

	public string username_punctuated;

	public status status_t;

	public world_string_type_t world_string_type;

	public string world_string_server_name;

	public int world_string_n_online;

	public bool last_online_set;

	public DateTime last_online;

	public ChatCollection chat = new ChatCollection();

	public Friend(string username, status status_t, string punctuated_username)
	{
		this.username_lower = username;
		this.username_punctuated = punctuated_username;
		this.status_t = status_t;
	}

	public void set_last_online(DateTime last_online)
	{
		this.last_online = last_online;
		last_online_set = true;
	}
}
