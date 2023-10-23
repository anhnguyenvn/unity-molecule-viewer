using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringsContainer : MonoBehaviour
{
    [SerializeField] private AtomCharacter _atomCharacterPrefab;

    private List<AtomCharacter> _currentAminoAcidCharacters = new List<AtomCharacter>();

    private string _currentProtein = String.Empty;
    readonly Dictionary<string, ProteinInfo> _listProteinData = new Dictionary<string, ProteinInfo>();
    

    private void ResetAtomChars()
    {
        foreach (var atomChar in _currentAminoAcidCharacters)
        {
            if (atomChar)
                ObjectPooler.Instance.ResetAtomToPool(atomChar);
        }
        
        _currentAminoAcidCharacters.Clear();
    }

    private void OnEnable()
    {
        // HighlightedMeshItem.OnAminoAcidDatalLoaded += PopulateStrings;
        // HighlightedMeshItem.OnMeshHighlighted += MutuallyHighlight;
        ProteinObjectManager.Instance.OnProteinDataLoaded += OnProteinDataLoaded;
        ProteinObjectManager.Instance.OnProteinsFocusStateChanged += OnProteinFocusChanged;
        ProteinObjectManager.Instance.OnAminoAcidMeshSelected += MutuallyHighlightV2;
    }

    private void OnProteinFocusChanged(object sender, Dictionary<string, bool> e)
    {
        foreach (var proteinState in e)
        {
            if ( proteinState.Value )
            {
                _currentProtein = proteinState.Key;
            }
        }

        if ( _listProteinData.ContainsKey(_currentProtein) )
        {
            ResetAtomChars();
            PopulateStrings(_listProteinData[_currentProtein]._aminoAcids);
        }
    }

    private void OnProteinDataLoaded(object sender, ProteinDataLoadArg e)
    {
        if ( !_listProteinData.ContainsKey(e._proteinInfo._proteinName) )
        {
            _listProteinData.Add(e._proteinInfo._proteinName, e._proteinInfo);
        }
    }

    private void MutuallyHighlightV2(object sender, AminoAcidMeshSelection e)
    {
        _currentAminoAcidCharacters[e._info.Order].ToggleBackground(e._isSelected);
    }

    private void OnDisable()
    {
        // HighlightedMeshItem.OnAminoAcidDatalLoaded -= PopulateStrings;
        // HighlightedMeshItem.OnMeshHighlighted -= MutuallyHighlight;
        
        ProteinObjectManager.Instance.OnProteinDataLoaded -= OnProteinDataLoaded;
        ProteinObjectManager.Instance.OnProteinsFocusStateChanged -= OnProteinFocusChanged;
        ProteinObjectManager.Instance.OnAminoAcidMeshSelected -= MutuallyHighlightV2;
    }

    private void PopulateStrings(List<AminoAcidShortInfo> atoms)
    {
        // since amino acid order starts from 1, we ignore the first element in the list.
        _currentAminoAcidCharacters?.Add(new AtomCharacter());
        foreach (var atom in atoms)
        {
            AtomCharacter atomCharacter = ObjectPooler.Instance.GetAtomFromPool(atom.Order-1); // Instantiate(_atomCharacterPrefab, transform);
            atomCharacter.transform.SetParent(transform);
            atomCharacter.SetInfo(atom);
            _currentAminoAcidCharacters.Add(atomCharacter);
        }
    }

    private void MutuallyHighlight(AminoAcidShortInfo atom, bool isHighlighted)
    {
        _currentAminoAcidCharacters[atom.Order].ToggleBackground(isHighlighted);
    }
}
