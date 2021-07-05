using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardController : MonoBehaviour
{
    private KeyCode positionKey = KeyCode.Alpha1;
    private KeyCode rotationKey = KeyCode.Alpha2;
    private KeyCode scaleKey = KeyCode.Alpha3;

    public Dropdown dropdown;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(positionKey)) {
            dropdown.value = 0;
            UIManager.Instance.OnModeChanged(0);
        }
        if (Input.GetKeyDown(rotationKey)) {
            dropdown.value = 1;
            UIManager.Instance.OnModeChanged(1);
        }
        if (Input.GetKeyDown(scaleKey)) {
            dropdown.value = 2;
            UIManager.Instance.OnModeChanged(2);
        }
    }
}
