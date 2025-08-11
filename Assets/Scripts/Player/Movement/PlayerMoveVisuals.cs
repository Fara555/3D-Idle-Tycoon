using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerMoveVisuals : MonoBehaviour
{
	[SerializeField] private Animator animator;
	[SerializeField] private string isOnWaterParam = "IsOnWater"; 

	void Update()
	{
		bool onWater = IsOnWater();
		animator.SetBool(isOnWaterParam, onWater);
	}

	bool IsOnWater()
	{
		if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1f, NavMesh.AllAreas))
		{
			return hit.mask.ToString() == "Water"; 
		}
		return false;
	}
}