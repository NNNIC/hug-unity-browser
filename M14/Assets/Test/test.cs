using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

public class test : MonoBehaviour {


    //http://d.hatena.ne.jp/s-kita/20090201/1233473013

    [ContextMenu("test")]
    void Test()
    {
        Type t = transform.GetType();        
        MethodInfo mi = t.GetMethod("RotateAroundLocal"); 
        Debug.Log(mi);
        mi.Invoke(transform,new object[]{Vector3.forward,30.0f});

    }

    public GameObject m_obj;

    [ContextMenu("test2")]
    void Test2()
    {
        Type t = m_obj.GetType();
        Debug.Log(t);
        //var mis = t.GetMethods();
        //foreach(var mi in mis) Debug.Log(mi);
        var pis = t.GetProperties();
        foreach(var pi in pis) Debug.Log(pi);
    }

    [ContextMenu("test3")]
    void Test3()
    {
        {
            //http://answers.unity3d.com/questions/23871/accessing-unityengine-properties-from-standalone-d.html
            var t = Type.GetType("UnityEngine.Time, UnityEngine");
            Debug.Log(t);

            var ps = t.GetProperties();
            //foreach(var mi in ps) Debug.Log(mi);
            var p = t.GetProperty("time");
            var gm = p.GetGetMethod();
            Debug.Log(gm);
            Debug.Log(gm.Invoke(null,null));


            Debug.Log(SystemInfo.systemMemorySize);

        }
    }

    [ContextMenu("Test4")]
    void Test4(){
        //transform.localPosition = Vector3.one;
        var pi1 = this.GetType().GetProperty("transform");
        var gm1 = pi1.GetGetMethod();
        var t   = gm1.Invoke(this,null);

        var gm2 = pi1.PropertyType.GetProperty("localPosition").GetSetMethod();
        gm2.Invoke(t,new object[]{  Vector3.one });

        Debug.Log(t.GetType());

    }

    [ContextMenu("Test5")]
    void Test5()
    {
        var t = typeof(Vector3);
        var m = t.GetMethod("op_Addition");
        var o = m.Invoke(null,new object[]{ (object)Vector3.one,(object)Vector3.one});
        Debug.Log(o);

    }
}





