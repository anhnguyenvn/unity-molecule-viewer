using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UMol;
using UMol.API;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


// public delegate void OnSelectAminoAcidMesh(string proteinName, AminoAcidShortInfo info);
// public delegate void OnProteinDataLoaded(ProteinInfo proteinInfo);

public class AminoAcidMeshSelection : EventArgs
{
    public string _proteinName;
    public AminoAcidShortInfo _info;
    public bool _isSelected;

    public AminoAcidMeshSelection(string proteinName, AminoAcidShortInfo info, bool isSelected)
    {
        _proteinName = proteinName;
        _info = info;
        _isSelected = isSelected;
    }
}

public class ProteinDataLoadArg : EventArgs
{
    public ProteinInfo _proteinInfo;

    public ProteinDataLoadArg(ProteinInfo proteinInfo)
    {
        _proteinInfo = proteinInfo;
    }
}

public class AminoAcidCharSelection : EventArgs
{
    public string _proteinName;
    public int _aminoAcidOrder;
    public bool _isSelected;

    public AminoAcidCharSelection(string proteinName, int aminoAcidOrder, bool isSelected)
    {
        _proteinName = proteinName;
        _aminoAcidOrder = aminoAcidOrder;
        _isSelected = isSelected;
    }
}

/// <summary>
/// To manage multiple protein objects in scene
/// </summary>
public class ProteinObjectManager : Singleton<ProteinObjectManager>
{
    public event EventHandler<AminoAcidMeshSelection> OnAminoAcidMeshSelected;
    public event EventHandler<AminoAcidCharSelection> OnAminoAcidCharSelected;
    public event EventHandler<ProteinDataLoadArg> OnProteinDataLoaded;
    public event EventHandler<Dictionary<string, bool>> OnProteinsFocusStateChanged;

    private readonly Dictionary<string, bool> _proteinFocusedStates = new Dictionary<string, bool>();
    private readonly Dictionary<string, ProteinInfo> _proteinInfos = new Dictionary<string, ProteinInfo>();
    private readonly Dictionary<string, GameObject> _proteinGameObjects = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, MeshRenderer> _proteinMeshRenderers = new Dictionary<string, MeshRenderer>();
    
    private string _currentProtein;

    [SerializeField]
    private float _dimRatio = 0.65f;
    
    private void Awake()
    {
        UnityMolStructureManager.OnMoleculeLoaded += () =>
        {
            var go = UnityMolMain.getRepresentationParent();
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            go.transform.localScale = Vector3.one;
        };

        AtomDetector.OnChangedNearestItem += ChangeFocusProtein;
    }

    private void OnDestroy()
    {
        AtomDetector.OnChangedNearestItem -= ChangeFocusProtein;
    }

    private void ChangeFocusProtein(string proteinName)
    {
        if ( _currentProtein != proteinName )
        {
            SetFocusToProtein(proteinName, true);
            _currentProtein = proteinName;
        }
    }

    public void SetFocusToProtein(string proteinName, bool isFocused)
    {
        HighlightProtein(proteinName);

        if ( string.IsNullOrEmpty(proteinName) )
        {
            ClearFocusStateAllProtein();
            return;
        }
        
        if ( !_proteinFocusedStates.ContainsKey(proteinName) )
        {
            _proteinFocusedStates.Add(proteinName, false);
        }

        _proteinFocusedStates[proteinName] = isFocused;

        var stateKeys = new List<string>(_proteinFocusedStates.Keys);
        
        foreach (var stateKey in stateKeys)
        {
            if ( stateKey == proteinName ) continue;
            _proteinFocusedStates[stateKey] = false;
        }

        OnProteinsFocusStateChanged?.Invoke(this, _proteinFocusedStates);
    }

    private void HighlightProtein(string proteinName)
    {
        foreach (var meshPair in _proteinMeshRenderers)
        {
            if ( meshPair.Value != null )
            {
                if ( meshPair.Key == proteinName )
                {
                    meshPair.Value.material.color = Color.white;
                }
                else
                {
                    meshPair.Value.material.color = Color.white*_dimRatio;
                }
            }
        }
    }

    public void ClearFocusStateAllProtein()
    {
        var keys = new List<string>(_proteinFocusedStates.Keys);
        foreach (var proteinName in keys)
        {
            _proteinFocusedStates[proteinName] = false;
        }

        OnProteinsFocusStateChanged?.Invoke(this, _proteinFocusedStates);
    }

    public bool AddProteinGameObject(string proteinName, GameObject proteinGameObject)
    {
        if ( _proteinGameObjects.ContainsKey(proteinName) )
        {
            Debug.Log($"Already contain the protein {proteinName}");
        }
        else
        {
            _proteinGameObjects.Add(proteinName, proteinGameObject);
        }

        return true;
    }

