using UnityEngine.UI;
using System.IO;
using TMPro;
using UnityEngine;

public class BanditCampsNewWindow : MonoBehaviour
{
	public TMP_InputField input_new_bandit_camp_name;

	private int curr_selected_dimensions;

	public GameObject button_2x2;

	public GameObject button_3x3;

	public GameObject button_4x4;

	private void Start()
	{
		SelectButton(button_2x2);
	}

	private void SelectButton(GameObject button)
	{
		button.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
		button.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = new Color(0f, 0f, 0f, 1f);
	}

	private void DeselectButton(GameObject button)
	{
		button.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
		button.transform.Find("Text").GetComponent<TextMeshProUGUI>().color = new Color(1f, 1f, 1f, 1f);
	}

	public void PressDimensionsButton(int id)
	{
		DeselectButton(DimensionIdToButton(curr_selected_dimensions));
		curr_selected_dimensions = id;
		SelectButton(DimensionIdToButton(curr_selected_dimensions));
	}

	private GameObject DimensionIdToButton(int id)
	{
		switch (id)
		{
		case 0:
			return button_2x2;
		case 1:
			return button_3x3;
		case 2:
			return button_4x4;
		default:
			return null;
		}
	}

	private void CreateOverviewFile(int w, int h, string new_directory)
	{
		File.WriteAllLines(Path.Combine(new_directory, "_overview.txt"), new string[2]
		{
			"w=" + w,
			"h=" + h
		});
	}

	private void CreateChunkFile(int x, int z, string new_directory)
	{
		File.WriteAllLines(Path.Combine(new_directory, "chunk(overworld," + x + ", " + z + ").txt"), new string[1] { "*is_blank=true*" });
	}

	public void PressOkay()
	{
		PopupControl.Instance.SetButtonWasPressed();
		string text = input_new_bandit_camp_name.text;
		if (string.IsNullOrWhiteSpace(text))
		{
			return;
		}
		// Changed so it works on any computer. The commented-out line below is how it looked on the developer's machine.
		string text2 = Path.Combine(Path.Combine(Application.dataPath, "SYNCHRONOUS/TextFiles/bandit-camps"), text);
		// string text2 = Path.Combine("C:\\Hybrid Animals Stuff\\Hybrid Animals Mobile\\Assets\\SYNCHRONOUS\\TextFiles\\bandit-camps", text);
		if (Directory.Exists(text2))
		{
			return;
		}
		Directory.CreateDirectory(text2);
		switch (curr_selected_dimensions)
		{
		case 2:
			CreateOverviewFile(4, 4, text2);
			CreateChunkFile(0, 0, text2);
			CreateChunkFile(0, 1, text2);
			CreateChunkFile(0, 2, text2);
			CreateChunkFile(0, 3, text2);
			CreateChunkFile(1, 0, text2);
			CreateChunkFile(1, 1, text2);
			CreateChunkFile(1, 2, text2);
			CreateChunkFile(1, 3, text2);
			CreateChunkFile(2, 0, text2);
			CreateChunkFile(2, 1, text2);
			CreateChunkFile(2, 2, text2);
			CreateChunkFile(2, 3, text2);
			CreateChunkFile(3, 0, text2);
			CreateChunkFile(3, 1, text2);
			CreateChunkFile(3, 2, text2);
			CreateChunkFile(3, 3, text2);
			break;
		case 1:
			CreateOverviewFile(3, 3, text2);
			CreateChunkFile(0, 0, text2);
			CreateChunkFile(0, 1, text2);
			CreateChunkFile(0, 2, text2);
			CreateChunkFile(1, 0, text2);
			CreateChunkFile(1, 1, text2);
			CreateChunkFile(1, 2, text2);
			CreateChunkFile(2, 0, text2);
			CreateChunkFile(2, 1, text2);
			CreateChunkFile(2, 2, text2);
			break;
		case 0:
			CreateOverviewFile(2, 2, text2);
			CreateChunkFile(0, 0, text2);
			CreateChunkFile(0, 1, text2);
			CreateChunkFile(1, 0, text2);
			CreateChunkFile(1, 1, text2);
			break;
		}
		DevBuildControl.Instance.StartBanditCampEditor(text);
		WindowPrefabsControl.Instance.DestroyScreen("dev - bandit new");
		DevBuildControl.Instance.disable_movement = false;
		DevBuildControl.Instance.ShowOrHideBaseDevElements(true);
	}

	public void PressCancel()
	{
		PopupControl.Instance.SetButtonWasPressed();
		WindowPrefabsControl.Instance.DestroyScreen("dev - bandit new");
		DevBuildControl.Instance.disable_movement = false;
		DevBuildControl.Instance.ShowOrHideBaseDevElements(true);
	}
}
