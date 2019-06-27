using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRange : MonoBehaviour {
    AI enAI;

	void Start () {
        enAI = GetComponentInParent<AI>();
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharecterStats>())
        {
            if (!enAI.allCharacters.Contains(other.GetComponent<CharecterStats>()) && other.GetComponent<CharecterStats>() != GetComponentInParent<CharecterStats>())
                enAI.allCharacters.Add(other.GetComponent<CharecterStats>());
        }
        if (other.GetComponent<POI_Base>())
        {
            POI_Base poi = other.GetComponent<POI_Base>();
            if (!enAI.PointsOfInterestList.Contains(poi))
            {
                enAI.PointsOfInterestList.Add(poi);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<CharecterStats>())
        {
            if (enAI.allCharacters.Contains(other.GetComponent<CharecterStats>()))
            {
                enAI.allCharacters.Remove(other.GetComponent<CharecterStats>());
            }
        }
        if (other.GetComponent<POI_Base>())
        {
            POI_Base poi = other.GetComponent<POI_Base>();
            if (enAI.PointsOfInterestList.Contains(poi))
            {
                enAI.PointsOfInterestList.Remove(poi);
            }
        }
    }
}
