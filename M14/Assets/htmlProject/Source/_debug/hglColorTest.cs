using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using hug_u;

public class hglColorTest : MonoBehaviour {

    public hglHtmlColor m_htmlColor;
    public List<Color>  m_list;
           List<Color>  m_savelist;
    IEnumerator Start()
    {
        yield return new WaitForSeconds(5);

        m_list = new List<Color>();
        m_savelist = new List<Color>();

        var renderobj = hgca.Find("Renderer").GetComponent<hglHtmlRender>();
        m_htmlColor =renderobj.m_render.m_htmlColor;

        for (int i = 0; i < m_htmlColor.CountIndex(); i++)
        {
            m_list.Add(m_htmlColor.GetColor(i));
            m_savelist.Add(m_htmlColor.GetColor(i));
        }

        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            bool bDirty=false;
            for (int i = 0; i < m_list.Count; i++)
            {
                if (m_list[i] != m_savelist[i])
                {
                    bDirty = true;
                    m_savelist[i] = m_list[i];
                    m_htmlColor.SetColor(i,m_list[i]);
                }
            }
            if (bDirty)
            {
                m_htmlColor.Apply();
            }
        }
    }
}
