using System.Collections.Generic;
using UnityEngine;

public class PerkData
{
	public enum limiter
	{
		unknown = 0,
		own_hp = 1,
		own_level = 2
	}

	public string original_key = "";

	public string full_name = "???";

	public string description = "???";

	public string detailed_description = "???";

	public bool large_upgrade_description_box;

	public string ultra_detailed_description = "";

	public bool not_unlockable;

	public string mana_cost_str = "";

	public int max_level = -1;

	public int unlock_prerequisite_perk_level;

	public string unlock_prerequisite_perk_key = "";

	public List<InitialCastCommand> on_initial_cast = new List<InitialCastCommand>();

	public Dictionary<string, SinglePerkEffect> all_effects = new Dictionary<string, SinglePerkEffect>();

	public void PackForWeb(Packet outgoing)
	{
		outgoing.PutString(original_key);
		outgoing.PutShort(on_initial_cast.Count);
		foreach (InitialCastCommand command in on_initial_cast) command.PackForWeb(outgoing);
		outgoing.PutShort(all_effects.Count);
		foreach (KeyValuePair<string, SinglePerkEffect> effect in all_effects)
		{
			outgoing.PutString(effect.Key);
			outgoing.PutShort(effect.Value.data.Count);
			foreach (KeyValuePair<string, string> entry in effect.Value.data)
			{
				outgoing.PutString(entry.Key);
				outgoing.PutString(entry.Value);
			}
		}
	}

	public void UnpackFromWeb(Packet incoming)
	{
		original_key = incoming.GetString();
		int count = incoming.GetShort();
		for (int i = 0; i < count; i++)
		{
			InitialCastCommand command = new InitialCastCommand();
			command.UnpackFromWeb(incoming);
			on_initial_cast.Add(command);
		}
		count = incoming.GetShort();
		for (int i = 0; i < count; i++)
		{
			string name = incoming.GetString();
			Dictionary<string, string> data = new Dictionary<string, string>();
			int n = incoming.GetShort();
			for (int j = 0; j < n; j++) data.Add(incoming.GetString(), incoming.GetString());
			all_effects.Add(name, new SinglePerkEffect(data));
		}
	}

	public void AddEffect(string effect_name, Dictionary<string, string> data)
	{
		SinglePerkEffect value = new SinglePerkEffect(data);
		all_effects.Add(effect_name, value);
	}

	public int GetManaCost(int perk_level)
	{
		if (!mana_cost_str.Contains("CLAMP"))
		{
			return int.Parse(mana_cost_str.Replace("%", ""), Startup.parse_culture);
		}
		string text = mana_cost_str.Replace("CLAMP (", "");
		int num = text.IndexOf('@');
		float a = float.Parse(text.Substring(0, num - 1).Replace("%", ""), Startup.parse_culture);
		num += 12;
		string text2 = mana_cost_str.Replace("CLAMP (", "").Substring(num, mana_cost_str.Replace("CLAMP (", "").Length - num);
		int num2 = text2.IndexOf('@');
		float b = float.Parse(text2.Substring(0, num2 - 1).Replace("%", ""), Startup.parse_culture);
		string text3 = mana_cost_str.Replace("CLAMP (", "").Substring(num, mana_cost_str.Replace("CLAMP (", "").Length - num);
		float b2 = float.Parse(text3.Substring(num2 + 6, text3.Length - num2 - 7), Startup.parse_culture);
		return (int)Mathf.Lerp(a, b, Mathf.InverseLerp(1f, b2, perk_level));
	}

	public string GetString(string key, string effect_name, int perk_level)
	{
		if (all_effects.ContainsKey(effect_name) && all_effects[effect_name].data.ContainsKey(key))
		{
			return all_effects[effect_name].data[key];
		}
		return "";
	}

	public bool GetBool(string key, string effect_name, int perk_level)
	{
		if (all_effects.ContainsKey(effect_name) && all_effects[effect_name].data.ContainsKey(key))
		{
			return all_effects[effect_name].data[key] == "true";
		}
		return false;
	}

