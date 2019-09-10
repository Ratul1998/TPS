using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandeler : MonoBehaviour {
    Animator animator;
    SoundController sc;
    AI enAI;

    public bool isAI;

    [System.Serializable]
    public class UserSettings
    {
        public Transform rightHand;
        public Transform lefthand;
        public Transform rightHandnades;
        public Transform pistolUnEquipSlot;
        public Transform rifleUnequipSlot;
        public Transform nadesUnequipSlot;
        public Transform SmokeNadesUnequipSlot;
    }
    [SerializeField]
    public UserSettings userSettings;

    [System.Serializable]
    public class Animations
    {
        public string weaponType = "weaponType";
        public string reloadingBool = "isReloading";
        public string aimingBool = "Aiming";
        public string nadetype = "nadeType";
    }
    [SerializeField]
    public Animations animations;

    public Weapons currentWeapon;
    public CharecterMovement ch;
    public Throwables nades;
    public List<Weapons> weaponList = new List<Weapons>();
    public List<Throwables> throwableList = new List<Throwables>();
    public int maxThrowables = 6;
    public int maxWeapons = 2;
    public bool aim;
    public bool reload;
    public int weapontype;
    public bool nadeEquiped;
    bool settingWeapon;

    private void Start()
    {
        ch = GetComponent<CharecterMovement>();
    }

    void OnEnable () {
        animator = GetComponent<Animator>();

        sc = GameObject.FindGameObjectWithTag("Sound Controller").GetComponent<SoundController>();

        SetUpWeapons();
    }

	void SetUpWeapons()
    {
        if (currentWeapon && !nades)
        {

            currentWeapon.SetEquipped(true);
            currentWeapon.SetOwner(this);
            AddWeapontoList(currentWeapon);
            if (currentWeapon.ammo.clipAmmo <= 0)
                Reload();
            if (reload)
                if (settingWeapon)
                    reload = false;
        }
        if (weaponList.Count > 0)
        {
            for (int i = 0; i < weaponList.Count; i++)
            {
                if (weaponList[i] != currentWeapon)
                {
                    weaponList[i].SetEquipped(false);
                    weaponList[i].SetOwner(this);
                }
            }
        }
    }
	
	void Update () {

        SetUpWeapons();
        if (!isAI)
        {
            if (currentWeapon)
            {
                animator.SetBool("WeaponEquiped", true);
                animator.SetFloat("WeaponRES", 0.5f);
            }
            else
            {
                animator.SetBool("WeaponEquiped", false);
                animator.SetFloat("WeaponRES", 0f);
            }
        }
        if (nades)
            animator.SetBool("nadeEquiped", true);
        else
            animator.SetBool("nadeEquiped", false);
        if (nades && !currentWeapon)
        {

            nades.SetEquipped(true);
            nades.SetOwner(this);
            AddNadesList(nades);
            nades.ownerAiming = aim;
        }
        if(throwableList.Count>0)
        {
            for (int i = 0; i < throwableList.Count; i++)
            {
                throwableList[i].SetEquipped(false);
                throwableList[i].SetOwner(this);
            }
        }
        Animate();
		
	}

    void Animate()
    {
        if (!animator)
            return;
        animator.SetBool(animations.aimingBool, aim);
        animator.SetBool(animations.reloadingBool, reload);
        animator.SetInteger(animations.weaponType, weapontype);
        if (!currentWeapon)
        {
            weapontype = 0;
            return;
        }
        switch (currentWeapon.weaponType)
        {
            case Weapons.WeaponType.Pistol:
                weapontype = 1;
                break;
            case Weapons.WeaponType.Rifle:
                weapontype = 2;
                break;
            case Weapons.WeaponType.Meele:
                weapontype = 3;
                break;
        }
       
    }

    void AddWeapontoList(Weapons weapon)
    {
        if (weaponList.Contains(weapon))
            return;
        weaponList.Add(weapon);

    }

    void AddNadesList(Throwables nade)
    {
        if (throwableList.Contains(nade))
            return;
        throwableList.Add(nade);
    }

    public void ThrowGranade(bool pulling)
    {
        if (!nades)
            return;
        nades.PullTrigger(pulling && aim);
    }

    public void Reload()
    {
        if (reload || !currentWeapon)
            return;
        if (currentWeapon.ammo.carryingAmmo <= 0 || currentWeapon.ammo.clipAmmo == currentWeapon.ammo.maxClipAmmo)
            return;

        if (sc != null)
        {
            if (currentWeapon.sounds.reloadSound != null)
            {
                if (currentWeapon.sounds.audioS != null)
                {
                    sc.PlaySound(currentWeapon.sounds.audioS, currentWeapon.sounds.reloadSound, true, currentWeapon.sounds.pitchMin, currentWeapon.sounds.pitchMax);
                }
            }
        }
        reload = true;
        StartCoroutine(StopReload());
    }

    IEnumerator StopReload()
    {
        yield return new WaitForSeconds(currentWeapon.weaponSettings.reloadDuration);
        currentWeapon.LoadClip();
        reload = false;
    }

    public void Aim(bool aiming)
    {
        aim = aiming;
    }

    public void DropCurrentWeapon()
    {
        if (!currentWeapon)
            return;
        if (currentWeapon.weaponType == Weapons.WeaponType.Meele)
            return;
        currentWeapon.GetComponent<WeaponPickup>().enabled = true;
        currentWeapon.SetEquipped(false);
        currentWeapon.SetOwner(null);
        weaponList.Remove(currentWeapon);
        currentWeapon = null;

    }

    public void switchWeapons()
    {
        if (settingWeapon||weaponList.Count==0)
            return;
        if (currentWeapon && !nades)
        {
            int currentWeaponIndex = weaponList.IndexOf(currentWeapon);
            int nextWeaonIndex = (currentWeaponIndex + 1) % weaponList.Count;
            currentWeapon = weaponList[nextWeaonIndex];
        }
        else if(nades && !currentWeapon)
        {
            UnequipCurrentNade();
            currentWeapon = weaponList[0];
        }
        else 
        {
            currentWeapon = weaponList[0];
        }
        settingWeapon = true;
        StartCoroutine(StopSettinWeapon());
    }

    public void switchNades()
    {
        if (settingWeapon || throwableList.Count == 0)
            return;
        if (nades && !currentWeapon)
        {
            int currentNadeIndex = throwableList.IndexOf(nades);
            int nextWeaonIndex = (currentNadeIndex + 1) % throwableList.Count;
            nades = throwableList[nextWeaonIndex];
        }
        else if(currentWeapon && !nades)
        {
            UnequipCurrentWeapon();
            nades = throwableList[0];
        }
        else
        {
            nades = throwableList[0];
        }
        settingWeapon = true;
        StartCoroutine(StopSettinWeapon());
    }

    IEnumerator StopSettinWeapon()
    {
        yield return new WaitForSeconds(0.7f);
        settingWeapon = false;
    }

    private void OnAnimatorIK(int Layer)
    {
        if (!animator)
            return;
        if (currentWeapon)
        {
            if (currentWeapon.userSetting.leftHandIKTarget && weapontype == 2 && !reload && !settingWeapon && !isAI)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                Transform target = currentWeapon.userSetting.leftHandIKTarget;
                Vector3 targetpos = target.position;
                Quaternion targetrot = target.rotation;
                animator.SetIKPosition(AvatarIKGoal.LeftHand, targetpos);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, targetrot);
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
            }
            
        }
    }

    public void UnequipCurrentWeapon()
    {
        if (!currentWeapon)
            return;
        currentWeapon.SetEquipped(false);
        currentWeapon = null;
    }

    public void UnequipCurrentNade()
    {
        if (!nades)
            return;
        nades.SetEquipped(false);
        nades = null;
    }
}
