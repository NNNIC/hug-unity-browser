using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using hug_u;

public class hgBMFont : MonoBehaviour {

    public UnityEngine.Object m_bmFontTxt; 

    public int m_cutUpperSize = 0;
    public int m_curLowerSize = 0;

    public hgBMFontData m_bmFonrData;

    [ContextMenu("Parse")]
    void Parse()
    {
        m_bmFonrData = new hgBMFontData(m_bmFontTxt.ToString(),m_cutUpperSize,m_curLowerSize);
        m_bmFonrData.Parse();
    }

#if XXX
    [System.Serializable]
    public class BMFONTDATA
    {
        //info
        public string face;
        public int    size;
        public int[]  padding;
        public int[]  spacing;
        public int    outline;

        //common
        public int lineHieht;
        public int _base;
        public int scaleW;
        public int scaleH;
        public int _pages;
        public int packed;
        public int alphaChnl;
        public int redChnl;
        public int greenChnl;
        public int blueChnl;

        //pages
        [System.Serializable]
        public class PAGE { public int id; public string file;}
        public PAGE[] pages;

        //char
        [System.Serializable]
        public class CHAR {
            public int id;
            public int x;
            public int y;
            public int width;
            public int height;
            public int xoffset;
            public int yoffset;
            public int xadvance;
            public int page;
            public int chnl;
        }
        
        public CHAR[] chars;

        //kerning
        [System.Serializable]
        public class KERNING
        {
            public int first;
            public int second;
            public int amount;
        }
        public KERNING[] kernings;
    }

    public BMFONTDATA bm; 



    [ContextMenu("Parse")]
    void Parse()
    {

        bm = new BMFONTDATA();

        Func<string,int> ip = (val)=> {
            return int.Parse(val);
        };

        int pagecnt = 0;
        int charcnt = 0;
        int kerncnt = 0;
        var rs = new ReadText(m_bmFontTxt.ToString());
        while (true)
        {
            var line = rs.GetLine();
            if (string.IsNullOrEmpty(line)) break;
            var parts = line.Split(' ');
            if (parts==null || parts.Length==0) continue;
            if (parts[0] == "info")
            {
                foreach (var p in parts)
                {
                    var kv = p.Split('=');
                    if (kv.Length!=2) continue;
                    var k = kv[0]; var v = kv[1];
                    switch (k)
                    {
                        case "face": bm.face = v; break;
                        case "size": bm.size = ip(v); break;
                        case "padding":
                        { 
                            bm.padding = new int[4];
                            var es = v.Split(',');
                            for (int i = 0; i < 4; i++) bm.padding[i] = int.Parse(es[i]);
                            break;
                        }
                        case "spacing": 
                        { 
                            bm.spacing = new int[2];
                            var es = v.Split(',');
                            for (int i = 0; i < 2; i++) bm.spacing[i] = int.Parse(es[i]);
                            break;
                        }
                        case "outline": bm.outline = ip(v); break;
                    }
                }
            }
            if (parts[0] == "common")
            {
                foreach(var p in parts)
                {
                    var kv = p.Split('=');
                    if (kv.Length!=2) continue;
                    var k = kv[0]; var v = kv[1];
                    switch(k)
                    {
                    case "lineHieht" : bm.lineHieht = ip(v); break;
                    case "base"      : bm._base     = ip(v); break;
                    case "scaleW"    : bm.scaleW    = ip(v); break;
                    case "scaleH"    : bm.scaleH    = ip(v); break;
                    case "pages"     : 
						bm._pages    = ip(v); break;
                    case "packed"    : bm.packed    = ip(v); break;
                    case "alphaChnl" : bm.alphaChnl = ip(v); break;
                    case "redChnl"   : bm.redChnl   = ip(v); break;
                    case "greenChnl" : bm.greenChnl = ip(v); break;
                    case "blueChnl"  : bm.blueChnl  = ip(v); break;
                    }                
                }
            }
            if (parts[0] == "page")
            {
                if (bm.pages == null || bm.pages.Length == 0) bm.pages = new BMFONTDATA.PAGE[bm._pages];

                var np = new BMFONTDATA.PAGE();
                foreach(var p in parts)
                {
                    var kv = p.Split('=');
                    if (kv.Length!=2) continue;
                    var k = kv[0]; var v = kv[1];
                    switch(k)
                    {
                    case "id"        : np.id   = ip(v); break;
                    case "file"      : np.file = v;     break;
                    }                
                }
                Debug.Log(line);
                bm.pages[pagecnt++] = np;
            }
            if (parts[0] == "chars")
            {
                foreach(var p in parts)
                {
                    var kv = p.Split('=');
                    if (kv.Length!=2) continue;
                    var k = kv[0]; var v = kv[1];
                    if (k == "count") 
                    {
                        int num = ip(v);
                        bm.chars = new BMFONTDATA.CHAR[num];
                    }
                }            
            }
            if (parts[0] == "char")
            {
                var nc = new BMFONTDATA.CHAR();
                foreach(var p in parts)
                {
                    var kv = p.Split('=');
                    if (kv.Length!=2) continue;
                    var k = kv[0]; var v = kv[1];
                    switch(k)
                    {
                    case "id"        : nc.id        = ip(v); break;
                    case "x"         : nc.x         = ip(v); break;
                    case "y"         : nc.y         = ip(v); break;
                    case "width"     : nc.width     = ip(v); break;
                    case "height"    : nc.height    = ip(v); break;
                    case "xoffset"   : nc.xoffset   = ip(v); break;
                    case "yoffset"   : nc.yoffset   = ip(v); break;
                    case "xadvance"  : nc.xadvance  = ip(v); break;
                    case "page"      : nc.page      = ip(v); break;
                    case "chnl"      : nc.chnl      = ip(v); break;
                    }
                }
                bm.chars[charcnt++] = nc;
            }
            if (parts[0] == "kernings")
            {
                foreach(var p in parts)
                {
                    var kv = p.Split('=');
                    if (kv.Length!=2) continue;
                    var k = kv[0]; var v = kv[1];
                    if (k == "count") 
                    {
                        int num = ip(v);
                        bm.kernings = new BMFONTDATA.KERNING[num];
                    }
                }            
            }
            if (parts[0] == "kerning")
            {
                var nk = new BMFONTDATA.KERNING();
                foreach(var p in parts)
                {
                    var kv = p.Split('=');
                    if (kv.Length!=2) continue;
                    var k = kv[0]; var v = kv[1];
                    switch(k)
                    {
                    case "first"     : nk.first     = ip(v); break;
                    case "second"    : nk.second    = ip(v); break;
                    case "amount"    : nk.amount      = ip(v); break;
                    }
                }
                bm.kernings[kerncnt++] = nk;
                
            }

        }
    }

