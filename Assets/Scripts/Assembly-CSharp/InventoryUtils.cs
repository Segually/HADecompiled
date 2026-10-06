using System.Collections.Generic;
using UnityEngine;

public class InventoryUtils
{
	public enum building_type
	{
		unknown = 0,
		shack = 1,
		mansion = 2,
		castle = 3,
		underground_room = 4,
		upstairs_room = 5,
		tent = 6,
		igloo = 7,
		windmill = 8,
		warehouse = 9
	}

	public static bool IsHouseObject(string item_name)
	{
		building_type buildingType = GetBuildingType(item_name);
		if ((uint)(buildingType - 1) < 8u)
		{
			return true;
		}
		return buildingType == building_type.warehouse;
	}

	public static bool IsStringItem(string item_name)
	{
		if (!(item_name == "String Lights") && !(item_name == "Holiday Lights") && !(item_name == "Blue String Lights"))
		{
			return item_name == "Red String Lights";
		}
		return true;
	}

	public static bool IsSimpleMob(string item_name)
	{
		return false;
	}

	public static bool ShouldReplaceOrDeleteExactItem(string item_name)
	{
		if (!(item_name == "String Lights") && !(item_name == "Holiday Lights") && !(item_name == "Blue String Lights"))
		{
			return item_name == "Red String Lights";
		}
		return true;
	}

	public static bool UsesShackId(string item_name)
	{
		int buildingType = (int)GetBuildingType(item_name);
		if (buildingType - 1 > 8 && !IsCaveObject(item_name) && !IsHeavenDimension(item_name) && !IsPureDimension(item_name) && !(item_name == "Pocket World Basement"))
		{
			return IsHellDimension(item_name);
		}
		return true;
	}

	public static bool UsesBasketId(InventoryItem item)
	{
		if (item.GetShort("uses_basket_id") == 1)
		{
			return true;
		}
		string item_name = item.item_name;
		if (!(item_name == "Basket") && !(item_name == "Chest") && !(item_name == "Egg Fuser") && !(item_name == "Gold Chest") && !(item_name == "Titanium Chest") && !(item_name == "Sky Chest") && !(item_name == "Cave Basket") && !(item_name == "Cave Chest") && !(item_name == "Weapon Display") && !(item_name == "Large Weapon Display") && !(item_name == "Armor Display") && !(item_name == "Custom Statue") && !(item_name == "Crate") && !(item_name == "Double Crate") && !(item_name == "Loot Basket") && !(item_name == "Loot Chest"))
		{
			return item_name == "Boss Chest";
		}
		return true;
	}

	public static bool IsChairObject(string item_name)
	{
		if (!(item_name == "Chair") && !(item_name == "Metal Chair") && !(item_name == "Stump Chair") && !(item_name == "Sofa Chair") && !(item_name == "Throne"))
		{
			return item_name == "Park Bench";
		}
		return true;
	}

	public static bool IsBedObject(string item_name)
	{
		return item_name == "Bed";
	}

	public static bool IsCaveObject(string obj_name)
	{
		if (!(obj_name == "cave") && !(obj_name == "Personal Mine") && !(obj_name == "Grass Cave Entrance") && !(obj_name == "Snow Cave Entrance") && !(obj_name == "Desert Cave Entrance") && !(obj_name == "Evergreen Cave Entrance") && !(obj_name == "Ocean Cave Entrance"))
		{
			return obj_name == "Swamp Cave Entrance";
		}
		return true;
	}

	public static bool IsHeavenDimension(string item_name)
	{
		return item_name == "Magic Bean";
	}

	public static bool IsHellDimension(string item_name)
	{
		return item_name == "Spooky Well";
	}

	public static bool IsPureDimension(string itme_name)
	{
		if (itme_name == "Pocket World Snow")
		{
			return true;
		}
		return itme_name == "Pocket World Evergreen";
	}

	public static float GetEquipmentScale(string hat_item_name, string body_item_name)
	{
		if (hat_item_name == "Ancient Skull Helm" || hat_item_name == "Ancient Eldritch Helm" || body_item_name == "Ancient Armor" || hat_item_name == "Spirit Eldritch Helm" || body_item_name == "Spirit Armor")
		{
			return 1.7f;
		}
		return 1f;
	}

	public static string GetEquipmentSkinMat(string hat_item_name, string body_item_name)
	{
		if (hat_item_name == "Spirit Eldritch Helm" || body_item_name == "Spirit Armor")
		{
			return "Blue Glow";
		}
		return "";
	}

	public static bool IsPaintbrush(string item_name)
	{
		if (item_name == "Debug Paint" || item_name == "Default")
		{
			return true;
		}
		return ResourceControl.Instance.GetStringFromItemFile(item_name, "is_paintbrush") == "true";
	}

	public static bool IsStamp(string item_name)
	{
		return ResourceControl.Instance.GetStringFromItemFile(item_name, "is_stamp") == "true";
	}

