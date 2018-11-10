using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using hug_u;

namespace xmlScriptJS  {
    public class ARGS
    {
        public object[] m_args;

        public bool check(params Type[] args)
        {
            if (m_args==null || m_args.Length != args.Length) return false;
            for(int i = 0; i < args.Length; i++)
            {
                if (m_args[i].GetType() != args[i]) return false;
            }
            return true;
        }
        public object Get(int i) 
        {
            return m_args[i];
        }
    }


    public class DocumentElement
    {
        public GameObject         gameObject;
        public hglParser.Element  xe;
        public hgRect             rect;
    }


    public class Document 
    {
        xmlScriptObj m_scrObj;

        public Document(xmlScriptObj scrObj)
        {
            m_scrObj = scrObj;
        }

        public void write(object o)
        {
            write(o.ToString());
        }

        void write(string str)
        {
            m_scrObj.m_stdout += str;   
        }

        public override string ToString()
        {
            return "Document";
        }

        public DocumentElement getElementById(object p)
        {
#if DISABLE 
            var id = p.ToString();
            var xmltags = m_scrObj.GetComponent<xmlTags_html>();

            Debug.Log("id="+id);
            Debug.Log("m_id["+id +"]="+xmltags.m_id[id]);

            string d = "ids=>"; foreach(var k in xmltags.m_id.Keys) d += k +"=" + xmltags.m_id[k] +",";
            Debug.Log(d);


            GameObject o = (GameObject)xmltags.m_id[id];
            if (o == null)
            {
                Debug.LogWarning("getElementById ERROR");
                write("getElementById ERROR:CAN'T FIND ID");
                return null;
            }
            var de = new DocumentElement();
            de.gameObject = o;
            Debug.LogWarning("getElementById o" + o);
            return de;
#endif
            var id = p.ToString();
            var body = ((hglWindowInfo)hgca.FindAscendantComponent(m_scrObj.gameObject,typeof(hglWindowInfo))).m_curBodyElement;

 
            var elm = hglParser.FindID(id,body);
            if (elm != null && elm.FindBone() != null)
            {
                var de = new DocumentElement();
                de.gameObject = elm.FindBone().gameObject;
                de.xe  = elm;

                var rect = new hgRect(elm.formatBase.doneRealRect);
                var v    = Vector2.zero - (elm.formatBase.doneRealRect.center);
                rect.Move( v );
                de.rect = rect;

                return de;
            }
            return null;
        }
    }
    
    public class Array
    {
        //enum Mode
        //{
        //    NONE,
        //    INT,
        //    FLOAT,
        //    DOUBLE,
        //    MIX,
        //}

        //Mode      m_mode;
        Hashtable m_hash;        

        public Array()
        {
            m_hash = new Hashtable();
            //m_mode = Mode.NONE;
        }
        public Array(ARGS arg )
        {
            //m_mode = Mode.NONE;
            m_hash = new Hashtable();

            if (arg != null && arg.m_args.Length == 1 && arg.m_args[0].GetType() == typeof(int))
            {
                for (int i = 0; i < (int)arg.m_args[0]; i++)
                {
                    Set(i,new object());
                }
            }
            
            if (arg!=null && arg.m_args.Length > 0)
            {
                for (int i = 0; i < arg.m_args.Length; i++)
                {
                    Debug.Log("[" + i + "]=" + arg.m_args[i]);
                    Set(i,arg.m_args[i]);
                }
            }
        }

