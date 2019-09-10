using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRange : MonoBehaviour {
    public List<AI> enimies = new List<AI>();
    public static PlayerRange instance;

    private void Awake()
    {
        instance = this;
    }

    public static PlayerRange GetInstance()
    {
        return instance;
    }

    private void Update()
    {
        foreach(AI c in enimies)
        {
            if (c.GetComponent<CharecterStats>().dead)
            {
                enimies.Remove(c);
            }
        }
    }

    public AI ClosestEnemy()
    {
        AI closestEnemy = null;
        float minDist = Mathf.Infinity;
        if (enimies.Count > 0)
        {
            foreach (AI c in enimies)
            {
                Vector3 start = GetComponentInParent<UserInput>().transform.position + (GetComponentInParent<UserInput>().transform.up * 1.8f );
                Vector3 dir = (c.transform.position + c.transform.up) - start;
                float distToCharacter = Vector3.Distance(c.transform.position, transform.position);
                float sightAngle = Vector3.Angle(dir, GetComponentInParent<UserInput>().transform.forward);
                if (distToCharacter < minDist && sightAngle < 30f )
                {
                    closestEnemy = c;
                    minDist = distToCharacter;
                }

            }
        }
        return closestEnemy;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<AI>())
        {
            if (!enimies.Contains(other.GetComponent<AI>()))
            {
                enimies.Add(other.GetComponent<AI>());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<AI>())
        {
            if (enimies.Contains(other.GetComponent<AI>()))
                enimies.Remove(other.GetComponent<AI>());
        }
    }

}
