using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsHidingPlace : MonoBehaviour
{
    public Transform HiddenItemReferenceTransform;
    public GameObject HiddenItem;

    // Start is called before the first frame update
    void Start()
    {
        PutItem(HiddenItem);
    }

    public GameObject GetItem()
    {
        if (HiddenItem != null)
        {
            HiddenItem.transform.parent = null;
            HiddenItem.SetActive(true);
        }

        return HiddenItem;
    }

    public bool PutItem(GameObject item)
    {
        bool success = false;

        if (HiddenItem != null)
        {
            if (HiddenItemReferenceTransform != null)
            {
                HiddenItem.transform.position = HiddenItemReferenceTransform.position;
            }
            HiddenItem.transform.parent = transform;
            HiddenItem.SetActive(false);

            success = true;
        }

        return success;
    }
}
