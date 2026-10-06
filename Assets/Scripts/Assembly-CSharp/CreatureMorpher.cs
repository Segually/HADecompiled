using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreatureMorpher : MonoBehaviour, OrderedStart
{
	public static CreatureMorpher Instance;

	public Material limb_material;

	public Material decorative_material;

	public Mesh limb_mesh;

	public Mesh decorative_mesh;

	public Mesh decorative_mesh_dummy;

	public int limb_mesh_vertices_len;

	public Vector3[] limb_mesh_vertices;

	public int decorative_mesh_vertices_len;

	public Vector3[] decorative_mesh_vertices;

	public int decorative_mesh_dummy_vertices_len;

	public Vector3[] decorative_mesh_dummy_vertices;

	public static float animation_choppiness = 1f;

	public Texture2D null_tex;

	private GameObject folder_hybrid_prefabs;

	public GameObject player_particle_type;

	private int max_secs_til_deload = 60;

	public GameObject type_creatureModel;

	public GameObject type_limb;

	public GameObject type_limb_dummy;

	public List<string> all_creature_names = new List<string>();

	public List<string> default_creature_names = new List<string>();

	public Dictionary<string, List<string>> premium_creature_names = new Dictionary<string, List<string>>();

	public string[] animationNames;

	public int[] maxFrames;

	public string session_animal = "";

	public bool screen_resized;

	private Dictionary<string, CreatureModel> hybrid_prefabs = new Dictionary<string, CreatureModel>();

	private List<ObjActionPair> adjust_lite_model_heights_next_frame = new List<ObjActionPair>();

	public GameObject floorType;

	public GameObject collidrType;

	private GameObject collidr;

	private int offset;

	private GameObject floor;

	public void Start_0()
	{
		if (Instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		Instance = this;
		limb_mesh_vertices_len = limb_mesh.vertices.Length;
		limb_mesh_vertices = new Vector3[limb_mesh_vertices_len];
		for (int i = 0; i < limb_mesh_vertices_len; i++)
		{
			limb_mesh_vertices[i] = limb_mesh.vertices[i];
		}
		decorative_mesh_vertices_len = decorative_mesh.vertices.Length;
		decorative_mesh_vertices = new Vector3[decorative_mesh_vertices_len];
		for (int j = 0; j < decorative_mesh_vertices_len; j++)
		{
			decorative_mesh_vertices[j] = decorative_mesh.vertices[j];
		}
		decorative_mesh_dummy_vertices_len = decorative_mesh_dummy.vertices.Length;
		decorative_mesh_dummy_vertices = new Vector3[decorative_mesh_dummy_vertices_len];
		for (int k = 0; k < decorative_mesh_dummy_vertices_len; k++)
		{
			decorative_mesh_dummy_vertices[k] = decorative_mesh_dummy.vertices[k];
		}
	}

	public void Start_1()
	{
		if (this != Instance)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		PlayerPrefs.SetInt("n_sessions", PlayerPrefs.GetInt("n_sessions") + 1);
		CreateCollider();
		SceneManager.activeSceneChanged += ChangedActiveScene;
		folder_hybrid_prefabs = new GameObject("Hybrid Prefabs");
		UnityEngine.Object.DontDestroyOnLoad(folder_hybrid_prefabs);
		StartCoroutine(ClearHybridPrefabsCoroutine());
	}

	public static void GenerateCreatureLists()
	{
		string text = Application.dataPath + "/SYNCHRONOUS/TextFiles/AutoGen/";
		string text2 = Application.dataPath + "/SYNCHRONOUS/TextFiles/creatures";
		string path = Application.dataPath + "/SYNCHRONOUS/TextFiles/PurchaseStructs";
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		string[] files = Directory.GetFiles(text2);
		foreach (string text3 in files)
		{
			if (Path.GetExtension(text3) == ".txt")
			{
				list.Add(Path.GetFileNameWithoutExtension(text3));
			}
		}
		files = Directory.GetFiles(path);
		foreach (string text4 in files)
		{
			if (!(Path.GetExtension(text4) == ".txt"))
			{
				continue;
			}
			list2.Add("[" + Path.GetFileNameWithoutExtension(text4) + "]");
			string[] array = File.ReadAllLines(text4);
			foreach (string text5 in array)
			{
				int num = text5.IndexOf('=');
				if (num == -1)
				{
					continue;
				}
				string text6 = text5.Substring(0, num - 1);
				string text7 = text5.Substring(num + 2, text5.Length - (num + 2));
				if (text6 == "animal0" || text6 == "animal1" || text6 == "animal2" || text6 == "animal3" || text6 == "animal4")
				{
					if (!File.Exists(text2 + "/" + text7 + ".txt"))
					{
						Debug.Log("ERROR: '" + text7 + "' DOES NOT EXIST IN '" + Path.GetFileNameWithoutExtension(text4) + "'");
					}
					else
					{
						list2.Add(text7);
					}
					list.Remove(text7);
				}
			}
			list2.Add("");
		}
		Startup.WriteOnlyIfChanged(text + "(Auto Gen) Creatures List Premium.txt", list2.ToArray());
		Startup.WriteOnlyIfChanged(text + "(Auto Gen) Creatures List Default.txt", list.ToArray());
	}

	public void CreateMutantParticle(LiteModel model)
	{
		int count = model.original.creatures_that_made_me.Count;
		if (count != 2)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(player_particle_type);
			gameObject.transform.SetParent(model.transform);
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.GetComponent<ParticleSystem>().startColor = get_mutant_particle_color(count - 2);
		}
	}

	private Color get_mutant_particle_color(int N)
	{
		switch (N)
		{
		case 1:
			return new Color(0f, 1f, 0f, 1f);
		case 2:
			return new Color(0.2f, 0.2f, 1f, 1f);
		case 3:
			return new Color(0f, 1f, 1f, 1f);
		case 4:
			return new Color(0.7f, 0.2f, 1f, 1f);
		case 5:
			return new Color(1f, 0f, 0f, 1f);
		default:
			return new Color(1f, 1f, 0f, 1f);
		}
	}

	private IEnumerator ClearHybridPrefabsCoroutine()
	{
		int interval = 5;
		while (true)
		{
			yield return new WaitForSeconds(interval);
			List<CreatureModel> list = new List<CreatureModel>();
			foreach (KeyValuePair<string, CreatureModel> hybrid_prefab in hybrid_prefabs)
			{
				CreatureModel value = hybrid_prefab.Value;
				if (value.n_LITE_instances == 0)
				{
					value.secs_until_deload -= interval;
					if (value.secs_until_deload < 1)
					{
						list.Add(value);
					}
				}
				else
				{
					value.secs_until_deload = max_secs_til_deload;
				}
			}
			foreach (CreatureModel item in list)
			{
				UnityEngine.Object.Destroy(item.gameObject);
				hybrid_prefabs.Remove(item.key);
			}
		}
	}

	public void ClearAllHybridPrefabs()
	{
		foreach (KeyValuePair<string, CreatureModel> hybrid_prefab in hybrid_prefabs)
		{
			UnityEngine.Object.Destroy(hybrid_prefab.Value.gameObject);
		}
		hybrid_prefabs.Clear();
	}

	public string GetRandomCreature()
	{
		return all_creature_names[UnityEngine.Random.Range(0, all_creature_names.Count)];
	}

	public string GetRandomPremiumCreature()
	{
		return null;
	}

	public int GetCreatureIndex_(string name)
	{
		return 0;
	}

	public string GetCreatureName_(int index)
	{
		return null;
	}

	public int GetNumCreatures()
	{
		return 0;
	}

	public void RecycleCreature(GameObject creature)
	{
	}

	public GameObject CreatePlayerCreatureModel(int slot)
	{
		GameObject hybridLite = GetHybridLite(GameController.Instance.player_parent_creatures);
		hybridLite.GetComponent<LiteModel>().animation_choppiness = GraphicsControl.Instance.SpecialAnimationChoppiness();
		return hybridLite;
	}

	public void SetCubeColor(float[] hsv_get, GameObject g, Color base_color)
	{
		HsvColor hsvColor = HSVUtil.ConvertRgbToHsv(base_color);
		float normalizedH = hsvColor.normalizedH;
		float num = hsv_get[0];
		float normalizedS = hsvColor.normalizedS;
		float num2 = hsv_get[1];
		float normalizedV = hsvColor.normalizedV;
		float num3 = hsv_get[2];
		normalizedS = Mathf.Clamp01(normalizedS + num2 / 100f * 3f);
		normalizedV = Mathf.Clamp01(normalizedV + num3 / 100f * 4f);
		normalizedH = (normalizedH + num / 100f) % 1f;
		g.GetComponent<Limb>().cube_color = HSVUtil.ConvertHsvToRgb(normalizedH * 360f, normalizedS, normalizedV, 1f);
	}

	public List<T> GetCulledParentList<T>(List<T> parents)
	{
		List<T> list = new List<T>();
		if (parents.Count < 13)
		{
			foreach (T parent in parents)
			{
				list.Add(parent);
			}
		}
		else
		{
			for (int i = 0; i < 3; i++)
			{
				list.Add(parents[i]);
			}
			int count = parents.Count;
			float num = 2f;
			for (int j = 0; j < 6; j++)
			{
				num += ((float)(count - 6) - 1f) / 6f;
				list.Add(parents[(int)num]);
			}
			for (int k = -3; k < 0; k++)
			{
				list.Add(parents[parents.Count + k]);
			}
		}
		return list;
	}

	public GameObject GetHybridLite(string parent, Action on_hybrid_ready = null)
	{
		List<string> list = new List<string>();
		list.Add(parent);
		GameObject hybridLite = GetHybridLite(list, on_hybrid_ready);
		hybridLite.GetComponent<LiteModel>().animation_choppiness = GraphicsControl.Instance.SpecialAnimationChoppiness();
		return hybridLite;
	}

	public GameObject GetHybridLite(List<string> parents, Action on_hybrid_ready = null)
	{
		string text = "";
		foreach (string parent in parents)
		{
			text = text + parent + "+";
		}
		if (!hybrid_prefabs.ContainsKey(text))
		{
			GameObject gameObject;
			if (parents.Count == 1)
			{
				gameObject = LoadPlainCreatureFromDisk(parents[0]);
				gameObject.SetActive(false);
			}
			else
			{
				List<string> culledParentList = GetCulledParentList(parents);
				List<GameObject> list = new List<GameObject>();
				foreach (string item in culledParentList)
				{
					list.Add(LoadPlainCreatureFromDisk(item));
				}
				gameObject = BeginMorph(list, false, 0, text);
				foreach (GameObject item2 in list)
				{
					UnityEngine.Object.Destroy(item2);
				}
			}
			gameObject.transform.SetParent(folder_hybrid_prefabs.transform);
			gameObject.GetComponent<CreatureModel>().GenerateModelAndTexture();
			hybrid_prefabs.Add(text, gameObject.GetComponent<CreatureModel>());
			hybrid_prefabs[text].key = text;
		}
		hybrid_prefabs[text].secs_until_deload = max_secs_til_deload;
		hybrid_prefabs[text].n_LITE_instances++;
		GameObject gameObject2 = CloneHybridLite(hybrid_prefabs[text], on_hybrid_ready);
		gameObject2.name = "model-LITE";
		return gameObject2;
	}

	private string ReadLine(ref List<string> lines, ref int curr_line, int n_skips = 0)
	{
		string text;
		do
		{
			if (n_skips > 50)
			{
				return "0";
			}
			n_skips++;
			text = lines[curr_line];
			curr_line++;
		}
		while (Startup.StringNullOrWhitespace(text));
		return text;
	}

	private GameObject LoadPlainCreatureFromDisk(string animal_ENGLISH)
	{
		bool file_exists = false;
		if (!all_creature_names.Contains(animal_ENGLISH))
		{
			animal_ENGLISH = "crab";
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(type_creatureModel);
		CreatureModel component = gameObject.GetComponent<CreatureModel>();
		component.creatures_that_made_me.Add(animal_ENGLISH);
		gameObject.name = "FULL-MODEL (" + animal_ENGLISH + ")";
		file_exists = false;
		List<string> lines = ResourceControl.Instance.GetTextFileLines("creatures/" + animal_ENGLISH, ref file_exists);
		int curr_line = 0;
		int.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
		component.height = float.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
		float r = float.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
		float g = float.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
		float b = float.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
		component.base_color = new Color(r, g, b, 1f);
		component.start_perk = ReadLine(ref lines, ref curr_line);
		ReadLine(ref lines, ref curr_line);
		component.seed = float.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
		string text = ReadLine(ref lines, ref curr_line);
		string text2 = ReadLine(ref lines, ref curr_line);
		string text3 = ReadLine(ref lines, ref curr_line);
		string text4 = ReadLine(ref lines, ref curr_line);
		string text5 = ReadLine(ref lines, ref curr_line);
		string text6 = ReadLine(ref lines, ref curr_line);
		string text7 = ReadLine(ref lines, ref curr_line);
		string text8 = ReadLine(ref lines, ref curr_line);
		string text9 = ReadLine(ref lines, ref curr_line);
		string text10 = ReadLine(ref lines, ref curr_line);
		string text11 = ReadLine(ref lines, ref curr_line);
		string text12 = ReadLine(ref lines, ref curr_line);
		string text13 = ReadLine(ref lines, ref curr_line);
		string text14 = ReadLine(ref lines, ref curr_line);
		string text15 = ReadLine(ref lines, ref curr_line);
		string text16 = ReadLine(ref lines, ref curr_line);
		string text17 = ReadLine(ref lines, ref curr_line);
		string text18 = ReadLine(ref lines, ref curr_line);
		string text19 = ReadLine(ref lines, ref curr_line);
		string text20 = ReadLine(ref lines, ref curr_line);
		string text21 = ReadLine(ref lines, ref curr_line);
		string text22 = ReadLine(ref lines, ref curr_line);
		string text23 = ReadLine(ref lines, ref curr_line);
		string text24 = ReadLine(ref lines, ref curr_line);
		switch ((int)TranslationControl.Instance.use_language)
		{
		case 0:
			component.myName = animal_ENGLISH;
			component.noun = text2;
			component.adjective_M = text;
			component.adjective_F = "?";
			component.adjective_N = "?";
			component.grammatical_gender = "?";
			component.creatures_that_made_me_TRANSLATED.Add(animal_ENGLISH);
			break;
		case 1:
			component.myName = text6;
			component.noun = text8;
			component.adjective_M = text7;
			component.adjective_F = text19;
			component.adjective_N = text24;
			component.grammatical_gender = text22;
			component.creatures_that_made_me_TRANSLATED.Add(text6);
			break;
		case 2:
			component.myName = text3;
			component.noun = text5;
			component.adjective_M = text4;
			component.adjective_F = text18;
			component.adjective_N = "?";
			component.grammatical_gender = text21;
			component.creatures_that_made_me_TRANSLATED.Add(text3);
			break;
		case 3:
			component.myName = text9;
			component.noun = text11;
			component.adjective_M = text10;
			component.adjective_F = "?";
			component.adjective_N = "?";
			component.grammatical_gender = "?";
			component.creatures_that_made_me_TRANSLATED.Add(text9);
			break;
		case 4:
			component.myName = text12;
			component.noun = text14;
			component.adjective_M = text13;
			component.adjective_F = text20;
			component.adjective_N = "?";
			component.grammatical_gender = text23;
			component.creatures_that_made_me_TRANSLATED.Add(text12);
			break;
		case 5:
			component.myName = text15;
			component.noun = text17;
			component.adjective_M = text16;
			component.adjective_F = "?";
			component.adjective_N = "?";
			component.grammatical_gender = "?";
			component.creatures_that_made_me_TRANSLATED.Add(text15);
			break;
		}
		ReadLine(ref lines, ref curr_line);
		for (int i = 0; i < 5; i++)
		{
			string text25 = ReadLine(ref lines, ref curr_line);
			string text26 = ((text25 == "none") ? "?" : text25);
			if (text26 != "?")
			{
				component.all_possible_drops.Add(text26);
			}
		}
		int num = int.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
		Limb[] array = new Limb[num];
		component.limbs_ = array;
		for (int j = 0; j < num; j++)
		{
			array[j] = UnityEngine.Object.Instantiate(type_limb).GetComponent<Limb>();
			array[j].SetUpFrames(false);
		}
		component.torso_obj = array[0].gameObject;
		for (int k = 0; k < num; k++)
		{
			array[k].transform.SetParent(gameObject.transform);
			Limb limb = array[k];
			GameObject visual = limb.visual;
			limb.compName = ReadLine(ref lines, ref curr_line);
			TestForSpecialLimb(limb.compName, component, limb.gameObject);
			if (limb.compName == "eyes" || limb.compName == "mouth")
			{
				limb.rigid_limb = true;
			}
			limb.isDecorative = bool.Parse(ReadLine(ref lines, ref curr_line));
			limb.symmetrical = bool.Parse(ReadLine(ref lines, ref curr_line));
			array[k].invertSymmAnm = bool.Parse(ReadLine(ref lines, ref curr_line));
			limb.inherit = bool.Parse(ReadLine(ref lines, ref curr_line));
			limb.hide_on_wear_hat = bool.Parse(ReadLine(ref lines, ref curr_line));
			Transform transform = visual.transform;
			float x = float.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
			float y = float.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
			float z = float.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
			transform.localScale = new Vector3(x, y, z);
			Transform transform2 = visual.transform;
			float x2 = float.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
			float y2 = float.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
			float z2 = float.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
			transform2.localPosition = new Vector3(x2, y2, z2);
			int num2 = int.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
			if (num2 != -1)
			{
				limb.parent_limb_ = array[num2];
				limb.snap_par = limb.parent_limb_.visual.transform;
			}
			limb.sub_limbs = new List<Limb>();
			int num3 = int.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
			for (int l = 0; l < num3; l++)
			{
				limb.sub_limbs.Add(array[int.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture)]);
			}
			for (int m = 0; m < 4; m++)
			{
				limb.hsv_values[m] = float.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
			}
			limb.textureName = ReadLine(ref lines, ref curr_line);
			SetCubeColor(limb.hsv_values, array[k].gameObject, component.base_color);
			int num4 = int.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
			for (int n = 0; n < num4; n++)
			{
				int num5 = int.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
				for (int num6 = 0; num6 < num5; num6++)
				{
					float x3 = float.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
					float y3 = float.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
					float z3 = float.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
					float w = float.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
					float x4 = float.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
					float y4 = float.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
					float z4 = float.Parse(ReadLine(ref lines, ref curr_line), Startup.parse_culture);
					if (num6 < limb.GetNumFrames(n))
					{
						limb.SetFrameRotation(n, num6, new Quaternion(x3, y3, z3, w));
						limb.SetFrameSnapPosition(n, num6, new Vector3(x4, y4, z4));
					}
				}
			}
		}
		if (component.head_obj == null)
		{
			component.head_obj = array[0].gameObject;
		}
		for (int num7 = 0; num7 < num; num7++)
		{
			if (array[num7].symmetrical)
			{
				array[num7].ChainMakeDummy(true);
			}
			array[num7].transform.rotation = array[num7].GetFrameRotation(0, 0);
			if (array[num7].parent_limb_ != null)
			{
				array[num7].SetSnapLocal(array[num7].GetFrameSnapPosition(0, 0));
			}
			else
			{
				array[num7].visual.transform.localPosition = array[num7].GetFrameSnapPosition(0, 0);
			}
		}
		array[0].transform.localPosition = Vector3.zero;
		return gameObject;
	}

	public GameObject CloneHybridLite(CreatureModel original_model, Action on_hybrid_ready = null)
	{
		GameObject gameObject = new GameObject("model-LITE");
		gameObject.AddComponent<LiteModel>();
		LiteModel component = gameObject.GetComponent<LiteModel>();
		component.original = original_model;
		component.limbs_ = new LimbLite[original_model.limbs_.Length];
		for (int i = 0; i < original_model.limbs_.Length; i++)
		{
			Limb limb = original_model.limbs_[i];
			LimbLite limbLite = new LimbLite();
			component.limbs_[i] = limbLite;
			limbLite.original = limb;
			limbLite.transform_position = limb.transform.localPosition;
			limbLite.transform_rotation = limb.transform.localRotation;
			limbLite.visual_transform_localPosition = limb.visual.transform.localPosition;
			limbLite.visual_transform_localScale = limb.visual.transform.localScale;
			if (limbLite.visual_transform_localScale.x < 0f || limbLite.visual_transform_localScale.z < 0f || limbLite.visual_transform_localScale.y < 0f)
			{
				limbLite.visual_transform_localScale = new Vector3(Mathf.Abs(limbLite.visual_transform_localScale.x), Mathf.Abs(limbLite.visual_transform_localScale.y), Mathf.Abs(limbLite.visual_transform_localScale.z));
			}
			if (original_model.head_obj == limb.gameObject)
			{
				component.head_limb_index = i;
			}
			else if (original_model.hand_obj == limb.gameObject)
			{
				component.hand_limb_index = i;
			}
			else if (original_model.eyeObject_ == limb.gameObject)
			{
				component.eye_limb_index = i;
			}
			else if (original_model.mouthObject_ == limb.gameObject)
			{
				component.mouth_limb_index = i;
			}
			if (limb.has_dummy)
			{
				limbLite.dummy_visual_transform_localPosition = new Vector3(0f - limb.visual.transform.localPosition.x, limb.visual.transform.localPosition.y, limb.visual.transform.localPosition.z);
			}
		}
		for (int j = 0; j < original_model.limbs_.Length; j++)
		{
			Limb limb2 = original_model.limbs_[j];
			LimbLite limbLite2 = component.limbs_[j];
			if (!(limb2.parent_limb_ != null))
			{
				continue;
			}
			for (int k = 0; k < original_model.limbs_.Length; k++)
			{
				if (original_model.limbs_[k] == limb2.parent_limb_)
				{
					limbLite2.parent_limb_ = component.limbs_[k];
					break;
				}
			}
		}
		component.animation_choppiness = GraphicsControl.Instance.DefaultAnimationChoppiness();
		component.CreateMeshClones();
		component.TryAssignEyeTexture();
		component.StartBlinking();
		component.TryAssignMouthTexture("Mouths");
		ObjActionPair objActionPair = new ObjActionPair();
		objActionPair.obj = gameObject;
		objActionPair.action = delegate
		{
			on_hybrid_ready?.Invoke();
		};
		if (original_model.height == 0f)
		{
			original_model.lite_models_that_need_height.Add(objActionPair);
		}
		else
		{
			component.original.height = original_model.height;
			adjust_lite_model_heights_next_frame.Add(objActionPair);
		}
		return gameObject;
	}

	private void SetHeight(GameObject obj)
	{
		Component component;
		Transform transform;
		Vector3 localPosition;
		float height;
		if (obj.GetComponent<CreatureModel>() != null)
		{
			CreatureModel component2 = obj.GetComponent<CreatureModel>();
			transform = component2.gameObject.transform;
			localPosition = component2.gameObject.transform.localPosition;
			height = component2.height;
			component = component2;
		}
		else
		{
			if (!(obj.GetComponent<LiteModel>() != null))
			{
				Debug.Log("ERROR: no creatureModel or liteModel");
				return;
			}
			LiteModel component3 = obj.GetComponent<LiteModel>();
			transform = component3.gameObject.transform;
			localPosition = component3.gameObject.transform.localPosition;
			height = component3.original.height;
			component = component3;
		}
		transform.localPosition = localPosition + Vector3.up * (height * component.gameObject.transform.localScale.y);
	}

	private void FixedUpdate()
	{
		if (adjust_lite_model_heights_next_frame.Count == 0)
		{
			return;
		}
		foreach (ObjActionPair item in adjust_lite_model_heights_next_frame)
		{
			if (item.obj != null)
			{
				SetHeight(item.obj);
			}
			if (item.action != null)
			{
				item.action();
			}
		}
		adjust_lite_model_heights_next_frame.Clear();
	}

	private void CreateCollider()
	{
		floor = UnityEngine.Object.Instantiate(floorType);
		collidr = UnityEngine.Object.Instantiate(collidrType);
		collidr.name = "Collidr";
		collidr.transform.localPosition = Vector3.one * 27f;
	}

	private void ChangedActiveScene(Scene current, Scene next)
	{
		floor = UnityEngine.Object.Instantiate(floorType);
		collidr = UnityEngine.Object.Instantiate(collidrType);
		collidr.name = "Collidr";
		collidr.transform.localPosition = Vector3.one * 27f;
	}

	private void TestForSpecialLimb(string str, CreatureModel model, GameObject o)
	{
		if (str == "head")
		{
			model.head_obj = o;
		}
		if (str == "mouth")
		{
			model.mouthObject_ = o;
		}
		if (str == "eyes")
		{
			model.eyeObject_ = o;
		}
		if (str == "hand")
		{
			model.hand_obj = o;
		}
		else if ((str == "arm" || str == "wing") && model.hand_obj == null)
		{
			model.hand_obj = o;
		}
	}

	private GameObject BeginMorph(List<GameObject> creaturesToMorph, bool justHead, int screenshot, string key)
	{
		List<string> list = new List<string>();
		List<GameObject> list2 = new List<GameObject>();
		GameObject gameObject = UnityEngine.Object.Instantiate(type_creatureModel);
		CreatureModel component = gameObject.GetComponent<CreatureModel>();
		component.myName = creaturesToMorph[0].GetComponent<CreatureModel>().myName + "-" + creaturesToMorph[1].GetComponent<CreatureModel>().myName;
		gameObject.name = "FULL-MODEL (" + component.myName + ")";
		CreatureModel component2 = creaturesToMorph[0].GetComponent<CreatureModel>();
		CreatureModel component3 = creaturesToMorph[creaturesToMorph.Count - 1].GetComponent<CreatureModel>();
		component.adjective_M = component2.adjective_M;
		component.adjective_F = component2.adjective_F;
		component.adjective_N = component2.adjective_N;
		component.noun = component3.noun;
		component.grammatical_gender = component3.grammatical_gender;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (GameObject item in creaturesToMorph)
		{
			CreatureModel component4 = item.GetComponent<CreatureModel>();
			foreach (string item2 in component4.creatures_that_made_me)
			{
				component.creatures_that_made_me.Add(item2);
				for (int i = 0; i < item2.Length; i++)
				{
					char c = item2[i];
					num3 += c;
					num2++;
					if (num2 > 2)
					{
						num += c;
						num2 = 0;
					}
				}
			}
			foreach (string item3 in component4.creatures_that_made_me_TRANSLATED)
			{
				component.creatures_that_made_me_TRANSLATED.Add(item3);
			}
			foreach (string all_possible_drop in component4.all_possible_drops)
			{
				if (!component.all_possible_drops.Contains(all_possible_drop))
				{
					component.all_possible_drops.Add(all_possible_drop);
				}
			}
		}
		if (component.all_possible_drops.Count > 0)
		{
			component.drop1 = component.all_possible_drops[num3 % component.all_possible_drops.Count];
			int index = ((component.all_possible_drops.Count >= 2) ? (num % component.all_possible_drops.Count) : (num3 % gameObject.GetComponent<CreatureModel>().all_possible_drops.Count));
			component.drop2 = component.all_possible_drops[index];
		}
		Color color = new Color(1f, 1f, 1f, 1f);
		if (creaturesToMorph.Count >= 1)
		{
			int num4 = -1;
			double num5 = 0.0;
			for (int j = 0; j < creaturesToMorph.Count; j++)
			{
				Color base_color = creaturesToMorph[j].GetComponent<CreatureModel>().base_color;
				HsvColor hsvColor = HSVUtil.ConvertRgbToHsv(base_color);
				double num6 = (((hsvColor.V > 0.85f && hsvColor.S < 0.15f) || hsvColor.V < 0.1f) ? 2.0 : (hsvColor.S + hsvColor.V));
				if (num5 < num6)
				{
					num5 = num6;
					color = base_color;
					num4 = j;
				}
			}
			bool flag = false;
			for (int k = 0; k < creaturesToMorph.Count; k++)
			{
				bool flag2 = false;
				for (int l = 0; l < creaturesToMorph[k].GetComponent<CreatureModel>().limbs_.Length; l++)
				{
					string compName = creaturesToMorph[k].GetComponent<CreatureModel>().limbs_[l].compName;
					bool flag3 = compName == "nose";
					if (list.IndexOf(compName) == -1)
					{
						list.Add(compName);
						GameObject gameObject2 = UnityEngine.Object.Instantiate(type_limb);
						gameObject2.GetComponent<Limb>().compName = compName;
						gameObject2.GetComponent<Limb>().SetUpFrames(false);
						list2.Add(gameObject2);
						TestForSpecialLimb(compName, component, gameObject2);
						if (compName == "eyes" || compName == "mouth")
						{
							gameObject2.GetComponent<Limb>().rigid_limb = true;
						}
					}
					flag2 = flag2 || flag3;
				}
				if (component.hand_obj == null)
				{
					component.hand_obj = list2[0];
				}
				if (component.head_obj == null)
				{
					component.head_obj = list2[0];
				}
				component.torso_obj = list2[0];
				if (k != num4)
				{
					CreatureModel component5 = creaturesToMorph[k].GetComponent<CreatureModel>();
					color = Color.Lerp(color, component5.base_color, 1f / (float)creaturesToMorph.Count * 0.25f);
				}
				flag = flag || !flag2;
			}
			if (flag)
			{
				int num7 = list.IndexOf("nose");
				if (num7 != -1)
				{
					UnityEngine.Object.Destroy(list2[num7]);
					list2.RemoveAt(num7);
					list.RemoveAt(num7);
				}
			}
		}
		for (int m = 0; m < list2.Count; m++)
		{
			list2[m].transform.SetParent(gameObject.transform);
			string text = list[m];
			int num8 = 0;
			for (int n = 0; n < creaturesToMorph.Count; n++)
			{
				for (int num9 = 0; num9 < creaturesToMorph[n].GetComponent<CreatureModel>().limbs_.Length; num9++)
				{
					if (!(creaturesToMorph[n].GetComponent<CreatureModel>().limbs_[num9].compName == text))
					{
						continue;
					}
					Limb limb = creaturesToMorph[n].GetComponent<CreatureModel>().limbs_[num9];
					Limb component6 = list2[m].GetComponent<Limb>();
					GameObject visual = component6.visual;
					GameObject visual2 = limb.visual;
					Transform transform = visual.transform;
					if (num8 == 0)
					{
						transform.localScale = visual2.transform.localScale;
						visual.transform.localPosition = visual2.transform.localPosition;
						component6.isDecorative = limb.isDecorative;
						component6.symmetrical = limb.symmetrical;
						component6.inherit = limb.inherit;
						if (limb.hide_on_wear_hat)
						{
							component6.hide_on_wear_hat = true;
						}
						list2[m].GetComponent<Limb>().invertSymmAnm = creaturesToMorph[n].GetComponent<CreatureModel>().limbs_[num9].invertSymmAnm;
						component6.hsv_values = (float[])limb.hsv_values.Clone();
						component6.textureName = limb.textureName;
						if (limb.parent_limb_ != null)
						{
							Limb component7 = list2[list.IndexOf(limb.parent_limb_.compName)].GetComponent<Limb>();
							component6.snap_par = component7.visual.transform;
							component6.parent_limb_ = component7;
							list2[list.IndexOf(limb.parent_limb_.compName)].GetComponent<Limb>().sub_limbs.Add(list2[m].GetComponent<Limb>());
						}
						component6.sub_limbs = new List<Limb>();
						for (int num10 = 0; num10 < limb.GetNumAnimations(); num10++)
						{
							for (int num11 = 0; num11 < limb.GetNumFrames(num10); num11++)
							{
								component6.SetFrameRotation(num10, num11, limb.GetFrameRotation(num10, num11));
								component6.SetFrameSnapPosition(num10, num11, limb.GetFrameSnapPosition(num10, num11));
							}
						}
						num8 = 1;
						continue;
					}
					float num12 = num8;
					num8++;
					float num13 = num8;
					transform.localScale = (visual.transform.localScale * num12 + visual2.transform.localScale) / num13;
					visual.transform.localPosition = (visual.transform.localPosition * num12 + visual2.transform.localPosition) / num13;
					for (int num14 = 0; num14 < 4; num14++)
					{
						component6.hsv_values[num14] = (component6.hsv_values[num14] * num12 + limb.hsv_values[num14]) / num13;
					}
					if (component6.textureName == "None")
					{
						component6.textureName = limb.textureName;
					}
					for (int num15 = 0; num15 < limb.GetNumAnimations(); num15++)
					{
						for (int num16 = 0; num16 < limb.GetNumFrames(num15); num16++)
						{
							component6.SetFrameRotation(num15, num16, Quaternion.Lerp(component6.GetFrameRotation(num15, num16), limb.GetFrameRotation(num15, num16), 0.5f));
							if (limb.parent_limb_ != null)
							{
								Vector3 normalized = component6.GetFrameSnapPosition(num15, num16).normalized;
								Vector3 normalized2 = limb.GetFrameSnapPosition(num15, num16).normalized;
								component6.SetFrameSnapPosition(num15, num16, ((normalized * num12 + normalized2) / num13).normalized);
							}
						}
					}
				}
			}
			if (num8 == 1 && list2[m].GetComponent<Limb>().compName == "mouth")
			{
				list2[m].GetComponent<Limb>().textureName = "InvisibleMouth";
			}
		}
		component.base_color = color;
		GameObject[] array = new GameObject[list2.Count];
		for (int num17 = 0; num17 < list2.Count; num17++)
		{
			SetCubeColor(list2[num17].GetComponent<Limb>().hsv_values, list2[num17], component.base_color);
		}
		for (int num18 = 0; num18 < list2.Count; num18++)
		{
			if (list2[num18].GetComponent<Limb>().symmetrical)
			{
				list2[num18].GetComponent<Limb>().ChainMakeDummy(true);
			}
			array[num18] = list2[num18];
		}
		gameObject.transform.position = new Vector3(offset + 100, offset + 100, offset + 100);
		offset += 10;
		for (int num19 = 0; num19 < list2[0].GetComponent<Limb>().GetNumAnimations(); num19++)
		{
			for (int num20 = 0; num20 < list2[0].GetComponent<Limb>().GetNumFrames(num19); num20++)
			{
				LoadFrame(num19, num20, array);
				for (int num21 = 0; num21 < list2.Count; num21++)
				{
					if (list2[num21].GetComponent<Limb>().parent_limb_ != null)
					{
						list2[num21].GetComponent<Limb>().parent_limb_.SetUpCollider(collidr);
						DoRaycast(list2[num21], num19, num20);
						collidr.GetComponent<BoxCollider>().enabled = false;
					}
				}
			}
		}
		collidr.transform.SetParent(null);
		collidr.transform.position = Vector3.up * 20f;
		LoadFrame(0, 0, array);
		Limb[] array2 = new Limb[array.Length];
		for (int num22 = 0; num22 < array.Length; num22++)
		{
			array2[num22] = array[num22].GetComponent<Limb>();
		}
		component.limbs_ = array2;
		StartCoroutine(DelayedCalculateHeightCoroutine(gameObject, floor, screenshot, key));
		component.start_perk = GetStartingPerk(creaturesToMorph[0].GetComponent<CreatureModel>().start_perk, creaturesToMorph[1].GetComponent<CreatureModel>().start_perk, creaturesToMorph[0].GetComponent<CreatureModel>().seed, creaturesToMorph[1].GetComponent<CreatureModel>().seed);
		list2[0].transform.localPosition = Vector3.zero;
		return gameObject;
	}

	public string GetStartingPerk(string perkA, string perkB, float seedA, float seedB)
	{
		int[] array = new int[6];
		for (int i = 0; i < 6; i++)
		{
			seedA *= 10f;
			array[i] = (int)seedA % 10;
		}
		int[] array2 = new int[6];
		for (int j = 0; j < 6; j++)
		{
			seedB *= 10f;
			array2[j] = (int)seedB % 10;
		}
		if ((float)(array2[1] + array2[0] * 10) * 0.1f < (float)(array[1] + array[0] * 10) * 0.1f)
		{
			return perkA;
		}
		return perkB;
	}

	private IEnumerator DelayedCalculateHeightCoroutine(GameObject MODEL, GameObject floor, int screenshot, string key)
	{
		yield return new WaitForSeconds(0.01f);
		if (MODEL == null || floor == null)
		{
			Debug.Log("MORPH ERROR");
			yield break;
		}
		_ = MODEL.transform.localPosition;
		MODEL.transform.localPosition = new Vector3(offset + 100, offset + 100, offset + 100);
		offset += 10;
		floor.transform.position = MODEL.transform.position + new Vector3(0f, -10f, 0f);
		Limb[] limbs_ = MODEL.GetComponent<CreatureModel>().limbs_;
		for (int i = 0; i < limbs_.Length; i++)
		{
			limbs_[i].visual.GetComponent<BoxCollider>().enabled = true;
		}
		if (floor.GetComponent<Rigidbody>().SweepTest(Vector3.up, out var hitInfo, 20f))
		{
			MODEL.GetComponent<CreatureModel>().height = 9.88f - hitInfo.distance;
		}
		SetHeight(MODEL);
		for (int j = 0; j < limbs_.Length; j++)
		{
			limbs_[j].visual.GetComponent<BoxCollider>().enabled = false;
		}
		MODEL.SetActive(false);
		if (!hybrid_prefabs.ContainsKey(key))
		{
			yield break;
		}
		CreatureModel creatureModel = hybrid_prefabs[key];
		foreach (ObjActionPair item in creatureModel.lite_models_that_need_height)
		{
			if (item.obj != null)
			{
				SetHeight(item.obj);
			}
			if (item.action != null)
			{
				item.action();
			}
		}
		creatureModel.lite_models_that_need_height.Clear();
	}

	public void DoRaycast(GameObject obj, int anm, int frame)
	{
		GameObject visual = obj.GetComponent<Limb>().parent_limb_.visual;
		Limb component = obj.GetComponent<Limb>();
		Vector3 normalized = (component.snap_global - visual.transform.position).normalized;
		float num = 10f;
		if (Physics.Raycast(visual.transform.position + normalized * 10f, -normalized, out var hitInfo, 10f))
		{
			num = hitInfo.distance;
		}
		component.snap_global = visual.transform.position + normalized * 10f + -normalized * num;
		obj.GetComponent<Limb>().SetFrameSnapPosition(anm, frame, component.GetSnapLocal());
	}

	public void LoadFrame(int animationNumber, int frameNumber, GameObject[] g)
	{
		for (int i = 0; i < g.Length; i++)
		{
			Limb component = g[i].GetComponent<Limb>();
			g[i].transform.rotation = component.GetFrameRotation(animationNumber, frameNumber);
			if (component.compName != "torso")
			{
				component.SetSnapLocal(component.GetFrameSnapPosition(animationNumber, frameNumber));
			}
			else
			{
				component.visual.transform.localPosition = component.GetFrameSnapPosition(animationNumber, frameNumber);
			}
		}
	}
}
