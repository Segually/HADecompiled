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

	private List<undo_canvas_struct> undo_canvases;

	public Transform undo_canvases_parent;

	public Transform workspace_segments_parent;

	private List<zoom_undo> zoom_undos;

	public Image[,] segment_images;

	public Texture2D[,] segment_textures;

	public static int n_horizontal_segs;

	public static int n_vertical_segs;

	private float[] stroke_fills;

	private int n_undos;

	public Image onion_skins_button;

	private bool onion_skins_enabled;

	private int curr_frame;

	public Image save_screen_preview;

	public GameObject speed_slider;

	private bool preview_frame_2;

	private IEnumerator animated_preview;

	private Color col_frame_button_selected;

	private Color col_frame_button_deselected;

	public Image frame_1_button;

	public Image frame_2_button;

	public Text speed_text;

	private int speed;

	public InputField input_save_title;

	public InputField input_save_description;

	private bool dev_frame_1_loaded;

	private bool dev_frame_2_loaded;

	public bool save_screen_open;

	public bool load_screen_open;

	private string loaded_painting_title;

	private string loaded_painting_desc;

	private Color paint_color;

	private bool allow_input;

	public GameObject[] hardcoded_color_swatches;

	public GameObject[] custom_color_swatches;

	private List<Color> custom_colors;

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

	private int prev_selected_tool_id;

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

	private float zoom_amount;

	public void PressOnionSkin()
	{
	}

	public void PressFrame(int index)
	{
	}

	public void PressNew()
	{
	}

	public void PressSave()
	{
	}

	private bool IsCanvasBlank(Texture2D tex)
	{
		return false;
	}

	private IEnumerator AnimatePreview()
	{
		return null;
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
	}

	private void MergeAllUndoCanvases()
	{
	}

	public void OnOpen()
	{
	}

	private void OpenPaintingSelectScreen(string text)
	{
	}

	public void ChangeSpeed(int dir)
	{
	}

	public void PressSaveCancel()
	{
	}

	public void PressSaveAccept()
	{
	}

	public void FinalizeLoad(int load_inv_slot)
	{
	}

	private Sprite CloneDevSprite(Texture2D tex)
	{
		return null;
	}

	private void OnDestroy()
	{
	}

	private void SavePaintingFrame(Texture2D canvas, string counter, string prefix, ExtraInventoryData extra_data)
	{
	}

	public void FinalizeSave(int save_inv_slot)
	{
	}

	public void PressBackOnSaveLoad()
	{
	}

	public void PressLoad()
	{
	}

	private void FillWhite(ref Texture2D tex)
	{
	}

	private void ClearWorkCanvas(int x, int y)
	{
	}

	private IEnumerator DelayedAllowInput()
	{
		return null;
	}

	public void AcceptCustomColor()
	{
	}

	public void AddCustomColor(Color C)
	{
	}

	public void PressColorNib(GameObject nib_obj)
	{
	}

	public void PressCustomColor()
	{
	}

	private void RedrawHueSlider()
	{
	}

	private void RedrawSaturationValueSlider()
	{
	}

	public void PressHueSlider()
	{
	}

	public void PressSatValSlider()
	{
	}

	public void PressToolNib(int index)
	{
	}

	public void PressUndo()
	{
	}

	private bool MouseWithinWorkArea()
	{
		return false;
	}

	private void FixedUpdate()
	{
	}

	private void CreateUndoCanvasFromWorkArea()
	{
	}

	private void Update()
	{
	}

	private float ColorDifference(Color col_A, Color col_B)
	{
		return 0f;
	}

	private bool TryFindCorrespondingSwatch(Color col)
	{
		return false;
	}

	private void CenterOn()
	{
	}

	private void UpdateZoom(float scale_amount)
	{
	}

	private void AddSegmentToPaintList(int x, int y, Dictionary<string, segment_to_paint_on> canvases_to_paint_on)
	{
	}

	private Vector2 CalcMousePercent()
	{
		return default(Vector2);
	}

	private Vector2 CalcScaledMousePixel(float mouse_percent_x, float mouse_percent_y)
	{
		return default(Vector2);
	}

	public void FloodFill()
	{
	}

	private void TryAddPixel(int x, int y, List<Vector2> list, bool[,] checked_map)
	{
	}

	public void PaintStroke(Texture2D pen_tex, float stroke_fill, Color paint_color)
	{
	}
}
