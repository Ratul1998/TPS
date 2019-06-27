using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POI_Deadbody : POI_Base {

    public CharecterStats owner;
	
	void Start () {
        owner = GetComponentInParent<CharecterStats>();
        owner.enableOnDeath = this;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
