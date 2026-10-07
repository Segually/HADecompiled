using UnityEngine.UI;
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
		GetComponent<Image>().enabled = false;
	}

	public void VisuallyShow()
	{
		visually_shown = true;
		if (WindowControl.Instance.ShouldRecreateOverheads() || WindowControl.Instance.curr_window == WindowControl.window_type_t.dialogue)
		{
			GetComponent<Image>().enabled = true;
			base.transform.SetParent(MobControl.Instance.gameObject.transform);
			base.transform.SetAsFirstSibling();
			base.transform.localRotation = Quaternion.identity;
			base.transform.localScale = Vector3.one * 0.25f;
			base.transform.localPosition = Vector2.zero;
			Vector2 vector = Vector2.one * 10f;
			((RectTransform)base.transform).anchorMax = vector;
			((RectTransform)base.transform).anchorMin = vector;
		}
		DoSpecialFunctionality();
	}

	public void DoSpecialFunctionality()
	{
		if (instrument_id == 15)
		{
			GameController.Instance.giant_shake_screen();
		}
	}

	public void PlayNote(int key, int instrument_id, bool override_use_single, float delay_in_seconds)
	{
		source = GetComponent<AudioSource>();
		canvasgrp = GetComponent<CanvasGroup>();
		this.instrument_id = instrument_id;
		dither = ((MusicBoxControl.Instance.instruments[instrument_id].override_dither != 0f) ? MusicBoxControl.Instance.instruments[instrument_id].override_dither : 0.1f);
		if (MusicBoxControl.Instance.instruments[instrument_id].single_source || override_use_single)
		{
			PlaySingleSource(key, instrument_id, delay_in_seconds);
		}
		else
		{
			PlayMultiSource(key, instrument_id, delay_in_seconds);
		}
	}

	private void PlaySingleSource(int key, int instrument_id, float delay)
	{
		if (MusicBoxControl.Instance.all_music_box_volume_mod == 0f)
		{
			return;
		}
		float num;
		switch (key % 12)
		{
		case 1:
			num = 1.125f;
			break;
		case 2:
			num = 1.26f;
			break;
		case 3:
			num = 1.333f;
			break;
		case 4:
			num = 1.49f;
			break;
		case 5:
			num = 1.66f;
			break;
		case 6:
			num = 1.89f;
			break;
		case 7:
			num = 1.058f;
			break;
		case 8:
			num = 1.19f;
			break;
		case 9:
			num = 1.41f;
			break;
		case 10:
			num = 1.57f;
			break;
		case 11:
			num = 1.79f;
			break;
		default:
			num = 1f;
			break;
		}
		float num2 = 2f;
		if (key < 24)
		{
			num2 = 1f;
		}
		if (key < 12)
		{
			num2 = 0.5f;
		}
		if (MusicBoxControl.Instance.instruments[instrument_id].override_volume != 0f)
		{
			source.volume = MusicBoxControl.Instance.instruments[instrument_id].override_volume;
		}
		source.volume *= MusicBoxControl.Instance.all_music_box_volume_mod;
		source.pitch = num2 * num;
		if (MusicBoxControl.Instance.instruments[instrument_id].non_resource_sfx == null)
		{
			ResourceControl.Instance.PlayMusicBoxNote(MusicBoxControl.Instance.instruments[instrument_id].middle_c_path, source, delay);
			return;
		}
		source.clip = MusicBoxControl.Instance.instruments[instrument_id].non_resource_sfx;
		source.PlayDelayed(delay);
	}

	private void PlayMultiSource(int key, int instrument_id, float delay)
	{
		if (MusicBoxControl.Instance.all_music_box_volume_mod == 0f)
		{
			return;
		}
		string text;
		float pitch;
		switch (key % 12)
		{
		case 0:
			text = "c";
			pitch = 1f;
			break;
		case 1:
			text = "c";
			pitch = 1.125f;
			break;
		case 2:
			text = "c";
			pitch = 1.26f;
			break;
		case 3:
			text = "c";
			pitch = 1.333f;
			break;
		case 4:
			text = "f#";
			pitch = 1.058f;
			break;
		case 5:
			text = "f#";
			pitch = 1.19f;
			break;
		case 6:
			text = "f#";
			pitch = 1.333f;
			break;
		case 7:
			text = "c";
			pitch = 1.058f;
			break;
		case 8:
			text = "c";
			pitch = 1.19f;
			break;
		case 9:
			text = "f#";
			pitch = 1f;
			break;
		case 10:
			text = "f#";
			pitch = 1.125f;
			break;
		case 11:
			text = "f#";
			pitch = 1.26f;
			break;
		default:
			text = "";
			pitch = 1f;
			break;
		}
		MusicBoxControl.instrument instrument = MusicBoxControl.Instance.instruments[instrument_id];
		if (instrument.override_volume != 0f)
		{
			source.volume = MusicBoxControl.Instance.instruments[instrument_id].override_volume;
		}
		source.volume *= MusicBoxControl.Instance.all_music_box_volume_mod;
		source.pitch = pitch;
		if (key < 12)
		{
			if (text == "c")
			{
				ResourceControl.Instance.PlayMusicBoxNote(instrument.low_c_path, source, delay);
			}
			else if (text == "f#")
			{
				ResourceControl.Instance.PlayMusicBoxNote(instrument.low_f_sh_path, source, delay);
			}
		}
		else if (key < 24)
		{
			if (text == "c")
			{
				ResourceControl.Instance.PlayMusicBoxNote(instrument.middle_c_path, source, delay);
			}
			else if (text == "f#")
			{
				ResourceControl.Instance.PlayMusicBoxNote(instrument.middle_f_sh_path, source, delay);
			}
		}
		else if (text == "c")
		{
			ResourceControl.Instance.PlayMusicBoxNote(instrument.high_c_path, source, delay);
		}
		else if (text == "f#")
		{
			ResourceControl.Instance.PlayMusicBoxNote(instrument.high_f_sh_path, source, delay);
		}
	}

	private void FixedUpdate()
	{
		if (visually_shown)
		{
			position += Vector3.up * 0.005f;
			MobControl.Instance.SnapOverhead((RectTransform)base.transform, position);
		}
		if (die)
		{
			source.volume = Mathf.Max(source.volume - dither, 0f);
			canvasgrp.alpha = source.volume;
			if (source.volume == 0f)
			{
				Delete();
			}
		}
	}

	public void KillNote()
	{
		if (!die)
		{
			die = true;
			MusicBoxControl.Instance.notes_dithering.Add(base.gameObject);
		}
	}

	private void Delete()
	{
		MusicBoxControl.Instance.notes_dithering.Remove(base.gameObject);
		Object.Destroy(base.gameObject);
		if (MusicBoxControl.Instance.n_music_notes > 0)
		{
			MusicBoxControl.Instance.n_music_notes--;
		}
	}
}
