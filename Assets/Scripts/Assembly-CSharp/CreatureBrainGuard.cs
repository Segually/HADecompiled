using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureBrainGuard : MonoBehaviour, CreatureBrainInterface
{
	private enum state
	{
		return_to_post = 0,
		attacking_target = 1
	}

	private state curr_state;

	public void Init()
	{
	}

	public IEnumerator Think()
	{
		while (true)
		{
			if (!GameController.Instance.is_paused())
			{
				if (curr_state == state.attacking_target)
				{
					CreatureBrain component = GetComponent<CreatureBrain>();
					if (component.main_target == null)
					{
						curr_state = state.return_to_post;
					}
					else if (!component.main_target.GetComponent<PerkReceiver>().CheckPerkBoolApplied("Mobs Targetting Me Will Pause") && !GetComponent<PerkReceiver>().CheckPerkBoolApplied("Let My Target Come To Me"))
					{
						component.PursueTarget();
					}
				}
				else if (curr_state == state.return_to_post)
				{
					GameObject gameObject = null;
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
							bool flag = component2.mob_type == Combatant.TYPE_T.creature && (value.GetComponent<SharedCreature>().brain_type == SharedCreature.brain_type_t.neutral || value.GetComponent<SharedCreature>().brain_type == SharedCreature.brain_type_t.aggressive || value.GetComponent<SharedCreature>().brain_type == SharedCreature.brain_type_t.fearful);
							float num2 = Vector3.Distance(value.transform.position, base.transform.position);
							if (flag && num2 < ai_lockon_range && num2 < num)
							{
								num = num2;
								gameObject = value;
							}
						}
					}
					if (gameObject != null)
					{
						GetComponent<CreatureBrain>().AddToFrontOfTargetList(gameObject);
						curr_state = state.attacking_target;
					}
					else
					{
						GetComponent<SharedCreature>().SetMoveTo(GetComponent<Combatant>().startPos);
					}
				}
			}
			yield return new WaitForSeconds(CreatureBrain.think_clock_speed);
		}
	}

	public void TriggerOnReachDesiredMoveAt()
	{
		if (curr_state == state.return_to_post)
		{
			SharedCreature component = GetComponent<SharedCreature>();
			switch (GetComponent<Combatant>().original_element_rot)
			{
			case 0:
				component.SpotterLookAt(base.transform.position + Vector3.back);
				break;
			case 1:
				component.SpotterLookAt(base.transform.position + Vector3.left);
				break;
			case 2:
				component.SpotterLookAt(base.transform.position + Vector3.forward);
				break;
			case 3:
				component.SpotterLookAt(base.transform.position + Vector3.right);
				break;
			}
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
