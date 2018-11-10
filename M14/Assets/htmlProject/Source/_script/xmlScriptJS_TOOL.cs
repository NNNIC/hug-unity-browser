using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using hug_u;

namespace xmlScriptJS {

    public class _TOOL
    {
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


        xmlScriptObj m_scrObj;
        public _TOOL(xmlScriptObj obj) { m_scrObj = obj; }
        public void LOCKCAMERA(object o)
        { 
            if (o.ToString() == "0")
            {
                m_winInfo.m_mainCamera.GetComponent<hglSlideControl>().m_lock = false;
            }
            else
            {
                m_winInfo.m_mainCamera.GetComponent<hglSlideControl>().m_lock = true;
            }
        }
        public void STARTPOS(object o)
        {
            if (o==null) throw new SystemException("SETSTARTPOS ARGS IS LEFT,RIGHT,TOP or BOT");

            var width  = m_winInfo.m_fixedWidth;
            var height = m_winInfo.m_height;

            Vector3 v = Vector3.zero;
            switch(o.ToString())
            {
            case "TOP"  : v = height * Vector3.up;   break;
            case "BOT"  : v = -height * Vector3.up;  break;
            case "RIGHT": v = width * Vector3.right; break;
            case "LEFT" : v = -width* Vector3.right; break;
            }

            m_renderInfo.m_renderTopLeft = v;
        }
        // ###################
        // # SEQUENCE ACTION #
        public void SEQACT(object o,object o2)
        {
            if (o==null||o.GetType()!=typeof(xmlScriptJS.Array)) throw new SystemException("ERROR STARTACT ARG IS ARRAY OF STRINGS");
            var array = (xmlScriptJS.Array)o;
            string[] list = new string[array.Lenght];
            for(var i=0;i<array.Lenght;i++)
            {
                var o1 = array.Get(i);
                list[i] = o1.ToString();
            }
			
            string func = (o2!=null) ? o2.ToString() : null;

            m_scrObj.StartCoroutine(SEQACT_1(list,func));
            //m_scrObj.StartCoroutine(ALLMV());
        }

        IEnumerator SEQACT_1(string[] list,string func)
        {
            foreach (var i in list)
            {
                var words = i.Split(',');
                switch (words[0])
                {
                case "WAIT"   : yield return m_scrObj.StartCoroutine(WAIT(words));  break;
                case "PLACE"  : yield return m_scrObj.StartCoroutine(PLACE(words)); break;
                case "APART"  : APART(words); break;
                case "SLIDE"  : yield return m_scrObj.StartCoroutine(SLIDE(words)); break;
                case "SLIDEBACK": yield return m_scrObj.StartCoroutine(SLIDEBACK(words)); break;
                case "FX_SZ"  : FX_SZ(words);  break;
                case "FX_ROT" : FX_ROT(words); break;
                case "FX_COL" : FX_COL(words); break;
                case "FX_ALFA": FX_ALFA(words);break;
                case "WHITE"  : yield return m_scrObj.StartCoroutine(WHITE(words)); break;
                }
            }

            m_scrObj.m_scriptMan.RunFunction(func,null,m_scrObj);

        }

        IEnumerator WAIT(string[] args)
        {
            float time = float.Parse(args[1]);
            yield return new WaitForSeconds(time);            
        }
        IEnumerator PLACE(string[] args)
        {
            float time = float.Parse(args[1]);

            //yield return null;
            var htmlrender = m_winInfo.GetComponentInChildren<hglHtmlRender>();
            Vector3 pos = Vector3.zero;
            while(pos!=m_renderInfo.m_renderTopLeft)
            {                    
                pos = htmlrender.transform.localPosition;
                Debug.Log(pos);
                yield return null;
            }

            yield return m_scrObj.StartCoroutine(hgMove.linear_Vector3(pos,Vector3.zero,time,(v)=>{ 
                htmlrender.transform.localPosition=v;            
            }));
        }

