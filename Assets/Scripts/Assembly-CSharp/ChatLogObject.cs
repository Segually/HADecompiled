using UnityEngine;

public class ChatLogObject : MonoBehaviour
{
	public int corresponding_chat_log;

	public void ClickButton()
	{
		FriendServerInterface.Instance.PressChatLogButton(corresponding_chat_log);
	}
}
