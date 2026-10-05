using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureBrainAggressive : MonoBehaviour, CreatureBrainInterface
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

	private int notice_player_timer;

	private int notice_player_timer_max;

	private float dist_return_to_spawn = 23f;

	public void Init()
	{
		roam_to_new_position_timer_max = (int)Random.Range(2f / CreatureBrain.think_clock_speed, 5f / CreatureBrain.think_clock_speed);
		notice_player_timer_max = (int)Random.Range(2f / CreatureBrain.think_clock_speed, 5f / CreatureBrain.think_clock_speed);
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
					GameObject gameObject = null;
					notice_player_timer++;
					if (notice_player_timer == notice_player_timer_max)
					{
						float num = float.MaxValue;
						foreach (KeyValuePair<string, GameObject> active_combatant in MobControl.Instance.active_combatants)
						{
							GameObject value = active_combatant.Value;
							if (value == base.gameObject)
							{
								continue;
							}
							float ai_lockon_range = GetComponent<SharedCreature>().ai_lockon_range;
							Combatant component2 = value.GetComponent<Combatant>();
							if (!value.GetComponent<PerkReceiver>().CheckPerkBoolApplied("Invisible To Mobs If Not Already Targetted"))
							{
								bool flag = false;
								if (component2.mob_type == Combatant.TYPE_T.creature)
								{
									SharedCreature.brain_type_t brain_type = value.GetComponent<SharedCreature>().brain_type;
									flag = ((brain_type != SharedCreature.brain_type_t.local_player) ? (brain_type == SharedCreature.brain_type_t.net_player || brain_type == SharedCreature.brain_type_t.companion) : (PlayerData.Instance.GetGlobalShort("dev_mode") != 1));
								}
								float num2 = Vector3.Distance(value.transform.position, base.transform.position);
								if (flag && num2 < ai_lockon_range && num2 < num)
								{
									num = num2;
									gameObject = value;
								}
							}
						}
						notice_player_timer = 0;
					}
					if (gameObject != null)
					{
						GetComponent<CreatureBrain>().AddToFrontOfTargetList(gameObject);
						curr_state = state.attacking_target;
					}
					else
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
