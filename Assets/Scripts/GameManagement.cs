using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManagement : MonoBehaviour {

    public static GameManagement GM;
    private UserInput player { get { return FindObjectOfType<UserInput>(); }set { player = value; } }
    private PlayerUI playerUI { get { return FindObjectOfType<PlayerUI>(); }set { playerUI = value; } }
    private WeaponHandeler wp { get { return FindObjectOfType<WeaponHandeler>(); } set { wp = value; } }
    private CharecterStats charecterStats { get { return FindObjectOfType<CharecterStats>(); } set { charecterStats = value; } }
    private void Awake()
    {
        if (GM == null)
        {
            GM = this;
        }
        else
        {
            if (GM != this)
                Destroy(gameObject);
        }
    }
    private void Update()
    {
       UpdateUI();
    }
    void UpdateUI()
    {
        if (player)
        {
            if (playerUI)
            {
                if (wp)
                {
                    if (wp.currentWeapon == null || wp.currentWeapon.weaponType==Weapons.WeaponType.Meele)
                    {
                        playerUI.ammoCount.text = "Unarmed";
                    }
                    else
                    {
                        playerUI.ammoCount.text = wp.currentWeapon.ammo.clipAmmo + "/" + wp.currentWeapon.ammo.carryingAmmo;
                    }
  
                }
                if(playerUI.HealthBar && playerUI.HealthText)
                {
                   playerUI.HealthBar.value = charecterStats.health;
                   playerUI.HealthText.text = Mathf.Round(playerUI.HealthBar.value).ToString();
                }
            }

        }
    }
}
