using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TakeDownCinematic : MonoBehaviour {

    public TimeManager tm;
    public bool runtakeDown;
    public bool initTakedown;
    public int t_index;
    public bool xray;
    public List<TakedownHolder> takedownList = new List<TakedownHolder>();
    public Transform camHelper;
    TakeDownTimeline curTimeline;
    public bool debugMode;
    public int debugID;

    public int xrayLayer;
    public int defaultLayer;

    public GameObject mainCameraRig;
    public Transform mainCamera;
    float curFov;

    public TakedownReferences playerRef;
    public TakedownReferences enemyref;

    Vector3 storeCamPosition;
    private void Start()
    {
        tm = TimeManager.GetInstance();
        camHelper = new GameObject().transform;
        InitTakedownHolders();
        if (debugMode)
        {
            playerRef.Init();
            enemyref.Init();
        }
    }

    private void Update()
    {
        if (runtakeDown)
        {
            Takedown t = takedownList[t_index].timeline.td;
            curTimeline = takedownList[t_index].timeline;
            InitTakedown(takedownList[t_index]);
            curTimeline.td.xRay = xray;
            curTimeline.Init(this, playerRef, enemyref);
            if (!initTakedown)
            {
                curTimeline.ChangeCameraTarget(0);
                InitParticles(t);
                t.enemy.transform.rotation = t.player.transform.rotation;
                Vector3 worldPos = t.enemy.transform.TransformDirection(t.info.offset);
                worldPos += t.enemy.transform.position;

                StartCoroutine(LerpToTargetPos_andPlayAnims(worldPos, t));
                initTakedown = true;
            }
            else
            {
                if (curTimeline)
                {
                    if (curTimeline.cinematicTakedown)
                        curTimeline.Tick();
                }
            }
        }
    }

    IEnumerator LerpToTargetPos_andPlayAnims(Vector3 worldPos, Takedown t)
    {
        Vector3 dest = worldPos;
        Vector3 from = t.player.transform.position;
        float prec = 0;
        while (prec < 1)
        {
            if (curTimeline.jumpCut)
                prec = 1;
            else
                prec += Time.deltaTime * 5;

            t.player.transform.position = Vector3.Lerp(from, dest, prec);
            yield return null;
        }
        t.cameraAnim.enabled = true;
        t.cameraAnim.Play(curTimeline.timlineName);
        t.player.anim.CrossFade(t.info.p_anim, t.info.p_crossfade_timer);
        t.player.anim.CrossFade("Takedown", t.info.p_crossfade_timer);
        yield return new WaitForSeconds(t.info.e_delay);
        t.enemy.anim.CrossFade(t.info.e_anim, t.info.e_crossfade_timer);
        t.enemy.anim.CrossFade("Takedown", t.info.e_crossfade_timer);

    }

    private void InitParticles(Takedown t)
    {
        for(int i = 0; i < t.particles.Length; i++)
        {
            ParticlesForTakedowns p = t.particles[i];
            GameObject go = p.particleGO;
            if (go == null)
            {
                go = Instantiate(p.particlePrefabs, transform.position, Quaternion.identity) as GameObject;
            }
            if (p.particles.Length == 0)
            {
                p.particles = go.GetComponentsInChildren<ParticleSystem>();
            }
            p.particleGO = go;
            if (p.placeOnBone)
            {
                p.targetTrans = (p.playerBone) ? t.player.anim.GetBoneTransform(p.bone) : t.enemy.anim.GetBoneTransform(p.bone);
            }
            p.particleGO.transform.parent = p.targetTrans;
            p.particleGO.transform.localPosition = p.targetPos;
            p.particleGO.transform.rotation = Quaternion.Euler(Vector3.zero);
        }
    }

    private void InitTakedown(TakedownHolder takedownHolder)
    {
        if (takedownHolder.cinematicTakedown)
        {
            Cursor.lockState = CursorLockMode.Confined;
            storeCamPosition = mainCamera.transform.localPosition;
            mainCamera.transform.parent = curTimeline.transform;
            mainCamera.transform.localPosition = Vector3.zero;
            mainCamera.transform.localRotation = Quaternion.Euler(Vector3.zero);

            if (curTimeline.td.xRay)
            {
                Transform xray = CameraReferences.GetInstance().xray.transform;
                xray.parent = curTimeline.transform;
                xray.localPosition = Vector3.zero;
                xray.localRotation = Quaternion.Euler(Vector3.zero);
                xray.gameObject.SetActive(true);
                curTimeline.td.c_xRay = CameraReferences.GetInstance().xray;
            }
            mainCameraRig.SetActive(false);
        }
        takedownHolder.holder.SetActive(true);
    }

    private void InitTakedownHolders()
    {
        for(int i = 0; i < takedownList.Count; i++)
        {
            TakedownHolder t = takedownList[i];
            t.timeline = t.holder.GetComponentInChildren<TakeDownTimeline>();
            t.holder.SetActive(false);
        }
    }

    public void CloseTakedown()
    {
        mainCameraRig.SetActive(true);

        Camera.main.fieldOfView = 60;

        mainCamera.transform.parent = CameraRig.instance.pivot.GetChild(0);
        mainCamera.transform.localPosition = storeCamPosition;
        mainCamera.transform.localRotation = Quaternion.Euler(Vector3.zero);
        CloseAllTakedown();
        initTakedown = false;
        runtakeDown = false;
        curTimeline.prec = 0;
        curTimeline = null;
    }

    private void CloseAllTakedown()
    {
        foreach(TakedownHolder t in takedownList)
        {
            t.holder.SetActive(false);
        }
    }
}
[System.Serializable]
public class Takedown
{
    public string id;
    public float totalLength;
    public Takedown_Info info;
    public Animator cameraAnim;
    public BezierCurve camPath;
    public TakedownReferences player;
    public TakedownReferences enemy;
    public int cam_t_index;
    public Takedown_CamTargets[] camT;
    public bool xRay;
    public Transform cameraRig;
    public Camera c_xRay;
    public ParticlesForTakedowns[] particles;
}
[System.Serializable]
public class ParticlesForTakedowns
{
    public GameObject particlePrefabs;
    public GameObject particleGO;
    public bool placeOnBone;
    public bool playerBone;
    public HumanBodyBones bone;
    public Transform targetTrans;
    public ParticleSystem[] particles;
    public Vector3 targetPos;
    public Vector3 targetRot;
}

[System.Serializable]
public class Takedown_Info
{
    public string p_anim;
    public float p_crossfade_timer = 0.2f;
    public string e_anim;
    public float e_crossfade_timer = 0.2f;
    public float e_delay;
    public Vector3 offset;
    public bool disableWeapons;
    public GameObject[] enable_GOs;
}
[System.Serializable]
public class Takedown_CamTargets
{
    public Transform target;
    public bool assignBone = true;
    public HumanBodyBones bone;
    public bool fromPlayer;
    public bool jumpTo;
}
[System.Serializable]
public class TakedownHolder
{
    public string id;
    public TakeDownTimeline timeline;
    public GameObject holder;
    public bool cinematicTakedown;
}
