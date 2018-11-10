using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;

/*

    xmlUtil  Dependent APIs

*/

public class hglUtil {

    // #################
    // # Interpret URL #
    public static bool InterpretURL(string url, out string url_wo_params, out Hashtable hash)
    {
        string parmstr;

        return InterpretURL(url,out url_wo_params,out parmstr, out hash);
    }
    public static bool InterpretURL(string url, out string url_wo_params, out string parmstr, out Hashtable hash)
    {
        parmstr = "";
        url_wo_params="";
        hash = new Hashtable();
        if (string.IsNullOrEmpty(url)) return false;

        //find ?
        int i1 = url.IndexOf('?');
        if (i1<0)
        {
            url_wo_params = url;
            return true;
        }

        url_wo_params = url.Substring(0,i1);
        parmstr       = url.Substring(i1+1);

        var prms = url.Substring(i1+1);
        if (prms.Length <= 3 || prms.Contains("=")==false) return true;

        var list = prms.Split(new char[]{'&'});
        foreach (var s in list)
        {
            int i2 = s.IndexOf('=');
            var key = s.Substring(0,i2);
            if (s.Length < key.Length + 2) continue;
            var val = s.Substring(i2+1);
            hash[key]=val;
        }

        // DEBUG
        var dbgs = " -- interpet URL - start - \n";
        dbgs += "url = " + url + ", wo params = "+ url_wo_params + "\n";
        foreach (var k in hash.Keys)
        {
            dbgs += k + "=" + (string)hash[k] + "\n";
        }
        dbgs += " -- interpet URL - end - ";

        return true;
    }
    // # Interpret URL #
    // #################


    // #######################
    // # GET RENDERER BOUNDS #
    public static Bounds GetRenderBounds(GameObject o)
    {
        Renderer[] list = o.GetComponentsInChildren<Renderer>();
        if (list==null || list.Length==0) return new Bounds();

        Bounds bds = list[0].bounds;
        if (list.Length > 1) for (int i = 1; i < list.Length; i++)
            {
                bds.Encapsulate(list[i].bounds);
            }

        return bds;
    }
    // # GET RENDERER BOUNDS #
    // #######################

    // ##################
    // # READ TEXT FILE #
    public class ReadTextFile
    {
        private ReadTextFile() { }
        
        string m_file;
        public bool   m_bHttp;
        public string m_error;
        public string m_text;

        public static ReadTextFile Create( string file )
        {
            ReadTextFile rf = new ReadTextFile();
            rf.m_file = file;
            rf.m_bHttp = file.Contains("http:") || file.Contains("https:");
            rf.m_error = null;
            rf.Read();      
            return rf;
        }

        void Read()
        {
            if (m_bHttp) return;
            if (hglEtc.check_head(m_file,hglConfig.GetResourceFrom(hglResourceFrom.RESOURCES)))//  hglConfig.GetResourceFrom()== hglResourceFrom.RESOURCES)
            {
                _ReadResourceFile();
            }
            else
            {
                _ReadFile();
            }
        }

        public IEnumerator Read(MonoBehaviour mono)
        {
            if (m_bHttp)
            {
                yield return mono.StartCoroutine(_ReadNetFile());
            }
        }

        void _ReadFile()
        {
            m_error = null;
            
            //Debug.LogError(m_file);

            try {
                var rs = new System.IO.StreamReader(m_file,System.Text.Encoding.UTF8);
                m_text =rs.ReadToEnd();
                rs.Close();

                Debug.Log( "_readfile="+ m_text);
            }
            catch
            {
                m_error = "FOLDER FILE NOT LOADED : " + m_file;
            }
        }

        void _ReadResourceFile()
        {            
            m_error = null;

            string assetpath = m_file.Substring(Application.dataPath.Length + "/Resources/".Length);
            Debug.Log(assetpath);
            if (assetpath.Substring(assetpath.LastIndexOf('.')) == ".css")
            {
                assetpath += ".txt";
            }
            {
                int i = assetpath.LastIndexOf('.');
                if (i<0)
                {
                    Debug.Log("unexpected");
                    return;
                }
                assetpath = assetpath.Substring(0,i);
            }


            Debug.Log(assetpath);
            UnityEngine.Object ast = Resources.Load(assetpath); 
            if (ast==null)
            {
                m_error = "FILE NOT LOADED : " + m_file;
                return;
            }
            m_text = ((TextAsset)ast).text;
        }

