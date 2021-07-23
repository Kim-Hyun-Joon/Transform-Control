using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour
{
    public LineRenderer lr;
    public bool isDraw = false;

    GameObject vertex;

    private void Awake() {
        lr = transform.GetComponent<LineRenderer>();
        isDraw = true;
    }

    public void SetLinePosition(int lrCount, Transform vertex) {
        lr.SetPosition(lrCount, vertex.position + Vector3.up * 0.3f);
        lr.positionCount++;

    }

    public void UpdateLinePosition(Vector3 pos) {
        if(lr.positionCount > 1) {
            lr.SetPosition(lr.positionCount-1, pos + Vector3.up * 0.2f);
        }
    }


}
