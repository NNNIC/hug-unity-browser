using UnityEngine;
using System.Collections;
using hug_u;

public class hglHtmlRenderInfo : MonoBehaviour {
    public hgBMFont m_bmFont;
    public Material m_fontMaterial;
    public hgAtlasInfo m_atlasInfo;

    public Material m_imgMaterial;

    public bool     m_1bone;
    public bool     m_bonedAtlas;
    public bool     m_bodyBackColorToCameraColor;
    public string   m_backboardAtlas;


    public xmlScriptMan m_scriptMan;
    public Vector3  m_renderTopLeft = Vector3.zero;
    public hglHtmlRender GetRender()
    {
        return transform.GetComponentInChildren<hglHtmlRender>();
    }

    void Start()
    {
        m_scriptMan = GetComponent<xmlScriptMan>();    
    }
}