        IEnumerator _ReadNetFile()
        {
            var www = new WWW(m_file);
            yield return www;
 
            m_error = www.error;
            if ( www.text!=null)
            {
                if (www.text.ToLower().Contains("shift_jis") || www.text.ToLower().Contains("x-sjis") )
                {
                    m_text = hglEtc.SJIS2STRING(www.bytes);
                }
                else 
                {
                    try
                    {
                        m_text = www.text;
                    }
                    catch
                    {
                        m_text = hglEtc.SJIS2STRING(www.bytes);
                    }
                }
                m_error = null;

                m_text.Replace("<br>","<br/>");
                m_text.Replace("<hr>","<hr/>");

                //Debug.Log(m_text);
            }
        }

        //public void NormalizeText()
        //{
        //    var lines = m_text.Split('\xa','\xd');
        //    string text2 = "";
        //    for(int i = 0 ;i <lines.Length; i++)
        //    {
        //        var line = lines[i];
        //        if (line.Contains("<html"))
        //        {
        //            text2 = "";
        //            for (int i2 = i; i2 < lines.Length; i2++)
        //            {
        //                text2 += lines[i2] + "\x0a";
        //            }
        //            break;
        //        }
        //    }
        //    m_text = text2;
        //}

    }
    // # READ TEXT FILE #
    // ##################


    // ################
    // # READ TEXTURE #
    public class ReadTextureFile
    {
        private ReadTextureFile() { }
        
        string m_file;
        public bool   m_bHttp;
        public string m_error;
        public Texture m_texture;

        public static ReadTextureFile Create( string file )
        {
            ReadTextureFile rf = new ReadTextureFile();
            if (rf != null && !string.IsNullOrEmpty(file))
            {
                rf.m_file = file;
                rf.m_bHttp = file.Contains("http:") || file.Contains("https:");
                rf.Read();
            }
            return rf;
        }

        void Read()
        {
            if (m_bHttp) return;
            if (hglEtc.check_head(m_file,hglConfig.GetResourceFrom(hglResourceFrom.RESOURCES)))
            {
                _ReadResourceFile();
            }
            else 
            {
                _ReadFile();
            }
        }

        public IEnumerator Read(MonoBehaviour mono)
        {
            if (m_bHttp)
            {
                yield return mono.StartCoroutine(_ReadNetFile());
            }
        }

        void _ReadFile() 
        {
            var bins = System.IO.File.ReadAllBytes(m_file);
            var t = new Texture2D(4,4);
            t.LoadImage(bins);
            m_texture = t;
        }

        public static Texture ReadLocalFile(string file)
        {
            ReadTextureFile rf = new ReadTextureFile();
            rf.m_file = file;
            rf._ReadResourceFile();
            
            return rf.m_texture;
        }

        void _ReadResourceFile()
        {
            m_error = null;
            if (string.IsNullOrEmpty (m_file) ) return;

            string assetpath = m_file.Substring(Application.dataPath.Length + "/Resources/".Length);
            {
                int i = assetpath.LastIndexOf('.');
                if (i<0) { Debug.LogError("_ReadFileError:"+ assetpath); return;
                }
                assetpath = assetpath.Substring(0,i);
            }

            m_texture = (Texture)Resources.Load(assetpath);     
        }

        IEnumerator _ReadNetFile()
        {
            var www = new WWW(m_file);
            yield return www;

            m_error = www.error;
            if (m_error==null) m_texture = www.texture;
        }
    }
    // # READ TEXTURE #
    // ################

    // ###################
    // # COLOR TO STRING #
    public static string Color2String_RRGGBB(Color c)
    {
        int r = (int)(255 * c.r);
        int g = (int)(255 * c.g);
        int b = (int)(255 * c.b);

        return r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
    }
    // # COLOR TO STRING #
    // ###################


    // ##############
    // # ACCUME LOG #
    static string s_log="";
    public static void Log(string msg)
    {
        s_log += msg;
    }
    public static string GetLog() {return s_log;}
    // # ACCUME LOG #
    // ##############

    // ##############################
    // # LOAD PREFAB FROM RESOURCES #
    public static GameObject LoadPrefabFromResources(string name)
    {
        string assetpath = "Prefab/"+name;

        var prefab = (GameObject)Resources.Load(assetpath);
        if (prefab==null)
        {
            Debug.LogError("GetPrefabFromResources:" + name);
            return null;
        }

        return prefab;
    }
    // # LOAD PREFAB FROM RESOURCES #
    // ##############################

