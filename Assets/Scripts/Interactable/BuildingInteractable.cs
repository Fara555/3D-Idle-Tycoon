using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using Utilities.UTask;

public interface IInteractable
{
    public abstract Vector3 GetInteractPoint();
    public abstract float GetInteractRadius();
	
    public virtual void OnLeftClick() { }
    public virtual void OnRightClick() { }
    public virtual void OnHover(bool state) { }
}

[RequireComponent(typeof(Collider))]
public class BuildingInteractable : MonoBehaviour, IInteractable
{
    [Header("Refs")]
    [SerializeField] private BuildingLogic logic;
    [SerializeField] private BuildingView view;
    [SerializeField] private Transform interactPoint;
    [SerializeField] private float interactRadius = 1.2f;
     
    private PlayerMover player;
    private PlayerAnimation playerAnim;
    
    private NavMeshAgent _agent;
    private IBuildingWindowUI windowUI; 

    private CancellationTokenSource _arriveCts;
    private CancellationTokenSource _workCts;
    
    public Vector3 GetInteractPoint() => interactPoint ? interactPoint.position : transform.position;
    public float GetInteractRadius() => interactRadius;

    void Awake()
    {
        if (player == null) player = FindObjectOfType<PlayerMover>();
        _agent = player ? player.GetComponent<NavMeshAgent>() : null;
        if (playerAnim == null && player) playerAnim = player.GetComponent<PlayerAnimation>();
        
        if (view && logic) view.Bind(logic);
    }

    void OnDestroy()
    {
        CancelArrive();
        StopWork();
        if (view) view.Unbind();
    }
    
    private void Update()
    {
        if (_workCts != null && logic != null && player != null)
        {
            float dist = Vector3.Distance(player.transform.position, GetInteractPoint());
            if (dist > GetInteractRadius() + 0.05f)
            {
                StopWork();
            }
        }
    }
    

    public void OnHover(bool state)
    {
        if (view) view.SetHover(state);
    }

    public void OnRightClick()
    {
        if (windowUI == null) windowUI = FindObjectOfType<BuildingWindowUI>();
        windowUI?.Show(logic);
    }

    public void OnLeftClick()
    {
        if (player == null || _agent == null) return;
        
        if (!logic.IsBuilt)
        {
            OnRightClick();
            return;
        }

        CancelArrive();
        _arriveCts = new CancellationTokenSource();

        Vector3 target = GetNavPointNear(GetInteractPoint(), 2f);
        player.MoveTo(target);
        _ = WaitAndStartWorkAsync(target, _arriveCts.Token);
    }

    private async Task WaitAndStartWorkAsync(Vector3 target, CancellationToken ct)
    {
        float threshold = Mathf.Max(_agent.stoppingDistance, interactRadius);

        await UTaskEx.WaitUntil(() => !_agent.pathPending, ct);
        while (!ct.IsCancellationRequested && _agent.remainingDistance > threshold)
            await UTaskEx.NextFrame(ct);
        if (ct.IsCancellationRequested) return;
        
        StartWork();
    }

    private Vector3 GetNavPointNear(Vector3 raw, float maxDistance)
    {
        return NavMesh.SamplePosition(raw, out var hit, maxDistance, NavMesh.AllAreas)
            ? hit.position
            : raw;
    }

    private void StartWork()
    {
        StopWork();
        _workCts = new CancellationTokenSource();
        
        string workState = logic.GetWorkAnimation();
        if (!string.IsNullOrEmpty(workState))
            playerAnim?.StartWork(workState);

        if (_agent) _agent.isStopped = true;

        logic.StartWork(_workCts.Token);
    }

    public void StopWork()
    {
        _workCts?.Cancel();
        _workCts?.Dispose();
        _workCts = null;
        
        if (_agent) _agent.isStopped = false;
        playerAnim?.StopWork();
    }

    private void CancelArrive()
    {
        _arriveCts?.Cancel();
        _arriveCts?.Dispose();
        _arriveCts = null;
    }
}
