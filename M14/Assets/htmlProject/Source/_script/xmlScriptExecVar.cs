using UnityEngine;
using System.Collections;
using xsr;

public class xmlScriptExecVar {

    public static bool ExecuteVariable(ELEMENT e, xmlScriptObj scrObj,STACKVAL stack,out object o)
    {
        string pointerstr = e.GetPointerString();
        ELEMENT last     = e.GetPointerLast();

        if (e.isVARIABLE())
        {
            if (ExecuteValiable_GetFromStack(e,scrObj,stack,out o))
            {
                return true;
            }
        }
        o = xmlScriptReflection.GetPropaty(e,scrObj,stack);
        if (o!=null) return true;
        return false;
    }
    public static bool ExecuteValiable_GetFromStack(ELEMENT e, xmlScriptObj scrObj, STACKVAL stack, out object o)
    {
        o = null;
        if (!e.isVARIABLE()) return false;
        if (e.isVARIABLE_NOTARRAY()) { o = stack.GetVal(e.raw); return true; }

        if (e.GetListCount()==1)
        {
            var index =  xmlScriptArray.GetArrayIndex(e.GetListElement(0),scrObj,stack);// Debug.LogError("index = " + index);
            var array = stack.GetVal(e.raw);
            if (array==null) { Debug.LogError("ExecuteValiable_GetFromStack #3x ERROR ");   return false;}
            if (xmlScriptGetMethod.ObjectGetType(array) == typeof(xmlScriptJS.Array))
            {
                o = ((xmlScriptJS.Array)array).Get(index);
            }
        }
        else 
        {
            object[] indexList = new object[e.GetListCount()];
            for (int i = 0; i < e.GetListCount(); i++)
            {
                indexList[i] = xmlScriptArray.GetArrayIndex(e.GetListElement(i),scrObj,stack);
            }
            var array = stack.GetVal(e.raw);
            if (array==null) return false;
            if (array!=null && xmlScriptGetMethod.ObjectGetType(array)==typeof(xmlScriptJS.Array))
            {
                o = xmlScriptJS.Array.GetMultidimension(array,indexList);
            }
        }
        return true;
    }
    public static bool ExecuteSetVariable(ELEMENT e, object val, xmlScriptObj scrObj, STACKVAL stack)
    {
        if (!e.isVARIABLE()) return false;
        if (e.isVARIABLE_NOTARRAY()) {   
            return stack.SetVal(e.raw,val); 
        }

        if (e.GetListCount() == 1)
        {
            var index = xmlScriptArray.GetArrayIndex(e.GetListElement(0), scrObj, stack); //Debug.LogError("index = " + index);
            var array = stack.GetVal(e.raw);
            if (array == null) array = new xmlScriptJS.Array();
            //Debug.LogError("array = " + array);
            if (xmlScriptGetMethod.ObjectGetType(array) == typeof(xmlScriptJS.Array))
            {
                ((xmlScriptJS.Array)array).Set(index, val);
                if (!stack.isExist(e.raw))
                {
                    stack.SetGlobalVal(e.raw, array);
                }
                else
                { 
                    stack.SetVal(e.raw,array);
                }
                //Debug.Log("array1 = " + array);
                //Debug.Log("array2 = " + stack.GetVal(e.raw));
            }
        }
        else
        {
            object[] indexList = new object[e.GetListCount()];
            for (int i = 0; i < e.GetListCount(); i++)
            {
                indexList[i] = xmlScriptArray.GetArrayIndex(e.GetListElement(i),scrObj,stack);
            }
            var array = stack.GetVal(e.raw);
            if (array == null) array = new xmlScriptJS.Array();
            if (array!=null && xmlScriptGetMethod.ObjectGetType(array)==typeof(xmlScriptJS.Array))
            {
                xmlScriptJS.Array.SetMultidimension(array,indexList,val);
                if (!stack.isExist(e.raw))
                {
                    stack.SetGlobalVal(e.raw, array);
                }
                else
                { 
                    stack.SetVal(e.raw,array);
                }
            }
        }
        return true;
    }
}
