//using UnityEngine;
using System.Collections;
using System;

public class hglCustomReader {
    
    public enum TYPE
    {
        NONE,
        NODE_ENTER_EXIT,
        NODE_ENTER,
        NODE_EXIT,
 
        COMMENT,

        TEXT,
    }

    public static void Read(string src, Action<TYPE,string, Hashtable, string> act)
    {
        int index = 0;
        while(index < src.Length)
        {
            bool bInComment = false;
            int lt=-1,gt=-1;

            int clt = src.IndexOf("<!--",index);
            lt = src.IndexOf('<',index); if (lt < 0) break;
           
            if (clt == lt)
            {
                bInComment = true;
                gt = src.IndexOf("-->",lt);
            }
            else 
            {
                gt = src.IndexOf('>',lt);    if (lt < 0) break;
            }
            var text =  (lt - index >= 1 ) ? src.Substring(index,lt - index) : null;

            if (string.IsNullOrEmpty(text)==false) 
            {
                act(TYPE.TEXT, text, null,null);
            }

            if (gt-lt-1 <=0 || lt+1 >= src.Length) break;
            var nodestr = src.Substring(lt+1, (gt-lt-1));

            if (bInComment)
            {
                text = nodestr.Substring(3);
                if (text.Length > 0)
                {
                    //Debug.Log(text);
                    //text = text.Substring(0, text.Length - 3);
                    //Debug.Log(text);
                }
                else
                {
                    text =null;
                }

                if (string.IsNullOrEmpty(text)==false)
                {
                    act(TYPE.COMMENT, text,null,null);
                }
            }
            else 
            {
                if (nodestr[0]=='/')
                {
                    string node;
                    Hashtable hash;
                    parse_nodestr(nodestr.Substring(1),out node, out hash);     

                    act(TYPE.NODE_EXIT,node,null,nodestr);
                }
                else
                {
                    string node;
                    Hashtable hash;
                    parse_nodestr(nodestr,out node, out hash);     
                    if (src[gt-1]=='/') 
                    {
                        act(TYPE.NODE_ENTER_EXIT,node,hash,nodestr);
                    }
                    else
                    {
                        act(TYPE.NODE_ENTER,node,hash,nodestr);
                    }
                }
            }

            if (bInComment)
            {
                index = gt+3;
            }
            else
            {
                index = gt+1;
            }
        }
    }

    static void parse_nodestr(string str,out string node, out Hashtable hash)
    {
        const int mode_none = 0;
        const int mode_nodename_in  = 1;
        const int mode_nodename_out = 2;
        const int mode_attr_in      = 3;
        const int mode_attr_out     = 4;
        const int mode_attrval_in   = 5;
        const int mode_attrval_out  = 6;

        node = null;
        hash = new Hashtable();

        int mode = mode_none;

        int index = 0;
        string attr="";
        string val="";
        bool   isInDQ = false;
        while(index < str.Length)
        {
            var c = str[index];
            if (mode == mode_none)
            {
                if (c<=0x20) goto _LOOPLAST;
                mode = mode_nodename_in;
                node = new string(c,1);
                goto _LOOPLAST;
            }
            if (mode == mode_nodename_in)
            {
                if (c<=0x20) 
                {
                    mode = mode_nodename_out;
                    goto _LOOPLAST;
                }
                if (c!='/') 
                {
                    node += c;
                }
                goto _LOOPLAST;
            }
            if (mode == mode_nodename_out)
            {
                if (c<=0x20) goto _LOOPLAST;
                mode = mode_attr_in;
                attr = new string(c,1);
                goto _LOOPLAST;
            }
            if (mode == mode_attr_in)
            {
                if (c=='=') 
                {
                    mode = mode_attr_out;
                    goto _LOOPLAST;
                }
                if (c<0x20) goto _LOOPLAST;
                attr +=  c;
                goto _LOOPLAST;
            }
            if (mode == mode_attr_out)
            {
                if (c<=0x20) goto _LOOPLAST;
                mode = mode_attrval_in;
                if (c=='\"')
                {
                    isInDQ = true;
                    val = "";
                    goto _LOOPLAST;
                }
                val = new string(c,1);
                goto _LOOPLAST;
            }
            if (mode == mode_attrval_in)
            {
                if (isInDQ)
                {
                    if (c=='\"')
                    {
                        isInDQ = false;
                        hash[attr.ToLower()]=val;
                        mode = mode_attrval_out;
                        goto _LOOPLAST;
                    }

                    val += c;
                    goto _LOOPLAST;
                }

                if (c<=0x20) 
                {
                    mode = mode_attrval_out;
                    hash[attr]=val;
                    goto _LOOPLAST;
                }
                val += c;
                goto _LOOPLAST;
            }
            if (mode == mode_attrval_out)
            {
                if (c<=0x20) goto _LOOPLAST;
                mode = mode_attr_in;
                attr = new string(c,1);
                goto _LOOPLAST;
            }

        _LOOPLAST:
            index++;
        }

        if (mode == mode_attrval_in)
        {
            if (string.IsNullOrEmpty(val)==false)
            {
                hash[attr] = val;
            }
        }
    }











}
