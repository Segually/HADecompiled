using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiteModel : MonoBehaviour
{
	private enum hat_style_t
	{
		unknown = 0,
		encompass_head = 1,
		top_of_head = 2
	}

	public CreatureModel original;

	public LimbLite[] limbs_;

	private hat_style_t hat_style;

	private float hat_offset;

	public GameObject hat;

	public GameObject armor_body;

	public GameObject holding_obj_;

	public GameObject holding_obj2;

	private int animation_timer;

	public int animation_choppiness = 1;

	private Dictionary<string, CreatureSubModel> sub_models = new Dictionary<string, CreatureSubModel>();

	public int head_limb_index;

	public int hand_limb_index;

	public int mouth_limb_index;

	public int eye_limb_index;

	public float started_anm_time;

	public bool animation_complete;

	public float spaghettiFactor_;

	public const float scale_corrector = 2f;

	private IEnumerator blink;

	public void OnDestroy()
	{
		original.n_LITE_instances--;
	}

	public void ApplyWeapon(InventoryItem weapon_item, Material mat, Action on_weapon_applied = null)
	{
	}

	private void OnWeaponReady(GameObject weapon_instance, Material mat, InventoryItem weapon_item)
	{
	}

	public void ApplyHat(InventoryItem hat_item, Material mat, Action on_hat_applied = null)
	{
	}

	private void OnHatReady(GameObject hat_instance, Material mat, InventoryItem hat_item)
	{
	}

	public void ApplyArmor(InventoryItem armor_item, Material mat, Action on_armor_applied = null)
	{
	}

	private void ResizeArmor(GameObject armor_instance)
	{
	}

	public Vector3 GetLimbLocalPosition(int limb_index)
	{
		LimbLite limbLite = limbs_[limb_index];
		Matrix4x4 matrix4x = Matrix4x4.TRS(limbLite.transform_position, limbLite.transform_rotation, Vector3.one);
		Matrix4x4 matrix4x2 = Matrix4x4.TRS(limbLite.visual_transform_localPosition, Quaternion.identity, limbLite.visual_transform_localScale);
		return (matrix4x * matrix4x2).MultiplyPoint(Vector3.zero);
	}

	public Vector3 GetDummyLocalPosition(int limb_index)
	{
		LimbLite limbLite = limbs_[limb_index];
		if (!limbLite.original.has_dummy)
		{
			return GetLimbLocalPosition(limb_index);
		}
		Matrix4x4 matrix4x = Matrix4x4.TRS(limbLite.dummy_transform_position, limbLite.dummy_transform_rotation, Vector3.one);
		Matrix4x4 matrix4x2 = Matrix4x4.TRS(limbLite.dummy_visual_transform_localPosition, Quaternion.identity, limbLite.visual_transform_localScale);
		return (matrix4x * matrix4x2).MultiplyPoint(Vector3.zero);
	}

	public Quaternion GetLimbLocalRotation(int limb_index)
	{
		return limbs_[limb_index].transform_rotation;
	}

	public Vector3 GetLimbWorldPosition(int limb_index)
	{
		return default(Vector3);
	}

	public void TryAssignEyeTexture()
	{
		if (sub_models.ContainsKey("eyes_mesh"))
		{
			string textureName = limbs_[eye_limb_index].original.textureName;
			if (!Startup.StringNullOrWhitespace(textureName))
			{
				ResourceControl.Instance.AssignCreatureDecorativeTexture("Eyes/" + textureName, sub_models["eyes_mesh"].obj.GetComponent<MeshRenderer>());
			}
		}
	}

	public void TryAssignMouthTexture(string folder)
	{
		if (sub_models.ContainsKey("mouth_mesh"))
		{
			string textureName = limbs_[mouth_limb_index].original.textureName;
			if (!Startup.StringNullOrWhitespace(textureName))
			{
				ResourceControl.Instance.AssignCreatureDecorativeTexture(folder + "/" + textureName, sub_models["mouth_mesh"].obj.GetComponent<MeshRenderer>());
			}
		}
	}

	private CreatureSubModel TryAddSubModel(string model_name, ref Material assign_mat, ref Texture2D assign_tex, Color cube_color, string tex_name)
	{
		if (!sub_models.ContainsKey(model_name))
		{
			CreatureSubModel creatureSubModel = new CreatureSubModel();
			creatureSubModel.obj = new GameObject(model_name);
			creatureSubModel.obj.transform.SetParent(base.transform);
			creatureSubModel.obj.transform.localPosition = Vector3.zero;
			creatureSubModel.obj.transform.localRotation = Quaternion.identity;
			creatureSubModel.obj.transform.localScale = Vector3.one;
			MeshRenderer meshRenderer = creatureSubModel.obj.AddComponent<MeshRenderer>();
			MeshFilter meshFilter = creatureSubModel.obj.AddComponent<MeshFilter>();
			creatureSubModel.mesh = UnityEngine.Object.Instantiate(original.complete_creature_meshes[model_name]);
			meshFilter.mesh = creatureSubModel.mesh;
			creatureSubModel.vertices = creatureSubModel.mesh.vertices;
			meshRenderer.sharedMaterial = assign_mat;
			if (assign_tex != null)
			{
				meshRenderer.material.color = new Color(1f, 1f, 1f, 1f);
				meshRenderer.material.mainTexture = assign_tex;
			}
			sub_models.Add(model_name, creatureSubModel);
			if (tex_name != "")
			{
				ResourceControl.Instance.AssignCreatureLimbTexture("Skins/" + tex_name, meshRenderer, cube_color);
			}
			return creatureSubModel;
		}
		return sub_models[model_name];
	}

	public void ApplySpecialMaterial(Material mat)
	{
	}

	public void CreateMeshClones()
	{
		for (int i = 0; i < limbs_.Length; i++)
		{
			LimbLite limbLite = limbs_[i];
			if (limbLite.original.compName == "eyes")
			{
				limbLite.my_sub_model = TryAddSubModel("eyes_mesh", ref CreatureMorpher.Instance.decorative_material, ref CreatureMorpher.Instance.null_tex, new Color(1f, 1f, 1f, 1f), "");
				continue;
			}
			if (limbLite.original.compName == "mouth")
			{
				limbLite.my_sub_model = TryAddSubModel("mouth_mesh", ref CreatureMorpher.Instance.decorative_material, ref CreatureMorpher.Instance.null_tex, new Color(1f, 1f, 1f, 1f), "");
				continue;
			}
			string textureName = limbLite.original.textureName;
			string text = (limbLite.original.hide_on_wear_hat ? "(DO_HIDE)" : "");
			if (textureName != "None")
			{
				limbLite.my_sub_model = TryAddSubModel("limbs_mesh(" + textureName + ")" + text, ref CreatureMorpher.Instance.limb_material, ref CreatureMorpher.Instance.null_tex, limbLite.original.cube_color, textureName);
			}
			else
			{
				limbLite.my_sub_model = TryAddSubModel("limbs_mesh(None)" + text, ref CreatureMorpher.Instance.limb_material, ref original.color_map, new Color(1f, 1f, 1f, 1f), "");
			}
		}
	}

	public void SetEarsActive(bool state)
	{
	}

	public void StartBlinking()
	{
		if (sub_models.ContainsKey("eyes_mesh"))
		{
			if (blink != null)
			{
				StopCoroutine(blink);
			}
			blink = CreatureBlinkCoroutine();
			StartCoroutine(blink);
		}
	}

	private void AnimateVertices(LimbLite limb)
	{
		Matrix4x4 matrix4x = Matrix4x4.TRS(limb.transform_position, limb.transform_rotation, Vector3.one);
		Matrix4x4 matrix4x2 = Matrix4x4.TRS(limb.visual_transform_localPosition, Quaternion.identity, limb.visual_transform_localScale);
		Matrix4x4 mult = matrix4x * matrix4x2;
		if (limb.original.compName == "eyes")
		{
			Animate(mult, limb.my_sub_model.vertices, CreatureMorpher.Instance.decorative_mesh_vertices, CreatureMorpher.Instance.decorative_mesh_vertices_len, limb, limb.original.animation_id);
		}
		else if (limb.original.compName == "mouth")
		{
			Animate(mult, limb.my_sub_model.vertices, CreatureMorpher.Instance.decorative_mesh_vertices, CreatureMorpher.Instance.decorative_mesh_vertices_len, limb, limb.original.animation_id);
		}
		else
		{
			Animate(mult, limb.my_sub_model.vertices, CreatureMorpher.Instance.limb_mesh_vertices, CreatureMorpher.Instance.limb_mesh_vertices_len, limb, limb.original.animation_id);
		}
	}

	private void AnimateVerticesDummy(LimbLite limb)
	{
		Matrix4x4 matrix4x = Matrix4x4.TRS(limb.dummy_transform_position, limb.dummy_transform_rotation, Vector3.one);
		Matrix4x4 matrix4x2 = Matrix4x4.TRS(limb.dummy_visual_transform_localPosition, Quaternion.identity, limb.visual_transform_localScale);
		Matrix4x4 mult = matrix4x * matrix4x2;
		if (limb.original.compName == "eyes")
		{
			Animate(mult, limb.my_sub_model.vertices, CreatureMorpher.Instance.decorative_mesh_dummy_vertices, CreatureMorpher.Instance.decorative_mesh_dummy_vertices_len, limb, limb.original.dummy_animation_id);
		}
		else
		{
			Animate(mult, limb.my_sub_model.vertices, CreatureMorpher.Instance.limb_mesh_vertices, CreatureMorpher.Instance.limb_mesh_vertices_len, limb, limb.original.dummy_animation_id);
		}
	}

	private void Animate(Matrix4x4 mult, Vector3[] vertices, Vector3[] template_vertices, int len, LimbLite limb, int animation_id)
	{
		for (int i = 0; i < len; i++)
		{
			vertices[animation_id * len + i] = mult.MultiplyPoint(template_vertices[i]);
		}
	}

	private void AnimateEquipment()
	{
		if (hat != null)
		{
			Quaternion limbLocalRotation = GetLimbLocalRotation(head_limb_index);
			if (hat_style < hat_style_t.top_of_head)
			{
				hat.transform.localPosition = GetLimbLocalPosition(head_limb_index);
			}
			else if (hat_style == hat_style_t.top_of_head)
			{
				hat.transform.localPosition = GetLimbLocalPosition(head_limb_index) + limbLocalRotation * Vector3.up * hat_offset;
			}
			hat.transform.localRotation = limbLocalRotation;
		}
		if (armor_body != null)
		{
			armor_body.transform.localPosition = GetLimbLocalPosition(0);
			armor_body.transform.localRotation = GetLimbLocalRotation(0);
		}
		if (holding_obj_ != null)
		{
			holding_obj_.transform.localPosition = GetLimbLocalPosition(hand_limb_index);
			holding_obj_.transform.rotation = base.transform.rotation;
		}
		if (holding_obj2 != null)
		{
			holding_obj2.transform.localPosition = GetDummyLocalPosition(hand_limb_index);
			holding_obj2.transform.rotation = base.transform.rotation;
		}
	}

	public void AnimateAll()
	{
		for (int i = 0; i < limbs_.Length; i++)
		{
			LimbLite limbLite = limbs_[i];
			limbLite.EvalAnimationCurves(animation_choppiness, started_anm_time, spaghettiFactor_);
			AnimateVertices(limbLite);
			if (limbLite.original.has_dummy)
			{
				AnimateVerticesDummy(limbLite);
			}
		}
		foreach (KeyValuePair<string, CreatureSubModel> sub_model in sub_models)
		{
			CreatureSubModel value = sub_model.Value;
			value.mesh.vertices = value.vertices;
			value.mesh.RecalculateBounds();
			value.mesh.RecalculateNormals();
		}
		AnimateEquipment();
		if (limbs_[0].curr_animation != null && Time.time - started_anm_time > limbs_[0].curr_animation.speed * (float)limbs_[0].curr_animation.animLength)
		{
			animation_complete = true;
		}
	}

	private void FixedUpdate()
	{
		if (animation_timer <= 0)
		{
			AnimateAll();
			animation_timer += animation_choppiness;
		}
		animation_timer--;
	}

	public void StartAnimation(int animationIndex, float forced_speed = -1f)
	{
		started_anm_time = Time.time;
		switch (animationIndex)
		{
		case 1:
			spaghettiFactor_ = 18f;
			break;
		case 2:
			spaghettiFactor_ = 18f;
			break;
		case 3:
			spaghettiFactor_ = 90f;
			break;
		default:
			spaghettiFactor_ = 18f;
			break;
		}
		for (int i = 0; i < limbs_.Length; i++)
		{
			limbs_[i].CreateAndPlayAnimation(animationIndex, started_anm_time, spaghettiFactor_, forced_speed);
		}
		animation_complete = false;
	}

	public void StopAnimation()
	{
		for (int i = 0; i < limbs_.Length; i++)
		{
			limbs_[i].StopAnimation();
		}
		spaghettiFactor_ = 90f;
	}

	private IEnumerator CreatureBlinkCoroutine()
	{
		while (true)
		{
			sub_models["eyes_mesh"].obj.SetActive(true);
			yield return new WaitForSeconds(1.5f);
			sub_models["eyes_mesh"].obj.SetActive(false);
			yield return new WaitForSeconds(0.1f);
			sub_models["eyes_mesh"].obj.SetActive(true);
			yield return new WaitForSeconds(0.5f);
			sub_models["eyes_mesh"].obj.SetActive(false);
			yield return new WaitForSeconds(0.07f);
			sub_models["eyes_mesh"].obj.SetActive(true);
			yield return new WaitForSeconds(UnityEngine.Random.Range(-0.2f, 0.2f) + 2.2f);
			sub_models["eyes_mesh"].obj.SetActive(false);
			yield return new WaitForSeconds(0.1f);
			sub_models["eyes_mesh"].obj.SetActive(true);
			yield return new WaitForSeconds(0.1f);
			sub_models["eyes_mesh"].obj.SetActive(false);
			yield return new WaitForSeconds(0.1f);
		}
	}
}
