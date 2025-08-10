using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Utilities.UTask;

[RequireComponent(typeof(Collider))]
public class BoatInteractable : MonoBehaviour, IInteractable
{
    [Header("Setup")]
    public BoatData data;
    public int boatIndexMultiplier = 1;

    [Header("State (runtime)")]
    public bool isBuilt = false;
    [Range(1, 30)] public int level = 1;

    [Header("Interact")]
    [SerializeField] private Transform interactPoint;
    [SerializeField] private float interactRadius = 1.2f;

    [Header("Visual")]
    [SerializeField] private GameObject hoverArrow;
    [SerializeField] private GameObject lockedIcon;
    [SerializeField] private GameObject boatModel;

    private bool _fishingActive;
    private Coroutine _fishRoutine;
    private Coroutine _waitRoutine;

    private PlayerMover _player;
    private NavMeshAgent _agent;

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
    }

    private void HandlePlayerMoveCommand(Vector3 target)
    {
        if (Vector3.Distance(target, GetInteractPoint()) > interactRadius + 0.05f)
            StopFishing();
    }

    public void OnLeftClick()
    {
        if (_player == null || _agent == null) return;

        // Pick a reachable point on the NavMesh near interactPoint
        Vector3 raw = GetInteractPoint();
        Vector3 navTarget = raw;
        if (NavMesh.SamplePosition(raw, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            navTarget = hit.position;

        // Move player
        _player.MoveTo(navTarget);

        // (Re)start wait routine toward that specific point
        if (_waitRoutine != null) StopCoroutine(_waitRoutine);
        _waitRoutine = StartCoroutine(Co_WaitAndStartFishing(navTarget));
    }

    public void OnRightClick()
    {
        BoatUI.I?.Show(this);
    }

    public void OnHover(bool state)
    {
        if (hoverArrow) hoverArrow.SetActive(state);
    }

    private IEnumerator Co_WaitAndStartFishing(Vector3 navTarget)
    {
        if (_agent == null) yield break;

        float arriveThreshold = Mathf.Max(_agent.stoppingDistance, interactRadius);

        // Wait until the path is computed
        while (_agent.pathPending) yield return null;

        // Wait until agent is close enough (robust & simple)
        while (_agent.remainingDistance > arriveThreshold)
        {
            yield return null;
        }

        // Extra safety: ensure we're really near the intended spot
        if (Vector3.Distance(_player.transform.position, navTarget) > arriveThreshold + 0.15f)
            yield break;

        Debug.Log($"[BoatInteractable] Reached boat. Start point: {navTarget}, dist: {Vector3.Distance(_player.transform.position, navTarget):0.###}");

        if (isBuilt)
        {
            if (_fishRoutine != null) StopCoroutine(_fishRoutine);
            _fishRoutine = StartCoroutine(Co_FishingLoop());
            Debug.Log($"[BoatInteractable] Started fishing on '{name}' (Level {level}).");
        }

        _waitRoutine = null;
    }

    private IEnumerator Co_FishingLoop()
    {
        _fishingActive = true;

        float cycle = data.GetCycleSeconds(level);
        float timer = 0f;

        while (true)
        {
            if (Vector3.Distance(_player.transform.position, GetInteractPoint()) > interactRadius + 0.05f)
                break;

            timer += Time.deltaTime;
            if (timer >= cycle)
            {
                timer = 0f;
                int amount = data.GetCatchAmount(level);
                CurrencyManager.Instance.AddFish(amount);
                // TODO: popup/FX
            }

            yield return null;
        }

        _fishingActive = false;
        _fishRoutine = null;
    }

    public void StopFishing()
    {
        if (_fishRoutine != null) StopCoroutine(_fishRoutine);
        _fishRoutine = null;
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
        Gizmos.color = new Color(1f, 0f, 0f, 0.9f);
        Gizmos.DrawWireSphere(GetInteractPoint(), interactRadius);
    }
#endif
}
