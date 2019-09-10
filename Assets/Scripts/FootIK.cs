using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace IK.Feet
{
    public class FootIK : MonoBehaviour
    {
        Animator anim;
        Transform leftFoot;
        Transform rightFoot;
        float lf_weight;
        float rf_weight;

        public LayerMask ignoreLayers;
        Vector3 lf_pos;
        Quaternion lf_rot;
        Vector3 rf_pos;
        Quaternion rf_rot;
        Vector3 offset = new Vector3(0, 0.15f, 0);

        Transform l_helper;
        Transform r_helper;

        bool ignoreRotation;

        public void Init()
        {
            anim = GetComponent<Animator>();
            leftFoot = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
            rightFoot = anim.GetBoneTransform(HumanBodyBones.RightFoot);

            l_helper = new GameObject().transform;
            l_helper.transform.parent = leftFoot;
            l_helper.localPosition = new Vector3(0.147f, 0.2f, 0.043f);

            r_helper = new GameObject().transform;
            r_helper.transform.parent = rightFoot;
            r_helper.localPosition = new Vector3(0.138f, 0.2f, -0.039f);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (anim == null)
                return;
            FindPositions(leftFoot,l_helper,ref lf_pos,ref lf_rot);
            FindPositions(rightFoot, r_helper, ref rf_pos, ref rf_rot);

            lf_weight = anim.GetFloat("leftFoot");
            rf_weight = anim.GetFloat("rightFoot");

            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, lf_weight);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, lf_weight);

            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, rf_weight);
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, rf_weight);

            anim.SetIKPosition(AvatarIKGoal.LeftFoot, lf_pos);
            anim.SetIKPosition(AvatarIKGoal.RightFoot, rf_pos);

            anim.SetIKRotation(AvatarIKGoal.LeftFoot, lf_rot);
            anim.SetIKRotation(AvatarIKGoal.RightFoot, rf_rot);
        }

        private void FindPositions(Transform t,Transform helper,ref Vector3 targetPosition,ref Quaternion targetRotation)
        {
            RaycastHit hit;
            Vector3 origin = t.position;
            origin += Vector3.up * 0.03f;
            if(Physics.Raycast(origin,-Vector3.up,out hit, 1, ignoreLayers))
            {
                ignoreRotation = false;
                if (hit.transform.gameObject.layer == 9)
                    ignoreRotation = true;
                targetPosition = hit.point + (offset.y * transform.forward) ;
                Vector3 dir = helper.position - t.position;
                dir.y = 0;
                Quaternion rot = Quaternion.LookRotation(dir);
                if (!ignoreRotation)
                    targetRotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * rot;
                else
                    targetRotation = rot;
            }
        }
    }
}
