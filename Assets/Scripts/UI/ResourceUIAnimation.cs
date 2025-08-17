using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.UTask;

public class ResourceUIAnimation : MonoBehaviour
{
	[Header("Timing")]
    [SerializeField] private float fadeInTime = 0.12f;
    [SerializeField] private float holdTime   = 0.4f;
    [SerializeField] private float fadeOutTime= 0.28f;

    [Header("Motion")]
    [SerializeField] private float rise = 0.6f;      
    [SerializeField] private float popScale = 1.08f;  

    private CanvasGroup _cg;
    private Vector3 _startLocalPos;
    private CancellationTokenSource _cts;

    void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        if (!_cg) _cg = gameObject.AddComponent<CanvasGroup>();
        _startLocalPos = transform.localPosition;
    }

    void OnEnable()
    {
        Restart();
    }

    void OnDisable()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        if (_cg) _cg.alpha = 0f;
        transform.localPosition = _startLocalPos;
        transform.localScale = Vector3.one;
    }

    public void Restart()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        _ = PlayAsync(_cts.Token);
    }

    private async Task PlayAsync(CancellationToken ct)
    {
        if (!_cg) _cg = gameObject.AddComponent<CanvasGroup>();

        _cg.alpha = 0f;
        transform.localPosition = _startLocalPos;
        transform.localScale = Vector3.one;

        SoundManager.Instance.PlaySound("GainResource");
        
        float t = 0f;
        float totalRiseTime = fadeInTime + holdTime + fadeOutTime;
        while (t < fadeInTime && !ct.IsCancellationRequested && enabled && gameObject.activeInHierarchy)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / fadeInTime);
            float easeIn = u * u * (3f - 2f * u); 
            _cg.alpha = easeIn;
            transform.localScale = Vector3.one * Mathf.Lerp(1f, popScale, u);
            transform.localPosition = Vector3.LerpUnclamped(_startLocalPos, _startLocalPos + Vector3.up * rise, (t / totalRiseTime));
            await UTaskEx.NextFrame(ct);
        }
        
        t = 0f;
        while (t < holdTime && !ct.IsCancellationRequested && enabled && gameObject.activeInHierarchy)
        {
            t += Time.deltaTime;
            float progressed = (fadeInTime + t) / totalRiseTime;
            transform.localPosition = Vector3.LerpUnclamped(_startLocalPos, _startLocalPos + Vector3.up * rise, progressed);
            await UTaskEx.NextFrame(ct);
        }
        
        t = 0f;
        while (t < fadeOutTime && !ct.IsCancellationRequested && enabled && gameObject.activeInHierarchy)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / fadeOutTime);
            _cg.alpha = 1f - u;
            transform.localScale = Vector3.one * Mathf.Lerp(popScale, 1f, u);
            float progressed = (fadeInTime + holdTime + t) / totalRiseTime;
            transform.localPosition = Vector3.LerpUnclamped(_startLocalPos, _startLocalPos + Vector3.up * rise, progressed);
            await UTaskEx.NextFrame(ct);
        }

        if (!ct.IsCancellationRequested && gameObject.activeSelf)
            gameObject.SetActive(false);
    }
}