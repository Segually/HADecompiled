using System.Collections.Generic;

public class ChatCollection
{
	public List<chat_log> entries = new List<chat_log>();

	public static int chatlogspacing;

	public int n_unread;

	public static int max_chat_logs;

	public void AddLog(chat_log log)
	{
	}

	public void DisableOldInvites()
	{
	}

	public void DisableOldJoins()
	{
	}
}
