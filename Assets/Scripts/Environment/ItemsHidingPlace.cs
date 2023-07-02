using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsHidingPlace : MonoBehaviour
{
    public Transform HiddenItemContainer;
    public GameObject StartHiddenItem;

    private GameObject _hiddenItem;

    // Start is called before the first frame update
    void Start()
    {
        PushItem(StartHiddenItem);
    }

    public bool PushItem(GameObject item)
    {
        bool success = false;

        if (_hiddenItem == null && item != null)
        {
            _hiddenItem = item;

            if (HiddenItemContainer != null)
            {
                _hiddenItem.transform.parent = HiddenItemContainer;
                _hiddenItem.transform.localPosition = Vector3.zero;
            }

            _hiddenItem.SetActive(false);

            success = true;
        }

        return success;
    }

    public GameObject PopItem()
    {
        GameObject item = null;

        if (_hiddenItem != null)
        {
            _hiddenItem.transform.parent = null;
            _hiddenItem.SetActive(true);
            item = _hiddenItem;

            _hiddenItem = null;
        }

        return item;
    }
}
