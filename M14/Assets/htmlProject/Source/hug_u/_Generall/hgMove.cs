using UnityEngine;
using System.Collections;
using System;

public class hgMove {

    public class linear
    {
        Vector3  save_start;
        Vector3  save_end;
        Vector3  s2e;
        float    magnitude;
        Vector3  v;

        public void Init(Vector3 start, Vector3 end, float speed)
        {
            save_start = start;
            save_end   = end;

            s2e = end -start;
            magnitude = s2e.magnitude;

            v = s2e.normalized * speed;
            
        }

        public IEnumerator Coroutine(Action<Vector3,float> a)
        {
            Vector3 c = Vector3.zero;
            Vector3 p = save_start;
            
            while(c.magnitude < magnitude)
            {
                a(p+c,c.magnitude);
                yield return new WaitForFixedUpdate();

                c += v * Time.deltaTime;
            }

            a(save_end,s2e.magnitude);

            yield return new WaitForEndOfFrame();

        }
    };


    public static IEnumerator linear_float(float start, float end, float time,Action<float> act )
    {
        float s = 0.0f;
        float speed = 1 / time;
        while (s < 1f)
        {
            float pos   = Mathf.Lerp(start,end,s);

            act(pos);
 
            yield return null;

            s += Time.deltaTime * speed;
        }
        act(end);
    }

    public static IEnumerator linear_float_realtime(float start, float end, float time,Action<float> act )
    {
        float s = 0.0f;
        float speed = 1 / time;

        float saveTime = Time.realtimeSinceStartup;
        while (s < 1f)
        {
            float pos   = Mathf.Lerp(start,end,s);

            act(pos);
 
            yield return null;

            float diffTime = Time.realtimeSinceStartup - saveTime; saveTime = Time.realtimeSinceStartup;
            s += diffTime * speed;
        }
        act(end);
    }


    public static IEnumerator linear_Vector2(Vector2 start, Vector2 end, float time, Action<Vector2> act, MonoBehaviour mono)
    {
        float vx=0;
        mono.StartCoroutine(linear_float(start.x,end.x,time,(v)=>{vx = v;}));
        yield return mono.StartCoroutine(linear_float(start.y,end.y,time,(vy)=>{act(new Vector2(vx,vy));}));
    }

    public static IEnumerator linear_Vector3(Vector3 start, Vector3 end, float time, Action<Vector3> act)
    {
        float s = 0.0f;
        float speed = 1 / time;
        while (s < 1f)
        {
            Vector3 pos   = Vector3.Lerp(start,end,s);

            act(pos);
 
            yield return new WaitForEndOfFrame();

            s += Time.deltaTime * speed;
        }

        act(end);
    }

    public static IEnumerator linear_Transform(Transform start,Transform end, float time, Action<Vector3,Quaternion> act)
    {
        Vector3 startPos = start.position;
        Vector3 endPos   = end.position;
 
        Quaternion startQt = start.rotation;
        Quaternion endQt   = end.rotation;

        float s = 0.0f;
        float speed = 1 / time;
        while (s < 1f)
        {
            Vector3 pos   = Vector3.Lerp(startPos,endPos,s);
            Quaternion qt = Quaternion.Lerp(startQt,endQt,s);

            act(pos,qt);
 
            yield return null;

            s += Time.deltaTime * speed;
        }

        act(endPos,endQt);
    }

    public static IEnumerator linear_Quatanion(Quaternion start, Quaternion end, float time, Action<Quaternion> act)
    {
        float s = 0.0f;
        float speed = 1 / time;

        while (s < 1f)
        {
            Quaternion qt = Quaternion.Lerp(start,end,s);
            act(qt);
            yield return null;
            s += Time.deltaTime * speed;
        }

        act(end);
    }
}
