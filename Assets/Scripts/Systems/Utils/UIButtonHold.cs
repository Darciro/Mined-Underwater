using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonHold : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public float holdThreshold = 0.2f; // Time (in seconds) to count as "holding"
    public Action onHoldDown;
    public Action onHoldRelease;

    private bool isHolding = false;
    private float holdTime;

    void Update()
    {
        if (isHolding)
        {
            holdTime += Time.unscaledDeltaTime;
            if (holdTime >= holdThreshold)
            {
                onHoldDown?.Invoke();
                holdTime = 0f; // Repeat hold if needed, or remove this line to call once
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
        holdTime = 0f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
        onHoldRelease?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHolding = false;
        onHoldRelease?.Invoke();
    }
}
