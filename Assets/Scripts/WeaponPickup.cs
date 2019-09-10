
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    WeaponHandeler owner;
    Weapons weapon;
    GameObject player;
    Transform interactableTransform;
    // Use this for initialization
    void Start()
    {

        weapon = GetComponent<Weapons>();
        owner = FindObjectOfType<WeaponHandeler>();
        player = FindObjectOfType<UserInput>().gameObject;
        if (interactableTransform == null)
            interactableTransform = transform;

    }

    // Update is called once per frame
    void Update()
    {
        Pickup();
    }

    private void OnDrawGizmos()
    {
        if (interactableTransform == null)
            interactableTransform = transform;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(interactableTransform.position, 2f);
    }
    void Pickup()
    {
        float Dist = Vector3.Distance(player.transform.position, interactableTransform.position);
        if (Dist < 2f)
        {

            if (Input.GetButtonDown("PickupWeapon"))
            {
                if (!owner)
                    return;
                else if (!owner.userSettings.rightHand)
                {
                    return;
                }

                if (owner.weaponList.Count != 0)
                {
                    for(int i = 0; i < owner.weaponList.Count; i++)
                    {
                        if (owner.weaponList[i].weaponType == weapon.weaponType)
                        {
                            owner.weaponList[i].ammo.carryingAmmo += 60;
                            Destroy(gameObject);
                            return;
                        }
                    }
                }
                transform.SetParent(owner.userSettings.rightHand);
                transform.localPosition =weapon. weaponSettings.equipPosition;
                Quaternion equipRot = Quaternion.Euler(weapon.weaponSettings.equipRotation);
                transform.localRotation = equipRot;
                weapon.SetEquipped(true);
                owner.weaponList.Add(gameObject.GetComponent<Weapons>());
                weapon.GetComponent<WeaponPickup>().enabled = false;
            }
        }
    }

}
