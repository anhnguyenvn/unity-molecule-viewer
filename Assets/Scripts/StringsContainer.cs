﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class StringsContainer : MonoBehaviour
{
    [SerializeField] private AtomCharacter _atomCharacterPrefab;
    [SerializeField] private int _spawnPerFrame = 40;
    

    private readonly List<AtomCharacter> _currentAminoAcidCharacters = new List<AtomCharacter>();

    private string _currentProtein = string.Empty;
    private readonly Dictionary<string, ProteinInfo> _listProteinData = new Dictionary<string, ProteinInfo>();

    private Stack<IEnumerator> listRunningTask = new Stack<IEnumerator>();

    private bool isPopulatingData = false;
    private IEnumerator currentPopulatingDataTask = null;
    private void OnEnable()
    {
        if ( ProteinObjectManager.Instance == null )
        {
            Debug.LogError($"{typeof(ProteinObjectManager)} is null");
            return;
        }
        ProteinObjectManager.Instance.OnAllProteinDataLoaded += OnAllProteinDataLoaded;
        ProteinObjectManager.Instance.OnProteinsFocusStateChanged += OnProteinFocusChanged;
        ProteinObjectManager.Instance.OnAminoAcidMeshSelected += MutuallyHighlight;
    }

    private void OnProteinFocusChanged(object sender, Dictionary<string, bool> focusStates)
    {
        if ( focusStates == null ) return;
        
        foreach (var proteinState in focusStates)
        {
            if (proteinState.Value)
            {
                _currentProtein = proteinState.Key;
            }
        }

        if ( _listProteinData != null && !string.IsNullOrEmpty(_currentProtein) && _listProteinData.ContainsKey(_currentProtein) )
        {
            var task = PopulateStrings(_listProteinData[_currentProtein]._aminoAcids);
            listRunningTask.Push(task);
        }
    }

    private void Update()
    {
        if ( isPopulatingData ) return;

        if ( listRunningTask.TryPop(out var latestTask) )
        {
            currentPopulatingDataTask = latestTask;
            StartCoroutine(currentPopulatingDataTask);
            listRunningTask.Clear();
        }
    }

    private void OnAllProteinDataLoaded(object sender, ProteinDataLoadArg proteinData)
    {
        if ( _listProteinData == null ) return;
        if ( proteinData == null ) return;
        foreach (var protein in proteinData._proteinInfos)
        {
            _listProteinData.TryAdd(protein._proteinName, protein);
        }
    }

    private void MutuallyHighlight(object sender, AminoAcidMeshSelection meshSelected)
    {
        if ( meshSelected == null ) return;
        if (meshSelected._info.Order < _currentAminoAcidCharacters.Count)
        {
            _currentAminoAcidCharacters?[meshSelected._info.Order - 1]?.ToggleBackground(meshSelected._isSelected);
        }
    }

    private void OnDisable()
    {
        if ( ProteinObjectManager.Instance == null )
        {
            Debug.LogWarning($"{typeof(ProteinObjectManager)} is null");
            return;
        }
        ProteinObjectManager.Instance.OnAllProteinDataLoaded -= OnAllProteinDataLoaded;
        ProteinObjectManager.Instance.OnProteinsFocusStateChanged -= OnProteinFocusChanged;
        ProteinObjectManager.Instance.OnAminoAcidMeshSelected -= MutuallyHighlight;
    }

    private IEnumerator PopulateStrings(List<AminoAcidShortInfo> aminoAcids)
    {
        isPopulatingData = true;
        // since amino acid order starts from 1, we ignore the first element in the list.
        // _currentAminoAcidCharacters?.Add(new AtomCharacter());
        _currentAminoAcidCharacters?.Clear();
        if ( aminoAcids == null ) yield break;

        for (int i = 0; i < aminoAcids.Count; i++)
        {
            var acid = aminoAcids[i];
            if ( ObjectPooler.Instance == null ) continue;
            var atomCharacter = ObjectPooler.Instance.GetAtomFromPool(acid.Order - 1);
            if ( atomCharacter == null ) continue;
            atomCharacter.SetInfo(acid);
            _currentAminoAcidCharacters?.Add(atomCharacter);

            if ( i%_spawnPerFrame == 0 ) yield return null;
        }
        // turn off other character object if any
        ObjectPooler.Instance.ResetAllAtomToPool(aminoAcids.Count);
        
        isPopulatingData = false;
    }
}
