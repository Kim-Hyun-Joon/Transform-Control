using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Info {
    private string name;
    

    //
    private float test;
    //

    //클래스 프로퍼티 사용중
    /*
      get 
        { 
            if (index >= 0) 
                return data[index]; 

            else 
                return -1; 
        } 

        set 
        { 
            if (index >= 0) 
                data[index] = value; 
        } 
     */

    public string Name {
        get {
            return name;
        }
        set {
            name = value;
        } 
    }
   
    public float Test { get; set; }
}

public class CubeInfo : MonoBehaviour
{
    public Info info;
    private void Awake() {
        info = new Info {
            Name = transform.name,
            Test = 30f
        };
    }
}
