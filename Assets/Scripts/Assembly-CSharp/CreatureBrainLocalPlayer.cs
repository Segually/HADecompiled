using System.Collections;
using UnityEngine;

public class CreatureBrainLocalPlayer : MonoBehaviour, CreatureBrainInterface
{
	private enum state
	{
		stand_still = 0,
		move = 1,
		collect_item = 2,
		attacking_target = 3
	}

	private state curr_state;

	public GameObject interaction_target;

	public void Init()
	{
	}

	public void StopEverything()
	{
		interaction_target = null;
		GetComponent<CreatureBrain>().ClearAllTargets();
		if (curr_state != state.stand_still)
		{
			GetComponent<SharedCreature>().CancelMoveto();
		}
		curr_state = state.stand_still;
	}

	public void PursueCollectible(GameObject closest_interactable_obj)
	{
		if (!GetComponent<SharedCreature>().snapped_to_chair_obj)
		{
			GetComponent<SharedCreature>().SetMoveTo(GetComponent<SharedCreature>().DestinationWithSpacing(base.transform.position, closest_interactable_obj.transform.position + Vector3.up * SharedCreature.H, closest_interactable_obj.GetComponent<Collectible>().interaction_distance));
			interaction_target = closest_interactable_obj;
			curr_state = state.collect_item;
		}
		else if (Vector3.Distance(new Vector3(base.transform.position.x, 0f, base.transform.position.z), new Vector3(closest_interactable_obj.transform.position.x, 0f, closest_interactable_obj.transform.position.z)) < 1.5f)
		{
			GameController.Instance.player_interact(closest_interactable_obj);
		}
	}

	public void PursueInteractable(GameObject closest_interactable_obj)
	{
		if (!GetComponent<SharedCreature>().snapped_to_chair_obj)
		{
			GetComponent<SharedCreature>().SetMoveTo(GetComponent<SharedCreature>().DestinationWithSpacing(base.transform.position, closest_interactable_obj.transform.position + Vector3.up * SharedCreature.H, closest_interactable_obj.GetComponent<Interactable>().interaction_distance));
			interaction_target = closest_interactable_obj;
			curr_state = state.collect_item;
		}
		else if (Vector3.Distance(new Vector3(base.transform.position.x, 0f, base.transform.position.z), new Vector3(closest_interactable_obj.transform.position.x, 0f, closest_interactable_obj.transform.position.z)) < 1.5f)
		{
			GameController.Instance.player_interact(closest_interactable_obj);
		}
	}

	public void SelectTarget(GameObject closest_combatant)
	{
		GetComponent<CreatureBrain>().AddToFrontOfTargetList(closest_combatant);
		interaction_target = null;
		curr_state = state.attacking_target;
	}

	public void SelectMovePosition(Vector3 position)
	{
		GetComponent<SharedCreature>().SetMoveTo(position);
		interaction_target = null;
		curr_state = state.move;
	}

	public IEnumerator Think()
	{
		while (true)
		{
			yield return new WaitForSeconds(CreatureBrain.think_clock_speed);
			if (GameController.Instance.is_paused() || curr_state != state.attacking_target)
			{
				continue;
			}
			if (GetComponent<CreatureBrain>().main_target == null)
			{
				GetComponent<SharedCreature>().CancelMoveto();
				curr_state = state.stand_still;
				continue;
			}
			CreatureBrain component = GetComponent<CreatureBrain>();
			if (!GetComponent<PerkReceiver>().CheckPerkBoolApplied("Let My Target Come To Me"))
			{
				component.PursueTarget();
			}
		}
	}

	public void ReactOnHit(GameObject hit_by)
	{
		if (!(hit_by == null) && hit_by != base.gameObject)
		{
			GameController.Instance.MakeAlliesSwarm(hit_by, true, true, false);
		}
	}

	public void OnFallOffWorld()
	{
		if (!InventoryUtils.IsHeavenDimension(ZoneDataControl.Instance.curr_zonedata.house_item.item_name))
		{
			GetComponent<SharedCreature>().ResetHeight();
			return;
		}
		WindowControl.Instance.CloseAllWindows();
		TransitionControl.Instance.BeginExitHouseTransition();
	}

	public void TriggerOnReachDesiredMoveAt()
	{
	}

	public void CustomFixedUpdate()
	{
		if (curr_state != state.attacking_target || GetComponent<SharedCreature>().isQuickTagging)
		{
			return;
		}
		GameObject main_target = GetComponent<CreatureBrain>().main_target;
		if (main_target != null && main_target.GetComponent<Combatant>().mob_type == Combatant.TYPE_T.creature)
		{
			SharedCreature component = main_target.GetComponent<SharedCreature>();
			switch (component.brain_type)
			{
			case SharedCreature.brain_type_t.ghost:
				return;
			case SharedCreature.brain_type_t.guard:
				if (!component.IsTargettingPlayerOrMyCompanions(true, true))
				{
					return;
				}
				break;
			case SharedCreature.brain_type_t.companion:
				if (component.is_local_mob && main_target.GetComponent<CreatureBrainCompanion>().companion_struct != null)
				{
					return;
				}
				break;
			}
		}
		if (GetComponent<CreatureBrain>().main_target != null)
		{
			GetComponent<CreatureBrain>().AttackWhenPossible();
		}
	}
}
