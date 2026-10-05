using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookControl : MonoBehaviour
{
	public static BookControl Instance;

	public InventoryItem book_reading;

	private Dictionary<string, string> dev_book_data;

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

	private float minLightness;

	private float maxLightness;

	private float minContrast;

	private Color backgroundColor;

	private Color textColor;

	public GameObject page_L;

	public GameObject page_Title;

	public int curr_page;

	public static Dictionary<string, string> LoadDevBook(string book_title)
	{
		return null;
	}

	public void InitialDraw()
	{
	}

	private static int GetMappedPaintValue(int input, string mappingString)
	{
		return 0;
	}

	private void AdjustColorsForContrast()
	{
	}

	public void RedrawPages()
	{
	}

	public void ClickPrevPage()
	{
	}

	public void ClickNextPage()
	{
	}

	public static string GetBookLanguage()
	{
		return null;
	}

	private bool HasPage(int hypothetical_page)
	{
		return false;
	}

	private Color HSLToColor(float h, float s, float l)
	{
		return default(Color);
	}

	private float HueToRGB(float p, float q, float t)
	{
		return 0f;
	}

	private void ColorToHSL(Color color, out float h, out float s, out float l)
	{
		h = default(float);
		s = default(float);
		l = default(float);
	}
}
