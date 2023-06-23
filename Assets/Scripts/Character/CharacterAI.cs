using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAI : MonoBehaviour
{
    private CharacterController _cc;

    private void Start()
    {
        _cc = GetComponent<CharacterController>();
    }

    #region Navigation

    const float PathUpdatingRepeatRate = 1f;

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

    #region NLP

    const string ReachIntent = "Reach";
    const string InspectIntent = "Inspect";
    const string PickIntent = "Pick";
    const string LeaveIntent = "Leave";
    const string ThrowIntent = "Throw";
    const string ComplimentIntent = "Compliment";
    const string InsultIntent = "Insult";

    const string ToReachRole = "ToReach";
    const string ToInspectRole = "ToInspect";
    const string ToPickRole = "ToPick";
    const string ToLeaveRole = "ToLeave";
    const string ToThrowRole = "ToThrow";
    const string ToHitRole = "ToHit";

    const float MinPredictionProbabilityThreshold = .4f;

    private enum UnderstandingErrorType
    {
        UnrecognisedIntent,
        MissingEntities,
        EntitiesNumberIconsistency,
    }

    private Coroutine _understandingCommandCoroutine;

    public void GiveCommand(string sentence)
    {
        if (_cc == null || _cc.IsBusy || _understandingCommandCoroutine != null)
        {
            return;
        }

        _understandingCommandCoroutine = StartCoroutine(UnderstandCommandCoroutine(sentence));
    }

    private IEnumerator UnderstandCommandCoroutine(string sentence)
    {
        Command command = null;

        #region Parsing command

        yield return new WaitForSeconds(0);

        float predictionProbability = 1;
        string recognizedIntent = InspectIntent;
        List<Command.RecognizedEntity> recognizedEntities = new List<Command.RecognizedEntity>();
        recognizedEntities.Add(new Command.RecognizedEntity(ToInspectRole, "Bush"));

        command = new Command(recognizedIntent, recognizedEntities);

        #endregion

        List<UnderstandingErrorType> errors = new List<UnderstandingErrorType>();

        if (predictionProbability < MinPredictionProbabilityThreshold)
        {
            errors.Add(UnderstandingErrorType.UnrecognisedIntent);
            _cc.DoConfusedExpression();
            yield return null;
        } else
        {
            Context contextComponent = null;

            #region Getting Context

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1);

            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.gameObject.layer == GameConsts.ContextLayer())
                {
                    contextComponent = hitCollider.GetComponent<Context>();
                    break;
                }
            }

            #endregion

            if (contextComponent == null)
            {
                _cc.DoConfusedExpression();
                yield return null;
            }

            switch (command.Intent)
            {
                case ReachIntent:

                    List<GameObject> gosToReach = LocateRecognizedEntitiesInContext(command, contextComponent, ToReachRole);

                    if (gosToReach.Count == 0)
                    {
                        errors.Add(UnderstandingErrorType.MissingEntities);
                        break;
                    }
                    else if (gosToReach.Count == 1)
                    {
                        _cc.ReachPosition(gosToReach[0].transform.position);
                    }
                    else if (gosToReach.Count > 1)
                    {
                        errors.Add(UnderstandingErrorType.EntitiesNumberIconsistency);
                    }

                    break;
                case PickIntent:

                    List<GameObject> gosToPick = LocateRecognizedEntitiesInContext(command, contextComponent, ToPickRole);

                    break;
                case InspectIntent:

                    List<GameObject> gosToInpect = LocateRecognizedEntitiesInContext(command, contextComponent, ToInspectRole);

                    if (gosToInpect.Count == 0)
                    {
                        errors.Add(UnderstandingErrorType.MissingEntities);
                        break;
                    }
                    else if (gosToInpect.Count == 1)
                    {
                        _cc.Inspect(gosToInpect[0].GetComponent<ItemsHidingPlace>());
                    }
                    else if (gosToInpect.Count > 1)
                    {
                        errors.Add(UnderstandingErrorType.EntitiesNumberIconsistency);
                    }

                    break;
            }

        }

        _understandingCommandCoroutine = null;
    }

    private List<GameObject> LocateRecognizedEntitiesInContext(Command command, Context context, string role)
    {
        List<GameObject> entitiesInContext = new List<GameObject>();

        List<string> recognizedEntities = command.GetEntitiesWithRole(role);

        foreach (string recognizedEntity in recognizedEntities)
        {
            foreach (Entity contextEntity in context.GetEntitiesInContext())
            {
                if (contextEntity.AnswersToTheNameOf(recognizedEntity))
                {
                    entitiesInContext.Add(contextEntity.gameObject);
                    break;
                }
            }
        }

        return entitiesInContext;
    }

    #endregion
}

