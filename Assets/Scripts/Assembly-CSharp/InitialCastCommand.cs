using System.Collections.Generic;

public class InitialCastCommand
{
	public enum projectile_target
	{
		none = 0,
		enemies = 1,
		allies = 2
	}

	public enum initial_cast_type
	{
		on_self = 0,
		on_projectile = 1,
		on_quick_tag = 2,
		on_click_location = 3
	}

	public List<string> effect_names = new List<string>();

	public string projectile_model = "";

	public initial_cast_type type;

	public projectile_target projectile_target_type;

	public void PackForWeb(Packet outgoing)
	{
		outgoing.PutShort(effect_names.Count);
		foreach (string name in effect_names) outgoing.PutString(name);
		outgoing.PutString(projectile_model);
		outgoing.PutByte((byte)type);
		outgoing.PutByte((byte)projectile_target_type);
	}

	public void UnpackFromWeb(Packet incoming)
	{
		int count = incoming.GetShort();
		for (int i = 0; i < count; i++) effect_names.Add(incoming.GetString());
		projectile_model = incoming.GetString();
		type = (initial_cast_type)incoming.GetByte();
		projectile_target_type = (projectile_target)incoming.GetByte();
	}
}
