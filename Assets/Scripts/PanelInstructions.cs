using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class PanelInstructions : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private float _hoveredAlpha = 0.75f;
    private float _defaultAlpha = 0.25f;
    private CanvasGroup _panelCanvasGroup;

    private void Start()
    {
        _panelCanvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _panelCanvasGroup.alpha = _hoveredAlpha;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _panelCanvasGroup.alpha = _defaultAlpha;
    }
}
