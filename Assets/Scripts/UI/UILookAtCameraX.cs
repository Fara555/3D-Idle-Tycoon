using System;
using UnityEngine;

[DisallowMultipleComponent]
public class UILookAtCameraX : MonoBehaviour
{
	private Camera cam;
	private Transform _parent;
	private float _fixedLocalY;
	private float _fixedLocalZ;

	[Header("Flip")]
	public bool mirrorY = false;

	private void Start()
	{
		if (!cam) cam = Camera.main;
		_parent = transform.parent;

		Vector3 e = transform.localEulerAngles;
		_fixedLocalY = e.y;
		_fixedLocalZ = e.z;
	}

	private void LateUpdate()
	{
		LookAtCamera();
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

		Vector3 angles = new Vector3(xAngle, _fixedLocalY, _fixedLocalZ);

		if (mirrorY)
		{
			angles.x += 180f;
		}

		transform.localEulerAngles = angles;
	}
}