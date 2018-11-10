#define __UNITY
using System.Collections;
using System.Collections.Generic;
using System;

#if  __UNITY
using UnityEngine;
#endif

namespace hug_u {
public class hglParser  {

    public enum Mode
    {
        NONE,
        TAG,
        TEXT,
        COMENT,
    }

    public class Element
    {
        public Mode       mode;
        public bool       selfClose;

        public string     text;
        public Element    parent;
        public Hashtable  attrib;

        //public bool       end;

        private List<Element> children;

#if __UNITY
        // Work
        public string       id;
        public hglBase      baseBody;
        public hglStyle     thisStyle;
               int          _colorIndex;
        public int          colorIndex
               {
                   set { _colorIndex = value;  }
                   get
                   {
                       if (_colorIndex < 0)
                       {
                            if (parent!=null) return parent.colorIndex;
                       }
                       return _colorIndex;
                   }
               }

        public int          backcolorIndex;
        public int          effectColorIndex;
        public int          frameColorIndex;
        
        public Transform    bone1;     public Vector3 bone1_savepos;
        public Transform    bone2;     public Vector3 bone2_savepos;

        public hglRender.BASE formatBase; // RealRect is here!

        public object       tmp1;       //For any work. Use only local! 
        public object       tmp2;       //For any work. Use only local! 
        public object       tmp3;       //For any work. Use only local! 
        public object       tmp4;       //For any work. Use only local! 
#endif
        public Element()
        {
            mode      = Mode.NONE;
            selfClose = false;
            text      = "";
            parent    = null;
            //end = false;
            children  = null;
#if __UNITY
            thisStyle = new hglStyle(this);
            _colorIndex      = -1;
            backcolorIndex   = -1;
            effectColorIndex = -1;
            frameColorIndex  = -1;

            bone1 = null;
            bone2 = null;
#endif
        }

        public List<Element> List() {return children; }
        public int              ChildCount{  get{ return children!=null ? children.Count : 0; } }
        public void Add(Element c)
        {
            if (children==null) children = new List<Element>();
            children.Add(c);
        }

#if __UNITY
        public Transform FindBone()
        {
            if (bone2!=null) return bone2;
            if (bone1!=null) return bone1;

            if (parent!=null) return parent.FindBone();

            return null;
        }

        public hglAnchor.Info GetAnchor()
        {
            if (baseBody is hglBaseBody)
            { 
                return ((hglBaseBody)baseBody).ainfo;
            }
            if (parent!=null) return parent.GetAnchor();
            return null;
        }

        public hglTags_body  GetTags()
        {
            if (baseBody is hglBaseBody)
            { 
                return ((hglBaseBody)baseBody).hglTags;
            }
            if (parent!=null) return parent.GetTags();
            return null;
        }
#endif

        public override string ToString()
        {
            string s = "";
            switch(mode)
            {
            case Mode.TAG: 
                s += "<" + text;  
                if (attrib!=null) foreach(var k in attrib.Keys) s+=" " + k + "=\"" + attrib[k] +"\"";
                if (ChildCount == 0)
                {
                    s += " />";
                }
                else
                {
                    s += " >";
                }
                break;
            case Mode.TEXT:
                s += text;
                break;
            case Mode.COMENT:
                s += "<!--" + text + "-->";
                break;
            default:
                s+="???";
                break;
            }

            if (ChildCount > 0)
            {
                s+="\n";
                foreach(var c in List()) s+=c.ToString();

                if (mode == Mode.TAG)
                { 
                    s+="</"+text; 
                    //s += end ? "" : "?"; 
                    s+= ">\n";
                }
            }
#if __UNITY
            return s.Length<100 ? s : s.Substring(0,100) + "...";
#endif
            return s;
        }
    }

    // Ignore tag http://victreal.com/Junk/htmlTag/
    static bool IsSimpleElement(string tag)
    {
        string[] list1 = new string[] { "br","img","hr","meta","input"     };
        if ( Array.IndexOf(list1,tag)>=0) return true;
        return false;
    }
    static bool IsInlineElement(string tag) //http://www.tagindex.com/html_tag/elements/inline.html
    {
        string[] list3 = new string[]{          "span",
                                                "em",
                                                "strong",
                                                "abbr",
                                                "acronym",
                                                "dfn",
                                                "q",
                                                "cite",
                                                "sup",
                                                "sub",
                                                "code",
                                                "var",
                                                "kbd",
                                                "samp",
                                                "bdo",
                                                "font",
                                                "big",
                                                "small",
                                                "b",
                                                "i",
                                                "s",
                                                "strike",
                                                "u",
                                                "tt",
                                                "a",
                                                "label"   };
        if ( Array.IndexOf(list3,tag)>=0) return true;
        return false;    
    }

    /*

        In-line element is included in block element.
        If the end of in-line element is missing in the block element, the end mark is inserted in the process.
        
        If unexpected end mark is appeared, ignore.

        Ignorable end elements are adequatedly inserted the end mark.    
    */

