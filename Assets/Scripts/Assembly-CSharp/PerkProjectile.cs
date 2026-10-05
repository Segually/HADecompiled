using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerkProjectile : MonoBehaviour
{
	public GameObject hide_on_collide;

	public ParticleSystem stop_on_collide;

	private List<string> EFFECTS = new List<string>();

	private PerkData perk_data;

	private int PERK_LEVEL;

	private float speed;

	private GameObject target;

	private Vector3 target_pos = Vector3.zero;

	private string caster_id;

	private int caster_level;

	private bool exploded;

	public void SHOOT_AT_(List<string> EFFECTS, PerkData perk_data, int PERK_LEVEL, float speed, Vector3 target_pos, GameObject target_obj, string caster_id, int caster_level)
	{
		this.speed = speed;
		this.EFFECTS = EFFECTS;
		this.perk_data = perk_data;
		this.PERK_LEVEL = PERK_LEVEL;
		this.target_pos = target_pos;
		target = target_obj;
		this.caster_id = caster_id;
		this.caster_level = caster_level;
	}

	private void FixedUpdate()
	{
		if (!exploded)
		{
			if (target != null)
			{
				target_pos = target.transform.position;
			}
			base.transform.LookAt(target_pos);
			base.transform.position = base.transform.position + base.transform.forward * speed;
			if (Vector3.Distance(base.transform.position, target_pos) < 0.5f)
			{
				EXPLODE();
			}
		}
	}

	private void EXPLODE()
	{
		exploded = true;
		if (hide_on_collide != null)
		{
			hide_on_collide.SetActive(false);
		}
		if (stop_on_collide != null)
		{
			stop_on_collide.Stop();
		}
		if (target != null && PerkControl.Instance.ShouldApplyEffectRightNow(caster_id))
		{
			foreach (string eFFECT in EFFECTS)
			{
				target.GetComponent<PerkReceiver>().ApplyPerkEffect(false, eFFECT, perk_data, PERK_LEVEL, caster_id, caster_level, true);
			}
		}
		StartCoroutine(delayed_destroy());
	}

	private IEnumerator delayed_destroy()
	{
		yield return new WaitForSeconds(1.5f);
		Object.Destroy(base.gameObject);
	}
}
