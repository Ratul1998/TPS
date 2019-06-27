using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swim : MonoBehaviour
{
    public Animator playerAnimator;
    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Enemy")
            return;
        else
        {
            
            if (!playerAnimator.GetBool("OnWater"))
            {
                if (other.tag == "Water_Detector")
                {
                    playerAnimator.SetBool("OnWater", true);
                }
            }

        }
        
    }
}