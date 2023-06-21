using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAI : MonoBehaviour
{
    const float PathUpdatingRepeatRate = 2f;

    #region Navigation

    private const float MinDistanceThreshold = 2f;
    private const float NextWaypointDistance = 2f;

    private Transform _target;

    private Transform Target
    {
        get
        {
            return _target;
        }

        set
        {
            _target = value;
        }
    }

    public bool HasTargetToFollow
    {
        get
        {
            return _target != null;
        }
    }

    private NullableVector3 _desiredPosition;

    private NullableVector3 DesiredPosition
    {
        get
        {
            return _desiredPosition;
        }

        set
        {
            _desiredPosition = value;
        }
    }

    private float _desiredFollowingDistance;

    private float FollowingDistance
    {
        get
        {
            return Mathf.Max(0, MinDistanceThreshold, _desiredFollowingDistance);
        }

        set
        {
            _desiredFollowingDistance = Mathf.Max(0, MinDistanceThreshold, value);
        }
    }

    private UnityEngine.AI.NavMeshPath _currentPath;
    private int _currentWaypointIndex;

    public bool IsNearToDesiredPosition
    {
        get
        {
            if (_currentPath == null)
            {
                return true;
            }
            else
            {
                return Vector3.Distance(transform.position, _currentPath.corners[^1]) <= FollowingDistance;
            }
        }
    }

    public Vector3 RawDesiredDirection { get; private set; }
    public Vector3 SmoothDesiredDirection { get; private set; }

    private Vector3 _smoothDesiredDirectionRefVelocity;

    #region Path Calculation

    private Coroutine _pathUpdatingCoroutine;

    private void UpdatePath(bool distanceBasedOnPathLength = true)
    {
        if (DesiredPosition == null)
        {
            _currentPath = null;
            _currentWaypointIndex = 0;
            return;
        }

        UnityEngine.AI.NavMeshPath newPath = CustomUE.GetPathBetweenPositions(transform.position, (Vector3)DesiredPosition);

        float distance;

        if (distanceBasedOnPathLength)
        {
            distance = CustomUE.GetPathLength(newPath);
        }
        else
        {
            distance = Vector3.Distance(transform.position, newPath.corners[^1]);
        }

        if (distance <= FollowingDistance)
        {
            if (Target == null)
            {
                DesiredPosition = null;
                StopUpdatingPath();
            }
            _currentPath = null;
            _currentWaypointIndex = 0;
        }
        else
        {
            _currentPath = newPath;
            _currentWaypointIndex = 0;
        }
    }

    private void StartUpdatingPath(float startTime, float repeatRate)
    {
        StopUpdatingPath();
        _pathUpdatingCoroutine = StartCoroutine(UpdatePathCoroutine(startTime, repeatRate));
    }

    private void StopUpdatingPath()
    {
        if (_pathUpdatingCoroutine != null)
        {
            StopCoroutine(_pathUpdatingCoroutine);
            _pathUpdatingCoroutine = null;
        }

        _currentPath = null;
        _currentWaypointIndex = 0;
    }

    private IEnumerator UpdatePathCoroutine(float startTime, float repeatRate)
    {
        startTime = Mathf.Max(0, startTime);
        repeatRate = Mathf.Max(0, repeatRate);

        if (startTime > 0)
        {
            yield return new WaitForSeconds(startTime);
        }

        UpdatePath();

        while (true)
        {
            yield return new WaitForSeconds(repeatRate);

            UpdatePath();
        }
    }

    #endregion

    #region Debug

    public Vector3 DebugRawDesiredDirection;
    public Vector3 DebugSmoothDesiredDirection;

    #endregion

    private void Update()
    {
        if (HasTargetToFollow)
        {
            DesiredPosition = Target.transform.position;
        }

        if (DesiredPosition == null || _currentPath == null)
        {
            RawDesiredDirection = Vector3.zero;
            return;
        }

        _currentWaypointIndex = Mathf.Max(_currentWaypointIndex, 0);

        if (_currentWaypointIndex >= _currentPath.corners.Length)
        {
            _currentPath = null;
            RawDesiredDirection = Vector3.zero;
            return;
        }

        RawDesiredDirection = (_currentPath.corners[_currentWaypointIndex] - transform.position).normalized;

        float distance = Vector3.Distance(transform.position, _currentPath.corners[_currentWaypointIndex]);

        if (distance < NextWaypointDistance)
        {
            _currentWaypointIndex++;
        }
    }

    private void FixedUpdate()
    {
        SmoothDesiredDirection = Vector3.SmoothDamp(SmoothDesiredDirection, RawDesiredDirection, ref _smoothDesiredDirectionRefVelocity, .1f);

        #region Debug

        DebugRawDesiredDirection = RawDesiredDirection;
        DebugSmoothDesiredDirection = SmoothDesiredDirection;

        Debug.DrawRay(transform.position, RawDesiredDirection, Color.blue);

        if (_currentPath != null)
        {
            for (int i = 0; i < _currentPath.corners.Length - 1; i++)
            {
                Vector3 currentWayPoint = _currentPath.corners[i];
                Vector3 direction = _currentPath.corners[i + 1] - currentWayPoint;

                Debug.DrawRay(currentWayPoint, direction, Color.red);
            }
        }

        //Debug.Log(IsInDesiredPosition);

        #endregion
    }

    public void SetDestination(Transform target, float followingDistance = 0)
    {
        if (_target == target)
        {
            return;
        }

        StopUpdatingPath();
        _target = target;

        if (_target == null)
        {
            _desiredPosition = null;
            FollowingDistance = 0;
            _currentPath = null;
            _currentWaypointIndex = 0;
            return;
        }

        _desiredPosition = _target.transform.position;
        FollowingDistance = followingDistance;

        StartUpdatingPath(0, PathUpdatingRepeatRate);
    }

    public void SetDestination(NullableVector3 position, float followingDistance = 0)
    {
        _target = null;

        if (_desiredPosition == position)
        {
            return;
        }

        StopUpdatingPath();
        _desiredPosition = position;

        if (_desiredPosition == null)
        {
            FollowingDistance = 0;
            _currentPath = null;
            _currentWaypointIndex = 0;
            return;
        }

        FollowingDistance = followingDistance;

        StartUpdatingPath(0, PathUpdatingRepeatRate);
    }

    public void CancelDestination()
    {
        _target = null;
        _desiredPosition = null;
        FollowingDistance = 0;
        StopUpdatingPath();
    }

    #endregion
}

