using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConsts
{
    public const string ContextLayerName = "Context";
    public const string CharacterControllerLayerName = "CharacterController";
    public const string CharacterModelLayerName = "CharacterModel";
    public const string GroundLayerName = "Ground";

    public static int ContextLayer()
    {
        return LayerMask.NameToLayer(ContextLayerName);
    }

    public static int CharacterControllerLayer()
    {
        return LayerMask.NameToLayer(CharacterControllerLayerName);
    }

    public static int CharacterModelLayer()
    {
        return LayerMask.NameToLayer(CharacterModelLayerName);
    }

    public static int GroundLayer()
    {
        return LayerMask.NameToLayer(GroundLayerName);
    }
}
