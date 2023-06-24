using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public const string ReachableEntity = "Reachable";
    public const string PickableEntity = "Pickable";
    public const string ThrowableEntity = "Throwable";
    public const string HittableEntity = "Hittable";
    public const string InspectableEntity = "Inspectable";

    public EntityData Data;

    public bool AnswersToTheNameOf(string name)
    {
        bool answersToTheNameOf = false;

        foreach (string syn in Data.GetGlobalSynset())
        {
            if (syn.ToLower() == name.ToLower())
            {
                answersToTheNameOf = true;
                break;
            }
        }

        return answersToTheNameOf;
    }

    public bool HasAllTheseTypes(List<string> entityTypes)
    {
        bool hasTypes = true;

        foreach (string entityType in entityTypes)
        {
            if (!HasType(entityType)) {
                hasTypes = false;
                break;
            }
        }

        return hasTypes;
    }

    public bool HasOneOfTheseTypes(List<string> entityTypes)
    {
        bool hasTypes = false;

        foreach (string entityType in entityTypes)
        {
            if (HasType(entityType))
            {
                hasTypes = true;
                break;
            }
        }

        return hasTypes;
    }

    public bool HasType(string entityType)
    {
        bool hasType = false;

        foreach (string ownedEntityType in Data.GetEntityTypes())
        {
            if (entityType.ToLower() == ownedEntityType.ToLower())
            {
                hasType = true;
                break;
            }
        }

        return hasType;
    }
}
