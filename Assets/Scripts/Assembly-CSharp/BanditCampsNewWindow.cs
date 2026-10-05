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
	}

	private void SelectButton(GameObject button)
	{
	}

	private void DeselectButton(GameObject button)
	{
	}

	public void PressDimensionsButton(int id)
	{
	}

	private GameObject DimensionIdToButton(int id)
	{
		return null;
	}

	private void CreateOverviewFile(int w, int h, string new_directory)
	{
	}

	private void CreateChunkFile(int x, int z, string new_directory)
	{
	}

	public void PressOkay()
	{
	}

	public void PressCancel()
	{
	}
}
