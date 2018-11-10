using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
//using System.Xml;
using System;

public class hglParserScript : MonoBehaviour {
    public static string ParserScript(string src, Func<string, string> runScript)
    {
        string tag="";
        string o = "";
        hglCustomReader.Read(src,(t,text,hash,dbg)=>{
            switch(t)
            {
                case hglCustomReader.TYPE.NODE_ENTER:
                case hglCustomReader.TYPE.NODE_ENTER_EXIT:
                if (text == "script") { tag = "script"; break;}
                    tag="";
                    o += "<" + text + " ";
                    if (hash!=null)
                    {
                        foreach(var k in hash.Keys)
                        {
                            o += k + "= \"" + hash[k] + "\"  ";
                        }
                    }
            
                    if (t == hglCustomReader.TYPE.NODE_ENTER_EXIT)
                    {
                        o += " />";
                    }
                    else
                    {
                        tag=text;
                        o += ">";
                    }
                    break;
                case hglCustomReader.TYPE.NODE_EXIT:
                    if (text=="script") { tag=""; break;}
                    o += "</"+text+">";     
                    tag="";   
                    break;
                case hglCustomReader.TYPE.TEXT:
                    if (tag == "script")
                    {
                        var nt = hglEtc.NormalizeText(text);
                        if (!string.IsNullOrEmpty(nt))
                        { 
                            o += "<!--" + nt + " -->";
                            var a = runScript(nt);
                            o += a;
                        }
                    }
                    else
                    { 
                        o += text;
                    }
                    break;
                case hglCustomReader.TYPE.COMMENT:
                    o += "<!--" + text + " -->";
                    if (tag == "script")
                    {
                        var a = runScript(text);
                        o+=a;
                    }
                    break;
            }
        });
        return o;
    }
}
