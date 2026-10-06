using System.Collections.Generic;
using UnityEngine;

public class PoisonSpikes : MonoBehaviour
{
	private int hit_checker;

	private void FixedUpdate()
	{
		if (hit_checker < 1)
		{
			foreach (KeyValuePair<string, GameObject> active_combatant in MobControl.Instance.active_combatants)
			{
				Combatant component = active_combatant.Value.GetComponent<Combatant>();
				if (component.mob_type != Combatant.TYPE_T.creature || !component.GetComponent<SharedCreature>().is_local_mob || !(Vector3.Distance(base.transform.position, component.transform.position) < 0.9f))
				{
					continue;
				}
				int num = (int)((float)component.HP_max * 0.07f);
				if (num == 0)
				{
					num = 1;
				}
				component.WasHit(num, component.gameObject, false, false, Combatant.hit_col.color_red, true);
				if (component.GetComponent<Rigidbody>() != null)
				{
					Rigidbody component2 = component.GetComponent<Rigidbody>();
					component2.velocity = Vector3.up * 3f;
					component2.velocity -= (base.transform.position - component.transform.position).normalized * 3f;
				}
			}
			hit_checker = 10;
		}
		hit_checker--;
	}
}
