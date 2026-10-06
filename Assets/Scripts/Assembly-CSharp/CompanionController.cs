using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompanionController : MonoBehaviour, OrderedStart
{
	public static CompanionController Instance;

	public Image healthbar_0;

	public Image healthbar_1;

	public Image healthbar_bg_0;

	public Image healthbar_bg_1;

	public List<ActiveCompanion> active_companions = new List<ActiveCompanion>();

	public GameObject type_animatedEgg;

	public GameObject EGG;

	public string selected_companion_name;

	public Color col_behaviour_tab_selected;

	public Color col_behaviour_tab_deselected;

	private int curr_wait_icon_selected;

	private int behaviour_tab_selected;

	private bool attack_XP_orbs_selected = true;

	public Color col_happy_icon_bg;

	public Color col_happy_icon_text;

	public Color col_happy_icon_bar;

	public Color col_info_icon_bg;

	public Color col_info_icon_text;

	public Color col_info_icon_bar;

	public int max_personal_companions_right_now = 2;

	public List<GameObject> trail_nodes__ = new List<GameObject>();

	public static int max_trail_nodes = 2;

	public GameObject companion_nib_0;

	public GameObject companion_nib_1;

	private int curr_companion_page;

	public Color col_switcher_YES;

	public Color col_switcher_NO;

	public static string default_wait_message1 = "Hello!";

	public static string default_wait_message2 = "Shall I come with you?";

	public static string default_statue_message1 = "It's a statue of somebody ...";

	public static string default_statue_message2 = "... I wonder who it is";

	public static string default_guard_message1 = "Hey boss!";

	public static string default_guard_message2 = "Is my guard duty finished?";

	public static string default_merchant_message1 = "Greetings.";

	public static string default_merchant_message2 = "Can I interest you in some items?";

	public Sprite companion_died_ico;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
		List<InventoryItem> items = LoadCompanionList(PlayerData.filename_t.active_companions);
		for (int i = 0; i < items.Count; i++)
		{
			ActiveCompanion companion = new ActiveCompanion();
			companion.companion_item = items[i];
			active_companions.Add(companion);
			active_companions[i].hatch_index = i;
		}
	}

	public static int WaitIconIdToStateIconId(int wait_icon_id)
	{
		return wait_icon_id == 1 ? 3 : 2;
	}

	public Color GetTextColorFromIcon(int icon_id)
	{
		switch (icon_id)
		{
		case 3:
			return col_info_icon_text;
		case 2:
			return col_happy_icon_text;
		default:
			return default(Color);
		}
	}

	public void PressGuard()
	{
	}

	public bool WasMyGuard(string owner_name)
	{
		return false;
	}

	public void OnGuardDie(string mob_name, string owner_name)
	{
	}

	public void AddDeadCompanion(InventoryItem companion_item)
	{
		List<InventoryItem> items = LoadCompanionList(PlayerData.filename_t.dead_companions);
		if (items.Count > 14) items.RemoveAt(7);
		items.Add(companion_item);
		SaveCompanionList(items, PlayerData.filename_t.dead_companions);
	}

	public void SetCompanionGuiHealth(int index, float percentage)
	{
		if (index > 1) return;
		Image health = index == 0 ? healthbar_0 : index == 1 ? healthbar_1 : null;
		health.rectTransform.sizeDelta = new Vector2(percentage / 100f * healthbar_bg_0.rectTransform.sizeDelta.x, health.rectTransform.sizeDelta.y);
		health.rectTransform.anchoredPosition = new Vector2(health.rectTransform.sizeDelta.x * 0.5f, 0f);
	}

	public void MoveCompanionsToPlayerPosition()
	{
		ClearTrailNodes();
		Vector3 vector = ((!(GameController.Instance.player != null)) ? GameController.Instance.prev_player_pos : GameController.Instance.player.transform.position);
		int num = ZoneDataControl.Instance.curr_zonedata.outer_item_rot;
		if (InventoryUtils.IsCaveObject(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
		{
			num = ((num + 1 < 4) ? (num + 1) : (num - 3));
		}
		foreach (ActiveCompanion active_companion in active_companions)
		{
			if (!(active_companion.obj != null))
			{
				continue;
			}
			if (active_companion.hatch_index < 2)
			{
				switch (num)
				{
				case 0:
				case 2:
					if (active_companion.hatch_index == 1)
					{
						active_companion.obj.transform.position = vector + new Vector3(0f, 0f, 1.3f);
					}
					else if (active_companion.hatch_index == 0)
					{
						active_companion.obj.transform.position = vector + new Vector3(0f, 0f, -1.3f);
					}
					break;
				case 1:
				case 3:
					if (active_companion.hatch_index == 1)
					{
						active_companion.obj.transform.position = vector + new Vector3(1.3f, 0f, 0f);
					}
					else if (active_companion.hatch_index == 0)
					{
						active_companion.obj.transform.position = vector + new Vector3(-1.3f, 0f, 0f);
					}
					break;
				}
			}
			else
			{
				active_companion.obj.transform.position = vector + Vector3.up;
			}
			active_companion.obj.GetComponent<CreatureBrainCompanion>().ForgetTargetsAndFollowPlayer();
		}
	}

	public void PressCompanionButton(int index)
	{
	}

	public void RedrawCompanionNibs()
	{
		companion_nib_0.SetActive(false);
		companion_nib_1.SetActive(false);
		if (active_companions.Count > 0 && active_companions[0].obj != null)
		{
			companion_nib_0.SetActive(true);
			companion_nib_0.transform.Find("name").GetComponent<Text>().text = active_companions[0].companion_name;
			Image component = companion_nib_0.transform.Find("happy").GetComponent<Image>();
			Sprite[] overhead_logos = DevBuildControl.Instance.overhead_logos;
			int wait_icon = active_companions[0].wait_icon;
			component.sprite = overhead_logos[(wait_icon != 0) ? ((wait_icon == 1) ? 3 : 2) : 2];
			if (active_companions[0].wait_icon == 0)
			{
				companion_nib_0.GetComponent<Image>().color = col_happy_icon_bg;
				companion_nib_0.transform.Find("healthbar bg").Find("healthbar").GetComponent<Image>().color = col_happy_icon_bar;
			}
			else if (active_companions[0].wait_icon == 1)
			{
				companion_nib_0.GetComponent<Image>().color = col_info_icon_bg;
				companion_nib_0.transform.Find("healthbar bg").Find("healthbar").GetComponent<Image>().color = col_info_icon_bar;
			}
			int wait_iconb = active_companions[0].wait_icon;
			companion_nib_0.transform.Find("name").GetComponent<Text>().color = ((wait_iconb == 1) ? col_info_icon_text : col_happy_icon_text);
		}
		if (active_companions.Count > 1 && active_companions[1].obj != null)
		{
			companion_nib_1.SetActive(true);
			companion_nib_1.transform.Find("name").GetComponent<Text>().text = active_companions[1].companion_name;
			Image component2 = companion_nib_1.transform.Find("happy").GetComponent<Image>();
			Sprite[] overhead_logos2 = DevBuildControl.Instance.overhead_logos;
			int wait_icon2 = active_companions[1].wait_icon;
			component2.sprite = overhead_logos2[(wait_icon2 != 0) ? ((wait_icon2 == 1) ? 3 : 2) : 2];
			if (active_companions[1].wait_icon == 0)
			{
				companion_nib_1.GetComponent<Image>().color = col_happy_icon_bg;
				companion_nib_1.transform.Find("healthbar bg").Find("healthbar").GetComponent<Image>().color = col_happy_icon_bar;
			}
			else if (active_companions[1].wait_icon == 1)
			{
				companion_nib_1.GetComponent<Image>().color = col_info_icon_bg;
				companion_nib_1.transform.Find("healthbar bg").Find("healthbar").GetComponent<Image>().color = col_info_icon_bar;
			}
			int wait_icon2b = active_companions[1].wait_icon;
			companion_nib_1.transform.Find("name").GetComponent<Text>().color = ((wait_icon2b == 1) ? col_info_icon_text : col_happy_icon_text);
		}
	}

	public void PrevPage()
	{
	}

	public void NextPage()
	{
	}

	private void RedrawPage(int dir)
	{
	}

	public void PressCommandAttack()
	{
	}

	public void PressCommandWalk()
	{
	}

	public void PressCommandItems()
	{
	}

	public void PressCommandWait()
	{
	}

	public void PressCommandRename()
	{
	}

	public void PressCommandAdvanced()
	{
	}

	public void PressOptionAttackExpOrb()
	{
	}

	private void RedrawAttackExpOrbSwitcher()
	{
	}

	public void PressAdvancedTab(int index)
	{
	}

	public void PressChangeCompanionWaitIcon(int dir)
	{
	}

	private void RedrawWaitLogo()
	{
	}

	public void PressBackOnAdvanced(bool save)
	{
	}

	public void PressCommandComingSoon()
	{
	}

	public void PressCommandMerchant()
	{
	}

	public void PressBackOnRename()
	{
	}

	public void PressAcceptOnRename()
	{
	}

	public void RenameCompanionManually(string rename_to)
	{
	}

	public void CreateSingleCompanion(ActiveCompanion companion)
	{
		Vector3 position = GameController.Instance.player != null ? GameController.Instance.player.transform.position : GameController.Instance.prev_player_pos;
		if (companion.hatch_index == 0) position += Vector3.left * 1.8f;
		else if (companion.hatch_index == 1) position += Vector3.forward * 1.8f;
		else position += Vector3.right * 1.8f * (companion.hatch_index - 1);
		CreateSingleCompanion(companion, position);
	}

	public void CreateSingleCompanion(ActiveCompanion companion, Vector3 V)
	{
		MobControl.Instance.SpawnCompanion(V, companion);
		SetCompanionGuiHealth(companion.hatch_index, 100f);
		RedrawCompanionNibs();
	}

	public void CompanionPocketsClosed(BasketContents companion_pockets)
	{
	}

	public void DeleteAllActiveCompanions()
	{
		active_companions.Clear();
		SaveActiveCompanions();
	}

	public void DestroyActiveCompanion(ActiveCompanion companion)
	{
		if (companion == null) return;
		RemoveActiveCompanionAt(companion.hatch_index);
		if (companion.obj != null) Object.Destroy(companion.obj);
		RedrawCompanionNibs();
		GameServerSender.Instance.SendDestroyCompanion(companion.combat_name);
	}

	public ActiveCompanion GetCurrSelectedCompanion()
	{
		foreach (ActiveCompanion companion in active_companions)
			if (companion.companion_name == selected_companion_name) return companion;
		return null;
	}

	public void RecreateAllCompanions()
	{
		List<InventoryItem> waiting = LoadCompanionList(PlayerData.filename_t.suspended_companions);
		List<ActiveCompanion> remove = new List<ActiveCompanion>();
		int count = 0;
		foreach (ActiveCompanion companion in active_companions)
		{
			if (count >= max_personal_companions_right_now)
			{
				remove.Add(companion);
				waiting.Add(companion.companion_item);
			}
			count++;
		}
		foreach (ActiveCompanion companion in remove) DestroyActiveCompanion(companion);
		if (count < max_personal_companions_right_now && waiting.Count > 0)
		{
			List<InventoryItem> added = new List<InventoryItem>();
			foreach (InventoryItem item in waiting)
			{
				ActiveCompanion companion = new ActiveCompanion();
				companion.companion_item = item;
				companion.hatch_index = active_companions.Count;
				active_companions.Add(companion);
				added.Add(item);
				if (active_companions.Count >= max_personal_companions_right_now) break;
			}
			foreach (InventoryItem item in added) waiting.Remove(item);
		}
		SaveActiveCompanions();
		SaveCompanionList(waiting, PlayerData.filename_t.suspended_companions);
		foreach (ActiveCompanion companion in active_companions)
			if (companion.obj == null) CreateSingleCompanion(companion);
	}

	public void AcceptCompanionFollow()
	{
	}

	public void AddTempCompanion(string creatureA, string creatureB, int start_lvl, string companion_name, InventoryItem hat_, InventoryItem body_, InventoryItem hand_)
	{
	}

	public void DestroyTempCompanions()
	{
		int num = 0;
		foreach (ActiveCompanion active_companion in active_companions)
		{
			num += (active_companion.is_temp_companion ? 1 : 0);
		}
		for (int i = 0; i < num; i++)
		{
			int j;
			for (j = 0; !active_companions[j].is_temp_companion; j++)
			{
			}
			ActiveCompanion activeCompanion = active_companions[j];
			RemoveActiveCompanionAt(activeCompanion.hatch_index);
			if (activeCompanion.obj != null)
			{
				Object.Destroy(activeCompanion.obj);
			}
		}
	}

	public void AcceptFreeCompanion()
	{
	}

	private void SaveCompanionList(List<InventoryItem> companion_item_list, PlayerData.filename_t filename_t)
	{
		string filename = PlayerData.Instance.GetFilenameString(filename_t);
		PlayerData.Instance.SetSlotShort("n_entries", companion_item_list.Count, filename_t, "default", -1);
		for (int i = 0; i < companion_item_list.Count; i++) companion_item_list[i].SaveToDisk(filename, "companion_" + i, "default");
	}

	public List<InventoryItem> LoadCompanionList(PlayerData.filename_t filename_t)
	{
		List<InventoryItem> items = new List<InventoryItem>();
		string filename = PlayerData.Instance.GetFilenameString(filename_t);
		short count = PlayerData.Instance.GetSlotShort("n_entries", filename, "default", -1);
		for (int i = 0; i < count; i++) items.Add(InventoryItem.LoadFromDisk("companion_" + i, filename, "default"));
		return items;
	}

	public void RenameCompanionOnHatch(string input)
	{
		BreedControl.Instance.text_result_name.text = input;
		ActiveCompanion companion = active_companions[active_companions.Count - 1];
		ExtraInventoryData data = companion.companion_item.GetExtraDataCopy();
		data.SetString("npc_display_name", input);
		companion.companion_item = new InventoryItem(companion.companion_item.item_name, data);
		SaveActiveCompanions();
		Color color = MobControl.Instance.GetOverheadNameColor(companion.creature_A + companion.creature_B);
		companion.obj.GetComponent<SharedCreature>().AssignOverheadName(input, color);
		RedrawCompanionNibs();
		GameServerSender.Instance.SendRenameCompanion(companion.combat_name, input);
	}

	public void SaveActiveCompanions()
	{
		List<InventoryItem> items = new List<InventoryItem>();
		foreach (ActiveCompanion companion in active_companions)
			if (!companion.is_temp_companion) items.Add(companion.companion_item);
		SaveCompanionList(items, PlayerData.filename_t.active_companions);
	}

	private void FixedUpdate()
	{
		if (GameController.Instance.player == null) return;
		if (trail_nodes__.Count == 0)
		{
			GameObject node = new GameObject("trail-node");
			node.transform.position = GameController.Instance.player.transform.position;
			trail_nodes__.Add(node);
			return;
		}
		if (Vector3.Distance(GameController.Instance.player.transform.position, trail_nodes__[0].transform.position) <= 1.3f) return;
		GameObject next = new GameObject("trail-node");
		next.transform.position = GameController.Instance.player.transform.position;
		trail_nodes__.Insert(0, next);
		if (trail_nodes__.Count > active_companions.Count * 2 + 2)
		{
			GameObject last = trail_nodes__[trail_nodes__.Count - 1];
			trail_nodes__.Remove(last);
			Object.Destroy(last);
		}
	}

	public void ClearTrailNodes()
	{
		foreach (GameObject item in trail_nodes__)
		{
			UnityEngine.Object.Destroy(item);
		}
		trail_nodes__.Clear();
	}

	public void CreateAnimatedEgg(int critterLevel, bool paid, string animal1 = "", string animal2 = "")
	{
		Quaternion rotation = Quaternion.Euler(0f, 268f, 0f);
		GameController.Instance.player.transform.rotation = rotation;
		GameController.Instance.player.GetComponent<SharedCreature>().SnapSpotterRotation(rotation);
		EGG = Object.Instantiate(type_animatedEgg);
		int index = active_companions.Count * 2 + 1;
		if (index >= trail_nodes__.Count) index = trail_nodes__.Count - 1;
		Vector3 position = trail_nodes__[index].transform.position;
		position.y = 0.1f;
		EGG.transform.position = position;
		StartCoroutine(AnimatedEggCoroutine(EGG, critterLevel, animal1, animal2));
		BreedControl.Instance.paid_companion = paid;
		BreedControl.Instance.state_t = BreedControl.state.view_companion_hatch;
		BreedControl.Instance.gameObject.SetActive(true);
		BreedControl.Instance.TransitionBackToBreeder(BreedControl.breeder_transition.on_companion);
	}

	private IEnumerator AnimatedEggCoroutine(GameObject EGG, int critterLevel, string animal1 = "", string animal2 = "")
	{
		BreedControl.Instance.view_result_rotate = true;
		yield return new WaitForSeconds(1f);
		EGG.GetComponent<Animation>().Play();
		yield return new WaitForSeconds(2.5f);
		EGG.GetComponent<CompanionEgg>().VisuallyCrackEgg();
		GameController.Instance.GetComponent<GameController>().sound_crackshell();
		GameController.Instance.GetComponent<GameController>().animation_sound_levelScreenAppear();
		string first = "";
		string path = "";
		switch (TranslationControl.Instance.use_language)
		{
		case TranslationControl.languages.English: path = "Lists/Companions - English Names"; break;
		case TranslationControl.languages.Russian: path = "lang-Russian/Russian-CommonNames"; break;
		case TranslationControl.languages.Portuguese: path = "lang-Portuguese/Portuguese-CommonNames"; break;
		case TranslationControl.languages.Indonesian: path = "lang-Indonesian/Indonesian-CommonNames"; break;
		case TranslationControl.languages.Spanish: path = "lang-Spanish/Spanish-CommonNames"; break;
		case TranslationControl.languages.Thai: path = "lang-Thai/Thai-CommonNames"; break;
		}
		bool exists = false;
		List<string> lines = ResourceControl.Instance.GetTextFileLines(path, ref exists);
		if (exists)
		{
			List<string> names = new List<string>();
			foreach (string line in lines) if (!Startup.StringNullOrWhitespace(line)) names.Add(line);
			first = names[Random.Range(0, names.Count)];
		}
		string descriptor = "";
		exists = false;
		lines = ResourceControl.Instance.GetTextFileLines("Lists/Companions - Descriptors", ref exists);
		if (exists)
		{
			List<string> names = new List<string>();
			foreach (string line in lines) if (!Startup.StringNullOrWhitespace(line)) names.Add(line);
			descriptor = names[Random.Range(0, names.Count)];
			if (TranslationControl.Instance.use_language != TranslationControl.languages.English) descriptor = TranslationControl.Instance.TranslateGeneral(descriptor, "CommonDescriptors");
		}
		string name = "???";
		switch (TranslationControl.Instance.use_language)
		{
		case TranslationControl.languages.English:
		case TranslationControl.languages.Russian: name = descriptor + " " + first; break;
		case TranslationControl.languages.Portuguese:
		case TranslationControl.languages.Indonesian:
		case TranslationControl.languages.Spanish: name = first + " " + descriptor; break;
		case TranslationControl.languages.Thai: name = first + descriptor; break;
		}
		BreedControl.Instance.SetBannerText(name, "");
		BreedControl.Instance.ShowBanners(BreedControl.banner_type.companion);
		if (animal1 == "") animal1 = CreatureMorpher.Instance.GetRandomCreature();
		if (animal2 == "") animal2 = CreatureMorpher.Instance.GetRandomCreature();
		string combat = ShopControl.RandomString();
		int pocket = ConstructionControl.Instance.GetNewUniqueId(true);
		ActiveCompanion companion = new ActiveCompanion();
		companion.companion_item = ActiveCompanion.CreateNewItem(animal1, animal2, 5, 0, GameController.Instance.NextLevelExp(5), combat, name, pocket, TranslationControl.Instance.TranslateGeneral(default_wait_message1, "CompanionsEtc"), TranslationControl.Instance.TranslateGeneral(default_wait_message2, "CompanionsEtc"), "", "", TranslationControl.Instance.TranslateGeneral(default_guard_message1, "CompanionsEtc"), TranslationControl.Instance.TranslateGeneral(default_guard_message2, "CompanionsEtc"), 0, false, new InventoryItem(""), new InventoryItem(""), new InventoryItem(""), TranslationControl.Instance.TranslateGeneral(default_merchant_message1, "Merchants"), TranslationControl.Instance.TranslateGeneral(default_merchant_message2, "Merchants"));
		companion.hatch_index = active_companions.Count;
		active_companions.Add(companion);
		SaveActiveCompanions();
		CreateSingleCompanion(companion, EGG.transform.position);
		GameServerSender.Instance.SendCreatedLocalMob(combat);
		yield return new WaitForSeconds(0.02f);
		BreedControl.Instance.AdjustCamHeightToCreatureHeight(companion.obj.GetComponent<SharedCreature>().myCreatureModel.gameObject, false);
		companion.obj.GetComponent<SharedCreature>().myCreatureModel.StartAnimation(0, -1f);
		yield return new WaitForSeconds(2.5f);
	}

	public void IncreaseCompanionExp(int amount, ActiveCompanion the_companion)
	{
		int exp = the_companion.curr_exp + amount;
		int next = the_companion.next_exp;
		int level = the_companion.level;
		if (exp >= next)
		{
			do
			{
				the_companion.obj.GetComponent<SharedCreature>().level++;
				level++;
				int new_next = GameController.Instance.NextLevelExp(level);
				the_companion.obj.GetComponent<SharedCreature>().ReCalcHpMaxAndHpRegen();
				the_companion.obj.GetComponent<Combatant>().GainSomeHPOnLevelup();
				exp -= next;
				GameServerSender.Instance.SendUpdateCreatureStats(the_companion.obj.GetComponent<Combatant>().combat_name, "");
				next = new_next;
			} while (exp >= next);
			the_companion.obj.GetComponent<SharedCreature>().ShowLevelupParticles(true, 0.6f);
			the_companion.obj.GetComponent<SharedCreature>().RedrawLevelText();
		}
		ExtraInventoryData data = the_companion.companion_item.GetExtraDataCopy();
		data.SetShort("curr_exp", exp);
		data.SetShort("next_exp", next);
		data.SetLong("level", level);
		the_companion.companion_item = new InventoryItem(the_companion.companion_item.item_name, data);
		SaveActiveCompanions();
	}

	public void CompanionDeath(ActiveCompanion companion)
	{
		if (!companion.is_temp_companion) AddDeadCompanion(companion.companion_item);
		RemoveActiveCompanionAt(companion.hatch_index);
		if (WindowControl.Instance.curr_miniwindow == WindowControl.miniwindow_type_t.companion_commands) WindowControl.Instance.CloseMiniwindow(true);
		GameplayGUIControl.Instance.ShowNotif("<color=#aaaaaa>" + companion.companion_name + " died</color>", companion_died_ico, new OnNotifClick(OnNotifClick.type.none));
		RedrawCompanionNibs();
	}

	private void RemoveActiveCompanionAt(int X)
	{
		for (int i = X + 1; i < active_companions.Count; i++)
		{
			active_companions[i].hatch_index--;
		}
		active_companions.RemoveAt(X);
		SaveActiveCompanions();
	}
}
