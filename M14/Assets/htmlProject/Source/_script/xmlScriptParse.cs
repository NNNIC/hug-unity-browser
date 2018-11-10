using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using xsr;

public class xmlScriptParse {

    string[] m_wlist;
    GROUP[]  m_glist;

    // #######
    // # SET #
    public void Set(string istr)
    {
        //Debug.Log("istr=" + istr);
        var str = OutComment(istr);
        //Debug.Log("str=" + str);
        //UnityEngine.Debug.LogError(xmlEtc.NormalizeText2(istr));
        //UnityEngine.Debug.LogError(xmlEtc.NormalizeText2(str));
        BreakUp(str);
        CheckGroup();
    }

#if OBSOLATED
    void BreakUp(string str)
    {
        List<string> list1 = new List<string>();
        {
            Func<char,bool> isChar=(c)=>{ return (char.IsLetterOrDigit(c) || c=='_'); };
            Action<string>  Add=(s)=>{ if (!string.IsNullOrEmpty(s)) list1.Add(s);     };
            const int mode_none = 0;
            const int mode_word = 1;
            const int mode_quote= 2;

            int index = 0;
            int mode = mode_none;
            string w = null;
            for(index=0; index < str.Length; index++)
            {
                var c = str[index];
                if (mode == mode_none)
                {
                    if (isChar(c))    { w = "" + c;    mode = mode_word;  }
                    else if (c=='\"') { w = "\"";      mode = mode_quote; }
                    else if (c > ' ') { Add("" + c);                      }
                }
                else if (mode == mode_word)
                {
                    if (isChar(c)) { w += c; }
                    else {
                        Add(w); w=null;
                        if (c == '\"') { w = "\"";      mode = mode_quote; }
                        else {
                            Add(w); w=null;
                            if (c > ' ') { Add("" + c); }
                            mode = mode_none;
                        }
                    }
                }
                else if (mode == mode_quote)
                {
                    if (c == '\"')
                    {
                        Add(w);
                        mode = mode_none;
                    }
                    else
                    {
                        w += c;
                    }
                }
            }
        }
        m_wlist = list1.ToArray();
    }
#endif

    string OutComment(string str)
    {
        var s = "";
        var lines = str.Split('\x0d','\x0a');
        foreach (var l in lines)
        {
            bool bInComment = false;
            for (int i = 0; i < l.Length; i++)
            {
                var c0 = i >= 0          ? l[i]   : '\x00';
                var c1 = i+1 < l.Length ? l[i+1] : '\x00';

                if (c0<0x20) continue;

                if (c0 == '/' && c1 == '/')
                {
                    break;
                }
                if (bInComment == false)
                {
                    if (c0 == '/' && c1 == '*')
                    {
                        bInComment = true;
                        continue;
                    }
                    s+=c0;
                }
                else
                {
                    if (c0 == '*' && c1 == '/')
                    {
                        bInComment = false;
                        i++;
                    }
                    continue;
                }
            }
            s += "\n";
        }
        return s;
    }