    // #################################
    // # GET LATEST NUMBER OF CHILDREN #
    public static int GetLatestNumOfChildren(GameObject p)
    {
        int num = -1;
        for(int i = 0; i<p.transform.childCount;i++)
        {
            var c = p.transform.GetChild(i);
            if ((c.name.Length>3) && (char.IsDigit(c.name[0])))
            {
                var a = int.Parse(c.name.Substring(0,3));
                if (a > num) num = a;
            }
        }
        return num;
    }
    // # GET LATEST NUMBER OF CHILDREN #
    // #################################


    // #################
    // # ADD PARAM URL #
    public static string AddParamURL(string url_i, string key, string val)
    {
        var url = hglUtil.RemoveParamURL(url_i,key);

        if (url==null||url.Length==0) return "?"+ key+"="+val;
        if (url[url.Length-1]=='?') return url  + key+"="+val;
        if (url.IndexOf('?')>=0)
        {
            if (url[url.Length-1]=='&') 
            {
                return url + key+"="+val;
            }
            else
            {
                return url + "&" + key + "=" + val;
            }
        }
        return url + "?" + key + "=" + val;
    }
    // # ADD PARAM URL #
    // #################

    // ####################
    // # REMOVE PARAM URL #
    public static string RemoveParamURL(string url, string key)
    {
        string url_wo_params; 
        Hashtable hash;

        if (InterpretURL(url,out url_wo_params, out hash)==false) return null;
        if (hash.ContainsKey(key)) hash.Remove(key);

        return CreateURLwithParam(url_wo_params,hash);
    }
    // # REMOVE PARAM URL #
    // ####################


    // #########################
    // # CREATE URL WITH PARAM #
    public static string CreateURLwithParam(string url, Hashtable hash)
    {
        if (hash==null || hash.Count==0) return url;

        string s ="";

        foreach(var key in hash.Keys)
        {
            if (s!="") s += "&"; 
            s+= key + "=" + (string)hash[key];
        }
        return url + "?" + s;
    }
    // # CREATE URL WITH PARAM #
    // #########################
 
    //###############
    //# CALC MARGIN #
    //public static void CalcMargin(GameObject[] lines, float def_top, float def_right, float def_bot, float def_left)
    //{
    //    Func<GameObject,bool> isTableObj = (o)=>{
    //        if (o.name.Contains("_TABLE?"))
    //        {
    //            return true;
    //        }

    //        return false;
    //    };

    //    // Calc Top and Bottom of each line.
    //    foreach(var line in lines)
    //    {
    //        if (isTableObj(line)) continue;
    //        var   lsd = line.GetComponent<xmlStyleData>();
    //        float top = Mathf.Max( def_top, lsd.marginSrc[xmlStyleData.TOP]);
    //        float bot = Mathf.Max( def_bot, lsd.marginSrc[xmlStyleData.BOT]);

    //        var list = xmlEtc.SortChildren(line);
    //        if (list != null) foreach(var o in list)
    //        {
    //            var osd = o.GetComponent<xmlStyleData>();
    //            if (osd!=null)
    //            {
    //                top = Mathf.Max(top,osd.marginSrc[xmlStyleData.TOP]);
    //                bot = Mathf.Max(bot,osd.marginSrc[xmlStyleData.BOT]);
    //            }
    //        }

    //        lsd.margin[xmlStyleData.TOP] = top;
    //        lsd.margin[xmlStyleData.BOT] = bot;
    //    }

    //    //Calc Right and Left
    //    foreach(var line in lines)
    //    {
    //        if (isTableObj(line)) continue;
    //        var lsd = line.GetComponent<xmlStyleData>();
    //        lsd.margin[xmlStyleData.RIGHT] = lsd.marginSrc[xmlStyleData.RIGHT];
    //        lsd.margin[xmlStyleData.LEFT]  = lsd.marginSrc[xmlStyleData.LEFT];

    //        var list = xmlEtc.SortChildren(line);
    //        if (list != null) foreach(var o in list)
    //        {
    //            var osd = o.GetComponent<xmlStyleData>();
    //            if (osd!=null)
    //            {
    //                osd.margin[xmlStyleData.RIGHT] = osd.marginSrc[xmlStyleData.RIGHT];
    //                osd.margin[xmlStyleData.LEFT]  = osd.marginSrc[xmlStyleData.LEFT];
    //            }
    //        }
    //    }
    //}

