using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PoolGameControl : MonoBehaviour
{
	public enum GamePhase_t
	{
		menu = 0,
		your_turn_intro = 1,
		your_turn = 2,
		your_turn_animated_hit = 3,
		your_turn_waitForBallsToStop = 4,
		you_scratched = 5,
		CPU_turn_intro = 6,
		CPU_turn_think = 7,
		CPU_turn_animated_hit = 8,
		CPU_turn_waitForBallsToStop = 9,
		CPU_scratched = 10,
		game_over_loss = 11,
		game_over_win = 12,
		play_again = 13,
		MP_game_intro_otherFirst = 14,
		MP_game_intro_selfFirst = 15,
		MP_other_turn_intro = 16,
		MP_other_turn_SYNC_WAIT = 17,
		MP_other_turn = 18,
		MP_other_turn_animated_hit = 19,
		MP_other_turn_waitForBallsToStop = 20,
		MP_other_scratched = 21,
		MP_self_turn_intro = 22,
		MP_self_turn_SYNC_WAIT = 23,
		MP_self_turn = 24,
		MP_self_turn_animated_hit = 25,
		MP_self_turn_waitForBallsToStop = 26,
		MP_self_scratched = 27,
		MP_game_over_loss = 28,
		MP_game_over_win = 29,
		MP_play_again = 30,
		MP_play_again_SYNC_WAIT = 31
	}

	public enum team_t
	{
		undefined = 0,
		blue = 1,
		green = 2
	}

	public enum touched
	{
		unknown = 0,
		wall = 1,
		CPU_ball = 2,
		player_ball = 3
	}

	public enum accuracy
	{
		miss = 0,
		hit = 1
	}

	public static PoolGameControl Instance;

	public PoolBall[] real_pool_balls;

	public PoolBallSimulated[] real_pool_balls_physics;

	public Transform[] holes;

	public GameObject clicker;

	public GameObject pool_cue;

	public Transform pool_cue_rotator;

	public GameObject collider_quad;

	public LineRenderer lineA;

	public LineRenderer lineB;

	public LineRenderer lineC;

	public Gradient power_meter_gradient;

	public GameObject error_prefab;

	public GameObject sink_particle_prefab;

	public static int bound_x = 229;

	public static int bound_y = 111;

	public AudioClip sfx_wrong_sink;

	public AudioClip sfx_good_sink_1;

	public AudioClip sfx_good_sink_2;

	public AudioClip sfx_good_sink_3;

	public AudioClip sfx_game_start;

	public AudioClip sfx_bad_move;

	public AudioClip sfx_good_move;

	public AudioClip sfx_win;

	public AudioClip sfx_lose;

	public string curr_opponent = "";

	private bool is_host;

	public GamePhase_t GamePhase;

	private bool pressed_power_slider;

	public team_t player_team;

	public Material green_mat;

	public Material blue_mat;

	public bool show_team_notif;

	private int MP_stream_cue_position_counter;

	private float MP_stream_cue_nextDegree;

	public bool sink_wrong_ball_OK;

	public int n_correct_balls_sunk;

	public bool white_was_sunk;

	public bool sunk_wrong_ball_;

	private float power = 0.5f;

	private int particle_sound_iterator;

	public static float pool_ball_w = 25f;

	public touched white_first_touched;

	public static int max_power = 2300;

	public AudioSource pool_cue_audio;

	public PoolGameRecording curr_recording;

	public float MP_next_power;

	public float MP_next_deg;

	public PoolGameRecording MP_next_recording;

	public int curr_pool_table_id;

	public bool other_player_ready;

	public Text thinking_text;

	public List<accuracy> CPU_accuracy_schedule = new List<accuracy>();

	public bool show_ad_on_close;

	public void AnmIntroComplete()
	{
		if (curr_opponent == "")
		{
			MinigameMenu.Instance.CPU_head.sprite = MinigameMenu.Instance.spr_gameguy_head;
			MinigameMenu.Instance.CPU_name.text = "Play vs\nGame Guy";
			MinigameMenu.Instance.ShowMenu(MinigameMenu.menu_type_t.pick_mode);
		}
		else
		{
			StartMpGame(false);
		}
	}

	public void StartMpGame(bool is_host)
	{
		this.is_host = is_host;
		MinigameMenu.Instance.ShowMenu(MinigameMenu.menu_type_t.in_game_multiplayer);
		if (is_host)
		{
			GamePhase = GamePhase_t.MP_game_intro_selfFirst;
			MinigameMenu.Instance.ShowNotif("<color=#43de4f>" + curr_opponent + "</color>\njoined!");
		}
		else
		{
			GamePhase = GamePhase_t.MP_game_intro_otherFirst;
			MinigameMenu.Instance.ShowNotif("Playing against\n<color=#43de4f>" + curr_opponent + "</color>");
		}
		if (sfx_game_start != null)
		{
			GetComponent<AudioSource>().PlayOneShot(sfx_game_start, AudioControl.Instance.general_sfx_volume * 0.5f);
		}
	}

	public void RestartMpGame()
	{
		thinking_text.gameObject.SetActive(false);
		if (is_host)
		{
			GamePhase = GamePhase_t.MP_self_turn_intro;
			MinigameMenu.Instance.ShowNotif("You go first!");
		}
		else
		{
			GamePhase = GamePhase_t.MP_other_turn_intro;
			MinigameMenu.Instance.ShowNotif("<color=#43de4f>" + curr_opponent + "</color>\ngoes first!");
		}
		if (sfx_game_start != null)
		{
			GetComponent<AudioSource>().PlayOneShot(sfx_game_start, AudioControl.Instance.general_sfx_volume * 0.5f);
		}
	}

	public void ArrangeBalls(int[] placements)
	{
		List<Vector2> list = new List<Vector2>();
		list.Add(new Vector2(56f, 0f));
		list.Add(new Vector2(56f + pool_ball_w, 0f - pool_ball_w * 0.5f));
		list.Add(new Vector2(56f + pool_ball_w, 0f + pool_ball_w * 0.5f));
		list.Add(new Vector2(56f + pool_ball_w * 2f, 0f - pool_ball_w));
		list.Add(new Vector2(56f + pool_ball_w * 2f, 0f));
		list.Add(new Vector2(56f + pool_ball_w * 2f, 0f + pool_ball_w));
		list.Add(new Vector2(56f + pool_ball_w * 3f, 0f - pool_ball_w * 1.5f));
		list.Add(new Vector2(56f + pool_ball_w * 3f, 0f - pool_ball_w * 0.5f));
		list.Add(new Vector2(56f + pool_ball_w * 3f, 0f + pool_ball_w * 0.5f));
		list.Add(new Vector2(56f + pool_ball_w * 3f, 0f + pool_ball_w * 1.5f));
		list.Add(new Vector2(56f + pool_ball_w * 4f, 0f - pool_ball_w * 2f));
		list.Add(new Vector2(56f + pool_ball_w * 4f, 0f - pool_ball_w));
		list.Add(new Vector2(56f + pool_ball_w * 4f, 0f + pool_ball_w));
		list.Add(new Vector2(56f + pool_ball_w * 4f, 0f + pool_ball_w * 2f));
		real_pool_balls_physics = new PoolBallSimulated[real_pool_balls.Length];
		for (int i = 0; i < real_pool_balls.Length; i++)
		{
			real_pool_balls[i].gameObject.SetActive(true);
			if (i == 0)
			{
				real_pool_balls[i].transform.localPosition = new Vector2(-127f, 0f);
			}
			else
			{
				real_pool_balls[i].transform.localPosition = list[placements[i - 1]];
			}
			real_pool_balls_physics[i] = real_pool_balls[i].physics;
			real_pool_balls_physics[i].localPosition = real_pool_balls[i].transform.localPosition;
			real_pool_balls_physics[i].velocity_mag_ = 0;
			real_pool_balls_physics[i].ball_id = i;
			real_pool_balls_physics[i].is_sunk = false;
			real_pool_balls_physics[i].team = real_pool_balls[i].team;
		}
	}

	private void RedrawPowerSlider()
	{
		Image image = WindowPrefabsControl.Instance.GetImage("POOL GAME - lower", "power_slider");
		Text textLegacy = WindowPrefabsControl.Instance.GetTextLegacy("POOL GAME - lower", "power_text");
		float x = Mathf.Lerp(-77f, 0f, power);
		image.rectTransform.sizeDelta = new Vector2(power * 153f, 23f);
		image.rectTransform.anchoredPosition = new Vector2(x, 0f);
		image.color = power_meter_gradient.Evaluate(power);
		textLegacy.text = (int)(power * 100f) + "% STRENGTH";
	}

	public void PressLowerBackground()
	{
		PopupControl.Instance.SetButtonWasPressed();
	}

	public void PressPowerSlider()
	{
		pressed_power_slider = true;
		PopupControl.Instance.SetButtonWasPressed();
	}

	public void ReleasePowerSlider()
	{
		pressed_power_slider = false;
	}

	public void SetPlayerTeam(PoolBall ball)
	{
		show_team_notif = true;
		sink_wrong_ball_OK = true;
		n_correct_balls_sunk++;
		switch (GamePhase)
		{
		case GamePhase_t.your_turn_animated_hit:
		case GamePhase_t.your_turn_waitForBallsToStop:
		case GamePhase_t.MP_self_turn_animated_hit:
		case GamePhase_t.MP_self_turn_waitForBallsToStop:
			if (ball.team == team_t.blue)
			{
				player_team = team_t.blue;
			}
			else if (ball.team == team_t.green)
			{
				player_team = team_t.green;
			}
			break;
		case GamePhase_t.CPU_turn_animated_hit:
		case GamePhase_t.CPU_turn_waitForBallsToStop:
		case GamePhase_t.MP_other_turn_animated_hit:
		case GamePhase_t.MP_other_turn_waitForBallsToStop:
			if (ball.team == team_t.blue)
			{
				player_team = team_t.green;
			}
			else if (ball.team == team_t.green)
			{
				player_team = team_t.blue;
			}
			break;
		}
	}

	public string GetPrefix(bool on_cpu_sink)
	{
		switch (n_correct_balls_sunk)
		{
		case 1:
		{
			if (on_cpu_sink)
			{
				return "";
			}
			float value = Random.value;
			if (value < 0.2f)
			{
				return "<color=#ffe712>Good Shot!</color>\n";
			}
			if (value < 0.4f)
			{
				return "<color=#ffe712>Epic Skillz!</color>\n";
			}
			if (value < 0.6f)
			{
				return "<color=#ffe712>Nice Shot!</color>\n";
			}
			if (value < 0.8f)
			{
				return "<color=#ffe712>Well Done!</color>\n";
			}
			return "<color=#ffe712>Great Shot!</color>\n";
		}
		case 2:
			return "<color=#b482ff>Double Sink!</color>\n";
		case 3:
			return "<color=#ff3ba0>TRIPLE SINK!</color>\n";
		case 4:
			return "<color=#ff3ba0>QUADRUPLE SINK!</color>\n";
		default:
			return "<color=#ff3ba0>X" + n_correct_balls_sunk + " BALLS SUNK</color>\n";
		}
	}

	private void YouLost(bool MP)
	{
		WindowPrefabsControl.Instance.DestroyScreen("POOL GAME - upper");
		if (MP)
		{
			GamePhase = GamePhase_t.MP_game_over_loss;
			MinigameMenu.Instance.ShowNotif("<color=#43de4f>" + curr_opponent + "</color>\nWins!");
		}
		else
		{
			GamePhase = GamePhase_t.game_over_loss;
			MinigameMenu.Instance.ShowNotif("Game Guy\nWins!");
		}
		if (sfx_lose != null)
		{
			GetComponent<AudioSource>().PlayOneShot(sfx_lose, AudioControl.Instance.general_sfx_volume);
		}
	}

	private void YouWon(bool MP)
	{
		WindowPrefabsControl.Instance.DestroyScreen("POOL GAME - upper");
		GamePhase = (MP ? GamePhase_t.MP_game_over_win : GamePhase_t.game_over_win);
		MinigameMenu.Instance.ShowNotif("You Win!");
		if (sfx_win != null)
		{
			GetComponent<AudioSource>().PlayOneShot(sfx_win, AudioControl.Instance.general_sfx_volume);
		}
	}

	private void CpuTurnIntro(bool with_score_prefix)
	{
		GamePhase = GamePhase_t.CPU_turn_intro;
		MinigameMenu.Instance.ShowNotif(with_score_prefix ? (GetPrefix(true) + "Game Guy goes again!") : "Game Guy's\nturn!");
	}

	private void YourTurnIntro(bool with_score_prefix, bool MP)
	{
		GamePhase = (MP ? GamePhase_t.MP_self_turn_intro : GamePhase_t.your_turn_intro);
		if (with_score_prefix)
		{
			MinigameMenu.Instance.ShowNotif(GetPrefix(false) + "You go again!");
			GetComponent<AudioSource>().PlayOneShot(sfx_good_move, AudioControl.Instance.general_sfx_volume * 0.8f);
		}
		else
		{
			MinigameMenu.Instance.ShowNotif("Your turn!");
		}
	}

	private void OpponentTurnIntro(bool with_score_prefix)
	{
		GamePhase = GamePhase_t.MP_other_turn_intro;
		if (with_score_prefix)
		{
			MinigameMenu.Instance.ShowNotif(GetPrefix(true) + "<color=#43de4f>" + curr_opponent + "</color> goes again!");
		}
		else
		{
			MinigameMenu.Instance.ShowNotif("<color=#43de4f>" + curr_opponent + "'s</color>\nturn!");
		}
	}

	private void FixedUpdate()
	{
		if (curr_recording != null && curr_recording.is_playing)
		{
			curr_recording.PopFrame();
		}
		if (GamePhase == GamePhase_t.your_turn_waitForBallsToStop || GamePhase == GamePhase_t.CPU_turn_waitForBallsToStop || GamePhase == GamePhase_t.MP_other_turn_waitForBallsToStop || GamePhase == GamePhase_t.MP_self_turn_waitForBallsToStop)
		{
			if (curr_recording != null && curr_recording.frames.Count == 0)
			{
				curr_recording = null;
			}
			if (curr_recording == null)
			{
				if (white_was_sunk)
				{
					switch (GamePhase)
					{
					case GamePhase_t.your_turn_waitForBallsToStop:
						GamePhase = GamePhase_t.you_scratched;
						MinigameMenu.Instance.ShowNotif("<color=#ff0000>You sunk the\nWhite Ball!</color>");
						break;
					case GamePhase_t.CPU_turn_waitForBallsToStop:
						GamePhase = GamePhase_t.CPU_scratched;
						MinigameMenu.Instance.ShowNotif("<color=#ff0000>Game Guy sunk\nthe White Ball!</color>");
						break;
					case GamePhase_t.MP_other_turn_waitForBallsToStop:
						GamePhase = GamePhase_t.MP_other_scratched;
						MinigameMenu.Instance.ShowNotif("<color=#ff0000>" + curr_opponent + " sunk\nthe White Ball!</color>");
						break;
					case GamePhase_t.MP_self_turn_waitForBallsToStop:
						GamePhase = GamePhase_t.MP_self_scratched;
						MinigameMenu.Instance.ShowNotif("<color=#ff0000>You sunk the\nWhite Ball!</color>");
						break;
					}
					GetComponent<AudioSource>().PlayOneShot(sfx_bad_move, AudioControl.Instance.general_sfx_volume * 0.5f);
				}
				else if (n_correct_balls_sunk > 0 && (sink_wrong_ball_OK || !sunk_wrong_ball_))
				{
					switch (GamePhase)
					{
					case GamePhase_t.your_turn_waitForBallsToStop:
						if (NumBallsRemaining(player_team) == 0)
						{
							YouWon(false);
						}
						else
						{
							YourTurnIntro(true, false);
						}
						break;
					case GamePhase_t.CPU_turn_waitForBallsToStop:
						if (NumBallsRemaining((player_team == team_t.green) ? team_t.blue : team_t.green) == 0)
						{
							YouLost(false);
						}
						else
						{
							CpuTurnIntro(true);
						}
						break;
					case GamePhase_t.MP_other_turn_waitForBallsToStop:
						if (NumBallsRemaining((player_team == team_t.green) ? team_t.blue : team_t.green) == 0)
						{
							YouLost(true);
						}
						else
						{
							OpponentTurnIntro(true);
						}
						break;
					case GamePhase_t.MP_self_turn_waitForBallsToStop:
						if (NumBallsRemaining(player_team) == 0)
						{
							YouWon(true);
						}
						else
						{
							YourTurnIntro(true, true);
						}
						break;
					}
				}
				else if (sunk_wrong_ball_)
				{
					switch (GamePhase)
					{
					case GamePhase_t.your_turn_waitForBallsToStop:
						GamePhase = GamePhase_t.you_scratched;
						MinigameMenu.Instance.ShowNotif("<color=#ff0000>You sunk the\nWrong Color!</color>");
						break;
					case GamePhase_t.CPU_turn_waitForBallsToStop:
						GamePhase = GamePhase_t.CPU_scratched;
						MinigameMenu.Instance.ShowNotif("<color=#ff0000>Game Guy sunk\nthe Wrong Color!</color>");
						break;
					case GamePhase_t.MP_other_turn_waitForBallsToStop:
						GamePhase = GamePhase_t.MP_other_scratched;
						MinigameMenu.Instance.ShowNotif("<color=#ff0000>" + curr_opponent + " sunk\nthe Wrong Color!</color>");
						break;
					case GamePhase_t.MP_self_turn_waitForBallsToStop:
						GamePhase = GamePhase_t.MP_self_scratched;
						MinigameMenu.Instance.ShowNotif("<color=#ff0000>You sunk the\nWrong Color!</color>");
						break;
					}
					GetComponent<AudioSource>().PlayOneShot(sfx_bad_move, AudioControl.Instance.general_sfx_volume * 0.5f);
				}
				else
				{
					switch (GamePhase)
					{
					case GamePhase_t.your_turn_waitForBallsToStop:
						CpuTurnIntro(false);
						break;
					case GamePhase_t.CPU_turn_waitForBallsToStop:
						YourTurnIntro(false, false);
						break;
					case GamePhase_t.MP_other_turn_waitForBallsToStop:
						YourTurnIntro(false, true);
						break;
					case GamePhase_t.MP_self_turn_waitForBallsToStop:
						OpponentTurnIntro(false);
						break;
					}
				}
			}
		}
		if (GamePhase == GamePhase_t.MP_other_turn)
		{
			Vector3 localPosition = real_pool_balls[0].transform.localPosition;
			float f = MP_stream_cue_nextDegree * 0.017453292f;
			clicker.transform.localPosition = Vector3.Lerp(clicker.transform.localPosition, localPosition + new Vector3(Mathf.Sin(f), Mathf.Cos(f), 0f), Time.fixedDeltaTime * 4f);
			RedrawPoolCue();
		}
		else if (GamePhase == GamePhase_t.MP_self_turn)
		{
			if (MP_stream_cue_position_counter == 0)
			{
				GameServerSender.Instance.SendUpdatePoolCuePosition();
				MP_stream_cue_position_counter = 25;
			}
			MP_stream_cue_position_counter--;
		}
	}

	private int NumBallsRemaining(team_t team)
	{
		int num = 0;
		for (int i = 0; i < real_pool_balls_physics.Length; i++)
		{
			if (i != 0 && real_pool_balls[i].team == team && !real_pool_balls_physics[i].is_sunk)
			{
				num++;
			}
		}
		return num;
	}

	private void Update()
	{
		if (GamePhase != GamePhase_t.MP_self_turn && GamePhase != GamePhase_t.your_turn)
		{
			return;
		}
		Vector3 mousePosition = GamepadInput.Instance.GetMousePosition();
		if (pressed_power_slider)
		{
			power = Mathf.Clamp01(((mousePosition.x - (float)Screen.width * 0.5f) / WindowControl.Instance.gui_canvas.scaleFactor + 141f) / 154f);
			if (power <= 0.15f)
			{
				power = 0.15f;
			}
			RedrawPowerSlider();
		}
		else if (GamepadInput.Instance.GetMouseButton() && !PopupControl.Instance.GetButtonWasPressed())
		{
			clicker.transform.localPosition = new Vector2((mousePosition.x - (float)Screen.width * 0.5f) / WindowControl.Instance.gui_canvas.scaleFactor, (mousePosition.y - (float)Screen.height * 0.5f) / WindowControl.Instance.gui_canvas.scaleFactor);
			RedrawPoolCue();
		}
	}

	public void CreateError(Vector2 localPosition)
	{
		GameObject gameObject = Object.Instantiate(error_prefab);
		gameObject.transform.SetParent(real_pool_balls[0].transform.parent);
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localPosition = localPosition;
		gameObject.SetActive(true);

		GetComponent<AudioSource>().PlayOneShot(sfx_wrong_sink, AudioControl.Instance.general_sfx_volume);
	}

	public void CreateParticle(Vector2 localPosition)
	{
		GameObject gameObject = Object.Instantiate(sink_particle_prefab);
		gameObject.transform.SetParent(real_pool_balls[0].transform.parent);
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localPosition = localPosition;
		gameObject.SetActive(true);

		switch (particle_sound_iterator)
		{
		case 0:
			GetComponent<AudioSource>().PlayOneShot(sfx_good_sink_1, AudioControl.Instance.general_sfx_volume);
			particle_sound_iterator++;
			break;
		case 1:
			GetComponent<AudioSource>().PlayOneShot(sfx_good_sink_2, AudioControl.Instance.general_sfx_volume);
			particle_sound_iterator++;
			break;
		case 2:
			GetComponent<AudioSource>().PlayOneShot(sfx_good_sink_3, AudioControl.Instance.general_sfx_volume);
			break;
		}
	}

	private void NewTurn()
	{
		if (real_pool_balls_physics[0].is_sunk)
		{
			RandomlyPlaceWhiteBall();
		}
		n_correct_balls_sunk = 0;
		white_was_sunk = false;
		sunk_wrong_ball_ = false;
		sink_wrong_ball_OK = false;
		particle_sound_iterator = 0;
		RedrawPoolCue();
	}

	private void GenerateAccuracySchedule(int miss_X_in_7)
	{
		CPU_accuracy_schedule = new List<accuracy>();
		List<int> list = new List<int>();
		for (int i = 0; i < 7; i++)
		{
			CPU_accuracy_schedule.Add(accuracy.hit);
			list.Add(i);
		}
		for (int j = 0; j < miss_X_in_7; j++)
		{
			int index = Random.Range(0, list.Count);
			int index2 = list[index];
			list.RemoveAt(index);
			CPU_accuracy_schedule[index2] = accuracy.miss;
		}
	}

	public void TryUpdateCuePosition(float deg)
	{
		if (GamePhase == GamePhase_t.MP_other_turn)
		{
			MP_stream_cue_nextDegree = deg;
		}
	}

	private IEnumerator BeginThinking()
	{
		thinking_text.gameObject.SetActive(true);
		bool can_scratch = true;
		bool must_bounce_off_myBall_first = false;
		PoolGameRecording recording = new PoolGameRecording();
		int miss_X_in_ = 7;
		switch (MinigameMenu.Instance.curr_difficulty)
		{
		case MinigameMenu.CPU_difficulty.easy:
			miss_X_in_ = 5;
			can_scratch = true;
			must_bounce_off_myBall_first = true;
			break;
		case MinigameMenu.CPU_difficulty.normal:
			miss_X_in_ = 4;
			can_scratch = true;
			must_bounce_off_myBall_first = true;
			break;
		case MinigameMenu.CPU_difficulty.hard:
			miss_X_in_ = 1;
			can_scratch = false;
			must_bounce_off_myBall_first = true;
			break;
		case MinigameMenu.CPU_difficulty.impossible:
			miss_X_in_ = 0;
			can_scratch = false;
			must_bounce_off_myBall_first = false;
			break;
		}
		int iterations_so_far = 0;
		int total_iterations = 1200;
		float best_deg = 0f;
		float best_pow = 0f;
		bool stop_thinking = false;
		if (CPU_accuracy_schedule.Count == 0)
		{
			GenerateAccuracySchedule(miss_X_in_);
		}
		accuracy hit_or_miss = CPU_accuracy_schedule[0];
		CPU_accuracy_schedule.RemoveAt(0);
		for (int p = 0; p < 5; p++)
		{
			float pow = (float)p * 0.1f + 0.3f;
			for (int d = 0; d < 240; d++)
			{
				float num = ((d >= 120) ? ((float)(d - 120) * 3f + 1.5f) : ((float)d * 3f));
				int num2 = SimulateShot(num, pow, must_bounce_off_myBall_first, can_scratch, recording, false);
				if ((hit_or_miss == accuracy.hit && num2 > 0) || (hit_or_miss == accuracy.miss && num2 == 0))
				{
					best_deg = num;
					stop_thinking = true;
					best_pow = pow;
					break;
				}
				if (iterations_so_far % 4 == 0)
				{
					int num3 = Mathf.Min((int)((float)iterations_so_far / (float)total_iterations * 100f), 100);
					thinking_text.text = "... Thinking (" + num3 + "%) ...";
					yield return new WaitForEndOfFrame();
				}
				iterations_so_far++;
			}
			if (stop_thinking)
			{
				break;
			}
		}
		thinking_text.gameObject.SetActive(false);
		if (stop_thinking)
		{
			curr_recording = recording;
			power = best_pow;
			float f = best_deg * 0.017453292f;
			clicker.transform.localPosition = real_pool_balls[0].transform.localPosition + new Vector3(Mathf.Sin(f), Mathf.Cos(f), 0f);
		}
		else
		{
			power = 0.5f;
			Vector3 vector = Vector3.zero;
			float num4 = float.MaxValue;
			for (int i = 0; i < real_pool_balls_physics.Length; i++)
			{
				PoolBallSimulated poolBallSimulated = real_pool_balls_physics[i];
				if (poolBallSimulated.team != team_t.undefined && poolBallSimulated.team != player_team && !poolBallSimulated.is_sunk)
				{
					float num5 = Vector3.Distance(real_pool_balls_physics[0].localPosition, poolBallSimulated.localPosition);
					if (num5 < num4)
					{
						num4 = num5;
						vector = real_pool_balls_physics[i].localPosition;
					}
				}
			}
			float cueAngle = GetCueAngle(vector);
			PoolGameRecording poolGameRecording = new PoolGameRecording();
			SimulateShot(cueAngle, power, false, true, poolGameRecording, true);
			curr_recording = poolGameRecording;
			clicker.transform.localPosition = real_pool_balls[0].transform.position + Vector3.Normalize(vector - real_pool_balls_physics[0].localPosition);
		}
		RedrawPoolCue();
		StartCoroutine(DelayedCpuShot());
	}

	public void PlaceWhiteBallAt(Vector2 V)
	{
		real_pool_balls[0].physics.velocity_mag_ = 0;
		real_pool_balls_physics[0].is_sunk = false;
		real_pool_balls[0].gameObject.SetActive(true);
		real_pool_balls[0].gameObject.transform.localPosition = V;
		real_pool_balls_physics[0].localPosition = V;
	}

	private void RandomlyPlaceWhiteBall()
	{
		List<Vector2> list = new List<Vector2>();
		list.Add(Vector2.zero);
		for (int i = -182; i < 182; i += 35)
		{
			for (int j = -76; j < 76; j += 35)
			{
				bool flag = false;
				for (int k = 0; k < real_pool_balls_physics.Length; k++)
				{
					if (Vector3.Distance(new Vector3(i, j, 0f), real_pool_balls_physics[k].localPosition) < pool_ball_w)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.Add(new Vector2(i, j));
				}
			}
		}
		PlaceWhiteBallAt(list[Random.Range(0, list.Count)]);
	}

	private IEnumerator DelayedCpuShot()
	{
		yield return new WaitForSeconds(1f);
		GamePhase = GamePhase_t.CPU_turn_animated_hit;
		AnimateShot();
	}

	private int SimulateShot(float deg, float pow, bool must_bounce_off_my_ball_first, bool can_scratch, PoolGameRecording recording, bool unlimited)
	{
		recording.frames = new List<List<object>>();
		recording.stop_recording_ball = new bool[real_pool_balls_physics.Length];
		recording.ball_positions = new Vector3[real_pool_balls_physics.Length];
		PoolBallSimulated[] array = new PoolBallSimulated[real_pool_balls_physics.Length];
		for (int i = 0; i < real_pool_balls_physics.Length; i++)
		{
			PoolBallSimulated poolBallSimulated = real_pool_balls_physics[i];
			PoolBallSimulated poolBallSimulated2 = new PoolBallSimulated();
			poolBallSimulated2.localPosition = poolBallSimulated.localPosition;
			poolBallSimulated2.is_sunk = poolBallSimulated.is_sunk;
			poolBallSimulated2.ball_id = i;
			poolBallSimulated2.team = real_pool_balls[i].team;
			array[i] = poolBallSimulated2;
			recording.ball_positions[i] = poolBallSimulated2.localPosition;
			if (poolBallSimulated2.is_sunk)
			{
				recording.stop_recording_ball[i] = true;
			}
		}
		float f = deg * 0.017453292f;
		white_first_touched = touched.unknown;
		array[0].SimulateFire(new Vector3(Mathf.Sin(f), Mathf.Cos(f), 0f), pow);
		int num = (unlimited ? 999 : 225);
		int num2 = 0;
		bool flag = false;
		while (num2 < num)
		{
			List<object> list = new List<object>();
			for (int j = 0; j < array.Length; j++)
			{
				array[j].SimulateTick(array, list);
			}
			for (int k = 0; k < array.Length; k++)
			{
				if (recording.stop_recording_ball[k])
				{
					continue;
				}
				PoolBallSimulated poolBallSimulated3 = array[k];
				if (poolBallSimulated3.localPosition != recording.ball_positions[k])
				{
					PoolGameRecording_UpdatePosition poolGameRecording_UpdatePosition = new PoolGameRecording_UpdatePosition();
					poolGameRecording_UpdatePosition.ball_id = k;
					poolGameRecording_UpdatePosition.localPosition = poolBallSimulated3.localPosition;
					recording.ball_positions[k] = poolBallSimulated3.localPosition;
					list.Add(poolGameRecording_UpdatePosition);
				}
				if (poolBallSimulated3.is_sunk)
				{
					PoolGameRecording_SinkBall poolGameRecording_SinkBall = new PoolGameRecording_SinkBall();
					poolGameRecording_SinkBall.ball_id = k;
					recording.stop_recording_ball[k] = true;
					list.Add(poolGameRecording_SinkBall);
				}
			}
			recording.frames.Add(list);
			num2++;
			if (!flag && must_bounce_off_my_ball_first && white_first_touched != touched.unknown)
			{
				if (white_first_touched != touched.CPU_ball)
				{
					return -1;
				}
				flag = true;
			}
			bool flag2 = false;
			for (int l = 0; l < array.Length; l++)
			{
				if (!array[l].is_sunk)
				{
					flag2 |= array[l].velocity_mag_ != 0;
				}
			}
			if (!flag2)
			{
				break;
			}
		}
		if (!must_bounce_off_my_ball_first || white_first_touched == touched.CPU_ball)
		{
			int num3 = 0;
			for (int m = 0; m < array.Length; m++)
			{
				if (!array[m].is_sunk || real_pool_balls_physics[m].is_sunk)
				{
					continue;
				}
				if (m == 0 || (player_team != team_t.undefined && real_pool_balls[m].team == player_team))
				{
					if (!can_scratch)
					{
						return -1;
					}
				}
				else
				{
					num3++;
				}
			}
			return num3;
		}
		return -1;
	}

	public void StartOpponentTurn()
	{
		thinking_text.gameObject.SetActive(false);
		NewTurn();
		if (MP_next_recording != null)
		{
			ShowMpRecording();
		}
		else
		{
			GamePhase = GamePhase_t.MP_other_turn;
		}
	}

	public void StartCPUTurn()
	{
		NewTurn();
		GamePhase = GamePhase_t.CPU_turn_think;
		StartCoroutine(BeginThinking());
	}

	public void StartYourTurn(GamePhase_t set_phase)
	{
		thinking_text.gameObject.SetActive(false);
		if (player_team != team_t.undefined)
		{
			WindowPrefabsControl.Instance.CreateScreen("POOL GAME - upper", WindowPrefabsControl.build_into_t.GAME_CTR);
			Text textLegacy = WindowPrefabsControl.Instance.GetTextLegacy("POOL GAME - upper", "sink_text");
			if (player_team == team_t.blue)
			{
				textLegacy.text = "Sink the Blue balls!";
				WindowPrefabsControl.Instance.GetObject("POOL GAME - upper", "ball").GetComponent<MeshRenderer>().material = blue_mat;
			}
			else
			{
				textLegacy.text = "Sink the Green balls!";
				WindowPrefabsControl.Instance.GetObject("POOL GAME - upper", "ball").GetComponent<MeshRenderer>().material = green_mat;
			}
		}
		NewTurn();
		GamePhase = set_phase;
		if (set_phase == GamePhase_t.MP_self_turn)
		{
			GameServerSender.Instance.SendPoolPlaceWhiteBall(real_pool_balls_physics[0].localPosition);
		}
		WindowPrefabsControl.Instance.CreateScreen("POOL GAME - lower", WindowPrefabsControl.build_into_t.GAME_CTR);
		RedrawPowerSlider();
	}

	private void RedrawPoolCue()
	{
		pool_cue.SetActive(true);
		lineA.enabled = true;
		lineB.enabled = true;
		collider_quad.SetActive(true);
		GetComponent<Animation>().Stop();
		GetComponent<Animation>().Play("Pool-cue-restore");
		pool_cue.transform.position = real_pool_balls[0].transform.position;
		pool_cue.transform.LookAt(clicker.transform);
		pool_cue_rotator.transform.localRotation = ((clicker.transform.localPosition.x >= pool_cue.transform.localPosition.x) ? Quaternion.Euler(180f, 90f, 180f) : Quaternion.Euler(0f, 90f, 180f));
		Vector3 localPosition = pool_cue.transform.localPosition;
		Vector3 normalized = (clicker.transform.localPosition - pool_cue.transform.localPosition).normalized;
		Vector2 vector = normalized;
		int num = 0;
		while (true)
		{
			localPosition += normalized;
			PoolBall poolBall = null;
			for (int i = 0; i < real_pool_balls.Length; i++)
			{
				PoolBall poolBall2 = real_pool_balls[i];
				if (!(poolBall2 == real_pool_balls[0]) && !poolBall2.physics.is_sunk && Vector3.Distance(poolBall2.transform.localPosition, localPosition) < pool_ball_w)
				{
					poolBall = poolBall2;
					break;
				}
			}
			if (poolBall != null)
			{
				Vector2 vector2 = (poolBall.transform.localPosition - localPosition).normalized;
				vector = ((normalized.y * vector2.x - normalized.x * vector2.y > normalized.x * vector2.y - normalized.y * vector2.x) ? new Vector2(0f - vector2.y, vector2.x) : new Vector2(vector2.y, 0f - vector2.x));
				Vector2 vector3 = poolBall.transform.localPosition;
				for (int j = 0; j < 100; j++)
				{
					vector3 += vector2;
					if ((vector.x < 0f && vector3.x < (float)(-bound_x)) || (vector.x > 0f && vector3.x > (float)bound_x) || (vector.y < 0f && vector3.y < (float)(-bound_y)) || (vector.y > 0f && vector3.y > (float)bound_y))
					{
						break;
					}
				}
				lineC.enabled = true;
				lineC.SetPosition(0, new Vector3(poolBall.transform.localPosition.x, poolBall.transform.localPosition.y, -40f));
				lineC.SetPosition(1, new Vector3(vector3.x, vector3.y, -40f));
				break;
			}
			lineC.enabled = false;
			if (localPosition.x < (float)(-bound_x) || localPosition.x > (float)bound_x)
			{
				vector = new Vector2(0f - normalized.x, normalized.y);
				break;
			}
			if (localPosition.y < (float)(-bound_y) || localPosition.y > (float)bound_y)
			{
				vector = new Vector2(normalized.x, 0f - normalized.y);
				break;
			}
			num++;
			if (num == 600)
			{
				vector = normalized;
				break;
			}
		}
		collider_quad.transform.localPosition = localPosition;
		lineA.SetPosition(0, new Vector3(pool_cue.transform.localPosition.x, pool_cue.transform.localPosition.y, -40f));
		lineA.SetPosition(1, new Vector3(localPosition.x, localPosition.y, -40f));
		Vector2 vector4 = localPosition;
		for (int k = 0; k < 100; k++)
		{
			vector4 += vector;
			if ((vector.x < 0f && vector4.x < (float)(-bound_x)) || (vector.x > 0f && vector4.x > (float)bound_x) || (vector.y < 0f && vector4.y < (float)(-bound_y)) || (vector.y > 0f && vector4.y > (float)bound_y))
			{
				break;
			}
		}
		lineB.SetPosition(0, new Vector3(localPosition.x, localPosition.y, -40f));
		lineB.SetPosition(1, new Vector3(vector4.x, vector4.y, -40f));
		pool_cue.transform.position = real_pool_balls[0].transform.position;
	}

	public float GetCueAngle(Vector3 point_at)
	{
		Vector2 to = (point_at - real_pool_balls_physics[0].localPosition).normalized;
		float num = Vector2.SignedAngle(Vector2.up, to);
		return ((num > 0f) ? 360f : 0f) - num;
	}

	public void PressShoot()
	{
		if (GamePhase != GamePhase_t.MP_self_turn && GamePhase != GamePhase_t.your_turn)
		{
			return;
		}
		show_ad_on_close = true;
		PopupControl.Instance.SetButtonWasPressed();
		WindowPrefabsControl.Instance.DestroyScreen("POOL GAME - lower");
		AnimateShot();
		float cueAngle = GetCueAngle(clicker.transform.localPosition);
		PoolGameRecording poolGameRecording = new PoolGameRecording();
		SimulateShot(cueAngle, power, false, true, poolGameRecording, true);
		curr_recording = poolGameRecording;
		if (GamePhase == GamePhase_t.MP_self_turn)
		{
			GamePhase = GamePhase_t.MP_self_turn_animated_hit;
			GameServerSender.Instance.SendPoolShoot(cueAngle, power, poolGameRecording.pack_for_web());
		}
		else if (GamePhase == GamePhase_t.your_turn)
		{
			GamePhase = GamePhase_t.your_turn_animated_hit;
		}
	}

	private void AnimateShot()
	{
		lineA.enabled = false;
		lineB.enabled = false;
		lineC.enabled = false;
		collider_quad.SetActive(false);
		GetComponent<Animation>().Stop();
		GetComponent<Animation>().Play("Pool-cue-hit");
	}

	public void AnmPullbackComplete()
	{
		GetComponent<Animation>().Stop();
		GetComponent<Animation>()["Pool-cue-followthru"].speed = power * (float)max_power / ((float)max_power * 0.5f);
		GetComponent<Animation>().Play("Pool-cue-followthru");
	}

	public void ShowMpRecording()
	{
		show_ad_on_close = true;
		curr_recording = MP_next_recording;
		MP_next_recording = null;
		power = MP_next_power;
		MP_next_power = 0f;
		float f = MP_next_deg * 0.017453292f;
		clicker.transform.localPosition = real_pool_balls[0].transform.localPosition + new Vector3(Mathf.Sin(f), Mathf.Cos(f), 0f);
		MP_next_deg = 0f;
		GamePhase = GamePhase_t.MP_other_turn_animated_hit;
		AnimateShot();
	}

	public void AnmHitBall()
	{
		pool_cue_audio.volume = power * AudioControl.Instance.general_sfx_volume;
		pool_cue_audio.pitch = Mathf.Lerp(0.7f, 1f, power);
		pool_cue_audio.Play();
		if (curr_recording != null)
		{
			curr_recording.is_playing = true;
		}
		StartCoroutine(DelayedHidePoolCue());
	}

	public void AnmNotifComplete()
	{
		switch (GamePhase)
		{
		case GamePhase_t.your_turn_intro:
			if (!show_team_notif)
			{
				StartYourTurn(GamePhase_t.your_turn);
				return;
			}
			break;
		case GamePhase_t.you_scratched:
			if (player_team == team_t.undefined || (NumBallsRemaining(player_team) != 0 && NumBallsRemaining((player_team == team_t.green) ? team_t.blue : team_t.green) != 0))
			{
				CpuTurnIntro(false);
			}
			else
			{
				YouLost(false);
			}
			return;
		case GamePhase_t.CPU_turn_intro:
			StartCPUTurn();
			return;
		case GamePhase_t.CPU_scratched:
			if (player_team == team_t.undefined || (NumBallsRemaining(player_team) != 0 && NumBallsRemaining((player_team == team_t.green) ? team_t.blue : team_t.green) != 0))
			{
				YourTurnIntro(false, false);
			}
			else
			{
				YouWon(false);
			}
			return;
		case GamePhase_t.game_over_win:
		{
			int index;
			switch (MinigameMenu.Instance.curr_difficulty)
			{
			case MinigameMenu.CPU_difficulty.normal:
				index = 1;
				break;
			case MinigameMenu.CPU_difficulty.hard:
				AchievesControl.Instance.UnlockAchievement("Pure Precision");
				index = 2;
				break;
			case MinigameMenu.CPU_difficulty.impossible:
				AchievesControl.Instance.UnlockAchievement("Pure Precision");
				index = 3;
				break;
			default:
				index = 0;
				break;
			}
			MinigameMenu.Instance.TryTakeReward(index, 0);
			GamePhase = GamePhase_t.play_again;
			MinigameMenu.Instance.ShowNotif("Play Again?", true);
			return;
		}
		case GamePhase_t.game_over_loss:
			GamePhase = GamePhase_t.play_again;
			MinigameMenu.Instance.ShowNotif("Play Again?", true);
			return;
		case GamePhase_t.MP_game_intro_otherFirst:
			GamePhase = GamePhase_t.MP_other_turn_intro;
			MinigameMenu.Instance.ShowNotif("<color=#43de4f>" + curr_opponent + "</color>\ngoes first!");
			return;
		case GamePhase_t.MP_game_intro_selfFirst:
			GamePhase = GamePhase_t.MP_self_turn_intro;
			MinigameMenu.Instance.ShowNotif("You go first!");
			return;
		case GamePhase_t.MP_other_turn_intro:
			GameServerSender.Instance.SendPoolSyncReady();
			if (other_player_ready)
			{
				other_player_ready = false;
				StartOpponentTurn();
				return;
			}
			GamePhase = GamePhase_t.MP_other_turn_SYNC_WAIT;
			thinking_text.gameObject.SetActive(true);
			thinking_text.text = "... Waiting for " + curr_opponent + " ...";
			return;
		case GamePhase_t.MP_other_scratched:
			if (player_team == team_t.undefined || (NumBallsRemaining(player_team) != 0 && NumBallsRemaining((player_team == team_t.green) ? team_t.blue : team_t.green) != 0))
			{
				YourTurnIntro(false, true);
			}
			else
			{
				YouWon(true);
			}
			return;
		case GamePhase_t.MP_self_turn_intro:
			if (!show_team_notif)
			{
				GameServerSender.Instance.SendPoolSyncReady();
				if (other_player_ready)
				{
					other_player_ready = false;
					StartYourTurn(GamePhase_t.MP_self_turn);
					return;
				}
				GamePhase = GamePhase_t.MP_self_turn_SYNC_WAIT;
				thinking_text.gameObject.SetActive(true);
				thinking_text.text = "... Waiting for " + curr_opponent + " ...";
				return;
			}
			break;
		case GamePhase_t.MP_self_scratched:
			if (player_team == team_t.undefined || (NumBallsRemaining(player_team) != 0 && NumBallsRemaining((player_team == team_t.green) ? team_t.blue : team_t.green) != 0))
			{
				OpponentTurnIntro(false);
			}
			else
			{
				YouLost(true);
			}
			return;
		case GamePhase_t.MP_game_over_loss:
		case GamePhase_t.MP_game_over_win:
			GamePhase = GamePhase_t.MP_play_again;
			MinigameMenu.Instance.ShowNotif("Play Again?", true);
			return;
		default:
			return;
		}
		show_team_notif = false;
		if (player_team == team_t.green)
		{
			MinigameMenu.Instance.ShowNotif("You must sink\nthe <color=#21ff51>Green balls!</color>");
		}
		else if (player_team == team_t.blue)
		{
			MinigameMenu.Instance.ShowNotif("You must sink\nthe <color=#21b1ff>Blue balls!</color>");
		}
	}

	public void OnOtherPlayerReady()
	{
		if (GamePhase == GamePhase_t.MP_other_turn_SYNC_WAIT)
		{
			StartOpponentTurn();
		}
		else if (GamePhase == GamePhase_t.MP_self_turn_SYNC_WAIT)
		{
			StartYourTurn(GamePhase_t.MP_self_turn);
		}
		else
		{
			other_player_ready = true;
		}
	}

	public void StartCPUGame()
	{
		MinigameMenu.Instance.ShowMenu(MinigameMenu.menu_type_t.in_game_CPU);
		CPU_accuracy_schedule.Clear();
		GamePhase = GamePhase_t.your_turn_intro;
		MinigameMenu.Instance.ShowNotif("You go first!");
		if (sfx_game_start != null)
		{
			GetComponent<AudioSource>().PlayOneShot(sfx_game_start, AudioControl.Instance.general_sfx_volume * 0.5f);
		}
	}

	public void AdFinished()
	{
		if (show_ad_on_close)
		{
			return;
		}
		if (GamePhase == GamePhase_t.MP_play_again)
		{
			GameServerSender.Instance.SendPlayPoolAgain();
			GetComponent<Animation>().Stop();
			MinigameMenu.Instance.HideNotif();
			GamePhase = GamePhase_t.MP_play_again_SYNC_WAIT;
			thinking_text.gameObject.SetActive(true);
			thinking_text.text = "... Waiting for " + curr_opponent + " ...";
		}
		else if (GamePhase == GamePhase_t.play_again)
		{
			ArrangeBalls(GetRandomBallLayout());
			MinigameMenu.Instance.ShowMenu(MinigameMenu.menu_type_t.cpu_select);
		}
	}

	public void OnPlayAgain()
	{
		player_team = team_t.undefined;
		show_team_notif = false;
		particle_sound_iterator = 0;
		white_was_sunk = false;
		sunk_wrong_ball_ = false;
		n_correct_balls_sunk = 0;
		sink_wrong_ball_OK = false;
		if (GamePhase == GamePhase_t.MP_play_again || GamePhase == GamePhase_t.play_again)
		{
			show_ad_on_close = false;
			AdvertControl.Instance.TryShowInterstitialAd(AdvertControl.ad_context.FORCED);
		}
	}

	public static int[] GetRandomBallLayout()
	{
		int[] array = new int[14];
		List<int> list = new List<int>();
		for (int i = 0; i < 14; i++)
		{
			list.Add(i);
		}
		for (int j = 0; j < 14; j++)
		{
			int index = Random.Range(0, list.Count);
			int num = list[index];
			list.RemoveAt(index);
			array[j] = num;
		}
		return array;
	}

	private IEnumerator DelayedHidePoolCue()
	{
		yield return new WaitForSeconds(0.5f);
		pool_cue.SetActive(false);
		switch (GamePhase)
		{
		case GamePhase_t.your_turn_animated_hit:
			GamePhase = GamePhase_t.your_turn_waitForBallsToStop;
			break;
		case GamePhase_t.CPU_turn_animated_hit:
			GamePhase = GamePhase_t.CPU_turn_waitForBallsToStop;
			break;
		case GamePhase_t.MP_other_turn_animated_hit:
			GamePhase = GamePhase_t.MP_other_turn_waitForBallsToStop;
			break;
		case GamePhase_t.MP_self_turn_animated_hit:
			GamePhase = GamePhase_t.MP_self_turn_waitForBallsToStop;
			break;
		}
	}
}
