using System.Collections;
using System.Collections.Generic;
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
		curr_kills++;
		WindowPrefabsControl.Instance.GetTextLegacy("QUEST KILL COUNT", "Kill Count").text = curr_kills.ToString() ?? "";
		WindowPrefabsControl.Instance.GetScreen("QUEST KILL COUNT").GetComponent<Animation>().Stop();
		WindowPrefabsControl.Instance.GetScreen("QUEST KILL COUNT").GetComponent<Animation>().Play();
	}

	public void Init()
	{
		Instance = this;
		source = gameObject.AddComponent<AudioSource>();
		countdown_text.GetComponent<CanvasGroup>().alpha = 0f;
		game_elements.alpha = 0f;
	}

	public void StartQuestTimerCountDown(int time_limit, int goal_kills, string quest_name, int curr_step, int goto_success, int goto_failure)
	{
		StartCoroutine(StartQuestTimerCountDownCoroutine(time_limit, goal_kills, quest_name, curr_step, goto_success, goto_failure));
	}

	private IEnumerator StartQuestTimerCountDownCoroutine(int time_limit, int goal_kills, string quest_name, int curr_step, int goto_success, int goto_failure)
	{
		yield return new WaitForSeconds(1.8f);
		countdown_text.text = "<color=#eeeeee>Squish</color> " + goal_kills + "\n<color=#eeeeee>Rat-Roaches</color>";
		GetComponent<Animation>().Play("count-down-slow");
		yield return new WaitForSeconds(3.5f);
		for (int i = 3; i >= 0; i--)
		{
			GetComponent<Animation>().Stop();
			if (i == 0)
			{
				countdown_text.text = "<size=70>GO!</size>";
				GetComponent<Animation>().Play("count-down-GO");
				source.PlayOneShot(sfx_go, AudioControl.Instance.general_sfx_volume);
				Dictionary<string, string> step = QuestControl.Instance.GetQuest(quest_name).steps[curr_step];
				if (step.ContainsKey("Soundtrack")) AudioControl.Instance.PlayBattleMusic(step["Soundtrack"]);
				WindowPrefabsControl.Instance.CreateScreen("QUEST KILL COUNT", WindowPrefabsControl.build_into_t.GAME_CTR);
				GameplayGUIControl.Instance.ShowGameplayGui();
				GameController.Instance.UNPAUSE_GAME();
			}
			else
			{
				countdown_text.text = "<size=70>" + i + "</size>";
				GetComponent<Animation>().Play("count-down");
				source.PlayOneShot(sfx_countdown, AudioControl.Instance.general_sfx_volume);
			}
			yield return new WaitForSeconds(1f);
		}
		yield return new WaitForSeconds(1f);
		timer_text.text = (time_limit - 2).ToString() ?? "";
		GetComponent<Animation>().Stop();
		GetComponent<Animation>().Play("count-down-show-timer");
		for (int i = time_limit - 3; i >= 0; i--)
		{
			for (int j = 0; j < 10;)
			{
				yield return new WaitForSeconds(0.1f);
				if (GameController.Instance.player == null || GameController.Instance.is_paused()) continue;
				j++;
			}
			timer_text.text = i.ToString() ?? "";
		}
		GetComponent<Animation>().Stop();
		countdown_text.text = "Time's up!";
		GetComponent<Animation>().Play("count-down-slow");
		source.PlayOneShot(time_up, AudioControl.Instance.general_sfx_volume);
		game_elements.alpha = 0f;
		WindowPrefabsControl.Instance.DestroyScreen("QUEST KILL COUNT");
		GameController.Instance.PAUSE_GAME();
		GameplayGUIControl.Instance.HideGameplayGui();
		yield return new WaitForSeconds(1.5f);
		QuestControl.Instance.doing_time_trial = false;
		QuestControl.Instance.SetQuestProgress(quest_name, curr_kills < goal_kills ? goto_failure : goto_success);
		yield return new WaitForSeconds(3f);
		WindowPrefabsControl.Instance.DestroyScreen("QUEST COUNTDOWN");
	}
}
