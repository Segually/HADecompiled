using UnityEngine;

public class LimbAnimation
{
	public int animLength;

	public float speed;

	public bool disableInversion;

	public AnimationCurve animPosx;

	public AnimationCurve animPosy;

	public AnimationCurve animPosz;

	public AnimationCurve animRotx;

	public AnimationCurve animRoty;

	public AnimationCurve animRotz;

	public AnimationCurve animRotw;

	public LimbAnimation(int animation_id, Limb limb, float forced_speed)
	{
		if (animation_id == 1 || animation_id == 2)
		{
			speed = 0.15f;
			disableInversion = false;
		}
		else if (animation_id == 3)
		{
			speed = 0.16f;
			disableInversion = false;
		}
		else
		{
			speed = 0.7f;
			disableInversion = true;
		}
		if (forced_speed != -1f)
		{
			speed = forced_speed;
		}
		animLength = limb.GetNumFrames(animation_id);
		Keyframe[] array = new Keyframe[animLength + 1];
		Keyframe[] array2 = new Keyframe[animLength + 1];
		Keyframe[] array3 = new Keyframe[animLength + 1];
		Keyframe[] array4 = new Keyframe[animLength + 1];
		Keyframe[] array5 = new Keyframe[animLength + 1];
		Keyframe[] array6 = new Keyframe[animLength + 1];
		Keyframe[] array7 = new Keyframe[animLength + 1];
		for (int i = 0; i < animLength; i++)
		{
			Vector3 frameSnapPosition = limb.GetFrameSnapPosition(animation_id, i);
			array[i] = new Keyframe(speed * (float)i, frameSnapPosition.x);
			array2[i] = new Keyframe(speed * (float)i, frameSnapPosition.y);
			array3[i] = new Keyframe(speed * (float)i, frameSnapPosition.z);
			Quaternion frameRotation = limb.GetFrameRotation(animation_id, i);
			array4[i] = new Keyframe(speed * (float)i, frameRotation.x);
			array5[i] = new Keyframe(speed * (float)i, frameRotation.y);
			array6[i] = new Keyframe(speed * (float)i, frameRotation.z);
			array7[i] = new Keyframe(speed * (float)i, frameRotation.w);
		}
		Vector3 frameSnapPosition2 = limb.GetFrameSnapPosition(animation_id, 0);
		array[animLength] = new Keyframe(speed * (float)animLength, frameSnapPosition2.x);
		array2[animLength] = new Keyframe(speed * (float)animLength, frameSnapPosition2.y);
		array3[animLength] = new Keyframe(speed * (float)animLength, frameSnapPosition2.z);
		Quaternion frameRotation2 = limb.GetFrameRotation(animation_id, 0);
		array4[animLength] = new Keyframe(speed * (float)animLength, frameRotation2.x);
		array5[animLength] = new Keyframe(speed * (float)animLength, frameRotation2.y);
		array6[animLength] = new Keyframe(speed * (float)animLength, frameRotation2.z);
		array7[animLength] = new Keyframe(speed * (float)animLength, frameRotation2.w);
		animPosx = new AnimationCurve(array);
		animPosy = new AnimationCurve(array2);
		animPosz = new AnimationCurve(array3);
		animRotx = new AnimationCurve(array4);
		animRoty = new AnimationCurve(array5);
		animRotz = new AnimationCurve(array6);
		animRotw = new AnimationCurve(array7);
	}

	public Vector3 EvaluatePositionAnimationCurve(bool offset, float started_anm_time)
	{
		float num = 0f;
		if (offset)
		{
			num = speed * (float)(animLength / 2);
		}
		if (disableInversion)
		{
			num = 0f;
		}
		float num2 = Time.time - started_anm_time + num;
		return new Vector3(animPosx.Evaluate(num2 % (speed * (float)animLength)), animPosy.Evaluate(num2 % (speed * (float)animLength)), animPosz.Evaluate(num2 % (speed * (float)animLength)));
	}

	public Quaternion EvaluateRotationAnimationCurve(bool offset, float started_anm_time)
	{
		float num = 0f;
		if (offset)
		{
			num = speed * (float)(animLength / 2);
		}
		if (disableInversion)
		{
			num = 0f;
		}
		float num2 = Time.time - started_anm_time + num;
		return new Quaternion(animRotx.Evaluate(num2 % (speed * (float)animLength)), animRoty.Evaluate(num2 % (speed * (float)animLength)), animRotz.Evaluate(num2 % (speed * (float)animLength)), animRotw.Evaluate(num2 % (speed * (float)animLength)));
	}
}