    //private static string[] History(Element e, int num)
    //{
    //    string[] list = new string[num]; for(int i = 0;i<num;i++) list[i] = null;
    //    Element cur = e;
    //    for (int i = 0; i < num; i++)
    //    {
    //        if (cur==null) break;
    //        list[i] = cur.text;
    //        cur = e.parent;
    //    } 
    //    return list;
    //}
    private static Element Ascend(Element e, string tag)
    {
        Element cur = e;
        while (cur != null)
        {
            if (cur.text == tag)
            {
                return cur;
            }
            cur = cur.parent;
        }
        return null;
    }
    private static Element Ascend(Element e, string[] tags)
    {
        Element cur = e;
        while (cur != null)
        {
            int i = Array.IndexOf(tags,cur.text);
            if (i>=0) return cur;
            cur = cur.parent;
        }
        return null;
    }
    public static Element Parser(string src)
    {
        Element root = new Element() { mode = Mode.TAG, text = "root"};

        Element cur  = root;
        string dt = "";

        hglCustomReader.Read(src,(t,text,hash,dbg)=>{
        var stext = text.ToLower();
        switch(t)
        {
            case hglCustomReader.TYPE.NODE_ENTER:
            case hglCustomReader.TYPE.NODE_ENTER_EXIT:
            {
                dt += "<" + stext + ">";


                // # CHECK IGNORABLE ELEMENT "li","dt","dd","p","tr","td", "th"
                if (stext == "li")
                {
                    var te = Ascend(cur,new string[]{"ul","ol"});
                    if (te!=null) cur =te;
                }
                else if (stext == "dt")
                {
                    var te = Ascend(cur,"dl");
                    if (te!=null) cur =te;
                }
                else if (stext == "dt")
                {
                    var te = Ascend(cur,"dl");
                    if (te!=null) cur =te;
                }
                else if (stext == "p")
                {
                    var te = Ascend(cur,"p");
                    if (te!=null && te.parent!=null) cur =te.parent;
                }
                else if (stext == "tr")
                {
                    var te = Ascend(cur,"table");
                    if (te!=null) cur =te;
                }
                else if (stext == "td")
                {
                    var te = Ascend(cur,"tr");
                    if (te!=null) cur =te;
                }
                else if (stext == "th")
                {
                    var te = Ascend(cur,"tr");
                    if (te!=null) cur =te;
                }

                Element ne = new Element();
				ne.mode = Mode.TAG;
                ne.text = stext;
                ne.attrib = hash;
                ne.parent = cur;
                cur.Add(ne);

                if (!IsSimpleElement(stext))
                {
                    cur = ne;
                    if (t== hglCustomReader.TYPE.NODE_ENTER_EXIT)
                    {
                        cur = cur.parent;
                        ne.selfClose = true;
                    }
                }

                break;
            }
            case hglCustomReader.TYPE.NODE_EXIT:
            {
                dt += "</" + stext + ">";

                if (IsSimpleElement(stext)) break;
                Element te = Ascend(cur,stext);
                if (te != null && te.parent != null)
                {
                    cur = te.parent;
                }
                break;
            }
            case hglCustomReader.TYPE.TEXT:
            {
                dt +=  text ;

                var te = new Element() { mode = Mode.TEXT};
                te.text = text;
				te.parent = cur;
                cur.Add(te);

                break;
            }
            case hglCustomReader.TYPE.COMMENT:
            {
                dt += "<!--"+ text +"-->" ;
                Element ne = new Element();
                ne.mode = Mode.COMENT;
                ne.text = text;
				ne.parent = cur;
                cur.Add(ne);
                break;
            }
        }
        });

        //Debug.LogWarning("!!!@@@:"+dt);
        //root.end =true;
        return root;
    }
#if __UNITY
    public static void TraverseHglWorkHead(Element xe, hglWorkHead xw)
    {
        switch(xe.mode)
        {
        case Mode.TAG:
            xw.ElementWork(xe);
            if (xe.ChildCount > 0) foreach (var c in xe.List())
                {
                    TraverseHglWorkHead(c,xw);
                }
            xw.EndElementWork(xe);
            break;
        case Mode.COMENT:
            xw.CommentWork(xe);
            break;
        case Mode.TEXT:
            xw.TextWork(xe);
            break;
        }
    }

    public static void TraverseHglWorkBody(Element xe, hglWorkBody xw )
    {
        switch(xe.mode)
        {
        case Mode.TAG:
            xw.ElementWork(xe);
            if (xe.ChildCount > 0) foreach (var c in xe.List())
                {
                    TraverseHglWorkBody(c,xw);
                }
            xw.EndElementWork(xe);
            break;
        case Mode.COMENT:
            xw.CommentWork(xe);
            break;
        case Mode.TEXT:
            xw.TextWork(xe);
            //Debug.LogError("ModeText = " + xe);
            break;
        }
    }

    //public static void TraverseHglWork_NG(Element xe, Action<Element> act,Action<Element> actEnd )
    //{
    //    if (act!=null) act(xe);
    //    if (xe.ChildCount > 0)
    //    { 
    //        foreach (var c in xe.List())
    //        {
    //            TraverseHglWork_NG(c,act, actEnd);
    //        }
    //        if (actEnd!=null) actEnd(xe);
    //    }
    //}

