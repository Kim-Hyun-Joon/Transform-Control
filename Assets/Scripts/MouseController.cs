using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour {
    private bool isClicked = false;
    private bool hover = false;

    private Vector3 prePos;

    private TransformControl transformControl;
    [SerializeField] private SelectedObject selectedObject;
    private void Awake() {
        transformControl = GetComponent<TransformControl>();
        selectedObject = GetComponentInParent<SelectedObject>();
    }
    private void OnMouseEnter() {
        hover = true;
    }
    private void OnMouseOver() {
        gameObject.GetComponent<Outline>().enabled = hover;
    }
    private void OnMouseExit() {
        hover = false;
        if(!isClicked)
            gameObject.GetComponent<Outline>().enabled = hover;
    }
    private void OnMouseDown() {
        if(!isClicked)
            gameObject.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);
        prePos = Input.mousePosition;
    }
    private void OnMouseUp() {
        Vector3 dis = prePos - Input.mousePosition;
        if(!isClicked)
            gameObject.transform.localScale -= new Vector3(0.1f, 0.1f, 0.1f);
        if (dis.sqrMagnitude < 0.001f) {
            isClicked = true;
            //selectedObject.Select = transform.parent.gameObject;  //CubePivot
            selectedObject.Select = gameObject;                     //Cube
        }
    }
    public void ReleaseClick() {
        isClicked = false;
        gameObject.GetComponent<Outline>().enabled = hover;
    }
}
