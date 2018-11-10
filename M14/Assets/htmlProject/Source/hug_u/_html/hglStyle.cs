using UnityEngine;
using System.Collections;
using hug_u;

namespace hug_u {
public enum StyleKey
{
    NONE,

    //Color
    color,

    //Margin
    margin,
    margin_top,
    margin_right,
    margin_bottom,
    margin_left,

    //Padding
    padding,
    padding_top,
    padding_right,
    padding_bottom,
    padding_left,

    //Border
    border,
    border_width,
    border_style,
    border_color,

    //Size
    max_width,
    width,
    height,

    //Background
    background,
    background_color,
    background_image,
    background_atlas,  // original

    //font
    font_size,
    font_style,
    font_weight,
    line_height,

    //text
    text_align,
    text_decoration,
    text_shadow,
    text_outline,  // original    ex) text-outline : width qualiry color;          quality 0 or 1

    //character space
    letter_spacing,

    //Unity:original
    //update_z_axis,
    _valign,

}


public class hglStyle {
    
    //Ref. http://www.tohoho-web.com/css/index.htm

    StyleKey[] m_inherit = {
                           
    //StyleKey.NONE,
    
    //Color
    StyleKey.color,
    
    //Margin
    //StyleKey.margin,
    //StyleKey.margin_top,
    //StyleKey.margin_right,
    //StyleKey.margin_bottom,
    //StyleKey.margin_left,
    
    //Size
    //StyleKey.max_width,
    //StyleKey.width,
    //StyleKey.height,
    
    //Background
    //StyleKey.background,
    //StyleKey.background_color,
    //StyleKey.background_image,
    //StyleKey.background_atlas,
    
    //font
    StyleKey.font_size,
    StyleKey.font_style,
    StyleKey.font_weight,
    StyleKey.line_height,
    
    //text
    StyleKey.text_align,
    StyleKey.text_shadow,
    StyleKey.text_outline,  // original
    
    //Unity:original
    //StyleKey.update_z_axis,
    
    //internal-use
    //StyleKey.address_href,  // for memorize address href 
    //StyleKey.address_param, // for memorize internal parameter
    //StyleKey.address_anchor,// for memorize internal 
    
    //StyleKey.link_link_color,    //http://tohoho-web.com/css/selector.htm#visited
    //StyleKey.link_hover_color,
    //StyleKey.link_visited_color,
    //StyleKey.link_pressed_color,

    //StyleKey.anchor_index,   // See XmlAnchor

    };
    private bool isInherit(StyleKey key) { foreach(var k in m_inherit) if (k==key) return true; return false; }


    hglParser.Element m_xe;
    Hashtable m_hash;

    public hglStyle( hglParser.Element xe )
    {
        m_xe = xe;
        m_hash = new Hashtable();
    }
    private void CopyFrom(hglStyle src,bool reset/*=false*/)
    {
        if (reset)
        {
            m_hash.Clear();
        }

        if (src!=null) foreach(var k in src.m_hash.Keys)
        {
            m_hash[k] = src.m_hash[k];
        }
    }
    private object GetVal(StyleKey key)
    {
        if (m_hash.ContainsKey(key)) return m_hash[key];
        if (m_xe != null&& m_xe.parent!=null && isInherit(key))
        {
            return m_xe.parent.thisStyle.GetVal(key);
        }
        return null;
    }

    public StyleKey GetKey(string keystr)
    {
        StyleKey key = StyleKey.NONE;
        try {
            key = (StyleKey)System.Enum.Parse(typeof(StyleKey),keystr.Replace('-','_'));
        }
        catch 
        {
            key= StyleKey.NONE;
        }
        return key;
    }

    public bool HasKey(StyleKey key)
    {
        return m_hash.ContainsKey(key);
    }

    public string Get(StyleKey key)
    {
        return (string)GetVal(key);//  m_hash[key];
    }
    
    public float GetFloat(StyleKey key, float def /*= float.NaN*/)
    {
        float x;
        string val = (string)GetVal(key);
        val = hglEtc.DropAlphabet(val);
        if (float.TryParse(val,out x))
        {
            return x;
        }
        return def;
    }

    public float GetFloat_allowPercent(StyleKey key, float def /*= float.NaN*/)
    {
        bool isPercent = false;
        float x;
        string val = (string)GetVal(key);
        if (hglEtc.check_tail(val, "%"))
        {
            isPercent = true;
        }
        val = hglEtc.DropAlphabet(val);
        if (float.TryParse(val,out x))
        {
            if (isPercent)
            {
                return - (x / 100f);  // give negative digit
            }
            return x;
        }
        return def;
    }

    public float GetFloat_allowAuto(StyleKey key, float def /*= float.NaN*/)
    {
        float x;
        string val = (string)GetVal(key);
        if (val=="auto")
        {
            return (float)-0xa;
        }
        val = hglEtc.DropAlphabet(val);
        if (float.TryParse(val,out x))
        {
            return x;
        }
        return def;
    }

