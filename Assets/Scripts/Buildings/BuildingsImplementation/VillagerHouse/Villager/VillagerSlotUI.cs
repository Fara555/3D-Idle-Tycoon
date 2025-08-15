using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VillagerSlotUI : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI nameText;
	[SerializeField] private Button assignBtn;

	private Villager _villager;

	public void Setup(Villager villager)
	{
		_villager = villager;
		nameText.text = "Villager";

		assignBtn.onClick.RemoveAllListeners();
		assignBtn.onClick.AddListener(() => {
			VillagerAssignmentManager.Instance.StartAssignment(_villager);
			transform.GetComponentInParent<VillagerHouseWindowUI>()?.Hide();
		});
	}
}