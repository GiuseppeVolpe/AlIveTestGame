using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public float SmoothTime = .3f;

    private Camera _camera;
    private Vector3 _refVelocity = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        Vector3 desiredPosition = new Vector3(target.transform.position.x + offset.x,
                                              target.transform.position.y + offset.y,
                                              target.transform.position.z + offset.z);

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _refVelocity, SmoothTime);
    }
}
