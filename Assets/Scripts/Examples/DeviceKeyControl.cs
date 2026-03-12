using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImstkUnity.Examples
{
    public class DeviceKeyControl : MonoBehaviour
    {
        public float speed = 0.005f;
        // Update is called once per frame
        void Update()
        {
            Vector3 dir = new Vector3(0, 0, 0);
            if (Input.GetKey(KeyCode.W))
            {
                dir.y = 1.0f;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                dir.y = -1.0f;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                dir.x = 1.0f;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                dir.x = -1.0f;
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                dir.z = 1.0f;
            }
            else if (Input.GetKey(KeyCode.E))
            {
                dir.z = -1.0f;
            }
            var pos = transform.position;
            pos += dir * speed;
            transform.position = pos;
        }
    }
}
