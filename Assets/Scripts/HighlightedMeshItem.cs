using System;
using System.Collections.Generic;
using System.Linq;
using HighlightPlus;
using UnityEngine;

public class HighlightedMeshItem : MonoBehaviour
{
    public static event Action<HighlightedInfo, bool> OnHighlightedInfo;
    public static event Action<List<HighlightedInfo>> OnHighlightedInfoLoaded;
    private static readonly List<HighlightedMeshItem> AllItems = new List<HighlightedMeshItem>();
    
    public HighlightedInfo _info;

    private HighlightManager _highlightManager;

    public void SetInfo(HighlightedInfo info)
    {
        _info = info;
    }

    public void SetHighlighted(int highlightItemOrder)
    {
        if ( AllItems == null ) return;
        var highlightedItem = AllItems.FirstOrDefault(e => e != null && e._info.Order == highlightItemOrder);
        if ( highlightedItem == null ) return;
        if ( _highlightManager != null ) _highlightManager.ToggleObject(highlightedItem.transform);
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
        _highlightManager.OnObjectHighlightStart -= HighlightStarted;
        _highlightManager.OnObjectHighlightEnd -= HighlightEnded;
    }

    private bool HighlightEnded(GameObject unhighlighted)
    {
        if ( unhighlighted != null && unhighlighted.GetInstanceID() == this.GetInstanceID() )
        {
            OnHighlightedInfo?.Invoke(_info, false);
            return true;
        }
        return false;
    }

    private bool HighlightStarted(GameObject highlighted)
    {
        if ( highlighted != null && highlighted.GetInstanceID() == this.GetInstanceID() )
        {
            OnHighlightedInfo?.Invoke(_info, true);
            return true;
        }

        return false;
    }

    private static void AllHighlightedInfoLoaded()
    {
        if ( AllItems == null ) return;
        var allInfos = AllItems.Select(e => e._info);
        OnHighlightedInfoLoaded?.Invoke(allInfos.ToList());
    }
}

public struct HighlightedInfo
{
    public int Order;
    public string ShortName;
    public string LongName;
}