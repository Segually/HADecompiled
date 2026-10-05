using System;
using System.Collections.Generic;
using UnityEngine;

public class FriendServerSender : MonoBehaviour, OrderedStart
{
	public static FriendServerSender Instance;

	public static string connecting_string;

	private bool sending_request_of_some_sort;

	private DateTime request_sent_at;

	public Connection connection => null;

	public FriendServerConnector connector => null;

	public FriendServerReceiver receiver => null;

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

	private void BeginTimeout()
	{
	}

	public void EndTimeout()
	{
	}

	public bool WaitingOnRequest()
	{
		return false;
	}

	public static void ChangeConnectingText(string str)
	{
	}

	public void SendSubmitReport()
	{
	}

	public void SendTakeTrophy(string random_trophy_id)
	{
	}

	public void RequestServerIcon(string server_name)
	{
	}

	public void SendPrivateMessage(string username_lower, string message)
	{
	}

	public void RequestPublicServerList()
	{
	}

	public void TryJoinPublicServer(string server_name)
	{
	}

	public void AskToJoinPlayer(string player_name, byte type, string server_name = "")
	{
	}

	public void TryAddFriend(string username_lower)
	{
	}

	public void AcceptFriendRequest(string username_lower)
	{
	}

	public void DeclineFriendRequest(string username_lower)
	{
	}

	public void InviteFriend(string username_lower)
	{
	}

	public void SendRemoveFriend(string remove_username)
	{
	}

	public void UpdateWorldString()
	{
	}

	public void PackWorldString(Packet outgoing)
	{
	}

	public void SendAttemptLogin()
	{
	}

	public void SendMathSolution(int solution)
	{
	}

	public void SignalIntent(byte intent)
	{
	}

	public void SendWantToLogInAsPlayer()
	{
	}

	public void SendAcceptInviteFailed(string tried_to_join, byte reason)
	{
	}

	public void SendYouMayJoinMyWorldNow(string tried_to_join_lower, byte type, string friend_of = "")
	{
	}

	public void SendDispatcherPingResult(Dictionary<string, short> results)
	{
	}

	public void SendAcceptInvite(string username_lower)
	{
	}

	public void SendPing()
	{
	}
}
