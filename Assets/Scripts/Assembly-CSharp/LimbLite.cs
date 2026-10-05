using UnityEngine;

public class LimbLite
{
	public Limb original;

	public LimbLite parent_limb_;

	public Vector3 snap_global;

	public Vector3 snap_global_dummy;

	public LimbAnimation curr_animation;

	public CreatureSubModel my_sub_model;

	public Vector3 transform_position;

	public Quaternion transform_rotation;

	public Vector3 visual_transform_localPosition;

	public Vector3 visual_transform_localScale;

	public Vector3 dummy_transform_position;

	public Quaternion dummy_transform_rotation;

	public Vector3 dummy_visual_transform_localPosition;

	public void SetSnapLocal(Vector3 input, Matrix4x4 mn)
	{
		if (parent_limb_ != null)
		{
			input = mn.MultiplyPoint3x4(input);
		}
		snap_global = input;
	}

	public void SetSnapLocalDummy(Vector3 input, Matrix4x4 mn)
	{
		if (parent_limb_ != null)
		{
			if (!original.symmetrical)
			{
				Matrix4x4 matrix4x = Matrix4x4.TRS(parent_limb_.dummy_transform_position, parent_limb_.dummy_transform_rotation, Vector3.one);
				Matrix4x4 matrix4x2 = Matrix4x4.TRS(parent_limb_.dummy_visual_transform_localPosition, Quaternion.identity, parent_limb_.visual_transform_localScale);
				mn = matrix4x * matrix4x2;
			}
			input = mn.MultiplyPoint3x4(new Vector3(0f - input.x, input.y, input.z));
		}
		snap_global_dummy = input;
	}

	public void StopAnimation()
	{
		curr_animation = null;
	}

	public void CreateAndPlayAnimation(int animation_id, float started_anm_time, float spaghettiFactor_, float forced_speed)
	{
		if (!original.cached_animations.ContainsKey(animation_id))
		{
			LimbAnimation value = new LimbAnimation(animation_id, original, forced_speed);
			original.cached_animations.Add(animation_id, value);
		}
		if (curr_animation == null)
		{
			EvalAnimationCurves(1, started_anm_time, spaghettiFactor_);
		}
		curr_animation = original.cached_animations[animation_id];
	}

	public void EvalAnimationCurves(int choppiness, float started_anm_time, float spaghettiFactor_)
	{
		float num = ((curr_animation != null && !original.rigid_limb) ? ((float)choppiness * spaghettiFactor_) : float.MaxValue);
		Matrix4x4 mn = default(Matrix4x4);
		Quaternion quaternion = Quaternion.identity;
		if (parent_limb_ != null)
		{
			Matrix4x4 matrix4x = Matrix4x4.TRS(parent_limb_.transform_position, parent_limb_.transform_rotation, Vector3.one);
			Matrix4x4 matrix4x2 = Matrix4x4.TRS(parent_limb_.visual_transform_localPosition, Quaternion.identity, parent_limb_.visual_transform_localScale);
			mn = matrix4x * matrix4x2;
		}
		Quaternion quaternion2;
		if (curr_animation == null)
		{
			SetSnapLocal(original.GetFrameSnapPosition(0, 0), mn);
			if (original.has_dummy)
			{
				SetSnapLocalDummy(original.GetFrameSnapPosition(0, 0), mn);
			}
			quaternion2 = original.GetFrameRotation(0, 0);
			if (original.has_dummy)
			{
				quaternion = quaternion2;
			}
		}
		else
		{
			Vector3 vector = curr_animation.EvaluatePositionAnimationCurve(false, started_anm_time);
			if (parent_limb_ == null)
			{
				visual_transform_localPosition = vector;
			}
			else
			{
				SetSnapLocal(vector, mn);
				if (original.has_dummy)
				{
					SetSnapLocalDummy(curr_animation.EvaluatePositionAnimationCurve(true, started_anm_time), mn);
				}
			}
			quaternion2 = curr_animation.EvaluateRotationAnimationCurve(false, started_anm_time);
			if (original.has_dummy)
			{
				quaternion = curr_animation.EvaluateRotationAnimationCurve(true, started_anm_time);
			}
		}
		if (snap_global != Vector3.zero)
		{
			transform_position = Vector3.Lerp(transform_position, snap_global, num * Time.deltaTime);
			dummy_transform_position = Vector3.Lerp(dummy_transform_position, snap_global_dummy, num * Time.deltaTime);
		}
		if (!original.inherit)
		{
			transform_rotation = Quaternion.Lerp(transform_rotation, quaternion2, num * Time.deltaTime);
			if (original.has_dummy)
			{
				dummy_transform_rotation = Quaternion.Lerp(dummy_transform_rotation, new Quaternion(0f - quaternion.x, quaternion.y, quaternion.z, 0f - quaternion.w), num * Time.deltaTime);
			}
			return;
		}
		transform_rotation = Quaternion.Lerp(transform_rotation, parent_limb_.transform_rotation * quaternion2, num * Time.deltaTime);
		if (original.has_dummy)
		{
			Quaternion quaternion3 = (original.symmetrical ? parent_limb_.transform_rotation : parent_limb_.dummy_transform_rotation);
			dummy_transform_rotation = Quaternion.Lerp(dummy_transform_rotation, quaternion3 * new Quaternion(0f - quaternion.x, quaternion.y, quaternion.z, 0f - quaternion.w), num * Time.deltaTime);
		}
	}
}
