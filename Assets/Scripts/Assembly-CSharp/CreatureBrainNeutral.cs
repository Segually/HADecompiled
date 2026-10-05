using System.Collections;
using UnityEngine;

public class CreatureBrainNeutral : MonoBehaviour, CreatureBrainInterface
{
	private enum state
	{
		roaming = 0,
		attacking_target = 1,
		override_return_to_spawn = 2
	}

	private state curr_state;

	private int roam_to_new_position_timer;

	private int roam_to_new_position_timer_max;

	private float dist_return_to_spawn = 23f;

	public void Init()
	{
		roam_to_new_position_timer_max = (int)Random.Range(2f / CreatureBrain.think_clock_speed, 5f / CreatureBrain.think_clock_speed);
	}

	public IEnumerator Think()
	{
		while (true)
		{
			if (!GameController.Instance.is_paused())
			{

				if (curr_state == state.attacking_target)
				{
					if (Vector3.Distance(base.transform.position, GetComponent<Combatant>().startPos) > dist_return_to_spawn)
					{
						curr_state = state.override_return_to_spawn;
						GetComponent<SharedCreature>().SetMoveTo(GetComponent<Combatant>().startPos);
					}
					else
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
				}
				else if (curr_state == state.roaming)
				{

					roam_to_new_position_timer++;
					if (roam_to_new_position_timer == roam_to_new_position_timer_max)
					{
						GetComponent<SharedCreature>().SetMoveTo(GetComponent<Combatant>().startPos + new Vector3(Random.value, 0f, Random.value) * GetComponent<SharedCreature>().wander_dist);
						roam_to_new_position_timer = 0;
					}
				}
			}
			yield return new WaitForSeconds(CreatureBrain.think_clock_speed);
		}
	}

	public void TriggerOnReachDesiredMoveAt()
	{
		if (curr_state == state.override_return_to_spawn)
		{
			curr_state = state.attacking_target;
		}
	}

	public void CustomFixedUpdate()
	{
		if (curr_state == state.attacking_target)
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
		if (curr_state <= state.attacking_target && !(hit_by == null))
		{
			GetComponent<CreatureBrain>().AddToFrontOfTargetList(hit_by);
			curr_state = state.attacking_target;
		}
	}
}
