using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class CustomUE
{
    public static bool NearestPointOnNavmesh(Vector3 origin, 
                                             float range,
                                             out Vector3 finalPosition, 
                                             bool onCircumference = false, 
                                             float maxDistanceFromNavmesh = Mathf.Infinity)
    {
        Vector2 bidimensionalOffset;

        if (onCircumference)
        {
            bidimensionalOffset = Random.insideUnitCircle.normalized * range;
        } else
        {
            bidimensionalOffset = Random.insideUnitCircle * range;
        }
        
        Vector3 positionOffset = 
            new Vector3(bidimensionalOffset.x, 0, bidimensionalOffset.y);
        Vector3 randomPoint = origin + positionOffset;

        NavMeshHit hit;

        bool succeded = 
            NavMesh.SamplePosition(randomPoint, out hit, maxDistanceFromNavmesh, NavMesh.AllAreas);

        finalPosition = (succeded) ? hit.position : Vector3.zero;

        return succeded;
    }
    
    public static NavMeshPath GetPathBetweenPositions(Vector3 startPosition, 
                                                      Vector3 targetPosition)
    {
        NavMeshHit hit;

        bool startPositionIsNearNavMesh = NavMesh
            .SamplePosition(startPosition, out hit,
                            Mathf.Infinity, NavMesh.AllAreas);

        if (!startPositionIsNearNavMesh)
        {
            return null;
        }

        startPosition = hit.position;

        bool targetPositionIsNearNavMesh = NavMesh
            .SamplePosition(targetPosition, out hit,
                            Mathf.Infinity, NavMesh.AllAreas);

        if (!targetPositionIsNearNavMesh)
        {
            return null;
        }

        targetPosition = hit.position;

        NavMeshPath path = new NavMeshPath();

        bool success = NavMesh.CalculatePath(startPosition, targetPosition,
                              NavMesh.AllAreas, path);

        if (!success)
        {
            path = null;
        }

        return path;
    }

    public static float GetPathLength(NavMeshPath path)
    {
        if (path == null)
        {
            return 0;
        }

        float pathLength = 0;

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Vector3 currentWayPoint = path.corners[i];
            Vector3 nextWayPoint = path.corners[i + 1];

            pathLength += Vector3.Distance(currentWayPoint, nextWayPoint);
        }

        return pathLength;
    }

    public static List<Vector3> NavMeshPathToVector3List(NavMeshPath path)
    {
        List<Vector3> positions = new List<Vector3>();

        if (path != null && path.corners != null)
        {
            foreach (Vector3 corner in path.corners)
            {
                positions.Add(corner);
            }
        }

        return positions;
    }

    public static Vector3 LerpByDistance
        (Vector3 currentPosition, Vector3 targetPosition, float distance)
    {
        Vector3 P = distance * 
            Vector3.Normalize(currentPosition - targetPosition) + targetPosition;
        return P;
    }

    public static List<Transform> GetNearTransforms(Vector3 origin, float range)
    {
        Collider[] collidersInRange = Physics.OverlapSphere(origin, range);

        List<Collider> collidersSortedByDistance = collidersInRange
            .OrderBy(collider => Vector3.Distance(collider.transform.position, origin))
            .ToList();

        List<Transform> nearTransforms = new List<Transform>();

        foreach (Collider collider in collidersSortedByDistance)
        {
            nearTransforms.Add(collider.transform);
        }

        return nearTransforms;
    }

    public static Vector3 GetCentroid(List<Vector3> vectors)
    {
        Vector3 sum = Vector3.zero;

        if (vectors == null || vectors.Count == 0)
        {
            return sum;
        }

        foreach (Vector3 vector in vectors)
        {
            sum += vector;
        }

        return sum / vectors.Count;
    }

    public static Quaternion TurretLookRotation(Vector3 approximateForward, Vector3 exactUp)
    {
        Quaternion rotateZToUp = Quaternion.LookRotation(exactUp, -approximateForward);
        Quaternion rotateYToZ = Quaternion.Euler(90f, 0f, 0f);

        return rotateZToUp * rotateYToZ;
    }

    #region Too specific, to review

    /*
     * 
    public static IEnumerator EmbarkOnNavMeshPath(GameObject walker,
                                                  NavMeshPath path,
                                                  float positionSpeedFactor = 1,
                                                  float rotationSpeedFactor = 1)
    {
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Vector3 startCorner = path.corners[i];
            Vector3 destinationCorner = path.corners[i + 1];

            Quaternion startRotation = walker.transform.rotation;

            Vector3 targetForRotation = new Vector3(destinationCorner.x,
                                                    startCorner.y,
                                                    destinationCorner.z);

            Quaternion desiredRotation = Quaternion
                .LookRotation(targetForRotation - startCorner);

            Task positionLerpingTask = new Task(LerpPosition(walker,
                                                             startCorner,
                                                             destinationCorner,
                                                             positionSpeedFactor));

            Task rotationLerpingTask = new Task(LerpRotation(walker,
                                                             startRotation,
                                                             desiredRotation,
                                                             rotationSpeedFactor));

            yield return new WaitUntil(() => !positionLerpingTask.Running &&
                                             !rotationLerpingTask.Running);
        }
    }

    public static IEnumerator LerpPosition(GameObject walker,
                                           Vector3 startPosition,
                                           Vector3 desiredPosition,
                                           float speedFactor,
                                           float distance = 0)
    {
        if (distance > 0)
        {
            desiredPosition = CustomUE.LerpByDistance(startPosition,
                                                      desiredPosition,
                                                      distance);
        }

        if (Vector3.Distance(startPosition, desiredPosition) == 0)
        {
            yield break;
        }

        float lerpFactor = 0;
        float increment;

        Vector3 currentPosition;

        do
        {
            increment = Time.fixedDeltaTime * speedFactor /
                Vector3.Distance(startPosition, desiredPosition);

            lerpFactor += increment;
            lerpFactor = Mathf.Clamp01(lerpFactor);

            currentPosition = Vector3.Lerp(startPosition, desiredPosition, lerpFactor);

            walker.transform.position = currentPosition;

            yield return new WaitForFixedUpdate();

        } while (lerpFactor < 1);

        walker.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    */

    public static IEnumerator LerpRotation(GameObject walker,
                                           Quaternion startRotation,
                                           Quaternion desiredRotation,
                                           float speedFactor)
    {
        if (Quaternion.Angle(startRotation, desiredRotation) == 0)
        {
            yield break;
        }

        float lerpFactor = 0;
        float increment;

        Quaternion currentRotation;

        do
        {
            increment = Time.fixedDeltaTime * speedFactor;

            lerpFactor += increment;
            lerpFactor = Mathf.Clamp01(lerpFactor);

            currentRotation = Quaternion.Slerp(startRotation, desiredRotation, lerpFactor);

            walker.transform.rotation = currentRotation;

            yield return new WaitForFixedUpdate();

        } while (lerpFactor < 1);
    }

    public static IEnumerator ParabolicLerpPosition(GameObject walker,
                                           Vector3 startPosition,
                                           Vector3 desiredPosition,
                                           float speedFactor,
                                           Vector3 up,
                                           float concavityFactor = 4)
    {
        float lerpFactor = 0;
        float increment;

        Vector3 currentPosition;
        concavityFactor *= Vector3.Distance(startPosition, desiredPosition) / 5;

        do
        {
            increment = Time.fixedDeltaTime * speedFactor /
                Vector3.Distance(startPosition, desiredPosition);

            lerpFactor += increment;
            lerpFactor = Mathf.Clamp01(lerpFactor);

            currentPosition = Vector3.Lerp(startPosition, desiredPosition, lerpFactor);

            float yModifier = (lerpFactor - Mathf.Pow(lerpFactor, 2)) * concavityFactor;

            currentPosition += (up * yModifier);

            walker.transform.position = currentPosition;

            yield return null;

        } while (lerpFactor < 1);
    }
    
    #endregion
}
