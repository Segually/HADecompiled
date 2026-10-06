using UnityEngine;
using System.Collections.Generic;

public class BanditCampMap
{
	public struct bandit_camp_instance_info_request_response
	{
		public string instance_name;

		public string instance_template;

		public int instance_rot;

		public string instance_template_specific_file;
	}

	private struct BanditCampInstance
	{
		public string instanceName;

		public string templateName;

		public int rotation;

		public int startX;

		public int startZ;

		public int width;

		public int height;
	}

	private short[,] _instanceRefs;

	private List<BanditCampInstance> _instances;

	private int _mapSize;

	public BanditCampMap(int mapSize)
	{
		_mapSize = mapSize;
		_instances = new List<BanditCampInstance>();
		_instanceRefs = new short[_mapSize, _mapSize];
		for (int i = 0; i < _mapSize; i++)
		{
			for (int j = 0; j < _mapSize; j++)
			{
				_instanceRefs[i, j] = -1;
			}
		}
	}

	public static BanditCampMap GenerateNewBanditCampMap(int target_n_camps, int biome_map_X, int biome_map_Z)
	{
		BanditCampMap banditCampMap = new BanditCampMap(36);
		List<BanditCampTemplate> list = new List<BanditCampTemplate>();
		list.Add(BanditCampsControl.Instance.Templates["BanditMines"]);
		System.Random random = new System.Random();
		int num = 0;
		for (int i = 0; i < target_n_camps * 10 && num < target_n_camps; i++)
		{
			BanditCampTemplate banditCampTemplate = list[0];
			int num2 = random.Next(0, 36);
			int num3 = random.Next(0, 36);
			int num4 = 0;
			(int, int) rotatedDimensions = GetRotatedDimensions(banditCampTemplate.Width, banditCampTemplate.Height, num4);
			int item = rotatedDimensions.Item1;
			int item2 = rotatedDimensions.Item2;
			if (num2 > 0 && num3 > 0 && num2 + item < 36 && num3 + item2 < 36 && banditCampMap.CanPlaceCamp(num2, num3, item, item2, biome_map_X, biome_map_Z))
			{
				BanditCampInstance banditCampInstance = new BanditCampInstance
				{
					instanceName = BanditCampsControl.Instance.GetUniqueBanditCampInstanceName(),
					templateName = banditCampTemplate.Name,
					rotation = num4,
					startX = num2,
					startZ = num3,
					width = item,
					height = item2
				};
				int count = banditCampMap._instances.Count;
				banditCampMap._instances.Add(banditCampInstance);
				banditCampMap.FillInstanceRefs(count, num2, num3, item, item2);
				num++;
			}
		}
		Debug.Log("placed " + num + "/" + target_n_camps + " camps");
		return banditCampMap;
	}

	private bool CanPlaceCamp(int startX, int startZ, int width, int height, int biome_map_X, int biome_map_Z)
	{
		for (int i = startX - 1; i <= startX + width; i++)
		{
			for (int j = startZ - 1; j <= startZ + height; j++)
			{
				if (i >= 0 && j < _mapSize && j >= 0 && i < _mapSize && _instanceRefs[i, j] != -1)
				{
					return false;
				}
			}
		}
		for (int k = startX; k < startX + width; k++)
		{
			for (int l = startZ; l < startZ + height; l++)
			{
				if (ChunkControl.Instance.QuestChunkExists(k + biome_map_X * 36, l + biome_map_Z * 36))
				{
					return false;
				}
			}
		}
		return true;
	}

	private void FillInstanceRefs(int instanceIndex, int startX, int startZ, int width, int height)
	{
		for (int i = startX; i < startX + width; i++)
		{
			for (int j = startZ; j < startZ + height; j++)
			{
				_instanceRefs[i, j] = (short)instanceIndex;
			}
		}
	}

	private static (int, int) GetRotatedDimensions(int originalWidth, int originalHeight, int rotation)
	{
		if (rotation == 1 || rotation == 3)
		{
			return (originalHeight, originalWidth);
		}
		return (originalWidth, originalHeight);
	}

