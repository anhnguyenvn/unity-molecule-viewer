using UnityEngine;
using TMPro;

public class PanelDetailsInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text _textDetailsInfo;

    private void OnEnable()
    {
        ProteinObjectManager.Instance.OnAminoAcidMeshSelected += PopulateDetailsInfo;
    }

    private void PopulateDetailsInfo(object sender, AminoAcidMeshSelection e)
    {
        SetText(e._info.ToString());
    }

    private void OnDisable()
    {
        ProteinObjectManager.Instance.OnAminoAcidMeshSelected -= PopulateDetailsInfo;
    }

    public void SetText(string text)
    {
        _textDetailsInfo.text = text;
    }
}