    public bool AddProtein(string proteinName, ProteinInfo proteinInfo)
    {
        if ( _proteinInfos.ContainsKey(proteinName) )
        {
            Debug.Log($"Already contain the protein {proteinName}");
        }
        else
        {
            _proteinInfos.Add(proteinName, proteinInfo);
            OnProteinDataLoaded?.Invoke(this, new ProteinDataLoadArg(proteinInfo));
        }

        return true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        foreach (var go in _proteinMeshRenderers)
        {
            Gizmos.DrawWireSphere(go.Value.bounds.center, go.Value.bounds.extents.y);
        }
    }

    public bool RemoveProtein(string proteinName)
    {
        if ( _proteinInfos.ContainsKey(proteinName) )
        {
            _proteinInfos.Remove(proteinName);
        }

        return true;
    }

    public ProteinInfo GetProtein(string proteinName)
    {
        return _proteinInfos.TryGetValue(proteinName, value: out var info) ? info : new ProteinInfo();
    }

    public AminoAcidShortInfo GetAminoAcidInfo(string proteinName, int acidOrder)
    {
        if ( GetProtein(proteinName)._aminoAcids.Count > 0 )
        {
            return GetProtein(proteinName)._aminoAcids.First(e => e.Order == acidOrder);
        }

        return new AminoAcidShortInfo();
    }

    private bool IsProteinFocused(string proteinName)
    {
        return _proteinFocusedStates != null && _proteinFocusedStates.ContainsKey(proteinName) &&
               _proteinFocusedStates[proteinName];
    }

    public void RaiseAminoMeshSelected(AminoAcidMeshSelection e)
    {
        if ( IsProteinFocused(e._proteinName) )
        {
            OnAminoAcidMeshSelected?.Invoke(this, e);
        }
    }

    public void SetHighlightAcidAminMesh(string proteinModelName, int acidOrder, bool isHighlight)
    {
        if ( IsProteinFocused(proteinModelName) )
        {
            OnAminoAcidCharSelected?.Invoke(this, new AminoAcidCharSelection(proteinModelName, acidOrder, isHighlight));
        }
    }

    public void LoadProteins(List<string> cifFilePaths, List<Transform> placeHodlers)
    {
        StartCoroutine(LoadCoroutines(cifFilePaths, placeHodlers));
    }

    IEnumerator LoadCoroutines(List<string> cifFilePaths, List<Transform> placeHodlers)
    {
        var minLoopTime = Mathf.Min(cifFilePaths.Count, placeHodlers.Count);
        for (int i = 0; i < minLoopTime; i++)
        {
            var cifPath = cifFilePaths[i];
            var proteinName = Path.GetFileNameWithoutExtension(cifPath);
            var proteinGoName = $"all({proteinName})";

            APIPython.load(cifPath);

            GameObject loadedProteinGo = null;
            while (loadedProteinGo == null)
            {
                loadedProteinGo = GameObject.Find(proteinGoName);
                yield return new WaitForSeconds(0.25f);
            }

            loadedProteinGo.transform.position = placeHodlers[i].position;
            loadedProteinGo.transform.rotation = placeHodlers[i].rotation;
            loadedProteinGo.transform.localScale = placeHodlers[i].localScale;

            yield return new WaitForEndOfFrame();

            var meshrender = loadedProteinGo.GetComponentsInChildren<MeshRenderer>();
            while (meshrender == null)
            {
                yield return new WaitForEndOfFrame();
                meshrender = loadedProteinGo.GetComponentsInChildren<MeshRenderer>();
            }

            var mesh0 = meshrender[0];
            mesh0.material.color *= _dimRatio;
            _proteinMeshRenderers.Add(proteinName, mesh0);

#if UNITY_EDITOR
            testFocustProteinNames.Add(Path.GetFileNameWithoutExtension(cifPath));
#endif

            yield return new WaitForEndOfFrame();
        }
    }

#if UNITY_EDITOR
    [SerializeField] public List<string> testFocustProteinNames = new List<string>();

    [Header("Choose from 1")] [SerializeField]
    public int _currentChoice = 0;

#endif
}

[Serializable]
public struct ProteinInfo
{
    public string _proteinName;
    public List<AminoAcidShortInfo> _aminoAcids;

    public bool IsEmpty => string.IsNullOrEmpty(_proteinName);
    public static ProteinInfo Empty = new ProteinInfo();
}


#if UNITY_EDITOR

[CustomEditor(typeof(ProteinObjectManager))]
public class ProteinManageEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var script = target as ProteinObjectManager;

        if ( GUILayout.Button("Test Focus") )
        {
            var choice = Mathf.Clamp(script._currentChoice, 1, script.testFocustProteinNames.Count);
            script._currentChoice = choice;
            script.SetFocusToProtein(script.testFocustProteinNames[choice - 1], true);
        }
    }
}
#endif