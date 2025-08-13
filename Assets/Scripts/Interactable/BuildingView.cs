using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.UTask;

public class BuildingView : MonoBehaviour
{
    [Header("World UI")]
    [SerializeField] private GameObject arrow;
    [SerializeField] private GameObject progressBar;
    [SerializeField] private Image progressFill;
    [SerializeField] private GameObject gainRoot; // объект с WorldUIAutoAnimator + иконкой + текстом
    [SerializeField] private TMP_Text gainText;
    [SerializeField] private int gainMaxDigits = 3; // для форматтера
    [SerializeField] private string gainPrefix = "+"; // "+"

    [Header("Model")]
    [SerializeField] private GameObject model;
    [SerializeField] private GameObject lockedIcon;

    [Header("Building Animation")]
    [SerializeField] private float appearYOffset = 2f;
    [SerializeField] private float appearDuration = 1f;

    private BuildingLogic _logic;
    private ResourceUIAnimation _resourceAnimation;
    
    private bool _isWorking;
    private bool _isHovered;
    private bool _wasBuilt;    
    private bool _animating;       
    
    private CancellationTokenSource _gainCts;
    
    public virtual void Bind(BuildingLogic logic)
    {
        if (_logic != null) Unbind();

        _logic = logic;
        _logic.OnStartWork += () => { HandleStart(); SetHover(false); };
        _logic.OnStopWork += HandleStop;
        _logic.OnProgressUpdate += HandleProgress;
        _logic.OnStateChanged += ApplyState;
        _logic.OnYield += HandleYield;

        _wasBuilt = _logic.IsBuilt;
        
        if (gainRoot) gainRoot.SetActive(false);

        ApplyState();
    }

    public virtual void Unbind()
    {
        if (_logic == null) return;
        _logic.OnStartWork -= HandleStart;
        _logic.OnStopWork -= HandleStop;
        _logic.OnProgressUpdate -= HandleProgress;
        _logic.OnStateChanged -= ApplyState;
        _logic.OnYield -= HandleYield;
        
        _logic = null;

        _gainCts?.Cancel();
        _gainCts?.Dispose();
        _gainCts = null;
    }

    private void OnDisable() => Unbind();

    public void SetHover(bool state)
    {
        _isHovered = state;
        UpdateArrow();
    }

    private void UpdateArrow()
    {
        bool canShow = _isHovered && _logic != null && _logic.IsBuilt && !_isWorking;
        if (arrow) arrow.SetActive(canShow);
    }

    private void HandleStart()
    {
        _isWorking = true;
        if (progressBar) progressBar.SetActive(true);
        UpdateArrow();
    }

    private void HandleStop()
    {
        _isWorking = false;
        if (progressBar) progressBar.SetActive(false);
        if (progressFill) progressFill.fillAmount = 0f;
        UpdateArrow();
    }

    private void HandleProgress(float v)
    {
        if (progressFill) progressFill.fillAmount = Mathf.Clamp01(v);
    }

    private void ApplyState()
    {
        if (_logic == null) return;

        bool built = _logic.IsBuilt;

        if (model)      model.SetActive(built);
        if (lockedIcon) lockedIcon.SetActive(!built);

        if (built && !_wasBuilt && model && model.activeInHierarchy && !_animating)
        {
            _ = AnimateBuildingAppear(model.transform, appearDuration, appearYOffset);
        }

        _wasBuilt = built;
        UpdateArrow();
    }

    private async Task AnimateBuildingAppear(Transform target, float duration, float startYOffset)
    {
        if (target == null) return;

        _animating = true;

        Vector3 endPos = target.localPosition;
        Vector3 startPos = endPos + Vector3.down * startYOffset;
        target.localPosition = startPos;

        float t = 0f;
        while (t < duration && enabled && gameObject.activeInHierarchy)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            float eased = 1f - Mathf.Pow(1f - u, 3f); 
            target.localPosition = Vector3.Lerp(startPos, endPos, eased);
            await UTaskEx.NextFrame();
        }

        target.localPosition = endPos;
        _animating = false;
    }
    
    private void HandleYield(int amount)
    {
        if (!gainRoot || amount <= 0) return;

        if (gainText)
            gainText.text = gainPrefix + BigNumberFormatter.Format(amount, gainMaxDigits);

        if (!gainRoot.activeSelf)
        {
            gainRoot.SetActive(true); // OnEnable запустит анимацию
        }
        else
        {
            if (_resourceAnimation == null) gainRoot.GetComponent<ResourceUIAnimation>();
            _resourceAnimation.Restart();
        }
    }
}
