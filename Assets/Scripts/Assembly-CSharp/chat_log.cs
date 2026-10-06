using System.Collections.Generic;
using UnityEngine;

public class chat_log
{
	public string text;

	public string text_simplified_for_report;

	public Sprite img;

	public bool has_bg;

	public Dictionary<string, string> accept_or_deny_data;

	public chat_log(string text, string text_simplified_for_report, Sprite img, bool has_bg, Dictionary<string, string> accept_or_deny_data)
	{
		this.text = text;
		this.text_simplified_for_report = text_simplified_for_report;
		this.img = img;
		this.accept_or_deny_data = accept_or_deny_data;
		this.has_bg = has_bg;
	}
}
