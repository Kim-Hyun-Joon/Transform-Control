using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;


public class TransformPanel : MonoBehaviour {

    [SerializeField] Text xLabel;
    [SerializeField] Text yLabel;
    [SerializeField] Text zLabel;

    public void SetTransform(Vector3 transform) {
        SetXYZ(transform.x, transform.y, transform.z);
    }

    public void SetXYZ(float x, float y, float z) {
        xLabel.text = x.ToString("0.00");
        yLabel.text = y.ToString("0.00");
        zLabel.text = z.ToString("0.00");
    }

}


