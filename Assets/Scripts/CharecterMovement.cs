using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class CharecterMovement : MonoBehaviour {
    public Animator animator;
    WeaponHandeler wp;
    CharacterController characterController;
    public List<CoverBehaviour> ignoreCover = new List<CoverBehaviour>();
    public GameObject CameraRig;
    [System.Serializable]
    public class AnimationSettings
    {
        public string verticalVelocityFloat = "Forward";
        public string horizontalVelocityFloat = "Strafe";
        public string groundedBool = "isGrounded";
        public string jumpBool = "isJumping";
        public string crouchBool = "isCrouching";
        public string porneBool = "isPorne";
    }
    [SerializeField]
    public AnimationSettings animations;

    [System.Serializable]
    public class PhysicsSettings
    {
        public float gravityModifier = 9.81f;
        public float baseGravity = 50.0f;
        public float resetGravityValue = 1.2f;
        public LayerMask groundLayers;
        public LayerMask WaterLayers;
        public float airSpeed = 2.5f;
    }
    [SerializeField]
    public PhysicsSettings physics;

    [System.Serializable]
    public class MovementSettings
    {
        public float jumpSpeed = 6;
        public float jumpTime = 0.25f;
    }
    [System.Serializable]
    public class CoverSettings
    {
        public float stance;
        public float coverPercentage;
        public bool inCover;
        public CoverBehaviour coverPos;
        public int coverDirection=1;
        public bool canAim=true;
        public LayerMask layerMask;
        public float coverAcceleration=0.5f;
        public float coverMaxSpeed=2f;
        public CoverBehaviour currentCover;
        public bool changeCover;
    }
    public CoverSettings coverSettings;
    [SerializeField]
    public MovementSettings movement;
    [System.Serializable]
    public class Swimming
    {
        public bool inWater = false;
        public bool UnderWater = false;
        public Camera TPSCamera;
        public float WaterSurfacePosition = 0;
        public Transform waterSurface;
        public AudioSource SwimmingAudio;
        public float Gravity = 3f;
        public float Damping = 1f;
        public float NormatSpeed = 2f;
        public float FastSpeed = 4f;
        public float SideWaySpeed = 1.5f;
        public float BackSpeed = 1f;
        public float UpSpeed = 10f;


    }
    public Swimming swim;
    Vector3 airControl;
    float forward;
    float strafe;
    bool jumping;
    bool resetGravity;
    float gravity;
    public Vector3 moveDir;
    public bool crouching;
    public bool porne;
    public bool vaulting;
    public bool climb;
    public bool isAI;
    public BezierCurve vaultcurve;
    Vector3 curvePos;
    float percentage;
    bool ignorevault;
    bool initvault;
    public Transform t;
    bool isGrounded()
    {
        if (swim.inWater)
            return true;
        else
        {
            RaycastHit hit;
            Vector3 start = transform.position + transform.up;
            Vector3 dir = Vector3.down;
            Debug.DrawRay(start, dir);
            float radius = characterController.radius;
            if (Physics.SphereCast(start, radius, dir, out hit, characterController.height / 2, physics.groundLayers))
            {
                return true;
            }

            return false;
        }
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
        SetupAnimator();
    }

    // Use this for initialization
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        wp = GetComponent<WeaponHandeler>();
    }

    // Update is called once per frame
    void Update()
    {
        swim.inWater = animator.GetBool("OnWater");
        if (!swim.inWater)
            swim.UnderWater = false;
        else if (swim.inWater && Input.GetKeyDown(KeyCode.LeftShift))
        {
            swim.UnderWater = true;
        }
        else if(swim.inWater && Input.GetKeyUp(KeyCode.LeftShift))
        {
            swim.UnderWater = false;
        }
        AirControl(forward, strafe);
        if(!swim.inWater && !vaulting)
            ApplyGravity();
        if (!coverSettings.inCover && !coverSettings.changeCover)
            SearchForCover();
        if (swim.inWater && !isAI)
        {
            wp.currentWeapon = null;
            if (swim.UnderWater)
            {
                characterController.enabled = false;
                float y = transform.position.y;
                y = Mathf.Clamp(y, 5, 48);
                transform.position = new Vector3(transform.position.x, y, transform.position.z);
            }
            else
            {
                characterController.enabled = false;
                float y = swim.waterSurface.position.y - 1.1f;
                if (transform.position.y < y-0.5f)
                {
                    transform.Translate(Vector3.up * Time.deltaTime * 5f);
                }
                else
                {   
                    transform.position = new Vector3(transform.position.x, y, transform.position.z);
                }
            }
            Vector3 start = transform.position + transform.up ;
            Vector3 dir = transform.forward;
            Ray ray = new Ray(start, dir);
            Debug.DrawRay(start,dir,Color.red);
            float radius = characterController.radius;
            if (Physics.SphereCast(ray, radius*2 ,characterController.height/2, physics.groundLayers))
            {
                if (!swim.UnderWater)
                {
                    animator.SetBool("OnWater", false);
                    characterController.enabled = true;
                }
                else
                {
                    animator.SetFloat("Forward", 0f);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        HandleVault();
    }
    //Animates the character and root motion handles the movement
    public void Animate(float forward, float strafe)
    {
        this.forward = forward;
        this.strafe = strafe;
        if (coverSettings.inCover)
        {
            animator.SetFloat(animations.verticalVelocityFloat,0f);
            animator.SetFloat(animations.horizontalVelocityFloat,0f);
            HandleCoverMovement(strafe);
            GetOutOfCover(forward);
            return;
        }
        animator.SetFloat(animations.verticalVelocityFloat, forward);
        animator.SetFloat(animations.horizontalVelocityFloat, strafe);
        animator.SetBool(animations.groundedBool, isGrounded());
        animator.SetBool(animations.jumpBool, jumping);
    }

    void AirControl(float forward, float strafe)
    {
        if (isGrounded() == false && !swim.inWater)
        {
            airControl.x = strafe;
            airControl.z = forward;
            airControl = transform.TransformDirection(airControl);
            airControl *= physics.airSpeed;

            characterController.Move(airControl * Time.deltaTime);
        }
    }

    public void Crouching()
    {
        crouching = !crouching;
        animator.SetBool(animations.crouchBool,crouching);

    }

    public void Porne()
    {
        porne = !porne;
        animator.SetBool(animations.porneBool,porne);
    }
    //Makes the character jump
    public void Jump()
    {
        if (jumping)
            return;

        if (isGrounded())
        {
            jumping = true;
            StartCoroutine(StopJump());
        }
    }

    //Stops us from jumping
    IEnumerator StopJump()
    {
        yield return new WaitForSeconds(movement.jumpTime);
        jumping = false;
    }

    //Applys downard force to the character when we aren't jumping
    void ApplyGravity()
    {
        if (!isGrounded())
        {
            if (!resetGravity)
            {
                gravity = physics.resetGravityValue;
                resetGravity = true;
            }
            gravity += Time.deltaTime * physics.gravityModifier;
        }
        else
        {
            gravity = physics.baseGravity;
            resetGravity = false;
        }

        Vector3 gravityVector = new Vector3();

        if (!jumping)
        {
            gravityVector.y -= gravity;
        }
        else
        {
            gravityVector.y = movement.jumpSpeed;
        }
       characterController.Move(gravityVector * Time.deltaTime);
    }

    //Setup the animator with the child avatar
    void SetupAnimator()
    {
        Animator wantedAnim = GetComponentsInChildren<Animator>()[1];
        Avatar wantedAvater = wantedAnim.avatar;

        animator.avatar = wantedAvater;
        Destroy(wantedAnim);
    }
    //Get in Cover
    public void GetInCover(CoverBehaviour cover)
    {
        ignoreCover.Add(cover);
        coverSettings.changeCover = false;
        coverSettings.coverPos = cover;
        if (coverSettings.coverPos)
        {
            if (coverSettings.coverPos.coverType == CoverBehaviour.CoverType.full)
            {
                coverSettings.canAim = false;
                animator.SetInteger("CoverType", 0);
            }
            else if (coverSettings.coverPos.coverType == CoverBehaviour.CoverType.half)
            {
                coverSettings.canAim = true;
                animator.SetInteger("CoverType", 1);
            }
        }
        float disFromPos1 = Vector3.Distance(transform.position, cover.curvepath.GetPointAt(0));
        coverSettings.coverPercentage = disFromPos1 / cover.length;
        Vector3 targetPos = cover.curvepath.GetPointAt(coverSettings.coverPercentage);
        StartCoroutine(LerpToCoverPositionPercentage(targetPos));
        coverSettings.inCover = true;
       
        animator.SetBool("Cover", coverSettings.inCover);

    }

    IEnumerator LerpToCoverPositionPercentage(Vector3 targetPos)
     {
        Vector3 startingPos = transform.position;
        Vector3 tPos = targetPos;
        targetPos.y = transform.position.y;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 5;
            transform.position = Vector3.Lerp(startingPos, tPos, t);
            yield return null;
        }
        
     }
    //Search For Cover
    public void SearchForCover()
    {
        Vector3 origin = transform.position + Vector3.up / 2;
        Vector3 direction = transform.forward;
        RaycastHit hit;
        if(Physics.Raycast(origin,direction,out hit, 2,coverSettings.layerMask))
        {
            float distance = Vector3.Distance(origin, hit.point);
            if (distance < 1f)
                if (hit.transform.GetComponentInParent<CoverBehaviour>())
                {
                    if (!hit.transform.GetComponentInParent<CoverBehaviour>().AICover)
                    {
                        if (!ignoreCover.Contains(hit.transform.GetComponentInParent<CoverBehaviour>()))
                        {
                            coverSettings.currentCover = hit.transform.GetComponentInParent<CoverBehaviour>();
                            if (Input.GetKey(KeyCode.LeftShift))
                            {
                                if (!vaulting)
                                {
                                    if (coverSettings.currentCover.coverType == CoverBehaviour.CoverType.half)
                                    {
                                        vault();
                                    }

                                }
                            }
                            else
                            {
                                GetInCover(hit.transform.GetComponentInParent<CoverBehaviour>());
                            }

                        }
                    }
                }

        }

    }

    private void vault()
    {
        BezierCurve curve = vaultcurve;
        curve.transform.position = transform.position;
        curve.transform.rotation = transform.rotation;

        string desiredAnimation =  "Vault";
        animator.CrossFade(desiredAnimation, 0);
        animator.CrossFade("Jumping", 0);
        curve.close = false;
        percentage = 0;
        vaulting = true;
    }

    void HandleVault()
    {
        if (vaulting)
        {
            characterController.enabled = false;
            BezierCurve curve =  vaultcurve;
            float linelength = curve.length;
            float speed = 6;
            float movement = speed * Time.deltaTime;
            float lerpMovement = movement / linelength;
            percentage += lerpMovement;
            if (percentage > 1)
            {
                vaulting = false;

            }
            Vector3 targetPosition = curve.GetPointAt(percentage);
            transform.position = targetPosition;
        }
        else
        {
            characterController.enabled = true;
        }
    }

    void HandleCoverMovement(float horizontal)
    {
        if (horizontal != 0)
        {
            if (horizontal < 0)
                coverSettings.coverDirection = -1;
            else
                coverSettings.coverDirection = 1;
        }
        animator.SetInteger("CoverDirection", coverSettings.coverDirection);
       
        float lineLength = coverSettings.coverPos.length;
        float movement = ((horizontal * coverSettings.coverAcceleration) * coverSettings.coverMaxSpeed) * Time.deltaTime;
        float lerpMovement = movement / lineLength;
        coverSettings.coverPercentage -= lerpMovement;
        coverSettings.coverPercentage = Mathf.Clamp01(coverSettings.coverPercentage);
        if (coverSettings.coverPercentage > 0.99f || coverSettings.coverPercentage == 0f)
        {
            if (coverSettings.coverPos.coverType == CoverBehaviour.CoverType.full)
                coverSettings.canAim = true;
            horizontal = 0f;
        }
        else
        {
            if (coverSettings.coverPos.coverType == CoverBehaviour.CoverType.full)
                coverSettings.canAim = false;
        }
        animator.SetFloat("Stance", Mathf.Abs(horizontal));
        Vector3 curvePathPosition = coverSettings.coverPos.curvepath.GetPointAt(coverSettings.coverPercentage);
        curvePathPosition.y = transform.position.y;
        HandleCoverRotation();
        transform.position = curvePathPosition;
    }

    private void HandleCoverRotation()
    {
        float forwardPerc = coverSettings.coverPercentage + 0.1f;
        if (forwardPerc > 0.98)
        {
            forwardPerc = 1;
        }
        Vector3 positionNow = coverSettings.coverPos.curvepath.GetPointAt(coverSettings.coverPercentage);
        Vector3 positionForward = coverSettings.coverPos.curvepath.GetPointAt(forwardPerc);
        Vector3 direction = Vector3.Cross(positionNow, positionForward);
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetaRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetaRotation, Time.deltaTime * 3f);
        }
    }

    public void GetOutOfCover(float vertical)
    {
        if (vertical<-0.5f)
        {
            GetComponent<UserInput>().indicator.SetActive(false);
            coverSettings.coverPos = null;
            coverSettings.inCover = false;
            coverSettings.canAim = true;
            animator.SetBool("Cover", coverSettings.inCover);
            StartCoroutine("ClearIgnoreList");
        }
    }

    public IEnumerator ClearIgnoreList()
    {
        yield return new WaitForSeconds(3);
        ignoreCover.Clear();
    }
}
