using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using hug_u;

public class hglReadHtml : MonoBehaviour {

    public hglResourceMan       m_resman;

    hglWindowInfo               m_winInfo;
    hglHtmlRenderInfo        m_renderInfo;
    
    hglHtmlRender        m_rendere;
    xmlScriptMan         m_xmlScript;

    string               m_saveUrl;

    class HISTORY
    {
        List<string> list;
        public HISTORY() {list = new List<string>();}

        public void Push(string url)
        {
            string latest = list.Count>0 ? list[list.Count-1] : "";
            if (latest != url)
            {
                list.Add(url);
            }
        }

        public string Pop()
        {
            string latest = list.Count>0 ? list[list.Count-1] : null;
            if (!string.IsNullOrEmpty(latest))
            {
                list.RemoveAt(list.Count-1);
            }
            return latest;
        }
    }

    HISTORY             m_history;


    void Start()
    {
        m_winInfo   = (hglWindowInfo)hgca.FindAscendantComponent(gameObject,typeof(hglWindowInfo));
        m_renderInfo= GetComponent<hglHtmlRenderInfo>();
        m_xmlScript = GetComponent<xmlScriptMan>();
        m_history   = new HISTORY();

        var file = hglConfig.GetResourceFrom(m_winInfo.m_ResourceFrom) + m_winInfo.m_url;
        
        Browse(file);
    }
    void CreateRenerObj()
    {
        var renderObj = new GameObject("Renderer");
        renderObj.transform.parent = transform;
        renderObj.transform.localPosition = Vector3.zero;
        m_rendere = renderObj.AddComponent<hglHtmlRender>();
		m_rendere.Init();
        m_rendere.m_render.m_camera = m_winInfo.m_mainCamera.camera;
    }
    public void Browse(string iurl)
    {
        m_history.Push(iurl);
        Browse(iurl,false);
    }
    public void Browse(string iurl,bool viewSrc)
    {
        m_saveUrl = iurl;

        m_winInfo.m_mainCamera.GetComponent<hglSlideControl>().Reset();
        m_renderInfo.m_renderTopLeft = Vector3.zero;

        string url;
        string urlparams;
        Hashtable parameters;
        hglUtil.InterpretURL(iurl,out url,out urlparams,out parameters);
        m_winInfo.m_curUrlParams = urlparams;

        StartCoroutine(HglReadFlow(url,viewSrc));
    }

    public void BrowseAgain()
    {
        Browse(m_saveUrl,false);
    }
    public void BrowseBack()
    {
        var url = m_history.Pop();
        if (string.IsNullOrEmpty(url)) return;
        if (url == m_saveUrl)
        {
            url = m_history.Pop();
            if (string.IsNullOrEmpty(url)) return;
        }
        Browse(url,false);
    }

    public void BrowseSrc()
    {
        Browse(m_saveUrl,true);
    }

    public void ExecuteScript(string url)
    {
        string funcs = url.Substring("javascript:".Length);
        GetComponent<xmlScriptMan>().RunFunctionFromHTML(funcs);
    }

