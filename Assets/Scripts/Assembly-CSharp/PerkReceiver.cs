using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerkReceiver : MonoBehaviour
{
	public GameObject curr_perk_animation;

	public List<DurationEffect> duration_effects = new List<DurationEffect>();

	public Combatant my_combatant;

	public bool delete_once_all_effects_gone;

	public void InitializeDurationTimer(int duration, string effect_name, PerkData perk_Data, int perk_level, string caster_id, int caster_level, bool pre_applied)
	{
		float @float = perk_Data.GetFloat("Apply every", effect_name, perk_level, caster_level, " seconds");
		float next_apply = 1f;
		if (@float != -1f)
		{
			next_apply = @float;
		}
		if (perk_Data.GetBool("Reset Caster ID to self", effect_name, perk_level) && my_combatant != null)
		{
			caster_id = my_combatant.combat_name;
		}
		DurationEffect durationEffect = new DurationEffect(perk_Data, effect_name, perk_level, duration, next_apply, caster_id, caster_level, null);
		duration_effects.Add(durationEffect);
		string @string = durationEffect.GetString("Particle (Duration)");
		string string2 = durationEffect.GetString("Particle (Screen Overlay)");
		if (@string != "")
		{
			ApplyDurationParticle(@string, durationEffect, perk_Data, effect_name, pre_applied);
		}
		else if (string2 != "" && base.gameObject == GameController.Instance.player)
		{
			ApplyScreenOverlay(string2, durationEffect);
		}
		int @int = durationEffect.GetInt("Num Creatures To Summon");
		if (@int != -1)
		{
			SummonMinions(durationEffect, @int, perk_Data, effect_name, perk_level, caster_id, caster_level);
		}
		string string3 = durationEffect.GetString("Change skin mat");
		if (string3 != "")
		{
			ApplySkinMaterial(string3);
		}
		string string4 = durationEffect.GetString("Custom Brain");
		if (string4 != "")
		{
			ApplyCustomBrain(durationEffect, string4);
		}
		int int2 = durationEffect.GetInt("Change Icon To");
		if (int2 != -1)
		{
			SharedCreature component = GetComponent<SharedCreature>();
			if (component != null)
			{
				component.RedrawIcon(int2);
			}
		}
		if (durationEffect.GetBool("Hide Creature Model") && my_combatant != null && my_combatant.mob_type == Combatant.TYPE_T.creature)
		{
			GetComponent<SharedCreature>().myCreatureModel.gameObject.SetActive(false);
		}
		RecalcAdditiveEffects(effect_name, perk_Data, perk_level, caster_level);
	}

	private void RecalcAdditiveEffects(string effect_name, PerkData perk_data, int perk_level, int caster_level)
	{
		if (base.gameObject == GameController.Instance.player && perk_data.GetFloat("Player Vision", effect_name, perk_level, caster_level, "%") != -1f)
		{
			GameController.Instance.ReCalcVisionMod();
		}
		if (!(my_combatant == null) && my_combatant.mob_type == Combatant.TYPE_T.creature)
		{
			if (perk_data.GetFloat("Walk Speed", effect_name, perk_level, caster_level, "%") != -1f)
			{
				GetComponent<SharedCreature>().ReCalcWalkSpeedMod();
			}
			if (perk_data.GetFloat("Player Size", effect_name, perk_level, caster_level, "%") != -1f)
			{
				GetComponent<SharedCreature>().UpdateSize();
			}
			if (perk_data.GetFloat("Attack Speed", effect_name, perk_level, caster_level, "%") != -1f)
			{
				GetComponent<SharedCreature>().ReCalcAttackSpeed();
			}
		}
	}

	public void ApplyPerkEffect(bool on_duration_reapply, string effect_name, PerkData perk_data, int perk_level, string caster_id, int caster_level, bool send)
	{
		if (!on_duration_reapply)
		{
			if (perk_data.GetBool("Dont Double Apply", effect_name, perk_level))
			{
				foreach (DurationEffect duration_effect in duration_effects)
				{
					if (duration_effect.perk_data.original_key == perk_data.original_key && duration_effect.effect_name == effect_name)
					{
						return;
					}
				}
			}
			string @string = perk_data.GetString("Particle (Instant)", effect_name, perk_level);
			if (@string != "")
			{
				ApplyInstantParticle(@string, perk_data, effect_name, perk_level, caster_level);
			}
			string string2 = perk_data.GetString("Animation prefab", effect_name, perk_level);
			if (string2 != "")
			{
				ApplyPerkAnimation(string2, perk_data, perk_level);
			}
			string string3 = perk_data.GetString("Remove Effect", effect_name, perk_level);
			if (string3 != "")
			{
				string text = string3.Replace("[", "").Replace("]", "");
				foreach (DurationEffect duration_effect2 in duration_effects)
				{
					if (duration_effect2.perk_data.original_key == perk_data.original_key && duration_effect2.effect_name == text)
					{
						OnDurationEffectRemoved(duration_effect2);
						duration_effects.Remove(duration_effect2);
						break;
					}
				}
			}
		}
		string string4 = perk_data.GetString("Type", effect_name, perk_level);
		if (string4 == "Instant Effect" || string4 == "Duration Effect")
		{
			if (PerkControl.Instance.ShouldApplyEffectRightNow(caster_id))
			{
				int num = (int)perk_data.GetFloat("Steal HP", effect_name, perk_level, caster_level);
				if (num != -1)
				{
					ProcessStealHPAndSend(num, perk_data, effect_name, perk_level, caster_id);
				}
				int num2 = (int)perk_data.GetFloat("Damage", effect_name, perk_level, caster_level);
				if (num2 != -1)
				{
					ProcessPerkDamageAndSend(num2, perk_data, effect_name, perk_level, caster_id);
				}
				int num3 = (int)perk_data.GetFloat("Heal", effect_name, perk_level, caster_level);
				if (num3 != -1)
				{
					ProcessHealAndSend(num3);
				}
			}
			int num4 = (int)perk_data.GetFloat("Blastback", effect_name, perk_level, caster_level, "%");
			if (num4 != -1)
			{
				ProcessBlastBack(num4, caster_id, caster_level);
			}
			if (perk_data.GetBool("Lose Me As Target In Online Mode", effect_name, perk_level) && GameController.Instance.player != null)
			{
				GameController.Instance.player.GetComponent<CreatureBrain>().RemoveFromTargetList(base.gameObject);
			}
			if (perk_data.GetBool("Lose All Targets In Online Mode", effect_name, perk_level) && base.gameObject == GameController.Instance.player)
			{
				GetComponent<CreatureBrain>().ClearAllTargets();
			}
			if (perk_data.GetBool("Remove All Effects", effect_name, perk_level))
			{
				foreach (DurationEffect duration_effect3 in duration_effects)
				{
					duration_effect3.time_remaining = 0f;
				}
			}
		}
		else if (string4 == "Area Effect")
		{
			if (PerkControl.Instance.ShouldApplyEffectRightNow(caster_id))
			{
				string text2 = perk_data.GetString("Area Effect to apply", effect_name, perk_level).Replace("[", "").Replace("]", "");
				float @float = perk_data.GetFloat("Area Effect Radius", effect_name, perk_level, caster_level, "m");
				float num5 = 1f;
				if (@float != -1f)
				{
					num5 = @float;
				}
				bool @bool = perk_data.GetBool("Dont Apply To Caster", effect_name, perk_level);
				bool bool2 = perk_data.GetBool("Dont Apply To Combatant_Stationary", effect_name, perk_level);
				bool bool3 = perk_data.GetBool("Dont Apply To Combatant_Minerals", effect_name, perk_level);
				if (text2 != "")
				{
					List<GameObject> list = new List<GameObject>();
					foreach (KeyValuePair<string, GameObject> active_combatant in MobControl.Instance.active_combatants)
					{
						GameObject value = active_combatant.Value;
						if (!(value == base.gameObject))
						{
							Vector3 normalized = (value.transform.position - base.transform.position).normalized;
							float num6 = Mathf.Min(Vector3.Distance(base.transform.position, value.transform.position), num5);
							if (value.GetComponent<Combatant>().WithinHitbox(base.transform.position + normalized * num6, clamp: false) && (!@bool || !(value.GetComponent<Combatant>().combat_name == caster_id)) && (!bool2 || value.GetComponent<Combatant>().mob_type != Combatant.TYPE_T.stationary) && (!bool3 || value.GetComponent<Combatant>().mob_type != Combatant.TYPE_T.mineral))
							{
								list.Add(value);
							}
						}
					}
					foreach (GameObject item in list)
					{
						item.GetComponent<PerkReceiver>().ApplyPerkEffect(false, text2, perk_data, perk_level, caster_id, caster_level, true);
					}
				}
				if (perk_data.GetBool("Regenerate Nearby Plants", effect_name, perk_level))
				{
					PerkControl.Instance.RegenerateSurroundingPlants(base.transform.position, num5, perk_level);
				}
			}
		}
		else if (string4 == "Drop Effect" && PerkControl.Instance.ShouldApplyEffectRightNow(caster_id))
		{
			Vector3 position = base.transform.position;
			Vector3 vector = base.transform.rotation * (perk_data.GetVector3("Drop Unit Circle Offset", effect_name, perk_level) * base.transform.localScale.x);
			Vector3 vector2 = base.transform.rotation * perk_data.GetVector3("Drop Additional Offset", effect_name, perk_level);
			PerkControl.Instance.CreateDrop(position + vector + vector2, effect_name, perk_data, perk_level, caster_id, caster_level, true);
		}
		if (!on_duration_reapply)
		{
			int num7 = (int)perk_data.GetFloat("Duration", effect_name, perk_level, caster_level, " seconds");
			if (num7 != -1)
			{
				InitializeDurationTimer(num7, effect_name, perk_data, perk_level, caster_id, caster_level, false);
			}
		}
		if (send && my_combatant != null)
		{
			GameServerSender.Instance.SendApplyPerk(effect_name, my_combatant.combat_name, caster_id, caster_level, perk_data, perk_level, on_duration_reapply, "");
		}
	}

	public void OnDurationEffectRemoved(DurationEffect duration_effect)
	{
		if (duration_effect.is_removed)
		{
			return;
		}
		if (duration_effect.duration_particle != null)
		{
			UnityEngine.Object.Destroy(duration_effect.duration_particle);
		}
		if (duration_effect.time_remaining > 0f)
		{
			duration_effect.time_remaining = 0f;
		}
		if (duration_effect.GetInt("Num Creatures To Summon") != -1)
		{
			foreach (GameObject spawned_minion in duration_effect.spawned_minions)
			{
				if (!(spawned_minion == null))
				{
					spawned_minion.GetComponent<SharedCreature>().Deload(true);
					UnityEngine.Object.Destroy(spawned_minion);
				}
			}
			duration_effect.spawned_minions.Clear();
		}
		if (duration_effect.GetString("Change skin mat") != "" && !CheckPerkStringApplied("Change skin mat", duration_effect) && !(my_combatant == null))
		{
			if (my_combatant.mob_type == Combatant.TYPE_T.creature)
			{
				GetComponent<SharedCreature>().OnEquipmentChanged();
			}
			else
			{
				string chunkString = ChunkControl.Instance.GetChunkString(my_combatant.origin_zone, my_combatant.origin_chunkX, my_combatant.origin_chunkZ);
				ChunkControl.Instance.RedrawAtSquare(chunkString, my_combatant.origin_innerX, my_combatant.origin_innerZ);
			}
		}
		if (duration_effect.GetString("Custom Brain") != "" && !CheckPerkStringApplied("Custom Brain", duration_effect) && my_combatant.mob_type == Combatant.TYPE_T.creature && GetComponent<SharedCreature>().is_local_mob)
		{
			GetComponent<CreatureBrain>().RemovePerkBrain();
		}
		if (duration_effect.GetInt("Change Icon To") != -1)
		{
			SharedCreature component = GetComponent<SharedCreature>();
			if (component != null)
			{
				component.RedrawIcon(component.icon_id);
			}
		}
		if (duration_effect.GetBool("Hide Creature Model") && !CheckPerkBoolApplied("Hide Creature Model", duration_effect) && !(my_combatant == null) && my_combatant.mob_type == Combatant.TYPE_T.creature)
		{
			GetComponent<SharedCreature>().myCreatureModel.gameObject.SetActive(true);
		}
		RecalcAdditiveEffects(duration_effect.effect_name, duration_effect.perk_data, duration_effect.perk_level, duration_effect.original_caster_level);
		duration_effect.is_removed = true;
	}

	private void OnEnable()
	{
		StartCoroutine(ProcessDurationEffects());
	}

	private IEnumerator ProcessDurationEffects()
	{
		while (true)
		{
			yield return new WaitForSeconds(0.25f);
			if (GameController.Instance.is_paused())
			{
				continue;
			}
			List<DurationEffect> list = new List<DurationEffect>();
			for (int i = 0; i < duration_effects.Count; i++)
			{
				duration_effects[i].time_remaining -= 0.25f;
				duration_effects[i].next_apply -= 0.25f;
				DurationEffect durationEffect = duration_effects[i];
				if (duration_effects[i].next_apply <= 0f)
				{
					if (PerkControl.Instance.ShouldApplyEffectRightNow(durationEffect.original_caster_id))
					{
						ApplyPerkEffect(true, durationEffect.effect_name, durationEffect.perk_data, durationEffect.perk_level, durationEffect.original_caster_id, durationEffect.original_caster_level, true);
					}
					float @float = durationEffect.GetFloat("Apply every", " seconds");
					float next_apply = 1f;
					if (@float != -1f)
					{
						next_apply = @float;
					}
					duration_effects[i].next_apply = next_apply;
				}
				if (!(duration_effects[i].time_remaining > 0f))
				{
					OnDurationEffectRemoved(duration_effects[i]);
					list.Add(duration_effects[i]);
				}
			}
			foreach (DurationEffect item in list)
			{
				duration_effects.Remove(item);
			}
			if (delete_once_all_effects_gone && duration_effects.Count == 0)
			{
				yield return new WaitForSeconds(3f);
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	public bool HasPerkRemaining(string perk_key)
	{
		foreach (DurationEffect duration_effect in duration_effects)
		{
			if (duration_effect.perk_data.original_key == perk_key && duration_effect.time_remaining > 0f)
			{
				return true;
			}
		}
		return false;
	}

	private bool AlreadyHasMinions(DurationEffect new_timer)
	{
		foreach (DurationEffect duration_effect in duration_effects)
		{
			if (duration_effect != new_timer && duration_effect.spawned_minions.Count != 0)
			{
				return true;
			}
		}
		return false;
	}

	public void RemovePerk(string perk_key)
	{
		bool flag = true;
		while (flag)
		{
			flag = false;
			for (int i = 0; i < duration_effects.Count; i++)
			{
				if (duration_effects[i].perk_data.original_key == perk_key)
				{
					OnDurationEffectRemoved(duration_effects[i]);
					duration_effects.RemoveAt(i);
					flag = true;
					break;
				}
			}
		}
	}

	public void ClearAllDurationEffects()
	{
		for (int i = 0; i < duration_effects.Count; i++)
		{
			OnDurationEffectRemoved(duration_effects[i]);
		}
		duration_effects.Clear();
	}

	public bool CheckPerkBoolApplied(string key, DurationEffect skip = null)
	{
		foreach (DurationEffect duration_effect in duration_effects)
		{
			if ((skip == null || duration_effect != skip) && duration_effect.GetBool(key))
			{
				return true;
			}
		}
		return false;
	}

	private void ApplyInstantParticle(string particle_instant, PerkData perk_data, string effect_name, int perk_level, int caster_level)
	{
		GameObject check_not_null = base.gameObject;
		ResourceControl.Instance.AsyncInstantiatePerkObj(particle_instant, delegate(GameObject particle_instance)
		{
			if (check_not_null == null)
			{
				UnityEngine.Object.Destroy(particle_instance);
			}
			else
			{
				particle_instance.transform.position = new Vector3(base.transform.position.x, 0.6f, base.transform.position.z);
				if (!perk_data.GetBool("Scale Particle With Creature Size", effect_name, perk_level))
				{
					float @float = perk_data.GetFloat("Particle Radius", effect_name, perk_level, caster_level, "m");
					if (@float != -1f)
					{
						particle_instance.transform.localScale = @float * Vector3.one;
					}
					if (particle_instance.GetComponent<DurationParticleSnapper>() != null)
					{
						particle_instance.GetComponent<DurationParticleSnapper>().snap_to = base.gameObject;
					}
				}
				else
				{
					particle_instance.transform.SetParent(base.transform);
					particle_instance.transform.localScale = Vector3.one;
					if (particle_instance.GetComponent<DurationParticleSnapper>() != null)
					{
						UnityEngine.Object.Destroy(particle_instance.GetComponent<DurationParticleSnapper>());
					}
				}
				AudioSource component = particle_instance.GetComponent<AudioSource>();
				if (component != null)
				{
					component.volume = AudioControl.Instance.general_sfx_volume;
					component.Play();
				}
			}
		});
	}

	private void ApplyPerkAnimation(string animation_prefab, PerkData perk_data, int perk_level)
	{
		GameObject check_not_null = base.gameObject;
		ResourceControl.Instance.AsyncInstantiatePerkObj(animation_prefab, delegate(GameObject particle_instance)
		{
			if (check_not_null == null || curr_perk_animation != null)
			{
				UnityEngine.Object.Destroy(particle_instance);
			}
			else
			{
				PerkAnimation component = particle_instance.GetComponent<PerkAnimation>();
				component.currently_animating = base.gameObject;
				component.perk_data = perk_data;
				component.perk_level = perk_level;
				if (!(my_combatant == null) && my_combatant.mob_type == Combatant.TYPE_T.creature)
				{
					GameObject gameObject = GetComponent<SharedCreature>().myCreatureModel.gameObject;
					component.original_localPosition = gameObject.transform.localPosition;
					component.original_localRotation = gameObject.transform.localRotation;
					component.original_localScale = gameObject.transform.localScale;
					particle_instance.transform.position = gameObject.transform.position;
					particle_instance.transform.rotation = gameObject.transform.rotation;
					particle_instance.transform.localScale = base.transform.localScale;
					particle_instance.transform.SetParent(base.transform.Find("model goes here"));
					gameObject.transform.SetParent(particle_instance.transform.Find("player_go_here"));
					curr_perk_animation = particle_instance;
				}
				AudioSource component2 = particle_instance.GetComponent<AudioSource>();
				if (component2 != null)
				{
					component2.volume = AudioControl.Instance.general_sfx_volume;
					component2.Play();
				}
			}
		});
	}

	private void ApplyDurationParticle(string particle_duration, DurationEffect new_timer, PerkData perk_Data, string effect_name, bool pre_applied)
	{
		GameObject check_not_null = base.gameObject;
		Action<GameObject> on_particle_loaded = delegate(GameObject particle_instance)
		{
			if (check_not_null == null || !duration_effects.Contains(new_timer))
			{
				UnityEngine.Object.Destroy(particle_instance);
			}
			else
			{
				string @string = new_timer.GetString("Particle Overlap Logic");
				string text = ((@string == "") ? "Stack" : @string);
				if (!(text == "Stack") && text == "Destroy Other Of The Same")
				{
					foreach (DurationEffect duration_effect in duration_effects)
					{
						if (duration_effect.perk_data.original_key == perk_Data.original_key && duration_effect.effect_name == effect_name && duration_effect.duration_particle != null)
						{
							UnityEngine.Object.Destroy(duration_effect.duration_particle);
						}
					}
				}
				new_timer.duration_particle = particle_instance;
				particle_instance.transform.position = new Vector3(base.transform.position.x, 0.6f, base.transform.position.z);
				particle_instance.transform.rotation = base.transform.rotation;
				if (!new_timer.GetBool("Scale Particle With Creature Size"))
				{
					float @float = new_timer.GetFloat("Particle Radius", "m");
					float num = 1f;
					if (@float != -1f)
					{
						num = @float;
					}
					particle_instance.transform.localScale = num * Vector3.one;
					if (particle_instance.GetComponent<DurationParticleSnapper>() != null)
					{
						particle_instance.GetComponent<DurationParticleSnapper>().snap_to = base.gameObject;
					}
				}
				else
				{
					particle_instance.transform.SetParent(base.transform);
					particle_instance.transform.localScale = Vector3.one;
					if (particle_instance.GetComponent<DurationParticleSnapper>() != null)
					{
						UnityEngine.Object.Destroy(particle_instance.GetComponent<DurationParticleSnapper>());
					}
				}
				if (perk_Data.original_key == "perk_ignite" && !(my_combatant == null) && my_combatant.IgnoreFireDamage())
				{
					particle_instance.GetComponent<AudioSource>().Stop();
				}
				if (!pre_applied)
				{
					AudioSource[] components = particle_instance.GetComponents<AudioSource>();
					foreach (AudioSource obj in components)
					{
						obj.volume = AudioControl.Instance.general_sfx_volume;
						obj.Play();
					}
				}
			}
		};
		if (!particle_duration.Contains("[item]"))
		{
			ResourceControl.Instance.AsyncInstantiatePerkObj(particle_duration, on_particle_loaded);
			return;
		}
		InventoryItem item = new InventoryItem(particle_duration.Replace("[item]", ""));
		ResourceControl.Instance.AsyncInstantiateWorldObjectPrefab(item, null, delegate(GameObject new_instance)
		{
			if (new_instance.GetComponent<PaintableObject>() != null)
			{
				string paintFromItemOrUseDefault = inventory_ctr.Instance.GetPaintFromItemOrUseDefault(item);
				string stampFromItem = inventory_ctr.Instance.GetStampFromItem(item);
				string layoutItemFromItem = inventory_ctr.Instance.GetLayoutItemFromItem(item.item_name);
				new_instance.GetComponent<PaintableObject>().Colorize(paintFromItemOrUseDefault, stampFromItem, layoutItemFromItem, item.item_name);
			}
			ConstructionControl.Instance.DeleteUnnecessaryComponents(item, new_instance);
			ConstructionControl.Instance.AdjustBuildableInstance(new_instance, item, ConstructionControl.usage_context_t.on_mouseObj_or_storeModel);
			new_instance.AddComponent<DurationParticleSnapper>();
			on_particle_loaded(new_instance);
		});
	}

	private void ApplyScreenOverlay(string particle_screen_overlay, DurationEffect new_timer)
	{
		GameObject check_not_null = base.gameObject;
		ResourceControl.Instance.AsyncInstantiatePerkObj(particle_screen_overlay, delegate(GameObject particle_instance)
		{
			if (check_not_null == null)
			{
				UnityEngine.Object.Destroy(particle_instance);
			}
			else
			{
				if (!duration_effects.Contains(new_timer))
				{
					UnityEngine.Object.Destroy(particle_instance);
				}
				else
				{
					new_timer.duration_particle = particle_instance;
					particle_instance.transform.SetParent(WindowControl.Instance.transform);
					particle_instance.transform.SetAsFirstSibling();
					particle_instance.transform.localPosition = Vector3.zero;
					particle_instance.transform.localScale = Vector3.one;
					((RectTransform)particle_instance.transform).offsetMin = new Vector2(0f, 0f);
					((RectTransform)particle_instance.transform).offsetMax = new Vector2(0f, 0f);
				}
				AudioSource component = particle_instance.GetComponent<AudioSource>();
				if (component != null)
				{
					component.volume = AudioControl.Instance.general_sfx_volume;
					component.Play();
				}
			}
		});
	}

	public void PerkAnimationComplete()
	{
		PerkAnimation component = curr_perk_animation.GetComponent<PerkAnimation>();
		if (!(my_combatant == null) && my_combatant.mob_type == Combatant.TYPE_T.creature)
		{
			GameObject gameObject = GetComponent<SharedCreature>().myCreatureModel.gameObject;
			gameObject.transform.SetParent(base.transform.Find("model goes here"));
			gameObject.transform.localPosition = component.original_localPosition;
			gameObject.transform.localRotation = component.original_localRotation;
			gameObject.transform.localScale = component.original_localScale;
		}
		curr_perk_animation = null;
	}

	private void SummonMinions(DurationEffect new_timer, int n_summon, PerkData perk_Data, string effect_name, int perk_level, string caster_id, int caster_level)
	{
		if (my_combatant == null || !(caster_id == "LOCAL"))
		{
			return;
		}
		if (AlreadyHasMinions(new_timer))
		{
			GameplayGUIControl.Instance.ShowNotif("<color=#dddddd><i>You cannot summon more creatures</i></color>", new OnNotifClick(OnNotifClick.type.none));
			return;
		}
		float @float = new_timer.GetFloat("Summoned Creature Size", "%");
		float critterSize_ = 1f;
		if (@float != -1f)
		{
			critterSize_ = @float / 100f;
		}
		float float2 = new_timer.GetFloat("Summoned Creature Follow Distance");
		float follow_dist = 2.5f;
		if (float2 != -1f)
		{
			follow_dist = float2;
		}
		string text = new_timer.GetString("Summoned Creature Name");
		if (text.Contains("[PLAYER_NAME]"))
		{
			text = text.Replace("[PLAYER_NAME]", PlayerData.Instance.GetSlotString("creatureName", PlayerData.filename_t.general));
		}
		int num = new_timer.GetInt("Summoned Creature Level");
		if (num == -1)
		{
			num = 1;
		}
		string @string = new_timer.GetString("Summoned Creature Hat");
		string string2 = new_timer.GetString("Summoned Creature Body");
		string string3 = new_timer.GetString("Summoned Creature Wep");
		string string4 = new_timer.GetString("Summoned Creature Skin Mat");
		float summonedCreatureWalkSpeed = new_timer.GetSummonedCreatureWalkSpeed(effect_name);
		bool @bool = new_timer.GetBool("Summoned Creature Extra Aggression");
		string text2 = new_timer.GetString("Summoned Creature Apply Effect");
		if (text2 != "")
		{
			text2 = text2.Replace("[", "").Replace("]", "");
		}
		List<float> list = new List<float>();
		switch (n_summon)
		{
		case 3:
			list.Add(0f);
			list.Add(180f);
			list.Add(270f);
			break;
		case 2:
			list.Add(0f);
			list.Add(180f);
			break;
		case 1:
			list.Add(270f);
			break;
		}
		List<string> creatureList = new_timer.GetCreatureList("Summoned Creature Combo");
		for (int i = 0; i < n_summon; i++)
		{
			string text3 = ShopControl.RandomString();
			CreatureStruct creature = new CreatureStruct(text3, creatureList, num, critterSize_, SharedCreature.brain_type_t.wolf_pack, summonedCreatureWalkSpeed, CreatureStruct.DEFAULT_HP_REGEN, CreatureStruct.DEFAULT_HP_MAX, CreatureStruct.DEFAULT_HP_CURR, CreatureStruct.DEFAULT_AI_LOCKON, CreatureStruct.DEFAULT_WANDER_DIST, new InventoryItem(@string), new InventoryItem(string2), new InventoryItem(string3), text, 0, string4, new InventoryItem(""), 0, 2, "overworld", 0, 0, 0, 0, 0, 0);
			int index = UnityEngine.Random.Range(0, list.Count);
			float num2 = list[index];
			list.RemoveAt(index);
			Vector3 vector = base.transform.rotation * new Vector3(Mathf.Cos(num2 * ((float)System.Math.PI / 180f)), 0f, Mathf.Sin(num2 * ((float)System.Math.PI / 180f)));
			GameObject gameObject = MobControl.Instance.SpawnLocalMob(creature, base.transform.position + vector * 10f);
			gameObject.GetComponent<CreatureBrainWolfPack>().follow_dist = follow_dist;
			gameObject.GetComponent<CreatureBrainWolfPack>().follow_angle = num2;
			new_timer.spawned_minions.Add(gameObject);
			gameObject.GetComponent<CreatureBrainWolfPack>().extra_agression = @bool;
			GameServerSender.Instance.SendCreatedLocalMob(text3);
			if (text2 != "")
			{
				gameObject.GetComponent<PerkReceiver>().ApplyPerkEffect(false, text2, perk_Data, perk_level, caster_id, caster_level, true);
			}
		}
	}

	private void ProcessStealHPAndSend(int steal_hp, PerkData perk_data, string effect_name, int perk_level, string caster_id)
	{
		Combatant.hit_col col_;
		if (my_combatant != null && my_combatant.mob_type == Combatant.TYPE_T.mineral)
		{
			steal_hp = 0;
			col_ = Combatant.hit_col.color_blue;
		}
		else
		{
			col_ = CombatControl.StringToHitCol(perk_data.GetString("Splat color", effect_name, perk_level));
		}
		GameObject gameObject = (MobControl.Instance.active_combatants.ContainsKey(caster_id) ? MobControl.Instance.active_combatants[caster_id] : null);
		if (my_combatant != null)
		{
			my_combatant.WasHit(steal_hp, gameObject, false, false, col_, true);
		}
		gameObject.GetComponent<Combatant>().IncreaseHp(steal_hp);
		GameServerSender.Instance.SendIncreaseHp(gameObject.GetComponent<Combatant>().combat_name, steal_hp, "");
	}

	private void ProcessPerkDamageAndSend(int damage, PerkData perk_data, string effect_name, int perk_level, string caster_id)
	{
		bool flag = my_combatant != null && my_combatant.mob_type == Combatant.TYPE_T.mineral;
		if (perk_data.original_key == "perk_ignite" && my_combatant != null && my_combatant.IgnoreFireDamage())
		{
			return;
		}
		Combatant.hit_col col_ = (flag ? Combatant.hit_col.color_blue : CombatControl.StringToHitCol(perk_data.GetString("Splat color", effect_name, perk_level)));
		int damage2 = ((!flag) ? damage : 0);
		GameObject attacker = (MobControl.Instance.active_combatants.ContainsKey(caster_id) ? MobControl.Instance.active_combatants[caster_id] : null);
		if (my_combatant != null)
		{
			my_combatant.WasHit(damage2, attacker, false, false, col_, true);
		}
	}

	private void ProcessHealAndSend(int heal)
	{
		if (my_combatant != null)
		{
			my_combatant.IncreaseHp(heal);
			GameServerSender.Instance.SendIncreaseHp(my_combatant.combat_name, heal, "");
		}
	}

	private void ProcessBlastBack(float blastback, string caster_id, int caster_level)
	{
		if (!(GetComponent<Rigidbody>() != null))
		{
			return;
		}
		Vector3 normalized = ((MobControl.Instance.active_combatants.ContainsKey(caster_id) ? MobControl.Instance.active_combatants[caster_id] : null).transform.position - base.transform.position).normalized;
		int num = Mathf.Clamp(caster_level - GetComponent<SharedCreature>().level, -10, 10);
		float num2 = Mathf.Clamp01(blastback / 100f);
		Rigidbody component = GetComponent<Rigidbody>();
		float num3 = (num2 * 0.75f + 1f) * ((float)num * 0.25f + 9f) * 0.6f;
		component.velocity -= normalized * num3;
		component.velocity += Vector3.up * (num2 * 5f + 3f);
	}

	private void ApplySkinMaterial(string skin_mat)
	{
		Material skinMaterialByName = MobControl.Instance.GetSkinMaterialByName(skin_mat);
		if (!(my_combatant == null))
		{
			if (my_combatant.mob_type == Combatant.TYPE_T.creature)
			{
				GetComponent<SharedCreature>().OnEquipmentChanged();
				return;
			}
			Transform t = ((base.gameObject.name == "Combatant") ? base.transform.parent : base.transform);
			my_combatant.RecursiveApplyMaterial(t, skinMaterialByName);
		}
	}

	private void ApplyCustomBrain(DurationEffect new_timer, string brain_str)
	{
		if (CheckPerkStringApplied("Custom Brain", new_timer) || my_combatant.mob_type != Combatant.TYPE_T.creature)
		{
			return;
		}
		SharedCreature component = GetComponent<SharedCreature>();
		if (component.is_local_mob && component.brain_type != SharedCreature.brain_type_t.local_player)
		{
			CreatureBrain component2 = GetComponent<CreatureBrain>();
			if (brain_str == "Chaotic")
			{
				component2.SetPerkBrain(base.gameObject.AddComponent<CreatureBrainChaoticMovement>());
				component2.custom_brain.Init();
				component2.RestartThinking();
			}
			else if (brain_str == "Charmed")
			{
				component2.SetPerkBrain(base.gameObject.AddComponent<CreatureBrainCharmedMovement>());
				component2.custom_brain.Init();
				component2.RestartThinking();
			}
		}
	}

	public bool CheckPerkStringApplied(string key, DurationEffect skip = null)
	{
		foreach (DurationEffect duration_effect in duration_effects)
		{
			if ((skip == null || duration_effect != skip) && duration_effect.GetString(key) != "")
			{
				return true;
			}
		}
		return false;
	}
}
