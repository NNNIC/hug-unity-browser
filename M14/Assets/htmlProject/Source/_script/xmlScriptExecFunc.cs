using UnityEngine;
using System.Collections;
using System;
using xsr;

public class xmlScriptExecFunc {
    public static bool ExecuteFunction(ELEMENT e, xmlScriptObj scrObj,STACKVAL stack,out object o)
    {
        o = null;
        object[] arglist = null;
        ELEMENT func = e.GetPointerLast();

        if (!func.isFUNCTION()) return false;

        if (func.raw == "typeof")
        {
            if (func.GetListElement(0).isBLOCK_C())
            {
                var p0e = func.GetListElement(0).GetListElement(0);
                ELEMENT last = p0e.GetPointerLast();
                var pstr = p0e.GetPointerString(); 
                pstr =  string.IsNullOrEmpty(pstr) ? last.raw : pstr + "." + last.raw;                
                Type t = xmlScriptReflection.GetTypeOf(pstr);
                if (t == null)
                {
                    var o_1 =xmlScriptFunc.Execute(p0e,scrObj,stack);
                    if (o_1!=null) t = xmlScriptGetMethod.ObjectGetType(o_1);
                }
                o =t;
                return true;
            }
            return false;
        }
        if (func.raw == "SETUPDATEFUNC"/*"S_SetUpdateFunc"*/)
        {
            if (func.GetListElement(0).isBLOCK_C())
            {
                var p0 = func.GetListElement(0).GetListElement(0);
                var p1 = func.GetListElement(0).GetListElement(1);

                var p0_o = xmlScriptFunc.Execute(p0,scrObj,stack);
                var funcname = (p1.isQUOTE() ? p1.raw : null);
                if (!string.IsNullOrEmpty(funcname) && xmlScriptGetMethod.ObjectGetType(p0_o)==typeof(GameObject))
                { 
                    var scriptMan = scrObj.GetComponent<xmlScriptMan>();
                    if (scriptMan != null)
                    {
                        scriptMan.SetUpdate(funcname,(GameObject)p0_o);
                        return true;
                    }
                }
            }
            return false;
        }
        else if (func.raw == "SETSTARTFUNC")
        {
            if (func.GetListElement(0).isBLOCK_C())
            {
                var p0 = func.GetListElement(0).GetListElement(0);
                var p1 = func.GetListElement(0).GetListElement(1);

                var p0_o = xmlScriptFunc.Execute(p0,scrObj,stack);
                var p1_o = xmlScriptFunc.Execute(p1,scrObj,stack);
                //var funcname = p1_o.ToString();
                if ( p1_o!=null &&  !string.IsNullOrEmpty(p1_o.ToString()) && (p0_o==null || xmlScriptGetMethod.ObjectGetType(p0_o)==typeof(GameObject)))
                { 
                    var scriptMan = scrObj.GetComponent<xmlScriptMan>();
                    if (scriptMan != null)
                    {
                        scriptMan.SetStart(p1_o.ToString(),(GameObject)p0_o);
                        return true;
                    }
                }
            }
            return false;
        }
        else if (func.raw == "CALL")
        {
            if (func.GetListElement(0).isBLOCK_C())
            {
                var p0 = func.GetListElement(0).GetListElement(0);

                var p0_o = xmlScriptFunc.Execute(p0,scrObj,stack);
                //var funcname = p1_o.ToString();
                if ( p0_o!=null &&  !string.IsNullOrEmpty(p0_o.ToString()))
                { 
                    var scriptMan = scrObj.GetComponent<xmlScriptMan>();
                    if (scriptMan != null)
                    {
                        scrObj.m_scriptMan.RunFunction( p0_o.ToString(),null,scrObj);
                        return true;
                    }
                }
            }
            return false;
        }

        arglist = GetArgs(func.GetListElement(0),scrObj,stack);

        if (e==func)
        {
            if (xmlScriptFunc.CallScriptFunction(func.raw,scrObj,stack,arglist, out o))
            {
                if (o!=null && xmlScriptGetMethod.ObjectGetType(o) == typeof(SENTENCE_STATE))
                {
                    var ss = (SENTENCE_STATE)o;
                    if (ss.state == SENTENCE_STATE.STATE.RETURN)
                    { 
                        o = ss.ret;
                    }
                }
                return true;
            }
        }
        else {
            var pointerstr = e.GetPointerString();
            if (xmlScriptReflection.InvokeFunc(e, arglist, scrObj, stack, out o))
            { 
                return true;
            }
        }
        return false;
    }

    public static object[] GetArgs(ELEMENT e, xmlScriptObj scrObj, STACKVAL stack)
    {
        if (!e.isBLOCK_C()) return null;
        var arglist = new object[e.GetListCount()];
        for (int i = 0; i < e.GetListCount(); i++)
        {
            arglist[i] = xmlScriptFunc.Execute(e.GetListElement(i),scrObj,stack);
        }
        return arglist;
    }
}
