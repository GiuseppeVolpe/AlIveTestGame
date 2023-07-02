using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : Pickable
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnPick()
    {
        base.OnPick();

        Debug.Log("You won the game");
    }
}
