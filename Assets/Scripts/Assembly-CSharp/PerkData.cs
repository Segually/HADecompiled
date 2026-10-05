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
	}

	public void UnpackFromWeb(Packet incoming)
	{
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
		return null;
	}

	public bool GetBool(string key, string effect_name, int perk_level)
	{
		return false;
	}

	public int GetInt(string key, string effect_name, int perk_level, int player_level, string remove_suffix = "")
	{
		return 0;
	}

	public float GetFloat(string key, string effect_name, int perk_level, int player_level, string remove_suffix = "")
	{
		return 0f;
	}

	public Vector3 GetVector3(string key, string effect_name, int perk_level, string remove_suffix = "")
	{
		return default(Vector3);
	}

	private Vector3 ExtractVector(string vector_str)
	{
		return default(Vector3);
	}

	public List<string> GetCreatureList(string key, string effect_name, int perk_level)
	{
		return null;
	}

	public float GetSummonedCreatureWalkSpeed(string effect_name, float perk_level)
	{
		return 0f;
	}
}
