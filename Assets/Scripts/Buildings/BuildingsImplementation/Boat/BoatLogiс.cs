using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.UTask;

public class BoatLogic : BuildingLogic, IWorkplace
{
    [SerializeField] private BoatData data;
    [SerializeField] private Transform workPoint;

    private Villager currentWorker;

    public override int MaxLevel => data.maxLevel;
    public override string BuildingName => data.displayName;
    public override BuildingData BuildingData => data;
    public bool IsOccupied => currentWorker != null;
    public Villager CurrentOccupant => currentWorker;
    
    public void SetOccupant(Villager villager)
    {
        currentWorker = villager;
    }

    public Vector3 GetWorkPoint() => workPoint ? workPoint.position : transform.position;

    public UTask PerformWork(Villager villager, CancellationToken ct)
    {
        return new UTask(PerformVillagerWorkAsync(villager, ct));
    }

    private async Task PerformVillagerWorkAsync(Villager villager, CancellationToken ct)
    {
        currentWorker = villager;
        float timer = 0f;
        float duration = Mathf.Max(0.01f, GetCycleTime());

        while (!ct.IsCancellationRequested && timer < duration)
        {
            timer += Time.deltaTime;
            await UTaskEx.NextFrame(ct);
        }

        if (!ct.IsCancellationRequested)
        {
            int amount = GetCatchAmount();
            if (currentWorker != null)
                currentWorker.CollectFish(amount);
            else
                CurrencyManager.Instance.AddFish(amount);
            onYield?.Invoke(amount);
        }
        currentWorker = null;
    }

    public override float GetCycleTime()
    {
        float baseTime = data.GetCycleSeconds(Level);
        float reductionFactor = 1f - 0.05f * (IndexMultiplier - 1);
        return Mathf.Max(0.5f, baseTime * reductionFactor);
    }

    public override int GetCatchAmount()
    {
        int baseAmount = data.GetCatchAmount(Level);
        return baseAmount * IndexMultiplier;
    }

    public override long GetBuildCost() => data.GetBuildCost(IndexMultiplier);
    public override long GetUpgradeCost() => data.GetUpgradeCost(Level, IndexMultiplier);
    public override string GetCycleLabel() => "Cycle";
    public override string GetAmountLabel() => "Catch";
    public override string GetWorkAnimation() => "Fishing Cast";
}