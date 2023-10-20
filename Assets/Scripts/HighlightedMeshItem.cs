using System;
using HighlightPlus;
using UnityEngine;

public class HighlightedMeshItem : MonoBehaviour
{
    public AminoAcidShortInfo _info;

    private HighlightManager _highlightManager;

    public void SetInfo(AminoAcidShortInfo info)
    {
        _info = info;
    }

    private void Awake()
    {
        _highlightManager = HighlightManager.instance;
        if ( _highlightManager == null ) return;
        _highlightManager.OnObjectHighlightStart += HighlightStarted;
        _highlightManager.OnObjectHighlightEnd += HighlightEnded;

        ProteinObjectManager.Instance.OnAminoAcidCharSelected += HighlightMe;
    }

    private void HighlightMe(object sender, AminoAcidCharSelection e)
    {
        if ( e._proteinName == _info.ProteinName && _info.Order == e._aminoAcidOrder )
        {
            if ( e._isSelected )
            {
                HighlightManager.instance.ToggleObject(transform);
            }
            else
            {
                HighlightManager.instance.UnselectObject(transform);
            }
        }
    }

    private void OnDestroy()
    {
        if ( _highlightManager == null ) return;
        _highlightManager.OnObjectHighlightStart -= HighlightStarted;
        _highlightManager.OnObjectHighlightEnd -= HighlightEnded;
        ProteinObjectManager.Instance.OnAminoAcidCharSelected -= HighlightMe;
    }

    private bool HighlightEnded(GameObject unhighlighted)
    {
        if ( unhighlighted != null && unhighlighted.GetInstanceID() == gameObject.GetInstanceID() )
        {
            ProteinObjectManager.Instance.RaiseAminoMeshSelected(new AminoAcidMeshSelection(_info.ProteinName, _info,
                false));
        }

        return true;
    }

    private bool HighlightStarted(GameObject highlighted)
    {
        if ( highlighted != null && highlighted.GetInstanceID() == gameObject.GetInstanceID() )
        {
            ProteinObjectManager.Instance.RaiseAminoMeshSelected(new AminoAcidMeshSelection(_info.ProteinName, _info,
                true));
        }

        return true;
    }
}

[Serializable]
public struct AminoAcidShortInfo
{
    public string ProteinName;
    public int Order;
    public string ShortName;
    public string LongName;
}