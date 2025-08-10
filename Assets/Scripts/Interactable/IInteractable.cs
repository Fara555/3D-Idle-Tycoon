using UnityEngine;

public interface IInteractable
{
	Vector3 GetInteractPoint();
	float GetInteractRadius();
	void OnLeftClick();   // подойти и начать действие
	void OnRightClick();  // открыть UI
	void OnHover(bool state); // показать/скрыть стрелку
}