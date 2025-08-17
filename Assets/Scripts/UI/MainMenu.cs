using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class MainMenu : MonoBehaviour
{
	[Header("Buttons")]
	[SerializeField] private Button playButton;
	[SerializeField] private Button deleteSaveButton;
	[SerializeField] private Button exitButton;

	[Header("References")] 
	[SerializeField] private GameObject deletePanel;

	[Header("Scene To Load")]
	[SerializeField] private string playSceneName = "Game";

	private void OnEnable()
	{
		playButton.onClick.AddListener(OnPlayClicked);
		deleteSaveButton.onClick.AddListener(OnDeleteSaveClicked);
		exitButton.onClick.AddListener(OnExitClicked);
		
		UpdateDeleteButtonState();
		deletePanel.SetActive(false);
	}

	private void OnDisable()
	{
		playButton.onClick.RemoveListener(OnPlayClicked);
		deleteSaveButton.onClick.RemoveListener(OnDeleteSaveClicked);
		exitButton.onClick.RemoveListener(OnExitClicked);
	}

	private void OnPlayClicked()
	{
		SceneManager.LoadScene(playSceneName, LoadSceneMode.Single);
	}

	private void OnExitClicked()
	{
		Application.Quit();
	}
	
	private void OnDeleteSaveClicked()
	{
		deletePanel.SetActive(true);
		UpdateDeleteButtonState();
	}
	
	private void UpdateDeleteButtonState()
	{
			deleteSaveButton.interactable = SaveManager.HasSave();
	}
}