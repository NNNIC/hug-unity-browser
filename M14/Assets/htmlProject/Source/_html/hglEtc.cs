using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using hug_u;

/*

    hglETC  General APIs.

*/
public class hglEtc : MonoBehaviour {
    public static bool check_head(string sample, string word)
    {
        try
        {
            if (
                (sample.Length >= word.Length)
                &&
                (sample.Substring(0, word.Length) == word)
                )
            {
                return true;
            }
        }
        catch { }
        return false;
    }
    public static bool check_tail(string sample, string word)
    {
        if (string.IsNullOrEmpty(sample) || string.IsNullOrEmpty(word)) return false;
        if (sample.Length < word.Length) return false;

        return (sample.Substring(sample.Length - word.Length) == word);
    }
    public static string get_fileTail(string filename)   // ex) hoge.txt    output=.txt
    {
        if (string.IsNullOrEmpty(filename)) return null;
        int i = filename.LastIndexOf('.');
        if (i<0) return null;
        return filename.Substring(i); 
    }
    public static void ifNanSetZero(ref float x) { x = float.IsNaN(x) ? 0 : x; }

    //public static bool check_valid(string s)
    //{
    //    if (s!=null && s.Length>0) return true;
    //    return false;
    //}
    public static string substring(string si, int size)
    {
        string s="";
        for (int i = 0; i < si.Length; i++)
        {
            var c = (si!=null && i < si.Length) ? si[i] : '\x00'; 
            s += c;
        }
        return s;
    }
    public static bool floatArrayTryParse(string s, out float[] list)
    {
        list = null;
        if (string.IsNullOrEmpty(s)) return false;
        var parts = s.Split(' ');
        var l = new List<float>();
        foreach(var i in parts)
        {
            float a;
            string val = hglEtc.DropAlphabet(i);
            if (float.TryParse(val,out a))
            {
                l.Add(a);
            }
            else
            {
                return false;
            }
        }
        list = l.ToArray();
        return true;
    }
    public static Vector3 toVector3(Vector2 v)
    {
        return new Vector3(v.x,v.y,0);
    }
    public static Vector2 toVector2(Vector3 v)
    {
        return new Vector2(v.x,v.y);
    }
    public static Vector3 toVector3(Color c)
    {
        return new Vector3(c.r,c.g,c.b);
    }
    public static Color toColor(Vector3 v)
    {
        return new Color(v.x,v.y,v.z,1);
    }
    public static string expand_hash(Hashtable hash)
    {
        if (hash==null || hash.Count==0) return "";
        string s = "";
        foreach(var key in hash.Keys)
        {
            s += (s=="") ? "" : "&";
            s += key + "=" + (string)hash[key];
        }
        return s;
    }
    public static GameObject CreateGameObject(string name, Transform parent)
    {
        GameObject o = new GameObject();
        o.name = name;
        o.transform.parent = parent;
        o.transform.localPosition = Vector3.zero;
        o.transform.localScale    = Vector3.one;
        return o;
    }
    public static string NormalizeText_withCR(string src)
    {
        //  Delete the chars less than and equal 0x20 ahead of and tail of each line.

        var lines = src.Split('\x0a','\x0d');
        
        string s="";
        
        foreach(var l in lines)
        {
            var t = l;
            while(t!=null&&t.Length>0&&t[0]<=0x20)
            {
                t = (t.Length==1) ?  t ="" : t.Substring(1);
            }
            while(t!=null&&t.Length>0&&t[t.Length-1]<=0x20)
            {
                t = (t.Length==1) ? t ="" : t.Substring(0,t.Length-1);
            }
            if (t!=null&&t.Length>0)
            {
                if (s!="") s+="\n";
                s+=t;
            }
        }

        return s;
    }

    public static string NormalizeText(string src)
    {
        var lines = src.Split('\x0a','\x0d');
        
        string s="";
        
        foreach(var l in lines)
        {
            var t = l;
            while(t!=null&&t.Length>0&&t[0]<=0x20)
            {
                t = (t.Length==1) ?  t ="" : t.Substring(1);
            }
            while(t!=null&&t.Length>0&&t[t.Length-1]<=0x20)
            {
                t = (t.Length==1) ? t ="" : t.Substring(0,t.Length-1);
            }
            if (t!=null&&t.Length>0)
            {
                s+=t;
            }
        }

        return s;
    }

    public static string DeleteUnsuportedChar(string src)
    {
        var s = "";
        foreach (var c in src)
        {
            if (c == '\n' || c >= ' ')
            {
                s += c;
            }
        }
        return s;
    }



