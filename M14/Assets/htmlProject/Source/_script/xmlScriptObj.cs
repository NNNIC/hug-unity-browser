using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using xmlScriptJS;
using xsr;

public class xmlScriptObj : MonoBehaviour {

    public bool m_valid = false;

    bool   m_isFirstDone;
    
    public string m_stdout = "";

    public ELEMENT m_rootElement;
    public xmlScriptMan m_scriptMan;

    public class FuncDesc
    {
        public ELEMENT  function;
        public STACKVAL stack;
    }
    
    public FuncDesc m_start  = new FuncDesc();
    public FuncDesc m_update = new FuncDesc();

    Document m_document;
    public Document document { get { return m_document; } }

    _TOOL m_tool;
    public _TOOL TOOL { get {  return m_tool; } }

    Location m_location;
    public Location location {get { return m_location;}}

    public _Debug __debug;


    //###########
    //# POINTER #
    List<object> m_pointerList = new List<object>();
    public void   PointerPush(string s) {m_pointerList.Add(s);}
    public object PointerPop() { var w = (m_pointerList.Count>0 ? m_pointerList[m_pointerList.Count-1] : "" );   if (m_pointerList.Count>0) m_pointerList.RemoveAt(m_pointerList.Count-1);  return w; }
    
    public object PointerGet() 
    { 
        if (m_pointerList.Count==0) return "";
        string s = "";
        foreach(var v in m_pointerList) s+=v +".";
        return s;    
    } 
    public List<object> PointerGetList() {return m_pointerList;}
    public void PointerClear() { m_pointerList.Clear(); }
    //# POINTER #
    //###########

    ////############
    ////# VARIABLE #

    //public STACKVAL m_stackval;

    ////# VARIABLE #
    ////############

    void Awake()
    {
        m_valid = false;
        m_document = new Document(this);
        m_location = new Location(this);
        __debug = new _Debug(this);
        m_tool = new _TOOL(this);
    }

    IEnumerator Start()
    {
        m_isFirstDone = false;
        yield return null;
        var renders = GetComponentsInChildren<Renderer>();
        foreach(var r in renders) r.material.renderQueue = 4000;
        if (renderer) renderer.material.renderQueue = 4000;
    }


    void Update()
    {
        if (m_valid)
        { 
            if (m_start.function != null)
            {
                //Debug.LogError(this + "update 1");
                if (m_isFirstDone == false)
                {
                    m_isFirstDone = true;
                    xmlScriptFunc.ExecuteDEC_FUNC(m_start.function, this,m_start.stack,null);
                }
                else
                {
                    if (m_update.function!=null) xmlScriptFunc.ExecuteDEC_FUNC(m_update.function, this, m_update.stack,null);
                }
            }
            else
            {
                //Debug.LogError(this + "update 2");
                if (m_update.function!=null) xmlScriptFunc.ExecuteDEC_FUNC(m_update.function, this,m_update.stack,null);
            }
        }
    }

    //public void ExecuteProgram(ELEMENT element,STACKVAL stack)
    //{
    //    if (element != null)
    //    {
    //        xmlScriptFunc.ExecutePROGRAM(element, this, stack);
    //    }
    //}
    //public void ExecuteFunc(ELEMENT func, STACKVAL stack,object args)
    //{
    //    //var func = ELEMENT.FindFunction(m_program,funcname);
    //    Debug.Log("Execute " + func);

    //    xmlScriptFunc.ExecuteDEC_FUNC(func,this,stack);
    //}
}

