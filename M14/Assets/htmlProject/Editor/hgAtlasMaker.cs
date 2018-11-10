using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using hug_u;


public class hgAtlasMaker : MonoBehaviour {

    [UnityEditor.MenuItem("Assets/Create Atlas")]
    static void CreateAtlas()
    {
        string path = Application.dataPath +"/htmlProject/_Data/Atlas/bmp0";

        List<Texture2D> texList = new List<Texture2D>();

        var files = (new System.IO.DirectoryInfo(path)).GetFiles();
        foreach (var f in files)
        {
                
            if (f.Extension == ".psd" || f.Extension == ".jpg" || f.Extension == ".png" || f.Extension == ".gif")
            {
                string path2 = "Assets/htmlProject/_Data/Atlas/bmp0/" + f.Name;
                //Debug.Log("Load=" + path2);
                Texture2D tex = (Texture2D)Resources.LoadAssetAtPath(path2, typeof(Texture2D));
                if (tex != null)
                { 
                    texList.Add((Texture2D)tex);
                    Debug.Log("tex=" + path2);
                }
            }
        }

        var size = 4096;
        var tex2d = new Texture2D(size, size);
        var rectlist = tex2d.PackTextures(texList.ToArray(), 1, size);
        tex2d.name = "atlas0";
        tex2d.Apply();

        using (var fs = new System.IO.FileStream(Application.dataPath + "/htmlProject/_Data/Atlas/" + tex2d.name + ".png", System.IO.FileMode.Create))
        {
            var bw = new System.IO.BinaryWriter(fs);
            bw.Write(tex2d.EncodeToPNG());
            bw.Close();
            fs.Close();
        }

        GameObject prefab = (GameObject)Resources.LoadAssetAtPath("Assets/htmlProject/_Data/Atlas/" + tex2d.name + ".prefab", typeof(GameObject));
        AssetDatabase.StartAssetEditing();

        {
            hgAtlasInfo ab = prefab.GetComponent<hgAtlasInfo>();
            hgAtlasInfoData ai = ab.m_data;

            List<hgAtlasInfoData.DATA> saveList = new List<hgAtlasInfoData.DATA>();
            if (ai!=null && ai.list!=null) foreach(var d in ai.list) saveList.AddRange(ai.list);
            Func<string, hgAtlasInfoData.DATA> find = (nm) =>{
                foreach (var d in saveList) if (d.texname == nm) return d;
                return null;
            };

            List<hgAtlasInfoData.DATA> list = new List<hgAtlasInfoData.DATA>();
            for(int i = 0;i<texList.Count;i++)
            {
                var t = texList[i];
                var r = rectlist[i];

                var old = find(t.name);
                var d = new hgAtlasInfoData.DATA(old);
                d.texname = t.name;
                d.rect    = r;
                
                list.Add(d);
            }
            ai.list      = list.ToArray();
            ai.atlasName = tex2d.name;
            ai.width     = tex2d.width;
            ai.height    = tex2d.height;
			ab.m_data    = ai;
        }

        AssetDatabase.StopAssetEditing();

        EditorUtility.SetDirty(prefab);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("INFO", "CREATED!", "OK");
    }   

}
