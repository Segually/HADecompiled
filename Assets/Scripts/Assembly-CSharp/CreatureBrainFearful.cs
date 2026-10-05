using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureBrainFearful : MonoBehaviour, CreatureBrainInterface
{
	private enum state
	{
		roaming = 0,
		fleeing_from_chaser = 1,
		low_HP_retaliate = 2
	}

	private state curr_state;

	private GameObject chasing_me;

	private int roam_to_new_position_timer;

	private int roam_to_new_position_timer_max;

	private int run_away_deke_timer;

	private int run_away_deke_timer_max;

	public void Init()
	{
		roam_to_new_position_timer_max = (int)Random.Range(2f / CreatureBrain.think_clock_speed, 5f / CreatureBrain.think_clock_speed);
		run_away_deke_timer_max = (int)Random.Range(1f / CreatureBrain.think_clock_speed, 3f / CreatureBrain.think_clock_speed);
	}

	public IEnumerator Think()
	{
		while (true)
		{
			if (!GameController.Instance.is_paused())
			{
				if (curr_state == state.low_HP_retaliate)
				{
					CreatureBrain component = GetComponent<CreatureBrain>();
					if (component.main_target == null)
					{
						GetComponent<SharedCreature>().CancelMoveto();
						curr_state = state.roaming;
					}
					else if (!component.main_target.GetComponent<PerkReceiver>().CheckPerkBoolApplied("Mobs Targetting Me Will Pause") && !GetComponent<PerkReceiver>().CheckPerkBoolApplied("Let My Target Come To Me"))
					{
						component.PursueTarget();
					}
				}
				else if (curr_state == state.fleeing_from_chaser)
				{
					if (chasing_me == null)
					{
						GetComponent<SharedCreature>().CancelMoveto();
						curr_state = state.roaming;
					}
					else if (Vector3.Distance(chasing_me.transform.position, base.transform.position) <= 10f)
					{
						run_away_deke_timer++;
						if (run_away_deke_timer == run_away_deke_timer_max)
						{
							Vector3 normalized = (base.transform.position - chasing_me.transform.position).normalized;
							Vector3 vector = Quaternion.Euler(0f, Random.Range(-50, 50), 0f) * normalized;
							GetComponent<SharedCreature>().SetMoveTo(vector * 15f + base.transform.position);
							run_away_deke_timer = 0;
						}
					}
					else
					{
						GetComponent<SharedCreature>().CancelMoveto();
						curr_state = state.roaming;
						chasing_me = null;
					}
				}
				else if (curr_state == state.roaming)
				{
					foreach (KeyValuePair<string, GameObject> active_combatant in MobControl.Instance.active_combatants)
					{
						GameObject value = active_combatant.Value;
						if (value == base.gameObject || Vector3.Distance(value.transform.position, base.transform.position) >= GetComponent<SharedCreature>().ai_lockon_range)
						{
							continue;
						}
						Combatant component2 = value.GetComponent<Combatant>();
						if (!value.GetComponent<PerkReceiver>().CheckPerkBoolApplied("Invisible To Mobs If Not Already Targetted"))
						{
							if (value == GameController.Instance.player)
							{
								chasing_me = value;
								curr_state = state.fleeing_from_chaser;
								break;
							}
							if (component2.mob_type == Combatant.TYPE_T.creature && value.GetComponent<SharedCreature>().brain_type != SharedCreature.brain_type_t.fearful)
							{
								chasing_me = value;
								curr_state = state.fleeing_from_chaser;
								break;
							}
						}
					}
					if (chasing_me == null)
					{

						roam_to_new_position_timer++;
						if (roam_to_new_position_timer == roam_to_new_position_timer_max)
						{
							GetComponent<SharedCreature>().SetMoveTo(GetComponent<Combatant>().startPos + new Vector3(Random.value, 0f, Random.value) * GetComponent<SharedCreature>().wander_dist);
							roam_to_new_position_timer = 0;
						}
					}
				}
			}
			yield return new WaitForSeconds(CreatureBrain.think_clock_speed);
		}
	}

	public void TriggerOnReachDesiredMoveAt()
	{
	}

	public void CustomFixedUpdate()
	{
		if (curr_state == state.low_HP_retaliate)
		{
			CreatureBrain component = GetComponent<CreatureBrain>();
			if (component.main_target != null && !component.main_target.GetComponent<PerkReceiver>().CheckPerkBoolApplied("Mobs Targetting Me Will Pause"))
			{
				component.AttackWhenPossible();
			}
		}
	}

	public void OnFallOffWorld()
	{
		bool flag = InventoryUtils.IsHeavenDimension(ZoneDataControl.Instance.curr_zonedata.house_item.item_name);
		SharedCreature component = GetComponent<SharedCreature>();
		if (flag)
		{
			component.Deload(true);
			Object.Destroy(base.gameObject);
		}
		else
		{
			component.ResetHeight();
		}
	}

	public void ReactOnHit(GameObject hit_by)
	{
		switch (curr_state)
		{
		case state.roaming:
			chasing_me = hit_by;
			curr_state = state.fleeing_from_chaser;
			return;
		case state.fleeing_from_chaser:
			if (GetComponent<Combatant>().hp >= (float)GetComponent<Combatant>().HP_max * 0.33333334f)
			{
				return;
			}
			chasing_me = null;
			break;
		case state.low_HP_retaliate:
			if (hit_by == null)
			{
				return;
			}
			break;
		default:
			return;
		}
		GetComponent<CreatureBrain>().AddToFrontOfTargetList(hit_by);
		curr_state = state.low_HP_retaliate;
	}
}
