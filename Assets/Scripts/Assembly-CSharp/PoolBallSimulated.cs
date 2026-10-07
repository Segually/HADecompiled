using System.Collections.Generic;
using UnityEngine;

public class PoolBallSimulated
{
	public int ball_id;

	public Vector3 velocity_dir;

	public int velocity_mag_;

	public bool is_sunk;

	public PoolGameControl.team_t team;

	public Vector3 localPosition;

	public void SimulateFire(Vector3 dir, float power)
	{
		velocity_dir = dir;
		velocity_mag_ = (int)((float)PoolGameControl.max_power * power);
	}

	private bool IsCollidingWith(PoolBallSimulated ball)
	{
		return Vector3.Distance(localPosition, ball.localPosition) <= PoolGameControl.pool_ball_w;
	}

	public void SimulateTick(PoolBallSimulated[] balls, List<object> recording_frame)
	{
		if (is_sunk)
		{
			return;
		}
		int num = velocity_mag_ / 100;
		for (int i = 0; i < num; i++)
		{
			localPosition += velocity_dir;
			for (int j = 0; j < balls.Length; j++)
			{
				PoolBallSimulated poolBallSimulated = balls[j];
				if (poolBallSimulated == this || poolBallSimulated.is_sunk || !IsCollidingWith(poolBallSimulated))
				{
					continue;
				}
				Vector2 vector = velocity_dir * velocity_mag_;
				Vector2 vector2 = poolBallSimulated.velocity_dir * poolBallSimulated.velocity_mag_;
				Vector2 vector3 = (poolBallSimulated.localPosition - localPosition).normalized;
				float num2 = Vector2.Dot(vector, vector3);
				float num3 = Vector2.Dot(vector2, vector3);
				vector += (num3 - num2) * vector3;
				velocity_dir = vector.normalized;
				velocity_mag_ = (int)vector.magnitude;
				vector2 += (num2 - num3) * vector3;
				poolBallSimulated.velocity_dir = vector2.normalized;
				poolBallSimulated.velocity_mag_ = (int)vector2.magnitude;
				int num4 = 0;
				do
				{
					localPosition -= (Vector3)vector3;
					num4++;
				}
				while (IsCollidingWith(poolBallSimulated) && num4 < 5);
				if (team == PoolGameControl.team_t.undefined && PoolGameControl.Instance.white_first_touched == PoolGameControl.touched.unknown)
				{
					if (PoolGameControl.Instance.player_team == PoolGameControl.team_t.undefined || poolBallSimulated.team != PoolGameControl.Instance.player_team)
					{
						PoolGameControl.Instance.white_first_touched = PoolGameControl.touched.CPU_ball;
					}
					else
					{
						PoolGameControl.Instance.white_first_touched = PoolGameControl.touched.player_ball;
					}
				}
				PoolGameRecording_SFX_collide poolGameRecording_SFX_collide = new PoolGameRecording_SFX_collide();
				poolGameRecording_SFX_collide.ball_id = ball_id;
				poolGameRecording_SFX_collide.velocity = velocity_mag_;
				recording_frame.Add(poolGameRecording_SFX_collide);
			}
			if ((localPosition.x < (float)(-PoolGameControl.bound_x) && velocity_dir.x < 0f) || (localPosition.x > (float)PoolGameControl.bound_x && velocity_dir.x > 0f))
			{
				velocity_dir = new Vector3(0f - velocity_dir.x, velocity_dir.y);
				if (PoolGameControl.Instance.white_first_touched == PoolGameControl.touched.unknown)
				{
					PoolGameControl.Instance.white_first_touched = PoolGameControl.touched.wall;
				}
			}
			if ((localPosition.y < (float)(-PoolGameControl.bound_y) && velocity_dir.y < 0f) || (localPosition.y > (float)PoolGameControl.bound_y && velocity_dir.y > 0f))
			{
				velocity_dir = new Vector3(velocity_dir.x, 0f - velocity_dir.y);
				if (PoolGameControl.Instance.white_first_touched == PoolGameControl.touched.unknown)
				{
					PoolGameControl.Instance.white_first_touched = PoolGameControl.touched.wall;
				}
			}
			Transform[] holes = PoolGameControl.Instance.holes;
			for (int k = 0; k < holes.Length; k++)
			{
				if (Vector3.Distance(localPosition, holes[k].localPosition) < holes[k].transform.localScale.x)
				{
					is_sunk = true;
					break;
				}
			}
			if (is_sunk)
			{
				break;
			}
		}
		if (velocity_mag_ > 0)
		{
			velocity_mag_ = Mathf.Max(velocity_mag_, 3) - 3;
		}
	}

