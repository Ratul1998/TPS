using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverBehaviour : MonoBehaviour {
    public BezierCurve curvepath;
    public bool blockPos1;
    public bool blockPos2;
    public bool AICover;
    public List<CoverBase> FrontPositions = new List<CoverBase>();
    public List<CoverBase> BackPositions = new List<CoverBase>();

    public float length;

    public enum CoverType
    {
        full,half
    }

    public CoverType coverType;
	
	void Start () {
        if (!AICover)
        {
            curvepath = GetComponentInChildren<BezierCurve>();
            length = curvepath.length;
        }
        else
        {
            for(int i = 0; i < BackPositions.Count; i++)
            {
                BackPositions[i].backPos = true;
            }
        }
	}
	
}
[System.Serializable]
public class CoverBase
{
    public bool occupied;
    public Transform positionObject;
    public bool backPos;
}
