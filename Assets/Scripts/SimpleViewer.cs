using System;
using System.Collections;
using System.Collections.Generic;
using UMol;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(CharacterController))]
public class SimpleViewer : MonoBehaviour
{
    private Camera _currentCamera;
    private CharacterController _character;

    private GameObject _focusedObject;

    [Header("Camera")] [SerializeField] private float moveForwardSpeed = 2f;
    [SerializeField] private float panningSpeed = 2f;
    [SerializeField] private float yawSpeed = 2f;
    [SerializeField] private float tiltSpeed = 2f;

    [Header("Protein")] [SerializeField] private float _rotateSpeed = 50f;
    [SerializeField] private bool _useCharacterController = false;

    [SerializeField] private BoxCollider _roomBox;
    
    [SerializeField] private Bounds _roomBound;


    private void Awake()
    {
        _currentCamera = GetComponent<Camera>();
        _character = GetComponent<CharacterController>();
        _roomBound = _roomBox.bounds;
        ProteinObjectManager.Instance.OnProteinsFocusObjectChanged += SetCurrentFocusObject;
    }

    private void SetCurrentFocusObject(object sender, GameObject currentFocusProtein)
    {
        _focusedObject = currentFocusProtein;
    }

    private void OnDestroy()
    {
        ProteinObjectManager.Instance.OnProteinsFocusObjectChanged -= SetCurrentFocusObject;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if ( _currentCamera == null ) return;
        var input2D = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if ( _character != null && _useCharacterController)
        {
            _character.SimpleMove(new Vector3(0f, 0f, input2D.y)*Time.deltaTime*moveForwardSpeed);
            _character.SimpleMove(new Vector3(input2D.x, 0f, 0)*Time.deltaTime*panningSpeed);
        }
        else
        {
            var fw = new Vector3(0f, 0f, input2D.y)*Time.deltaTime*moveForwardSpeed;
            var side = new Vector3(input2D.x, 0f, 0)*Time.deltaTime*panningSpeed;
            var newPos = _currentCamera.transform.position + ( _currentCamera.transform.localRotation * (fw + side) );

            var clappedpos = newPos;
            if (!_roomBound.Contains(clappedpos))
            {
                clappedpos = _roomBound.ClosestPoint(newPos);
            }
            _currentCamera.transform.position = clappedpos;
        }

        if ( Input.GetMouseButton(1) )
        {
            var tiltDelta = Input.GetAxis("Mouse Y")*Time.deltaTime*tiltSpeed;
            var yawDelta = Input.GetAxis("Mouse X")*Time.deltaTime*yawSpeed;
            var newRot = _currentCamera.transform.rotation*Quaternion.Euler(-tiltDelta, yawDelta, 0f);
            _currentCamera.transform.rotation = Quaternion.Euler(newRot.eulerAngles.x, newRot.eulerAngles.y, 0f);
        }

        if ( Input.GetMouseButton(0) && _focusedObject != null )
        {
            var tiltDelta = Input.GetAxis("Mouse Y")*Time.deltaTime*_rotateSpeed;
            var yawDelta = -Input.GetAxis("Mouse X")*Time.deltaTime*_rotateSpeed;
            // var newRot = _focusedObject.transform.rotation*Quaternion.Euler(tiltDelta, yawDelta, 0f);
            _focusedObject.transform.Rotate(_currentCamera.transform.right, tiltDelta, Space.World);
            _focusedObject.transform.Rotate(Vector3.up, yawDelta, Space.World);
        }
    }
}