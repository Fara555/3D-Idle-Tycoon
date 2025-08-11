using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using Utilities.UTask;

[RequireComponent(typeof(Collider))]
public class BoatInteractable : MonoBehaviour, IInteractable
{
    [Header("Setup")] public BoatData data;
    public int boatIndexMultiplier = 1;

    [Header("State (runtime)")] public bool isBuilt = false;
    [Range(1, 30)] public int level = 1;

    [Header("Interact")]
    [SerializeField] private Transform interactPoint;
    [SerializeField] private float interactRadius = 1.2f;

    [Header("Visual")]
    [SerializeField] private GameObject hoverArrow;
    [SerializeField] private GameObject lockedIcon;
    [SerializeField] private GameObject boatModel;

    [Header("Timing")] [SerializeField] private bool debugFishingTiming = false;

    private PlayerMover _player;
    private NavMeshAgent _agent;

    private CancellationTokenSource _arriveCts;
    private CancellationTokenSource _fishCts;

    private bool _fishingActive;

    public Vector3 GetInteractPoint() => interactPoint ? interactPoint.position : transform.position;
    public float GetInteractRadius() => interactRadius;

    private void Awake()
    {
        UTaskRunner.Ensure();

        _player = FindObjectOfType<PlayerMover>();
        _agent  = _player ? _player.GetComponent<NavMeshAgent>() : null;

        if (_player) _player.OnMoveCommand += HandlePlayerMoveCommand;

        ApplyVisuals();
        if (hoverArrow) hoverArrow.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_player) _player.OnMoveCommand -= HandlePlayerMoveCommand;
        _arriveCts?.Cancel(); _arriveCts?.Dispose();
        _fishCts?.Cancel();   _fishCts?.Dispose();
    }

    private void HandlePlayerMoveCommand(Vector3 target)
    {
        if (Vector3.Distance(target, GetInteractPoint()) > interactRadius + 0.05f)
            StopFishing();
    }

    public void OnLeftClick()
    {
        if (_player == null || _agent == null) return;

        _arriveCts?.Cancel();
        _arriveCts?.Dispose();
        _arriveCts = new CancellationTokenSource();

        Vector3 raw = GetInteractPoint();
        Vector3 navTarget = raw;
        if (NavMesh.SamplePosition(raw, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            navTarget = hit.position;

        _player.MoveTo(navTarget);
        _ = WaitAndStartFishingAsync(navTarget, _arriveCts.Token);
    }

    public void OnRightClick() => BoatUI.I?.Show(this);

    public void OnHover(bool state)
    {
        if (hoverArrow) hoverArrow.SetActive(state);
    }

    private async Task WaitAndStartFishingAsync(Vector3 navTarget, CancellationToken ct)
    {
        float arriveThreshold = Mathf.Max(_agent.stoppingDistance, interactRadius);

        await UTaskEx.WaitUntil(() => !_agent.pathPending, ct);

        while (!ct.IsCancellationRequested && _agent.remainingDistance > arriveThreshold)
            await UTaskEx.NextFrame(ct);
        if (ct.IsCancellationRequested) return;

        if (Vector3.Distance(_player.transform.position, navTarget) > arriveThreshold + 0.2f)
            return;
        
        if (isBuilt)
        {
            StartFishingLoop();
        }
        else
        {
            BoatUI.I?.Show(this);
        }
    }

    private void StartFishingLoop()
    {
        _fishCts?.Cancel();
        _fishCts?.Dispose();
        _fishCts = new CancellationTokenSource();

        _ = FishingLoopAsync(_fishCts.Token);
    }

    private async Task FishingLoopAsync(CancellationToken ct)
    {
        _fishingActive = true;

        float cycle = data.GetCycleSeconds(level);
        float timer = 0f;
        float realElapsed = 0f; 

        try
        {
            while (!ct.IsCancellationRequested)
            {
                if (Vector3.Distance(_player.transform.position, GetInteractPoint()) > interactRadius + 0.05f)
                    break;
                
                timer += Time.deltaTime;
                realElapsed += Time.unscaledDeltaTime;

                if (timer >= cycle)
                {
                    timer -= cycle;

                    int amount = data.GetCatchAmount(level);
                    CurrencyManager.Instance.AddFish(amount);

                    if (debugFishingTiming)
                    {
                        Debug.Log($"[BoatInteractable] Cycle done. Target={cycle:0.###}s, " +
                                  $"RealElapsed={realElapsed:0.###}s, timeScale={Time.timeScale:0.###}");
                        realElapsed = 0f;
                    }
                }

                await UTaskEx.NextFrame(ct);
            }
        }
        catch (TaskCanceledException) {  }
        finally
        {
            _fishingActive = false;
        }
    }

    public void StopFishing()
    {
        _fishCts?.Cancel();
        _fishingActive = false;
    }

    public void Build()
    {
        if (isBuilt) return;
        int cost = data.GetBuildCost(boatIndexMultiplier);
        if (!CurrencyManager.Instance.TrySpendGold(cost)) return;

        isBuilt = true;
        level = Mathf.Clamp(level, 1, data.maxLevel);
        ApplyVisuals();
        BoatUI.I?.RefreshIfShown(this);
    }

    public void Upgrade()
    {
        if (!isBuilt) return;
        if (level >= data.maxLevel) return;

        int cost = data.GetUpgradeCost(level, boatIndexMultiplier);
        if (!CurrencyManager.Instance.TrySpendGold(cost)) return;

        level++;
        BoatUI.I?.RefreshIfShown(this);
    }

    private void ApplyVisuals()
    {
        if (lockedIcon) lockedIcon.SetActive(!isBuilt);
        if (boatModel)  boatModel.SetActive(isBuilt);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.DrawSphere(GetInteractPoint(), interactRadius);
        Gizmos.color = new Color(1f, 0f, 0.9f, 0.9f);
        Gizmos.DrawWireSphere(GetInteractPoint(), interactRadius);
    }
#endif
}
