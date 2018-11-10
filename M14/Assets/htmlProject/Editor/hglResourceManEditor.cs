using UnityEngine;
using UnityEditor;
using System.Collections;


[CustomEditor(typeof(hglResourceMan))]
public class hglResourceManEditor : Editor {

    public override void  OnInspectorGUI()
    {
        hglResourceMan rm = (hglResourceMan)target;
        if (rm.m_table!=null)
        {
            string dt = "";
            foreach(var k in rm.m_table.Keys)
            {
                var fd = (hglResourceMan.FILEDATA)rm.m_table[k];
                if (fd!=null) dt += string.Format("{0}\n",fd.fullname);
            }
            EditorGUILayout.TextArea(dt);
        }
    }
}
