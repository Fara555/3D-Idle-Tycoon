using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BoatUI : MonoBehaviour
{
    public static BoatUI I;

    [Header("Refs")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI lvlText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI catchText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button buildBtn;
    [SerializeField] private Button upgradeBtn;
    [SerializeField] private Button closeBtn;

    private BoatInteractable _current;

    void Awake()
    {
        I = this;
        if (panel) panel.SetActive(false);
        if (closeBtn) closeBtn.onClick.AddListener(Hide);
    }

    public void Show(BoatInteractable boat)
    {
        _current = boat;
        if (panel) panel.SetActive(true);
        Refresh();
        
        buildBtn.gameObject.SetActive(!boat.isBuilt);
        upgradeBtn.gameObject.SetActive(boat.isBuilt);

        buildBtn.onClick.RemoveAllListeners();
        upgradeBtn.onClick.RemoveAllListeners();

        buildBtn.onClick.AddListener(() => { boat.Build(); Refresh(); });
        upgradeBtn.onClick.AddListener(() => { boat.Upgrade(); Refresh(); });
    }

    public void RefreshIfShown(BoatInteractable boat)
    {
        if (panel && panel.activeSelf && _current == boat) Refresh();
    }

    private void Refresh()
    {
        if (_current == null) return;

        var d = _current.data;

        if (_current.isBuilt)
        {
            int lvl = _current.level;
            title.text = "Boat";
            lvlText.text   = $"Lvl: {lvl}";
            speedText.text = $"Speed: {d.GetCycleSeconds(lvl):0.0}s";
            catchText.text = $"Catch: {d.GetCatchAmount(lvl)}";

            if (lvl < d.maxLevel)
                costText.text = $"Cost: {d.GetUpgradeCost(lvl, _current.boatIndexMultiplier)}.";
            else
                costText.text = "Max Lvl";
        }
        else
        {
            title.text   = "Broken boat";
            lvlText.text   = " ";
            speedText.text = "Speed: ?";
            catchText.text = "Catch: ?";
            costText.text  = $"Cost: {d.GetBuildCost(_current.boatIndexMultiplier)}.";
        }
    }

    public void Hide()
    {
        if (panel) panel.SetActive(false);
        _current = null;
    }
}
