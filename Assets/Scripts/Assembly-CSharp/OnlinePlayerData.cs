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

	public List<string> creatures;

	public void Unpack(Packet incoming)
	{
	}
}
