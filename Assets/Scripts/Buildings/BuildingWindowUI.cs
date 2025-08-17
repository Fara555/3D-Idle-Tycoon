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
    private UIWindow _window;

    void Awake()
    {
        if (panel)
        {
            _window = panel.GetComponent<UIWindow>();
            if (_window == null)
                _window = panel.AddComponent<UIWindow>(); 
        }

        if (closeBtn) closeBtn.onClick.AddListener(Hide);
    }

    public virtual void Show(BuildingLogic logic)
    {
        _logic = logic;
        _logic.OnStateChanged += Refresh;

        if (_window) _window.Show();
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
                costText.text = $"Upgrade: {BigNumberFormatter.Format(_logic.GetUpgradeCost(), 3)}";
            else
                costText.text = "Max Level";
        }
        else
        {
            costText.text = $"Build: {BigNumberFormatter.Format(_logic.GetBuildCost(), 3)}";
        }

        speedText.text = $"{_logic.GetCycleLabel()}: {BigNumberFormatter.Format(_logic.GetCycleTime(), 3)} s";
        amountText.text = $"{_logic.GetAmountLabel()}: {BigNumberFormatter.Format(_logic.GetCatchAmount(), 3)}";

        buildBtn.gameObject.SetActive(!_logic.IsBuilt);
        upgradeBtn.gameObject.SetActive(_logic.IsBuilt && _logic.Level < _logic.MaxLevel);
    }

    public virtual void Hide()
    {
        if (_window) _window.Hide();
        if (_logic != null) _logic.OnStateChanged -= Refresh;
        _logic = null;
    }
}