    public static void TraverseHglWork(Element xe, Action<Element> act)
    {
        act(xe);
        if (xe.ChildCount > 0)
        { 
            foreach (var c in xe.List())
            {
                TraverseHglWork(c,act);
            }
            act(xe);
        }
    }

    public static void Traverse(Element xe, Action<Element> act)
    {
        act(xe);
        if (xe.ChildCount > 0) foreach (var c in xe.List()) Traverse(c,act);
    }
    public static void Traverse_w_depth(Element xe,int start_depth, Action<Element,int> act)
    {
        act(xe,start_depth);
        if (xe.ChildCount > 0) foreach (var c in xe.List()) Traverse_w_depth(c,start_depth+1,act);
    }
    public static void Traverse_w_index_count_depth(Element xe,int index, int count, int depth, Action<Element,int,int,int> act)
    {
        if (index >= 0) act(xe,index,count,depth);
        if (xe.ChildCount > 0)
        {
            var list = xe.List();
            for (int i = 0; i < list.Count; i++)
            {
                Traverse_w_index_count_depth(list[i],i,list.Count,depth+1,act);
            }
        }
    }

    public static IEnumerator TraverseAsync(Element xe, Action<Element> act, MonoBehaviour mono)
    {
        act(xe);
        if (xe.ChildCount > 0) foreach (var c in xe.List()) yield return mono.StartCoroutine(TraverseAsync(c,act,mono));
    }

    public static IEnumerator TraverseAsync_w_depth(Element xe, int depth, Action<Element, int> act, MonoBehaviour mono)
    {
        act(xe,depth);
        if (xe.ChildCount > 0) foreach (var c in xe.List()) { 
            yield return mono.StartCoroutine(TraverseAsync_w_depth(c,depth+1,act,mono));
            //yield return new WaitForSeconds(interval);
        }
    }
    public static IEnumerator TraverseAsync_w_depth_interval(Element xe, int depth, Func<Element,int,int,float> act, MonoBehaviour mono)
    {
        if (xe.ChildCount > 0) for(int i = 0;i<xe.List().Count;i++) {//foreach (var c in xe.List()) { 
            var c = xe.List()[i];
            var interval = act(c,i,depth);
            yield return mono.StartCoroutine(TraverseAsync_w_depth_interval(c,depth+1,act,mono));
            //yield return new WaitForSeconds(interval);
            yield return new WaitForSeconds(1);
        }
    }

    public static int GetMaxDepth(Element xe)
    {
        int max = 0;
        Traverse_w_depth(xe,0,(e,d)=>{
            max = Mathf.Max(max,d);
        });
        return max;
    }

    public static Element FindTag(string tag, Element xe)
    {
        Element foundXe=null;

        Action<Element> traverse = null;
        traverse = (e) => {
            if (foundXe!=null) return;
            if (e.mode != Mode.TAG) return;
            if (e.text == tag)
            {
                foundXe = e;
                return;
            }
            if (e.ChildCount>0) foreach(var c in e.List())
            {
                traverse(c);
            }
        };

        traverse(xe);

        return foundXe;
    }

    public static Element[] FindTags(string tag, Element xe)
    {
        List<Element> list = new List<Element>();

        Action<Element> traverse = null;
        traverse = (e) => {
            if (e.mode != Mode.TAG) return;
            if (e.text == tag)
            {
                list.Add(e);
            }
            if (e.ChildCount>0) foreach(var c in e.List())
            {
                traverse(c);
            }
        };
        traverse(xe);

        return list.ToArray();
    }

    public static Element FindID(string id, Element xe)
    {
        if (string.IsNullOrEmpty(id)) return null;
        Element foundXe=null;

        Action<Element> traverse = null;
        traverse = (e) => {
            if (foundXe!=null) return;
            if (e.mode != Mode.TAG) return;
            if (e.id == id)
            {
                foundXe = e;
                return;
            }
            if (e.ChildCount>0) foreach(var c in e.List())
            {
                traverse(c);
            }
        };

        traverse(xe);

        return foundXe;
    }

    public static void SavePositions(Element xe)
    {
        Action<Element> traverse = null;
        traverse = (e) => {
            if (e.bone1!=null) e.bone1_savepos = e.bone1.localPosition;
            if (e.bone2!=null) e.bone2_savepos = e.bone2.localPosition;
            if (e.ChildCount>0) foreach(var c in e.List())
            {
                traverse(c);
            }
        };
        traverse(xe);
    }

    public static Element CreateRowText(string itext)
    {
        Element root = new Element{mode = Mode.TAG, text = "root"};
        Element ne   = new Element{mode = Mode.TAG, text = "pre", attrib = new Hashtable(),parent = root};
        Element netxt= new Element{mode = Mode.TEXT, text = itext,attrib = new Hashtable(),parent = ne  };
 
        ne.Add(netxt);
        root.Add(ne);

        return root;
    }
#endif

}
}