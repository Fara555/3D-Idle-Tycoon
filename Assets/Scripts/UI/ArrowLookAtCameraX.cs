using System;
using UnityEngine;

[DisallowMultipleComponent]
public class ArrowLookAtCameraX : MonoBehaviour
{
	[Header("Bob Animation")]
	[SerializeField] private float bobAmplitude = 0.15f;
	[SerializeField] private float bobSpeed = 2.0f;

	private Camera cam;
	private Transform _parent;
	private float _fixedLocalY;
	private float _fixedLocalZ;
	private Vector3 _startLocalPos;

	private void Start()
	{
		if (!cam) cam = Camera.main;
		_parent = transform.parent;

		Vector3 e = transform.localEulerAngles;
		_fixedLocalY = e.y;
		_fixedLocalZ = e.z;

		_startLocalPos = transform.localPosition;
	}

	void LateUpdate()
	{
		LookAtCamera();
		ApplyBob();
	}

	private void LookAtCamera()
	{
		if (!cam || !_parent) return;

		Vector3 camLocal = _parent.InverseTransformPoint(cam.transform.position);
		Vector3 posLocal = transform.localPosition;
		Vector3 dirLocal = camLocal - posLocal;

		Quaternion yzLock = Quaternion.Euler(0f, _fixedLocalY, _fixedLocalZ);
		Vector3 dirInYz = Quaternion.Inverse(yzLock) * dirLocal;

		float xAngle = Mathf.Atan2(dirInYz.y, dirInYz.z) * Mathf.Rad2Deg;

		transform.localEulerAngles = new Vector3(xAngle, _fixedLocalY, _fixedLocalZ);
	}

	private void ApplyBob()
	{
		float offset = Mathf.Sin(Time.time * Mathf.PI * 2f * bobSpeed) * bobAmplitude;

		Vector3 p = _startLocalPos;
		p.z += offset;
		transform.localPosition = p;
	}
}