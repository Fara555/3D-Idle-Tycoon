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
			SoundManager.Instance.PlaySound("AssignIncorrectly");
			CancelAssignment();
			return;
		}

		if (workplace.IsOccupied && workplace.CurrentOccupant != selectedVillager)
		{
			SoundManager.Instance.PlaySound("AssignIncorrectly");
			CancelAssignment();
			return;
		}
		
		if (selectedVillager.CurrentWorkplace != null && selectedVillager.CurrentWorkplace != workplace)
		{
			selectedVillager.CurrentWorkplace.SetOccupant(null);
			selectedVillager.Unassign();
		}

		selectedVillager.AssignWorkplace(workplace);
		SoundManager.Instance.PlaySound("AssignCorrectly");
		workplace.SetOccupant(selectedVillager);

		selectedVillager = null;
	}
	
	public void CancelAssignment()
	{
		selectedVillager = null;
	}
}