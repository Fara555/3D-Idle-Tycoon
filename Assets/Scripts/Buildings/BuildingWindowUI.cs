using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IBuildingWindowUI
{
	void Show(BuildingLogic logic);
	void Refresh();
	void Hide();
}

public abstract class BuildingWindowUI : MonoBehaviour, IBuildingWindowUI
{
	[SerializeField] private GameObject panel;
	[SerializeField] private TextMeshProUGUI titleText, levelText, costText;
	[SerializeField] private TextMeshProUGUI speedText, amountText; 
	[SerializeField] private Button buildBtn, upgradeBtn, closeBtn;

	private BuildingLogic _logic;
	

	void Awake()
	{
		if (panel) panel.SetActive(false);
		if (closeBtn) closeBtn.onClick.AddListener(Hide);
	}

	public virtual void Show(BuildingLogic logic)
	{
		_logic = logic;
		_logic.OnStateChanged += Refresh;

		if (panel) panel.SetActive(true);
		Refresh();

		buildBtn.onClick.RemoveAllListeners();
		upgradeBtn.onClick.RemoveAllListeners();
		
		buildBtn.onClick.AddListener(() => { _logic.Build(); Refresh(); });
		upgradeBtn.onClick.AddListener(() => { _logic.Upgrade(); Refresh(); });
	}

	public virtual void Refresh()
	{
		if (_logic == null) return;

		titleText.text = _logic.BuildingName;
		levelText.text = _logic.IsBuilt ? $"Level: {_logic.Level}" : "Broken";
		
		if (_logic.IsBuilt)
		{
			if (_logic.Level < _logic.MaxLevel)
				costText.text = $"Upgrade: {_logic.GetUpgradeCost()}";
			else
				costText.text = "Max Level";
		}
		else
		{
			costText.text = $"Build: {_logic.GetBuildCost()}";
		}
		
		speedText.text = $"{_logic.GetCycleLabel()}: {_logic.GetCycleTime():0.#} s";
		amountText.text = $"{_logic.GetAmountLabel()}: {_logic.GetCatchAmount()}";

		buildBtn.gameObject.SetActive(!_logic.IsBuilt);
		upgradeBtn.gameObject.SetActive(_logic.IsBuilt && _logic.Level < _logic.MaxLevel);
	}

	public virtual void Hide()
	{
		if (panel) panel.SetActive(false);
		if (_logic != null) _logic.OnStateChanged -= Refresh;
		_logic = null;
	}
}