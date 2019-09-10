using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharecterMovement))]
[RequireComponent(typeof(CharecterStats))]
public class AI : MonoBehaviour {
    public NavMeshAgent navMesh;

    private CharecterMovement charecterMove;

    private Animator animator;

    private WeaponHandeler weaponHandeler;

    private CharecterStats characterStats;

    public enum AIStates { Patrol, Attack, FindEnemy, FindCover, hasTarget, search, deciding, retreat }

    public AIStates aIStates;

    [System.Serializable]
    public class PatrolSettings
    {
        public List<WaypointBase> waypoints = new List<WaypointBase>();
        public bool CircularWayPoints;
    }
    public PatrolSettings patrolSettings;

    [System.Serializable]
    public class SightSettings
    {
        public float sightRange = 30f;
        public LayerMask sightlayers;
        public float fieldofView = 120f;
        [Tooltip("Start From Centre Of Chatrecter")]
        public float eyeHeight;
        public float alertlevel = 0f;
    }
    public SightSettings sight;

    [System.Serializable]
    public class AttackSettings
    {
        public float fireChance = 0.1f;
        public float attackR = 0f;
        public int timeShoot = 0;
        public bool startShooting;
    }
    public AttackSettings attack;

    [System.Serializable]
    public class AlertBehaviour
    {
        public bool onPatrol;
        public bool canChase;
        public List<WaypointBase> onAlertExtraBehaviours = new List<WaypointBase>();
        public string[] alertLogic;
        public bool circularwayPoints;
    }
    public AlertBehaviour alert;

    [System.Serializable]
    public class SearchEnemy
    {
        public bool decideBehaviour;
        public float decideBehaviourThreshold = 15;
        public float alertMultiplier = 1;
        public List<Transform> possibleHigingPlaces = new List<Transform>();
        public List<Vector3> positionsArountUnit = new List<Vector3>();
        public bool populateListOfPositions;
        public float waitTime = 1.3f;
        public bool CircularWaypoints;
    }
    public SearchEnemy search;

    [System.Serializable]
    public class CoverSettings
    {
        public bool findCoverPosition;
        public List<Transform> coverPositions = new List<Transform>();
        public List<Transform> ignorePositions = new List<Transform>();
        public CoverBase currentCoverPosition;
        public int maxTries = 3;
        public int _curTries;
    }
    public CoverSettings cover;

    [System.Serializable]
    public class RetreatBehaviour
    {
        public List<Transform> retreatPositions = new List<Transform>();
        public List<Transform> exitLevelPositions = new List<Transform>();
        public bool hasAction;
        public bool exitLevel;
    }
    public RetreatBehaviour retreat;

    #region Variables
    public float delayTillNewBehaviour = 3f;
    float _timerTillNewBehaviour;
    bool lookAtPOI;
    bool initAlert;
    Vector3 pointOfIntrest;
    float currentWaitTime;
    int wayPointIndex = 0;
    float distance;
    Transform currentLookTransform;
    bool walkingToDest;
    bool setDestination;
    bool reachedDestination;
    bool desecding;
    float forward;
    bool aiming;
    bool initCheck;
    public Transform Target;
    public Vector3 targetLastKnownPosition;
    public Vector3 lastKnownPosition;
    float alertTimer;
    float alertTimerIncrement = 1;
    bool getPossibleHidingPositions;
    bool searchAtPositions;
    bool createSearchPositions;
    int indexSearchPosition;
    bool searchHigingSpots;
    Transform targetHidingSpot;
    ObjectDistanceComparer objectDistanceComparer;
    public List<CharecterStats> allCharacters=new List<CharecterStats>();
    float _delayAnim;
    bool validCover;
    int timeToShoot;
    bool onAimingAnimation;
    Vector3 targetDestination;
    #endregion

    RetreatActionBase currentRetratPosition;

    LevelManager levelManager;

    [HideInInspector]
    public PointofInterestBehaviour pointofInterest;

    public GameObject AlertLevel;

    public GameObject EnimiesUnit;

    public List<AI> AlliesNer = new List<AI>();

    public List<POI_Base> PointsOfInterestList = new List<POI_Base>();

