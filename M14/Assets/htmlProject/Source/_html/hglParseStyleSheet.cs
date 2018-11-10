using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using hug_u;

public class hglParseStyleSheet {
	
	// ref : http://www.htmq.com/csskihon/007.shtml

    class KEYVAL
    {
        public List<string> keys;
        public List<string> KeyGroups;
        public string  val;

        public void MakeKeyGroups()
        {
            KeyGroups = new List<string>();
            
            string s=null;
            foreach (var w in keys){
                var c = w[0];
                if (c == ','){
                    if (s != null) KeyGroups.Add(s.ToLower());
                    s=null;
                } else{
                    if (s==null) s = "";
                    else if (s==":")
					{
						//s=s;
					}
					else 
					{
						s+= " ";
					}
                    s+=w;
                }
            }
            if (s!=null) KeyGroups.Add(s.ToLower());
        }

        public static string Find(string n, List<KEYVAL> list)
        {
            if (list!=null)foreach (var kv in list)
            {
                foreach (var k in kv.KeyGroups)
                {
                    if (n == k) return kv.val;
                }
            }
            return null;
        }
    }
    List<KEYVAL> m_list;

    public hglParseStyleSheet()
    {
        m_list = new List<KEYVAL>();
    }

    public void Parse(string istr)
    {
        const int mode_none=0;
        const int mode_name=1;
        const int mode_data=2;

        var str       = commentout(istr);

        if (string.IsNullOrEmpty(str)) return;
        if (!str.Contains(":")) return;

        var words = hglEtc.BreakUpStyleString(str);// foreach(var w in words) Debug.Log(w);
        int mode = mode_none;        
      
        KEYVAL kv = null;
        
        for(var n=0;n<words.Length;n++)
        {
            var w = words[n];
            var c = words[n][0];
            if (mode == mode_none)
            {
                mode = mode_name;
                if (kv==null) kv = new KEYVAL(){ keys = new List<string>(), val = "" };
                kv.keys.Add(w);
            }
            else if (mode == mode_name)
            {
                if (c == '{')
                {
                    if (kv == null) Debug.LogError("Unexpected!");
                    mode = mode_data;
                }
                else
                {
			        //w =w.ToLower();
                    kv.keys.Add(w);
                }
            }
            else // if (mode == mode_data)
            {
                if (c == '}')
                {
                    mode = mode_none;
                    if (kv == null) Debug.LogError("Unexpected!");
                    kv.MakeKeyGroups();
                    m_list.Add(kv); kv = null;
                }
                else
                {
                    kv.val += w + " ";   
                }
            }
        }
    }

    string commentout(string str)
    {
        if (string.IsNullOrEmpty(str)) return null;
        string s = "";
        int index = 0;
        bool inCmt =false;
        while(index < str.Length)
        {
            if (inCmt)
            {
                var i = str.IndexOf("*/",index);
                inCmt = false;
                s += " ";
                index = i + 2;
            }
            else
            {
                var i = str.IndexOf("/*",index);
                if (i > 0)
                {
                    inCmt = true;
                    s += str.Substring(index, i-index);
                    index = i + 2;
                }
                else
                {
                    s += str.Substring(index);
                    break;
                }
            }
        }
        return s;
    }

    public override string ToString()
    {
        string t = "";

        if (m_list!=null)foreach(var i in m_list)
        {
            var k = ""; foreach(var l in i.KeyGroups) k += l + ",";
            t += k + " { " + i.val + "} \n";            
        }

        return t;
    }

    public string GetStyle(string ikey)
    {
        if (m_list==null) return null;

        foreach (var i in m_list)
        {
            if (ikey[0] == '.')
            {
                var key = ikey.Substring(1);
                if (i.keys.Count > 1 && key == i.keys[1])
                {
                    //Debug.Log( ikey + "{" + i.val  );
                    return i.val;
                }
            }

            if (i.keys[0] == ikey)
            {
                //Debug.Log( ikey + "{" + i.val  );
                return i.val;
            }
        }
        return null;
    }

    public string GetStyle_class_tag(string classname, hglParser.Element xe)
    {
        var curTag = xe!=null ? xe.text : null ;
        if (curTag==null) return null;

        string s = null;
        if (string.IsNullOrEmpty(classname))
        {
            s = KEYVAL.Find(curTag,m_list);           
        }
        if (s == null)
        {
            s = KEYVAL.Find(curTag + "." + classname, m_list);
        }
        if (s == null)
        {
            s = KEYVAL.Find("." + classname, m_list);
        }

        return s; 
    }

    public bool GetLinkStyleColor(string kind, out Color? ocol)
    {
        Color col;
        ocol = null;
        if (GetLinkStyleColor(kind, out col))
        {
            ocol = (Color?)col;
            return true;
        }
        return false;       
    }
    public bool GetLinkStyleColor(string kind, out Color col)
    {
        col = Color.black;
        var s = KEYVAL.Find(kind,m_list);
        if (string.IsNullOrEmpty(s)) return false;
        hglStyle style = new hglStyle(null);
        style.Parse(s);
        col = style.GetColor(StyleKey.color);
        return true;
    }
}
