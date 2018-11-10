using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class hglWebCamera : MonoBehaviour {
    //public bool stopUpdateInEditor;
    public float fixedWidth;

    Vector2? screenSize;


    Vector2 GetScreenSize()
    {
        if (screenSize==null) screenSize = new Vector2(Screen.width, Screen.height );
        return (Vector2)screenSize;
    }

    void Start()
    {
        screenSize = new Vector2(Screen.width, Screen.height );
        ChangeSize();
    }
    [ContextMenu("Change Size")]
    public void ChangeSize()
    {
        //if (m_info==null) m_info = (hglWindowInfo)hgca.FindAscendantComponent(gameObject,typeof(hglWindowInfo));
        camera.orthographicSize = GetScreenSize().y * fixedWidth / (2 * GetScreenSize().x);
        Debug.Log("Width = " + GetScreenSize().x);
        Debug.Log("Height= " + GetScreenSize().y);

    }

    //void Update()
    //{
    //    if (Application.isEditor && stopUpdateInEditor==false)
    //    {
    //        ChangeSize();
    //    }
    //}

    [ContextMenu("Center Camera")]
    public void CenterCamera()
    {
        //ChangeSize();
        transform.localPosition = new Vector3( fixedWidth / 2 , - camera.orthographicSize, transform.localPosition.z);
    }

    //public void SetClearBuffer(bool b)
    //{
    //    if (!b)
    //    {
    //        camera.clearFlags = CameraClearFlags.Nothing;
    //    }
    //    else
    //    {
    //        camera.clearFlags = CameraClearFlags.SolidColor;
    //    }
    //}
}
