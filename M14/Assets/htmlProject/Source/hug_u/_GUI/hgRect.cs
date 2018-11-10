using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using hug_u;

namespace hug_u {
public class hgRect  {

    public Vector2 max_v;
    public Vector2 min_v;
    public Vector2 center  {  get{  return new Vector2( (max_v.x + min_v.x)/2f , (max_v.y + min_v.y)/2f   );      }   }
    public Vector2 topLeft {  get{  return new Vector2( min_v.x, max_v.y );                                       }   }
    public Vector2 topRight{  get{  return new Vector2( max_v.x, max_v.y );                                       }   }
    public Vector2 botLeft {  get{  return new Vector2( min_v.x, min_v.y  );                                      }   }
    public Vector2 botRight{  get{  return new Vector2( max_v.x, min_v.y  );                                      }   }
    public float   width   {  get{  return max_v.x - min_v.x;                                                     }   }
    public float   height  {  get{  return max_v.y - min_v.y;                                                     }   }
    public float   ext_w   {  get{  return width  / 2; } }
    public float   ext_h   {  get{  return height / 2; } }
    public float   _leftX  {  get{  return min_v.x;    } }
    public float   _botY   {  get{  return min_v.y;    } }
    public float   _rightX {  get{  return max_v.x;    } }
    public float   _topY   {  get{  return max_v.y;    } }

    public static hgRect zero {  get { return new hgRect(0,0,0,0);       } }

    public hgRect() {
        Clear();
    }
    public hgRect(hgRect x)
    {
        max_v = x.max_v;
        min_v = x.min_v;
    }
    public hgRect(float left, float top, float width, float height)
    {
        Clear();
        Sample( new Vector2(left,top) );
        Sample( new Vector2(left+width,top-height));
    }
    public hgRect(Vector3[] vlist)
    {
        Clear();
        Sample(vlist);
    }
    public hgRect(Vector2 imax_v, Vector2 imin_v)
    {
        max_v = imax_v;
        min_v = imin_v;
    }
    public void Clear()
    {
        max_v.x = max_v.y = float.MinValue;
        min_v.x = min_v.y = float.MaxValue;
    }
    public void Sample(Vector2 v)
    {
        max_v.x = Mathf.Max(max_v.x,v.x);
        max_v.y = Mathf.Max(max_v.y,v.y);
        min_v.x = Mathf.Min(min_v.x,v.x);
        min_v.y = Mathf.Min(min_v.y,v.y);
    }
    public void Sample(Vector3[] vl)
    {
        if (vl==null || vl.Length==0) return;
        foreach (var v in vl)
        {
            Sample(new Vector2(v.x,v.y));
        }
    }
    public void Sample(hgRect r)
    {
        Vector2 tmp_max, tmp_min;
        tmp_max = r.max_v;
        tmp_min = r.min_v;
        max_v.x = Mathf.Max(max_v.x,tmp_max.x);
        max_v.y = Mathf.Max(max_v.y,tmp_max.y);
        min_v.x = Mathf.Min(min_v.x,tmp_min.x);
        min_v.y = Mathf.Min(min_v.y,tmp_min.y);
    }
    public bool CheckEqual(hgRect x)
    {
        return (x.max_v == max_v && x.min_v == min_v);
    }
    public bool CheckSizeNotEqual(hgRect x)
    {
        return (x.width!=width || x.height!=height);
    }
    public bool CheckPosition(hgRect x)
    {
        return ( max_v != x.max_v || min_v != x.min_v);
    }
    public bool IsInifinityHeight() { return height>1e+5f;}
    //public xgRect MoveTopToDown(float height)
    //{
    //    var top = max_v.y;
    //    max_v.y -= height;

    //    if (max_v.y <= min_v.y) throw new SystemException("Unsuported");
    //    return this;
    //}
    public hgRect MoveTop(float topPos)
    {
        //if (topPos > max_v.y || topPos < min_v.y) throw new SystemException("Unsuporte");
        //max_v.y = topPos;

        float v = topPos - _topY;
        MoveY(v);
 
        return this;
    }
    public hgRect MoveCenter(float newCenterX)
    {
        var v = newCenterX - center.x;
        max_v.x += v; 
        min_v.x += v;
        return this;
    }
    public hgRect MoveX(float v)
    {
        max_v.x += v;
        min_v.x += v;
        return this;
    }
    public hgRect MoveY(float v)
    {
        max_v.y += v;
        min_v.y += v;
        return this;
    }

    public hgRect CalcLocal(Transform cur, Transform target)
    {
        var max_v3 = hglEtc.toVector3(max_v);
        var min_v3 = hglEtc.toVector3(min_v);

        var new_max_v3 = hglEtc.CalcLocalPosition(cur,max_v3,target);
        var new_min_v3 = hglEtc.CalcLocalPosition(cur,min_v3,target);

        var newRect = new hgRect();
        newRect.max_v = hglEtc.toVector2(new_max_v3);
        newRect.min_v = hglEtc.toVector2(new_min_v3);

        return newRect;
    }

