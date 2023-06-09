using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IKProject3
{
    public class JointController : MonoBehaviour
    {
        private GameObject[] joint = new GameObject[3];
        private GameObject[] arm = new GameObject[3];
        private float[] armL = new float[3];
        private Vector3[] angle = new Vector3[3];

        private GameObject TCP; // Tool center point
        private GameObject Pointer; // Pointer game object
        private Text TCPValueText;
        private Renderer TCPRenderer;
        private Text TCPLabel;
        private TextMesh TCP3DValueTextMesh; // TCP 3D value TextMesh object

        public GameObject RocketLauncher;
        public GameObject RocketPrefab;
        private GameObject currentRocket;
        private Transform rocketSpawnLocation; // The location where rockets will spawn

        public Vector3 rocketSize = Vector3.one; // Default size is (1, 1, 1)
        public Vector3 rocketRotation = Vector3.zero; // Default rotation is (0, 0, 0)

        public float fireRate = 0.3f; // Fire rate in seconds
        private float nextFire = 0.3f; // Time when the next rocket can be fired

        void Start()
        {
            for (int i = 0; i < joint.Length; i++)
            {
                joint[i] = GameObject.Find("Joint_" + i.ToString());
                arm[i] = GameObject.Find("Arm_" + i.ToString());
                if (i == 0) armL[i] = arm[i].transform.localScale.y;
                else armL[i] = arm[i].transform.localScale.x;
            }

            TCP = GameObject.Find("TCP");
            Pointer = GameObject.Find("Pointer"); // Find the Pointer game object
            TCPRenderer = TCP.GetComponent<Renderer>();
            TCPValueText = GameObject.Find("TCP_Value").GetComponent<Text>();
            TCPLabel = GameObject.Find("TCP_Display_Label").GetComponent<Text>();
            TCP3DValueTextMesh = GameObject.Find("TCP3DVALUE").GetComponent<TextMesh>(); // Find the TCP3DVALUE TextMesh object

            GameObject tcp3dvalueObject = GameObject.Find("TCP3DVALUE");
            if (tcp3dvalueObject != null)
            {
                TCP3DValueTextMesh = tcp3dvalueObject.GetComponent<TextMesh>();
                if (TCP3DValueTextMesh == null)
                {
                    Debug.LogError("No TextMesh component found on TCP3DVALUE GameObject.");
                }
            }
            else
            {
                Debug.LogError("No GameObject named TCP3DVALUE found in the scene.");
            }

            // Find the RocketSpawnLocation object
            rocketSpawnLocation = RocketLauncher.transform.Find("RocketSpawnLocation");
            if (rocketSpawnLocation == null)
            {
                Debug.LogError("No GameObject named RocketSpawnLocation found as a child of the RocketLauncher.");
            }
        }

        void Update()
        {
            Vector3 pointerPosition = Pointer.transform.position; // Get the position of the Pointer
            TCP.transform.position = pointerPosition; // Set the position of the TCP to the Pointer's position
            string tcpValue = pointerPosition.x.ToString("F2") + ", " + pointerPosition.y.ToString("F2") + ", " + pointerPosition.z.ToString("F2");
            TCPValueText.text = tcpValue;
            TCP3DValueTextMesh.text = tcpValue; // Set the text of the TCP3DVALUE TextMesh object to the TCP value
            ComputeIK(TCP.transform.position);

            if (TCP3DValueTextMesh != null)
            {
                TCP3DValueTextMesh.text = tcpValue; // Set the text of the TCP3DVALUE TextMesh object to the TCP value
            }

            if (TiltFive.Input.TryGetButton(TiltFive.Input.WandButton.One, out bool onePressed, TiltFive.ControllerIndex.Right, TiltFive.PlayerIndex.One))
            {
                if (onePressed && Time.time > nextFire)
                {
                    nextFire = Time.time + fireRate; // Update the next fire time

                    // Instantiate a new Rocket at the RocketSpawnLocation's position
                    currentRocket = Instantiate(RocketPrefab, rocketSpawnLocation.position, Quaternion.identity);

                    // Set the size and rotation of the Rocket
                    currentRocket.transform.localScale = rocketSize;
                    currentRocket.transform.eulerAngles = rocketRotation;

                    // Set the orientation of the Rocket to face the +x axis of the TCP
                    currentRocket.transform.forward = TCP.transform.right;

                    // Pass the forward direction of the Rocket to the Rocket
                    currentRocket.GetComponent<Rocket>().SetDirection(currentRocket.transform.forward);
                }
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
                TCPRenderer.material.color = Color.red;
                TCPLabel.color = Color.red;
                TCPValueText.color = Color.red;
                Debug.Log("TCP is out of bounds");
            }
            else
            {
                TCPRenderer.material.color = Color.white;
                TCPLabel.color = Color.white;
                TCPValueText.color = Color.white;

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
