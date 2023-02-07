using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uprt.test;

namespace sample.uprt
{
    public class UPRTExampleBehaviour : MonoBehaviour
    {
        private void Start()
        {
            UPRTTest uprtTest = new UPRTTest();
            uprtTest.Print();
        }

        private void Update()
        {
            this.gameObject.transform.Rotate(Vector3.up * Time.deltaTime * 100, Space.World);
        }
    }
}