    class ReadText
    {
        string text;
        int    index;

        public ReadText(string itext)
        {
            text = itext;
            index = 0;
        }

        public string GetLine()
        {
            if (index >= text.Length) return null;
            string s = "";
            while(true)
            {
                if (index>=text.Length) return s;
                var c = text[index++];
                if (c < 0x20)
                {
                    if (s == "")
                    {
                        continue;
                    }
                    else
                    {
                        return s;
                    }
                }
                s += c;
            }
        }
    }
#if OBSOLATED
    [ContextMenu("TestMesh")]
    void TestMesh()
    {
        var go = m_testObject;
        var mf = go.GetComponent<MeshFilter>();

        var mesh = UserMesh.CreateOneRectangle(30,30);

        mf.sharedMesh = mesh; 
    }
#endif
    public bool GetInfo(char code, char firstcode, out int size, out int _base, out Vector2[] uv, out int widht, out int height, out int xoffset, out int yoffset,out int xadvance, out int chnl)
    {
        Func<int, int> getCode = (index) =>{ if (index >=0 && index < bm.chars.Length)  return bm.chars[index].id; return -1;/* throw new SystemException( "ERROR:GetInfo outof index:"+index + ",code:" + ((int)code).ToString() );  */ };
       
        size = 0; _base = 0; uv=null;widht=0;height=0;xoffset = 0; yoffset=0;xadvance = 0; chnl=0;

        int len = bm.chars.Length;
        int bpoint = -1;
        int icode = (int)code;
        for (int n = 1; n < 2000; n++)
        {
            if (bpoint<0) bpoint = len >> 1;
            int w = (len >> (n+1));
            if (w<1) w =1;

            var c = getCode(bpoint);
            if (c < 0) return false;
            if (icode < c)
            {
                bpoint -= w;
            }
            else if (icode > c)
            {
                bpoint += w;
            }
            else
            {   /*
                   0-1
                   | |
                   2-3
                */
                var ans = bm.chars[bpoint];
                size = Mathf.Abs(bm.size) - m_cutUpperSize - m_curLowerSize;
                _base = bm._base  - m_cutUpperSize;
                uv   = new Vector2[4];
                {
                    var  u0 = (float)ans.x/2048f;
                    var  v0 = (float)(2048 - ans.y)/2048f;
                    var  wh = (float)ans.width / 2048f;
                    var  ht = (float)ans.height/ 2048f;
                    
                    uv[0] = new Vector2(u0,   v0);
                    uv[1] = new Vector2(u0+wh,v0);
                    uv[2] = new Vector2(u0,   v0-ht);
                    uv[3] = new Vector2(u0+wh,v0-ht);
                }
                widht   = ans.width;
                height  = ans.height;
                xoffset = ans.xoffset;
                yoffset = ans.yoffset - m_cutUpperSize ;
                xadvance = ans.xadvance;
                chnl     = ans.chnl;

                int ka = KerningAmount(firstcode, code);
                xadvance -= ka;


                return true;
            }
        }

        size = _base = widht = height = xoffset = yoffset = xadvance = chnl = 0;
        uv   = null;
        return false;
    }

    public int KerningAmount(int first, int second)
    {
        if (bm.kernings==null || bm.kernings.Length==0) return 0;
        foreach (var d in bm.kernings)
        {
            if (d.first == first && d.second == second)
            {
                return d.amount;
            }
        }
        return 0;
    }
    #endif

}
