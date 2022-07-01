using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = Unity.Mathematics.Random;
using UnityEngine.Events;

public class RobotAI : MonoBehaviour
{
    enum CoverStatus {None, RunningTo, AtCover }

    [SerializeField] private float _aiHealth;
    private float _defaultHealth;
    private Vector3 _currDestination;
    private NavMeshAgent _navMeshAgent;
    [SerializeField] private Vector3 _waypointEndPosition;
    [SerializeField] private GameObject _waypointsParentGO;
    [SerializeField] private int _numOfWaypoints;
    [SerializeField] private Transform[] _waypoints;
    [Space] [Space]
    [SerializeField] private GameObject _coverWaypointsParentGO;
    [SerializeField] private int _numOfCoverWaypoints;
    [SerializeField] private Transform[] _coverWaypoints;
    [SerializeField] private bool _coverCooldown;
    [SerializeField] private bool _waitToRun = false;
    [SerializeField] private CoverStatus _coverStatus;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _animSpeed;
    [SerializeField] private bool _animDeath;
    [SerializeField] private bool _animHiding;


    private void OnEnable()
    {
        if (_navMeshAgent != null)
        {
            _navMeshAgent.isStopped = false;
            _navMeshAgent.destination = _currDestination;
            SetAnimationState("WalkToRun");
        }
    }

    private void Start()
    {
        if (_aiHealth < 1) { _aiHealth = 100; Debug.Log("Player:Start() _aiHealth is < 1! Set to 100."); }
        _defaultHealth = _aiHealth;
        
        _navMeshAgent = GetComponent<NavMeshAgent>();
        LoadWaypointArrays(ref _waypointsParentGO, "Waypoints", ref _waypoints, ref _numOfWaypoints);
        LoadWaypointArrays(ref _coverWaypointsParentGO, "CoverWaypoints", ref _coverWaypoints,
            ref _numOfCoverWaypoints);
        _coverStatus = CoverStatus.None;

        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogError("RobotAI|Start() _animator is NULL!");
        }

        _animDeath = false;
        _animHiding = false;
        _animSpeed = 0f;

        _waypointEndPosition = _waypoints[_numOfWaypoints - 1].position;

        //AI will move to end destination unless interrupted
        if (_waypointEndPosition == null)
        {
            Debug.LogError("RobotAI|Start() _waypointEndPosition is NULL!");
        }
        else
        {
            _currDestination = _waypointEndPosition;
        }

        _navMeshAgent.destination = _currDestination;

        SetAnimationState("WalkToRun");

