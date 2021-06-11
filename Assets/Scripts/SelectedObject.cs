using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedObject : MonoBehaviour
{
    [SerializeField] private GameObject select;
    
    public GameObject Select {
        get {
            return select;
        }
        set {
            if(select) {
                if(select != value) {
                    select.GetComponentInChildren<MouseController>().ReleaseClick();  //CubePivot
                    //select.GetComponent<MouseController>().ReleaseClick();              //Cube
                } else {
                    return;
                }
                Destroy(select.GetComponent<TransformControl>());
            }
            select = value;
            UIManager.Instance.control = select.AddComponent<TransformControl>();
        }
    }

    private void Update() {
        UpdateUI();
    }

    private void UpdateUI() {
        if (select == null) return;

        UIManager.Instance.SetTransformLabel(select.transform);
        UIManager.Instance.UpdateNameText(select.GetComponentInChildren<CubeInfo>().info.Name);
    }
}
