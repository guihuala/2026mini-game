using UnityEngine;

public static class Uitiles_test
{
    public static T Find<T>() where T : Component
    {
        return Object.FindObjectOfType<T>();
    }
}