        public int Lenght { get { return m_hash.Count;}}
        public Array concat(ARGS arg)
        {
            if (arg==null || arg.m_args==null || arg.m_args.Length==0) return this;

            int i = 0;

            Array newarray = new Array();
            foreach (var k in m_hash.Keys)
            {
                newarray.Set(i++, m_hash[k]);
            }

            foreach (var a in arg.m_args)
            {
                if (a.GetType() == typeof(Array))
                { 
                    Hashtable h = (Hashtable)a;
                    foreach (var k in h.Keys)
                    {
                        newarray.Set(i++, h[k]);
                    }
                }
            }
            
            return newarray;
        }
        public string Join(ARGS arg)
        {
            string cchar = null;
            if (arg==null || arg.m_args==null || arg.m_args.Length==0) cchar = "";

            if (cchar == null)
            {
                if (arg.m_args[0].GetType()==typeof(string)) cchar = (string)arg.m_args[0];
            }
            else cchar = "";

            string s = "";

            var keys = new List<object>(); foreach (var k in m_hash.Keys) keys.Add(k);
            for (int i = 0; i < keys.Count; i++)
            {
                if (i!=0) s+=cchar;
                s += m_hash[keys[i]];
            }

            return s;
        }
        public Array slice(ARGS arg)
        {
            if (!arg.check(typeof(int),typeof(int))) return null;
            var start = (int)arg.Get(0);
            var end   = (int)arg.Get(1); 
            
            var keyList = new List<object>(); foreach(var k in m_hash.Keys) keyList.Add(k);
            var valList = new List<object>(); foreach(var k in m_hash.Keys) keyList.Add(m_hash[k]);
            var newarray = new Array();
            for(int i = start;i<end;i++) newarray.Set(keyList[i],valList[i]);
            return newarray;
        }
        public Array splice(ARGS arg)
        {
            if (!arg.check(typeof(int),typeof(int),typeof(string))) return null;
            int start = (int)arg.Get(0);
            int count = (int)arg.Get(1);
            string wd = (string)arg.Get(2);

            var keyList = new List<object>(); foreach(var k in m_hash.Keys) keyList.Add(k);
            var valList = new List<object>(); foreach(var k in m_hash.Keys) keyList.Add(m_hash[k]);
            var newarray = new Array();

            for (int i = 0; i < keyList.Count; i++)
            {
                object o = valList[i];
                if (i >= start && i<start + count) o = wd;
                newarray.Set(keyList[i],o);
            }

            return newarray;
        }

        public void Set(object index, object obj)
        {
            if (index == null || (index.GetType()==typeof(string) && index.ToString().Length==0)) throw new SystemException("ERROR ARRAY INDEX.");
            m_hash[index.ToString()] = obj;

            //switch(m_mode)
            //{
            //case Mode.NONE:
            //    if (obj.GetType()==typeof(int)) m_mode = Mode.INT;
            //    else if (obj.GetType()==typeof(float)) m_mode = Mode.FLOAT;
            //    break;
            //case Mode.INT:
            //    if (obj.GetType()!=typeof(int)) m_mode = Mode.MIX;
            //    break;
            //case Mode.FLOAT:
            //    if (obj.GetType()!=typeof(float)) m_mode = Mode.MIX;
            //    break;
            //default:
            //    m_mode = Mode.MIX;
            //    break;
            //}
        }
        public object Get(object index) { 
            object o = m_hash[index.ToString()];
            //if (o==null) return ReturnNULL(m_mode);
            return o;
        }

        public override string ToString()
        {
            if (m_hash==null || m_hash.Count==0) return "[(no array)]";
            
            string s = ToString_print_ARRAY(m_hash);

            return s;
        }

        string ToString_print_ARRAY(object ihash)
        {
            if (ihash==null) return "(unknown)";
            if (ihash.GetType() != typeof(Hashtable))
            {
                return ihash.ToString();
            }
            Hashtable hash = (Hashtable)ihash;

            if (hash.Count==0) return "[(no array)]";
            string s = "[";
            List<object> keys = new List<object>(); foreach(var k in hash.Keys) keys.Add(k);
            for(int i = 0; i<hash.Count;i++)
            {   
                var o = hash[keys[i]];
                
                string os = (o.GetType()==typeof(Hashtable)) ?  ToString_print_ARRAY((Hashtable)o) : o.ToString();
                if (i!=0) s+=",";
                s+= keys[i].ToString() + "=>'" + os +"'";
            }
            s+="]";
            return s;
        }

        //static object ReturnNULL(Mode mode)
        //{
        //    switch(mode)
        //    {
        //    case Mode.INT: return 0;
        //    case Mode.FLOAT: return 0f;
        //    }
        //    return "";
        //}