    void BreakUp(string str)
    {
        string[] SpecialWord_Double = new string[]
        {
            "++","--","+=","-=",
            "==","<=",">=","!=",
            "&&","||",">>","<<"
        };

        List<string> list1 = new List<string>();
        {
            Func<char,bool> isChar=(c)=>{ return (char.IsLetterOrDigit(c) || c=='_');                       };
            Action<string>  Add=(s)=>{ if (!string.IsNullOrEmpty(s)) list1.Add(s);                          };
            Func<int,char>  strc=(i)=> { return (i<str.Length ? str[i] : '\x00');                           };
            Func<char,char,string,bool> _check2c=(c0,c1,s)=>{ return (s.Length==2 && c0==s[0] && c1==s[1]); };
            Func<char,char,bool> check2c=(c0,c1)=>
            {
                string wd=""+c0+c1;
                foreach(var i in SpecialWord_Double) 
                {
                    if (i==wd) return true;
                }
                return false;
            };
            Func<char,bool> checkHex=(c)=>{
                var n = char.ToLower(c);
                if (  (n >= '0' && n<='9') || (n>='a' && n<='f') )
                {
                    return true;
                }
                return false;
            };

            const int mode_none = 0;
            const int mode_word = 1;
            const int mode_quote= 2;

            int index = 0;
            int mode = mode_none;
            char quote = '\x00';  // For Dobule Quote and Single Quote
            string w = null;
            for(index=0; index < str.Length;)
            {
                Func<char[],char> checkEscape=(c)=>{
                    if (c[0]!='\\') return '\xffff';
                    switch(c[1])
                    {
                    case 'n':  index+=2; return '\n';
                    case 't':  index+=2; return '\t';
                    case 'b':  index+=2; return '\b';
                    case 'r':  index+=2; return '\r';
                    case 'f':  index+=2; return '\f';
                    case '\\': index+=2; return '\\';
                    case '\'': index+=2; return '\'';
                    case '\"': index+=2; return '\"';
                    }
                    if (
                        c[1] >= '0' && c[1] <= '8'
                        &&
                        c[2] >= '0' && c[2] <= '8'
                        &&
                        c[3] >= '0' && c[3] <= '8'
                        )
                    {
                        int n0 = (int)(c[1]-'0');
                        int n1 = (int)(c[2]-'0');
                        int n2 = (int)(c[3]-'0');

                        index+=4;
                        return (char)(n0 * 64 + n1 * 8 + n2);
                    }
                    if (c[1] == 'x' && checkHex(c[2]) && checkHex(c[3]))
                    {
                        string s = c[2].ToString() + c[3].ToString();
                        int x;
                        if (int.TryParse(s, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out x))
                        {
                            index+=4;
                            return (char)x;
                        }
                    }
                    if (c[1] == 'u' && checkHex(c[2]) && checkHex(c[3]) && checkHex(c[4]) && checkHex(c[5]))
                    {
                        string s = c[2].ToString() + c[3] + c[4] + c[5];
                        int x;
                        if (int.TryParse(s, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out x))
                        {
                            index+=6;
                            return (char)x;
                        }
                    }
            
                    return '\xffff';
                }; 

                var c0 = strc(index);
                var c1 = strc(index+1);
                var c2 = strc(index+2);
                var c3 = strc(index+3);
                var c4 = strc(index+4);
                var c5 = strc(index+5);
                char[] cs = new char[]{c0,c1,c2,c3,c4,c5};

                if (mode == mode_quote)
                {
                    if (c0 == quote)
                    {
                        Add(w); w=null;
                        mode = mode_none;
                        index++;
                        continue;
                    }
                    
                    var xc = checkEscape(cs);
                    if (xc!='\xffff')
                    {
                        w += xc;  
                        continue;
                    }
                    w += c0;
                    index++;
                    continue;
                }
                
                if (isChar(c0))
                {
                    if (mode == mode_word)
                    {
                        w+=c0;
                        index++;
                        continue;
                    }
                    else // mode_none
                    {
                        mode = mode_word;
                        w = ""+c0;
                        index++;
                        continue;
                    }
                }

                if (mode == mode_word)
                {
                    Add(w);
                    w=null;
                    mode = mode_none;
                    continue;
                }

                if (c0 == '\"' || c0 =='\'')
                {
                    w = "\"";
                    mode = mode_quote;
                    quote = c0;
                    index++;
                    continue;
                }

                if (check2c(c0,c1)) 
                {
                    Add(""+c0+c1);
                    index+=2;
                    continue;
                }
                if (c0 > ' ')
                { 
                    Add(""+c0);
                    index++;
                    continue;
                }
                index++;
                continue;
            }
        }
        List<string> list2 = new List<string>();
        {
            Func<string,bool> isDigit = (v)=>{
                foreach(var c in v) if (!char.IsDigit(c)) return false;
                return true;            
            }; 
            Func<int, string> strc=(i)=>{    
                return (i < list1.Count ? list1[i] : "");
            };

            int ix = 0;
            while (ix < list1.Count)
            {
                var w1 = strc(ix);
                var w2 = strc(ix+1);
                var w3 = strc(ix+2);

                if (isDigit(w1) && w2 == "." && isDigit(w3))
                {
                    list2.Add(w1 + w2 + w3);
                    ix += 3;
                }
                else
                {
                    list2.Add(w1);
                    ix += 1;
                }
            }
        }

        m_wlist = list2.ToArray();
    }


    void CheckGroup()
    {
        Func<string,GROUP> check=(w1)=>{ 
            if (string.IsNullOrEmpty(w1)) return GROUP.UNKNOWN;
            var c = w1[0];
            if ( char.IsLetter(c) || c=='_') return GROUP.NAME;
            if (char.IsDigit(c))
            {
                bool b = true;
                foreach(var c1 in w1) if ( (c1=='.' || char.IsDigit(c1))==false) b=false;
                if (b) return GROUP.NUMBER;
            }
            if (c == '"') return GROUP.QUOTE;

            if (w1.Length==1) return GROUP.SPECIALCHAR;
            return GROUP.SPECIALWORD;
        }; 

        // Phase 1
        m_glist = new GROUP[m_wlist.Length]; 
        for(int i = 0; i < m_wlist.Length; i++)
        {
            m_glist[i] = check(m_wlist[i]); 

            if (m_glist[i]== GROUP.QUOTE) m_wlist[i] = m_wlist[i].Substring(1);
        }
    }
    // # SET #
    // #######

    class ListElemet
    {
        public List<ELEMENT> list;
        public ListElemet()
        {
            list = new List<ELEMENT>();
        }
        public ELEMENT this[int i]
        {
            set { list[i] = value; }
            get { return list[i];  }
        }
        public int Count {get{return list.Count;}}
        public void Add(ELEMENT e) { list.Add(e);}
        public override string ToString()
        {
            string dt = "";
            foreach(var e in list) dt += e.ToString() + ",";
            return dt;
        }
        public string dbg_output
        {
            get { return ToString();}
        }
        
    }

