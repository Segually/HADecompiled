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
		return null;
	}

	private string GetRandomBiomeParticle(GameObject caller)
	{
		return null;
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
		return 0f;
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
	}

	public void ApplyInitialCastOnto(PerkData perk_data, int perkLevel, string casted_from, int caster_level, GameObject cast_onto)
	{
	}

	private void DeselectCurrentPerk()
	{
	}

	public void PressSpendPerkLater()
	{
	}

	public void PressSpendPerkNow()
	{
	}

	public void OpenPerkManageScreen()
	{
	}

	private void ActuallyOpenPerkManage()
	{
	}

	public void CreateDrop(Vector3 position, string effect_name, PerkData perk_data, int perk_level, string caster_id, int caster_level, bool notify_others_of_drop)
	{
	}

	public bool ShouldApplyEffectRightNow(string caster_id)
	{
		return false;
	}

	public void PlayerCastPerkAtTarget(PerkData perk_data, int perk_level, GameObject target, Vector3 custom_location)
	{
	}

	public void LaunchProjectile(PerkData perk_data, int perk_level, string preferred_target, string shot_from, int shot_from_level, Vector3 end_pos, Vector3 start_pos)
	{
	}

	public void SpendMana(float cast_cost)
	{
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
		return 0f;
	}

	public void EquipPerk(int index, string perk_key_equip)
	{
	}

	public void UpdateManaVisual()
	{
	}

	public void RegenerateSurroundingPlants(Vector3 origin, float radius, int perk_level)
	{
	}

	private IEnumerator RegenerateMana()
	{
		return null;
	}

	public void SoundMaxout()
	{
	}

	public void ShowPerkGet(Color gem_col)
	{
	}

	public string GetPerkEnergyCostString(PerkData perk_data, int perk_level, bool show_differences)
	{
		return null;
	}

	public void LoadGenomesFromDisk()
	{
		genomes = Mathf.Max(0, PlayerData.Instance.GetSlotShort("genomes", PlayerData.filename_t.perks));
	}

	public void SaveGenomes()
	{
	}

	public void LoadPerkScreenPositioningFromDisk()
	{
		perk_screen_x = PlayerData.Instance.GetSlotShort("perk_screen_x", PlayerData.filename_t.perks);
		perk_screen_y = PlayerData.Instance.GetSlotShort("perk_screen_y", PlayerData.filename_t.perks);
	}

	public void SavePerkScreenPositioningToDisk()
	{
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
		return false;
	}

	public bool HasEnoughGenomesForNextLevel(int perk_level, PerkData perk_Data)
	{
		return false;
	}

	public bool HasPrerequisiteForNextLevel(PerkData perk_data)
	{
		return false;
	}

	public int GenomesForNextLevel(int curr_perk_level, PerkData perk_data)
	{
		return 0;
	}

	public string GetPerkUnlockCostString(PerkData perk_data, int perk_level)
	{
		return null;
	}

	public string GetPerkDetailedDescription(PerkData perk_data, int perk_level_now, int player_level, bool show_diffs)
	{
		return null;
	}

	public string ParseDescription(PerkData perk_data, string description_str, int curr_perk_level, int player_level, bool show_diffs)
	{
		return null;
	}

	public int GetStandardPlayerLevel(float perk_level, bool exact)
	{
		return 0;
	}

	public void TryAchievements()
	{
	}
}
