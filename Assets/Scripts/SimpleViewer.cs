using System;
using System.Collections;
using System.Collections.Generic;
using UMol;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SimpleViewer : MonoBehaviour
{
    private Camera _currentCamera;

    private GameObject _proteinGameObject;

    [Header("Camera")]
    [SerializeField] private float moveForwardSpeed = 2f;
    [SerializeField] private float panningSpeed = 2f;
    [SerializeField] private float yawSpeed = 2f;
    [SerializeField] private float tiltSpeed = 2f;
    
    [Header("Protein")] [SerializeField]
    private float _rotateSpeed = 50f;

    [SerializeField] private Transform _placeHolder;
    
    
    
    private void Awake()
    {
        _currentCamera = GetComponent<Camera>();
    }

    private IEnumerator Start()
    {
        while (_proteinGameObject == null)
        {
            yield return new WaitForSeconds(0.25f);
            _proteinGameObject = UnityMolMain.getRepresentationParent();
        }

        _proteinGameObject.transform.position = _placeHolder.position;
        _proteinGameObject.transform.rotation = _placeHolder.rotation;
        _proteinGameObject.transform.localScale = _placeHolder.localScale;
        
    }

    // Update is called once per frame
    void Update()
    {
        if ( _currentCamera == null ) return;
        var input2D = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        _currentCamera.transform.Translate(new Vector3(0f, 0f, input2D.y)*Time.deltaTime*moveForwardSpeed);
        _currentCamera.transform.Translate(new Vector3(input2D.x, 0f, 0)*Time.deltaTime*panningSpeed);
        if ( Input.GetMouseButton(1) )
        {
            var tiltDelta = Input.GetAxis("Mouse Y")*Time.deltaTime*tiltSpeed;
            var yawDelta = Input.GetAxis("Mouse X")*Time.deltaTime*yawSpeed;
            var newRot = _currentCamera.transform.rotation*Quaternion.Euler(tiltDelta, yawDelta, 0f);
            _currentCamera.transform.rotation = Quaternion.Euler(newRot.eulerAngles.x, newRot.eulerAngles.y, 0f);
        }

        if ( Input.GetMouseButton(0) && _proteinGameObject != null )
        {
            var tiltDelta = Input.GetAxis("Mouse Y")*Time.deltaTime*_rotateSpeed;
            var yawDelta = -Input.GetAxis("Mouse X")*Time.deltaTime*_rotateSpeed;
            var newRot = _proteinGameObject.transform.rotation*Quaternion.Euler(tiltDelta, yawDelta, 0f);
            _proteinGameObject.transform.Rotate(_currentCamera.transform.right, tiltDelta, Space.World);
            _proteinGameObject.transform.Rotate(Vector3.up, yawDelta, Space.World);
        }
    }
}
