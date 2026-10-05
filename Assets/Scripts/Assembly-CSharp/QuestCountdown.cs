using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class QuestCountdown : MonoBehaviour
{
	public static QuestCountdown Instance;

	private AudioSource source;

	public AudioClip sfx_countdown;

	public AudioClip sfx_go;

	public AudioClip time_up;

	public Text countdown_text;

	public Text timer_text;

	public CanvasGroup game_elements;

	public int curr_kills;

	public void IncreaseKills()
	{
	}

	public void Init()
	{
	}

	public void StartQuestTimerCountDown(int time_limit, int goal_kills, string quest_name, int curr_step, int goto_success, int goto_failure)
	{
	}

	private IEnumerator StartQuestTimerCountDownCoroutine(int time_limit, int goal_kills, string quest_name, int curr_step, int goto_success, int goto_failure)
	{
		return null;
	}
}
