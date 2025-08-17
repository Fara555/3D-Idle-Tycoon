using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeleteSaveWindow : MonoBehaviour
{
	[Header("Buttons")]
	[SerializeField] private Button yesButton;
	[SerializeField] private Button noButton;
	[SerializeField] private Button deleteSaveButton;
	
	private void OnEnable()
	{
		yesButton.onClick.AddListener(OnConfirmDelete);
		noButton.onClick.AddListener(OnRejectDelete);
	}
	
	private void OnDisable()
	{
		yesButton.onClick.RemoveListener(OnConfirmDelete);
		noButton.onClick.RemoveListener(OnRejectDelete);
	}

	private void OnConfirmDelete()
	{
		SaveManager.DeleteSave();
		deleteSaveButton.interactable = SaveManager.HasSave();
		gameObject.SetActive(false);
	}
	
	private void OnRejectDelete()
	{
		gameObject.SetActive(false);
	}
}
