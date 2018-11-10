using UnityEngine;
using System.Collections;
using xsr;
using System;

public class xmlScriptExecPointer  {

    public static bool ExecutePointer(ELEMENT e, xmlScriptObj scrObj, STACKVAL stack, out object o)
    {
        ELEMENT last = e.GetPointerLast();
        var pointerstr = e.GetPointerString(); 

        if (xmlScriptExecFunc.ExecuteFunction(e, scrObj, stack, out o))
        {
            return true;
        }
        if (xmlScriptExecVar.ExecuteVariable(e, scrObj, stack, out o))
        {
            return true;
        }

        throw new SystemException("ERROR EXECUTE POINTER" + e.raw);
    }

    public static bool ExecuteSetPointer(ELEMENT e, object val, xmlScriptObj scrObj, STACKVAL stack)
    {
        if (!e.isPOINTER()) return false;
        ELEMENT last = e.GetPointerLast();
        var pointerstr = e.GetPointerString(); // GetPointerToString(e,out last );

        var o = xmlScriptReflection.SetPropaty(e,new object[]{val},scrObj,stack);

        if (o!=null) return true;

        return false;
    }
}
