using UnityEngine;

public class VillagerAssignmentManager : MonoBehaviour
{
	public static VillagerAssignmentManager Instance { get; private set; }

	private Villager selectedVillager;
	public bool HasSelectedVillager => selectedVillager != null;


	private void Awake()
	{
		if (Instance != null && Instance != this) Destroy(gameObject);
		else Instance = this;
	}

	public void StartAssignment(Villager v)
	{
		selectedVillager = v;
	}

	public void TryAssignTo(IWorkplace workplace)
	{
		if (selectedVillager == null) return;

		if (workplace is BuildingLogic logic && !logic.IsBuilt)
		{
			Debug.Log("Нельзя назначить на недостроенное здание!");
			CancelAssignment();
			return;
		}

		if (workplace.IsOccupied && workplace.CurrentOccupant != selectedVillager)
		{
			Debug.Log("Рабочее место уже занято!");
			CancelAssignment();
			return;
		}
		
		if (selectedVillager.CurrentWorkplace != null && selectedVillager.CurrentWorkplace != workplace)
		{
			selectedVillager.CurrentWorkplace.SetOccupant(null);
			selectedVillager.Unassign();
		}

		selectedVillager.AssignWorkplace(workplace);
		workplace.SetOccupant(selectedVillager);

		selectedVillager = null;
	}
	
	public void CancelAssignment()
	{
		selectedVillager = null;
	}
}