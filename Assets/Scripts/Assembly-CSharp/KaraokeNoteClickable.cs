using UnityEngine;

public class KaraokeNoteClickable : MonoBehaviour
{
	public int when;

	public byte button_id;

	public void OnClick()
	{
		KaraokeControl.Instance.press_on_note(when, button_id);
	}
}
