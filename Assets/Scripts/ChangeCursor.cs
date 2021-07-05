using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeCursor : MonoBehaviour
{
    [SerializeField] private Texture2D translateCursor;
    [SerializeField] private Texture2D rotateCursor;

    public void TranslateCursor() {

        //Cursor.SetCursor(translateCursor, Input.mousePosition, CursorMode.ForceSoftware);
        Cursor.SetCursor(translateCursor, Vector2.zero, CursorMode.ForceSoftware);
    }
    public void RotateCursor() {

        //Cursor.SetCursor(rotateCursor, Input.mousePosition, CursorMode.ForceSoftware);
        Cursor.SetCursor(rotateCursor, Vector2.zero, CursorMode.ForceSoftware);
    }
    public void DefaultCursor() {

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
