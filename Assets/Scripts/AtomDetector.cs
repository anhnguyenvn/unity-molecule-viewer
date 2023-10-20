using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AtomDetector : MonoBehaviour
{
    [SerializeField] private float _maxRayDistance = 1.0f;

    private List<HighlightedMeshItem> _meshesInRange = new List<HighlightedMeshItem>();
    private HighlightedMeshItem _nearestItem;
    private bool _itemInRange = false;
    private RaycastHit _raycastHit;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<HighlightedMeshItem>(out var meshItem))
        {
            if (!_meshesInRange.Contains(meshItem))
            {
                _itemInRange = true;
                _meshesInRange.Add(meshItem);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<HighlightedMeshItem>(out var meshItem))
        {
            if (_meshesInRange.Contains(meshItem))
            {
                _meshesInRange.Remove(meshItem);
            }
            if (_meshesInRange.Count == 0)
            {
                _nearestItem = null;
                _itemInRange = false;
            }
        }
    }

    private void Update()
    {
        if (!_itemInRange)
            return;


        if (Physics.Raycast(transform.position, transform.forward, out _raycastHit, _maxRayDistance))
        {
            if (_raycastHit.transform.TryGetComponent<HighlightedMeshItem>(out var meshItem))
            {
                _nearestItem = meshItem;
            }
        }
        else
        {
            _nearestItem = null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
    }
}
