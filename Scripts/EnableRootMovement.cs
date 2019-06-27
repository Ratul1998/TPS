using UnityEngine;
using System.Collections;

public class EnableRootMovement : StateMachineBehaviour
{

    HandlePlayer ce;

    public float timer = 0.2f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (ce == null)
        {
            ce = animator.transform.GetComponent<HandlePlayer>();
        }

        if (ce == null)
            return;

        ce.enableRootMovement=true;
    }

}
