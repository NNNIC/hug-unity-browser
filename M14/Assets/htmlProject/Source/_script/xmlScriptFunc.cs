using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using xsr;
using System.Reflection;


public class xmlScriptFunc  {

    //static int pgm_num = 0;
    public static void ExecutePROGRAM(ELEMENT i_e, xmlScriptObj scrObj, STACKVAL stack)
    {
        ELEMENT e=i_e;
        
        if (e.list!=null) foreach(var sentence in e.list)
        {
            if (sentence.isSENTENCE())
            {
                Execute(sentence,scrObj,stack);
            }
        }
    }
    public static object ExecuteDEC_FUNC(ELEMENT e, xmlScriptObj scrObj,STACKVAL stack,object args)
    {
        if (e==null || !e.isDEC_FUNC()) return null;

        var newstack = stack.CreateStack(e.decname,e);

        var block_c = e.GetListElement(0);
        for(int i = 0; i<block_c.GetListCount();i++)
        {
            var vname = block_c.GetListElement(i).raw;
            var val = ((object[])args)[i];
            newstack.DeclareLocalVal(vname);
            newstack.SetVal(vname,val);
        }

        var o = Execute(e.GetListElement(1),scrObj,newstack);
        stack.DestroyStack(newstack);
        return o;
    }

    public static object Execute(ELEMENT i_e, xmlScriptObj scrObj,STACKVAL stack)
    {
        if (i_e==null || i_e.isNONE()) return null;

        ELEMENT e = i_e;
        object o;

        if (e.isSENTENCE()) e = e.GetListElement(0);

        if (e.isNUMBER())
        {
            double x = 0;
            double.TryParse(e.raw, out x);
            return x;        
        }
        else if (e.isQUOTE())
        {
            return e.raw;
        }
        else if (e.isNULL())
        {
            return null;
        }
        else if (e.isBLOCK_M())
        {

            o = null;
            stack.PushStack();
            var l = e.list;
            if (l != null) foreach (var s in l)
                {
                    if (s.isSENTENCE())
                    {
                        o = Execute(s.list[0], scrObj, stack);
                        if (o != null &&  xmlScriptGetMethod.ObjectGetType(o) == typeof(SENTENCE_STATE))
                        {
                            if (((SENTENCE_STATE)o).state != SENTENCE_STATE.STATE.NONE) break;
                        }
                    }
                }
            stack.PopStack();
            return o;
        }
        else if (e.isBLOCK_L())
        {
            return xmlScriptArray.CreateArray(e, scrObj, stack);
        }
        else if (e.isBLOCK_C())
        {
            if (e.list == null || e.list.Count == 0) return null;
            o = null;
            foreach (var s in e.list)
            {
                o = Execute(s, scrObj, stack);
            }
            return o;
        }

