using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Collections;

public class UserInput : MonoBehaviour
{
    public CharecterMovement characterMove { get; protected set; }
    public WeaponHandeler weaponHandler { get; protected set; }
    public GameObject indicator;

    [System.Serializable]
    public class InputSettings
    {
        public string verticalAxis = "Vertical";
        public string horizontalAxis = "Horizontal";
        public string jumpButton = "Jump";
        public string reloadButton = "Reload";
        public string aimButton = "Fire2";
        public string fireButton = "Fire1";
        public string dropWeaponButton = "DropWeapon";
        public string switchWeaponButton = "SwitchWeapon";
        public string switchNadeBotton = "NadeSwitch";
        public string pickupWeapon = "PickupWeapon";
        public string crouch = "crouch";
    }
    [SerializeField]
    public InputSettings input;

    [System.Serializable]
    public class OtherSettings
    {
        public float lookSpeed = 5.0f;
        public float lookDistance = 30.0f;
        public bool requireInputForTurn = true;
        public LayerMask aimDetectionLayers;
    }
    [SerializeField]
    public OtherSettings other;

    public Camera TPSCamera;
    CameraRig rig;
    Camera miniMap;
    public Transform spine;
    public bool aiming=false;
    Weapons PC = null;
    bool reloading;
    Dictionary<Weapons, GameObject> crosshairPrefabMap = new Dictionary<Weapons, GameObject>();
    Animator anim;
    PlayerRange playerRange;
    Ray ray;
    NavMeshAgent navMesh;
    bool setdestination = false;
    bool stableAim = false;
    bool initCheck = false;
    CoverBehaviour nextCover;
    Vector3 point;
    // Use this for initialization
    void Start()
    {
        characterMove = GetComponent<CharecterMovement>();
        weaponHandler = GetComponent<WeaponHandeler>();
        anim = GetComponent<Animator>();
        rig = CameraRig.GetInstance();
        playerRange = GetComponentInChildren<PlayerRange>();
        navMesh = GetComponent<NavMeshAgent>();
        navMesh.enabled = false;
        miniMap = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        reloading = anim.GetBool("isReloading");
        UpdateCrossHair();
        CharacterLogic();
        CameraLookLogic();
        WeaponLogic();
        NadeLogic();
    }

    void LateUpdate()
    {
        if (weaponHandler)
        {
            if (weaponHandler.currentWeapon)
            {
                if (aiming) {
                    PositionSpine();
                }
            }
        }
        //miniMap.transform.rotation = Quaternion.Euler(90f, transform.eulerAngles.y, 0f);
    }

    //Handles character logic
    void CharacterLogic()
    {
        if (!characterMove)
            return;
        if (characterMove.coverSettings.inCover || characterMove.coverSettings.changeCover)
            ChangeCover();
        if (Input.GetKey(KeyCode.LeftShift))
            characterMove.GetComponent<Animator>().SetBool("Run", true);
        else
        {
            characterMove.animator.SetBool("Run", false);
        }
        if(!characterMove.coverSettings.changeCover)
            characterMove.Animate(Input.GetAxis(input.verticalAxis), Input.GetAxis(input.horizontalAxis));
        if (Input.GetButtonDown(input.jumpButton) && characterMove.animator.GetBool("Run"))
            characterMove.Jump();
        else if (Input.GetButtonDown(input.jumpButton) && !characterMove.animator.GetBool("Run"))
            characterMove.animator.CrossFade("jump_Idle", 0f);
        if (Input.GetButtonDown(input.crouch))
        {
            characterMove.porne = false;
            characterMove.animator.SetBool("isPorne", characterMove.porne);
            characterMove.Crouching();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            characterMove.crouching = false;
            characterMove.animator.SetBool("isCrouching", characterMove.crouching);
            characterMove.Porne();
        }
    }

    //Handles camera logic
    void CameraLookLogic()
    {
        if (!TPSCamera)
            return;

        other.requireInputForTurn = !aiming;

        if (other.requireInputForTurn)
        {
            if (Input.GetAxis(input.horizontalAxis) != 0 || Input.GetAxis(input.verticalAxis) != 0)
            {
                CharacterLook();
            }
        }
        else
        {
            CharacterLook();
        }
    }

    private void NadeLogic()
    {
        if (!weaponHandler)
            return;
        if(characterMove.coverSettings.canAim)
            aiming = Input.GetButton(input.aimButton);

        weaponHandler.Aim(aiming);
        other.requireInputForTurn = !aiming;
        if (weaponHandler.nades != null)
        {
            weaponHandler.ThrowGranade(aiming);
        }

        if (Input.GetButtonDown(input.switchNadeBotton) && !reloading)
        {
            weaponHandler.switchNades();
        }
        if (Input.GetButtonDown("UnequipNade"))
            weaponHandler.UnequipCurrentNade();
        if (!weaponHandler.nades)
            return;

        weaponHandler.nades.shootRay = new Ray(TPSCamera.transform.position, TPSCamera.transform.forward);
    }