    void Start() {
        levelManager = LevelManager.GEtINstance();
        retreat.exitLevelPositions = levelManager.exitPositions;
        charecterMove = GetComponent<CharecterMovement>();
        animator = GetComponent<Animator>();
        weaponHandeler = GetComponent<WeaponHandeler>();
        navMesh = GetComponentInChildren<NavMeshAgent>();
        pointofInterest = GetComponent<PointofInterestBehaviour>();
        characterStats = GetComponent<CharecterStats>();
        navMesh.speed = 0;
        navMesh.acceleration = 0;
        if (navMesh.stoppingDistance == 0)
            navMesh.stoppingDistance = 1.3f;
        AlertLevel.transform.localScale = Vector3.zero;
    }

    void Update() {

        if (!characterStats || characterStats.dead)
            return;

        charecterMove.Animate(forward, 0f);
        distance = Vector3.Distance(transform.position, patrolSettings.waypoints[wayPointIndex].destination.position);
        navMesh.transform.position = transform.position;
        weaponHandeler.aim = aiming;
        LookForTarget();
        //AlliesNear();
        switch (aIStates)
        {
            case AIStates.Patrol:
                search.alertMultiplier = 1;
                Patrol();
                DecreaseAlertLevel();
                pointofInterest.POIBehaviour();
                break;
            case AIStates.Attack:
                AlertAllies();
                if (!characterStats.hascover)
                    FireAtEnemy();
                else
                    AttackFromCover();
                //Retreat();
                break;
            case AIStates.FindCover:
                CoverBeheviour();
                break;
            case AIStates.FindEnemy:
                FindEnemy(targetLastKnownPosition);
                break;
            case AIStates.hasTarget:
                HasTarget();
                break;
            case AIStates.search:
                search.alertMultiplier = 0.5f;
                SearchBehaviour();
                DecreaseAlertLevel();
                break;
            case AIStates.deciding:
                DecideAttackBehaviourByStats();
                break;
            case AIStates.retreat:
                RetreatActionBase();
                break;
        }
        if(aIStates!=AIStates.FindCover && aIStates != AIStates.Attack)
        {
            if (cover.currentCoverPosition != null)
                cover.currentCoverPosition.occupied = false;
        }
        AlertLevel.transform.localScale = Vector3.one * (sight.alertlevel * 0.015f);

    }

    private void DecideAttackBehaviourByStats()
    {
        bool supPass = supressionPass();
        bool morPass = moralePass();

        Debug.Log("Supression " + supPass + " morale " + morPass);
        if (supPass && morPass)
        {
            aIStates = AIStates.Attack;
        }
        else
        {
            if (!supPass)
            {
                aIStates = AIStates.FindCover;
            }
        }

    }

    bool supressionPass()
    {
        int ranValu = Random.Range(0, 101);

        if (ranValu < characterStats.supressionLevel)
            return false;
        else
            return true;
    }

    bool moralePass()
    {
        int ranValue = Random.Range(0, 101);
        int health = Mathf.RoundToInt(characterStats.health / 10);
        int friendLies = 0;
        if (AlliesNer.Count > 0)
        {
            friendLies = 10;
            for (int i = 0; i <AlliesNer.Count; i++)
            {
                if (AlliesNer[i].characterStats.unitRank > characterStats.unitRank)
                {
                    friendLies += 10;
                }
            }
        }
        int modifiers = health + friendLies;
        ranValue -= modifiers;
        if (ranValue > characterStats.morale)
            return false;
        else
            return true;
    }

    void AlertAllies()
    {
        if (AlliesNer.Count > 0)
        {
            for (int i = 0; i < AlliesNer.Count ; i++)
            {
                if (AlliesNer[i].aIStates == AIStates.Patrol || AlliesNer[i].aIStates==AIStates.search)
                {
                    AlliesNer[i].targetLastKnownPosition = lastKnownPosition;
                    AlliesNer[i].AI_State_HasTarget();
                    AlliesNer[i].sight.alertlevel = 10;

                }
            }
        }
    }

    void Retreat()
    {
        if (AlliesNer.Count == 0)
            aIStates = AIStates.retreat;
    }

    public void AlertEveryOneInsideRange(float range)
    {
        LayerMask mask = 1 << gameObject.layer;
        Collider[] cols = Physics.OverlapSphere(transform.position, range,mask);
        Debug.Log(cols.Length);
        for (int i = 0; i < cols.Length ; i++)
        {
            Debug.Log(cols[i].gameObject.layer);
            if (cols[i].transform.GetComponent<AI>())
            {
                AI otherAI = cols[i].transform.GetComponent<AI>();

                if (otherAI.aIStates == AIStates.Patrol)
                {
                    otherAI.Target = Target;
                    otherAI.AI_State_HasTarget();
                    otherAI.sight.alertlevel = 10;
                }
            }
        }
    }