	public void Sink(bool is_real_ball, PoolBall parent_ball)
	{
		is_sunk = true;
		if (!is_real_ball)
		{
			return;
		}
		Transform transform = null;
		float num = float.MaxValue;
		Transform[] holes = PoolGameControl.Instance.holes;
		foreach (Transform transform2 in holes)
		{
			float num2 = Vector3.Distance(localPosition, transform2.localPosition);
			if (num2 < num)
			{
				num = num2;
				transform = transform2;
			}
		}
		parent_ball.sunk_position = transform.localPosition;
		PoolGameControl.GamePhase_t gamePhase = PoolGameControl.Instance.GamePhase;
		if (gamePhase == PoolGameControl.GamePhase_t.your_turn_waitForBallsToStop || gamePhase == PoolGameControl.GamePhase_t.your_turn_animated_hit || gamePhase == PoolGameControl.GamePhase_t.MP_self_turn_waitForBallsToStop || gamePhase == PoolGameControl.GamePhase_t.MP_self_turn_animated_hit)
		{
			if (parent_ball.team == PoolGameControl.team_t.undefined)
			{
				PoolGameControl.Instance.white_was_sunk = true;
				PoolGameControl.Instance.CreateError(transform.localPosition);
			}
			else if (PoolGameControl.Instance.player_team == PoolGameControl.team_t.undefined)
			{
				PoolGameControl.Instance.SetPlayerTeam(parent_ball);
				PoolGameControl.Instance.CreateParticle(transform.localPosition);
			}
			else if (parent_ball.team == PoolGameControl.Instance.player_team)
			{
				PoolGameControl.Instance.n_correct_balls_sunk++;
				PoolGameControl.Instance.CreateParticle(transform.localPosition);
			}
			else
			{
				PoolGameControl.Instance.sunk_wrong_ball_ = true;
				if (!PoolGameControl.Instance.sink_wrong_ball_OK)
				{
					PoolGameControl.Instance.CreateError(transform.localPosition);
				}
			}
		}
		else
		{
			if (gamePhase != PoolGameControl.GamePhase_t.CPU_turn_waitForBallsToStop && gamePhase != PoolGameControl.GamePhase_t.CPU_turn_animated_hit && gamePhase != PoolGameControl.GamePhase_t.MP_other_turn_waitForBallsToStop && gamePhase != PoolGameControl.GamePhase_t.MP_other_turn_animated_hit)
			{
				return;
			}
			if (parent_ball.team == PoolGameControl.team_t.undefined)
			{
				PoolGameControl.Instance.white_was_sunk = true;
				PoolGameControl.Instance.CreateError(transform.localPosition);
			}
			else if (PoolGameControl.Instance.player_team == PoolGameControl.team_t.undefined)
			{
				PoolGameControl.Instance.SetPlayerTeam(parent_ball);
				PoolGameControl.Instance.CreateParticle(transform.localPosition);
			}
			else if (PoolGameControl.Instance.player_team == PoolGameControl.team_t.green)
			{
				if (parent_ball.team == PoolGameControl.team_t.blue)
				{
					PoolGameControl.Instance.n_correct_balls_sunk++;
					PoolGameControl.Instance.CreateParticle(transform.localPosition);
					return;
				}
				PoolGameControl.Instance.sunk_wrong_ball_ = true;
				if (!PoolGameControl.Instance.sink_wrong_ball_OK)
				{
					PoolGameControl.Instance.CreateError(transform.localPosition);
				}
			}
			else if (PoolGameControl.Instance.player_team == PoolGameControl.team_t.blue)
			{
				if (parent_ball.team == PoolGameControl.team_t.green)
				{
					PoolGameControl.Instance.n_correct_balls_sunk++;
					PoolGameControl.Instance.CreateParticle(transform.localPosition);
					return;
				}
				PoolGameControl.Instance.sunk_wrong_ball_ = true;
				if (!PoolGameControl.Instance.sink_wrong_ball_OK)
				{
					PoolGameControl.Instance.CreateError(transform.localPosition);
				}
			}
		}
	}
}
