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

	public List<GameObject> spawned_minions = new List<GameObject>();

	public bool is_removed;

	public DurationEffect(PerkData perk_data, string _effect_name, int perk_level, float time_remaining, float next_apply, string original_caster_id, int original_caster_level, GameObject duration_particle)
	{
		this.perk_data = perk_data;
		effect_name = _effect_name;
		this.perk_level = perk_level;
		this.time_remaining = time_remaining;
		this.next_apply = next_apply;
		this.original_caster_id = original_caster_id;
		this.duration_particle = duration_particle;
		this.original_caster_level = original_caster_level;
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
		return perk_data.GetBool(key, effect_name, perk_level);
	}

	public float GetSummonedCreatureWalkSpeed(string key)
	{
		return perk_data.GetSummonedCreatureWalkSpeed(effect_name, perk_level);
	}

	public List<string> GetCreatureList(string key)
	{
		return perk_data.GetCreatureList(key, effect_name, perk_level);
	}
}
