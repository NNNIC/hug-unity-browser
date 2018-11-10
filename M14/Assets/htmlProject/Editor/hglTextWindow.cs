using UnityEngine;
using UnityEditor;
using System.Collections;

public class hglTextWindow : ScriptableWizard {

    public hglWindowInfo m_wininfo;
    string  text;

    static hglTextWindow win;

    public static void CreateWizard(hglWindowInfo wininfo)
    {
        if (win!=null) return;
        win = ScriptableWizard.DisplayWizard<hglTextWindow>("Edit Text","Apply");
        win.m_wininfo = wininfo;
        win.text = new string(wininfo.m_text.ToCharArray()); 
    }

    void OnGUI()
    {
        if (GUILayout.Button("Apply")) { 
            m_wininfo.m_text = new string(text.ToCharArray());
            Close();
        }
        text = EditorGUILayout.TextArea(text);
    }

    void OnDestroy()
    {
        win = null;
    }

}
