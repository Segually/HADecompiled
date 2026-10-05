using System;
using System.Collections.Generic;
using System.IO;

public class SlotFilesGroup
{
	private string folder = "";

	public Dictionary<string, SingleFile> loaded_files = new Dictionary<string, SingleFile>();

	public SlotFilesGroup(string folder)
	{
		this.folder = folder;
	}

	public void InstantlySaveAll()
	{
		foreach (KeyValuePair<string, SingleFile> loaded_file in loaded_files)
		{
			if (loaded_file.Value.edited_since_load)
			{
				SaveSlotFile(loaded_file.Key);
			}
		}
	}

	public string[] FindRelatedFiles()
	{
		return Directory.GetFiles(Startup.persistentDataPath + Path.DirectorySeparatorChar + folder);
	}

	public void SaveSlotFile(string generic_filename)
	{
		SingleFile file = GetFile(generic_filename);
		byte[] bytes = file.ToByteArray();
		File.WriteAllBytes(Path.Combine(Startup.persistentDataPath + Path.DirectorySeparatorChar + folder, generic_filename), bytes);
		file.edited_since_load = false;
	}

	public SingleFile GetFile(string generic_filename)
	{
		if (loaded_files.ContainsKey(generic_filename))
		{
			return loaded_files[generic_filename];
		}
		string full_path = Path.Combine(Startup.persistentDataPath + Path.DirectorySeparatorChar + folder, generic_filename);
		SingleFile singleFile = PlayerData.Instance.TryLoadFromDiskWithNoExtension(full_path);
		if (singleFile == null)
		{
			singleFile = new SingleFile();
		}
		if (!loaded_files.ContainsKey(generic_filename))
		{
			loaded_files.Add(generic_filename, singleFile);
		}
		return singleFile;
	}

	public void DeloadUnusedFiles()
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, SingleFile> loaded_file in loaded_files)
		{
			if ((DateTime.Now - loaded_file.Value.last_time_used).TotalSeconds > 30.0)
			{
				if (loaded_file.Value.edited_since_load)
				{
					SaveSlotFile(loaded_file.Key);
				}
				list.Add(loaded_file.Key);
			}
		}
		foreach (string item in list)
		{
			loaded_files.Remove(item);
		}
	}

	public void DeleteOneFile(string generic_file_name)
	{
		if (loaded_files.ContainsKey(generic_file_name))
		{
			loaded_files.Remove(generic_file_name);
			string path = Path.Combine(Startup.persistentDataPath + Path.DirectorySeparatorChar + folder, generic_file_name);
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
	}
}
