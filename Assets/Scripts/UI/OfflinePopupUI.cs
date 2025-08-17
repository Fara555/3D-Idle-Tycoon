using UnityEngine;
using TMPro;
using UnityEngine.UI;


[DefaultExecutionOrder(-102)]
public class OfflinePopupUI : MonoBehaviour
{
	[SerializeField] private GameObject root;
	[SerializeField] private TMP_Text earnedAmountText;
	[SerializeField] private Button closeButton;
	
	private static OfflinePopupUI instance;
	
	private void OnEnable()
	{
		closeButton.onClick.AddListener(Close);
	}
	
	private void OnDisable()
	{
		closeButton.onClick.RemoveListener(Close);
	}
	
	private void Awake()
	{
		instance = this;
		root.SetActive(false);
	}


	public static void Show(long earnedGold)
	{
		if (instance == null) return;
		instance.ShowInstance(earnedGold);
	}

	private void ShowInstance(long earnedGold)
	{
		root.SetActive(true);
		earnedAmountText.text = BigNumberFormatter.Format(earnedGold, 3);
	}

	private void Close()
	{
		root.SetActive(false);
	}
}