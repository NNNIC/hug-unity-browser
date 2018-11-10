using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace xsr
{
    //http://www.fureai.or.jp/~tato/JS/BOOK/BOOKSAMPL/PART3/PART3.HTM

    public enum GROUP
    {
        NONE,

        UNKNOWN,

        //RAW
        NAME,
        NUMBER, 
        QUOTE,  

        SPECIALCHAR,
        /*
        EQUAL,         //=
        PARENTHESIS,   //(
        BLACKET,       //[
        CURLYBLACKET,  //{

        PLUS,       //+
        MINUS,      //-
        MULTIPLE,   //*
        DIVIDE,     ///
        PERCENT,    //%
        EXP,        //^
        EXCLAMATION,//!

        PERIOD,     //.
        SEMICOLLON, //;
        COLLON,     //:
        QUESTION,   //?

        RIGHTOPEN,  //<
        LEFTOPEN,   //>
        */

        SPECIALWORD,
        /*
            ++
            --
            ==
            !=
            <=
            >=
        */


        TYPE,
        POINTER,   // (**).
        VARIABLE,  //
        FUNCTION,  //  
        VALFUNCTION, // X[x](xxx) 
        //FORMULAR,  // x = xxx;

        //WORD,    // INDEPENDED NAME 

        SENTENCE,  // X = XXX * XXX ---;
        //SENTENCE,  //  XX();

        //IF_SENTENCE,
        IF,
        ELSE,
        ELSEIF,

        //SWITCH_SENTENCE
        SWITCH,
        CASE,
        DEFAULT,

        //LOOP_SENTENCE,
        WHILE,
        FOR,
        FOREACH,
        BREAK,
        CONTINUE,

        //DEC_SENTENCE,
        DEC_VAR,  //var
        DEC_FUNC, //function

        RETURN,
        NEW,
        DELETE,

        //BLOCK
        BLOCK_L, //[]
        BLOCK_M, //{}
        BLOCK_C, //()
        //BLOCK_L_CLAMP, //[][][]..


        PROGRAM, // Program block

        
        //SPECIAL
	    NULL,
	    
        //BEHAVIOUR,
        PARSEOUTPUT,
        ROOT,
        
        //OPERATOR http://en.csharp-online.net/Common_Type_System%E2%80%94Operator_Overloading
        //  Single OP
        OP_SINGLE_START,     // Marking

        op_Increment_L,      //  ++X
        op_Increment_R,      //  X++
        op_Decrement_L,      //  --X
        op_Decrement_R,      //  X--
        op_UnaryPlus,        //  +X
        op_UnaryNegation,    //  -X

        OP_DUAL_START,       // Marking

        //  Dual OP
        op_Multiply,         // A * B
        op_Division,         // A / B
        op_Modulus,          // A % B

        op_Addition,         // A + B
        op_Subtraction,      // A - B
                          
        op_Equality,          // A==B
        op_Inequality,        // A!=B
        op_LessThan,          // A<B
        op_GreaterThan,       // A>B
        op_LessThanOrEqual,   // A<=B
        op_GreaterThanOrEqua, // A>=B

        op_Assign,            // A = B

        OP_TRI_START,

        OP_END
    }
    public class ELEMENT
    {
        public GROUP  group;
        public string raw;
        public List<ELEMENT> list;

        public string        decname;
       
        public CACHEDATA     cache;   // for work
        public string        mode;    // for BLOCK_L

        public string        dbg_string 
        {
            get{ return ToString(); }
        }

        public ELEMENT CreateClone()
        {
            ELEMENT ne      = new ELEMENT();
            ne.group        = group;
            ne.raw          = raw;
            ne.list         = list;
            ne.decname      = decname;
            return ne;
        }
        public ELEMENT GetPointerVARIABLE()
        {
            if (!isPOINTER()) return null;
            var p0 = GetListElement(0);
            if (!p0.isVARIABLE()) return null;
            return p0;
        }
        public ELEMENT GetPointerFUNCTION()
        {
            if (!isPOINTER()) return null;
            var p0 = GetListElement(0);
            if (!p0.isFUNCTION()) return null;
            return p0;
        }
        public ELEMENT GetPointerInside()
        {
            if (!isPOINTER()) return null;
            var p0 = GetListElement(0);
            return p0;
        }
        public GROUP GetPointerGROUP()
        {
            if (!isPOINTER()) return GROUP.NONE;
            var p0 = GetListElement(0);
            return p0.group;          
        }

        public override string ToString()
        {
            Func<int,string> getp = (i)=>{ 
                if (list!=null&&i>=0&&i<list.Count) return list[i].ToString();
                return "?";
            };
            Func<string> sl = () => {
                string a = "";
                if (list!=null) foreach(var i in list)
                {
                    a += i.ToString();
                }
                return a;
            };
            Func<string> slc = () => {
                string a = "";
                if (list!=null) for(int i = 0;i<list.Count;i++)
                {   
                    if (i!=0) a+=",";
                    a += list[i].ToString();
                }
                return a;
            };
            Func<string> get2p = () => {
                return "(" + getp(0) + "," + getp(1) + ")";
            };
            Func<bool,string> ifstr = (b) => {
                string d = (b ? "if" : "else if")  + getp(0);
                for(int i = 1;i<list.Count;i++) d += getp(i);
                return d;
            };
            Func<string> pointstr = () => {
                //if (list.Count==2) return GetPointerName() + list[0] +"." +list[1];
                //return GetPointerName() + "." +sl();

                return GetListElement(0).ToString() + "." +GetListElement(1).ToString();
            };


            string s="";
            switch(group)
            {
            //case GROUP.BEHAVIOUR:   s = "behaviour \"" + decname + "\"" + sl(); break;
            case GROUP.DEC_FUNC:      s = "function " + decname + sl(); break;
            case GROUP.DEC_VAR:       s = "var " + decname + sl(); break;
            case GROUP.BLOCK_M:       s = " {" + sl() + "} "; break;
            case GROUP.BLOCK_C:       s = "(" + slc() + ")"; break;
            case GROUP.BLOCK_L:       s = "[" + slc() + "]"; break;
            //case GROUP.BLOCK_L_CLAMP: s =   slc(); break;
            case GROUP.PROGRAM:       s = "<pgm>" + sl() + "</pgm>\n.\n."; break;
            case GROUP.FUNCTION:      s = raw + sl(); break;
            case GROUP.SENTENCE:      s = "`" + sl() +";"; break;
            case GROUP.POINTER:       s = pointstr(); break; // raw + "." +sl(); break;
            case GROUP.RETURN:        s = raw; break;
                                      
            case GROUP.QUOTE:         
            case GROUP.NAME:          s = "\""+raw+"\"";break;
            case GROUP.NUMBER:        s = "#"+raw; break;
            case GROUP.SPECIALCHAR:   s = "/"+raw+"/";break;
            case GROUP.SPECIALWORD:   s = "/"+raw+"/";break;
            case GROUP.VARIABLE:      s = "$"+raw+sl();break;
                                      
            case GROUP.IF:            s = ifstr(true); break;
            case GROUP.ELSE:          s = "else "+getp(0); break;
            case GROUP.ELSEIF:        s = ifstr(false); break;

            case GROUP.op_Multiply:           
            case GROUP.op_Division:           
            case GROUP.op_Modulus:
            case GROUP.op_Addition:           
            case GROUP.op_Subtraction:           
            case GROUP.op_Equality:          
            case GROUP.op_LessThan:      
            case GROUP.op_GreaterThan:    
            case GROUP.op_LessThanOrEqual:   
            case GROUP.op_GreaterThanOrEqua:
            case GROUP.op_Assign:    s = group.ToString().Substring(2)+ get2p(); break;


            
            default:                 s ="<<" + group.ToString() +":" + sl() + ">>"; break;            
            }

            return s;
        }

        string GetRowString()
        {
            string s = "";
            if (list!=null) 
            {
                foreach(var i in list)
                {
                    s += i.GetRowString() + " ";
                }
                return s;
            }
            else 
            {
                return (raw!=null ? raw : "");
            }
        }

        public string GetPointerString()
        {
            if (isPOINTER())
            {
                string n = GetPointerNext().GetPointerString();

                string orgraw = "";
                var org = GetListElement(0);
                switch(org.group)
                {
                case GROUP.VARIABLE: orgraw = org.raw; break;
                case GROUP.FUNCTION: orgraw = org.raw + "()"; break;
                case GROUP.QUOTE:    orgraw = "\""+org.raw + "\""; break;
                case GROUP.BLOCK_C:  orgraw = "()"; break;
                }

                return string.IsNullOrEmpty(n) ? orgraw : orgraw + "." + n;
            }
            return "";
        }
        public ELEMENT GetPointerLast()
        {
            if (isPOINTER())
            {
                return GetPointerNext().GetPointerLast();
            }
            if (isFUNCTION())
            {
                return this;
            }
            if (isVARIABLE())
            {
                return this;
            }
            if (isVALFUNCTION())
            {
                return this;
            }
            return null;
        }
        public ELEMENT GetPointerNext()
        {
            if (isPOINTER())
            {
                return GetListElement(1);
            }
            return null;
        }
        public ELEMENT GetPointerNext(int n)
        {
            ELEMENT  p = this;
            for (int i = 0; i < n; i++)
            {
                if (p==null || p.isNONE()) return null;
                p = p.GetPointerNext();
            }
            return p;
        }

        public string GetPointerName(bool allowVariable)
        {
            if (isPOINTER())
            {
                var org = GetListElement(0);
                switch(org.group)
                {
                case GROUP.VARIABLE:
                    if (isVARIABLE_ARRAY()) return raw + "[]";
                    return org.raw;
                case GROUP.FUNCTION:
                    return org.raw + "()";
                case GROUP.BLOCK_C:
                    return "()";
                case GROUP.QUOTE:
                    return "\"" + org.raw + "\"";
                }
            }

            if (allowVariable)
            {
                if (isVARIABLE())
                {
                    if (isVARIABLE_ARRAY()) return raw +"[]";
                    return raw;
                }
            }

            return null;
        }

        public int GetPointerCount(bool isPointerOrVariable=false)
        {
            int c = 0;
            ELEMENT p = this;
            for (int i = 0; i < 100; i++)
            {
                if (p==null || p.isNONE()) break;
                if (isPointerOrVariable)
                {
                    if (p.isPOINTER() || p.isVARIABLE() ) c++;
                }
                else
                {
                    c++;
                }
                p = p.GetPointerNext();
            }
            return c;
        }

        public bool isPHRASE_DONE()
        {
            if (hasUnkown()) return false;
            switch(group)
            {
            case GROUP.NONE:
            case GROUP.UNKNOWN:
            case GROUP.NAME:
                return false;
            }
            return true;
        }

        public static bool isSENTENCE_1Line_CANDIDATE(ELEMENT[] e,int i)
        {
            if (
                (e[i].isOP_DONE() && e[i + 1].isSPECIALCHAR(';'))
                ||
                (e[i].isFUNCTION_DONE() && e[i + 1].isSPECIALCHAR(';'))
                ||
                (e[i].isPOINTER_DONE() && e[i + 1].isSPECIALCHAR(';'))
                ||
                (e[i].isBREAK() && e[i + 1].isSPECIALCHAR(';'))
                ||
                (e[i].isCONTINUE() && e[i + 1].isSPECIALCHAR(';'))                
                ||
                (e[i].isRETURN() && e[i + 1].isSPECIALCHAR(';'))   
                ||
                (e[i].isVALFUNCTION_DONE() && e[i + 1].isSPECIALCHAR(';'))
                )
            {
                return true;
            }
            return false;
        }
      
        public bool isOPERATION_ITEM()
        {
            if (isOP()) return true;

            switch(group)
            {
            case GROUP.DEC_VAR:
            case GROUP.FUNCTION:
            case GROUP.POINTER:
            case GROUP.BLOCK_C:
            case GROUP.BLOCK_L:
            case GROUP.VARIABLE:
            case GROUP.NUMBER:
            case GROUP.QUOTE:
            case GROUP.NEW:
            case GROUP.NULL:
                return true;
            }
            return false;
        }

        public bool isOPERATION_ITEM_DONE()
        {
            if (isOPERATION_ITEM() && hasUnkown() == false)
            {
                return true;
            }
            return false;
        }

        public bool isBLOCK_ANY_OPEN_NOTDONE()
        {
            if (hasUnkown()==false) return false;

            switch(group)
            {
            case GROUP.BLOCK_C:
            case GROUP.BLOCK_M:
            case GROUP.BLOCK_L:
                return true;
            }
            return false;
        }

 
        public bool hasUnkown() {
            if (list!=null) foreach(var i in list) if (i.group == GROUP.UNKNOWN) return true;

            return false;
        } 

        public void SetGroup(GROUP g, bool addUnkown=false)
        {
            group = g;
            if (addUnkown) AddUnkown();
        }
        public void AddUnkown()
        {
            if (list==null) list = new List<ELEMENT>();
            if (list.Count > 0 && list[list.Count - 1].group == GROUP.UNKNOWN)
            {
                return; // do nothing. because it already has set.
            }
            list.Add(new ELEMENT(){group = GROUP.UNKNOWN});
        }
        public void DelUnknown()
        {
            if (list==null && list.Count==0) return;
            if (list[list.Count - 1].group == GROUP.UNKNOWN)
            {
                list.RemoveAt(list.Count-1);
            }
        }
        public void Fix(ELEMENT e)
        {
            if (list == null)
            {
                list = new List<ELEMENT>();
                list.Add(e);
            }
            else
            { 
                var t = list[list.Count-1];
                if (t.group == GROUP.UNKNOWN)
                {
                    list[list.Count-1]=e;
                }
                else
                {
                    list.Add(e);
                }
            }
        }
        public ELEMENT GetListElement(int index)
        {
            if (list==null||index < 0 || index>=list.Count) return new ELEMENT(){ group = GROUP.NONE};
            return list[index];
        }
        public int GetListCount() { return list==null || list.Count==0 ? 0 : list.Count;  }


        public bool isNONE()                { return  (group == GROUP.NONE);     }
                                            
        public bool isNAME()                { return  (group == GROUP.NAME);     }
        public bool isNUMBER()              { return  (group == GROUP.NUMBER);   }
        public bool isQUOTE()               { return  (group == GROUP.QUOTE);    }
        public bool isNAME(string w)        { return  (group == GROUP.NAME && w == raw); }
        public bool isSPECIAL()             { return  (group == GROUP.SPECIALCHAR || group == GROUP.SPECIALWORD);}
        public bool isSPECIAL(string w)     { return  (raw!=null && raw == w) && ( (group == GROUP.SPECIALCHAR && raw.Length==1) || (group == GROUP.SPECIALWORD && raw.Length==2) ); }
        public bool isSPECIALCHAR(char c)   { return  (group == GROUP.SPECIALCHAR && c.ToString() == raw); }
        public bool isSPECIALWORD(string w) { return  (group == GROUP.SPECIALWORD && w == raw); }
                                          
        public bool isPOINTER()             { return  (group == GROUP.POINTER);  }
        public bool isPOINTER_DONE()        { return  (group == GROUP.POINTER && hasUnkown() == false); }
        public bool isPOINTER_NOTDONE()     { return  (group == GROUP.POINTER && hasUnkown() == true);  }
                                            
        public bool isVARIABLE()            { return  (group == GROUP.VARIABLE);  }
        public bool isVARIABLE_DONE()       { return  (group == GROUP.VARIABLE && hasUnkown() == false); }
        public bool isVARIABLE_NOTDONE()    { return  (group == GROUP.VARIABLE && hasUnkown() == true);  }
        public bool isVARIABLE_ARRAY()      { return  (group == GROUP.VARIABLE && GetListCount()>0);     }
        public bool isVARIABLE_NOTARRAY()   { return  (group == GROUP.VARIABLE && GetListCount()==0);    }
                                            
        public bool isFUNCTION()            { return  (group == GROUP.FUNCTION); }
        public bool isFUNCTION_DONE()       { return  (group == GROUP.FUNCTION && hasUnkown() == false); }
        public bool isFUNCTION_NOTDONE()    { return  (group == GROUP.FUNCTION && hasUnkown() == true);  }
                                            
        public bool isVALFUNCTION()         { return  (group == GROUP.VALFUNCTION); }
        public bool isVALFUNCTION_DONE()    { return  (group == GROUP.VALFUNCTION && hasUnkown() == false); }
        public bool isVALFUNCTION_NOTDONE() { return  (group == GROUP.VALFUNCTION && hasUnkown() == true);  }
                                            
        public bool isDEC_FUNC()            { return  (group == GROUP.DEC_FUNC); }
        public bool isDEC_FUNC_DONE()       { return  (group == GROUP.DEC_FUNC  && hasUnkown() == false);}
        public bool isDEC_FUNC_NOTDONE()    { return  (group == GROUP.DEC_FUNC  && hasUnkown() == true); }
                                            
        public bool isDEC_VAR()             { return  (group == GROUP.DEC_VAR); }
        public bool isDEC_VAR_DONE()        { return  (group == GROUP.DEC_VAR  && hasUnkown() == false);}
        public bool isDEC_VAR_NOTDONE()     { return  (group == GROUP.DEC_VAR  && hasUnkown() == true); }
        
        public bool isIF()                  { return  (group == GROUP.IF);} 
        public bool isIF_DONE()             { return  (group == GROUP.IF && hasUnkown()==false);        }              
        public bool isIF_NOTDONE()          { return  (group == GROUP.IF && hasUnkown()==true);         }        

        public bool isELSE()                { return  (group == GROUP.ELSE);} 
        public bool isELSE_DONE()           { return  (group == GROUP.ELSE && hasUnkown()==false);        }              
        public bool isELSE_NOTDONE()        { return  (group == GROUP.ELSE && hasUnkown()==true);         } 

        public bool isELSEIF()              { return  (group == GROUP.ELSEIF);} 
        public bool isELSEIF_DONE()         { return  (group == GROUP.ELSEIF && hasUnkown()==false);        }              
        public bool isELSEIF_NOTDONE()      { return  (group == GROUP.ELSEIF && hasUnkown()==true);         } 

        public bool isSWITCH()              { return  (group == GROUP.SWITCH);} 
        public bool isSWITCH_DONE()         { return  (group == GROUP.SWITCH && hasUnkown()==false);        }              
        public bool isSWITCH_NOTDONE()      { return  (group == GROUP.SWITCH && hasUnkown()==true);         } 

        public bool isCASE()                { return  (group == GROUP.CASE);} 
        public bool isCASE_DONE()           { return  (group == GROUP.CASE && hasUnkown()==false);        }              
        public bool isCASE_NOTDONE()        { return  (group == GROUP.CASE && hasUnkown()==true);         } 

        public bool isDEFAULT()             { return  (group == GROUP.DEFAULT);} 
        public bool isDEFAULT_DONE()        { return  (group == GROUP.DEFAULT && hasUnkown()==false);        }              
        public bool isDEFAULT_NOTDONE()     { return  (group == GROUP.DEFAULT && hasUnkown()==true);         } 

        public bool isWHILE()               { return  (group == GROUP.WHILE);} 
        public bool isWHILE_DONE()          { return  (group == GROUP.WHILE && hasUnkown()==false);        }              
        public bool isWHILE_NOTDONE()       { return  (group == GROUP.WHILE && hasUnkown()==true);         } 

        public bool isFOR()                 { return  (group == GROUP.FOR);} 
        public bool isFOR_DONE()            { return  (group == GROUP.FOR && hasUnkown()==false);        }              
        public bool isFOR_NOTDONE()         { return  (group == GROUP.FOR && hasUnkown()==true);         } 

        public bool isFOREACH()             { return  (group == GROUP.FOREACH);} 
        public bool isFOREACH_DONE()        { return  (group == GROUP.FOREACH && hasUnkown()==false);    }              
        public bool isFOREACH_NOTDONE()     { return  (group == GROUP.FOREACH && hasUnkown()==true);     } 

        public bool isRETURN()              { return  (group == GROUP.RETURN);} 
        public bool isRETURN_DONE()         { return  (group == GROUP.RETURN && hasUnkown()==false);    }              
        public bool isRETURN_NOTDONE()      { return  (group == GROUP.RETURN && hasUnkown()==true);     } 

        public bool isNEW()                 { return  (group == GROUP.NEW);} 
        public bool isNEW_DONE()            { return  (group == GROUP.NEW && hasUnkown()==false);    }              
        public bool isNEW_NOTDONE()         { return  (group == GROUP.NEW && hasUnkown()==true);     } 

        public bool isDELETE()              { return  (group == GROUP.DELETE);} 
        public bool isDELETE_DONE()         { return  (group == GROUP.DELETE && hasUnkown()==false);    }              
        public bool isDELETE_NOTDONE()      { return  (group == GROUP.DELETE && hasUnkown()==true);     } 

        public bool isBREAK()               { return  (group == GROUP.BREAK);                            }
        public bool isCONTINUE()            { return  (group == GROUP.CONTINUE);                         }
                                    
        public bool isBLOCK_C()             { return (group == GROUP.BLOCK_C);   }
        public bool isBLOCK_C_DONE()        { return (group == GROUP.BLOCK_C && hasUnkown() == false);   }
        public bool isBLOCK_C_NOTDONE()     { return (group == GROUP.BLOCK_C && hasUnkown() == true);    }
                                            
        public bool isSENTENCE()            { return (group == GROUP.SENTENCE);  }
        public bool isSENTENCE_DONE()       { return (group == GROUP.SENTENCE && hasUnkown() == false);  }
        public bool isSENTENCE_NOTDONE()    { return (group == GROUP.SENTENCE && hasUnkown() == true);   }
                                            
        public bool isBLOCK_M()             { return (group == GROUP.BLOCK_M);   }
        public bool isBLOCK_M_DONE()        { return (group == GROUP.BLOCK_M && hasUnkown() == false);   }
        public bool isBLOCK_M_NOTDONE()     { return (group == GROUP.BLOCK_M && hasUnkown() == true);    }
                                            
        public bool isBLOCK_L()             { return (group == GROUP.BLOCK_L);   }
        public bool isBLOCK_L_DONE()        { return (group == GROUP.BLOCK_L && hasUnkown() == false);   }
        public bool isBLOCK_L_NOTDONE()     { return (group == GROUP.BLOCK_L && hasUnkown() == true);    }

        //public bool isBLOCK_L_CLAMP()       { return (group == GROUP.BLOCK_L_CLAMP);   }
        //public bool isBLOCK_L_CLAMP_DONE()  { return (group == GROUP.BLOCK_L_CLAMP   && hasUnkown() == false);   }
        //public bool isBLOCK_L_CLAMP_NOTDONE() { return (group == GROUP.BLOCK_L_CLAMP && hasUnkown() == true);    }

        public bool isNULL()                { return (group == GROUP.NULL);                              }
        public bool isNULL_DONE()           { return (group == GROUP.NULL && hasUnkown() == false);      }
        public bool isNULL_NOTDONE()        { return (group == GROUP.NULL && hasUnkown() == true);       }

        public bool isPROGRAM()             { return (group == GROUP.PROGRAM);   }
        public bool isPROGRAM_DONE()        { return (group == GROUP.PROGRAM && hasUnkown() == false);   }
        public bool isPROGRAM_NOTDONE()     { return (group == GROUP.PROGRAM && hasUnkown() == true);    }
        public bool isROOT()                { return (group == GROUP.ROOT);                              }

        public bool isPARSEOUTPUT()         { return  (group == GROUP.PARSEOUTPUT);             }

        public bool isOP()                  { return  hglEtc.check_head(group.ToString(),"op_");}
        public bool isOP_DONE()             { return  (isOP() && hasUnkown()==false);           }
        public bool isOP_NOTDONE()          { return  (isOP() && hasUnkown()==true);            }

        public int GetOP_ARGC()
        {
            if (group > GROUP.OP_SINGLE_START && group < GROUP.OP_DUAL_START) return 1;
            if (group > GROUP.OP_DUAL_START   && group < GROUP.OP_TRI_START)  return 2;

            return -1;
        }
                                            
        public static void assert(bool b, string msg) { if (!b) throw new SystemException(msg);     }

        public static ELEMENT[] GetRange(List<ELEMENT> list, int start, int num = 5) 
        {
            ELEMENT[] l = new ELEMENT[num];
            for(int i = 0; i< num; i++) l[i] = (i+start<list.Count ? list[i+start] : new ELEMENT(){ group=GROUP.NONE});
            return l;
        }
        public static ELEMENT Get(List<ELEMENT> list, int index)
        {
            return ( (list!=null &&  index >= 0 && index < list.Count) ? list[index] : new ELEMENT(){group=GROUP.NONE});
        }

        public static void Traverse(ELEMENT e, Action<ELEMENT> act)
        {
            act(e);
            if (e.list!=null&&e.list.Count>0) foreach(var i in e.list) Traverse(i,act);
        }

        public static ELEMENT CreateListUnkown(GROUP g,int c) { 
            var e = new ELEMENT() { group = g};
            e.list = new List<ELEMENT>();
            for(int i = 0; i< c; i++)
            {
                e.list.Add(new ELEMENT() { group = GROUP.UNKNOWN });
            }
            return e;
        }
        public static bool AddElement(List<ELEMENT> list, ELEMENT e)
        {
            if (e != null) { list.Add(e); return true; }
            return false;
        }
        //---- for output

        public static bool FindFunctionFromRoot(ELEMENT root, string name, out ELEMENT funcElement, out ELEMENT pgmElement) // pgmElement will be needed when get stack.
        {
            funcElement = new ELEMENT(){group=GROUP.NONE};
            pgmElement  = new ELEMENT(){group=GROUP.NONE};

            if (root.list==null) return false;

            foreach (var v in root.list)
            { 
                pgmElement = v;

                        //UnityEngine.Debug.LogError("v ="+v);
                

                for (int i = 0; i < v.GetListCount(); i++)
                {
                    var e = v.GetListElement(i);
                    if (e.isSENTENCE() && e.GetListElement(0).isDEC_FUNC())
                    {
                        var cd = e.GetListElement(0);
                        if (name == cd.decname)
                        {
                            funcElement = cd;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public static bool FindFunction(STACKVAL istack, string name, out ELEMENT funcElement, out ELEMENT pgmElement) // pgmElement will be needed when get stack.
        {
            STACKVAL stack = istack;
            while (stack.OwnerElement.isPROGRAM() == false)
            {
                stack = stack.Parent;
                if (stack==null) Debug.Log("Unexpected!");
            }

            pgmElement = stack.OwnerElement;
            funcElement = new ELEMENT(){group=GROUP.NONE};

            for (int i = 0; i < pgmElement.GetListCount(); i++)
            {
                var e = pgmElement.GetListElement(i);
                if (e.isSENTENCE() && e.GetListElement(0).isDEC_FUNC())
                {
                    var cd = e.GetListElement(0);
                    if (name == cd.decname)
                    {
                        funcElement = cd;
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public class STACKVAL
    {
        //readonly string FUNCINFO = "@funcinfo";
        //const string ARGS = "@ARGS";

        string          m_stackname;
        STACKVAL        m_root;
        STACKVAL        m_parent;
        ELEMENT         m_ownerElement;
        int             m_idInParent;

        List<Hashtable> m_stackList;
        Hashtable m_curStack { get {     
            return (m_stackList!=null && m_stackList.Count>0 ? m_stackList[m_stackList.Count-1] : null); 
        }}

        public STACKVAL()
        {
            m_stackList = new List<Hashtable>();
            m_stackList.Add(new Hashtable());
            m_root      = this;
        }
        public STACKVAL CreateStack(string istackname,ELEMENT e)
        {
            var newstack = new STACKVAL();
            var stackname = "<" + istackname + ">";
            newstack.m_stackname = stackname;
            newstack.m_root = m_root;
            newstack.m_parent = this;
            newstack.m_ownerElement = e;
            newstack.m_idInParent = m_stackList.Count-1;

            m_curStack[stackname]=newstack;
            return newstack;
        }

        public ELEMENT  OwnerElement { get{return m_ownerElement;} }
        public STACKVAL Parent       { get{return m_parent;}}

        public STACKVAL FindStack(string istackname)
        {
            var stackname = "<" + istackname + ">";
            if (m_curStack.ContainsKey(stackname))
            {
                return (STACKVAL)m_curStack[stackname];
            }
            return null;
        }
        public STACKVAL FindStackFromRoot(string istackname)
        {
            return m_root.FindStack(istackname);
        }
        public void DestroyStack(STACKVAL stack)
        {
            var stackname = stack.m_stackname;
            var table = m_stackList[stack.m_idInParent];
            table.Remove(stackname);
        }
        public void PushStack()
        {
            m_stackList.Add(new Hashtable());
        }
        public void PopStack()
        {
            m_stackList.RemoveAt(m_stackList.Count-1);
        }

        public void DeclareLocalVal(string vname)
        {
            //UnityEngine.Debug.Log("DeclearLocalVal " + vname);
            m_curStack[vname] = new object();
        }

        public bool SetVal(string vname,object val)
        {
            //UnityEngine.Debug.Log("SetVal " + vname + "=" + val);
            for (int i = m_stackList.Count - 1; i >= 0; i--)
            {
                if (m_stackList[i].ContainsKey(vname)) { m_stackList[i][vname] = val; return true;}
            }

            if (m_parent != null)
            {
                bool b =m_parent.SetVal(vname,val);
                if (b) return true;
            }
            return false;
        }
        public void SetGlobalVal(string vname, object val)
        {
            //UnityEngine.Debug.LogError("SetGlobalVal " + vname + "=" + val);
            STACKVAL ancester = this;
            while(ancester.m_parent!=null) ancester = ancester.m_parent;
            ancester.DeclareLocalVal(vname);
            SetVal(vname,val);
            //UnityEngine.Debug.LogError("SetGlobalVal " + vname + "=" + val + "--->" + b) ;
        }

        public object GetVal(string vname)
        {
            if (string.IsNullOrEmpty(vname)) 
				throw new SystemException("ERROR VARIABLE NAME");
            for (int i = m_stackList.Count - 1; i >= 0; i--)
            {
                if (m_stackList[i].ContainsKey(vname)) return m_stackList[i][vname];
            }

            if (m_parent != null)
            {
                return m_parent.GetVal(vname);
            }

            //UnityEngine.Debug.LogError("Get Val vname = null");
            return null;
        }
        public bool isExist(string vname)
        {
            for (int i = m_stackList.Count - 1; i >= 0; i--)
            {
                if (m_stackList[i].ContainsKey(vname)) return true;
            }

            if (m_parent != null)
            {
                return m_parent.isExist(vname);
            }

            //UnityEngine.Debug.LogError("Get Val vname = null");
            return false;
        }

    }

    public class SENTENCE_STATE
    {
        public enum STATE
        {
            NONE,
            BREAK,
            CONTINUE,
            RETURN,
        }
        public STATE state;

        public object ret;
    }

    public class CACHEDATA
    {
        public static bool enable;
        public object info;
    }
    
    public class NULLOBJ {
        public override string ToString()
        {
            return "null";
        }
    }

}

