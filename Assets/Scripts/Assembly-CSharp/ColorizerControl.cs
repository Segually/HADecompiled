using UnityEngine;

public class ColorizerControl : MonoBehaviour, OrderedStart
{
	public static ColorizerControl Instance;

	public void Start_0()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	public void Start_1()
	{
	}

	public void ColorizeCurrentShackInterior()
	{
	}
}
