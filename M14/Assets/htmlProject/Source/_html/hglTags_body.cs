using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using xmlDisplayItem2;
using System;
using hug_u;

namespace hug_u {
public class hglTags_body {

    public float      m_font_default_size  = 16;

    hglParseStyleSheet m_styleSheet;
    hglWorkBody        m_hglWork;
    hglForm            m_hglForm;  // <form> <input xxx> </form> 

    public hglRender          m_htmlRender;
    public hglWindowInfo      m_info;
    public hglResourceMan     m_resman;
    
    public void Init(hglWorkBody xw, hglParseStyleSheet styleSheet, hglWindowInfo winInfo, hglRender hren, hglResourceMan resman)
    {
        m_hglWork    = xw;
        m_styleSheet = styleSheet;
        m_htmlRender = hren;
        m_info       = winInfo;
        m_resman     = resman;
        m_hglForm    = null;

        m_htmlRender.CreateRootBlock(winInfo.m_fixedWidth,float.MaxValue);

        RegisterAll();
    }
    void RegisterAll()
    {
        m_hglWork.Init(this);
        m_hglWork.Register(typeof(html_a));
        m_hglWork.Register(typeof(html_b));
        m_hglWork.Register(typeof(html_body));
        m_hglWork.Register(typeof(html_br));
        m_hglWork.Register(typeof(html_canvas));
        m_hglWork.Register(typeof(html_div));
        m_hglWork.Register(typeof(html_font));
        m_hglWork.Register(typeof(html_form));
        m_hglWork.Register(typeof(html_h1));
        m_hglWork.Register(typeof(html_h2));
        m_hglWork.Register(typeof(html_h3));
        m_hglWork.Register(typeof(html_h4));
        m_hglWork.Register(typeof(html_h5));
        m_hglWork.Register(typeof(html_h6));
        m_hglWork.Register(typeof(html_hr));
        m_hglWork.Register(typeof(html_i));
        m_hglWork.Register(typeof(html_img));
        m_hglWork.Register(typeof(html_input));
        m_hglWork.Register(typeof(html_li));
        m_hglWork.Register(typeof(html_nobr));
        m_hglWork.Register(typeof(html_p));
        m_hglWork.Register(typeof(html_pre));
        m_hglWork.Register(typeof(html_s));
        m_hglWork.Register(typeof(html_small));
        m_hglWork.Register(typeof(html_span));
        m_hglWork.Register(typeof(html_table));
        m_hglWork.Register(typeof(html_td));
        m_hglWork.Register(typeof(html_th));
        m_hglWork.Register(typeof(html_tr));
        m_hglWork.Register(typeof(html_u));
        m_hglWork.Register(typeof(html_ul));
    }

    // # A
    public class html_a : hglBaseBody
    {
        public hglAnchor.MODE m_mode;

        public override void ElementWork(hglParser.Element te)
        {
            string url=null;
            m_mode = hglAnchor.MODE.JUMP;

            var atrs = te.attrib;
            if (atrs["href"] != null)
            { 
                if (ainfo==null) ainfo = new hglAnchor.Info();         
                ainfo = new hglAnchor.Info();
                ainfo.m_url = (string)atrs["href"];
            }
            //if (atrs["onclick"] != null)   // @@ onclick has been processed at hglBaseBody @@
            //{
            //    url = "javascript:" + (string)atrs["onclick"] ;
            //}
            if (atrs["a"] != null)
            {
                m_mode = hglAnchor.MODE.ANCHOR;
                url = (string)atrs["a"];
            }

            if (ainfo!=null)
            {
                //Color
                ainfo.m_hover = ( hglTags.m_styleSheet.GetLinkStyleColor("a:hover",     out ainfo.m_hover)  ||  hglTags.m_styleSheet.GetLinkStyleColor(":hover"  ,out ainfo.m_hover))     ? ainfo.m_hover   : Color.green;
                ainfo.m_link   = ( hglTags.m_styleSheet.GetLinkStyleColor("a:link"  ,   out ainfo.m_link)   ||  hglTags.m_styleSheet.GetLinkStyleColor(":link"   ,out ainfo.m_link))      ? ainfo.m_link    : Color.blue;
                ainfo.m_visited= ( hglTags.m_styleSheet.GetLinkStyleColor("a:visited",  out ainfo.m_visited)||  hglTags.m_styleSheet.GetLinkStyleColor(":visited",out ainfo.m_visited))   ? ainfo.m_visited : Color.gray;
                ainfo.m_pressed= ( hglTags.m_styleSheet.GetLinkStyleColor("a:pressed",  out ainfo.m_pressed)||  hglTags.m_styleSheet.GetLinkStyleColor(":pressed",out ainfo.m_pressed))   ? ainfo.m_pressed : Color.red;
            }
        }