	public int GetInt(string key, string effect_name, int perk_level, int player_level, string remove_suffix = "")
	{
		return (int)GetFloat(key, effect_name, perk_level, player_level, remove_suffix);
	}

	public float GetFloat(string key, string effect_name, int perk_level, int player_level, string remove_suffix = "")
	{
		if (!all_effects.ContainsKey(effect_name))
		{
			return -1f;
		}
		Dictionary<string, string> data = all_effects[effect_name].data;
		if (!data.ContainsKey(key))
		{
			return -1f;
		}
		string text = data[key];
		if (text.Contains("CLAMP"))
		{
			string text2 = text.Replace("CLAMP (", "");
			int num = text2.IndexOf('@');
			string text3 = text2.Substring(0, num - 1);
			if (remove_suffix != "")
			{
				text3 = text3.Replace(remove_suffix, "");
			}
			float num2 = float.Parse(text3, Startup.parse_culture);
			string text4 = text.Replace("CLAMP (", "");
			num += 12;
			string text5 = text4.Substring(num, text4.Length - num);
			int num3 = text5.IndexOf('@');
			string text6 = text5.Substring(0, num3 - 1);
			if (remove_suffix != "")
			{
				text6 = text6.Replace(remove_suffix, "");
			}
			float num4 = float.Parse(text6, Startup.parse_culture);
			string text7 = text.Replace("CLAMP (", "");
			string text8 = text7.Substring(num, text7.Length - num);
			float num5 = float.Parse(text8.Substring(num3 + 6, text8.Length - num3 - 7), Startup.parse_culture);
			float num6 = 0f;
			if (num5 != 1f)
			{
				num6 = Mathf.Clamp01(((float)perk_level - 1f) / (num5 - 1f));
			}
			return num2 + (num4 - num2) * num6;
		}
		if (!text.Contains("RANGE"))
		{
			if (remove_suffix != "")
			{
				text = text.Replace(remove_suffix, "");
			}
			return float.Parse(text, Startup.parse_culture);
		}
		string text9 = data[key].Replace("RANGE (", "");
		int num7;
		if (text.Contains(" of own HP"))
		{
			text9 = text9.Replace(" of own HP)", "");
			num7 = 1;
		}
		else if (text.Contains(" of own level"))
		{
			text9 = text9.Replace(" of own level)", "");
			num7 = 2;
		}
		else
		{
			num7 = 0;
		}
		string text10 = text9.Substring(0, text9.IndexOf("%"));
		text9 = text9.Replace(text10 + "%-", "");
		string text11 = text9.Substring(0, text9.IndexOf("%"));
		text9 = text9.Replace(text11 + "%-", "");
		string s = text9.Substring(0, text9.Length - 1);
		float num8 = float.Parse(text10, Startup.parse_culture);
		float num9 = float.Parse(text11, Startup.parse_culture);
		float num10 = float.Parse(s, Startup.parse_culture);
		int numInfinitelyLevelablePerks = PerkControl.Instance.GetNumInfinitelyLevelablePerks();
		float num11 = (float)player_level;
		float num12 = num11 / 6f / (float)numInfinitelyLevelablePerks;
		float num13;
		if ((float)perk_level <= num12)
		{
			float t = 0f;
			if (num12 != 0f)
			{
				t = Mathf.Clamp01((float)perk_level / num12);
			}
			num13 = Mathf.Lerp(num8, num9, t);
		}
		else
		{
			num13 = Mathf.Lerp(num9, num10, 1f - 1f / (((float)perk_level - num12) * 0.5f + 1f));
		}
		switch (num7)
		{
		case 1:
			num11 = CombatControl.GetHpMaxPlayer(player_level, 0.5f);
			break;
		default:
			return 9999f;
		case 2:
			break;
		}
		int num14 = (int)(num13 * num11 * 0.01f);
		if (num14 == 0)
		{
			num14 = 1;
		}
		return num14;
	}

