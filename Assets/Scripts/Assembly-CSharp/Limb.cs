using System.Collections.Generic;
using UnityEngine;

public class Limb : MonoBehaviour
{
	public bool has_dummy;

	public string textureName;

	public List<Limb> sub_limbs;

	public bool symmetrical;

	public string compName;

	public bool inherit;

	public Limb parent_limb_;

	public GameObject visual;

	public bool isDecorative;

	public bool rigid_limb;

	public Color cube_color;

	public float[] hsv_values;

	public List<Quaternion[]> frames_rotations_;

	public List<Vector3[]> frames_snapPositions_;

	public Transform snap_par;

	public Vector3 snap_global;

	public int animation_id;

	public int dummy_animation_id;

	public bool invertSymmAnm;

	public Dictionary<int, LimbAnimation> cached_animations = new Dictionary<int, LimbAnimation>();

	public bool hide_on_wear_hat;

	public void CreateSnap(Transform par)
	{
		snap_par = par;
	}

	public void SetSnapLocal(Vector3 input)
	{
		if (snap_par == null)
		{
			snap_global = input;
		}
		else
		{
			snap_global = snap_par.TransformPoint(input);
		}
	}

	public Vector3 GetSnapLocal()
	{
		if (snap_global == Vector3.zero)
		{
			return Vector3.zero;
		}
		if (snap_par == null)
		{
			return snap_global;
		}
		return snap_par.InverseTransformPoint(snap_global);
	}

	public void SetUpCollider(GameObject collidr)
	{
		collidr.GetComponent<BoxCollider>().enabled = true;
		collidr.transform.SetParent(visual.transform);
		collidr.transform.localPosition = Vector3.zero;
		collidr.transform.localRotation = Quaternion.identity;
		collidr.transform.localScale = new Vector3(2f, 2f, 2f);
	}

	public void SetDecorative(bool state)
	{
		isDecorative = state;
	}

	public void SetUpFrames(bool newCube)
	{
		frames_rotations_ = new List<Quaternion[]>();
		frames_snapPositions_ = new List<Vector3[]>();
		for (int i = 0; i < CreatureMorpher.Instance.animationNames.Length; i++)
		{
			frames_rotations_.Add(new Quaternion[CreatureMorpher.Instance.maxFrames[i]]);
			frames_snapPositions_.Add(new Vector3[CreatureMorpher.Instance.maxFrames[i]]);
		}
		for (int j = 0; j < GetNumAnimations(); j++)
		{
			for (int k = 0; k < GetNumFrames(j); k++)
			{
				SetFrameRotation(j, k, Quaternion.identity);
				SetFrameSnapPosition(j, k, Vector3.zero);
			}
		}
		hsv_values = new float[4];
	}

	public void ChainMakeDummy(bool first)
	{
		has_dummy = true;
		for (int i = 0; i < sub_limbs.Count; i++)
		{
			sub_limbs[i].ChainMakeDummy(false);
		}
	}

	public int GetNumFrames(int animation_id)
	{
		return frames_rotations_[animation_id].Length;
	}

	public int GetNumAnimations()
	{
		return frames_rotations_.Count;
	}

	public void SetFrameRotation(int animation, int frame, Quaternion value)
	{
		frames_rotations_[animation][frame] = value;
	}

	public void SetFrameSnapPosition(int animation, int frame, Vector3 value)
	{
		frames_snapPositions_[animation][frame] = value;
	}

	public Vector3 GetFrameSnapPosition(int animation, int frame)
	{
		return frames_snapPositions_[animation][frame];
	}

	public Quaternion GetFrameRotation(int animation, int frame)
	{
		return frames_rotations_[animation][frame];
	}

	public void EvalAnimationCurves(int choppiness)
	{
		SetSnapLocal(GetFrameSnapPosition(0, 0));
		Quaternion frameRotation = GetFrameRotation(0, 0);
		if (snap_global != Vector3.zero)
		{
			base.transform.position = Vector3.Lerp(base.transform.position, snap_global, Time.deltaTime * float.MaxValue);
		}
		if (inherit)
		{
			base.transform.localRotation = Quaternion.Lerp(base.transform.localRotation, parent_limb_.transform.localRotation * frameRotation, Time.deltaTime * float.MaxValue);
		}
		else
		{
			base.transform.localRotation = Quaternion.Lerp(base.transform.localRotation, frameRotation, Time.deltaTime * float.MaxValue);
		}
	}
}
