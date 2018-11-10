using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class hglConverter : MonoBehaviour {
    
    public static float referenceFontSize=16;

    public static bool GetColorString(string str, out Color color)
    {
        color = Color.black;
        if (str==null||str.Length==0) return false;
        if (str==null) return false;

        bool check = false;

        if (str[0] != '#')
        {
            switch (str)
            {
            case "black":  str="#000000"; check = true; break;
            case "silver": str="#c0c0c0"; check = true; break;
            case "gray":   str="#808080"; check = true; break;
            case "white":  str="#ffffff"; check = true; break;
            case "maroon": str="#800000"; check = true; break;
            case "red":    str="#ff0000"; check = true; break;
            case "purple": str="#800080"; check = true; break;
            case "fuchsia":str="#ff00ff"; check = true; break;
            case "green":  str="#008000"; check = true; break;
            case "lime":   str="#00ff00"; check = true; break;
            case "olive":  str="#808000"; check = true; break;
            case "yellow": str="#ffff00"; check = true; break;
            case "navy":   str="#000080"; check = true; break;
            case "blue":   str="#0000ff"; check = true; break;
            case "teal":   str="#008080"; check = true; break;
            case "aqua":   str="#00ffff"; check = true; break;
            case "windowtext" : str="#808080"; check = true; break;

            }

        }

        if (str.Length == 7 && str[0] == '#')
        {
            float r = (float)System.Convert.ToInt32(str.Substring(1, 2), 16);
            float g = (float)System.Convert.ToInt32(str.Substring(3, 2), 16);
            float b = (float)System.Convert.ToInt32(str.Substring(5, 2), 16);

            color = new Color(r / 255f, g / 255f, b / 255f);

            check = true;
        }
        return check;
    }

    public static bool ConvertSizeString(string str, float in_size, out float out_size)
    {
        out_size = in_size;
        if (str == null || str.Length == 0) return false;

        try
        {
            if (str.Length >= 2 && str[str.Length - 1] == '%')
            {
                string n_str = str.Substring(0, str.Length - 1);
                float v = float.Parse(n_str);
                out_size = in_size * v / 100f;
                return true;
            }

            float v2 = float.Parse(str);
            out_size = v2;
            return true;
        }
        catch
        {
            Debug.LogError("unexpected");
            return false;
        }
    }

    public static bool GetMarginStyle(string str, out float margin_top, out float margin_right, out float margin_bot, out float margin_left)
    {
        //Debug.LogError("ConvertMarginStyle2");
        margin_top = margin_right = margin_bot = margin_left = 0f;
        if (string.IsNullOrEmpty(str)) return false;

        float[] list;
        //if (!hglEtc.floatArrayTryParse(str, out list)) return false;
        {
            var words = str.Split(' ');
            list = new float[words.Length];
            for (int i = 0; i < words.Length; i++)
            {
                list[i] = GetLength(words[i]);
                if (float.IsNaN(list[i]))
                {
                    list[i]=0;
                }
            }
        }         

        if (list.Length==0) return false;

        // ref : http://www.web-mame.net/css_course/css_properties/properties4.html

        if (list.Length == 4)
        {
            margin_top = list[0]; margin_right = list[1]; margin_bot = list[2]; margin_left = list[3];
        }
        else if (list.Length == 3)
        {
            margin_top = list[0]; margin_right = margin_left = list[1]; margin_bot = list[2];
        }
        else if (list.Length == 2)
        {
            margin_top = margin_bot = list[0]; margin_right = margin_right = list[1];
        }
        else
        {
            margin_top = margin_right = margin_bot = margin_left = list[0];
        }
        return true;
    }

    public static bool ConvertWidthHeightByTexture(string w, string h, Texture tex, float view_w,  out float ow, out float oh)
    {
        ow = tex.width;
        oh = tex.height;

        if ( (!string.IsNullOrEmpty(w) && w.Contains("%")) || (!string.IsNullOrEmpty(h) && h.Contains("%")) )
        {
            float fw = hglEtc.GetFloatFromStr_w_percent(w);
            float fh = hglEtc.GetFloatFromStr_w_percent(h);
            
            if (!float.IsNaN(fw))
            {
                ow      = view_w * fw;
                float s = ow / tex.width;
                oh      = tex.height * s;
                return true;
            }

            return false;
        }
        {
            float fw,fh;
            if (float.TryParse(w,out fw))
            {
                ow = fw;
            }
            if (float.TryParse(h,out fh))
            {
                oh = fh;
            }
        }
        return true;
    }

    public static bool GetTextShadowStyle(string val, out float x, out float y, out Color col)
    {
        x=y=3;
        col = Color.red;

        if (string.IsNullOrEmpty(val)) return false;

        var val2 = val.Replace("px"," ");
        var list = val2.Split(' ',',');
        
        int index = 0;
        foreach( var w in list)
        {
            if (string.IsNullOrEmpty(w)) continue;
            switch(index)
            {
                case 0: x = float.Parse(w); index++; break;
                case 1: y = float.Parse(w); index++; break;
                case 2: 
                case 3:
                    float a;
                    if (float.TryParse(w, out a))
                    {
                        index++;
                        break;
                    }
                    GetColorString(w,out col);
                    index++;
                    break;
            }
        }

        if (index>=3) return true;

        return false;
    }
    public static bool GetTextOutLineStyle(string val, out float x, out float y, out Color col)
    {
        x=y=3;
        col = Color.red;

        if (string.IsNullOrEmpty(val)) return false;

        var val2 = val.Replace("px"," ");
        var list = val2.Split(' ',',');
        
        int index = 0;
        foreach( var w in list)
        {
            if (string.IsNullOrEmpty(w)) continue;
            switch(index)
            {
                case 0: x = float.Parse(w); index++; break;
                case 1: y = float.Parse(w); index++; break;
                case 2: 
                case 3:
                    float a;
                    if (float.TryParse(w, out a))
                    {
                        index++;
                        break;
                    }
                    GetColorString(w,out col);
                    index++;
                    break;
            }
        }

        if (index>=3) return true;

        return false;
    }

    public static bool GetBackgroundStyle(string istr, out Color ocolor, out string oimgurl, out string oimgatlas)
    {
        ocolor  = new Color();
        oimgurl ="";
        oimgatlas = "";

        if (string.IsNullOrEmpty(istr)) return false;

        bool b = false;

        var lines = istr.Split(' ',';');
        foreach(var line in lines)
        {
            if (string.IsNullOrEmpty(line)) continue;

            //Debug.Log("GetBackgroundStyle : line = " + line);

            Color tcol;
            if (hglConverter.GetColorString(line,out tcol))
            {
                ocolor = tcol;
                b =true;

                //Debug.Log("GetBackgroundStyle : color = " + tcol);

                continue;
            }
            string turl;
            if (hglConverter.GetUrlStyle(line,out turl))
            {
                oimgurl = turl;
                b=true;
                //Debug.Log("GetBackgroundStyle : img url = " + turl);

                continue;
            }
            string tatlas;
            if (hglConverter.GetAtlasStyle(line,out tatlas))
            {
                oimgatlas = tatlas;
                b=true;
                //Debug.Log("GetBackgroundStyle : img atlas = " + tatlas);

                continue;
            }
        }
        return b;
    }

    public static bool GetUrlStyle(string istr, out string ousr)
    {
        ousr="";
        var line = hglEtc.NormalizeText_withCR(istr);
        if (string.IsNullOrEmpty(line)) return false;
      
        if (hglEtc.check_head(line,"url("))
        {
            var s1 = line.Replace("url(","").Replace(")","").Replace(";","");
            ousr = s1;
            return true;
        }
        return false;
    }
    public static bool GetAtlasStyle(string istr, out string oatlas)
    {
        oatlas="";
        var line = hglEtc.NormalizeText_withCR(istr);
        if (string.IsNullOrEmpty(line)) return false;
      
        //Debug.Log("line = " + line);

        if (hglEtc.check_head(line,"atlas("))
        {
            var s1 = line.Replace("atlas(","").Replace(")","").Replace(";","");
            oatlas = s1;
            return true;
        }
        return false;
    }

    public static float GetLength(string slen) //ref: http://tohoho-web.com/css/value/length.htm
    {
        string unitstr="";
        for (int i = slen.Length - 1; i >= 0; i--)
        {
            var c = slen[i];
            if (char.IsLetter(c))
            {
                unitstr = c + unitstr;
            }
            else
            {
                break;
            }
        }
        float u = 1;
        switch(unitstr)
        {
        case "em":
        case "ex":
        case "ch":
        case "rem": u = referenceFontSize;   break;
        case "vw":  u = Screen.width;        break;
        case "vh":  u = Screen.height;       break;
        case "vmin": u= Screen.width;        break;
        case "cm":   u = referenceFontSize;  break;
        case "mm":   u =1;                   break;
        case "in":   u = referenceFontSize;  break;
        default  :   u =1;                   break; 
        }

        var num = hglEtc.DropAlphabet(slen);
        
        float val = float.NaN;
        if (float.TryParse(num, out val))
        {
            return val;
        }

        if (slen=="auto") return -0xa;
        return float.NaN;
    }

    public static bool GetBorder(string border, out float width, out Color? color, out string style)
    {
        width = -1;
        style = null;
        color = null;
        if (string.IsNullOrEmpty(border)) return false;
        var words = border.Split(' ');
        foreach (var w in words)
        {
            if (string.IsNullOrEmpty(w)) continue;
            if (color==null)
            {
                Color tcol;
                if (GetColorString(w,out tcol)) { color =tcol; continue; }
            }
            if (width < 0)
            {
                var i = GetLength(w);
                if (!float.IsNaN(i)) width = i;
                continue;
            }
            if (style == null)
            {
                style = w;
            }
        }
        if (color!=null || width >=0 || style!=null) return true;
        return false;
    }
}