    //Handles all weapon logic
    private void WeaponLogic()
    {
        if (!weaponHandler)
            return;

        if (characterMove.coverSettings.canAim)
        {
            aiming = Input.GetButton(input.aimButton);
            rig.aiming = aiming;
            if (!initCheck)
            {
                if (aiming)
                {
                    initCheck = true;
                    stableAim = true;
                }
                else
                {
                    stableAim = false;
                    initCheck = false;
                }
            }
        }
        weaponHandler.Aim(aiming);
        if (Input.GetButtonDown(input.switchWeaponButton) && !reloading)
            weaponHandler.switchWeapons();
        if (weaponHandler.currentWeapon)
        {
            Ray aimRay = new Ray(TPSCamera.transform.position, TPSCamera.transform.forward);

            Debug.DrawRay(aimRay.origin, aimRay.direction);

            if (weaponHandler.currentWeapon.weaponType.GetHashCode() == 0 && Input.GetButtonDown(input.fireButton) && !weaponHandler.reload && aiming)
                weaponHandler.currentWeapon.Fire(aimRay);
            else if (weaponHandler.currentWeapon.weaponType.GetHashCode() == 1 && Input.GetButton(input.fireButton) && !weaponHandler.reload && aiming)
                weaponHandler.currentWeapon.Fire(aimRay);

            if (Input.GetButtonDown(input.reloadButton))
                weaponHandler.Reload();
            if (Input.GetButtonDown(input.dropWeaponButton) && !weaponHandler.reload && !aiming)
            {
                DeleteCrosshair(weaponHandler.currentWeapon);
                ToggleCrosshair(false, weaponHandler.currentWeapon);
                weaponHandler.DropCurrentWeapon();
            }
            if (Input.GetButtonDown("UnequipWeapon") && !reloading)
                weaponHandler.UnequipCurrentWeapon();
            if (!weaponHandler.currentWeapon)
                return;

            if (aiming)
            {
                PositionCrosshair(aimRay, weaponHandler.currentWeapon);
            }
            else
            {
                ToggleCrosshair(false, weaponHandler.currentWeapon);
            }
        }
        else
        {
            TurnOffAllCrosshairs();
        }
    }

    void TurnOffAllCrosshairs()
    {
        foreach (Weapons wep in crosshairPrefabMap.Keys)
        {
            ToggleCrosshair(false, wep);
        }
    }

    public void CreateCrosshair(Weapons wep)
    {
        GameObject prefab = wep.weaponSettings.CrossHair;
        if (prefab != null)
        {
            prefab = Instantiate(prefab);
            crosshairPrefabMap.Add(wep, prefab);
            ToggleCrosshair(false, wep);
        }
    }

    void DeleteCrosshair(Weapons wep)
    {
        if (!crosshairPrefabMap.ContainsKey(wep))
            return;

        Destroy(crosshairPrefabMap[wep]);
        crosshairPrefabMap.Remove(wep);
    }

    // Position the crosshair to the point that we are aiming
    void PositionCrosshair(Ray ray, Weapons wep)
    {
        Weapons curWeapon = weaponHandler.currentWeapon;
        if (curWeapon == null)
            return;
        if (!crosshairPrefabMap.ContainsKey(wep))
            return;
        AI nearEnemy = playerRange.ClosestEnemy();
        GameObject crosshairPrefab = crosshairPrefabMap[wep];
        RaycastHit hit;
        Transform bSpawn = curWeapon.weaponSettings.bulletSpwan;
        Vector3 bSpawnPoint = bSpawn.position;
        Vector3 dir = Vector3.zero;
        if (nearEnemy != null && stableAim)
        {
            dir = nearEnemy.transform.position + Vector3.up * 1.1f - bSpawnPoint;
            if (Input.GetAxis("Mouse X") > 0.5f || Input.GetAxisRaw("Mouse Y") > 0.5f)
                stableAim = false;
            
        }
        else if(nearEnemy==null || !stableAim)
            dir = ray.GetPoint(curWeapon.weaponSettings.range) - bSpawnPoint;
        if (Physics.Raycast(bSpawnPoint, dir, out hit, curWeapon.weaponSettings.range, other.aimDetectionLayers))
        {
            if (crosshairPrefab != null)
            {
                ToggleCrosshair(true, curWeapon);
                crosshairPrefab.transform.position = hit.point;
                crosshairPrefab.transform.LookAt(Camera.main.transform);
                spine.LookAt(hit.point);
            }
        }
        else
        {
            ToggleCrosshair(false, curWeapon);
        }
    }

