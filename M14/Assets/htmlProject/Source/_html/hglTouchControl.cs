using UnityEngine;
using System.Collections;

public class hglTouchControl : MonoBehaviour {

    public hglWindowInfo m_mainWinInfo;
    public hglReadHtml m_mainReader;
    Camera m_camera;
    Vector2 m_pos;
    Vector2 m_vpos;
    
    void Start()
    {
        //m_mainWinInfo   = GetComponent<hglWindowInfo>();
        m_camera        = m_mainWinInfo.m_mainCamera.camera;

        //m_reader = GetComponentInChildren<hglReadHtml>();

        StartCoroutine(UpdateCoroutine());
    }

    IEnumerator UpdateCoroutine()
    {
        while (true)
        { 
            yield return null;

#if UNITY_EDITOR ||  UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
            m_pos = (Vector2)Input.mousePosition;
#else
            m_pos  = (Input.touchCount!=0) ? Input.touches[0].position : new Vector2(float.NaN, float.NaN);
#endif
            if (float.IsNaN(m_pos.x)) continue;

            m_vpos = new Vector2((m_pos.x) / Screen.width, (m_pos.y) / Screen.height);

            Ray ray = m_camera.ViewportPointToRay(new Vector3(m_vpos.x, m_vpos.y, 0));

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                //Debug.Log("Hit" + hit.transform.name);
                var bl = hit.transform.GetComponent<hglButtonLink>();

                if (Input.GetMouseButtonUp(0))
                {
                    string url;
                    if (bl.GetJumpUrl(out url))
                    {
                        bl.Touched();
                        yield return new WaitForSeconds(0.5f);
                        Debug.Log("Hit! and go to " + url);

                        if (hglEtc.check_head(url, "javascript:"))
                        {
                            m_mainReader.ExecuteScript(url);
                        }
                        else
                        { 
                            m_mainReader.Browse(m_mainWinInfo.CreateFullPath(url) );
                        }
                    }
                }
                else
                { 
                    bl.Hover();
                }
            }
        }
    }

}
