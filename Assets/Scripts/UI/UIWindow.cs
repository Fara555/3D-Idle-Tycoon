using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.UTask;

[DisallowMultipleComponent]
public class UIWindow : MonoBehaviour
{
    [Header("Panel Animation")]
    [SerializeField] private float panelFadeDuration = 0.25f;
    [SerializeField] private float panelScaleDuration = 0.25f;
    [SerializeField] private Vector3 hiddenScale = new Vector3(0.85f, 0.85f, 1f);

    [Header("Background (dark overlay)")]
    [SerializeField] private GameObject darkBG;         
    [SerializeField] private float bgTargetAlpha = 0.6f;
    [SerializeField] private float bgFadeInDuration = 0.18f;
    [SerializeField] private float bgFadeOutDuration = 0.18f;

    [Header("General")]
    [SerializeField] private bool startHidden = true;
    [SerializeField] private bool useUnscaledTime = true;
    
    [Header("Pause Settings")]
    [SerializeField] private bool affectTimeScale = false;

    private CanvasGroup panelCg;
    private RectTransform panelRt;

    private CanvasGroup bgCg; 
    private CancellationTokenSource animCts;

    private void Awake()
    {
        panelCg = GetComponent<CanvasGroup>();
        panelRt = GetComponent<RectTransform>();
        if (!panelCg) panelCg = gameObject.AddComponent<CanvasGroup>();

        if (darkBG)
        {
            bgCg = darkBG.GetComponent<CanvasGroup>();
            if (!bgCg) bgCg = darkBG.AddComponent<CanvasGroup>();
        }

        if (affectTimeScale)
            Time.timeScale = 1f; 
        
        if (startHidden) HideInstant();
        else ShowInstant();
    }

    private void OnDisable()
    {
        CancelAnim();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        if (darkBG) darkBG.SetActive(true);

        if (affectTimeScale)
            Time.timeScale = 0f; 

        CancelAnim();
        animCts = new CancellationTokenSource();
        _ = ShowSequence(animCts.Token);
    }

    public void Hide()
    {
        if (affectTimeScale)
            Time.timeScale = 1f; 

        CancelAnim();
        animCts = new CancellationTokenSource();
        _ = HideSequence(animCts.Token);
    }

    public void Toggle()
    {
        if (gameObject.activeSelf && panelCg.alpha > 0.99f) Hide();
        else Show();
    }

    public void ShowInstant()
    {
        CancelAnim();

        if (darkBG)
        {
            darkBG.SetActive(true);
            EnsureBgInteractable(true);
            bgCg.alpha = bgTargetAlpha;
        }

        gameObject.SetActive(true);
        panelCg.alpha = 1f;
        panelCg.blocksRaycasts = true;
        panelCg.interactable   = true;
        panelRt.localScale     = Vector3.one;
    }

    public void HideInstant()
    {
        CancelAnim();

        if (darkBG)
        {
            bgCg.alpha = 0f;
            EnsureBgInteractable(false);
            darkBG.SetActive(false);
        }

        panelCg.alpha = 0f;
        panelCg.blocksRaycasts = false;
        panelCg.interactable   = false;
        panelRt.localScale     = hiddenScale;
        gameObject.SetActive(false);
    }
    
    private async Task ShowSequence(CancellationToken ct)
    {
        if (darkBG)
        {
            darkBG.SetActive(true);
            EnsureBgInteractable(true);
            await FadeCanvasGroup(bgCg, bgCg.alpha, bgTargetAlpha, bgFadeInDuration, ct);
        }

        panelCg.blocksRaycasts = true;
        panelCg.interactable   = true;
        await AnimatePanel(1f, Vector3.one, ct);
    }

    private async Task HideSequence(CancellationToken ct)
    {
        await AnimatePanel(0f, hiddenScale, ct);

        if (darkBG)
        {
            EnsureBgInteractable(false);
            await FadeCanvasGroup(bgCg, bgCg.alpha, 0f, bgFadeOutDuration, ct);
            darkBG.SetActive(false);
        }

        gameObject.SetActive(false);
    }
    
    private async Task AnimatePanel(float targetAlpha, Vector3 targetScale, CancellationToken ct)
    {
        float startAlpha = panelCg.alpha;
        Vector3 startScale = panelRt.localScale;

        bool showing = targetAlpha > startAlpha;
        if (showing)
        {
            panelCg.blocksRaycasts = true;
            panelCg.interactable   = true;
        }

        float total = Mathf.Max(0.0001f, Mathf.Max(panelFadeDuration, panelScaleDuration));
        float elapsed = 0f;

        try
        {
            while (elapsed < total)
            {
                ct.ThrowIfCancellationRequested();
                float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                elapsed += dt;

                float tA = panelFadeDuration  > 0f ? Mathf.Clamp01(elapsed / panelFadeDuration)  : 1f;
                float tS = panelScaleDuration > 0f ? Mathf.Clamp01(elapsed / panelScaleDuration) : 1f;

                tA = Smooth01(tA);
                tS = Smooth01(tS);

                panelCg.alpha       = Mathf.LerpUnclamped(startAlpha,  targetAlpha,  tA);
                panelRt.localScale  = Vector3.LerpUnclamped(startScale, targetScale, tS);

                await UTaskEx.NextFrame(ct);
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }

        panelCg.alpha      = targetAlpha;
        panelRt.localScale = targetScale;

        bool hidden = targetAlpha <= 0.001f;
        panelCg.blocksRaycasts = !hidden;
        panelCg.interactable   = !hidden;
    }

    private async Task FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration, CancellationToken ct)
    {
        if (!cg) return;
        if (duration <= 0f)
        {
            cg.alpha = to;
            return;
        }

        float t = 0f;
        try
        {
            while (t < 1f)
            {
                ct.ThrowIfCancellationRequested();
                float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                t += dt / duration;
                float e = Smooth01(Mathf.Clamp01(t));
                cg.alpha = Mathf.LerpUnclamped(from, to, e);
                await UTaskEx.NextFrame(ct);
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
        cg.alpha = to;
    }
    
    
    private static float Smooth01(float t) => t * t * (3f - 2f * t);

    private void EnsureBgInteractable(bool on)
    {
        if (!bgCg) return;
        bgCg.blocksRaycasts = on; 
        bgCg.interactable = on;
    }

    private void CancelAnim()
    {
        if (animCts != null)
        {
            if (!animCts.IsCancellationRequested) animCts.Cancel();
            animCts.Dispose();
            animCts = null;
        }
    }
}
