using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugManager
{
    // Start is called before the first frame update
    static bool _log = true;
    static int _level = 2000;
    public static void Log(string message, int level)
    {
        if(_log && level <= _level){
            Debug.Log(message);
        }
    }

}
