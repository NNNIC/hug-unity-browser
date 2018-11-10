using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using xsr;

public class xmlScriptOP {
    public static object _Execute_OP_INTFLOATDOUBLE(ELEMENT e,int argc, object p0,object p1,xmlScriptObj scrObj,STACKVAL stack) // FOR FAST!
    {
        var p0_type = xmlScriptGetMethod.ObjectGetType(p0);
        var p1_type = xmlScriptGetMethod.ObjectGetType(p1);

        if (argc == 2)
        { 
            if (p0_type == typeof(double) && (p1_type == typeof(double) || p1_type == typeof(float) ||  p1_type == typeof(int) ) )
            {
                double a= (double)p0;
                double b= (double)(System.Convert.ChangeType(p1,typeof(double)));
                switch(e.group)
                {
                case GROUP.op_Multiply:          return a * b;
                case GROUP.op_Division:          return a / b;        
                case GROUP.op_Modulus:           return a % b;       
                                             
                case GROUP.op_Addition:          return a + b;         
                case GROUP.op_Subtraction:       return a - b;
                                             
                case GROUP.op_Equality:          return (bool)(a==b);  
                case GROUP.op_Inequality:        return (bool)(a!=b);   
                case GROUP.op_LessThan:          return (bool)(a<b);     
                case GROUP.op_GreaterThan:       return (bool)(a>b);     
                case GROUP.op_LessThanOrEqual:   return (bool)(a<=b);
                case GROUP.op_GreaterThanOrEqua: return (bool)(a>=b);            
                }
                throw new SystemException(e + "Unexpected Operation");
            }
            else if (p0_type == typeof(int) && (p1_type == typeof(int) || p1_type == typeof(float) || p1_type == typeof(double)) )
            {
                int a= (int)p0;
                int b= (int)(System.Convert.ChangeType(p1,typeof(int)));
                switch(e.group)
                {
                case GROUP.op_Multiply:          return a * b;
                case GROUP.op_Division:          return a / b;        
                case GROUP.op_Modulus:           return a % b;       
                                             
                case GROUP.op_Addition:          return a + b;         
                case GROUP.op_Subtraction:       return a - b;
                                             
                case GROUP.op_Equality:          return (bool)(a==b);  
                case GROUP.op_Inequality:        return (bool)(a!=b);   
                case GROUP.op_LessThan:          return (bool)(a<b);     
                case GROUP.op_GreaterThan:       return (bool)(a>b);     
                case GROUP.op_LessThanOrEqual:   return (bool)(a<=b);
                case GROUP.op_GreaterThanOrEqua: return (bool)(a>=b);            
                }
                throw new SystemException(e + "Unexpected Operation");
            }
            else if (p0_type == typeof(float) && (p1_type == typeof(int) || p1_type == typeof(float) || p1_type == typeof(double) ) )
            {
                float a= (float)p0;
                float b= (float)(System.Convert.ChangeType(p1,typeof(float)));
                switch(e.group)
                {
                case GROUP.op_Multiply:          return a * b;
                case GROUP.op_Division:          return a / b;        
                case GROUP.op_Modulus:           return a % b;       
                                             
                case GROUP.op_Addition:          return a + b;         
                case GROUP.op_Subtraction:       return a - b;
                                             
                case GROUP.op_Equality:          return (bool)(a==b);  
                case GROUP.op_Inequality:        return (bool)(a!=b);   
                case GROUP.op_LessThan:          return (bool)(a<b);     
                case GROUP.op_GreaterThan:       return (bool)(a>b);     
                case GROUP.op_LessThanOrEqual:   return (bool)(a<=b);
                case GROUP.op_GreaterThanOrEqua: return (bool)(a>=b);            
                }
                throw new SystemException(e + "Unexpected Operation");
            }
        }
        if (argc == 1)
        { 
            if (p0_type == typeof(double))
            {
                Action<double> setVal=(i)=>{
                    var pe0 = e.GetListElement(0);
                    if (pe0.isVARIABLE())
                    {
                        xmlScriptExecVar.ExecuteSetVariable(pe0,i,scrObj,stack);//stack.SetVal(pe0.raw,i);
                    }
                };

                double a= (double)p0;
                switch(e.group)
                {
                case GROUP.op_Increment_L:    a= a+1; setVal(a); return a;
                case GROUP.op_Increment_R:    return a;                
                case GROUP.op_Decrement_L:    a= a-1; setVal(a); return a;
                case GROUP.op_Decrement_R:    return a;       
                                            


                case GROUP.op_UnaryPlus:      return a;       
                case GROUP.op_UnaryNegation:  return -a;
                }
                throw new SystemException(e + "Unexpected Operation");
            }
            else if ( p0_type == typeof(int))
            {
                Action<int> setVal=(i)=>{
                    var pe0 = e.GetListElement(0);
                    if (pe0.isVARIABLE())
                    {
                        xmlScriptExecVar.ExecuteSetVariable(pe0,i,scrObj,stack);//stack.SetVal(pe0.raw,i);
                    }
                };

                int a= (int)p0;
                switch(e.group)
                {
                case GROUP.op_Increment_L:    a= a+1; setVal(a); return a;
                case GROUP.op_Increment_R:    return a;                
                case GROUP.op_Decrement_L:    a= a-1; setVal(a); return a;
                case GROUP.op_Decrement_R:    return a;       
                                            


                case GROUP.op_UnaryPlus:      return a;       
                case GROUP.op_UnaryNegation:  return -a;
                }
                throw new SystemException(e + "Unexpected Operation");
            }
            else if (p0_type == typeof(float))
            {
                Action<float> setVal=(i)=>{
                    var pe0 = e.GetListElement(0);
                    if (pe0.isVARIABLE())
                    {
                        xmlScriptExecVar.ExecuteSetVariable(pe0,i,scrObj,stack);//stack.SetVal(pe0.raw,i);
                    }
                };
                float a= (float)p0;
                switch(e.group)
                {
                case GROUP.op_Increment_L:    a = a+1; setVal(a); return a;
                case GROUP.op_Increment_R:    return a;        
                case GROUP.op_Decrement_L:    a = a-1; setVal(a); return a;
                case GROUP.op_Decrement_R:    return a;

                case GROUP.op_UnaryPlus:      return a;
                case GROUP.op_UnaryNegation:  return -a;
                }
                throw new SystemException(e + "Unexpected Operation");
            }
        }

        return null;
    }

    public static object _Execute_OP_STRING(ELEMENT e, object p0,object p1,xmlScriptObj scrObj, STACKVAL stack)
    {
        if (p0==null || p1==null) return null;
        if ( xmlScriptGetMethod.ObjectGetType(p0) == typeof(string))
        {
            string a= p0.ToString();
            string b= p1.ToString();
            switch(e.group)
            {
            case GROUP.op_Addition:          return a + b;                                                      
            case GROUP.op_Equality:          return (bool)(a==b);       
            case GROUP.op_Inequality:        return (bool)(a!=b);                
            }
            throw new SystemException(e + "Unexpected Operation");
        }
        return null;
    }
}
