using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;

public class RobotAI : MonoBehaviour
{
    enum CoverStatus {None, RunningTo, AtCover }
    private Vector3 _currDestination;
    private NavMeshAgent _navMeshAgent;
    [SerializeField] private Vector3 _waypointEndPosition;
    [SerializeField] private GameObject _waypointsParentGO;
    [SerializeField] private Transform[] _waypoints;
    [SerializeField] private int _numOfWaypoints;
    [SerializeField] private bool _reachedEnd = false;
    [SerializeField] private bool _waitToRun = false;
    [SerializeField] private CoverStatus _coverStatus;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _animSpeed;
    [SerializeField] private bool _animDeath;
    [SerializeField] private bool _animHiding;


    void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _waypointsParentGO = GameObject.Find("Waypoints");
        _coverStatus = CoverStatus.None;
        
        _animator = GetComponent<Animator>();
        if (_animator == null) { Debug.LogError("RobotAI|Start() _animator is NULL!"); }
        
        _animDeath = false;
        _animHiding = false;
        _animSpeed = 0f;

        if (_waypointsParentGO == null) { Debug.LogError("RobotAI|Start() _waypointsParentGO is NULL!"); }
        else
        {
            _numOfWaypoints = _waypointsParentGO.GetComponentInChildren<Transform>().childCount;
            _waypoints = new Transform[_numOfWaypoints];
            
            int i = 0;
            foreach (Transform wp in _waypointsParentGO.GetComponentInChildren<Transform>())
            { _waypoints[i] = wp; i++; }
        }
        
        _waypointEndPosition = _waypoints[_numOfWaypoints - 1].position;

        //AI will move to end destination unless interrupted
        if (_waypointEndPosition == null) {Debug.LogError("RobotAI|Start() _waypointEndPosition is NULL!");}
        else { _currDestination = _waypointEndPosition; }

        _navMeshAgent.destination = _currDestination;
        
        SetAnimationState("WalkToRun");

        //Testing --------------------------------
        StartCoroutine(TestingCoroutine());
        //----------------------------------------
    }
    private IEnumerator TestingCoroutine()
    {
        yield return new WaitForSeconds(8f);
        SetCoverStatus(CoverStatus.RunningTo);
    }
    
    void Update()
    {
        DetectHitOrNearMiss();
        
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
                        _currDestination = GetNearestCoverWaypoint();
                        _navMeshAgent.destination = _currDestination;
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
        yield return new WaitForSeconds(2f);
        _animator.SetFloat("Speed", 5f);
    }

    private Vector3 GetNearestCoverWaypoint()
    {
        return new Vector3(0f, 0f, 4f); //Temp to avoid error, remove later
    }
    
    private void DetectHitOrNearMiss()
    {
        //Detect hit or near miss
        
        //if hit or near miss
        
        // switch (_coverStatus)
        // {
        //     case CoverStatus.None:
        //         //Move to nearest cover
        //         _coverStatus = CoverStatus.RunningTo;
        //         break;
        //     case CoverStatus.RunningTo:
        //         //Do nothing, in progress
        //         
        //         break;
        //     case CoverStatus.AtCover:
        //         //Reset coroutine WaitToRun
        //         WaitToRun();
        //         break;
        // }
    }

    private IEnumerator WaitToRun()
    {
        yield return new WaitForSeconds(3f);
        //Start running towards endpoint
        _currDestination = _waypointEndPosition;
        _navMeshAgent.destination = _currDestination;
        SetCoverStatus(CoverStatus.None);
    }
    
    private void ReachedEnd()
    {
        //Tell spawn manager or use collider on spawn manager to detect by GO tag?
        //ResetValues();
        _reachedEnd = true;
        _navMeshAgent.isStopped = true;
    }

    public void ResetValues()
    {
        //called from spawn manager before repositioning at start waypoint
        _coverStatus = CoverStatus.None;
        _currDestination = _waypointEndPosition;
        _reachedEnd = false;
        _waitToRun = false;
        _animDeath = false;
        _animHiding = false;
        _animSpeed = 0f;
        _animator.ResetTrigger("Death");
        _animator.SetFloat("Speed", 0f);
        _animator.SetBool("Hiding", false);
        SetAnimationState("WalkToRun");
        _navMeshAgent.isStopped = false;
    }

    private void OnDeath()
    {
        _animator.SetTrigger("Death");
    }
}
