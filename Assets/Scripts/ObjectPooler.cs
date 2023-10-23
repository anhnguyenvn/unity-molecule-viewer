using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    [Header("References")]
    [SerializeField] private List<AtomCharacter> _atomCharsPooledObject = new List<AtomCharacter>();
    [SerializeField] private Transform _stringsContainer;
    [SerializeField] private int _initialCount = 10;

    [Header("Prefabs")]
    [SerializeField] private AtomCharacter _atomCharPrefab;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // initial pool
        for (int i = 0; i < _initialCount; i++)
        {
            var obj = Instantiate(_atomCharPrefab, _stringsContainer);
            _atomCharsPooledObject.Add(obj);
            obj.gameObject.SetActive(false);
        }
    }

    public AtomCharacter GetAtomFromPool(int index)
    {
        if (index < _atomCharsPooledObject.Count)
        {
            _atomCharsPooledObject[index].gameObject.SetActive(true);
            return _atomCharsPooledObject[index];
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
    }
}
