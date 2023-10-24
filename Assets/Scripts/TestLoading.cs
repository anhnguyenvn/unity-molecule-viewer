using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class TestLoading : MonoBehaviour
{

    [SerializeField] private List<Transform> _listPlaceHolders;
    [SerializeField] private Image _cover;
    [SerializeField] private float _fadeDuration = 1f;
    [SerializeField] private AnimationCurve _fadeCurve;
    
    
    private bool _fadeCover = false;
    private readonly Color _transColor = new Color(1f, 1f, 1f, 0f);

    private float _initTime = 0f;
    
    // Start is called before the first frame update
    private IEnumerator Start()
    {
        var listFiles = Directory.GetFiles(
            $"{Application.streamingAssetsPath}{Path.DirectorySeparatorChar}CIFData{Path.DirectorySeparatorChar}",
            "*.cif");
        
        yield return new WaitForEndOfFrame();
        ProteinObjectManager.Instance.LoadProteins(new List<string>(listFiles), _listPlaceHolders);
        ProteinObjectManager.Instance.OnAllProteinDataLoaded += FadeIn;
        _initTime = 0f;
    }

    private async void FadeIn(object sender, ProteinDataLoadArg e)
    {
        await Task.Delay(TimeSpan.FromSeconds(1f));
        _fadeCover = true;
    }

    private void Update()
    {
        if ( _fadeCover )
        {
            _initTime += Time.deltaTime;
            if ( _cover == null ) return;
            var t = _fadeCurve.Evaluate(_initTime / _fadeDuration);
            // Debug.Log($"<color=yellow>t {t}</color>");
            _cover.color = Color.Lerp(Color.black, _transColor, t);
            if ( _cover.color.a <= 0.01f )
            {
                _fadeCover = false;
                _cover.gameObject.SetActive(false);
            }
        }
    }
}