    public static GameObject[] SortChildren(GameObject o, string containWord = null)
    {
        if (o.transform.childCount==0) return null;
        List<GameObject> dest_c_list  = new List<GameObject>();
        for (int i = 0; i < o.transform.childCount; i++) 
        {
            var oa = o.transform.GetChild(i);
            if (containWord != null)
            {
                if (oa.name.Contains(containWord)) dest_c_list.Add(oa.gameObject);
            }
            else
            {
                dest_c_list.Add(oa.gameObject);
            }
        }

        dest_c_list.Sort(delegate(GameObject a, GameObject b){ return a.name.CompareTo(b.name); });
       
        var list = dest_c_list.ToArray();

        return dest_c_list.ToArray();
    }

    public static float GetFloatFromStr_w_percent(string x)
    {
        float a;
        if (float.TryParse(x,out a))
        {
            return a;
        }

        if (string.IsNullOrEmpty(x)) return float.NaN;

        if (x[x.Length-1]=='%') 
        {
            if (float.TryParse(x.Substring(0,x.Length-1),out a))
            {
                return a/100f;
            }
        }
        return float.NaN;
    }

    public static bool ParseMethodText(string str, out string cmd, out string[] paras)
    {
        cmd = null;
        paras = null;
        
        var cmds = str.Split('(');
        if (cmds.Length!=2) return false;

        cmd = cmds[0];
        paras = cmds[1].Split(',',')');

        return true;
    }

    public static GameObject[] FindGameObjectByName(GameObject root, string name, bool isContain, int NestNum = 0)
    {
        List<GameObject> list = new List<GameObject>();

        try
        {
            hgca.TraverseGameObject(root.transform, (t) =>
            {
                bool b = false;
                if (isContain)
                {
                    if (t.name.Contains(name)) b = true;
                }
                else
                {
                    if (t.name == name) b = true;
                }

                if (b)
                {
                    list.Add(t.gameObject);
                }
            }, NestNum);
        }
        catch { return null;  }
        return list.ToArray();
    }

    public static string DropAlphabet(string s)
    {
        string d="";
		if (string.IsNullOrEmpty(s)) return null;
        foreach (var c in s)
        {
            if (c=='.' || (c>='0' && c<='9')) d += c;
        }
        return d;
    }

    public static float GetFloat(string key, Hashtable hash, float error = float.NaN)
    {
        if (hash.ContainsKey(key))
        {
            var str = (string)hash[key];
            str = DropAlphabet(str);            
            float f;
            if (float.TryParse(str, out f))
            {
                return f;
            }
        }

        return error;
    }
    public static float GetFloat< T >(int index, T[] list  , float error = float.NaN)
    {
        if (list==null || index >= list.Length) return error;
        object o = list[index];  if (o==null) return error;
        if (o.GetType().IsPrimitive)
         {
             if (o.GetType()==typeof(float))
             {
                return (float)o;
             }
             System.Convert.ChangeType(o,typeof(float));
             return (float)o;
        }
        var s = o.ToString();
        float f;
        if (float.TryParse(s, out f))
        {
            return f;
        }
        return error;
    }

    public static Color GetColor(string key, Hashtable hash, Color error)
    {
        if (hash.ContainsKey(key))
        {
            var str = (string)hash[key];
            Color col;
            if (hglConverter.GetColorString(str, out col))
            {
                return col;
            }
        }
        return error;
    }
    public static Color? GetColorIfErrorNull(string key, Hashtable hash)
    {
        if (hash.ContainsKey(key))
        {
            var str = (string)hash[key];
            Color col;
            if (hglConverter.GetColorString(str, out col))
            {
                return col;
            }
        }
        return null;
    }


    public static int CountNest(GameObject o)
    {
        int c = 0;
        Transform t = o.transform;
        while (t != null)
        {
            c++;
            t = t.parent;
        }
        return c;
    }

    public static GameObject[] SortByDeepNest(GameObject[] src)
    {
        List<GameObject> list = new List<GameObject>();
        list.AddRange(src);
        list.Sort(delegate(GameObject a, GameObject b){ return CountNest(b).CompareTo(CountNest(a));  });

        return list.ToArray();
    }

