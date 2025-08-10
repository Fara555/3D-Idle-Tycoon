// CurrencyUI.cs

using System;
using UnityEngine;
using TMPro;

public class CurrencyUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private TMP_Text goldCountText;
	[SerializeField] private TMP_Text fishCountText;

	[Header("Formatting")]
	[Tooltip("Significant digits used when abbreviating numbers (K, M, B...).")]
	[SerializeField, Min(1)] private int maxSignificantDigits = 3;
	[SerializeField] private bool thousandSeparatorsUnderK = true;

	private void OnEnable()
	{
		if (CurrencyManager.Instance != null)
		{
			CurrencyManager.Instance.OnGoldChanged += HandleGoldChanged;
			CurrencyManager.Instance.OnFishChanged += HandleFishChanged;
		}
	}

	private void OnDisable()
	{
		if (CurrencyManager.Instance != null)
		{
			CurrencyManager.Instance.OnGoldChanged -= HandleGoldChanged;
			CurrencyManager.Instance.OnFishChanged -= HandleFishChanged;
		}
	}

	private void Start()
	{
		// Initial sync
		HandleGoldChanged(CurrencyManager.Instance.Gold);
		HandleFishChanged(CurrencyManager.Instance.Fish);
	}

	private void HandleGoldChanged(long value)
	{
		if (!goldCountText) return;
		string formatted = BigNumberFormatter.Format(value, maxSignificantDigits, thousandSeparatorsUnderK);
		goldCountText.text = formatted;
	}

	private void HandleFishChanged(long value)
	{
		if (!fishCountText) return;
		string formatted = BigNumberFormatter.Format(value, maxSignificantDigits, thousandSeparatorsUnderK);
		fishCountText.text = formatted;
	}
}