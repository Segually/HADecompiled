using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerkProjectile : MonoBehaviour
{
	public GameObject hide_on_collide;

	public ParticleSystem stop_on_collide;

	private List<string> EFFECTS;

	private PerkData perk_data;

	private int PERK_LEVEL;

	private float speed;

	private GameObject target;

	private Vector3 target_pos;

	private string caster_id;

	private int caster_level;

	private bool exploded;

	public void SHOOT_AT_(List<string> EFFECTS, PerkData perk_data, int PERK_LEVEL, float speed, Vector3 target_pos, GameObject target_obj, string caster_id, int caster_level)
	{
	}

	private void FixedUpdate()
	{
	}

	private void EXPLODE()
	{
	}

	private IEnumerator delayed_destroy()
	{
		return null;
	}
}