        public override void TextWork(hglParser.Element te)
        {
            if (!te.thisStyle.HasKey(StyleKey.text_decoration))
            {
                te.thisStyle.Set(StyleKey.text_decoration,"underline");
            }
            
            base.TextWork(te);
        }

        public override bool IsInline()
        {
            return true;
        }
    }
    // # B
    public class html_b    : hglBaseBody
    {
        public override void ElementWork(hglParser.Element te)
        {
            te.thisStyle.Set(StyleKey.font_weight,"bold");
        }
    }
    public class html_body : hglBaseBody
    {
        public override void ElementWork(hglParser.Element te)
        {
            hglTags.m_htmlRender.CreateChildBlock(te,true);
        }

        public override void EndElementWork(hglParser.Element te)
        {
            hglTags.m_htmlRender.MoveToParent();
        }
    }
    public class html_br : hglBaseBody
    {
        public override void ElementWork(hglParser.Element te)
        {
            hglTags.m_htmlRender.WriteCR(te);
        }
        public override bool IsInline() {  return true; }
    }
    // # C
    public class html_canvas : hglBaseBody
    {
        //http://www.htmq.com/html5/canvas.shtml
        public override void ElementWork(hglParser.Element te)
        {
            var atrs = te.attrib;
        
            string width  = atrs["width"] != null ? (string)atrs["width"] : "300";
            string height = atrs["height"]!= null ? (string)atrs["height"]: "150"; 
            float w,h;
            if (float.TryParse(width, out w))
            { 
                te.thisStyle.Set(StyleKey.width,width);
            }
            if (float.TryParse(height, out h))
            { 
                te.thisStyle.Set(StyleKey.height,height);
            }

            hglTags.m_htmlRender.CreateChildBlock(te,true);

            hglTags.m_htmlRender.WriteText(te," ", hglRender.TextMode.RAW); // dummy
        }
        public override void EndElementWork(hglParser.Element te)
        {
            hglTags.m_htmlRender.MoveToParent();
        }
    }
    // # D
    public class html_div : hglBaseBody { 
        public override void  ElementWork(hglParser.Element te)
        {
            hglTags.m_htmlRender.CreateChildBlock(te,true);
        }
        public override void EndElementWork(hglParser.Element te)
        {
            hglTags.m_htmlRender.MoveToParent();
        }
    }

    // # F
    public class html_footer : hglBaseBody {}
    public class html_font : hglBaseBody   
    {
        public override void ElementWork(hglParser.Element te)
        {
            var color = te.thisStyle.GetColor(StyleKey.color);
            color = hglEtc.GetColor("color",te.attrib,color);
            te.thisStyle.SetColor(StyleKey.color,color);
            var size  = te.thisStyle.GetFloat(StyleKey.font_size,float.NaN);
            size = hglEtc.GetFloat("size",te.attrib,size);
            te.thisStyle.Set(StyleKey.font_size,size);
        }
    }
    public class html_form : hglBaseBody
    {
        public override void ElementWork(hglParser.Element te)
        {
            var atrs = te.attrib;
            hglTags.m_hglForm = new hglForm();
            hglTags.m_hglForm.Init(atrs,hglTags);
        }
        public override void EndElementWork(hglParser.Element te)
        {
            hglTags.m_hglForm = null;
        }
    }    

    // # H
    public class html_h1 : hglBaseBody  {
        public override void  ElementWork(hglParser.Element te)
        {
            hglTags.m_htmlRender.CreateChildBlock(te,true);
        }
        public override void EndElementWork(hglParser.Element te)
        {
            hglTags.m_htmlRender.MoveToParent();
        }        
    }
    public class html_h2 : html_h1 {}
    public class html_h3 : html_h1 {}
    public class html_h4 : html_h1 {}
    public class html_h5 : html_h1 {}
    public class html_h6 : html_h1 {}
    public class html_hr : hglBaseBody
    { 
        public override void ElementWork(hglParser.Element te)
        {
            hglTags.m_htmlRender.DrawHR(te);
        }
        public override bool IsInline() {  return true; }    
    }

