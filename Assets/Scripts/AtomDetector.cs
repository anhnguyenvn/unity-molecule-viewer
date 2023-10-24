using System.Collections.Generic;
using UnityEngine;
using System;

public class AtomDetector : MonoBehaviour
{
    [SerializeField] private float _maxRayDistance = 2.0f;
    [SerializeField] private float _sphereCastRadius = 1f;
    

    private List<HighlightedMeshItem> _meshesInRange = new List<HighlightedMeshItem>();
    private HighlightedMeshItem _nearestItem;
    public HighlightedMeshItem NearestItem => _nearestItem;
    private bool _itemInRange = false;
    private RaycastHit _raycastHit;

    public static Action<string> OnChangedNearestItem;

    private bool hasJustFoundAtom = false;
    private bool hasJustLostAtom = false;

    private void OnTriggerEnter(Collider other)
    {
        if ( other.TryGetComponent<HighlightedMeshItem>(out var meshItem) )
        {
            if ( !_meshesInRange.Contains(meshItem) )
            {
                _itemInRange = true;
                _meshesInRange.Add(meshItem);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ( other.TryGetComponent<HighlightedMeshItem>(out var meshItem) )
        {
            if ( _meshesInRange.Contains(meshItem) )
            {
                _meshesInRange.Remove(meshItem);
            }

            if ( _meshesInRange.Count == 0 )
            {
                _nearestItem = null;
                _itemInRange = false;
                OnChangedNearestItem?.Invoke(string.Empty);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(_raycastHit.point, _sphereCastRadius);
    }

    private void FixedUpdate()
    {
        var raycasted =
            Physics.SphereCast(transform.position, _sphereCastRadius, transform.forward, out _raycastHit, _maxRayDistance);
        if ( raycasted && _raycastHit.transform.TryGetComponent<HighlightedMeshItem>(out var meshItem) )
        {
            _nearestItem = meshItem;
            if ( !hasJustFoundAtom )
            {
                hasJustFoundAtom = true;
                OnChangedNearestItem?.Invoke(_nearestItem._info.ProteinName);
                Debug.Log($"<color=yellow>FOUND {_nearestItem._info.ProteinName} </color>");
            }

            hasJustLostAtom = false;
        }
        else
        {
            _nearestItem = null;

            if ( !hasJustLostAtom )
            {
                hasJustLostAtom = true;
                OnChangedNearestItem?.Invoke(string.Empty);
                Debug.Log($"<color=yellow>NO FOCUSS </color>");
            }

            hasJustFoundAtom = false;
        }
    }
}