        else if (Execute_Func_Pointer_Variable(e, scrObj, stack, out o))
        {
            return o;
        }
        else if (e.isRETURN())
        {
            var ss = new SENTENCE_STATE();
            ss.state = SENTENCE_STATE.STATE.RETURN;
            ss.ret = Execute(e.GetListElement(0), scrObj, stack);

            return ss;
        }
        else if (e.isDEC_VAR())
        {
            stack.DeclareLocalVal(e.decname);
            return e.decname;
        }
        else if (e.isOP())
        {
            if (e.group == GROUP.op_Assign)
            {
                var p0 = e.GetListElement(0);
                var p1 = e.GetListElement(1);
                var o_p1 = Execute(p1, scrObj, stack);


                if (p0.isVARIABLE())
                {
                    bool b = xmlScriptExecVar.ExecuteSetVariable(p0, o_p1, scrObj, stack);//  stack.SetVal(p0.raw, o_p1);
                    if (!b)
                    {
                        stack.SetGlobalVal(p0.raw, o_p1);
                    }
                }
                else if (p0.isPOINTER())
                {
                    xmlScriptExecPointer.ExecuteSetPointer(p0, o_p1, scrObj, stack);
                }
                else if (p0.isDEC_VAR())
                {
                    stack.SetVal((string)Execute(p0, scrObj, stack), o_p1);
                }
                return null;
            }
            else
            {
                o = Execute_OP(e, scrObj, stack);

                if (e.group == GROUP.op_Increment_R || e.group == GROUP.op_Decrement_R)
                {
                    var p0 = e.GetListElement(0);
                    if (p0.isVARIABLE())
                    {
                        if (xmlScriptGetMethod.ObjectGetType(o) == typeof(double))
                        {
                            switch (e.group)
                            {
                            case GROUP.op_Increment_R: xmlScriptExecVar.ExecuteSetVariable(p0, (double)o + 1, scrObj, stack); break; //stack.SetVal(p0.raw, (int)o + 1); break;
                            case GROUP.op_Decrement_R: xmlScriptExecVar.ExecuteSetVariable(p0, (double)o - 1, scrObj, stack); break; // stack.SetVal(p0.raw, (int)o - 1); break;
                            }
                        }
                        else if (xmlScriptGetMethod.ObjectGetType(o) == typeof(int))
                        {
                            switch (e.group)
                            {
                            case GROUP.op_Increment_R: xmlScriptExecVar.ExecuteSetVariable(p0, (int)o + 1, scrObj, stack); break; //stack.SetVal(p0.raw, (int)o + 1); break;
                            case GROUP.op_Decrement_R: xmlScriptExecVar.ExecuteSetVariable(p0, (int)o - 1, scrObj, stack); break; // stack.SetVal(p0.raw, (int)o - 1); break;
                            }
                        }
                        else if (xmlScriptGetMethod.ObjectGetType(o) == typeof(float))
                        {
                            switch (e.group)
                            {
                            case GROUP.op_Increment_R: xmlScriptExecVar.ExecuteSetVariable(p0, (float)o + 1.0f, scrObj, stack); break; //  stack.SetVal(p0.raw, (float)o + 1.0f); break;
                            case GROUP.op_Decrement_R: xmlScriptExecVar.ExecuteSetVariable(p0, (float)o - 1.0f, scrObj, stack); break; //stack.SetVal(p0.raw, (float)o - 1.0f); break;
                            }
                        }
                    }
                }
                return o;
            }
        }
        else if (Execute_FlowControl(e, scrObj, stack, out o))
        {
            return o;
        }
        else if (e.isNEW())
        {
            ELEMENT last = e.GetListElement(0).GetPointerLast();
            if (!last.isFUNCTION()) throw new SystemException("ERROR NEW EXPECTED () ");

            object[] args = last.GetListElement(0).GetListCount() > 0 ? new object[last.GetListElement(0).GetListCount()] : null;
            for (int i = 0; i < last.GetListElement(0).GetListCount(); i++) args[i] = Execute(last.GetListElement(0).GetListElement(i), scrObj, stack);

            return xmlScriptReflection.CreateNewObject(e.GetListElement(0), args, scrObj, stack);
        }
        else if (e.isDELETE())
        {
            var p_o = Execute(e.GetListElement(0), scrObj, stack);
            if (p_o is UnityEngine.Object)
            {
                UnityEngine.Object.Destroy((UnityEngine.Object)p_o);
            }
            else if (p_o is System.IDisposable)
            {
                ((System.IDisposable)p_o).Dispose();
            }
            //ELEMENT last;
            //var pointerstr = GetPointerToString(e.GetListElement(0), out last);
            //TBD
        }
        return null;
    }

