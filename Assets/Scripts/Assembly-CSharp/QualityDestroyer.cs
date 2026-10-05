using UnityEngine;

public class QualityDestroyer : MonoBehaviour
{
	public enum level
	{
		unknown = 0,
		_66_percent = 1,
		_100_percent = 2
	}

	public level only_show_at_level;

	private void Start()
	{
	}
}
