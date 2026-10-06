using UnityEngine;

public class FurnitureEditButtons : MonoBehaviour
{
	public void MouseClick()
	{
		if (!PopupControl.Instance.GetButtonWasPressed())
		{
			PopupControl.Instance.SetButtonWasPressed();
			ConstructionControl.Instance.RotateMouseObj();
		}
	}

	public void DeleteClicked()
	{
		if (!PopupControl.Instance.GetButtonWasPressed())
		{
			PopupControl.Instance.SetButtonWasPressed();
			ConstructionControl.Instance.DeleteMouseObject();
		}
	}

	public void AcceptClicked()
	{
		if (!PopupControl.Instance.GetButtonWasPressed())
		{
			PopupControl.Instance.SetButtonWasPressed();
			ConstructionControl.Instance.ClickAcceptBuild();
		}
	}
}