    // # I
    public class html_i   : hglBaseBody
    {
        public override void ElementWork(hglParser.Element te)
        {
            te.thisStyle.Set(StyleKey.font_style,"italic");
        }
    }
    public class html_img : hglBaseBody
    {
        public override void ElementWork(hglParser.Element te)
        {
            var atrs = te.attrib;
            var dt = "<img : "; 
            if (atrs!=null&&atrs.Keys.Count!=0) foreach(var k in atrs.Keys) dt+="["+(string)k+"]="+(string)atrs[k]+";"; 
//            Debug.Log(dt);
            
            hglTags.m_htmlRender.DrawImage(te);
            //xmlTags.CreateImageItem(atrs);
        }
    }
    public class html_input : hglBaseBody
    {
        public override void ElementWork(hglParser.Element te)
        {
            var atrs = te.attrib;
            if (hglTags.m_hglForm!=null) 
            {
                hglTags.m_hglForm.CreateInputUI(atrs);
            }
        }
    }
    // # L
    public class html_li : hglBaseBody
    {
        public override void  ElementWork(hglParser.Element te)
        {
            hglTags.m_htmlRender.WriteText(te,"E", hglRender.TextMode.RAW);
        }
        public override void EndElementWork(hglParser.Element te)
        {
            hglTags.m_htmlRender.WriteCR(te);
        }
    }
    // # N
    public class html_nobr : hglBaseBody { }

    // # P
    public class html_p : hglBaseBody
    {
        public override void  ElementWork(hglParser.Element te)
        {
            hglTags.m_htmlRender.CreateChildBlock(te,true);
        }
        public override void EndElementWork(hglParser.Element te)
        {
            hglTags.m_htmlRender.MoveToParent();
        }
    }
    public class html_pre : hglBaseBody
    {
        public override void ElementWork (hglParser.Element te)
        {
            hglTags.m_htmlRender.CreateChildBlock(te,true);
        }
        
        public override void TextWork(hglParser.Element te)
        {
            hglTags.m_htmlRender.WriteText(te,te.text, hglRender.TextMode.RAW);
        }

        public override void EndElementWork(hglParser.Element te)
        {
            hglTags.m_htmlRender.MoveToParent();
        }
    }
    
    // # S
    public class html_s : hglBaseBody
    {
        public override void TextWork(hglParser.Element te)
        {
            te.thisStyle.Set(StyleKey.text_decoration,"line-through");
            base.TextWork(te);
        }
    }

    public class html_select : hglBaseBody
    {
        public override void ElementWork(hglParser.Element te)
        {
            var atrs = te.attrib;
            if (hglTags.m_hglForm!=null) 
            {
                //var u = xmlTags.m_xmlForm.CreateSelectUI(atrs);
                //xmlTags.AddChild((string)atrs["id"],u.gameObject);
            }
        }
    }
    public class html_small : hglBaseBody
    {
        public override bool IsInline()
        {
            return true;
        }
    }

    public class html_spacer : hglBaseBody
    {
        public override void ElementWork(hglParser.Element te)
        {
            var atrs = te.attrib;
            int intnum = 1;
            string num = (string)atrs["num"];
            if (num != null && num.Length > 0)
            {
                try
                {
                    intnum = int.Parse(num);
                }
                catch
                {
                }
            }
            //xmlTags.WriteSpace(intnum);
        }
    }
    public class html_span : hglBaseBody
    {
    }
    public class html_style : hglBaseBody
    {

    }
    // # T
    public class html_table : hglBaseBody
    {
        public override void ElementWork(hglParser.Element te)
        {
            var atrs = te.attrib;
            ALIGN align = ALIGN.NONE;
            if ((string)atrs["align"]=="left")   align = ALIGN.LEFT;
            if ((string)atrs["align"]=="center") align = ALIGN.CENTER;
            if ((string)atrs["align"]=="right")  align = ALIGN.RIGHT;
            if (align == ALIGN.NONE)
            {
                var margin = te.thisStyle.GetMargin();
                if (margin[1]==-(float)0x0a && margin[3]==-(float)0x0a) align = ALIGN.CENTER;
            }

            float cellpadding = hglEtc.GetFloat("cellpadding",atrs,0f);
            float hspace      = hglEtc.GetFloat("hspace",atrs,0f);
            float vspace      = hglEtc.GetFloat("vspace",atrs,0f);
            float width       = hglEtc.GetFloat("width",atrs,0f);
            float height      = hglEtc.GetFloat("height",atrs,0f);
            float border      = hglEtc.GetFloat("border",atrs,1f);
            Color bordercolor = hglEtc.GetColor("bordercolor",atrs,te.thisStyle.GetColor(StyleKey.color));
            te.thisStyle.Set(StyleKey.border_width,border);
            te.thisStyle.SetColor(StyleKey.border_color,bordercolor);

            Color? bgColor    = hglEtc.GetColorIfErrorNull("bgcolor",atrs);
            if (bgColor!=null) te.thisStyle.SetColor(StyleKey.background_color,(Color)bgColor);

            hglTags.m_htmlRender.CreateTableBlock(te,align,cellpadding,hspace,vspace,width,height);
        }
        public override void EndElementWork(hglParser.Element te) {
            hglTags.m_htmlRender.MoveToParent();
        }
    }
    public class html_td : hglBaseBody {
        public override void ElementWork(hglParser.Element te)
        {
            var atrs = te.attrib;
            ALIGN align = ALIGN.LEFT;
            if ((string)atrs["align"]=="center") align = ALIGN.CENTER;
            if ((string)atrs["align"]=="right")  align = ALIGN.RIGHT;
            int colspan = (int)hglEtc.GetFloat("colspan",atrs,0);
            int rowspan = (int)hglEtc.GetFloat("rowspan",atrs,0);

            hglTags.m_htmlRender.CreateTableData(te,align,colspan,rowspan);
        }
        public override void EndElementWork(hglParser.Element te) {
            hglTags.m_htmlRender.MoveToParent();
        }
    }
    public class html_th : html_td     { }
    public class html_tr : hglBaseBody
    {
        public override void ElementWork(hglParser.Element te)
        {
            var atrs = te.attrib;
            ALIGN align = ALIGN.LEFT;
            if ((string)atrs["align"]=="center") align = ALIGN.CENTER;
            if ((string)atrs["align"]=="right")  align = ALIGN.RIGHT;
            float height = hglEtc.GetFloat("height",atrs,0f);

            hglTags.m_htmlRender.CreateTableRow(te,align,height);
        }
        public override void EndElementWork(hglParser.Element te) {
            hglTags.m_htmlRender.MoveToParent();
        }
    }

