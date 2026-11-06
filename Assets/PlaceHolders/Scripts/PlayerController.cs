using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // initial cursor state
    [SerializeField] private CursorLockMode _cursorMode = CursorLockMode.Locked;
    // make character look in Camera direction instead of MoveDirection
    [SerializeField] private bool _lookInCameraDirection;
    [SerializeField] private GameObject _camera;

    [Header("Interactable")]
    [SerializeField] private float _interactionRadius;
    [SerializeField] private LayerMask _interactableLayer = 1 << 7;
    [SerializeField] private GameObject _interactionUi;
    [SerializeField] private float _interactCd = 5f;

    private bool fix_trig = false;
    private CharacterMovement _characterMovement;
    private CharacterAnimations _characterAnimations;
    private Vector2 _moveInput;
    [SerializeField] private bool _onUI = false;
    private PlayerInput _input;
    private float _justInteractedTimer = 0f;

    [Header("UI Panels")]
    [SerializeField] private GameObject _skillPanel;

    private void Start()
    {
        Debug.Log($"PlayerInput: {_input}");
        Debug.Log($"Actions: {_input?.actions}");
        Debug.Log($"Current Map: {_input?.currentActionMap?.name}");

        var move = _input?.actions?.FindAction("Move");

        if (move == null)
        {
            Debug.LogError("Move action not found!");
            return;
        }

        Debug.Log($"Move enabled: {move.enabled}");
        Debug.Log($"Move bindings: {move.bindings.Count}");
        Debug.Log($"Move controls: {move.controls.Count}");
    }
    private void Awake()
    {
        _characterMovement = GetComponent<CharacterMovement>();
        _characterAnimations = GetComponent<CharacterAnimations>();
        Cursor.lockState = _cursorMode;
        //_camera = Camera.main.gameObject;
        _input = GetComponent<PlayerInput>();
    }

    public void OnMove(InputValue value)
    {

        //if (_onUI) return;
        Debug.Log("input move");
        _moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (_onUI) return;
        _characterAnimations.SetAnimatorTrigger("Jump");
    }

    public void SetOnUI(bool b)
    {
        _onUI = b;
    }


    private void Update()
    {

        if (_characterMovement == null) return;
        // find correct right/forward directions based on main camera rotation
        // send player input to character movement
        Vector3 moveInput = new Vector3();

            Vector3 up = Vector3.up;
            Vector3 right = _camera.transform.right;
            Vector3 forward = Vector3.Cross(right, up);
            moveInput = forward * _moveInput.y + right * _moveInput.x;
            _characterMovement.SetLookDirection(moveInput);

        _characterMovement.SetMoveInput(moveInput);
    }

}
