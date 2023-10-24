
using System;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SimpleViewer : MonoBehaviour
{
    private Camera _currentCamera;
    private CharacterController _character;

    private GameObject _focusedObject;

    [Header("Camera")] [SerializeField] private float moveForwardSpeed = 2f;
    [SerializeField] private float panningSpeed = 2f;
    [SerializeField] private float updownSpeed = 2f;
    
    [SerializeField] private float yawSpeed = 2f;
    [SerializeField] private float tiltSpeed = 2f;

    [SerializeField] private float boostSpeedFactor = 2f;
    

    [Header("Protein")] [SerializeField] private float _rotateSpeed = 50f;
    [SerializeField] private bool _useCharacterController = false;

    [SerializeField] private BoxCollider _roomBox;
    
    [SerializeField] private Bounds _roomBound;

    private float currentBoost = 1f; 
    private bool _controlable = false;
    
    private void Awake()
    {
        _currentCamera = GetComponent<Camera>();
        _character = GetComponent<CharacterController>();
        _roomBound = _roomBox.bounds;
        ProteinObjectManager.Instance.OnProteinsFocusObjectChanged += SetCurrentFocusObject;
        ProteinObjectManager.Instance.OnAllProteinDataLoaded += ActiveControl;
    }

    private async void ActiveControl(object sender, ProteinDataLoadArg e)
    {
        await Task.Delay(TimeSpan.FromSeconds(2.5f));
        _controlable = true;
    }

    private void SetCurrentFocusObject(object sender, GameObject currentFocusProtein)
    {
        _focusedObject = currentFocusProtein;
    }

    private void OnDestroy()
    {
        if ( ProteinObjectManager.Instance == null ) return;
        ProteinObjectManager.Instance.OnProteinsFocusObjectChanged -= SetCurrentFocusObject;
        ProteinObjectManager.Instance.OnAllProteinDataLoaded -= ActiveControl;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!_controlable) return;
        if ( _currentCamera == null ) return;
        var input2D = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        var up = Input.GetKey(KeyCode.Q) ? 1 : 0;
        var down = Input.GetKey(KeyCode.E) ? -1 : 0;
        if ( _character != null && _useCharacterController)
        {
            _character.SimpleMove(new Vector3(0f, 0f, input2D.y)*Time.deltaTime*moveForwardSpeed);
            _character.SimpleMove(new Vector3(input2D.x, 0f, 0)*Time.deltaTime*panningSpeed);
        }
        else
        {
            currentBoost = Input.GetKey(KeyCode.LeftShift) ? boostSpeedFactor : 1f;
            
            var fw = new Vector3(0f, 0f, input2D.y)*Time.deltaTime*moveForwardSpeed * currentBoost;
            var side = new Vector3(input2D.x, 0f, 0f)*Time.deltaTime*panningSpeed * currentBoost;;
            var updown =  new Vector3(0f, up + down, 0f)*Time.deltaTime*updownSpeed * currentBoost;;
            var newPos = _currentCamera.transform.position + ( _currentCamera.transform.localRotation * (fw + side + updown) );

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