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
	}

	public void PlaySignSound()
	{
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
		return 0f;
	}

	public void SetLightAngleToOverworld()
	{
		curr_light_angle = Quaternion.Euler(52f, 0f, 0f);
		directional_light.intensity = 1f;
		directional_light.shadowStrength = 0.5f;
	}

	public void SetLightAngleToIndoors()
	{
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
	}

	public void revive_cancel()
	{
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
	}

	public void sound_mixmutants()
	{
	}

	public void sound_death()
	{
	}

	public void sound_ding()
	{
	}

	public void sound_sell()
	{
	}

	public void sound_buy()
	{
	}

	public void sound_levelButton()
	{
	}

	public void sound_relax()
	{
	}

	public void level_up_meter_pressed(int index)
	{
	}

	private void show_get_perk(int index)
	{
	}

	private IEnumerator perk_get_display(int index)
	{
		return null;
	}

	public void PressViewAchievements()
	{
	}

	public void AttemptAdOnLevelup()
	{
	}

	public void level_up_screen_close()
	{
	}

	public void TryLevelAchieves()
	{
	}

	public void HideLevelScreen()
	{
	}

	public void edit_navpost()
	{
	}

	public void edit_merchantSign()
	{
	}

	public void NoteInteractingElement(int chunkX, int chunkZ, int innerX, int innerZ, InventoryItem item, int rot)
	{
	}

	public void ForgetInteractingElement()
	{
	}

	public void PressEndSitInChair()
	{
	}

	public void player_interact(GameObject interaction_target)
	{
	}

	private void OnInteractWithStatue(string zone, int chunkX, int chunkZ, int innerX, int innerZ)
	{
	}

	private void OnInteractWithCompanion(string zone, int chunkX, int chunkZ, int innerX, int innerZ)
	{
	}

	private void OnInteractWithNPC(string zone, int chunkX, int chunkZ, int innerX, int innerZ)
	{
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
	}

	public void animation_sound_levelScreenAppear()
	{
	}

	public void sound_gameStart(float intensity)
	{
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
	}

	public int LevelsToLose()
	{
		return 0;
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
		return null;
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
		return default(Vector3);
	}

	public int NextLevelExp(int curr_level)
	{
		return 0;
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