    //public static float CalcTopMargin(GameObject[] lines,int index, float def_top)
    //{
    //    xmlStyleData past = (index - 1 >= 0) ? lines[index-1].GetComponent<xmlStyleData>() : null;
    //    xmlStyleData cur  = lines[index].GetComponent<xmlStyleData>();
    //    float past_bot = (past!=null) ? Mathf.Max(def_top,past.margin[xmlStyleData.BOT],past.breakLineMargin_Back) : def_top;
    //    return Mathf.Max(past_bot,cur.breakLineMargin_Front,cur.margin[xmlStyleData.TOP]);
    //}

    //public static float CalcLeftMargin(GameObject[] list, int index, float def_left)
    //{
    //    xmlStyleData past = (index - 1 >= 0) ? list[index-1].GetComponent<xmlStyleData>() : null;
    //    xmlStyleData cur  = list[index].GetComponent<xmlStyleData>();
    //    float past_right  = (past!=null) ? Mathf.Max(def_left,past.margin[xmlStyleData.RIGHT]) : def_left;
    //    return Mathf.Max(past_right,cur.margin[xmlStyleData.LEFT]);  
    //}

    //# CALC MARGIN #
    //###############


    //###############################
    //# GET LOCAL TOP LEFT POSITION #
    //public static Vector3 GetLocalTopLeftPosition(GameObject o, GameObject panel)
    //{
    //    var bo = NGUIMath.CalculateRelativeWidgetBounds(panel.transform, o.transform);
    //    return bo.center - (bo.extents.x * Vector3.right) + (bo.extents.y * Vector3.up); 
    //}
    //# GET LOCAL TOP LEFT POSITION #
    //###############################

    //#########################
    //# SET TOP LEFT POSITION #
    //public static void SetLocalTopLeftPositionWidget(Vector3 dst, GameObject o, GameObject panel)
    //{
    //    Bounds bo = NGUIMath.CalculateRelativeWidgetBounds(panel.transform, o.transform);
    //    {
    //        var p =  bo.center + new Vector3(- bo.extents.x, bo.extents.y, 0 );
    //        var pc = bo.center - p;
    //        var cv = o.transform.localPosition - bo.center;
    //        var pv  = pc + cv;

    //        var mv = dst + pv;

    //        if (float.IsNaN(mv.x) || float.IsNaN(mv.y) || float.IsNaN(mv.z))
    //        {
    //            //
    //        }
    //        else 
    //        {
    //            o.transform.localPosition = new Vector3(mv.x, mv.y, dst.z);
    //        }
    //    }
    //}
    public static void SetLocalTopLeftPosition(Vector3 dst, GameObject o)
    {
        Bounds bo = o.renderer.bounds;
        {
            var p =  bo.center + new Vector3(- bo.extents.x, bo.extents.y, 0 );
            var pc = bo.center - p;
            var cv = o.transform.localPosition - bo.center;
            var pv  = pc + cv;

            var mv = dst + pv;

            if (float.IsNaN(mv.x) || float.IsNaN(mv.y) || float.IsNaN(mv.z))
            {
                //
            }
            else 
            {
                o.transform.localPosition = new Vector3(mv.x, mv.y, dst.z);
            }
        }
    }
    //# SET TOP LEFT POSITION #
    //#########################

    //#######################
    //# SET CENTER POSITION #
    //public static void SetCenterPosition(Vector3 dst, GameObject o, GameObject panel)
    //{
    //    Bounds bo = NGUIMath.CalculateRelativeWidgetBounds(panel.transform, o.transform);

    //    var cv = o.transform.localPosition - bo.center;
    //    var mv = dst + cv;
    //    o.transform.localPosition = new Vector3(mv.x, mv.y, dst.z);
    //}
    //# SET CENTER POSITION #
    //#######################

    //######################
    //# MERGE STYLE STRING #
    //public static string MergeStyleLine(string org, string add)
    //{
    //    var orglist = ParseStyleLine(org);
    //    var addlist = ParseStyleLine(add);

    //    foreach(var key in addlist.Keys)
    //    {
    //        orglist[key] = addlist[key];
    //    }

    //    return MakeStyleLine(orglist);

    //}

