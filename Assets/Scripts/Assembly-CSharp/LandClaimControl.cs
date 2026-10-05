using System.Collections.Generic;
using UnityEngine;

public class LandClaimControl : MonoBehaviour, OrderedStart
{
	public enum land_screen_context_t
	{
		none = 0,
		open_active = 1,
		open_expired = 2
	}

	public static LandClaimControl Instance;

	public land_screen_context_t land_screen_context;

	public string curr_land_claim_player_0;

	public string curr_land_claim_player_1;

	public string curr_land_claim_player_2;

	public static int three_day_land_claim_add_seconds;

	public static int three_day_land_claim_add_days;

	public static int eight_day_land_claim_add_seconds;

	public static int eight_day_land_claim_add_days;

	public static int admin_land_claim_add_seconds;

	public static int admin_land_claim_add_days;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
	}

	public void GetLandClaimPlayers(ChunkData chunk_data, string land_claim_str)
	{
	}

	private void RedrawLandClaimPlayers()
	{
	}

	public void PressLandClaimDeleteUser(int slot)
	{
	}

	public void FinishedTypingSlot2()
	{
	}

	public void FinishedTypingSlot3()
	{
	}

	public void ModifyCurrent(int slot, string new_username)
	{
	}

	public bool IsFarEnoughAwayFromEnemyLandClaims(string zone, int chunkX, int chunkZ, string username_lower)
	{
		return false;
	}

	public void ModifyLandClaimTimer(string zone, int chunkX, int chunkZ, int innerX, int innerZ, int user_index, string new_username, string[] mp_cache_keys)
	{
	}

	private Dictionary<string, LandClaimChunkTimer> GetLandClaimChunkTimersAt(string zone, int chunkX, int chunkZ)
	{
		return null;
	}

	public bool AllowedToBuild(string zone, int chunkX, int chunkZ)
	{
		return false;
	}

	public bool LandOwnedByMe(string zone, int chunkX, int chunkZ)
	{
		return false;
	}

	public bool IsCurrLandClaimMine()
	{
		return false;
	}

	public void OpenLandClaimScreen(string land_claim_item_name, string zone, int chunkX, int chunkZ, int innerX, int innerZ)
	{
	}

	public void OnClose()
	{
	}

	public void RedrawTimeRemaining()
	{
	}

	public void AddLandClaimsToNearbyChunks(string zone, int chunkX, int chunkZ, int innerX, int innerZ, InventoryItem landclaim_item, string builder_player)
	{
	}

	public void RemoveLandClaimsFromNearbyChunks(string zone, int chunkX, int chunkZ, int innerX, int innerZ)
	{
	}
}
