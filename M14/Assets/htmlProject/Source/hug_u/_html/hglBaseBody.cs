using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using hug_u;

namespace hug_u {
public class hglBaseBody : hglBase {
    public hglTags_body      hglTags;
    public System.Type       delivativeType;
    public string            name;
    public hglAnchor.Info    ainfo;

    public hglParser.Element hglElement;

    public virtual void PreElementWork(hglParser.Element xe) { 
        hglElement = xe; 
        xe.id = xe.attrib["id"]!=null ? (string)xe.attrib["id"] : null; 
        hglTags.StyleElementWork(this);
        xe.colorIndex = hglTags.m_htmlRender.m_htmlColor.GetNewIndex(Color.white);
        hglConverter.referenceFontSize = xe.thisStyle.GetFloat(StyleKey.font_size,float.NaN);

        if (xe.attrib["onclick"] != null)
        {
                ainfo = new hglAnchor.Info();
                ainfo.m_url = "javascript:" + (string)xe.attrib["onclick"];
        }
    }
    public virtual void ElementWork(hglParser.Element te)         { }
    public virtual void TextWork(hglParser.Element te)            { hglTags.m_htmlRender.WriteText(te,te.text, hglRender.TextMode.NORMALIZE);  }
    public virtual void EndElementWork(hglParser.Element te)      { }
    public virtual void PostElementWork(hglParser.Element te)  {
        Color? curBack  = te.thisStyle.GetColorErrorNull(StyleKey.background_color);
        if (hglElement.id!=null || curBack!=null)
        {
            if (curBack==null) curBack = new Color(0,0,0,0);
            te.backcolorIndex = hglTags.m_htmlRender.m_htmlColor.GetNewIndex((Color)curBack);
        }
    }

    public virtual bool IsInline()                     {return false;}

    public hglBaseBody Clone() {
        hglBaseBody c = (hglBaseBody)System.Activator.CreateInstance(delivativeType);
        c.hglTags = hglTags;
        c.delivativeType = delivativeType;
        c.name = name;
        return c;
    }

    public virtual void CommentWork(string text)  {}

    public override string ToString()
    {
        return name +">"+delivativeType.ToString();
    }

}


public class hglWorkBody
{
    hglTags_body  m_hglTags;
    Hashtable     m_tags; 

    public void Init(hglTags_body hgltags)
    {
        m_hglTags = hgltags;
        m_tags = new Hashtable();
    }

    public override string ToString()
    {
        var dt = "------ s_tags start -----\n";
        foreach(var s in m_tags.Keys)
        {
            dt += s + "," + ((hglBaseBody)m_tags[s]).name +"\n";
        }
        dt+="------ s_tags end -----";
        return dt;
    }

    public void Register(System.Type t)
    {
        hglBaseBody o = (hglBaseBody)System.Activator.CreateInstance(t);
        o.hglTags = m_hglTags;
        o.delivativeType = t;
        var name = t.ToString().Substring("hag_u+hglTags_html+html_".Length);
        o.name = name;
        m_tags[name] = o;
    }

    public void ElementWork(hglParser.Element xe)
    {
        var name = xe.text;
        hglBaseBody work = (hglBaseBody)m_tags[name];
        if (work!=null) 
        {
            hglBaseBody tag = work.Clone();
            xe.baseBody = tag;
    
            tag.PreElementWork(xe);
            tag.ElementWork(xe);
        }
    }


    public void EndElementWork(hglParser.Element xe)
    {
        if (xe.baseBody!=null)
        {
            var name = xe.text;
            hglBaseBody tag  = (hglBaseBody)xe.baseBody;
            tag.EndElementWork(xe);
            tag.PostElementWork(xe);
        }
    }
    public void TextWork(hglParser.Element xe)
    {
        hglBaseBody tag = (hglBaseBody)xe.parent.baseBody;
        if (tag!=null) tag.TextWork(xe);
    }

    public void CommentWork(hglParser.Element xe)
    {
        if (xe.parent!= null && xe.parent.baseBody != null)
        {
            hglBaseBody tag = (hglBaseBody)xe.parent.baseBody;
            tag.CommentWork(xe.text);
        }
    }
}
}