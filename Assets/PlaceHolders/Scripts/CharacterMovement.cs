using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float _walkSpeed = 5f;
    [SerializeField] protected float _runSpeed = 8f;
    [SerializeField] private float _acceleration = 10f;
    [SerializeField] private float _turnSpeed = 10f;
    public bool ControlRotation = true;



    [Header("Airborne")]
    [SerializeField] private float _gravity = -20f;
    [SerializeField] private float _jumpHeight = 2.25f;
    [SerializeField] private float _airControl = 0.1f;
    [SerializeField] private bool _airTurning = true;

    [Header("Grounding")]
    [SerializeField] private float _groundCheckOffset = 0.1f;
    [SerializeField] private float _groundCheckDistance = 0.4f;
    [SerializeField] private float _maxSlopeAngle = 40f;
    [SerializeField] private float _groundFudgeTime = 0.25f;
    [SerializeField] private LayerMask _groundMask = 1 << 0;
    //[SerializeField] private Transform _stairDetectingPos;
    //[SerializeField] private Transform _slopeDetectingPos;
    //[SerializeField] private Transform _stairLimitingPos;
    //[SerializeField] private float _stepHeight = 0.3f;
    //[SerializeField] private float _stepSmooth = 0.1f;
    [SerializeField] private float _groundCheckToleranceOffset = 0.1f;
    //[SerializeField] private Vector3 _wallCheck;
    //[SerializeField] private float _slipSpeed = 8f;

    //[Header("Dash")]
    //public AnimationCurve dashCurve;
    //[SerializeField] private float _dashForce = 1f;
    //[SerializeField] private float _dashTime = 0.3f;
    //private float _dashTimer = 0f;
    //public bool IsRun = false;





    [HideInInspector]
    //public bool IsStepping { get; set; }

    public float MoveSpeedMultiplier = 1f;
    public float Speed => _walkSpeed;
    public bool IsFudgeGrounded => Time.timeSinceLevelLoad < _lastGroundedTime + _groundFudgeTime;
    public Vector3 Velocity { get => _rigidbody.linearVelocity; protected set => _rigidbody.linearVelocity = value; }
    public Vector3 MoveInput;
    public Vector3 LocalMoveInput;
    public Vector3 LookDirection;
    public Vector3 ControlVelocity = Vector3.zero;
    public bool HasMoveInput;
    public bool IsGrounded;
    public Vector3 SurfaceVelocity;
    public bool CanMove = true;
    public Vector3 GroundNormal = Vector3.up;
    protected float _lastGroundedTime;


    public float TurnSpeedMultiplier { get; set; } = 1f;
    public Vector3 GroundPoint;
    private Vector3 _groundCheckStart => transform.position + transform.up * _groundCheckOffset;

    protected Rigidbody _rigidbody;
    protected CharacterAnimations _characterAnimations;
    protected NavMeshAgent _navMeshAgent;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.useGravity = false;
        LookDirection = transform.forward;

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.updatePosition = false;
        _navMeshAgent.updateRotation = false;
    }

    public void EnterKnockedAway()
    {
        
    }


    public void SetMoveInput(Vector3 input)
    {

        if (!CanMove)
        {
            MoveInput = Vector3.zero;
            return;
        }
        input = Vector3.ClampMagnitude(input, 1f);
        HasMoveInput = input.magnitude > 0.1f;
        input = HasMoveInput ? input : Vector3.zero;
        Vector3 flattened = new Vector3(input.x, 0f, input.z);
        flattened = flattened.normalized * input.magnitude;
        MoveInput = flattened;
        LocalMoveInput = transform.InverseTransformDirection(MoveInput);
    }

    public void ToggleRun()
    {
        //IsRun = !IsRun;
    }

    public virtual void SetLookDirection(Vector3 direction)
    {
        if (!CanMove) return;
        if (direction.magnitude < 0.1f) return;
        LookDirection = new Vector3(direction.x, 0f, direction.z).normalized;
    }

    public void Jump()
    {
        if (!CanMove || !IsFudgeGrounded) return;

        float jumpVelocity = Mathf.Sqrt(2f * -_gravity * _jumpHeight);
        Velocity = new Vector3(Velocity.x, jumpVelocity, Velocity.z);
    }

    //public void Dash()
    //{
    //    _dashTimer += _dashTime;
    //    _character.stats.Stamina -= 30f;
    //}

    public virtual void BeginAttackCorrection()
    {

    }
    //public bool CheckSlopeStep(Vector3 targetFacing)
    //{
    //    if (Physics.Raycast(_stairDetectingPos.transform.position, targetFacing, out rc, 0.07f, _groundMask))
    //    {
    //        if (Vector3.Cross(rc.normal, Vector3.up).magnitude > 0.8f ||
    //        (!Physics.Raycast(_stairLimitingPos.transform.position, targetFacing, out rc, 0.08f, _groundMask)))
    //        {
    //            _stairTimer = 0.2f;

    //            return true;
    //        }
    //    }
    //    if (Physics.Raycast(_slopeDetectingPos.transform.position, targetFacing, out rc, 0.2f, _groundMask)) return true;
    //    return false;
    //}

    //private bool stepClimb()
    //{

    //    bool step = CheckSlopeStep(transform.TransformDirection(Vector3.forward)) || CheckSlopeStep(transform.TransformDirection(1.5f, 0, 1)) || CheckSlopeStep(transform.TransformDirection(-1.5f, 0, 1));
    //    if (step) _rigidbody.position += _stepSmooth * Vector3.up * Mathf.Clamp01(MoveInput.magnitude * 10);
    //    return step;
    //}

    public void SetCanMove(bool canMove)
    {
        CanMove = canMove;
    }


    protected virtual Vector3 VelocityCalc()
    {
        Vector3 input = MoveInput;
        Vector3 right = Vector3.Cross(transform.up, input);
        Vector3 forward = Vector3.Cross(right, GroundNormal);
        //float spd = IsRun ? _runSpeed : _walkSpeed;
        Vector3 targetVelocity = forward * (MoveSpeedMultiplier);
        if (!CanMove) targetVelocity = Vector3.zero;
        // adds velocity of surface under character, if character is stationary
        targetVelocity += SurfaceVelocity * (1f - Mathf.Abs(MoveInput.magnitude));
        return targetVelocity;
    }




    public virtual void FixedUpdate()
    {

            //IsStepping = stepClimb();
            //_stairTimer = Mathf.Min(_stairTimer - Time.fixedDeltaTime, 0);
            //if (_stairTimer > 0)
            //{
            //    _rigidbody.position += _stepSmooth * Vector3.up;
            //}
            //if (_dashTimer > 0)
            //{
            //    _dashTimer -= Time.fixedDeltaTime;
            //    float t = (_dashTime - _dashTimer) / _dashTime;
            //    float dashMultiplier = dashCurve.Evaluate(t);
            //    _rigidbody.linearVelocity = (transform.forward * 0.9f + MoveInput * 0.1f) * _dashForce * dashMultiplier;
            //    //acceleration *= _dashDodgingInputMod;
            //    //mix lookdirection with forward transform
            //    LookDirection = LookDirection * 0.1f + transform.forward * 0.9f;
            //}
            //lock movement while dashing
            //else
            //{
                Vector3 targetVelocity = VelocityCalc();
                
                // calculates acceleration required to reach desired velocity and applies air control if not grounded
                Vector3 velocityDiff = targetVelocity - Velocity;
                velocityDiff.y = 0f;
                float control = IsGrounded ? 1f : _airControl;
                Vector3 acceleration = velocityDiff * (_acceleration * control);
                // zeros acceleration if airborne and not trying to move (allows for nice jumping arcs)
                if (!IsGrounded && !HasMoveInput) acceleration = Vector3.zero;
                // add gravity
                acceleration += GroundNormal * _gravity;
                _rigidbody.AddForce(acceleration);
            


        //}
        // rotates character towards movement direction
        if (ControlRotation && (IsGrounded || _airTurning))
        {
            Vector3 forwardLocal = transform.forward;
            float turnDirection = -Mathf.Sign(Vector3.Cross(LookDirection, forwardLocal).y);
            float turnMagnitude = 1f - Mathf.Clamp01(Vector3.Dot(forwardLocal, LookDirection));
            float angularVelocity = turnMagnitude * _turnSpeed * TurnSpeedMultiplier * turnDirection;
            _rigidbody.angularVelocity = new Vector3(0f, angularVelocity, 0f);
        }

    }


    // Update is called once per frame
    public virtual void Update()
    {
        bool b = CheckGrounded();
        //IsGrounded = IsStepping ? true : b;

        if (_navMeshAgent.hasPath)
        {
            Vector3 nextPathPoint = _navMeshAgent.path.corners[1];
            Vector3 pathDir = (nextPathPoint - transform.position).normalized;
            SetMoveInput(pathDir);
            SetLookDirection(pathDir);

        }

        _navMeshAgent.nextPosition = transform.position;
    }


    private bool CheckGrounded()
    {
        Vector3 gCLeft = _groundCheckStart;
        gCLeft -= _groundCheckToleranceOffset * transform.right;
        Vector3 gCRight = _groundCheckStart;
        gCRight += _groundCheckToleranceOffset * transform.right;
        Vector3 gCForward = _groundCheckStart;
        gCRight += _groundCheckToleranceOffset * transform.forward;
        // raycast to find ground
        bool hit1 = Physics.Raycast(gCLeft, -transform.up, out RaycastHit hitInfo, _groundCheckDistance, _groundMask);
        bool hit2 = Physics.Raycast(gCRight, -transform.up, out RaycastHit hitInfo2, _groundCheckDistance, _groundMask);
        bool hit3 = Physics.Raycast(gCForward, -transform.up, out RaycastHit hitInfo3, _groundCheckDistance, _groundMask);
        bool hit = hit1 || hit2 || hit3;
        //if (hit)
        //{
            // set default ground surface normal and SurfaceVelocity
            //if (hit1 && (_stairLayer & (1 << hitInfo.collider.gameObject.layer)) != 0)
            //{
            //    OnStair = true;
            //    InteractingStair = hitInfo.collider.GetComponent<Stair>();
            //}
            //else
            //{
            //    if (hit2 && (_stairLayer & (1 << hitInfo2.collider.gameObject.layer)) != 0)
            //    {
            //        OnStair = true;
            //        InteractingStair = hitInfo2.collider.GetComponent<Stair>();
            //    }
            //    else
            //    {
            //        if (hit3 && (_stairLayer & (1 << hitInfo3.collider.gameObject.layer)) != 0)
            //        {
            //            OnStair = true;
            //            InteractingStair = hitInfo3.collider.GetComponent<Stair>();
            //        }
            //        else
            //        {
            //            OnStair = false;
            //        }
            //    }
            //}
           
        //}

        GroundNormal = Vector3.up;
        SurfaceVelocity = Vector3.zero;

        // if ground wasn't hit, character is not grounded
        //if (!hit)
        //{
        //    if (_rigidbody.linearVelocity.magnitude < 0.05f) SlipCheck();
        //    return false;
        //}

        // gets velocity of surface underneath character if applicable
        if (hitInfo.rigidbody != null || hitInfo2.rigidbody != null || hitInfo3.rigidbody != null) SurfaceVelocity = hitInfo.rigidbody.linearVelocity;

        //stores ground point
        GroundPoint = hitInfo.point;
        // test angle between character up and ground, angles above _maxSlopeAngle are invalid
        bool angleValid = Vector3.Angle(transform.up, hitInfo.normal) < _maxSlopeAngle;
        if (angleValid)
        {
            // record last time character was grounded and set correct floor normal direction
            _lastGroundedTime = Time.timeSinceLevelLoad;
            GroundNormal = hitInfo.normal;
            return true;
        }

        return false;
    }
    public void InterpolateRigidBody() => _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

    //private void SlipCheck()
    //{
    //    RaycastHit hitInfo;
    //    Vector3 raySpawnPos = transform.position + Vector3.up * _wallCheck.y;

    //    Vector3 forward = transform.forward * _wallCheck.x;
    //    Vector3 back = -transform.forward * _wallCheck.x;
    //    Vector3 right = transform.right * _wallCheck.x;
    //    Vector3 left = -transform.right * _wallCheck.x;

    //    Ray frontRay = new Ray(raySpawnPos, forward);
    //    Ray backRay = new Ray(raySpawnPos, back);
    //    Ray rightRay = new Ray(raySpawnPos, right);
    //    Ray leftRay = new Ray(raySpawnPos, left);

    //    float dis = _wallCheck.x;

    //    bool hit = false;
    //    if (Physics.Raycast(frontRay, out hitInfo, dis, _groundMask))
    //    {
    //        HitForSlip(transform.forward);
    //        hit = true;
    //    }
    //    if (Physics.Raycast(backRay, out hitInfo, dis, _groundMask) || Physics.Raycast(rightRay, out hitInfo, dis, _groundMask) || Physics.Raycast(leftRay, out hitInfo, dis, _groundMask))
    //    {
    //        HitForSlip(hitInfo.normal);
    //        hit = true;
    //    }
    //}

    //private void HitForSlip(Vector3 dir)
    //{
    //    _rigidbody.AddForce(((dir * _slipSpeed) + Vector3.down) * Time.deltaTime);
    //}

    public void MoveTo(Vector3 destination)
    {

        _navMeshAgent.SetDestination(destination);
    }



    public void Stop()
    {
        _navMeshAgent.ResetPath();
        MoveSpeedMultiplier = 1;
        SetMoveInput(Vector3.zero);
    }
}
