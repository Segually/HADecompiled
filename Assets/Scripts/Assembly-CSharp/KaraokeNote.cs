using UnityEngine;

public class KaraokeNote
{
	public bool dead;

	public byte button_id;

	public int timestamp;

	public GameObject obj;

	public KaraokeNote(byte button_id, int timestamp)
	{
		this.button_id = button_id;
		this.timestamp = timestamp;
	}
}
