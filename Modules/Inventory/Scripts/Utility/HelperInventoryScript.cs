using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class HelperInventoryScript
{
    public static bool Contains(this string source, string toCheck, StringComparison comp)
    {
        return source != null && toCheck != null && source.IndexOf(toCheck, comp) >= 0;
    }
}
