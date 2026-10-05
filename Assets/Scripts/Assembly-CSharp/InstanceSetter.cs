using UnityEngine;

public class InstanceSetter : MonoBehaviour
{
	public MonoBehaviour[] new_awake_objects;

	private void Start()
	{
		for (int i = 0; i < new_awake_objects.Length; i++)
		{
			if (new_awake_objects[i] != null)
			{
				((OrderedStart)new_awake_objects[i]).Start_0();
			}
		}
		for (int j = 0; j < new_awake_objects.Length; j++)
		{
			if (new_awake_objects[j] != null)
			{
				((OrderedStart)new_awake_objects[j]).Start_1();
			}
		}
	}
}
