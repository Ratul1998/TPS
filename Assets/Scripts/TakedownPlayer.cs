using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TakedownPlayer : MonoBehaviour {
    UserInput ih;
    TakeDownCinematic tdManager;
    TakeDownTimeline tl;
    TakedownReferences plRef;
    public TakedownReferences enRef = null;
    Weapons prev;
    WeaponHandeler wp;
    bool initText;
    Text UIText;

    public int takedown;
    public bool xray;

    private void Start()
    {
        tdManager = GetComponentInChildren<TakeDownCinematic>();
        ih = GetComponent<UserInput>();
        plRef = GetComponent<TakedownReferences>();
        wp = GetComponent<WeaponHandeler>();
        tl = GetComponentInChildren<TakeDownTimeline>();
        plRef.Init();
        tdManager.mainCameraRig = CameraRig.GetInstance().transform.gameObject;
        tdManager.mainCamera = Camera.main.transform;
    }

    private void FixedUpdate()
    {
        if (enRef)
        {
            if (Input.GetKeyUp(KeyCode.M))
            {
                if (!tdManager.runtakeDown)
                {
                    tdManager.t_index = takedown;
                    tdManager.xray = xray;

                    ih.enabled = false;
                    prev = wp.currentWeapon;
                    wp.currentWeapon = tl.takedownWeapon;
                    plRef.Init();
                    tdManager.playerRef = plRef;
                    tdManager.enemyref = enRef;
                    tdManager.runtakeDown = true;
                }
            }
        }
    }
    public void EndTakedown()
    {
        tdManager.CloseTakedown();
        ih.enabled = true;
        wp.currentWeapon = prev;
        tdManager.runtakeDown = false;
        tdManager.enemyref.GetComponent<CharecterStats>().TakeDamage(100);
        tdManager.enemyref = null;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
