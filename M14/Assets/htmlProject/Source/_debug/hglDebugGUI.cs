using UnityEngine;
using System.Collections;

public class hglDebugGUI : MonoBehaviour {

    public GUISkin m_skin;
    public hglReadHtml m_read;
    public bool isOn=true;

    void OnGUI()
    {
        if (!isOn) return;
        GUI.skin = m_skin;
        GUILayout.BeginArea(new Rect(Screen.width -150,0,150,450));
        {
            if (GUILayout.Button("SOURCE"))
            {
                m_read.BrowseSrc();
            }
            if (GUILayout.Button("VIEW AGAIN"))
            {
                m_read.BrowseAgain();
            }
            if (GUILayout.Button("BACK"))
            {
                m_read.BrowseBack();
            }
            if (GUILayout.Button("RESET"))
            {
                Application.LoadLevel(0);
            }
            
        }
        GUILayout.EndArea(); 
    }
}
