using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointofInterestBehaviour : MonoBehaviour {
    AI enAI_main;
	
	void Start () {
        enAI_main = GetComponent<AI>();
	}
	
	public void POIBehaviour()
    {

        if (enAI_main.PointsOfInterestList.Count > 0)
        {
            for(int i = 0; i < enAI_main.PointsOfInterestList.Count; i++)
            {
                if (enAI_main.PointsOfInterestList[i] != null)
                {

                    POIBehaviour(enAI_main.PointsOfInterestList[i].poiType, enAI_main.PointsOfInterestList[i]);
                }
            }
        }
    }

    void POIBehaviour(POI_Base.POIType type, POI_Base poi)
    {
        switch (type)
        {
            case POI_Base.POIType.deadbody:
                POI_Deadbody body = poi.transform.GetComponentInChildren<POI_Deadbody>();
                if (body)
                {
                    if (body.isActiveAndEnabled)
                    {
                        Vector3 directionTowardsPOI = body.transform.position - transform.position;
                        float angleTowardsTarget = Vector3.Angle(directionTowardsPOI.normalized, transform.forward);

                        if (angleTowardsTarget < enAI_main.sight.fieldofView)
                        {
                            Vector3 origin = transform.position + new Vector3(0, 1.8f, 0);
                            Vector3 rayDirection = body.transform.position - origin;
                            Debug.DrawRay(origin, rayDirection, Color.green);
                            RaycastHit hit;
                            if (Physics.Raycast(origin, rayDirection, out hit, enAI_main.sight.sightRange))
                            {
                                if (hit.transform.Equals(body.transform) || hit.transform.GetComponentInParent<CharecterStats>())
                                {
                                    if (hit.transform.GetComponentInParent<CharecterStats>().dead)
                                    {
                                        enAI_main.targetLastKnownPosition = hit.transform.GetComponentInParent<CharecterStats>().gameObject.transform.position;
                                        enAI_main.sight.alertlevel = 10;
                                        enAI_main.AI_State_HasTarget();
                                        enAI_main.PointsOfInterestList.Remove(poi);
                                        if(enAI_main.allCharacters.Contains(hit.transform.GetComponentInParent<CharecterStats>()))
                                            enAI_main.allCharacters.Remove(hit.transform.GetComponentInParent<CharecterStats>());
                                        Destroy(hit.transform.GetComponentInParent<CharecterStats>().gameObject, 5f);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        enAI_main.PointsOfInterestList.Remove(poi);
                    }
                }
                break;
            case POI_Base.POIType.other:
                break;
        }
    }
}
