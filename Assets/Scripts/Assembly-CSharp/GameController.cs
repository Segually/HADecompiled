using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour, OrderedStart
{
	private enum background_type_t
	{
		breeder = 0,
		NPC_background = 1,
		explore_background = 2
	}

	public static GameController Instance;

	public static float time_of_day = 0.35f;

	private IEnumerator track_distance;

	public AudioClip[] sfx_flesh_impact;

	public AudioClip[] sfx_ore_impact;

	public AudioClip[] sfx_creatures;

	public AudioClip[] sfx_player;

	public AudioClip sfx_got_level;

	public AudioClip sfx_exp_get;

	public AudioClip sfx_got_level_b;

	public AudioClip sfx_levelButton;

	public AudioClip sfx_death;

	public AudioClip sfx_gamestart;

	public AudioClip sfx_wrongpass;

	public AudioClip sfx_typewriter;

	public AudioClip sfx_chestopen;

	public AudioClip sfx_landclaim;

	public AudioClip sfx_sign;

	public AudioClip[] sfx_breeder_hits;

	public AudioClip sfx_mixMutants;

	public AudioClip sfx_craft;

	public AudioClip sfx_sell;

	public AudioClip sfx_buy;

	public AudioClip sfx_relax;

	public AudioClip sfx_crackShell;

	public GameObject type_healthbar;

	public GameObject type_creature;

	public GameObject type_expParticle;

	public GameObject type_creatureLevelDisplay;

	public GameObject type_levelMeter_nib;

	public GameObject type_friendly_levelup_particles;

	public GameObject type_NPC_overhead;

	public Sprite[] hit_sprite_frames;

	public Texture2D target_circle_blue;

	public Texture2D target_circle_red;

	public Texture2D target_circle_yellow;

	public Sprite sprite_splatHit;

	public Color[] nibColors;

	public Plane plane = new Plane(Vector3.up, Vector3.zero);

	public GameObject[] stat_meters;

	public AudioSource sfx;

	public GameObject breeder_floor_plane;

	public GameObject scenic_elevator;

	public GameObject targeted_circle_graphic;

	public GameObject mainCamera;

	public GameObject player;

	public bool targetShowing;

	public int currentEXP;

	public int playerLevel;

	public int skillPointsSpendable;

	public List<string> player_parent_creatures;

	private bool pause = true;

	public int[] player_stats;

	public bool level_up_animation_playing;

	public bool lock_levelup_text;

	public float visualEXP;

	public float cam_offset;

	public bool animate_exp_bar;

	public Gradient day_night_colors;

	public Gradient day_night_SKY;

	public Gradient day_night_exploreBG;

	public Gradient torch_cols;

	public Light directional_light;

	private Quaternion curr_light_angle;

	private IEnumerator daynight_cycle_t;

	public List<GameObject> torch_glows = new List<GameObject>();

	private background_type_t background_type;

	public static int n_stats = 8;

	public GameObject curr_pickup_overhead;

	public GameObject splash_prefab;

	public GameObject splash_lava_prefab;

	public bool perk_get_animation_playing;

	private int levelups_since_ad;

	private int rapid_click_count;

	private bool shown_rapid_click_display;

	public bool casting_projectile_at_enemy;

	public bool casting_projectile_at_ally;

	public bool picking_cast_custom_location;

	public bool is_picking_companion_target;

	public bool is_picking_companion_walk_location;

	public int interacting_element_chunkX;

	public int interacting_element_chunkZ;

	public int interacting_element_innerX;

	public int interacting_element_innerZ;

	public InventoryItem interacting_element_item = new InventoryItem("");

	public int interacting_element_rot;

	public static float lowest_player_y = -3f;

	public List<GameObject> possible_destroy = new List<GameObject>();

	private int sound_creature_hit_iterator;

	private int sound_player_hit_iterator;

	public bool click_being_held;

	public bool processed_click_start;

	public bool processed_click_release = true;

	public bool resume_input_on_next_click;

	public Vector3 previous_step_clicked_at;

	public GameObject no_fall_thru_floor;

	private Vector3 prev_no_fall_thru_floor_pos;

	public Vector3 prev_player_pos;

	private float eagle_view_mod = 1f;

	private float prev_eagle_view_mod = 1f;

	public float creature_scale_view_mod = 1f;

	public Vector3 cam_angle = new Vector3(5f, 15f, -5f);

	public GameObject nearest_giant;

	public void Start_0()
	{
		Instance = this;
		visualEXP = 0f;
		playerLevel = 0;
		skillPointsSpendable = 0;
		currentEXP = 0;
		player_stats = new int[n_stats];
		for (int i = 0; i < n_stats; i++)
		{
			player_stats[i] = 0;
		}
		player_parent_creatures = new List<string>();
	}

	public void Start_1()
	{
		PopupControl.Instance.GetComponent<Canvas>().worldCamera = ShopControl.Instance.GetComponent<Camera>();
		PopupControl.Instance.black_background.GetComponent<Canvas>().worldCamera = ShopControl.Instance.GetComponent<Camera>();
		cam_offset = Mathf.Lerp(0.59f, 0.44f, Mathf.InverseLerp(1.33f, 2f, (float)Screen.width / (float)Screen.height));
	}

	public void MakeAlliesSwarm(GameObject to_attack, bool companions, bool wolf_pack, bool companions_overwrite_target)
	{
		if (to_attack == null)
		{
			return;
		}
		foreach (KeyValuePair<string, GameObject> active_combatant in MobControl.Instance.active_combatants)
		{
			GameObject value = active_combatant.Value;
			if (value.GetComponent<Combatant>().mob_type != Combatant.TYPE_T.creature)
			{
				continue;
			}
			SharedCreature component = value.GetComponent<SharedCreature>();
			if (!component.is_local_mob)
			{
				continue;
			}
			if (component.brain_type == SharedCreature.brain_type_t.companion && companions)
			{
				if (companions_overwrite_target)
				{
					value.GetComponent<CreatureBrainCompanion>().Swarm(to_attack);
				}
				else if (value.GetComponent<CreatureBrain>().main_target == null)
				{
					value.GetComponent<CreatureBrainCompanion>().Swarm(to_attack);
				}
			}
			else if (component.brain_type == SharedCreature.brain_type_t.wolf_pack && wolf_pack)
			{
				value.GetComponent<CreatureBrainWolfPack>().Swarm(to_attack);
			}
		}
	}

	public void PlaySignSound()
	{
		sfx.PlayOneShot(sfx_sign, AudioControl.Instance.general_sfx_volume * 0.75f);
	}

	public bool is_paused()
	{
		if (!GameServerConnector.Instance.FullyInGame())
		{
			if (GameServerConnector.Instance.MidConnect())
			{
				return true;
			}
			return pause;
		}
		return false;
	}

	public void SnapCam(float dist = 0.65f)
	{
		Vector3 vector = ((!(ChunkControl.Instance.follow_obj != null)) ? prev_player_pos : ChunkControl.Instance.follow_obj.transform.position);
		mainCamera.transform.position = vector + get_cam_offset() * dist;
		mainCamera.transform.LookAt(vector);
	}

	private float calculate_cam_offset(float ratio)
	{
		return Mathf.Lerp(0.59f, 0.44f, Mathf.InverseLerp(1.33f, 2f, ratio));
	}

	public void SetLightAngleToOverworld()
	{
		curr_light_angle = Quaternion.Euler(52f, 0f, 0f);
		directional_light.intensity = 1f;
		directional_light.shadowStrength = 0.5f;
	}

	public void SetLightAngleToIndoors()
	{
		curr_light_angle = Quaternion.Euler(90f, 0f, 0f);
		directional_light.intensity = 0.8f;
		directional_light.shadowStrength = 0.4f;
	}

	public void SetLightAngleToDadAnimal()
	{
		curr_light_angle = Quaternion.Euler(52f, 51f, 0f);
		directional_light.intensity = 1f;
		directional_light.shadowStrength = 0.8f;
	}

	public void SetLightAngleToMomAnimal()
	{
		curr_light_angle = Quaternion.Euler(52f, 101f, 0f);
		directional_light.intensity = 1f;
		directional_light.shadowStrength = 0.8f;
	}

	public void SetLightAngleToResultAnimal()
	{
		curr_light_angle = Quaternion.Euler(52f, 0f, 0f);
		directional_light.intensity = 1f;
		directional_light.shadowStrength = 0.6f;
	}

	public void start_daynight_cycle()
	{
		if (daynight_cycle_t != null)
		{
			StopCoroutine(daynight_cycle_t);
		}
		daynight_cycle_t = daynight_cycle();
		StartCoroutine(daynight_cycle_t);
	}

	public void stop_daynight_cycle()
	{
		if (daynight_cycle_t != null)
		{
			StopCoroutine(daynight_cycle_t);
		}
		EvalDaynight();
	}

	private IEnumerator daynight_cycle()
	{
		while (true)
		{
			time_of_day += 0.002f;
			if (time_of_day > 1f)
			{
				time_of_day = 0f;
			}
			EvalDaynight();
			yield return new WaitForSeconds(0.9f);
		}
	}

	public void SetBackground_Breeder()
	{
		background_type = background_type_t.breeder;
		Camera.main.backgroundColor = new Color(1f, 1f, 1f, 1f);
	}

	public void SetBackground_NPC()
	{
		background_type = background_type_t.NPC_background;
		EvalDaynight();
	}

	public void SetBackground_Explore()
	{
		background_type = background_type_t.explore_background;
		EvalDaynight();
	}

	private void RedrawTorches(float progress)
	{
		if (!GraphicsControl.Instance.ShowLightingEffects())
		{
			return;
		}
		Color color = torch_cols.Evaluate(progress);
		List<GameObject> list = new List<GameObject>();
		foreach (GameObject torch_glow in torch_glows)
		{
			if (!(torch_glow == null))
			{
				Color color2 = torch_glow.GetComponent<MeshRenderer>().material.color;
				torch_glow.GetComponent<MeshRenderer>().material.color = new Color(color2.r, color2.g, color2.b, color.a);
				list.Add(torch_glow);
			}
		}
		torch_glows = list;
	}

	public void EvalDaynight()
	{
		float num = time_of_day;
		if (BreedControl.Instance.state_t == BreedControl.state.select_father || BreedControl.Instance.state_t == BreedControl.state.select_mother)
		{
			num = 0.35f;
		}
		num = Mathf.Clamp01(num);
		short num2 = ZoneDataControl.Instance.curr_zonedata.house_item.GetShort("depth");
		if (InventoryUtils.IsCaveObject(ZoneDataControl.Instance.curr_zonedata.house_item.item_name) || InventoryUtils.IsHellDimension(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
		{
			RedrawTorches(0.72f);
			Camera.main.backgroundColor = new Color(0.11f, 0.11f, 0.11f, 1f);
			return;
		}
		if (ZoneDataControl.Instance.curr_zonedata.house_item.item_name == "Pocket World Basement")
		{
			RedrawTorches(0.72f);
			Color color = day_night_colors.Evaluate(0.5f);
			RenderSettings.ambientLight = color;
			directional_light.color = color;
			return;
		}
		Color color2;
		if (InventoryUtils.IsHouseObject(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
		{
			color2 = day_night_colors.Evaluate(0.5f);
		}
		else
		{
			color2 = day_night_colors.Evaluate(num);
			if (num2 >= 1 && num2 <= 5)
			{
				color2 = Color.Lerp(color2, Color.black, ((float)num2 - 1f) * 0.25f);
			}
			else if (num2 >= 6 && num2 <= 8)
			{
				color2 = Color.black;
			}
			else if (num2 >= 9)
			{
				color2 = Color.red;
			}
		}
		RedrawTorches(num);
		RenderSettings.ambientLight = color2;
		directional_light.color = color2;
		if (InventoryUtils.IsHeavenDimension(ZoneDataControl.Instance.curr_zonedata.house_item.item_name) || InventoryUtils.IsPureDimension(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
		{
			Camera.main.backgroundColor = day_night_SKY.Evaluate(num);
		}
		else if (ChunkControl.Instance.player_zone != "overworld")
		{
			if (num2 >= 1 && num2 <= 5)
			{
				Camera.main.backgroundColor = Color.Lerp(new Color(0.11f, 0.11f, 0.11f, 1f), Color.black, ((float)num2 - 1f) * 0.25f);
			}
			else if (num2 >= 6 && num2 <= 8)
			{
				Camera.main.backgroundColor = Color.black;
			}
			else if (num2 >= 9)
			{
				Camera.main.backgroundColor = Color.red;
			}
			else
			{
				Camera.main.backgroundColor = new Color(0.11f, 0.11f, 0.11f, 1f);
			}
		}
		else if (background_type == background_type_t.explore_background || background_type == background_type_t.NPC_background)
		{
			Camera.main.backgroundColor = day_night_SKY.Evaluate(num);
		}
	}

	public void ResetVariables()
	{
		level_up_animation_playing = false;
		lock_levelup_text = false;
		targetShowing = false;
	}

	public void PressOptionRevive()
	{
		WindowPrefabsControl.Instance.DestroyScreen("You Died");
		WindowPrefabsControl.Instance.DestroyScreen("You Died - bottom left");
		short globalShort = PlayerData.Instance.GetGlobalShort("GEMS");
		if (globalShort > 1)
		{
			PlayerData.Instance.SetGlobalShort("GEMS", globalShort - 2);
			ReviveAccepted();
			return;
		}
		AudioControl.Instance.PlayGenericClick();
		GameplayGUIControl.Instance.HideGameplayGui();
		Instance.DestroyAllOverheads();
		string text = TranslationControl.Instance.TranslateGeneral("OOPS!", "Market");
		string text2 = TranslationControl.Instance.TranslateGeneral("You do not have enough Gems.", "Market");
		string text3 = TranslationControl.Instance.TranslateGeneral("Get more gems?", "Market");
		ShopControl.Instance.ShowShopPopup("YES", ShopControl.button_color_t.yes_green, "CANCEL", ShopControl.button_color_t.no_red, false, "<color=#00aaff>" + text + "</color> " + text2 + "\n" + text3, new Color(1f, 1f, 1f, 1f), ShopControl.Instance.purchase_sad, new Color(1f, 1f, 1f, 1f), null, new Color(1f, 1f, 1f, 1f), new Color(0.29803923f, 0.50980395f, 0.5294118f, 1f), ShopControl.popup_context.revive_get_gems_yes_no);
	}

	public void PressOptionRespawn()
	{
		PopupControl.Instance.on_yes_pressed = delegate
		{
			Instance.Respawn_Accepted();
		};
		PopupControl.Instance.ShowYesNo("Are you sure?", "Yes", "No", (PopupControl.context)13);
	}

	public void PressOptionRestart()
	{
		PopupControl.Instance.on_yes_pressed = delegate
		{
			Instance.RestartAccepted();
		};
		PopupControl.Instance.ShowYesNo("Are you sure?\n<color=#ff2222>You will start over from Level 1 and lose anything you built.</color>\n\n(Note: you'll keep all your Gems and Purchases, but not companions)", "Yes", "No", (PopupControl.context)13);
	}

	public void revive_gems_accept()
	{
		AudioControl.Instance.PlayGenericClick();
		WindowPrefabsControl.Instance.CreateScreen("Shop-getgems", WindowPrefabsControl.build_into_t.GAME_CTR);
		WindowControl.Instance.OpenWindow(WindowControl.window_type_t.buy_gems_revive);
	}

	public void revive_cancel()
	{
		WindowPrefabsControl.Instance.CreateScreen("You Died", WindowPrefabsControl.build_into_t.GAME_CTR);
		WindowPrefabsControl.Instance.GetScreen("You Died").GetComponent<Animation>().Play("new death anm 2");
		WindowPrefabsControl.Instance.CreateScreen("You Died - bottom left", WindowPrefabsControl.build_into_t.GAME_CTR);
	}

	public void ReviveAccepted()
	{
		string player_zone = ChunkControl.Instance.player_zone;
		Vector3 start_position = Instance.prev_player_pos;
		ResetGame(false);
		BreedControl.Instance.gameObject.SetActive(true);
		BreedControl.Instance.SkipBreedScreen(false, player_zone, start_position, null, true);
		SnapCam(0f);
		pause = false;
		GameplayGUIControl.Instance.ShowGameplayGui();
		if (QuestControl.Instance.quest_perk_reapply_key != "")
		{
			PerkData perk_data = PerkControl.Instance.ClonePerkForCasting(QuestControl.Instance.quest_perk_reapply_key, Instance.player);
			PerkControl.Instance.ApplyInitialCastOnto(perk_data, 1, "LOCAL", 1, Instance.player);
		}
		PlayerData.Instance.SetSlotShort("PLAYER_ALIVE", 1, PlayerData.filename_t.general);
	}

	public void Respawn_Accepted()
	{
		ResetGame(true);
		BreedControl.Instance.gameObject.SetActive(true);
		int num = (int)((float)playerLevel * 0.13f);
		int num2 = playerLevel - num;
		if (num2 < 2)
		{
			num2 = 1;
		}
		OverwritePlayerLevel(num2, "");
		SavePlayerLevelToDisk();
		currentEXP = 0;
		SaveCurrentExpToDisk();
		int num3 = skillPointsSpendable;
		visualEXP = 0f;
		animate_exp_bar = true;
		if (num3 < num)
		{
			skillPointsSpendable = 0;
			SaveSkillPointsSpendableToDisk();
			for (int i = 0; i < num - num3; i++)
			{
				int num4 = Random.Range(0, n_stats);
				player_stats[num4] = Mathf.Max(player_stats[num4] - 1, 0);
			}
			SaveAllStatsToDisk();
			GameplayGUIControl.Instance.RedrawAllStatNibs();
		}
		else
		{
			skillPointsSpendable = num3 - num;
			SaveSkillPointsSpendableToDisk();
		}
		inventory_ctr.Instance.ClearInventory(true);
		BreedControl instance = BreedControl.Instance;
		instance.SkipBreedScreen(false, "overworld", instance.campos_result.position, null, true);
		SnapCam(0f);
		pause = false;
		GameplayGUIControl.Instance.ShowGameplayGui();
		PlayerData.Instance.SetSlotShort("PLAYER_ALIVE", 1, PlayerData.filename_t.general);
	}

	public void RestartAccepted()
	{
		PopupControl.Instance.ShowConnecting("Deleting (0%)", (PopupControl.context)14);
		StartCoroutine(AsyncRestartGame());
	}

	private IEnumerator AsyncRestartGame()
	{
		yield return new WaitForSeconds(0.1f);
		PlayerData.Instance.DeleteSlot(PlayerData.Instance.SLOT, RestartComplete);
	}

	private void RestartComplete()
	{
		if (GameServerConnector.Instance.FullyInGame())
		{
			GameServerConnector.Instance.dont_goto_menu_on_connect = true;
			GameServerConnector.Instance.Disconnect();
		}
		SceneManager.LoadScene("Game");
	}

	public void ResetGame(bool end_curr_quest)
	{
		WindowPrefabsControl.Instance.DestroyScreen("You Died");
		WindowPrefabsControl.Instance.DestroyScreen("You Died - bottom left");
		AudioControl.Instance.PlayGenericClick();
		level_up_animation_playing = false;
		levelups_since_ad = 0;
		CompanionController.Instance.ClearTrailNodes();
		if (end_curr_quest)
		{
			CompanionController.Instance.DestroyTempCompanions();
			QuestControl.Instance.RevertQuestIfNecessary();
			AudioControl.Instance.EndBattleMusic();
		}
		DestroyAllOverheads();
		ChunkControl.Instance.DestroyAllTerrain();
		MobControl.Instance.ClearAllCreatures(false, true, true);
		Instance.GiveAllOverheads();
	}

	public void EnableElevator(bool true_or_false)
	{
		scenic_elevator.SetActive(true_or_false);
		ShopControl.Instance.breeder_base.SetActive(true_or_false);
	}

	public void showOverheadNotif(string str, Vector3 position, bool sound, bool delete_on_many)
	{
		if (sound)
		{
			sfx.PlayOneShot(sfx_exp_get, AudioControl.Instance.general_sfx_volume * 0.7f);
		}
		GameObject gameObject = Object.Instantiate(type_expParticle);
		gameObject.transform.SetParent(MobControl.Instance.gameObject.transform);
		gameObject.transform.SetAsFirstSibling();
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one * 0.61f;
		gameObject.transform.localPosition = Vector2.zero;
		gameObject.GetComponent<ExpGainParticle>().Init(position, Camera.main, str);
		possible_destroy.Add(gameObject);
		gameObject.GetComponent<Animation>().Stop();
		gameObject.GetComponent<Animation>().PlayQueued("expText");
		if (delete_on_many)
		{
			if (curr_pickup_overhead != null)
			{
				Object.Destroy(curr_pickup_overhead);
			}
			curr_pickup_overhead = gameObject;
		}
	}

	public void CreatePlayer(GameObject creatureObj, Vector3 startPosition, bool on_breeder)
	{
		MobControl.Instance.SpawnMainPlayer(creatureObj, startPosition + Vector3.up * SharedCreature.H, "LOCAL");
		if (on_breeder)
		{
			creatureObj.transform.localPosition = Vector3.up * creatureObj.GetComponent<LiteModel>().original.height;
		}
		GameplayGUIControl.Instance.DrawComboText();
		track_distance = TRACK_DISTANCE();
		StartCoroutine(track_distance);
	}

	public static string first_upper(string input)
	{
		return (input[0].ToString() ?? "").ToUpper() + input.Substring(1, input.Length - 1);
	}

	public void giant_shake_screen()
	{
		mainCamera.GetComponent<Animation>().Stop();
		mainCamera.GetComponent<Animation>().Play();
	}

	public void PlayerDied()
	{
		PlayerData.Instance.SetSlotShort("PLAYER_ALIVE", 2, PlayerData.filename_t.general);
		if (track_distance != null)
		{
			StopCoroutine(track_distance);
		}
		PerkReceiver component = player.GetComponent<PerkReceiver>();
		foreach (DurationEffect duration_effect in component.duration_effects)
		{
			component.OnDurationEffectRemoved(duration_effect);
		}
		player = null;
		HideTargetCircle();
		if (ConstructionControl.Instance.done_button_context != ConstructionControl.button_state.none)
		{
			ConstructionControl.Instance.DonePlacing(true, true);
		}
		GameplayGUIControl.Instance.HideGameplayGui();
		ShowDeathScreen("new death anm");
	}

	public void ShowDeathScreen(string animation_name)
	{
		WindowPrefabsControl.Instance.CreateScreen("You Died", WindowPrefabsControl.build_into_t.GAME_CTR);
		UnityEngine.UI.Text textLegacy = WindowPrefabsControl.Instance.GetTextLegacy("You Died", "text_revive_title");
		UnityEngine.UI.Text textLegacy2 = WindowPrefabsControl.Instance.GetTextLegacy("You Died", "text_revive_desc");
		UnityEngine.UI.Text textLegacy3 = WindowPrefabsControl.Instance.GetTextLegacy("You Died", "text_respawn_title");
		UnityEngine.UI.Text textLegacy4 = WindowPrefabsControl.Instance.GetTextLegacy("You Died", "text_respawn_desc");
		UnityEngine.UI.Text textLegacy5 = WindowPrefabsControl.Instance.GetTextLegacy("You Died", "text_restart_title");
		UnityEngine.UI.Text textLegacy6 = WindowPrefabsControl.Instance.GetTextLegacy("You Died", "text_restart_desc");
		textLegacy.text = TranslationControl.Instance.TranslateGeneral("REVIVE", "Market");
		textLegacy2.text = TranslationControl.Instance.TranslateGeneral("You keep everything!", "Market");
		textLegacy3.text = TranslationControl.Instance.TranslateGeneral("RESPAWN", "Market");
		if ((int)((float)playerLevel * 0.13f) != 0)
		{
			textLegacy4.text = TranslationControl.Instance.TranslateGeneral("You lose the items in your inventory, and lose 999 levels.", "Market").Replace("999", ((int)((float)playerLevel * 0.13f)).ToString() ?? "");
		}
		else
		{
			textLegacy4.text = TranslationControl.Instance.TranslateGeneral("You lose the items in your inventory.", "Market");
		}
		textLegacy5.text = TranslationControl.Instance.TranslateGeneral("RESTART", "Market");
		textLegacy6.text = TranslationControl.Instance.TranslateGeneral("Start a new game, with a new creature.", "Market");
		Animation component = WindowPrefabsControl.Instance.GetScreen("You Died").GetComponent<Animation>();
		component.Stop();
		component.Play(animation_name);
	}

	public float DepthAt(Vector3 position)
	{
		if (player != null)
		{
			return Vector3.Distance(position, BreedControl.Instance.campos_result.position);
		}
		return 0f;
	}

	private IEnumerator TRACK_DISTANCE()
	{
		while (true)
		{
			yield return new WaitForSeconds(0.25f);
			GameplayGUIControl.Instance.UpdateDistanceDisplay();
		}
	}

	public void sound_crackshell()
	{
		sfx.PlayOneShot(sfx_crackShell, AudioControl.Instance.general_sfx_volume * 0.4f);
	}

	public void sound_mixmutants()
	{
		sfx.PlayOneShot(sfx_mixMutants, AudioControl.Instance.general_sfx_volume);
	}

	public void sound_death()
	{
		sfx.PlayOneShot(sfx_death, AudioControl.Instance.general_sfx_volume);
	}

	public void sound_ding()
	{
		sfx.PlayOneShot(sfx_craft, AudioControl.Instance.general_sfx_volume * 0.5f);
	}

	public void sound_sell()
	{
		sfx.PlayOneShot(sfx_sell, AudioControl.Instance.general_sfx_volume * 0.6f);
	}

	public void sound_buy()
	{
		sfx.PlayOneShot(sfx_buy, AudioControl.Instance.general_sfx_volume * 0.75f);
	}

	public void sound_levelButton()
	{
		sfx.PlayOneShot(sfx_levelButton, AudioControl.Instance.general_sfx_volume);
		AudioControl.Instance.PlayGenericClick();
	}

	public void sound_relax()
	{
		sfx.PlayOneShot(sfx_relax, AudioControl.Instance.general_sfx_volume * 0.31f);
	}

	public void level_up_meter_pressed(int index)
	{
		string random_fn_validator = "";
		if (!perk_get_animation_playing && skillPointsSpendable >= 1)
		{
			skillPointsSpendable--;
			SaveSkillPointsSpendableToDisk();
			GameplayGUIControl.Instance.RedrawSkillPointsSpendableText();
			sound_levelButton();
			stat_meters[index].GetComponent<Animation>().Play();
			int num = ++player_stats[index];
			SaveAllStatsToDisk();
			GameplayGUIControl.Instance.CreateStatNib(index);
			GameplayGUIControl.Instance.RedrawStatName(index, num);
			Instance.player.GetComponent<SharedCreature>().ReCalcHpMaxAndHpRegen();
			GameServerSender.Instance.SendUpdateCreatureStats("LOCAL", random_fn_validator);
			if (num != 0 && num % 6 == 0)
			{
				perk_get_animation_playing = true;
				StartCoroutine(perk_get_display(index));
			}
		}
	}

	private void show_get_perk(int index)
	{
		StartCoroutine(perk_get_display(index));
	}

	private IEnumerator perk_get_display(int index)
	{
		PerkControl.Instance.SoundMaxout();
		for (int j = 0; j < 2; j++)
		{
			for (int i = 0; i < 6; i++)
			{
				GameplayGUIControl.Instance.instantiated_stat_nibs[index][i].GetComponent<Animation>().Stop();
				GameplayGUIControl.Instance.instantiated_stat_nibs[index][i].GetComponent<Animation>().Play();
				yield return new WaitForSeconds(0.03f);
			}
			yield return new WaitForSeconds(0.05f);
		}
		Color gem_col = GameplayGUIControl.Instance.instantiated_stat_nibs[index][0].GetComponent<UnityEngine.UI.Image>().color;
		for (float i2 = 0f; i2 < 10f; i2 += 1f)
		{
			foreach (GameObject item in GameplayGUIControl.Instance.instantiated_stat_nibs[index])
			{
				item.transform.localPosition = Vector3.Lerp(item.transform.localPosition, Vector2.zero, i2 * 0.125f);
			}
			yield return new WaitForEndOfFrame();
		}
		foreach (GameObject item2 in GameplayGUIControl.Instance.instantiated_stat_nibs[index])
		{
			Object.Destroy(item2);
		}
		GameplayGUIControl.Instance.instantiated_stat_nibs[index].Clear();
		stat_meters[index].GetComponent<Animation>().Play();
		GetComponent<PerkControl>().ShowPerkGet(gem_col);
	}

	public void PressViewAchievements()
	{
		PopupControl.Instance.SetButtonWasPressed();
		QuestControl.Instance.OpenQuestWindow();
	}

	public void AttemptAdOnLevelup()
	{
		levelups_since_ad++;
		if (levelups_since_ad > 3)
		{
			levelups_since_ad = 0;
			AdvertControl.Instance.TryShowInterstitialAd(AdvertControl.ad_context.after_few_levelups);
		}
	}

	public void level_up_screen_close()
	{
		if (!perk_get_animation_playing && !(player == null))
		{
			PopupControl.Instance.SetButtonWasPressed();
			TryLevelAchieves();
			HideLevelScreen();
			pause = false;
			AudioControl.Instance.PlayGenericClick();
		}
	}

	public void TryLevelAchieves()
	{
		int num = playerLevel;
		if (num >= 3)
		{
			AchievesControl.Instance.UnlockAchievement("Better, Faster, Stronger");
			if (num >= 15)
			{
				AchievesControl.Instance.UnlockAchievement("Mighty Creature");
			}
			if (num >= 25)
			{
				AchievesControl.Instance.UnlockAchievement("Survival of the Fittest");
			}
			if (num >= 50)
			{
				AchievesControl.Instance.UnlockAchievement("King of the Wild");
			}
			if (num >= 100)
			{
				AchievesControl.Instance.UnlockAchievement("Ultimate Species");
			}
		}
	}

	public void HideLevelScreen()
	{
		WindowControl.Instance.GetComponent<Animation>().Stop();
		WindowControl.Instance.GetComponent<Animation>().Play("levelup hide");
		level_up_animation_playing = false;
	}

	public void edit_navpost()
	{
		WindowControl.Instance.OpenMiniwindow(WindowControl.miniwindow_type_t.edit_navpost);
		WindowControl.Instance.HideMiniwindowHeaders();
		WindowPrefabsControl.Instance.CreateScreen("NAV POST", WindowPrefabsControl.build_into_t.mini_window);
		WindowPrefabsControl.Instance.GetObject("NAV POST", "item_spr").GetComponent<ItemSprite>().RedrawBasic(new InventoryItem("Navpost"), 1);
	}

	public void edit_merchantSign()
	{
		WindowControl.Instance.OpenMiniwindow(WindowControl.miniwindow_type_t.edit_merchant_sign);
		WindowControl.Instance.HideMiniwindowHeaders();
		MerchantSignEditor.Instance = WindowPrefabsControl.Instance.CreateScreen("MERCHANT SIGN", WindowPrefabsControl.build_into_t.mini_window).GetComponent<MerchantSignEditor>();
		MerchantSignEditor.Instance.item_spr.RedrawBasic(new InventoryItem("Merchant Sign"), 1);
	}

	public void NoteInteractingElement(int chunkX, int chunkZ, int innerX, int innerZ, InventoryItem item, int rot)
	{
		interacting_element_chunkX = chunkX;
		interacting_element_chunkZ = chunkZ;
		interacting_element_innerX = innerX;
		interacting_element_innerZ = innerZ;
		interacting_element_item = item;
		interacting_element_rot = rot;
	}

	public void ForgetInteractingElement()
	{
	}

	public void PressEndSitInChair()
	{
		PopupControl.Instance.SetButtonWasPressed();
		GameplayGUIControl.Instance.end_sit_button.SetActive(false);
		GameServerSender.Instance.SendFinishedSittingInChair();
		player.GetComponent<SharedCreature>().EndSittingInChair();
	}

	public void player_interact(GameObject interaction_target)
	{
		if (interaction_target == null)
		{
			return;
		}
		if (interaction_target.GetComponent<Collectible>() != null)
		{
			interaction_target.GetComponent<Collectible>().OnCollectLocal();
		}
		else if (interaction_target.GetComponent<Interactable>() != null)
		{
			if (!WindowControl.Instance.CanOpenGenericWindow())
			{
				return;
			}
			Interactable component = interaction_target.GetComponent<Interactable>();
			if (component.replace_for != null)
			{
				interaction_target = component.replace_for;
				component = interaction_target.GetComponent<Interactable>();
			}
			Transform transform = component.transform;
			if (interaction_target.name == "Interactable")
			{
				transform = transform.parent;
			}
			Vector3 position = transform.position;
			string player_zone = ChunkControl.Instance.player_zone;
			Vector3 inner = ChunkControl.Instance.GetInner(position);
			int num = (int)inner.x;
			int num2 = (int)inner.z;
			Vector3 chunkCoords = ChunkControl.Instance.GetChunkCoords(position);
			int num3 = (int)chunkCoords.x;
			int num4 = (int)chunkCoords.z;
			string chunkString = ChunkControl.Instance.GetChunkString(position);
			bool flag = false;
			bool flag2 = false;
			if (component.corresponding_item != null)
			{
				flag = DevBuildControl.Instance.IsLockedByDev(component.corresponding_item);
				flag2 = inventory_ctr.Instance.is_locked_by_player(component.corresponding_item);
			}
			if (component.is_house_exit)
			{
				TransitionControl.Instance.BeginExitHouseTransition();
			}
			else
			{
				string item_name = component.corresponding_item.item_name;
				if (InventoryUtils.UsesShackId(item_name))
				{
					int @long = component.corresponding_item.GetLong("shack_id");
					if (flag)
					{
						string @string = component.corresponding_item.GetString("quest_key_req");
						if (Startup.StringNullOrWhitespace(@string))
						{
							if (component.item_name == "Underground Room" || component.item_name == "Upstairs Room")
							{
								GameplayGUIControl.Instance.ShowNotif("Room locked.", DevBuildControl.Instance.lockedhome_notif, new OnNotifClick(OnNotifClick.type.none));
							}
							else if (component.item_name == "Magic Bean" || InventoryUtils.IsCaveObject(component.item_name))
							{
								GameplayGUIControl.Instance.ShowNotif("Area locked.", DevBuildControl.Instance.lockedhome_notif, new OnNotifClick(OnNotifClick.type.none));
							}
							else
							{
								GameplayGUIControl.Instance.ShowNotif("House locked.", DevBuildControl.Instance.lockedhome_notif, new OnNotifClick(OnNotifClick.type.none));
							}
							goto IL_end;
						}
						ExtraInventoryData extraInventoryData = new ExtraInventoryData();
						extraInventoryData.SetString("key_name", @string);
						InventoryItem inventoryItem = new InventoryItem("Key", extraInventoryData);
						if (!inventory_ctr.Instance.HasItem(inventoryItem))
						{
							PopupControl.Instance.ShowMessage("To open this, you'll need\n<color=#ff0000>" + inventory_ctr.Instance.GetFullItemName(inventoryItem) + "</color>", PopupControl.context.message, inventoryItem);
							goto IL_end;
						}
						inventory_ctr.Instance.player_inventory.RemoveItemExact(inventoryItem, 1);
						PopupControl.Instance.ShowMessage("Opened with\n<color=#69caff>" + inventory_ctr.Instance.GetFullItemName(inventoryItem) + "</color>", PopupControl.context.message, inventoryItem);
					}
					if (flag2)
					{
						interacting_element_chunkX = num3;
						interacting_element_chunkZ = num4;
						interacting_element_innerX = num;
						interacting_element_innerZ = num2;
						interacting_element_item = component.corresponding_item;
						interacting_element_rot = component.temp_rot;
						LockControl.Instance.OpenLockScreen("Enter password to unlock", LockControl.lock_context.unlock_house);
					}
					else
					{
						TransitionControl.Instance.BeginEnterHouseTransition("shack" + @long);
					}
				}
				else
				{
					string text = "";
					switch (item_name)
					{
					case "Trophy":
					{
						string string2 = component.corresponding_item.GetString("trophy_name");
						string string3 = component.corresponding_item.GetString("trophy_reason");
						string string4 = component.corresponding_item.GetString("trophy_from");
						string string5 = component.corresponding_item.GetString("trophy_for");
						string string6 = component.corresponding_item.GetString("trophy_date");
						string string7 = component.corresponding_item.GetString("trophy_VALIDATOR");
						if (string7 != FriendServerInterface.Instance.GetTrophyValidatorString(string2, string3, component.corresponding_item.GetString("paint"), string4, string5, string6))
						{
							GameplayGUIControl.Instance.ShowNotif("Hacked Trophy", new OnNotifClick(OnNotifClick.type.none));
							return;
						}
						if (!WindowControl.Instance.CanOpenGenericWindow())
						{
							return;
						}
						WindowControl.Instance.DoOpenGenericWindow();
						Dictionary<int, Dictionary<string, object>> dictionary = new Dictionary<int, Dictionary<string, object>>();
						int num5 = 0;
						if (!Startup.StringNullOrWhitespace(string2))
						{
							Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
							DialogueControl.Instance.AddImportant("It's a " + string2 + "!", string2 + "!", dictionary2);
							dictionary2.Add("go_to", num5 + 1);
							dictionary.Add(num5, dictionary2);
							num5++;
						}
						if (!Startup.StringNullOrWhitespace(string3))
						{
							Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
							dictionary3.Add("type", "NPC_speak");
							dictionary3.Add("text", "\"" + string3 + "\"");
							dictionary3.Add("go_to", num5 + 1);
							dictionary.Add(num5, dictionary3);
							num5++;
						}
						if (!Startup.StringNullOrWhitespace(string5))
						{
							Dictionary<string, object> dictionary4 = new Dictionary<string, object>();
							dictionary4.Add("type", "NPC_speak");
							dictionary4.Add("text", "For: " + string5);
							dictionary4.Add("go_to", num5 + 1);
							dictionary.Add(num5, dictionary4);
							num5++;
						}
						if (!Startup.StringNullOrWhitespace(string4))
						{
							Dictionary<string, object> dictionary5 = new Dictionary<string, object>();
							dictionary5.Add("type", "NPC_speak");
							dictionary5.Add("text", "From: " + string4);
							dictionary5.Add("go_to", num5 + 1);
							dictionary.Add(num5, dictionary5);
							num5++;
						}
						if (!Startup.StringNullOrWhitespace(string6))
						{
							Dictionary<string, object> dictionary6 = new Dictionary<string, object>();
							dictionary6.Add("type", "NPC_speak");
							dictionary6.Add("text", "Awarded on: " + string6);
							dictionary6.Add("go_to", num5 + 1);
							dictionary.Add(num5, dictionary6);
							num5++;
						}
						Dictionary<string, object> dictionary7 = new Dictionary<string, object>();
						dictionary7.Add("type", "MY_options");
						dictionary7.Add("optionA", "DONE");
						dictionary7.Add("optionA_goto", -1);
						dictionary.Add(num5, dictionary7);
						interacting_element_chunkX = num3;
						interacting_element_chunkZ = num4;
						interacting_element_innerX = num;
						interacting_element_innerZ = num2;
						interacting_element_item = component.corresponding_item;
						interacting_element_rot = component.temp_rot;
						DialogueControl.Instance.SetFocusNpc(interaction_target, string2, "", DialogueControl.focus_type_t.trophy);
						DialogueControl.Instance.EnterDialogue(dictionary, 0, "");
						break;
					}
					case "Painting":
					{
						if (!WindowControl.Instance.CanOpenGenericWindow())
						{
							return;
						}
						WindowControl.Instance.DoOpenGenericWindow();
						Dictionary<int, Dictionary<string, object>> dictionary8 = new Dictionary<int, Dictionary<string, object>>();
						string text2 = "";
						short @short = component.corresponding_item.GetShort("dev_painting_id");
						string text3;
						string text4;
						string text5;
						if (@short == 0)
						{
							text3 = component.corresponding_item.GetString("painting_name");
							text4 = component.corresponding_item.GetString("painting_desc");
							text5 = component.corresponding_item.GetString("painting_creator");
							text2 = component.corresponding_item.GetString("painting_date");
						}
						else
						{
							text3 = DevBuildControl.Instance.NPC_paintings[@short].name;
							text5 = DevBuildControl.Instance.NPC_paintings[@short].creator;
							text4 = DevBuildControl.Instance.NPC_paintings[@short].desc;
						}
						int num6 = 0;
						if (text3 != "")
						{
							Dictionary<string, object> dictionary9 = new Dictionary<string, object>();
							DialogueControl.Instance.AddImportant(text3, text3, dictionary9);
							dictionary9.Add("go_to", num6 + 1);
							dictionary8.Add(num6, dictionary9);
							num6++;
						}
						if (text4 != "")
						{
							Dictionary<string, object> dictionary10 = new Dictionary<string, object>();
							dictionary10.Add("type", "NPC_speak");
							dictionary10.Add("text", "\"" + text4 + "\"");
							dictionary10.Add("go_to", num6 + 1);
							dictionary8.Add(num6, dictionary10);
							num6++;
						}
						if (!Startup.StringNullOrWhitespace(text5))
						{
							Dictionary<string, object> dictionary11 = new Dictionary<string, object>();
							dictionary11.Add("type", "NPC_speak");
							dictionary11.Add("text", "Created by: " + text5);
							dictionary11.Add("go_to", num6 + 1);
							dictionary8.Add(num6, dictionary11);
							num6++;
						}
						if (text2 != "")
						{
							Dictionary<string, object> dictionary12 = new Dictionary<string, object>();
							dictionary12.Add("type", "NPC_speak");
							dictionary12.Add("text", "Created on: " + text2);
							dictionary12.Add("go_to", num6 + 1);
							dictionary8.Add(num6, dictionary12);
							num6++;
						}
						Dictionary<string, object> dictionary13 = new Dictionary<string, object>();
						dictionary13.Add("type", "MY_options");
						dictionary13.Add("optionA", "DONE");
						dictionary13.Add("optionA_goto", -1);
						dictionary8.Add(num6, dictionary13);
						interacting_element_chunkX = num3;
						interacting_element_chunkZ = num4;
						interacting_element_innerX = num;
						interacting_element_innerZ = num2;
						interacting_element_item = component.corresponding_item;
						interacting_element_rot = component.temp_rot;
						DialogueControl.Instance.SetFocusNpc(interaction_target, "Painting", "", DialogueControl.focus_type_t.painting);
						DialogueControl.Instance.EnterDialogue(dictionary8, 0, "");
						if (GameServerConnector.Instance.FullyInGame())
						{
							GameServerReceiver.Instance.ShowReportObjectButton("Report Painting", GameServerReceiver.Instance.EncodeObjectIntoReportData(player_zone, num3, num4, num, num2, component.corresponding_item));
						}
						break;
					}
					case "Chair":
					case "Bed":
					case "Sofa Chair":
					case "Metal Chair":
					case "Stump Chair":
					case "Throne":
					case "Park Bench":
						if (!player.GetComponent<SharedCreature>().snapped_to_chair_obj)
						{
							if (item_name == "Bed" || item_name == "Sofa Chair" || item_name == "Throne")
							{
								sound_relax();
							}
							player.GetComponent<SharedCreature>().TrySitInChairObj(component.active_obj_str);
							GameplayGUIControl.Instance.end_sit_button.SetActive(true);
							GameServerSender.Instance.SendSitInChair(component.active_obj_str);
						}
						break;
					case "Navpost":
					{
						string string8 = component.corresponding_item.GetString("sign_text");
						if (Startup.StringNullOrWhitespace(string8))
						{
							interacting_element_chunkX = num3;
							interacting_element_chunkZ = num4;
							interacting_element_innerX = num;
							interacting_element_innerZ = num2;
							interacting_element_item = component.corresponding_item;
							interacting_element_rot = component.temp_rot;
							edit_navpost();
						}
						else
						{
							string string9 = component.corresponding_item.GetString("text_col");
							GameplayGUIControl.Instance.ShowNotif(ConstructionControl.Instance.FormatNavpostString(string8, string9), new InventoryItem("Navpost"), 1, new OnNotifClick(OnNotifClick.type.none));
						}
						break;
					}
					case "Merchant Sign":
					{
						string string10 = component.corresponding_item.GetString("sign_header");
						string string11 = component.corresponding_item.GetString("sign_text");
						if (string11 == "" && string10 == "")
						{
							interacting_element_chunkX = num3;
							interacting_element_chunkZ = num4;
							interacting_element_innerX = num;
							interacting_element_innerZ = num2;
							interacting_element_item = component.corresponding_item;
							interacting_element_rot = component.temp_rot;
							edit_merchantSign();
							break;
						}
						PopupControl.Instance.ShowMessage(string10 + "\n<size=17><color=#aaaaaa>\"" + string11 + "\"</color></size>", PopupControl.context.sign_etc);
						PlaySignSound();
						if (GameServerConnector.Instance.FullyInGame())
						{
							GameServerReceiver.Instance.ShowReportObjectButton("Report Sign", GameServerReceiver.Instance.EncodeObjectIntoReportData(player_zone, num3, num4, num, num2, component.corresponding_item));
						}
						break;
					}
					case "Sign":
					case "Gravestone":
					{
						string string12 = component.corresponding_item.GetString("sign_text");
						if (!Startup.StringNullOrWhitespace(string12))
						{
							PopupControl.Instance.ShowMessage("<size=17><color=#bbbbbb>The sign says:</color></size>\n" + string12, PopupControl.context.sign_etc);
							PlaySignSound();
							if (GameServerConnector.Instance.FullyInGame())
							{
								GameServerReceiver.Instance.ShowReportObjectButton("Report Sign", GameServerReceiver.Instance.EncodeObjectIntoReportData(player_zone, num3, num4, num, num2, component.corresponding_item));
							}
						}
						else
						{
							interacting_element_chunkX = num3;
							interacting_element_chunkZ = num4;
							interacting_element_innerX = num;
							interacting_element_innerZ = num2;
							interacting_element_item = component.corresponding_item;
							interacting_element_rot = component.temp_rot;
							PopupControl.Instance.ShowMessage("<color=#a5a5a5>[Click to add text]</color>", PopupControl.context.message);
						}
						break;
					}
					case "Bonsai Tree":
					{
						int num7 = component.corresponding_item.GetShort("bonsai_age");
						PopupControl.Instance.ShowMessage("This Bonsai Tree is <color=#ffe747>" + num7 + "</color> years old!", PopupControl.context.message, component.corresponding_item);
						break;
					}
					case "Painting Easel":
						OpenPaintingScreen();
						break;
					case "Crafting Table":
						inventory_ctr.Instance.OpenInventoryAndCrafting(inventory_ctr.Instance.GetCraftList("Crafting - Crafting Table"), component.corresponding_item);
						break;
					case "Anvil":
						inventory_ctr.Instance.OpenInventoryAndCrafting(inventory_ctr.Instance.GetCraftList("Crafting - Anvil"), component.corresponding_item);
						break;
					case "Cauldron":
						inventory_ctr.Instance.OpenInventoryAndCrafting(inventory_ctr.Instance.GetCraftList("Crafting - Cauldron"), component.corresponding_item);
						break;
					case "Loom":
						inventory_ctr.Instance.OpenInventoryAndCrafting(inventory_ctr.Instance.GetCraftList("Crafting - Loom"), component.corresponding_item);
						break;
					case "Stamp Maker":
						inventory_ctr.Instance.OpenInventoryAndCrafting(inventory_ctr.Instance.GetCraftList("Crafting - Stamp Maker"), component.corresponding_item);
						break;
					case "Paint Mixer":
						inventory_ctr.Instance.OpenInventoryAndCrafting(inventory_ctr.Instance.GetCraftList("Crafting - Paint Mixer"), component.corresponding_item);
						break;
					case "Paint Shaker":
						inventory_ctr.Instance.OpenInventoryAndCrafting(inventory_ctr.Instance.GetCraftList("Crafting - Paint Shaker"), component.corresponding_item);
						break;
					case "Oven":
						inventory_ctr.Instance.OpenInventoryAndCrafting(inventory_ctr.Instance.GetCraftList("Crafting - Oven"), component.corresponding_item);
						break;
					case "Campfire":
						inventory_ctr.Instance.OpenInventoryAndCrafting(inventory_ctr.Instance.GetCraftList("Crafting - Campfire"), component.corresponding_item);
						break;
					case "Crucible":
						inventory_ctr.Instance.OpenInventoryAndCrafting(inventory_ctr.Instance.GetCraftList("Crafting - Crucible"), component.corresponding_item);
						break;
					case "Teleporter":
						Instance.interacting_element_chunkX = num3;
						Instance.interacting_element_chunkZ = num4;
						Instance.interacting_element_innerX = num;
						Instance.interacting_element_innerZ = num2;
						Instance.interacting_element_item = component.corresponding_item;
						Instance.interacting_element_rot = component.temp_rot;
						CustomTeleporterControl.Instance.InteractWithTeleporter(interaction_target, chunkString, player_zone, num3, num4, num, num2);
						break;
					case "Companion":
						text = player_zone + "," + num3 + "," + num4 + "," + num + "," + num2;
						if (GameServerInterface.Instance.AnyoneUsing(text))
						{
							PopupControl.Instance.ShowMessage("Can't talk with NPC\nSomeone else is talking with them right now!", PopupControl.context.message);
							break;
						}
						OnInteractWithCompanion(player_zone, num3, num4, num, num2);
						GameServerSender.Instance.SendClaimObject(text);
						if (GameServerConnector.Instance.FullyInGame())
						{
							GameServerReceiver.Instance.ShowReportObjectButton("Report Companion", GameServerReceiver.Instance.EncodeObjectIntoReportData(player_zone, num3, num4, num, num2, component.corresponding_item));
						}
						break;
					case "DEBUG-npc":
						text = player_zone + "," + num3 + "," + num4 + "," + num + "," + num2;
						if (GameServerInterface.Instance.AnyoneUsing(text))
						{
							PopupControl.Instance.ShowMessage("Can't talk with NPC\nSomeone else is talking with them right now!", PopupControl.context.message);
							break;
						}
						OnInteractWithNPC(player_zone, num3, num4, num, num2);
						GameServerSender.Instance.SendClaimObject(text);
						break;
					case "Custom Statue":
						text = player_zone + "," + num3 + "," + num4 + "," + num + "," + num2;
						if (GameServerInterface.Instance.AnyoneUsing(text))
						{
							PopupControl.Instance.ShowMessage("Can't use Statue\nSomeone else is using that right now!", PopupControl.context.message);
							break;
						}
						OnInteractWithStatue(player_zone, num3, num4, num, num2);
						GameServerSender.Instance.SendClaimObject(text);
						if (GameServerConnector.Instance.FullyInGame())
						{
							GameServerReceiver.Instance.ShowReportObjectButton("Report Statue", GameServerReceiver.Instance.EncodeObjectIntoReportData(player_zone, num3, num4, num, num2, component.corresponding_item));
						}
						break;
					case "Karaoke":
						KaraokeControl.is_tweeto_version = false;
						text = player_zone + "," + num3 + "," + num4 + "," + num + "," + num2;
						if (GameServerInterface.Instance.AnyoneUsing(text))
						{
							string text6 = "";
							foreach (KeyValuePair<string, OnlinePlayer> nearby_player in GameServerInterface.Instance.nearby_players)
							{
								if (nearby_player.Value.currently_using == text)
								{
									text6 = nearby_player.Value.username_lower;
									break;
								}
							}
							if (text6 != "")
							{
								PopupControl.Instance.ShowConnecting("Entering minigame");
								GameServerSender.Instance.TryChallengeMinigameOwner(text6, 1);
							}
							else
							{
								PopupControl.Instance.ShowMessage("Can't use Karaoke\nSomeone else is using that!", PopupControl.context.message);
							}
							break;
						}
						interacting_element_chunkX = num3;
						interacting_element_chunkZ = num4;
						interacting_element_innerX = num;
						interacting_element_innerZ = num2;
						interacting_element_item = component.corresponding_item;
						interacting_element_rot = component.temp_rot;
						OpenKaraoke();
						GameServerSender.Instance.SendClaimObject(text);
						break;
					case "Pool Table":
						text = player_zone + "," + num3 + "," + num4 + "," + num + "," + num2;
						if (GameServerInterface.Instance.AnyoneUsing(text))
						{
							string text7 = "";
							foreach (KeyValuePair<string, OnlinePlayer> nearby_player2 in GameServerInterface.Instance.nearby_players)
							{
								if (nearby_player2.Value.currently_using == text)
								{
									text7 = nearby_player2.Value.username_lower;
									break;
								}
							}
							if (text7 != "")
							{
								PopupControl.Instance.ShowConnecting("Entering minigame");
								GameServerSender.Instance.TryChallengeMinigameOwner(text7, 0);
							}
							else
							{
								PopupControl.Instance.ShowMessage("Can't use Pool Table\nSomeone else is using that!", PopupControl.context.message);
							}
							break;
						}
						interacting_element_chunkX = num3;
						interacting_element_chunkZ = num4;
						interacting_element_innerX = num;
						interacting_element_innerZ = num2;
						interacting_element_item = component.corresponding_item;
						interacting_element_rot = component.temp_rot;
						OpenPoolTable("", PoolGameControl.GetRandomBallLayout());
						GameServerSender.Instance.SendClaimObject(text);
						break;
					case "Trading Table":
						text = player_zone + "," + num3 + "," + num4 + "," + num + "," + num2;
						if (GameServerInterface.Instance.AnyoneUsing(text))
						{
							string text8 = "";
							foreach (KeyValuePair<string, OnlinePlayer> nearby_player3 in GameServerInterface.Instance.nearby_players)
							{
								if (nearby_player3.Value.currently_using == text)
								{
									text8 = nearby_player3.Value.username_lower;
									break;
								}
							}
							if (text8 != "")
							{
								PopupControl.Instance.ShowConnecting("Entering Trading Table");
								GameServerSender.Instance.TryChallengeMinigameOwner(text8, 2);
							}
							else
							{
								PopupControl.Instance.ShowMessage("Can't use Trading Table\nSomeone else is using that!", PopupControl.context.message);
							}
							break;
						}
						if (flag)
						{
							GameplayGUIControl.Instance.ShowNotif(component.item_name + " locked.", DevBuildControl.Instance.lockedhome_notif, new OnNotifClick(OnNotifClick.type.none));
							break;
						}
						interacting_element_chunkX = num3;
						interacting_element_chunkZ = num4;
						interacting_element_innerX = num;
						interacting_element_innerZ = num2;
						interacting_element_item = component.corresponding_item;
						interacting_element_rot = component.temp_rot;
						inventory_ctr.Instance.SucceedOpenTradingTable();
						GameServerSender.Instance.SendClaimObject(text);
						break;
					case "Admin Land Claim":
					case "Old Land Claim":
					case "3-day Land Claim":
					case "8-day Land Claim":
						if (flag)
						{
							GameplayGUIControl.Instance.ShowNotif("Land Claim locked.", DevBuildControl.Instance.lockedhome_notif, new OnNotifClick(OnNotifClick.type.none));
							break;
						}
						text = player_zone + "," + num3 + "," + num4 + "," + num + "," + num2;
						if (GameServerInterface.Instance.AnyoneUsing(text))
						{
							PopupControl.Instance.ShowMessage("Can't use Land Claim\nSomeone else is using that!", PopupControl.context.message);
							break;
						}
						interacting_element_chunkX = num3;
						interacting_element_chunkZ = num4;
						interacting_element_innerX = num;
						interacting_element_innerZ = num2;
						interacting_element_item = component.corresponding_item;
						interacting_element_rot = component.temp_rot;
						LandClaimControl.Instance.OpenLandClaimScreen(component.item_name, player_zone, num3, num4, num, num2);
						GameServerSender.Instance.SendClaimObject(text);
						break;
					case "Vending Machine":
						text = player_zone + "," + num3 + "," + num4 + "," + num + "," + num2;
						if (flag)
						{
							GameplayGUIControl.Instance.ShowNotif("Vending Machine locked.", DevBuildControl.Instance.lockedhome_notif, new OnNotifClick(OnNotifClick.type.none));
							break;
						}
						if (GameServerInterface.Instance.AnyoneUsing(text))
						{
							PopupControl.Instance.ShowMessage("Can't use Vending Machine\nSomeone else is using that!", PopupControl.context.message);
							break;
						}
						WindowControl.Instance.OpenMiniwindow(WindowControl.miniwindow_type_t.vending_machine);
						WindowControl.Instance.HideMiniwindowHeaders();
						VendingMachineControl.Instance = WindowPrefabsControl.Instance.CreateScreen("VENDING MACHINE", WindowPrefabsControl.build_into_t.mini_window).GetComponent<VendingMachineControl>();
						interacting_element_chunkX = num3;
						interacting_element_chunkZ = num4;
						interacting_element_innerX = num;
						interacting_element_innerZ = num2;
						interacting_element_item = component.corresponding_item;
						interacting_element_rot = component.temp_rot;
						VendingMachineControl.Instance.OnOpen();
						GameServerSender.Instance.SendClaimObject(text);
						break;
					case "Music Box":
						text = player_zone + "," + num3 + "," + num4 + "," + num + "," + num2;
						if (flag)
						{
							GameplayGUIControl.Instance.ShowNotif("Music Box locked.", DevBuildControl.Instance.lockedhome_notif, new OnNotifClick(OnNotifClick.type.none));
							break;
						}
						if (flag2)
						{
							interacting_element_chunkX = num3;
							interacting_element_chunkZ = num4;
							interacting_element_innerX = num;
							interacting_element_innerZ = num2;
							interacting_element_item = component.corresponding_item;
							interacting_element_rot = component.temp_rot;
							LockControl.Instance.OpenLockScreen("Enter password to unlock", LockControl.lock_context.unlock_musicbox);
							GameServerSender.Instance.SendClaimObject(text);
							break;
						}
						if (GameServerInterface.Instance.AnyoneUsing(text))
						{
							PopupControl.Instance.ShowMessage("Can't use Music Box\nSomeone else is using that!", PopupControl.context.message);
							break;
						}
						interacting_element_chunkX = num3;
						interacting_element_chunkZ = num4;
						interacting_element_innerX = num;
						interacting_element_innerZ = num2;
						interacting_element_item = component.corresponding_item;
						interacting_element_rot = component.temp_rot;
						MusicBoxControl.Instance.OpenMusicBox(component.corresponding_item, position);
						GameServerSender.Instance.SendClaimObject(text);
						break;
					case "Gold Chest":
					case "Basket":
					case "Loot Chest":
					case "Sky Chest":
					case "Chest":
					case "Loot Basket":
					case "Titanium Chest":
					case "Cave Chest":
					case "Crate":
					case "Cave Basket":
					case "Armor Display":
					case "Boss Chest":
					case "Wisdom Chest":
					case "Large Weapon Display":
					case "Egg Fuser":
					case "Weapon Display":
					case "Double Crate":
						text = player_zone + "," + num3 + "," + num4 + "," + num + "," + num2;
						if (GameServerInterface.Instance.AnyoneUsing(text))
						{
							PopupControl.Instance.ShowMessage("Can't open\nSomeone else is using that!", PopupControl.context.message);
							break;
						}
						if (item_name == "Basket" || item_name == "Loot Basket" || item_name == "Boss Chest" || item_name == "Chest" || item_name == "Egg Fuser" || item_name == "Weapon Display" || item_name == "Large Weapon Display" || item_name == "Armor Display" || (item_name == "Crate" && component.corresponding_item.GetString("tag") == "dev_obj") || (item_name == "Double Crate" && component.corresponding_item.GetString("tag") == "dev_obj") || item_name == "Trading Table")
						{
							if (flag)
							{
								string string13 = component.corresponding_item.GetString((item_name == "Boss Chest") ? "bandit_camp_instance" : "quest_key_req");
								if (Startup.StringNullOrWhitespace(string13))
								{
									GameplayGUIControl.Instance.ShowNotif(component.item_name + " locked.", DevBuildControl.Instance.lockedhome_notif, new OnNotifClick(OnNotifClick.type.none));
									break;
								}
								ExtraInventoryData extraInventoryData2 = new ExtraInventoryData();
								extraInventoryData2.SetString("key_name", string13);
								InventoryItem inventoryItem2 = new InventoryItem("Key", extraInventoryData2);
								string fullItemName = inventory_ctr.Instance.GetFullItemName(inventoryItem2);
								if (!inventory_ctr.Instance.HasItem(inventoryItem2))
								{
									PopupControl.Instance.ShowMessage("To open this, you'll need\n<color=#ff0000>" + fullItemName + "</color>", PopupControl.context.message, inventoryItem2);
									break;
								}
								inventory_ctr.Instance.player_inventory.RemoveItemExact(inventoryItem2, 1);
								PopupControl.Instance.ShowMessage("Opened with\n<color=#69caff>" + fullItemName + "</color>", PopupControl.context.message, inventoryItem2);
							}
							else if (flag2)
							{
								interacting_element_chunkX = num3;
								interacting_element_chunkZ = num4;
								interacting_element_innerX = num;
								interacting_element_innerZ = num2;
								interacting_element_item = component.corresponding_item;
								interacting_element_rot = component.temp_rot;
								LockControl.Instance.OpenLockScreen("Enter password to unlock", LockControl.lock_context.unlock_container);
								GameServerSender.Instance.SendClaimObject(text);
								break;
							}
						}
						interacting_element_chunkX = num3;
						interacting_element_chunkZ = num4;
						interacting_element_innerX = num;
						interacting_element_innerZ = num2;
						interacting_element_item = component.corresponding_item;
						interacting_element_rot = component.temp_rot;
						inventory_ctr.Instance.TryOpenWorldContainer(component.corresponding_item, component.temp_rot, num, num2, num3, num4);
						GameServerSender.Instance.SendClaimObject(text);
						break;
					}
				}
			}
		}
		IL_end:
		HideTargetCircle();
		resume_input_on_next_click = true;
	}

	private void OnInteractWithStatue(string zone, int chunkX, int chunkZ, int innerX, int innerZ)
	{
		string chunkString = ChunkControl.Instance.GetChunkString(zone, chunkX, chunkZ);
		if (!ChunkControl.Instance.IsChunkFullyLoadedOrMidload(chunkString))
		{
			return;
		}
		GameObject buildableInstanceByName = ChunkControl.Instance.GetChunkObj(chunkString).GetBuildableInstanceByName(innerX, innerZ, "Custom Statue");
		if (buildableInstanceByName == null)
		{
			return;
		}
		Interactable component = buildableInstanceByName.GetComponent<Interactable>();
		if (WindowControl.Instance.CanOpenGenericWindow())
		{
			WindowControl.Instance.DoOpenGenericWindow();
			string text = component.corresponding_item.GetString("statue_message1");
			string text2 = component.corresponding_item.GetString("statue_message2");
			if (Startup.StringNullOrWhitespace(text) && Startup.StringNullOrWhitespace(text2))
			{
				text = TranslationControl.Instance.TranslateGeneral(CompanionController.default_statue_message1, "CompanionsEtc");
				text2 = TranslationControl.Instance.TranslateGeneral(CompanionController.default_statue_message2, "CompanionsEtc");
			}
			Dictionary<int, Dictionary<string, object>> dictionary = new Dictionary<int, Dictionary<string, object>>();
			dictionary.Add(0, new Dictionary<string, object>
			{
				{ "type", "NPC_speak" },
				{ "text", text },
				{ "go_to", 1 }
			});
			dictionary.Add(1, new Dictionary<string, object>
			{
				{ "type", "NPC_speak" },
				{ "text", text2 },
				{ "go_to", 2 }
			});
			dictionary.Add(2, new Dictionary<string, object>
			{
				{ "type", "MY_options" },
				{ "optionA", "EDIT STATUE" },
				{ "optionA_goto", -77 }
			});
			interacting_element_innerX = innerX;
			interacting_element_chunkX = chunkX;
			interacting_element_chunkZ = chunkZ;
			interacting_element_innerZ = innerZ;
			interacting_element_item = component.corresponding_item;
			interacting_element_rot = (byte)component.temp_rot;
			DialogueControl.Instance.SetFocusNpc(component.gameObject, "Custom Statue", "", DialogueControl.focus_type_t.stationary_npc);
			DialogueControl.Instance.EnterDialogue(dictionary, 0, "");
		}
	}

	private void OnInteractWithCompanion(string zone, int chunkX, int chunkZ, int innerX, int innerZ)
	{
		string chunkString = ChunkControl.Instance.GetChunkString(zone, chunkX, chunkZ);
		if (!ChunkControl.Instance.IsChunkFullyLoadedOrMidload(chunkString))
		{
			return;
		}
		GameObject buildableInstanceByName = ChunkControl.Instance.GetChunkObj(chunkString).GetBuildableInstanceByName(innerX, innerZ, "Companion");
		if (buildableInstanceByName == null)
		{
			return;
		}
		Interactable component = buildableInstanceByName.GetComponent<Interactable>();
		if (!WindowControl.Instance.CanOpenGenericWindow())
		{
			return;
		}
		WindowControl.Instance.DoOpenGenericWindow();
		Dictionary<int, Dictionary<string, object>> dictionary = new Dictionary<int, Dictionary<string, object>>();
		List<string> list = new List<string>();
		string text = component.corresponding_item.GetString("companion_mode");
		if (text == "wait")
		{
			string text2 = component.corresponding_item.GetString("wait_message1");
			string text3 = component.corresponding_item.GetString("wait_message2");
			string text4 = component.corresponding_item.GetString("wait_message3");
			string text5 = component.corresponding_item.GetString("wait_message4");
			if (!Startup.StringNullOrWhitespace(text2))
			{
				list.Add(text2);
			}
			if (!Startup.StringNullOrWhitespace(text3))
			{
				list.Add(text3);
			}
			if (!Startup.StringNullOrWhitespace(text4))
			{
				list.Add(text4);
			}
			if (!Startup.StringNullOrWhitespace(text5))
			{
				list.Add(text5);
			}
		}
		else if (text == "merchant")
		{
			string text6 = component.corresponding_item.GetString("merchant_message1");
			string text7 = component.corresponding_item.GetString("merchant_message2");
			if (!Startup.StringNullOrWhitespace(text6))
			{
				list.Add(text6);
			}
			if (!Startup.StringNullOrWhitespace(text7))
			{
				list.Add(text7);
			}
		}
		int num = 0;
		foreach (string item in list)
		{
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			dictionary2.Add("type", "NPC_speak");
			dictionary2.Add("text", item);
			dictionary2.Add("go_to", num + 1);
			dictionary.Add(num, dictionary2);
			num++;
		}
		if (text == "wait")
		{
			Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
			dictionary3.Add("type", "MY_options");
			dictionary3.Add("optionA", TranslationControl.Instance.TranslateGeneral("FOLLOW ME", "CompanionsEtc"));
			dictionary3.Add("optionA_goto", -66);
			dictionary.Add(num, dictionary3);
		}
		else if (text == "merchant")
		{
			Dictionary<string, object> dictionary4 = new Dictionary<string, object>();
			dictionary4.Add("type", "MY_options");
			dictionary4.Add("optionA", TranslationControl.Instance.TranslateGeneral("BUY/SELL ITEMS", "Merchants"));
			dictionary4.Add("optionB", TranslationControl.Instance.TranslateGeneral("FOLLOW ME", "CompanionsEtc"));
			dictionary4.Add("optionA_goto", -88);
			dictionary4.Add("optionB_goto", -66);
			dictionary.Add(num, dictionary4);
		}
		string curr_NPC_display_name = component.corresponding_item.GetString("npc_display_name").ToUpper();
		string text8 = component.transform.Find("creature-go-here").GetChild(0).GetComponent<LiteModel>().original.creatures_that_made_me_TRANSLATED[0];
		string text9 = component.transform.Find("creature-go-here").GetChild(0).GetComponent<LiteModel>().original.creatures_that_made_me_TRANSLATED[1];
		string curr_NPC_combo_text = "(" + text8 + " + " + text9 + ")";
		interacting_element_chunkX = chunkX;
		interacting_element_item = component.corresponding_item;
		interacting_element_chunkZ = chunkZ;
		interacting_element_innerX = innerX;
		interacting_element_innerZ = innerZ;
		interacting_element_rot = component.temp_rot;
		DialogueControl.Instance.SetFocusNpc(component.gameObject, curr_NPC_display_name, curr_NPC_combo_text, DialogueControl.focus_type_t.stationary_npc);
		DialogueControl.Instance.EnterDialogue(dictionary, 0, "");
	}

	private void OnInteractWithNPC(string zone, int chunkX, int chunkZ, int innerX, int innerZ)
	{
		string chunkString = ChunkControl.Instance.GetChunkString(zone, chunkX, chunkZ);
		if (!ChunkControl.Instance.IsChunkFullyLoadedOrMidload(chunkString))
		{
			return;
		}
		GameObject buildableInstanceByName = ChunkControl.Instance.GetChunkObj(chunkString).GetBuildableInstanceByName(innerX, innerZ, "DEBUG-npc");
		if (buildableInstanceByName == null)
		{
			return;
		}
		Interactable component = buildableInstanceByName.GetComponent<Interactable>();
		if (!WindowControl.Instance.CanOpenGenericWindow())
		{
			return;
		}
		WindowControl.Instance.DoOpenGenericWindow();
		FullNPC fullNPC = DialogueControl.Instance.GetFullNPC(component.corresponding_item);
		string curr_NPC_display_name = "???";
		if (fullNPC != null)
		{
			curr_NPC_display_name = ((!(fullNPC.translated_display_name == "")) ? fullNPC.translated_display_name : component.corresponding_item.GetString("npc_display_name"));
		}
		string @string = component.corresponding_item.GetString("voice");
		Dictionary<int, Dictionary<string, object>> dictionary = null;
		if (component.corresponding_item.GetString("npc_is_free_follower") == "true" && GameServerConnector.Instance.FullyInGame() && !GameServerConnector.Instance.is_host)
		{
			dictionary = new Dictionary<int, Dictionary<string, object>>();
			string text = GameServerConnector.Instance.server_name.Replace("(private)", "");
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			dictionary2.Add("type", "NPC_speak");
			dictionary2.Add("text", "I will only speak to " + text + "...");
			dictionary2.Add("highlight", text);
			dictionary2.Add("go_to", 1);
			dictionary.Add(0, dictionary2);
			Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
			dictionary3.Add("type", "NPC_speak");
			dictionary3.Add("text", "......");
			dictionary3.Add("go_to", -1);
			dictionary.Add(1, dictionary3);
		}
		if (dictionary == null && fullNPC != null)
		{
			dictionary = fullNPC.dialogue_data;
		}
		string text2 = component.transform.Find("creature-go-here").GetChild(0).GetComponent<LiteModel>().original.creatures_that_made_me_TRANSLATED[0];
		string text3 = component.transform.Find("creature-go-here").GetChild(0).GetComponent<LiteModel>().original.creatures_that_made_me_TRANSLATED[1];
		string curr_NPC_combo_text = "(" + text2 + " + " + text3 + ")";
		interacting_element_chunkX = chunkX;
		interacting_element_chunkZ = chunkZ;
		interacting_element_innerX = innerX;
		interacting_element_innerZ = innerZ;
		interacting_element_item = component.corresponding_item;
		interacting_element_rot = component.temp_rot;
		DialogueControl.Instance.SetFocusNpc(buildableInstanceByName, curr_NPC_display_name, curr_NPC_combo_text, DialogueControl.focus_type_t.stationary_npc);
		DialogueControl.Instance.EnterDialogue(dictionary, 0, @string);
	}

	private void OpenPaintingScreen()
	{
	}

	public void OpenKaraoke()
	{
	}

	public void DestroyKaraokeScreens()
	{
	}

	public void OpenPoolTable(string opponent, int[] ball_arrangement)
	{
	}

	public void DestroyPoolScreen()
	{
	}

	public void HideTargetCircle()
	{
		if (targetShowing)
		{
			targeted_circle_graphic.GetComponent<Animation>().PlayQueued("targetDisappear");
			targetShowing = false;
		}
	}

	public void OnClickFloor(bool allow_combat)
	{
		if (player == null)
		{
			return;
		}
		GameObject main_target = player.GetComponent<CreatureBrain>().main_target;
		player.GetComponent<CreatureBrain>().ClearAllTargets();
		bool flag = true;
		if (allow_combat)
		{
			GameObject closestCombatant = MobControl.Instance.GetClosestCombatant(previous_step_clicked_at, true, true, false, false);
			if (closestCombatant != null)
			{
				player.GetComponent<CreatureBrainLocalPlayer>().SelectTarget(closestCombatant);
				Material material;
				Texture2D mainTexture;
				if (closestCombatant.GetComponent<Combatant>().mob_type == Combatant.TYPE_T.mineral)
				{
					material = targeted_circle_graphic.transform.Find("graphic").GetComponent<Renderer>().material;
					mainTexture = target_circle_yellow;
				}
				else if (closestCombatant.GetComponent<Combatant>().mob_type != Combatant.TYPE_T.creature)
				{
					material = targeted_circle_graphic.transform.Find("graphic").GetComponent<Renderer>().material;
					mainTexture = target_circle_red;
				}
				else
				{
					closestCombatant.GetComponent<Combatant>();
					SharedCreature component = closestCombatant.GetComponent<SharedCreature>();
					SharedCreature.brain_type_t brain_type = component.brain_type;
					if (component.is_local_mob && brain_type == SharedCreature.brain_type_t.companion)
					{
						material = targeted_circle_graphic.transform.Find("graphic").GetComponent<Renderer>().material;
						mainTexture = target_circle_yellow;
					}
					else if (brain_type == SharedCreature.brain_type_t.guard)
					{
						bool num = component.IsTargettingPlayerOrMyCompanions(true, true);
						material = targeted_circle_graphic.transform.Find("graphic").GetComponent<Renderer>().material;
						mainTexture = (num ? target_circle_red : target_circle_yellow);
					}
					else
					{
						material = targeted_circle_graphic.transform.Find("graphic").GetComponent<Renderer>().material;
						mainTexture = ((brain_type == SharedCreature.brain_type_t.ghost) ? target_circle_yellow : target_circle_red);
					}
				}
				material.mainTexture = mainTexture;
				float num2 = Mathf.Max(Combatant.min_target_scale, closestCombatant.GetComponent<Combatant>().scale);
				targeted_circle_graphic.transform.Find("graphic").transform.localScale = Vector3.one * num2;
				if (closestCombatant == main_target)
				{
					rapid_click_count++;
					if (rapid_click_count == 6 && !shown_rapid_click_display)
					{
						pause = true;
						PopupControl.Instance.ShowMessage("Note:\n\nYou only need to click an enemy\n<color=#00ff00>once</color> to target it.\n\nYour creature will attack it\nrepeatedly on it's own.", PopupControl.context.message_unpause_on_okay);
						shown_rapid_click_display = true;
					}
					flag = false;
				}
				else
				{
					flag = true;
					rapid_click_count = 0;
				}
				resume_input_on_next_click = true;
			}
		}
		if (player.GetComponent<CreatureBrain>().main_target == null)
		{
			GameObject closestInteractable = GetClosestInteractable(previous_step_clicked_at);
			if (closestInteractable == null)
			{
				targeted_circle_graphic.transform.Find("graphic").GetComponent<Renderer>().enabled = true;
				targeted_circle_graphic.transform.Find("graphic").GetComponent<Renderer>().material.mainTexture = target_circle_blue;
				targeted_circle_graphic.transform.Find("graphic").transform.localScale = Vector3.one * player.GetComponent<Combatant>().scale;
				targeted_circle_graphic.GetComponent<Animation>().Stop();
				targeted_circle_graphic.transform.localScale = Vector3.one * 0.168f;
				targeted_circle_graphic.transform.position = new Vector3(previous_step_clicked_at.x, 0f, previous_step_clicked_at.z);
				player.GetComponent<CreatureBrainLocalPlayer>().SelectMovePosition(previous_step_clicked_at);
				targetShowing = true;
				return;
			}
			if (player.GetComponent<CreatureBrainLocalPlayer>().interaction_target == closestInteractable)
			{
				resume_input_on_next_click = true;
				return;
			}
			Interactable component2 = closestInteractable.GetComponent<Interactable>();
			if (component2 == null)
			{
				player.GetComponent<CreatureBrainLocalPlayer>().PursueCollectible(closestInteractable);
				ShowYellowCircle(closestInteractable.transform.position, 1.1f);
				targeted_circle_graphic.transform.Find("graphic").transform.localScale = Vector3.one * closestInteractable.GetComponent<Collectible>().circle_size;
			}
			else
			{
				player.GetComponent<CreatureBrainLocalPlayer>().PursueInteractable(closestInteractable);
				ShowYellowCircle(closestInteractable.transform.position, component2.circle_size);
			}
			resume_input_on_next_click = true;
		}
		if (flag)
		{
			BlobbleTargetCircle();
			if (player.GetComponent<SharedCreature>().snapped_to_chair_obj)
			{
				HideTargetCircle();
			}
		}
	}

	public void BlobbleTargetCircle()
	{
		targeted_circle_graphic.GetComponent<Animation>().Stop();
		targeted_circle_graphic.GetComponent<Animation>().PlayQueued("targetGraphic");
		targetShowing = true;
	}

	public void ShowYellowCircle(Vector3 V, float scale)
	{
		targeted_circle_graphic.transform.Find("graphic").GetComponent<Renderer>().material.mainTexture = target_circle_yellow;
		targeted_circle_graphic.transform.position = new Vector3(V.x, 0f, V.z);
		targeted_circle_graphic.transform.Find("graphic").transform.localScale = Vector3.one * scale;
	}

	public GameObject GetClosestInteractable(Vector3 pos, float extra_reach = 0f)
	{
		GameObject result = null;
		float num = float.MaxValue;
		foreach (KeyValuePair<string, GameObject> active_interactible in ChunkControl.Instance.active_interactibles)
		{
			GameObject value = active_interactible.Value;
			if (value == null)
			{
				Debug.Log("ERROR: NULL INTERACTABLE");
				continue;
			}
			Vector3 position = value.transform.position;
			float num2 = ((!(value.GetComponent<Interactable>() != null)) ? value.GetComponent<Collectible>().circle_size : value.GetComponent<Interactable>().circle_size);
			float num3 = Vector3.Distance(position, pos) - extra_reach;
			if (num3 <= 0f)
			{
				num3 = 0f;
			}
			if (num3 < num && num3 < num2 * 0.7f)
			{
				Interactable component = value.GetComponent<Interactable>();
				result = value;
				num = num3;
				if (component != null && component.redirect_to != null)
				{
					result = component.redirect_to;
				}
			}
		}
		return result;
	}

	private void Update()
	{
		if (GamepadInput.Instance.GetMouseButton())
		{
			click_being_held = true;
			processed_click_release = false;
			Ray ray = Camera.main.ScreenPointToRay(GamepadInput.Instance.GetMousePosition());
			float enter = 0f;
			if (plane.Raycast(ray, out enter))
			{
				previous_step_clicked_at = ray.GetPoint(enter) + Vector3.up * SharedCreature.H;
			}
		}
		else
		{
			click_being_held = false;
			processed_click_start = false;
		}
	}

	public void ClickBackToMenu()
	{
		if (GameServerConnector.Instance.game_server_connection != null && (GameServerConnector.Instance.game_server_connection.GetStatus() == Connection.connection_status.connecting || GameServerConnector.Instance.game_server_connection.GetStatus() == Connection.connection_status.connected))
		{
			GameServerConnector.Instance.Disconnect();
		}
		else
		{
			GoToMenu(false);
		}
	}

	public void GoToMenu(bool was_connected_to_server)
	{
		AudioControl.Instance.TryResumeGameMusic(true);
		MenuController.on_press_back_to_menu = true;
		if (!was_connected_to_server || GameServerConnector.Instance.is_host)
		{
			if (was_connected_to_server)
			{
				foreach (KeyValuePair<string, List<int>> item in GameServerReceiver.Instance.unique_ids_given_away)
				{
					ConstructionControl.Instance.RecycleUniqueIds(item.Value);
				}
				GameServerReceiver.Instance.unique_ids_given_away.Clear();
			}
			ChunkControl.Instance.SaveAllLandClaimChunkTimersWithoutDestroying();
		}
		pause = true;
		PopupControl.Instance.HideAll();
		PopupControl.Instance.ShowConnecting("Saving Game (0%)", PopupControl.context.loading_NO_TIMEOUT);
		PlayerData.Instance.AsyncSaveAll(async_return_to_menu, true, true);
	}

	private void async_return_to_menu()
	{
		StartCoroutine(async_return_to_menu_i());
	}

	private IEnumerator async_return_to_menu_i()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Menu");
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}

	public GameObject find_in_children(string t_name, Transform start)
	{
		if (start.gameObject.name == t_name)
		{
			return start.gameObject;
		}
		for (int i = 0; i < start.transform.childCount; i++)
		{
			GameObject gameObject = find_in_children(t_name, start.transform.GetChild(i));
			if (gameObject != null)
			{
				return gameObject;
			}
		}
		return null;
	}

	public void DestroyAllOverheads()
	{
		foreach (GameObject item in possible_destroy)
		{
			if (item != null)
			{
				Object.Destroy(item);
			}
		}
		possible_destroy.Clear();
		foreach (KeyValuePair<string, OnlinePlayer> nearby_player in GameServerInterface.Instance.nearby_players)
		{
			if (nearby_player.Value.obj != null)
			{
				nearby_player.Value.obj.GetComponent<SharedCreature>().MP_display = null;
			}
		}
	}

	public void GiveAllOverheads()
	{
		foreach (KeyValuePair<string, GameObject> active_combatant in MobControl.Instance.active_combatants)
		{
			GameObject value = active_combatant.Value;
			if (value.GetComponent<Combatant>().mob_type == Combatant.TYPE_T.creature && !GameServerInterface.Instance.nearby_players.ContainsKey(active_combatant.Key) && value != player && value.GetComponent<SharedCreature>().levelDisplay == null)
			{
				value.GetComponent<SharedCreature>().RedrawLevelDisplay();
			}
		}
		foreach (KeyValuePair<string, GameObject> active_interactible in ChunkControl.Instance.active_interactibles)
		{
			Interactable component = active_interactible.Value.GetComponent<Interactable>();
			if (!(component != null))
			{
				continue;
			}
			if (component.replace_for == null)
			{
				if (component.icon_spr != null || component.icon2_spr != null)
				{
					component.RedrawOverheadIcon();
				}
				continue;
			}
			Interactable component2 = component.replace_for.GetComponent<Interactable>();
			if (component2.icon_spr != null || component2.icon2_spr != null)
			{
				component2.RedrawOverheadIcon();
			}
		}
		foreach (KeyValuePair<string, OnlinePlayer> nearby_player in GameServerInterface.Instance.nearby_players)
		{
			OnlinePlayer value2 = nearby_player.Value;
			if (value2.obj != null)
			{
				int level = value2.obj.GetComponent<SharedCreature>().level;
				value2.obj.GetComponent<SharedCreature>().CreateMultiplayerDisplay(value2.username_punctuated, level);
			}
		}
	}

	public void PAUSE_GAME()
	{
		pause = true;
	}

	public void UNPAUSE_GAME()
	{
		pause = false;
	}

	public void animation_set_levelup_text()
	{
		GameplayGUIControl.Instance.text_playerLevel.text = "Level Up";
		sfx.PlayOneShot(sfx_got_level_b, AudioControl.Instance.general_sfx_volume);
	}

	public void animation_sound_levelScreenAppear()
	{
		sfx.PlayOneShot(sfx_got_level, AudioControl.Instance.general_sfx_volume);
	}

	public void sound_gameStart(float intensity)
	{
		sfx.PlayOneShot(sfx_gamestart, AudioControl.Instance.general_sfx_volume);
	}

	public void sound_creature_hit(float intensity, float pitch)
	{
		int num = sound_creature_hit_iterator;
		sound_creature_hit_iterator = num + 1;
		AudioControl.Instance.PlayPitch(sfx_creatures[num % 3], pitch, intensity);
	}

	public void sound_player_hit(float intensity, float pitch)
	{
		int num = sound_player_hit_iterator;
		sound_player_hit_iterator = num + 1;
		AudioControl.Instance.PlayPitch(sfx_player[num % 2], pitch, intensity);
	}

	public void animation_unlock_levelup_text()
	{
		GameplayGUIControl.Instance.text_playerLevel.text = "Level " + playerLevel;
		lock_levelup_text = false;
	}

	private void ManuallySelectTarget(bool on_cast_projectile_at_enemies, bool on_cast_projectile_at_allies, bool on_pick_companion_target)
	{
		GameObject gameObject = null;
		if (on_cast_projectile_at_enemies || on_pick_companion_target)
		{
			gameObject = MobControl.Instance.GetClosestCombatant(previous_step_clicked_at, false, false, true, false);
		}
		else if (on_cast_projectile_at_allies)
		{
			gameObject = MobControl.Instance.GetClosestCombatant(previous_step_clicked_at, false, true, false, true);
		}
		if (player != null)
		{
			if (gameObject == null)
			{
				return;
			}
			if (on_cast_projectile_at_enemies || on_cast_projectile_at_allies)
			{
				PerkControl.Instance.PlayerCastPerkAtTarget(PerkControl.Instance.about_to_cast_data, PerkControl.Instance.about_to_cast_lvl, gameObject, Vector3.zero);
				PerkControl.Instance.SpendMana(PerkControl.Instance.about_to_cast_data.GetManaCost(PerkControl.Instance.about_to_cast_lvl));
			}
			else if (on_pick_companion_target)
			{
				ActiveCompanion currSelectedCompanion = CompanionController.Instance.GetCurrSelectedCompanion();
				if (currSelectedCompanion != null)
				{
					currSelectedCompanion.obj.GetComponent<CreatureBrainCompanion>().ManuallySelectedTarget(gameObject);
				}
			}
		}
		ConstructionControl.Instance.DonePlacing(true, true);
	}

	public Vector3 get_cam_offset()
	{
		float num = ((eagle_view_mod != 1f) ? (eagle_view_mod * cam_offset) : (cam_offset * creature_scale_view_mod));
		return cam_angle * num * ChunkControl.Instance.view_zoom;
	}

	public void ReCalcVisionMod()
	{
		float num = 1f;
		if (player != null)
		{
			float num2 = -1f;
			foreach (DurationEffect duration_effect in player.GetComponent<PerkReceiver>().duration_effects)
			{
				if (duration_effect.time_remaining > 0f)
				{
					float @float = duration_effect.GetFloat("Player Vision", "%");
					if (@float > num2 && @float != -1f)
					{
						num2 = @float;
					}
				}
			}
			num = ((num2 != -1f) ? (num2 * 0.01f) : 1f);
		}
		eagle_view_mod = num;
		if (prev_eagle_view_mod != eagle_view_mod && GraphicsControl.Instance.GraphicsLevel() == 4)
		{
			ChunkControl.Instance.UpdateTerrain(false);
		}
		prev_eagle_view_mod = eagle_view_mod;
	}

	public int LevelsToLose()
	{
		return (int)((float)playerLevel * 0.13f);
	}

	public void LoadPlayerLevelFromDisk()
	{
		int slotLong = PlayerData.Instance.GetSlotLong("new_playerLevel", PlayerData.filename_t.general);
		if (slotLong < 2)
		{
			slotLong = 1;
		}
		OverwritePlayerLevel(slotLong, "");
	}

	public void OverwritePlayerLevel(int new_value, string random_fn_validator)
	{
		playerLevel = new_value;
		if (player != null)
		{
			player.GetComponent<SharedCreature>().level = new_value;
		}
	}

	public void SavePlayerLevelToDisk()
	{
		PlayerData.Instance.SetSlotLong("new_playerLevel", playerLevel, PlayerData.filename_t.general);
	}

	public void LoadSkillPointsSpendableFromDisk()
	{
		skillPointsSpendable = Mathf.Max(0, PlayerData.Instance.GetSlotShort("skillPointsSpendable", PlayerData.filename_t.general));
	}

	public void OverwriteSkillPointsSpendable(int new_value, string random_fn_validator)
	{
		skillPointsSpendable = new_value;
	}

	public void SaveSkillPointsSpendableToDisk()
	{
		PlayerData.Instance.SetSlotShort("skillPointsSpendable", skillPointsSpendable, PlayerData.filename_t.general);
	}

	public void LoadCurrentExpFromDisk()
	{
		currentEXP = Mathf.Max(0, PlayerData.Instance.GetSlotShort("currentEXP", PlayerData.filename_t.general));
	}

	public void OverwriteCurrentExp(int new_value, string random_fn_validator)
	{
		currentEXP = new_value;
	}

	public void SaveCurrentExpToDisk()
	{
		PlayerData.Instance.SetSlotShort("currentEXP", currentEXP, PlayerData.filename_t.general);
	}

	public void LoadPlayerStatsFromDisk()
	{
		player_stats = new int[n_stats];
		for (int i = 0; i < n_stats; i++)
		{
			player_stats[i] = PlayerData.Instance.GetSlotShort("stat" + i, PlayerData.filename_t.general);
		}
	}

	public void ClearPlayerStats()
	{
		player_stats = new int[n_stats];
		for (int i = 0; i < n_stats; i++)
		{
			player_stats[i] = 0;
		}
	}

	public void SaveAllStatsToDisk()
	{
		for (int i = 0; i < n_stats; i++)
		{
			PlayerData.Instance.SetSlotShort("stat" + i, player_stats[i], PlayerData.filename_t.general);
		}
	}

	public void LoadParentCreaturesFromDisk()
	{
		player_parent_creatures.Clear();
		int num = PlayerData.Instance.GetSlotShort("n_morphed_creatures", PlayerData.filename_t.general);
		if (num < 3)
		{
			num = 2;
		}
		for (int i = 0; i < num; i++)
		{
			player_parent_creatures.Add(PlayerData.Instance.GetSlotString("parent" + i, PlayerData.filename_t.general));
		}
	}

	public void SaveParentCreaturesToDisk()
	{
		PlayerData.Instance.SetSlotShort("n_morphed_creatures", player_parent_creatures.Count, PlayerData.filename_t.general);
		for (int i = 0; i < player_parent_creatures.Count; i++)
		{
			PlayerData.Instance.SetSlotString("parent" + i, player_parent_creatures[i], PlayerData.filename_t.general);
		}
	}

	private bool ProcessClickLogic()
	{
		if (resume_input_on_next_click)
		{
			return false;
		}
		if (GameServerConnector.Instance.FullyInGame() && GameServerReceiver.Instance.waiting_on_initial_zone_data)
		{
			return false;
		}
		if (!PopupControl.Instance.popup_open)
		{
			if (WindowControl.Instance.curr_window != WindowControl.window_type_t.none)
			{
				return false;
			}
			if (WindowControl.Instance.curr_miniwindow != WindowControl.miniwindow_type_t.none)
			{
				return false;
			}
			if (Application.isEditor && (ConsoleControl.Instance.console_open || DevBuildControl.Instance.disable_movement))
			{
				return false;
			}
			if (!pause || is_picking_companion_target || casting_projectile_at_enemy || casting_projectile_at_ally || picking_cast_custom_location || is_picking_companion_walk_location)
			{
				return true;
			}
		}
		return false;
	}

	private void FixedUpdate()
	{
		directional_light.transform.rotation = Quaternion.Lerp(directional_light.transform.rotation, curr_light_angle, Time.fixedDeltaTime * 4f);
		if (player != null)
		{
			prev_player_pos = player.transform.position;
			if (Vector3.Distance(player.transform.position, prev_no_fall_thru_floor_pos) > 3f)
			{
				no_fall_thru_floor.transform.position = new Vector3(player.transform.position.x, 0f, player.transform.position.z);
				prev_no_fall_thru_floor_pos = no_fall_thru_floor.transform.position;
			}
		}
		bool num = click_being_held;
		bool flag = ProcessClickLogic();
		if (!num)
		{
			if (flag && !processed_click_release && player != null && player.GetComponent<CreatureBrain>().main_target == null && player.GetComponent<CreatureBrainLocalPlayer>().interaction_target == null)
			{
				BlobbleTargetCircle();
			}
			processed_click_release = true;
			if (resume_input_on_next_click)
			{
				resume_input_on_next_click = false;
			}
		}
		else
		{
			if (flag && !processed_click_start)
			{
				if (casting_projectile_at_enemy)
				{
					ManuallySelectTarget(true, false, false);
				}
				else if (casting_projectile_at_ally)
				{
					ManuallySelectTarget(false, true, false);
				}
				else if (picking_cast_custom_location)
				{
					PerkControl.Instance.PlayerCastPerkAtTarget(PerkControl.Instance.about_to_cast_data, PerkControl.Instance.about_to_cast_lvl, null, previous_step_clicked_at);
					PerkControl.Instance.SpendMana(PerkControl.Instance.about_to_cast_data.GetManaCost(PerkControl.Instance.about_to_cast_lvl));
					ConstructionControl.Instance.DonePlacing(true, true);
				}
				else if (is_picking_companion_target)
				{
					ManuallySelectTarget(false, false, true);
				}
				else if (is_picking_companion_walk_location)
				{
					ActiveCompanion currSelectedCompanion = CompanionController.Instance.GetCurrSelectedCompanion();
					if (currSelectedCompanion != null && currSelectedCompanion.obj != null)
					{
						currSelectedCompanion.obj.GetComponent<CreatureBrainCompanion>().ManuallySelectOverwriteWalkTo(previous_step_clicked_at);
					}
					ConstructionControl.Instance.DonePlacing(true, true);
				}
			}
			processed_click_start = true;
			if (ProcessClickLogic())
			{
				OnClickFloor(true);
			}
		}
		if (ChunkControl.Instance.follow_obj != null)
		{
			Vector3 vector = Vector3.zero;
			Quaternion b = Quaternion.identity;
			if (DialogueControl.Instance.focus_type == DialogueControl.focus_type_t.none || MapEditorControl.Instance.editing_map)
			{
				Vector3 vector2 = new Vector3(ChunkControl.Instance.follow_obj.transform.position.x, (ChunkControl.Instance.follow_obj.transform.position.y < lowest_player_y) ? lowest_player_y : ChunkControl.Instance.follow_obj.transform.position.y, ChunkControl.Instance.follow_obj.transform.position.z);
				vector = vector2 + get_cam_offset();
				b = Quaternion.LookRotation((vector2 - vector).normalized);
			}
			else if (DialogueControl.Instance.focus_type == DialogueControl.focus_type_t.stationary_npc || DialogueControl.Instance.focus_type == DialogueControl.focus_type_t.moving_npc)
			{
				Vector3 vector3;
				if (DialogueControl.Instance.curr_NPC_obj != null)
				{
					LiteModel component = DialogueControl.Instance.curr_NPC_obj.GetComponent<LiteModel>();
					vector3 = ((!(component != null)) ? DialogueControl.Instance.curr_NPC_obj.transform.position : component.GetLimbWorldPosition(component.head_limb_index));
				}
				else
				{
					vector3 = Vector3.zero;
					Debug.Log("ERROR: curr_NPC_obj == null");
				}
				float num2;
				float num3;
				switch (interacting_element_rot)
				{
				case 1:
					num2 = -1.25f;
					num3 = -1.25f;
					break;
				case 2:
					num2 = 1.25f;
					num3 = 1.25f;
					break;
				default:
					num2 = 1.48f;
					num3 = -0.962f;
					break;
				}
				vector = new Vector3(vector3.x + num2, 0.7f, vector3.z + num3);
				b = Quaternion.LookRotation((vector3 + Vector3.down * 0.12f - vector).normalized);
			}
			else if (DialogueControl.Instance.focus_type == DialogueControl.focus_type_t.painting)
			{
				Vector3 position = DialogueControl.Instance.curr_NPC_obj.transform.position;
				vector = position + DialogueControl.Instance.curr_NPC_obj.transform.forward * 1.5f + DialogueControl.Instance.curr_NPC_obj.transform.right * 0.25f;
				b = Quaternion.LookRotation((DialogueControl.Instance.curr_NPC_obj.transform.position - vector).normalized);
			}
			else if (DialogueControl.Instance.focus_type == DialogueControl.focus_type_t.trophy)
			{
				Vector3 vector4 = DialogueControl.Instance.curr_NPC_obj.transform.position + Vector3.down * 0.5f;
				float num4;
				float num5;
				switch (interacting_element_rot)
				{
				case 0:
				case 1:
					num4 = 1.48f;
					num5 = -0.962f;
					break;
				case 2:
					num4 = -1.25f;
					num5 = -1.25f;
					break;
				default:
					num4 = 1.25f;
					num5 = 1.25f;
					break;
				}
				vector = new Vector3(vector4.x + num4, 0.7f, vector4.z + num5);
				b = Quaternion.LookRotation((vector4 + Vector3.down * 0.12f - vector).normalized);
			}
			mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, vector, Time.deltaTime * 4f);
			mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, b, Time.deltaTime * 4f);
		}
		if (player == null)
		{
			return;
		}
		GameObject main_target = player.GetComponent<CreatureBrain>().main_target;
		if (main_target != null)
		{
			targeted_circle_graphic.transform.position = new Vector3(main_target.transform.position.x, 0f, main_target.transform.position.z);
		}
		if (!animate_exp_bar || casting_projectile_at_enemy || is_picking_companion_target || casting_projectile_at_ally || picking_cast_custom_location)
		{
			return;
		}
		visualEXP = Mathf.Lerp(visualEXP, currentEXP, Time.deltaTime * 5f);
		int num6 = playerLevel;
		int num7 = ((num6 == 1) ? 4 : ((num6 <= 26) ? ((int)((float)(num6 - 1) * 1.7f + 9f)) : 50));
		float num8;
		if (num7 == 0)
		{
			num8 = 0.02f;
		}
		else
		{
			num8 = visualEXP / (float)num7;
			if (num8 >= 1f)
			{
				visualEXP = 0f;
				currentEXP -= num7;
				SaveCurrentExpToDisk();
				animate_exp_bar = true;
				OverwritePlayerLevel(num6 + 1, "");
				SavePlayerLevelToDisk();
				player.GetComponent<SharedCreature>().ReCalcHpMaxAndHpRegen();
				player.GetComponent<Combatant>().GainSomeHPOnLevelup();
				GameServerSender.Instance.SendUpdateCreatureStats("LOCAL", "");
				player.GetComponent<SharedCreature>().ShowLevelupParticles(false, 0.6f);
				skillPointsSpendable++;
				SaveSkillPointsSpendableToDisk();
				GameplayGUIControl.Instance.RedrawSkillPointsSpendableText();
				if (!lock_levelup_text)
				{
					GameplayGUIControl.Instance.text_playerLevel.text = "Level " + playerLevel;
				}
				num8 = 0f;
				if (!level_up_animation_playing)
				{
					GameplayGUIControl.Instance.levelbar.GetComponent<Animation>().Play("on level up");
					level_up_animation_playing = true;
					lock_levelup_text = true;
				}
			}
		}
		GameplayGUIControl.Instance.expBarYellow.rectTransform.sizeDelta = new Vector2(num8 * 358f, 18f);
		GameplayGUIControl.Instance.expBarYellow.transform.localPosition = new Vector3((1f - num8) * -178f, 0f, 0f);
		if (Mathf.Abs((float)currentEXP - visualEXP) < 0.1f)
		{
			animate_exp_bar = false;
		}
	}

	public string GetSavedPlayerZoneOnSlot()
	{
		string slotString = PlayerData.Instance.GetSlotString("player_zone", PlayerData.filename_t.playerpos);
		if (!(slotString == ""))
		{
			return slotString;
		}
		return "overworld";
	}

	public string GetSavedPlayerZoneOnServer(string server_name)
	{
		string globalString = PlayerData.Instance.GetGlobalString(server_name + "player_zone");
		if (!(globalString == ""))
		{
			return globalString;
		}
		return "overworld";
	}

	public Vector3 GetSavedPlayerPositionOnSlot()
	{
		short slotShort = PlayerData.Instance.GetSlotShort("player_chunk_x", PlayerData.filename_t.playerpos);
		short slotShort2 = PlayerData.Instance.GetSlotShort("player_chunk_z", PlayerData.filename_t.playerpos);
		short slotShort3 = PlayerData.Instance.GetSlotShort("player_inner_x", PlayerData.filename_t.playerpos);
		short slotShort4 = PlayerData.Instance.GetSlotShort("player_inner_z", PlayerData.filename_t.playerpos);
		return new Vector3((float)(slotShort * 10 + slotShort3) + 0.5f, 0.5f, (float)(slotShort2 * 10 + slotShort4) + 0.5f);
	}

	public Vector3 GetSavedPlayerPositionOnServer(string server_name)
	{
		short globalShort = PlayerData.Instance.GetGlobalShort(server_name + "player_chunk_x");
		short globalShort2 = PlayerData.Instance.GetGlobalShort(server_name + "player_chunk_z");
		short globalShort3 = PlayerData.Instance.GetGlobalShort(server_name + "player_inner_x");
		short globalShort4 = PlayerData.Instance.GetGlobalShort(server_name + "player_inner_z");
		if (globalShort2 == 0 && globalShort == 0 && globalShort3 == 0 && globalShort4 == 0)
		{
			return BreedControl.Instance.campos_result.transform.position;
		}
		return new Vector3((float)(globalShort3 + globalShort * 10) + 0.5f, 0.5f, (float)(globalShort4 + globalShort2 * 10) + 0.5f);
	}

	public int NextLevelExp(int curr_level)
	{
		if (curr_level - 1 == 0)
		{
			return 4;
		}
		if (curr_level < 27)
		{
			return (int)((float)(curr_level - 1) * 1.7f + 9f);
		}
		return 50;
	}

	public void AssignHealthbar(GameObject obj)
	{
		if (obj.GetComponent<Combatant>().healthbar == null)
		{
			SharedCreature component = obj.GetComponent<SharedCreature>();
			if (component != null && component.levelDisplay != null)
			{
				possible_destroy.Remove(component.levelDisplay);
				Object.Destroy(component.levelDisplay);
			}
			obj.GetComponent<Combatant>().ReceiveHealthbar(Object.Instantiate(type_healthbar), Camera.main);
		}
		else
		{
			obj.GetComponent<Combatant>().RefreshHealthbarDisappearTimer(5f);
		}
	}
}
