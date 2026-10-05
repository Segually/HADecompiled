using System.Collections;
using UnityEngine;

public interface CreatureBrainInterface
{
	void Init();

	IEnumerator Think();

	void ReactOnHit(GameObject hit_by);

	void OnFallOffWorld();

	void TriggerOnReachDesiredMoveAt();

	void CustomFixedUpdate();
}
