using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using xsr;
using hug_u;

public class xmlScriptMan : MonoBehaviour {

    public bool m_enableCACHE;

    public STACKVAL m_stackval;
    public ELEMENT  m_rootElement;

    public IEnumerator Init()
    {
        CACHEDATA.enable = m_enableCACHE;

        m_rootElement = new ELEMENT() { group = GROUP.ROOT };
        m_rootElement.list = new List<ELEMENT>();
        m_stackval = new STACKVAL();

        {
            var scrObj = gameObject.GetComponent<xmlScriptObj>();
            if (scrObj != null)
            {
                Destroy(scrObj);
                yield return null;
                scrObj = gameObject.AddComponent<xmlScriptObj>();
            }
        }
    }

    static int pgm_num = 0;
    public string RunProgram(string program)
    {
        xmlScriptParse  scriptParse = new xmlScriptParse();
        scriptParse.Set(program);
        var element = scriptParse.Parse1();                  
        var pgmelement = (element.GetListElement(0).isPROGRAM()) ? element.GetListElement(0) : null;
        if (pgmelement==null) throw new SystemException("Error program");

        m_rootElement.list.Add(pgmelement);

        var scrObj = GetScriptObj(null);

        if (string.IsNullOrEmpty(pgmelement.decname)) pgmelement.decname = "program_" + pgm_num++;  
        var stack = m_stackval.CreateStack(pgmelement.decname,pgmelement);
        scrObj.m_stdout   ="";
        try
        {
            xmlScriptFunc.ExecutePROGRAM(pgmelement, scrObj, stack);
        }
        catch (SystemException e)
        {
            Debug.Log(e.Message);
        }
        return scrObj.m_stdout;
    }


    public void SetUpdate(string i_updateFuncName,GameObject obj)
    {
        var updateFuncName = i_updateFuncName.Replace("()","");

        var scrObj = GetScriptObj(obj);
        ELEMENT updatefunc;
        ELEMENT pgmelement;
        var b = ELEMENT.FindFunctionFromRoot(m_rootElement,updateFuncName, out updatefunc, out pgmelement);
        Debug.Log(m_rootElement);
        if (!b) throw new SystemException("CANNOT FIND FUNCTION : " + i_updateFuncName);

        Debug.Log("updatefunc = " + updatefunc);
        scrObj.m_update.function   = updatefunc;
        scrObj.m_update.stack      = m_stackval.FindStack(pgmelement.decname);

        {
            var parentScrObj = (xmlScriptObj)hgca.FindAscendantComponent(scrObj.gameObject,typeof(xmlScriptObj));
            if (parentScrObj != null)
            {
                scrObj.m_valid = parentScrObj.m_valid;
            }
        }

    }
    
    public void SetStart(string i_startFuncName,GameObject obj)
    {
        var startFuncName = i_startFuncName.Replace("()","");

        var scrObj = GetScriptObj(obj);

        ELEMENT startfunc;
        ELEMENT pgmelement;
        var b = ELEMENT.FindFunctionFromRoot(m_rootElement,startFuncName, out startfunc, out pgmelement);
        if (!b) throw new SystemException("CANNOT FIND FUNCTION : " + i_startFuncName);

        scrObj.m_start.function   = startfunc;
        scrObj.m_start.stack      = m_stackval.FindStack(pgmelement.decname);

        {
            var parentScrObj = (xmlScriptObj)hgca.FindAscendantComponent(scrObj.gameObject,typeof(xmlScriptObj));
            if (parentScrObj != null)
            {
                scrObj.m_valid = parentScrObj.m_valid;
            }
        }
    }
    
    private xmlScriptObj GetScriptObj(GameObject obj)
    {
        if (obj==null) obj = gameObject;
        var scrObj = obj.GetComponent<xmlScriptObj>();
        if (scrObj == null)
        {
            scrObj = obj.AddComponent<xmlScriptObj>();
        }
        scrObj.m_rootElement = m_rootElement;  scrObj.m_scriptMan   = this;
        return scrObj;
    }

#if OBSOLATED
    public void RunFunctionFromHTML(string i_funcName, object args , GameObject obj=null)
    {
        var funcname = i_funcName.Substring(0,i_funcName.IndexOf('('));
        var scrObj = GetScriptObj(obj);

        ELEMENT funcElement;
        ELEMENT pgmElement;
        var b = ELEMENT.FindFunctionFromRoot(m_rootElement,funcname,out funcElement, out pgmElement);
        if (!b) throw new SystemException("CANNOT FIND FUNCTION: " + i_funcName);
        var stack = m_stackval.FindStack(pgmElement.decname);
        xmlScriptFunc.ExecuteDEC_FUNC(funcElement,scrObj,stack,args);        
    }
#endif
    public void RunFunctionFromHTML(string funcs)
    {
        if (string.IsNullOrEmpty(funcs)) return;
        var pgm = (funcs[funcs.Length-1]!=';') ? funcs + ";" : funcs;
        RunProgram(pgm);
    }
    public object RunFunction(string funcname, object args, xmlScriptObj scrObj)
    {
        ELEMENT funcElement;
        ELEMENT pgmElement;
        var b = ELEMENT.FindFunctionFromRoot(m_rootElement,funcname,out funcElement, out pgmElement);
        if (!b) return false;
        var stack = m_stackval.FindStack(pgmElement.decname);
        return xmlScriptFunc.ExecuteDEC_FUNC(funcElement,scrObj,stack,args);
    } 
}
