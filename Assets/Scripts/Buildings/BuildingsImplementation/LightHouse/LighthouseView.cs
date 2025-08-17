using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class LighthouseView : BuildingView
{
    [SerializeField] private GameObject buyerBoat;
    [SerializeField] private Transform boatStart;
    [SerializeField] private Transform boatEnd;
    [SerializeField] private float moveDuration = 1.5f;

    private BuildingLogic logic;
    private CancellationTokenSource boatCts;

    public override void Bind(BuildingLogic logic)
    {
        base.Bind(logic);
        
        this.logic = logic;
        if (logic == null) return;

        logic.OnStopWork += OnStop;
        logic.OnWorkStartedWithSource += OnWorkStarted;        

        if (buyerBoat) buyerBoat.SetActive(false);
    }

    public override void Unbind()
    {
        base.Unbind();
        
        if (logic == null) return;

        logic.OnStopWork -= OnStop;
        logic.OnWorkStartedWithSource -= OnWorkStarted;

        boatCts?.Cancel();
        boatCts?.Dispose();
        boatCts = null;
    }
    
    private void OnWorkStarted(bool isVillager)
    {
        if (isVillager) return;

        boatCts?.Cancel();
        boatCts = new CancellationTokenSource();
        _ = MoveBoatInAsync(boatCts.Token);
    }

    private void OnStop()
    {
        boatCts?.Cancel();
        boatCts = new CancellationTokenSource();
        _ = MoveBoatOutAsync(boatCts.Token);
    }

    private async Task MoveBoatInAsync(CancellationToken ct)
    {
        if (!buyerBoat || boatStart == null || boatEnd == null) return;

        buyerBoat.SetActive(true);
        float t = 0f;
        while (t < moveDuration && !ct.IsCancellationRequested)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / moveDuration);
            buyerBoat.transform.position = Vector3.Lerp(boatStart.position, boatEnd.position, p);
            await Utilities.UTask.UTaskEx.NextFrame(ct);
        }
    }

    private async Task MoveBoatOutAsync(CancellationToken ct)
    {
        if (!buyerBoat || boatStart == null || boatEnd == null) return;

        float t = 0f;
        while (t < moveDuration && !ct.IsCancellationRequested)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / moveDuration);
            buyerBoat.transform.position = Vector3.Lerp(boatEnd.position, boatStart.position, p);
            await Utilities.UTask.UTaskEx.NextFrame(ct);
        }

        buyerBoat.SetActive(false);
    }
}
