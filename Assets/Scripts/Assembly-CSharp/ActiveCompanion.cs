using UnityEngine;

public class ActiveCompanion
{
	public GameObject obj;

	public int hatch_index;

	public bool is_temp_companion;

	public InventoryItem companion_item;

	public string companion_name => null;

	public string combat_name => null;

	public string creature_A => null;

	public string creature_B => null;

	public string guard_message1 => null;

	public string guard_message2 => null;

	public string wait_message1 => null;

	public string wait_message2 => null;

	public string wait_message3 => null;

	public string wait_message4 => null;

	public int level => 0;

	public int curr_exp => 0;

	public int next_exp => 0;

	public int wait_icon => 0;

	public bool attack_xp_orbs => false;

	public InventoryItem hat_ => null;

	public InventoryItem body_ => null;

	public InventoryItem hand_ => null;

	public static InventoryItem CreateNewItem(string creatureA, string creatureB, int level, int curr_exp, int next_exp, string combat_name, string companion_name, int pocket_container_id, string wait_message1, string wait_message2, string wait_message3, string wait_message4, string guard_message1, string guard_message2, int wait_icon, bool attack_XP_orbs, InventoryItem start_weapon, InventoryItem start_hat, InventoryItem start_armor, string merchant_message1, string merchant_message2)
	{
		return null;
	}

	public BasketContents GetPocketContents()
	{
		return null;
	}
}
