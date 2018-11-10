using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using xsr;
using System;

public class xmlScriptReflection : MonoBehaviour {

    public static object CreateNewObject(ELEMENT e, object[] args, xmlScriptObj scrObj, STACKVAL stack)
    {
        ELEMENT last = e.GetPointerLast();
        string  pointerstr = e.GetPointerString();
        string name = string.IsNullOrEmpty(pointerstr) ? last.raw : pointerstr + "." + last.raw;
        object o = null;

        var t = GetTypeOf(name);
        if (hglEtc.check_head(t.ToString(),"xmlScriptJS."))
        {
            if (args != null)
            {
                var jsargs = new xmlScriptJS.ARGS();
                jsargs.m_args = args;
                o = System.Activator.CreateInstance(t, jsargs);
            }
            else
            {
                o = System.Activator.CreateInstance(t);
            }

            return o;
        }

        var nargs = xmlScriptGetMethod.CreateArgsForCreateInstance(t,args);
        o = System.Activator.CreateInstance(t,nargs);

        return o;
    }

    public static Type GetTypeOf(string name)
    {
        Type t = System.Type.GetType(name);
        if (t == null)
        {
            t = System.Type.GetType("xmlScriptJS." + name);
        }
        if (t==null)
        {
            t = System.Type.GetType("UnityEngine." + name + ",UnityEngine");
        }
        if (t == null)
        {
            t = System.Type.GetType(name + ",UnityEngine");
        }
        if (t == null)
        {
            t = System.Type.GetType("System."+name);
        }


        if (t==null) throw new SystemException("ERROR TYPE,[" + name +"]");
        
        return t;
    }

    public static bool InvokeFunc(ELEMENT ie, object[] args, xmlScriptObj baseObj,STACKVAL stack, out object o)
    {
        // ref :  http://dobon.net/vb/dotnet/programing/typegetmember.html
        o = null;

        ELEMENT e = ie;
        ELEMENT last = e.GetPointerLast();
        string func = last.raw;
        object p = null;
        Type   t = null;

        p = GetPropaty(e,baseObj,stack);

        if (p!=null) 
        {
            t = xmlScriptGetMethod.ObjectGetType(p);
        }
        else //if (xmlEtc.CountCharInString(pointerstr,'.') == 1)  // **.**.
        {
            string pointerstr=e.GetPointerString();
            t= GetTypeOf(pointerstr);
        }
        if (t==null) throw new SystemException("ERROR CAN'T SOLVE TYPE :" + e.GetPointerString());
        if (string.IsNullOrEmpty(func) || t == null) throw new SystemException("ERROR NOT DEFINED FUNC NAME");

        return __InvokeFunc(last, args, ref o, func, p, t);
    }


    public static object GetPropaty(ELEMENT e, xmlScriptObj scrObj, STACKVAL stack)
    {
        var len = e.GetPointerCount(true);
        var n0  = e.GetPointerName(true);
        var n1  = len>1 ?  e.GetPointerNext().GetPointerName(true) : null;

        int index = 0;
        object p  = null;
        {
            var val = e.GetPointerVARIABLE();
            if (val!=null)
            {
                object o = null;
                if (xmlScriptExecVar.ExecuteValiable_GetFromStack(val, (xmlScriptObj)scrObj, stack, out o))
                {
                    p = o;
					if (p!=null) index=1;
                }
            }
        }

        if (p==null && scrObj != null)
        { 
            p = _ObjectGetPropaty(scrObj,e,scrObj,stack);
            if (p!=null) index=1;
        }

        if (p == null)
        {
            p = _StaticGetPropaty(n0,n1);
            if (p!=null) index=2;
        }

        if (p==null) return null;

        if (index < len)
        {
            for (int i = index; i < len; i++)
            {
                var ne = e.GetPointerNext(i);
                var pn = ne.GetPointerName(true);
                p = _ObjectGetPropaty(p,ne,scrObj,stack);

                if (p==null) break;        
            }
        } 
        return p;
    }

    public static object SetPropaty(ELEMENT e,object[] args, xmlScriptObj scrObj,STACKVAL stack)
    {

        var len = e.GetPointerCount(true);
        var n0  = e.GetPointerName(true);
        var n1  = len>1 ?  e.GetPointerNext().GetPointerName(true) : null;

        int index = 0;
        object p  = null;

