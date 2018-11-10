using UnityEngine;
using UnityEditor;
using System.Collections;


[CustomEditor(typeof(hglWindowInfo))]
public class hglWindowInfoEditor : Editor {

    public override void OnInspectorGUI()
    {
        hglWindowInfo hglt = (hglWindowInfo)target;  if (hglt==null) return;

        // Show Source
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Show Source:");
        hglt.m_showSrc = EditorGUILayout.Toggle(hglt.m_showSrc);
        EditorGUILayout.EndHorizontal();

        // Main Camera
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Main Camera:");
        hglt.m_mainCamera = (hglWebCamera)EditorGUILayout.ObjectField(hglt.m_mainCamera,typeof(hglWebCamera));
        EditorGUILayout.EndHorizontal();
        
        // Window Type
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Window Type:");
        hglt.m_winType = (hglWindowType)EditorGUILayout.EnumPopup(hglt.m_winType);
        EditorGUILayout.EndHorizontal();

        if (hglt.m_winType != hglWindowType.MAIN)
        {
            // Main Win Info
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Main Window Info:");
            hglt.m_mainWinInfo= (hglWindowInfo)EditorGUILayout.ObjectField(hglt.m_mainWinInfo,typeof(hglWindowInfo));
            EditorGUILayout.EndHorizontal();
        }

        //Rendering Order
        EditorGUILayout.BeginHorizontal();
        var s = "";
        if (hglt.m_winType == hglWindowType.MAIN) s = "1000+";
        else if (hglt.m_winType == hglWindowType.POPUP) s="4000+";
        GUILayout.Label("Rendering Order:" + s  );
        hglt.m_renderingOrder = EditorGUILayout.IntField(hglt.m_renderingOrder);
        EditorGUILayout.EndHorizontal();

        if (hglt.m_winType == hglWindowType.MAIN)
        {
            // Fixed Width
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Fixed Width");
            hglt.__fixedWidth = EditorGUILayout.FloatField(hglt.__fixedWidth);
            if (hglt.m_winType == hglWindowType.MAIN && hglt.m_mainCamera != null)
            {
                hglt.m_mainCamera.GetComponent<hglWebCamera>().fixedWidth = hglt.__fixedWidth;
            }
            EditorGUILayout.EndHorizontal();

            // Resource From
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Resource from:");
            hglt.m_ResourceFrom = (hglResourceFrom)EditorGUILayout.EnumPopup(hglt.m_ResourceFrom);
            EditorGUILayout.EndHorizontal();

            var from = hglConfig.GetResourceFrom(hglt.m_ResourceFrom);
            if (!string.IsNullOrEmpty(from)) GUILayout.Label("      (" + hglConfig.GetResourceFrom(hglt.m_ResourceFrom) + ")");

            // use Text
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Use Text?:");
            hglt.m_useText = EditorGUILayout.Toggle(hglt.m_useText);
            EditorGUILayout.EndHorizontal();
        }
        else //if (hglt.m_winType == hglWindowType.POPUP)
        {
            hglt.m_useText = true;   
        }

        if (hglt.m_useText)
        {
            EditorGUILayout.BeginHorizontal();
            var s1 = string.IsNullOrEmpty(hglt.__urlText) ? "": hglt.__urlText;
            GUILayout.Label("path: " + s1);            
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Edit Text"))
            {
                hglTextWindow.CreateWizard(hglt);
            }
            if (Application.isPlaying && hglt.gameObject.activeSelf==true)
            {
                if (GUILayout.Button("Update"))
                {
                    hglt.gameObject.BroadcastMessage("Browse",hglt.__urlText);
                }
            }
        }
        else
        { 
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("path:");
            var s1 = string.IsNullOrEmpty(hglt.__url) ? "": hglt.__url;
            hglt.__url = EditorGUILayout.TextField(s1);
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.Separator();
        if (!string.IsNullOrEmpty(hglt.m_curUrl))
        { 
            GUILayout.Label("Base:" + hglt.m_curBaseUrl);
            GUILayout.Label("File:" + hglt.m_curUrlShort);
            GUILayout.Label("Para:" + hglt.m_curUrlParams);
        }
    }
}
