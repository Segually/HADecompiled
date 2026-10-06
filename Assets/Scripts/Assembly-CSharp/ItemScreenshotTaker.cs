using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemScreenshotTaker : MonoBehaviour, OrderedStart
{
	public static ItemScreenshotTaker Instance;

	public Camera item_screenshots_cam;

	public GameObject item_screenshots_light;

	public Dictionary<InventoryItem, Texture2D> cached_model3d_graphics = new Dictionary<InventoryItem, Texture2D>();

	public List<screenshot_pair> screenshot_queue = new List<screenshot_pair>();

	public Color item_screenshots_ambientCol;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
		StartCoroutine(ProcessScreenshots());
	}

	private IEnumerator ProcessScreenshots()
	{
		int frames_delay = ((GraphicsControl.Instance.GraphicsLevel() >= 4) ? 3 : ((GraphicsControl.Instance.GraphicsLevel() == 3) ? 5 : 7));
		while (true)
		{
			if (screenshot_queue.Count != 0)
			{
				screenshot_pair pair = screenshot_queue[0];
				screenshot_queue.RemoveAt(0);
				if (!cached_model3d_graphics.ContainsKey(pair.item_unmodified_for_caching))
				{
					for (int i = 0; i < 5; i++)
					{
						if (ShouldCancel(pair))
						{
							for (int j = 0; j < frames_delay; j++)
							{
								yield return new WaitForEndOfFrame();
							}
							if (pair.obj != null)
							{
								UnityEngine.Object.Destroy(pair.obj);
							}
							break;
						}
						bool finished_screenshot = false;
						switch (i)
						{
						case 0:
							InstantiateObjectForScreenshot(pair);
							break;
						case 1:
							if (!pair.obj_created)
							{
								for (int k = 0; k < frames_delay; k++)
								{
									yield return new WaitForEndOfFrame();
								}
								i--;
								continue;
							}
							PositionCamera(pair);
							TakeBaseScreenshot(pair);
							break;
						case 2:
							if (!pair.model_has_particles_by_default && !pair.paint_has_particles_and_model_supports_them)
							{
								ApplyOutlineGlowWithoutParticles(pair);
							}
							else
							{
								TakeParticlesScreenshot(pair);
							}
							break;
						case 3:
							if (!pair.model_has_particles_by_default && !pair.paint_has_particles_and_model_supports_them)
							{
								Finalize(pair);
								finished_screenshot = true;
							}
							else
							{
								ApplyOutlineGlowWithParticles(pair);
							}
							break;
						case 4:
							if (pair.model_has_particles_by_default || pair.paint_has_particles_and_model_supports_them)
							{
								Finalize(pair);
								finished_screenshot = true;
							}
							break;
						}
						for (int l = 0; l < frames_delay; l++)
						{
							yield return new WaitForEndOfFrame();
						}
						if (finished_screenshot)
						{
							if (pair.obj != null)
							{
								UnityEngine.Object.Destroy(pair.obj);
							}
							break;
						}
					}
					pair = null;
				}
				else
				{
					if (pair.itemSprite != null)
					{
						pair.itemSprite.Model3DScreenshotComplete(cached_model3d_graphics[pair.item_unmodified_for_caching]);
					}
					if (pair.obj != null)
					{
						UnityEngine.Object.Destroy(pair.obj);
					}
					for (int m = 0; m < frames_delay; m++)
					{
						yield return new WaitForEndOfFrame();
					}
				}
			}
			else
			{
				for (int n = 0; n < frames_delay; n++)
				{
					yield return new WaitForEndOfFrame();
				}
			}
		}
	}

	public void QueueForScreenshot(InventoryItem item_unmodified, GameObject model3d_generated_graphic_, ItemSprite original_sprite)
	{
		InventoryItem inventoryItem = InventoryItem.SetDisplayDefaults(item_unmodified);
		if (!cached_model3d_graphics.ContainsKey(item_unmodified))
		{
			model3d_generated_graphic_.GetComponent<RawImage>().enabled = false;
			screenshot_pair screenshot_pair = new screenshot_pair();
			screenshot_pair.item_unmodified_for_caching = item_unmodified;
			screenshot_pair.item_adjusted_for_rendering = inventoryItem;
			screenshot_pair.itemSprite = original_sprite;
			if (InventoryUtils.IsPaintbrush(inventoryItem.item_name))
			{
				screenshot_pair.paint_str = inventoryItem.item_name;
				screenshot_pair.stamp_str = "";
			}
			else
			{
				bool flag = InventoryUtils.IsStamp(inventoryItem.item_name);
				screenshot_pair.paint_str = inventory_ctr.Instance.GetPaintFromItemOrUseDefault(inventoryItem);
				screenshot_pair.stamp_str = (flag ? inventoryItem.item_name : inventory_ctr.Instance.GetStampFromItem(inventoryItem));
			}
			screenshot_pair.paint_layout_item = inventory_ctr.Instance.GetLayoutItemFromItem(inventoryItem.item_name);
			screenshot_pair.glowCol = AssignGlowCol(screenshot_pair);
			Instance.screenshot_queue.Add(screenshot_pair);
		}
		else
		{
			original_sprite.Model3DScreenshotComplete(cached_model3d_graphics[item_unmodified]);
		}
	}

	private void InstantiateObjectForScreenshot(screenshot_pair pair)
	{
		InventoryItem display_item = pair.item_adjusted_for_rendering;
		bool itemBool = inventory_ctr.Instance.GetItemBool(display_item.item_name, "is_wall_obj");
		bool itemBool2 = inventory_ctr.Instance.GetItemBool(display_item.item_name, "is_flooring_obj");
		Action<GameObject> on_model_ready = delegate(GameObject instance)
		{
			pair.obj = instance;
			OnObjectInstantiated(pair);
		};
		if (itemBool)
		{
			GameObject prefab = null;
			GameObject mesh1 = null;
			int n_to_load = 2;
			Action on_all_loaded = delegate
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
				gameObject.gameObject.SetActive(true);
				gameObject.transform.localScale = Vector3.one;
				gameObject.transform.localRotation = Quaternion.identity;
				gameObject.transform.localPosition = Vector3.zero;
				PartiallyGeneratedModularModel new_model = new PartiallyGeneratedModularModel(display_item, ModularObjectControl.segment.undefined, "", "", -1, -1, -1);
				ModularObjectControl.Instance.AddVertices(mesh1, 0, 0, 0, 0, 0, new_model);
				ModularObjectControl.Instance.AddVertices(mesh1, 1, 0, 0, 0, 2, new_model);
				ModularObjectControl.Instance.CreateMesh(gameObject, new_model, false);
				on_model_ready(gameObject);
			};
			ModularObjectControl.Instance.AsyncLoadModularModel("Wall-Models/" + display_item.item_name + "/1_prefab", delegate(GameObject new_mesh)
			{
				mesh1 = new_mesh;
				n_to_load--;
				if (n_to_load == 0)
				{
					on_all_loaded();
				}
			});
			ModularObjectControl.Instance.AsyncLoadModularModel("Wall-Prefabs/" + display_item.item_name, delegate(GameObject new_prefab)
			{
				prefab = new_prefab;
				n_to_load--;
				if (n_to_load == 0)
				{
					on_all_loaded();
				}
			});
		}
		else if (itemBool2)
		{
			GameObject prefab = null;
			GameObject mesh5 = null;
			GameObject mesh9 = null;
			GameObject mesh13 = null;
			string stringFromItemFile = ResourceControl.Instance.GetStringFromItemFile(display_item.item_name, "flooring_model");
			int n_to_load = 4;
			Action on_all_loaded = delegate
			{
				GameObject gameObject = new GameObject("inv_pathway_parent");
				gameObject.transform.localScale = Vector3.one;
				gameObject.transform.localRotation = Quaternion.identity;
				gameObject.transform.localPosition = Vector3.zero;
				GameObject gameObject2 = UnityEngine.Object.Instantiate(prefab);
				gameObject2.gameObject.SetActive(true);
				gameObject2.transform.SetParent(gameObject.transform);
				gameObject2.transform.localScale = Vector3.one;
				gameObject2.transform.localRotation = Quaternion.identity;
				gameObject2.transform.localPosition = Vector3.zero;
				GameObject gameObject3 = UnityEngine.Object.Instantiate(prefab);
				gameObject3.gameObject.SetActive(true);
				gameObject3.transform.SetParent(gameObject.transform);
				gameObject3.transform.localScale = Vector3.one;
				gameObject3.transform.localRotation = Quaternion.identity;
				gameObject3.transform.localPosition = Vector3.zero;
				PartiallyGeneratedModularModel new_model = new PartiallyGeneratedModularModel(display_item, ModularObjectControl.segment.undefined, "", "", -1, -1, -1);
				ModularObjectControl.Instance.AddVertices(mesh5, -1, -1, 3, 0, 1, new_model);
				ModularObjectControl.Instance.AddVertices(mesh5, 1, -1, 3, 0, 2, new_model);
				ModularObjectControl.Instance.AddVertices(mesh5, -1, 1, 3, 0, 0, new_model);
				ModularObjectControl.Instance.AddVertices(mesh5, 1, 1, 3, 0, 3, new_model);
				ModularObjectControl.Instance.AddVertices(mesh9, 0, -1, 1, 0, 2, new_model);
				ModularObjectControl.Instance.AddVertices(mesh9, -1, 0, 1, 0, 1, new_model);
				ModularObjectControl.Instance.AddVertices(mesh9, 1, 0, 1, 0, 3, new_model);
				ModularObjectControl.Instance.AddVertices(mesh9, 0, 1, 1, 0, 0, new_model);
				PartiallyGeneratedModularModel new_model2 = new PartiallyGeneratedModularModel(display_item, ModularObjectControl.segment.undefined, "", "", -1, -1, -1);
				ModularObjectControl.Instance.AddVertices(mesh13, 0, 0, 2, 0, 2, new_model2);
				ModularObjectControl.Instance.CreateMesh(gameObject2, new_model, false);
				ModularObjectControl.Instance.CreateMesh(gameObject3, new_model2, false);
				gameObject3.GetComponent<PaintableObject>().Colorize(pair.paint_str, pair.stamp_str, pair.paint_layout_item, display_item.item_name);
				gameObject2.GetComponent<PaintableObject>().Colorize(pair.paint_str, "", pair.paint_layout_item, display_item.item_name);
				on_model_ready(gameObject);
			};
			ModularObjectControl.Instance.AsyncLoadModularModel(stringFromItemFile + "/5_prefab", delegate(GameObject new_mesh)
			{
				mesh5 = new_mesh;
				n_to_load--;
				if (n_to_load == 0)
				{
					on_all_loaded();
				}
			});
			ModularObjectControl.Instance.AsyncLoadModularModel(stringFromItemFile + "/9_prefab", delegate(GameObject new_mesh)
			{
				mesh9 = new_mesh;
				n_to_load--;
				if (n_to_load == 0)
				{
					on_all_loaded();
				}
			});
			ModularObjectControl.Instance.AsyncLoadModularModel(stringFromItemFile + "/13_prefab", delegate(GameObject new_mesh)
			{
				mesh13 = new_mesh;
				n_to_load--;
				if (n_to_load == 0)
				{
					on_all_loaded();
				}
			});
			ModularObjectControl.Instance.AsyncLoadModularModel("Pathway-Prefabs/" + display_item.item_name, delegate(GameObject new_prefab)
			{
				prefab = new_prefab;
				n_to_load--;
				if (n_to_load == 0)
				{
					on_all_loaded();
				}
			});
		}
		else
		{
			ResourceControl.Instance.AsyncInstantiateWorldObjectPrefab(display_item, null, on_model_ready);
		}
	}

	private void OnObjectInstantiated(screenshot_pair pair)
	{
		GameObject instance = pair.obj;
		InventoryItem item_adjusted_for_rendering = pair.item_adjusted_for_rendering;
		DetectParticles(pair);
		string text = ResourceControl.Instance.GetStringFromItemFile(pair.item_adjusted_for_rendering.item_name, "Copy3dModelPosition");
		if (text == "")
		{
			text = pair.item_adjusted_for_rendering.item_name;
		}
		if (!inventory_ctr.Instance.GetItemBool(item_adjusted_for_rendering.item_name, "is_flooring_obj") && instance.GetComponent<PaintableObject>() != null)
		{
			inventory_ctr.inv_type_t itemType = inventory_ctr.Instance.GetItemType(pair.item_adjusted_for_rendering);
			float particle_scale = ((itemType == inventory_ctr.inv_type_t.armor || itemType == inventory_ctr.inv_type_t.helmet) ? 3f : 1f);
			instance.GetComponent<PaintableObject>().Colorize(pair.paint_str, pair.stamp_str, pair.paint_layout_item, item_adjusted_for_rendering.item_name, particle_scale, pair.paint_has_particles_and_model_supports_them);
		}
		instance.SetActive(false);
		ConstructionControl.Instance.DeleteUnnecessaryComponents(item_adjusted_for_rendering, instance);
		ConstructionControl.Instance.AdjustBuildableInstance(instance, item_adjusted_for_rendering, ConstructionControl.usage_context_t.on_item_screenshot, delegate
		{
			ItemSprite.RecursiveApplyLayer(instance.transform, LayerMask.NameToLayer("ItemScreenshot"), false);
			pair.obj_created = true;
		});
	}

	private void PositionCamera(screenshot_pair pair)
	{
		string text = ResourceControl.Instance.GetStringFromItemFile(pair.item_adjusted_for_rendering.item_name, "Copy3dModelPosition");
		if (text == "")
		{
			text = pair.item_adjusted_for_rendering.item_name;
		}
		float floatFromItemFile = ResourceControl.Instance.GetFloatFromItemFile(text, "model3d_camDist");
		float floatFromItemFile2 = ResourceControl.Instance.GetFloatFromItemFile(text, "model3d_camHeight");
		float floatFromItemFile3 = ResourceControl.Instance.GetFloatFromItemFile(text, "model3d_xRot");
		float floatFromItemFile4 = ResourceControl.Instance.GetFloatFromItemFile(text, "model3d_yRot");
		float floatFromItemFile5 = ResourceControl.Instance.GetFloatFromItemFile(text, "model3d_fov");
		float floatFromItemFile6 = ResourceControl.Instance.GetFloatFromItemFile(text, "model3d_recenterX");
		float floatFromItemFile7 = ResourceControl.Instance.GetFloatFromItemFile(text, "model3d_recenterY");
		float floatFromItemFile8 = ResourceControl.Instance.GetFloatFromItemFile(text, "model3d_objRot");
		float floatFromItemFile9 = ResourceControl.Instance.GetFloatFromItemFile(text, "model3d_wep_tilt");
		bool flag = ResourceControl.Instance.GetStringFromItemFile(text, "dual_wield") == "true";
		pair.obj.transform.position = Vector3.zero;
		if (pair.item_adjusted_for_rendering.item_name == "Painting" || pair.item_adjusted_for_rendering.item_name == "Blank Canvas")
		{
			pair.obj.transform.rotation = Quaternion.Euler(180f, 90f, 90f);
		}
		else
		{
			pair.obj.transform.rotation = Quaternion.identity;
		}
		pair.obj.transform.RotateAround(Vector3.left, floatFromItemFile9 * ((float)Math.PI / 180f));
		pair.obj.transform.RotateAround(Vector3.up, floatFromItemFile8 * 45f * ((float)Math.PI / 180f));
		if (flag)
		{
			Vector3 vector3FromItemFile = ResourceControl.Instance.GetVector3FromItemFile(text, "model3d_wep_dual_wield_offset");
			Vector3 vector3FromItemFile2 = ResourceControl.Instance.GetVector3FromItemFile(text, "model3d_wep_dual_wield_rotate");
			GameObject obj = pair.obj;
			GameObject gameObject = UnityEngine.Object.Instantiate(obj);
			gameObject.transform.RotateAround(Vector3.right, vector3FromItemFile2.x * ((float)Math.PI / 180f));
			gameObject.transform.RotateAround(Vector3.forward, vector3FromItemFile2.y * ((float)Math.PI / 180f));
			gameObject.transform.RotateAround(Vector3.up, vector3FromItemFile2.z * ((float)Math.PI / 180f));
			gameObject.transform.position = gameObject.transform.position + vector3FromItemFile;
			gameObject.transform.SetParent(obj.transform);
			gameObject.SetActive(true);
		}
		pair.obj.transform.RotateAround(Vector3.up, 30f * ((float)Math.PI / 180f));
		inventory_ctr.inv_type_t itemType = inventory_ctr.Instance.GetItemType(pair.item_adjusted_for_rendering);
		if (itemType == inventory_ctr.inv_type_t.armor || itemType == inventory_ctr.inv_type_t.helmet)
		{
			pair.obj.transform.localScale = Vector3.one * 0.333f;
		}
		else
		{
			pair.obj.transform.localScale = Vector3.one;
		}
		item_screenshots_cam.transform.rotation = Quaternion.identity;
		item_screenshots_cam.transform.position = new Vector3(0f, floatFromItemFile2, 0f - floatFromItemFile);
		item_screenshots_cam.transform.RotateAround(Vector3.zero, Vector3.up, floatFromItemFile3);
		item_screenshots_cam.transform.RotateAround(Vector3.zero, item_screenshots_cam.transform.right, floatFromItemFile4);
		item_screenshots_cam.fieldOfView = floatFromItemFile5;
		item_screenshots_cam.transform.position = item_screenshots_cam.transform.position + floatFromItemFile6 * item_screenshots_cam.transform.right * 0.02f + floatFromItemFile7 * item_screenshots_cam.transform.up * 0.02f;
		item_screenshots_cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
	}

	private void TakeBaseScreenshot(screenshot_pair pair)
	{
		Color col_before = new Color(0f, 0f, 0f, 1f);
		ShowLights(pair, ref col_before);
		if (pair.model_has_particles_by_default || pair.paint_has_particles_and_model_supports_them)
		{
			SetVisibleByTag(pair.obj.transform, "ItemScreenshotParticle", false);
		}
		SetVisibleByTag(pair.obj.transform, "ItemScreenshotHide", false);
		ScreenshotWithEffects.Instance.CaptureBaseScreenshot();
		HideLights(pair, ref col_before);
	}

	private void TakeParticlesScreenshot(screenshot_pair pair)
	{
		string stringFromItemFile = ResourceControl.Instance.GetStringFromItemFile(pair.item_adjusted_for_rendering.item_name, "model3d_override_particle_bg_col");
		Color color = ((stringFromItemFile != "") ? ParseRGBString(stringFromItemFile) : ((!pair.model_has_particles_by_default && !pair.paint_has_particles_and_model_supports_them) ? new Color(0f, 0f, 0f) : pair.glowCol));
		Color col_before = new Color(0f, 0f, 0f, 1f);
		ShowLights(pair, ref col_before);
		item_screenshots_cam.backgroundColor = new Color(color.r, color.g, color.b, 0f);
		SetVisibleByTag(pair.obj.transform, "ItemScreenshotParticle", true);
		SetOtherVisibleByTag(pair.obj.transform, "ItemScreenshotParticle", false, true);
		SetVisibleByTag(pair.obj.transform, "ItemScreenshotHide", false);
		ScreenshotWithEffects.Instance.CaptureParticlesScreenshot();
		HideLights(pair, ref col_before);
	}

	private Color AssignGlowCol(screenshot_pair pair)
	{
		string text = ResourceControl.Instance.GetStringFromItemFile(pair.item_adjusted_for_rendering.item_name, "model3d_override_glow_col");
		if (text == "" && pair.paint_str != "")
		{
			text = ResourceControl.Instance.GetColorScheme(pair.paint_str).GetGlowCol();
		}
		switch (text)
		{
		case "faint_red":
			return RGBCol(217, 128, 128);
		case "faint_white":
			return RGBCol(185, 185, 185);
		case "lt_blue":
			return RGBCol(167, 230, 255);
		case "yellow":
			return RGBCol(242, 248, 130);
		case "lt_yellow":
			return RGBCol(242, 245, 190);
		case "blue":
			return RGBCol(110, 218, 252);
		case "lt_orange":
			return RGBCol(255, 215, 148);
		default:
			return ParseRGBString(text);
		}
	}

	public void AddCachedGraphic(InventoryItem item, Texture2D tex)
	{
		int num = ((GraphicsControl.Instance.GraphicsLevel() > 4) ? 30 : 25);
		if (cached_model3d_graphics.Count > num)
		{
			List<InventoryItem> list = new List<InventoryItem>(cached_model3d_graphics.Keys);
			cached_model3d_graphics.Remove(list[UnityEngine.Random.Range(0, list.Count)]);
		}
		cached_model3d_graphics.Add(item, tex);
	}

	private void DetectParticles(screenshot_pair pair)
	{
		if (HasTag(pair.obj.transform, "ItemScreenshotParticle"))
		{
			pair.model_has_particles_by_default = true;
			return;
		}
		string text = ResourceControl.Instance.GetStringFromItemFile(pair.item_adjusted_for_rendering.item_name, "Copy3dModelPosition");
		if (text == "")
		{
			text = pair.item_adjusted_for_rendering.item_name;
		}
		if (ResourceControl.Instance.GetStringFromItemFile(text, "ParticleTransform0") != "" && pair.paint_str != "" && ResourceControl.Instance.GetColorScheme(pair.paint_str).GetParticleBool("enabled"))
		{
			pair.paint_has_particles_and_model_supports_them = true;
		}
	}

	private void ApplyOutlineGlowWithoutParticles(screenshot_pair pair)
	{
		ScreenshotWithEffects.Instance.ApplyGlowOutlineWithoutParticles(pair.glowCol);
	}

	private void ApplyOutlineGlowWithParticles(screenshot_pair pair)
	{
		ScreenshotWithEffects.Instance.ApplyGlowOutlineWithParticles(pair.glowCol);
	}

	private void Finalize(screenshot_pair pair)
	{
		Texture2D texture2D = ScreenshotWithEffects.Instance.FinalizeTexture();
		if (pair.itemSprite != null)
		{
			pair.itemSprite.Model3DScreenshotComplete(texture2D);
		}
		AddCachedGraphic(pair.item_unmodified_for_caching, texture2D);
	}

	private void ShowLights(screenshot_pair pair, ref Color col_before)
	{
		pair.obj.SetActive(true);
		item_screenshots_light.SetActive(true);
		col_before = RenderSettings.ambientLight;
		RenderSettings.ambientLight = item_screenshots_ambientCol;
	}

	private void HideLights(screenshot_pair pair, ref Color col_before)
	{
		pair.obj.SetActive(false);
		item_screenshots_light.SetActive(false);
		RenderSettings.ambientLight = col_before;
	}

	public void CancelScreenshotByItemSprite(ItemSprite itemSprite)
	{
		for (int i = 0; i < screenshot_queue.Count; i++)
		{
			if (screenshot_queue[i].itemSprite == itemSprite)
			{
				screenshot_queue[i].cancelled = true;
				break;
			}
		}
	}

	private bool HasTag(Transform T, string tag)
	{
		if (T.gameObject.tag == tag)
		{
			return true;
		}
		for (int i = 0; i < T.childCount; i++)
		{
			if (HasTag(T.GetChild(i), tag))
			{
				return true;
			}
		}
		return false;
	}

	private void SetVisibility(GameObject G, bool state)
	{
		MeshRenderer component = G.GetComponent<MeshRenderer>();
		if (component != null)
		{
			component.enabled = state;
		}
		ParticleSystemRenderer component2 = G.GetComponent<ParticleSystemRenderer>();
		if (component2 != null)
		{
			ParticleSystem component3 = G.GetComponent<ParticleSystem>();
			if (state)
			{
				component3.Emit(3);
				component2.enabled = true;
			}
			else
			{
				component3.Clear();
				component2.enabled = false;
			}
		}
	}

	private void SetVisibleByTag(Transform T, string tag, bool state)
	{
		if (T.gameObject.tag == tag)
		{
			SetVisibility(T.gameObject, state);
		}
		for (int i = 0; i < T.childCount; i++)
		{
			SetVisibleByTag(T.GetChild(i), tag, state);
		}
	}

	private void SetOtherVisibleByTag(Transform T, string tag, bool state, bool ignore_root)
	{
		if (!ignore_root && T.gameObject.tag != tag)
		{
			SetVisibility(T.gameObject, state);
		}
		for (int i = 0; i < T.childCount; i++)
		{
			SetOtherVisibleByTag(T.GetChild(i), tag, state, false);
		}
	}

	public bool ShouldCancel(screenshot_pair pair)
	{
		if (pair.cancelled)
		{
			return true;
		}
		return pair.itemSprite == null;
	}

	private Color RGBCol(int r, int g, int b)
	{
		return new Color((float)r / 255f, (float)g / 255f, (float)b / 255f, 1f);
	}

	private Color ParseRGBString(string col_str)
	{
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < col_str.Length; i++)
		{
			if (col_str[i] == ',')
			{
				if (num == -1)
				{
					num = i;
				}
				else if (num2 == -1)
				{
					num2 = i;
				}
			}
		}
		if (num2 == -1 || num == -1)
		{
			return Color.red;
		}
		int r = int.Parse(col_str.Substring(0, num), Startup.parse_culture);
		int g = int.Parse(col_str.Substring(num + 1, num2 - (num + 1)), Startup.parse_culture);
		int b = int.Parse(col_str.Substring(num2 + 1, col_str.Length - (num2 + 1)), Startup.parse_culture);
		return RGBCol(r, g, b);
	}
}
