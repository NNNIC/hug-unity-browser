using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using hug_u;

public class hglHtmlFormat {

    public class FRAMES
    {
        public hgRect backFrame;
        public List<hgRect> leftList;
        public List<hgRect> rightList;
        public float yPos
        {
            get { return backFrame._topY; }
            set { backFrame.max_v.y = value;}
        }

        public void AddImg(hgMesh.CD_IMAGE cd)
        {
            if (leftList==null) leftList = new List<hgRect>();
            if (rightList==null) rightList = new List<hgRect>();

            hgRect r = new hgRect(cd.outer_v);

            if (cd.align == hgMesh.CD_IMAGE.ALIGN.LEFT) leftList.Add(r);
            else if (cd.align == hgMesh.CD_IMAGE.ALIGN.RIGHT) rightList.Add(r);
        }

        public float[] GetSpace(float y=float.NaN)
        {
            if (float.IsNaN(y)) y = yPos;
            if (y<backFrame.min_v.y || y>yPos) return null;

            float left = backFrame.min_v.x;
            if (leftList!=null)foreach (var r in leftList)
            {
                if (y <= r.max_v.y && y>=r.min_v.y) left = Mathf.Max(left, r.max_v.x);
            }
            float right= backFrame.max_v.x;
            if (rightList!=null) foreach (var r in rightList)
            {
                if (y <= r.max_v.y && y>=r.min_v.y) right = Mathf.Min(right, r.min_v.x);
            }

            return new float[]{left,right};
        }

        public override string ToString()
        {
            if (backFrame==null) return "no backframe";
            return "yPos=" + yPos;
        }
    }

    public enum RESULTCODE
    {
        SUCCESS,
        //SUCCESS_BLANKLINE,
        FOUND_FIX_IMAGE,
        ERROR,
    }

    public static RESULTCODE FormatLine(FRAMES frame, List<hgMesh.CD> list, ref int index, ALIGN align, out List<hgMesh.CD> newList, out Vector3 leftBase)
    {
        if (index == 0 && list[index].isHR)
        {
            return FormatLine_NORMAL(frame,list,ref index, align, out newList,out leftBase);
        }
        else
        { 
            return FormatLine_NORMAL(frame,list,ref index, align, out newList,out leftBase);
        }
    }
    private static RESULTCODE FormatLine_NORMAL(FRAMES frame, List<hgMesh.CD> list, ref int index, ALIGN align, out List<hgMesh.CD> newList, out Vector3 leftBase)
    {
        leftBase = Vector3.zero;
        newList   = null;
        float left,right,width,top;
        {
            float[] space = frame.GetSpace();
            if (space == null) return RESULTCODE.ERROR;
            left = space[0];
            right =space[1];
            width =space[1]-space[0];
            top = frame.yPos;
        }

        leftBase = new Vector3(left,top,0);
        int      startIndex = index;
        for(int loop = 0;loop<100;loop++)
        {
            index = startIndex;
            newList  = new List<hgMesh.CD>();
            while (index < list.Count)
            {
                var cd = list[index];
                if (cd.isCR)
                {
                    if (index == 0)  newList.Add(cd);
                    index++;
                    goto _ALIGN;
                }
                else if (cd is hgMesh.CD_IMAGE)
                {
                    var cdimg = (hgMesh.CD_IMAGE)cd;
                    if (cdimg.align == hgMesh.CD_IMAGE.ALIGN.LEFT)
                    {
                        SetImageLeft(list, index, left, top);
                        return RESULTCODE.FOUND_FIX_IMAGE;
                    }
                    else if (cdimg.align == hgMesh.CD_IMAGE.ALIGN.RIGHT)
                    {
                        SetImageRight(list, index, right, top);
                        return RESULTCODE.FOUND_FIX_IMAGE;                    
                    }
                }                

                var r      = hgMesh.CD.CalcRectwScan(newList,cd,leftBase);
                var vspace = hgMesh.CD.GetVSpace(newList,cd);
                if (r._topY + vspace > top)
                {
                    var y = leftBase.y - ( (r._topY + vspace) - top) - 1; // -1 : For calculation error.
                    leftBase = new Vector3(leftBase.x, y, leftBase.z);
                    goto _LOOP1;
                }
                if (r._rightX > right)
                {
                    goto _ALIGN;
                }
                newList.Add(cd);
                index++;
            }
            goto _ALIGN;
            

        _LOOP1:;
        }
        return RESULTCODE.ERROR;

    _ALIGN:
        {
            if (align == ALIGN.CENTER)
            { 
                var r = hgMesh.CD.CalcRectwScan(newList,null,newList[0].leftBase);
                var center_X = frame.backFrame.center.x;
                var r_c = r.MoveCenter(center_X);
                if (r_c._leftX < left)
                {
                    var v = left - r_c._leftX;
                    r_c.MoveX(v);
                }
                else if (r_c._rightX > right)
                {
                    var v = right - r_c._rightX;
                    r_c.MoveX(v);
                }

                leftBase = new Vector3(r_c._leftX,leftBase.y,leftBase.z);
            }
            else if(align == ALIGN.RIGHT)
            {
                var r = hgMesh.CD.CalcRectwScan(newList,null,newList[0].leftBase);
                var v = right - r._rightX;
                r.MoveX(v);
                leftBase = new Vector3(r._leftX,leftBase.y,leftBase.z);
            }            
        }

        return RESULTCODE.SUCCESS;
    }