    public void DecreaseAlliesMorale(int amount)
    {
        if (AlliesNer.Count > 0)
        {
            for (int i = 0; i < AlliesNer.Count; i++)
            {
                AlliesNer[i].characterStats.morale -= amount;
            }
        }
    }

    void AttackFromCover()
    {
        if (!attack.startShooting)
        {
            LookAtPosition(lastKnownPosition);
            float attackPenalty = 0;
            attackPenalty = characterStats.supressionLevel * 0.01f;
            attack.attackR += Time.deltaTime;
            aiming = false;
            if(attack.attackR> attack.fireChance + attackPenalty)
            {
                ReEvaluateCover();
                if (validCover)
                {
                    characterStats.supressionLevel -= 10;
                    if (characterStats.supressionLevel < 0)
                        characterStats.supressionLevel = 0;
                    if (supressionPass())
                    {
                        attack.startShooting = true;
                        attack.timeShoot = 0;
                        timeToShoot = Random.Range(1, 5);
                        _delayAnim = 0;
                    }
                }
                else
                {
                    aiming = false;
                    cover.findCoverPosition = false;
                    cover._curTries = 0;
                    animator.SetBool("isCrouching", false);
                    aIStates = AIStates.FindCover;
                }
                attack.attackR = 0;
            }
        }
        else
        {
            aiming = true;
            _delayAnim += Time.deltaTime;
            if (_delayAnim > 1.5f)
            {
                if (Target != null)
                {
                    LookAtPosition(Target.position);
                    Vector3 start = transform.position + transform.up;
                    Vector3 dir = Target.position - transform.position;
                    Ray ray = new Ray(start, dir);
                    if (attack.timeShoot < timeToShoot)
                    {
                        weaponHandeler.currentWeapon.Fire(ray);
                        attack.timeShoot++;
                    }
                    else
                    {
                        attack.startShooting = false;
                    }

                }
                else if(Target == null && targetLastKnownPosition != Vector3.zero)
                {
                    attack.startShooting = false;
                    aiming = false;
                    attack.attackR = 0;
                    attack.timeShoot = 0;
                    animator.SetBool("isCrouching", false);
                    characterStats.hascover = false;
                    aIStates = AIStates.FindEnemy;
                }
            }
        }
    }

    void ReEvaluateCover()
    {
        Vector3 targetPosition = lastKnownPosition;
        Transform validatePosition = cover.currentCoverPosition.positionObject.parent.parent.transform;

        Vector3 directionOfTarget = targetPosition - validatePosition.position;
        Vector3 coverforward = validatePosition.TransformDirection(Vector3.forward);

        if (Vector3.Dot(coverforward, directionOfTarget) > 0)
        {
            if (cover.currentCoverPosition.backPos)
                validCover = false;
            else
                validCover = true;
        }
        else
        {
            if (cover.currentCoverPosition.backPos)
                validCover = true;
            else
                validCover = false;
        }
    }

    void RetreatActionBase()
    {
        if (!retreat.hasAction)
        {
            retreat.retreatPositions.Clear();
            for (int i = 0; i < levelManager.retreatActions.Count; i++)
            {
                if (!levelManager.retreatActions[i].visited)
                {
                    retreat.retreatPositions.Add(levelManager.retreatActions[i].retreatPosition);
                }
            }
            if (retreat.retreatPositions.Count > 0)
            {
                SortPosition(retreat.retreatPositions);
                currentRetratPosition = levelManager.ReturnRetreatPosition(retreat.retreatPositions[0]);
                if (currentRetratPosition.inUSe == false)
                {
                    currentRetratPosition.inUSe = true;
                    targetDestination = currentRetratPosition.retreatPosition.position;
                    retreat.exitLevel = false;
                }
                else
                {

                }
            }
            else
            {
                SortPosition(retreat.exitLevelPositions);
                targetDestination = retreat.exitLevelPositions[0].position;
                retreat.exitLevel = true;
            }
            retreat.hasAction = true;
        }
        else
        {
            if (retreat.exitLevel)
            {
                ExitLevel();
            }
            else
            {
                RetreatToPosition();
            }
        }
    }

    void ExitLevel()
    {
        AI_State_Retreat();
        if (!setDestination)
        {
            navMesh.SetDestination(targetDestination);
            setDestination = true;
        }
        if (Vector3.Distance(transform.position, targetDestination) < 1)
        {
            walkingToDest = false;
            setDestination = false;
            forward = LerpSpeed(forward, 0f, 30f);
            gameObject.SetActive(false);
        }
        else
        {
            walkingToDest = true;
            forward = LerpSpeed(forward, 1f, 5f);
            LookAtPosition(navMesh.steeringTarget);
            currentLookTransform = null;
        }
    }