    static bool Execute_Func_Pointer_Variable(ELEMENT e, xmlScriptObj scrObj, STACKVAL stack, out object o)
    {
        o =null;
        if (e.isFUNCTION())
        {
            if (xmlScriptExecFunc.ExecuteFunction(e, scrObj, stack, out o))
            {
                return true;
            }
            throw new SystemException("ERROR EXECUTE FUNCTION " + e.raw);
        }
        else if (e.isPOINTER())
        {
            if (xmlScriptExecPointer.ExecutePointer(e, scrObj, stack, out o))
            {
                return true;
            }
            throw new SystemException("ERROR EXECUTE POINTER" + e.raw);
        }
        else if (e.isVARIABLE())
        {
            if (xmlScriptExecVar.ExecuteVariable(e, scrObj, stack, out o))
            { 
                return true;
            }
            throw new SystemException("ERROR EXECUTE VARIABLE" + e.raw);
        }

        return false;
    }
    static bool Execute_FlowControl(ELEMENT e, xmlScriptObj scrObj, STACKVAL stack, out object o)
    {
        o = null;
        if (e.isIF())
        {
            bool b = (bool)Execute(e.GetListElement(0), scrObj, stack);
            if (b)
            {
                o = Execute(e.GetListElement(1), scrObj, stack);  //Debug.LogError("isIF o=" + o);
                return true;
            }
            for (int i = 2; i < e.list.Count; i++)
            {
                var s = e.GetListElement(i); if (s.isNONE()) { o = null; return true; }
                if (s.isELSE())
                {
                    o = Execute(s.GetListElement(0), scrObj, stack);
                    return true;
                }
                else if (s.isELSEIF())
                {
                    bool b2 = (bool)Execute(s.GetListElement(0), scrObj, stack);
                    if (b2)
                    {
                        o =  Execute(s.GetListElement(1), scrObj, stack);
                        return true;
                    }
                }
            }
        }
        else if (e.isSWITCH())
        {
            object x = null;
            {
                var p0 = e.GetListElement(0).GetListElement(0);
                x = Execute(p0, scrObj, stack);
            }

            var block = e.GetListElement(1);
            int start_index = -1;
            for (int i = 0; i < block.GetListCount(); i++)
            {
                if (!block.GetListElement(i).isSENTENCE()) continue;
                var case_e = block.GetListElement(i).GetListElement(0);
                if (case_e.isCASE())
                {
                    var case_a = case_e.GetListElement(0);
                    if (case_a.raw == x.ToString())
                    {
                        start_index = i + 1;
                        break;
                    }
                }
                else if (case_e.isDEFAULT())
                {
                    start_index = i + 1;
                    break;
                }
            }

            if (start_index > 0)
            {
                for (int i = start_index; i < block.GetListCount(); i++)
                {
                    o = Execute(block.GetListElement(i), scrObj, stack);
                    if (o != null && xmlScriptGetMethod.ObjectGetType(o) == typeof(SENTENCE_STATE))
                    {
                        var ss = (SENTENCE_STATE)o;
                        if (ss.state == SENTENCE_STATE.STATE.BREAK) break;
                    }
                }
            }
            return true;
        }
        else if (e.isFOR()) 
        {
            var e_bc = e.GetListElement(0); //Debug.Log("e_bc" + e_bc); Debug.Log("e_bc.list[0]=" + e_bc.GetListElement(0)); Debug.Log("e_bc.list[1]=" + e_bc.GetListElement(1)); Debug.Log("e_bc.list[2]=" + e_bc.GetListElement(2));
            var e_bm = e.GetListElement(1); //Debug.Log("e_bm" + e_bm);
            stack.PushStack();
            Execute(e_bc.GetListElement(0), scrObj, stack);
            o = null;
            for (var i = 0; i < 200; i++)
            {
                bool b = (bool)Execute(e_bc.GetListElement(1), scrObj, stack);
                if (!b) break;
                o = Execute(e_bm, scrObj, stack);
                if (o != null && xmlScriptGetMethod.ObjectGetType(o) == typeof(SENTENCE_STATE))
                {
                    var state = (SENTENCE_STATE)o;
                    if (state.state == SENTENCE_STATE.STATE.BREAK) break;
                    if (state.state == SENTENCE_STATE.STATE.CONTINUE) goto _FOR_LOOPEND;
                    if (state.state == SENTENCE_STATE.STATE.RETURN) break;

                }
            _FOR_LOOPEND:
                Execute(e_bc.GetListElement(2), scrObj, stack);
            }
            stack.PopStack();

            o = null;
            return true;
        }
        else if (e.isWHILE())
        {
            var e_bc = e.GetListElement(0);
            var e_bm = e.GetListElement(1);

            stack.PushStack();
            Execute(e_bc.GetListElement(0), scrObj, stack);
            o = null;
            for (var i = 0; i < 200; i++)
            {
                bool b = (bool)Execute(e_bc.GetListElement(0), scrObj, stack);
                if (!b) break;
                o = Execute(e_bm, scrObj, stack);
                if (o != null && xmlScriptGetMethod.ObjectGetType(o) == typeof(SENTENCE_STATE))
                {
                    var state = (SENTENCE_STATE)o;
                    if (state.state == SENTENCE_STATE.STATE.BREAK) break;
                    if (state.state == SENTENCE_STATE.STATE.CONTINUE) continue;
                    if (state.state == SENTENCE_STATE.STATE.RETURN) break;
                }
            }
            stack.PopStack();

            o = null;
            return true;
        }
        else if (e.isBREAK())
        {
            Debug.LogWarning("BREAK " + e);
            o = new SENTENCE_STATE() { state = SENTENCE_STATE.STATE.BREAK };
            return true;
        }
        else if (e.isCONTINUE())
        {
            o = new SENTENCE_STATE() { state = SENTENCE_STATE.STATE.CONTINUE };
            return true;
        }
        return false;
    }


