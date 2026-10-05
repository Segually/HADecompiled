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
	}

	private bool IsCollidingWith(PoolBallSimulated ball)
	{
		return false;
	}

	public void SimulateTick(PoolBallSimulated[] balls, List<object> recording_frame)
	{
	}

	public void Sink(bool is_real_ball, PoolBall parent_ball)
	{
	}
}
