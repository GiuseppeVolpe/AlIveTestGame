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

    public static bool IsWithinBoxBounds(Vector3 point, BoxCollider boxCollider)
    {
        point = boxCollider.transform.InverseTransformPoint(point) - boxCollider.center;

        float halfX = (boxCollider.size.x * 0.5f);
        float halfY = (boxCollider.size.y * 0.5f);
        float halfZ = (boxCollider.size.z * 0.5f);
        if (point.x < halfX && point.x > -halfX &&
           point.y < halfY && point.y > -halfY &&
           point.z < halfZ && point.z > -halfZ)
            return true;
        else
            return false;
    }

    public static Collider[] BoxColliderOverlaps(BoxCollider target, int layerMask, bool hitTriggers)
    {
        return Physics.OverlapBox(target.transform.position + target.center, target.size * 0.5f, target.transform.rotation, layerMask, hitTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore);
    }
}
