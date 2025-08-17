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
		Save();
	}
	
	private void OnApplicationPause(bool pause)
	{
		if (pause)
			Save();
	}
	
	public void Save() => SaveManager.SaveGame(allBuildings);
}