using System;
using System.Collections.Generic;
using UnityEngine;

public class PaintableObject : MonoBehaviour
{
	[Serializable]
	public struct target_mesh
	{
		public MeshRenderer renderer;

		public Texture2D details_tex;

		public int[] paint_material;

		public int render_queue;
	}

	public static string DebugPaint;

	public bool is_interior_model;

	public target_mesh[] target_meshes;

	public void Colorize(string paint_name, string stamp, string layout_item, string actual_item, bool process_particles = false)
	{
		Colorize(paint_name, stamp, layout_item, actual_item, 1f, process_particles);
	}

	public void Colorize(string paint_name, string stamp, string layout_item, string actual_item, float particle_scale, bool process_particles = false)
	{
		ColorScheme colorScheme = ResourceControl.Instance.GetColorScheme(paint_name);
		LoadLayoutData(colorScheme.GetLayout(), layout_item);
		if (process_particles)
		{
			int num = 0;
			string stringFromItemFile = ResourceControl.Instance.GetStringFromItemFile(layout_item, "ParticleTransform" + num);
			while (stringFromItemFile != "")
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(ResourceControl.Instance.prefab_weapon_particle_system);
				gameObject.transform.SetParent(base.transform);
				Vector3 local_position = Vector3.zero;
				Quaternion local_rotation = Quaternion.identity;
				Vector3 local_scale = Vector3.one;
				string shape_type = "";
				Dictionary<string, string> shape_data = new Dictionary<string, string>();
				ResourceControl.DeserializeParticleTransform(stringFromItemFile, ref local_position, ref local_rotation, ref local_scale, ref shape_type, ref shape_data);
				gameObject.transform.localPosition = local_position;
				gameObject.transform.localRotation = local_rotation;
				gameObject.transform.localScale = local_scale;
				if (!colorScheme.GetParticleBool("enabled"))
				{
					gameObject.SetActive(false);
				}
				else
				{
					gameObject.SetActive(true);
					ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
					if (shape_type == "CONE")
					{
						ParticleSystem.ShapeModule shape = component.shape;
						shape.shapeType = ParticleSystemShapeType.Cone;
						shape.length = float.Parse(shape_data["shape_CONE_length"], Startup.parse_culture);
						shape.radius = float.Parse(shape_data["shape_CONE_radius"], Startup.parse_culture);
					}
					component.startSpeed = colorScheme.GetParticleFloat("start_speed");
					component.startSize = colorScheme.GetParticleFloat("start_size") * particle_scale;
					component.startLifetime = colorScheme.GetParticleFloat("start_lifetime");
					component.startColor = colorScheme.GetParticleColor("start_col");
					component.emissionRate = colorScheme.GetParticleFloat("emission_over_time");
					ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = component.sizeOverLifetime;
					if (colorScheme.GetParticleBool("size_over_lifetime_enabled"))
					{
						sizeOverLifetime.enabled = true;
						sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, colorScheme.GetParticleCurve("size_over_lifetime_curve"));
					}
					else
					{
						sizeOverLifetime.enabled = false;
					}
					ParticleSystem.ColorOverLifetimeModule colorOverLifetime = component.colorOverLifetime;
					if (colorScheme.GetParticleBool("color_over_lifetime_enabled"))
					{
						colorOverLifetime.enabled = true;
						colorOverLifetime.color = new ParticleSystem.MinMaxGradient(colorScheme.GetParticleGradient("color_over_lifetime_gradient"));
					}
					else
					{
						colorOverLifetime.enabled = false;
					}
					Material material = UnityEngine.Object.Instantiate(ResourceControl.Instance.mat_weapon_particle);
					material.mainTexture = ResourceControl.Instance.GetParticleTexture(colorScheme.GetTexture("Particle Texture", "pattern_tex"));
					ParticleSystemRenderer component2 = component.GetComponent<ParticleSystemRenderer>();
					component2.material = material;
					component.gravityModifier = colorScheme.GetParticleFloat("gravity_modifier");
					string particleString = colorScheme.GetParticleString("render_mode");
					component2.renderMode = ((!(particleString == "billboard")) ? ((particleString == "stretched billboard") ? ParticleSystemRenderMode.Stretch : ParticleSystemRenderMode.Billboard) : ParticleSystemRenderMode.Billboard);
					ParticleSystem.MainModule main = component.main;
					main.prewarm = true;
				}
				num++;
				stringFromItemFile = ResourceControl.Instance.GetStringFromItemFile(layout_item, "ParticleTransform" + num);
			}
		}
		for (int i = 0; i < target_meshes.Length; i++)
		{
			ColorizeMesh(target_meshes[i].renderer, target_meshes[i].details_tex, colorScheme, i, stamp, layout_item, actual_item);
		}
	}

	private void LoadLayoutData(string layout_name, string layout_item)
	{
		for (int i = 0; i < target_meshes.Length; i++)
		{
			Material[] materials = target_meshes[i].renderer.materials;
			target_meshes[i].paint_material = new int[materials.Length];
			for (int j = 0; j < materials.Length; j++)
			{
				target_meshes[i].paint_material[j] = 0;
			}
			target_meshes[i].render_queue = -1;
		}
		if (!(layout_name != "None"))
		{
			return;
		}
		for (int k = 0; k < target_meshes.Length; k++)
		{
			string text = "PaintLayout" + (is_interior_model ? "INTERIOR" : "") + k;
			string stringFromItemFile = ResourceControl.Instance.GetStringFromItemFile(layout_item, text + "_" + layout_name);
			if (!Startup.StringNullOrWhitespace(stringFromItemFile))
			{
				int num = 0;
				int num2 = -1;
				for (int l = 0; l < stringFromItemFile.Length; l++)
				{
					char c = stringFromItemFile[l];
					if (c == ',')
					{
						int num3 = int.Parse(stringFromItemFile.Substring(num, l - num), Startup.parse_culture);
						target_meshes[k].paint_material[num2] = num3;
						num = l + 2;
					}
					else if (c == '-')
					{
						num2 = int.Parse(stringFromItemFile.Substring(num, l - num), Startup.parse_culture);
						num = l + 1;
					}
				}
			}
			int intFromItemFile = ResourceControl.Instance.GetIntFromItemFile(layout_item, text + "_RENDERQUEUE");
			if (intFromItemFile != 0)
			{
				target_meshes[k].render_queue = intFromItemFile;
			}
		}
	}

	private void ColorizeMesh(MeshRenderer mesh_renderer, Texture2D tex, ColorScheme scheme, int mesh_index, string stamp, string layout_item, string actual_item)
	{
		Material[] materials = mesh_renderer.materials;
		int[] paint_material = target_meshes[mesh_index].paint_material;
		int render_queue = target_meshes[mesh_index].render_queue;
		for (int i = 0; i < paint_material.Length; i++)
		{
			string text = "None";
			if (paint_material[i] != 0)
			{
				foreach (KeyValuePair<string, Dictionary<string, object>> datum in scheme.data)
				{
					if (datum.Value.ContainsKey("palette_index") && int.Parse((string)datum.Value["palette_index"], Startup.parse_culture) == paint_material[i] - 1)
					{
						text = datum.Key;
						break;
					}
				}
			}
			string swatchType = scheme.GetSwatchType(text, "Solid");
			string symbolType = scheme.GetSymbolType("Side Pattern");
			if ((actual_item == "Armor Display" || actual_item == "Custom Statue") && mesh_index == 1)
			{
				materials[i] = UnityEngine.Object.Instantiate(inventory_ctr.Instance.mobile_diffuse_colorONLY);
				materials[i].color = scheme.GetColor(text, "color");
				continue;
			}
			materials[i] = UnityEngine.Object.Instantiate((actual_item == "Lava") ? ResourceControl.Instance.mat_paint_advanced_no_light : ResourceControl.Instance.mat_paint_advanced);
			materials[i].SetTexture("_MainTex", (tex != null) ? tex : ResourceControl.Instance.white_tex);
			materials[i].SetColor("_ColorA", (text == "None") ? default(Color) : scheme.GetColor(text, "color"));
			if (swatchType == "Pattern")
			{
				materials[i].SetTexture("_PatternTex", ResourceControl.Instance.GetFullPattern(scheme.GetTexture(text, "pattern_tex")));
				materials[i].SetColor("_ColorB", scheme.GetColor(text, "colorB"));
			}
			if (symbolType == "Enabled")
			{
				materials[i].SetTexture("_SideTex", ResourceControl.Instance.GetSidePattern(scheme.GetTexture("Side Pattern", "pattern_tex")));
				materials[i].SetColor("_ColorC", scheme.GetColor(text, "col_side"));
			}
			if (!Startup.StringNullOrWhitespace(stamp))
			{
				materials[i].SetTexture("_SymbolTex", ResourceControl.Instance.GetSymbol(stamp));
				materials[i].SetColor("_ColorD", scheme.GetColor(text, "col_symbol"));
			}
			if (render_queue != -1)
			{
				materials[i].SetFloat("_ZTest", 8f);
				materials[i].renderQueue = render_queue;
			}
		}
		mesh_renderer.materials = materials;
	}
}
