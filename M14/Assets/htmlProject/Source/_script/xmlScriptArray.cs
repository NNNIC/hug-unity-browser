using UnityEngine;
using System.Collections;
using xsr;

public class xmlScriptArray {
    public static object GetArrayIndex(ELEMENT e, xmlScriptObj scrObj, STACKVAL stack)
    {
        if (!e.isBLOCK_L()) return null;
        var p = e.GetListElement(0);
        if (!p.isNONE()) 
            return xmlScriptFunc.Execute(p,scrObj,stack);
        return null;
    }
    public static object CreateArray(ELEMENT e, xmlScriptObj scrObj, STACKVAL stack)
    {
        if (!e.isBLOCK_L()) return null;
        if (e.GetListCount()==0) return null;

        var array = new xmlScriptJS.Array();
        for (int i = 0; i < e.GetListCount(); i++)
        {
            array.Set(i,xmlScriptFunc.Execute(e.GetListElement(i),scrObj,stack));
        }
        return array;
    }
}
