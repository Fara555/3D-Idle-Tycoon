using UnityEngine;
using UnityEngine.AI;

public class DestinationMarker : MonoBehaviour
{
	[Header("Refs")]
	[SerializeField] private NavMeshAgent agent;
	[SerializeField] private Transform markerVisual; 

	[Header("Pulse")]
	[SerializeField] private float baseScale = 1f;
	[SerializeField] private float pulseAmplitude = 0.2f;
	[SerializeField] private float pulseSpeed = 4f;

	[Header("Visibility")]
	[SerializeField] private float hideWhenRemainingLess = 0.15f;
	[SerializeField] private float yOffset = 0.02f;

	void Awake()
	{
		if (agent == null) agent = GetComponentInParent<NavMeshAgent>();
		if (markerVisual == null) markerVisual = transform;
	}

	void LateUpdate()
	{
		if (agent == null || !agent.enabled || agent.pathPending || !agent.hasPath)
		{
			if (markerVisual.gameObject.activeSelf) markerVisual.gameObject.SetActive(false);
			return;
		}

		if (agent.remainingDistance <= hideWhenRemainingLess)
		{
			if (markerVisual.gameObject.activeSelf) markerVisual.gameObject.SetActive(false);
			return;
		}

		var corners = agent.path.corners;
		if (corners == null || corners.Length == 0)
		{
			if (markerVisual.gameObject.activeSelf) markerVisual.gameObject.SetActive(false);
			return;
		}

		Vector3 dest = corners[corners.Length - 1];
		dest.y += yOffset;
		transform.position = dest;
		
		float s = baseScale + Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
		markerVisual.localScale = Vector3.one * s;

		if (!markerVisual.gameObject.activeSelf) markerVisual.gameObject.SetActive(true);
	}
}