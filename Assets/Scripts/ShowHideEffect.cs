using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShowHideEffect : MonoBehaviour
{

    [SerializeField] private Animator _animator;
    [SerializeField] private float _duration = 1f;
    

    private int _animIdx = Animator.StringToHash("showRatio");

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        ProteinObjectManager.Instance.OnProteinsFocusStateChanged += HandleFocusStatus;

    }

    private const float epsilon = 0.001f;
    private bool currentFocusState = false; 
    private bool previousFocusState = false;
    private bool canStartEffect = true;

    private float flipped = 1f;
    private float initElapsed = 0f;

    private void HandleFocusStatus(object sender, Dictionary<string, bool> proteinFocusStatus)
    {
        if ( proteinFocusStatus == null ) return;
        currentFocusState = proteinFocusStatus.Values.Any(p => p);

        if ( currentFocusState != previousFocusState )
        {
            flipped *= -1;
            previousFocusState = currentFocusState;
            canStartEffect = true;
        }
    }


    private void Update()
    {
        if (canStartEffect)
        {
            initElapsed += flipped*Time.deltaTime;
            initElapsed = Mathf.Clamp(initElapsed, 0f, _duration);
            if ( currentFocusState )
            {
                _animator.SetFloat(_animIdx, Mathf.Lerp(1f, 0f, (_duration - initElapsed)/_duration));
                canStartEffect = !(initElapsed < epsilon);
            }
            else
            {
                _animator.SetFloat(_animIdx, Mathf.Lerp(0f, 1f, initElapsed/_duration));
                canStartEffect = !(initElapsed > (_duration - epsilon));
            }
        }
    }
}
