using System;
using UnityEngine;

public class NullableVector3
{
    #region Static Operators

    public static implicit operator NullableVector3(Vector3 v) => new NullableVector3(v);
    public static explicit operator Vector3(NullableVector3 v) => v.ToVector3();

    public static bool operator ==(NullableVector3 nullableVector3, Vector3 vector3)
    {
        if (nullableVector3 is null)
        {
            return false;
        }

        // Equals handles case of null on right side.
        return nullableVector3.ToVector3().Equals(vector3);
    }

    public static bool operator !=(NullableVector3 nullableVector3, Vector3 vector3)
    {
        if (nullableVector3 is null)
        {
            return true;
        }

        // Equals handles case of null on right side.
        return !nullableVector3.ToVector3().Equals(vector3);
    }

    #endregion

    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }

    public NullableVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public NullableVector3(Vector3 vector3)
    {
        x = vector3.x;
        y = vector3.y;
        z = vector3.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    #region Equals, HashCode, ToString

    public override bool Equals(object obj)
    {
        return obj is NullableVector3 vector &&
               x == vector.x &&
               y == vector.y &&
               z == vector.z;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(x, y, z);
    }

    public override string ToString()
    {
        return ToVector3().ToString();
    }

    #endregion
}
