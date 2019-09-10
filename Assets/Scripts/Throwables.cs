using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwables : MonoBehaviour {

    Collider col;
    Rigidbody rb;
    WeaponHandeler owner;

    Granade g;

    public enum ThrowableType
    {
        Nades , SmokeNades
    }

    public ThrowableType throwableType;

    public Ray shootRay { get; set; }
    public bool ownerAiming { get; set; }

    public bool equiped;
    public bool pullingTrigger;

    [System.Serializable]
    public class Throwablesettings
    {
        [Header("Bullet Options")]
        public float damage = 200f;
        public LayerMask bulletLayers;
        public float range = 10f;


        [Header("Other")]
        public GameObject CrossHair;
        public Transform nadeSpwan;


        [Header("Positioning")]
        public Vector3 equipPosition;
        public Vector3 equipRotation;
        public Vector3 unequipPosition;
        public Vector3 unequipRotation;
    }
    [SerializeField]
    public Throwablesettings throwablesettings;

    void Start()
    {
        col = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        if (throwablesettings.CrossHair != null)
        {
            throwablesettings.CrossHair = Instantiate(throwablesettings.CrossHair);
            ToggleCrossHair(false);
        }
        g = GetComponent<Granade>();
    }

    private void Update()
    {
        if (owner)
        {
            DisableEnableComponents(false);
            if (owner.nades!=null)
            {
                EquipNade();
                if (pullingTrigger)
                {
                    ThrowNade();
                }
                if (ownerAiming)
                {
                    PositionCrosshair(shootRay);
                }
                else
                {
                    ToggleCrossHair(false);
                }
            }
            
            else
            {
                UnEquipNade(throwableType);
                ToggleCrossHair(false);
            }
        }
        else
        {
            ToggleCrossHair(false);
            DisableEnableComponents(true);
            transform.SetParent(null);
            ownerAiming = false;
        }
    }

    private void ThrowNade()
    {
        g.enabled = true;
        this.enabled = false;
        Destroy(throwablesettings.CrossHair);
    }

    public void SetEquipped(bool equip)
    {
        equiped = equip;
    }

    void ToggleCrossHair(bool enabled)
    {
        if (throwablesettings.CrossHair != null)
        {
            throwablesettings.CrossHair.SetActive(enabled);
        }
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

    public void EquipNade()
    {
        if (!owner)
            return;
        else if (!owner.userSettings.rightHandnades)
        {
            return;
        }
        transform.SetParent(owner.userSettings.rightHandnades);
        transform.localPosition = throwablesettings.equipPosition;
        Quaternion equipRot = Quaternion.Euler(throwablesettings.equipRotation);
        transform.localRotation = equipRot;
    }
    void UnEquipNade(ThrowableType NType)
    {
        if (!owner)
            return;
        switch (NType)
        {
            case ThrowableType.Nades:
                transform.SetParent(owner.userSettings.nadesUnequipSlot);
                break;
            case ThrowableType.SmokeNades:
                transform.SetParent(owner.userSettings.SmokeNadesUnequipSlot);
                break;
        }
        transform.localPosition = throwablesettings.unequipPosition;
        Quaternion unequipRot = Quaternion.Euler(throwablesettings.unequipRotation);
        transform.localRotation = unequipRot;
    }

    public void PullTrigger(bool isPulling)
    {
        pullingTrigger = isPulling;
    }
    public void SetOwner(WeaponHandeler wp)
    {
        owner = wp;
    }

    void PositionCrosshair(Ray ray)
    {
        RaycastHit hit;
        Transform bSpwan = throwablesettings.nadeSpwan;
        Vector3 bSpwanPoint = bSpwan.position;
        Vector3 dir = ray.GetPoint(throwablesettings.range) - bSpwanPoint;

        if (Physics.Raycast(bSpwanPoint, dir, out hit, throwablesettings.range, throwablesettings.bulletLayers))
        {
            if (throwablesettings.CrossHair != null)
            {
                ToggleCrossHair(true);
                throwablesettings.CrossHair.transform.position = hit.point;
                throwablesettings.CrossHair.transform.LookAt(Camera.main.transform);
            }
            else
            {
                ToggleCrossHair(false);
            }
        }
        else
            ToggleCrossHair(false);
    }
}
