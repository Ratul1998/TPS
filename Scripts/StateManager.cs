using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour {

    public GameObject[] modelPrefab;
    public Transform PlayerGFX;
    public GameObject curve;
    public bool inGame;
    public bool isPlayer;
    public int slot;

    public float groundDistance = 1f;
    public float groundOffset = 0.2f;
    public float distanceToCheckForward = 1.3f;
    public float runSpeed = 6;
    public float walkSpeed = 4;
    public float  jumforce=15;
    public float airTimeThreshold=0.8f;

    public float horizontal;
    public float vertical;
    public bool jumpInput;

    public bool obsTacleForward;
    public bool groundForward;
    public float groundAngle;
    public bool vaulting;
    public float vaultOverHeight=1.5f;
    public float vaultFloorHeightDifference=0.3f;
    public Vector3 targetVaultPosition;
    public Vector3 startVaultPosition;
    public string climbAnimName;
    public bool skipGroundCheck;

    #region stateRequests
    public CharStates curState;
    public VaultType CurValutType;
    public bool onGround;
    public bool run;
    public bool walk;
    public bool onLocomotion;
    public bool inAngle_MoveDir;
    public bool jumping;
    public bool canJump;
    public bool canVault = true;
    #endregion
    public enum CharStates
    {
        idle, moving, onAir, hold, vaulting
    }

    public enum VaultType
    {
        idle, walk, run,
        walk_up,
        climp_up
    }
    public enum ClimbCheckType
    {
        walk_up,climb_up
    }
    public ClimbCheckType climbCheck;
    #region References
    GameObject activeModel;
    public Animator anim;
    public Collider contollerCollider;

    public void FixedTick()
    {
        if (curState == CharStates.hold || curState == CharStates.vaulting)
            return;

        obsTacleForward = false;
        groundForward = false;
        onGround = OnGround();

        if (onGround)
        {
            Vector3 origin = transform.position;
            //Clear Forward
            origin += Vector3.up * 0.75f;
            IsClear(origin, transform.forward, distanceToCheckForward, ref obsTacleForward);
            if (!obsTacleForward)
            {
                //is ground forward?
                origin += transform.forward * 0.6f;
                IsClear(origin, -Vector3.up, groundDistance * 3, ref groundForward);
            }
            else
            {
                if (Vector3.Angle(transform.forward, moveDirection) > 30)
                {
                    obsTacleForward = false;
                }
            }
        }
        UpdateState();
        MonitorAirTime();
    }

    private void MonitorAirTime()
    {
        if (!jumping)
            anim.SetBool(Statics.onAir, !onGround);
        if (onGround)
        {
            if (prevGround != onGround)
            {
                anim.SetInteger(Statics.jumpType, (airTime > airTimeThreshold) ? (horizontal != 0 || vertical != 0) ? 2 : 1 : 0);
            }
            airTime = 0;
        }
        else
        {
            airTime += Time.deltaTime;
        }
        prevGround = onGround;
    }

    private void UpdateState()
    {
        if (curState == CharStates.hold)
            return;
        if (horizontal != 0 || vertical != 0)
        {
            curState = CharStates.moving;
        }
        else
        {
            curState = CharStates.idle;
        }
        if (!onGround)
        {
            curState = CharStates.onAir;
        }
        if (vaulting)
        {
            curState = CharStates.vaulting;
        }
    }

    private void IsClear(Vector3 origin, Vector3 forward, float distanceToCheckForward, ref bool isHit)
    {
        RaycastHit hit = new RaycastHit();
        float targetDistance = distanceToCheckForward;
        if (run)
        {
            targetDistance += 0.5f;
        }
        int numberOfHits = 0;
        for (int i = -1; i < 2; i++)
        {
            Vector3 targetOrigin = origin;
            targetOrigin += transform.right * (i * 0.3f) - (Vector3. up* 0.15f);
            Debug.DrawRay(targetOrigin, forward * distanceToCheckForward, Color.green);
            if (Physics.Raycast(targetOrigin, forward, out hit, distanceToCheckForward, ignorelayers))
            {
               
                numberOfHits++;
            }
           
        }
        if (numberOfHits > 2)
        {
            isHit = true;
        }
        else
        {
            isHit = false;
        }
        if (obsTacleForward)
        {
            Vector3 incomingVec = hit.point - origin;
            Vector3 reflectvector = Vector3.Reflect(incomingVec, hit.normal);
            float angle = Vector3.Angle(incomingVec, reflectvector);

            if (angle < 70)
            {
                obsTacleForward = false;
            }
            else
            {
                if (numberOfHits >2)
                {
                    bool willVault = false;
                    canVaultOver(hit, ref willVault);
                    if (willVault)
                    {
                        CurValutType = VaultType.walk;
                        if (run)
                            CurValutType = VaultType.run;
                        obsTacleForward = false;
                        return;
                    }
                    else
                    {
                        bool willClimb = false;
                        ClimbOver(hit, ref willClimb, ClimbCheckType.walk_up);
                        if (!willClimb)
                        {
                            ClimbOver(hit, ref willClimb, ClimbCheckType.climb_up);
                            if (willClimb)
                            {
                                obsTacleForward = false;
                                return;
                            }
                        }
                        if (!willClimb)
                        {
                            obsTacleForward = true;
                            return;
                        }
                    }
                }
            }
        }
        if (groundForward)
        {
            if (curState==CharStates.moving)
            {
                Vector3 p1 = transform.position;
                Vector3 p2 = hit.point;
                float diff = p1.y - p2.y;
                groundAngle = diff;
            }
            float targetIncline = 0;
            if (Mathf.Abs(groundAngle) > 0.3f)
            {
                if (groundAngle > 0)
                {

                    targetIncline = -1;
                }
                else 
                {
                    targetIncline = 1;
                }
            }
            if(groundAngle == 0){
                targetIncline = 0;
            }
            anim.SetFloat(Statics.incline, targetIncline, 0.1f, Time.deltaTime);
        }
    }

    private void ClimbOver(RaycastHit hit, ref bool willClimb, ClimbCheckType type)
    {
        float targetDistance = distanceToCheckForward + 0.1f;
        if (run)
            targetDistance += 0.5f;
        Vector3 climCheckOrigin = transform.position;
        switch (type)
        {
            case ClimbCheckType.walk_up:
                climCheckOrigin += Vector3.up * Statics.walkUpHeight;
                break;
            case ClimbCheckType.climb_up:
                climCheckOrigin += Vector3.up * Statics.climbMaxHeight;
                break;
        }
        RaycastHit climbHit;
        Vector3 wllDirection = -hit.normal * targetDistance;
        Debug.DrawRay(climCheckOrigin, wllDirection, Color.yellow);
        if(Physics.Raycast(climCheckOrigin,wllDirection,out climbHit, ignorelayers))
        {
        }
        else
        {
            Vector3 origin2 = hit.point;
            origin2.y = transform.position.y;
            switch (type)
            {
                case ClimbCheckType.walk_up:
                    origin2 += Vector3.up * Statics.walkUpHeight;
                    break;
                case ClimbCheckType.climb_up:
                    origin2 += Vector3.up * Statics.climbMaxHeight;
                    break;
            }
            
            origin2 += wllDirection * 0.2f;
            Debug.DrawRay(origin2, -Vector3.up, Color.yellow);
            if(Physics.Raycast(origin2,-Vector3.up,out climbHit, 1f,ignorelayers))
            {
                float diff = climbHit.point.y - transform.position.y;
                if (Math.Abs(diff) > Statics.walkUpThreshold)
                {
                    vaulting = true;
                    targetVaultPosition = climbHit.point ;
                    obsTacleForward = false;
                    willClimb = true;
                    skipGroundCheck = true;
                    switch (type)
                    {
                        case ClimbCheckType.walk_up:
                            CurValutType = VaultType.walk_up;
                            break;
                        case ClimbCheckType.climb_up:
                            CurValutType = VaultType.climp_up;
                            Vector3 startPos = hit.normal * Statics.climbUpStartPosOffset;
                            startPos = hit.point + startPos;
                            startPos.y = transform.position.y;
                            startVaultPosition = startPos;
                            float Climbdiff = targetVaultPosition.y - transform.position.y;
                            if (Mathf.Abs(Climbdiff) > 1.7)
                            {
                                climbAnimName = Statics.climb_up;
                            }
                            else
                            {
                                climbAnimName = Statics.climb_up_medium;
                            }
                            break;
                    }
                    return;
                }
            }
        }
    }

    private void canVaultOver(RaycastHit hit, ref bool willVault)
    {
        if (!onLocomotion || !inAngle_MoveDir)
            return;
        Vector3 wallDirection = -hit.normal * 0.5f;
        RaycastHit vHit;
       

        Vector3 wallOrigin = transform.position + Vector3.up * vaultOverHeight;
        Debug.DrawRay(wallOrigin, wallDirection * Statics.vaultCheckDistance, Color.red);
        if(Physics.Raycast(wallOrigin, wallDirection, out vHit, Statics.vaultCheckDistance, ignorelayers))
        {
            willVault = false;
            return;
        }
        else
        {
            if(canVault && !vaulting)
            {
                Vector3 startOrigin = hit.point;
                startOrigin.y = transform.position.y;
                Vector3 vOrigin = startOrigin + Vector3.up * vaultOverHeight;
                if (!run)
                    vOrigin += wallDirection * Statics.vaultCheckDistance;
                else
                    vOrigin += wallDirection * Statics.vaultCheckDistance_Run;
                Debug.DrawRay(vOrigin,- Vector3.up * Statics.vaultCheckDistance);
                if(Physics.Raycast(vOrigin,-Vector3.up,out vHit, Statics.vaultCheckDistance, ignorelayers))
                {
                    float hitY = vHit.point.y;
                    float diff = hitY - transform.position.y;
                    if (Mathf.Abs(diff) < vaultFloorHeightDifference)
                    {
                        vaulting = true;
                        targetVaultPosition = vHit.point;
                        willVault = true;
                        return;
                    }
                }
            }
        }
    }

    private bool OnGround()
    {
        if (skipGroundCheck)
            return false;
        bool r = false;
        if (curState == CharStates.hold)
            return false;
        Vector3 origin = transform.position + (Vector3.up * 0.55f);

        RaycastHit hit = new RaycastHit();
        bool isHit = false;
        FindGround(origin, ref hit, ref isHit);
        if (!isHit)
        {
            for(int i = 0; i < 4; i++)
            {
                Vector3 neworigin = origin;
                switch (i)
                {
                    case 0:
                        neworigin += Vector3.forward/3;
                        break;
                    case 1:
                        neworigin -= Vector3.forward / 3;
                        break;
                    case 2:
                        neworigin -= Vector3.right / 3;
                        break;
                    case 3:
                        neworigin += Vector3.right / 3;
                        break;
                    
                }
                FindGround(neworigin, ref hit, ref isHit);
                if (isHit == true)
                {
                    break;
                }
            }
        }
        r = isHit;
        if (r != false)
        {
            Vector3 targetPosition = transform.position;
            targetPosition.y = hit.point.y + groundOffset;
            transform.position = targetPosition;
        }
        return r;
    }

    private void FindGround(Vector3 origin, ref RaycastHit hit, ref bool isHit)
    {
        Debug.DrawRay(origin, -Vector3.up * 0.5f, Color.red);
        if(Physics.Raycast(origin,-Vector3.up,out hit, groundDistance, ignorelayers))
        {
            isHit = true;
        }
    }

    public Rigidbody rBody;
    #endregion

    #region Variables;
    public Vector3 moveDirection;

    public void RegularTick()
    {
        onGround = OnGround();
    }

    public float airTime;
    public bool prevGround;
    #endregion

    LayerMask ignorelayers;

   

    

    #region Init Phase
    public void init()
    {
        inGame = true;
        CreateModel();
        SetupAnimator();
        AddControllerReference();
        canJump = true;
        gameObject.layer = 8;
        ignorelayers = ~(1 << 3 | 1 << 8);

        contollerCollider = GetComponent<Collider>();
        if (contollerCollider == null)
        {
            Debug.Log("No Collider Found!!");
        }
    }
    #endregion
    private void AddControllerReference()
    {
        gameObject.AddComponent<Rigidbody>();
        rBody = GetComponent<Rigidbody>();
        rBody.angularDrag = 999;
        rBody.drag = 4;
        rBody.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;
    }

    private void SetupAnimator()
    {
        anim = GetComponent<Animator>();
        Animator chilAnim = activeModel.GetComponent<Animator>();
        anim.avatar = chilAnim.avatar;
        Destroy(chilAnim);
    }

    private void CreateModel()
    {
        activeModel = Instantiate(modelPrefab[slot]) as GameObject;
        activeModel.transform.parent = this.transform;
        activeModel.transform.localPosition = PlayerGFX.localPosition;
        activeModel.transform.localEulerAngles = Vector3.zero;
        activeModel.transform.localScale = Vector3.one;
        Destroy(PlayerGFX.gameObject);
    }

    public void LegFront()
    {
        Vector3 ll = anim.GetBoneTransform(HumanBodyBones.LeftFoot).position;
        Vector3 rl = anim.GetBoneTransform(HumanBodyBones.RightFoot).position;
        Vector3 rel_ll = transform.InverseTransformPoint(ll);
        Vector3 rel_rl = transform.InverseTransformPoint(rl);

        bool left = rel_ll.z > rel_rl.z;
        anim.SetBool(Statics.mirrorJump, left);

        

    }
    
}

