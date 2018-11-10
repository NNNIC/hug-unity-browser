using UnityEngine;
using System.Collections;
using hug_u;

public class hglButtonLink : MonoBehaviour {

    hglParser.Element   m_xe;
    //hglTags_body.html_a m_a;
    hglAnchor.Info      m_a;
    hglHtmlColor        m_colorMan;
    int                 m_colorIndex;

    public Color        m_col;
           Color        m_colSave;

    public void Init(hglParser.Element xe)
    {
        m_xe         = xe;
        m_a          = xe.GetAnchor();
        m_colorMan   = xe.GetTags().m_htmlRender.m_htmlColor;
        m_colorIndex = xe.colorIndex;
        m_col        = m_colorMan.GetColor(m_colorIndex);
        m_colSave    = m_col;
    }

	void Update () {
        ColorUpdate();
        if (m_colorIndex>=0 &&  m_colorMan!=null && m_colSave != m_col)
        {
            m_colSave = m_col;
            m_colorMan.SetColor(m_colorIndex,m_col);
            //m_colorMan.Apply();
        }
	}

    public void Hover()
    {
        m_hoverTime = 0.1f;
    }

    float? m_hoverTime = 0f;
    void ColorUpdate()
    {
        if (m_hoverTime != null)
        { 
            m_hoverTime -= Time.deltaTime;

            if (m_hoverTime >= 0)
            {
                if (m_a.m_hover!=null)
                { 
                    m_col = (Color)m_a.m_hover;
                }
            }
            else
            {
                if (m_a.m_link!=null)
                {
                    m_col = (Color)m_a.m_link;
                }
            }
        }
    }

    public void Touched()
    {
        m_hoverTime = null;
        StartCoroutine(TouchedCoroutine());
    }

    IEnumerator TouchedCoroutine()
    {
        if (m_a.m_pressed!=null)
        { 
            m_col = (Color)m_a.m_pressed;
        }
        
        yield return new WaitForSeconds(0.5f);
    }

    public bool GetJumpUrl(out string url)
    {
        url = m_a.m_url;
        return true;
    }
 
}