        //Testing --------------------------------
        //StartCoroutine(TestingCoroutine());
        //StartCoroutine(TestingHitDetected());
        //----------------------------------------
    }

    //TESTING COROUTINES AND METHODS ------------------------
    private IEnumerator TestingCoroutine()
    {
        yield return new WaitForSeconds(8f);
        SetCoverStatus(CoverStatus.RunningTo);
    }

    private IEnumerator TestingHitDetected()
    {
        yield return new WaitForSeconds(5f);
        TestingDebugLogHit();
        RunToCover();
        yield return new WaitForSeconds(1f);
        TestingDebugLogHit();
        RunToCover();
        yield return new WaitForSeconds(7f);
        TestingDebugLogHit();
        RunToCover();
        yield return new WaitForSeconds(9f);
        TestingDebugLogHit();
        RunToCover();
        yield return new WaitForSeconds(15f);
        TestingDebugLogHit();
        RunToCover();
        yield return new WaitForSeconds(19f);
        TestingDebugLogHit();
        RunToCover();
        yield return new WaitForSeconds(15f);
        TestingDebugLogHit();
        RunToCover();
        yield return new WaitForSeconds(11f);
        TestingDebugLogHit();
        RunToCover();
    }

    private void TestingDebugLogHit()
    {
        Debug.Log("Hit detected!");
    }
    //--------------------------------------------------------
    
    void Update()
    {
        // DetectHitOrNearMiss();
        
        if (!_navMeshAgent.isStopped)
        { Move(); }
    }

    private void Move()
    {
            //Check distance to next waypoint
            if (_coverStatus == CoverStatus.RunningTo && _navMeshAgent.remainingDistance < 1f)
            {
                SetCoverStatus(CoverStatus.AtCover);
            }
            else if (_coverStatus == CoverStatus.None && _currDestination == _waypointEndPosition && _navMeshAgent.remainingDistance < 1f)  //TO DO This runs at start of game, need to skip at start of game
            {
                //SetCoverStatus(CoverStatus.AtCover);  //This will keep the AI from running indefinitely after reaching the end
                //Deal with reached end, need respawn logic
                //ReachedEnd();
            }
            else
            {
                //Run towards end position
            }
    }

    private void SetCoverStatus(CoverStatus newCoverStatus)
    {
        switch (newCoverStatus)
        {
            case CoverStatus.None: //Run to endpoint
                switch (_coverStatus)
                {
                    case CoverStatus.None:
                        //Should only occur on first spawn, do nothing
                        SetAnimationState("Run");
                        break;
                    case CoverStatus.AtCover:
                            //Was idling at cover
                            SetAnimationState("WalkToRun");
                            break;
                    case CoverStatus.RunningTo:
                            //Was running to cover, resume running to end point
                            _currDestination = _waypointEndPosition;
                            break;
                }
                break;
            case CoverStatus.RunningTo: //Run to cover
                switch (_coverStatus)
                {
                    case CoverStatus.None:
                        //Was running to end point, now running to nearest cover
                        if (!_coverCooldown)
                        {
                            _currDestination = GetNearestCoverWaypoint();
                            _navMeshAgent.destination = _currDestination; 
                        }
                        break;
                    case CoverStatus.AtCover:
                        //Already at cover, do nothing
                        break;
                    case CoverStatus.RunningTo:
                        //Should not occur, do nothing
                        break;
                }
                break;
            case CoverStatus.AtCover: //Reached cover, stay for set duration
                switch (_coverStatus)
                {
                    case CoverStatus.None:
                        //Should not occur, do nothing | if does occur, same as RunningTo
                        break;
                    case CoverStatus.AtCover:
                        //Should not occur, do nothing
                        break;
                    case CoverStatus.RunningTo:
                        //Was running to cover, now at cover
                        SetAnimationState("CoverIdle");
                        StartCoroutine(WaitToRun());
                        StartCoroutine(StartCoverCooldown());
                        break;
                }
                break;
        }
        _coverStatus = newCoverStatus;
    }

    private void SetAnimationState(string animationState)
    {
        //WalkToRun | CoverIdle | Run
        switch (animationState)
        {
            case "Run":
                _navMeshAgent.isStopped = false;
                _animator.SetFloat("Speed", 5f);
                _animator.SetBool("Hiding", false);
                break;
            case "WalkToRun":
                _navMeshAgent.isStopped = false;
                _animator.SetBool("Hiding", false);
                StartCoroutine(WalkToRun());
                break;
            case "CoverIdle":
                _navMeshAgent.isStopped = true;
                _animator.SetBool("Hiding", true);
                _animHiding = true;
                break;
        }
    }

    private IEnumerator WalkToRun()
    {
        _animator.SetFloat("Speed", 2f);
        yield return new WaitForSeconds(0.5f);
        _animator.SetFloat("Speed", 5f);
    }

    private IEnumerator StartCoverCooldown()
    {
        _coverCooldown = true;
        yield return new WaitForSeconds(3f);
        _coverCooldown = false;
    }

    private Vector3 GetNearestCoverWaypoint()
    {
        float shortestDistance = 1000f;
        int closestCoverWPID = 0;
        Transform closestCoverWP;
        
        for (int i = 0; i < _numOfCoverWaypoints; i++)
        {
            float tempDistance = Vector3.Distance(_coverWaypoints[i].position, transform.position);
            if (tempDistance < shortestDistance)
            {
                shortestDistance = tempDistance;
                closestCoverWPID = i;
            }
        }

        closestCoverWP = _coverWaypoints[closestCoverWPID];
        
        //Determine if closest waypoint is behind AI
        //1st floor - if waypoint.x > transform.x      cover waypoints 1 - 4 (0 to 3)
        //2nd floor - if waypoint.x < transform.x      5 - 9                 (4 to 8)
        //3rd floor - same as 1st                      remaining             ( > 8)

        bool nextWaypointIsPositiveX = !(closestCoverWPID > 3 && closestCoverWPID < 9);

        if (closestCoverWPID + 1 < _numOfCoverWaypoints)
        {
            if (nextWaypointIsPositiveX == true)
            {
                //Debug.Log("nextWPisPositiveX = true");
                if (closestCoverWP.position.x > transform.position.x) { closestCoverWP = _coverWaypoints[closestCoverWPID + 1]; }
            }
            else
            {
                //Debug.Log("nextWPisPositiveX = false");
                if (closestCoverWP.position.x < transform.position.x) { closestCoverWP = _coverWaypoints[closestCoverWPID + 1]; }
            }
        }
        
        return closestCoverWP.position;
    }

    private void RunToCover()
    {
        switch (_coverStatus)
        {
            case CoverStatus.None:  //Already running
                //Move to nearest cover
                SetCoverStatus(CoverStatus.RunningTo);
                break;
            case CoverStatus.RunningTo: //Already running to cover
                //Do nothing, in progress
                break;
            case CoverStatus.AtCover: //Already at cover
                //Reset coroutine WaitToRun
                WaitToRun();
                break;
        }
    }
    
    private IEnumerator WaitToRun()
    {
        float waitTime = UnityEngine.Random.Range(3f, 7f);   
        yield return new WaitForSeconds(waitTime);
        _currDestination = _waypointEndPosition;
        _navMeshAgent.destination = _currDestination;
        SetCoverStatus(CoverStatus.None);
    }
    
    private void LoadWaypointArrays(ref GameObject parentGO, string parentGOName, ref Transform[] waypointsArray, ref int numOfWaypoints)
    {
        parentGO = GameObject.Find(parentGOName);
        
        if (parentGO == null) {Debug.LogError("RobotAI|LoadWaypointArrays() "+ parentGOName + " is NULL!");}
        else
        {
            numOfWaypoints = parentGO.GetComponentInChildren<Transform>().childCount;
            waypointsArray = new Transform[numOfWaypoints];

            int i = 0;
            foreach (Transform wp in parentGO.GetComponentInChildren<Transform>())
            { waypointsArray[i] = wp; i++; }
        }
    }
    
    public void ReachedEnd()
    {
        ResetValues();
    }

    public void ResetValues()
    {
        _navMeshAgent.isStopped = true;
        _aiHealth = _defaultHealth;
        _coverStatus = CoverStatus.None;
        _currDestination = _waypointEndPosition;
        _waitToRun = false;
        _animDeath = false;
        _animHiding = false;
        _animSpeed = 0f;
        _animator.ResetTrigger("Death");
        _animator.SetFloat("Speed", 0f);
        _animator.SetBool("Hiding", false);
        transform.SetPositionAndRotation(_waypoints[0].position, quaternion.Euler(0f, -90f, 0f));
        this.GameObject().SetActive(false);
    }

    public bool TakeDamage(float damageAmount) {
        _aiHealth -= damageAmount;
        if (_aiHealth <= 0) {
            OnDeath();
            return true;
        }
        RunToCover();
        return false;
    }

    public void DetectNearMiss()
    { RunToCover(); }
    
    private void OnDeath() {
        _animator.SetTrigger("Death");
        StartCoroutine(waitForDeathAnimation());
    }

    IEnumerator waitForDeathAnimation() {
        yield return new WaitForSeconds(3.3f);
        ResetValues(); 
    }
}
