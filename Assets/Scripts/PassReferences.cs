using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassReferences : MonoBehaviour {

    public TakedownReferences tr;
    private void OnTriggerEnter(Collider other)
    {
       
        if (other.GetComponentInChildren<TakedownPlayer>())
        {
            other.GetComponent<TakedownPlayer>().enRef = tr;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<TakedownPlayer>())
        {
            if (other.GetComponent<TakedownPlayer>().enRef == tr)
            {
                other.GetComponent<TakedownPlayer>().enRef = null;
            }
        }
    }
}
