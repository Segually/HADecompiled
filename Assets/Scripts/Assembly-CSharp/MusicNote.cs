using UnityEngine;

public class MusicNote : MonoBehaviour
{
	private AudioSource source;

	private CanvasGroup canvasgrp;

	private bool die;

	private float dither;

	public Vector3 position;

	public bool visually_shown;

	private int instrument_id;

	public void VisuallyHide()
	{
	}

	public void VisuallyShow()
	{
	}

	public void DoSpecialFunctionality()
	{
	}

	public void PlayNote(int key, int instrument_id, bool override_use_single, float delay_in_seconds)
	{
	}

	private void PlaySingleSource(int key, int instrument_id, float delay)
	{
	}

	private void PlayMultiSource(int key, int instrument_id, float delay)
	{
	}

	private void FixedUpdate()
	{
	}

	public void KillNote()
	{
	}

	private void Delete()
	{
	}
}
