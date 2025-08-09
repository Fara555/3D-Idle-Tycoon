using UnityEngine;
using UnityEditor;

public class ReplaceWithPrefabWindow : EditorWindow
{
	private GameObject prefabToUse;

	[MenuItem("Tools/Replace With Prefab Tool")]
	public static void ShowWindow()
	{
		GetWindow<ReplaceWithPrefabWindow>("Replace With Prefab");
	}

	private void OnGUI()
	{
		GUILayout.Label("Выбери префаб, которым заменить объекты:", EditorStyles.boldLabel);

		prefabToUse = (GameObject)EditorGUILayout.ObjectField("Prefab", prefabToUse, typeof(GameObject), false);

		if (GUILayout.Button("Заменить выделенные объекты"))
		{
			if (prefabToUse == null)
			{
				Debug.LogError("Префаб не выбран.");
				return;
			}

			foreach (GameObject selected in Selection.gameObjects)
			{
				GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToUse);
				newObj.transform.position = selected.transform.position;
				newObj.transform.rotation = selected.transform.rotation;
				newObj.transform.localScale = selected.transform.localScale;
				newObj.transform.SetParent(selected.transform.parent);

				Undo.RegisterCreatedObjectUndo(newObj, "Replaced with Prefab");
				Undo.DestroyObjectImmediate(selected);
			}

			Debug.Log($"Заменено {Selection.gameObjects.Length} объектов на {prefabToUse.name}");
		}
	}
}