        void APART(string[] args)
        {
            var width  = m_winInfo.m_fixedWidth;
            var height = m_winInfo.m_height;

            Vector3 v = Vector3.zero;
            switch(args[1].ToString())
            {
            case "TOP"  : v = height * Vector3.up;   break;
            case "BOT"  : v = -height * Vector3.up;  break;
            case "RIGHT": v = width * Vector3.right; break;
            case "LEFT" : v = -width* Vector3.right; break;
            }

            hglParser.Traverse(m_winInfo.m_curBodyElement,(e)=>{
                if (e.bone1) e.bone1.transform.localPosition+=v;
                if (e.bone2) e.bone2.transform.localPosition+=v;
            });
        }

/*       
        IEnumerator SLIDEBACK(string[] args)
        {
            float time = float.Parse(args[1]);
            float amp  = float.Parse(args[2]);
            yield return m_scrObj.StartCoroutine(hglParser.TraverseAsync_w_depth(m_webInfo.m_curBodyElement,0,(e,d)=>{
                if (e.bone1!=null) m_scrObj.StartCoroutine(hgMove.linear_Vector3(e.bone1.transform.localPosition,e.bone1_savepos,time+ (float)d*amp*time, (v)=>{e.bone1.transform.localPosition = v;} ));
                if (e.bone2!=null) m_scrObj.StartCoroutine(hgMove.linear_Vector3(e.bone2.transform.localPosition,e.bone2_savepos,time+ (float)d*amp*time, (v)=>{e.bone2.transform.localPosition = v;} ));
            },m_scrObj));
        }
        */
#if XX
        IEnumerator SLIDEBACK(string[] args)
        {
            float time   = float.Parse(args[1]);
            float delay  = float.Parse(args[2]);

            var root = m_webInfo.m_curBodyElement;
            float max_time = 0;
            hglParser.Traverse_w_index_count_depth(root,-1,0,0,(e,i,c,d)=>{
                float interval = 0;
                float f = delay;
                interval = d * f  + (c>1 ? (float)i/(float)(c-1) * f : 0);
                if (interval <0 ) interval = 0;
                Vector3[] vs = new Vector3[2];
                if (e.bone1!=null) vs[0] = e.bone1_savepos;
                if (e.bone2!=null) vs[1] = e.bone2_savepos;
                e.tmp1 = vs;
                e.tmp2 = interval;
                e.tmp3 = time;
                e.tmp4 = d;
                max_time = Mathf.Max(max_time,interval+time);
            });

            hglParser.Traverse(root,(e)=>{
                m_scrObj.StartCoroutine(SLIDE_SUB(e,0)); 
            });
            yield return new WaitForSeconds(max_time);


            //yield return m_scrObj.StartCoroutine(hglParser.TraverseAsync_w_depth(m_webInfo.m_curBodyElement,0,(e,d)=>{
            //    if (e.bone1!=null) m_scrObj.StartCoroutine(hgMove.linear_Vector3(e.bone1.transform.localPosition,e.bone1_savepos,time+ (float)d*amp*time, (v)=>{e.bone1.transform.localPosition = v;} ));
            //    if (e.bone2!=null) m_scrObj.StartCoroutine(hgMove.linear_Vector3(e.bone2.transform.localPosition,e.bone2_savepos,time+ (float)d*amp*time, (v)=>{e.bone2.transform.localPosition = v;} ));
            //},m_scrObj));
        }
#endif
        IEnumerator SLIDEBACK(string[] args)
        {
            //float time = 0.000f;//float.Parse(args[1]);
            //float amp  = 0f;// float.Parse(args[2]);
            //m_scrObj.StartCoroutine(hglParser.TraverseAsync_w_depth(m_webInfo.m_curBodyElement,0,(e,d)=>{
            //    if (e.bone1!=null) m_scrObj.StartCoroutine(hgMove.linear_Vector3(e.bone1.transform.localPosition,e.bone1_savepos,time+ (float)d*amp, (v)=>{e.bone1.transform.localPosition = v;} ));
            //    if (e.bone2!=null) m_scrObj.StartCoroutine(hgMove.linear_Vector3(e.bone2.transform.localPosition,e.bone2_savepos,time+ (float)d*amp, (v)=>{e.bone2.transform.localPosition = v;} ));
            //},m_scrObj));

            float time   = float.Parse(args[1]);
            float delay  = float.Parse(args[2]);
            var root = m_winInfo.m_curBodyElement;
            float max_time = 0;

            hglParser.Traverse_w_index_count_depth(root,-1,0,0,(e,i,c,d)=>{
                float interval = 0;
                float f = delay;
                interval = (d -2) * f  + (c>1 ? (float)i/(float)(c-1) * f : 0);
                if (interval <0 ) interval = 0;
                //Vector3[] vs = new Vector3[2];
                //if (e.bone1!=null) vs[0] = e.bone1_savepos;
                //if (e.bone2!=null) vs[1] = e.bone2_savepos;
                //e.tmp1 = vs;
                  e.tmp2 = interval;
//                e.tmp3 = time;
//                e.tmp4 = d;
                max_time = Mathf.Max(max_time,interval+time);
            });

            
            hglParser.Traverse(root,(e)=>{
                m_scrObj.StartCoroutine(SLIDEBACK_SUB(e,time));
            });
            yield return null;
        }
        IEnumerator SLIDEBACK_SUB(hglParser.Element e,float time)
        {
            float interval =  (e.tmp2!=null) ? (float)e.tmp2 : 0;
            if (interval>0) yield return new WaitForSeconds(interval);
            if (e.bone1!=null) m_scrObj.StartCoroutine(hgMove.linear_Vector3(e.bone1.transform.localPosition,e.bone1_savepos,0.1f+ (float)0.1f, (v)=>{e.bone1.transform.localPosition = v;} ));
            if (e.bone2!=null) m_scrObj.StartCoroutine(hgMove.linear_Vector3(e.bone2.transform.localPosition,e.bone2_savepos,0.1f+ (float)0.1f, (v)=>{e.bone2.transform.localPosition = v;} ));
        }