        {
            var val = e.GetPointerVARIABLE();
            if (val!=null)
            {
                object o = null;
                if (xmlScriptExecVar.ExecuteValiable_GetFromStack(val, (xmlScriptObj)scrObj, stack, out o))
                {
                    p = o;
					if (p!=null) index=1;
                }
            }
        }
       
        if (p==null && scrObj != null)
        { 
            //p = __ObjectGetPropaty(baseObj,n0);
            p = _ObjectGetPropaty(scrObj,e, scrObj,stack);
            if (p!=null) index=1;
        }

        if (p == null)
        {
            if (len == 2)
            {
                return _StaticSetPropaty(n0,n1,args);
            }
            p = _StaticGetPropaty(n0,n1);
            if (p!=null) index=2;
        }

        if (p==null) return null;

        if (index < len-1)
        {
            for (int i = index; i < len - 1; i++)
            {
                var ne = e.GetPointerNext(i);
                var pn = ne.GetPointerName(true);
                p = __ObjectGetPropaty(ne, p, pn);
                if (p==null) break;        
            }
        }

        var setelem = e.GetPointerNext(len-1);
        var setname = setelem.GetPointerName(true); //names[names.Length-1];
        return _ObjectSetPropaty(setelem, p,setname,args);
    }

    static object _StaticGetPropaty(string name1,string name2)
    {
        if (string.IsNullOrEmpty(name1) || string.IsNullOrEmpty(name2)) return null;       
        var t = GetTypeOf(name1);                                        if (t==null)  return null; 
        var pi = t.GetProperty(name2);                                   if (pi==null) return null;
        var gm = pi.GetGetMethod();                                      if (gm==null) return null;
        return xmlScriptGetMethod.MethodInfoInvoke(gm,null, null); 
        //return gm.Invoke(null, null); 
    }
    static object _StaticSetPropaty(string name1,string name2, object[] args)
    {
        if (string.IsNullOrEmpty(name1) || string.IsNullOrEmpty(name2)) return null;       
        Type t = Type.GetType(name1);
        if (t==null) t = GetTypeOf(name1);                               if (t==null)  return null; 
        var pi = t.GetProperty(name2);                                   if (pi==null) return null;
        var sm = pi.GetSetMethod();                                      if (sm==null) return null;
        return xmlScriptGetMethod.MethodInfoInvoke(sm,null, args); 
        //return sm.Invoke(null, args); 
    }
    static object __ObjectGetPropaty(ELEMENT e, object obj, string name)
    {
        if (CACHEDATA.enable && e.cache != null && e.cache.info != null)
        {
            if ( xmlScriptGetMethod.ObjectGetType( e.cache.info) == typeof(System.Reflection.MethodInfo))
            {
                var mi = (System.Reflection.MethodInfo)e.cache.info;
                xmlScriptGetMethod.MethodInfoInvoke(mi,obj,null);
                //return mi.Invoke(obj,null);
            }
            if ( xmlScriptGetMethod.ObjectGetType(e.cache.info) == typeof(System.Reflection.FieldInfo))
            {
                var fi = (System.Reflection.FieldInfo)e.cache.info;
                return fi.GetValue(obj);
            }
        }

        object o = null;
        if (obj==null || string.IsNullOrEmpty(name))     return null;
        var  t = xmlScriptGetMethod.ObjectGetType(obj);  if (t==null)  return null;
        var pi = t.GetProperty(name);
        if (pi == null)
        {
            var fi = t.GetField(name);
            if (fi == null) return null;

            if (CACHEDATA.enable && (e.cache == null || e.cache.info == null))
            {
                e.cache = new CACHEDATA(){info = fi};
            }
            o = fi.GetValue(obj);
            if (o==null) Debug.LogError(">>>>>>>>>>>> o = null");
            return o;
        }
        var gm = pi.GetGetMethod();    if (gm==null) return null;
        if (CACHEDATA.enable && (e.cache == null || e.cache.info == null))
        {
            e.cache = new CACHEDATA(){info = gm};
        }
        //o = gm.Invoke(obj, null);
        o = xmlScriptGetMethod.MethodInfoInvoke(gm,obj,null);
        return o;
    }
    static object _ObjectGetPropaty(object obj, ELEMENT ie,xmlScriptObj scrObj,STACKVAL stack)
    {
        object o = null;
        if (obj==null) 
            throw new SystemException("ERRPOR POINTERS HAS NULL:"+ie);

