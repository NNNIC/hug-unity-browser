using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using hug_u;

namespace hug_u {
public class hglBase {} 

public class hglBaseHead : hglBase {

    public hglTags_head hglTags;
    public System.Type  delivativeType;
    public string       name;
    public string       id; //handle for JS

    public hglStyle saveStyle;

    public virtual void ElementWork(Hashtable atrs)    {  }
    public virtual void TextWork(string text)          {  /*hglTags.WriteText(text);*/ }
    public virtual void EndElementWork()               { }
    public virtual hglBaseHead Create()                {return null;}

    public hglBaseHead Clone() {
        hglBaseHead c = (hglBaseHead)System.Activator.CreateInstance(delivativeType);
        c.hglTags = hglTags;
        c.delivativeType = delivativeType;
        c.name = name;
        return c;
    }
    public virtual void CommentWork(string text)
    {
    }
}

public class hglWorkHead
{
    hglTags_head  m_hglTags;
    Hashtable m_tags; 

    public void Init(hglTags_head hgltags)
    {
        m_hglTags = hgltags;
        m_tags = new Hashtable();
    }

    public override string ToString()
    {
        var dt = "------ s_tags start -----\n";
        foreach(var s in m_tags.Keys)
        {
            dt += s + "," + ((hglBaseHead)m_tags[s]).name +"\n";
        }
        dt+="------ s_tags end -----";
        return dt;
    }

    public void Register(System.Type t)
    {
        hglBaseHead o = (hglBaseHead)System.Activator.CreateInstance(t);
        o.hglTags = m_hglTags;
        o.delivativeType = t;
        var name = t.ToString().Substring("hglTags_head+html_".Length);
        o.name = name;
        m_tags[name] = o;
    }

    public void ElementWork(hglParser.Element xe)
    {
        hglBaseHead work = (hglBaseHead)m_tags[xe.text];
        if (work!=null) 
        {
            hglBaseHead tag = work.Clone();
            xe.baseBody  = tag;
            tag.ElementWork(xe.attrib);
        }
    }

    public void TextWork(hglParser.Element xe)
    {
        if (xe.parent!=null && xe.parent.baseBody!= null)
        {
            hglBaseHead tag = (hglBaseHead)xe.parent.baseBody;
            var nt = hglEtc.decodeTextToDisplay(xe.text);
            tag.TextWork(nt);
        }
    }

    public void EndElementWork(hglParser.Element xe)
    {
        if (xe.baseBody != null)
        { 
            hglBaseHead tag = (hglBaseHead)xe.baseBody;
            tag.EndElementWork();
        }
    }

    public void CommentWork(hglParser.Element xe)
    {
        if (xe.parent != null && xe.parent.baseBody != null)
        {
            hglBaseHead tag = (hglBaseHead)xe.parent.baseBody;
            tag.CommentWork(xe.text);
        }
    }
}
}