using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaintingControl : MonoBehaviour
{
	private struct undo_canvas_struct
	{
		public Image image;

		public Texture2D tex;
	}

	private enum curr_screen_t
	{
		painting = 0,
		color_customize = 1,
		save = 2,
		pick_on_LOAD = 3,
		pick_on_SAVE = 4
	}

	private struct zoom_undo
	{
		public Vector2 local_pos;

		public Vector2 local_scale;
	}

	private struct partially_drawn_segment
	{
		public int x;

		public int y;

		public Color[] cols;
	}

	private struct segment_to_paint_on
	{
		public int x;

		public int y;
	}

	public static PaintingControl Instance;

	private Texture2D pen_tex;

	public Image mask;

	public Image base_canvas_img;

	public Image onion_skins_canvas;

	private Texture2D base_canvas_1_tex;

	private Texture2D base_canvas_2_tex;

	private List<undo_canvas_struct> undo_canvases = new List<undo_canvas_struct>();

	public Transform undo_canvases_parent;

	public Transform workspace_segments_parent;

	private List<zoom_undo> zoom_undos = new List<zoom_undo>();

	public Image[,] segment_images;

	public Texture2D[,] segment_textures;

	public static int n_horizontal_segs = 4;

	public static int n_vertical_segs = 4;

	private float[] stroke_fills = new float[5] { 0.75f, 0.75f, 2f, 2.5f, 3f };

	private int n_undos = 5;

	public Image onion_skins_button;

	private bool onion_skins_enabled;

	private int curr_frame = 1;

	public Image save_screen_preview;

	public GameObject speed_slider;

	private bool preview_frame_2;

	private IEnumerator animated_preview;

	private Color col_frame_button_selected;

	private Color col_frame_button_deselected;

	public Image frame_1_button;

	public Image frame_2_button;

	public Text speed_text;

	private int speed = 3;

	public InputField input_save_title;

	public InputField input_save_description;

	private bool dev_frame_1_loaded;

	private bool dev_frame_2_loaded;

	public bool save_screen_open;

	public bool load_screen_open;

	private string loaded_painting_title = "";

	private string loaded_painting_desc = "";

	private Color paint_color = new Color(0f, 0f, 0f, 1f);

	private bool allow_input;

	public GameObject[] hardcoded_color_swatches;

	public GameObject[] custom_color_swatches;

	private List<Color> custom_colors = new List<Color>();

	public GameObject color_selector;

	private GameObject prev_selected_color_nib;

	private curr_screen_t curr_screen;

	public GameObject painting_window;

	public GameObject custom_color_window;

	public GameObject save_window;

	private float slider_value_H;

	private float slider_value_S;

	private float slider_value_V;

	public GameObject[] tool_buttons;

	public GameObject size_selector;

	private float stroke_fill;

	private int prev_selected_tool_id = -1;

	private float prev_mouse_percent_x;

	private float prev_mouse_percent_y;

	private float curr_mouse_percent_x;

	private float curr_mouse_percent_y;

	private bool draw_stroke;

	private bool create_undo_canvas;

	public GameObject zoom_marker;

	public Text zoom_Text;

	public RectTransform hue_slider;

	public RectTransform saturation_value_slider;

	public Image custom_col_swatch;

	private bool is_clicking_hue_slider;

	private bool is_clicking_satval_slider;

	public GameObject zoom_parent;

	private float zoom_amount = 1.5f;

	public void PressOnionSkin()
	{
		onion_skins_enabled = !onion_skins_enabled;
		if (onion_skins_enabled)
		{
			onion_skins_canvas.gameObject.SetActive(true);
			onion_skins_button.color = col_frame_button_selected;
		}
		else
		{
			onion_skins_canvas.gameObject.SetActive(false);
			onion_skins_button.color = col_frame_button_deselected;
		}
	}

	public void PressFrame(int index)
	{
		switch (index)
		{
		case 1:
			if (curr_frame != 1)
			{
				frame_1_button.color = col_frame_button_selected;
				frame_2_button.color = col_frame_button_deselected;
				MergeAllUndoCanvases();
				Sprite sprite2 = base_canvas_img.sprite;
				base_canvas_img.sprite = onion_skins_canvas.sprite;
				onion_skins_canvas.sprite = sprite2;
				curr_frame = 1;
			}
			break;
		case 2:
			if (curr_frame != 2)
			{
				frame_1_button.color = col_frame_button_deselected;
				frame_2_button.color = col_frame_button_selected;
				MergeAllUndoCanvases();
				if (base_canvas_2_tex == null)
				{
					Color[] array = new Color[n_horizontal_segs * n_vertical_segs * 1024];
					for (int i = 0; i < array.Length; i++)
					{
						array[i] = new Color(1f, 1f, 1f, 1f);
					}
					base_canvas_2_tex = new Texture2D(n_horizontal_segs * 32, n_vertical_segs * 32);
					base_canvas_2_tex.filterMode = FilterMode.Point;
					base_canvas_2_tex.SetPixels(array);
					base_canvas_2_tex.Apply();
					onion_skins_canvas.sprite = Sprite.Create(base_canvas_2_tex, new Rect(0f, 0f, n_horizontal_segs * 32, n_vertical_segs * 32), Vector2.zero);
				}
				Sprite sprite = base_canvas_img.sprite;
				base_canvas_img.sprite = onion_skins_canvas.sprite;
				onion_skins_canvas.sprite = sprite;
				curr_frame = 2;
			}
			break;
		}
	}

	public void PressNew()
	{
		PopupControl.Instance.on_yes_pressed = delegate
		{
			Instance.ClearAll();
		};
		PopupControl.Instance.ShowYesNo("Are you sure you want to delete this drawing?", "Yes", "Cancel", PopupControl.context.yesno_ACTION);
	}

	public void PressSave()
	{
		MergeAllUndoCanvases();
		bool flag = IsCanvasBlank(base_canvas_1_tex);
		bool flag2 = IsCanvasBlank(base_canvas_2_tex);
		if (flag && flag2)
		{
			PopupControl.Instance.ShowMessage("You must draw something first!");
			return;
		}
		curr_screen = curr_screen_t.save;
		painting_window.SetActive(false);
		save_window.SetActive(true);
		input_save_title.SetTextWithoutNotify(loaded_painting_title);
		input_save_description.SetTextWithoutNotify(loaded_painting_desc);
		if (!flag && !flag2)
		{
			speed_slider.SetActive(true);
			preview_frame_2 = true;
			ChangeSpeed(0);
		}
		else if (flag)
		{
			speed_slider.SetActive(false);
			save_screen_preview.sprite = ((curr_frame == 2) ? base_canvas_img.sprite : onion_skins_canvas.sprite);
		}
		else
		{
			speed_slider.SetActive(false);
			save_screen_preview.sprite = ((curr_frame == 1) ? base_canvas_img.sprite : onion_skins_canvas.sprite);
		}
	}

	private bool IsCanvasBlank(Texture2D tex)
	{
		if (tex == null)
		{
			return true;
		}
		Color[] pixels = tex.GetPixels();
		for (int i = 0; i < pixels.Length; i++)
		{
			if (pixels[i] != Color.white)
			{
				return false;
			}
		}
		return true;
	}

	private IEnumerator AnimatePreview()
	{
		while (true)
		{
			preview_frame_2 = !preview_frame_2;
			if (preview_frame_2)
			{
				if (curr_frame == 1)
				{
					save_screen_preview.sprite = onion_skins_canvas.sprite;
				}
				else if (curr_frame == 2)
				{
					save_screen_preview.sprite = base_canvas_img.sprite;
				}
			}
			else if (curr_frame == 1)
			{
				save_screen_preview.sprite = base_canvas_img.sprite;
			}
			else if (curr_frame == 2)
			{
				save_screen_preview.sprite = onion_skins_canvas.sprite;
			}
			yield return new WaitForSeconds(GetPreviewSpeed(speed));
		}
	}

	public static float GetPreviewSpeed(int preview_speed)
	{
		switch (preview_speed)
		{
		case 0:
			return 1.5f;
		case 1:
			return 1.1f;
		case 2:
			return 0.8f;
		case 3:
			return 0.5f;
		case 4:
			return 0.3f;
		case 5:
			return 0.1f;
		case 6:
			return 0.05f;
		default:
			return 0.5f;
		}
	}

	public void ClearAll()
	{
		loaded_painting_title = "";
		loaded_painting_desc = "";
		speed = 3;
		PressFrame(1);
		if (onion_skins_enabled)
		{
			PressOnionSkin();
		}
		FillWhite(ref base_canvas_1_tex);
		base_canvas_img.sprite = Sprite.Create(base_canvas_1_tex, new Rect(0f, 0f, n_horizontal_segs * 32, n_vertical_segs * 32), Vector2.zero);
		base_canvas_2_tex = null;
		onion_skins_canvas.sprite = null;
		foreach (undo_canvas_struct undo_canvase in undo_canvases)
		{
			Object.Destroy(undo_canvase.image.gameObject);
		}
		undo_canvases.Clear();
	}

	private void MergeAllUndoCanvases()
	{
		Texture2D texture2D = ((curr_frame == 1) ? base_canvas_1_tex : base_canvas_2_tex);
		Color[] pixels = texture2D.GetPixels();
		List<Color[]> list = new List<Color[]>();
		foreach (undo_canvas_struct undo_canvase in undo_canvases)
		{
			list.Add(undo_canvase.tex.GetPixels());
		}
		foreach (Color[] item in list)
		{
			for (int i = 0; i < n_vertical_segs * 32; i++)
			{
				for (int j = 0; j < n_horizontal_segs * 32; j++)
				{
					int num = j + i * 32 * n_horizontal_segs;
					if (item[num].a != 0f)
					{
						pixels[num] = item[num];
					}
				}
			}
		}
		texture2D.SetPixels(pixels);
		texture2D.Apply();
		foreach (undo_canvas_struct undo_canvase2 in undo_canvases)
		{
			Object.Destroy(undo_canvase2.image.gameObject);
		}
		undo_canvases.Clear();
	}

	public void OnOpen()
	{
		segment_images = new Image[n_horizontal_segs, n_vertical_segs];
		segment_textures = new Texture2D[n_horizontal_segs, n_vertical_segs];
		float num = base_canvas_img.rectTransform.sizeDelta.x * (1f / (float)n_horizontal_segs);
		float num2 = base_canvas_img.rectTransform.sizeDelta.y * (1f / (float)n_vertical_segs);
		for (int i = 0; i < n_horizontal_segs; i++)
		{
			for (int j = 0; j < n_vertical_segs; j++)
			{
				GameObject gameObject = new GameObject("segment(" + i + "," + j + ")");
				gameObject.transform.SetParent(base_canvas_img.transform);
				float x = base_canvas_img.rectTransform.sizeDelta.x;
				float y = base_canvas_img.rectTransform.sizeDelta.y;
				gameObject.transform.localPosition = new Vector3(num * 0.5f - x * 0.5f + num * (float)i, num2 * 0.5f - y * 0.5f + num2 * (float)j, 0f);
				gameObject.transform.localScale = Vector3.one;
				Image image = gameObject.AddComponent<Image>();
				image.rectTransform.sizeDelta = new Vector2(num, num2);
				segment_images[i, j] = image;
				image.gameObject.transform.SetParent(workspace_segments_parent);
				segment_textures[i, j] = new Texture2D(32, 32);
				segment_textures[i, j].filterMode = FilterMode.Point;
				ClearWorkCanvas(i, j);
			}
		}
		FillWhite(ref base_canvas_1_tex);
		base_canvas_img.sprite = Sprite.Create(base_canvas_1_tex, new Rect(0f, 0f, n_horizontal_segs * 32, n_vertical_segs * 32), Vector2.zero);
		PressToolNib(1);
		allow_input = false;
		StartCoroutine(DelayedAllowInput());
		zoom_marker.SetActive(false);
		painting_window.SetActive(true);
		custom_color_window.SetActive(false);
		save_window.SetActive(false);
		curr_screen = curr_screen_t.painting;
		col_frame_button_selected = frame_1_button.color;
		col_frame_button_deselected = frame_2_button.color;
	}

	private void OpenPaintingSelectScreen(string text)
	{
		WindowPrefabsControl.Instance.CreateScreen("INVENTORY-pickItem", WindowPrefabsControl.build_into_t.mini_window);
		WindowPrefabsControl.Instance.GetTextLegacy("INVENTORY-pickItem", "saveload_text").text = text;
		inventory_ctr.Instance.LayOutInvSlots(false, false, inventory_ctr.slots_positionings.show_15_centered, false, inventory_ctr.Instance.NumPlayerPages(), false, "", inventory_ctr.fusion_button.hide);
		inventory_ctr.Instance.RedrawInventorySlots();
	}

	public void ChangeSpeed(int dir)
	{
		speed = Mathf.Clamp(speed + dir, 0, 6);
		switch (speed)
		{
		case 0:
			speed_text.text = "Super Slow";
			break;
		case 1:
			speed_text.text = "Very Slow";
			break;
		case 2:
			speed_text.text = "Slow";
			break;
		case 3:
			speed_text.text = "Normal";
			break;
		case 4:
			speed_text.text = "Fast";
			break;
		case 5:
			speed_text.text = "Very Fast";
			break;
		case 6:
			speed_text.text = "Extreme";
			break;
		}
		if (animated_preview != null)
		{
			StopCoroutine(animated_preview);
		}
		animated_preview = AnimatePreview();
		StartCoroutine(animated_preview);
	}

	public void PressSaveCancel()
	{
		PopupControl.Instance.SetButtonWasPressed();
		if (animated_preview != null)
		{
			StopCoroutine(animated_preview);
		}
		curr_screen = curr_screen_t.painting;
		save_window.SetActive(false);
		painting_window.SetActive(true);
	}

	public void PressSaveAccept()
	{
		if (animated_preview != null)
		{
			StopCoroutine(animated_preview);
		}
		save_window.SetActive(false);
		curr_screen = curr_screen_t.pick_on_SAVE;
		save_screen_open = true;
		OpenPaintingSelectScreen("Select a 'Blank Canvas' from your inventory");
	}

	public void FinalizeLoad(int load_inv_slot)
	{
		PopupControl.Instance.SetButtonWasPressed();
		curr_screen = curr_screen_t.painting;
		painting_window.SetActive(true);
		WindowPrefabsControl.Instance.DestroyScreen("INVENTORY-pickItem");
		load_screen_open = false;
		inventory_ctr.Instance.HideInventoryTab(false);
		InventoryItem item = inventory_ctr.Instance.player_inventory[load_inv_slot].item;
		if (item.GetShort("dev_painting_id") != 0)
		{
			PopupControl.Instance.ShowMessage("You cannot open that painting!");
			return;
		}
		foreach (undo_canvas_struct undo_canvase in undo_canvases)
		{
			Object.Destroy(undo_canvase.image.gameObject);
		}
		undo_canvases.Clear();
		if (curr_frame == 2)
		{
			PressFrame(1);
		}
		if (onion_skins_enabled)
		{
			PressOnionSkin();
		}
		loaded_painting_title = item.GetString("painting_name");
		loaded_painting_desc = item.GetString("painting_desc");
		speed = item.GetShort("painting_speed");
		if (item.GetShort("dev_painting_id") == 0)
		{
			if (base_canvas_1_tex != null)
			{
				Object.Destroy(base_canvas_1_tex);
			}
			base_canvas_1_tex = inventory_ctr.Instance.LoadPaintingFrame(item, "n_bytes_frame_1", "b1-");
			base_canvas_img.sprite = Sprite.Create(base_canvas_1_tex, new Rect(0f, 0f, base_canvas_1_tex.width, base_canvas_1_tex.height), new Vector2(0f, 0f));
			if (speed == -1)
			{
				base_canvas_2_tex = null;
				onion_skins_canvas.sprite = null;
				return;
			}
			if (base_canvas_2_tex != null)
			{
				Object.Destroy(base_canvas_2_tex);
			}
			base_canvas_2_tex = inventory_ctr.Instance.LoadPaintingFrame(item, "n_bytes_frame_2", "b2-");
			onion_skins_canvas.sprite = Sprite.Create(base_canvas_2_tex, new Rect(0f, 0f, base_canvas_2_tex.width, base_canvas_2_tex.height), new Vector2(0f, 0f));
			return;
		}
		if (base_canvas_2_tex != null)
		{
			Object.Destroy(base_canvas_2_tex);
		}
		if (base_canvas_1_tex != null)
		{
			Object.Destroy(base_canvas_1_tex);
		}
		PopupControl.Instance.ShowConnecting("Loading Painting");
		dev_frame_1_loaded = false;
		dev_frame_2_loaded = false;
		DevBuildControl.NPC_painting nPC_painting = DevBuildControl.Instance.NPC_paintings[item.GetShort("dev_painting_id")];
		short num = nPC_painting.speed;
		string painting_filename = nPC_painting.painting_filename;
		ResourceControl.Instance.AssignPainting(painting_filename + "0", base_canvas_img, delegate
		{
			base_canvas_img.sprite = CloneDevSprite(base_canvas_img.sprite.texture);
			base_canvas_1_tex = base_canvas_img.sprite.texture;
			dev_frame_1_loaded = true;
			if (dev_frame_2_loaded)
			{
				PopupControl.Instance.HideAll();
			}
		});
		ResourceControl.Instance.AssignPainting(painting_filename + "1", onion_skins_canvas, delegate
		{
			onion_skins_canvas.sprite = CloneDevSprite(onion_skins_canvas.sprite.texture);
			base_canvas_2_tex = onion_skins_canvas.sprite.texture;
			dev_frame_2_loaded = true;
			if (dev_frame_1_loaded)
			{
				PopupControl.Instance.HideAll();
			}
		});
		speed = num;
	}

	private Sprite CloneDevSprite(Texture2D tex)
	{
		Texture2D texture2D = new Texture2D(tex.width, tex.height);
		texture2D.filterMode = FilterMode.Point;
		texture2D.SetPixels(tex.GetPixels());
		texture2D.Apply();
		return Sprite.Create(texture2D, new Rect(0f, 0f, tex.width, tex.height), Vector2.zero);
	}

	private void OnDestroy()
	{
		if (base_canvas_1_tex != null)
		{
			Object.Destroy(base_canvas_1_tex);
		}
		if (base_canvas_2_tex != null)
		{
			Object.Destroy(base_canvas_2_tex);
		}
	}

	private void SavePaintingFrame(Texture2D canvas, string counter, string prefix, ExtraInventoryData extra_data)
	{
		byte[] array = canvas.EncodeToPNG();
		extra_data.SetLong(counter, array.Length);
		int num = 0;
		byte[] array2 = new byte[4];
		int num2 = 0;
		for (int i = 0; i < array.Length; i++)
		{
			array2[num2++] = array[i];
			if (num2 == 4)
			{
				extra_data.SetLong(prefix + num, System.BitConverter.ToInt32(array2, 0));
				array2 = new byte[4];
				num2 = 0;
				num++;
			}
		}
		if (num2 != 0)
		{
			extra_data.SetLong(prefix + num, System.BitConverter.ToInt32(array2, 0));
		}
	}

	public void FinalizeSave(int save_inv_slot)
	{
		PopupControl.Instance.SetButtonWasPressed();
		ExtraInventoryData extraInventoryData = new ExtraInventoryData();
		extraInventoryData.SetString("painting_name", input_save_title.text);
		extraInventoryData.SetString("painting_desc", input_save_description.text);
		extraInventoryData.SetString("painting_creator", (PlayerData.Instance.GetGlobalString("username_lower") == "") ? "ME" : PlayerData.Instance.GetGlobalString("username_lower"));
		extraInventoryData.SetString("painting_date", System.DateTime.Now.ToString("dd-MM-yyyy"));
		bool flag = IsCanvasBlank(base_canvas_1_tex);
		bool flag2 = IsCanvasBlank(base_canvas_2_tex);
		if (!flag && !flag2)
		{
			extraInventoryData.SetShort("painting_speed", speed);
			SavePaintingFrame(base_canvas_1_tex, "n_bytes_frame_1", "b1-", extraInventoryData);
			SavePaintingFrame(base_canvas_2_tex, "n_bytes_frame_2", "b2-", extraInventoryData);
		}
		else
		{
			extraInventoryData.SetShort("painting_speed", -1);
			SavePaintingFrame((!flag) ? base_canvas_1_tex : base_canvas_2_tex, "n_bytes_frame_1", "b1-", extraInventoryData);
		}
		inventory_ctr.Instance.player_inventory[save_inv_slot] = new ItemCountPair(new InventoryItem("Painting", extraInventoryData), 1);
		WindowPrefabsControl.Instance.DestroyScreen("PAINTING");
		WindowPrefabsControl.Instance.DestroyScreen("INVENTORY-pickItem");
		save_screen_open = false;
		inventory_ctr.Instance.press_inv_button();
		if (save_inv_slot >= 20)
		{
			save_inv_slot -= 20;
		}
		inventory_ctr.Instance.show_angular(inventory_ctr.Instance.instantiated_inv_slots[save_inv_slot].transform.localPosition, inventory_ctr.Instance.default_angular_col);
	}

	public void PressBackOnSaveLoad()
	{
		inventory_ctr.Instance.HideInventoryTab(false);
		WindowPrefabsControl.Instance.DestroyScreen("INVENTORY-pickItem");
		save_screen_open = false;
		load_screen_open = false;
		painting_window.SetActive(true);
		curr_screen = curr_screen_t.painting;
	}

	public void PressLoad()
	{
		painting_window.SetActive(false);
		curr_screen = curr_screen_t.pick_on_LOAD;
		load_screen_open = true;
		OpenPaintingSelectScreen("Select a Painting to load from your inventory");
	}

	private void FillWhite(ref Texture2D tex)
	{
		Color[] array = new Color[n_horizontal_segs * n_vertical_segs * 1024];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new Color(1f, 1f, 1f, 1f);
		}
		tex = new Texture2D(n_horizontal_segs * 32, n_vertical_segs * 32);
		tex.filterMode = FilterMode.Point;
		tex.SetPixels(array);
		tex.Apply();
	}

	private void ClearWorkCanvas(int x, int y)
	{
		Color[] array = new Color[1024];
		for (int i = 0; i < 1024; i++)
		{
			array[i] = new Color(0f, 0f, 0f, 0f);
		}
		segment_textures[x, y].SetPixels(array);
		segment_textures[x, y].Apply();
		segment_images[x, y].sprite = Sprite.Create(segment_textures[x, y], new Rect(0f, 0f, 32f, 32f), Vector2.zero);
	}

	private IEnumerator DelayedAllowInput()
	{
		yield return new WaitForSeconds(0.5f);
		allow_input = true;
	}

	public void AcceptCustomColor()
	{
		PopupControl.Instance.SetButtonWasPressed();
		AddCustomColor(custom_col_swatch.color);
		painting_window.SetActive(true);
		custom_color_window.SetActive(false);
		curr_screen = curr_screen_t.painting;
	}

	public void AddCustomColor(Color C)
	{
		if (custom_colors.Count == 0)
		{
			custom_colors.Add(C);
		}
		else
		{
			custom_colors.Insert(0, C);
		}
		if (custom_colors.Count > 3)
		{
			custom_colors.RemoveAt(custom_colors.Count - 1);
		}
		custom_color_swatches[0].GetComponent<Image>().color = custom_colors[0];
		if (custom_colors.Count > 1)
		{
			custom_color_swatches[1].GetComponent<Image>().color = custom_colors[1];
		}
		if (custom_colors.Count > 2)
		{
			custom_color_swatches[2].GetComponent<Image>().color = custom_colors[2];
		}
		PressColorNib(custom_color_swatches[0]);
	}

	public void PressColorNib(GameObject nib_obj)
	{
		if (nib_obj.GetComponent<Image>().color.a == 1f)
		{
			if (prev_selected_color_nib != null)
			{
				prev_selected_color_nib.GetComponent<Shadow>().enabled = true;
			}
			paint_color = nib_obj.GetComponent<Image>().color;
			nib_obj.GetComponent<Shadow>().enabled = false;
			prev_selected_color_nib = nib_obj;
			color_selector.transform.localPosition = nib_obj.transform.localPosition;
			color_selector.transform.SetAsFirstSibling();
		}
	}

	public void PressCustomColor()
	{
		painting_window.SetActive(false);
		custom_color_window.SetActive(true);
		curr_screen = curr_screen_t.color_customize;
		HsvColor hsvColor = HSVUtil.ConvertRgbToHsv(paint_color);
		slider_value_S = (float)hsvColor.S;
		slider_value_H = (float)(hsvColor.H * 0.0027777778450399637);
		slider_value_V = (float)hsvColor.V;
		((RectTransform)hue_slider.transform.Find("nib").transform).anchoredPosition = new Vector2(slider_value_H * hue_slider.sizeDelta.x, 68f);
		((RectTransform)saturation_value_slider.transform.Find("nib").transform).anchoredPosition = new Vector2(slider_value_S * saturation_value_slider.sizeDelta.x, slider_value_V * saturation_value_slider.sizeDelta.y);
		custom_col_swatch.color = HSVUtil.ConvertHsvToRgb(slider_value_H * 360f, slider_value_S, slider_value_V, 1f);
		RedrawHueSlider();
		RedrawSaturationValueSlider();
	}

	private void RedrawHueSlider()
	{
		Texture2D texture2D = new Texture2D(30, 1);
		Color[] array = new Color[30];
		for (int i = 0; i < 30; i++)
		{
			array[i] = HSVUtil.ConvertHsvToRgb((float)i / 30f * 360f, 1.0, 1.0, 1f);
		}
		texture2D.SetPixels(array);
		texture2D.Apply();
		hue_slider.gameObject.GetComponent<Image>().sprite = Sprite.Create(texture2D, new Rect(0f, 0f, 30f, 1f), new Vector2(0f, 0f));
	}

	private void RedrawSaturationValueSlider()
	{
		Texture2D texture2D = new Texture2D(40, 15);
		Color[] array = new Color[600];
		for (int i = 0; i < 15; i++)
		{
			for (int j = 0; j < 40; j++)
			{
				array[j + i * 40] = HSVUtil.ConvertHsvToRgb(slider_value_H * 360f, (float)j / 40f, (float)i / 15f, 1f);
			}
		}
		texture2D.SetPixels(array);
		texture2D.Apply();
		saturation_value_slider.gameObject.GetComponent<Image>().sprite = Sprite.Create(texture2D, new Rect(0f, 0f, 40f, 15f), new Vector2(0f, 0f));
	}

	public void PressHueSlider()
	{
		is_clicking_hue_slider = true;
	}

	public void PressSatValSlider()
	{
		is_clicking_satval_slider = true;
	}

	public void PressToolNib(int index)
	{
		GameObject gameObject = tool_buttons[index];
		prev_selected_tool_id = index;
		size_selector.transform.localPosition = gameObject.transform.localPosition;
		size_selector.transform.SetAsFirstSibling();
		if (index > 4)
		{
			return;
		}
		stroke_fill = stroke_fills[index];
		Texture2D texture = gameObject.transform.Find("Image").GetComponent<Image>().sprite.texture;
		Color[] pixels = texture.GetPixels();
		int num = texture.width - 1;
		int num2 = texture.height - 1;
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < texture.height; i++)
		{
			for (int j = 0; j < texture.width; j++)
			{
				if (pixels[j + texture.width * i] == Color.white)
				{
					if (j < num)
					{
						num = j;
					}
					if (num3 < j)
					{
						num3 = j + 1;
					}
					if (i < num2)
					{
						num2 = i;
					}
					if (num4 < i)
					{
						num4 = i + 1;
					}
				}
			}
		}
		int num5 = num3 - num;
		int num6 = num4 - num2;
		Color[] array = new Color[num5 * num6];
		int num7 = 0;
		for (int k = num2; k < num4; k++)
		{
			for (int l = 0; l < num5; l++)
			{
				array[num7 + l] = pixels[num + l + k * texture.width];
			}
			num7 += num5;
		}
		Texture2D texture2D = new Texture2D(num5, num6);
		texture2D.filterMode = FilterMode.Point;
		texture2D.SetPixels(array);
		texture2D.Apply();
		pen_tex = texture2D;
	}

	public void PressUndo()
	{
		if (undo_canvases.Count != 0)
		{
			undo_canvas_struct undo_canvas_struct = undo_canvases[undo_canvases.Count - 1];
			undo_canvases.RemoveAt(undo_canvases.Count - 1);
			Object.Destroy(undo_canvas_struct.image.gameObject);
		}
	}

	private bool MouseWithinWorkArea()
	{
		if (curr_mouse_percent_x >= 0f && curr_mouse_percent_y < 1f && curr_mouse_percent_x < 1f)
		{
			return curr_mouse_percent_y >= 0f;
		}
		return false;
	}

	private void FixedUpdate()
	{
		if (curr_screen == curr_screen_t.painting)
		{
			if (prev_selected_tool_id < 5 && draw_stroke)
			{
				PaintStroke(pen_tex, stroke_fill, paint_color);
			}
			prev_mouse_percent_x = curr_mouse_percent_x;
			prev_mouse_percent_y = curr_mouse_percent_y;
		}
	}

	private void CreateUndoCanvasFromWorkArea()
	{
		Color[] array = new Color[n_horizontal_segs * n_vertical_segs * 1024];
		for (int i = 0; i < n_vertical_segs; i++)
		{
			for (int j = 0; j < n_horizontal_segs; j++)
			{
				Color[] pixels = segment_textures[j, i].GetPixels();
				for (int k = 0; k < 32; k++)
				{
					for (int l = 0; l < 32; l++)
					{
						array[j * 32 + i * 1024 * n_horizontal_segs + l + k * 32 * n_horizontal_segs] = pixels[k * 32 + l];
					}
				}
				ClearWorkCanvas(j, i);
			}
		}
		GameObject gameObject = new GameObject("undo_canvas");
		gameObject.transform.SetParent(base_canvas_img.transform.parent);
		gameObject.transform.localPosition = base_canvas_img.transform.localPosition;
		gameObject.transform.localRotation = base_canvas_img.transform.localRotation;
		gameObject.transform.localScale = base_canvas_img.transform.localScale;
		Texture2D texture2D = new Texture2D(n_horizontal_segs * 32, n_vertical_segs * 32);
		texture2D.filterMode = FilterMode.Point;
		texture2D.SetPixels(array);
		texture2D.Apply();
		undo_canvas_struct undo_canvas_struct = default(undo_canvas_struct);
		undo_canvas_struct.image = gameObject.AddComponent<Image>();
		undo_canvas_struct.image.rectTransform.sizeDelta = base_canvas_img.rectTransform.sizeDelta;
		undo_canvas_struct.image.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, n_horizontal_segs * 32, n_vertical_segs * 32), Vector2.zero);
		undo_canvas_struct.tex = texture2D;
		gameObject.transform.SetParent(undo_canvases_parent);
		if (undo_canvases.Count < n_undos)
		{
			undo_canvases.Add(undo_canvas_struct);
			return;
		}
		Texture2D texture2D2 = ((curr_frame == 1) ? base_canvas_1_tex : base_canvas_2_tex);
		Color[] pixels2 = undo_canvases[0].tex.GetPixels();
		Color[] pixels3 = texture2D2.GetPixels();
		for (int m = 0; m < n_vertical_segs * 32; m++)
		{
			for (int n = 0; n < n_horizontal_segs * 32; n++)
			{
				int num = n + m * 32 * n_horizontal_segs;
				if (pixels2[num].a != 0f)
				{
					pixels3[num] = pixels2[num];
				}
			}
		}
		texture2D2.SetPixels(pixels3);
		texture2D2.Apply();
		base_canvas_img.sprite = Sprite.Create(texture2D2, new Rect(0f, 0f, n_horizontal_segs * 32, n_vertical_segs * 32), Vector2.zero);
		Object.Destroy(undo_canvases[0].image.gameObject);
		for (int num2 = 0; num2 < undo_canvases.Count - 1; num2++)
		{
			undo_canvases[num2] = undo_canvases[num2 + 1];
		}
		undo_canvases[undo_canvases.Count - 1] = undo_canvas_struct;
	}

	private void Update()
	{
		if (curr_screen == curr_screen_t.painting)
		{
			if (PopupControl.Instance.GetButtonWasPressed())
			{
				return;
			}
			Vector2 vector = CalcMousePercent();
			if (GamepadInput.Instance.GetMouseButtonDown())
			{
				prev_mouse_percent_x = vector.x;
				prev_mouse_percent_y = vector.y;
			}
			curr_mouse_percent_x = vector.x;
			curr_mouse_percent_y = vector.y;
			if (prev_selected_tool_id < 5)
			{
				if (!allow_input)
				{
					return;
				}
				if (!GamepadInput.Instance.GetMouseButton())
				{
					if (create_undo_canvas)
					{
						CreateUndoCanvasFromWorkArea();
						create_undo_canvas = false;
					}
					draw_stroke = false;
				}
				else if (MouseWithinWorkArea())
				{
					draw_stroke = true;
				}
				return;
			}
			switch (prev_selected_tool_id)
			{
			case 5:
				if (GamepadInput.Instance.GetMouseButtonDown() && MouseWithinWorkArea() && zoom_undos.Count >= 1)
				{
					zoom_undo zoom_undo = zoom_undos[zoom_undos.Count - 1];
					onion_skins_canvas.transform.localPosition = zoom_undo.local_pos;
					workspace_segments_parent.transform.localPosition = zoom_undo.local_pos;
					undo_canvases_parent.transform.localPosition = zoom_undo.local_pos;
					base_canvas_img.transform.localPosition = zoom_undo.local_pos;
					zoom_undo zoom_undo2 = zoom_undos[zoom_undos.Count - 1];
					onion_skins_canvas.transform.localScale = zoom_undo2.local_scale;
					workspace_segments_parent.transform.localScale = zoom_undo2.local_scale;
					undo_canvases_parent.transform.localScale = zoom_undo2.local_scale;
					base_canvas_img.transform.localScale = zoom_undo2.local_scale;
					zoom_undos.RemoveAt(zoom_undos.Count - 1);
					if (zoom_undos.Count == 0)
					{
						zoom_marker.SetActive(false);
					}
					else
					{
						zoom_Text.text = (int)(base_canvas_img.transform.localScale.x * 100f) + "%";
					}
				}
				break;
			case 6:
				if (GamepadInput.Instance.GetMouseButtonDown() && MouseWithinWorkArea() && zoom_undos.Count <= 4)
				{
					zoom_undos.Add(new zoom_undo
					{
						local_pos = base_canvas_img.transform.localPosition,
						local_scale = base_canvas_img.transform.localScale
					});
					CenterOn();
					UpdateZoom(zoom_amount);
					zoom_marker.SetActive(true);
					zoom_Text.text = (int)(base_canvas_img.transform.localScale.x * 100f) + "%";
				}
				break;
			case 7:
				if (GamepadInput.Instance.GetMouseButtonDown() && MouseWithinWorkArea())
				{
					FloodFill();
				}
				break;
			case 8:
			{
				if (!GamepadInput.Instance.GetMouseButtonDown() || !MouseWithinWorkArea())
				{
					break;
				}
				Vector2 vector2 = CalcScaledMousePixel(curr_mouse_percent_x, curr_mouse_percent_y);
				int x = (int)vector2.x;
				int y = (int)vector2.y;
				int num = undo_canvases.Count;
				Color pixel;
				do
				{
					num--;
					if (num < 0)
					{
						pixel = ((curr_frame == 1) ? base_canvas_1_tex : base_canvas_2_tex).GetPixel(x, y);
						break;
					}
					pixel = undo_canvases[num].tex.GetPixel(x, y);
				}
				while (pixel.a == 0f);
				if (!TryFindCorrespondingSwatch(pixel))
				{
					AddCustomColor(pixel);
				}
				break;
			}
			}
		}
		else
		{
			if (curr_screen != curr_screen_t.color_customize)
			{
				return;
			}
			Vector3 mousePosition = GamepadInput.Instance.GetMousePosition();
			if (is_clicking_hue_slider)
			{
				Transform transform = hue_slider.transform.Find("nib");
				float value = ((mousePosition.x - (float)Screen.width * 0.5f) / WindowControl.Instance.gui_canvas.scaleFactor / WindowControl.Instance.miniwindow.transform.localScale.x - hue_slider.localPosition.x) / hue_slider.sizeDelta.x + 0.5f;
				value = Mathf.Clamp01(value);
				((RectTransform)transform.transform).anchoredPosition = new Vector2(value * hue_slider.sizeDelta.x, 68f);
				slider_value_H = value;
				RedrawSaturationValueSlider();
				custom_col_swatch.color = HSVUtil.ConvertHsvToRgb(slider_value_H * 360f, slider_value_S, slider_value_V, 1f);
				if (GamepadInput.Instance.GetMouseButtonUp())
				{
					is_clicking_hue_slider = false;
				}
			}
			else if (is_clicking_satval_slider)
			{
				Transform transform2 = saturation_value_slider.transform.Find("nib");
				float value2 = ((mousePosition.x - (float)Screen.width * 0.5f) / WindowControl.Instance.gui_canvas.scaleFactor / WindowControl.Instance.miniwindow.transform.localScale.x - saturation_value_slider.localPosition.x) / saturation_value_slider.sizeDelta.x + 0.5f;
				float value3 = ((mousePosition.y - (float)Screen.height * 0.5f) / WindowControl.Instance.gui_canvas.scaleFactor / WindowControl.Instance.miniwindow.transform.localScale.y - saturation_value_slider.localPosition.y) / saturation_value_slider.sizeDelta.y + 0.5f;
				value2 = Mathf.Clamp01(value2);
				value3 = Mathf.Clamp01(value3);
				((RectTransform)transform2.transform).anchoredPosition = new Vector2(value2 * saturation_value_slider.sizeDelta.x, value3 * saturation_value_slider.sizeDelta.y);
				slider_value_S = value2;
				slider_value_V = value3;
				custom_col_swatch.color = HSVUtil.ConvertHsvToRgb(slider_value_H * 360f, value2, value3, 1f);
				if (GamepadInput.Instance.GetMouseButtonUp())
				{
					is_clicking_satval_slider = false;
				}
			}
		}
	}

	private float ColorDifference(Color col_A, Color col_B)
	{
		return Mathf.Abs(col_A.b - col_B.b) + Mathf.Abs(col_A.r - col_B.r) + Mathf.Abs(col_A.g - col_B.g);
	}

	private bool TryFindCorrespondingSwatch(Color col)
	{
		GameObject[] array = hardcoded_color_swatches;
		foreach (GameObject gameObject in array)
		{
			if (ColorDifference(gameObject.GetComponent<Image>().color, col) < 0.02f)
			{
				PressColorNib(gameObject);
				return true;
			}
		}
		array = custom_color_swatches;
		foreach (GameObject gameObject2 in array)
		{
			if (ColorDifference(gameObject2.GetComponent<Image>().color, col) < 0.02f)
			{
				PressColorNib(gameObject2);
				return true;
			}
		}
		return false;
	}

	private void CenterOn()
	{
		float x = base_canvas_img.rectTransform.sizeDelta.x;
		float y = base_canvas_img.rectTransform.sizeDelta.y;
		float t = Mathf.Clamp01(curr_mouse_percent_x);
		float t2 = Mathf.Clamp01(curr_mouse_percent_y);
		zoom_parent.transform.localPosition = new Vector3(Mathf.Lerp(x * 0.5f, (0f - x) * 0.5f, t), Mathf.Lerp(y * 0.5f, (0f - y) * 0.5f, t2), 0f) + zoom_parent.transform.localPosition;
	}

	private void UpdateZoom(float scale_amount)
	{
		GameObject gameObject = new GameObject("temp scaler");
		gameObject.transform.SetParent(mask.transform);
		gameObject.transform.localPosition = Vector2.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector2.one;
		base_canvas_img.gameObject.transform.SetParent(gameObject.transform);
		workspace_segments_parent.parent = gameObject.transform;
		undo_canvases_parent.parent = gameObject.transform;
		onion_skins_canvas.gameObject.transform.SetParent(gameObject.transform);
		gameObject.transform.localScale = Vector3.one * scale_amount;
		zoom_parent.transform.localPosition = Vector2.zero;
		base_canvas_img.gameObject.transform.SetParent(zoom_parent.transform);
		undo_canvases_parent.parent = zoom_parent.transform;
		workspace_segments_parent.parent = zoom_parent.transform;
		onion_skins_canvas.gameObject.transform.SetParent(zoom_parent.transform);
		Object.Destroy(gameObject);
	}

	private void AddSegmentToPaintList(int x, int y, Dictionary<string, segment_to_paint_on> canvases_to_paint_on)
	{
		if (!canvases_to_paint_on.ContainsKey(x + "," + y) && x >= 0 && y >= 0 && x < n_horizontal_segs && y < n_vertical_segs)
		{
			canvases_to_paint_on.Add(x + "," + y, new segment_to_paint_on
			{
				x = x,
				y = y
			});
		}
	}

	private Vector2 CalcMousePercent()
	{
		Vector3 mousePosition = GamepadInput.Instance.GetMousePosition();
		float num = (mousePosition.x - (float)Screen.width * 0.5f) / WindowControl.Instance.gui_canvas.scaleFactor;
		float num2 = (mousePosition.y - (float)Screen.height * 0.5f) / WindowControl.Instance.gui_canvas.scaleFactor;
		float x = WindowControl.Instance.miniwindow.transform.localScale.x;
		float y = WindowControl.Instance.miniwindow.transform.localScale.y;
		return new Vector2((num / x - mask.rectTransform.localPosition.x) / mask.rectTransform.sizeDelta.x + 0.5f, (num2 / y - mask.rectTransform.localPosition.y) / mask.rectTransform.sizeDelta.y + 0.5f);
	}

	private Vector2 CalcScaledMousePixel(float mouse_percent_x, float mouse_percent_y)
	{
		float num = base_canvas_img.transform.localScale.x * 362.5f;
		float num2 = Mathf.InverseLerp(num, 0f - num, base_canvas_img.transform.localPosition.x);
		float num3 = base_canvas_img.transform.localScale.y * 362.5f;
		float num4 = Mathf.InverseLerp(num3, 0f - num3, base_canvas_img.transform.localPosition.y);
		float num5 = (float)n_horizontal_segs * 32f;
		float num6 = (float)n_vertical_segs * 32f;
		return new Vector2((int)(num5 * num2 + (mouse_percent_x - 0.5f) * (num5 / base_canvas_img.transform.localScale.x)), (int)(num6 * num4 + (mouse_percent_y - 0.5f) * (num6 / base_canvas_img.transform.localScale.y)));
	}

	public void FloodFill()
	{
		Texture2D texture2D = ((curr_frame == 1) ? base_canvas_1_tex : base_canvas_2_tex);
		List<Color[]> list = new List<Color[]>();
		list.Add(texture2D.GetPixels());
		foreach (undo_canvas_struct undo_canvase in undo_canvases)
		{
			list.Add(undo_canvase.tex.GetPixels());
		}
		bool[,] checked_map = new bool[n_horizontal_segs * 32, n_vertical_segs * 32];
		Vector2 vector = CalcScaledMousePixel(curr_mouse_percent_x, curr_mouse_percent_y);
		int num = (int)(vector.x + vector.y * (float)n_horizontal_segs * 32f);
		Color color = new Color(1f, 1f, 1f, 1f);
		for (int num2 = list.Count - 1; num2 >= 0; num2--)
		{
			if (list[num2][num].a != 0f)
			{
				color = list[num2][num];
				break;
			}
		}
		List<Vector2> list2 = new List<Vector2>();
		int num3 = (int)vector.x;
		int num4 = (int)vector.y;
		checked_map[num3, num4] = true;
		list2.Add(new Vector2(num3, num4));
		List<Vector2> list3 = new List<Vector2>();
		TryAddPixel(num3 + 1, num4, list3, checked_map);
		TryAddPixel(num3 - 1, num4, list3, checked_map);
		TryAddPixel(num3, num4 + 1, list3, checked_map);
		TryAddPixel(num3, num4 - 1, list3, checked_map);
		do
		{
			List<Vector2> list4 = new List<Vector2>();
			while (list3.Count > 0)
			{
				Vector2 vector2 = list3[list3.Count - 1];
				int num5 = (int)vector2.x;
				int num6 = (int)vector2.y;
				int num7 = num5 + num6 * n_horizontal_segs * 32;
				for (int num8 = list.Count - 1; num8 >= 0; num8--)
				{
					if (list[num8][num7].a != 0f)
					{
						checked_map[num5, num6] = true;
						if (list[num8][num7] == color)
						{
							list2.Add(new Vector2(num5, num6));
							TryAddPixel(num5 + 1, num6, list4, checked_map);
							TryAddPixel(num5 - 1, num6, list4, checked_map);
							TryAddPixel(num5, num6 + 1, list4, checked_map);
							TryAddPixel(num5, num6 - 1, list4, checked_map);
						}
						break;
					}
				}
				list3.RemoveAt(list3.Count - 1);
			}
			list3 = list4;
		}
		while (list3.Count != 0);
		Dictionary<string, partially_drawn_segment> dictionary = new Dictionary<string, partially_drawn_segment>();
		foreach (Vector2 item in list2)
		{
			int num9 = (int)(item.x * 0.03125f);
			int num10 = (int)(item.y * 0.03125f);
			if (!dictionary.ContainsKey(num9 + "," + num10))
			{
				partially_drawn_segment value = new partially_drawn_segment
				{
					x = num9,
					y = num10,
					cols = new Color[1024]
				};
				for (int i = 0; i < 32; i++)
				{
					for (int j = 0; j < 32; j++)
					{
						value.cols[i * 32 + j] = new Color(0f, 0f, 0f, 0f);
					}
				}
				dictionary.Add(num9 + "," + num10, value);
			}
			int num11 = (int)(item.x - (float)(num9 * 32));
			int num12 = (int)(item.y - (float)(num10 * 32));
			dictionary[num9 + "," + num10].cols[num12 * 32 + num11] = paint_color;
		}
		foreach (KeyValuePair<string, partially_drawn_segment> item2 in dictionary)
		{
			segment_textures[item2.Value.x, item2.Value.y].SetPixels(item2.Value.cols);
			segment_textures[item2.Value.x, item2.Value.y].Apply();
			segment_images[item2.Value.x, item2.Value.y].sprite = Sprite.Create(segment_textures[item2.Value.x, item2.Value.y], new Rect(0f, 0f, 32f, 32f), Vector2.zero);
		}
		CreateUndoCanvasFromWorkArea();
	}

	private void TryAddPixel(int x, int y, List<Vector2> list, bool[,] checked_map)
	{
		if (x >= 0 && y >= 0 && x < n_horizontal_segs * 32 && y < n_vertical_segs * 32 && !checked_map[x, y] && !list.Contains(new Vector2(x, y)))
		{
			list.Add(new Vector2(x, y));
		}
	}

	public void PaintStroke(Texture2D pen_tex, float stroke_fill, Color paint_color)
	{
		Vector2 vector = CalcScaledMousePixel(prev_mouse_percent_x, prev_mouse_percent_y);
		Vector2 vector2 = CalcScaledMousePixel(curr_mouse_percent_x, curr_mouse_percent_y);
		Dictionary<string, partially_drawn_segment> dictionary = new Dictionary<string, partially_drawn_segment>();
		float num = Vector2.Distance(vector, vector2);
		int num2 = (int)(num / stroke_fill);
		if (num2 < 2)
		{
			num2 = 1;
		}
		Vector2 normalized = (vector2 - vector).normalized;
		float num3 = num / (float)num2;
		for (int i = 0; i <= num2; i++)
		{
			int x = (int)((vector.x + (float)pen_tex.width * 0.5f) * 0.03125f);
			int x2 = (int)((vector.x - (float)pen_tex.width * 0.5f) * 0.03125f);
			int y = (int)((vector.y + (float)pen_tex.height * 0.5f) * 0.03125f);
			int y2 = (int)((vector.y - (float)pen_tex.height * 0.5f) * 0.03125f);
			Dictionary<string, segment_to_paint_on> dictionary2 = new Dictionary<string, segment_to_paint_on>();
			AddSegmentToPaintList((int)(vector.x * 0.03125f), (int)(vector.y * 0.03125f), dictionary2);
			AddSegmentToPaintList(x, y2, dictionary2);
			AddSegmentToPaintList(x, y, dictionary2);
			AddSegmentToPaintList(x2, y, dictionary2);
			AddSegmentToPaintList(x2, y2, dictionary2);
			Color[] pixels = pen_tex.GetPixels();
			foreach (KeyValuePair<string, segment_to_paint_on> item in dictionary2)
			{
				segment_to_paint_on value = item.Value;
				if (!dictionary.ContainsKey(value.x + "," + value.y))
				{
					dictionary.Add(value.x + "," + value.y, new partially_drawn_segment
					{
						x = value.x,
						y = value.y,
						cols = segment_textures[value.x, value.y].GetPixels()
					});
				}
				int num4 = (int)(vector.x - (float)(value.x * 32) - (float)(pen_tex.width / 2));
				int num5 = (int)(vector.y - (float)(value.y * 32) - (float)(pen_tex.height / 2));
				int num6 = num4 + num5 * 32;
				for (int j = 0; j < pen_tex.height; j++)
				{
					for (int k = 0; k < pen_tex.width; k++)
					{
						if (pixels[k + j * pen_tex.width] == Color.white && (uint)((num4 + k) | (j + num5)) < 32u)
						{
							dictionary[value.x + "," + value.y].cols[num6 + k] = paint_color;
							create_undo_canvas = true;
						}
					}
					num6 += 32;
				}
			}
			vector.x += num3 * normalized.x;
			vector.y += num3 * normalized.y;
		}
		foreach (KeyValuePair<string, partially_drawn_segment> item2 in dictionary)
		{
			segment_textures[item2.Value.x, item2.Value.y].SetPixels(item2.Value.cols);
			segment_textures[item2.Value.x, item2.Value.y].Apply();
			segment_images[item2.Value.x, item2.Value.y].sprite = Sprite.Create(segment_textures[item2.Value.x, item2.Value.y], new Rect(0f, 0f, 32f, 32f), Vector2.zero);
		}
	}
}
