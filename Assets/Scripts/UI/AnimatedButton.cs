using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AnimatedButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	[Header("Scale Settings")]
	[SerializeField] private float normalScale = 1f;
	[SerializeField] private float hoverScale = 1.1f;
	[SerializeField] private float clickScale = 0.9f;
	[SerializeField] private float scaleSpeed = 10f;

	private Vector3 targetScale;
	private RectTransform rectTransform;
	private Button button;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		button = GetComponent<Button>();
		targetScale = Vector3.one * normalScale;
	}

	private void Update()
	{
		rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.unscaledDeltaTime * scaleSpeed);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (button.interactable)
			targetScale = Vector3.one * hoverScale;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		targetScale = Vector3.one * normalScale;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		targetScale = Vector3.one * clickScale;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		targetScale = Vector3.one * hoverScale;
		SoundManager.Instance.PlaySound("UIButton");
	}
}