using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagDollManager : MonoBehaviour
{
    private Collider[] colliders { get { return GetComponentsInChildren<Collider>(); } set { colliders = value; } }
    private Rigidbody[] rigidbodies { get { return GetComponentsInChildren<Rigidbody>(); } set { rigidbodies = value; } }
    private Animator animator { get { return GetComponentInParent<Animator>(); } set { animator = value; } }
    private void Start()
    {
        if (colliders.Length == 0)
            return;
        if (rigidbodies.Length == 0)
            return;
        foreach (Collider col in colliders)
        {
            col.isTrigger=true;
        }
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
        }
    }
    public void Ragdoll()
    {
        if (animator == null)
            return;
        if (colliders.Length == 0)
            return;
        if (rigidbodies.Length == 0)
            return;

        animator.enabled = false;
        foreach (Collider col in colliders)
        {
            col.isTrigger=false;
        }
        foreach(Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = false;
        }
    }
}