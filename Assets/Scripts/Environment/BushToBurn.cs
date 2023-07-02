using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BushToBurn : FireSource
{
    public Entity NotAccessibleEntity;

    protected override void TurnOff()
    {

    }

    protected override void TurnOn()
    {
        base.TurnOn();

        StartCoroutine(BurnCoroutine());
    }

    private IEnumerator BurnCoroutine()
    {
        yield return new WaitForSeconds(2);
        Destroy(gameObject);

        if (NotAccessibleEntity != null)
        {
            NotAccessibleEntity.Accessible = true;
        }
    }
}
