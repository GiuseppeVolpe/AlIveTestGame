using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Context : MonoBehaviour
{
    public LayerMask LayerMask;

    private BoxCollider _bc;

    private void Start()
    {
        _bc = GetComponent<BoxCollider>();
    }

    public bool IsWithinContext(Vector3 point)
    {
        return CustomUE.IsWithinBoxBounds(point, _bc);
    }

    public List<Entity> GetEntitiesInContext(int layerMask = -1)
    {
        List<Entity> entitiesInContext = new List<Entity>();

        if (_bc == null)
        {
            return entitiesInContext;
        }

        if (layerMask == -1)
        {
            layerMask = LayerMask;
        }

        Collider[] colliders = CustomUE.BoxColliderOverlaps(_bc, layerMask, false);

        foreach (Collider collider in colliders)
        {
            Entity entityComponent = collider.gameObject.GetComponent<Entity>();

            if (entityComponent != null)
            {
                entitiesInContext.Add(entityComponent);
            }
        }

        return entitiesInContext;
    }
}
