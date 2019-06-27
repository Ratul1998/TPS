using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour {

    public static TimeManager instance;
    public float myTimeScale;

    private void Awake()
    {
        instance = this;

    }
    public static TimeManager GetInstance()
    {
        return instance;
    }
    public float GetDelta()
    {
        return Time.deltaTime * myTimeScale;
    }
    
}
