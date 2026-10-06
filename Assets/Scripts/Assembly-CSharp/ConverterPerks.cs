using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ConverterPerks : MonoBehaviour
{
	public bool complete;

	public IEnumerator BeginConverting()
	{
		GrandConverter.Instance.DrawProgressBar(0.01f);
		GrandConverter.Instance.filename_text.text = "";
		yield return new WaitForEndOfFrame();
		foreach (string item in new List<string> { "Slot_0", "Slot_1", "Slot_2" })
		{
			string path = Path.Combine(Path.Combine(Startup.persistentDataPath, item), "perks");
			string path2 = Path.Combine(Path.Combine(Startup.persistentDataPath, item), "general");
			if (!File.Exists(path) || !File.Exists(path2))
			{
				continue;
			}
			SingleFile singleFile = new SingleFile();
			singleFile.LoadFromBytes(File.ReadAllBytes(path));
			SingleFile singleFile2 = new SingleFile();
			singleFile2.LoadFromBytes(File.ReadAllBytes(path2));
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("perk_bind", "perk_sticky_web");
			dictionary.Add("perk_confuse", "perk_screech");
			dictionary.Add("perk_eagleEye", "perk_eagle_eye");
			dictionary.Add("perk_forcePsh", "perk_forceful_push");
			dictionary.Add("perk_heal", "perk_rejuvenate");
			dictionary.Add("perk_sting", "perk_poison_sting");
			Dictionary<string, OldPerk> dictionary2 = new Dictionary<string, OldPerk>();
			dictionary2.Add("perk_dash", new OldPerk(5, "perk_sprint", 3));
			dictionary2.Add("perk_defensive_shell", new OldPerk(-1));
			dictionary2.Add("perk_disguise", new OldPerk(15));
			dictionary2.Add("perk_eagle_eye", new OldPerk(15));
			dictionary2.Add("perk_fireball", new OldPerk(-1));
			dictionary2.Add("perk_fire_storm", new OldPerk(-1, "perk_fireball", 3));
			dictionary2.Add("perk_forceful_push", new OldPerk(-1));
			dictionary2.Add("perk_giant", new OldPerk(-1));
			dictionary2.Add("perk_healing_touch", new OldPerk(-1, "perk_rejuvenate", 1));
			dictionary2.Add("perk_ink_cannon", new OldPerk(12));
			dictionary2.Add("perk_lightning", new OldPerk(-1));
			dictionary2.Add("perk_poison_sting", new OldPerk(-1));
			dictionary2.Add("perk_pollinator", new OldPerk(15, "perk_healing_touch", 2));
			dictionary2.Add("perk_rage", new OldPerk(-1, "perk_ram", 2));
			dictionary2.Add("perk_ram", new OldPerk(-1));
			dictionary2.Add("perk_rejuvenate", new OldPerk(-1));
			dictionary2.Add("perk_screech", new OldPerk(15));
			dictionary2.Add("perk_shrink", new OldPerk(15));
			dictionary2.Add("perk_sprint", new OldPerk(15));
			dictionary2.Add("perk_sticky_web", new OldPerk(8));
			dictionary2.Add("perk_summon_bandits", new OldPerk(-1, "perk_summon_pack", 2));
			dictionary2.Add("perk_summon_big_momma", new OldPerk(-1, "perk_summon_pack", 1));
			dictionary2.Add("perk_summon_pack", new OldPerk(-1));
			dictionary2.Add("perk_tail_slap", new OldPerk(-1, "perk_sprint", 1));
			dictionary2.Add("perk_toxic_plume", new OldPerk(-1, "perk_poison_sting", 2));
			dictionary2.Add("perk_vampire", new OldPerk(-1, "perk_wewaken", 2));
			dictionary2.Add("perk_weaken", new OldPerk(-1));
			RenameAllPerks(singleFile, dictionary);
			int longValue = singleFile2.GetLongValue("new_playerLevel");
			int num = 0;
			int num2 = 0;
			foreach (KeyValuePair<string, OldPerk> item2 in dictionary2)
			{
				int shortValue = singleFile.GetShortValue(item2.Key);
				num += shortValue;
				singleFile.SetShort(item2.Key, 0);
				Debug.Log("[post-rename] " + item2.Key + "=lvl_" + shortValue);
				if (item2.Value.max_level == -1)
				{
					num2++;
				}
			}
			float averagePerkLevel = GetAveragePerkLevel(longValue, num2);
			bool flag;
			do
			{
				if (num <= 0)
				{
					break;
				}
				flag = false;
				foreach (KeyValuePair<string, OldPerk> item3 in dictionary2)
				{
					OldPerk value = item3.Value;
					if (value.curr_level == 0 && value.prerequisite_perk != "" && dictionary2.ContainsKey(value.prerequisite_perk) && dictionary2[value.prerequisite_perk].curr_level < value.prerequisite_level)
					{
						continue;
					}
					if (value.curr_level == value.max_level && value.max_level != -1)
					{
						continue;
					}
					int num3 = 1;
					if (value.curr_level != 0)
					{
						if (value.max_level == -1)
						{
							if ((float)value.curr_level >= averagePerkLevel)
							{
								num3 = (int)(((float)value.curr_level - averagePerkLevel + 1.6666666f) * 0.6f);
							}
						}
						else if (value.max_level != 0)
						{
							float num4 = (float)value.curr_level / (float)value.max_level;
							if (num4 >= 0.8f)
							{
								num3 = 3;
							}
							else if (num4 >= 0.5f)
							{
								num3 = 2;
							}
						}
					}
					if (num3 <= num)
					{
						flag = true;
						value.curr_level++;
						num -= num3;
					}
				}
			}
			while (flag);
			foreach (KeyValuePair<string, OldPerk> item4 in dictionary2)
			{
				singleFile.SetShort(item4.Key, item4.Value.curr_level);
			}
			singleFile.SetShort("genomes", num);
			if (singleFile.edited_since_load)
			{
				File.WriteAllBytes(path, singleFile.ToByteArray());
			}
		}
		complete = true;
	}

	public float GetAveragePerkLevel(int player_level, int n_infinitely_levelable_perks)
	{
		return (float)player_level / 6f / (float)n_infinitely_levelable_perks;
	}

	private void RenameAllPerks(SingleFile file, Dictionary<string, string> perks_to_rename)
	{
		foreach (KeyValuePair<string, string> item in perks_to_rename)
		{
			int shortValue = file.GetShortValue(item.Key);
			if (shortValue != 0)
			{
				file.SetShort(item.Value, shortValue);
			}
			Debug.Log("[pre-rename] " + item.Key + "=lvl_" + shortValue);
		}
		string stringValue = file.GetStringValue("perk_slot_A");
		if (perks_to_rename.ContainsKey(stringValue))
		{
			file.SetString("perk_slot_A", perks_to_rename[stringValue]);
		}
		stringValue = file.GetStringValue("perk_slot_B");
		if (perks_to_rename.ContainsKey(stringValue))
		{
			file.SetString("perk_slot_B", perks_to_rename[stringValue]);
		}
	}
}