        public static object GetMultidimension(object iarray, object[] index)
        {
            if (iarray==null || iarray.GetType()!=typeof(Array) || index==null || index.Length<2) return null;
            var array = (Array)iarray;

//            foreach(var i in index) Debug.Log("index="+i);
//            Debug.Log("GetMultidimension ARRAY=" + array);

            Hashtable hash = array.m_hash;
            for (int i = 0; i < index.Length-1; i++)
            {
                var _index = index[i];
                var _t = hash[_index.ToString()]; 
                //Debug.Log("GetMultidimension " + _index + "=>" + array.ToString_print_ARRAY(_t));
                if (_t.GetType() == typeof(Array))
                {
                    hash =((Array)_t).m_hash;
                }
                else 
                { 
                    Debug.Log("GetMultidimension ERROR " + _index + "=>" + _t);
                    return null;
                }
            }

            var o = hash[index[index.Length-1].ToString()];
            //if (o==null) return ReturnNULL(array.m_mode);
            return o;
        }

        public static void SetMultidimension(object iarray, object[] index, object val)
        {
            if (iarray==null || iarray.GetType()!=typeof(Array) || index == null || index.Length <2 ) return;
            var array = (Array)iarray;

            Hashtable hash = array.m_hash;
            for (int i = 0; i < index.Length - 1; i++)
            {
                var _index = index[i];

                if (hash[_index.ToString()] == null || hash[_index.ToString()].GetType() !=typeof(Array)) hash[_index.ToString()]= new Array();
                var o = hash[_index.ToString()];
                if (o.GetType() == typeof(Hashtable))
                { 
                    hash = (Hashtable)o;
                }
                else if (o.GetType() == typeof(Array))
                {
                    var ao = (Array)o;
                    hash = ao.m_hash;
                }
                else throw new SystemException("UNEXPECTED!");
            }

            hash[index[index.Length-1].ToString()] = val;
        }
    }

    public class _Debug
    {
        public xmlScriptObj m_scrObj;
        public _Debug(xmlScriptObj scrObj)
        {
            m_scrObj = scrObj;
        }
        
        public string pgm
        {
            get { return m_scrObj.m_rootElement.ToString().Replace("\\n","[BR]").Replace("`","\\n") + "\\n"; }
        }
    }

    public class Location
    {
        xmlScriptObj m_scrObj;

        hglWindowInfo   __winInfo;
        hglWindowInfo   m_winInfo
        {
            get{  
                if (__winInfo==null) __winInfo = (hglWindowInfo)hgca.FindAscendantComponent(m_scrObj.gameObject,typeof(hglWindowInfo));
                return __winInfo;
            }
        }
        hglHtmlRenderInfo __renderInfo;
        hglHtmlRenderInfo m_renderInfo
        {
            get
            {
                if (__renderInfo == null) {
                    __renderInfo = m_scrObj.GetComponent<hglHtmlRenderInfo>();
                    if (__renderInfo==null) __renderInfo = (hglHtmlRenderInfo)hgca.FindAscendantComponent(m_scrObj.gameObject,typeof(hglHtmlRenderInfo));
                }
                return __renderInfo;
            }
        }

        public Location(xmlScriptObj obj) {m_scrObj = obj;}

        public string href
        {
            get {
                return m_winInfo.m_curUrl + (string.IsNullOrEmpty(m_winInfo.m_curUrlParams) ? "" : "?" + m_winInfo.m_curUrlParams);
            }
            set
            {
                var url = m_winInfo.CreateFullPath(value);
                m_scrObj.SendMessageUpwards("Browse",url);
            }
        }
        public string search_get(object o)
        {
            string url;
            Hashtable hash;

            if (hglUtil.InterpretURL(href, out url, out hash))
            {
                var s = (string)hash[o.ToString()];
                if (string.IsNullOrEmpty(s)) return "";
                return s;
            }

            return "";
        }
        public string search
        {
            get {
                return m_winInfo.m_curUrlParams;
            }
        }
    }


}
