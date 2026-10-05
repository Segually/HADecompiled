using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GrandConverter : MonoBehaviour
{
	public static GrandConverter Instance;

	private ConverterAntihack converterAntihack;

	private ConverterPerks converterPerks;

	private ConverterWeapons converterWeapons;

	private ConverterWeapons2 converterWeapons2;

	private ConverterArmor converterArmor;

	public GameObject notif_screen;

	public GameObject progress_bar_screen;

	public Text progress_text;

	public Text header_text;

	public Text filename_text;

	public Image fill_bar;

	private void Start()
	{
	}

	private IEnumerator BeginConverting()
	{
		return null;
	}

	public void PressOkay()
	{
	}

	public void DrawProgressBar(float p)
	{
	}

	public void SetProgressText(string text)
	{
	}
}
