using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uprt.test
{
    public class UPRTTest
    {
        public static bool CreateUPRTTestObject()
        {
            try
            {
                GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                gameObject.name = "UPRTTest Cube";
                return true;
            }
            catch(Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }
    }
}