    void RetreatToPosition()
    {
        AI_State_Retreat();
        if (!setDestination)
        {
            navMesh.SetDestination(targetDestination);
            setDestination = true;
        }
        if (Vector3.Distance(transform.position, targetDestination) < 1)
        {
            walkingToDest = false;
            setDestination = false;
            forward = LerpSpeed(forward, 0f, 30f);
            currentRetratPosition.visited = true;
            if (currentRetratPosition.reinforceMents.Count > 0)
            {
                for(int i = 0; i < currentRetratPosition.reinforceMents.Count; i++)
                {
                    if(currentRetratPosition.reinforceMents[i].aIStates==AIStates.Patrol)
                    {
                        currentRetratPosition.reinforceMents[i].targetLastKnownPosition = lastKnownPosition;
                        currentRetratPosition.reinforceMents[i].AI_State_HasTarget();
                        currentRetratPosition.reinforceMents[i].sight.alertlevel = 10;
                    }
                }
            }
            else
            {
                AlertEveryOneInsideRange(20);
            }
            characterStats.morale = 100;
            AI_State_HasTarget();

            retreat.hasAction = false;
            retreat.exitLevel = false;
        }
        else
        {
            walkingToDest = true;
            forward = LerpSpeed(forward, 1f, 5f);
            LookAtPosition(navMesh.steeringTarget);
            currentLookTransform = null;
        }
    }

    private void CoverBeheviour()
    {
        AI_State_FindCover();
        if (!cover.findCoverPosition)
        {
            FindCover();
        }
        else
        {
            if (!setDestination)
            {
                navMesh.SetDestination(cover.currentCoverPosition.positionObject.position);
                setDestination = true;
            }
            float distance = Vector3.Distance(transform.position, cover.currentCoverPosition.positionObject.position);
            if (distance <= 0.5f)
            {
                setDestination = false;
                walkingToDest = false;
                characterStats.hascover = true;
                forward = LerpSpeed(forward, 0f, 30f);
                animator.SetBool("isCrouching", true);
                aIStates = AIStates.Attack;
            }
            else
            {
                walkingToDest = true;
                forward = LerpSpeed(forward, 1f, 15f);
                LookAtPosition(navMesh.steeringTarget);
                currentLookTransform = null;
                animator.SetBool("isCrouching", false);
            }
        }
    }

