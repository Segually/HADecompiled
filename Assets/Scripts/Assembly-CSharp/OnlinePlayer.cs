using UnityEngine;

public class OnlinePlayer
{
	public GameObject obj;

	public string username_lower;

	public string username_punctuated;

	public int snap_mobs_to_realpos_counter;

	public int snap_player_to_realpos_counter;

	public string currently_using;

	public string sitting_in_chair;

	public OnlinePlayer(string username_lower, string username_punctuated, OnlinePlayerData player_stats)
	{
		this.username_lower = username_lower;
		this.username_punctuated = username_punctuated;
		currently_using = player_stats.currently_using;
		sitting_in_chair = player_stats.sitting_in_chair;
	}
}
