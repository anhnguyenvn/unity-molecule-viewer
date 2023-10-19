using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringsContainer : MonoBehaviour
{
    [SerializeField] private AtomCharacter _atomCharacterPrefab;

    private List<AtomCharacter> atomCharacters = new List<AtomCharacter>();

    private void Awake()
    {
        // since amino acid order starts from 1, we ignore the first element in the list.
        atomCharacters?.Add(new AtomCharacter());
    }

    private void OnEnable()
    {
        HighlightedMeshItem.OnAminoAcidDatalLoaded += PopulateStrings;
        HighlightedMeshItem.OnMeshHighlighted += MutuallyHighlight;
    }

    private void OnDisable()
    {
        HighlightedMeshItem.OnAminoAcidDatalLoaded -= PopulateStrings;
        HighlightedMeshItem.OnMeshHighlighted -= MutuallyHighlight;
    }

    private void PopulateStrings(List<AminoAcidShortInfo> atoms)
    {
        foreach (var atom in atoms)
        {
            AtomCharacter atomCharacter = Instantiate(_atomCharacterPrefab, transform);
            atomCharacter.SetInfo(atom.Order, atom.ShortName);
            atomCharacters.Add(atomCharacter);
        }
    }

    private void MutuallyHighlight(AminoAcidShortInfo atom, bool isHighlighted)
    {
        atomCharacters[atom.Order].ToggleBackground(isHighlighted);
    }
}
