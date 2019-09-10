using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeDownTimeline : MonoBehaviour {
    public string timlineName;
    public Takedown td;
    TakeDownCinematic main;
    [Range(0, 1)]
    public float prec;
    float delta;

    Vector3 targetPos;
    Transform targettrans;

    public bool cinematicTakedown;
    public bool jumpCut;

    public Weapons takedownWeapon;

    public void Init(TakeDownCinematic m,TakedownReferences pl,TakedownReferences en)
    {
        td.player = pl;
        td.enemy = en;
        if (main == null)
        {
            main = m;
            if (!cinematicTakedown)
            {
                td.xRay=false;
            }
        }
    }

    public void Tick()
    {
        if (!td.xRay)
            if (td.c_xRay)
                td.c_xRay.gameObject.SetActive(false);
        if (targettrans)
            targetPos = targettrans.position;

        main.camHelper.position = Vector3.Lerp(main.camHelper.position, targetPos, Time.deltaTime * 5);
        delta = main.tm.GetDelta();
        td.player.anim.speed = main.tm.myTimeScale;
        td.enemy.anim.speed = main.tm.myTimeScale;
        td.cameraAnim.speed = main.tm.myTimeScale;

        prec += delta / td.totalLength;
        if (prec > 1)
            prec = 1;
        if (prec < 0)
            prec = 0;
        Vector3 camPos = td.camPath.GetPointAt(prec);

        if (camPos != Vector3.zero)
        {
            td.cameraRig.transform.position = camPos;
        }
        td.cameraRig.LookAt(main.camHelper);
        
    }
    
    public void OpenSkeleton_Enemy()
    {
        if(td.xRay)
        {
            td.enemy.OpenSkeleton();
            td.player.ChangeLayer(main.xrayLayer);
        }
    }

    public void CloseSkeletons()
    {
        td.enemy.CloseSkeleton();
        td.player.CloseSkeleton();
        td.player.ChangeLayer(main.defaultLayer);
    }

    public IEnumerator ChangeTimeScale(float targetScale)
    {
        float target = targetScale;
        float cur = main.tm.myTimeScale;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 15;
            float v = Mathf.Lerp(cur, target, t);
            if (cinematicTakedown)
            {
                main.tm.myTimeScale = v;
            }
            yield return null;
        }
    }

    public IEnumerator ChangeFOV(float targetValue)
    {
        float target = targetValue;
        float curalue = Camera.main.fieldOfView;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 15;
            float v = Mathf.Lerp(curalue, target, t);
            if (cinematicTakedown)
            {
                Camera.main.fieldOfView = v;
                if (td.xRay)
                    td.c_xRay.fieldOfView = v;
            }
            yield return null;
        }
    }

    public void BreakBone(string b_name)
    {
        BonesList bone = ReturnBone(b_name);
        if (bone != null)
        {
            bone.bone.SetActive(false);
            bone.destroyed = true;
        }
    }

    public void PlayParticles(int i)
    {
        foreach(ParticleSystem ps in td.particles[i].particles)
        {
            ps.Play();
        }
    }

    private BonesList ReturnBone(string b_name)
    {
        BonesList retVal = null;
        for(int i = 0; i < td.enemy.bonesLists.Count; i++)
        {
            if (string.Equals(td.enemy.bonesLists[i].boneId, b_name))
            {
                retVal = td.enemy.bonesLists[i];
            }
        }
        return retVal;
    }

    public void ChangeDOF(int i)
    {
        if (UnityStandardAssets.CinematicEffects.ControlDOF.GetInstance())
        {
            UnityStandardAssets.CinematicEffects.ControlDOF.GetInstance().ChangeDOFValues(i);
        }
    }

    public void ChangeCameraTarget(int i)
    {
        Takedown_CamTargets tInfo = td.camT[i];
        if (tInfo.assignBone)
        {
            if (tInfo.fromPlayer)
            {
                targettrans = td.player.anim.GetBoneTransform(tInfo.bone);
            }
            else
            {
                targettrans = td.enemy.anim.GetBoneTransform(tInfo.bone);

            }
        }
        else
        {
            targettrans = tInfo.target;
        }
        if (tInfo.jumpTo)
        {
            main.camHelper.position = targettrans.position;
        }
    }


}
