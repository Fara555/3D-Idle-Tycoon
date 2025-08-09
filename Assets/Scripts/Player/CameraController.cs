using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
	[Header("Movement")] public float dragSpeed = 1.5f;
	[SerializeField] private bool invertX, invertY;

	[Header("Y lock")] public float fixedY = 30f;
	[SerializeField] private bool lockYOnStart = true;

	[Header("Bounds")] public bool useBounds = false;
	[SerializeField] private Vector2 minXZ = new Vector2(-200f, -200f);
	[SerializeField] private Vector2 maxXZ = new Vector2(200f, 200f);

	private Vector3 _prevMouse;
	private Camera _cam;
	private bool _dragging;

	void Awake()
	{
		_cam = Camera.main;
		if (lockYOnStart) fixedY = transform.position.y;
	}

	void Update()
	{
		Vector3 mouse = Input.mousePosition;
		bool overUI = EventSystem.current && EventSystem.current.IsPointerOverGameObject();

		if (Input.GetMouseButtonDown(0))
		{
			_dragging = !overUI;
			_prevMouse = mouse;
		}

		if (Input.GetMouseButtonUp(0))
			_dragging = false;

		if (!_dragging)
		{
			_prevMouse = mouse;
			return;
		}

		Vector2 delta = (Vector2)(mouse - _prevMouse);

		if (delta.sqrMagnitude > 90000f) delta = Vector2.zero; // 300px за кадр

		if (invertX) delta.x = -delta.x;
		if (invertY) delta.y = -delta.y;

		Vector3 right = _cam.transform.right; right.y = 0; right.Normalize();
		Vector3 fwd   = _cam.transform.forward; fwd.y = 0; fwd.Normalize();

		Vector3 move = (right * -delta.x + fwd * -delta.y) * (dragSpeed * Time.deltaTime);
		Vector3 pos = transform.position + move;
		pos.y = fixedY;

		if (useBounds)
		{
			pos.x = Mathf.Clamp(pos.x, minXZ.x, maxXZ.x);
			pos.z = Mathf.Clamp(pos.z, minXZ.y, maxXZ.y);
		}

		transform.position = pos;
		_prevMouse = mouse;
	}

}