using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldInputSystem : MonoBehaviour
{
    public GameObject cube;
    public Material mat1;
    public Material mat2;
    public float speed;

    // Update is called once per frame
    void Update()
    {
        if(TiltFive.Input.TryGetButtonDown(TiltFive.Input.WandButton.One, out bool onePressed, TiltFive.ControllerIndex.Right, TiltFive.PlayerIndex.One))
        {
           if (onePressed)
           {
           cube.GetComponent<MeshRenderer>().material = mat1;
           }
        }

      if(TiltFive.Input.TryGetButtonDown(TiltFive.Input.WandButton.Two, out bool twoPressed, TiltFive.ControllerIndex.Right, TiltFive.PlayerIndex.One))
        {
           if (twoPressed)
           {
           cube.GetComponent<MeshRenderer>().material = mat2;
           }
        }
      
      if(TiltFive.Input.TryGetStickTilt(out Vector2 joystick, TiltFive.ControllerIndex.Right, TiltFive.PlayerIndex.One)) 
      {
        cube.transform.Translate(joystick.x * Time.deltaTime * speed, 0.0f, joystick.y * Time.deltaTime * speed);
      }
    }
}
