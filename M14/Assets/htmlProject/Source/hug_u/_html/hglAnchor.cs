using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using hug_u;

namespace hug_u {
public class hglAnchor {

    public enum MODE
    {
        ANCHOR,
        JUMP
    }

    public class Info
    {
        public string m_url;

        public Color? m_hover;
        public Color? m_link;
        public Color? m_visited;
        public Color? m_pressed;
    }

    public class CDDATA
    {
        public List<hgMesh.CD>  clamp;
    }
    //public class BLOCKDATA
    //{
    //    public List<hglHtmlRender.BASE> 
    //}


    public static void CreateAnchor(List<CDDATA> list)
    {
        foreach (var d in list)
        {
            hgRect r = new hgRect();
            if (d.clamp.Count==0) continue;
            var rcd = d.clamp[0];
            string nm="BC.";
            foreach (var cd in d.clamp)
            {
                r.Sample(cd.outer_v);
                nm += cd.ToString();
            }
            GameObject o = new GameObject(nm);
            var bc = o.AddComponent<BoxCollider>();
            o.transform.parent = rcd.xe.FindBone();

            if (rcd is hgMesh.CD_IMAGE)
            {
                o.transform.localPosition = Vector3.zero;
            }
            else
            { 
                o.transform.localPosition = r.center;
            }
            bc.center = Vector3.zero;
            bc.size   = new Vector2(r.width,r.height);

            var button = o.AddComponent<hglButtonLink>();
            button.Init(rcd.xe);
        }
    }
    public static void CreateAnchor(List<hglRender.BASE> list)
    {
        foreach (var d in list)
        {
            if (d.xe==null) continue;
            hgRect r = d.doneRealRect;
            string nm="BC.";
            if (d.xe.text!=null) nm += d.xe.text;
            GameObject o = new GameObject(nm);
            var bc = o.AddComponent<BoxCollider>();
            o.transform.parent = d.xe.FindBone();
            o.transform.localPosition =  Vector3.zero;

            bc.center = Vector3.zero;
            bc.size   = new Vector2(r.width,r.height);

            var button = o.AddComponent<hglButtonLink>();
            button.Init(d.xe);
        }
    }

}
}