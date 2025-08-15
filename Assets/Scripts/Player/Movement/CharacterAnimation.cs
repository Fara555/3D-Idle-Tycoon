using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CharacterAnimation : MonoBehaviour
{
	[Header("Animator")]
	[SerializeField] private Animator animator;
	[SerializeField] private string isOnWaterParam = "IsOnWater";

	[Header("Offsets")]
	[SerializeField] private float groundOffset = 0f;
	[SerializeField] private float waterOffset  = 0.6f; 
	
	[Header("Layers")]
	[SerializeField] private LayerMask waterLayer;
	[SerializeField] private LayerMask layerForCheck;

	private NavMeshAgent agent;
	
	private string idleStateName; 
	private bool _isWorking = false;
	private string _workState = null;

	void Awake()
	{
		agent = GetComponent<NavMeshAgent>();
		waterLayer = LayerMask.NameToLayer("Water");
		layerForCheck = LayerMask.GetMask("Water","Ground");
	}

	void Update()
	{
		bool onWater = IsOnWater();
		if (onWater) idleStateName = "Sitting Idle";
		else idleStateName = "Idle";
		animator.SetBool(isOnWaterParam, onWater);
		agent.baseOffset = onWater ? waterOffset : groundOffset;  
	}

	bool IsOnWater()
	{
		var origin = transform.position + Vector3.up * 1.5f;
		return Physics.Raycast(origin, Vector3.down, out var hit, 5f, layerForCheck) && hit.collider.gameObject.layer == waterLayer;
	}
	
	public void StartWork(string stateName)
	{
		if (string.IsNullOrEmpty(stateName) || animator == null) return;
		_isWorking = true;
		_workState = stateName;
		animator.Play(stateName, 0, 0f); 
	}

	public void StopWork()
	{
		if (animator == null) return;
		_isWorking = false;
		_workState = null;
		if (!string.IsNullOrEmpty(idleStateName))
			animator.Play(idleStateName, 0, 0f);
	}
}
