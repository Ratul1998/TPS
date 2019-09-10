using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TPSCamera : MonoBehaviour {

    public bool holdCamera;
    public bool addDefaultAsNormal;
    public Transform target;

    #region variables
    public string activeStateID;
    public float moveSpeed = 5f;
    public float turnSpeed = 1.5f;
    public float turnSpeedController = 5.5f;
    public float turnSmoothing = 0.1f;
    public bool isController;
    public bool lockCursor;
    #endregion

    #region references
    public Transform pivot;
    public Transform camTrans;
    #endregion

    static public TPSCamera singleton;

    Vector3 targetPosition;
    public Vector3 targetPositionOffset;

    #region Internal variables
    float x;
    float y;
    float LookAnge;
    float titleAngle;
    float offsetX;
    float offsetY;
    float smoothX = 0;
    float SmoothY = 0;
    float smoothXvelocity = 0;
    float smoothYvelocity = 0;
    LayerMask ignoreLayers;
    #endregion

    [System.Serializable]
    public class CameraState
    {
        public string id;
        public float minAngle;
        public float maxAngle;
        public bool useDefaultPosition;
        public Vector3 pivotPosition;
        public bool useDefaultCameraZ;
        public float cameraZ;
        public bool useDefaultFOV;
        public float cameraFOV;
    }

    public List<CameraState> cameraState = new List<CameraState>();
    CameraState activeState;
    CameraState defaultState;

    private void Awake()
    {
        singleton = this;
    }

    private void Start()
    {
        if (Camera.main.transform == null)
        {
            Debug.Log("No mainCamera Assigned");
        }

        camTrans = Camera.main.transform.parent;
        pivot = camTrans.parent;


        //Create Default State
        CameraState cs = new CameraState();
        cs.id = "defaault";
        cs.minAngle = 35;
        cs.maxAngle = 35;
        cs.cameraFOV = Camera.main.fieldOfView;
        cs.cameraZ = camTrans.localPosition.z;
        cs.pivotPosition = pivot.localPosition;
        defaultState = cs;

        if (addDefaultAsNormal)
        {
            cameraState.Add(defaultState);
            defaultState.id = "normal";
        }

        activeState = defaultState;
        activeStateID = activeState.id;
        FixPositions();

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        ignoreLayers = ~(1 << 3 | 1 << 8);
    }

    private void FixedUpdate()
    {
        if (target)
        {
            targetPosition = target.position + targetPositionOffset;
        }
        CameraFollow();

        if (!holdCamera)
            HandleRotation();
        FixPositions();
    }

    private void HandleRotation()
    {
        HandleOffsets();

        x = Input.GetAxis("Mouse X") + offsetX;
        y = Input.GetAxis("Mouse Y") + offsetY;

        float targetTurnSpeed = turnSpeed;

        if (isController)
        {
            targetTurnSpeed = turnSpeedController;
        }
        if (turnSmoothing > 0)
        {
            smoothX = Mathf.SmoothDamp(smoothX, x, ref smoothXvelocity, turnSmoothing);
            SmoothY = Mathf.SmoothDamp(SmoothY, y, ref smoothYvelocity, turnSmoothing);
        }
        else
        {
            smoothX = x;
            SmoothY = y;
        }

        LookAnge += smoothX * targetTurnSpeed;

        //reset the look angle when it does a full circle
        if (LookAnge > 360)
        {
            LookAnge = 0;
        }
        if (LookAnge < -360)
            LookAnge = 0;

        transform.rotation = Quaternion.Euler(0f, LookAnge, 0f);
        titleAngle -= SmoothY * targetTurnSpeed;
        titleAngle = Mathf.Clamp(titleAngle, -activeState.minAngle, activeState.maxAngle);
        pivot.localRotation = Quaternion.Euler(titleAngle, 0, 0);

    }

    private void HandleOffsets()
    {
        if (offsetX != 0)
        {
            offsetX = Mathf.MoveTowards(offsetX, 0, Time.deltaTime);
        }
        if (offsetY != 0)
        {
            offsetY = Mathf.MoveTowards(offsetY, 0, Time.deltaTime);
        }
    }

    private void CameraFollow()
    {
        Vector3 camPosition = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
        transform.position = camPosition;
    }

    private void FixPositions()
    {
        Vector3 targetPivotPosition = (activeState.useDefaultPosition) ? defaultState.pivotPosition : activeState.pivotPosition;
        pivot.localPosition = Vector3.Lerp(pivot.localPosition, targetPivotPosition,Time.deltaTime*5f);

        float targetZ = (activeState.useDefaultCameraZ) ? defaultState.cameraZ : activeState.cameraZ;
        float actualZ = targetZ;

        CameraCollision(targetZ, ref actualZ);

        Vector3 targetP = camTrans.localPosition;
        targetP.z = Mathf.Lerp(targetP.z, actualZ, Time.deltaTime * 5f);
        camTrans.localPosition = targetP;

        float targetFOV = (activeState.useDefaultFOV) ? defaultState.cameraFOV : activeState.cameraFOV;
        if (targetFOV < 1)
        {
            targetFOV = 2;
        }
        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFOV, Time.deltaTime * 5f);

    }

    CameraState GetStates(string id)
    {
        CameraState r = null;
        for(int i = 0; i < cameraState.Count; i++)
        {
            if (cameraState[i].id == id)
            {
                r = cameraState[i];
                break;
            }
        }
        return r;
    }

    public void ChangeState(string id)
    {
        if (activeState.id != id)
        {
            CameraState targetState = GetStates(id);
            if (targetState == null)
            {
                Debug.LogError("State not found");
                return;
            }
            activeState = targetState;
            activeStateID = activeState.id;
        }
    }
    void CameraCollision(float targetZ,ref float actualZ)
    {
        float step = Mathf.Abs(targetZ);
        int stepCount = 4;
        float stepIncremental = step / stepCount;
        
        
        RaycastHit hit;
        Vector3 origin = pivot.position;
        Vector3 direction = -pivot.forward;
        Debug.DrawRay(origin, direction * step, Color.blue);
        if(Physics.Raycast(origin,direction,out hit,step, ignoreLayers))
        {
            float distance = Vector3.Distance(hit.point, origin);
            actualZ = -(distance / 2);

        }
        else
        {
            for (int s = 0; s < stepCount + 1; s++)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector3 dir = Vector3.zero;
                    Vector3 secondOrigin = origin + (direction * s) * stepIncremental;

                    switch (i)
                    {
                        case 0:
                            dir = camTrans.right;
                            break;
                        case 1:
                            dir = -camTrans.right;
                            break;
                        case 2:
                            dir = camTrans.up;
                            break;
                        case 3:
                            dir = -camTrans.up;
                            break;
                        default:
                            break;
                    }
                    Debug.DrawRay(secondOrigin, dir * 1, Color.red);
                    if (Physics.Raycast(secondOrigin, dir, out hit, ignoreLayers))
                    {
                        float distance = Vector3.Distance(secondOrigin, origin);
                        actualZ = -(distance / 2);
                        offsetX = dir.x;
                        offsetY = dir.y;
                        break;
                    }
                }
            }
        }
        
    }
}