    private void FindCover()
    {
       
        if (cover._curTries <= cover.maxTries)
        {
            if (!cover.findCoverPosition)
            {
                cover.findCoverPosition = true;
                cover._curTries++;
                CoverBase targetCoverPosition = null;
                float distanceToTarget = Vector3.Distance(transform.position, lastKnownPosition);
                cover.coverPositions.Clear();
                Vector3 targetPosition = lastKnownPosition;
                Collider[] colliders = Physics.OverlapSphere(transform.position, 20);
                for(int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i].GetComponent<CoverBehaviour>())
                    {
                        if (!cover.ignorePositions.Contains(colliders[i].transform))
                        {
                            float distanceToCandidate = Vector3.Distance(transform.position, colliders[i].transform.position);
                            if (distanceToCandidate < distanceToTarget)
                            {
                                cover.coverPositions.Add(colliders[i].transform);
                            }
                        }
                    }
                }
                if (cover.coverPositions.Count > 0)
                {
                    SortPosition(cover.coverPositions);
                    CoverBehaviour validatePosition = cover.coverPositions[0].GetComponent<CoverBehaviour>();
                    Vector3 directionTarget = targetPosition - validatePosition.transform.position;
                    Vector3 coverForward = validatePosition.transform.TransformDirection(Vector3.forward);
                    if (Vector3.Dot(coverForward, directionTarget) < 0)
                    {
                        for (int i = 0; i < validatePosition.BackPositions.Count; i++)
                        {
                            if (!validatePosition.BackPositions[i].occupied)
                            {
                                targetCoverPosition = validatePosition.BackPositions[i];
                            }
                        }
                    }
                    else
                    {
                        for(int i = 0; i < validatePosition.FrontPositions.Count; i++)
                        {
                            if (!validatePosition.FrontPositions[i].occupied)
                            {
                                targetCoverPosition = validatePosition.FrontPositions[i];
                            }
                        }
                    }
                }
                if (targetCoverPosition == null)
                {
                    cover.findCoverPosition = false;
                    if (cover.coverPositions.Count > 0)
                    {
                        cover.ignorePositions.Add(cover.coverPositions[0]);
                    }
                }
                else
                {
                    targetCoverPosition.occupied = true;
                    cover.currentCoverPosition = targetCoverPosition;
                }
            }
        }
        else
        {
            Debug.Log("Max tries Reached!");
            aIStates = AIStates.Attack;
        }
    }

    public void SortPosition(List<Transform> coverPositions)
    {
        objectDistanceComparer = new ObjectDistanceComparer(this.transform);
        coverPositions.Sort(objectDistanceComparer);
    }

    private class ObjectDistanceComparer : IComparer<Transform>
    {
        private Transform refernceObject;
        public ObjectDistanceComparer(Transform reference)
        {
            refernceObject = reference;
        }
        public int Compare(Transform x,Transform y)
        {
            float distX = Vector3.Distance(x.position, refernceObject.position);
            float distY = Vector3.Distance(y.position, refernceObject.position);
            int retVal = 0;
            if (distX < distY)
            {
                retVal = -1;
            }
            else if(distX > distY)
            {
                retVal = 1;
            }
            return retVal;
        }
    }

    void SearchBehaviour()
    {
        if (Target == null)
        {
            AI_State_Search();
            if (!search.decideBehaviour)
            {
                int ranValue = Random.Range(0, 11);
                if (ranValue < search.decideBehaviourThreshold)
                {
                    searchAtPositions = true;
                    searchHigingSpots = false;
                    Debug.Log("Searching in Positions Arount Unit");
                }
                else
                {
                    searchHigingSpots = true;
                    searchAtPositions = false;
                    Debug.Log("Searcing in Hiding Spots");
                }
                search.decideBehaviour = true;
            }
            else
            {
                #region search For HidingPositions
                if (searchHigingSpots)
                {
                    if (!search.populateListOfPositions)
                    {
                        search.possibleHigingPlaces.Clear();
                        Collider[] allColliders = Physics.OverlapSphere(transform.position, 20);
                        for (int i = 0; i < allColliders.Length; i++)
                        {
                            if (allColliders[i].GetComponent<HidingPlace>())
                            {
                                search.possibleHigingPlaces.Add(allColliders[i].transform);
                            }
                        }
                        search.populateListOfPositions = true;
                    }
                    else if (search.possibleHigingPlaces.Count > 0)
                    {
                        if (!targetHidingSpot)
                        {
                            int ranValue = Random.Range(0, search.possibleHigingPlaces.Count);
                            targetHidingSpot = search.possibleHigingPlaces[ranValue];
                        }
                        else
                        {
                            if (!setDestination)
                            {
                                navMesh.SetDestination(targetHidingSpot.position);
                                setDestination = true;
                            }

                            float distanceToTarget = Vector3.Distance(transform.position, targetHidingSpot.position);
                            if (distanceToTarget < 3f)
                            {
                                setDestination = false;
                                walkingToDest = false;
                                forward = LerpSpeed(forward, 0f, 1f);
                                _timerTillNewBehaviour += Time.deltaTime;
                                if (_timerTillNewBehaviour > delayTillNewBehaviour)
                                {
                                    search.populateListOfPositions = false;
                                    targetHidingSpot = null;
                                    search.decideBehaviour = false;
                                    _timerTillNewBehaviour = 0;
                                }
                            }
                            else
                            {
                                Debug.Log("Going To Hiding Spot");
                                walkingToDest = true;
                                forward = LerpSpeed(forward, 1f, 1);
                                LookAtPosition(navMesh.steeringTarget);
                                currentLookTransform = null;
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("No Hiding Spots Found");
                        searchAtPositions = true;
                        search.populateListOfPositions = false;
                        targetHidingSpot = null;
                    }
                }
                #endregion
                if (searchAtPositions)
                {
                    if (!createSearchPositions)
                    {
                        search.positionsArountUnit.Clear();
                        int ranalue = Random.Range(4, 10);
                        for (int i = 0; i < ranalue; i++)
                        {
                            float offsetX = Random.Range(-7, 7);
                            float offsetZ = Random.Range(-7, 7);

                            Vector3 originPos = transform.position;
                            originPos += new Vector3(offsetX, 0f, offsetZ);

                            NavMeshHit hit;
                            if (NavMesh.SamplePosition(originPos, out hit, 5, NavMesh.AllAreas))
                            {
                                search.positionsArountUnit.Add(hit.position);
                            }
                        }
                        if (search.positionsArountUnit.Count > 0)
                        {
                            indexSearchPosition = 0;
                            createSearchPositions = true;
                        }
                    }
                    searchAtPositions = false;
                }
                else
                {
                    Vector3 targetPosition = search.positionsArountUnit[indexSearchPosition];
                    if (!navMesh.isOnNavMesh)
                    {
                        return;
                    }
                    if (!setDestination)
                    {

                        navMesh.SetDestination(targetPosition);
                        setDestination = true;
                    }
                    float distaceToPosition = Vector3.Distance(transform.position, targetPosition);
                    if ((distaceToPosition <= navMesh.stoppingDistance) || reachedDestination && navMesh.pathPending)
                    {
                        setDestination = false;
                        walkingToDest = false;
                        forward = LerpSpeed(forward, 0f, 15f);
                        int retVal = Random.Range(0, 11);
                        search.decideBehaviour = (retVal<5);
                        currentWaitTime -= Time.deltaTime;
                        if (currentWaitTime <= 0)
                        {

                            reachedDestination = false;
                            if (search.CircularWaypoints)
                            {
                                if (indexSearchPosition < search.positionsArountUnit.Count - 1)
                                {
                                    indexSearchPosition++;
                                }
                                else
                                {
                                    indexSearchPosition = 0;
                                }
                            }
                            else
                            {
                                if (!desecding)
                                {
                                    if (indexSearchPosition == search.positionsArountUnit.Count - 1)
                                    {
                                        desecding = true;
                                        indexSearchPosition--;
                                    }
                                    else
                                    {
                                        indexSearchPosition++;
                                    }
                                }
                                else
                                {
                                    if (indexSearchPosition > 0)
                                    {
                                        indexSearchPosition--;
                                    }
                                    else
                                    {
                                        desecding = false;
                                        indexSearchPosition++;
                                    }
                                }
                            }

                        }
                        else
                        {
                            reachedDestination = true;
                        }
                    }
                    else
                    {

                        walkingToDest = true;
                        forward = LerpSpeed(forward, 0.5f,15f);
                        LookAtPosition(navMesh.steeringTarget);
                        currentWaitTime = search.waitTime;
                        currentLookTransform = null;

                    }

                }
            }
        }
        else
        {
            AI_State_DecideByStats();
        }
    }

    private void DecreaseAlertLevel()
    {
        if (sight.alertlevel > 0)
        {
            alertTimer += Time.deltaTime * search.alertMultiplier ;
            if(alertTimer > alertTimerIncrement * 2)
            {
                sight.alertlevel--;
                
                alertTimer = 0;
            }
        }
        if (sight.alertlevel == 0)
        {
            if (aIStates != AIStates.Patrol)
            {
                aIStates = AIStates.Patrol;
            }
        }
    }

    private void HasTarget()
    {
        if (Target != null)
        {
            if (sight.alertlevel < 10)
            {
                float distanceToTarget = Vector3.Distance(transform.position, Target.transform.position);
                float multiplier = 1 + (distanceToTarget * 0.1f);
                alertTimer += Time.deltaTime * multiplier;
                if (alertTimer > alertTimerIncrement)
                {
                    sight.alertlevel++;
                   
                    alertTimer = 0;
                }
            }
            else
            {
                 aIStates=AIStates.deciding;
            }
            LookAtPosition(lastKnownPosition);
        }
        else if (Target == null)
        {
            DecreaseAlertLevel();
            if (sight.alertlevel > 5)
            {
                aIStates = AIStates.FindEnemy;
            }
            else
            {
                _timerTillNewBehaviour += Time.deltaTime;
                if (_timerTillNewBehaviour > delayTillNewBehaviour)
                {
                    aIStates = AIStates.Patrol;
                    _timerTillNewBehaviour = 0;
                }
            }
        }

    }

    void AI_State_Normal()
    {
        aIStates = AIStates.Patrol;
        walkingToDest = false;
        lookAtPOI = false;
        initCheck = false;
        setDestination = false;
        reachedDestination = false;
        currentLookTransform = null;
    }

    public void AI_State_HasTarget()
    {
        aIStates = AIStates.hasTarget;
        forward = LerpSpeed(forward, 0f, 30f);
        lookAtPOI = false;
        initCheck = false;
        aiming = false;
        walkingToDest = false;
        setDestination = false;
        reachedDestination = false;
        currentLookTransform = null;
    }

    void AI_State_Search()
    {
        aiming = false;
        lookAtPOI = false;
        walkingToDest = false;
        setDestination = false;
        reachedDestination = false;
        currentLookTransform = null;
    }

    void AI_State_FindCover()
    {
        walkingToDest = false;
        lookAtPOI = false;
        aiming = false;
        initCheck = false;
        setDestination = false;
        reachedDestination = false;
        currentLookTransform = null;
    }

    void AI_State_DecideByStats()
    {
        aIStates = AIStates.deciding;
        walkingToDest = false;
        forward = LerpSpeed(forward, 0f, 15f);
        lookAtPOI = false;
        initCheck = false;
        setDestination = false;
        reachedDestination = false;
        currentLookTransform = null;
        characterStats.hascover = false;
    }

    void AI_State_Retreat()
    {
        animator.SetBool("isCrouching", false);
        aiming = false;
        characterStats.hascover = false;
        lookAtPOI = false;
        initCheck = false;
        cover.findCoverPosition = false;
        walkingToDest = false;
        reachedDestination = false;
        setDestination = false;
    }

    public void ChangeAIBehaviour(string logic, int index)
    {
        Invoke(logic, index);
    }

    private void FindEnemy(Vector3 targetPos)
    {
        if (Target != null)
        {
            aIStates = AIStates.Attack;
            sight.alertlevel = 10;
        }
        else
        {
            aiming = false;
            if (!navMesh.isOnNavMesh)
            {
                return;
            }
            if (!setDestination)
            {
                navMesh.SetDestination(targetPos);
                setDestination = true;
            }

            if ((navMesh.remainingDistance <= navMesh.stoppingDistance))
            {
                setDestination = false;
                walkingToDest = false;
                forward = LerpSpeed(forward, 0f, 5f);
                _timerTillNewBehaviour += Time.deltaTime;
                if (_timerTillNewBehaviour > delayTillNewBehaviour)
                {
                    if (alert.canChase)
                        aIStates = AIStates.search;
                    else
                        aIStates = AIStates.Patrol;
                    _timerTillNewBehaviour = 0;
                }
            }
            else
            {
                walkingToDest = true;
                forward = LerpSpeed(forward, 1f, 5f);
                LookAtPosition(navMesh.steeringTarget);
                currentLookTransform = null;

            }
        }
    }

    void LookForTarget()
    {
        if (allCharacters.Count > 0)
        {
            foreach (CharecterStats c in allCharacters)
            {
                if (c != characterStats && c.faction != characterStats.faction && c == ClosestEnemy())
                {
                    RaycastHit hit;
                    Vector3 start = transform.position + (transform.up * sight.eyeHeight);
                    Vector3 dir = (c.transform.position + c.transform.up) - start;
                    Debug.DrawRay(start, dir,Color.yellow);
                    float sightAngle = Vector3.Angle(dir, transform.forward);
                    if (Physics.Raycast(start, dir, out hit, sight.sightRange, sight.sightlayers) &&
                        sightAngle < sight.fieldofView && hit.collider.GetComponent<CharecterStats>())
                    {
                        Target = hit.transform;
                        lastKnownPosition = Target.position;
                    }
                    else
                    {
                        if (Target != null)
                        {
                            targetLastKnownPosition = Target.position;
                            Target = null;
                        }
                        
                    }
                }
            }
        }
    }

    CharecterStats ClosestEnemy()
    {
        CharecterStats closestCharacter = null;
        float minDistance = Mathf.Infinity;
        foreach (CharecterStats c in allCharacters)
        {
            if (c != characterStats && c.faction != characterStats.faction)
            {
                float distToCharacter = Vector3.Distance(c.transform.position, transform.position);
                if (distToCharacter < minDistance)
                {
                    closestCharacter = c;
                    minDistance = distToCharacter;
                }
            }
        }
        return closestCharacter;
    }

    void Patrol()
    {
        if (Target == null)
        {
            PatrolBehaviour();
            if (!navMesh.isOnNavMesh)
            {
                return;
            }
            if (patrolSettings.waypoints.Count == 0)
                return;
            if (!setDestination)
            {
          
                navMesh.SetDestination(patrolSettings.waypoints[wayPointIndex].destination.position);
                setDestination = true;
            }
           
            if (( distance <= navMesh.stoppingDistance) || reachedDestination && navMesh.pathPending)
            {
          
                setDestination = false;
                walkingToDest = false;
                forward = LerpSpeed(forward, 0f, 5f);
                currentWaitTime -= Time.deltaTime;
                if (patrolSettings.waypoints[wayPointIndex].LookAtTarget != null)
                    currentLookTransform = patrolSettings.waypoints[wayPointIndex].LookAtTarget;
                if (currentWaitTime <= 0)
                {
               
                    reachedDestination = false;
                    if (patrolSettings.CircularWayPoints)
                    {
                        if (wayPointIndex < patrolSettings.waypoints.Count-1)
                        {
                            wayPointIndex++;
                        }
                        else
                        {
                            wayPointIndex = 0;
                        }
                    }
                    else
                    {
                        if (!desecding)
                        {
                            if (wayPointIndex == patrolSettings.waypoints.Count - 1)
                            {
                                desecding = true;
                                wayPointIndex--;
                            }
                            else
                            {
                                wayPointIndex++;
                            }
                        }
                        else
                        {
                            if (wayPointIndex > 0)
                            {
                                wayPointIndex--;
                            }
                            else
                            {
                                desecding = false;
                                wayPointIndex++;
                            }
                        }
                    }
                    
                }
                else
                {
                    reachedDestination = true;
                }
            }
            else
            {
                
                walkingToDest = true;
                forward = LerpSpeed(forward, 0.5f, 5);
                LookAtPosition(navMesh.steeringTarget);
                currentWaitTime = patrolSettings.waypoints[wayPointIndex].waittime;
                currentLookTransform = null;

            }
        }
        else if(Target!=null)
        {
            ChangeAIBehaviour("AI_State_HasTarget",0);
        }
       
    }

    void FireAtEnemy()
    {
        if (Target != null)
        {
            AttackBehaviour();
            LookAtPosition(Target.position);
            Vector3 start = transform.position + transform.up;
            Vector3 dir = Target.position - transform.position;
            Ray ray = new Ray(start, dir);
            if (Random.value <= attack.fireChance)
                weaponHandeler.currentWeapon.Fire(ray);
        }
        else if (Target==null && targetLastKnownPosition!=Vector3.zero)
        {
            if (sight.alertlevel > 5)
            {
                aIStates = AIStates.FindEnemy;
            }
            else
            {
                aIStates = AIStates.Patrol;
            }
        }
    }

    float LerpSpeed(float curSpeed, float destSpeed, float time)
    {
        return curSpeed = Mathf.Lerp(curSpeed, destSpeed, Time.deltaTime * time);
    }

    void LookAtPosition(Vector3 pos)
    {
        Vector3 dir =pos - transform.position ;
        Quaternion LookRot = Quaternion.LookRotation(dir);
        LookRot.x = 0;
        LookRot.z = 0;
        transform.rotation = Quaternion.Lerp(transform.rotation,LookRot,Time.deltaTime*5f);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (currentLookTransform != null && !walkingToDest)
        {
                animator.SetLookAtPosition(currentLookTransform.position);
                animator.SetLookAtWeight(1f,0f,0.5f,0.7f);
        }
        else if (Target != null)
        {
            float dist = Vector3.Distance(Target.position, transform.position);
            if (dist > 3f)
            {
                animator.SetLookAtPosition(Target.transform.position +  transform.right *0.3f);
                animator.SetLookAtWeight(1f, 1f, 0.3f, 0.2f);
            }
            else
            {
                animator.SetLookAtPosition(Target.transform.position + Target.up + transform.right*0.3f);
                animator.SetLookAtWeight(1f, 1f, 0.3f, 0.2f);
            }

        }
    }

    void PatrolBehaviour()
    {
        aiming = false;
        lookAtPOI = false;
        walkingToDest = false;
        setDestination = false;
        reachedDestination = false;
        currentLookTransform = null;
        AlertLevel.transform.localScale = Vector3.zero;
    }

    void AttackBehaviour()
    {
        aiming = true;
        walkingToDest = false;
        setDestination = false;
        reachedDestination = false;
        forward = LerpSpeed(forward,0f,15f);
        currentLookTransform = null;
    }

    void AlliesNear()
    {
        foreach (Transform c in EnimiesUnit.transform)
        {
            AI otherAI = c.GetComponent<AI>();
            if (otherAI)
            {
                if (otherAI != this)
                {
                    if (!AlliesNer.Contains(otherAI))
                    {
                        AlliesNer.Add(otherAI);
                    }
                }
            }
        }
    }
}
[System.Serializable]
public class WaypointBase
{
    public Transform destination;
    public float waittime;
    public Transform LookAtTarget;
    public bool overrideAnimation;
    public string[] animationRoutines;
}
