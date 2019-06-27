using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharecterStats : MonoBehaviour {

    CharacterController characterController { get { return GetComponent<CharacterController>(); } set { characterController = value; } }

    RagDollManager ragDollManager { get { return GetComponentInChildren<RagDollManager>(); } set { ragDollManager = value; } }

    WeaponHandeler weaponHandeler { get { return GetComponent<WeaponHandeler>(); } set { weaponHandeler = value; } }
    public MonoBehaviour[] scriptToDisable;
    AI enAI;
    PlayerRange playerRange;
    public bool dead;
    public bool alert = true;
    [Range(0, 100)] public float health;
    public int faction;
    public int morale = 100;
    public int supressionLevel = 20;
    public int unitRank = 0;
    public bool hascover;
    public bool Shooting;
    public POI_Deadbody enableOnDeath;
    public Vector3 LookPosition;

    private void Start()
    {
        if(GetComponent<AI>())
            enAI = GetComponent<AI>();
        playerRange = PlayerRange.GetInstance();
    }

    void Update () {
        health = Mathf.Clamp(health, 0, 100);
	}

    public void TakeDamage(float amt)
    {
        health -= amt;
        if (health <= 0)
        {
            dead = true;
            Die();
        }
    }

    private void Die()
    {
        weaponHandeler.DropCurrentWeapon();
        characterController.enabled = false;
        if (enAI)
            enAI.DecreaseAlliesMorale(10);
        
        if (scriptToDisable.Length == 0)
        {
            return;
        }
        
        AI[] otherAI = FindObjectsOfType<AI>();
        foreach(AI ai in otherAI)
        {
            if (ai != enAI)
            {
                if (ai.AlliesNer.Contains(enAI))
                {
                    ai.AlliesNer.Remove(enAI);
                }
                if (ai.allCharacters.Contains(this))
                {
                    ai.allCharacters.Remove(this);
                }
            }
            else
            {
                if (playerRange.enimies.Contains(ai))
                    playerRange.enimies.Remove(ai);
                ai.AlertLevel.SetActive(false);
            }
        }
        bool destroy = false;
        if (enAI)
        {
            if (enAI.aIStates == AI.AIStates.Attack)
                destroy = true;
            else
                destroy = false;
        }
        GetComponent<CharacterController>().enabled = false;
        foreach(MonoBehaviour script in scriptToDisable)
        {
            script.enabled = false;
        }
        if (ragDollManager != null)
        {
            ragDollManager.Ragdoll();
        }
        if (destroy)
        {
            //Destroy(gameObject, 3f);
        }
    }
}
