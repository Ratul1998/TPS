using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSpeed : MonoBehaviour {

    TimeManager tM;
    ParticleSystem[] ps;

    private void Start()
    {
        tM = TimeManager.GetInstance();
        ps = GetComponentsInChildren<ParticleSystem>();
    }
    private void Update()
    {
        for(int i = 0; i < ps.Length; i++)
        {
            ps[i].playbackSpeed = tM.myTimeScale;
        }
    }
}
