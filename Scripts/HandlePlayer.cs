using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandlePlayer : MonoBehaviour
{
    StateManager states;
    Rigidbody rb;

    public bool doAngleCheck=true;
    float degreesRunThreshold = 8;
    public bool useDot = true;
    bool overrideForce;
    bool inAngle;
    bool initVault = false;

    float rotateTimer_;
    float velocityChange = 4;
    bool applyJumpForce;

    Vector3 storeDirection;
    InputHandler ih;

    Vector3 curvelocity;
    Vector3 targetVelocity;
    float prevAngle;
    Vector3 prevDir;

    Vector3 overrideDirection;
    float overrideSpeed;
    float forceOverideTimer;
    float forceOverLife;
    bool stopVelocity;
    bool useForceCurve;
    AnimationCurve forceCurve;
    float fc_t;
    Vector3 startPosition;


    bool canVault;
    Vector3 targetVaultPosition;

    bool forceOverHasRan;
    delegate void ForceOverrideStart();
    ForceOverrideStart forceOverStart;
    delegate void ForceOverrideWrap();
    ForceOverrideWrap forceOverWrap;

    BezierCurve climbCurve;
    public bool enableRootMovement;
    private bool isAtStart;

    public void Init(StateManager st, InputHandler inputHandler)
    {
        ih = inputHandler;
        states = st;
        rb = st.rBody;
        states.anim.applyRootMotion = false;
        GameObject go = Instantiate(states.curve);
        climbCurve = go.GetComponentInChildren<BezierCurve>();
    }

    public void Tick()
    {
        if (states.curState == StateManager.CharStates.vaulting)
        {
            if (!initVault)
            {
                VaultLogicInit();
                initVault = true;
            }
            else
            {
                HandleVaulting();
            }
            return;
        }
       
        if (!overrideForce && !initVault)
        {
            
            HandleDrag();
            if (states.onLocomotion)
            {
           
                MovementNormal();
            }
            HandleJump();
        }
        else
        {
           
            states.horizontal = 0;
            states.vertical = 0;
            OverrideLogic();
        }
    }

    private void HandleVaulting()
    {
        if (states.CurValutType == StateManager.VaultType.climp_up)
        {
            HandleCurveMovement();
            return;
        }
        fc_t += Time.deltaTime;
        float targetspeed = overrideSpeed * ih.vaultCurve.Evaluate(fc_t);
        forceOverideTimer += Time.deltaTime * targetspeed / forceOverLife;
        if (forceOverideTimer > 1)
        {
            forceOverideTimer = 1;
            StopVaulting();
        }
        Vector3 targetPosition = Vector3.Lerp(startPosition, targetVaultPosition, forceOverideTimer);
        transform.position = targetPosition;
        if (overrideDirection == Vector3.zero)
            overrideDirection = transform.forward;
        Quaternion targetRot = Quaternion.LookRotation(overrideDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5);
    }

    private void HandleCurveMovement()
    {
        if (!isAtStart)
        {
            forceOverideTimer += Time.deltaTime;
            if (forceOverideTimer > forceOverLife)
            {
                forceOverideTimer = 1;
                isAtStart = true;
                InitClimbCurve();
                
            }
            Vector3 targetpos = Vector3.Lerp(startPosition, targetVaultPosition, forceOverideTimer);
            transform.position = targetpos;
        }
        else
        {
            
            if (enableRootMovement)
            {
                forceOverideTimer += Time.deltaTime;
            }
            if (forceOverideTimer > 0.95)
            {
                forceOverideTimer = 1;
                StopVaulting();
            }
            Vector3 targetpos = climbCurve.GetPointAt(forceOverideTimer);
            transform.position = targetpos;
            if (overrideDirection == Vector3.zero)
                overrideDirection = transform.forward;
            Quaternion targetRot = Quaternion.LookRotation(overrideDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
        }
    }

    public void EnableRootMovement()
    {
        enableRootMovement = true;
    }

    private void InitClimbCurve()
    {
        startPosition = states.startVaultPosition ;
        targetVaultPosition = states.targetVaultPosition;
        overrideDirection = targetVaultPosition - startPosition;
        overrideDirection.y = 0;
        enableRootMovement = true;
        climbCurve.transform.position = startPosition;
        climbCurve.transform.rotation = Quaternion.LookRotation(overrideDirection);
        BezierPoint[] points = climbCurve.GetAnchorPoints();
        points[0].transform.position = startPosition;
        points[points.Length - 1].transform.position = targetVaultPosition + Vector3.up * 0.05f;
        forceOverideTimer = 0;
    }

    private void StopVaulting()
    {
        states.curState = StateManager.CharStates.moving;
        states.vaulting = false;
        states.contollerCollider.isTrigger = false;
        states.skipGroundCheck = false;
        states.rBody.isKinematic = false;
        initVault = false;
        isAtStart = false;
        enableRootMovement = false;
        StartCoroutine("OpenCanVaultIfApplicable");
        
    }
    IEnumerator OpenCanVaultIfApplicable()
    {
        yield return new WaitForSeconds(0.4f);
        states.canVault = canVault;
    }
    private void VaultLogicInit()
    { 
        //forceOverWrap = StopVaulting;
        canVault = states.canVault;
        VaultPhaseInit(states.targetVaultPosition);
    }

    private void VaultPhaseInit(Vector3 targetPos)
    {
        states.contollerCollider.isTrigger = true;
        switch (states.CurValutType)
        {
            case StateManager.VaultType.idle:
            case StateManager.VaultType.walk:
                overrideSpeed = Statics.vaultSpeedWalking;
                states.anim.CrossFade(Statics.walkVault, 0.1f);
                break;
            case StateManager.VaultType.run:
                overrideSpeed = Statics.vaultSpeedRunning;
                states.anim.CrossFade(Statics.runVault, 0.05f);
                break;
            case StateManager.VaultType.walk_up:
                overrideSpeed = Statics.walkUpSpeed;
                if (!states.run)
                {
                    states.anim.CrossFade(Statics.walkUp, 0.05f);

                }
                else
                {
                    states.anim.CrossFade(Statics.runUp, 0.1f);
                }
                break;
            case StateManager.VaultType.climp_up:
                states.anim.CrossFade(Statics.climb_up, 0.4f);
                overrideSpeed = Statics.climpSpeed;
                break;
        }
        int mirror = UnityEngine.Random.Range(0, 2);
        states.anim.SetBool(Statics.mirrorJump, (mirror > 0));

        forceOverideTimer = 0;
        forceOverLife = Vector3.Distance(transform.position, targetPos);
        fc_t = 0;

        states.rBody.isKinematic = true;
        startPosition = transform.position;
        overrideDirection = targetPos - startPosition;
        overrideDirection.y = 0;
        targetVaultPosition = targetPos;

        if (states.CurValutType == StateManager.VaultType.climp_up)
        {
            startPosition = transform.position;
            targetVaultPosition = states.startVaultPosition;
            overrideDirection = targetPos - startPosition;
            overrideDirection.y = 0;
        }
    }

    private void OverrideLogic()
    {
        rb.drag = 0;
        if (!forceOverHasRan)
        {
            if (forceOverStart != null)
                forceOverStart();
            forceOverHasRan = true;
        }
        float targetSpeed = overrideSpeed;
        if (useForceCurve)
        {
            fc_t += Time.deltaTime / forceOverLife;
            targetSpeed *= forceCurve.Evaluate(fc_t);
        }

        rb.velocity = overrideDirection * overrideSpeed;
        forceOverideTimer += Time.deltaTime;
        if (forceOverideTimer > forceOverLife)
        {
            if (stopVelocity)
            {
                rb.velocity = Vector3.zero;
            }
            stopVelocity = false;
            overrideForce = false;
            forceOverHasRan = false;

            if (forceOverWrap!=null){
                forceOverWrap();
            }
            forceOverWrap = null;
            forceOverStart = null;

        }
    }
    public void AddVelocity(Vector3 direction, float t, float force, bool clamp,AnimationCurve fCurve,bool useForceCureve)
    {
        forceOverLife = t;
        overrideSpeed = force;
        forceOverideTimer = 0;
        overrideDirection = direction;
        rb.velocity = Vector3.zero;
        stopVelocity = clamp;
        forceCurve = fCurve;
        useForceCurve = useForceCureve;
    }

    private void HandleJump()
    {
        if(states.onGround && states.canJump)
        {
            if(states.jumpInput&&!states.jumping&&states.onLocomotion&&states.curState!=StateManager.CharStates.hold&&states.curState!=StateManager.CharStates.onAir)
            {
                if (states.curState == StateManager.CharStates.idle || states.obsTacleForward)
                {
                    states.anim.SetBool(Statics.special, true);
                    states.anim.SetInteger(Statics.specialType, Statics.GetAnimSpecialTypes(Statics.AnimSpecials.jump_idle));
                }
                if (states.curState == StateManager.CharStates.moving && !states.obsTacleForward)
                {
                    states.LegFront();
                    states.jumping = true;
                    states.anim.SetBool(Statics.special, true);
                    states.anim.SetInteger(Statics.specialType, Statics.GetAnimSpecialTypes(Statics.AnimSpecials.run_jump));
                    states.curState = StateManager.CharStates.hold;
                    states.anim.SetBool(Statics.onAir, true);
                    states.canJump = false;
                }
            }
        }
        if (states.jumping)
        {
            if (states.onGround)
            {
                if (!applyJumpForce)
                {
                    StartCoroutine(AddJumpForce(0f));
                    applyJumpForce = true;
                }
            }
            else
            {
                states.jumping = false;
            }
        }
      
    }

    IEnumerator AddJumpForce(float v)
    {
        yield return new WaitForSeconds(v);
        rb.drag = 0;
        Vector3 vel = rb.velocity;
        Vector3 forward = transform.forward;
        vel = forward * 3;
        vel.y = states.jumforce;
        rb.velocity = vel;
        StartCoroutine(CloseJump());
    }

    IEnumerator CloseJump()
    {
        yield return new WaitForSeconds(0.1f);
        states.curState = StateManager.CharStates.onAir;
        states.jumping = false;
        applyJumpForce = false;
        states.canJump = false;
        StartCoroutine(EnableJump());
    }

    IEnumerator EnableJump() 
    {
        yield return new WaitForSeconds(1.3f);
        states.canJump = true;
    }

    private void MovementNormal()
    { 
        inAngle = states.inAngle_MoveDir;

        Vector3 v = ih.camManager.transform.forward * states.vertical;
        Vector3 h = ih.camManager.transform.right * states.horizontal;

        v.y = 0;
        h.y = 0;

        if (states.onGround)
        {
            if (states.onLocomotion)
                HandleRotation_Normal(h, v);
            float targetSpeed = states.walkSpeed;
            if (states.run && states.groundAngle == 0)
            {
                targetSpeed = states.runSpeed;
            }
            if (inAngle)
                HandleVelocity_Normal(h, v, targetSpeed);
            else
                rb.velocity = Vector3.zero;
        }
        HandleAnimations_Normal();
    }

    private void HandleAnimations_Normal()
    {
        Vector3 relativeDirection = transform.InverseTransformDirection(states.moveDirection);

        float h = relativeDirection.x;
        float v = relativeDirection.z;
        if (states.obsTacleForward)
            v = 0;

        states.anim.SetFloat(Statics.vertical, v);
        states.anim.SetFloat(Statics.horizontal, h);
    }

    private void HandleVelocity_Normal(Vector3 h, Vector3 v, float targetSpeed)
    {
        Vector3 curVelocity = rb.velocity;

        if(states.horizontal!=0 || states.vertical != 0)
        {
            targetVelocity = (h + v).normalized * targetSpeed;
            velocityChange = 3;
        }
        else
        {
            velocityChange = 2;
            targetVelocity = Vector3.zero;
        }
        Vector3 vel = Vector3.Lerp(curVelocity, targetVelocity, Time.deltaTime * velocityChange);
        rb.velocity = vel;

        if (states.obsTacleForward)
            rb.velocity = Vector3.zero;
    }

    private void HandleRotation_Normal(Vector3 h, Vector3 v)
    {
        if (Mathf.Abs(states.vertical) > 0 || Mathf.Abs(states.horizontal) > 0)
        {
            storeDirection = (v + h).normalized;
            float targetAngle = Mathf.Atan2(storeDirection.x, storeDirection.z) * Mathf.Rad2Deg;

            if(states.run && doAngleCheck)
            {
                if (!useDot)
                {
                    if (Mathf.Abs(prevAngle - targetAngle) > degreesRunThreshold)
                    {
                        prevAngle = targetAngle;
                        PlayAnimSpecial(Statics.AnimSpecials.runToStop,false);
                        return;
                    }
                }
                else
                {
                    float dot = Vector3.Dot(prevDir, states.moveDirection);
                    if (dot < 0)
                    {
                        prevDir = states.moveDirection;
                        PlayAnimSpecial(Statics.AnimSpecials.runToStop,false);
                    }
                }
            }
            prevDir = states.moveDirection;
            prevAngle = targetAngle;

            storeDirection += transform.position;
            Vector3 targetDir = (storeDirection - transform.position).normalized;
            targetDir.y = 0;
            if (targetDir == Vector3.zero)
                targetDir = transform.forward;
            Quaternion targetRot = Quaternion.LookRotation(targetDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, velocityChange * Time.deltaTime);
        }
    }

    private void PlayAnimSpecial(Statics.AnimSpecials runToStop, bool spTrue=true)
    {
        int n = Statics.GetAnimSpecialTypes(runToStop);
        states.anim.SetBool(Statics.specialType, spTrue);
        states.anim.SetInteger(Statics.specialType, n);
        StartCoroutine(closeSpecialOnAnim(1f));
    }

    IEnumerator closeSpecialOnAnim(float v)
    {
        yield return new WaitForSeconds(v);
        states.anim.SetBool(Statics.special, false);
    }

    private void HandleDrag()
    {
        if (states.horizontal != 0 || states.vertical != 0 || states.onGround == false)
        {
            if (states.run)
                rb.drag = 0.1f;
            else if (states.walk && states.onGround)
                rb.drag = 4f;
            else
                rb.drag = 0;
        }
        else
            rb.drag = 4;
    }

}
