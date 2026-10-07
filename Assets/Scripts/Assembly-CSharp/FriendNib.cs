using UnityEngine;

public class FriendNib : MonoBehaviour
{
	public int visual_index;

	public string friend_username_lower;

	public void PressAccept()
	{
		FriendServerInterface.Instance.PressAcceptFriend(friend_username_lower);
	}

	public void PressDecline()
	{
		FriendServerInterface.Instance.PressDeclineFriend(friend_username_lower);
	}

	public void Clicked()
	{
		WindowPrefabsControl.Instance.GetScreen("FRIENDS-friends_list").GetComponent<Scrollable>().TryClickNib(visual_index, FriendServerInterface.Instance.ClickFriendNib);
	}
}
