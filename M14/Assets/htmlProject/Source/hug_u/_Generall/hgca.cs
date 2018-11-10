/*

    CA: COMMON API

*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace hug_u {
public delegate void Function();

public class hgca : MonoBehaviour {

    /*
        ######################################
        # ACTIVATE AND DEACTIVATE GAMEOBJECT #
    */
    public static void ActivateAllChildren( GameObject o )
    {
        for (int i = 0; i < o.transform.childCount; i++)
        {
            o.transform.GetChild(i).gameObject.SetActiveRecursively(true);
        }
    }

    public static void DeactivateAllChildren( GameObject o)
    {
        for (int i = 0; i < o.transform.childCount; i++)
        {
            o.transform.GetChild(i).gameObject.SetActiveRecursively(false);
        }
    }

    public static void DestroyAllChildren( GameObject o)
    {
        if (o.transform.childCount==0) return;
        GameObject[] children = new GameObject[o.transform.childCount];
        for(int i =0;i<o.transform.childCount;i++) children[i] = o.transform.GetChild(i).gameObject;
        foreach(var c in children) Destroy(c);
    }
    /*
        # ACTIVATE AND DEACTIVATE GAMEOBJECT #
        ######################################
    */


    /*
        ###################
        # FIND GAMEOBJECT #  

        This API can find a GameObject by name or path name likely GameObject.Find.
        But GameObject.Find cannnot find an inactive GameObject. So, I made this API.

    */
    public static GameObject Find(string name) { return Find(null,name);}
    public static GameObject Find(GameObject root, string name)
    {
        bool bPathFormat = name.Contains("/");
        
        if (bPathFormat) 
        {
            bool bPathRoot = (bool)(name[0]=='/');
            Transform findobj = null;
    
            string tailName = null;
            {
                string[] namelist = name.Split('/');
                tailName = namelist[namelist.Length-1];
            }


            Action<Transform> a = (t) => {
                if (findobj!=null) return;
                
                if (t.name == tailName)
                {
                    string path = MakePath(t.gameObject);

                    if (bPathRoot)
                    {
                        if (name == path)
                        {
                            findobj = t;
                        }
                    }
                    else 
                    {
                        if (path.Contains(name)) 
                        {
                            findobj = t;
                        }
                    }
                }
            };

            if (root) 
            {
                hgca.TraverseGameObject(root.transform,a,0);
            }
            else
            {
                hgca.TraverseGameObject(a);
            }
            
            if (findobj!=null) 
            {
                return findobj.gameObject;
            }
        }
        else
        {
            Transform findobj = null;
            Action<Transform> a = (t) => {
               if (findobj!=null) return;
                if (t.name == name)
               {
                   findobj = t;
               }
            };

            if (root) 
            {
                hgca.TraverseGameObject(root.transform,a,0);
            }
            else
            {
                hgca.TraverseGameObject(a);
            }

            if (findobj!=null) 
            {
                return findobj.gameObject;
            }
        }

        Debug.Log("Warning : ca.Find cannot find obj. return null");

        return null;
    }
    /*
        # FIND GAMEOBJECT #
        ###################
    */

    /*
        ########################
        # Make GameObject Path #
    */
    public static string MakePath(GameObject obj)
    {
        if (obj==null)
        {
            Debug.LogError("Warning: MakePath obj is null");
            return "null";
        }
        string path = obj.name;
        Transform t = obj.transform;

        while(t.parent!=null)
        {
            path = t.parent.name + "/" + path;
            t = t.parent;
        }
        path = "/" + path;

        return path;
    }

    public static int GetDepth(GameObject obj)
    {
        int c = 0;
        Transform t = obj.transform;

        while (t != null)
        {
            t = t.parent;
            c++;
        }

        return c;
    }

    /*
        # Make GameObject Path #
        ########################
    */


    /*
        ############################################
        # Find Game Component by its name and type #
    */
    public static UnityEngine.Object Find(GameObject o, string name, Type type)
    {
        Component[]  objs  = o.GetComponentsInChildren(type,true);
        foreach(Component w in objs)
        {
            if (w.gameObject.name == name)
            {
                return w;
            }
        }
        Debug.LogError("ca.Find Error");
        return null;
    }
    /*
        # Find Game Component by its name and type #
        ############################################
    */
    
    /*
        ###################################
        # Find Game Component by its type #
    */
    public static UnityEngine.Object Find(GameObject o, Type type)
    {
        Component[]  objs  = o.GetComponentsInChildren(type,true);
        if (objs!=null && objs.Length>0 ) return objs[0];

        Debug.LogError("ca.Find Error");
        return null;
    }
    /*
        # Find Game Component by its type #
        ###################################
    */


    /*
        #############################
        # Find upper Game Component #
    */
    public static Component FindAscendantComponent(GameObject o, string type)
    {
        Transform t = o.transform;
        while(t.parent!=null)
        {
            Component c = t.parent.GetComponent(type);
            if (c!=null) return c;
            t = t.parent;
        }
        return null;
    }
    public static Component FindAscendantComponent(GameObject o, Type type)
    {
        Transform t = o.transform;
        while(t.parent!=null)
        {
            Component c = t.parent.GetComponent(type);
            if (c!=null) return c;
            t = t.parent;
        }
        return null;
    }
    /*
        # Find upper Game Component #
        #############################
    */
    /*
        #####################################
        # Find Ascendant GameObject by Name #
    */
    /*public static GameObject FindAscendantGameObjectByName(GameObject o, string name ,bool isContain=false)
    {
        Transform t = o.transform;
        while (t != null)
        {
            if (isContain)
            {
                if (t.name.Contains(name))
                {
                    return t.gameObject;
                }
            }
            else
            {
                if (t.name == name)
                {
                    return t.gameObject;
                }
            }
            t = t.parent;
        }
        return null;
    }
    */
    /*
        # Find Ascendant GameObject by Name #
        #####################################
    */






    public static Vector3 GetCenterInRect(Rect r)
    {
        Vector3 pos = new Vector3(
               r.x + r.width / 2.0f,
               r.y + r.height / 2.0f,
               0f
               );

        return pos;
    }

    public static Rect GetRectFromBounds(Bounds bounds)
    {
        Rect rect1       = new Rect(
        bounds.center.x -  bounds.extents.x, 
        bounds.center.y - bounds.extents.y, 
        bounds.size.x, bounds.size.y
        );
    
        return new Rect(rect1.x + Screen.width / 2  , rect1.y + Screen.height / 2, rect1.width, rect1.height);
    }

    // #####################
    // # GetGameObjectInfo #  For Debug
    class objInfo
    {
        string CR = "\n";

        public string     m_info;

        public objInfo(GameObject obj)
        {
            m_info  = "Name:" + obj.name   + CR;
            m_info += "Tag:"  + obj.tag    + CR;
            m_info += "Compos:" + GetComponetsInfo(obj)+ CR;
            m_info += "Ancestors:" + GetAncestors(obj) + CR;
            m_info += "Siblings:"  + GetSibilings(obj) + CR;
            m_info += "ChildCount:" + obj.transform.childCount + CR;
            m_info += "Children {"  + CR;
            if (obj.transform.childCount!=0) {
                string path = obj.name;
                GetChilds(obj.transform,path);
            }
            m_info += "}" + CR;
        }

        void GetChilds(Transform t, string path)
        {
            if (t.childCount==0)
            {
                m_info += "\t" + path + CR;
            }

           for(int i=0;i<t.childCount;i++)
           {
                path += "/" + t.GetChild(i).name;
               GetChilds(t.GetChild(i),path);
           }
        }
        string GetSibilings(GameObject obj)
        {
            string str = "";
            Transform parent = obj.transform.parent;
            for(int i=0;i<parent.childCount;i++)
            {
                str += parent.GetChild(i).name + ",";
            }
            return str;
        }
        string GetAncestors(GameObject obj)
        {
            string path = obj.name;
            Transform t = obj.transform.parent;
            while(t!=null)
            {
                path = t.name + "/" + path;
                t = t.parent;
            }
        
            return path;
        }

        string GetComponetsInfo(GameObject obj)
        {
            string dbgstr="";
            Component[] clist = obj.GetComponents<Component>();
            foreach(Component c in clist) dbgstr += c.GetType().Name + ",";

            return dbgstr;
        }
    }

    public static string GetGameObjectInfo(GameObject obj)
    {
        objInfo inf = new objInfo(obj);
        return  inf.m_info;
    }
    // # GetGameObjectInfo # 
    // #####################

    // ############################
    // # Traverese all GameObject #
    public static void TraverseGameObject(Action<Transform> action) {TraverseGameObject(null,action,0);}
    public static void TraverseGameObject(Transform root, Action<Transform> action, int nestNum) //if 'root' is null, it will traverse from the top level.
    {
        var rootList = new List<Transform>();
        
        if (root==null)
        {
            foreach(GameObject o in UnityEngine.Object.FindObjectsOfType(typeof(GameObject)))
            {
                if (o.transform.parent==null) rootList.Add(o.transform);
            }
        }
        else
        {
            rootList.Add(root);
        }

        int cntNest = 0;

        Action<Transform> travchildren = null;
        travchildren = (t) => 
        {
            if (nestNum > 0)
            {
                if (cntNest >= nestNum) return;
                cntNest++;
            }

            action(t);
            if (t.childCount==0) return;
            for(int i=0;i<t.childCount;i++)
            {
                travchildren(t.GetChild(i));
            }

            if (nestNum > 0)
            {
                cntNest--;
            }
        };

        rootList.ForEach(travchildren);
    }
    // # Traverese all GameObject #
    // ############################

    public static bool IsTouch(Renderer renderer, bool isMixMouse)
    {
        bool b = false;

        Rect rect = hgca.GetRectFromBounds(renderer.bounds);
        foreach(Touch t in Input.touches)
        {
            if (t.phase == TouchPhase.Began)
            {
                if (rect.Contains(t.position))
                {
                    b=true;
                    break;
                }
            }
        }

        if (isMixMouse) 
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (rect.Contains(Input.mousePosition))
                {
                    b=true;
                }
            }
        }

        return b;
    }

    public static bool IsTouch(Camera camera, GameObject obj, bool isMixMouse)
    {
        Vector2 position = Vector2.zero;

        if (Input.touchCount>0)
        {
            var touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                position = touch.position;
            }
        }

        if (position == Vector2.zero)
        {
            if (isMixMouse && Input.GetMouseButtonDown(0))
            {
                position = Input.mousePosition;
            }
        }

        if (position != Vector2.zero) 
        {
            var ray = camera.ScreenPointToRay(position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (obj == hit.collider.gameObject)
                {
                    return true;
                }
            }
        }

        return false;
        
    }


    public static bool IsTouch(Renderer renderer,Camera camera, bool isMixMouse)
    {
        bool b = false;
        Bounds realBounds = renderer.bounds;
        float  ratio      = Screen.height / (2 * camera.orthographicSize);
        Bounds normalBoudns;
        {
            Vector3 v_camera2boundCenter        = realBounds.center - camera.transform.position;
            normalBoudns  = new Bounds(v_camera2boundCenter * ratio,realBounds.size * ratio);
        }


        Rect rect = hgca.GetRectFromBounds(normalBoudns);
        foreach(Touch t in Input.touches)
        {
            if (t.phase == TouchPhase.Began)
            {
                if (rect.Contains(t.position))
                {
                    b=true;
                    break;
                }
            }
        }

        if (isMixMouse) 
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (rect.Contains(Input.mousePosition))
                {
                    b=true;
                }
            }
        }

        return b;
    }

    public static Color ColorParse(string col)
    {
        //S.Log("ColorParse " + col);
        var strings = col.Split(",()".ToCharArray());

        Color output;
        output.r = float.Parse(strings[1]);
        output.g = float.Parse(strings[2]);
        output.b = float.Parse(strings[3]);
        output.a = float.Parse(strings[4]);

        return output;
    }

    public static void Destroy(ref GameObject o)
    {
        if (Application.isEditor)
        {
            GameObject.DestroyImmediate(o);
        }
        else
        {
            GameObject.Destroy(o);
        }
        o=null;
    }

    public static bool checkRange(float min,float max, float x)
    {
        if (min < max)
        {
            return (bool)(min <= x && x <= max);
        }
        else
        {
            return (bool)(max <= x && x <= min);
        }
    }

    public static void changeLayer(GameObject o, LayerMask mask)
    {
        o.layer = mask;
        for (int i = 0; i < o.transform.childCount; i++)
        {
            changeLayer(o.transform.GetChild(i).gameObject,mask);
        }
    }

    public static Vector2 GetViewportPositon(Vector2 rawposition)
    {
        float x = rawposition.x / Screen.width;// xBugFix.V.ScreenWidth;
        float y = rawposition.y / Screen.height;// xBugFix.V.ScreenHight;

        return new Vector2(x,y);
    }

    public static bool IsPortrait()
    {
        return (bool)(Screen.height > Screen.width);
        //return (bool)(BugFix.V.ScreenHight > BugFix.V.ScreenWidth);
    }

    // ################
    // # Timer Action #
    public static void TimerAction(float time, Action action, MonoBehaviour mb)
    {
        TimerActionClass tac = new TimerActionClass();
        mb.StartCoroutine(tac.Flow(time,action));
    }
    class TimerActionClass
    {
        public IEnumerator Flow(float time, Action action)
        {
            yield return new WaitForSeconds(time);
            action();
        }
    }
    public static void TimerTickAction(int count, Action action, MonoBehaviour mb)
    {
        TimerTickActionClass tac = new TimerTickActionClass();
        mb.StartCoroutine(tac.Flow(count,action));
    }
    class TimerTickActionClass
    {
        public IEnumerator Flow(int count, Action action)
        {
            for(int i = 0; i<count; i++) yield return null;
            action();
        }
    }


    // # Timer Action #
    // ################

    public static Bounds GetCompoundRendererBounds(GameObject o)
    {
        Renderer[] list = o.GetComponentsInChildren<Renderer>();

        Bounds bounds = new Bounds();

        Array.ForEach(list,r=>{ bounds.Encapsulate(r.bounds); });

        return bounds; 
    }
}


}