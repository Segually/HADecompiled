using System.Collections.Generic;
using UnityEngine;

public class DurationEffect
{
	public PerkData perk_data;

	public string effect_name;

	public int perk_level;

	public float time_remaining;

	public float next_apply;

	public string original_caster_id;

	public int original_caster_level;

	public GameObject duration_particle;

	public List<GameObject> spawned_minions;

	public bool is_removed;

	public DurationEffect(PerkData perk_data, string _effect_name, int perk_level, float time_remaining, float next_apply, string original_caster_id, int original_caster_level, GameObject duration_particle)
	{
	}

	public float GetFloat(string key, string remove_suffix = "")
	{
		return perk_data.GetFloat(key, effect_name, perk_level, original_caster_level, remove_suffix);
	}

	public int GetInt(string key, string remove_suffix = "")
	{
		return perk_data.GetInt(key, effect_name, perk_level, original_caster_level, remove_suffix);
	}

	public string GetString(string key)
	{
		return perk_data.GetString(key, effect_name, perk_level);
	}

	public bool GetBool(string key)
	{
		return false;
	}

	public float GetSummonedCreatureWalkSpeed(string key)
	{
		return 0f;
	}

	public List<string> GetCreatureList(string key)
	{
		return null;
	}
}
