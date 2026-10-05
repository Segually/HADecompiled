using UnityEngine;

public class GraphicsControl : MonoBehaviour, OrderedStart
{
	public static GraphicsControl Instance;

	public bool initial_pixel_cap;

	public void Start_0()
	{
		Instance = this;
	}

	public void Start_1()
	{
	}

	public int GraphicsLevel()
	{
		return PlayerPrefs.GetInt("GraphicsLevel");
	}

	public int SpecialAnimationChoppiness()
	{
		int num = GraphicsLevel();
		if (num < 5)
		{
			if (num < 3)
			{
				return 2;
			}
			return 1;
		}
		return 1;
	}

	public int DefaultAnimationChoppiness()
	{
		int num = GraphicsLevel();
		if (num < 5)
		{
			if (num < 3)
			{
				return 3;
			}
			return 2;
		}
		return 1;
	}

	public bool ShowLightingEffects()
	{
		return GraphicsLevel() > 3;
	}

	public int HowMuchGrassToSpawn(int regular_num)
	{
		if (regular_num == 0)
		{
			return 0;
		}
		int num = GraphicsLevel();
		int num2 = regular_num;
		if (num < 4)
		{
			num2 = ((num != 3) ? ((int)((float)regular_num * 0.333f)) : ((int)((float)regular_num * 0.666f)));
		}
		if (num2 == 0)
		{
			num2 = 1;
		}
		return num2;
	}

	public int GetNumFloorParticles(ModularObjectControl.segment segment)
	{
		int num = GraphicsLevel();
		if (num >= 4)
		{
			if (segment < ModularObjectControl.segment.topLeft_corner)
			{
				return 2;
			}
			if (segment <= ModularObjectControl.segment.bottomRight_corner)
			{
				return 1;
			}
			return 3;
		}
		if (num == 3)
		{
			if (segment < ModularObjectControl.segment.topLeft_corner)
			{
				return 1;
			}
			if (segment <= ModularObjectControl.segment.bottomRight_corner)
			{
				return 0;
			}
			return 2;
		}
		return 0;
	}

	public bool ShowNonPlayerSplashes()
	{
		return GraphicsLevel() > 3;
	}

	public bool ShouldDestroyParticle(QualityDestroyer.level _level)
	{
		switch (_level)
		{
		case QualityDestroyer.level._100_percent:
			return GraphicsLevel() < 4;
		case QualityDestroyer.level._66_percent:
			return GraphicsLevel() < 3;
		default:
			return false;
		}
	}

	public float GetResolutionMod()
	{
		switch (GraphicsLevel())
		{
		case 1:
			return 0.65f;
		case 2:
			return 0.75f;
		case 3:
			return 0.85f;
		default:
			return 1f;
		}
	}

	public void CapPixels()
	{
		QualitySettings.resolutionScalingFixedDPIFactor = GetResolutionMod();
	}
}
