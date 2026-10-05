using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameServerSender : MonoBehaviour, OrderedStart
{
	private enum variation_commited_direction
	{
		none = 0,
		up = 1,
		down = 2
	}

	public enum chest_request_type
	{
		normal = 0,
		unopened_loot = 1,
		egg_fuser = 2
	}

	public static GameServerSender Instance;

	public Dictionary<string, CreatureStruct> mobs_I_am_trying_to_claim_awaiting_response;

	public List<string> other_players_mobs_that_I_inquired_about;

	public string packet_validator_code;

	public int packet_validator_total_variation;

	public Dictionary<string, ChunkData> cached_chunks;

	public List<string> chunks_mid_request;

	public bool requesting_unique_ids;

	private int stream_positions_in_N_frames;

	private Vector3 last_nearby_check;

	private IEnumerator zone_data_timeout;

	public Connection connection => null;

	public GameServerConnector connector => null;

	public GameServerReceiver receiver => null;

	public GameServerInterface interface_ => null;

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

	public void SendReturningBackToBreeder()
	{
	}

	public void SendCreatedLocalMob(string minion_combat_id)
	{
	}

	public void SendCreatePerkDrop(Vector3 position, string effect_name, PerkData perk_data, int perk_level, string caster_id, int caster_level, string fn_validator)
	{
	}

	public void SendLaunchProjectilePerk(PerkData perk_data, int perk_level, string target_id, string caster_id, int caster_level, Vector3 target_pos, Vector3 caster_pos, string fn_validator)
	{
	}

	public void SendAllPreAppliedPerks(string send_to_user, string fn_validator)
	{
	}

	public void SendApplyPerk(string effect_name, string target_id, string caster_id, int caster_level, PerkData perk_data, int perk_level, bool on_duration_reapply, string fn_validator)
	{
	}

	public void SendQuickTag(byte active, string fn_validator)
	{
	}

	public void SendRenameCompanion(string combat_name, string new_companion_name)
	{
	}

	public void SendCompanionChangeEquip(string combat_name, InventoryItem hat_, InventoryItem body_, InventoryItem hand_)
	{
	}

	public void SendDestroyCompanion(string combat_name)
	{
	}

	public void SendDeloadMob(string combat_id)
	{
	}

	public void SendTryClaimMobs(List<CreatureStruct> attempt_request)
	{
	}

	private void WriteMobIds(List<string> ids, Packet outgoing)
	{
	}

	public void UpdateSyncedTargetIds(GameObject caller)
	{
	}

	public void PutSingleMobPosition(string creature_id, Packet outgoing)
	{
	}

	public bool DontSendToSaveBandwidth()
	{
		return false;
	}

	public void SendMyMobPositions()
	{
	}

	public void SendHitMob(string defender_combat_id, int real_dmg, int fake_dmg, Combatant.hit_col hit_col, bool missed, bool dodged, GameObject attacker, byte mob_type, string fn_validator)
	{
	}

	private string PacketValidatorVariation()
	{
		return null;
	}

	public void SendAttackAnimation(string combat_id)
	{
	}

	public void SendMusicBoxRealtimeNotePress(int octave, int key, int instrument, byte type)
	{
	}

	public void SendPoolSyncReady()
	{
	}

	public void SendPlayPoolAgain()
	{
	}

	public void SendUpdatePoolCuePosition()
	{
	}

	public void SendPoolPlaceWhiteBall(Vector2 localPosition)
	{
	}

	public void SendPoolShoot(float deg, float power, byte[] recording_data)
	{
	}

	public void SendExitMinigame()
	{
	}

	public void SendBeginMinigame(string owner, byte response, byte minigame_type, int[] ball_layout)
	{
	}

	public void TryChallengeMinigameOwner(string username, byte minigame_type)
	{
	}

	public void SendMinigameResponse(byte response, string challenger, byte minigame_type)
	{
	}

	public void SendTeleporterScreenshot(string zone, int chunkX, int chunkZ, int innerX, int innerZ, byte[] screenshot_data)
	{
	}

	public void SendPing()
	{
	}

	public void SendNewTeleSearch(string search_term)
	{
	}

	public void SendReleaseInteractingObject()
	{
	}

	public void SendChangeZone(string zone, Vector3 position, bool on_map_change)
	{
	}

	public void SendChangeLandClaimUser(string zone, int chunkX, int chunkZ, int innerX, int innerZ, int user_index, string new_username, string[] mp_cache_keys)
	{
	}

	public void SendClaimObject(string obj_str)
	{
	}

	public void SendBanditFlagDestroyed(string bandit_camp_instance)
	{
	}

	public void SendRequestCurrContainer(chest_request_type chest_request_type_t, string fn_validator)
	{
	}

	public void SendCloseBasket(int basket_id, BasketContents contents, string fn_validator)
	{
	}

	public void SendBuildFurniture(InventoryItem item, byte rot, string zone, int chunkX, int chunkZ, int innerX, int innerZ, string mp_cache_key, string fn_validator)
	{
	}

	public void SendRemoveObject(string zone, int chunkX, int chunkZ, int innerX, int innerZ, ChunkElement element, string mp_cache_key, string fn_validator)
	{
	}

	public void SendReplaceBuildable(InventoryItem new_item, InventoryItem old_element_item, int old_element_rot, string zone, int chunkX, int chunkZ, int innerX, int innerZ, string mp_cache_key, string fn_validator)
	{
	}

	public void SendUpdateParentCreatures()
	{
	}

	public void SendRequestChunkAt(string zone, int chunkX, int chunkZ)
	{
	}

	public void SendLoginAttempt(string random_join_code)
	{
	}

	public void RequestMoreUniqueIds()
	{
	}

	public void SendUsedUniqueId(int unique_id)
	{
	}

	public void SendGameChat(string message)
	{
	}

	private void FixedUpdate()
	{
		if (stream_positions_in_N_frames == 0)
		{
			SendPlayerPosition();
			SendMyMobPositions();
			stream_positions_in_N_frames = 45;
		}
		stream_positions_in_N_frames--;
	}

	public void RequestPageOfTeleportersByPageNumber(int page, bool in_search_page)
	{
	}

	public void RequestPageOfTeleportersByTeleStr(string zone, int chunkX, int chunkZ, int innerX, int innerZ)
	{
	}

	public void RequestTeleporterScreenshot(OnlineTeleporter teleporter)
	{
	}

	public void SendFinishedEditingTeleporter(string title, string description, string zone, int chunkX, int chunkZ, int innerX, int innerZ)
	{
	}

	public void SendPlayerPosition()
	{
	}

	public void RequestZoneData(string zone, ZoneDataControl.change_zone_type type)
	{
	}

	public void RequestZoneData(string zone_name, ZoneDataControl.change_zone_type type, Vector3 position)
	{
	}

	public void StopZoneDataTimeout()
	{
	}

	private IEnumerator ZoneDataTimeout()
	{
		return null;
	}

	public void SendStartTeleport(string tele_str)
	{
	}

	public void SendEndTeleport(Vector3 new_position)
	{
	}

	public void SendSitInChair(string chair_interactable_id)
	{
	}

	public void SendFinishedSittingInChair()
	{
	}

	public void SendGuardDieNotif(string mob_name, string owner_name)
	{
	}

	public void SendShowExpReceive(string text, Vector3 position)
	{
	}

	public void SendUpdateCreatureStats(string combat_id, string fn_validator)
	{
	}

	public void SendIncreaseHp(string combat_id, int amount_inc, string fn_validator)
	{
	}

	public void SendMobDie(string dead_mob_id, float delay, float splat_delay, string origin_zone, int origin_chunkX, int origin_chunkZ, int origin_innerX, int origin_innerZ, int respawn_secs, GameObject killer, byte mob_type, bool darksword_kill, bool aether_banish, InventoryItem original_element_item, string fn_validator)
	{
	}

	public void SendChangeEquipment(byte type, InventoryItem new_item)
	{
	}

	public void SendRespawn()
	{
	}

	public void SendInitialPlayerData()
	{
	}

	public void PackPageOfTeleporters(string user_requesting, int page)
	{
	}

	public void PackPosition(Packet outgoing, Vector3 pos)
	{
	}

	public void PackRotation(Packet outgoing, Quaternion rotation)
	{
	}
}
