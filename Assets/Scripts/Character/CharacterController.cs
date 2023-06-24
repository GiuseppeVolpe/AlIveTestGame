using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CharacterController : MonoBehaviour
{
    public const float HappinessStartValue = 100;
    public const float ComplimentIncrementValue = 5;
    public const float InsultDecrementValue = 15;

    public float FollowingDistance;
    [Range(0, .6f)]
    public float RotateSpeed = .1f;
    public float MoveSpeed = 20;
    public float RotationLerping = 3;
    public Transform PickedItemTransform;
    [Range(1, Mathf.Infinity)]
    public float ThrowDistance = 5;

    private CharacterAI _ai;
    private Rigidbody _rb;
    private Animator _ac;
    private SphereCollider _sc;
    private BoxCollider _bc;

    public float Happiness { get; private set; }

    private Coroutine _currentActionCoroutine;

    public bool IsBusy
    {
        get
        {
            return _currentActionCoroutine != null;
        }
    }

    public bool IDBA;
    public bool IsDoingBlockingAction { get; private set; }

    private GameObject _pickedItem;

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.GetComponent<CharacterAI>() == null)
        {
            gameObject.AddComponent<CharacterAI>();
        }

        if (gameObject.GetComponent<Rigidbody>() == null)
        {
            gameObject.AddComponent<Rigidbody>();
        }

        if (gameObject.GetComponent<SphereCollider>() == null)
        {
            gameObject.AddComponent<SphereCollider>();
            gameObject.GetComponent<SphereCollider>().radius = .5f;
            gameObject.GetComponent<SphereCollider>().center = new Vector3(0, .5f, 0);
        }

        _ai = gameObject.GetComponent<CharacterAI>();
        _rb = gameObject.GetComponent<Rigidbody>();
        _ac = gameObject.GetComponentInChildren<Animator>();
        _sc = gameObject.GetComponent<SphereCollider>();
        _bc = gameObject.GetComponentInChildren<BoxCollider>();

        _rb.mass = 50;
        _rb.drag = 4;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;

        Happiness = HappinessStartValue;

        IsDoingBlockingAction = false;
    }

    public bool ReachPosition(Vector3 desiredPosition, float followingDistance=-1, float speed=-1)
    {
        bool success = false;

        if (followingDistance <= 0)
        {
            followingDistance = FollowingDistance;
        }

        if (speed <= 0)
        {
            speed = MoveSpeed;
        }

        if (!IsDoingBlockingAction)
        {
            if (_currentActionCoroutine != null)
            {
                StopCoroutine(_currentActionCoroutine);
                _currentActionCoroutine = null;
            }

            _currentActionCoroutine = StartCoroutine(ReachPositionCoroutine(desiredPosition, followingDistance, speed));
            success = true;
        }

        return success;
    }

    public bool ReachTarget(Transform desiredTarget, float followingDistance = -1, float speed = -1)
    {
        bool success = false;

        if (followingDistance <= 0)
        {
            followingDistance = FollowingDistance;
        }

        if (speed <= 0)
        {
            speed = MoveSpeed;
        }

        if (!IsDoingBlockingAction)
        {
            if (_currentActionCoroutine != null)
            {
                StopCoroutine(_currentActionCoroutine);
                _currentActionCoroutine = null;
            }

            _currentActionCoroutine = StartCoroutine(ReachTargetCoroutine(desiredTarget, followingDistance));
            success = true;
        }

        return success;
    }

    public bool PickItem(GameObject item)
    {
        bool success = false;

        if (!IsDoingBlockingAction)
        {
            if (item != null)
            {
                if (_pickedItem != null)
                {
                    LeaveItem();
                }

                if (_currentActionCoroutine != null)
                {
                    StopCoroutine(_currentActionCoroutine);
                    _currentActionCoroutine = null;
                }

                _currentActionCoroutine = StartCoroutine(PickItemCoroutine(item));
                success = true;
            }
        }

        return success;
    }

    public bool LeaveItem()
    {
        bool success = false;

        if (_pickedItem != null)
        {
            _pickedItem.GetComponent<Rigidbody>().isKinematic = false;
            _pickedItem.GetComponent<Collider>().isTrigger = false;
            _pickedItem.transform.parent = null;
            _pickedItem = null;
            success = true;
        }

        return success;
    }

    public bool ThrowItem(GameObject item, GameObject target)
    {
        bool success = false;

        Debug.Log(_pickedItem);

        if (item == null || item == _pickedItem)
        {
            if (!IsDoingBlockingAction)
            {
                if (_currentActionCoroutine != null)
                {
                    StopCoroutine(_currentActionCoroutine);
                    _currentActionCoroutine = null;
                }

                _currentActionCoroutine = StartCoroutine(ThrowItemCoroutine(target));
                success = true;
            }
        }

        return success;
    }

    public bool Inspect(ItemsHidingPlace itemsHidingPlace)
    {
        bool success = false;

        if (!IsDoingBlockingAction)
        {
            if (_pickedItem != null)
            {
                LeaveItem();
            }

            if (_currentActionCoroutine != null)
            {
                StopCoroutine(_currentActionCoroutine);
                _currentActionCoroutine = null;
            }

            _currentActionCoroutine = StartCoroutine(InspectCoroutine(itemsHidingPlace));
            success = true;
        }

        return success;
    }

    public void DoConfusedExpression()
    {
        if (_currentActionCoroutine != null)
        {
            return;
        }

        _currentActionCoroutine = StartCoroutine(DoConfusedExpressionCoroutine());
    }

    public void ExpressHappiness()
    {
        if (_currentActionCoroutine != null)
        {
            return;
        }

        _currentActionCoroutine = StartCoroutine(ExpressHappinessCoroutine());
    }

    public void ExpressSadness()
    {
        if (_currentActionCoroutine != null)
        {
            return;
        }

        _currentActionCoroutine = StartCoroutine(ExpressSadnessCoroutine());
    }

    #region Coroutines

    private IEnumerator ReachPositionCoroutine(Vector3 desiredPosition, float followingDistance, float speed)
    {
        _ai.SetDestination(desiredPosition, followingDistance);

        yield return new WaitUntil(() => _ai.IsNearToDesiredPosition == true);

        _ai.CancelDestination();

        Vector3 lookDirection = (desiredPosition - transform.position).normalized;

        if (lookDirection != Vector3.zero)
        {
            Quaternion startRotation = transform.rotation;
            Quaternion desiredRotation = CustomUE.TurretLookRotation(lookDirection, transform.up);
            float lerpFactor = 0;
            Quaternion oldRotation;

            while (transform.rotation != desiredRotation)
            {
                lerpFactor += RotateSpeed;

                oldRotation = transform.rotation;
                transform.rotation = Quaternion.Slerp(startRotation, desiredRotation, lerpFactor);

                if (transform.rotation == oldRotation)
                {
                    break;
                }

                yield return new WaitForFixedUpdate();
            }
        }

        _currentActionCoroutine = null;
    }

    private IEnumerator ReachTargetCoroutine(Transform desiredTarget, float followingDistance = -1, float speed = -1)
    {
        _ai.SetDestination(desiredTarget, followingDistance);

        yield return new WaitUntil(() => _ai.IsNearToDesiredPosition == true);

        _ai.CancelDestination();

        Vector3 lookDirection = (desiredTarget.position - transform.position).normalized;

        if (lookDirection != Vector3.zero)
        {
            Quaternion startRotation = transform.rotation;
            Quaternion desiredRotation = CustomUE.TurretLookRotation(lookDirection, transform.up);
            float lerpFactor = 0;
            Quaternion oldRotation;

            while (transform.rotation != desiredRotation)
            {
                lerpFactor += RotateSpeed;

                oldRotation = transform.rotation;
                transform.rotation = Quaternion.Slerp(startRotation, desiredRotation, lerpFactor);

                if (transform.rotation == oldRotation)
                {
                    break;
                }

                yield return new WaitForFixedUpdate();
            }
        }

        _currentActionCoroutine = null;
    }

    private IEnumerator PickItemCoroutine(GameObject item)
    {
        ReachTarget(item.transform);

        yield return new WaitUntil(() => _ai.IsNearToDesiredPosition == true);

        _ai.CancelDestination();

        IsDoingBlockingAction = true;

        Vector3 lookDirection = (item.transform.position - transform.position).normalized;

        if (lookDirection != Vector3.zero)
        {
            Quaternion startRotation = transform.rotation;
            Quaternion desiredRotation = CustomUE.TurretLookRotation(lookDirection, transform.up);
            float lerpFactor = 0;
            Quaternion oldRotation;

            while (transform.rotation != desiredRotation)
            {
                lerpFactor += RotateSpeed;

                oldRotation = transform.rotation;
                transform.rotation = Quaternion.Slerp(startRotation, desiredRotation, lerpFactor);

                if (transform.rotation == oldRotation)
                {
                    break;
                }

                yield return new WaitForFixedUpdate();
            }
        }

        item.GetComponent<Rigidbody>().isKinematic = true;
        item.GetComponent<Collider>().isTrigger = true;
        item.transform.parent = PickedItemTransform;
        item.transform.localPosition = Vector3.zero;

        _pickedItem = item;

        IsDoingBlockingAction = false;
        _currentActionCoroutine = null;
    }

    private IEnumerator ThrowItemCoroutine(GameObject target)
    {
        _ai.SetDestination(target.transform, ThrowDistance);

        yield return new WaitUntil(() => _ai.IsNearToDesiredPosition == true);

        _ai.CancelDestination();

        IsDoingBlockingAction = true;

        Vector3 targetPosition = target.transform.position;

        Vector3 lookDirection = (targetPosition - transform.position).normalized;

        if (lookDirection != Vector3.zero)
        {
            Quaternion startRotation = transform.rotation;
            Quaternion desiredRotation = CustomUE.TurretLookRotation(lookDirection, transform.up);
            float lerpFactor = 0;
            Quaternion oldRotation;

            while (transform.rotation != desiredRotation)
            {
                lerpFactor += RotateSpeed;

                oldRotation = transform.rotation;
                transform.rotation = Quaternion.Slerp(startRotation, desiredRotation, lerpFactor);

                if (transform.rotation == oldRotation)
                {
                    break;
                }

                yield return new WaitForFixedUpdate();
            }
        }

        GameObject thrownItem = _pickedItem;
        _pickedItem = null;

        thrownItem.GetComponent<Rigidbody>().isKinematic = true;
        thrownItem.GetComponent<Collider>().isTrigger = true;
        thrownItem.transform.parent = null;

        Coroutine lerpCoroutine =
            StartCoroutine(CustomUE.ParabolicLerpPosition(thrownItem, thrownItem.transform.position, targetPosition, 10, Vector3.up));

        yield return new WaitUntil(() => Vector3.Distance(thrownItem.transform.position, targetPosition) < 1);

        thrownItem.GetComponent<Rigidbody>().isKinematic = false;
        thrownItem.GetComponent<Collider>().isTrigger = false;

        Hittable hittableComponent = target.GetComponentInChildren<Hittable>();

        if (hittableComponent != null)
        {
            hittableComponent.OnHit();
        }

        StopCoroutine(lerpCoroutine);

        IsDoingBlockingAction = false;
        _currentActionCoroutine = null;
    }

    private IEnumerator InspectCoroutine(ItemsHidingPlace itemsHidingPlace)
    {
        ReachTarget(itemsHidingPlace.transform);

        yield return new WaitUntil(() => _ai.IsNearToDesiredPosition == true);

        _ai.CancelDestination();

        IsDoingBlockingAction = true;

        Vector3 lookDirection = (itemsHidingPlace.transform.position - transform.position).normalized;

        if (lookDirection != Vector3.zero)
        {
            Quaternion startRotation = transform.rotation;
            Quaternion desiredRotation = CustomUE.TurretLookRotation(lookDirection, transform.up);
            float lerpFactor = 0;
            Quaternion oldRotation;

            while (transform.rotation != desiredRotation)
            {
                lerpFactor += RotateSpeed;

                oldRotation = transform.rotation;
                transform.rotation = Quaternion.Slerp(startRotation, desiredRotation, lerpFactor);

                if (transform.rotation == oldRotation)
                {
                    break;
                }

                yield return new WaitForFixedUpdate();
            }
        }

        GameObject hiddenItem = itemsHidingPlace.PopItem();

        if (hiddenItem != null)
        {
            hiddenItem.GetComponent<Rigidbody>().isKinematic = true;
            hiddenItem.GetComponent<Collider>().isTrigger = true;
            hiddenItem.transform.parent = PickedItemTransform;
            hiddenItem.transform.localPosition = Vector3.zero;

            _pickedItem = hiddenItem;
        }

        IsDoingBlockingAction = false;
        _currentActionCoroutine = null;
    }

    private IEnumerator DoConfusedExpressionCoroutine()
    {
        Debug.Log("Doggo is confused");

        yield return new WaitForSeconds(0);

        _currentActionCoroutine = null;
    }

    private IEnumerator ExpressHappinessCoroutine()
    {
        Debug.Log("Doggo is happy!");
        Happiness += ComplimentIncrementValue;

        yield return new WaitForSeconds(0);

        _currentActionCoroutine = null;
    }

    private IEnumerator ExpressSadnessCoroutine()
    {
        Debug.Log("Doggo is sad... :(");
        Happiness -= InsultDecrementValue;

        yield return new WaitForSeconds(0);

        _currentActionCoroutine = null;
    }

    private IEnumerator RotateTowards(Vector3 targetPosition)
    {
        Vector3 lookDirection = (targetPosition - transform.position).normalized;

        if (lookDirection != Vector3.zero)
        {
            Quaternion startRotation = transform.rotation;
            Quaternion desiredRotation = CustomUE.TurretLookRotation(lookDirection, transform.up);
            float lerpFactor = 0;
            Quaternion oldRotation;

            while (transform.rotation != desiredRotation)
            {
                lerpFactor += RotateSpeed;

                oldRotation = transform.rotation;
                transform.rotation = Quaternion.Slerp(startRotation, desiredRotation, lerpFactor);

                if (transform.rotation == oldRotation)
                {
                    Debug.Log("Stopped rotation here");
                    break;
                }

                Debug.Log("still rotating");
                yield return new WaitForFixedUpdate();
            }
        }
    }

    #endregion

    // Update is called once per frame
    void Update()
    {
        float normalizedSpeed = _rb.velocity.magnitude / MoveSpeed;
        _ac.SetFloat("Speed", normalizedSpeed);
    }

    private void FixedUpdate()
    {
        if (_ai.RawDesiredDirection != null && _ai.RawDesiredDirection != Vector3.zero)
        {
            _rb.AddForce(_ai.RawDesiredDirection * MoveSpeed * _rb.mass);

            #region Calculating rotation

            Vector3 lookDirection = Vector3.zero;

            Vector3 velocity = _rb.velocity;
            velocity.y = 0;

            if (velocity.magnitude > .5f && _ai.RawDesiredDirection.magnitude > .1f)
            {
                lookDirection = _ai.SmoothDesiredDirection;
            }

            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = CustomUE.TurretLookRotation(lookDirection, transform.up);
                Quaternion desiredRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, RotationLerping);
                _rb.MoveRotation(desiredRotation);
            }

            #endregion

        }
    }
}
