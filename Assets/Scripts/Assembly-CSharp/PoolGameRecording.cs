using System.Collections.Generic;
using UnityEngine;

public class PoolGameRecording
{
	public bool is_playing;

	public bool[] stop_recording_ball;

	public Vector3[] ball_positions;

	public List<List<object>> frames;

	public void PopFrame()
	{
		if (frames.Count == 0)
		{
			return;
		}
		List<object> list = frames[0];
		frames.RemoveAt(0);
		foreach (object item in list)
		{
			if (item.GetType() == typeof(PoolGameRecording_UpdatePosition))
			{
				PoolGameRecording_UpdatePosition poolGameRecording_UpdatePosition = (PoolGameRecording_UpdatePosition)item;
				PoolGameControl.Instance.real_pool_balls_physics[poolGameRecording_UpdatePosition.ball_id].localPosition = poolGameRecording_UpdatePosition.localPosition;
				PoolGameControl.Instance.real_pool_balls[poolGameRecording_UpdatePosition.ball_id].transform.localPosition = poolGameRecording_UpdatePosition.localPosition;
			}
			else if (item.GetType() == typeof(PoolGameRecording_SinkBall))
			{
				PoolGameRecording_SinkBall poolGameRecording_SinkBall = (PoolGameRecording_SinkBall)item;
				PoolGameControl.Instance.real_pool_balls_physics[poolGameRecording_SinkBall.ball_id].Sink(true, PoolGameControl.Instance.real_pool_balls[poolGameRecording_SinkBall.ball_id]);
			}
			else if (item.GetType() == typeof(PoolGameRecording_SFX_collide))
			{
				PoolGameRecording_SFX_collide poolGameRecording_SFX_collide = (PoolGameRecording_SFX_collide)item;
				AudioSource component = PoolGameControl.Instance.real_pool_balls[poolGameRecording_SFX_collide.ball_id].GetComponent<AudioSource>();
				component.Stop();
				float t = Mathf.Clamp01((float)poolGameRecording_SFX_collide.velocity / 2000f);
				component.volume = Mathf.Lerp(0.1f, 1f, t) * AudioControl.Instance.general_sfx_volume;
				component.pitch = Mathf.Lerp(0.8f, 1.3f, t);
				component.Play();
			}
		}
	}

	public byte[] pack_for_web()
	{
		return null;
	}

	public void unpack_from_web(byte[] bytes)
	{
	}
}
