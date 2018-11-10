using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class hglResourceMan : MonoBehaviour {

    public Hashtable m_table;

    public enum FILEKIND { UNKNOWN, TEXT, TEXTURE}

    public class FILEDATA
    {
        public string name;
        public string fullname;

        public object obj;
        public int    size;
        public float  latestUsed;

        public bool      isNet;
        public FILEKIND  kind;
    }

    public FILEDATA Register(string name, string fullname)
    {
        if (m_table==null) m_table = new Hashtable();
        if (!m_table.ContainsKey(fullname))
        {
            var fd = new FILEDATA()
            {
                name = name,
                fullname = fullname,
                kind = checkKindOf(fullname),
                isNet = checkIsNet(fullname)
            };
            m_table[fullname] = fd;

            if (!fd.isNet)
            {
                ReadLocal(fd);
            }
            return fd;
        }
        else
        {
            return (FILEDATA)m_table[fullname];
        }
    }

    public void Register(string name, string fullname, string text)
    {
        if (m_table==null) m_table = new Hashtable();
        m_table[fullname] = new FILEDATA() { 
            name     = name, 
            fullname = fullname, 
            kind     = FILEKIND.TEXT,
            isNet    = false,
            obj      = text, 
            size     = text.Length};
    }
    
    private FILEKIND checkKindOf(string file)
    {
        string[] txtExts = new string[] { ".html",".htm",".txt",".js",".css"  };
        string[] bmpExts = new string[] { ".png",".jpg",".gif" };
        
        Func<string,bool> checkTxt = (s) => {
            var ext = hglEtc.get_fileTail(s);
            if (Array.IndexOf(txtExts,ext)>=0) return true;
            return false;
        };

        Func<string,bool> checkBmp = (s) => {
            var ext = hglEtc.get_fileTail(s);
            if (Array.IndexOf(bmpExts,ext)>=0) return true;
            return false;
        };

        if (checkTxt(file)) return FILEKIND.TEXT;
        if (checkBmp(file)) return FILEKIND.TEXTURE;

        return FILEKIND.UNKNOWN;
    }

    private bool checkIsNet(string file)
    {
        return file.Contains("http:") || file.Contains("https:");
    }
    private void ReadLocal(FILEDATA fd)
    {
        if (fd.obj!=null) return;

        if (fd.kind == FILEKIND.TEXT)
        {
            var readFileText = hglUtil.ReadTextFile.Create(fd.fullname);
            string src = "unknown";
            if (readFileText.m_error != null || readFileText.m_text == null)
            {
                if (readFileText.m_error != null)
                {
                    src = "<p>" + readFileText.m_error + "</p>";
                }
            }
            else
            {
                src = readFileText.m_text;
            }                
            fd.obj = src;
            fd.size = src.Length;
        }
        else if (fd.kind == FILEKIND.TEXTURE)
        {
            var readTexture = hglUtil.ReadTextureFile.Create(fd.fullname);
            if (readTexture.m_error != null || readTexture.m_texture == null)
            {
                fd.obj = null;
            }
            else
            {
                fd.obj  =  readTexture.m_texture;
                fd.size =  readTexture.m_texture.width * readTexture.m_texture.height; // Assumed 
            }
        }
    }
    public bool CheckHasNetFiles()
    {
        if (m_table!=null) foreach (var k in m_table.Keys)
        {
            FILEDATA fd = (FILEDATA)m_table[k];
            if (fd.isNet && fd.obj==null) return true;
        }
        return false;
    }
    public IEnumerator ReadAllNet(MonoBehaviour mono)
    {
        foreach (var k in m_table.Keys)
        {
            FILEDATA fd = (FILEDATA)m_table[k];
            Debug.Log("1>>>" + fd.fullname);
            if (fd.isNet== false) continue;
            if (fd.obj  != null)  continue;
            if (fd.kind == FILEKIND.TEXT)
            {
                Debug.Log("text>>>" + fd.fullname);
                var readFileText = hglUtil.ReadTextFile.Create(fd.fullname);
                if (readFileText.m_bHttp) yield return mono.StartCoroutine(readFileText.Read(mono));
                string src = "unknown";
                if (readFileText.m_error != null || readFileText.m_text == null)
                {
                    if (readFileText.m_error != null)
                    {
                        hglUtil.Log(readFileText.m_error);
                        src = "<p>" + hglUtil.GetLog() + "</p>";
                    }
                }
                else
                {
                    src = readFileText.m_text;
                }
                
                fd.obj = src;
                fd.size = src.Length;
            }
            else if (fd.kind == FILEKIND.TEXTURE)
            {
                Debug.Log("texture>>>" + fd.fullname);
                var readTexture = hglUtil.ReadTextureFile.Create(fd.fullname);
                if (readTexture.m_bHttp) yield return mono.StartCoroutine(readTexture.Read(mono));
                if (readTexture.m_error != null || readTexture.m_texture == null)
                {
                    fd.obj = null;
                }
                else
                {
                    fd.obj  =  readTexture.m_texture;
                    fd.size =  readTexture.m_texture.width * readTexture.m_texture.height; // Assumed 
                }
            }
        }
    }

    public IEnumerator ReadNet(FILEDATA fd, MonoBehaviour mono)
    {
        if (fd.kind == FILEKIND.TEXT)
        {
            var readFileText = hglUtil.ReadTextFile.Create(fd.fullname);
            if (readFileText.m_bHttp) yield return mono.StartCoroutine(readFileText.Read(mono));
            string src = "unknown";
            if (readFileText.m_error != null || readFileText.m_text == null)
            {
                if (readFileText.m_error != null)
                {
                    hglUtil.Log(readFileText.m_error);
                    src = "<p>" + hglUtil.GetLog() + "</p>";
                }
            }
            else
            {
                src = readFileText.m_text;
            }
                
            fd.obj = src;
            fd.size = src.Length;
        }
        else if (fd.kind == FILEKIND.TEXTURE)
        {
            var readTexture = hglUtil.ReadTextureFile.Create(fd.fullname);
            if (readTexture.m_bHttp) yield return mono.StartCoroutine(readTexture.Read(mono));
            if (readTexture.m_error != null || readTexture.m_texture == null)
            {
                fd.obj = null;
            }
            else
            {
                fd.obj  =  readTexture.m_texture;
                fd.size =  readTexture.m_texture.width * readTexture.m_texture.height; // Assumed 
            }
        }
    }

#if XX
    public IEnumerator ReadAll(MonoBehaviour mono)
    {
        string[] txtExts = new string[] { "html","htm",".txt",".js",".css"  };
        string[] bmpExts = new string[] { ".png",".jpg",".gif" };
        
        Func<string,bool> checkTxt = (s) => {
            var ext = hglEtc.get_fileTail(s);
            foreach(var w in txtExts) if (ext==w) return true;
            return false;
        };

        Func<string,bool> checkBmp = (s) => {
            var ext = hglEtc.get_fileTail(s);
            foreach(var w in bmpExts) if (ext==w) return true;
            return false;
        };

        if (m_table!=null) foreach (var k in m_table.Keys)
        {
            var fd = (FILEDATA)m_table[k];
            if (fd.obj!=null) continue;

            if (checkTxt(fd.fullname))
            {
                var readFileText = hglUtil.ReadTextFile.Create(fd.fullname);
                if (readFileText.m_bHttp) yield return mono.StartCoroutine(readFileText.Read(mono));
                string src = "unknown";
                if (readFileText.m_error != null || readFileText.m_text == null)
                {
                    if (readFileText.m_error != null)
                    {
                        hglUtil.Log(readFileText.m_error);
                        src = "<p>" + hglUtil.GetLog() + "</p>";
                    }
                }
                else
                {
                    src = readFileText.m_text;
                }
                
                fd.obj = src;
                fd.size = src.Length;
            }
            else if (checkBmp(fd.fullname))
            {
                var readTexture = hglUtil.ReadTextureFile.Create(fd.fullname);
                if (readTexture.m_bHttp) yield return mono.StartCoroutine(readTexture.Read(mono));
                if (readTexture.m_error != null || readTexture.m_texture == null)
                {
                    fd.obj = null;
                }
                else
                {
                    fd.obj  =  readTexture.m_texture;
                    fd.size =  readTexture.m_texture.width * readTexture.m_texture.height; // Assumed 
                }
            }
        }
        yield return null;
    }
    public bool ReadAll2()
    {
        string[] txtExts = new string[] { "html","htm",".txt",".js",".css"  };
        string[] bmpExts = new string[] { ".png",".jpg",".gif" };
        
        Func<string,bool> checkTxt = (s) => {
            var ext = hglEtc.get_fileTail(s);
            foreach(var w in txtExts) if (ext==w) return true;
            return false;
        };

        Func<string,bool> checkBmp = (s) => {
            var ext = hglEtc.get_fileTail(s);
            foreach(var w in bmpExts) if (ext==w) return true;
            return false;
        };

        if (m_table!=null) foreach (var k in m_table.Keys)
        {
            var fd = (FILEDATA)m_table[k];
            if (fd.obj!=null) continue;

            if (checkTxt(fd.fullname))
            {
                var readFileText = hglUtil.ReadTextFile.Create(fd.fullname);
                if (readFileText.m_bHttp) yield return mono.StartCoroutine(readFileText.Read(mono));
                string src = "unknown";
                if (readFileText.m_error != null || readFileText.m_text == null)
                {
                    if (readFileText.m_error != null)
                    {
                        hglUtil.Log(readFileText.m_error);
                        src = "<p>" + hglUtil.GetLog() + "</p>";
                    }
                }
                else
                {
                    src = readFileText.m_text;
                }
                
                fd.obj = src;
                fd.size = src.Length;
            }
            else if (checkBmp(fd.fullname))
            {
                var readTexture = hglUtil.ReadTextureFile.Create(fd.fullname);
                if (readTexture.m_bHttp) yield return mono.StartCoroutine(readTexture.Read(mono));
                if (readTexture.m_error != null || readTexture.m_texture == null)
                {
                    fd.obj = null;
                }
                else
                {
                    fd.obj  =  readTexture.m_texture;
                    fd.size =  readTexture.m_texture.width * readTexture.m_texture.height; // Assumed 
                }
            }
        }

    }
#endif

    public string GetText(string file)
    {
        if (m_table==null) return null;
        var v  = m_table[file];
        if (v != null)
        { 
            var fd = (FILEDATA)v;
            if (fd!=null) return (string)fd.obj;
        }
        return null;
    }

    public Texture GetTexture(string file)
    {
        if (m_table == null) return null;
        var v  = m_table[file];
        if (v != null)
        { 
            var fd = (FILEDATA)v;
            if (fd!=null) return (Texture)fd.obj;
        }
        return null;
    }
}
