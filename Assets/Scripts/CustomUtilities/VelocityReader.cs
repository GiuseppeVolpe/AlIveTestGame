using System.Collections;
using UnityEngine;

public class VelocityReader : MonoBehaviour
{
    private const float UpdateDelay = .25f;

    public Vector3 Velocity;
    public float Speed;

    private Coroutine SpeedTrackerCoroutine { get; set; }

    private void OnEnable()
    {
        SpeedTrackerCoroutine = StartCoroutine(SpeedTracker());
    }

    private IEnumerator SpeedTracker()
    {
        Vector3 lastPosition = transform.position;
        float lastTimestamp = Time.time;

        while (enabled)
        {
            yield return new WaitForSeconds(UpdateDelay);

            Vector3 deltaPosition = (transform.position - lastPosition);
            var deltaTime = Time.time - lastTimestamp;

            if (Mathf.Approximately(deltaPosition.magnitude, 0f))
                deltaPosition = Vector3.zero;

            Velocity = deltaPosition / deltaTime;
            Speed = Velocity.magnitude;

            lastPosition = transform.position;
            lastTimestamp = Time.time;
        }
    }

    private void OnDisable()
    {
        StopCoroutine(SpeedTrackerCoroutine);
    }
    
    private void OnDestroy()
    {
        StopCoroutine(SpeedTrackerCoroutine);
    }
}
