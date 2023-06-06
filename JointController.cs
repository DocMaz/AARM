using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Include this to use the Text component

namespace IKProject3
{
    public class JointController : MonoBehaviour
    {
        // robot
        private GameObject[] joint = new GameObject[3];
        private GameObject[] arm = new GameObject[3];
        private float[] armL = new float[3];
        private Vector3[] angle = new Vector3[3];

        private GameObject TCP; // Tool center point
        private Vector3 screenPoint; 
        private Vector3 offset;
        private Text TCPValueText; // Reference to the Text component of the TCP value display
        private Renderer TCPRenderer; // Renderer of the TCP
        private Text TCPLabel; // Reference to the Text component of the TCP label display

        void Start()
        {
            for (int i = 0; i < joint.Length; i++)
            {
                joint[i] = GameObject.Find("Joint_" + i.ToString());
                arm[i] = GameObject.Find("Arm_" + i.ToString());
                if(i == 0) armL[i] = arm[i].transform.localScale.y;
                else armL[i] = arm[i].transform.localScale.x;
            }

            TCP = GameObject.Find("TCP");
            TCPRenderer = TCP.GetComponent<Renderer>(); // Get the Renderer of the TCP

            TCPValueText = GameObject.Find("TCP_Value").GetComponent<Text>();
            TCPLabel = GameObject.Find("TCP_Display_Label").GetComponent<Text>(); // Get the Text of the TCP label
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.gameObject == TCP)
                    {
                        screenPoint = Camera.main.WorldToScreenPoint(TCP.transform.position);
                        offset = TCP.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
                    }
                }
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
                Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
                TCP.transform.position = curPosition;
                TCPValueText.text = curPosition.x.ToString("F2") + ", " + curPosition.y.ToString("F2") + ", " + curPosition.z.ToString("F2"); // Update the TCP value display
                ComputeIK(TCP.transform.position);
            }
        }

        void ComputeIK(Vector3 pos)
        {
            float x = pos.x;
            float y = pos.y;
            float z = pos.z;

            angle[0].y = -Mathf.Atan2(z, x);
            float a = x / Mathf.Cos(angle[0].y);
            float b = y - armL[0];

            if (Mathf.Pow(a * a + b * b, 0.5f) > (armL[1] + armL[2]))
            {
                TCPRenderer.material.color = Color.red; // Set TCP color to red
                TCPLabel.color = Color.red; // Set label color to red
                TCPValueText.color = Color.red; // Set value color to red
                Debug.Log("TCP is out of bounds"); // Print to the console
            }
            else
            {
                TCPRenderer.material.color = Color.white; // Set TCP color to white
                TCPLabel.color = Color.white; // Set label color to white
                TCPValueText.color = Color.white; // Set value color to white

                float alfa = Mathf.Acos((armL[1] * armL[1] + armL[2] * armL[2] - a * a - b * b) / (2f * armL[1] * armL[2]));
                angle[2].z = -Mathf.PI + alfa;
                float beta = Mathf.Acos((armL[1] * armL[1] + a * a + b * b - armL[2] * armL[2]) / (2f * armL[1] * Mathf.Pow((a * a + b * b), 0.5f)));
                angle[1].z = Mathf.Atan2(b, a) + beta;

                for (int i = 0; i < joint.Length; i++)
                {
                    joint[i].transform.localEulerAngles = angle[i] * Mathf.Rad2Deg;
                }
            }
        }
    }
}
