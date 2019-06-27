using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(LineRenderer))]
public class Granade : MonoBehaviour {

    public float delay = 3f;
    public Animator anim;
    Rigidbody rb;

    Collider col;

    public float countDown;

    bool NadeThrown = false;

    public float blastradius = 5f;

    public GameObject explosionEffect;

    bool hasExploded = false;
    bool CountdownStart = false;
    public float ExplosionForce = 1000f;

    public float velocity;
    public float angle;
    public int resolution = 10 ;

    float g;
    float radianAngle;
    LineRenderer Lr;
    Vector3 retval;
    private void Awake()
    {
        Lr = GetComponent<LineRenderer>();
        g = Mathf.Abs(Physics.gravity.y);
    }
    void Start()
    {
        RenderArc();
        countDown = delay;
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.isKinematic = true;
    }

    private void RenderArc()
    {
        Lr.SetVertexCount(resolution + 1);
        Lr.SetPositions(CalculateArcArray());
    }

    private Vector3[] CalculateArcArray()
    {
        Vector3[] arcArray = new Vector3[resolution + 1];
        radianAngle = Mathf.Deg2Rad * angle;
        float maxDistance = (velocity * velocity * Mathf.Sin(2 * radianAngle)) / g;
        for(int i = 0; i <= resolution; i++)
        {
            float t = (float)i / (float)resolution;
            arcArray[i] = CalculateArcPoint(t,maxDistance);
        }
        return arcArray;
    }

    private Vector3 CalculateArcPoint(float t,float maxDist)
    {
        float x = t * maxDist;
        float z = t * maxDist;
        float y = (x * Mathf.Tan(radianAngle)) - ((g*x*x) / (2*velocity*velocity*Mathf.Cos(radianAngle)*Mathf.Cos(radianAngle)));
        retval = new Vector3(x, y, z);
        return retval;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            CountdownStart = true;
            
            col.enabled = true;
            rb.useGravity = false;
            rb.isKinematic = false;
        }
        if (Input.GetButtonUp("Fire1"))
        {
            anim.SetTrigger("ThrowTrigger");
            gameObject.transform.parent = null;
            rb.AddForce((transform.forward) *500f) ;
            rb.useGravity = true;
            NadeThrown = true;
        }
        if (!NadeThrown && Input.GetButtonUp("Fire2"))
        {
            anim.SetBool("nadeEquiped", false);
            countDown = delay;
            GetComponent<Throwables>().enabled = true;
            this.enabled = false;
        }
        if (CountdownStart)
        {
            countDown -= Time.deltaTime;
            if (countDown <= 0f && !hasExploded)
            {
                Explode();
                hasExploded = true;
                NadeThrown = true;
            }
        }
    }

    private void Explode()
    {
        GameObject EXPINS = Instantiate(explosionEffect, transform.position, transform.rotation);
        Destroy(EXPINS, 0.5f);
        Collider[] colliders= Physics.OverlapSphere(transform.position, blastradius);
        foreach(Collider nearbyObj in colliders)
        {
           Rigidbody rb = nearbyObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(ExplosionForce,transform.position,blastradius);
            }
            DestructableObjects dest = nearbyObj.GetComponent<DestructableObjects>();
            if (dest != null)
            {
                dest.TakeDamage(100f);

            }

        }
        Collider[] colliderstoMove = Physics.OverlapSphere(transform.position, blastradius);
        foreach(Collider nearbyObj in colliderstoMove)
        {
            Rigidbody rb = nearbyObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(ExplosionForce, transform.position, blastradius);
            }
        }


        Destroy(gameObject);
    }
}
