using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using Utilities.UTask;

public interface IWorkplace
{
	Vector3 GetWorkPoint();
	UTask PerformWork(Villager villager, CancellationToken ct);
	
	bool IsOccupied { get; }
	void SetOccupant(Villager villager);

	Villager CurrentOccupant { get; }
}

public class Villager : MonoBehaviour
{
	private VillagerHouseLogic owner;
	private IWorkplace assignedWorkplace;
	
	public IWorkplace CurrentWorkplace => assignedWorkplace;


	public float moveSpeed = 3f;
	public int carryCapacity = 30;
	private int carriedFish = 0;
	
	public int CarriedFish => carriedFish;

	private CancellationTokenSource workCts;

	public void Initialize(VillagerHouseLogic house)
	{
		owner = house;
	}

	public void AssignWorkplace(IWorkplace workplace)
	{
		assignedWorkplace = workplace;

		workCts?.Cancel();
		workCts?.Dispose();
		workCts = new CancellationTokenSource();

		_ = RunWorkLoop(workCts.Token); // запускаем без await
	}

	public void Unassign()
	{
		workCts?.Cancel();
		workCts?.Dispose();
		workCts = null;

		assignedWorkplace = null;
	}

	private UTask RunWorkLoop(CancellationToken ct)
	{
		return new UTask(RunWorkLoopAsync(ct));
	}

	private async Task RunWorkLoopAsync(CancellationToken ct)
	{
		while (!ct.IsCancellationRequested && assignedWorkplace != null)
		{
			// 🐟 Собираем рыбу
			while (carriedFish < carryCapacity && !ct.IsCancellationRequested)
			{
				await MoveTo(assignedWorkplace.GetWorkPoint(), ct);
				await assignedWorkplace.PerformWork(this, ct);
			}

			// 🚚 Доставка рыбы
			var sellTarget = await LighthouseLocator.GetSellPointAsync();
			await MoveTo(sellTarget, ct);

			// 💰 Продажа через Lighthouse
			var lighthouse = LighthouseLocator.Find();
			if (lighthouse != null)
			{
				await lighthouse.StartWorkAsync(ct, true, this); // ✅ независимая продажа
			}

			carriedFish = 0;
		}
	}



	private UTask MoveTo(Vector3 target, CancellationToken ct)
	{
		return new UTask(MoveToAsync(target, ct));
	}

	private async Task MoveToAsync(Vector3 target, CancellationToken ct)
	{
		var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
		if (agent == null) return;

		agent.SetDestination(target);
		agent.isStopped = false;

		while (!ct.IsCancellationRequested && agent.pathPending)
			await UTaskEx.NextFrame(ct);

		while (!ct.IsCancellationRequested && agent.remainingDistance > agent.stoppingDistance + 0.05f)
			await UTaskEx.NextFrame(ct);

		agent.isStopped = true;
	}
	
	public void ApplyStats(float moveSpeed, int capacity)
	{
		this.moveSpeed = moveSpeed;
		this.carryCapacity = capacity;

		var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
		if (agent != null)
			agent.speed = moveSpeed;
	}

	public void CollectFish(int amount)
	{
		carriedFish = Mathf.Min(carriedFish + amount, carryCapacity);
	}
	
	public void ResetFish()
	{
		carriedFish = 0;
	}

}
