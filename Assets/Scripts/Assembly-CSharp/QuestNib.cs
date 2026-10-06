using UnityEngine;

public class QuestNib : MonoBehaviour
{
	public int index;

	public void Click()
	{
		WindowPrefabsControl.Instance.GetScreen("QUESTS").GetComponent<Scrollable>().TryClickNib(index, QuestControl.Instance.SucceedClickQuestNib);
	}
}
