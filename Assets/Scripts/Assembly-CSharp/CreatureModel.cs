using System.Collections.Generic;
using UnityEngine;

public class CreatureModel : MonoBehaviour
{
	public string key;

	public int secs_until_deload;

	public int n_LITE_instances;

	public Limb[] limbs_;

	public Color base_color;

	public string start_perk;

	public float seed;

	public GameObject eyeObject_;

	public GameObject mouthObject_;

	public List<string> creatures_that_made_me = new List<string>();

	public List<string> creatures_that_made_me_TRANSLATED = new List<string>();

	public string myName = "Shark";

	public string grammatical_gender = "?";

	public string adjective_M = "Untitled";

	public string adjective_F = "Untitled";

	public string adjective_N = "Untitled";

	public string noun = "Untitled";

	public List<string> all_possible_drops = new List<string>();

	public string drop1 = "";

	public string drop2 = "";

	public float height;

	public GameObject hand_obj;

	public GameObject head_obj;

	public GameObject torso_obj;

	private int animation_timer;

	public int animation_choppiness = 1;

	public Texture2D color_map;

	public List<ObjActionPair> lite_models_that_need_height = new List<ObjActionPair>();

	public Dictionary<string, Mesh> complete_creature_meshes = new Dictionary<string, Mesh>();

	public void OnDestroy()
	{
		Object.Destroy(color_map);
	}

	public void GenerateModelAndTexture()
	{
		List<Color> list = new List<Color>();
		for (int i = 0; i < limbs_.Length; i++)
		{
			if (limbs_[i].textureName == "None" && !list.Contains(limbs_[i].cube_color))
			{
				list.Add(limbs_[i].cube_color);
			}
		}
		int count = list.Count;
		int num = 1;
		int num2 = 1;
		if (count >= 2)
		{
			int num3 = 1;
			do
			{
				num = (int)Mathf.Pow(2f, num3);
				num2 = num * num;
				num3++;
			}
			while (num2 < count);
		}
		Texture2D texture2D = new Texture2D(num, num);
		Color[] array = new Color[num2];
		for (int j = 0; j < count; j++)
		{
			array[j] = list[j];
		}
		texture2D.SetPixels(array);
		texture2D.filterMode = FilterMode.Point;
		texture2D.Apply();
		color_map = texture2D;
		Dictionary<string, IncomepleteMesh> dictionary = new Dictionary<string, IncomepleteMesh>();
		for (int k = 0; k < limbs_.Length; k++)
		{
			Limb limb = limbs_[k];
			if (limb.compName == "eyes")
			{
				AddVertices(limb, "eyes_mesh", -1, -1, CreatureMorpher.Instance.decorative_mesh, false, dictionary);
				if (limb.has_dummy)
				{
					AddVertices(limb, "eyes_mesh", -1, -1, CreatureMorpher.Instance.decorative_mesh_dummy, true, dictionary);
				}
				continue;
			}
			if (limb.compName == "mouth")
			{
				AddVertices(limb, "mouth_mesh", -1, -1, CreatureMorpher.Instance.decorative_mesh, false, dictionary);
				continue;
			}
			string textureName = limb.textureName;
			string text = (limb.hide_on_wear_hat ? "(DO_HIDE)" : "");
			if (textureName != "None")
			{
				AddVertices(limb, "limbs_mesh(" + textureName + ")" + text, -1, -1, CreatureMorpher.Instance.limb_mesh, false, dictionary);
				if (limb.has_dummy)
				{
					AddVertices(limb, "limbs_mesh(" + textureName + ")" + text, -1, -1, CreatureMorpher.Instance.limb_mesh, true, dictionary);
				}
			}
			else
			{
				int color_index = list.IndexOf(limb.cube_color);
				AddVertices(limb, "limbs_mesh(None)" + text, color_index, num, CreatureMorpher.Instance.limb_mesh, false, dictionary);
				if (limb.has_dummy)
				{
					AddVertices(limb, "limbs_mesh(None)" + text, color_index, num, CreatureMorpher.Instance.limb_mesh, true, dictionary);
				}
			}
		}
		foreach (KeyValuePair<string, IncomepleteMesh> item in dictionary)
		{
			string key = item.Key;
			IncomepleteMesh value = item.Value;
			Mesh mesh = new Mesh();
			mesh.name = key;
			mesh.vertices = value.mesh_vertices.ToArray();
			mesh.triangles = value.mesh_tris.ToArray();
			mesh.uv = value.mesh_uvs.ToArray();
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			complete_creature_meshes.Add(key, mesh);
		}
	}

	private void AddVertices(Limb limb, string sub_mesh_name, int color_index, int color_map_w, Mesh target_mesh, bool dummy, Dictionary<string, IncomepleteMesh> incomplete_meshes)
	{
		if (!incomplete_meshes.ContainsKey(sub_mesh_name))
		{
			incomplete_meshes.Add(sub_mesh_name, new IncomepleteMesh());
		}
		IncomepleteMesh incomepleteMesh = incomplete_meshes[sub_mesh_name];
		Matrix4x4 matrix4x = Matrix4x4.TRS(limb.transform.position, limb.transform.rotation, Vector3.one);
		Matrix4x4 matrix4x2 = Matrix4x4.TRS(limb.visual.transform.localPosition, Quaternion.identity, limb.visual.transform.localScale);
		Matrix4x4 matrix4x3 = matrix4x * matrix4x2;
		for (int i = 0; i < target_mesh.vertices.Length; i++)
		{
			incomepleteMesh.mesh_vertices.Add(matrix4x3.MultiplyPoint3x4(target_mesh.vertices[i]));
		}
		for (int j = 0; j < target_mesh.triangles.Length; j++)
		{
			incomepleteMesh.mesh_tris.Add(target_mesh.triangles[j] + incomepleteMesh.curr_animation_id * target_mesh.vertices.Length);
		}
		int curr_animation_id = incomepleteMesh.curr_animation_id;
		if (dummy)
		{
			limb.dummy_animation_id = curr_animation_id;
		}
		else
		{
			limb.animation_id = curr_animation_id;
		}
		incomepleteMesh.curr_animation_id = curr_animation_id + 1;
		if (color_index != -1)
		{
			float num = 1f / (float)color_map_w;
			for (int k = 0; k < 6; k++)
			{
				int num2 = 0;
				int num3 = 0;
				for (int l = 0; l < color_index; l++)
				{
					if (num2 + 1 == color_map_w)
					{
						num3++;
						num2 = 0;
					}
					else
					{
						num2++;
					}
				}
				float num4 = num * (float)num2;
				float num5 = num * (float)num3;
				float y = num + num5;
				float x = num + num4;
				incomepleteMesh.mesh_uvs.Add(new Vector2(num4, y));
				incomepleteMesh.mesh_uvs.Add(new Vector2(x, y));
				incomepleteMesh.mesh_uvs.Add(new Vector2(x, num5));
				incomepleteMesh.mesh_uvs.Add(new Vector2(num4, num5));
			}
		}
		else
		{
			for (int m = 0; m < target_mesh.uv.Length; m++)
			{
				incomepleteMesh.mesh_uvs.Add(target_mesh.uv[m]);
			}
		}
	}

	private void FixedUpdate()
	{
		if (animation_timer <= 0)
		{
			for (int i = 0; i < limbs_.Length; i++)
			{
				limbs_[i].EvalAnimationCurves(animation_choppiness);
			}
			animation_timer += animation_choppiness;
		}
		animation_timer--;
	}
}
