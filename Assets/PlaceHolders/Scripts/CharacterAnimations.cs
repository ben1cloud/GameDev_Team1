using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimations : MonoBehaviour
{
    protected Animator _animator;
    //private CharacterStats _characterStats;
    private CharacterMovement _characterMovement;
    [SerializeField] protected float _dampTime = 0.1f;
    public float _slowTimer = 0f;


    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _characterMovement = GetComponent<CharacterMovement>();
        //_characterStats = GetComponent<CharacterStats>();
    }
    // Start is called before the first frame update
    void Start()
    {
        _animator.SetFloat("RandAtSpawn", Random.Range(0f, 100f));
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 velocity = _characterMovement.Velocity;
        Vector3 flattenedVelocity = new Vector3(velocity.x, 0f, velocity.z);

        float horizontalVelocity = Vector3.Dot(flattenedVelocity, transform.right);
        float verticleVelocity = Vector3.Dot(flattenedVelocity, transform.forward);
        float speed = Mathf.Min(_characterMovement.MoveInput.magnitude, flattenedVelocity.magnitude / _characterMovement.Speed);


        _animator.SetFloat("HorizontalSpeed", horizontalVelocity, _dampTime, Time.deltaTime);
        _animator.SetFloat("VerticalSpeed", verticleVelocity, _dampTime, Time.deltaTime);
        _animator.SetFloat("Speed", speed, _dampTime, Time.deltaTime);
        // send grounded state
        _animator.SetBool("IsGrounded", _characterMovement.IsGrounded);
        //_animator.SetBool("IsAlive", _characterStats.IsAlive);


        _animator.SetFloat("RandomFloat", Random.Range(0, 100));
    }

    public void SetDefensePose(bool b)
    {
        _animator.SetBool("DefensePose", b);
    }

    public void SetOnStair(bool b)
    {
        _animator.SetBool("OnStair", b);
    }


    public void SetAnimatorTrigger(string s)
    {
        _animator.SetTrigger(s);
    }

    public void SetAnimationFloat(string s, float f)
    {
        _animator.SetFloat(s, f);
    }
    public void SetAnimationInt(string s, int i)
    {
        _animator.SetInteger(s, i);
    }

    public void SetAnimationBool(string s, bool b)
    {
        _animator.SetBool(s, b);
    }

    //Animation Events
    public void AE_TL_Lock(int tlNum)
    {
        _animator.SetBool("TL" + tlNum.ToString(), true);
    }



    public void AE_TL_Unlock(int tlNum)
    {
        _animator.SetBool("TL" + tlNum.ToString(), false);
    }

    //public void AE_Dash()
    //{
    //    _characterMovement.Dash();
    //    _character.stats.SetInvulnerable();
    //}

    public void AE_Jump()
    {
        //_animator.applyRootMotion = false;
        _characterMovement.SetCanMove(true);
        _characterMovement.Jump();
    }

}
