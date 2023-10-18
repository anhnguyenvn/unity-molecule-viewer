using System.Collections;
using System.Collections.Generic;
using HighlightPlus;
using UnityEngine;

public class GeneratedMeshOnHover : MonoBehaviour
{
    private HighlightManager highlightManager;

    [SerializeField]
    private Material mat;
    // Start is called before the first frame update
    void Start()
    {
        highlightManager = FindObjectOfType<HighlightManager>();
        highlightManager.OnObjectHighlightStart += AddMesh;
        highlightManager.OnObjectHighlightEnd += RemoveMesh;
    }

    private bool RemoveMesh(GameObject obj)
    {
        // 
        return true;
    }

    private bool AddMesh(GameObject segmentGo)
    {
        if ( segmentGo == null || segmentGo.GetComponent<MeshRenderer>() != null ) return false;
        var segmentMeshFilter = segmentGo.AddComponent<MeshFilter>();
        segmentMeshFilter.mesh = segmentGo.GetComponent<MeshCollider>().sharedMesh;
        segmentGo.AddComponent<MeshRenderer>().material = mat;
        return true;

    }
}
