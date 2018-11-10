using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using xsr;

public class xmlScriptGetMethod {

    //public static MethodInfo GetMethod(Type baseClass, string name, object[] iargs, out object[] outargs)
    //{
    //    outargs = null;
    //    Type[] iargtypes = new Type[ iargs.Length ];
    //    for (int i = 0; i < iargs.Length; i++)
    //    {
    //        iargtypes[i] = iargs[i].GetType();
    //    }

    //    Type[]     oargtypes;
    //    MethodInfo mi;
    //    if (_GetMethod(baseClass, name, iargtypes, out mi, out oargtypes))
    //    {
    //        outargs = ConvertParameters(iargs, oargtypes);
    //        return mi;
    //    }
    //    return null;
    //}

    //static bool _GetMethod(Type baseClass, string name, Type[] iargTypes, out MethodInfo omi, out Type[] oargTypes)
    //{
    //    omi = null;
    //    oargTypes = null;
    //    List<MethodInfo> mclist  = new List<MethodInfo>();
    //    List<Type[]>     mtplist = new List<Type[]>();

    //    var mis = baseClass.GetMethods();
    //    foreach (var m in mis)
    //    {
    //        if (!m.IsPublic) continue;
    //        if (m.Name == name)
    //        { 
    //            var pts = GetParameterTypes(m.ToString());
    //            if (iargTypes.Length == pts.Length)
    //            { 
    //                mclist.Add(m);
    //                mtplist.Add(pts);
    //            }
    //        }
    //    }
    //    if (mclist.Count == 1) { omi = mclist[0]; oargTypes = mtplist[0]; return true;  }
    //    if (mclist.Count == 0) { return false;                  }

    //    for(int c = 0; c<mclist.Count; c++)
    //    {
    //        int mc = 0;
    //        var mtypes = mtplist[c];
    //        for (int i = 0; i < mtypes.Length; i++)
    //        {
    //            if (iargTypes[i] == typeof(double))
    //            {
    //                if (isNumberType(mtypes[i])) mc++;
    //            }
    //            else if (iargTypes[i] == mtypes[i])
    //            {
    //                mc++;
    //            }
    //        }
    //        if (mc == mtypes.Length)
    //        {
    //            oargTypes = mtypes;
    //            omi = mclist[c];
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    public static object[] CreateArgsForCreateInstance(Type baseClass, object[] iargs)
    {
        Type[] oargTypes = null;
        if (iargs!=null && iargs.Length>0) {
            Type[] iargTypes = new Type[ iargs.Length ];
            for (int i = 0; i < iargs.Length; i++)
            {
                iargTypes[i] = ObjectGetType(iargs[i]);
            }

            List<ConstructorInfo> cilist  = new List<ConstructorInfo>();
            List<Type[]>          citypes = new List<Type[]>();

            var cis = baseClass.GetConstructors();
            foreach (var ci in cis)
            {
                if (!ci.IsPublic) continue;
                var pts = GetParameterTypes(ci.ToString());
                if (iargTypes.Length == pts.Length)
                {
                    cilist.Add(ci);
                    citypes.Add(pts);
                }
            }
            if (cilist.Count== 0 ) return null;
            if (cilist.Count == 1)
            {
                oargTypes = citypes[0];
            }
            else
            {
                for(int c = 0; c<cilist.Count; c++)
                {
                    int mc = 0;
                    var mtypes = citypes[c];
                    for (int i = 0; i < mtypes.Length; i++)
                    {
                        if (iargTypes[i] == typeof(double))
                        {
                            if (mtypes[i].IsPrimitive) mc++;
                        }
                        else if (iargTypes[i] == typeof(NULLOBJ))
                        {
                            if (!mtypes[i].IsValueType) mc++;
                        }
                        else if (iargTypes[i] == mtypes[i])
                        {
                            mc++;
                        }
                    }
                    if (mc == mtypes.Length)
                    {
                        oargTypes = mtypes;
                        break;
                    }
                }
            }

            var outargs = ConvertParameters(iargs,oargTypes);

            return outargs;
        }
        return iargs;
    }


    public static object[] ConvertParameters( object[] parameters, Type[] types )
    {
		if (parameters == null) return null;
        if (parameters.Length!=types.Length) 
			throw new SystemException("ERROR CONVERT");

        object[] outparam = new object[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            if ( xmlScriptGetMethod.ObjectGetType(parameters[i]) == types[i])
            {
                outparam[i] =parameters[i];
                continue;
            }
            if (/*parameters[i] == null ||*/ types[i]==null)
            {
                throw new SystemException("ERROR MSG"); 
            }
            outparam[i] = _ConvertType(parameters[i],types[i]);
        }

        return outparam;
    }