    public Color GetColor(StyleKey key) // if Error return Color(-1,-1,-1);
    {
        Color x;
        if (hglConverter.GetColorString((string)GetVal(key),out x))
        {
            return x;
        }

        return new Color(-1,-1,-1);
    }
    public Color? GetColorErrorNull(StyleKey key) // if Error return Color(-1,-1,-1);
    {
        Color x;
        if (hglConverter.GetColorString((string)GetVal(key),out x))
        {
            return x;
        }

        return null;
    }

    public void SetColor(StyleKey key, Color val)
    {
        string s = "#" + hglUtil.Color2String_RRGGBB(val);
        m_hash[key] = s;
    }

    public void Set<T>(StyleKey key, T val)
    {
        if (val==null) return;
        if (key.ToString().Contains("margin"))
        {
            MaginWork(key,val);
        }
        else if (key.ToString().Contains("padding"))
        {
            PaddingWork(key,val);
        }
        else if (key.ToString().Contains("border"))
        {
            BorderWork(key,val);
        }
        else if (key.ToString().Contains("background"))
        {
            BackgroundWork(key, val);
        }
        else if (key == StyleKey.font_size)
        {
            float f = 1;
            var v1 = val.ToString().Replace("pt", "");
            if (float.TryParse(v1.ToString(), out f))
            {
                m_hash[key] = v1.ToString();
            }
            else
            {
                //Debug.LogError("val=" + val);
                string v2 = "1";
                switch (val.ToString())
                {
                case "xx-small":
                case "x-small":
                case "small": v2 = "16"; break;
                case "medium": v2 = "20"; break;
                case "large": v2 = "30"; break;
                case "x-large":
                case "xx-large": v2 = "50"; break;

                default: v2 = "16"; break;
                }

                m_hash[key] = v2;
            }
        }
        else
        {
            m_hash[key] = val.ToString();
        }
    }



    public string GetAll()
    {
        string s="";
        foreach(var k in m_hash.Keys) {
            if ((StyleKey)k==StyleKey.NONE) continue;
            var k2   = ((StyleKey)k).ToString().Replace('_', '-');
            var val  = (string)GetVal((StyleKey)k);
            s += k2 + ":" + val + " ;";
        }
        return s;
    }

    //public void DebugDamp(string memo) { Debug.Log( " ALL OUTPUT :" + memo +  ":=\n" + GetAll());}

    public override string ToString()
    {
        return GetAll();
    }

    public void Parse(string s)
    {
        //Debug.Log(s);
        var lines = s.Split(';');
        foreach(var line in lines)
        {
            if (string.IsNullOrEmpty(line)) continue;
            var paras = line.Split(':');
            var atr   = hglEtc.NormalizeText_withCR(paras[0]);
            var val   = paras.Length > 1 ?  hglEtc.NormalizeText_withCR(paras[1]) : null;

            if (!string.IsNullOrEmpty(val))
            {
                var key = GetKey(atr);
                if (key != StyleKey.NONE)
                {
                    //Debug.Log(atr + " = " + val);
                    Set(key, val);
                }
            }
        }
    }

    public void AddParse(string s)
    {
        if (string.IsNullOrEmpty(s)) return;
        hglStyle tmpStyle=new hglStyle(null);
        tmpStyle.Parse(s);

        CopyFrom(tmpStyle,false);
    }


    // ##########
    // # MARGIN #
    private void MaginWork(StyleKey key, object ival)
    {
        m_hash[key] =(string)ival;
        if (key == StyleKey.margin)
        {
            float top,right,bot,left;
            if (hglConverter.GetMarginStyle((string)ival,out top, out right, out bot, out left))
            {
                m_hash[StyleKey.margin_top]   = (top  ==-0xa ? "auto" : top.ToString());
                m_hash[StyleKey.margin_right] = (right==-0xa ? "auto" : right.ToString());
                m_hash[StyleKey.margin_bottom]= (bot  ==-0xa ? "auto" : bot.ToString());
                m_hash[StyleKey.margin_left]  = (left ==-0xa ? "auto" : left.ToString());
            }
            return;
        }

        var top1  = GetFloat_allowAuto(StyleKey.margin_top,   0);
        var left1 = GetFloat_allowAuto(StyleKey.margin_left,  0);
        var bot1  = GetFloat_allowAuto(StyleKey.margin_bottom,0);
        var right1= GetFloat_allowAuto(StyleKey.margin_right, 0);

        m_hash[StyleKey.margin_top]   = (top1  ==-0xa ? "auto" : top1.ToString());
        m_hash[StyleKey.margin_right] = (right1==-0xa ? "auto" : right1.ToString());
        m_hash[StyleKey.margin_bottom]= (bot1  ==-0xa ? "auto" : bot1.ToString());
        m_hash[StyleKey.margin_left]  = (left1 ==-0xa ? "auto" : left1.ToString());

        m_hash[StyleKey.margin] = m_hash[StyleKey.margin_top].ToString() + " " 
                                + m_hash[StyleKey.margin_right].ToString() + " " 
                                + m_hash[StyleKey.margin_bottom].ToString() + " " 
                                + m_hash[StyleKey.margin_left].ToString();
    }