    // # U
    public class html_u : hglBaseBody {
        public override void TextWork(hglParser.Element te)   //Because underline is not inherit.
        {
            te.thisStyle.Set(StyleKey.text_decoration,"underline");  
            base.TextWork(te);
        }
    }
    public class html_ul : hglBaseBody {
    }

    // #########
    // # Style #  // Style Priortiy : http://www.stylish-style.com/csstec/base/order.html  http://www.res-system.com/weblog/item/565
    public void StyleElementWork(hglBaseBody xb)
    {
        var xe   = xb.hglElement;
        var atrs = xe.attrib;
        hglStyle elementStyle = new hglStyle(xe);

        {
            elementStyle.AddParse(m_styleSheet.GetStyle("*"));
        }

        {
            var classname = (string)atrs["class"];
            elementStyle.AddParse(m_styleSheet.GetStyle_class_tag(classname,xe));
        }

        if (atrs == null) return;

        if (atrs.ContainsKey("style"))
        {
            elementStyle.AddParse((string)atrs["style"]);
        }

        xe.thisStyle = elementStyle;
    }
    // # Style #
    // #########

#if OBSOLATED
    //public void WriteText(displayTextItem itextItem, xmlBaseBody tag, xmlParser.Element te )
    //{

        displayTextItem ti = itextItem;
        var text = xmlEtc.NormalizeText(te.text);
        if (string.IsNullOrEmpty(text)) return ti;
        text = xmlEtc.decodeTextToDisplay(text);
        var style = ((xmlBaseBody)te.parent.baseBody).thisStyle;

        if (ti!=null)
        {
            ti.text += text;
        }
        else
        {
            ti = new displayTextItem();
            ti.fontsize = tag.thisStyle.GetFloat(StyleKey.font_size);
            ti.text     = text;
            ti.size     = _size.Create( style.GetFloat(StyleKey.width),0f );
            //ti.uiFont   = m_uiFont;
            var align = tag.thisStyle.Get(StyleKey.text_align);
            //ti.CreateWrapedText();
            switch(align)
            {
            case "center": ti.label_pivot = UIWidget.Pivot.Center; break;
            case "right" : ti.label_pivot = UIWidget.Pivot.Right;  break;
            default:       ti.label_pivot = UIWidget.Pivot.Left;   break;
            }

            te.displayItem = ti;
        }
        return ti;
    //}
    public displayTextItem WriteCR(displayTextItem itextItem, xmlBaseBody tag, xmlParser.Element te)
    {
        displayTextItem ti = itextItem;
        var style = ((xmlBaseBody)te.parent.baseBody).thisStyle;

        if (ti!=null)
        {
            ti.text += "\n";
        }
        else
        {
            var dispblank = new displayBlank();
            dispblank.size = _size.Create(style.GetFloat(StyleKey.width),style.GetFloat(StyleKey.font_size));
            te.displayItem = dispblank;
        }
        return ti;
    }
#endif

}
}