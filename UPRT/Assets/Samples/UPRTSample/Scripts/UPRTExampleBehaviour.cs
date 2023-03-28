using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uprt.test;

namespace sample.uprt
{
    public class UPRTExampleBehaviour : MonoBehaviour
    {
        private GameObject uprtCube;
        private void Start()
        {
            uprtCube = GameObject.Find("UPRTTest Cube");

            if (uprtCube == null)
                Debug.LogError("[UPRTExampleBehaviour] Can't find");
        }

        private void Update()
        {
            if (uprtCube != null)
                uprtCube.transform.Rotate(Vector3.up * Time.deltaTime * 100, Space.World);
        }
    }
}
