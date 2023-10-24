using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectPooler : Singleton<ObjectPooler>
{
    [Header("References")]
    [SerializeField] private List<AtomCharacter> _atomCharsPooledObject = new();
    [SerializeField] private Transform _stringsContainer;
    [SerializeField] private int _initialCount = 10;

    [Header("Prefabs")]
    [SerializeField] private AtomCharacter _atomCharPrefab;

    public List<AtomCharacter> AtomCharsPooledObject => _atomCharsPooledObject;

    private void Awake()
    {
        // initial maximum character count based on the longest protein
        if ( ProteinObjectManager.Instance != null ) ProteinObjectManager.Instance.OnAllProteinDataLoaded += Initialize;
    }

    private void Initialize(object sender, ProteinDataLoadArg proteinData)
    {
        _initialCount = proteinData._proteinInfos.Max(e => e._aminoAcids.Count);
        // initial pool
        for (var i = 0; i < _initialCount; i++)
        {
            var atomCharObject = Instantiate(_atomCharPrefab, _stringsContainer);
            atomCharObject.gameObject.SetActive(false);
            _atomCharsPooledObject.Add(atomCharObject);
        }
    }

    public AtomCharacter GetAtomFromPool(int index)
    {
        if (_atomCharsPooledObject != null && index < _atomCharsPooledObject.Count)
        {
            _atomCharsPooledObject[index].gameObject.SetActive(true);
            return AtomCharsPooledObject[index];
        }
        else
        {
            var obj = Instantiate(_atomCharPrefab, transform);
            _atomCharsPooledObject.Add(obj);
            return obj;
        }
    }

    public void ResetAtomToPool(AtomCharacter atomChar)
    {
        atomChar.gameObject.SetActive(false);
        atomChar.ToggleBackground(false);
    }

    public void ResetAllAtomToPool(int fromOrder)
    {
        if ( fromOrder < _atomCharsPooledObject.Count )
        {
            for (int i = fromOrder; i <  _atomCharsPooledObject.Count; i++)
            {
                _atomCharsPooledObject[i].gameObject.SetActive(false);
            }   
        }
    }
}