    static Type[] GetParameterTypes(string s)
    {
        List<string> list = new List<string>();
        {
            var l1 = s.Split('(');
            var w1 = l1[1];
            var tl = w1.Split(',',')');
			foreach(var n in tl)
			{
				if (string.IsNullOrEmpty(n)) continue;
				var n2 = n.Replace(" ","");
				list.Add(n2);
			}
        }

        Type[] tlist = new Type[list.Count];
        for (int i = 0; i < list.Count; i++)
        { 
            tlist[i] = xmlScriptReflection.GetTypeOf(list[i]); //System.Type.GetType(list[i]);
			if (tlist[i]==null) 
			{
				Debug.LogError("ERROR TLIST == null");
				throw new SystemException("ERROR MSG");
			}
        }

        return tlist;
    }



    //static bool isNumberType(Type t)
    //{
    //    Type[] list = new Type[] { 
    //        //typeof(int),       
    //        typeof(Int16),       
    //        typeof(Int32),       
    //        typeof(Int64),       
    //        //typeof(uint),       
    //        typeof(UInt16),       
    //        typeof(UInt32),       
    //        typeof(UInt64),       

    //        typeof(Byte),       
    //        typeof(Char),     
    //        typeof(Single),       
    //        typeof(Double)       
    //    };

    //    foreach (var i in list)
    //    {
    //        if (t==i) return true;
    //    }
    //    return false;
    //}

    public static MethodInfo GetMethod2(Type baseClass, string name, object[] iargs)
    {
        Type[] iargtypes = new Type[ iargs.Length ];
        for (int i = 0; i < iargs.Length; i++)
        {
            iargtypes[i] = xmlScriptGetMethod.ObjectGetType( iargs[i] );
        }

        MethodInfo mi;
        if (_GetMethod2(baseClass, name, iargtypes, out mi))
        {
            return mi;
        }
        return null;
    }

    static bool _GetMethod2(Type baseClass, string name, Type[] iargTypes, out MethodInfo omi)
    {
        omi = null;
        List<MethodInfo> mclist  = new List<MethodInfo>();
        List<Type[]>     mtplist = new List<Type[]>();

        var mis = baseClass.GetMethods();
        foreach (var m in mis)
        {
            if (!m.IsPublic) continue;
            if (m.Name == name)
            { 
                var pts = GetParameterTypes(m.ToString());
                if (iargTypes.Length == pts.Length)
                { 
                    mclist.Add(m);
                    mtplist.Add(pts);
                }
            }
        }
        if (mclist.Count == 1) { omi = mclist[0]; return true;  }
        if (mclist.Count == 0) { return false; }

        for(int c = 0; c<mclist.Count; c++)
        {
            int mc = 0;
            var mtypes = mtplist[c];
            for (int i = 0; i < mtypes.Length; i++)
            {
                if (iargTypes[i] == typeof(double))
                {
                    if (mtypes[i].IsPrimitive) mc++;
                }
                else if (iargTypes[i] == typeof(NULLOBJ))
                {
                    if (!mtypes[i].IsValueType) mc++;
                }
                else if (iargTypes[i] == mtypes[i])
                {
                    mc++;
                }
            }
            if (mc == mtypes.Length)
            {
                omi = mclist[c];
                return true;
            }
        }
        return false;
    }

    public static object MethodInfoInvoke(MethodInfo mi, object obj, object[] parameters)
    {
        var methodParameterTypes = GetParameterTypes(mi.ToString());
        var adjustedParameters = ConvertParameters(parameters, methodParameterTypes); 
        return mi.Invoke(obj,adjustedParameters);
    }
    public static object FieldInfoSetValue(FieldInfo fi, object obj, object ivalue )
    {
        object value = _ConvertType(ivalue,fi.FieldType);
        fi.SetValue(obj,value);
        return value;
    }
    public static object _ConvertType(object o, Type type)
    {
        Type t = ObjectGetType(o);
        if (t == typeof(double))
        {
            double v = (double)o;
            if (type == typeof(Int16)) return (Int16)v; 
		    if (type == typeof(Int32)) return (Int32)v; 
		    if (type == typeof(Int64)) return (Int64)v; 
		
		    if (type == typeof(UInt16)) return (UInt16)v; 
		    if (type == typeof(UInt32)) return (UInt32)v; 
		    if (type == typeof(UInt64)) return (UInt64)v; 
		
		    if (type == typeof(Byte))   return (Byte)v; 
		    if (type == typeof(SByte))  return (SByte)v; 
		    if (type == typeof(Char))   return (Char)v; 

		    if (type == typeof(Single)) return (Single)v; 
		    if (type == typeof(Double)) return (Double)v; 
        }
        else if (t == typeof(NULLOBJ))
        {
            return null;
        }
        return o;
    }

    public static Type ObjectGetType(object o)
    {
        if (o==null) return typeof(NULLOBJ);
        return o.GetType();
    }
}
