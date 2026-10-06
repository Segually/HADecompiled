using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
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
		Instance = this;
		notif_screen.SetActive(true);
		progress_bar_screen.SetActive(false);
		converterAntihack = GetComponent<ConverterAntihack>();
		converterPerks = GetComponent<ConverterPerks>();
		converterWeapons = GetComponent<ConverterWeapons>();
		converterWeapons2 = GetComponent<ConverterWeapons2>();
		converterArmor = GetComponent<ConverterArmor>();
	}

	private IEnumerator BeginConverting()
	{
		header_text.text = "Installing";
		DrawProgressBar(0.05f);
		yield return new WaitForSeconds(0.25f);
		string path = Path.Combine(Startup.persistentDataPath, "conversion_progress.txt");
		if (File.Exists(path))
		{
			string[] array = File.ReadAllLines(path);
			if (array.Length != 0)
			{
				if (array[0] == "complete11")
				{
					converterAntihack.complete = true;
				}
				else if (array[0] == "complete12")
				{
					converterAntihack.complete = true;
					converterPerks.complete = true;
				}
				else if (array[0] == "complete13")
				{
					converterAntihack.complete = true;
					converterPerks.complete = true;
					converterWeapons.complete = true;
				}
				else if (array[0] == "complete14")
				{
					converterAntihack.complete = true;
					converterPerks.complete = true;
					converterWeapons.complete = true;
					converterWeapons2.complete = true;
				}
				else if (array[0] == "complete15")
				{
					converterAntihack.complete = true;
					converterPerks.complete = true;
					converterWeapons.complete = true;
					converterWeapons2.complete = true;
					converterArmor.complete = true;
				}
			}
		}
		if (!converterAntihack.complete)
		{
			header_text.text = "Installing 1/5";
			converterAntihack.complete = false;
			converterAntihack.StartCoroutine(converterAntihack.BeginConverting());
			while (!converterAntihack.complete)
			{
				yield return new WaitForSeconds(0.25f);
			}
			File.WriteAllLines(Path.Combine(Startup.persistentDataPath, "conversion_progress.txt"), new string[1] { "complete11" });
		}
		if (!converterPerks.complete)
		{
			header_text.text = "Installing 2/5";
			converterPerks.complete = false;
			converterPerks.StartCoroutine(converterPerks.BeginConverting());
			while (!converterPerks.complete)
			{
				yield return new WaitForSeconds(0.25f);
			}
			File.WriteAllLines(Path.Combine(Startup.persistentDataPath, "conversion_progress.txt"), new string[1] { "complete12" });
		}
		if (!converterWeapons.complete)
		{
			header_text.text = "Installing 3/5";
			converterWeapons.complete = false;
			converterWeapons.StartCoroutine(converterWeapons.BeginConverting());
			while (!converterWeapons.complete)
			{
				yield return new WaitForSeconds(0.25f);
			}
			File.WriteAllLines(Path.Combine(Startup.persistentDataPath, "conversion_progress.txt"), new string[1] { "complete13" });
		}
		if (!converterWeapons2.complete)
		{
			header_text.text = "Installing 4/5";
			converterWeapons2.complete = false;
			converterWeapons2.StartCoroutine(converterWeapons2.BeginConverting());
			while (!converterWeapons2.complete)
			{
				yield return new WaitForSeconds(0.25f);
			}
			File.WriteAllLines(Path.Combine(Startup.persistentDataPath, "conversion_progress.txt"), new string[1] { "complete14" });
		}
		if (!converterArmor.complete)
		{
			header_text.text = "Installing 5/5";
			converterArmor.complete = false;
			converterArmor.StartCoroutine(converterArmor.BeginConverting());
			while (!converterArmor.complete)
			{
				yield return new WaitForSeconds(0.25f);
			}
			File.WriteAllLines(Path.Combine(Startup.persistentDataPath, "conversion_progress.txt"), new string[1] { "complete15" });
		}
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Menu");
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}

	public void PressOkay()
	{
		notif_screen.SetActive(false);
		progress_bar_screen.SetActive(true);
		StartCoroutine(BeginConverting());
	}

	public void DrawProgressBar(float p)
	{
		float x = ((RectTransform)fill_bar.transform.parent).sizeDelta.x;
		fill_bar.rectTransform.sizeDelta = new Vector2(x * p, fill_bar.rectTransform.sizeDelta.y);
		fill_bar.rectTransform.anchoredPosition = new Vector2(x * p * 0.5f, fill_bar.rectTransform.anchoredPosition.y);
		progress_text.text = "Progress " + (int)(p * 100f) + "%";
	}

	public void SetProgressText(string text)
	{
		progress_text.text = text;
	}
}
