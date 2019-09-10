using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastStand : MonoBehaviour {

    UserInput ih;
    public Rigidbody rb;
    CharecterMovement states;
    Collider col;
    bool initDownSate;
    PhysicMaterial zFriction;
    PhysicMaterial mFriction;

    float horizontal;
    float vertical;

    float moveDuration = 1;
    float crawlSpeed = 0.1f;

    bool move;
    bool faceDown;
    float curTime;
    Vector3 moveDirection;

    public void Init(CharecterMovement st)
    {
        ih = GetComponent<UserInput>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        states = st;
        zFriction = new PhysicMaterial();
        zFriction.dynamicFriction = 0;
        zFriction.staticFriction = 0;
        mFriction = new PhysicMaterial();
        mFriction.dynamicFriction = 1;
        mFriction.staticFriction = 1;
    }

    public void Tick()
    {
        if (!initDownSate)
        {
            states.animator.CrossFade("Hit_to_Down", 0.3f);
            states.animator.SetBool("Down", true);
            
        }
    }

}
