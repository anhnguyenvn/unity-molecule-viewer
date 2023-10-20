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
    private string _proteinName;
    public void OnPointerEnter(PointerEventData eventData)
    {
        ToggleBackground(true);
        ProteinObjectManager.Instance.SetHighlightAcidAminMesh(_proteinName, _order, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToggleBackground(false);
        ProteinObjectManager.Instance.SetHighlightAcidAminMesh(_proteinName, _order, false);
    }

    public void SetInfo(AminoAcidShortInfo info)
    {
        _order = info.Order;
        _proteinName = info.ProteinName;
        _textProtein.text = info.ShortName;
    }

    public void ToggleBackground(bool value)
    {
        _backgroundImage.SetActive(value);
    }
}
