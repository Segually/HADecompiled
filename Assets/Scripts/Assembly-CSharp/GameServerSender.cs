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

	public Dictionary<string, CreatureStruct> mobs_I_am_trying_to_claim_awaiting_response = new Dictionary<string, CreatureStruct>();

	public List<string> other_players_mobs_that_I_inquired_about = new List<string>();

	public string packet_validator_code = "";

	public int packet_validator_total_variation;

	public Dictionary<string, ChunkData> cached_chunks = new Dictionary<string, ChunkData>();

	public List<string> chunks_mid_request = new List<string>();

	public bool requesting_unique_ids;

	private int stream_positions_in_N_frames;

	private Vector3 last_nearby_check;

	private IEnumerator zone_data_timeout;

	public Connection connection => GameServerConnector.Instance.game_server_connection;

	public GameServerConnector connector => GameServerConnector.Instance;

	public GameServerReceiver receiver => GameServerReceiver.Instance;

	public GameServerInterface interface_ => GameServerInterface.Instance;

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
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(87);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendCreatedLocalMob(string minion_combat_id)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(89);
		outgoing.PutString(minion_combat_id);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendCreatePerkDrop(Vector3 position, string effect_name, PerkData perk_data, int perk_level, string caster_id, int caster_level, string fn_validator)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(85);
		if (interface_.nearby_players.Count == 0) return;
		if (caster_id == "LOCAL") caster_id = PlayerData.Instance.GetGlobalString("username_lower");
		outgoing.PutString(PacketValidatorVariation());
		PackPosition(outgoing, position);
		outgoing.PutString(effect_name);
		perk_data.PackForWeb(outgoing);
		outgoing.PutShort(perk_level);
		outgoing.PutString(caster_id);
		outgoing.PutLong(caster_level);
		connection.Send(outgoing, Connection.priority.DEFAULT);
		Debug.Log("Sent: Create Drop");
	}

	public void SendLaunchProjectilePerk(PerkData perk_data, int perk_level, string target_id, string caster_id, int caster_level, Vector3 target_pos, Vector3 caster_pos, string fn_validator)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(82);
		if (interface_.nearby_players.Count == 0) return;
		if (caster_id == "LOCAL") caster_id = PlayerData.Instance.GetGlobalString("username_lower");
		if (target_id == "LOCAL") target_id = PlayerData.Instance.GetGlobalString("username_lower");
		outgoing.PutString(PacketValidatorVariation());
		perk_data.PackForWeb(outgoing);
		outgoing.PutShort(perk_level);
		outgoing.PutString(target_id);
		outgoing.PutString(caster_id);
		outgoing.PutLong(caster_level);
		PackPosition(outgoing, target_pos);
		PackPosition(outgoing, caster_pos);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendAllPreAppliedPerks(string send_to_user, string fn_validator)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(84);
		if (GameController.Instance.player == null) return;
		outgoing.PutString(PacketValidatorVariation());
		outgoing.PutString(send_to_user);
		List<DurationEffect> effects = GameController.Instance.player.GetComponent<PerkReceiver>().duration_effects;
		outgoing.PutShort(effects.Count);
		foreach (DurationEffect effect in effects)
		{
			string caster = effect.original_caster_id;
			if (caster == "LOCAL") caster = PlayerData.Instance.GetGlobalString("username_lower");
			outgoing.PutString(caster);
			outgoing.PutLong(effect.original_caster_level);
			effect.perk_data.PackForWeb(outgoing);
			outgoing.PutShort(effect.perk_level);
			outgoing.PutString(effect.effect_name);
			outgoing.PutShort(effect.time_remaining);
			outgoing.PutShort(effect.next_apply);
		}
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendApplyPerk(string effect_name, string target_id, string caster_id, int caster_level, PerkData perk_data, int perk_level, bool on_duration_reapply, string fn_validator)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(81);
		if (caster_id == "LOCAL") caster_id = PlayerData.Instance.GetGlobalString("username_lower");
		if (target_id == "LOCAL") target_id = PlayerData.Instance.GetGlobalString("username_lower");
		outgoing.PutString(PacketValidatorVariation());
		outgoing.PutString(caster_id);
		outgoing.PutLong(caster_level);
		outgoing.PutString(target_id);
		perk_data.PackForWeb(outgoing);
		outgoing.PutShort(perk_level);
		outgoing.PutString(effect_name);
		outgoing.PutByte((byte)(on_duration_reapply ? 1 : 0));
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendQuickTag(byte active, string fn_validator)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(83);
		outgoing.PutString(PacketValidatorVariation());
		outgoing.PutByte(active);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendRenameCompanion(string combat_name, string new_companion_name)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(79);
		outgoing.PutString(combat_name);
		outgoing.PutString(new_companion_name);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendCompanionChangeEquip(string combat_name, InventoryItem hat_, InventoryItem body_, InventoryItem hand_)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(78);
		outgoing.PutString(combat_name);
		hat_.PackForWeb(outgoing);
		body_.PackForWeb(outgoing);
		hand_.PackForWeb(outgoing);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendDestroyCompanion(string combat_name)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(80);
		outgoing.PutString(combat_name);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendDeloadMob(string combat_id)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(64);
		outgoing.PutString(combat_id);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendTryClaimMobs(List<CreatureStruct> attempt_request)
	{
		if (!connector.FullyInGame()) return;
		List<string> ids = new List<string>();
		foreach (CreatureStruct creature in attempt_request)
		{
			if (!mobs_I_am_trying_to_claim_awaiting_response.ContainsKey(creature.combat_name))
			{
				ids.Add(creature.combat_name);
				mobs_I_am_trying_to_claim_awaiting_response.Add(creature.combat_name, creature);
			}
		}
		if (ids.Count == 0) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(63);
		WriteMobIds(ids, outgoing);
		connection.Send(outgoing);
	}

	private void WriteMobIds(List<string> ids, Packet outgoing)
	{
		outgoing.PutByte((byte)ids.Count);
		foreach (string id in ids) outgoing.PutString(id);
	}

	public void UpdateSyncedTargetIds(GameObject caller)
	{
		if (!connector.FullyInGame()) return;
		List<GameObject> targets = caller.GetComponent<CreatureBrain>().GetTargetList();
		string combatId = caller.GetComponent<Combatant>().combat_name;
		if (combatId == "LOCAL") combatId = PlayerData.Instance.GetGlobalString("username_lower");
		Packet outgoing = new Packet();
		outgoing.PutByte(88);
		outgoing.PutString(combatId);
		outgoing.PutByte((byte)targets.Count);
		foreach (GameObject target in targets)
		{
			if (target != null)
			{
				string targetId = target.GetComponent<Combatant>().combat_name;
				if (targetId == "LOCAL") targetId = PlayerData.Instance.GetGlobalString("username_lower");
				outgoing.PutString(targetId);
			}
		}
		connection.Send(outgoing);
	}

	public void PutSingleMobPosition(string creature_id, Packet outgoing)
	{
		Vector3 position = Vector3.zero;
		Vector3 target = Vector3.zero;
		Quaternion rotation = Quaternion.Euler(Vector3.zero);
		if (MobControl.Instance.active_combatants.ContainsKey(creature_id) && MobControl.Instance.active_combatants[creature_id] != null)
		{
			GameObject obj = MobControl.Instance.active_combatants[creature_id];
			position = obj.transform.position;
			target = obj.GetComponent<SharedCreature>().GetMoveTo();
			rotation = obj.GetComponent<SharedCreature>().GetSpotterRotation();
		}
		outgoing.PutString(creature_id);
		PackPosition(outgoing, position);
		PackPosition(outgoing, target);
		PackRotation(outgoing, rotation);
	}

	public bool DontSendToSaveBandwidth()
	{
		return false;
	}

	public void SendMyMobPositions()
	{
		if (!connector.FullyInGame()) return;
		if (MobControl.Instance.my_claimed_creatures.Count == 0 && CompanionController.Instance.active_companions.Count == 0) return;
		if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Game") return;
		Packet outgoing = new Packet();
		outgoing.PutByte(65);
		outgoing.PutByte((byte)(MobControl.Instance.my_claimed_creatures.Count + CompanionController.Instance.active_companions.Count));
		foreach (string id in MobControl.Instance.my_claimed_creatures) PutSingleMobPosition(id, outgoing);
		foreach (ActiveCompanion companion in CompanionController.Instance.active_companions) PutSingleMobPosition(companion.combat_name, outgoing);
		connection.Send(outgoing);
	}

	public void SendHitMob(string defender_combat_id, int real_dmg, int fake_dmg, Combatant.hit_col hit_col, bool missed, bool dodged, GameObject attacker, byte mob_type, string fn_validator)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(71);
		if (defender_combat_id == "LOCAL") defender_combat_id = PlayerData.Instance.GetGlobalString("username_lower");
		string attackerId = "";
		if (attacker != null && attacker.GetComponent<Combatant>() != null)
		{
			attackerId = attacker.GetComponent<Combatant>().combat_name;
			if (attackerId == "LOCAL") attackerId = PlayerData.Instance.GetGlobalString("username_lower");
		}
		outgoing.PutString(PacketValidatorVariation());
		outgoing.PutString(defender_combat_id);
		outgoing.PutLong(real_dmg);
		outgoing.PutLong(fake_dmg);
		outgoing.PutByte((byte)hit_col);
		outgoing.PutByte((byte)(missed ? 1 : 0));
		outgoing.PutByte((byte)(dodged ? 1 : 0));
		outgoing.PutString(attackerId);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	private string PacketValidatorVariation()
	{
		string alphabet = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz";
		int remaining = packet_validator_total_variation;
		char[] code = packet_validator_code.ToCharArray();
		variation_commited_direction[] directions = new variation_commited_direction[packet_validator_code.Length];
		int attempts = 100;
		while (remaining > 0)
		{
			int index = Random.Range(0, packet_validator_code.Length);
			if (directions[index] == variation_commited_direction.none)
			{
				if (code[index] == 'A') directions[index] = variation_commited_direction.up;
				else if (code[index] == 'z') directions[index] = variation_commited_direction.down;
				else directions[index] = Random.value < 0.5f ? variation_commited_direction.up : variation_commited_direction.down;
			}
			if (directions[index] == variation_commited_direction.up && code[index] != 'z')
			{
				code[index] = alphabet[alphabet.IndexOf(code[index]) + 1];
				remaining--;
			}
			else if (directions[index] == variation_commited_direction.down && code[index] != 'A')
			{
				code[index] = alphabet[alphabet.IndexOf(code[index]) - 1];
				remaining--;
			}
			attempts--;
			if (attempts == 0) return "infloop";
		}
		return new string(code);
	}

	public void SendAttackAnimation(string combat_id)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(70);
		outgoing.PutString(combat_id == "LOCAL" ? PlayerData.Instance.GetGlobalString("username_lower") : combat_id);
		connection.Send(outgoing, Connection.priority.LOW);
	}

	public void SendMusicBoxRealtimeNotePress(int octave, int key, int instrument, byte type)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(45);
		outgoing.PutByte(type);
		outgoing.PutShort(octave);
		outgoing.PutShort(key);
		outgoing.PutShort(instrument);
		connection.Send(outgoing, Connection.priority.LOW);
	}

	public void SendPoolSyncReady()
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(59);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendPlayPoolAgain()
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(61);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendUpdatePoolCuePosition()
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(57);
		outgoing.PutLong((int)(PoolGameControl.Instance.GetCueAngle(PoolGameControl.Instance.pool_cue.transform.localPosition) * 100f));
		connection.Send(outgoing, Connection.priority.LOW);
	}

	public void SendPoolPlaceWhiteBall(Vector2 localPosition)
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(60);
		outgoing.PutLong((int)localPosition.x);
		outgoing.PutLong((int)localPosition.y);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendPoolShoot(float deg, float power, byte[] recording_data)
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(58);
		outgoing.PutLong((int)(deg * 100f));
		outgoing.PutShort((float)(int)(power * 100f));
		outgoing.PutLong(recording_data.Length);
		foreach (byte value in recording_data) outgoing.PutByte(value);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendExitMinigame()
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(56);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendBeginMinigame(string owner, byte response, byte minigame_type, int[] ball_layout)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(55);
		outgoing.PutString(owner);
		outgoing.PutByte(response);
		outgoing.PutByte(minigame_type);
		if (ball_layout != null) for (int i = 0; i < 14; i++) outgoing.PutByte((byte)ball_layout[i]);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void TryChallengeMinigameOwner(string username, byte minigame_type)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(53);
		outgoing.PutString(username);
		outgoing.PutByte(minigame_type);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendMinigameResponse(byte response, string challenger, byte minigame_type)
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(54);
		outgoing.PutByte(response);
		outgoing.PutString(challenger);
		outgoing.PutByte(minigame_type);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendTeleporterScreenshot(string zone, int chunkX, int chunkZ, int innerX, int innerZ, byte[] screenshot_data)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(48);
		outgoing.PutString(zone);
		outgoing.PutShort(chunkX);
		outgoing.PutShort(chunkZ);
		outgoing.PutShort(innerX);
		outgoing.PutShort(innerZ);
		outgoing.PutLong(screenshot_data.Length);
		foreach (byte value in screenshot_data) outgoing.PutByte(value);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendPing()
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(1);
		connection.Send(outgoing, Connection.priority.SUPER_HIGH);
	}

	public void SendNewTeleSearch(string search_term)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(52);
		outgoing.PutString(search_term);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendReleaseInteractingObject()
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(40);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendChangeZone(string zone, Vector3 position, bool on_map_change)
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(20);
		outgoing.PutString(zone);
		PackPosition(outgoing, position);
		outgoing.PutShort((short)(on_map_change ? 1 : 0));
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendChangeLandClaimUser(string zone, int chunkX, int chunkZ, int innerX, int innerZ, int user_index, string new_username, string[] mp_cache_keys)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(35);
		outgoing.PutString(zone);
		outgoing.PutShort(chunkX);
		outgoing.PutShort(chunkZ);
		outgoing.PutShort(innerX);
		outgoing.PutShort(innerZ);
		outgoing.PutByte((byte)user_index);
		outgoing.PutString(new_username);
		for (int i = 0; i < 9; i++) outgoing.PutString(mp_cache_keys[i]);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendClaimObject(string obj_str)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(39);
		outgoing.PutString(obj_str);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendBanditFlagDestroyed(string bandit_camp_instance)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(90);
		outgoing.PutString(bandit_camp_instance);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendRequestCurrContainer(chest_request_type chest_request_type_t, string fn_validator)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(26);
		outgoing.PutString(PacketValidatorVariation());
		outgoing.PutLong(GameController.Instance.interacting_element_item.GetLong("basket_id"));
		outgoing.PutByte((byte)chest_request_type_t);
		outgoing.PutString(ChunkControl.Instance.player_zone);
		outgoing.PutShort(GameController.Instance.interacting_element_chunkX);
		outgoing.PutShort(GameController.Instance.interacting_element_chunkZ);
		outgoing.PutShort(GameController.Instance.interacting_element_innerX);
		outgoing.PutShort(GameController.Instance.interacting_element_innerZ);
		connection.Send(outgoing);
		PopupControl.Instance.ShowConnecting("Opening container");
	}

	public void SendCloseBasket(int basket_id, BasketContents contents, string fn_validator)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(30);
		outgoing.PutString(PacketValidatorVariation());
		outgoing.PutLong(basket_id);
		contents.Pack(outgoing);
		outgoing.PutString(GameController.Instance.interacting_element_item.item_name);
		outgoing.PutString(ChunkControl.Instance.player_zone);
		outgoing.PutShort(GameController.Instance.interacting_element_chunkX);
		outgoing.PutShort(GameController.Instance.interacting_element_chunkZ);
		outgoing.PutShort(GameController.Instance.interacting_element_innerX);
		outgoing.PutShort(GameController.Instance.interacting_element_innerZ);
		connection.Send(outgoing);
	}

	public void SendBuildFurniture(InventoryItem item, byte rot, string zone, int chunkX, int chunkZ, int innerX, int innerZ, string mp_cache_key, string fn_validator)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(32);
		outgoing.PutString(PacketValidatorVariation());
		item.PackForWeb(outgoing);
		outgoing.PutByte(rot);
		outgoing.PutString(zone);
		outgoing.PutShort(chunkX);
		outgoing.PutShort(chunkZ);
		outgoing.PutShort(innerX);
		outgoing.PutShort(innerZ);
		outgoing.PutString(mp_cache_key);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendRemoveObject(string zone, int chunkX, int chunkZ, int innerX, int innerZ, ChunkElement element, string mp_cache_key, string fn_validator)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(33);
		outgoing.PutString(PacketValidatorVariation());
		outgoing.PutString(zone);
		outgoing.PutShort(chunkX);
		outgoing.PutShort(chunkZ);
		outgoing.PutShort(innerX);
		outgoing.PutShort(innerZ);
		outgoing.PutByte((byte)element.rot);
		element.item.PackForWeb(outgoing);
		outgoing.PutString(mp_cache_key);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendReplaceBuildable(InventoryItem new_item, InventoryItem old_element_item, int old_element_rot, string zone, int chunkX, int chunkZ, int innerX, int innerZ, string mp_cache_key, string fn_validator)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(34);
		outgoing.PutString(PacketValidatorVariation());
		new_item.PackForWeb(outgoing);
		old_element_item.PackForWeb(outgoing);
		outgoing.PutByte((byte)old_element_rot);
		outgoing.PutString(zone);
		outgoing.PutShort(chunkX);
		outgoing.PutShort(chunkZ);
		outgoing.PutShort(innerX);
		outgoing.PutShort(innerZ);
		outgoing.PutString(mp_cache_key);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendUpdateParentCreatures()
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(25);
		List<string> parents = CreatureMorpher.Instance.GetCulledParentList(GameController.Instance.player_parent_creatures);
		outgoing.PutShort(parents.Count);
		foreach (string parent in parents) outgoing.PutString(parent);
		connection.Send(outgoing);
	}

	public void SendRequestChunkAt(string zone, int chunkX, int chunkZ)
	{
		string key = ChunkControl.Instance.GetChunkString(zone, chunkX, chunkZ);
		if (chunks_mid_request.Contains(key)) return;
		chunks_mid_request.Add(key);
		string item = ZoneDataControl.Instance.curr_zonedata.house_item.item_name;
		byte type = zone == "overworld" ? (byte)0 : InventoryUtils.IsCaveObject(item) ? (byte)1 : InventoryUtils.IsHeavenDimension(item) ? (byte)2 : InventoryUtils.IsHellDimension(item) ? (byte)3 : InventoryUtils.IsPureDimension(item) || item == "Pocket World Basement" ? (byte)4 : (byte)5;
		Packet outgoing = new Packet();
		outgoing.PutByte(12);
		outgoing.PutString(zone);
		outgoing.PutShort(chunkX);
		outgoing.PutShort(chunkZ);
		outgoing.PutByte(type);
		outgoing.PutString(cached_chunks.ContainsKey(key) ? cached_chunks[key].mp_cache_key : "");
		connection.Send(outgoing);
	}

	public void SendLoginAttempt(string random_join_code)
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(38);
		outgoing.PutString(random_join_code);
		outgoing.PutString(PlayerData.Instance.GetGlobalString("username_lower"));
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void RequestMoreUniqueIds()
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(41);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendUsedUniqueId(int unique_id)
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(43);
		outgoing.PutLong(unique_id);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendGameChat(string message)
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(6);
		outgoing.PutString(message);
		connection.Send(outgoing, Connection.priority.HIGH);
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
		Packet outgoing = new Packet();
		outgoing.PutByte(46);
		outgoing.PutByte(0);
		outgoing.PutByte((byte)(in_search_page ? 1 : 0));
		outgoing.PutShort(page);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void RequestPageOfTeleportersByTeleStr(string zone, int chunkX, int chunkZ, int innerX, int innerZ)
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(46);
		outgoing.PutByte(1);
		outgoing.PutString(zone);
		outgoing.PutShort(chunkX);
		outgoing.PutShort(chunkZ);
		outgoing.PutShort(innerX);
		outgoing.PutShort(innerZ);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void RequestTeleporterScreenshot(OnlineTeleporter teleporter)
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(49);
		outgoing.PutString(teleporter.to_zone);
		outgoing.PutShort(teleporter.to_chunkX);
		outgoing.PutShort(teleporter.to_chunkZ);
		outgoing.PutShort(teleporter.to_innerX);
		outgoing.PutShort(teleporter.to_innerZ);
		connection.Send(outgoing, Connection.priority.LOW);
	}

	public void SendFinishedEditingTeleporter(string title, string description, string zone, int chunkX, int chunkZ, int innerX, int innerZ)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(51);
		outgoing.PutString(title);
		outgoing.PutString(description);
		outgoing.PutString(zone);
		outgoing.PutShort(chunkX);
		outgoing.PutShort(chunkZ);
		outgoing.PutShort(innerX);
		outgoing.PutShort(innerZ);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendPlayerPosition()
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(17);
		GameObject player = GameController.Instance.player;
		if (player == null)
		{
			PackPosition(outgoing, GameController.Instance.prev_player_pos);
			PackPosition(outgoing, Vector3.zero);
			PackRotation(outgoing, Quaternion.Euler(Vector3.zero));
			outgoing.PutByte(1);
			last_nearby_check = GameController.Instance.prev_player_pos;
		}
		else
		{
			PackPosition(outgoing, player.transform.position);
			PackPosition(outgoing, player.GetComponent<SharedCreature>().GetMoveTo());
			PackRotation(outgoing, player.GetComponent<SharedCreature>().GetSpotterRotation());
			if (Vector3.Distance(player.transform.position, last_nearby_check) > 13f)
			{
				outgoing.PutByte(1);
				last_nearby_check = player.transform.position;
			}
			else outgoing.PutByte(0);
		}
		connection.Send(outgoing, Connection.priority.HIGH);
	}

	public void RequestZoneData(string zone, ZoneDataControl.change_zone_type type)
	{
		RequestZoneData(zone, type, Vector3.zero);
	}

	public void RequestZoneData(string zone_name, ZoneDataControl.change_zone_type type, Vector3 position)
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(10);
		outgoing.PutString(zone_name);
		outgoing.PutByte((byte)type);
		if ((int)type == 2 || (int)type == 3) PackPosition(outgoing, position);
		connection.Send(outgoing);
		StopZoneDataTimeout();
		zone_data_timeout = ZoneDataTimeout();
		StartCoroutine(zone_data_timeout);
	}

	public void StopZoneDataTimeout()
	{
		if (zone_data_timeout != null) StopCoroutine(zone_data_timeout);
	}

	private IEnumerator ZoneDataTimeout()
	{
		yield return new WaitForSeconds(5f);
		GameplayGUIControl.Instance.ShowNotif("Couldn't enter area", new OnNotifClick(OnNotifClick.type.none));
		TransitionControl.Instance.EndTransitionNow(false);
	}

	public void SendStartTeleport(string tele_str)
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(21);
		outgoing.PutString(tele_str);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendEndTeleport(Vector3 new_position)
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(22);
		PackPosition(outgoing, new_position);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendSitInChair(string chair_interactable_id)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(62);
		outgoing.PutString(chair_interactable_id);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendFinishedSittingInChair()
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(62);
		outgoing.PutString("");
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendGuardDieNotif(string mob_name, string owner_name)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(9);
		outgoing.PutString(mob_name);
		outgoing.PutString(owner_name);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendShowExpReceive(string text, Vector3 position)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(76);
		if (interface_.nearby_players.Count == 0) return;
		outgoing.PutString(text);
		PackPosition(outgoing, position);
		connection.Send(outgoing, Connection.priority.LOW);
	}

	public void SendUpdateCreatureStats(string combat_id, string fn_validator)
	{
		if (!connector.FullyInGame()) return;
		if (!MobControl.Instance.active_combatants.ContainsKey(combat_id)) return;
		GameObject obj = MobControl.Instance.active_combatants[combat_id];
		if (obj == null) return;
		int level = obj.GetComponent<SharedCreature>().level;
		int maxHp = obj.GetComponent<Combatant>().HP_max;
		float hp = obj.GetComponent<Combatant>().hp;
		int regen = obj.GetComponent<SharedCreature>().hp_regen;
		if (combat_id == "LOCAL") combat_id = PlayerData.Instance.GetGlobalString("username_lower");
		Packet outgoing = new Packet();
		outgoing.PutByte(74);
		outgoing.PutString(PacketValidatorVariation());
		outgoing.PutString(combat_id);
		outgoing.PutLong(level);
		outgoing.PutLong(maxHp);
		outgoing.PutLong((int)hp);
		outgoing.PutLong(regen);
		connection.Send(outgoing);
	}

	public void SendIncreaseHp(string combat_id, int amount_inc, string fn_validator)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(75);
		if (combat_id == "LOCAL") combat_id = PlayerData.Instance.GetGlobalString("username_lower");
		outgoing.PutString(PacketValidatorVariation());
		outgoing.PutString(combat_id);
		outgoing.PutLong(amount_inc);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendMobDie(string dead_mob_id, float delay, float splat_delay, string origin_zone, int origin_chunkX, int origin_chunkZ, int origin_innerX, int origin_innerZ, int respawn_secs, GameObject killer, byte mob_type, bool darksword_kill, bool aether_banish, InventoryItem original_element_item, string fn_validator)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(72);
		if (dead_mob_id == "LOCAL") dead_mob_id = PlayerData.Instance.GetGlobalString("username_lower");
		string killerId = "";
		if (killer != null && killer.GetComponent<Combatant>() != null)
		{
			killerId = killer.GetComponent<Combatant>().combat_name;
			if (killerId == "LOCAL") killerId = PlayerData.Instance.GetGlobalString("username_lower");
		}
		outgoing.PutString(PacketValidatorVariation());
		outgoing.PutString(dead_mob_id);
		outgoing.PutShort(delay * 10f);
		outgoing.PutShort(splat_delay * 10f);
		outgoing.PutString(origin_zone);
		outgoing.PutShort(origin_chunkX);
		outgoing.PutShort(origin_chunkZ);
		outgoing.PutShort(origin_innerX);
		outgoing.PutShort(origin_innerZ);
		outgoing.PutShort(respawn_secs);
		outgoing.PutString(killerId);
		outgoing.PutByte(mob_type);
		outgoing.PutByte((byte)(darksword_kill ? 1 : 0));
		outgoing.PutByte((byte)(aether_banish ? 1 : 0));
		original_element_item.PackForWeb(outgoing);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendChangeEquipment(byte type, InventoryItem new_item)
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(24);
		outgoing.PutByte(type);
		new_item.PackForWeb(outgoing);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendRespawn()
	{
		if (!connector.FullyInGame()) return;
		Packet outgoing = new Packet();
		outgoing.PutByte(86);
		outgoing.PutLong(GameController.Instance.playerLevel);
		inventory_ctr.Instance.player_inventory[inventory_ctr.hat_index].item.PackForWeb(outgoing);
		inventory_ctr.Instance.player_inventory[inventory_ctr.body_index].item.PackForWeb(outgoing);
		inventory_ctr.Instance.player_inventory[inventory_ctr.hand_index].item.PackForWeb(outgoing);
		GameObject player = GameController.Instance.player;
		outgoing.PutLong(player == null ? 0 : player.GetComponent<Combatant>().HP_max);
		outgoing.PutLong(player == null ? 0 : (int)player.GetComponent<Combatant>().hp);
		outgoing.PutLong(player == null ? 0 : player.GetComponent<SharedCreature>().hp_regen);
		List<string> parents = CreatureMorpher.Instance.GetCulledParentList(GameController.Instance.player_parent_creatures);
		outgoing.PutShort(parents.Count);
		foreach (string parent in parents) outgoing.PutString(parent);
		connection.Send(outgoing, Connection.priority.DEFAULT);
	}

	public void SendInitialPlayerData()
	{
		Packet outgoing = new Packet();
		outgoing.PutByte(3);
		Vector3 position = connector.is_host ? GameController.Instance.prev_player_pos : GameController.Instance.GetSavedPlayerPositionOnServer(connector.server_name);
		string zone = connector.is_host ? ChunkControl.Instance.player_zone : GameController.Instance.GetSavedPlayerZoneOnServer(connector.server_name);
		PackPosition(outgoing, position);
		outgoing.PutString(zone);
		short alive = PlayerData.Instance.GetSlotShort("PLAYER_ALIVE", (PlayerData.filename_t)0);
		outgoing.PutByte(alive == 2 ? (byte)0 : PlayerData.Instance.GetSlotShort("PLAYER_ALIVE", (PlayerData.filename_t)0) == 0 ? (byte)1 : (byte)2);
		outgoing.PutLong(GameController.Instance.playerLevel);
		inventory_ctr.Instance.player_inventory[inventory_ctr.hat_index].item.PackForWeb(outgoing);
		inventory_ctr.Instance.player_inventory[inventory_ctr.body_index].item.PackForWeb(outgoing);
		inventory_ctr.Instance.player_inventory[inventory_ctr.hand_index].item.PackForWeb(outgoing);
		GameObject player = GameController.Instance.player;
		outgoing.PutLong(player == null ? 0 : player.GetComponent<Combatant>().HP_max);
		outgoing.PutLong(player == null ? 0 : (int)player.GetComponent<Combatant>().hp);
		outgoing.PutLong(player == null ? 0 : player.GetComponent<SharedCreature>().hp_regen);
		List<string> parents = CreatureMorpher.Instance.GetCulledParentList(GameController.Instance.player_parent_creatures);
		outgoing.PutShort(parents.Count);
		foreach (string parent in parents) outgoing.PutString(parent);
		if (connector.is_host)
		{
			outgoing.PutShort(GameController.time_of_day * 1000f);
			if (ChunkControl.Instance.player_zone != "overworld")
			{
				ZoneDataControl.Instance.curr_zonedata.PackForWeb(outgoing);
				List<string> trail = ZoneDataControl.Instance.GetZoneTrail(ChunkControl.Instance.player_zone);
				outgoing.PutShort(trail.Count);
				foreach (string trailZone in trail)
				{
					outgoing.PutString(trailZone);
					ZoneData data = ZoneDataControl.Instance.LoadZoneDataFromDisk(trailZone);
					data.PackForWeb(outgoing);
					data.ClearOutdoorLandClaims();
				}
			}
			WriteMobIds(MobControl.Instance.my_claimed_creatures, outgoing);
		}
		connection.Send(outgoing, Connection.priority.SUPER_HIGH);
	}

	public void PackPageOfTeleporters(string user_requesting, int page)
	{
		int active = CustomTeleporterControl.Instance.GetNumberOfActiveTeleporters();
		Packet outgoing = new Packet();
		outgoing.PutByte(47);
		outgoing.PutString(user_requesting);
		outgoing.PutShort(page);
		int skip = page * 3;
		outgoing.PutByte((byte)(skip + 3 < active ? 1 : 0));
		int total = PlayerData.Instance.GetSlotShort("n_teleporters", (PlayerData.filename_t)4);
		int sent = 0;
		for (int i = 0; i < total; i++)
		{
			string prefix = "teleporter_" + i;
			if (PlayerData.Instance.GetSlotShort(prefix + "_deleted", (PlayerData.filename_t)4) != 0) continue;
			if (skip != 0)
			{
				skip--;
				continue;
			}
			outgoing.PutByte(1);
			string name = PlayerData.Instance.GetSlotString(prefix + "_name", (PlayerData.filename_t)4);
			string description = PlayerData.Instance.GetSlotString(prefix + "_desc", (PlayerData.filename_t)4);
			string zone = PlayerData.Instance.GetSlotString(prefix + "_to_zone", (PlayerData.filename_t)4);
			int x = PlayerData.Instance.GetSlotShort(prefix + "_to_chunkX", (PlayerData.filename_t)4);
			int z = PlayerData.Instance.GetSlotShort(prefix + "_to_chunkZ", (PlayerData.filename_t)4);
			int innerX = PlayerData.Instance.GetSlotShort(prefix + "_to_innerX", (PlayerData.filename_t)4);
			int innerZ = PlayerData.Instance.GetSlotShort(prefix + "_to_innerZ", (PlayerData.filename_t)4);
			outgoing.PutString(name);
			outgoing.PutString(description);
			outgoing.PutString(zone + "," + x + "," + z + "," + innerX + "," + innerZ);
			outgoing.PutString(zone);
			outgoing.PutShort(x);
			outgoing.PutShort(z);
			outgoing.PutShort(innerX);
			outgoing.PutShort(innerZ);
			sent++;
			if (sent == 3) break;
		}
		for (int i = sent; i < 3; i++) outgoing.PutByte(0);
		connection.Send(outgoing);
	}

	public void PackPosition(Packet outgoing, Vector3 pos)
	{
		Vector3 chunk = ChunkControl.Instance.GetChunkCoords(pos);
		outgoing.PutShort(chunk.x);
		outgoing.PutShort(chunk.z);
		outgoing.PutShort((pos.x - chunk.x * 10f) * 10f);
		outgoing.PutShort((pos.z - chunk.z * 10f) * 10f);
	}

	public void PackRotation(Packet outgoing, Quaternion rotation)
	{
		outgoing.PutShort(rotation.x * 100f);
		outgoing.PutShort(rotation.y * 100f);
		outgoing.PutShort(rotation.z * 100f);
		outgoing.PutShort(rotation.w * 100f);
	}
}