        IEnumerator SLIDE(string[] args)
        {
            float time     = hglEtc.GetFloat(2,args,0);
            float delay    = hglEtc.GetFloat(3,args,0);
            int validDepth = (int)hglEtc.GetFloat(4,args,0);

            var width  = m_winInfo.m_fixedWidth;
            var height = m_winInfo.m_height;

            Vector3 v = Vector3.zero;
            switch(args[1].ToString())
            {
            case "TOP"  : v = height * Vector3.up;   break;
            case "BOT"  : v = -height * Vector3.up;  break;
            case "RIGHT": v = width * Vector3.right; break;
            case "LEFT" : v = -width* Vector3.right; break;
            }

            var root = m_winInfo.m_curBodyElement;

            hglParser.Traverse(root,(e)=>{
                Vector3[] vs = new Vector3[2];
                if (e.bone1) vs[0] = e.bone1.transform.localPosition+v;
                if (e.bone2) vs[1] = e.bone2.transform.localPosition+v;
                e.tmp1 = vs;
            });

            int max_depth = hglParser.GetMaxDepth(m_winInfo.m_curBodyElement);
            float max_time  = 0;

            hglParser.Traverse_w_index_count_depth(root,-1,0,0,(e,i,c,d)=>{
                float interval = 0;
                float f = delay;
                interval = (max_depth - d) * f  + (c>1 ? (float)i/(float)(c-1) * f : 0);
                if (interval <0 ) interval = 0;
                e.tmp2 = interval;
                e.tmp3 = time;
                e.tmp4 = d;
                max_time = Mathf.Max(max_time,interval+time);
            });

            hglParser.Traverse(root,(e)=>{
                m_scrObj.StartCoroutine(SLIDE_SUB(e,validDepth));
            });
            yield return new WaitForSeconds(max_time);
        }

