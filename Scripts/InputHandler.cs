using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour {

    StateManager states;
    public TPSCamera camManager;
    HandlePlayer hMove;
    public AnimationCurve vaultCurve;
    float Horizontal;
    float Vertical;

    private void Start()
    {
        //Add reference
        gameObject.AddComponent<HandlePlayer>();

        //GetReference
        camManager = TPSCamera.singleton;
        states = GetComponent<StateManager>();
        hMove = GetComponent<HandlePlayer>();

        camManager.target = this.transform;

        states.isPlayer = true;
        states.init();
        hMove.Init(states,this);

        FixPlayerMeshes();

    }

    private void FixPlayerMeshes()
    {
        SkinnedMeshRenderer[] skined = GetComponentsInChildren<SkinnedMeshRenderer>();
        for(int i = 0; i < skined.Length; i++)
        {
            skined[i].updateWhenOffscreen = true;
        }
    }

    private void FixedUpdate()
    {
        states.FixedTick();
        UpdateStatesFromInput();
        hMove.Tick();
    }

    private void Update()
    {
        states.RegularTick();
    }

    private void UpdateStatesFromInput()
    {
        Vertical = Input.GetAxis(Statics.Vertical);
        Horizontal = Input.GetAxis(Statics.Horizontal);

        Vector3 v = camManager.transform.forward * Vertical;
        Vector3 h = camManager.transform.right * Horizontal;

        v.y = 0;
        h.y = 0;

        states.horizontal = Horizontal;
        states.vertical = Vertical;

        Vector3 moveDir = (h + v).normalized;
        states.moveDirection = moveDir;
        states.inAngle_MoveDir = InAngle(states.moveDirection, 25);
        if(states.walk && Horizontal!=0 || states.walk && Vertical != 0)
        {
            states.inAngle_MoveDir = true;
        }
        states.onLocomotion = states.anim.GetBool(Statics.onLocomotion);
        HandleRun();

        states.jumpInput = Input.GetButton(Statics.Jump);
    }

    private void HandleRun()
    {
        bool runInput = Input.GetButton(Statics.Fire3);

        if (runInput)
        {
            states.walk = false;
            states.run = true;
        }
        else
        {
            states.walk = true;
            states.run = false;
        }
        if(Horizontal!=0||Vertical!=0)
        {
            states.run = runInput;
            states.anim.SetInteger(Statics.specialType,Statics.GetAnimSpecialTypes(Statics.AnimSpecials.run));
        }
        else
        {
            if (states.run)
                states.run = false;
        }
        if(!states.inAngle_MoveDir && hMove.doAngleCheck)
        {
            states.run = false;
        }
        if (states.obsTacleForward)
            states.run = false;
        if (states.run == false)
        {
            states.anim.SetInteger(Statics.specialType, Statics.GetAnimSpecialTypes(Statics.AnimSpecials.runToStop));
        }
    }
    private bool InAngle(Vector3 moveDirection, int v)
    {
        bool r = false;
        float angle = Vector3.Angle(transform.forward, moveDirection);
        if (angle < v)
        {
            r = true;
        }
        return r;
    }
    public void EnableRootMovement()
    {
        hMove.EnableRootMovement();
    }
}