	public static float GetBonsaiScale(int unique_id)
	{
		if (unique_id > 100)
		{
			float num = Mathf.Pow((float)(unique_id - 100) / 1000f, 0.6f);
			float result = num + 1f;
			if (num + 1f > 2.9f)
			{
				result = 2.9f;
			}
			return result;
		}
		return 1f;
	}

	public static int GenerateBonsaiAge()
	{
		float value = Random.value;
		float value2 = Random.value;
		if (value < 0.94f)
		{
			if (value2 < 0.8f)
			{
				return Random.Range(10, 130);
			}
			if (value2 < 0.95f)
			{
				return Random.Range(50, 230);
			}
			return Random.Range(70, 400);
		}
		if (value2 < 0.8f)
		{
			return Random.Range(130, 421);
		}
		if (value2 < 0.95f)
		{
			return Random.Range(230, 1000);
		}
		return Random.Range(400, 3010);
	}

	public static building_type GetBuildingType(string item_name)
	{
		switch (item_name)
		{
		case "mansion":
		case "shack_red":
		case "Shack":
		case "shack_blue":
		case "shack":
		case "shack_default":
			return building_type.shack;
		case "Mansion":
			return building_type.mansion;
		case "Castle":
			return building_type.castle;
		case "Underground Room":
			return building_type.underground_room;
		case "Upstairs Room":
			return building_type.upstairs_room;
		case "Tent":
			return building_type.tent;
		case "Igloo":
			return building_type.igloo;
		case "Windmill":
			return building_type.windmill;
		case "Warehouse":
			return building_type.warehouse;
		default:
			return building_type.unknown;
		}
	}

	public static List<Vector3> GetShackBuildArea()
	{
		return null;
	}

	public static List<Vector3> GetShackLeftWalls()
	{
		return null;
	}

	public static List<Vector3> GetShackRightWalls()
	{
		return null;
	}

	public static List<Vector3> GetShackForwardWalls()
	{
		return null;
	}

	public static List<Vector3> GetShackBackWalls()
	{
		return null;
	}

	public static List<Vector3> GetMansionBuildArea()
	{
		return null;
	}

	public static List<Vector3> GetMansionLeftWalls()
	{
		return null;
	}

	public static List<Vector3> GetMansionRightWalls()
	{
		return null;
	}

	public static List<Vector3> GetMansionForwardWalls()
	{
		return null;
	}

	public static List<Vector3> GetMansionBackWalls()
	{
		return null;
	}

	public static List<Vector3> GetWarehouseBuildArea()
	{
		return null;
	}

	public static List<Vector3> GetWarehouseLeftWalls()
	{
		return null;
	}

	public static List<Vector3> GetWarehouseRightWalls()
	{
		return null;
	}

	public static List<Vector3> GetWarehouseForwardWalls()
	{
		return null;
	}

	public static List<Vector3> GetWarehouseBackWalls()
	{
		return null;
	}

	public static List<Vector3> GetWindmillBuildArea()
	{
		return null;
	}

	public static List<Vector3> GetWindmillLeftWalls()
	{
		return null;
	}

	public static List<Vector3> GetWindmillRightWalls()
	{
		return null;
	}

	public static List<Vector3> GetWindmillForwardWalls()
	{
		return null;
	}

	public static List<Vector3> GetWindmillBackWalls()
	{
		return null;
	}

	public static List<Vector3> GetCastleBuildArea()
	{
		return null;
	}

	public static List<Vector3> GetCastleLeftWalls()
	{
		return null;
	}

	public static List<Vector3> GetCastleRightWalls()
	{
		return null;
	}

	public static List<Vector3> GetCastleForwardWalls()
	{
		return null;
	}

	public static List<Vector3> GetCastleBackWalls()
	{
		return null;
	}

	public static List<Vector3> GetUndergroundBuildArea()
	{
		return null;
	}

	public static List<Vector3> GetUndergroundLeftWalls()
	{
		return null;
	}

	public static List<Vector3> GetUndergroundRightWalls()
	{
		return null;
	}

	public static List<Vector3> GetUndergroundForwardWalls()
	{
		return null;
	}

	public static List<Vector3> GetUndergroundBackWalls()
	{
		return null;
	}

	public static string GetCoinSprite(int count)
	{
		if (count == 1)
		{
			return "item coins 1";
		}
		if (count < 13)
		{
			return "item coins 2";
		}
		if (count < 160)
		{
			return "item coins 3";
		}
		if (count < 1000)
		{
			return "item coins 4";
		}
		if (count > 9999)
		{
			return "item coins 6";
		}
		return "item coins 5";
	}

	public static int GetPickTier(string hand_obj)
	{
		if (hand_obj == "Wood Pick")
		{
			return 1;
		}
		if (hand_obj == "Stone Pick")
		{
			return 2;
		}
		if (hand_obj == "Metal Pick")
		{
			return 3;
		}
		if (hand_obj == "Titanium Pick")
		{
			return 4;
		}
		return 0;
	}

	public static List<UniqueIdStatus> GenerateUniqueIdSummary()
	{
		return null;
	}
}
