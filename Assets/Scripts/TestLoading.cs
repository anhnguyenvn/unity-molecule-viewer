using System.Collections;
using System.Collections.Generic;
using UMol.API;
using UnityEngine;

public class TestLoading : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        APIPython.load(@"C:\Users\anh\Downloads\AF-O15552-F1-model_v4.cif", forceDSSP:true);
        
    }
}