    public ELEMENT Parse1()
    {
        //Action<List<ELEMENT>,string> dump = (l,mk) =>{
        //    if (l==null) return;
        //    var dt = ""; foreach(var d in l) dt += d + ",";
        //    Debug.Log(mk +"=" + dt);
        //};
        
        //#1
        var curList = new ListElemet();

        var e_pg = new ELEMENT(); e_pg.SetGroup(GROUP.PROGRAM,true);   curList.Add(e_pg);
        var e_st = new ELEMENT(); e_st.SetGroup(GROUP.SENTENCE,true);  curList.Add(e_st);

        for(int i = 0; i<m_wlist.Length; i++)
        {
            ELEMENT e = new ELEMENT() { raw = m_wlist[i], group = m_glist[i] };
            curList.Add(e);
        }
        
        //#2
        for(var i = 0; i<1000; i++)
        {            
            var cnt = 0;
            ListElemet newList;

            //Console.WriteLine(curList);
            //1st
            newList = checkElement(curList);
            if (newList != null) { curList = newList; continue; } else { cnt++; }

            newList = checkElement2(curList);
            if (newList != null) { curList = newList; continue; } else { cnt++; }

            newList = checkElement3(curList);
            if (newList != null) { curList = newList; continue; } else { cnt++; }

            //2nd
            newList = checkElement(curList);
            if (newList != null) { curList = newList; continue; } else { cnt++; }

            newList = checkElement2(curList);
            if (newList != null) { curList = newList; continue; } else { cnt++; }

            newList = checkElement3(curList);
            if (newList != null) { curList = newList; continue; } else { cnt++; }

            
            if (cnt==3*2) break;
        }
        //dump(curList,"[5]");
        var root = new ELEMENT() { group = GROUP.PARSEOUTPUT } ;
        root.list = curList.list;

        //Debug.Log("[6] =" + root);

        return root;

        //Console.WriteLine(m_stack.curList[0].ToString());
        //var sum = 0;for(int i = 0;i<1000;i++) sum+=i; 
    }
    ListElemet checkElement(ListElemet list)
    {
        ListElemet newList = new ListElemet();
        int cnt   = 0;
        int index = 0;
        while(index < list.Count)
        {
            var w = list[index]; // for debug
    
            bool b= false;

            // 1. Process if the word has a special meaning.
            if (!b) b=isNULL(ref index, list, newList);
            if (!b) b=isELSEIF(ref index, list, newList);
            if (!b) b=isELSE(ref index, list, newList);
            if (!b) b=isIF(ref index, list, newList);
            if (!b) b=isSWITCH(ref index, list, newList);
            if (!b) b=isCASE(ref index, list, newList);
            if (!b) b=isDEFAULT(ref index, list, newList);
            if (!b) b=isFOR(ref index, list, newList);
            if (!b) b=isWHILE(ref index, list, newList);
            if (!b) b=isBREAK(ref index,list,newList);
            if (!b) b=isCONTINUE(ref index,list,newList);
            if (!b) b=isRETURN(ref index,list,newList);
            if (!b) b=isNEW(ref index,list,newList);
            if (!b) b=isDELETE(ref index,list,newList);

            if (!b) b=isDEC_FUNC(ref index, list,newList);
            if (!b) b=isDEC_VAR(ref index, list,newList);


            if (!b) b=isBLOCK_C(ref index, list, newList);
            if (!b) b=isBLOCK_M(ref index, list, newList);
            if (!b) b=isBLOCK_L(ref index, list, newList);

            if (!b) b=isFUNCTION(ref index, list, newList);
            if (!b) b=isSENTENCE(ref index, list, newList);

            if (!b) b=isPROGRAM(ref index, list, newList);

            if (!b) b=isVALIABLE(ref index, list, newList);
            if (!b) b=isVALFUNCTION(ref index, list, newList);
            //if (!b) b=isPOINTER(ref index, list, newList);

            if (!b)
            { 
                isETC(ref index, list, newList);
                cnt++;
            }
        }
        return (cnt!=list.Count ? newList : null);
    }
    ListElemet checkElement2(ListElemet list)
    {
        ListElemet newList = new ListElemet();
        int cnt   = 0;
        int index = 0;
        while(index < list.Count)
        {
            var w = list[index]; // for debug
    
            bool b=isPOINTER(ref index, list, newList);

            if (!b)
            { 
                isETC(ref index, list, newList);
                cnt++;
            }
        }
        return (cnt!=list.Count ? newList : null);
    }

    ListElemet checkElement3(ListElemet list)
    {
        ListElemet newlist = null;

        if (newlist==null) newlist = _checkElement2Sub(list,convertOP_PLUS);
        if (newlist==null) newlist = _checkElement2Sub(list,convertOP_MINUS);
        if (newlist==null) newlist = _checkElement2Sub(list,convertOP_PLUSPLUS);
        if (newlist==null) newlist = _checkElement2Sub(list,convertOP_MINUSMINUS);
        if (newlist==null) newlist = _checkElement2Sub(list,convertOP_PLUSPLUS_BACK);
        if (newlist==null) newlist = _checkElement2Sub(list,convertOP_MINUSMINUS_BACK);

        if (newlist==null) newlist = _checkElement2Sub(list,convertMULTIPLY);            
        if (newlist==null) newlist = _checkElement2Sub(list,convertDIVIDE);            
        if (newlist==null) newlist = _checkElement2Sub(list,convertODD);
        if (newlist==null) newlist = _checkElement2Sub(list,convertADD);                 
        if (newlist==null) newlist = _checkElement2Sub(list,convertSUB);                 
        if (newlist==null) newlist = _checkElement2Sub(list,convertEQEQ);                 
        if (newlist==null) newlist = _checkElement2Sub(list,convertNOTEQ);                 
        if (newlist==null) newlist = _checkElement2Sub(list,convertLESSTHAN);
        if (newlist==null) newlist = _checkElement2Sub(list,convertLESSTHAN_EQ);
        if (newlist==null) newlist = _checkElement2Sub(list,convertGRATERTHAN);         
        if (newlist==null) newlist = _checkElement2Sub(list,convertGRATERTHAN_EQ);
        if (newlist==null) newlist = _checkElement2Sub(list,convertEQUAL);         
        
        return newlist;
    }

    ListElemet _checkElement2Sub(ListElemet list, Func<int,ListElemet,ListElemet,int > f )
    {
        int cnt = 0;
        ListElemet newList = new ListElemet();
        for (int index=0; index < list.Count; )
        {
            var newindex = f(index,list,newList);
            if (newindex == index)
            {
                newList.Add(list[index]);

                cnt++;
                index++;
            }
            else
            {
                index = newindex;
            }
        }
        return (cnt!=list.Count ? newList : null );
    }