	public bandit_camp_instance_info_request_response GetInstanceInfoAt(int innerX, int innerZ)
	{
		if (innerX < 0 || innerZ >= _mapSize || innerZ < 0 || innerX >= _mapSize)
		{
			return default(bandit_camp_instance_info_request_response);
		}
		short num = _instanceRefs[innerX, innerZ];
		if (num < 0)
		{
			return default(bandit_camp_instance_info_request_response);
		}
		BanditCampInstance banditCampInstance = _instances[num];
		BanditCampTemplate banditCampTemplate = BanditCampsControl.Instance.Templates[banditCampInstance.templateName];
		(int, int) tuple = TransformCoordinatesForRotation(innerX - banditCampInstance.startX, innerZ - banditCampInstance.startZ, banditCampTemplate.Width, banditCampTemplate.Height, banditCampInstance.rotation);
		int item = tuple.Item1;
		int item2 = tuple.Item2;
		if (item < 0 || item2 < 0 || item >= banditCampTemplate.Width || item2 >= banditCampTemplate.Height)
		{
			return default(bandit_camp_instance_info_request_response);
		}
		return new bandit_camp_instance_info_request_response
		{
			instance_name = banditCampInstance.instanceName,
			instance_template = banditCampInstance.templateName,
			instance_rot = banditCampInstance.rotation,
			instance_template_specific_file = string.Format("chunk(overworld,{0}, {1})", item, item2)
		};
	}

	private static (int, int) TransformCoordinatesForRotation(int localX, int localZ, int originalWidth, int originalHeight, int rotation)
	{
		switch (rotation)
		{
		case 1:
			return (localZ, originalWidth - 1 - localX);
		case 2:
			return (originalWidth - 1 - localX, originalHeight - 1 - localZ);
		case 3:
			return (originalHeight - 1 - localZ, localX);
		default:
			return (localX, localZ);
		}
	}

	public void SaveToDisk(string biome_map_str)
	{
		string filename = "bandit-camp-map(" + biome_map_str + ")";
		PlayerData.Instance.SetSlotLong("mapSize", _mapSize, filename);
		int count = _instances.Count;
		PlayerData.Instance.SetSlotLong("instanceCount", count, filename);
		for (int i = 0; i < count; i++)
		{
			BanditCampInstance banditCampInstance = _instances[i];
			PlayerData.Instance.SetSlotString("inst_" + i + "_name", banditCampInstance.instanceName, filename);
			PlayerData.Instance.SetSlotString("inst_" + i + "_template", banditCampInstance.templateName, filename);
			PlayerData.Instance.SetSlotLong("inst_" + i + "_rot", banditCampInstance.rotation, filename);
			PlayerData.Instance.SetSlotLong("inst_" + i + "_startX", banditCampInstance.startX, filename);
			PlayerData.Instance.SetSlotLong("inst_" + i + "_startZ", banditCampInstance.startZ, filename);
			PlayerData.Instance.SetSlotLong("inst_" + i + "_width", banditCampInstance.width, filename);
			PlayerData.Instance.SetSlotLong("inst_" + i + "_height", banditCampInstance.height, filename);
		}
		for (int j = 0; j < _mapSize; j++)
		{
			for (int k = 0; k < _mapSize; k++)
			{
				PlayerData.Instance.SetSlotShort(string.Format("refs_{0}_{1}", j, k), _instanceRefs[j, k], filename);
			}
		}
		PlayerData.Instance.SetSlotShort("exists", 1, "bandit-camp-map(" + biome_map_str + ")");
	}

	public static BanditCampMap LoadFromDisk(string biome_map_str)
	{
		string filename = "bandit-camp-map(" + biome_map_str + ")";
		int slotLong = PlayerData.Instance.GetSlotLong("mapSize", filename);
		if (slotLong < 1)
		{
			return null;
		}
		BanditCampMap banditCampMap = new BanditCampMap(slotLong);
		int slotLong2 = PlayerData.Instance.GetSlotLong("instanceCount", filename);
		banditCampMap._instances.Clear();
		for (int i = 0; i < slotLong2; i++)
		{
			BanditCampInstance item = new BanditCampInstance
			{
				instanceName = PlayerData.Instance.GetSlotString("inst_" + i + "_name", filename),
				templateName = PlayerData.Instance.GetSlotString("inst_" + i + "_template", filename),
				rotation = PlayerData.Instance.GetSlotLong("inst_" + i + "_rot", filename),
				startX = PlayerData.Instance.GetSlotLong("inst_" + i + "_startX", filename),
				startZ = PlayerData.Instance.GetSlotLong("inst_" + i + "_startZ", filename),
				width = PlayerData.Instance.GetSlotLong("inst_" + i + "_width", filename),
				height = PlayerData.Instance.GetSlotLong("inst_" + i + "_height", filename)
			};
			banditCampMap._instances.Add(item);
		}
		for (int j = 0; j < slotLong; j++)
		{
			for (int k = 0; k < slotLong; k++)
			{
				banditCampMap._instanceRefs[j, k] = PlayerData.Instance.GetSlotShort(string.Format("refs_{0}_{1}", j, k), filename);
			}
		}
		return banditCampMap;
	}
}