    public void SetHeight(float iheight)
    {
        if (float.IsNaN(iheight) || iheight<=0 ||  iheight > height ) return;
        min_v = new Vector2(min_v.x, max_v.y-iheight);
    }
    public void SetBot(float yPos)
    {
        min_v = new Vector2(min_v.x, yPos);
    }
    public void setTop(float yPos)
    {
        max_v = new Vector2(max_v.x,yPos);
    }
    public void setLeft(float xPos)
    {
        min_v = new Vector2(xPos,min_v.y);
    }
    public void setRight(float xPos)
    {
        max_v = new Vector2(xPos,max_v.y);
    }
    public void SetWidth(float iwidth)
    {
        if (float.IsNaN(iwidth) || iwidth<=0 || iwidth > width) return;
        max_v = new Vector2(min_v.x + iwidth, max_v.y);
    }

    public void ChangWidth(float nwidth)
    {
        float d = nwidth - width;
        max_v.x += d/2;
        min_v.x -= d/2;
    }

    public void ChangHeight(float nheight)
    {
        float d = nheight - height;
        max_v.y += d/2;
        min_v.y -= d/2;
    }

    public void SetCenterX(float ix)
    {
        var v = new Vector2( ix - center.x, 0);
        max_v += v;
        min_v += v; 
    }
    public void Move(Vector2 v)
    {
        max_v += v;
        min_v += v; 
    }
    public void Move(Vector3 v)
    {
        Move(new Vector2(v.x,v.y));
    }
    public hgRect CreateCenterPivotRect()
    {
        hgRect nr = new hgRect(-width/2,height/2,width,height );
        return nr;
    }

    public hgRect CalcRealRect(float[] margin, ALIGN align, float iwidth, float iheight, Vector2 disparea)
    {
        var rbase = CalcPadding(margin);
        hgRect r2 = new hgRect(rbase);
        var height = (iheight<0) ? (r2.IsInifinityHeight() ? disparea.y : r2.height) * (-iheight) : iheight;  
        r2.SetHeight(height);
        //r2.SetHeight(iheight);

        var width = (iwidth<0) ? r2.width * (-iwidth) : iwidth;  //Negative is a ratio.
        r2.SetWidth(iwidth);

        if (align == ALIGN.CENTER)
        {
            r2.SetCenterX(rbase.center.x);
        }
        else if (align == ALIGN.RIGHT)
        {
            var v = new Vector2( rbase._rightX - r2._rightX,0); 
            r2.Move(v);
        }

        return r2;
    }

    public hgRect CalcPadding(float[] pad)  // 0-top, 1-right, 2-bot, 3-left
    {
        if (pad==null||pad.Length!=4) return new hgRect(this);

        Vector2 n_min_v = new Vector2(min_v.x + pad[3], min_v.y + pad[2] );
        Vector2 n_max_v = new Vector2(max_v.x - pad[1], max_v.y - pad[0] );

        return new hgRect(n_max_v,n_min_v);
    }

    public bool CheckContains(hgRect x)
    {
        var tmp = new hgRect(this);
        tmp.Sample(x);
        return CheckEqual(tmp);
    }
    public bool CheckContains(Vector3 v)
    {
        return (
            v.x >= min_v.x && v.x <= max_v.x
            &&
            v.y >= min_v.y && v.y <= max_v.y
            );
    }

    public Vector3[] GetVector3Positions(float z/*=0*/)
    {
        return new Vector3[]
        {
            new Vector3(topLeft.x,  topLeft.y,  z),
            new Vector3(topRight.x, topRight.y, z),
            new Vector3(botLeft.x,  botLeft.y,  z),
            new Vector3(botRight.x, botRight.y, z)
        };
    }

    public override string ToString()
    {
        return string.Format(" TL:({0},{1}),W:{2},H:{3}",_leftX,_topY,width,height );
    }


    public static bool CheckCollision(hgRect _1, hgRect _2)
    {
        /* http://wiki.processing.org/w/Rect-Rect_intersection
        return !(
            (_1._x > _2._x + _2.width )  //x_1 > x_2+width_2
            ||
            (_1._x + _1.width < _2._x)   //x_1+width_1 < x_2
            ||
            (_1._y > _2._y + _2.height)  //y_1 > y_2+height_2 
            ||
            (_1._y + _1.height < _2._y)  //y_1+height_1 < y_2
            );
        */
        float dx = Mathf.Abs(_1.center.x - _2.center.x);
        float dy = Mathf.Abs(_1.center.y - _2.center.y);

        return  (  
            (dx <= _1.ext_w + _2.ext_w ) 
                      &&
            (dy <= _1.ext_h + _2.ext_h )
        );
    }
    public static bool CheckCollision(hgRect frame, List<hgRect> list) //Chack last one only.
    {
        var last = list[list.Count-1];
        hgRect tr = new hgRect(frame);
        tr.Sample(last);
        if (!tr.CheckEqual(last)) return true;
        if (list.Count > 1) for (int i = 0; i < list.Count - 1; i++)
        {
            if (CheckCollision(last,list[i])) return true;
        }
        return false;
    }
    public static float GetLeftPosition(float yPos, hgRect frame, List<hgRect> list)
    {
        
        for (var x = frame._leftX + 1; x < frame._rightX; x++)
        {
            Vector3 v = new Vector3( x, yPos, 0 );
            foreach(var r in list) if (!r.CheckContains(v)) return x;
        }

        return float.NaN;
    }

}
}