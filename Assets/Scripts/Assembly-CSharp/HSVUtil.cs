using System;
using UnityEngine;

public static class HSVUtil
{
	public static HsvColor ConvertRgbToHsv(Color color)
	{
		return ConvertRgbToHsv((int)(color.r * 255f), (int)(color.g * 255f), (int)(color.b * 255f));
	}

	public static HsvColor ConvertRgbToHsv(double r, double b, double g)
	{
		double num = Math.Min(Math.Min(r, g), b);
		double num2 = Math.Max(Math.Max(r, g), b);
		double num3 = num2 - num;
		double num4 = ((num2 != 0.0) ? (num3 / num2) : 0.0);
		double num5;
		if (num4 == 0.0)
		{
			num5 = 360.0;
		}
		else
		{
			double num6 = ((num2 == r) ? ((g - b) / num3) : ((num2 == g) ? ((b - r) / num3 + 2.0) : ((num2 != b) ? 0.0 : ((r - g) / num3 + 4.0))));
			num5 = num6 * 60.0;
			if (!(num5 > 0.0))
			{
				num5 += 360.0;
			}
		}
		HsvColor result = default(HsvColor);
		result.H = 360.0 - num5;
		result.S = num4;
		result.V = num2 / 255.0;
		return result;
	}

	public static Color ConvertHsvToRgb(double h, double s, double v, float alpha)
	{
		double num;
		double num2;
		double num3;
		if (s == 0.0)
		{
			num = v;
			num2 = v;
			num3 = v;
		}
		else
		{
			double num4 = ((h != 360.0) ? (h / 60.0) : 0.0);
			int num5 = (int)num4;
			double num6 = num4 - (double)num5;
			double num7 = (1.0 - s) * v;
			double num8 = (1.0 - num6 * s) * v;
			double num9 = (1.0 - (1.0 - num6) * s) * v;
			switch (num5)
			{
			case 0:
				num = v;
				num2 = num9;
				num3 = num7;
				break;
			case 1:
				num = num8;
				num2 = v;
				num3 = num7;
				break;
			case 2:
				num = num7;
				num2 = v;
				num3 = num9;
				break;
			case 3:
				num = num7;
				num2 = num8;
				num3 = v;
				break;
			case 4:
				num = num9;
				num2 = num7;
				num3 = v;
				break;
			default:
				num = v;
				num2 = num7;
				num3 = num8;
				break;
			}
		}
		return new Color((float)num, (float)num2, (float)num3, alpha);
	}
}
