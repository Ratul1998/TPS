using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityStandardAssets.CinematicEffects {
    public class ControlDOF : MonoBehaviour {
        public UserInput states;
        //public DepthOfField dof;

        public DOFvalues defValues;
        public DOFvalues aimingValues;
        public DOFvalues cinematic;
        public DOFvalues closeup;

        private void Start()
        {
            //defValues.focusPlane = dof.focuss.focusPlane; 
            //defValues.focusRange = dof.focuss.range;
            //defValues.fstops = dof.focuss.farBlurRadius;
        }

        private void Update()
        {
            if (states == null)
                states = CameraRig.instance.target.GetComponent<UserInput>();
            if (states.aiming)
            {
                ChangeDOFValues(1);
            }
            else
            {
                ChangeDOFValues(0);
            }

        }
        int curStatus;
        public void ChangeDOFValues(int v)
        {
            if (v == curStatus)
                return;
            switch (v)
            {
                case 0:
                    StartCoroutine(ChangeValues(defValues, false));
                    break;
                case 1:
                    StartCoroutine(ChangeValues(aimingValues, false));
                    break;
                case 2:
                    StartCoroutine(ChangeValues(cinematic, false));
                    break;
                case 3:
                    StartCoroutine(ChangeValues(closeup, false));
                    break;
            }
            curStatus = v;
        }

        IEnumerator ChangeValues(DOFvalues v, bool instant)
        {
            //float curFP = dof.focuss.focusPlane;
            //float curFR = dof.focuss.range;
            //float curFS = dof.focuss.farBlurRadius;

            float targetFP = v.focusPlane;
            float targetFR = v.focusRange;
            float targetFS = v.fstops;

            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * 15;
                if (instant)
                {
                    t = 1;
                }
                //dof.focuss.focusPlane = Mathf.Lerp(curFP, targetFP, t);
                //dof.focuss.range = Mathf.Lerp(curFR, targetFR, t);
                //dof.focuss.farBlurRadius = Mathf.Lerp(curFS, targetFS, t);
                yield return null;
            }
        }
        public static ControlDOF instance;
        public static ControlDOF GetInstance()
        {
            return instance;
        }
        private void Awake()
        {
            instance = this;
        }
    }
    [System.Serializable]
    public class DOFvalues
    {
        public float focusPlane;
        public float fstops;
        public float focusRange;
        public Transform targetTrans = null;
    }
}