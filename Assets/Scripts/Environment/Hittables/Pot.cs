using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pot : Hittable
{
    public GameObject HiddenItem;

    private void Start()
    {
        if (HiddenItem != null)
        {
            HiddenItem.SetActive(false);
        }
    }

    public override void OnHit()
    {
        Destroy(gameObject);

        if (HiddenItem != null)
        {
            HiddenItem.SetActive(true);
        }
    }
}
