using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace uprt.test
{
    public class UPRTTestEditor
    {
        [MenuItem("UPRT/Create UPRT Object/Cube")]
        public static void Create_UPRTCube()
        {
            if (UPRTTest.CreateUPRTTestObject())
            {
                Debug.Log("[UPRT] Success to create UPRT Cube");
            }
            else
            {
                Debug.Log("[UPRT] Fail to create UPRT Cube");
            }
        }
    }
}
