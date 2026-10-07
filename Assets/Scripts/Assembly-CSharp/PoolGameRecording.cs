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
		Packet packet = new Packet();
		packet.PutLong(frames.Count);
		foreach (List<object> frame in frames)
		{
			packet.PutShort((float)frame.Count);
			foreach (object item in frame)
			{
				if (item.GetType() == typeof(PoolGameRecording_UpdatePosition))
				{
					packet.PutByte(0);
					PoolGameRecording_UpdatePosition updatePosition = (PoolGameRecording_UpdatePosition)item;
					packet.PutByte((byte)updatePosition.ball_id);
					packet.PutLong((int)updatePosition.localPosition.x);
					packet.PutLong((int)updatePosition.localPosition.y);
				}
				else if (item.GetType() == typeof(PoolGameRecording_SinkBall))
				{
					packet.PutByte(1);
					PoolGameRecording_SinkBall sinkBall = (PoolGameRecording_SinkBall)item;
					packet.PutByte((byte)sinkBall.ball_id);
				}
				else if (item.GetType() == typeof(PoolGameRecording_SFX_collide))
				{
					packet.PutByte(2);
					PoolGameRecording_SFX_collide collide = (PoolGameRecording_SFX_collide)item;
					packet.PutByte((byte)collide.ball_id);
					packet.PutShort((float)collide.velocity);
				}
			}
		}
		return packet.ToByteArray();
	}

	public void unpack_from_web(byte[] bytes)
	{
		Packet packet = new Packet(bytes);
		frames = new List<List<object>>();
		int frameCount = packet.GetLong();
		for (int i = 0; i < frameCount; i++)
		{
			List<object> frame = new List<object>();
			int eventCount = packet.GetShort();
			for (int j = 0; j < eventCount; j++)
			{
				byte eventType = packet.GetByte();
				if (eventType == 0)
				{
					PoolGameRecording_UpdatePosition updatePosition = new PoolGameRecording_UpdatePosition();
					updatePosition.ball_id = packet.GetByte();
					int x = packet.GetLong();
					int y = packet.GetLong();
					updatePosition.localPosition = new Vector3(x, y, 0f);
					frame.Add(updatePosition);
				}
				else if (eventType == 1)
				{
					PoolGameRecording_SinkBall sinkBall = new PoolGameRecording_SinkBall();
					sinkBall.ball_id = packet.GetByte();
					frame.Add(sinkBall);
				}
				else if (eventType == 2)
				{
					PoolGameRecording_SFX_collide collide = new PoolGameRecording_SFX_collide();
					collide.ball_id = packet.GetByte();
					collide.velocity = packet.GetShort();
					frame.Add(collide);
				}
			}
			frames.Add(frame);
		}
	}
}
