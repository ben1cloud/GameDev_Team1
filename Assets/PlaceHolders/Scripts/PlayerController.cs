using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    // initial cursor state
    [SerializeField] private CursorLockMode _cursorMode = CursorLockMode.Locked;
    // make character look in Camera direction instead of MoveDirection
    [SerializeField] private bool _lookInCameraDirection;
    [SerializeField] private GameObject _camera;
    [SerializeField] private GameObject _pausePanel;

    //InventoryPanel�������һ��ѣ���PlayerController�����Ѽ���
    [SerializeField] private GameObject inventoryPanel;


    private float originalXAxisSpeed;
    private float originalYAxisSpeed;
    
    private bool _lockLook = false;
    private float _interactionTimer;

    
    

    //some objects come in pairs
    private CharacterMovement _characterMovement;
    private CharacterAnimations _characterAnimations;
    //private PlayerStats _characterStats;
    private Vector2 _moveInput;
    [SerializeField] private bool _onUI = false;
    private PlayerInput _input;

    public bool HasInput => _moveInput.magnitude > 0.05f;


    
    public void OnPause(InputValue value)
    {
        _pausePanel.SetActive(true);
        SetOnUI(true);
    }


    private void Awake()
    {
        _characterMovement = GetComponent<CharacterMovement>();
        _characterAnimations = GetComponent<CharacterAnimations>();

        //_characterStats = GetComponent<PlayerStats>();
        Cursor.lockState = _cursorMode;
        //_camera = Camera.main.gameObject;
        _input = GetComponent<PlayerInput>();
        

    }

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }


    public void OnJump(InputValue value)
    {
        if (_onUI) return;
        _characterAnimations.SetAnimatorTrigger("Jump");
    }

    public void OnInteract(InputValue value)
    {
        if (_onUI) return;

    }

 
    /*
    public void OnInventory(InputValue value)
    {
        Debug.Log("Inventory pressed");

        if (inventoryPanel == null) return;

        bool open = !inventoryPanel.activeSelf;

        inventoryPanel.SetActive(open);
        SetOnUI(open);

        if (open)
        {
            _moveInput = Vector2.zero;
        }
    }
    */

    //public void OnDash()
    //{
    //    if (_character.stats.Stamina < 30f) return;
    //    if (_onUI) return;
    //    Vector3 moveInput = new Vector3();
    //    Vector3 up = Vector3.up;
    //    Vector3 right = _camera.transform.right;
    //    Vector3 forward = Vector3.Cross(right, up);
    //    moveInput = forward * _moveInput.y + right * _moveInput.x;
    //    if (Vector3.Dot(moveInput, transform.forward) >= 0f)
    //    {
    //        _characterAnimations.SetAnimatorTrigger("Dash");
    //    }
    //    else
    //    {
    //        _characterAnimations.SetAnimatorTrigger("DodgeBack");
    //    }
    //}

    //public void OnRun()
    //{
    //    if (_onUI) return;
    //    if (!_characterMovement.IsRun && _character.stats.Stamina < 10f) return;
    //    _characterMovement.ToggleRun();
    //}





    //public void OnSkill1()
    //{
    //    for (int i = 0; i < 5; i++)
    //    {
    //        if (SkillManager.SkillsScript.Skill1Composition[i] != -1)
    //        {
    //            _characterAnimations.Attack($"SK{SkillManager.SkillsScript.Skill1Composition[i]}");
    //            break;
    //        }
    //    }
    //    Debug.Log("This skillset does not contain any content");
    //}

    //public void OnSkill2()
    //{
    //    for (int i = 0; i < 5; i++)
    //    {
    //        if (SkillManager.SkillsScript.Skill2Composition[i] != -1)
    //        {
    //            _characterAnimations.Attack($"SK{SkillManager.SkillsScript.Skill2Composition[i]}");
    //            break;
    //        }
    //    }
    //    Debug.Log("This skillset does not contain any content");
    //}

    //public void OnSkill3()
    //{
    //    for (int i = 0; i < 5; i++)
    //    {
    //        if (SkillManager.SkillsScript.Skill3Composition[i] != -1)
    //        {
    //            _characterAnimations.Attack($"SK{SkillManager.SkillsScript.Skill3Composition[i]}");
    //            break;
    //        }
    //    }
    //    Debug.Log("This skillset does not contain any content");
    //}

    //public void OnUnequip()
    //{
    //    if (_onUI) return;
    //    if (_moveModes == MoveModes.sword)
    //    {
    //        _characterAnimations.SetAnimatorTrigger("UnequipWeapon");
    //        _characterMovement.SetHasWeapon(false);
    //        _moveModes = MoveModes.unarmed;

    //    }
    //}

    public void SetOnUI(bool b)
    {

        _onUI = b;
        if (b) Cursor.lockState = CursorLockMode.Confined;
        else Cursor.lockState = CursorLockMode.Locked;
    }

    IEnumerator WaitEndPlayerAttack()
    {
        yield return new WaitForSeconds(0.2f);
        //GlobalManagement.PlayerAttack = false;
    }

    private void Update()
    {
        if (_onUI)
        {
            _moveInput = Vector2.zero;

            if (_characterMovement != null)
            {
                _characterMovement.SetMoveInput(Vector3.zero);
                _characterMovement.SetLookDirection(Vector3.zero);
            }

            return;
        }
        
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


    
    //public void SetDefense(bool b)
    //{
    //    if (_moveModes == MoveModes.sword)
    //    {
    //        if (!b) _characterStats.SetDefensePose(false);
    //        _characterAnimations.SetDefensePose(b);
    //    }
    //}
}
