using System.Collections;
using System.Collections.Generic;
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
		if (curr_state == state.move)
		{
			GameController.Instance.HideTargetCircle();
			return;
		}
		if (curr_state == state.collect_item)
		{
			GameController.Instance.player_interact(interaction_target);
			interaction_target = null;
			curr_state = state.stand_still;
			return;
		}
		if (curr_state != state.attacking_target)
		{
			return;
		}
		bool isQuickTagging = GetComponent<SharedCreature>().isQuickTagging;
		GameObject main_target = GetComponent<CreatureBrain>().main_target;
		if (isQuickTagging)
		{
			if (main_target != null)
			{
				foreach (string quickTagEffect in GetComponent<SharedCreature>().quickTagEffects)
				{
					main_target.GetComponent<PerkReceiver>().ApplyPerkEffect(false, quickTagEffect, GetComponent<SharedCreature>().quickTagPerkData, GetComponent<SharedCreature>().quickTagPerkLevel, "LOCAL", GameController.Instance.playerLevel, true);
				}
			}
			GetComponent<SharedCreature>().isQuickTagging = false;
			GameServerSender.Instance.SendQuickTag(0, "");
			GetComponent<CreatureBrain>().ClearAllTargets();
			curr_state = state.stand_still;
			return;
		}
		if (main_target != null && main_target.GetComponent<Combatant>().mob_type == Combatant.TYPE_T.creature)
		{
			SharedCreature component = main_target.GetComponent<SharedCreature>();
			SharedCreature.brain_type_t brain_type = component.brain_type;
			if (brain_type == SharedCreature.brain_type_t.ghost)
			{
				GetComponent<CreatureBrain>().ClearAllTargets();
				GameController.Instance.HideTargetCircle();
				if (WindowControl.Instance.CanOpenGenericWindow())
				{
					WindowControl.Instance.DoOpenGenericWindow();
					Combatant component2 = main_target.GetComponent<Combatant>();
					InventoryItem inventoryItem = component2.original_element_item.LoadSubItem("ghost");
					string text = CreatureBrainGhost.CorruptString(inventoryItem.GetString("wait_message1"));
					string text2 = CreatureBrainGhost.CorruptString(inventoryItem.GetString("wait_message2"));
					string text3 = CreatureBrainGhost.CorruptString(inventoryItem.GetString("wait_message3"));
					string text4 = CreatureBrainGhost.CorruptString(inventoryItem.GetString("wait_message4"));
					Debug.Log("message1=" + text);
					Dictionary<int, Dictionary<string, object>> dictionary = new Dictionary<int, Dictionary<string, object>>();
					List<string> list = new List<string>();
					if (!Startup.StringNullOrWhitespace(text))
					{
						list.Add(text);
					}
					if (!Startup.StringNullOrWhitespace(text2))
					{
						list.Add(text2);
					}
					if (!Startup.StringNullOrWhitespace(text3))
					{
						list.Add(text3);
					}
					if (!Startup.StringNullOrWhitespace(text4))
					{
						list.Add(text4);
					}
					int num = 1;
					foreach (string item in list)
					{
						Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
						dictionary2.Add("type", "NPC_speak");
						dictionary2.Add("text", item);
						dictionary2.Add("go_to", num);
						dictionary.Add(num - 1, dictionary2);
						num++;
					}
					string curr_NPC_display_name = component.creature_name.ToUpper();
					string curr_NPC_combo_text = "(" + component.myCreatureModel.original.creatures_that_made_me_TRANSLATED[0] + " + " + component.myCreatureModel.original.creatures_that_made_me_TRANSLATED[1] + ")";
					GameController.Instance.NoteInteractingElement(component2.origin_chunkX, component2.origin_chunkZ, component2.origin_innerX, component2.origin_innerZ, component2.original_element_item, component2.original_element_rot);
					DialogueControl.Instance.SetFocusNpc(main_target, curr_NPC_display_name, curr_NPC_combo_text, DialogueControl.focus_type_t.moving_npc);
					DialogueControl.Instance.EnterDialogue(dictionary, 0, "ghost");
				}
				return;
			}
			if (brain_type == SharedCreature.brain_type_t.guard)
			{
				if (!component.IsTargettingPlayerOrMyCompanions(true, true))
				{
					GetComponent<CreatureBrain>().ClearAllTargets();
					GameController.Instance.HideTargetCircle();
					if (WindowControl.Instance.CanOpenGenericWindow())
					{
						WindowControl.Instance.DoOpenGenericWindow();
						Combatant component3 = main_target.GetComponent<Combatant>();
						string text5 = component3.original_element_item.GetString("guard_message1");
						string text6 = component3.original_element_item.GetString("guard_message2");
						Dictionary<int, Dictionary<string, object>> dictionary3 = new Dictionary<int, Dictionary<string, object>>();
						if (component3.original_element_item.GetString("tag") == "dev_obj")
						{
							text5 = TranslationControl.Instance.TranslateGeneral(text5, "CompanionsEtc");
							text6 = TranslationControl.Instance.TranslateGeneral(text6, "CompanionsEtc");
							bool flag = Startup.StringNullOrWhitespace(text6);
							Dictionary<string, object> dictionary4 = new Dictionary<string, object>();
							dictionary4.Add("type", "NPC_speak");
							dictionary4.Add("text", text5);
							if (!flag)
							{
								dictionary4.Add("go_to", 1);
								dictionary3.Add(0, dictionary4);
								dictionary4 = new Dictionary<string, object>();
								dictionary4.Add("type", "NPC_speak");
								dictionary4.Add("text", text6);
								dictionary4.Add("go_to", -1);
								dictionary3.Add(1, dictionary4);
							}
							else
							{
								dictionary4.Add("go_to", -1);
								dictionary3.Add(0, dictionary4);
							}
						}
						else
						{
							List<string> list2 = new List<string>();
							if (!Startup.StringNullOrWhitespace(text5))
							{
								list2.Add(text5);
							}
							if (!Startup.StringNullOrWhitespace(text6))
							{
								list2.Add(text6);
							}
							int num2 = 0;
							foreach (string item2 in list2)
							{
								Dictionary<string, object> dictionary5 = new Dictionary<string, object>();
								dictionary5.Add("type", "NPC_speak");
								dictionary5.Add("text", item2);
								dictionary5.Add("go_to", num2 + 1);
								dictionary3.Add(num2, dictionary5);
								num2++;
							}
							Dictionary<string, object> dictionary6 = new Dictionary<string, object>();
							dictionary6.Add("type", "MY_options");
							dictionary6.Add("optionA", TranslationControl.Instance.TranslateGeneral("FOLLOW ME", "CompanionsEtc"));
							dictionary6.Add("optionA_goto", -66);
							dictionary3.Add(num2, dictionary6);
						}
						string curr_NPC_display_name2 = component.creature_name.ToUpper();
						string curr_NPC_combo_text2 = "(" + component.myCreatureModel.original.creatures_that_made_me_TRANSLATED[0] + " + " + component.myCreatureModel.original.creatures_that_made_me_TRANSLATED[1] + ")";
						string voice = component3.original_element_item.GetString("voice");
						GameController.Instance.NoteInteractingElement(component3.origin_chunkX, component3.origin_chunkZ, component3.origin_innerX, component3.origin_innerZ, component3.original_element_item, component3.original_element_rot);
						DialogueControl.Instance.SetFocusNpc(main_target, curr_NPC_display_name2, curr_NPC_combo_text2, DialogueControl.focus_type_t.moving_npc);
						DialogueControl.Instance.EnterDialogue(dictionary3, 0, voice);
					}
					return;
				}
			}
			else if (brain_type == SharedCreature.brain_type_t.companion && component.is_local_mob)
			{
				CreatureBrainCompanion component4 = main_target.GetComponent<CreatureBrainCompanion>();
				if (component4.companion_struct != null)
				{
					CompanionController.Instance.PressCompanionButton(component4.companion_struct.hatch_index);
					GetComponent<CreatureBrain>().ClearAllTargets();
					GameController.Instance.HideTargetCircle();
					return;
				}
			}
		}
		if (GetComponent<CreatureBrain>().main_target != null)
		{
			GameController.Instance.MakeAlliesSwarm(GetComponent<CreatureBrain>().main_target, false, true, false);
		}
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
