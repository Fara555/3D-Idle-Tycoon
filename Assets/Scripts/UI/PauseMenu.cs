using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
	[Header("References")] 
	[SerializeField] private GameObject pausePanel;
	[SerializeField] private SaveController saveController;
	
	[Header("Buttons")]
	[SerializeField] private Button resumeButton;
	[SerializeField] private Button returnButton;
	
	[Header("Scene To Load")]
	[SerializeField] private string mainMenuSceneName = "MainMenu";
	
	private UIWindow window;

	private void Awake()
	{
		window = pausePanel.GetComponent<UIWindow>();
	}

	private void OnEnable()
	{
		resumeButton.onClick.AddListener(ResumeGame);
		returnButton.onClick.AddListener(ReturnToMainMenu);
	}
	
	private void OnDisable()
	{
		resumeButton.onClick.RemoveListener(ResumeGame);
		returnButton.onClick.RemoveListener(ReturnToMainMenu);
	}

	private void ResumeGame()
	{
		window.Hide();
	}

	private void ReturnToMainMenu()
	{
		saveController.Save();
		SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
	}
}