using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PerkControl : MonoBehaviour, OrderedStart
{
	private struct pollinator_summary_entry
	{
		public int level;

		public string item;
	}

	public enum perk_effect_type
	{
		unknown = 0,
		Instant_effect = 1,
		Duration_effect = 2,
		Area_effect = 3,
		Drop_effect = 4
	}

	public static PerkControl Instance;

	public int genomes;

	public Dictionary<string, PerkData> loaded_perks = new Dictionary<string, PerkData>();

	private PerkData null_perk;

	private int n_infinitely_levelable_perks = -1;

	public Sprite perk_not_loaded;

	public Color unavailable_for_casting;

	public Color available_for_casting;

	public AudioSource sfx_source;

	public AudioClip maxout;

	public AudioClip resolve;

	public GameObject prefab_darksword_kill;

	public GameObject prefab_aether_banish;

	public int about_to_cast_lvl;

	public PerkData about_to_cast_data;

	public GameObject slotA_main;

	public GameObject slotB_main;

	public GameObject curr_sel_perk;

	public string curr_sel_perk_key;

	public float mana_modifier;

	public float mana_available;

	public Text mana_text;

	public Image mana_foreground;

	public Image mana_mask;

	private Vector2 mana_mask_start_pos;

	private Vector2 mana_overlay_start_pos;

	public GameObject mana_meter;

	public int perk_screen_x;

	public int perk_screen_y;

	public string perk_slot_A = "";

	public string perk_slot_B = "";

	public Dictionary<string, int> perk_levels = new Dictionary<string, int>();

	public void Start_0()
	{
		Instance = this;
		genomes = 0;
		mana_modifier = 1f;
		mana_available = 100f;
		null_perk = new PerkData();
		null_perk.mana_cost_str = "0%";
	}

	public void Start_1()
	{
		mana_mask_start_pos = mana_mask.transform.localPosition;
		mana_overlay_start_pos = mana_foreground.transform.localPosition;
		StartCoroutine(RegenerateMana());
	}

	public static float GetManaMaxPlayer(int stat_6_lvl)
	{
		return 0f;
	}

	public PerkData GetPerkDataForInfoDisplay(string perk_key)
	{
		if (perk_key == "")
		{
			return null_perk;
		}
		if (!loaded_perks.ContainsKey(perk_key))
		{
			LoadPerkFromDisk(perk_key);
		}
		return loaded_perks[perk_key];
	}

	public PerkData ClonePerkForCasting(string perk_key, GameObject caster)
	{
		if (perk_key == "")
		{
			return null_perk;
		}
		if (!loaded_perks.ContainsKey(perk_key))
		{
			LoadPerkFromDisk(perk_key);
		}
		PerkData perkData = loaded_perks[perk_key];
		PerkData perkData2 = new PerkData();
		perkData2.original_key = perkData.original_key;
		perkData2.full_name = perkData.full_name;
		perkData2.description = perkData.description;
		perkData2.detailed_description = perkData.detailed_description;
		perkData2.large_upgrade_description_box = perkData.large_upgrade_description_box;
		perkData2.ultra_detailed_description = perkData.ultra_detailed_description;
		perkData2.mana_cost_str = perkData.mana_cost_str;
		perkData2.max_level = perkData.max_level;
		perkData2.unlock_prerequisite_perk_level = perkData.unlock_prerequisite_perk_level;
		perkData2.unlock_prerequisite_perk_key = perkData.unlock_prerequisite_perk_key;
		foreach (InitialCastCommand item in perkData.on_initial_cast)
		{
			InitialCastCommand initialCastCommand = new InitialCastCommand();
			foreach (string effect_name in item.effect_names)
			{
				initialCastCommand.effect_names.Add(effect_name);
			}
			initialCastCommand.projectile_model = item.projectile_model;
			initialCastCommand.type = item.type;
			initialCastCommand.projectile_target_type = item.projectile_target_type;
			perkData2.on_initial_cast.Add(initialCastCommand);
		}
		foreach (KeyValuePair<string, SinglePerkEffect> all_effect in perkData.all_effects)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (KeyValuePair<string, string> datum in all_effect.Value.data)
			{
				string value = datum.Value;
				if (datum.Key == "Particle (Duration)" && value == "[RAND_BIOME]")
				{
					value = GetRandomBiomeParticle(caster);
				}
				dictionary.Add(datum.Key, value);
			}
			perkData2.all_effects.Add(all_effect.Key, new SinglePerkEffect(dictionary));
		}
		return perkData2;
	}

	private string GetRandomBiomeParticle(GameObject caller)
	{
		string text;
		if (ChunkControl.Instance.player_zone == "overworld")
		{
			int overworldBiomeBelow = ChunkControl.Instance.GetOverworldBiomeBelow(caller, true);
			text = ChunkControl.Instance.biomes[overworldBiomeBelow].biome_scenic[UnityEngine.Random.Range(0, ChunkControl.Instance.biomes[overworldBiomeBelow].biome_scenic.Length)].item_name;
		}
		else if (InventoryUtils.IsCaveObject(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
		{
			ChunkControl.cave_define correspondingCave = ChunkControl.Instance.GetCorrespondingCave(ZoneDataControl.Instance.curr_zonedata.house_item.item_name);
			List<string> list = new List<string>();
			ChunkControl.cave_mineral_pairs[] possible_mineral_pairs = correspondingCave.possible_mineral_pairs;
			for (int i = 0; i < possible_mineral_pairs.Length; i++)
			{
				ChunkControl.cave_mineral_pairs cave_mineral_pairs = possible_mineral_pairs[i];
				if (cave_mineral_pairs.small_mineral != "")
				{
					list.Add(cave_mineral_pairs.small_mineral);
				}
				if (cave_mineral_pairs.large_mineral != "")
				{
					list.Add(cave_mineral_pairs.large_mineral);
				}
			}
			possible_mineral_pairs = correspondingCave.possible_SUPER_RARE_minerals;
			for (int i = 0; i < possible_mineral_pairs.Length; i++)
			{
				ChunkControl.cave_mineral_pairs cave_mineral_pairs2 = possible_mineral_pairs[i];
				if (cave_mineral_pairs2.small_mineral != "")
				{
					list.Add(cave_mineral_pairs2.small_mineral);
				}
				if (cave_mineral_pairs2.large_mineral != "")
				{
					list.Add(cave_mineral_pairs2.large_mineral);
				}
			}
			possible_mineral_pairs = correspondingCave.possible_gem_pairs;
			for (int i = 0; i < possible_mineral_pairs.Length; i++)
			{
				ChunkControl.cave_mineral_pairs cave_mineral_pairs3 = possible_mineral_pairs[i];
				if (cave_mineral_pairs3.small_mineral != "")
				{
					list.Add(cave_mineral_pairs3.small_mineral);
				}
				if (cave_mineral_pairs3.large_mineral != "")
				{
					list.Add(cave_mineral_pairs3.large_mineral);
				}
			}
			if (correspondingCave.shroom_obj_name != "")
			{
				list.Add(correspondingCave.shroom_obj_name);
			}
			if (correspondingCave.giant_shroom_obj_name != "")
			{
				list.Add(correspondingCave.giant_shroom_obj_name);
			}
			if (correspondingCave.stalagmite_name != "")
			{
				list.Add(correspondingCave.stalagmite_name);
			}
			if (correspondingCave.small_stalagmite_name != "")
			{
				list.Add(correspondingCave.small_stalagmite_name);
			}
			list.Add("Cave Basket");
			list.Add("Cave Chest");
			list.Add("Old Torch");
			list.Add("Spawner - Bones");
			list.Add("Spawner - Ancient Bones");
			list.Add("Spawner - Fossils");
			text = list[UnityEngine.Random.Range(0, list.Count)];
		}
		else if (InventoryUtils.IsHeavenDimension(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
		{
			List<string> list2 = new List<string>();
			list2.Add("Sky Chest");
			list2.Add("Stone Vein (White)");
			text = list2[UnityEngine.Random.Range(0, list2.Count)];
		}
		else if (InventoryUtils.IsPureDimension(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
		{
			int num = ((ZoneDataControl.Instance.curr_zonedata.house_item.item_name == "Pocket World Snow") ? 1 : ((ZoneDataControl.Instance.curr_zonedata.house_item.item_name == "Pocket World Evergreen") ? 3 : 0));
			text = ChunkControl.Instance.biomes[num].biome_scenic[UnityEngine.Random.Range(0, ChunkControl.Instance.biomes[num].biome_scenic.Length)].item_name;
		}
		else
		{
			bool num2 = InventoryUtils.IsHellDimension(ZoneDataControl.Instance.curr_zonedata.house_item.item_name);
			List<string> list3 = new List<string>();
			if (num2)
			{
				list3.Add("Small Black Stalagmite");
				list3.Add("Large Black Stalagmite");
				list3.Add("Old Torch");
				list3.Add("Magmite Vein");
			}
			else
			{
				list3.Add("Anvil");
				list3.Add("Basket");
				list3.Add("Big Stone Head");
				list3.Add("Big Table");
				list3.Add("Bonsai Tree");
				list3.Add("Chair");
				list3.Add("Chest");
				list3.Add("Circular Rug");
				list3.Add("Crafting Table");
				list3.Add("Crate");
				list3.Add("Egg Fuser");
				list3.Add("Fireplace");
				list3.Add("Hunter Rug");
				list3.Add("Karaoke");
				list3.Add("Large Weapon Display");
				list3.Add("Loom");
				list3.Add("Metal Chair");
				list3.Add("Metal Lamp Post");
				list3.Add("Music Box");
				list3.Add("Oven");
				list3.Add("Paint Mixer");
				list3.Add("Paint Shaker");
				list3.Add("Pool Table");
				list3.Add("Sign");
				list3.Add("Small Table");
				list3.Add("Sofa Chair");
				list3.Add("Stamp Maker");
				list3.Add("Throne");
				list3.Add("Torch");
				list3.Add("Underground Room");
				list3.Add("Upstairs Room");
				list3.Add("Vending Machine");
				list3.Add("Weapon Display");
			}
			text = list3[UnityEngine.Random.Range(0, list3.Count)];
		}
		return "[item]" + text;
	}

	public static void GeneratePollinatorSummary()
	{
	}

	public static void GeneratePerkList()
	{
	}

	private void LoadPerkFromDisk(string perk_key)
	{
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("PerkDefines/" + perk_key, ref file_exists);
		if (!file_exists)
		{
			return;
		}
		PerkData perk_data = new PerkData();
		perk_data.original_key = perk_key;
		string effect_name = "";
		Dictionary<string, string> dictionary = null;
		foreach (string item in textFileLines)
		{
			if (Startup.StringNullOrWhitespace(item) || item == "-")
			{
				continue;
			}
			if (item[0] == '[')
			{
				if (dictionary != null)
				{
					perk_data.AddEffect(effect_name, dictionary);
				}
				effect_name = item.Replace("[", "").Replace("]", "");
				dictionary = new Dictionary<string, string>();
				continue;
			}
			int num = item.IndexOf('=');
			string text = item.Substring(0, num - 1);
			string text2 = item.Substring(num + 2, item.Length - (num + 2));
			if (dictionary == null)
			{
				switch (text)
				{
				case "Full Name":
					perk_data.full_name = text2;
					break;
				case "Description":
					perk_data.description = text2;
					break;
				case "Detailed Description":
					perk_data.detailed_description = text2;
					break;
				case "Large Upgrade Description Box":
					perk_data.large_upgrade_description_box = text2 == "true";
					break;
				case "Ultra Detailed Description":
					perk_data.ultra_detailed_description = text2;
					break;
				case "Max level":
					perk_data.max_level = int.Parse(text2, Startup.parse_culture);
					break;
				case "Unlock Prerequisite":
				{
					int num2 = text2.IndexOf("(lvl.");
					perk_data.unlock_prerequisite_perk_level = int.Parse(text2.Substring(num2 + 5, text2.Length - num2 - 6), Startup.parse_culture);
					perk_data.unlock_prerequisite_perk_key = text2.Substring(0, num2);
					break;
				}
				case "Energy Cost":
					perk_data.mana_cost_str = text2;
					break;
				case "Not_unlockable":
					if (text2 == "true")
					{
						perk_data.not_unlockable = true;
					}
					break;
				case "On cast":
					ParseInitialCastCommand(text2, ref perk_data);
					break;
				}
			}
			else
			{
				dictionary.Add(text, text2);
			}
		}
		if (dictionary != null)
		{
			perk_data.AddEffect(effect_name, dictionary);
		}
		if (!loaded_perks.ContainsKey(perk_key))
		{
			loaded_perks.Add(perk_key, perk_data);
		}
	}

	public float GetAveragePerkLevel(int player_level)
	{
		return (float)player_level / 6f / (float)GetNumInfinitelyLevelablePerks();
	}

	public int GetNumInfinitelyLevelablePerks()
	{
		int result = n_infinitely_levelable_perks;
		if (n_infinitely_levelable_perks == -1)
		{
			bool file_exists = false;
			List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("AutoGen/(Auto Gen) Perks N Infinitely Levelable", ref file_exists);
			result = ((!file_exists) ? 1 : int.Parse(textFileLines[0], Startup.parse_culture));
		}
		return result;
	}

	public bool PerkExists(string perk_key)
	{
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("AutoGen/(Auto Gen) Perks List", ref file_exists);
		if (!file_exists)
		{
			return false;
		}
		foreach (string item in textFileLines)
		{
			if (!Startup.StringNullOrWhitespace(item) && item == perk_key)
			{
				return true;
			}
		}
		return false;
	}

	private void ParseInitialCastCommand(string suffix, ref PerkData perk_data)
	{
		InitialCastCommand initialCastCommand = new InitialCastCommand();
		List<char> list = new List<char>();
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		for (int i = 0; i < suffix.Length; i++)
		{
			char c = suffix[i];
			if (initialCastCommand.effect_names.Count == 0)
			{
				if (!flag3)
				{
					flag3 = c == '[';
				}
				else if (c == ']')
				{
					initialCastCommand.effect_names = new List<string>(new string(list.ToArray()).Split(','));
					flag3 = false;
					list.Clear();
				}
				else
				{
					list.Add(suffix[i]);
					flag3 = true;
				}
			}
			else if (flag)
			{
				if (c != ')')
				{
					list.Add(suffix[i]);
					flag = true;
					continue;
				}
				string text = new string(list.ToArray()).Replace("target=", "");
				if (text == "enemies")
				{
					initialCastCommand.projectile_target_type = InitialCastCommand.projectile_target.enemies;
				}
				else if (text == "allies")
				{
					initialCastCommand.projectile_target_type = InitialCastCommand.projectile_target.allies;
				}
				flag = false;
				list.Clear();
			}
			else if (flag2)
			{
				if (c == ')' || suffix[i] == ',')
				{
					initialCastCommand.projectile_model = new string(list.ToArray()).Replace("(model=", "");
					list.Clear();
					flag = suffix[i] == ',';
					flag2 = false;
				}
				else
				{
					list.Add(suffix[i]);
					flag = false;
					flag2 = true;
				}
			}
			else if (c != ',')
			{
				list.Add(suffix[i]);
				string text2 = new string(list.ToArray());
				if (text2 == " on self")
				{
					flag = false;
					flag2 = false;
					initialCastCommand.type = InitialCastCommand.initial_cast_type.on_self;
					list.Clear();
				}
				else if (text2 == " on quick_tag")
				{
					flag2 = false;
					flag = false;
					initialCastCommand.type = InitialCastCommand.initial_cast_type.on_quick_tag;
					list.Clear();
				}
				else if (text2 == " on click_location")
				{
					flag2 = false;
					flag = false;
					initialCastCommand.type = InitialCastCommand.initial_cast_type.on_click_location;
					list.Clear();
				}
				else if (text2 == " on projectile")
				{
					flag = false;
					flag2 = true;
					initialCastCommand.type = InitialCastCommand.initial_cast_type.on_projectile;
					list.Clear();
				}
				else
				{
					flag = false;
					flag2 = false;
				}
			}
			else
			{
				perk_data.on_initial_cast.Add(initialCastCommand);
				initialCastCommand = new InitialCastCommand();
				flag = false;
				flag2 = false;
			}
		}
		perk_data.on_initial_cast.Add(initialCastCommand);
	}

	public void PressPerkCastButton(int index)
	{
		PopupControl.Instance.SetButtonWasPressed();
		if (GameController.Instance.level_up_animation_playing)
		{
			return;
		}
		string text = ((index == 0) ? perk_slot_A : perk_slot_B);
		int perkLevel = GetPerkLevel(text);
		PerkData perkData = ClonePerkForCasting(text, GameController.Instance.player);
		if (GameServerConnector.Instance.FullyInGame() && GameServerReceiver.Instance.disabled_perks.Contains(text))
		{
			PopupControl.Instance.ShowMessage(perkData.full_name + " is not allowed on this server right now.\n\nIt will be enabled in the future =)", PopupControl.context.message);
			return;
		}
		if (text == "perk_giant" && ZoneDataControl.Instance.curr_zonedata.house_item.GetString("quest_miniworld") == "true")
		{
			PopupControl.Instance.ShowMessage("Giant perk is not allowed during this quest.", PopupControl.context.message);
			return;
		}
		int manaCost = perkData.GetManaCost(perkLevel);
		if (mana_available - (float)manaCost < 0f)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		foreach (InitialCastCommand item in perkData.on_initial_cast)
		{
			switch (item.type)
			{
			case InitialCastCommand.initial_cast_type.on_projectile:
				if (item.projectile_target_type == InitialCastCommand.projectile_target.enemies)
				{
					flag2 = true;
				}
				else if (item.projectile_target_type == InitialCastCommand.projectile_target.allies)
				{
					flag3 = true;
				}
				break;
			case InitialCastCommand.initial_cast_type.on_quick_tag:
				flag2 = true;
				break;
			case InitialCastCommand.initial_cast_type.on_click_location:
				flag = true;
				break;
			}
		}
		if (!flag && !flag2 && !flag3)
		{
			ApplyInitialCastOnto(perkData, perkLevel, "LOCAL", GameController.Instance.playerLevel, GameController.Instance.player);
			SpendMana(manaCost);
			return;
		}
		if (flag2)
		{
			GameController.Instance.casting_projectile_at_enemy = true;
		}
		else if (flag3)
		{
			GameController.Instance.casting_projectile_at_ally = true;
		}
		else if (flag)
		{
			GameController.Instance.picking_cast_custom_location = true;
		}
		GameController.Instance.PAUSE_GAME();
		GameplayGUIControl.Instance.HideGameplayGui();
		if (flag2)
		{
			ConstructionControl.Instance.ShowDoneButton("CANCEL", ConstructionControl.button_state.CAST_PROJECTILE_AT_ENEMY);
		}
		else if (flag3)
		{
			ConstructionControl.Instance.ShowDoneButton("CANCEL", ConstructionControl.button_state.CAST_PROJECTILE_AT_ALLY);
		}
		else if (flag)
		{
			ConstructionControl.Instance.ShowDoneButton("CANCEL", ConstructionControl.button_state.PICKING_CAST_CUSTOM_LOCATION);
		}
		ConstructionControl.Instance.click_to_place.SetActive(true);
		if (flag2)
		{
			ConstructionControl.Instance.click_to_place_txt.text = "Click on an enemy!";
		}
		else if (flag3)
		{
			ConstructionControl.Instance.click_to_place_txt.text = "Click on a friend!";
		}
		else if (flag)
		{
			ConstructionControl.Instance.click_to_place_txt.text = "Click on a location!";
		}
		about_to_cast_data = perkData;
		about_to_cast_lvl = perkLevel;
	}

	public void ApplyInitialCastOnto(PerkData perk_data, int perkLevel, string casted_from, int caster_level, GameObject cast_onto)
	{
		PerkReceiver component = cast_onto.GetComponent<PerkReceiver>();
		foreach (InitialCastCommand item in perk_data.on_initial_cast)
		{
			foreach (string effect_name in item.effect_names)
			{
				component.ApplyPerkEffect(false, effect_name, perk_data, perkLevel, casted_from, caster_level, true);
			}
		}
	}

	private void DeselectCurrentPerk()
	{
		if (!(curr_sel_perk == null))
		{
			curr_sel_perk.GetComponent<Animation>().Stop();
			curr_sel_perk.GetComponent<Image>().raycastTarget = true;
			curr_sel_perk.transform.localScale = Vector3.one;
		}
	}

	public void PressSpendPerkLater()
	{
		WindowPrefabsControl.Instance.DestroyScreen("NewPerkGet - center");
		WindowPrefabsControl.Instance.DestroyScreen("NewPerkGet - bottom");
	}

	public void PressSpendPerkNow()
	{
		WindowPrefabsControl.Instance.DestroyScreen("NewPerkGet - center");
		WindowPrefabsControl.Instance.DestroyScreen("NewPerkGet - bottom");
		GameController.Instance.HideLevelScreen();
		ActuallyOpenPerkManage();
		PerkScreen.Instance.came_from_levelup_screen = true;
		PerkScreen.Instance.stop_spend_mode_after_one_unlock = true;
		PerkScreen.Instance.PressSpendPoints();
		PerkScreen.Instance.cancel_button.SetActive(false);
	}

	public void OpenPerkManageScreen()
	{
		if (WindowControl.Instance.CanOpenGenericWindow())
		{
			WindowControl.Instance.DoOpenGenericWindow();
			ActuallyOpenPerkManage();
		}
	}

	private void ActuallyOpenPerkManage()
	{
		GameObject gameObject = WindowPrefabsControl.Instance.CreateScreen("NewPerkScreen", WindowPrefabsControl.build_into_t.GAME_CTR);
		PerkScreen.Instance = gameObject.GetComponent<PerkScreen>();
		PerkScreenDev.Instance = gameObject.GetComponent<PerkScreenDev>();
		PerkScreen.Instance.Init();
		WindowControl.Instance.OpenWindow(WindowControl.window_type_t.perkmanage);
	}

	public void CreateDrop(Vector3 position, string effect_name, PerkData perk_data, int perk_level, string caster_id, int caster_level, bool notify_others_of_drop)
	{
		GameObject gameObject = new GameObject("drop_effect");
		PerkReceiver perkReceiver = gameObject.AddComponent<PerkReceiver>();
		perkReceiver.delete_once_all_effects_gone = true;
		gameObject.transform.position = position;
		string effect_name2 = perk_data.GetString("Apply effect to drop", effect_name, perk_level).Replace("[", "").Replace("]", "");
		perkReceiver.ApplyPerkEffect(false, effect_name2, perk_data, perk_level, caster_id, caster_level, notify_others_of_drop);
		if (notify_others_of_drop)
		{
			GameServerSender.Instance.SendCreatePerkDrop(position, effect_name, perk_data, perk_level, caster_id, caster_level, "");
		}
	}

	public bool ShouldApplyEffectRightNow(string caster_id)
	{
		if (!GameServerConnector.Instance.FullyInGame() || caster_id == "LOCAL")
		{
			return true;
		}
		return MobControl.Instance.my_claimed_creatures.Contains(caster_id);
	}

	public void PlayerCastPerkAtTarget(PerkData perk_data, int perk_level, GameObject target, Vector3 custom_location)
	{
		int playerLevel = GameController.Instance.playerLevel;
		foreach (InitialCastCommand item in perk_data.on_initial_cast)
		{
			switch (item.type)
			{
			case InitialCastCommand.initial_cast_type.on_self:
				foreach (string effect_name in item.effect_names)
				{
					GameController.Instance.player.GetComponent<PerkReceiver>().ApplyPerkEffect(false, effect_name, perk_data, perk_level, "LOCAL", playerLevel, true);
				}
				break;
			case InitialCastCommand.initial_cast_type.on_projectile:
			{
				string combat_name = target.GetComponent<Combatant>().combat_name;
				LaunchProjectile(perk_data, perk_level, combat_name, "LOCAL", playerLevel, target.transform.position, GameController.Instance.player.transform.position);
				GameServerSender.Instance.SendLaunchProjectilePerk(perk_data, perk_level, combat_name, "LOCAL", playerLevel, target.transform.position, GameController.Instance.player.transform.position, "");
				break;
			}
			case InitialCastCommand.initial_cast_type.on_quick_tag:
				GameController.Instance.player.GetComponent<CreatureBrainLocalPlayer>().SelectTarget(target);
				GameController.Instance.player.GetComponent<SharedCreature>().isQuickTagging = true;
				GameController.Instance.player.GetComponent<SharedCreature>().quickTagEffects = item.effect_names;
				GameController.Instance.player.GetComponent<SharedCreature>().quickTagPerkData = perk_data;
				GameController.Instance.player.GetComponent<SharedCreature>().quickTagPerkLevel = perk_level;
				GameServerSender.Instance.SendQuickTag(1, "");
				break;
			case InitialCastCommand.initial_cast_type.on_click_location:
				foreach (string effect_name2 in item.effect_names)
				{
					if (perk_data.GetBool("Set MoveAt To Drop Location", effect_name2, perk_level))
					{
						GameController.Instance.player.GetComponent<CreatureBrainLocalPlayer>().SelectMovePosition(custom_location);
						break;
					}
				}
				CreateDrop(custom_location, item.effect_names[0], perk_data, perk_level, "LOCAL", playerLevel, true);
				break;
			}
		}
	}

	public void LaunchProjectile(PerkData perk_data, int perk_level, string preferred_target, string shot_from, int shot_from_level, Vector3 end_pos, Vector3 start_pos)
	{
		foreach (InitialCastCommand initial_cast in perk_data.on_initial_cast)
		{
			if (initial_cast.type != InitialCastCommand.initial_cast_type.on_projectile)
			{
				continue;
			}
			ResourceControl.Instance.AsyncInstantiatePerkObj(initial_cast.projectile_model, delegate(GameObject projectile_instance)
			{
				projectile_instance.transform.position = new Vector3(start_pos.x, 0.6f, start_pos.z);
				GameObject target_obj = (MobControl.Instance.active_combatants.ContainsKey(preferred_target) ? MobControl.Instance.active_combatants[preferred_target] : null);
				projectile_instance.GetComponent<PerkProjectile>().SHOOT_AT_(initial_cast.effect_names, perk_data, perk_level, 0.22f, end_pos, target_obj, shot_from, shot_from_level);
				AudioSource component = projectile_instance.GetComponent<AudioSource>();
				if (component != null)
				{
					component.volume = AudioControl.Instance.general_sfx_volume;
					component.Play();
				}
			});
			break;
		}
	}

	public void SpendMana(float cast_cost)
	{
		if (cast_cost > 0f)
		{
			mana_available -= cast_cost;
			UpdateManaVisual();
			UpdateMainSlotsColor();
		}
	}

	public void UpdateMainSlotsColor()
	{
		float num = mana_available;
		PerkData perkDataForInfoDisplay = GetPerkDataForInfoDisplay(perk_slot_A);
		int perkLevel = GetPerkLevel(perk_slot_A);
		PerkData perkDataForInfoDisplay2 = GetPerkDataForInfoDisplay(perk_slot_B);
		int perkLevel2 = GetPerkLevel(perk_slot_B);
		slotA_main.GetComponent<Image>().color = (((float)perkDataForInfoDisplay.GetManaCost(perkLevel) <= num) ? available_for_casting : unavailable_for_casting);
		slotB_main.GetComponent<Image>().color = (((float)perkDataForInfoDisplay2.GetManaCost(perkLevel2) <= num) ? available_for_casting : unavailable_for_casting);
	}

	public void ClosePerkManageScreen()
	{
		if (PerkScreen.Instance.unlocked_at_least_one_perk)
		{
			Instance.TryAchievements();
		}
		if (PerkScreen.Instance.came_from_levelup_screen)
		{
			GameController.Instance.TryLevelAchieves();
		}
		AudioControl.Instance.PlayGenericClick();
		perk_screen_x = (int)PerkScreen.Instance.scrollwheel_parent.transform.localPosition.x;
		perk_screen_y = (int)PerkScreen.Instance.scrollwheel_parent.transform.localPosition.y;
		SavePerkScreenPositioningToDisk();
		WindowPrefabsControl.Instance.DestroyScreen("NewPerkScreen");
		GameController.Instance.level_up_animation_playing = false;
	}

	public void RedrawEquippedPerkSlots()
	{
		int perkLevel = GetPerkLevel(perk_slot_A);
		bool flag = perk_slot_A != "";
		if (perkLevel > 0 && flag)
		{
			slotA_main.SetActive(true);
			ResourceControl.Instance.AssignPerkSprite(perk_slot_A, slotA_main.GetComponent<Image>());
			GameObject gameObject = slotA_main.transform.Find("Image").gameObject;
			if (perkLevel > 1)
			{
				gameObject.SetActive(true);
				slotA_main.transform.Find("Image").Find("Text").gameObject.GetComponent<Text>().text = perkLevel.ToString() ?? "";
			}
			else
			{
				gameObject.SetActive(false);
			}
		}
		else
		{
			slotA_main.SetActive(false);
		}
		int perkLevel2 = GetPerkLevel(perk_slot_B);
		bool flag2 = perk_slot_B != "";
		if (perkLevel2 > 0 && flag2)
		{
			slotB_main.SetActive(true);
			ResourceControl.Instance.AssignPerkSprite(perk_slot_B, slotB_main.GetComponent<Image>());
			GameObject gameObject2 = slotB_main.transform.Find("Image").gameObject;
			if (perkLevel2 > 1)
			{
				gameObject2.SetActive(true);
				slotB_main.transform.Find("Image").Find("Text").gameObject.GetComponent<Text>().text = perkLevel2.ToString() ?? "";
			}
			else
			{
				gameObject2.SetActive(false);
			}
			mana_meter.transform.localPosition = new Vector3(-182f, 160f, 0f);
		}
		else
		{
			slotB_main.SetActive(false);
			mana_meter.transform.localPosition = new Vector3(-80f, 153f, 0f);
		}
		UpdateMainSlotsColor();
	}

	public float MaxManaWithModifiers()
	{
		return mana_modifier * 100f;
	}

	public void EquipPerk(int index, string perk_key_equip)
	{
		switch (index)
		{
		case 0:
			perk_slot_A = perk_key_equip;
			break;
		case 1:
			perk_slot_B = perk_key_equip;
			break;
		}
		SaveEquippedPerksToDisk();
		RedrawEquippedPerkSlots();
	}

	public void UpdateManaVisual()
	{
		float num = mana_available;
		mana_text.text = (int)num + "%";
		float num2 = num / MaxManaWithModifiers();
		mana_mask.rectTransform.sizeDelta = new Vector2(100f, num2 * 100f);
		float num3 = 1f - num2;
		mana_mask.transform.localPosition = mana_mask_start_pos + Vector2.down * num3 * 50f;
		mana_foreground.transform.localPosition = mana_overlay_start_pos + num3 * Vector2.up * 50f;
	}

	public void RegenerateSurroundingPlants(Vector3 origin, float radius, int perk_level)
	{
		string player_zone = ChunkControl.Instance.player_zone;
		foreach (string allChunkKey in ChunkControl.Instance.GetAllChunkKeys())
		{
			ChunkData chunkData = ChunkControl.Instance.GetChunkData(allChunkKey);
			if (chunkData == null)
			{
				continue;
			}
			for (int i = 0; i < 10; i++)
			{
				for (int j = 0; j < 10; j++)
				{
					if (!(Vector3.Distance(origin, new Vector3(i + chunkData.X * 10, 0f, j + chunkData.Z * 10)) < radius))
					{
						continue;
					}
					foreach (ChunkElement item in chunkData.GetElementsAt(i, j))
					{
						int intFromItemFile = ResourceControl.Instance.GetIntFromItemFile(item.item.item_name, "Pollinator Regen Level");
						if (intFromItemFile != 0 && intFromItemFile <= perk_level)
						{
							string stringFromItemFile = ResourceControl.Instance.GetStringFromItemFile(item.item.item_name, "Pollinator Regen RespawnId");
							if (item.item.HasActiveRespawn(stringFromItemFile))
							{
								ExtraInventoryData extraDataCopy = item.item.GetExtraDataCopy();
								extraDataCopy.SetString("has_respawn_" + stringFromItemFile, "");
								InventoryItem new_item = new InventoryItem(item.item.item_name, extraDataCopy);
								ConstructionControl.Instance.PlayerReplaceAt(new_item, item.item, item.rot, player_zone, chunkData.X, chunkData.Z, i, j, true, ConstructionControl.GenerateCacheKey());
							}
						}
					}
				}
			}
		}
	}

	private IEnumerator RegenerateMana()
	{
		while (true)
		{
			yield return new WaitForSeconds(0.2f);
			float t = CombatControl.Instance.CalcCombatSlider(CombatControl.slider_type.mana_recharge);
			mana_available = Mathf.Min(mana_available + Mathf.Lerp(0.6f, 1.1f, t), MaxManaWithModifiers());
			UpdateManaVisual();
			UpdateMainSlotsColor();
		}
	}

	public void SoundMaxout()
	{
		sfx_source.volume = AudioControl.Instance.general_sfx_volume;
		sfx_source.PlayOneShot(maxout);
	}

	public void ShowPerkGet(Color gem_col)
	{
		WindowPrefabsControl.Instance.CreateScreen("NewPerkGet - center", WindowPrefabsControl.build_into_t.GAME_CTR);
		WindowPrefabsControl.Instance.GetScreen("NewPerkGet - center").GetComponent<AnimationFunctionsNewPerkScreen>().Initialize(gem_col);
		WindowPrefabsControl.Instance.GetScreen("NewPerkGet - center").GetComponent<Animation>().Play();
	}

	public string GetPerkEnergyCostString(PerkData perk_data, int perk_level, bool show_differences)
	{
		return "<color=#00aaff>uses " + perk_data.GetManaCost(perk_level) + "% of energy</color>\n";
	}

	public void LoadGenomesFromDisk()
	{
		genomes = Mathf.Max(0, PlayerData.Instance.GetSlotShort("genomes", PlayerData.filename_t.perks));
	}

	public void SaveGenomes()
	{
		PlayerData.Instance.SetSlotShort("genomes", genomes, PlayerData.filename_t.perks);
	}

	public void LoadPerkScreenPositioningFromDisk()
	{
		perk_screen_x = PlayerData.Instance.GetSlotShort("perk_screen_x", PlayerData.filename_t.perks);
		perk_screen_y = PlayerData.Instance.GetSlotShort("perk_screen_y", PlayerData.filename_t.perks);
	}

	public void SavePerkScreenPositioningToDisk()
	{
		PlayerData.Instance.SetSlotShort("perk_screen_x", perk_screen_x, PlayerData.filename_t.perks);
		PlayerData.Instance.SetSlotShort("perk_screen_y", perk_screen_y, PlayerData.filename_t.perks);
	}

	public void LoadEquippedPerksFromDisk()
	{
		perk_slot_A = PlayerData.Instance.GetSlotString("perk_slot_A", PlayerData.filename_t.perks);
		perk_slot_B = PlayerData.Instance.GetSlotString("perk_slot_B", PlayerData.filename_t.perks);
	}

	public void SaveEquippedPerksToDisk()
	{
		PlayerData.Instance.SetSlotString("perk_slot_A", perk_slot_A, PlayerData.filename_t.perks);
		PlayerData.Instance.SetSlotString("perk_slot_B", perk_slot_B, PlayerData.filename_t.perks);
	}

	public void LoadPerkLevelsFromDisk()
	{
		perk_levels.Clear();
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("AutoGen/(Auto Gen) Perks List", ref file_exists);
		if (!file_exists)
		{
			return;
		}
		foreach (string item in textFileLines)
		{
			if (!Startup.StringNullOrWhitespace(item))
			{
				perk_levels.Add(item, PlayerData.Instance.GetSlotShort(item, PlayerData.filename_t.perks));
			}
		}
	}

	public void OverwritePerkLevel(string perk_key, int new_level)
	{
		if (perk_levels.ContainsKey(perk_key))
		{
			perk_levels[perk_key] = new_level;
		}
		else
		{
			perk_levels.Add(perk_key, new_level);
		}
	}

	public void SavePerkLevel(string perk_key)
	{
		PlayerData.Instance.SetSlotShort(perk_key, GetPerkLevel(perk_key), PlayerData.filename_t.perks);
	}

	public int GetPerkLevel(string perk_key)
	{
		short globalShort = PlayerData.Instance.GetGlobalShort("dev_perk_level");
		if (globalShort != 0)
		{
			return globalShort;
		}
		if (perk_levels.ContainsKey(perk_key))
		{
			return perk_levels[perk_key];
		}
		return 0;
	}

	public void InitializeEmptyPerkList()
	{
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("AutoGen/(Auto Gen) Perks List", ref file_exists);
		if (!file_exists)
		{
			return;
		}
		foreach (string item in textFileLines)
		{
			if (!Startup.StringNullOrWhitespace(item))
			{
				OverwritePerkLevel(item, 0);
			}
		}
	}

	public bool CanUnlockNextLevel(PerkData perk_data)
	{
		int perkLevel = GetPerkLevel(perk_data.original_key);
		if ((perk_data.max_level == -1 || perkLevel != perk_data.max_level) && GenomesForNextLevel(perkLevel, perk_data) <= genomes)
		{
			return HasPrerequisiteForNextLevel(perk_data);
		}
		return false;
	}

	public bool HasEnoughGenomesForNextLevel(int perk_level, PerkData perk_Data)
	{
		return GenomesForNextLevel(perk_level, perk_Data) <= genomes;
	}

	public bool HasPrerequisiteForNextLevel(PerkData perk_data)
	{
		if (perk_data.unlock_prerequisite_perk_level != 0)
		{
			int perkLevel = Instance.GetPerkLevel(perk_data.unlock_prerequisite_perk_key);
			if (perkLevel == 0)
			{
				return false;
			}
			if (perkLevel < perk_data.unlock_prerequisite_perk_level)
			{
				return false;
			}
		}
		return true;
	}

	public int GenomesForNextLevel(int curr_perk_level, PerkData perk_data)
	{
		if (curr_perk_level == 0)
		{
			return 1;
		}
		if (perk_data.max_level == -1)
		{
			float num = (float)GameController.Instance.playerLevel / 6f / (float)GetNumInfinitelyLevelablePerks();
			if ((float)curr_perk_level >= num)
			{
				return (int)(((float)curr_perk_level - num + 1.6666666f) * 0.6f);
			}
		}
		else if (perk_data.max_level != 0)
		{
			float num2 = Mathf.Clamp01((float)curr_perk_level / (float)perk_data.max_level);
			if (num2 >= 0.8f)
			{
				return 3;
			}
			if (num2 >= 0.5f)
			{
				return 2;
			}
		}
		return 1;
	}

	public string GetPerkUnlockCostString(PerkData perk_data, int perk_level)
	{
		string text = "<color=#fa3956>";
		string text2 = "<color=#66fa88>";
		string text3 = "";
		text3 += ((GenomesForNextLevel(perk_level, perk_data) <= genomes) ? text2 : text);
		int num = GenomesForNextLevel(perk_level, perk_data);
		text3 = text3 + "\n" + num + " genome" + ((num < 2) ? "" : "s") + "</color>";
		if (perk_data.unlock_prerequisite_perk_level != 0)
		{
			text3 += "\n";
			text3 += (HasPrerequisiteForNextLevel(perk_data) ? text2 : text);
			if (GetPerkLevel(perk_data.unlock_prerequisite_perk_key) != 0)
			{
				PerkData perkDataForInfoDisplay = GetPerkDataForInfoDisplay(perk_data.unlock_prerequisite_perk_key);
				return text3 + "Level " + perk_data.unlock_prerequisite_perk_level + " \"" + perkDataForInfoDisplay.full_name + "\"</color>";
			}
			return text3 + "Level " + perk_data.unlock_prerequisite_perk_level + " \"???\"</color>";
		}
		return text3;
	}

	public string GetPerkDetailedDescription(PerkData perk_data, int perk_level_now, int player_level, bool show_diffs)
	{
		string text = ParseDescription(perk_data, perk_data.detailed_description, perk_level_now, player_level, show_diffs);
		if (show_diffs && perk_data.mana_cost_str.Contains("CLAMP"))
		{
			int manaCost = perk_data.GetManaCost(perk_level_now);
			int manaCost2 = perk_data.GetManaCost(perk_level_now - 1);
			if (manaCost != manaCost2)
			{
				char c = text[text.Length - 1];
				if (c != '!' && c != '.')
				{
					text += ".";
				}
				return text + " Uses " + manaCost + "% of energy <color=#00ff00>(" + (manaCost - manaCost2) + "%)</color>";
			}
		}
		return text;
	}

	public string ParseDescription(PerkData perk_data, string description_str, int curr_perk_level, int player_level, bool show_diffs)
	{
		int num = 0;
		while (description_str.Contains("GetFloat["))
		{
			int num2 = description_str.IndexOf("GetFloat[") + 9;
			for (int i = 0; num2 + i < description_str.Length; i++)
			{
				if (description_str[num2 + i] == ']')
				{
					string text = description_str.Substring(num2, i);
					string text2 = text.Substring(0, text.IndexOf(','));
					string text3 = text.Replace(text2 + ", \"", "");
					string text4 = text3.Substring(0, text3.IndexOf(',') - 1);
					text3 = text3.Replace(text4 + "\", remove \"", "");
					string remove_suffix = text3.Substring(0, text3.Length - 1);
					float @float = perk_data.GetFloat(text4, text2, curr_perk_level, player_level, remove_suffix);
					description_str = description_str.Replace("GetFloat[" + text + "]", @float.ToString("0.##") ?? "");
					break;
				}
			}
			num++;
			if (num == 31)
			{
				return "ERROR";
			}
		}
		num = 0;
		while (description_str.Contains("DiffFloat["))
		{
			int num3 = description_str.IndexOf("DiffFloat[") + 10;
			for (int j = 0; num3 + j < description_str.Length; j++)
			{
				if (description_str[num3 + j] == ']')
				{
					string text5 = description_str.Substring(num3, j);
					string text6 = text5.Substring(0, text5.IndexOf(','));
					string text7 = text5.Replace(text6 + ", \"", "");
					string text8 = text7.Substring(0, text7.IndexOf(',') - 1);
					text7 = text7.Replace(text8 + "\", remove \"", "");
					string text9 = text7.Substring(0, text7.IndexOf(',') - 1);
					text7 = text7.Replace(text9 + "\", add \"", "");
					text7 = text7.Substring(0, text7.Length - 1);
					float num4 = perk_data.GetFloat(text8, text6, curr_perk_level, player_level, text9) - perk_data.GetFloat(text8, text6, curr_perk_level - 1, player_level, text9);
					string oldValue = "DiffFloat[" + text5 + "]";
					string newValue = "";
					if (num4 != 0f && show_diffs)
					{
						newValue = "<color=#00ff00>(" + ((num4 >= 0f) ? "+" : "") + num4.ToString("0.##") + text7 + ")</color>";
					}
					description_str = description_str.Replace(oldValue, newValue);
					break;
				}
			}
			num++;
			if (num == 31)
			{
				return "ERROR";
			}
		}
		num = 0;
		while (description_str.Contains("GetInt["))
		{
			int num5 = description_str.IndexOf("GetInt[") + 7;
			for (int k = 0; num5 + k < description_str.Length; k++)
			{
				if (description_str[num5 + k] == ']')
				{
					string text10 = description_str.Substring(num5, k);
					string text11 = text10.Substring(0, text10.IndexOf(','));
					string text12 = text10.Replace(text11 + ", \"", "");
					string text13 = text12.Substring(0, text12.IndexOf(',') - 1);
					text12 = text12.Replace(text13 + "\", remove \"", "");
					string remove_suffix2 = text12.Substring(0, text12.Length - 1);
					int num6 = (int)perk_data.GetFloat(text13, text11, curr_perk_level, player_level, remove_suffix2);
					description_str = description_str.Replace("GetInt[" + text10 + "]", num6.ToString() ?? "");
					break;
				}
			}
			num++;
			if (num == 31)
			{
				return "ERROR";
			}
		}
		num = 0;
		while (description_str.Contains("DiffInt["))
		{
			int num7 = description_str.IndexOf("DiffInt[") + 8;
			for (int l = 0; num7 + l < description_str.Length; l++)
			{
				if (description_str[num7 + l] == ']')
				{
					string text14 = description_str.Substring(num7, l);
					string text15 = text14.Substring(0, text14.IndexOf(','));
					string text16 = text14.Replace(text15 + ", \"", "");
					string text17 = text16.Substring(0, text16.IndexOf(',') - 1);
					text16 = text16.Replace(text17 + "\", remove \"", "");
					string text18 = text16.Substring(0, text16.IndexOf(',') - 1);
					text16 = text16.Replace(text18 + "\", add \"", "");
					text16 = text16.Substring(0, text16.Length - 1);
					int num8 = (int)perk_data.GetFloat(text17, text15, curr_perk_level, player_level, text18) - (int)perk_data.GetFloat(text17, text15, curr_perk_level - 1, player_level, text18);
					string oldValue2 = "DiffInt[" + text14 + "]";
					if (num8 == 0 || !show_diffs)
					{
						description_str = description_str.Replace(oldValue2, "");
					}
					else
					{
						description_str = description_str.Replace(oldValue2, "<color=#00ff00>(" + ((num8 < 0) ? "" : "+") + num8 + text16 + ")</color>");
					}
					break;
				}
			}
			num++;
			if (num == 31)
			{
				return "ERROR";
			}
		}
		num = 31;
		while (description_str.Contains("  "))
		{
			description_str = description_str.Replace("  ", " ");
			num--;
			if (num == 0)
			{
				return "ERROR";
			}
		}
		num = 31;
		while (description_str.Contains(" ."))
		{
			description_str = description_str.Replace(" .", ".");
			num--;
			if (num == 0)
			{
				return "ERROR";
			}
		}
		num = 31;
		while (description_str.Contains(" ,"))
		{
			description_str = description_str.Replace(" ,", ",");
			num--;
			if (num == 0)
			{
				return "ERROR";
			}
		}
		return description_str;
	}

	public int GetStandardPlayerLevel(float perk_level, bool exact)
	{
		return 1;
	}

	public void TryAchievements()
	{
		AchievesControl.Instance.UnlockAchievement("You're A Wizard!");
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("AutoGen/(Auto Gen) Perks List", ref file_exists);
		if (!file_exists)
		{
			return;
		}
		int num = 0;
		foreach (string item in textFileLines)
		{
			if (!Startup.StringNullOrWhitespace(item) && GetPerkLevel(item) > 0)
			{
				num++;
			}
		}
		if (num >= 5)
		{
			AchievesControl.Instance.UnlockAchievement("Skillmaster");
			if (num >= 10)
			{
				AchievesControl.Instance.UnlockAchievement("Jack of All Trades");
			}
		}
	}
}