    public float[] GetMargin()
    {
        var s = Get(StyleKey.margin);
        float top,right,bot,left;
        if (hglConverter.GetMarginStyle(s,out top, out right, out bot, out left))
        {
            return new float[] {top,right,bot,left };
        }
        return new float[4]{0,0,0,0};
    }
    // # MARGIN #
    // ##########

    // ###########
    // # PADDING #
    private void PaddingWork(StyleKey key, object val)
    {
        string padding = (key == StyleKey.padding) ? (string)val : (string)GetVal(StyleKey.padding);
        
        float top,right,bot,left;
        hglConverter.GetMarginStyle(padding,out top, out right, out bot, out left);

        float f;
        if (float.TryParse(hglEtc.DropAlphabet((string)val),out f))
        {
            switch(key)
            {
                case StyleKey.padding_top:     top   = f; break;
                case StyleKey.padding_right:   right = f; break;
                case StyleKey.padding_bottom:  bot   = f; break;
                case StyleKey.padding_left:    left  = f; break;
            }
        } 
        m_hash[StyleKey.padding] = top + " " + right + " " + bot + " " + left;  
        m_hash[StyleKey.padding_top]   = top.ToString();
        m_hash[StyleKey.padding_right] = right.ToString();
        m_hash[StyleKey.padding_bottom]= bot.ToString();
        m_hash[StyleKey.padding_left]  = left.ToString();
    }
    public float[] GetPadding()
    {
        var s = Get(StyleKey.padding);
        float[] list;
        if (hglEtc.floatArrayTryParse(s, out list))
        { 
            return list;
        }
        return new float[4]{0,0,0,0};

    }
    public float[] GetPadding_w_border()
    {
        float[] padding = GetPadding();
        var style = (string)Get(StyleKey.border_style);
        float bw = GetFloat(StyleKey.border_width,float.NaN);
        if (style=="none"||style=="hidden") bw = 0;
        
        if (!float.IsNaN(bw) && padding!=null) for(int i = 0;i<padding.Length;i++) padding[i] += bw;

        return padding;
    }
    // # PADDING #
    // ###########

    // ###############
    // # BORDER WORK #
    private void BorderWork(StyleKey key, object val)
    {
        float  width;
        Color? color=null;
        string style;
        if (key == StyleKey.border)
        {
            string border = (string)val;
            if (!hglConverter.GetBorder(border, out width, out color, out style)) return;
            if (width>=0) m_hash[StyleKey.border_width] = width.ToString();
            if (color != null)  m_hash[StyleKey.border_color] = "#" + hglUtil.Color2String_RRGGBB((Color)color);
            if (style!=null) m_hash[StyleKey.border_style] = style;
            return;
        }
        
        m_hash[key] = val.ToString();

        string borderstr = "";
        if (m_hash[StyleKey.border_width]!=null) borderstr += (string)m_hash[StyleKey.border_width] + " ";
        if (m_hash[StyleKey.border_color]!=null) borderstr += (string)m_hash[StyleKey.border_color] + " ";
        if (m_hash[StyleKey.border_style]!=null) borderstr += (string)m_hash[StyleKey.border_style];
        
        m_hash[StyleKey.border] = borderstr;
    }

    // # BORDER WORK #
    // ###############

    // ###################
    // # BACKGROUND WORK #
    private void BackgroundWork(StyleKey key, object val)
    {
        string background = (key == StyleKey.background) ? (string)val : (string)GetVal(StyleKey.background);

        Color  color = new Color(float.NaN,float.NaN,float.NaN);
        string imgUrl="";
        string imgAtlas="";

        hglConverter.GetBackgroundStyle(background, out color, out imgUrl, out imgAtlas);

        switch(key)
        {
            case StyleKey.background_color: 
                {
                    Color tcolor;
                    if (hglConverter.GetColorString((string)val,out tcolor)) 
                    {
                        color= tcolor;
                    }
                }
                break;
            case StyleKey.background_image:
                {
                    string url;
                    if (hglConverter.GetUrlStyle((string)val,out url))
                    {
                        imgUrl = url;
                    }
                }
                break;
            case StyleKey.background_atlas:
                {
                    string atlas;
                    if (hglConverter.GetAtlasStyle((string)val,out atlas))
                    {
                        imgAtlas = atlas;
                    }
                }
                break;
        }

        m_hash[StyleKey.background] = "#" + hglUtil.Color2String_RRGGBB(color) + " url(" + imgUrl +") atlas(" + imgAtlas + ")"; 
        m_hash[StyleKey.background_color] = "#" + hglUtil.Color2String_RRGGBB(color);
        m_hash[StyleKey.background_image] = imgUrl;       
        m_hash[StyleKey.background_atlas] = imgAtlas;
        
    }
    // # BACKGROUND WORK #
    // ###################


    // #######################
    // # EASY ACCESS UTILITY # 
    public float uFontSize
    {
        get { return GetFloat(StyleKey.font_size,float.NaN); }
    }
    // # EASY ACCESS UTILITY #
    // #######################
}
}