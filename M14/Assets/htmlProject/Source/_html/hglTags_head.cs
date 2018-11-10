using UnityEngine;
using System.Collections;
using hug_u;

public class hglTags_head  {

    hglParseStyleSheet m_styleSheet;
    hglWorkHead        m_hglWork;
    hglResourceMan     m_resman;
    hglWindowInfo      m_winInfo;
    
    public void Init(hglWorkHead xw, hglParseStyleSheet styleSheet,hglResourceMan resman,hglWindowInfo wininfo)
    {
        m_hglWork    = xw;
        m_styleSheet = styleSheet;
        m_resman     = resman;
        m_winInfo    = wininfo;
        RegisterAll();
    }

    void RegisterAll()
    {
        m_hglWork.Init(this);
        m_hglWork.Register(typeof(html_link));
        m_hglWork.Register(typeof(html_style));
    }

    void SendMessageUpwards(string s, string href){}

    public class html_link : hglBaseHead
    {
        public override void ElementWork(Hashtable atrs)
        {
            string rel  = (string)atrs["rel"];
            string href = (string)atrs["href"];
                 
            rel = rel.ToLower();
            if (href==null && atrs.ContainsKey("src"))
            {
                href = (string)atrs["src"];
            }
            if (string.IsNullOrEmpty(rel) || string.IsNullOrEmpty(href)) return;

            if (rel=="top-bar")
            {
                hglTags.SendMessageUpwards("StartTopBar",href);
            }
            else if (rel=="bottom-bar")
            {
                hglTags.SendMessageUpwards("StartBotBar",href);
            }
            else if (rel=="stylesheet")
            {
                string src = hglTags.m_resman.GetText(hglTags.m_winInfo.CreateFullPath(href));
                hglTags.m_styleSheet.Parse(src);
                Debug.LogError(hglTags.m_styleSheet.ToString());
            }
        }
    }

    public class html_style : hglBaseHead
    {
        public override void TextWork(string text)
        {
            hglTags.m_styleSheet.Parse(text);
			//Debug.LogError(xmlTags.m_styleSheet.ToString());
        }
        public override void CommentWork(string text)
        {
            hglTags.m_styleSheet.Parse(text);           
			//Debug.LogError(xmlTags.m_styleSheet.ToString());
        }
    }
}

