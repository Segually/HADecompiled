using System.Collections.Generic;
using UnityEngine;

public class OnlinePlayerData
{
	public Vector3 at;

	public Vector3 to;

	public Quaternion rot;

	public bool is_dead;

	public string currently_using;

	public string sitting_in_chair;

	public int level;

	public InventoryItem hat_;

	public InventoryItem body_;

	public InventoryItem hand_;

	public int hp_max;

	public int hp;

	public int hp_regen;

	public List<string> creatures = new List<string>();

	public void Unpack(Packet incoming)
	{
		at = GameServerReceiver.Instance.UnpackPosition(incoming);
		to = GameServerReceiver.Instance.UnpackPosition(incoming);
		rot = GameServerReceiver.Instance.UnpackRotation(incoming);
		is_dead = incoming.GetByte() == 1;
		currently_using = incoming.GetString();
		sitting_in_chair = incoming.GetString();
		level = incoming.GetLong();
		hat_ = InventoryItem.UnpackFromWeb(incoming);
		body_ = InventoryItem.UnpackFromWeb(incoming);
		hand_ = InventoryItem.UnpackFromWeb(incoming);
		hp_max = incoming.GetLong();
		hp = incoming.GetLong();
		hp_regen = incoming.GetLong();
		int count = incoming.GetShort();
		for (int i = 0; i < count; i++) creatures.Add(incoming.GetString());
	}
}
