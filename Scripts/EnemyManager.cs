using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour {

    public GameObject[] EnemyUnit;
    public GameObject[] EnemyUnitWayPoints;
    List<AI> enimies = new List<AI>();
    int count = 0;
    int DeathCount = 0;
    bool initCheck = false;
    
    void Update () {
        if (!initCheck)
        {
            for (int i = count+1 ; i < EnemyUnit.Length; i++)
            {
                EnemyUnit[count].SetActive(true);
                EnemyUnitWayPoints[count].SetActive(true);
                EnemyUnit[i].SetActive(false);
                EnemyUnitWayPoints[i].SetActive(false);
            }
            foreach (Transform child in EnemyUnit[count].transform)
            {
                if (child.GetComponent<AI>())
                {
                    if (!enimies.Contains(child.GetComponent<AI>()))
                        enimies.Add(child.GetComponent<AI>());
                }
            }
            initCheck = true;
        }
        foreach (Transform child in EnemyUnit[count].transform)
        {
            if (enimies.Contains(child.GetComponent<AI>()))
            {
                if (child.GetComponent<CharecterStats>().dead)
                {
                    DeathCount++;
                    enimies.Remove(child.GetComponent<AI>());
                }
            }
        }
        if (DeathCount == EnemyUnit[count].transform.childCount)
        {
            EnemyUnit[count].SetActive(false);
            EnemyUnitWayPoints[count].SetActive(false);
            count++;
            DeathCount = 0;
            initCheck = false;
        }
	}
}
