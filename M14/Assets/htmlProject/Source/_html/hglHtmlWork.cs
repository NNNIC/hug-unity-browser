using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using hug_u;

public class hglHtmlWork {


    public static string[] GetAllFilenames(hglParser.Element root )
    {
        string[] allowexts = new string[] { ".txt",".png",".css",".js",".gif",".jpg" };
        HashSet<string> files = new HashSet<string>();

        Action<string> check = (s)=> {
            foreach (var t in allowexts)
            {
                if (hglEtc.check_tail(s, t))
                {
                    files.Add(s);
                    break;
                }
            }
        };

        hglParser.TraverseHglWork(root,(e)=>{
            if (e.mode != hglParser.Mode.TAG) return;
            if (e.text == "link")
            {
                check((string)e.attrib["href"]);
                check((string)e.attrib["src"]);
            }
            else if (e.text == "img")
            {
                check((string)e.attrib["src"]);
            }
        });
        
        string[] ans = new string[files.Count];
        files.CopyTo(ans);

        return ans;
    }
}
