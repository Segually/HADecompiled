using System.Collections.Generic;

public class ChatCollection
{
	public List<chat_log> entries = new List<chat_log>();

	public static int chatlogspacing = 92;

	public int n_unread;

	public static int max_chat_logs = 35;

	public void AddLog(chat_log log)
	{
		entries.Insert(0, log);
		if (entries.Count >= max_chat_logs)
		{
			List<chat_log> list = new List<chat_log>();
			for (int i = 0; i < max_chat_logs; i++)
			{
				list.Add(entries[i]);
			}
			entries = list;
		}
	}

	public void DisableOldInvites()
	{
		foreach (chat_log entry in entries)
		{
			if (entry.accept_or_deny_data != null && entry.accept_or_deny_data["action"] == "accept_invite")
			{
				entry.accept_or_deny_data = null;
				entry.img = null;
				entry.has_bg = false;
			}
		}
	}

	public void DisableOldJoins()
	{
		foreach (chat_log entry in entries)
		{
			if (entry.accept_or_deny_data != null && entry.accept_or_deny_data["action"] == "accept_other_join")
			{
				entry.accept_or_deny_data = null;
				entry.img = null;
				entry.has_bg = false;
			}
		}
	}
}
