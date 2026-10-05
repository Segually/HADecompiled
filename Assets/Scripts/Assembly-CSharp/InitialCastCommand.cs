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
	}

	public void UnpackFromWeb(Packet incoming)
	{
	}
}
