using UnityEngine;
using System.Collections;

public class hglSlideControl : MonoBehaviour {

    public float  m_click_time = 0.1f;
    public bool   m_lock = false;
    public Transform m_background;
    public Transform m_popup;

    hglWebCamera  m_webCamera;
    Vector3 m_orgPosition;
    Vector2 m_savePos;

    public void Reset()
    {
        //camera.enabled = true;
        transform.localPosition = m_orgPosition;
        StopCoroutine("Flow");
        StartCoroutine("Flow");
    }

    void Awake()
    {
        m_orgPosition = transform.localPosition;
        //camera.enabled = false;
    }

    void Start()
    {
        StartCoroutine("Flow");
    }

    IEnumerator Flow()
    {
        m_webCamera = GetComponent<hglWebCamera>();
        m_webCamera.ChangeSize();
        m_webCamera.CenterCamera();
        //camera.enabled = true;

        while(true)
        {
            yield return null;

            if (m_lock) continue;

            float save_y = GetInputPositionY();

            float sumMove = 0;
            float sumTime = 0;

            while (
                Input.GetMouseButton(0)  
                ||
                ( Input.touchCount==1 && Input.GetTouch(0).phase == TouchPhase.Moved ) 
            )
            {
                float cy = GetInputPositionY();
                float dy = save_y - cy;
                save_y   = cy;

                float v = (camera.orthographicSize * 2) * dy / Screen.height;/* xBugFix.V.ScreenHight*/
                transform.Translate(Vector3.up * v );
                if (m_background!=null) m_background.Translate(Vector3.up *v);
                if (m_popup!=null)      m_popup.Translate(Vector3.up * v);

                sumMove += Mathf.Abs(v);
                sumTime += Time.deltaTime;

                yield return null;
            }

            if (sumMove < 5 && sumTime > m_click_time)
            {
                m_savePos = GetInputPosition();
            }
        }
    }


    float GetInputPositionY()
    {
        return GetInputPosition().y;
    }
    Vector2 m_saveV;
    Vector2 GetInputPosition()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.touchCount > 0)
            {
                m_saveV = Input.GetTouch(0).position;
            }
            return m_saveV;
        }
        m_saveV = Input.mousePosition;
        return m_saveV;
    }
}