    static object Execute_OP(ELEMENT e,xmlScriptObj scrObj,STACKVAL stack)
    {
        if (!e.isOP()) return null;
        object[] args = null;
        Type[]   types = null;
        object o = null;

        var p0 = Execute(e.GetListElement(0),scrObj,stack);
        var p1 = Execute(e.GetListElement(1),scrObj,stack);
        if (e.GetOP_ARGC()==2)
        {
            args = new object[]{p0,p1};
            types = new Type[]{ xmlScriptGetMethod.ObjectGetType(p0) , xmlScriptGetMethod.ObjectGetType(p1)};
        }
        else if (e.GetOP_ARGC()==1)
        {
            args = new object[]{p0};
            types = new Type[]{ xmlScriptGetMethod.ObjectGetType(p0)};
        }
        
        o = xmlScriptOP._Execute_OP_INTFLOATDOUBLE(e,e.GetOP_ARGC(),p0,p1,scrObj,stack);
        if (o!=null) return o;

        o = xmlScriptOP._Execute_OP_STRING(e,p0,p1,scrObj,stack);
        if (o!=null) return o;

        {
            var t = xmlScriptGetMethod.ObjectGetType(p0);                             if (t==null) throw new System.Exception(e + " Unexcted Operation, nonetype");
            object[] iparam = new object[]{p0,p1};
            var mi   = xmlScriptGetMethod.GetMethod2(t,e.group.ToString() ,iparam);
            return xmlScriptGetMethod.MethodInfoInvoke(mi,null,iparam);
        }

        throw new System.Exception(e + " Unexcted Operation");
    }


    public static bool CallScriptFunction(string funcname, xmlScriptObj scrObj, STACKVAL stack, object args, out object o)
    {
        o = null;
        ELEMENT funcElement;
        ELEMENT pgmElement;
        var b = ELEMENT.FindFunction(stack,funcname,out funcElement, out pgmElement);
        if (!b) b =ELEMENT.FindFunctionFromRoot(scrObj.m_rootElement,funcname,out funcElement, out pgmElement);
        if (!b) return false;
        o = xmlScriptFunc.ExecuteDEC_FUNC(funcElement,scrObj,stack,args);
        return true;
    } 
}