        ELEMENT e = ie;
        if (ie.isPOINTER())
        {
            e = ie.GetPointerInside();
        }

        switch(e.group)
        {
        case GROUP.VARIABLE:
            if (e.isVARIABLE_ARRAY()) throw new SystemException("ERROR THE ARRAY IN THE MIDDLE OF POINTER ISN'T SUPPORTED.");
            return __ObjectGetPropaty(e,obj,e.raw);           
            
        case GROUP.FUNCTION:
            if (_ObjectInvokeFunc(obj, e, scrObj, stack, out o))
            {
                return o;
            }
            throw new SystemException("ERROR FUNCTION :" + e.ToString());
        default:
            throw new SystemException("ERROR NOT SUPPORTED :" + e.ToString());
        }
    }
    static object _ObjectSetPropaty(ELEMENT e,object obj, string name, object[] args)
    {
        if (obj==null || string.IsNullOrEmpty(name)) return null;

        if (CACHEDATA.enable && e.cache != null && e.cache.info != null)
        {
            if (xmlScriptGetMethod.ObjectGetType(e.cache.info)==typeof(System.Reflection.MethodInfo))
            {
                var sm = (System.Reflection.MethodInfo)e.cache.info;
                return xmlScriptGetMethod.MethodInfoInvoke(sm,obj,args);
            }
            if (xmlScriptGetMethod.ObjectGetType(e.cache.info) == typeof(System.Reflection.FieldInfo))
            {
                var fi = (System.Reflection.FieldInfo)e.cache.info;
                xmlScriptGetMethod.FieldInfoSetValue(fi,obj,args[0]); //fi.SetValue(obj,args[0]);
                return args[0];
            }
        }

        var  t = xmlScriptGetMethod.ObjectGetType(obj);        if (t==null)  return null;
        var pi = t.GetProperty(name);
        if (pi != null)
        {
            var sm = pi.GetSetMethod(); if (sm == null) return null;
            if (CACHEDATA.enable && (e.cache==null || e.cache.info==null) )
            {
                e.cache = new CACHEDATA(){info = sm};
            }
            return xmlScriptGetMethod.MethodInfoInvoke(sm,obj, args);
        }
        else
        {
            var fi = t.GetField(name);
            if (fi==null) return null;

            args[0] = xmlScriptGetMethod._ConvertType(args[0],fi.DeclaringType);

            xmlScriptGetMethod.FieldInfoSetValue(fi,obj,args[0]);//fi.SetValue(obj,args[0]);
            if (CACHEDATA.enable && (e.cache==null || e.cache.info==null) )
            {
                e.cache = new CACHEDATA(){info = fi};
            }
            return args[0];
        }
    }

    public static bool _ObjectInvokeFunc(object ip, ELEMENT e, xmlScriptObj baseObj, STACKVAL stack, out object o)
    {
        o = null;
        if (ip==null) throw new SystemException("ERROR THE PARENT OF THE FUNCTION IS NULL :" + e.ToString());
        if (!e.isFUNCTION()) throw new SystemException("ERROR STATEMENT IS NOT FUNCTION :" + e.ToString());
        var t= xmlScriptGetMethod.ObjectGetType(ip);

        object[] arglist = xmlScriptExecFunc.GetArgs(e,baseObj,stack);
        return __InvokeFunc(e,arglist, ref o, e.raw, ip, t);
    }
    private static bool __InvokeFunc(ELEMENT e, object[] iargs, ref object o, string func, object p, Type t)
    {
        object[] args = iargs;

        System.Reflection.MethodInfo mi = null;
        if (CACHEDATA.enable && e.cache != null)
        {
            if (e.cache.info!=null && xmlScriptGetMethod.ObjectGetType(e.cache.info) == typeof(System.Reflection.MethodInfo))
            {
                mi = (System.Reflection.MethodInfo)e.cache.info;
            }
        }

        if (mi == null)
        { 
            mi   = xmlScriptGetMethod.GetMethod2(t,func ,iargs);
        }
        if (mi == null)
        {
            throw new SystemException("ERROR CAN'T FIND METHOD:" + func);
        }
        o = xmlScriptGetMethod.MethodInfoInvoke(mi,p,args);

        if (CACHEDATA.enable && e.cache == null)
        {
            e.cache = new CACHEDATA() { info = mi};
        }
   
        return true;
    }

}
