using System;
using System.Collections.Generic;
using UnityEngine;

public class PopupSaw : MonoBehaviour
{
	public enum action_type_t
	{
		saw = 0,
		blast = 1
	}

	public GameObject saw;

	private int last_second_processed = -1;

	private bool saw_up = true;

	public string animation_saw_up;

	public string animation_saw_down;

	public bool reverse_timing;

	public action_type_t action_type;

	public float hit_check_dist;

	private int hit_checker;

	private void Descend()
	{
		if (saw_up)
		{
			GetComponent<Animation>().Stop();
			GetComponent<Animation>().Play(animation_saw_down);
			saw_up = false;
		}
	}

	private void Ascend()
	{
		if (saw_up)
		{
			return;
		}
		GetComponent<Animation>().Stop();
		GetComponent<Animation>().Play(animation_saw_up);
		saw_up = true;
		if (action_type != action_type_t.blast)
		{
			return;
		}
		foreach (KeyValuePair<string, GameObject> active_combatant in MobControl.Instance.active_combatants)
		{
			Combatant component = active_combatant.Value.GetComponent<Combatant>();
			if (component.mob_type != Combatant.TYPE_T.creature || !component.GetComponent<SharedCreature>().is_local_mob)
			{
				continue;
			}
			for (int i = 0; i < base.transform.childCount; i++)
			{
				Transform child = base.transform.GetChild(i);
				if (child.name.Contains("hitchecker") && Vector3.Distance(child.position, component.transform.position) < hit_check_dist)
				{
					DamageCombatant(0.04f, component);
					if (component.GetComponent<Rigidbody>() != null)
					{
						component.GetComponent<Rigidbody>().velocity = Vector3.up * 10f;
						component.GetComponent<Rigidbody>().velocity -= base.transform.right * 15f;
					}
					break;
				}
			}
		}
	}

	private void FixedUpdate()
	{
		if (action_type == action_type_t.saw)
		{
			saw.transform.Rotate(base.transform.forward, 57.29578f);
			if (saw_up)
			{
				if (hit_checker < 10)
				{
					hit_checker++;
				}
				else
				{
					foreach (KeyValuePair<string, GameObject> active_combatant in MobControl.Instance.active_combatants)
					{
						Combatant component = active_combatant.Value.GetComponent<Combatant>();
						if (component.mob_type != Combatant.TYPE_T.creature || !component.GetComponent<SharedCreature>().is_local_mob)
						{
							continue;
						}
						for (int i = 0; i < base.transform.childCount; i++)
						{
							Transform child = base.transform.GetChild(i);
							if (child.name.Contains("hitchecker") && Vector3.Distance(child.position, component.transform.position) < hit_check_dist)
							{
								DamageCombatant(0.1f, component);
								if (component.GetComponent<Rigidbody>() != null)
								{
									component.GetComponent<Rigidbody>().velocity = Vector3.up * 3f;
									component.GetComponent<Rigidbody>().velocity -= (base.transform.position - component.transform.position).normalized * 3f;
								}
								break;
							}
						}
					}
					hit_checker = 0;
				}
			}
		}
		int second = DateTime.Now.Second;
		if (last_second_processed == second)
		{
			return;
		}
		if (second <= 60)
		{
			bool flag = second % 4 < 2;
			bool flag2 = second % 4 >= 2 && second != 46 && second != 47;
			if (flag)
			{
				if (!reverse_timing)
				{
					Descend();
				}
				else
				{
					Ascend();
				}
			}
			else if (flag2)
			{
				if (reverse_timing)
				{
					Descend();
				}
				else
				{
					Ascend();
				}
			}
		}
		last_second_processed = second;
	}

	private void DamageCombatant(float hurt_percentage, Combatant combatant)
	{
		int num = (int)((float)combatant.HP_max * hurt_percentage);
		if (num == 0)
		{
			num = 1;
		}
		combatant.WasHit(num, combatant.gameObject, false, false, Combatant.hit_col.color_red, true);
	}
}
