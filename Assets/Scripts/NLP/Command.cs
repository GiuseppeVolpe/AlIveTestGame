using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Command
{
    public string Intent { get; private set; }

    public List<string> Entities { get; private set; }

    public Command(string intent, List<string> entities)
    {
        Intent = intent;
        Entities = entities;
    }
}
