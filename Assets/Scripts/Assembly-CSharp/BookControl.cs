using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookControl : MonoBehaviour
{
	public static BookControl Instance;

	public InventoryItem book_reading = new InventoryItem("");

	private Dictionary<string, string> dev_book_data = new Dictionary<string, string>();

	public TextMeshProUGUI book_title_text;

	public TextMeshProUGUI page_L_text;

	public TextMeshProUGUI page_R_text;

	public CanvasGroup button_next_page;

	public CanvasGroup button_prev_page;

	public GameObject background_book;

	public RawImage book_title_symbol;

	public AudioClip sfx_open;

	public AudioClip sfx_pageturn;

	public Image book_img_base;

	public Image book_img_overlay;

	private float minLightness = 0.25f;

	private float maxLightness = 0.75f;

	private float minContrast = 0.4f;

	private Color backgroundColor;

	private Color textColor;

	public GameObject page_L;

	public GameObject page_Title;

	public int curr_page;

	public static Dictionary<string, string> LoadDevBook(string book_title)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		bool file_exists = false;
		List<string> textFileLines = ResourceControl.Instance.GetTextFileLines("Books/" + book_title, ref file_exists);
		if (!file_exists)
		{
			return dictionary;
		}
		List<string> list = new List<string>();
		string text = null;
		foreach (string item in textFileLines)
		{
			if (string.IsNullOrWhiteSpace(item))
			{
				continue;
			}
			if (item.StartsWith("[") && item.EndsWith("]"))
			{
				if (text != null)
				{
					dictionary[text] = string.Join(" ", list);
				}
				text = item.Trim('[', ']');
				list.Clear();
			}
			else
			{
				list.Add(item);
			}
		}
		if (text != null)
		{
			dictionary[text] = string.Join(" ", list);
		}
		return dictionary;
	}

	public void InitialDraw()
	{
		AudioControl.Instance.Play(sfx_open);
		if (!string.IsNullOrWhiteSpace(book_reading.GetString("dev_book_title")))
		{
			dev_book_data = LoadDevBook(book_reading.GetString("dev_book_title"));
			if (GetBookLanguage() == "English")
			{
				book_title_text.text = book_reading.GetString("dev_book_title");
			}
			else if (dev_book_data.ContainsKey("book name - " + GetBookLanguage()))
			{
				book_title_text.text = dev_book_data["book name - " + GetBookLanguage()];
			}
			else
			{
				book_title_text.text = book_reading.GetString("dev_book_title");
			}
		}
		else
		{
			book_title_text.text = book_reading.GetString("book_title");
		}
		ColorScheme colorScheme = ResourceControl.Instance.GetColorScheme(book_reading.GetString("paint"));
		string stringFromItemFile = ResourceControl.Instance.GetStringFromItemFile("Book", "PaintLayout0_" + colorScheme.GetLayout());
		backgroundColor = colorScheme.GetColor("Col" + GetMappedPaintValue(0, stringFromItemFile), "color");
		textColor = colorScheme.GetColor("Col" + GetMappedPaintValue(1, stringFromItemFile), "color");
		AdjustColorsForContrast();
		book_img_base.color = backgroundColor;
		book_img_overlay.color = textColor;
		page_L_text.color = textColor;
		page_R_text.color = textColor;
		book_title_text.color = textColor;
		book_title_symbol.color = textColor;
		book_title_symbol.texture = ResourceControl.Instance.GetSymbol(book_reading.GetString("stamp"));
		Color color = colorScheme.GetColor("Col" + GetMappedPaintValue(2, stringFromItemFile), "color");
		button_prev_page.gameObject.GetComponent<Image>().color = color;
		button_next_page.gameObject.GetComponent<Image>().color = color;
	}

	private static int GetMappedPaintValue(int input, string mappingString)
	{
		string[] array = mappingString.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(new char[1] { '-' }, StringSplitOptions.RemoveEmptyEntries);
			if (array2.Length == 2)
			{
				int num = int.Parse(array2[0].Trim());
				int result = int.Parse(array2[1].Trim());
				if (num == input)
				{
					return result;
				}
			}
		}
		return 0;
	}

	private void AdjustColorsForContrast()
	{
		ColorToHSL(backgroundColor, out var h, out var s, out var l);
		ColorToHSL(textColor, out var h2, out var s2, out var l2);
		if (Mathf.Abs(l - l2) >= minContrast)
		{
			return;
		}
		float num = l;
		float num2 = l2;
		if (l > l2)
		{
			l2 = Mathf.Clamp(l - minContrast, minLightness, l2);
			float num3 = minContrast - (l - l2);
			if (num3 > 0f)
			{
				l = Mathf.Clamp(l + num3, l, maxLightness);
			}
		}
		else
		{
			l2 = Mathf.Clamp(l + minContrast, l2, maxLightness);
			float num4 = minContrast - (l2 - l);
			if (num4 > 0f)
			{
				l = Mathf.Clamp(l - num4, minLightness, l);
			}
		}
		if (Mathf.Abs(l - l2) < minContrast)
		{
			l2 = ((!(num > num2)) ? Mathf.Clamp(l + minContrast, l2, maxLightness) : Mathf.Clamp(l - minContrast, minLightness, l2));
		}
		backgroundColor = HSLToColor(h, s, l);
		textColor = HSLToColor(h2, s2, l2);
	}

	public void RedrawPages()
	{
		if (!string.IsNullOrWhiteSpace(book_reading.GetString("dev_book_title")))
		{
			if (curr_page == 0)
			{
				page_L.SetActive(false);
				page_Title.SetActive(true);
			}
			else
			{
				page_L.SetActive(true);
				page_Title.SetActive(false);
				if (dev_book_data.ContainsKey("page " + curr_page + " - " + GetBookLanguage()))
				{
					page_L_text.text = dev_book_data["page " + curr_page + " - " + GetBookLanguage()];
				}
				else
				{
					page_L_text.text = "";
				}
			}
			if (dev_book_data.ContainsKey("page " + (curr_page + 1) + " - " + GetBookLanguage()))
			{
				page_R_text.text = dev_book_data["page " + (curr_page + 1) + " - " + GetBookLanguage()];
			}
			else
			{
				page_R_text.text = "";
			}
		}
		else
		{
			page_L_text.text = book_reading.GetString("book_text_page_" + curr_page);
			page_R_text.text = book_reading.GetString("book_text_page_" + (curr_page + 1));
		}
		button_prev_page.alpha = ((curr_page > 0) ? 1f : 0.333f);
		button_next_page.alpha = (HasPage(curr_page + 2) ? 1f : 0.333f);
		background_book.transform.localScale = new Vector3(0f - background_book.transform.localScale.x, background_book.transform.localScale.y, background_book.transform.localScale.z);
	}

	public void ClickPrevPage()
	{
		if (curr_page > 0)
		{
			curr_page -= 2;
			RedrawPages();
			AudioControl.Instance.Play(sfx_pageturn);
		}
	}

	public void ClickNextPage()
	{
		if (HasPage(curr_page + 2))
		{
			curr_page += 2;
			RedrawPages();
			AudioControl.Instance.Play(sfx_pageturn);
		}
	}

	public static string GetBookLanguage()
	{
		switch (TranslationControl.Instance.use_language)
		{
		case TranslationControl.languages.Russian:
			return "Russian";
		case TranslationControl.languages.Portuguese:
			return "Portuguese";
		case TranslationControl.languages.Indonesian:
			return "Indonesian";
		case TranslationControl.languages.Spanish:
			return "Spanish";
		case TranslationControl.languages.Thai:
			return "Thai";
		default:
			return "English";
		}
	}

	private bool HasPage(int hypothetical_page)
	{
		if (!string.IsNullOrWhiteSpace(book_reading.GetString("dev_book_title")))
		{
			if (dev_book_data.ContainsKey("page " + hypothetical_page + " - " + GetBookLanguage()))
			{
				return !string.IsNullOrWhiteSpace(dev_book_data["page " + hypothetical_page + " - " + GetBookLanguage()]);
			}
			return false;
		}
		return !string.IsNullOrWhiteSpace(book_reading.GetString("book_text_page_" + hypothetical_page));
	}

	private Color HSLToColor(float h, float s, float l)
	{
		float r;
		float g;
		float b;
		if (s == 0f)
		{
			r = l;
			g = l;
			b = l;
		}
		else
		{
			float num = ((l < 0.5f) ? (l * (1f + s)) : (l + s - l * s));
			float p = 2f * l - num;
			r = HueToRGB(p, num, h + 1f / 3f);
			g = HueToRGB(p, num, h);
			b = HueToRGB(p, num, h - 1f / 3f);
		}
		return new Color(r, g, b, 1f);
	}

	private float HueToRGB(float p, float q, float t)
	{
		if (t < 0f)
		{
			t += 1f;
		}
		if (t > 1f)
		{
			t -= 1f;
		}
		if (t < 1f / 6f)
		{
			return p + (q - p) * 6f * t;
		}
		if (t < 0.5f)
		{
			return q;
		}
		if (t < 2f / 3f)
		{
			return p + (q - p) * (2f / 3f - t) * 6f;
		}
		return p;
	}

	private void ColorToHSL(Color color, out float h, out float s, out float l)
	{
		float num = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
		float num2 = Mathf.Min(color.r, Mathf.Min(color.g, color.b));
		h = 0f;
		s = 0f;
		l = (num + num2) / 2f;
		if (Mathf.Approximately(num, num2))
		{
			h = 0f;
			s = 0f;
			return;
		}
		float num3 = num - num2;
		s = ((l > 0.5f) ? (num3 / (2f - num - num2)) : (num3 / (num + num2)));
		if (Mathf.Approximately(num, color.r))
		{
			h = (color.g - color.b) / num3 + ((color.g < color.b) ? 6f : 0f);
		}
		else if (Mathf.Approximately(num, color.g))
		{
			h = (color.b - color.r) / num3 + 2f;
		}
		else
		{
			h = (color.r - color.g) / num3 + 4f;
		}
		h /= 6f;
	}
}