    // #######################################
    // # PARSE STYLE LINE /  MAKE STYLE LINE #
    public static Hashtable ParseStyleLine(string src)
    {
        Hashtable hash = new Hashtable();
        if (string.IsNullOrEmpty(src)) return hash;

        var list = src.Split(';');
        foreach(var s in list)
        {
            if (s.Contains(":")==false) continue;
            var s1 = hglEtc.NormalizeText_withCR(s);
            var key = hglEtc.NormalizeText_withCR(s1.Substring(0,s1.IndexOf(':')));  
            var val = hglEtc.NormalizeText_withCR(s1.Substring(s1.IndexOf(':')+1));

            hash[key] = val;
        }
        return hash;
    }
    public static string MakeStyleLine(Hashtable hash)
    {
        string s = "";
        foreach(var k in hash.Keys)
        {
            s+=(string)k + ":" + (string)hash[k] + ";";
        }
        return s;
    }
    // # PARSE STYLE LINE /  MAKE STYLE LINE #
    // #######################################


    // ############################
    // # SET SHADOW / SET OUTLINE #
    //public static void SetShadow(UILabel ul, xmlStyle style)
    //{
    //    float x,y;
    //    Color color;
    //    if (xmlConverter.GetTextShadowStyle(style.Get(StyleKey.text_shadow),out x, out y, out color))
    //    {
    //        ul.effectStyle = UILabel.Effect.Shadow;
    //        ul.effectColor = color;
    //        ul.effectDistance = new Vector2(x,y);
    //    }
    //}
    //public static void SetOutLine(UILabel ul, xmlStyle style)
    //{
    //    float x,y;
    //    Color color;
    //    if (xmlConverter.GetTextShadowStyle(style.Get(StyleKey.text_outline),out x, out y, out color))
    //    {
    //        ul.effectStyle = UILabel.Effect.Outline;
    //        ul.effectColor = color;
    //        ul.effectDistance = new Vector2(x,y);
    //    }
    //}
    // # SET SHADOW / SET OUTLINE #
    // ############################

    // ###########################################
    // # CREATE UI [LABEL|TEXTURE|SPRITE] BUTTON #
#if OBOSLATED
    public static GameObject CreateUITexture(GameObject panel, Texture tex, Material mat, Vector2 srcSize, Vector2 dstSize, bool bRepeat, UIWidget.Pivot pivot )
    {
        var ut = NGUITools.AddWidget<UITexture>(panel);
        ut.pivot = pivot;
        ut.mainTexture = tex;
        ut.renderer.material = mat;
        ut.transform.localScale = new Vector3(dstSize.x, dstSize.y, 1);
        if (bRepeat)
        {
            ut.renderer.material.SetTextureScale("_MainTex",new Vector2(dstSize.x / srcSize.x, dstSize.y/srcSize.y));
        }

        return ut.gameObject;
    }

    public static GameObject CreateUISprite(GameObject panel, UIAtlas atlas, string spriteName, Vector2 dstSize, bool bRepeat, UIWidget.Pivot pivot)
    {
        var us = NGUITools.AddWidget<UISprite>(panel);
        us.pivot = pivot;
        us.atlas = atlas;
        us.spriteName = spriteName;
        us.transform.localScale = new Vector3(dstSize.x, dstSize.y, 1);
        us.type = UISprite.Type.Simple;
        if (bRepeat)
        {
            us.type = UISprite.Type.Tiled;
        }

        return us.gameObject;
    }

    public static GameObject CreateUISpriteButton(
        GameObject panel, 
        UIAtlas    atlas, 
        string     spriteName,
        Vector2    dstSize,
        string     text = null,
        UIFont     font = null,
        float      fontSize = 16
    )
    {
        //refer to CreateButton at UICreateWidgetWizard.cs 

		int depth = NGUITools.CalculateNextDepth(panel);
		var go = NGUITools.AddChild(panel);
		go.name = "Button";

		UISprite bg = NGUITools.AddWidget<UISprite>(go);
		bg.type = UISprite.Type.Sliced;
		bg.name = "Background";
		bg.depth = depth;
		bg.atlas = atlas;
		bg.spriteName = spriteName;
		//bg.MakePixelPerfect();
		bg.transform.localScale = new Vector3(dstSize.x, dstSize.y, 1f);

		if (text != null)
		{
			UILabel lbl = NGUITools.AddWidget<UILabel>(go);
			lbl.font = font;
			lbl.text = go.name;
			lbl.MakePixelPerfect();
            lbl.transform.localPosition += Vector3.back;
            lbl.transform.localScale = fontSize * Vector3.one;
		}

		// Add a collider
		NGUITools.AddWidgetCollider(go);

		// Add the scripts
		go.AddComponent<UIButton>().tweenTarget = bg.gameObject;
		go.AddComponent<UIButtonScale>();
		//go.AddComponent<UIButtonOffset>();
		go.AddComponent<UIButtonSound>();
        var ubm = go.AddComponent<UIButtonMessage>();
        ubm.target = panel.GetComponentInChildren<xmlTags_html>().gameObject;
        ubm.functionName = "OnButtonClick";
        return go;
    }

