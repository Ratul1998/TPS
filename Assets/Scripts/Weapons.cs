
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Weapons : MonoBehaviour {
    Collider col;
    Rigidbody rb;
    Animator animator;
    SoundController sc;

    public enum WeaponType {
        Pistol, Rifle, Meele
    }
    public WeaponType weaponType;

    [System.Serializable]
    public class UserSetting
    {
        public Transform leftHandIKTarget;
        public Vector3 spineRotation;
    }

    [SerializeField]
    public UserSetting userSetting;

    [System.Serializable]
    public class WeaponSettings
    {
        [Header("Bullet Options")]
        public Transform bulletSpwan;
        public float damage = 30f;
        public float bulletSpeed = 5f;
        public float fireRate = 1f;
        public LayerMask bulletLayers;
        public float range = 200f;


        [Header("Effects")]
        public GameObject muzzleFlash;
        public GameObject decal;
        public GameObject shell;
        public GameObject clip;
        public GameObject BloodEffect;

        [Header("Other")]
        public float reloadDuration = 2f;
        public Transform shellEjectSpot;
        public float shellEjectSpeed = 7.5f;
        public Transform clipEjectPos;
        public GameObject clipGO;
        public GameObject CrossHair;


        [Header("Positioning")]
        public Vector3 equipPosition;
        public Vector3 equipRotation;
        public Vector3 unequipPosition;
        public Vector3 unequipRotation;

        [Header("Animation")]
        public bool useAnimation;
        public int fireAnimationLayer = 0;
        public string fireAnimationName = "Fire";
    }
    [SerializeField]
    public WeaponSettings weaponSettings;

    [System.Serializable]
    public class Ammunation
    {
        public int carryingAmmo;
        public int clipAmmo;
        public int maxClipAmmo;
    }
    [SerializeField]
    public Ammunation ammo;
    WeaponHandeler owner;
    PlayerRange playerRange;
    bool equiped;
    bool resetCartiage;

    [System.Serializable]
    public class SoundSettings
    {
        public AudioClip[] gunshotSounds;
        public AudioClip reloadSound;
        [Range(0, 3)] public float pitchMin = 1;
        [Range(0, 3)] public float pitchMax = 1.2f;
        public AudioSource audioS;
    }
    [SerializeField]
    public SoundSettings sounds;

    // Use this for initialization
    void Start () {
        col = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        sc = GameObject.FindGameObjectWithTag("Sound Controller").GetComponent<SoundController>();
        playerRange = PlayerRange.GetInstance();
	}
	
	// Update is called once per frame
	void Update () {
        if (owner)
        {
            DisableEnableComponents(false);
            if (equiped)
            {
                if (owner.userSettings.rightHand)
                {
                    Equip();
                }
            }
            else
            {
                if (weaponSettings.bulletSpwan.childCount > 0)
                {
                    foreach(Transform t in weaponSettings.bulletSpwan.GetComponentsInChildren<Transform>())
                    {
                        if (t != weaponSettings.bulletSpwan)
                        {
                            Destroy(t.gameObject);
                        }
                    }
                }
                UnEquip(weaponType);
            }
        }
        else
        {
            DisableEnableComponents(true);
            transform.SetParent(null);
        }
		
	}

    public void Fire(Ray ray)
    {
        if (ammo.clipAmmo <= 0 || resetCartiage || !weaponSettings.bulletSpwan)
        {
            return;
        }

        RaycastHit hit;
        AI nearEnemy = playerRange.ClosestEnemy();
        Transform bSpwan = weaponSettings.bulletSpwan;
        Vector3 bSpwanPoint = bSpwan.position;
        Vector3 dir = Vector3.zero;
        if (nearEnemy != null && !owner.isAI)
        {
            if (Input.GetAxis("Mouse X") > 0.2f || Input.GetAxisRaw("Mouse Y") > 0.2f)
            {
                dir = ray.GetPoint(weaponSettings.range) - bSpwanPoint;
            }
            else
                dir = nearEnemy.transform.position + Vector3.up * 1.1f - bSpwanPoint;
        }
        else if (nearEnemy == null || owner.isAI)
        {
            dir = ray.GetPoint(weaponSettings.range) - bSpwanPoint;
            dir += (Vector3)Random.insideUnitCircle * weaponSettings.bulletSpeed;
        }
        Debug.DrawRay(bSpwanPoint, dir,Color.red);
        if (Physics.Raycast(bSpwanPoint, dir ,out hit, weaponSettings.range, weaponSettings.bulletLayers))
        {
            hit.transform.SendMessage("TakeDamage", weaponSettings.damage,SendMessageOptions.DontRequireReceiver);
            HitEffects(hit);
        }

        GunEffects();

        if (weaponSettings.useAnimation)
        {
            animator.Play(weaponSettings.fireAnimationName, weaponSettings.fireAnimationLayer);
        }
        ammo.clipAmmo--;
        resetCartiage = true;
        StartCoroutine(LoadNextBullet());
    }

    IEnumerator LoadNextBullet()
    {
        yield return new WaitForSeconds(weaponSettings.fireRate);
        resetCartiage = false;
    }

    void DisableEnableComponents(bool enabed)
    {
        if (!enabed)
        {
            rb.isKinematic = true;
            col.enabled = false;
        }
        else
        {
            rb.isKinematic = false;
            col.enabled = true;
        }
    }
    //Equips this weapons to hands
    public void Equip()
    {
        if (!owner)
            return;
        else if(!owner.userSettings.rightHand)
        {
            return;
        }
        if (weaponType == WeaponType.Meele)
            transform.SetParent(owner.userSettings.lefthand);
        else
            transform.SetParent(owner.userSettings.rightHand);
        transform.localPosition = weaponSettings.equipPosition;
        Quaternion equipRot = Quaternion.Euler(weaponSettings.equipRotation);
        transform.localRotation = equipRot;
    }
    //Unequips
    void UnEquip(WeaponType wpType)
    {
        if (!owner)
            return;
        switch (wpType)
        {
            case WeaponType.Pistol:
                transform.SetParent(owner.userSettings.pistolUnEquipSlot);
                break;
            case WeaponType.Rifle:
                transform.SetParent(owner.userSettings.rifleUnequipSlot);
                break;
            case WeaponType.Meele:
                transform.SetParent(owner.userSettings.pistolUnEquipSlot);
                break;
        }
        transform.localPosition = weaponSettings.unequipPosition;
        Quaternion unequipRot = Quaternion.Euler(weaponSettings.unequipRotation);
        transform.localRotation = unequipRot;
    }
    //Loads the clip and Calculate the ammo
    public void LoadClip()
    {
        int ammoNeeded = ammo.maxClipAmmo - ammo.clipAmmo;
        if (ammoNeeded >= ammo.carryingAmmo)
        {
            ammo.clipAmmo = ammo.carryingAmmo;
            ammo.carryingAmmo = 0;
        }
        else
        {
            ammo.carryingAmmo -= ammoNeeded;
            ammo.clipAmmo = ammo.maxClipAmmo;
        }
    }

    //Set the weapons equip state
    public void SetEquipped(bool equip)
    {
        equiped = equip;
    }

    public void SetOwner(WeaponHandeler wp)
    {
        owner = wp;
    }

    void HitEffects(RaycastHit hit)
    {
        #region Effect
        if (hit.collider.gameObject.tag == "Player" || hit.collider.gameObject.tag == "Enemy")
        {
            if (weaponSettings.BloodEffect)
            {
                Vector3 hitPoint = hit.point;
                Quaternion LookRotation = Quaternion.LookRotation(hit.normal);
                GameObject blood = Instantiate(weaponSettings.BloodEffect, hitPoint, LookRotation) as GameObject;
                Transform bloodT = blood.transform;
                Transform hitT = hit.transform;
                bloodT.SetParent(hitT);
                Destroy(blood, 0.5f);
            }
        }
        else
        {
            if (weaponSettings.decal)
            {
                Vector3 hitPoint = hit.point;
                Quaternion LookRotation = Quaternion.LookRotation(hit.normal);
                GameObject decal = Instantiate(weaponSettings.decal, hitPoint, LookRotation) as GameObject;
                Transform decalT = decal.transform;
                Transform hitT = hit.transform;
                decalT.SetParent(hitT);
                Destroy(decal, 5f);
            }
        }
       


        #endregion
    }

    void GunEffects()
    {
        #region muzzleFlash
        if (weaponSettings.muzzleFlash)
        {
            Vector3 bulletSpwansPos = weaponSettings.bulletSpwan.position;
            GameObject muzzleFlash = Instantiate(weaponSettings.muzzleFlash, bulletSpwansPos, Quaternion.identity);
            Transform muzzleT = muzzleFlash.transform;
            muzzleT.SetParent(weaponSettings.bulletSpwan);
            Destroy(muzzleFlash, 0.5f);
        }
        #endregion

        #region Shell

        if (weaponSettings.shell)
        {
            if (weaponSettings.shellEjectSpot)
            {
                Vector3 shellEjectPos = weaponSettings.shellEjectSpot.position;
                Quaternion shellEjectRot = weaponSettings.shellEjectSpot.rotation;
                GameObject shell = Instantiate(weaponSettings.shell, shellEjectPos, shellEjectRot);
                if (shell.GetComponent<Rigidbody>())
                {
                    Rigidbody rigidB = shell.GetComponent<Rigidbody>();
                    rigidB.AddForce(weaponSettings.shellEjectSpot.forward * weaponSettings.shellEjectSpeed, ForceMode.Impulse);

                }
                Destroy(shell, Random.Range(1f,2f));
            }
        }

        #endregion

        PlayGunshotSound();

    }

    void PlayGunshotSound()
    {
        if (sc == null)
        {
            return;
        }

        if (sounds.audioS != null)
        {
            if (sounds.gunshotSounds.Length > 0)
            {
                sc.InstantiateClip(
                    weaponSettings.bulletSpwan.position, // Where we want to play the sound from
                    sounds.gunshotSounds[Random.Range(0, sounds.gunshotSounds.Length)],  // What audio clip we will use for this sound
                    1f, // How long before we destroy the audio
                    true, // Do we want to randomize the sound?
                    sounds.pitchMin, // The minimum pitch that the sound will use.
                    sounds.pitchMax); // The maximum pitch that the sound will use.
            }
        }
    }
   
   
}