    IEnumerator HglReadFlow(string i_file,bool viewSrc)
    {
        m_winInfo.m_curRootElement = null;
        m_winInfo.m_curHeadElement = null;
        m_winInfo.m_curBodyElement = null;
        
        string file1;

        if (string.IsNullOrEmpty(i_file)) yield break;


        if (   hglEtc.check_head(i_file,"http") )
        {
            file1 = i_file;
        }
        else
        {
            file1 = (new System.IO.FileInfo(i_file)).FullName.Replace('\\', '/');
        }

        {
            var i = file1.LastIndexOf('/');
            m_winInfo.m_curBaseUrl = file1.Substring(0,i+1);
            m_winInfo.m_curUrl = file1;
        }

        string src = m_resman.GetText(file1);
        if (src == null)
        {
            if (m_winInfo.m_useText && m_winInfo.m_curUrlShort == m_winInfo.m_url)
            {
                src = m_winInfo.m_text;
                m_resman.Register(m_winInfo.m_url, file1, m_winInfo.m_text);
            }
            else
            {
                var fd  = m_resman.Register(m_winInfo.m_curUrlShort,file1);
                if (fd.obj==null && fd.isNet) yield return StartCoroutine(m_resman.ReadNet(fd,this));
                src = (string)fd.obj;
            }
        }
        
        hglParseStyleSheet hglStyleSheet = new hglParseStyleSheet();
        
        hglTags_head       tagsHead = new hglTags_head();
        hglWorkHead        hglwork_head = new hglWorkHead();

        hglWorkBody        hglwork_body = new hglWorkBody();

        tagsHead.Init(hglwork_head,hglStyleSheet,m_resman,m_winInfo);

        if (ReadResource1(src)) yield return StartCoroutine(m_resman.ReadAllNet(this));

        hglParser.Element root = null;
        hglParser.Element head = null;
        hglParser.Element body = null;

        if (!viewSrc)
        {
            yield return StartCoroutine(m_xmlScript.Init());
            string src_output = hglParserScript.ParserScript(src, (s) => m_xmlScript.RunProgram(s));
            if (ReadResource1(src_output)) yield return StartCoroutine(m_resman.ReadAllNet(this));

            root = hglParser.Parser(src_output);

            head = hglParser.FindTag("head", root);
            body = hglParser.FindTag("body", root);

            if (head != null) hglParser.TraverseHglWorkHead(head, hglwork_head);
            if (body == null) body = root;
        }
        else
        {
            body = hglParser.CreateRowText(src);
            root = body;
        }
        root.thisStyle.Set(StyleKey.font_size, 20);
        root.thisStyle.SetColor(StyleKey.color, Color.black);
        root.thisStyle.SetColor(StyleKey.background_color, Color.white);
        root.thisStyle.Set(StyleKey.margin,"");
        root.thisStyle.Set(StyleKey.text_align,"");
        root.thisStyle.Set(StyleKey.line_height,2f);
        root.thisStyle.Set(StyleKey.width, m_winInfo.m_fixedWidth);

        if (m_winInfo.m_winType == hglWindowType.MAIN)
        { 
            m_winInfo.m_mainCamera.camera.backgroundColor = root.thisStyle.GetColor(StyleKey.background_color);
        }

        //while (true)
        { 
            hgca.DestroyAllChildren(gameObject);
            yield return null;
            CreateRenerObj();
            yield return null; //create
            yield return null; //exec start

            hglTags_body       tagsBody = new hglTags_body(); 
            tagsBody.Init(hglwork_body,hglStyleSheet,m_winInfo,m_rendere.m_render,m_resman);
            hglParser.TraverseHglWorkBody(body,hglwork_body);
            m_rendere.m_render.OutputRendering(m_rendere.gameObject);
        }

        hglParser.SavePositions(body);

        m_winInfo.m_curRootElement = root;
        m_winInfo.m_curHeadElement = head;
        m_winInfo.m_curBodyElement = body;

        {
            var jsobjs = GetComponentsInChildren<xmlScriptObj>();
            foreach(var jo in jsobjs) jo.m_valid = true;
        }
    }

    bool ReadResource1(string src)
    {
        var tmp   = hglParser.Parser(src);
        var files = hglHtmlWork.GetAllFilenames(tmp);
        foreach (var f in files)
        {
            m_resman.Register(f,m_winInfo.CreateFullPath(f) );
        }
        return m_resman.CheckHasNetFiles();
    }


    //IEnumerator ReadResource(string src)
    //{
    //    var tmp   = hglParser.Parser(src);
    //    var files = hglHtmlWork.GetAllFilenames(tmp);
    //    foreach (var f in files)
    //    {
    //        m_resman.Register(f,m_winInfo.CreateFullPath(f) );
    //    }

    //    yield return StartCoroutine(m_resman.ReadAll(this));
    //}
}
