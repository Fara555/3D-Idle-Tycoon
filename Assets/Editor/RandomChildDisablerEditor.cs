#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class RandomChildDisablerEditor : MonoBehaviour
{
	[ContextMenu("Случайно отключить дочерние объекты")]
	public void DisableRandomChildren()
	{
		Transform parent = transform;
		int childCount = parent.childCount;

		if (childCount == 0)
		{
			Debug.LogWarning("Нет дочерних объектов.");
			return;
		}

		int numberToDisable = Random.Range(1, childCount + 1);

		// Создаём массив индексов и перемешиваем его
		int[] indices = new int[childCount];
		for (int i = 0; i < childCount; i++) indices[i] = i;
		System.Random rng = new System.Random();
		for (int i = indices.Length - 1; i > 0; i--)
		{
			int j = rng.Next(i + 1);
			(indices[i], indices[j]) = (indices[j], indices[i]);
		}

		// Включаем всех
		for (int i = 0; i < childCount; i++)
		{
			GameObject go = parent.GetChild(i).gameObject;
			Undo.RecordObject(go, "Enable Child");
			go.SetActive(true);
			EditorUtility.SetDirty(go);
		}

		// Отключаем случайные
		for (int i = 0; i < numberToDisable; i++)
		{
			GameObject go = parent.GetChild(indices[i]).gameObject;
			Undo.RecordObject(go, "Disable Child");
			go.SetActive(false);
			EditorUtility.SetDirty(go);
		}

		Debug.Log($"Отключено {numberToDisable} из {childCount} дочерних объектов.");
	}

	[ContextMenu("Удалить отключённые дочерние объекты")]
	public void DeleteInactiveChildren()
	{
		Transform parent = transform;
		int deletedCount = 0;

		// Собираем отключённые объекты в список
		var toDelete = new System.Collections.Generic.List<GameObject>();
		foreach (Transform child in parent)
		{
			if (!child.gameObject.activeSelf)
			{
				toDelete.Add(child.gameObject);
			}
		}

		// Удаляем
		foreach (var go in toDelete)
		{
			Undo.DestroyObjectImmediate(go);
			deletedCount++;
		}

		Debug.Log($"Удалено {deletedCount} отключённых дочерних объектов.");
	}
}
#endif