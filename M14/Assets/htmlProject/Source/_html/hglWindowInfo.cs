using UnityEngine;
using System.Collections;
using hug_u;


public enum hglWindowType { MAIN, POPUP, SELECT }

public class hglWindowInfo : MonoBehaviour {
    public int           m_id
    {
        get { return this.GetHashCode();}
    }
    public bool          m_showSrc      = false;

    
    public float         __fixedWidth;     
    public float         m_fixedWidth
    {
        set { if (m_winType == hglWindowType.MAIN) __fixedWidth = value;  }
        get { 
              if  (m_winType != hglWindowType.MAIN && m_mainWinInfo!=null)  return m_mainWinInfo.__fixedWidth;
              else if (m_winType == hglWindowType.MAIN) return __fixedWidth;
              return 0;
        }
    }
    public hglWebCamera  m_mainCamera;
    public hglWindowType m_winType = hglWindowType.MAIN;
    public int           m_renderingOrder = 0;

    public hglResourceFrom  m_ResourceFrom;

    public string        __url;
    public string        __urlText { get{ return this.GetHashCode() + "__tmp.html";   } }
    public string        m_url
    {
        get {  return m_useText ? __urlText : __url; }
    }
    //---
    public string        m_curUrlParams;
    public string        m_curBaseUrl;
    private string       __curUrl;
    public string        m_curUrl
    {
        get { return __curUrl;  }
        set { __curUrl = value; 
            int i = __curUrl.LastIndexOf('/');
            m_curUrlShort = (i>=0) ? __curUrl.Substring(i+1) : __curUrl;
        }
    }
    public string        m_curUrlShort;
    
    //--
    public hglParser.Element m_curRootElement;
    public hglParser.Element m_curHeadElement;
    public hglParser.Element m_curBodyElement;

    //
    public float m_height
    {
        get
        {
            return m_fixedWidth / (float)Screen.width * (float)Screen.height;
        }
    }
    public Vector2 m_dispArea
    {
        get{ return new Vector2(m_fixedWidth,m_height);}
    }

    //--
    public bool             m_useText;
    public string           m_text = "<head>\n</head>\n<body>\nIndex\n</body>";
    //--
    public hglWindowInfo    m_mainWinInfo;
    //--
    public string CreateFullPath(string path)
    {
        return hglUtil.MergePath(m_curBaseUrl,path); 
    }
    public int GetRenderingOrder()
    {
        switch(m_winType)
        {
        case hglWindowType.MAIN:   return 1000 + m_renderingOrder;
        case hglWindowType.POPUP:  return 4500 + m_renderingOrder;
        case hglWindowType.SELECT: return 4000 + m_renderingOrder;
        }
        throw new System.SystemException("Unexpected");
    }
}
