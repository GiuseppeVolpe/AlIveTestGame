using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public float FollowingDistance;
    public float Speed = 10;
    public float RotationLerping = 3;

    public Transform PickedItemTransform;

    private CharacterAI _ai;
    private Rigidbody _rb;
    private Animator _ac;

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

        _ai = gameObject.GetComponent<CharacterAI>();
        _rb = gameObject.GetComponent<Rigidbody>();
        _ac = gameObject.GetComponent<Animator>();

        _rb.mass = 50;
        _rb.drag = 4;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;
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

    public void PickItem(GameObject item)
    {
        StartCoroutine(PickItemCoroutine(item));
    }

    public IEnumerator PickItemCoroutine(GameObject item)
    {
        ReachPosition(item.transform.position);

        yield return new WaitUntil(() => _ai.IsNearToDesiredPosition == true);

        item.GetComponent<Rigidbody>().isKinematic = true;
        item.GetComponent<Collider>().isTrigger = true;
        item.transform.parent = PickedItemTransform;
        item.transform.localPosition = Vector3.zero;
    }
}
