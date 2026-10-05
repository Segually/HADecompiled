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

	public static int bound_x;

	public static int bound_y;

	public AudioClip sfx_wrong_sink;

	public AudioClip sfx_good_sink_1;

	public AudioClip sfx_good_sink_2;

	public AudioClip sfx_good_sink_3;

	public AudioClip sfx_game_start;

	public AudioClip sfx_bad_move;

	public AudioClip sfx_good_move;

	public AudioClip sfx_win;

	public AudioClip sfx_lose;

	public string curr_opponent;

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

	private float power;

	private int particle_sound_iterator;

	public static float pool_ball_w;

	public touched white_first_touched;

	public static int max_power;

	public AudioSource pool_cue_audio;

	public PoolGameRecording curr_recording;

	public float MP_next_power;

	public float MP_next_deg;

	public PoolGameRecording MP_next_recording;

	public int curr_pool_table_id;

	public bool other_player_ready;

	public Text thinking_text;

	public List<accuracy> CPU_accuracy_schedule;

	public bool show_ad_on_close;

	public void AnmIntroComplete()
	{
	}

	public void StartMpGame(bool is_host)
	{
	}

	public void RestartMpGame()
	{
	}

	public void ArrangeBalls(int[] placements)
	{
	}

	private void RedrawPowerSlider()
	{
	}

	public void PressLowerBackground()
	{
	}

	public void PressPowerSlider()
	{
	}

	public void ReleasePowerSlider()
	{
	}

	public void SetPlayerTeam(PoolBall ball)
	{
	}

	public string GetPrefix(bool on_cpu_sink)
	{
		return null;
	}

	private void YouLost(bool MP)
	{
	}

	private void YouWon(bool MP)
	{
	}

	private void CpuTurnIntro(bool with_score_prefix)
	{
	}

	private void YourTurnIntro(bool with_score_prefix, bool MP)
	{
	}

	private void OpponentTurnIntro(bool with_score_prefix)
	{
	}

	private void FixedUpdate()
	{
	}

	private int NumBallsRemaining(team_t team)
	{
		return 0;
	}

	private void Update()
	{
	}

	public void CreateError(Vector2 localPosition)
	{
	}

	public void CreateParticle(Vector2 localPosition)
	{
	}

	private void NewTurn()
	{
	}

	private void GenerateAccuracySchedule(int miss_X_in_7)
	{
	}

	public void TryUpdateCuePosition(float deg)
	{
	}

	private IEnumerator BeginThinking()
	{
		return null;
	}

	public void PlaceWhiteBallAt(Vector2 V)
	{
	}

	private void RandomlyPlaceWhiteBall()
	{
	}

	private IEnumerator DelayedCpuShot()
	{
		return null;
	}

	private int SimulateShot(float deg, float pow, bool must_bounce_off_my_ball_first, bool can_scratch, PoolGameRecording recording, bool unlimited)
	{
		return 0;
	}

	public void StartOpponentTurn()
	{
	}

	public void StartCPUTurn()
	{
	}

	public void StartYourTurn(GamePhase_t set_phase)
	{
	}

	private void RedrawPoolCue()
	{
	}

	public float GetCueAngle(Vector3 point_at)
	{
		return 0f;
	}

	public void PressShoot()
	{
	}

	private void AnimateShot()
	{
	}

	public void AnmPullbackComplete()
	{
	}

	public void ShowMpRecording()
	{
	}

	public void AnmHitBall()
	{
	}

	public void AnmNotifComplete()
	{
	}

	public void OnOtherPlayerReady()
	{
	}

	public void StartCPUGame()
	{
	}

	public void AdFinished()
	{
	}

	public void OnPlayAgain()
	{
	}

	public static int[] GetRandomBallLayout()
	{
		return null;
	}

	private IEnumerator DelayedHidePoolCue()
	{
		return null;
	}
}
