using UnityEngine;

public class ActiveCompanion
{
	public GameObject obj;

	public int hatch_index;

	public bool is_temp_companion;

	public InventoryItem companion_item;

	public string companion_name => companion_item.GetString("npc_display_name");

	public string combat_name => companion_item.GetString("combat_name");

	public string creature_A => companion_item.GetString("creature_A");

	public string creature_B => companion_item.GetString("creature_B");

	public string guard_message1 => companion_item.GetString("guard_message1");

	public string guard_message2 => companion_item.GetString("guard_message2");

	public string wait_message1 => companion_item.GetString("wait_message1");

	public string wait_message2 => companion_item.GetString("wait_message2");

	public string wait_message3 => companion_item.GetString("wait_message3");

	public string wait_message4 => companion_item.GetString("wait_message4");

	public int level => companion_item.GetLong("level");

	public int curr_exp => companion_item.GetShort("curr_exp");

	public int next_exp => companion_item.GetShort("next_exp");

	public int wait_icon => companion_item.GetShort("npc_icon");

	public bool attack_xp_orbs => companion_item.GetShort("attack_xp_orbs") == 1;

	public InventoryItem hat_ => GetPocketContents()[3].item;

	public InventoryItem body_ => GetPocketContents()[8].item;

	public InventoryItem hand_ => GetPocketContents()[13].item;

	public static InventoryItem CreateNewItem(string creatureA, string creatureB, int level, int curr_exp, int next_exp, string combat_name, string companion_name, int pocket_container_id, string wait_message1, string wait_message2, string wait_message3, string wait_message4, string guard_message1, string guard_message2, int wait_icon, bool attack_XP_orbs, InventoryItem start_weapon, InventoryItem start_hat, InventoryItem start_armor, string merchant_message1, string merchant_message2)
	{
		ExtraInventoryData data = new ExtraInventoryData();
		data.SetString("npc_display_name", companion_name);
		data.SetString("combat_name", combat_name);
		data.SetString("creature_A", creatureA);
		data.SetString("creature_B", creatureB);
		data.SetString("guard_message1", guard_message1);
		data.SetString("guard_message2", guard_message2);
		data.SetString("wait_message1", wait_message1);
		data.SetString("wait_message2", wait_message2);
		data.SetString("wait_message3", wait_message3);
		data.SetString("wait_message4", wait_message4);
		data.SetLong("level", level);
		data.SetShort("curr_exp", curr_exp);
		data.SetShort("next_exp", next_exp);
		data.SetShort("npc_icon", wait_icon);
		data.SetShort("attack_xp_orbs", attack_XP_orbs ? 1 : 0);
		data.SetString("merchant_message1", merchant_message1);
		data.SetString("merchant_message2", merchant_message2);
		InventoryItem item = new InventoryItem("Companion", data);
		ItemCountPair[] pockets = new ItemCountPair[inventory_ctr.n_slots_per_page_];
		for (int i = 0; i < pockets.Length; i++) pockets[i] = new ItemCountPair("", 0);
		pockets[3] = new ItemCountPair(start_hat, 1);
		pockets[8] = new ItemCountPair(start_armor, 1);
		pockets[13] = new ItemCountPair(start_weapon, 1);
		return ChunkControl.Instance.EncodeItemListIntoItem("pockets", pockets, item);
	}

	public BasketContents GetPocketContents()
	{
		ItemCountPair[] items = ChunkControl.Instance.GetItemListFromItem("pockets", companion_item);
		BasketContents contents = new BasketContents();
		for (int i = 0; i < items.Length; i++) contents[i] = items[i];
		return contents;
	}
}
