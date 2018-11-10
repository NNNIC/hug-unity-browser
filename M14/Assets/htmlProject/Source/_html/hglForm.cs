using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using hug_u;

public class hglForm {

    public enum METHOD
    {
        POST,
        GET,
    }

    public class InputItem
    {
        //public UIWidget ui;

        public string type;
        public string name;
        public string val;
    }

    METHOD          m_method;
    List<InputItem> m_list;

    hglTags_body    m_hglTags;

    public hglForm()
    {
        m_list = new List<InputItem>();
    }

    public void Init(Hashtable hash, hglTags_body hgltags)
    {
        try
        {
            m_method = (METHOD)System.Enum.Parse(typeof(METHOD), (string)hash["method"]);
        }
        catch { }

        m_hglTags = hgltags;
    }

    public void CreateInputUI(Hashtable hash)
    {
#if OBSOLATED
        if ((string)hash["type"] == "submit")
        {
            var val = (string)hash["value"];
            string text = val!=null ? val : "OK";

            m_xmlTags.CreateTextButtonItem(text,":submit","na");
            return;
        }
        else if ((string)hash["type"] == "text")
        {
            //Scale
            float padding = 3f;

            float sx = 100f;
            float sy = m_xmlTags.m_uiFont.size;
            {
                var fs = m_xmlTags.m_curStyle.GetFloat(StyleKey.font_size);
                if (!float.IsNaN(fs))
                {
                    sx = 10f * fs;
                    sy = fs;
                }
            }
            //
            int depth = NGUITools.CalculateNextDepth(m_xmlTags.m_panel);
            var go = NGUITools.AddChild(m_xmlTags.m_panel);
            go.name = "Input";


            UISprite bg = NGUITools.AddWidget<UISprite>(go);
            bg.type = UISprite.Type.Sliced;
            bg.name = "Background";
            bg.depth = depth;
            bg.atlas = m_xmlTags.m_atlas;
            bg.spriteName = m_xmlTags.m_BGsprite;
            bg.pivot = UIWidget.Pivot.Left;
            bg.transform.localScale = new Vector3(sx, sy + padding * 2f, 1f);
            bg.transform.localPosition = Vector3.zero;
            bg.MakePixelPerfect();
            bg.color = Color.white; //m_xmlTags.m_curStyle.GetColor(StyleKey.background_color);

            UILabel lbl = NGUITools.AddWidget<UILabel>(go);
            lbl.font = m_xmlTags.m_uiFont;
            lbl.pivot = UIWidget.Pivot.Left;
            lbl.transform.localPosition = new Vector3(padding, 0f, 0f);
            lbl.multiLine = false;
            lbl.supportEncoding = false;
            lbl.lineWidth = Mathf.RoundToInt(sx - padding * 2f);
            lbl.text = "You can type here";
            lbl.MakePixelPerfect();
            lbl.color = m_xmlTags.m_curStyle.GetColor(StyleKey.color);
            lbl.transform.localScale = sy * Vector3.one;

            // Add a collider to the background
            NGUITools.AddWidgetCollider(go);

            // Add an input script to the background and have it point to the label
            UIInput input = go.AddComponent<UIInput>();
            input.label = lbl;
            input.activeColor = m_xmlTags.m_curStyle.GetColor(StyleKey.color);

            go.AddComponent<xmlDisplayItem>();

            m_xmlTags.AddChild(null,go);

        }
#endif
    }

    //public UIWidget CreateSelectUI(Hashtable hash)
    //{
    //    return null;
    //}

    public void Submit()
    {

    }

    public void Reset()
    {

    }
}
