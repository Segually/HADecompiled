using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BreedControl : MonoBehaviour, OrderedStart
{
	public enum breeder_transition
	{
		on_death = 0,
		on_mutation = 1,
		on_companion = 2,
		on_play = 3
	}

	private enum buttons_pos
	{
		left = 0,
		right = 1
	}

	public enum banner_type
	{
		breed = 0,
		mutate = 1,
		companion = 2
	}

	public enum state
	{
		none = 0,
		select_father = 1,
		select_mother = 2,
		select_mutant = 3,
		view_companion_hatch = 4
	}

	public static BreedControl Instance;

	private bool RANDOM_MOB_CYCLE;

	private string random_cycle_force_father = "hummingbird";

	private bool random_cycle_force_premium_mother;

	private bool SHOOTING_TRAILER;

	private float trailer_animation_speed = -1f;

	private string trailer_animal_1 = "";

	private string trailer_animal_2 = "";

	private string trailer_forced_result = "";

	private string trailer_overwrite_name = "";

	private string trailer_pre_animal = "";

	public Transform campos_fathr_16_by_9;

	public Transform campos_mothr_16_by_9;

	public Transform campos_fathr_4_by_3;

	public Transform campos_mothr_4_by_3;

	public Transform campos_first_animal_short;

	public Transform campos_first_animal_tall;

	public Transform campos_second_animal_short;

	public Transform campos_second_animal_tall;

	public Transform campos_result;

	public Transform cam_result_rotate_around;

	public FakeTransform spawn_mutant;

	private FakeTransform campos_fathr_gen;

	private FakeTransform campos_mothr_gen;

	private FakeTransform campos_first_gen;

	private FakeTransform campos_second_gen;

	private FakeTransform campos_mutant_gen;

	private FakeTransform rotate_around;

	private FakeTransform mid_mutant;

	private FakeTransform rot_point;

	private FakeTransform cam_curr_dest;

	public Transform spawn_father;

	public Transform spawn_mother;

	public GameObject prefab_breed_button;

	public Transform button_parent;

	public Animation header_fade;

	public Animation result_emerge;

	public RectTransform[] elements_to_reverse;

	private List<GameObject> buttons = new List<GameObject>();

	private Vector2 buttons_parent_start_pos;

	private Vector3 prev_mouse;

	private float buttons_velocity;

	public Text select_Father_text;

	public Color col_select_father_Text;

	public Color col_select_mother_Text;

	public Color col_select_mutant_text;

	public Sprite spr_select_father_button;

	public Sprite spr_select_mother_button;

	public Sprite spr_premium_button;

	public Sprite spr_mutant_button;

	public state state_t;

	public GameObject prev_button_pressed;

	public GameObject accept_button;

	private GameObject father;

	private GameObject mother;

	public GameObject result;

	public Text animal_selected_name;

	public TextMeshProUGUI animal_one_name_;

	public TextMeshProUGUI animal_two_name_;

	public GameObject plus_obj;

	public GameObject equals_obj;

	private float cam_lerp_speed;

	public float result_rot_angle;

	public float result_cam_h;

	public float result_cam_view_h;

	public float result_view_dist;

	private bool viewing_result;

	public bool view_result_rotate;

	private Vector3 elevator_start_pos;

	private bool can_interact_with_buttons;

	public Text text_result_name;

	public GameObject text_THE;

	public Text text_perk;

	public Image img_perk;

	private string father_creature;

	private string mother_creature;

	private string mutant_creature;

	private string prev_mother = "";

	private string prev_father = "";

	private List<string> creatures_created;

	private float max_buttons_y;

	private GameObject attempted_to_click_button;

	private float release_button_timer;

	private string starting_perk_key = "";

	public Animation results_anm;

	private bool cam_lerp;

	public bool initial_pick_animal = true;

	public GameObject elevator;

	public GameObject pos_elevator_finished;

	public GameObject breeder_fine_details;

	private bool on_battle_skipBreed;

	private buttons_pos curr_buttons_pos = buttons_pos.right;

	public GameObject perk_display;

	public GameObject leveup_button;

	public GameObject banner_button_A;

	public GameObject banner_button_B;

	public GameObject type_mutation_bubbles;

	private GameObject mutant;

	public Canvas canvas;

	private bool lerp_mutants;

	private GameObject bubbles;

	public bool paid_companion;

	private TouchScreenKeyboard keyboard;

	public Text accept_Text;

	private int redraw_buttons_timer;

	private int n_buttons;

	public Font translated_font;

	private IEnumerator blobble;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
		CreateCameraPoints();
		if (!Application.isEditor)
		{
			RANDOM_MOB_CYCLE = false;
			SHOOTING_TRAILER = false;
		}
		PopupControl.Instance.HideAll();
		short globalShort = PlayerData.Instance.GetGlobalShort("dev_mode");
		GameplayGUIControl.Instance.curr_GUI = ((globalShort == 1) ? GameplayGUIControl.GUI_layout_t.dev_interface : GameplayGUIControl.GUI_layout_t.standard_gameplay);
		if (!MapEditorControl.Instance.editing_map)
		{
			short slotShort = PlayerData.Instance.GetSlotShort("PLAYER_ALIVE", PlayerData.filename_t.general);
			switch (slotShort)
			{
			case 2:
				SkipBreedScreen(true, GameController.Instance.GetSavedPlayerZoneOnSlot(), GameController.Instance.GetSavedPlayerPositionOnSlot(), null, false);
				GameController.Instance.SnapCam(0.65f);
				GameController.Instance.sound_death();
				GameController.Instance.ShowDeathScreen("new death anm 2");
				WindowPrefabsControl.Instance.CreateScreen("You Died - bottom left", WindowPrefabsControl.build_into_t.GAME_CTR);
				break;
			case 0:
				if (PlayerData.Instance.GetGlobalShort("dev_mode") == 1)
				{
					string randomCreature = CreatureMorpher.Instance.GetRandomCreature();
					string randomCreature2 = CreatureMorpher.Instance.GetRandomCreature();
					List<string> parents = new List<string> { randomCreature, randomCreature2 };
					BreedComplete(randomCreature, randomCreature2, "DEV MODE", CreatureMorpher.Instance.GetHybridLite(parents));
					GameController.Instance.SnapCam(0.65f);
					PlayBreedWhoosh();
				}
				else
				{
					QualitySettings.shadows = ShadowQuality.HardOnly;
					QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
					SetUpBreeder();
					GameController.Instance.SetBackground_Breeder();
					GameController.Instance.SetLightAngleToDadAnimal();
					PreloadChunks();
				}
				break;
			default:
				SkipBreedScreen(false, GameController.Instance.GetSavedPlayerZoneOnSlot(), GameController.Instance.GetSavedPlayerPositionOnSlot(), TransitionControl.Instance.StartFadeBackInSilent, false);
				TransitionControl.Instance.ShowSplash(true);
				PopupControl.Instance.ShowConnecting("Loading world", PopupControl.context.initial_load_world);
				break;
			}
		}
		else
		{
			SkipBreedScreenMapEditor();
			MapEditorControl.Instance.OnEnterGame();
		}
		spawn_mutant = new FakeTransform();
		accept_Text.text = TranslationControl.Instance.TranslateGeneral("ACCEPT", "GUI");
		if (SHOOTING_TRAILER && trailer_pre_animal != "")
		{
			CreateParent(ref father, spawn_father.transform.position, spawn_father.transform.rotation, trailer_pre_animal);
		}
		prev_mouse = GamepadInput.Instance.GetMousePosition();
		can_interact_with_buttons = true;
	}

	public void PreloadChunks()
	{
		ChunkControl instance = ChunkControl.Instance;
		instance.player_chunk_X = -1;
		instance.player_chunk_Z = 0;
		instance.UpdateTerrain(false);
	}

	public void TransitionBackToBreeder(breeder_transition transition)
	{
		GameController.Instance.enabled = false;
		GameplayGUIControl.Instance.HideGameplayGui();
		QualitySettings.shadows = ShadowQuality.HardOnly;
		QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
		switch (transition)
		{
		case breeder_transition.on_death:
		{
			initial_pick_animal = true;
			breeder_fine_details.SetActive(true);
			SetUpBreeder(state.select_father);
			GameController.Instance.SetBackground_Breeder();
			PreloadChunks();
			GameController.Instance.breeder_floor_plane.SetActive(true);
			GameController.Instance.breeder_floor_plane.transform.position = Vector3.zero;
			Texture2D texture2D = new Texture2D(2, 2);
			Color[] array = new Color[4];
			for (int i = 0; i < 4; i++)
			{
				array[i] = new Color(1f, 1f, 1f, 1f);
			}
			texture2D.SetPixels(0, 0, 2, 2, array);
			texture2D.Apply();
			GameController.Instance.breeder_floor_plane.GetComponent<Renderer>().material.mainTexture = texture2D;
			break;
		}
		case breeder_transition.on_companion:
			cam_lerp = true;
			cam_lerp_speed = 5f;
			ViewResult(CompanionController.Instance.EGG.transform, -1.5f);
			GameController.Instance.SetBackground_NPC();
			break;
		case breeder_transition.on_mutation:
			SetUpBreeder(state.select_mutant);
			GameController.Instance.SetBackground_NPC();
			break;
		}
	}

	private void DestroyButtons()
	{
		foreach (GameObject button in buttons)
		{
			UnityEngine.Object.Destroy(button);
		}
		buttons.Clear();
		n_buttons = 0;
	}

	private void SetUpBreeder(state state_t = state.select_father)
	{
		creatures_created = new List<string>();
		CreateButtons(CreatureMorpher.Instance.default_creature_names, false);
		bool isEditor = Application.isEditor;
		if (PlayerData.Instance.GetGlobalShort("creature_pack_1") == 1 || isEditor)
		{
			CreateButtons(GetPackCreatures("Animal Pack (Mix 2)"), true);
		}
		if (PlayerData.Instance.GetGlobalShort("creature_pack_2") == 1 || isEditor)
		{
			CreateButtons(GetPackCreatures("Animal Pack (Mix 3)"), true);
		}
		if (PlayerData.Instance.GetGlobalShort("creature_pack_3") == 1 || isEditor)
		{
			CreateButtons(GetPackCreatures("Animal Pack (Food 3)"), true);
		}
		if (PlayerData.Instance.GetGlobalShort("creature_pack_4") == 1 || isEditor)
		{
			CreateButtons(GetPackCreatures("Animal Pack (Food 1)"), true);
		}
		if (PlayerData.Instance.GetGlobalShort("creature_pack_5") == 1 || isEditor)
		{
			CreateButtons(GetPackCreatures("Animal Pack (Vehicles)"), true);
		}
		if (PlayerData.Instance.GetGlobalShort("creature_pack_6") == 1 || isEditor)
		{
			CreateButtons(GetPackCreatures("Animal Pack (Food 2)"), true);
		}
		if (PlayerData.Instance.GetGlobalShort("creature_pack_7") == 1 || isEditor)
		{
			CreateButtons(GetPackCreatures("Animal Pack (Mix 1)"), true);
		}
		if (PlayerData.Instance.GetGlobalShort("creature_pack_11") == 1 || isEditor)
		{
			CreateButtons(GetPackCreatures("Animal Pack (Cute 1)"), true);
		}
		if (PlayerData.Instance.GetGlobalShort("creature_pack_10") == 1 || isEditor)
		{
			CreateButtons(GetPackCreatures("Animal Pack (Bugs)"), true);
		}
		if (PlayerData.Instance.GetGlobalShort("creature_pack_8") == 1 || isEditor)
		{
			CreateButtons(GetPackCreatures("Animal Pack (Dogs)"), true);
		}
		if (PlayerData.Instance.GetGlobalShort("creature_pack_9") == 1 || isEditor)
		{
			CreateButtons(GetPackCreatures("Animal Pack (People)"), true);
		}
		if (PlayerData.Instance.GetGlobalShort("creature_pack_12") == 1 || isEditor)
		{
			CreateButtons(GetPackCreatures("Animal Pack (Cute 2)"), true);
		}
		if (PlayerData.Instance.GetGlobalShort("creature_pack_13") == 1 || isEditor)
		{
			CreateButtons(GetPackCreatures("Animal Pack (Birds)"), true);
		}
		if (PlayerData.Instance.GetGlobalShort("creature_pack_14") == 1 || isEditor)
		{
			CreateButtons(GetPackCreatures("Animal Pack (Jungle)"), true);
		}
		List<string> list = new List<string>();
		for (int i = 0; i < PlayerData.Instance.GetGlobalShort("n_free_creatures"); i++)
		{
			string globalString = PlayerData.Instance.GetGlobalString("free_animal_" + i);
			if (!creatures_created.Contains(globalString))
			{
				list.Add(globalString);
			}
		}
		if (list.Count != 0)
		{
			CreateButtons(list, true);
		}
		max_buttons_y = (float)(int)((float)(n_buttons - 1) / 3f) * 110f;
		cam_lerp = true;
		SetState(state_t);
	}

	private void CreateCameraPoints()
	{
		rot_point = new FakeTransform();
		float screenRatio = GetScreenRatio();
		campos_fathr_gen = new FakeTransform();
		campos_fathr_gen.position = Vector3.Lerp(campos_fathr_4_by_3.position, campos_fathr_16_by_9.position, screenRatio);
		campos_fathr_gen.rotation = Quaternion.Lerp(campos_fathr_4_by_3.rotation, campos_fathr_16_by_9.rotation, screenRatio);
		campos_mothr_gen = new FakeTransform();
		campos_mothr_gen.position = Vector3.Lerp(campos_mothr_4_by_3.position, campos_mothr_16_by_9.position, screenRatio);
		campos_mothr_gen.rotation = Quaternion.Lerp(campos_mothr_4_by_3.rotation, campos_mothr_16_by_9.rotation, screenRatio);
		campos_first_gen = new FakeTransform();
		campos_second_gen = new FakeTransform();
		campos_mutant_gen = new FakeTransform();
		mid_mutant = new FakeTransform();
		buttons_parent_start_pos = button_parent.transform.localPosition;
		elevator_start_pos = campos_result.parent.transform.localPosition;
	}

	private void EndBreeder()
	{
		release_button_timer = 0f;
		attempted_to_click_button = null;
		prev_button_pressed = null;
		DestroyButtons();
		StopViewingResult();
		cam_lerp = false;
		HideBanners();
	}

	public void PressFinishedBreeding()
	{
		PopupControl.Instance.SetButtonWasPressed();
		if (state_t == state.view_companion_hatch)
		{
			GameController.Instance.UNPAUSE_GAME();
			StopViewingResult();
			cam_lerp = false;
			HideBanners();
			if (!paid_companion)
			{
				StartCoroutine(DelayedCompanionAd());
			}
			else
			{
				paid_companion = false;
			}
			InitializeGameplay(true);
			state_t = state.none;
			GameController.Instance.GiveAllOverheads();
			GameplayGUIControl.Instance.ShowGameplayGui();
			StartCoroutine(DelayedHideBreeder(2f));
			return;
		}
		EndBreeder();
		UnityEngine.Object.Destroy(mother);
		UnityEngine.Object.Destroy(father);
		BreedComplete(father_creature, mother_creature, text_result_name.text, result);
		PlayBreedWhoosh();
		foreach (GameObject item in ChunkControl.Instance.enable_on_accept)
		{
			if (!(item != null))
			{
				continue;
			}
			item.SetActive(true);
			if (item.GetComponent<SharedCreature>() != null && item.GetComponent<SharedCreature>().is_local_mob && (!(GameController.Instance.player != null) || !(item == GameController.Instance.player)))
			{
				item.GetComponent<SharedCreature>().ReEnable();
				item.GetComponent<CreatureBrain>().ReEnable();
			}
		}
		ChunkControl.Instance.enable_on_accept.Clear();
	}

	private void BreedComplete(string parent0, string parent1, string result_name, GameObject player_model)
	{
		GameplayGUIControl.Instance.ShowGameplayGui();
		AudioControl.Instance.PlayGenericClick();
		AudioControl.Instance.PlayIntroMusic();
		AdvertControl.Instance.ADS_num_rebreeds = 0;
		PlayerData.Instance.SetSlotShort("PLAYER_ALIVE", 1, PlayerData.filename_t.general);
		GameController.Instance.player_parent_creatures.Add(parent0);
		GameController.Instance.player_parent_creatures.Add(parent1);
		GameController.Instance.SaveParentCreaturesToDisk();
		PlayerData.Instance.SetSlotString("creatureName", result_name, PlayerData.filename_t.general);
		GameController.Instance.OverwritePlayerLevel(1, "");
		GameController.Instance.SavePlayerLevelToDisk();
		GameplayGUIControl.Instance.text_playerLevel.text = "Level 1";
		GameplayGUIControl.Instance.RedrawSkillPointsSpendableText();
		PerkControl.Instance.InitializeEmptyPerkList();
		if (starting_perk_key == "")
		{
			starting_perk_key = "perk_eagle_eye";
		}
		PerkControl.Instance.OverwritePerkLevel(starting_perk_key, PerkControl.Instance.GetPerkLevel(starting_perk_key) + 1);
		PerkControl.Instance.SavePerkLevel(starting_perk_key);
		PerkControl.Instance.perk_slot_A = starting_perk_key;
		PerkControl.Instance.SaveEquippedPerksToDisk();
		PerkControl.Instance.RedrawEquippedPerkSlots();
		ChunkControl.Instance.player_chunk_X = -1;
		ChunkControl.Instance.player_chunk_Z = 0;
		GameController.Instance.animate_exp_bar = true;
		GameController.Instance.ResetVariables();
		GameController.Instance.CreatePlayer(player_model, campos_result.transform.position, true);
		GameController.Instance.UNPAUSE_GAME();
		GameServerSender.Instance.SendRespawn();
		if (GameServerConnector.Instance.FullyInGame() && !GameServerConnector.Instance.is_host)
		{
			GameServerConnector.Instance.slow_load_chunks_begin = DateTime.UtcNow;
		}
		state_t = state.none;
		ZoneData new_zone_data = ZoneDataControl.Instance.LoadOverworld();
		ZoneDataControl.Instance.ChangeZone(new_zone_data, ZoneDataControl.change_zone_type.custom_position_no_transition, campos_result.transform.position, null, true, false, false);
		if (!GameServerConnector.Instance.FullyInGame())
		{
			GameController.time_of_day = 0.35f;
		}
		InitializeGameplay(false);
		StartCoroutine(DelayedHideBreeder(2f));
	}

	public void PlayBreedWhoosh()
	{
		AudioControl.Instance.Play(AudioControl.Instance.sfx_game_Start, 0.8f);
	}

	public void InitializeGameplay(bool on_mutate_or_breed)
	{
		int num = GraphicsControl.Instance.GraphicsLevel();
		if (num >= 5)
		{
			QualitySettings.shadows = ShadowQuality.HardOnly;
			QualitySettings.shadowResolution = ShadowResolution.Medium;
		}
		else if (num == 4)
		{
			QualitySettings.shadows = ShadowQuality.HardOnly;
			QualitySettings.shadowResolution = ShadowResolution.Low;
		}
		else
		{
			QualitySettings.shadows = ShadowQuality.Disable;
		}
		initial_pick_animal = false;
		GameController.Instance.enabled = true;
		GameController.Instance.SetBackground_Explore();
		GameController.Instance.start_daynight_cycle();
		breeder_fine_details.SetActive(false);
		if (!on_mutate_or_breed)
		{
			GameController.Instance.breeder_floor_plane.SetActive(false);
			return;
		}
		if (ChunkControl.Instance.player_zone == "overworld")
		{
			ShopControl.Instance.ReEnableNearbyObjects();
		}
		GameplayGUIControl.Instance.txt_GUI_player_combo.text = "";
	}

	public IEnumerator DelayedHideBreeder(float secs)
	{
		yield return new WaitForSeconds(secs);
		base.gameObject.SetActive(false);
	}

	private void RandomizeButtons()
	{
		for (int i = 0; i < 30; i++)
		{
			SwapTwoButtons(UnityEngine.Random.Range(0, buttons.Count), UnityEngine.Random.Range(0, buttons.Count));
		}
		if (!SHOOTING_TRAILER)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		for (int j = 0; j < n_buttons; j++)
		{
			if (!flag && buttons[j].GetComponent<BreedButton>().creature == trailer_animal_1)
			{
				if (j != 8)
				{
					SwapTwoButtons(8, j);
				}
				flag = true;
			}
			if (!flag2 && buttons[j].GetComponent<BreedButton>().creature == trailer_animal_2)
			{
				if (j != 12)
				{
					SwapTwoButtons(12, j);
				}
				flag2 = true;
			}
		}
	}

	private void SwapTwoButtons(int a, int b)
	{
		string creature = buttons[a].GetComponent<BreedButton>().creature;
		Sprite sprite = buttons[a].transform.Find("Image").GetComponent<Image>().sprite;
		Sprite sprite2 = buttons[a].GetComponent<Image>().sprite;
		buttons[a].GetComponent<BreedButton>().creature = buttons[b].GetComponent<BreedButton>().creature;
		buttons[a].transform.Find("Image").GetComponent<Image>().sprite = buttons[b].transform.Find("Image").GetComponent<Image>().sprite;
		buttons[a].GetComponent<Image>().sprite = buttons[b].GetComponent<Image>().sprite;
		buttons[b].GetComponent<BreedButton>().creature = creature;
		buttons[b].transform.Find("Image").GetComponent<Image>().sprite = sprite;
		buttons[b].GetComponent<Image>().sprite = sprite2;
	}

	public void PressTryBreedAgain()
	{
		if (state_t == state.view_companion_hatch)
		{
			if (!Application.isEditor)
			{
				keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
			}
			return;
		}
		GetComponent<Animation>().Play("hide-banners");
		UnityEngine.Object.Destroy(mother);
		StopViewingResult();
		SetState(state.select_father);
		GameController.Instance.SetLightAngleToDadAnimal();
		AdvertControl.Instance.TryShowInterstitialAd(AdvertControl.ad_context.on_breeder);
	}

	private IEnumerator SwitchToMother()
	{
		cam_curr_dest = campos_mothr_gen;
		cam_lerp_speed = 1f;
		yield return new WaitForSeconds(0.5f);
		SetState(state.select_mother);
	}

	private void CreateResultAnimal()
	{
		if (result != null)
		{
			UnityEngine.Object.Destroy(result);
		}
		campos_result.parent.transform.localPosition = elevator_start_pos;
		List<string> list = new List<string>();
		if (SHOOTING_TRAILER && trailer_forced_result != "")
		{
			list.Add(trailer_forced_result);
			list.Add(trailer_forced_result);
		}
		else
		{
			list.Add(father_creature);
			list.Add(mother_creature);
		}
		result = CreatureMorpher.Instance.GetHybridLite(list);
		result.GetComponent<LiteModel>().animation_choppiness = GraphicsControl.Instance.SpecialAnimationChoppiness();
		result.transform.SetParent(campos_result);
		result.transform.localPosition = Vector3.zero;
		SetPerkText();
	}

	private void SetPerkText()
	{
		starting_perk_key = result.GetComponent<LiteModel>().original.start_perk;
		text_perk.text = PerkControl.Instance.GetPerkDataForInfoDisplay(starting_perk_key).full_name;
		ResourceControl.Instance.AssignPerkSprite(starting_perk_key, img_perk);
	}

	public void PressAccept()
	{
		if (!can_interact_with_buttons)
		{
			return;
		}
		can_interact_with_buttons = false;
		HideButtons();
		switch (state_t)
		{
		case state.select_father:
			GameController.Instance.SetLightAngleToMomAnimal();
			StartCoroutine(SwitchToMother());
			break;
		case state.select_mother:
			CreateResultAnimal();
			SetResultText();
			results_anm.Play();
			break;
		case state.select_mutant:
			ShopControl.Instance.mutation_scroll_white.GetComponent<Animation>().Play();
			results_anm.Play();
			break;
		}
	}

	private void SetResultText()
	{
		LiteModel component = result.GetComponent<LiteModel>();
		string creatureNameDeterminer = GetCreatureNameDeterminer(component.original.grammatical_gender);
		string text = "?";
		string grammatical_gender = component.original.grammatical_gender;
		string noun = component.original.noun;
		if (grammatical_gender == "M" || grammatical_gender == "?")
		{
			text = component.original.adjective_M;
		}
		else if (grammatical_gender == "F")
		{
			text = component.original.adjective_F;
		}
		else if (grammatical_gender == "N")
		{
			text = component.original.adjective_N;
		}
		switch (TranslationControl.Instance.use_language)
		{
		case TranslationControl.languages.English:
		case TranslationControl.languages.Russian:
			SetBannerText((text + " " + noun).ToUpper(), creatureNameDeterminer);
			break;
		case TranslationControl.languages.Portuguese:
		case TranslationControl.languages.Indonesian:
		case TranslationControl.languages.Spanish:
			SetBannerText((noun + " " + text).ToUpper(), creatureNameDeterminer);
			break;
		case TranslationControl.languages.Thai:
			SetBannerText(noun + text, creatureNameDeterminer);
			break;
		}
	}

	private string GetCreatureNameDeterminer(string gender)
	{
		switch (TranslationControl.Instance.use_language)
		{
		case TranslationControl.languages.English:
			return "THE";
		case TranslationControl.languages.Russian:
		case TranslationControl.languages.Indonesian:
		case TranslationControl.languages.Thai:
			return "";
		case TranslationControl.languages.Portuguese:
			if (gender == "M" || gender == "?")
			{
				return "O";
			}
			if (gender == "F")
			{
				return "A";
			}
			break;
		case TranslationControl.languages.Spanish:
			if (gender == "M" || gender == "?")
			{
				return "EL";
			}
			if (gender == "F")
			{
				return "LA";
			}
			break;
		}
		return "THE";
	}

	private void SetState(state state_t)
	{
		can_interact_with_buttons = true;
		header_fade.Play();
		RandomizeButtons();
		if (blobble != null)
		{
			StopCoroutine(blobble);
			blobble = null;
		}
		blobble = BobbleButtonsCoroutine();
		StartCoroutine(blobble);
		this.state_t = state_t;
		accept_button.GetComponent<CanvasGroup>().alpha = 1f;
		accept_button.SetActive(false);
		animal_selected_name.transform.parent.GetComponent<CanvasGroup>().alpha = 1f;
		animal_selected_name.text = "";
		button_parent.transform.localPosition = buttons_parent_start_pos;
		bool isBatchMode = Application.isBatchMode;
		buttons_pos buttons_pos;
		switch (state_t)
		{
		case state.select_mutant:
		{
			spawn_mutant.position = spawn_mother.position - spawn_father.position + GameController.Instance.player.transform.position;
			spawn_mutant.position.y = 0f;
			spawn_mutant.rotation = spawn_mother.transform.rotation;
			campos_mutant_gen.position = campos_mothr_gen.position - spawn_father.transform.position + GameController.Instance.player.transform.position;
			campos_mutant_gen.position.y = campos_mothr_gen.position.y;
			campos_mutant_gen.rotation = campos_mothr_gen.rotation;
			Vector3 vector = campos_mothr_gen.position - spawn_mother.transform.position;
			GameController.Instance.player.GetComponent<CreatureBrainLocalPlayer>().StopEverything();
			Quaternion rotation = spawn_father.transform.rotation;
			GameController.Instance.player.GetComponent<SharedCreature>().SnapSpotterRotation(rotation);
			GameController.Instance.player.transform.rotation = rotation;
			cam_curr_dest = campos_mutant_gen;
			cam_lerp_speed = 3f;
			select_Father_text.text = "Pick a Mate";
			select_Father_text.color = col_select_mutant_text;
			foreach (GameObject button in buttons)
			{
				if (button.GetComponent<Image>().sprite != spr_premium_button)
				{
					button.GetComponent<Image>().sprite = spr_mutant_button;
				}
			}
			buttons_pos = buttons_pos.right;
			break;
		}
		case state.select_mother:
			select_Father_text.text = TranslationControl.Instance.TranslateGeneral("Pick a Mom", "GUI");
			select_Father_text.color = col_select_mother_Text;
			foreach (GameObject button2 in buttons)
			{
				if (button2.GetComponent<Image>().sprite != spr_premium_button)
				{
					button2.GetComponent<Image>().sprite = spr_select_mother_button;
				}
			}
			buttons_pos = buttons_pos.right;
			break;
		case state.select_father:
			cam_curr_dest = campos_fathr_gen;
			cam_lerp_speed = 3f;
			select_Father_text.text = TranslationControl.Instance.TranslateGeneral("Pick a Dad", "GUI");
			select_Father_text.color = col_select_father_Text;
			foreach (GameObject button3 in buttons)
			{
				if (button3.GetComponent<Image>().sprite != spr_premium_button)
				{
					button3.GetComponent<Image>().sprite = spr_select_father_button;
				}
			}
			buttons_pos = buttons_pos.left;
			break;
		default:
			buttons_pos = buttons_pos.left;
			break;
		}
		if (curr_buttons_pos != buttons_pos)
		{
			RectTransform[] array = elements_to_reverse;
			foreach (RectTransform r in array)
			{
				ReverseElement(r);
			}
			curr_buttons_pos = buttons_pos;
			animal_selected_name.alignment = ((buttons_pos == buttons_pos.left) ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft);
			if (curr_buttons_pos == buttons_pos.left)
			{
				((RectTransform)accept_button.transform).anchoredPosition = Vector2.right * 30f;
			}
			else
			{
				((RectTransform)accept_button.transform).anchoredPosition = Vector2.left * 30f;
			}
		}
	}

	private void Update()
	{
		if (SHOOTING_TRAILER || RANDOM_MOB_CYCLE)
		{
			if (GamepadInput.Instance.GetKeyDown(Key.R))
			{
				if (result != null)
				{
					UnityEngine.Object.Destroy(result);
				}
				father_creature = CreatureMorpher.Instance.GetRandomCreature();
				if (RANDOM_MOB_CYCLE && random_cycle_force_premium_mother)
				{
					mother_creature = CreatureMorpher.Instance.GetRandomPremiumCreature();
				}
				else
				{
					mother_creature = CreatureMorpher.Instance.GetRandomCreature();
				}
				List<string> list = new List<string>();
				if (RANDOM_MOB_CYCLE && random_cycle_force_father != "")
				{
					list.Add(random_cycle_force_father);
				}
				else
				{
					list.Add(father_creature);
				}
				list.Add(mother_creature);
				result = CreatureMorpher.Instance.GetHybridLite(list);
				result.GetComponent<LiteModel>().animation_choppiness = GraphicsControl.Instance.SpecialAnimationChoppiness();
				result.transform.SetParent(campos_result);
				result.transform.localPosition = Vector3.zero;
				result.transform.localRotation = Quaternion.identity;
				result.GetComponent<LiteModel>().StartAnimation(0);
				father.GetComponent<LiteModel>().original.myName = list[0];
				mother.GetComponent<LiteModel>().original.myName = list[1];
				SetResultText();
				SetPerkText();
			}
			if (GamepadInput.Instance.GetKeyDown(Key.T))
			{
				result_rot_angle = 3f;
				rot_point.position = rotate_around.position + new Vector3(Mathf.Sin(result_rot_angle) * result_view_dist, result_cam_h, Mathf.Cos(result_rot_angle) * result_view_dist);
				rot_point.LookAt(rotate_around.position + Vector3.up * result_cam_view_h);
				GameController.Instance.mainCamera.transform.position = cam_curr_dest.position;
				GameController.Instance.mainCamera.transform.rotation = cam_curr_dest.rotation;
			}
		}
		if (can_interact_with_buttons)
		{
			if (GamepadInput.Instance.GetMouseButtonDown())
			{
				prev_mouse = GamepadInput.Instance.GetMousePosition();
			}
			if (release_button_timer > 0f && GamepadInput.Instance.GetMouseButtonUp())
			{
				SucceedClickButton(attempted_to_click_button);
				attempted_to_click_button = null;
				release_button_timer = 0f;
			}
			if (GamepadInput.Instance.GetMouseButton())
			{
				float num = (GamepadInput.Instance.GetMousePosition().y - prev_mouse.y) / canvas.scaleFactor;
				button_parent.transform.localPosition += new Vector3(0f, num, 0f);
				ClampButtonParent();
				buttons_velocity = num;
			}
			prev_mouse = GamepadInput.Instance.GetMousePosition();
		}
		if (keyboard != null)
		{
			if (keyboard.status == TouchScreenKeyboard.Status.Done)
			{
				CompanionController.Instance.RenameCompanionOnHatch(keyboard.text);
				keyboard = null;
			}
			else if (keyboard.status == TouchScreenKeyboard.Status.Canceled || keyboard.status == TouchScreenKeyboard.Status.LostFocus)
			{
				keyboard = null;
			}
		}
	}

	private void FixedUpdate()
	{
		if (state_t == state.select_mutant && lerp_mutants)
		{
			mutant.transform.position = Vector3.Lerp(mutant.transform.position, mid_mutant.position, Time.fixedDeltaTime * 3f);
			mutant.transform.localScale = Vector3.Lerp(mutant.transform.localScale, Vector3.zero, Time.fixedDeltaTime);
			GameController.Instance.player.transform.position = Vector3.Lerp(GameController.Instance.player.transform.position, mid_mutant.position, Time.fixedDeltaTime * 5f);
			GameController.Instance.player.GetComponent<SharedCreature>().myCreatureModel.gameObject.transform.localScale = Vector3.Lerp(GameController.Instance.player.GetComponent<SharedCreature>().myCreatureModel.gameObject.transform.localScale, Vector3.zero, Time.fixedDeltaTime * 2.5f);
		}
		if (redraw_buttons_timer == 0)
		{
			redraw_buttons_timer = 10;
			foreach (GameObject button in buttons)
			{
				if (0f - button.transform.localPosition.y - button_parent.transform.localPosition.y < 225f)
				{
					ResourceControl.Instance.AssignCreatureSprite(button.GetComponent<BreedButton>().creature, button.transform.Find("Image").GetComponent<Image>());
				}
			}
		}
		redraw_buttons_timer--;
		if (release_button_timer > 0f)
		{
			release_button_timer -= 1f;
		}
		if (viewing_result)
		{
			rot_point.position = rotate_around.position + new Vector3(Mathf.Sin(result_rot_angle) * result_view_dist, result_cam_h, Mathf.Cos(result_rot_angle) * result_view_dist);
			rot_point.LookAt(rotate_around.position + Vector3.up * result_cam_view_h);
		}
		if (view_result_rotate)
		{
			result_rot_angle -= 0.003f;
		}
		if (cam_lerp)
		{
			GameController.Instance.mainCamera.transform.position = Vector3.Lerp(GameController.Instance.mainCamera.transform.position, cam_curr_dest.position, Time.fixedDeltaTime * cam_lerp_speed);
			GameController.Instance.mainCamera.transform.rotation = Quaternion.Lerp(GameController.Instance.mainCamera.transform.rotation, cam_curr_dest.rotation, Time.fixedDeltaTime * cam_lerp_speed);
		}
		if (!GamepadInput.Instance.GetMouseButton())
		{
			button_parent.transform.localPosition += new Vector3(0f, buttons_velocity, 0f);
			buttons_velocity *= 0.94f;
			ClampButtonParent();
		}
	}

	public void TryClickBreedButton(GameObject clicked)
	{
		release_button_timer = 13f;
		attempted_to_click_button = clicked;
	}

	private void SucceedClickButton(GameObject clicked)
	{
		AudioControl.Instance.PlayGenericClick();
		if (prev_button_pressed != null)
		{
			prev_button_pressed.GetComponent<Animation>().Stop();
			prev_button_pressed.transform.localScale = Vector3.one;
		}
		prev_button_pressed = clicked;
		clicked.GetComponent<Animation>().Play("new-breedbutton-click");
		string creature = clicked.GetComponent<BreedButton>().creature;
		switch (state_t)
		{
		case state.select_mutant:
			mutant_creature = creature;
			break;
		case state.select_mother:
			mother_creature = creature;
			prev_mother = creature;
			break;
		case state.select_father:
			father_creature = creature;
			prev_father = creature;
			break;
		}
		accept_button.SetActive(true);
		switch (state_t)
		{
		case state.select_mutant:
			CreateParent(ref mutant, spawn_mutant.position, spawn_mutant.rotation, creature);
			break;
		case state.select_mother:
			CreateParent(ref mother, spawn_mother.transform.position, spawn_mother.transform.rotation, creature);
			break;
		case state.select_father:
			CreateParent(ref father, spawn_father.transform.position, spawn_father.transform.rotation, creature);
			break;
		}
	}

	private void ClampButtonParent()
	{
		if (button_parent.transform.localPosition.y < buttons_parent_start_pos.y)
		{
			button_parent.transform.localPosition = new Vector3(buttons_parent_start_pos.x, buttons_parent_start_pos.y, 0f);
		}
		else if (button_parent.transform.localPosition.y > max_buttons_y)
		{
			button_parent.transform.localPosition = new Vector3(buttons_parent_start_pos.x, max_buttons_y, 0f);
		}
	}

	public void AnimatedViewFirstCreature()
	{
		GameObject gameObject;
		if (state_t == state.select_mutant)
		{
			animal_one_name_.text = PlayerData.Instance.GetSlotString("creatureName", PlayerData.filename_t.general);
			gameObject = mutant;
		}
		else
		{
			animal_one_name_.text = father.GetComponent<LiteModel>().original.myName.ToUpper();
			gameObject = mother;
		}
		animal_two_name_.text = gameObject.GetComponent<LiteModel>().original.myName.ToUpper();
		AnimationFunctionsBreeder.Instance.OnTextWasSet();
		cam_lerp_speed = 3f;
		if (state_t == state.select_mutant)
		{
			float heightValue = GetHeightValue(GameController.Instance.player.GetComponent<SharedCreature>().myCreatureModel.original.height);
			Vector3 position = GameController.Instance.player.transform.position;
			campos_first_gen.position = Vector3.Lerp(position + (campos_first_animal_short.position - spawn_father.position), position + (campos_first_animal_tall.position - spawn_father.position), heightValue);
			campos_first_gen.rotation = Quaternion.Lerp(campos_first_animal_short.rotation, campos_first_animal_tall.rotation, heightValue);
		}
		else
		{
			float heightValue2 = GetHeightValue(father.GetComponent<LiteModel>().original.height);
			campos_first_gen.position = Vector3.Lerp(campos_first_animal_short.position, campos_first_animal_tall.position, heightValue2);
			campos_first_gen.rotation = Quaternion.Lerp(campos_first_animal_short.rotation, campos_first_animal_tall.rotation, heightValue2);
			GameController.Instance.SetLightAngleToDadAnimal();
		}
		cam_curr_dest = campos_first_gen;
	}

	public void AnimatedViewSecondCreature()
	{
		if (state_t == state.select_mutant)
		{
			float heightValue = GetHeightValue(mutant.GetComponent<LiteModel>().original.height);
			campos_second_gen.position = Vector3.Lerp(spawn_mutant.position + (campos_second_animal_short.position - spawn_mother.position), spawn_mutant.position + (campos_second_animal_tall.position - spawn_mother.position), heightValue);
			campos_second_gen.rotation = Quaternion.Lerp(campos_second_animal_short.rotation, campos_second_animal_tall.rotation, heightValue);
		}
		else
		{
			float heightValue2 = GetHeightValue(mother.GetComponent<LiteModel>().original.height);
			campos_second_gen.position = Vector3.Lerp(campos_second_animal_short.position, campos_second_animal_tall.position, heightValue2);
			campos_second_gen.rotation = Quaternion.Lerp(campos_second_animal_short.rotation, campos_second_animal_tall.rotation, heightValue2);
			GameController.Instance.SetLightAngleToMomAnimal();
		}
		cam_curr_dest = campos_second_gen;
	}

	public void AnimatedViewResult()
	{
		if (state_t == state.select_mutant)
		{
			mid_mutant.position = Vector3.Lerp(mutant.transform.position, GameController.Instance.player.transform.position, 0.5f);
			mid_mutant.position.y = SharedCreature.H;
			ViewResult(mid_mutant, -1.5f);
			return;
		}
		result.transform.localRotation = Quaternion.identity;
		if (SHOOTING_TRAILER)
		{
			result.GetComponent<LiteModel>().StartAnimation(0, trailer_animation_speed);
		}
		else
		{
			result.GetComponent<LiteModel>().StartAnimation(0);
		}
		ViewResult(cam_result_rotate_around, 3f);
		GameController.Instance.SetLightAngleToResultAnimal();
	}

	public void AnimatedMergeMutants()
	{
		if (state_t == state.select_mutant)
		{
			lerp_mutants = true;
			bubbles = UnityEngine.Object.Instantiate(type_mutation_bubbles);
			bubbles.transform.position = new Vector3(mid_mutant.position.x, 0f, mid_mutant.position.z);
			AudioControl.Instance.Play(AudioControl.Instance.sfx_mutate);
		}
	}

	public void AdjustCamHeightToCreatureHeight(GameObject target_creature, bool creature_on_floor = false)
	{
		float heightValue = GetHeightValue(target_creature.GetComponent<LiteModel>().original.height);
		float num = (creature_on_floor ? 0.2f : 0f);
		result_cam_h = Mathf.Lerp(0.9f, 0.7f, heightValue) - num;
		result_cam_view_h = Mathf.Lerp(0.2f, 0.7f, heightValue) - num;
		result_view_dist = Mathf.Lerp(2.25f, 2.6f, heightValue);
	}

	public void AnimatedResultCreatureEmergeFromElevator()
	{
		if (state_t != state.select_mutant)
		{
			cam_lerp_speed = 1.5f;
			view_result_rotate = true;
			AdjustCamHeightToCreatureHeight(result);
			ShowBanners(banner_type.breed);
			result_emerge.Play();
		}
	}

	public void SetBannerText(string text, string THE_str = "")
	{
		if (TranslationControl.Instance.use_language == TranslationControl.languages.Russian || TranslationControl.Instance.use_language == TranslationControl.languages.Thai)
		{
			text_result_name.font = translated_font;
		}
		if (SHOOTING_TRAILER && trailer_overwrite_name != "")
		{
			text_result_name.text = trailer_overwrite_name;
			text_THE.SetActive(false);
			return;
		}
		if (RANDOM_MOB_CYCLE)
		{
			text_result_name.text = father.GetComponent<LiteModel>().original.myName + " + " + mother.GetComponent<LiteModel>().original.myName;
			text_THE.SetActive(false);
			return;
		}
		text_result_name.text = text;
		if (THE_str != "")
		{
			text_THE.GetComponent<Text>().text = THE_str;
			text_THE.SetActive(true);
			int num = Mathf.Max(-32 - (int)(text_result_name.preferredWidth * 0.5f), -333);
			text_THE.transform.localPosition = new Vector3(num, text_THE.transform.localPosition.y, text_THE.transform.localPosition.z);
		}
		else
		{
			text_THE.SetActive(false);
		}
	}

	public void ShowBanners(banner_type banner_type_t)
	{
		GetComponent<Animation>().Play("show-banners");
		switch (banner_type_t)
		{
		case banner_type.breed:
		case banner_type.companion:
			banner_button_A.SetActive(true);
			banner_button_B.SetActive(true);
			leveup_button.SetActive(false);
			perk_display.SetActive(true);
			if (banner_type_t == banner_type.breed)
			{
				banner_button_A.transform.Find("Text").GetComponent<Text>().text = TranslationControl.Instance.TranslateGeneral("ACCEPT", "GUI");
				banner_button_B.transform.Find("Text (1)").GetComponent<Text>().text = TranslationControl.Instance.TranslateGeneral("TRY AGAIN", "GUI");
			}
			else
			{
				banner_button_A.transform.Find("Text").GetComponent<Text>().text = "OKAY";
				banner_button_B.transform.Find("Text (1)").GetComponent<Text>().text = "RENAME";
			}
			break;
		case banner_type.mutate:
			banner_button_A.SetActive(false);
			banner_button_B.SetActive(false);
			leveup_button.SetActive(true);
			perk_display.SetActive(false);
			break;
		}
	}

	public void HideBanners()
	{
		if (!on_battle_skipBreed)
		{
			GetComponent<Animation>().Play("hide-banners");
		}
	}

	private void ViewResult(Transform rotate_around, float start_rot)
	{
		FakeTransform fakeTransform = new FakeTransform();
		fakeTransform.position = rotate_around.position;
		fakeTransform.rotation = rotate_around.rotation;
		ViewResult(fakeTransform, start_rot);
	}

	private void ViewResult(FakeTransform rotate_around, float start_rot)
	{
		viewing_result = true;
		this.rotate_around = rotate_around;
		cam_curr_dest = rot_point;
		result_rot_angle = start_rot;
		result_cam_h = 0.6f;
		result_cam_view_h = 0f;
		result_view_dist = 2.4f;
	}

	private void StopViewingResult()
	{
		viewing_result = false;
		view_result_rotate = false;
	}

	public void OnMutateComplete()
	{
		if (state_t != state.select_mutant)
		{
			return;
		}
		lerp_mutants = false;
		UnityEngine.Object.Destroy(mutant);
		UnityEngine.Object.Destroy(bubbles);
		ShowBanners(banner_type.mutate);
		view_result_rotate = true;
		AdjustCamHeightToCreatureHeight(result, true);
		List<string> list = new List<string>();
		foreach (string item in GameController.Instance.player.GetComponent<SharedCreature>().myCreatureModel.original.creatures_that_made_me)
		{
			list.Add(item);
		}
		list.Add(mutant.GetComponent<LiteModel>().original.creatures_that_made_me[0]);
		result = CreatureMorpher.Instance.GetHybridLite(list);
		result.GetComponent<LiteModel>().animation_choppiness = GraphicsControl.Instance.SpecialAnimationChoppiness();
		result.transform.position = new Vector3(mid_mutant.position.x, 0f, mid_mutant.position.z);
		result.transform.rotation = Quaternion.Euler(0f, 220f, 0f);
		result.GetComponent<LiteModel>().StartAnimation(0);
		string text = "?";
		string text2 = "?";
		string text3 = "?";
		TranslationControl.languages use_language = TranslationControl.Instance.use_language;
		switch (list.Count)
		{
		case 3:
			switch (use_language)
			{
			case TranslationControl.languages.English:
				text = "Mutant";
				break;
			case TranslationControl.languages.Russian:
				text = "\u043c\u0443\u0442\u0438\u0440\u043e\u0432\u0430\u0432\u0448\u0438\u0439";
				text2 = "\u043c\u0443\u0442\u0438\u0440\u043e\u0432\u0430\u0432\u0448\u0430\u044f";
				text3 = "\u043c\u0443\u0442\u0438\u0440\u043e\u0432\u0430\u0432\u0448\u0435\u0433\u043e";
				break;
			case TranslationControl.languages.Portuguese:
			case TranslationControl.languages.Spanish:
				text = "Mutante";
				text2 = "Mutante";
				break;
			case TranslationControl.languages.Indonesian:
				text = "Mutan";
				break;
			case TranslationControl.languages.Thai:
				text = "\u0e01\u0e25\u0e32\u0e22\u0e1e\u0e31\u0e19\u0e18\u0e38\u0e4c";
				break;
			}
			break;
		case 4:
			switch (use_language)
			{
			case TranslationControl.languages.English:
				text = "Legendary";
				break;
			case TranslationControl.languages.Russian:
				text = "\u043b\u0435\u0433\u0435\u043d\u0434\u0430\u0440\u043d\u044b\u0439";
				text2 = "\u043b\u0435\u0433\u0435\u043d\u0434\u0430\u0440\u043d\u0430\u044f";
				text3 = "\u043b\u0435\u0433\u0435\u043d\u0434\u0430\u0440\u043d\u043e\u0435";
				break;
			case TranslationControl.languages.Portuguese:
				text = "Lend\u00e1rio";
				text2 = "Lend\u00e1ria";
				break;
			case TranslationControl.languages.Indonesian:
				text = "Legendaris";
				break;
			case TranslationControl.languages.Spanish:
				text = "Legendario";
				text2 = "Legendaria";
				break;
			case TranslationControl.languages.Thai:
				text = "\u0e15\u0e33\u0e19\u0e32\u0e19";
				break;
			}
			break;
		case 5:
			switch (use_language)
			{
			case TranslationControl.languages.English:
				text = "God";
				break;
			case TranslationControl.languages.Russian:
				text = "\u0411\u043e\u0433";
				text3 = "\u0411\u043e\u0433";
				text2 = "\u0411\u043e\u0433";
				break;
			case TranslationControl.languages.Portuguese:
				text = "De Deus";
				text2 = "De Deus";
				break;
			case TranslationControl.languages.Indonesian:
				text = "Tuhan";
				break;
			case TranslationControl.languages.Spanish:
				text = "Dios";
				text2 = "Dios";
				break;
			case TranslationControl.languages.Thai:
				text = "\u0e1e\u0e23\u0e30 \u0e40\u0e08\u0e49\u0e32";
				break;
			}
			break;
		case 6:
			switch (use_language)
			{
			case TranslationControl.languages.English:
				text = "Mythical";
				break;
			case TranslationControl.languages.Russian:
				text = "\u043c\u0438\u0444\u0438\u0447\u0435\u0441\u043a\u0438\u0439";
				text2 = "\u043c\u0438\u0444\u0438\u0447\u0435\u0441\u043a\u0430\u044f";
				text3 = "\u043c\u0438\u0444\u0438\u0447\u0435\u0441\u043a\u043e\u0435";
				break;
			case TranslationControl.languages.Portuguese:
			case TranslationControl.languages.Spanish:
				text = "M\u00edtico";
				text2 = "M\u00edtica";
				break;
			case TranslationControl.languages.Indonesian:
				text = "Mitos";
				break;
			case TranslationControl.languages.Thai:
				text = "\u0e15\u0e4d\u0e32\u0e19\u0e32\u0e19";
				break;
			}
			break;
		case 7:
			switch (use_language)
			{
			case TranslationControl.languages.English:
				text = "Unholy";
				break;
			case TranslationControl.languages.Russian:
				text = "\u0434\u0435\u043c\u043e\u043d\u0438\u0447\u0435\u0441\u043a\u0438\u0439";
				text2 = "\u0434\u0435\u043c\u043e\u043d\u0438\u0447\u0435\u0441\u043a\u0438\u0439";
				text3 = "\u0434\u0435\u043c\u043e\u043d\u0438\u0447\u0435\u0441\u043a\u043e\u0435";
				break;
			case TranslationControl.languages.Portuguese:
			case TranslationControl.languages.Spanish:
				text = "Demon\u00edaco";
				text2 = "Demon\u00edaca";
				break;
			case TranslationControl.languages.Indonesian:
				text = "Jurang";
				break;
			case TranslationControl.languages.Thai:
				text = "\u0e44\u0e21\u0e48\u0e2b\u0e21\u0e01\u0e2b\u0e27\u0e32";
				break;
			}
			break;
		default:
			switch (use_language)
			{
			case TranslationControl.languages.English:
				text = "Supreme";
				break;
			case TranslationControl.languages.Russian:
				text = "\u0432\u0435\u0440\u0445\u043e\u0432\u043d\u044b\u0439";
				text2 = "\u0432\u044b\u0441\u0448\u0430\u044f";
				text3 = "\u0432\u044b\u0441\u0448\u0435\u0435";
				break;
			case TranslationControl.languages.Portuguese:
			case TranslationControl.languages.Spanish:
				text = "Supremo";
				text2 = "Suprema";
				break;
			case TranslationControl.languages.Indonesian:
				text = "Tertinggi";
				break;
			case TranslationControl.languages.Thai:
				text = "\u0e0e\u0e35\u0e01\u0e32";
				break;
			}
			break;
		}
		CreatureModel original = result.GetComponent<LiteModel>().original;
		string creatureNameDeterminer = GetCreatureNameDeterminer(original.grammatical_gender);
		string noun = original.noun;
		string grammatical_gender = original.grammatical_gender;
		string text4 = "???";
		if (!Startup.StringNullOrWhitespace(noun))
		{
			text4 = char.ToUpper(noun[0]) + noun.Substring(1);
		}
		string text5 = ((grammatical_gender == "M" || grammatical_gender == "?") ? text : ((grammatical_gender == "F") ? text2 : ((!(grammatical_gender == "N")) ? "?" : text3)));
		switch (TranslationControl.Instance.use_language)
		{
		case TranslationControl.languages.Portuguese:
		case TranslationControl.languages.Indonesian:
		case TranslationControl.languages.Spanish:
			SetBannerText(text4 + " " + text5, creatureNameDeterminer);
			break;
		case TranslationControl.languages.English:
		case TranslationControl.languages.Russian:
			SetBannerText(text5 + " " + text4, creatureNameDeterminer);
			break;
		case TranslationControl.languages.Thai:
			SetBannerText(text4 + text5, creatureNameDeterminer);
			break;
		}
		UnityEngine.Object.Destroy(GameController.Instance.player.GetComponent<SharedCreature>().myCreatureModel.gameObject);
		GameController.Instance.player.transform.position = new Vector3(mid_mutant.position.x, SharedCreature.H, mid_mutant.position.z);
		GameController.Instance.player.transform.localScale = Vector3.one;
		result.transform.SetParent(GameController.Instance.player.transform.Find("model goes here"));
		result.transform.localPosition = Vector3.zero;
		GameController.Instance.player.GetComponent<SharedCreature>().myCreatureModel = result.GetComponent<LiteModel>();
		GameController.Instance.player.GetComponent<SharedCreature>().OnEquipmentChanged();
		CreatureMorpher.Instance.CreateMutantParticle(GameController.Instance.player.GetComponent<SharedCreature>().myCreatureModel.GetComponent<LiteModel>());
	}

	public void AcceptMutant()
	{
		GameController.Instance.UNPAUSE_GAME();
		EndBreeder();
		result.transform.localRotation = Quaternion.identity;
		GainExtraLevels(12);
		PlayerData.Instance.SetSlotString("creatureName", text_result_name.text, PlayerData.filename_t.general);
		GameController.Instance.player_parent_creatures.Add(mutant_creature);
		GameController.Instance.SaveParentCreaturesToDisk();
		GameServerSender.Instance.SendUpdateParentCreatures();
		InitializeGameplay(true);
		state_t = state.none;
		GameController.Instance.GiveAllOverheads();
		GameplayGUIControl.Instance.ShowGameplayGui();
		StartCoroutine(DelayedHideBreeder(2f));
	}

	private IEnumerator DelayedCompanionAd()
	{
		yield return new WaitForSeconds(1.7f);
		AdvertControl.Instance.TryShowInterstitialAd(AdvertControl.ad_context.FORCED);
	}

	public void SkipBreedScreen(bool player_auto_dead, string start_zone, Vector3 start_position, Action on_interior_model_complete, bool send_respawn)
	{
		elevator.transform.localPosition = pos_elevator_finished.transform.localPosition;
		AudioControl.Instance.PlayIntroMusic();
		GameController.Instance.EnableElevator(false);
		GameController.Instance.ResetVariables();
		LoadEverythingFromDisk(start_position);
		if (send_respawn)
		{
			GameServerSender.Instance.SendRespawn();
		}
		InitializeGameplay(false);
		text_result_name.text = PlayerData.Instance.GetSlotString("creatureName", PlayerData.filename_t.general);
		ZoneData new_zone_data = ZoneDataControl.Instance.LoadZoneDataFromDisk(start_zone);
		ZoneDataControl.Instance.ChangeZone(new_zone_data, ZoneDataControl.change_zone_type.custom_position, start_position, on_interior_model_complete, true, false, true);
		CompanionController.Instance.RecreateAllCompanions();
		if (player_auto_dead)
		{
			UnityEngine.Object.Destroy(GameController.Instance.player);
		}
		StartCoroutine(DelayedHideBreeder(0.1f));
	}

	public void SkipBreedScreenMapEditor()
	{
		elevator.transform.localPosition = pos_elevator_finished.transform.localPosition;
		AudioControl.Instance.PlayIntroMusic();
		GameController.Instance.EnableElevator(false);
		InitializeGameplay(false);
		ZoneDataControl.Instance.ChangeZone(ZoneDataControl.Instance.LoadOverworld(), ZoneDataControl.change_zone_type.custom_position, MapEditorControl.Instance.editor_start_position, null, true, false);
		StartCoroutine(DelayedHideBreeder(0.1f));
	}

	public void LoadEverythingFromDisk(Vector3 player_start_position)
	{
		GameController.Instance.LoadPlayerLevelFromDisk();
		GameplayGUIControl.Instance.text_playerLevel.text = "Level " + GameController.Instance.playerLevel;
		GameController.Instance.LoadSkillPointsSpendableFromDisk();
		GameplayGUIControl.Instance.RedrawSkillPointsSpendableText();
		GameController.Instance.LoadCurrentExpFromDisk();
		GameController.Instance.animate_exp_bar = true;
		GameController.Instance.LoadPlayerStatsFromDisk();
		GameplayGUIControl.Instance.RedrawAllStatNibs();
		GameController.Instance.LoadParentCreaturesFromDisk();
		result = CreatureMorpher.Instance.CreatePlayerCreatureModel(PlayerData.Instance.SLOT);
		result.transform.position = campos_result.position;
		result.transform.SetParent(campos_result);
		result.transform.localRotation = Quaternion.identity;
		GameController.Instance.CreatePlayer(result, campos_result.transform.position, false);
		CreatureMorpher.Instance.CreateMutantParticle(GameController.Instance.player.GetComponent<SharedCreature>().myCreatureModel.GetComponent<LiteModel>());
		GameController.Instance.player.transform.position = player_start_position;
		GameController.Instance.prev_player_pos = GameController.Instance.player.transform.position;
		PerkControl.Instance.LoadPerkLevelsFromDisk();
		PerkControl.Instance.LoadEquippedPerksFromDisk();
		PerkControl.Instance.LoadPerkScreenPositioningFromDisk();
		PerkControl.Instance.LoadGenomesFromDisk();
		PerkControl.Instance.RedrawEquippedPerkSlots();
		inventory_ctr.Instance.LoadInventory();
	}

	public void GainExtraLevels(int n_levelups)
	{
		int num = GameController.Instance.playerLevel;
		int currentEXP = GameController.Instance.currentEXP;
		int num2 = GameController.Instance.NextLevelExp(num);
		int num3 = (int)((1f - (float)currentEXP / (float)num2) * (float)num2) + 2;
		for (int i = 0; i < n_levelups - 1; i++)
		{
			num++;
			num3 += GameController.Instance.NextLevelExp(num);
		}
		GameController.Instance.currentEXP = num3 + currentEXP;
		GameController.Instance.SaveCurrentExpToDisk();
		GameController.Instance.animate_exp_bar = true;
		GameController.Instance.showOverheadNotif("+" + num3 + " Exp!", GameController.Instance.player.transform.position, true, true);
	}

	private void CreateParent(ref GameObject parent, Vector3 position, Quaternion rotation, string creature_name)
	{
		if (parent != null)
		{
			UnityEngine.Object.Destroy(parent);
		}
		parent = CreatureMorpher.Instance.GetHybridLite(creature_name);
		parent.transform.position = position;
		parent.transform.rotation = rotation;
		parent.GetComponent<LiteModel>().StartAnimation(0);
		animal_selected_name.text = FirstCharUpper(parent.GetComponent<LiteModel>().original.myName);
	}

	private void HideButtons()
	{
		header_fade.Play("fade_out_2");
		animal_selected_name.transform.parent.GetComponent<Animation>().Play("fade_out_2");
		accept_button.GetComponent<Animation>().Play("fade_out_2");
		foreach (GameObject button in buttons)
		{
			button.GetComponent<Animation>().Play("new-breedbutton-disappear");
		}
	}

	private IEnumerator BobbleButtonsCoroutine()
	{
		foreach (GameObject button in buttons)
		{
			button.GetComponent<Animation>().Stop();
			button.transform.localScale = Vector3.zero;
		}
		for (int i = 0; i <= 12; i++)
		{
			buttons[i].GetComponent<Animation>().Play("new-breedbutton-appear");
			AudioControl.Instance.PlayPitch(AudioControl.Instance.sfx_bubble, UnityEngine.Random.Range(0.8f, 1f), 0.6f);
			yield return new WaitForSeconds(0.05f);
		}
		for (int j = 13; j < buttons.Count; j++)
		{
			buttons[j].transform.localScale = Vector3.one;
		}
	}

	private List<string> GetPackCreatures(string pack_name)
	{
		return CreatureMorpher.Instance.premium_creature_names[pack_name];
	}

	private void CreateButtons(List<string> creature_names, bool premium)
	{
		for (int i = 0; i < creature_names.Count; i++)
		{
			int num = n_buttons;
			int num2 = (int)((float)num / 3f);
			GameObject gameObject = UnityEngine.Object.Instantiate(prefab_breed_button);
			gameObject.transform.SetParent(button_parent);
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.zero;
			gameObject.transform.localPosition = new Vector3((float)(num - num2 * 3) * 110f - 110f, (float)(-num2) * 110f, 0f);
			gameObject.transform.Find("Image").GetComponent<Image>().enabled = false;
			gameObject.GetComponent<BreedButton>().creature = creature_names[i];
			creatures_created.Add(creature_names[i]);
			if (premium && !SHOOTING_TRAILER)
			{
				gameObject.GetComponent<Image>().sprite = spr_premium_button;
			}
			buttons.Add(gameObject);
			n_buttons++;
		}
	}

	public static string FirstCharUpper(string input)
	{
		return char.ToUpper(input[0]) + input.Substring(1);
	}

	private float GetHeightValue(float height)
	{
		return height * 1.9084f - 0.3416f;
	}

	private float GetScreenRatio()
	{
		return (float)Screen.width / (float)Screen.height * 2.25225f - 3.00225f;
	}

	public void ReverseElement(RectTransform R)
	{
		Vector2 vector = new Vector2(1f - R.anchorMin.x, R.anchorMax.y);
		R.anchorMin = vector;
		R.anchorMax = vector;
		R.anchoredPosition = Vector2.zero;
	}
}
