using System.Collections;
using System.Collections.Generic;
using System.IO;
using UMol.API;
using UnityEngine;

public class TestLoading : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        APIPython.load($"{Application.streamingAssetsPath}{Path.DirectorySeparatorChar}CIFData{Path.DirectorySeparatorChar}AF-O15552-F1-model_v4.cif");
    }
}
