using UnityEngine;
using System.Collections;

[System.Serializable]
public class hgAtlasInfoData {

    public string    atlasName;
    public float     width;
    public float     height;

    //public string[]  texname;
    //public Rect[]    rectlist;
    public enum   StrechMode
    {
        STRECH_CENTER,
        STRECH_EDGE,
    }

    [System.Serializable]
    public class DATA
    {
        public string texname;
        public Rect   rect;
        public bool   cutEdge;
        public StrechMode   mode;

        /*
                                          point index 
                +-----+                     [00][01][02][03]
                | | | | } topPx             
                |-----|                     [04][05][06][07]
                | | | |
                |-----|                     [08][09][10][11]
                | | | | } botPx
                +-----+                     [12][13][14][15]
                 V   V
                 |  rightPx
                LeftPx

        */
        public int topPx;
        public int botPx;
        public int leftPx;
        public int rightPx;

        public DATA(){ cutEdge = false; mode = StrechMode.STRECH_CENTER; topPx = botPx = leftPx = rightPx = 0; }
        public DATA(DATA r)
        {
            if (r != null)
            {
                cutEdge = r.cutEdge;
                topPx = r.topPx;
                botPx = r.botPx;
                leftPx = r.leftPx;
                rightPx = r.rightPx;
            }
            else
            {
                cutEdge = false; topPx = botPx = leftPx = rightPx = 0;
            }
        }
    }

    public DATA[]     list;

    private Rect GetRect(string name)
    {
        for (int i = 0; i < list.Length; i++)
        {
            DATA d = list[i];
            if (d.texname == name)
            {
                if (d.cutEdge)
                {
                    return new Rect((d.rect.xMin * width + 0.5f)/width ,(d.rect.yMin * height+0.5f)/height, (d.rect.width * width-1)/width,(d.rect.height *  height-1)/height);   
                }

                return d.rect;
            }
        }
        return new Rect();
    }	

    public bool GetPerfectSize(string name, out float owidth, out float oheight)
    {
        Rect r = GetRect(name);
        if (r.width > 0)
        {
            owidth  = width * r.width;
            oheight = height * r.height;
            return true;
        }
        owidth = 0;
        oheight = 0;
        return true;
    }

    public enum RECTPATTERN { ONE, THREEHORIZON, THREEVERTIACAL, NINE }
    public bool GetRect(string name,out Vector2 perfectSize, out Vector2[] uvs, out RECTPATTERN pattern, out int[] size, out StrechMode mode ) // size[]{top,bot,left,right};
    {
        return GetRect_sub(name,null,out perfectSize,out uvs,out pattern,out size,out mode );
    }
    public bool GetRectForceEdgeSize(string name, int[] edgeSize,out Vector2 perfectSize, out Vector2[] uvs, out RECTPATTERN pattern)
    {
        StrechMode mode;
        int[] dummy;
        return GetRect_sub(name,edgeSize,out perfectSize,out uvs, out pattern, out dummy, out mode);       
    }

