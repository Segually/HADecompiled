using System.Collections.Generic;
using UnityEngine;

public class ColorScheme
{
	public Dictionary<string, Dictionary<string, object>> data = new Dictionary<string, Dictionary<string, object>>();

	public string GetLayout()
	{
		if (data.ContainsKey("Layout") && data["Layout"].ContainsKey("value"))
		{
			return (string)data["Layout"]["value"];
		}
		return "None";
	}

	public string GetGlowCol()
	{
		if (data.ContainsKey("Layout") && data["Layout"].ContainsKey("glowCol"))
		{
			return (string)data["Layout"]["glowCol"];
		}
		return "";
	}

	public void SetSwatchData(string swatch_name, Dictionary<string, object> swatch_data)
	{
		if (data.ContainsKey(swatch_name))
		{
			data[swatch_name] = swatch_data;
		}
		else
		{
			data.Add(swatch_name, swatch_data);
		}
	}

	public string GetSwatchType(string swatch_name, string default_val)
	{
		if (data.ContainsKey(swatch_name) && data[swatch_name].ContainsKey("type"))
		{
			return (string)data[swatch_name]["type"];
		}
		return default_val;
	}

	public string GetSymbolType(string pattern_name)
	{
		if (data.ContainsKey(pattern_name) && data[pattern_name].ContainsKey("type"))
		{
			return (string)data[pattern_name]["type"];
		}
		return "Disabled";
	}

	public Color GetColor(string swatch_name, string color_name)
	{
		if (data.ContainsKey(swatch_name) && data[swatch_name].ContainsKey(color_name))
		{
			return (Color)data[swatch_name][color_name];
		}
		return default(Color);
	}

	public string GetTexture(string swatch_name, string subtexture_name)
	{
		if (data.ContainsKey(swatch_name) && data[swatch_name].ContainsKey(subtexture_name))
		{
			return (string)data[swatch_name][subtexture_name];
		}
		return "";
	}

	public bool GetParticleBool(string key)
	{
		if (data.ContainsKey("Particles") && data["Particles"].ContainsKey(key))
		{
			return bool.Parse((string)data["Particles"][key]);
		}
		return false;
	}

	public float GetParticleFloat(string key)
	{
		if (data.ContainsKey("Particles") && data["Particles"].ContainsKey(key))
		{
			return float.Parse((string)data["Particles"][key], Startup.parse_culture);
		}
		return 0f;
	}

	public string GetParticleString(string key)
	{
		if (data.ContainsKey("Particles") && data["Particles"].ContainsKey(key))
		{
			return (string)data["Particles"][key];
		}
		return "";
	}

	public Color GetParticleColor(string key)
	{
		if (data.ContainsKey("Particles") && data["Particles"].ContainsKey(key))
		{
			return (Color)data["Particles"][key];
		}
		return default(Color);
	}

	public AnimationCurve GetParticleCurve(string key)
	{
		if (data.ContainsKey("Particles") && data["Particles"].ContainsKey(key))
		{
			return (AnimationCurve)data["Particles"][key];
		}
		return new AnimationCurve();
	}

	public Gradient GetParticleGradient(string key)
	{
		if (data.ContainsKey("Particles") && data["Particles"].ContainsKey(key))
		{
			return (Gradient)data["Particles"][key];
		}
		return new Gradient();
	}
}
