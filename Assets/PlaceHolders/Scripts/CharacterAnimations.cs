using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimations : MonoBehaviour
{
    protected Animator _animator;
    private CharacterMovement _characterMovement;
    [SerializeField] protected float _dampTime = 0.1f;
    [SerializeField] private Collider[] _attackColliders;
    [SerializeField] private ParticleSystem[] _effects;
    [SerializeField] private ObjectPool[] _pooledEffects;
    [SerializeField] private float _slowTime = 0.3f;
    [Range(0.0f, 1.0f)] [SerializeField] private float _slowAmount = 0.5f;
    [SerializeField] private bool _useSwordCollider = false;
    [SerializeField] private GameObject _swordCollider;
    [SerializeField] private ParticleSystem[] _visualEffects;
    [SerializeField] private AudioSource[] _soundEffects;
    public float _slowTimer = 0f;


    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _characterMovement = GetComponent<CharacterMovement>();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 velocity = _characterMovement.Velocity;
        Vector3 flattenedVelocity = new Vector3(velocity.x, 0f, velocity.z);

        float horizontalVelocity = Vector3.Dot(flattenedVelocity, transform.right);
        float verticleVelocity = Vector3.Dot(flattenedVelocity, transform.forward);
        float speed = Mathf.Min(_characterMovement.MoveInput.magnitude, flattenedVelocity.magnitude / _characterMovement.Speed);

        _animator.SetFloat("HorizontalSpeed", horizontalVelocity * 0.5f, _dampTime, Time.deltaTime);
        _animator.SetFloat("VerticalSpeed", verticleVelocity * 0.3f, _dampTime, Time.deltaTime);
        _animator.SetFloat("Speed", speed, _dampTime, Time.deltaTime);
        _animator.SetFloat("BackwardModifier", _characterMovement.BackwardModifier);

        // send grounded state
        _animator.SetBool("IsGrounded", _characterMovement.IsGrounded);
        //_animator.SetBool("IsAlive", _characterStats.IsAlive);

        if (_slowTimer > 0f)
        {
            _slowTimer--;
            Time.timeScale = 1 - _slowAmount;
            if (_slowTimer <= 0) Time.timeScale = 1f;
        }
    }

    public void SetDefensePose(bool b)
    {
        _animator.SetBool("DefensePose", b);
    }

    public void SetOnStair(bool b)
    {
        _animator.SetBool("OnStair", b);
    }
    public void SetTimeSlow()
    {
        _slowTimer = _slowTime;
    }

    public void SetAnimatorTrigger(string s)
    {
        _animator.SetTrigger(s);
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

    public void AE_Dash()
    {
        _characterMovement.Dash();
    }

    public void AE_Dodge()
    {
        _characterMovement.DodgeBack();
    }

    public void AE_Jump()
    {
        //_animator.applyRootMotion = false;
        _characterMovement.SetCanMove(true);
        _characterMovement.Jump();
    }

    public void AE_Attack(int slashNum)
    {
        if (!_useSwordCollider)
            _attackColliders[slashNum].gameObject.SetActive(true);
        else
            _swordCollider.SetActive(true);
    }


    public void AE_PlayVisualEffect(int effectNum)
    {
        if (_visualEffects[effectNum] != null) _visualEffects[effectNum].Play();

    }
    public void AE_PlayVisualEffectRandom(int effectNum)
    {

    }

    public void AE_PlaySoundEffect(int effectNum)
    {
        if (_soundEffects[effectNum] != null) _soundEffects[effectNum].Play();

    }

    public void AE_PlaySoundEffectRandom(int effectNum)
    {

    }
    public void AE_ResetTrigger(string triggerName)
    {
        _animator.ResetTrigger(triggerName);
    }

    public void AE_UnlockTransition()
    {
        _animator.SetBool("TransitionUnlocked", true);
    }
}
