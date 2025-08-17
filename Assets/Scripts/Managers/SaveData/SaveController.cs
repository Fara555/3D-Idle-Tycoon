using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-101)]
public class SaveController : MonoBehaviour
{
	[SerializeField] private List<BuildingLogic> allBuildings;

	private void Awake()
	{
		SaveManager.LoadGame(allBuildings);
	}

	private void OnApplicationQuit()
	{
		SaveManager.SaveGame(allBuildings);
	}
	
	private void OnApplicationPause(bool pause)
	{
		if (pause)
			SaveManager.SaveGame(allBuildings);
	}
	
	// public void Save() => SaveManager.SaveGame(allBuildings);
	// public void Load() => SaveManager.LoadGame(allBuildings);
}