	public Vector3 GetVector3(string key, string effect_name, int perk_level, string remove_suffix = "")
	{
		if (all_effects.ContainsKey(effect_name) && all_effects[effect_name].data.ContainsKey(key))
		{
			string text = all_effects[effect_name].data[key];
			if (text.Contains("CLAMP"))
			{
				string text2 = text.Replace("CLAMP (", "");
				int num = text2.IndexOf('@');
				string text3 = text2.Substring(0, num - 1);
				if (remove_suffix != "")
				{
					text3 = text3.Replace(remove_suffix, "");
				}
				Vector3 vector = ExtractVector(text3);
				string text4 = text.Replace("CLAMP (", "");
				num += 12;
				string text5 = text4.Substring(num, text4.Length - num);
				int num2 = text5.IndexOf('@');
				string text6 = text5.Substring(0, num2 - 1);
				if (remove_suffix != "")
				{
					text6 = text6.Replace(remove_suffix, "");
				}
				Vector3 vector2 = ExtractVector(text6);
				string text7 = text.Replace("CLAMP (", "");
				string text8 = text7.Substring(num, text7.Length - num);
				float num3 = float.Parse(text8.Substring(num2 + 6, text8.Length - num2 - 7), Startup.parse_culture);
				float num4 = 0f;
				if (num3 != 1f)
				{
					num4 = Mathf.Clamp01(((float)perk_level - 1f) / (num3 - 1f));
				}
				return vector + (vector2 - vector) * num4;
			}
			return ExtractVector(text);
		}
		return Vector3.zero;
	}

	private Vector3 ExtractVector(string vector_str)
	{
		int num = vector_str.IndexOf(',');
		string s = vector_str.Substring(0, num);
		string text = vector_str.Substring(num + 1, vector_str.Length - (num + 1));
		int num2 = text.IndexOf(',');
		string s2 = text.Substring(0, num2);
		string s3 = text.Substring(num2 + 1, text.Length - (num2 + 1));
		return new Vector3(float.Parse(s, Startup.parse_culture), float.Parse(s2, Startup.parse_culture), float.Parse(s3, Startup.parse_culture));
	}

	public List<string> GetCreatureList(string key, string effect_name, int perk_level)
	{
		if (all_effects.ContainsKey(effect_name) && all_effects[effect_name].data.ContainsKey(key))
		{
			string text = all_effects[effect_name].data[key];
			if (text == "[PLAYER_COMBO]")
			{
				List<string> list = new List<string>();
				if (GameController.Instance.player != null)
				{
					foreach (string item in GameController.Instance.player.GetComponent<SharedCreature>().myCreatureModel.original.creatures_that_made_me)
					{
						list.Add(item);
					}
					return list;
				}
				list.Add("crab");
				list.Add("crab");
				return list;
			}
			if (text == "[RANDOM]")
			{
				return new List<string>
				{
					CreatureMorpher.Instance.GetRandomCreature(),
					CreatureMorpher.Instance.GetRandomCreature()
				};
			}
			return new List<string>(text.Split('+'));
		}
		return new List<string> { "crab", "crab" };
	}

	public float GetSummonedCreatureWalkSpeed(string effect_name, float perk_level)
	{
		if (!all_effects.ContainsKey(effect_name))
		{
			return -1f;
		}
		Dictionary<string, string> data = all_effects[effect_name].data;
		if (!data.ContainsKey("Summoned Creature Walk Speed"))
		{
			return CreatureStruct.DEFAULT_WALK_SPEED;
		}
		string text = data["Summoned Creature Walk Speed"];
		if (text == "[PLAYER_SPEED]")
		{
			float result = CreatureStruct.DEFAULT_WALK_SPEED;
			if (GameController.Instance.player != null)
			{
				result = GameController.Instance.player.GetComponent<SharedCreature>().walk_speed;
			}
			return result;
		}
		return float.Parse(text, Startup.parse_culture);
	}
}