    // Toggle on and off the crosshair prefab
    void ToggleCrosshair(bool enabled, Weapons wep)
    {
        if (!crosshairPrefabMap.ContainsKey(wep))
            return;

        crosshairPrefabMap[wep].SetActive(enabled);
    }

    //Postions the spine when aiming
    void PositionSpine()
    {
        if (!spine || !weaponHandler.currentWeapon || !TPSCamera)
            return;

        Transform mainCamT = TPSCamera.transform;
        Vector3 mainCamPos = mainCamT.position;
        Vector3 dir = mainCamT.forward;
        Ray ray = new Ray(mainCamPos, dir);

        spine.LookAt(ray.GetPoint(50));

        Vector3 eulerAngleOffset = weaponHandler.currentWeapon.userSetting.spineRotation;
        spine.Rotate(eulerAngleOffset);
    }

    //Make the character look at a forward point from the camera
    void CharacterLook()
    {
        Transform mainCamT = TPSCamera.transform;
        Transform pivotT = mainCamT.parent.parent;
        Vector3 pivotPos = pivotT.position;
        Vector3 lookTarget = pivotPos + (pivotT.forward * other.lookDistance);
        Vector3 thisPos = transform.position;
        Vector3 lookDir = lookTarget - thisPos;
        Quaternion lookRot = Quaternion.LookRotation(lookDir);
        if (!characterMove.swim.inWater || (characterMove.swim.inWater && !characterMove.swim.UnderWater))
        {
            lookRot.x = 0;
            lookRot.z = 0;
            Quaternion newRotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * other.lookSpeed);
            transform.rotation = newRotation;
        }
        else if(characterMove.swim.inWater && characterMove.swim.UnderWater)
        {
            Quaternion newRotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * other.lookSpeed);
            transform.rotation = newRotation;
        }

       
    }

    void UpdateCrossHair()
    {
        if (weaponHandler.weaponList.Count == 0)
            return;
        if (PC != weaponHandler.currentWeapon)
        {
            foreach (Weapons wep in weaponHandler.weaponList)
            {
                if (wep == weaponHandler.currentWeapon)
                {
                    CreateCrosshair(wep);
                }
                else
                    DeleteCrosshair(wep);
            }
            PC = weaponHandler.currentWeapon;
        }
    }

    void ChangeCover()
    {
        ray = new Ray(TPSCamera.transform.position, TPSCamera.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction, Color.black);
        RaycastHit hit;
        if (!characterMove.coverSettings.changeCover)
        {
            if (Physics.Raycast(ray.origin, ray.direction, out hit, 20f, characterMove.coverSettings.layerMask))
            {
                if (hit.transform.GetComponentInParent<CoverBehaviour>() && !hit.transform.GetComponentInParent<CoverBehaviour>().AICover )
                {
                    if (!characterMove.ignoreCover.Contains(hit.transform.GetComponentInParent<CoverBehaviour>()))
                    {
                        indicator.transform.position = hit.point;
                        indicator.transform.rotation = Quaternion.LookRotation(hit.normal);
                        indicator.SetActive(true);
                        if (Input.GetKeyDown(KeyCode.H))
                        {
                            navMesh.enabled = true;
                            characterMove.animator.applyRootMotion = false;
                            nextCover = hit.transform.GetComponentInParent<CoverBehaviour>();
                            point = hit.point;
                            if (characterMove.coverSettings.inCover)
                            {
                                characterMove.coverSettings.inCover = false;
                                StartCoroutine(characterMove.ClearIgnoreList());
                                characterMove.coverSettings.coverPos = null;
                                characterMove.coverSettings.canAim = true;
                                characterMove.animator.SetBool("Cover", characterMove.coverSettings.inCover);
                            }
                            characterMove.coverSettings.changeCover = true;
                        }
                    }
                }
                else
                {
                    indicator.SetActive(false);
                }
            }
        }
        else
        {
            indicator.SetActive(false);
            if (!setdestination)
            {
                navMesh.SetDestination(point);
                setdestination = true;
            }
            float distance = Vector3.Distance(transform.position,point);
            if (distance <= 2f)
            {
                setdestination = false;
                characterMove.animator.SetFloat("Forward", 0f);
                navMesh.enabled = false;
                characterMove.animator.applyRootMotion = true;
                characterMove.GetInCover(nextCover);
            }
            else
            {
                navMesh.enabled=true;
                characterMove.animator.SetFloat("Forward", 1f);
                characterMove.animator.applyRootMotion = false;
            }
        }
    }

}
