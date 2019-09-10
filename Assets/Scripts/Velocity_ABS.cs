using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Velocity_ABS : StateMachineBehaviour {

    public float life = 0.4f;
    public float force = 6;
    public Vector3 direction;
    [Space]
    public bool useTransformForward;
    public bool additive;
    public bool onEnter;
    public bool onExit;
    public bool onEndClampVelocity;

    public AnimationCurve forceCurve;
    public bool useCurve;

    StateManager states;
    HandlePlayer ply;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (onEnter)
        {
            if (useTransformForward && !additive)
            {
                direction = animator.transform.forward;
            }
            if (useTransformForward && additive)
                direction += animator.transform.forward;
            if (states == null)
                states = animator.transform.GetComponent<StateManager>();
            if (!states.isPlayer)
                return;
            if (ply == null)
                ply = animator.transform.GetComponent<HandlePlayer>();
            ply.AddVelocity(direction, life, force, onEndClampVelocity,forceCurve,useCurve);
        }
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (onExit)
        {
            if (useTransformForward && !additive)
                direction = animator.transform.forward;
            if (useTransformForward && additive)
                direction += animator.transform.forward;
            if (states == null)
                states = animator.transform.GetComponent<StateManager>();
            if (!states.isPlayer)
                return;
            if (ply == null)
                ply = animator.transform.GetComponent<HandlePlayer>();
            ply.AddVelocity(direction, life, force, onEndClampVelocity,forceCurve,useCurve);
        }
    }
}
