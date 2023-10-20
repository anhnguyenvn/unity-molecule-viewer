using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TestLoading : MonoBehaviour
{

    [SerializeField] private List<Transform> _listPlaceHolders;

    
    // Start is called before the first frame update
    IEnumerator Start()
    {
        var listFiles = Directory.GetFiles(
            $"{Application.streamingAssetsPath}{Path.DirectorySeparatorChar}CIFData{Path.DirectorySeparatorChar}",
            "*.cif");
        yield return new WaitForEndOfFrame();
        ProteinObjectManager.Instance.LoadProteins(new List<string>(listFiles), _listPlaceHolders);
    }
}
