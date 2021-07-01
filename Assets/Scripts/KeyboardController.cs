using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardController : MonoBehaviour
{
    private KeyCode positionKey = KeyCode.Alpha1;
    private KeyCode rotationKey = KeyCode.Alpha2;
    private KeyCode scaleKey = KeyCode.Alpha3;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(positionKey)) {
            Debug.Log("aa");
        }
        if (Input.GetKeyDown(rotationKey)) {

        }
        if (Input.GetKeyDown(scaleKey)) {

        }
    }
}