    private bool GetRect_sub(string name,int[] forceSize, out Vector2 perfectSize, out Vector2[] uvs, out RECTPATTERN pattern, out int[] size, out StrechMode mode ) // size[]{top,bot,left,right};
    {
        perfectSize = Vector2.zero;
        uvs = null; pattern = RECTPATTERN.ONE;
        size = new int[]{0,0,0,0};
        mode = StrechMode.STRECH_CENTER;

        int topPx = 0,botPx = 0, leftPx = 0, rightPx = 0;

        DATA fd = null;
        for (int i = 0; i < list.Length; i++)
        {
            DATA d = list[i];
            if (d.texname == name)
            {
                fd =d;
                mode = fd.mode;
                perfectSize = new Vector2(fd.rect.width * width, fd.rect.height * height);
                if (forceSize!=null && forceSize.Length==4)
                {
                    topPx = forceSize[0]; botPx = forceSize[1]; leftPx = forceSize[2]; rightPx = forceSize[3];
                }
                else
                {
                    topPx = fd.topPx; botPx = fd.botPx; leftPx = fd.leftPx; rightPx = fd.rightPx;
                }
                break;
            }
        }
        if (fd==null) return false;
        
        Rect r = fd.rect;
        if (fd.cutEdge)
        {
            r = new Rect((fd.rect.xMin * width + 0.5f)/width ,(fd.rect.yMin * height+0.5f)/height, (fd.rect.width * width-1)/width,(fd.rect.height *  height-1)/height);   
            pattern = RECTPATTERN.ONE;
            uvs = new Vector2[]
            {            
                /*0*/  new Vector2(r.xMin,r.yMax),
                /*1*/  new Vector2(r.xMax,r.yMax),
                /*2*/  new Vector2(r.xMin,r.yMin),
                /*3*/  new Vector2(r.xMax,r.yMin)
            };
            return true;
        }
        float tx = (float)topPx   / height;
        float bx = (float)botPx   / height;
        float lx = (float)leftPx  / width;
        float rx = (float)rightPx / width;

        /*
            y [0][1]
            | [2][3]
            o--x
        */
        Vector2[] uvsBase = new Vector2[]
        {
            /*0*/  new Vector2(r.xMin,r.yMax),
            /*1*/  new Vector2(r.xMax,r.yMax),
            /*2*/  new Vector2(r.xMin,r.yMin),
            /*3*/  new Vector2(r.xMax,r.yMin)
        };
        pattern = RECTPATTERN.ONE;
        uvs     = uvsBase;
        if (tx == 0 && bx == 0 && rx == 0 && lx == 0)
        {
            return true;
        }
        if (rx<0 || lx<0 || rx+lx>r.width ) return false;
        if (tx<0 || bx<0 || tx+bx>r.height) return false;
        if (tx == 0 && bx == 0)
        {
            pattern = RECTPATTERN.THREEHORIZON;
            /*
            y [0][1][2][3]
            | [4][5][6][7]
            o--x
            */
            uvs = new Vector2[]
            {
                /*0*/ uvsBase[0],
                /*1*/ uvsBase[0] + lx * Vector2.right,
                /*2*/ uvsBase[1] - rx * Vector2.right,
                /*3*/ uvsBase[1],
                /*4*/ uvsBase[2],
                /*5*/ uvsBase[2] + lx * Vector2.right,
                /*6*/ uvsBase[3] - rx * Vector2.right,
                /*7*/ uvsBase[3]
            };

            size = new int[]
            {
                0,0,
                leftPx,rightPx
            };
            return true;
        }
        if (lx == 0 && rx == 0)
        {
            pattern = RECTPATTERN.THREEVERTIACAL;
            /*
                y [0][1]
                | [2][3]
                | [4][5]
                | [6][7]
                o--x
            */
            uvs = new Vector2[]
            {
                /*0*/ uvsBase[0],
                /*1*/ uvsBase[1],
                /*2*/ uvsBase[0] - tx * Vector2.up,
                /*3*/ uvsBase[1] - tx * Vector2.up,
                /*4*/ uvsBase[2] + bx * Vector2.up,
                /*5*/ uvsBase[3] + bx * Vector2.up,
                /*6*/ uvsBase[2],
                /*7*/ uvsBase[3],
            };
            size = new int[]{
                topPx,botPx,
                0,0
            };
            return true;
        }
        /*
            y [00][01][02][03]
            | [04][05][06][07]
            | [08][09][10][11]
            | [12][13][14][15]
            o--x
        */

        pattern = RECTPATTERN.NINE;
        uvs = new Vector2[]
        {
            /*00*/ uvsBase[0],
            /*01*/ uvsBase[0] + lx * Vector2.right, 
            /*02*/ uvsBase[1] - rx * Vector2.right,
            /*03*/ uvsBase[1],

            /*04*/ uvsBase[0]                       - tx * Vector2.up,
            /*05*/ uvsBase[0] + lx * Vector2.right  - tx * Vector2.up,
            /*06*/ uvsBase[1] - rx * Vector2.right  - tx * Vector2.up,
            /*07*/ uvsBase[1]                       - tx * Vector2.up,

            /*08*/ uvsBase[2]                       + bx * Vector2.up,
            /*09*/ uvsBase[2] + lx * Vector2.right  + bx * Vector2.up,
            /*10*/ uvsBase[3] - lx * Vector2.right  + bx * Vector2.up,
            /*11*/ uvsBase[3]                       + bx * Vector2.up,

            /*12*/ uvsBase[2],
            /*13*/ uvsBase[2] + lx * Vector2.right,
            /*14*/ uvsBase[3] - lx * Vector2.right,
            /*15*/ uvsBase[3]
        };
        size = new int[]{
            topPx ,leftPx,
            leftPx,rightPx
        };

        return true;
    }



    public bool GetRectOneMesh(string atlasPartName, out Vector2 perfectSize, out Vector2[] atlasUvs)
    {
        for (int i = 0; i < list.Length; i++)
        {
            DATA d = list[i];
            if (d.texname == atlasPartName)
            {
                perfectSize = new Vector2(d.rect.width * width, d.rect.height * height);
                var r = d.rect;
                atlasUvs = new Vector2[]
                {
                    /*0*/  new Vector2(r.xMin,r.yMax),
                    /*1*/  new Vector2(r.xMax,r.yMax),
                    /*2*/  new Vector2(r.xMin,r.yMin),
                    /*3*/  new Vector2(r.xMax,r.yMin)
                };
                return true;
            }
        }
        perfectSize = Vector2.zero;
        atlasUvs = null;
        return false;
    }

}
