using UnityEngine;
using System;

[DisallowMultipleComponent]
public class UniqueId : MonoBehaviour
{
	[SerializeField] private string uniqueId;
	public string Id => uniqueId;

	private void Awake()
	{
		if (string.IsNullOrEmpty(uniqueId))
			GenerateNewId();
	}

	private void OnValidate()
	{
		if (string.IsNullOrEmpty(uniqueId))
			GenerateNewId(); 
	}

	public void GenerateNewId()
	{
		uniqueId = Guid.NewGuid().ToString();
	}
	
	public void LoadId(string existingId)
	{
		if (!string.IsNullOrEmpty(existingId))
			uniqueId = existingId;
	}

}