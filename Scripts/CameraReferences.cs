using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraReferences : MonoBehaviour {
    public Camera normalCamera;
    public Camera xray;
	// Use this for initialization
	void Start () {
        xray.gameObject.SetActive(false);
	}
    public static CameraReferences instance;
    public static CameraReferences GetInstance()
    {
        return instance;
    }
    private void Awake()
    {
        instance = this;
    }
}
