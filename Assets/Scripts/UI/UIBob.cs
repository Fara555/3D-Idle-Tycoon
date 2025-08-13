using System;
using UnityEngine;

public class UIBob : MonoBehaviour
{
	
	[Header("Bob Animation")]
	[SerializeField] private float bobAmplitude = 0.15f;
	[SerializeField] private float bobSpeed = 2.0f;
	
	private Vector3 _startLocalPos;

	private void Start()
	{
		_startLocalPos = transform.localPosition;
	}

	private void LateUpdate()
	{
		ApplyBob();
	}
	
	private void ApplyBob()
	{
		float offset = Mathf.Sin(Time.time * Mathf.PI * 2f * bobSpeed) * bobAmplitude;

		Vector3 p = _startLocalPos;
		p.z += offset;
		transform.localPosition = p;
	}
}