    private static void SetImageRight(List<hgMesh.CD> list, int index, float right, float top)
    {
        //list[index] is Fix Image!
        Vector3 dummy;
        hgMesh.CD_IMAGE.GetNextRightVertexUV(list, index, null, hgMesh.CD_IMAGE.POSMODE.TOPRIGHT, new Vector3(right, top, 0), out dummy,-1);
    }

    private static void SetImageLeft(List<hgMesh.CD> list, int index, float left, float top)
    {
        //list[index] is Fix Image!
        Vector3 dummy;
        hgMesh.CD_IMAGE.GetNextRightVertexUV(list, index, null, hgMesh.CD_IMAGE.POSMODE.TOPLEFT, new Vector3(left, top, 0), out dummy,-1);
    }

    private static RESULTCODE FormatLine_2(FRAMES frame, List<hgMesh.CD> list, ref int index, ALIGN align, out List<hgMesh.CD> newList, out Vector3 leftBase)
    {
        leftBase = Vector3.zero;
        newList   = null;
        float left,right,width,top;
        {
            float[] space = frame.GetSpace();
            if (space == null) return RESULTCODE.ERROR;
            left = space[0];
            right =space[1];
            width =space[1]-space[0];
            top = frame.yPos;
        }

        leftBase = new Vector3(left,top,0);
        int      startIndex = index;
        for(int loop = 0;loop<100;loop++)
        {
            index = startIndex;
            newList  = new List<hgMesh.CD>();
            while (index < list.Count)
            {
                var cd = list[index];
                if (cd.isCR)
                {
                    if (index == 0)  newList.Add(cd);
                    index++;
                    goto _ALIGN;
                }
                else if (cd is hgMesh.CD_IMAGE)
                {
                    var cdimg = (hgMesh.CD_IMAGE)cd;
                    if (cdimg.align == hgMesh.CD_IMAGE.ALIGN.LEFT)
                    {
                        SetImageLeft(list, index, left, top);
                        return RESULTCODE.FOUND_FIX_IMAGE;
                    }
                    else if (cdimg.align == hgMesh.CD_IMAGE.ALIGN.RIGHT)
                    {
                        SetImageRight(list, index, right, top);
                        return RESULTCODE.FOUND_FIX_IMAGE;                    
                    }
                }           

                var r      = hgMesh.CD.CalcRectwScan(newList,cd,leftBase);
                var vspace = hgMesh.CD.GetVSpace(newList,cd);
                if (r._topY + vspace > top)
                {
                    var y = leftBase.y - ( (r._topY + vspace) - top);
                    leftBase = new Vector3(leftBase.x, y, leftBase.z);
                    goto _LOOP1;
                }
                if (r._rightX > right)
                {
                    goto _ALIGN;
                }
                newList.Add(cd);
                index++;
            }
            goto _ALIGN;
            

        _LOOP1:;
        }
        return RESULTCODE.ERROR;

    _ALIGN:
        {
            if (align == ALIGN.CENTER)
            { 
                var r = hgMesh.CD.CalcRectwScan(newList,null,newList[0].leftBase);
                var center_X = frame.backFrame.center.x;
                var r_c = r.MoveCenter(center_X);
                if (r_c._leftX < left)
                {
                    var v = left - r_c._leftX;
                    r_c.MoveX(v);
                }
                else if (r_c._rightX > right)
                {
                    var v = right - r_c._rightX;
                    r_c.MoveX(v);
                }

                leftBase = new Vector3(r_c._leftX,leftBase.y,leftBase.z);
            }
            else if(align == ALIGN.RIGHT)
            {
                var r = hgMesh.CD.CalcRectwScan(newList,null,newList[0].leftBase);
                var v = right - r._rightX;
                r.MoveX(v);
                leftBase = new Vector3(r._leftX,leftBase.y,leftBase.z);
            }            
        }

        return RESULTCODE.SUCCESS;
    }
}