        IEnumerator SLIDE_SUB(hglParser.Element e, int validDepth)
        {
            if (e==null || e.tmp1 == null || e.tmp2 == null || e.tmp3==null  ) yield break;
            float interval = (float)e.tmp2;
            float time     = (float)e.tmp3;
            int   depth    = (int)e.tmp4;
            if (validDepth>0 && depth > validDepth) yield break;

            yield return new WaitForSeconds(interval);
            Vector3[] vs = (Vector3[])e.tmp1;
           
            if (e.bone1!=null) m_scrObj.StartCoroutine(hgMove.linear_Vector3(e.bone1.transform.localPosition,vs[0],time, (vx)=>{e.bone1.transform.localPosition = vx;} ));
            if (e.bone2!=null) m_scrObj.StartCoroutine(hgMove.linear_Vector3(e.bone2.transform.localPosition,vs[1],time, (vx)=>{e.bone2.transform.localPosition = vx;} ));
        }


        void FX_SZ(string[] args)
        {
            var tag = args[1];
            float speed = float.Parse(args[2]);
            float dh    = float.Parse(args[3]); 

            var alllist = hglParser.FindTags(tag,m_winInfo.m_curBodyElement);
            foreach (var tg in alllist)
            { 
                m_scrObj.StartCoroutine( FX_SZ_sub(tg,speed,dh) );
            }
        }
        IEnumerator FX_SZ_sub(hglParser.Element e,float speed,float maxscale)
        {
            if (e.bone1 != null)
            {
                float d = 0;
                while (true)
                {
                    d+=speed * Time.deltaTime;
                    yield return null;
                    e.bone1.transform.localScale = (1 + maxscale * Mathf.Sin( d * Mathf.Deg2Rad )) *  Vector3.one;
                }
            }
        }
        void FX_ROT(string[] args)
        {
            var tag = args[1];
            var pivot = args[2].ToLower();
            float speed = float.Parse(args[3]);
            
            var alllist = hglParser.FindTags(tag,m_winInfo.m_curBodyElement);
            foreach (var tg in alllist)
            { 
                m_scrObj.StartCoroutine( FX_ROT_sub(tg,pivot,speed ) );
            }
        }
        IEnumerator FX_ROT_sub(hglParser.Element e, string pivot, float speed)
        {
            Vector3 pv = Vector3.up;
            switch(pivot)
            {
            case "x" : pv = Vector3.right;   break;
            case "y" : pv = Vector3.up;      break;
            case "z" : pv = Vector3.forward; break;
            }

            if (e.bone1 != null)
            { 
                while (true)
                {
                    var v = speed * Time.deltaTime;
                    yield return null;
                    e.bone1.transform.RotateAroundLocal(pv,v);
                }
            }
        }
        void FX_COL(string[] args)
        {
            var tag       = args[1];
            var mode      = args[2].ToLower();  //  main (""), back, frame, effect 
            var dstcolstr = args[3];
            var speed     = float.Parse(args[4]);

            switch(mode)
            {
            case "back": 
            case "frame":
            case "effect": break;
            default: mode = "main"; break;
            }

            Color dstCol;
            hglConverter.GetColorString(dstcolstr,out dstCol);

            var alllist = hglParser.FindTags(tag,m_winInfo.m_curBodyElement);
            foreach (var tg in alllist)
            { 
                m_scrObj.StartCoroutine( FX_COL_sub(tg,mode,dstCol,speed ) );
            }
            
        }
        IEnumerator FX_COL_sub(hglParser.Element e,string mode,Color dst, float speed)
        {
            int index = -1;
            switch(mode)
            {
            case "main":  index =e.colorIndex;      break;
            case "back":  index =e.backcolorIndex;  break;
            case "frame": index =e.frameColorIndex; break; 
            case "effect":index =e.effectColorIndex;break;
            }
            if (index < 0) yield break;

            hglHtmlColor htmlColor = null;
            {
                var render = m_renderInfo.GetRender();
                htmlColor = render.m_render.m_htmlColor;
            }

            Vector3 startColor = hglEtc.toVector3(htmlColor.GetColor(index));
            Vector3 endColor   = hglEtc.toVector3(dst);

            float i = 0;
            while (true)
            {
                yield return null;

                i += speed * Time.deltaTime;
                float h = 0.5f + 0.5f * Mathf.Sin( i * Mathf.Deg2Rad );
                var cur = Vector3.Lerp(startColor,endColor,h);
                htmlColor.SetColor(index,hglEtc.toColor(cur));
            }
        }
        void FX_ALFA(string[] args)
        {
            var tag       = args[1];
            var mode      = args[2].ToLower();  //  main (""), back, frame, effect 
            var speed     = float.Parse(args[3]);

            switch(mode)
            {
            case "back": 
            case "frame":
            case "effect": break;
            default: mode = "main"; break;
            }

            var alllist = hglParser.FindTags(tag,m_winInfo.m_curBodyElement);
            foreach (var tg in alllist)
            { 
                m_scrObj.StartCoroutine( FX_ALFA_sub(tg,mode,speed ) );
            }
            
        }
        IEnumerator FX_ALFA_sub(hglParser.Element e,string mode, float speed)
        {
            if (e.mode == hglParser.Mode.TAG && e.text == "img" && e.bone1.renderer!=null)
            {
                var mat = e.bone1.renderer.material;
                float i = 0;
                while (true)
                {
                    yield return null;
                    i += speed * Time.deltaTime;
                    float h = 0.5f + 0.5f * Mathf.Cos( i * Mathf.Deg2Rad );
                    mat.SetFloat("_Alpha1",h);
                }
            }
            else if (e.mode == hglParser.Mode.TAG && e.text == "body")
            {
                var mat = m_renderInfo.GetRender().renderer.material;
                float i = 0;
                while (true)
                {
                    yield return null;
                    i += speed * Time.deltaTime;
                    float h = 0.5f + 0.5f * Mathf.Cos( i * Mathf.Deg2Rad );
                    mat.SetFloat("_Alpha",h);
                }
            }
            else
            { 
                int index = -1;
                switch(mode)
                {
                case "main":  index =e.colorIndex;      break;
                case "back":  index =e.backcolorIndex;  break;
                case "frame": index =e.frameColorIndex; break; 
                case "effect":index =e.effectColorIndex;break;
                }
                if (index < 0) yield break;

                hglHtmlColor htmlColor = null;
                {
                    var render = m_renderInfo.GetRender();
                    htmlColor = render.m_render.m_htmlColor;
                }

                var col = htmlColor.GetColor(index);
                float i = 0;
                while (true)
                {
                    yield return null;

                    i += speed * Time.deltaTime;
                    float h = 0.5f + 0.5f * Mathf.Cos( i * Mathf.Deg2Rad );
                    col.a = h;
                    htmlColor.SetColor(index,col);
                }
            }
        }

        IEnumerator WHITE(string[] args)
        {
            var tag = args[1];
            float time = hglEtc.GetFloat(2,args,0.1f);
            var alllist = hglParser.FindTags(tag,m_winInfo.m_curBodyElement);


            float s = 0.0f;
            float speed = 1/time;

            while (s < 1f)
            {
                float pos   = Mathf.Lerp(0,1,s);
                foreach (var tg in alllist)
                {
                    tg.bone1.renderer.material.SetFloat("_White",pos);
                }
                yield return null;
                s += Time.deltaTime*speed;
            }
            foreach (var tg in alllist)
            {
                tg.bone1.renderer.material.SetFloat("_White",1);
            }
        }

        // # SEQUENCE ACTION #
        // ###################


        
    }

}
