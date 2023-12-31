﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class AtomCharacter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private int _order;
    [SerializeField] private TextMeshProUGUI _textProtein;
    [SerializeField] private GameObject _backgroundImage;
    private string _proteinName;
    public void OnPointerEnter(PointerEventData eventData)
    {
        ToggleBackground(true);
        if ( ProteinObjectManager.Instance != null )
        {
            ProteinObjectManager.Instance.SetHighlightAcidAminMesh(_proteinName, _order, true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToggleBackground(false);
        if ( ProteinObjectManager.Instance != null )
        {
            ProteinObjectManager.Instance.SetHighlightAcidAminMesh(_proteinName, _order, false);
        }
    }

    public void SetInfo(AminoAcidShortInfo info)
    {
        _order = info.Order;
        _proteinName = info.ProteinName;
        _textProtein.text = info.ShortName;
        ToggleBackground(false);
    }

    public void ToggleBackground(bool value)
    {
        _backgroundImage.SetActive(value);
    }
}