    bool isPOINTER(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);
        if (!e[0].isPOINTER()) {
            //if (e[0].isPHRASE_DONE() && e[1].isSPECIALCHAR('.'))
            if (   
                (e[0].isVARIABLE_DONE() && e[1].isSPECIALCHAR('.'))
                ||
                (e[0].isFUNCTION_DONE() && e[1].isSPECIALCHAR('.'))
                ||
                (e[0].isQUOTE() && e[1].isSPECIALCHAR('.'))
                ||
                (e[0].isNUMBER() && e[1].isSPECIALCHAR('.'))
                ||
                (e[0].isBLOCK_C_DONE() && e[1].isSPECIALCHAR('.'))
            )
            {
                index += 2;
                var ne = new ELEMENT();
                ne.SetGroup(GROUP.POINTER,true);
                ne.Fix(e[0]);
                ne.AddUnkown();
                newlist.Add(ne);
                return true;
            }
        }
        else if (e[0].isPOINTER_NOTDONE())
        {
            if (e[1].isPOINTER_DONE() || e[1].isFUNCTION_DONE() || e[1].isVARIABLE_DONE())
            {
                index+=2;
                e[0].Fix(e[1]);
                newlist.Add(e[0]);
                return true;
            }
        }
        return false;
    }

    bool isFUNCTION(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);

        if (!e[0].isFUNCTION())
        {
            if (e[0].isNAME() && e[1].isSPECIALCHAR('('))
            {
                index += 1;
                e[0].SetGroup(GROUP.FUNCTION,true);
                newlist.Add(e[0]);
                return true;
            }
        }
        else if (e[0].isFUNCTION_NOTDONE())
        {
            if (e[1].isBLOCK_C_DONE())
            {
                index += 2;
                e[0].Fix(e[1]);
                newlist.Add(e[0]);
                return true;
            }
        }
        return false;
    }

    bool isDEC_FUNC(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);   // function name() {}

        if (!e[0].isDEC_FUNC())
        {
            if (e[0].isNAME("function"))
            {
                //e[1].assertNAME();
                //e[2].assertSPECIAL('(');  

                index += 2;
                e[0].SetGroup(GROUP.DEC_FUNC,true);
                e[0].decname = e[1].raw;

                newlist.Add(e[0]);
                return true;
            }
        }
        else if (e[0].isDEC_FUNC_NOTDONE())
        {
            if (e[1].isBLOCK_C_DONE() && e[2].isBLOCK_M_DONE())
            {
                index += 3;
                e[0].Fix(e[1]);
                e[0].Fix(e[2]);
                newlist.Add(e[0]);
                return true;
            }
        }
        return false;
    }
    bool isDEC_VAR(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);   // var name; or var name = xxxx; 

        if (!e[0].isDEC_VAR())
        {
            if (e[0].isNAME("var"))
            {
                //e[1].assertNAME();

                //ELEMENT.assert((e[2].isSPECIALCHAR(';') || e[2].isSPECIALCHAR(':') || e[2].isSPECIALCHAR('=')),"VAR ERROR");

                if (e[1].isNAME())
                { 
                    if (e[2].isSPECIALCHAR(';') || e[2].isSPECIALCHAR('='))
                    {
                        index+=2;
                        e[0].SetGroup(GROUP.DEC_VAR);
                        e[0].decname = e[1].raw;
                        newlist.Add(e[0]);
                        return true;
                    }
                    //if (e[2].isSPECIALCHAR(':') && e[3].hasUnkown()==false && e[4].isSPECIALCHAR(';'))
                    //{
                    //    //ELEMENT.assert(e[3].isNAME() && e[4].isSPECIALCHAR(';'),"VAR : DECREAR ERROR");
                    //    index+=4;
                    //    e[0].SetGroup(GROUP.DEC_VAR,true);
                    //    e[0].decname = e[1].raw;
                    //    e[0].Fix(e[3]);
                    //    newlist.Add(e[0]);
                    //    return true;
                    //}
                }
            }
        }
        //else if (e[0].isDEC_VAR_NOTDONE())
        //{
        //    ELEMENT.assert(false,"VAR : UNEXPECTED");
        //}
        return false;
    }

    bool isRETURN(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);

        if (!e[0].isRETURN())
        {
            if (e[0].isNAME())
            {
                if (e[0].isNAME("return") && e[1].isSPECIAL(";") )
                {
                    index++;
                    e[0].SetGroup(GROUP.RETURN);
                    newlist.Add(e[0]);
                    return true;
                }
                else if (e[0].isNAME("return"))
                {
                    index++;
                    e[0].SetGroup(GROUP.RETURN,true);
                    newlist.Add(e[0]);
                    return true;
                }
            }
        }
        else if (e[0].isRETURN_NOTDONE())
        {
            if (e[1].hasUnkown() == false && e[2].isSPECIAL(";"))
            {
                index+=2;
                e[0].Fix(e[1]);
                newlist.Add(e[0]);
                return true;
            }
        }
        return false;
    }

    bool isNEW(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);

        if (!e[0].isNEW())
        {
            if (e[0].isNAME("new"))
            {
                index++;
                e[0].SetGroup(GROUP.NEW,true);
                newlist.Add(e[0]);
                return true;
            }
        }
        else if (e[0].isNEW_NOTDONE())
        {
            if (e[1].hasUnkown() == false && e[2].isSPECIALCHAR(';'))
            {
                index+=2;
                e[0].Fix(e[1]);
                newlist.Add(e[0]);
                return true;
            }
        }
        return false;
    }
    bool isDELETE(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);

        if (!e[0].isDELETE())
        {
            if (e[0].isNAME("delete"))
            {
                index++;
                e[0].SetGroup(GROUP.DELETE,true);
                newlist.Add(e[0]);
                return true;
            }
        }
        else if (e[0].isDELETE_NOTDONE())
        {
            if (e[1].hasUnkown() == false && e[2].isSPECIALCHAR(';'))
            {
                index+=2;
                e[0].Fix(e[1]);
                newlist.Add(e[0]);
                return true;
            }
        }
        return false;
    }

    bool isIF(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);

        if (!e[0].isIF())
        {
            if (e[0].isNAME("if")) 
            {
                index++;
                e[0].SetGroup(GROUP.IF,true);
                newlist.Add(e[0]);
                return true;
            }
        }
        if (e[0].isIF_NOTDONE())
        {
            if (e[0].list.Count < 2)
            {
                if (e[1].isBLOCK_C_DONE())
                {
                    if (e[2].isBLOCK_M_DONE())
                    {
                        index += 3;
                        e[0].Fix(e[1]); // if (??) { xxx }
                        e[0].Fix(e[2]); //     e[1]  e[2]
                        e[0].AddUnkown();
                        newlist.Add(e[0]);
                        return true;
                    }
                    if (ELEMENT.isSENTENCE_1Line_CANDIDATE(e, 2))
                    {
                        index += 4;
                        var ne = new ELEMENT();
                        ne.SetGroup(GROUP.SENTENCE);
                        ne.Fix(e[2]);
                        e[0].Fix(e[1]); // ()
                        e[0].Fix(ne);   // sentence
                        e[0].AddUnkown();
                        newlist.Add(e[0]);
                        return true;
                    }
                }            
                return false;
            }
            else 
            {
                if (e[1].hasUnkown() == false)
                { 
                    if (e[1].isELSE_DONE() || e[1].isELSEIF_DONE())
                    {
                        index+=2;
                        e[0].Fix(e[1]);
                        if (e[1].isELSEIF_DONE()) e[0].AddUnkown();
                        newlist.Add(e[0]);
                        return true;
                    }
                    else
                    {
                        e[0].DelUnknown();
                        newlist.Add(e[0]);
                        return true;
                    }
                }
            }
        }
        return false;
    }
    bool isELSE(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);

        if (!e[0].isELSE())
        {
            if (e[0].isNAME("else") && !e[1].isNAME("if"))
            {
                index++;
                e[0].SetGroup(GROUP.ELSE,true);
                newlist.Add(e[0]);
                return true;
            }
        }
        if (e[0].isELSE_NOTDONE())
        {
            if (e[1].isBLOCK_M_DONE())
            {
                index+=2;
                e[0].Fix(e[1]);
                newlist.Add(e[0]);
                return true;
            }
            if (ELEMENT.isSENTENCE_1Line_CANDIDATE(e, 1))
            {
                index+=3;
                var ne = new ELEMENT();
                ne.SetGroup(GROUP.SENTENCE);
                ne.Fix(e[1]);
                e[0].Fix(ne);
                newlist.Add(e[0]);
                return true;
            }
        }
        return false;
    }
    bool isELSEIF(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);

        if (!e[0].isELSEIF())
        {
            if (e[0].isNAME("else") && e[1].isNAME("if"))
            {
                index+=2;
                e[0].SetGroup(GROUP.ELSEIF,true);
                newlist.Add(e[0]);
                return true;
            }
        }
        if (e[0].isELSEIF_NOTDONE())
        {
            if (e[1].isBLOCK_C_DONE())
            { 
                if (e[2].isBLOCK_M_DONE())
                {
                    index+=3;
                    e[0].Fix(e[1]); //()
                    e[0].Fix(e[2]); //{}
                    newlist.Add(e[0]);
                    return true;
                }
                if (ELEMENT.isSENTENCE_1Line_CANDIDATE(e, 2))
                {
                    index+=4;
                    var ne = new ELEMENT();
                    ne.SetGroup(GROUP.SENTENCE);
                    ne.Fix(e[2]);
                    e[0].Fix(e[1]); // ()
                    e[0].Fix(ne);   // sentence;
                    newlist.Add(e[0]);
                    return true;
                }
            }
        }
        return false;
    }

    bool isSWITCH(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);

        if (!e[0].isSWITCH())
        {
            if (e[0].isNAME("switch"))
            {
                index++;
                e[0].SetGroup(GROUP.SWITCH,true);
                newlist.Add(e[0]);
                return true;
            }
        }
        if (e[0].isSWITCH_NOTDONE())
        {
            if (e[1].isBLOCK_C_DONE() && e[2].isBLOCK_M_DONE())
            {
                index+=3;
                e[0].Fix(e[1]); // ()
                e[0].Fix(e[2]); // {}
                newlist.Add(e[0]);
                return true;
            }
        }
        return false;
    }

    bool isCASE(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);

        if (!e[0].isCASE())
        {
            if (e[0].isNAME("case"))
            {
                index++;
                e[0].SetGroup(GROUP.CASE,true);
                newlist.Add(e[0]);
                return true;
            }
        }
        if (e[0].isCASE_NOTDONE())
        {
            if ( (e[1].isNUMBER() || e[1].isQUOTE()) && e[2].isSPECIAL(":"))
            {
                index+=3;
                e[0].Fix(e[1]); //
                newlist.Add(e[0]);
                return true;
            }
        }
        return false;
    }

    bool isDEFAULT(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);

        if (!e[0].isDEFAULT())
        {
            if (e[0].isNAME("default"))
            {
                index++;
                e[0].SetGroup(GROUP.DEFAULT,true);
                newlist.Add(e[0]);
                return true;
            }
        }
        if (e[0].isDEFAULT_NOTDONE())
        {
            if (e[1].isSPECIAL(":"))
            {
                index+=2;
                e[0].DelUnknown();
                newlist.Add(e[0]);
                return true;
            }
        }
        return false;
    }
 


    bool isCONTINUE(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);
        if (!e[0].isCONTINUE())
        {
            if (e[0].isNAME("continue"))
            {
                index++;
                e[0].SetGroup(GROUP.CONTINUE);
                newlist.Add(e[0]);
                return true;
            }
        }
        return false;
    }
    bool isBREAK(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);
        if (!e[0].isBREAK())
        {
            if (e[0].isNAME("break"))
            {
                index++;
                e[0].SetGroup(GROUP.BREAK);
                newlist.Add(e[0]);
                return true;
            }
        }
        return false;
    }
    bool isFOR(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);

        if (!e[0].isFOR())
        {
            if (e[0].isNAME("for"))
            {
                index++;
                e[0].SetGroup(GROUP.FOR,true);
                newlist.Add(e[0]);
                return true;
            }
        }
        if (e[0].isFOR_NOTDONE())
        {
            if (e[1].isBLOCK_C_DONE())
            {
                if (e[2].isBLOCK_M_DONE())
                {             
                    index+=3;
                    e[0].Fix(e[1]); // ()
                    e[0].Fix(e[2]); // {}
                    newlist.Add(e[0]);
                    return true;
                }
                if (ELEMENT.isSENTENCE_1Line_CANDIDATE(e, 2))
                {
                    index+=4;
                    var ne = new ELEMENT();
                    ne.SetGroup(GROUP.SENTENCE);
                    ne.Fix(e[2]);
                    e[0].Fix(e[1]); // ()
                    e[0].Fix(ne);   // sentence
                    newlist.Add(e[0]);
                    return true;
                }
            }
        }
        return false;
    }
    bool isWHILE(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);
        if (!e[0].isWHILE())
        {
            if (e[0].isNAME("while"))
            {
                index++;
                e[0].SetGroup(GROUP.WHILE,true);
                newlist.Add(e[0]);
                return true;
            }
        }
        if (e[0].isWHILE_NOTDONE())
        {
            if (e[1].isBLOCK_C_DONE())
            {
                if (e[2].isBLOCK_M_DONE())
                {             
                    index+=3;
                    e[0].Fix(e[1]); // ()
                    e[0].Fix(e[2]); // {}
                    newlist.Add(e[0]);
                    return true;
                }
                if (ELEMENT.isSENTENCE_1Line_CANDIDATE(e, 2))
                {
                    index+=4;
                    var ne = new ELEMENT();
                    ne.SetGroup(GROUP.SENTENCE);
                    ne.Fix(e[2]);
                    e[0].Fix(e[1]); // ()
                    e[0].Fix(ne);   // sentence
                    newlist.Add(e[0]);
                    return true;
                }
            }
        }
        return false;
    }
    bool isBLOCK_C(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);  // xx,xxx)

        if (!e[0].isBLOCK_C())
        {
            if (e[0].isSPECIALCHAR('(') && e[1].isSPECIALCHAR(')'))
            {
                index+=2;
                e[0].SetGroup(GROUP.BLOCK_C);
                newlist.Add(e[0]);
                return true;
            }
            else if (e[0].isSPECIALCHAR('('))
            {
                index+=1;
                e[0].SetGroup(GROUP.BLOCK_C,true);
                newlist.Add(e[0]);
                return true;
            }
        }
        else if (e[0].isBLOCK_C_NOTDONE())
        {
            if (e[1].isPHRASE_DONE() && (e[2].isSPECIALCHAR(')') || e[2].isSPECIALCHAR(',') || e[2].isSPECIALCHAR(';') )) 
            {
                index += 3;
                e[0].Fix(e[1]);
                if (!e[2].isSPECIALCHAR(')'))
                {
                    e[0].AddUnkown();
                }
                newlist.Add(e[0]);
                return true;
            }
        }
        return false;
    }

    bool isBLOCK_M(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);  // { xx; xx}

        if (!e[0].isBLOCK_M())
        {
            if (e[0].isSPECIALCHAR('{') && e[1].isSPECIALCHAR('}'))
            {
                index+=2;
                e[0].SetGroup(GROUP.BLOCK_M);
                newlist.Add(e[0]);
                return true;
            }
            else if (e[0].isSPECIALCHAR('{'))
            {
                index+=1;
                e[0].SetGroup(GROUP.BLOCK_M,true);
                newlist.Add(e[0]);

                ELEMENT se = new ELEMENT();
                se.SetGroup(GROUP.SENTENCE,true);
                newlist.Add(se);

                return true;
            }
        } 
        else if (e[0].isBLOCK_M_NOTDONE())
        {
            if (e[1].isSENTENCE_DONE())
            {
                bool b = (e[2].isSPECIALCHAR('}') || e[2].group == GROUP.NONE);
                
                index += 2;
                if(b) index++;

                e[0].Fix(e[1]);
                if (!b)
                {
                    e[0].AddUnkown();
                }
                newlist.Add(e[0]);

                if (!b)
                {
                    ELEMENT se = new ELEMENT();
                    se.SetGroup(GROUP.SENTENCE,true);
                    newlist.Add(se);
                }

                return true;
            }
        }
        return false;
    }
    bool isBLOCK_L(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);  // { xx; xx}
        var e_ahead = ELEMENT.Get(list.list,index-1);

        if (!e[0].isBLOCK_L())
        {
            if (e[0].isSPECIALCHAR('[') && e[1].isSPECIALCHAR(']'))
            {
                index+=2;
                e[0].SetGroup(GROUP.BLOCK_L);
                newlist.Add(e[0]);
                return true;
            }
            else if (e[0].isSPECIALCHAR('['))
            {
                index+=1;
                e[0].SetGroup(GROUP.BLOCK_L,true);
                newlist.Add(e[0]);
                return true;
            }
        } 
        else if (e[0].isBLOCK_L_NOTDONE())
        {
            bool needCheck = false;
            if ( !string.IsNullOrEmpty(e[0].mode) && e[0].mode == "array") needCheck = true;
            else if (e_ahead.isBLOCK_L_DONE()) needCheck = true;
            else if (!e_ahead.isSPECIAL("]") && !e_ahead.isBLOCK_L()) needCheck =true;
            
            
            if ( needCheck )
            { 
                if (e[1].hasUnkown()==false && e[2].isSPECIAL("]"))
                {
                    index += 3;
                    e[0].Fix(e[1]);
                    newlist.Add(e[0]);
                    return true;
                }
            }

            if (e[1].hasUnkown() == false && e[2].isSPECIAL(","))
            {
                index += 3;
                e[0].Fix(e[1]);
                e[0].AddUnkown();
                e[0].mode="array";
                newlist.Add(e[0]);
                return true;
            }
        }
        return false;
    }

    bool isNULL(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.Get(list.list,index);

        if (!e.isNULL())
        {
            if (e.isNAME("null"))
            {
                index++;
                e.SetGroup(GROUP.NULL);
                newlist.Add(e);
                return true;
            }
        }
        return false;
    }

    bool isPROGRAM(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);  // { xx; xx}

        if (e[0].isPROGRAM_NOTDONE())
        {
            if (e[1].isSENTENCE_DONE())
            {
                bool b = (e[2].group == GROUP.NONE);
                
                index += 2;
                if(b) index++;

                e[0].Fix(e[1]);
                if (!b)
                {
                    e[0].AddUnkown();
                }
                newlist.Add(e[0]);

                if (!b)
                {
                    ELEMENT se = new ELEMENT();
                    se.SetGroup(GROUP.SENTENCE,true);
                    newlist.Add(se);
                }
                return true;
            }
        }
        return false;
    }


    bool isSENTENCE(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);  // xx,xxx)

        if (e[0].isSENTENCE_NOTDONE())
        {
            if (e[1].isPHRASE_DONE() && e[2].isSPECIALCHAR(';'))
            {
                index+=3;
                e[0].Fix(e[1]);
                newlist.Add(e[0]);

                return true;
            }
            else if (ELEMENT.isSENTENCE_1Line_CANDIDATE(e, 1))
            {
                index+=3;
                e[0].Fix(e[1]);
                newlist.Add(e[0]);

                return true;
            }
            else if (e[1].isSPECIALCHAR(';'))
            {
                index += 2;
                e[0].list = null;
                newlist.Add(e[0]);
            }
            else if (
                e[1].isDEC_FUNC_DONE() || e[1].isBLOCK_M_DONE() ||
                e[1].isIF_DONE() || e[1].isFOR_DONE() || e[1].isWHILE_DONE() ||
                e[1].isSWITCH_DONE() || e[1].isCASE_DONE() || e[1].isDEFAULT_DONE()
                )
            {
                index += 2;
                e[0].Fix(e[1]);
                newlist.Add(e[0]);
                return true;
            }
            else if (e[1].isSPECIALCHAR('}') || e[1].group == GROUP.NONE)
            {
                index += 1;
                e[0].list = null;
                newlist.Add(e[0]);
                return true;
            }
        }    
        return false;
    }
    bool isVALIABLE(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);

        if (!e[0].isVARIABLE())
        { 
            if (e[0].isNAME())
            {
                index++;
                
                e[0].SetGroup(GROUP.VARIABLE);

                if (e[1].isSPECIAL("[") || e[1].isBLOCK_L())
                {
                    e[0].AddUnkown();
                }
                newlist.Add(e[0]);
                return true;
            }
        }
        else if (e[0].isVARIABLE_NOTDONE())
        {
            if (e[1].isBLOCK_L_DONE())
            {
                index += 2;
                e[0].Fix(e[1]);
                if (e[2].isSPECIAL("[") || e[2].isBLOCK_L())
                {
                    e[0].AddUnkown();
                }
                newlist.Add(e[0]);
                return true;
            }
        }
        return false;
    }

    bool isVALFUNCTION(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);

        if (e[0].isVARIABLE_DONE() && e[1].isBLOCK_C())
        {
            index += 2;
            ELEMENT ne = new ELEMENT(){ group = GROUP.VALFUNCTION};
            ne.Fix(e[0]);
            ne.Fix(e[1]);

            newlist.Add(ne);

            return true;
        }
        return false;
    }


    bool isETC(ref int index, ListElemet list, ListElemet newlist)
    {
        var e = ELEMENT.GetRange(list.list,index);
        if (e[0].group != GROUP.NONE)
        {
            index++;
            newlist.Add(e[0]);
            return true;
        }
        return false;
    }
    // #  CHECK ELEMENT #
    // ##################

    // ##################
    // # PARSE FORMULAR #   = + - * / % && || ? :

    int _convertOP_UNARY_AHEAD(string w, int index, ListElemet list, ListElemet newlist, GROUP g) // + or -
    {
        var accept_chars_of_ahead = new string[]{ /**/"=", "+","-","*","/","%", /**/"==",">=","<=","<",">" }; 
        var e0     = ELEMENT.Get(list.list,index);
        var e1     = ELEMENT.Get(list.list,index+1);
        var e_ahead = ELEMENT.Get(list.list,index-1);

        if (
            (
             ( e_ahead.isSPECIAL() && (Array.IndexOf(accept_chars_of_ahead,e_ahead.raw)>=0))
             ||
             ( e_ahead.isBLOCK_ANY_OPEN_NOTDONE())
            )
            &&
            e0.isSPECIAL(w) 
            && 
            e1.isOPERATION_ITEM_DONE()
           )
        {
            index+=2;
            e0.SetGroup(g,true);
            e0.Fix(e1);
            newlist.Add(e0);
            return index;
        }

        return index;
    }
    int _convertOP_UNARY_BACK(string w, int index, ListElemet list, ListElemet newlist, GROUP g)
    {
        var e0     = ELEMENT.Get(list.list,index);
        var e1     = ELEMENT.Get(list.list,index+1);

        //if (
        //        e0.hasUnkown() == false
        //        &&
        //        ( 
        //          (w.Length==2 && e1.isSPECIALWORD(w))
        //           ||
        //          (w.Length==1 && e1.isSPECIALCHAR(w[0]))
        //        )
        //    )
        //{
        //    index+=2;
        //    e1.SetGroup(g);
        //    e1.Fix(e0);
        //    newlist.Add(e1);
        //    return index;
        //}

        if (e0.isOPERATION_ITEM_DONE() && e1.isSPECIAL(w))
        {
            index+=2;
            e1.SetGroup(g);
            e1.Fix(e0);
            newlist.Add(e1);
            return index;
        }
        return index;
    }

    int convertOP_PLUS(int index, ListElemet list, ListElemet newlist) { return _convertOP_UNARY_AHEAD("+", index, list, newlist, GROUP.op_UnaryPlus); }
    int convertOP_MINUS(int index, ListElemet list, ListElemet newlist) { return _convertOP_UNARY_AHEAD("-", index, list, newlist, GROUP.op_UnaryNegation); }
    int convertOP_PLUSPLUS(int index, ListElemet list, ListElemet newlist) { return _convertOP_UNARY_AHEAD("++", index, list, newlist, GROUP.op_Increment_L); }
    int convertOP_MINUSMINUS(int index, ListElemet list, ListElemet newlist) { return _convertOP_UNARY_AHEAD("--", index, list, newlist, GROUP.op_Decrement_L); }

    int convertOP_PLUSPLUS_BACK(int index, ListElemet list, ListElemet newlist) { return _convertOP_UNARY_BACK("++", index, list, newlist, GROUP.op_Increment_R); }
    int convertOP_MINUSMINUS_BACK(int index, ListElemet list, ListElemet newlist) { return _convertOP_UNARY_BACK("--", index, list, newlist, GROUP.op_Decrement_R); }

    int _convertOP_BINALY(string w, int index, ListElemet list, ListElemet newlist, GROUP g)
    {
        var e0 = ELEMENT.Get(list.list,index);
        var e1 = ELEMENT.Get(list.list,index+1);
        var e2 = ELEMENT.Get(list.list,index+2);

        if (e0.isOPERATION_ITEM_DONE() && e1.isSPECIAL(w) && e2.isOPERATION_ITEM_DONE())
        {
            index+=3;
            var ne = e1;
            ne.SetGroup(g,true);
            ne.Fix(e0);
            ne.Fix(e2);
            newlist.Add(ne);
        }
        return index;
    }

    int convertMULTIPLY(int index,ListElemet list, ListElemet newlist)      { return _convertOP_BINALY("*",  index, list, newlist, GROUP.op_Multiply);           }
    int convertDIVIDE(int index,ListElemet list, ListElemet newlist)        { return _convertOP_BINALY("/",  index, list, newlist, GROUP.op_Division);           }
    int convertODD(int index,ListElemet list, ListElemet newlist)           { return _convertOP_BINALY("%",  index, list, newlist, GROUP.op_Modulus);           }

    int convertADD(int index,ListElemet list, ListElemet newlist)           { return _convertOP_BINALY("+",  index, list, newlist, GROUP.op_Addition);           }
    int convertSUB(int index,ListElemet list, ListElemet newlist)           { return _convertOP_BINALY("-",  index, list, newlist, GROUP.op_Subtraction);           }

    int convertEQEQ(int index,ListElemet list, ListElemet newlist)          { return _convertOP_BINALY("==", index, list, newlist, GROUP.op_Equality);           }
    int convertNOTEQ(int index,ListElemet list, ListElemet newlist)         { return _convertOP_BINALY("!=", index, list, newlist, GROUP.op_Inequality);           }
    int convertLESSTHAN(int index,ListElemet list, ListElemet newlist)      { return _convertOP_BINALY("<",  index, list, newlist, GROUP.op_LessThan);      }
    int convertLESSTHAN_EQ(int index,ListElemet list, ListElemet newlist)   { return _convertOP_BINALY("<=", index, list, newlist, GROUP.op_LessThanOrEqual);   }
    int convertGRATERTHAN(int index,ListElemet list, ListElemet newlist)    { return _convertOP_BINALY(">",  index, list, newlist, GROUP.op_GreaterThan);    }
    int convertGRATERTHAN_EQ(int index,ListElemet list, ListElemet newlist) { return _convertOP_BINALY(">=", index, list, newlist, GROUP.op_GreaterThanOrEqua); }

    int convertEQUAL(int index,ListElemet list, ListElemet newlist)         { return _convertOP_BINALY("=",  index, list, newlist, GROUP.op_Assign);         }
}
