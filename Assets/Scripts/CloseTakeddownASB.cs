using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseTakeddownASB : StateMachineBehaviour {

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        if (animator.GetComponent<TakedownPlayer>())
        {
            animator.GetComponent<TakedownPlayer>().EndTakedown();
        }
    }
}
