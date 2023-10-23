using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PanelDetailsInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text _textDetailsInfo;

    private void OnEnable()
    {
        AtomDetector.OnChangedNearestItem += SetText;
    }

    private void OnDisable()
    {
        AtomDetector.OnChangedNearestItem -= SetText;
    }

    public void SetText(string text)
    {
        _textDetailsInfo.text = text;
    }
}
