using UnityEngine;
using TMPro;
using UnityEngine.UI;

[DefaultExecutionOrder(-102)]
public class OfflinePopupUI : MonoBehaviour
{
	[Header("Refs")]
	[SerializeField] private GameObject panelRoot;    
	[SerializeField] private TMP_Text earnedAmountText;
	[SerializeField] private Button closeButton;

	private static OfflinePopupUI instance;
	private UIWindow window;

	private void Awake()
	{
		instance = this;

		if (!panelRoot) panelRoot = gameObject; 
		window = panelRoot.GetComponent<UIWindow>();
		if (!window)
		{
			window = panelRoot.AddComponent<UIWindow>();
		}
	}

	private void OnEnable()
	{
		if (closeButton) closeButton.onClick.AddListener(Close);
	}

	private void OnDisable()
	{
		if (closeButton) closeButton.onClick.RemoveListener(Close);
	}
	
	public static void Show(long earnedGold)
	{
		if (instance == null) return;
		instance.ShowInstance(earnedGold);
	}

	private void ShowInstance(long earnedGold)
	{
		if (!panelRoot.activeSelf) panelRoot.SetActive(true); 
		if (earnedAmountText) earnedAmountText.text = BigNumberFormatter.Format(earnedGold, 3);
		if (window) window.Show();
		else panelRoot.SetActive(true);
	}

	private void Close()
	{
		if (window) window.Hide();
		else panelRoot.SetActive(false);
	}
}