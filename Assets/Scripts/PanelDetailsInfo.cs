using UnityEngine;
using TMPro;

public class PanelDetailsInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text _textDetailsInfo;

    private void OnEnable()
    {
        if ( ProteinObjectManager.Instance == null ) return;
        ProteinObjectManager.Instance.OnAminoAcidMeshSelected += PopulateDetailsInfo;
    }

    private void PopulateDetailsInfo(object sender, AminoAcidMeshSelection e)
    {
        SetText(e._info.ToString());
    }

    private void OnDisable()
    {
        if ( ProteinObjectManager.Instance == null ) return;
        ProteinObjectManager.Instance.OnAminoAcidMeshSelected -= PopulateDetailsInfo;
    }

    public void SetText(string text)
    {
        _textDetailsInfo.text = text;
    }
}