    public static string decodeTextToDisplay(string itext)
    { //http://pst.co.jp/powersoft/html/index.php?f=3401
        string s = "";

        int index = 0;
        while (index < itext.Length)
        {
            int start_lt = itext.IndexOf('&',index);
            if (start_lt >= 0)
            {
                s += itext.Substring(index, start_lt - index);

                index = start_lt;
                int end_gt = itext.IndexOf(';', index);
                if (end_gt < 0)
                {
                    s+="&??";
                    index++;
                    continue;
                }
                var w = itext.Substring(index, end_gt - index);
                switch (w)
                {
                case "&lt": s += "<"; break;
                case "&gt": s += ">"; break;
                case "&amp": s += "&"; break;
                case "&quot": s += "\""; break;
                case "&nbsp": s += " "; break;
                case "&copy": s += "\x00a9"; break;
                case "&laquo": s+= "\x00ab";break;
                case "&raquo": s+= "\x00bb";break;

                default:
                s += w + ";";
                break;
                }

                index = end_gt + 1;
                continue;
            }
            else
            {
                s += itext.Substring(index);
                break;
            }
        }

        return s;
    }
    public static  byte[] ObjectToByteArray(UnityEngine.Object obj)
    {
        if(obj == null)
            return null;
        var bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        var ms = new System.IO.MemoryStream();
        bf.Serialize(ms, obj);
        return ms.ToArray();
    }
    public static string SJIS2STRING(byte[] bytes)
    {
        string s = "";
        int index = 0;
        while (index < bytes.Length)
        {
            var c1 = bytes[index];
            if (index + 1 >= bytes.Length) break;

            var c2 = bytes[index+1];

            if ((c1 >= 0x81 && c1 < 0xa0) || (c1 >= 0xe0))
            {
                uint w = ((uint)c1 << 8) + (uint)c2;
                char c = GetChar((ushort)w);
                if (c != 0) s += c;
                index += 2;
                continue;
            }
            else if (c1 >= 0xa0 && c1 < 0xe0)
            {
                char c= GetChar((ushort)c1);
                if (c != 0) s+=c;
                index ++;
                continue;
            }

            s += (char)c1;

            index++;
        }

        return s;
    }

    static char GetChar(ushort w)
    {
        Func<int,ushort> getsjis = (i) => { return hgSjis2uc16table.table[i*2];    };
        Func<int,ushort> getuc   = (i) => { return hgSjis2uc16table.table[i*2+1];  };

        int len = hgSjis2uc16table.table.Length / 2;

        int bpoint = len >> 1; 
        for(int n = 2; n < 20; n++)
        {
            int i = (len >> n);
            if (i<1) i =1;

            var c = getsjis(bpoint);
            if (w < c)
            {
                bpoint -= i;
            }
            else if (w > c)
            {
                bpoint += i;
            }
            else
            {
                
                var uc = getuc(bpoint);
                //Debug.Log( "find : " + (n-1) + ", sjis:" + w.ToString("x") + ", uc:" + ((int)uc).ToString("x") + " " +uc  );
                return  (char)uc;
            }
        }
        return (char)0;
    }


    //
    //#
    public static string[] BreakUpStyleString(string str)
    {
        const int mode_none = 0;
        const int mode_word = 1;

        Func<char,bool> _isLetter = (c) => { return  (char.IsLetterOrDigit(c) || "_-#%.()".Contains(""+c) ); };
            
        List<string> list = new List<string>();
        int  mode = mode_none;
        int index = 0;
        string s = "";

        while (index < str.Length) {
            var c = str[index];
            if (mode == mode_none) {
                if (_isLetter(c)) {
                    s = "" + c;
                    mode = mode_word;
                }
                else if (c > ' ')
                {
                    list.Add("" + c);
                }
            }
            else 
            {
                if (_isLetter(c)) {
                    s += c;
                } else {
                    if (s!=null) list.Add(s); s=null;  
                    mode = mode_none;
                    if (c>' ') list.Add("" + c); 
                }
            }
            index++;
        }
        if (!string.IsNullOrEmpty(s)) list.Add(s);
        return list.ToArray();
    }

    public static int CountCharInString(string str, char sc)
    {
        int cnt = 0;
        if (string.IsNullOrEmpty(str)) return 0;
        foreach (var c in str)
        {
            if (c==sc) cnt++;
        }
        return cnt;
    }
    //public static bool CheckNguiColorString(string text, int start, out int newIndex, out string cstr, out int cnum)
    //{
    //    newIndex = -1;
    //    cstr     = null;
    //    cnum     = -1;

    //    if (string.IsNullOrEmpty(text) || start < text.Length) return false;
    //    char c0 = text[start];
    //    char c1 = start+1 < text.Length ? text[start+1] : '\x00';

     
    //    if (c0!='[') return false;

    //    int endIndex = text.IndexOf(']',start);
    //    int len = start-endIndex - 1;
    //    if (len==1 && c1=='-') 
    //    {
    //        newIndex = start + 3;
    //        cstr = c1.ToString();
    //        return true;
    //    }
    //    else if (len==6)
    //    {
    //        cstr = text.Substring(start+1,6);
    //        newIndex += 8;
    //        int x;
    //        if (int.TryParse(cstr, System.Globalization.NumberStyles.HexNumber,System.Globalization.CultureInfo.InvariantCulture,out x))
    //        {
    //            cnum = x; 
    //            return true;    
    //        }
    //    }
    //    return false;
    //}

    public static Vector3 CalcLocalPosition(Transform cur, Vector3 pos ,Transform target)
    {
        Vector3 worldpos = cur.TransformPoint(pos);
        return target.InverseTransformPoint(worldpos);
    }
}
