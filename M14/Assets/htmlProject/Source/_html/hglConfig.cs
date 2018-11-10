using UnityEngine;
using System.Collections;


public enum hglResourceFrom
{
    //NONE,
    NET,
    RESOURCES,
    //ASSETS_WEB_FOLDER,
    FILE_FOLDER,
    LOCALHOST,
}

public class hglConfig : MonoBehaviour {

    public string resurl = "Web/";
    public string neturl ="https://dl.dropboxusercontent.com/u/24585365/Web/";   // =          "https://dl.dropbox.com/u/265/";
    public string fileFolder;// = "/User/XX/";
    public string localhosturl = "Web/";

    public static hglConfig V;

    void Awake()
    {
        V = this;
    }

    public static string GetResourceFrom(hglResourceFrom from)
    {
        if (V==null) return "";
        switch(from)
        {
        case hglResourceFrom.NET:         return V.neturl;
        case hglResourceFrom.RESOURCES:   return Application.dataPath+"/Resources/"+V.resurl;
        case hglResourceFrom.FILE_FOLDER: return V.fileFolder;
        case hglResourceFrom.LOCALHOST:   return "http://localhost/" + V.localhosturl;
        }

        throw new System.SystemException("Unexpected");

    }
}
