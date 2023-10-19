using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class AtomCharacter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private int _order;
    [SerializeField] private TMP_Text _textProtein;
    [SerializeField] private GameObject _backgroundImage;

    public void OnPointerEnter(PointerEventData eventData)
    {
        ToggleBackground(true);
        HighlightedMeshItem.SetHighlighted(_order, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToggleBackground(false);
        HighlightedMeshItem.SetHighlighted(_order, false);
    }

    public void SetInfo(int order, string value)
    {
        _order = order;
        _textProtein.text = value;
    }

    public void ToggleBackground(bool value)
    {
        _backgroundImage.SetActive(value);
    }
}
