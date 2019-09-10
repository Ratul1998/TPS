using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructableObjects : MonoBehaviour {
    public GameObject Weapon;
    public float Health = 20f;
    public GameObject DestructableCreate;

    public void TakeDamage(float amt)
    {
        Health -= amt;
        if (Health <= 0)
        {
            GameObject Ins= Instantiate(DestructableCreate, transform.position, transform.rotation);
            GameObject WeaponIns = Instantiate(Weapon, transform.position, transform.rotation);
            WeaponIns.name = Weapon.name;
            Destroy(gameObject);
            Destroy(Ins, 2f);

        }
    }
}
