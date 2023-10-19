using System;
using System.Collections.Generic;
using System.Linq;
using HighlightPlus;
using UnityEngine;

public class HighlightedMeshItem : MonoBehaviour
{
    public static event Action<AminoAcidShortInfo, bool> OnMeshHighlighted;
    public static event Action<List<AminoAcidShortInfo>> OnAminoAcidDatalLoaded;
    private static readonly List<HighlightedMeshItem> AllItems = new List<HighlightedMeshItem>();
    
    public AminoAcidShortInfo _info;

    private HighlightManager _highlightManager;

    public void SetInfo(AminoAcidShortInfo info)
    {
        _info = info;
    }

    public static void SetHighlighted(int order, bool isHighlight)
    {
        if ( AllItems == null ) return;
        var highlightedItem = AllItems.FirstOrDefault(e => e != null && e._info.Order == order);
        if ( highlightedItem == null ) return;
        if (HighlightManager.instance != null)
        {
            if (isHighlight)
                HighlightManager.instance.ToggleObject(highlightedItem.transform);
            else
                HighlightManager.instance.UnselectObject(highlightedItem.transform);
        }

    }

    private void Awake()
    {
        _highlightManager = HighlightManager.instance;
        AllItems?.Add(this);
        if ( _highlightManager == null ) return;
        _highlightManager.OnObjectHighlightStart += HighlightStarted;
        _highlightManager.OnObjectHighlightEnd += HighlightEnded;
    }

    private void OnDestroy()
    {
        if ( AllItems != null && AllItems.Contains(this) )
        {
            AllItems.Remove(this);
        }
        
        if ( _highlightManager == null ) return;
        // _highlightManager.OnObjectHighlightStart -= HighlightStarted;
        // _highlightManager.OnObjectHighlightEnd -= HighlightEnded;
    }

    private bool HighlightEnded(GameObject unhighlighted)
    {
        if ( unhighlighted != null && unhighlighted.GetInstanceID() == gameObject.GetInstanceID() )
        {
            OnMeshHighlighted?.Invoke(_info, false);
        }
        
        return true;
    }

    private bool HighlightStarted(GameObject highlighted)
    {
        if ( highlighted != null && highlighted.GetInstanceID() == gameObject.GetInstanceID() )
        {
            OnMeshHighlighted?.Invoke(_info, true);
        }

        return true;
    }
    

    public static void AllHighlightedInfoLoaded()
    {
        if ( AllItems == null ) return;
        var allInfos = AllItems.Select(e => e._info);
        OnAminoAcidDatalLoaded?.Invoke(allInfos.ToList());
    }
}

[Serializable]
public struct AminoAcidShortInfo
{
    public int Order;
    public string ShortName;
    public string LongName;
}