    public static void UpdateUISpriteButtonByLabel(GameObject o, GameObject panel)
    {
        var label = o.GetComponentInChildren<UILabel>();
        var labelBo = NGUIMath.CalculateRelativeWidgetBounds(panel.transform,label.transform);
        var sprite = o.GetComponentInChildren<UISprite>();

        var sx = labelBo.size.x <= 0 ? 1 : labelBo.size.x;
        var sy = labelBo.size.y <= 0 ? 1 : labelBo.size.y;
        sprite.transform.localScale = new Vector3(sx,sy, 1);
        o.GetComponent<BoxCollider>().size = new Vector3(sx,sy, 1);
    }


    public static GameObject CreateUITextureButton_NULL(
        GameObject panel,
        string     text = null,
        UIFont     font = null,
        float      fontSize = 16
        )
    {
        // refer to CreateButton at UICreateWidgetWizard

		int depth = NGUITools.CalculateNextDepth(panel);
		var go = NGUITools.AddChild(panel);
		go.name = "Button";

		var bg = NGUITools.AddWidget<UITexture>(go);
		bg.name = "Background";
		bg.depth = depth;

		if (text != null)
		{
			UILabel lbl = NGUITools.AddWidget<UILabel>(go);
			lbl.font = font;
			lbl.text = go.name;
			lbl.MakePixelPerfect();
		}

		// Add a collider
		NGUITools.AddWidgetCollider(go);

		// Add the scripts
		go.AddComponent<UIButton>().tweenTarget = bg.gameObject;
		go.AddComponent<UIButtonScale>();
		//go.AddComponent<UIButtonOffset>();
		go.AddComponent<UIButtonSound>();
        var ubm = go.AddComponent<UIButtonMessage>();
        ubm.target = panel.GetComponentInChildren<xmlTags_html>().gameObject;
        ubm.functionName = "OnButtonClick";
        return go;
    }
    // # CREATE UI [TEXTURE|SPRITE] BUTTON #
    // #####################################


    // ######################
    // # FIND DISPLAY ITEMS #
    //public static GameObject[] FindDisplayItems(GameObject root)
    //{
    //    var list = new List<GameObject>();
        
    //    Action<Transform> travchildren = null;
    //    travchildren = (t) => 
    //    {
    //        if (t.GetComponent<xmlDisplayItem>() != null)
    //        {
    //            list.Add(t.gameObject);
    //            return;
    //        }

    //        if (t.childCount==0) return;
    //        for(int i=0;i<t.childCount;i++)
    //        {
    //            travchildren(t.GetChild(i));
    //        }
    //    };

    //    travchildren(root.transform);

    //    // Sort by name

    //    list.Sort(delegate(GameObject a, GameObject b){ return a.name.CompareTo(b.name); }  );

    //    return list.ToArray();
    //}
    // # FIND DISPLAY ITEMS #
    // ######################
#endif


    // ##############
    // # MERGE PATH #
    public static string MergePath(string path, string add)
    {
        string s = path;
        int index = 0;

        if (string.IsNullOrEmpty(add) || string.IsNullOrEmpty(path)) return null;

        if (hglEtc.check_head(add, "http:") || hglEtc.check_head(add, "https:") || hglEtc.check_head(add, "file:") ||  hglEtc.check_head(add,Application.dataPath) )
        {
            return add;
        }

        while (index < add.Length)
        {
            var c1 = add[index];
            var c2 = index+1 < add.Length ? add[index+1] : 0;
            var c3 = index+2 < add.Length ? add[index+2] : 0;

            if (c1 == '.' && c2 == '.' && c3 == '/')
            {
                // s + "../"
                int i = s.Length > 2 ? s.LastIndexOf('/',s.Length-2) : -1;
                if (i < 0)
                {
                    return null;
                }
                s = s.Substring(0,i+1);
                index+=3;
                continue;
            }
            else if (c1 == '.' && c2 == '/')
            {
                // s + "./"
                index += 2;
                continue;
            }
            
            s += c1;
            index ++;
        }

        //Debug.LogError( path + "+" + add + "=" + s );

        return s;
    }
    // # MARGE PATH #
    // ##############




    //public static string CreateAnchorText(string text, int id)
    //{
        


    //}
}
