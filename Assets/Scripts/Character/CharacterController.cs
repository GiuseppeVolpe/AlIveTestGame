using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CharacterController : MonoBehaviour
{
    public float FollowingDistance;
    public float Speed = 10;
    public float RotationLerping = 3;

    public Transform PickedItemTransform;

    private CharacterAI _ai;
    private Rigidbody _rb;
    private Animator _ac;
    private SphereCollider _sc;
    private BoxCollider _bc;

    private Coroutine _currentActionCoroutine;

    public bool IsBusy
    {
        get
        {
            return _currentActionCoroutine != null;
        }
    }

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
    }

    public void ReachPosition(Vector3 desiredPosition, float followingDistance=-1, float speed=-1)
    {
        if (followingDistance <= 0)
        {
            followingDistance = FollowingDistance;
        }


        if (speed <= 0)
        {
            speed = Speed;
        }

        _ai.SetDestination(desiredPosition, followingDistance);
    }

    public void FollowTarget(Transform targetToFollow, float followingDistance = -1, float speed = -1)
    {
        if (followingDistance <= 0)
        {
            followingDistance = FollowingDistance;
        }


        if (speed <= 0)
        {
            speed = Speed;
        }

        _ai.SetDestination(targetToFollow, followingDistance);
    }

    public bool Inspect(ItemsHidingPlace itemsHidingPlace)
    {
        bool success = false;

        if (_currentActionCoroutine == null)
        {
            _currentActionCoroutine = StartCoroutine(InspectCoroutine(itemsHidingPlace));
            success = true;
        }

        return success;
    }

    private IEnumerator InspectCoroutine(ItemsHidingPlace itemsHidingPlace)
    {
        Debug.Log(itemsHidingPlace);
        FollowTarget(itemsHidingPlace.transform);

        yield return new WaitUntil(() => _ai.IsNearToDesiredPosition == true);

        _ai.CancelDestination();

        GameObject hiddenItem = itemsHidingPlace.GetItem();

        if (hiddenItem != null)
        {
            hiddenItem.GetComponent<Rigidbody>().isKinematic = true;
            hiddenItem.GetComponent<Collider>().isTrigger = true;
            hiddenItem.transform.parent = PickedItemTransform;
            hiddenItem.transform.localPosition = Vector3.zero;
        }
    }

    public bool PickItem(GameObject item)
    {
        bool success = false;

        if (_currentActionCoroutine == null)
        {
            _currentActionCoroutine = StartCoroutine(PickItemCoroutine(item));
            success = true;
        }

        return success;
    }

    private IEnumerator PickItemCoroutine(GameObject item)
    {
        FollowTarget(item.transform);

        yield return new WaitUntil(() => _ai.IsNearToDesiredPosition == true);

        _ai.CancelDestination();

        item.GetComponent<Rigidbody>().isKinematic = true;
        item.GetComponent<Collider>().isTrigger = true;
        item.transform.parent = PickedItemTransform;
        item.transform.localPosition = Vector3.zero;

        _currentActionCoroutine = null;
    }

    public bool LeaveItem()
    {
        bool success = false;

        if (_currentActionCoroutine == null && _pickedItem != null)
        {
            _pickedItem.transform.parent = null;
            _pickedItem.GetComponentInChildren<Rigidbody>().isKinematic = false;
            success = true;
        }

        return success;
    }

    public bool ThrowItem(Vector3 target)
    {
        bool success = false;

        if (_currentActionCoroutine == null && _pickedItem != null)
        {
            _currentActionCoroutine = StartCoroutine(ThrowItemCoroutine(_pickedItem, target));
            success = true;
        }

        return success;
    }

    private IEnumerator ThrowItemCoroutine(GameObject item, Vector3 target)
    {
        yield return null;
    }

    public void DoConfusedExpression()
    {
        if (_currentActionCoroutine != null)
        {
            return;
        }

        _currentActionCoroutine = StartCoroutine(DoConfusedExpressionCoroutine());
    }

    private IEnumerator DoConfusedExpressionCoroutine()
    {
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        float normalizedSpeed = _rb.velocity.magnitude / Speed;
        _ac.SetFloat("Speed", normalizedSpeed);
    }

    private void FixedUpdate()
    {
        if (_ai.RawDesiredDirection != null && _ai.RawDesiredDirection != Vector3.zero)
        {
            _rb.AddForce(_ai.RawDesiredDirection * Speed * _rb.mass);

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
