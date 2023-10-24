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

            Debug.Log($"<color=green>\t{_info}</color>");
        }

        return true;
    }
}

[Serializable]
public struct AminoAcidShortInfo
{
    public string ProteinName;

    public string Description;
    public string EntryId;
    
    public int Order;
    public string ShortName;
    public string LongName;
    public string TypeChar;

    public string DBAccession;
    public float plddTScore;

    public override string ToString()
    {
        return
            $"{Description}\n" 
            + $"{EntryId} | {ShortName} | <b>{LongName} {Order}</b>\n" 
            + $"UNP {DBAccession} {Order} {TypeChar}\n" 
            + $"pLDDT Score {plddTScore} ({StringByConfidence(plddTScore)})";
    }
    
    private static string StringByConfidence(float confidence)
    {
        if ( confidence >= 90f )
        {
            return "Very high";
        }
        else if ( confidence >= 70f && confidence < 90 )
        {
            return "Confident";
        }
        else if ( confidence >= 50f && confidence < 70 )
        {
            return "Low";
        }
        else if ( confidence < 50 )
        {
            return "Very low";
        }

        return string.Empty;
    }

    public override bool Equals(object obj)
    {
        var otherAcid = (AminoAcidShortInfo)obj;
        return otherAcid.ProteinName == this.ProteinName &&
               otherAcid.Order == this.Order;
    }


    private static AminoAcidShortInfo _empty;
    public static AminoAcidShortInfo Emty => _empty;
}