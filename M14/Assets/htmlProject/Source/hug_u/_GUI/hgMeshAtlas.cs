using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using hug_u;

namespace hug_u {
public class hgMeshAtlas {

    public static void DrawAtlas_Sub3(hgAtlasInfoData.RECTPATTERN pattern, Vector2[] atlasRect,hgMesh.MeshSet meshSet, int boneIndex, Vector3[] vs, int colorIndex)
    {
        int save_vsize = (meshSet!=null) ? meshSet.vclist.Count : 0;

        // Vertex
        meshSet.vlist.AddRange(vs);

        // Vcolor
        var ncolor = hglHtmlColor.IndexToVColor(colorIndex);
        ncolor.a = 17f / 64f - 1f / 128f;
        meshSet.vclist.AddRange(new Color[] { ncolor, ncolor, ncolor, ncolor });

        // Triangle
        meshSet.trilist.AddRange(new int[] { save_vsize, save_vsize + 1, save_vsize + 3 });
        meshSet.trilist.AddRange(new int[] { save_vsize, save_vsize + 3, save_vsize + 2 });

        /* uv
            0--1     
            |  |
            2--3

        */
        meshSet.uvlist.AddRange(atlasRect);

        // bone weights
        if (boneIndex < 0) throw new SystemException("Unexpected!");
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[0]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[1]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[2]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[3]
    }

    public static void DrawAtlas_Sub4(hgAtlasInfoData.RECTPATTERN pattern, Vector2[] atlasRect, int[] size,Vector2 perfectSize ,hgMesh.MeshSet meshSet, int boneIndex, Vector3[] vs, int colorIndex, hgAtlasInfoData.StrechMode mode, bool frameOnly/*=false*/)
    {
        switch(pattern)
        {
        case hgAtlasInfoData.RECTPATTERN.ONE:            DrawAtlas_ONE(atlasRect,meshSet,boneIndex,vs,colorIndex); break;
        case hgAtlasInfoData.RECTPATTERN.THREEHORIZON:   DrawAtlas_HORIZON(atlasRect,size,perfectSize,meshSet,boneIndex,vs,colorIndex, mode); break;
        case hgAtlasInfoData.RECTPATTERN.THREEVERTIACAL: DrawAtlas_VERTICAL(atlasRect,size,perfectSize,meshSet,boneIndex,vs,colorIndex, mode); break;
        case hgAtlasInfoData.RECTPATTERN.NINE:           DrawAtlas_NINE(atlasRect,size,perfectSize,meshSet,boneIndex,vs,colorIndex,mode,frameOnly); break;
        }
    }

    private static void DrawAtlas_ONE(Vector2[] atlasRect, hgMesh.MeshSet meshSet, int boneIndex, Vector3[] vs, int colorIndex)
    {
        int save_vsize = (meshSet!=null) ? meshSet.vclist.Count : 0;

        // Vertex
        meshSet.vlist.AddRange(vs);

        // Vcolor
        var ncolor = hglHtmlColor.IndexToVColor(colorIndex);
        ncolor.a = 17f / 64f - 1f / 128f;
        meshSet.vclist.AddRange(new Color[] { ncolor, ncolor, ncolor, ncolor });

        // Triangle
        meshSet.trilist.AddRange(new int[] { save_vsize, save_vsize + 1, save_vsize + 3 });
        meshSet.trilist.AddRange(new int[] { save_vsize, save_vsize + 3, save_vsize + 2 });

        /* uv
            0--1     
            |  |
            2--3

        */
        meshSet.uvlist.AddRange(atlasRect);

        // bone weights
        if (boneIndex < 0) throw new SystemException("Unexpected!");
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[0]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[1]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[2]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[3]
    }
        /*
                                        point index 
            +-----+                     [00][01][02][03]
            | | | | } topPx             
            |-----|                     [04][05][06][07]
            | | | |
            |-----|                     [08][09][10][11]
            | | | | } botPx
            +-----+                     [12][13][14][15]
                |   |
                |  rightPx
            LeftPx

    */
    private static void DrawAtlas_HORIZON(Vector2[] auv, int[] size,Vector2 perfectSize, hgMesh.MeshSet meshSet, int boneIndex, Vector3[] vs, int colorIndex, hgAtlasInfoData.StrechMode mode)
    {
        int save_vsize = (meshSet!=null) ? meshSet.vclist.Count : 0;

        float w  = vs[1].x - vs[0].x;
        //float h  = vs[0].y - vs[1].y;

        /*
            [0][1][2][3]  
            [4][5][6][7]
              V  V  V 
              lx |  |
                 cw |
                    rx
        */
        float lx  = size[2]; //auv[1].x - auv[0].x;
        float rx  = size[3]; //auv[3].x - auv[2].x;
        float cw  = perfectSize.x - (lx+rx); if (cw < 0) throw new SystemException("ERROR: UNEXPECTED ATLAS EDGE INFO:TYPE HORIZON");
        float cwh = cw/2f;      

        Vector3[] nvs = null;
        if (mode == hgAtlasInfoData.StrechMode.STRECH_CENTER)
        {
            if (w < lx + rx)
            {
                Vector2[] nv = new Vector2[]
                {
                    auv[1],auv[2],
                    auv[5],auv[6]
                };
                DrawAtlas_ONE(nv,meshSet,boneIndex,vs,colorIndex);
                return;
            }
            nvs = new Vector3[]
            {
                /*0*/ vs[0],
                /*1*/ vs[0] + lx * Vector3.right,
                /*2*/ vs[1] - rx * Vector3.right,
                /*3*/ vs[1],
                /*4*/ vs[2],
                /*5*/ vs[2] + lx * Vector3.right,
                /*6*/ vs[3] - lx * Vector3.right,
                /*7*/ vs[3]
            };
        }
        else
        {
            if (w < cw)
            {
                Vector2[] nv = new Vector2[]
                {
                    auv[0],auv[3],
                    auv[4],auv[7]
                };
                DrawAtlas_ONE(nv,meshSet,boneIndex,vs,colorIndex);
                return;
            }

            /*
                            ctrX
                            |
              vs   [0]      v       [1]
                    +---------------+
                    |    |     |    |
                    +---------------+
                   [2]   ^     ^    [3]
                         |     |
                         |    ctrRX
                        ctrLX

            */

            var ctrX  = (vs[0].x + vs[1].x) / 2;
            var ctrLX = ctrX - cwh;
            var ctrRX = ctrX + cwh;

            nvs = new Vector3[]
            {
                /*0*/ vs[0],
                /*1*/ vs[0].y * Vector3.up + ctrLX * Vector3.right,
                /*2*/ vs[0].y * Vector3.up + ctrRX * Vector3.right,
                /*3*/ vs[1],
                /*4*/ vs[2],
                /*5*/ vs[2].y * Vector3.up + ctrLX * Vector3.right,
                /*6*/ vs[2].y * Vector3.up + ctrRX * Vector3.right,
                /*7*/ vs[3]            
            };
        }

        // Vertex
        meshSet.vlist.AddRange(nvs);

        // Vcolor
        var ncolor = hglHtmlColor.IndexToVColor(colorIndex);
        ncolor.a = 17f / 64f - 1f / 128f;
        meshSet.vclist.AddRange(new Color[] { ncolor, ncolor, ncolor, ncolor,    ncolor, ncolor, ncolor, ncolor });

        // Triangle
        meshSet.trilist.AddRange(new int[] { save_vsize + 0, save_vsize + 1, save_vsize + 5 });
        meshSet.trilist.AddRange(new int[] { save_vsize + 0, save_vsize + 5, save_vsize + 4 });

        meshSet.trilist.AddRange(new int[] { save_vsize + 1, save_vsize + 2, save_vsize + 6 });
        meshSet.trilist.AddRange(new int[] { save_vsize + 1, save_vsize + 6, save_vsize + 5 });

        meshSet.trilist.AddRange(new int[] { save_vsize + 2, save_vsize + 3, save_vsize + 7 });
        meshSet.trilist.AddRange(new int[] { save_vsize + 2, save_vsize + 7, save_vsize + 6 });
            
        /* uv
        */
        meshSet.uvlist.AddRange(auv);

        // bone weights
        if (boneIndex < 0) throw new SystemException("Unexpected!");
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[0]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[1]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[2]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[3]

        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[4]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[5]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[6]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[7] 
    }

    private static void DrawAtlas_VERTICAL(Vector2[] auv,int[] size,Vector2 perfectSize, hgMesh.MeshSet meshSet, int boneIndex, Vector3[] vs, int colorIndex, hgAtlasInfoData.StrechMode mode)
    {
        int save_vsize = (meshSet!=null) ? meshSet.vclist.Count : 0;

        //float w  = vs[1].x - vs[0].x;
        float h  = vs[0].y - vs[2].y;

        /*
            [0][1]
                   } tx
            [2][3]   
                   } ch
            [4][5]
                   } bx
            [6][7]
        */
        float tx = size[0];//auv[0].y - auv[2].y;
        float bx = size[1];//auv[4].y - auv[6].y;
        float ch = perfectSize.y - (tx+bx); if (ch < 0) throw new SystemException("ERROR: UNEXPECTED ATLAS EDGE INFO:TYPE VERTICAL");
        float chh= ch/2f;

        Vector3[] nvs = null ;

        if (mode == hgAtlasInfoData.StrechMode.STRECH_EDGE)
        {
            if (h < tx + bx)
            {
                Vector2[] nv = new Vector2[]
                {
                    auv[2],auv[3],
                    auv[4],auv[5]
                };
                DrawAtlas_ONE(nv,meshSet,boneIndex,vs,colorIndex);
                return;
            }
            nvs = new Vector3[]
            {
                /*0*/ vs[0],
                /*1*/ vs[1],
                /*2*/ vs[0] - tx * Vector3.up,
                /*3*/ vs[1] - tx * Vector3.up,
                /*4*/ vs[2] + bx * Vector3.up,
                /*5*/ vs[3] + bx * Vector3.up,
                /*6*/ vs[2],
                /*7*/ vs[3]
            };
        }
        else
        {
            if (h < ch)
            {
                Vector2[] nv = new Vector2[]
                {
                    auv[0],auv[1],
                    auv[6],auv[7]
                };
                DrawAtlas_ONE(nv,meshSet,boneIndex,vs,colorIndex);
                return;
            }
            /*


            vs [0]  [1] 
                +---+
                |   |
                |---|<---ctrTY
        ctrY--->|   | 
                |---|<---ctrBY
                |   |
                +---+
               [2]  [3]
            */

            var ctrY  = (vs[0].y + vs[2].y) / 2f;
            var ctrTY = ctrY + chh;
            var ctrBY = ctrY - chh;
            nvs = new Vector3[]
            {
                /*0*/ vs[0],
                /*1*/ vs[1],
                /*2*/ vs[0].x * Vector3.right + ctrTY * Vector3.up,
                /*3*/ vs[1].x * Vector3.right + ctrTY * Vector3.up,
                /*4*/ vs[0].x * Vector3.right + ctrBY * Vector3.up,
                /*5*/ vs[1].x * Vector3.right + ctrBY * Vector3.up,
                /*6*/ vs[2],
                /*7*/ vs[3]            
            };
        }
        // Vertex
        meshSet.vlist.AddRange(nvs);

        // Vcolor
        var ncolor = hglHtmlColor.IndexToVColor(colorIndex);
        ncolor.a = 17f / 64f - 1f / 128f;
        meshSet.vclist.AddRange(new Color[] { ncolor, ncolor, ncolor, ncolor,    ncolor, ncolor, ncolor, ncolor });

        // Triangle
        meshSet.trilist.AddRange(new int[] { save_vsize + 0, save_vsize + 1, save_vsize + 3 });
        meshSet.trilist.AddRange(new int[] { save_vsize + 0, save_vsize + 3, save_vsize + 2 });

        meshSet.trilist.AddRange(new int[] { save_vsize + 2, save_vsize + 3, save_vsize + 5 });
        meshSet.trilist.AddRange(new int[] { save_vsize + 2, save_vsize + 5, save_vsize + 4 });

        meshSet.trilist.AddRange(new int[] { save_vsize + 4, save_vsize + 5, save_vsize + 7 });
        meshSet.trilist.AddRange(new int[] { save_vsize + 4, save_vsize + 7, save_vsize + 6 });
            
        /* uv
        */
        meshSet.uvlist.AddRange(auv);

        // bone weights
        if (boneIndex < 0) throw new SystemException("Unexpected!");
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[0]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[1]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[2]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[3]

        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[4]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[5]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[6]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[7] 

    }

    private static void DrawAtlas_NINE(Vector2[] auv,int[] size, Vector2 perfectSize, hgMesh.MeshSet meshSet, int boneIndex, Vector3[] vs, int colorIndex, hgAtlasInfoData.StrechMode mode, bool frameOnly)
    {
        int save_vsize = (meshSet!=null) ? meshSet.vclist.Count : 0;

        float w  = vs[1].x - vs[0].x;
        float h  = vs[0].y - vs[2].y;

        /*
            y [00][01][02][03]
            |                  } tx
            | [04][05][06][07]
            |                  } ch
            | [08][09][10][11]
            |                  } bx
            | [12][13][14][15]
            o--x V   V   V
                 |   |   | 
                 lx  cw  rx
        */
        float tx = size[0];//auv[0].y - auv[4].y;
        float bx = size[1];//auv[8].y - auv[12].y;
        float lx = size[2];//auv[1].x - auv[0].x;
        float rx = size[3];//auv[3].x - auv[2].x;

        float cw = perfectSize.x - (lx+rx); if (cw < 0) throw new SystemException("ERROR: UNEXPECTED ATLAS EDGE INFO:TYPE NINE EDGE #H");
        float cwh= cw/2f;
        float ch = perfectSize.y - (tx+bx); if (ch < 0) throw new SystemException("ERROR: UNEXPECTED ATLAS EDGE INFO:TYPE NINE EDGE #V");
        float chh= ch/2f;

        Vector3[] nvs = null;
        if (mode == hgAtlasInfoData.StrechMode.STRECH_CENTER)
        {
            if (h < tx + bx || w < lx + rx)
            {
                Vector2[] nv = new Vector2[]
                {
                    auv[5],auv[6],
                    auv[9],auv[10]
                };
                DrawAtlas_ONE(nv, meshSet, boneIndex, vs, colorIndex);
                return;
            }
            nvs = new Vector3[]
            {
                /*00*/ vs[0],
                /*01*/ vs[0] + lx * Vector3.right, 
                /*02*/ vs[1] - rx * Vector3.right,
                /*03*/ vs[1],             
                                          
                /*04*/ vs[0]                       - tx * Vector3.up,
                /*05*/ vs[0] + lx * Vector3.right  - tx * Vector3.up,
                /*06*/ vs[1] - rx * Vector3.right  - tx * Vector3.up,
                /*07*/ vs[1]                       - tx * Vector3.up,
                                                                
                /*08*/ vs[2]                       + bx * Vector3.up,
                /*09*/ vs[2] + lx * Vector3.right  + bx * Vector3.up,
                /*10*/ vs[3] - lx * Vector3.right  + bx * Vector3.up,
                /*11*/ vs[3]                       + bx * Vector3.up,
                                           
                /*12*/ vs[2],              
                /*13*/ vs[2] + lx * Vector3.right,
                /*14*/ vs[3] - lx * Vector3.right,
                /*15*/ vs[3]            
            };
        }
        else
        {
            if (w < cw || h < ch)
            {
                Vector2[] nv = new Vector2[]
                {
                    auv[0],auv[3],
                    auv[12],auv[15]
                };
                DrawAtlas_ONE(nv, meshSet, boneIndex, vs, colorIndex);
                return;
            }

            /*
                    ctrX
                      |
            vs [0] [-] [-] [1]
                +--- --- ---+
                |   |   |   |
               [-] [-] [-] [-] <--ctrTY
        ctrY--> |   |   |   |
               [-] [-] [-] [-] <--ctrBY
                |   |   |   |
                +--- --- ---+
               [2] [-] [-] [3]
                    |   |
                    | ctrRX
                  ctrLX
            */
            var ctrX  = (vs[0].x + vs[1].x) / 2;
            var ctrLX = ctrX - cwh;
            var ctrRX = ctrX + cwh;
            var ctrY  = (vs[0].y + vs[2].y) / 2f;
            var ctrTY = ctrY + chh;
            var ctrBY = ctrY - chh;
            
            nvs = new Vector3[]
            {
                /*00*/ vs[0],
                /*01*/ ctrLX * Vector3.right + vs[0].y * Vector3.up, 
                /*02*/ ctrRX * Vector3.right + vs[0].y * Vector3.up, 
                /*03*/ vs[1],             
                                          
                /*04*/ vs[0].x * Vector3.right + ctrTY * Vector3.up,
                /*05*/ ctrLX   * Vector3.right + ctrTY * Vector3.up,
                /*06*/ ctrLX   * Vector3.right + ctrTY * Vector3.up,
                /*07*/ vs[1].x * Vector3.right + ctrTY * Vector3.up,
                                                                
                /*08*/ vs[2].x * Vector3.right + ctrBY * Vector3.up,
                /*09*/ ctrLX   * Vector3.right + ctrBY * Vector3.up,
                /*10*/ ctrRX   * Vector3.right + ctrBY * Vector3.up,
                /*11*/ vs[3].x * Vector3.right + ctrBY * Vector3.up,
                                           
                /*12*/ vs[2],              
                /*13*/ ctrLX * Vector3.right + vs[2].y * Vector3.up,
                /*14*/ ctrRX * Vector3.right + vs[2].y * Vector3.up,
                /*15*/ vs[3]            
            };
        }

        // Vertex
        meshSet.vlist.AddRange(nvs);

        // Vcolor
        var ncolor = hglHtmlColor.IndexToVColor(colorIndex);
        ncolor.a = 17f / 64f - 1f / 128f;
        meshSet.vclist.AddRange(new Color[] { 
            ncolor, ncolor, ncolor, ncolor,    
            ncolor, ncolor, ncolor, ncolor,  
            ncolor, ncolor, ncolor, ncolor,    
            ncolor, ncolor, ncolor, ncolor,  
        });

        // Triangles
        meshSet.trilist.AddRange(new int[] { save_vsize + 0, save_vsize + 1, save_vsize + 5 });
        meshSet.trilist.AddRange(new int[] { save_vsize + 0, save_vsize + 5, save_vsize + 4 });

        meshSet.trilist.AddRange(new int[] { save_vsize + 1, save_vsize + 2, save_vsize + 6 });
        meshSet.trilist.AddRange(new int[] { save_vsize + 1, save_vsize + 6, save_vsize + 5 });

        meshSet.trilist.AddRange(new int[] { save_vsize + 2, save_vsize + 3, save_vsize + 7 });
        meshSet.trilist.AddRange(new int[] { save_vsize + 2, save_vsize + 7, save_vsize + 6 });
        /**/

        meshSet.trilist.AddRange(new int[] { save_vsize + 4, save_vsize + 5, save_vsize + 9 });
        meshSet.trilist.AddRange(new int[] { save_vsize + 4, save_vsize + 9, save_vsize + 8 });

        if (frameOnly == false)
        { 
            meshSet.trilist.AddRange(new int[] { save_vsize + 5, save_vsize + 6, save_vsize + 10 });
            meshSet.trilist.AddRange(new int[] { save_vsize + 5, save_vsize + 10, save_vsize + 9 });
        }
        meshSet.trilist.AddRange(new int[] { save_vsize + 6, save_vsize + 7, save_vsize + 11 });
        meshSet.trilist.AddRange(new int[] { save_vsize + 6, save_vsize + 11, save_vsize + 10 });
        /**/
            
        meshSet.trilist.AddRange(new int[] { save_vsize + 8, save_vsize + 9, save_vsize + 13 });
        meshSet.trilist.AddRange(new int[] { save_vsize + 8, save_vsize + 13, save_vsize + 12});

        meshSet.trilist.AddRange(new int[] { save_vsize + 9, save_vsize + 10, save_vsize + 14 });
        meshSet.trilist.AddRange(new int[] { save_vsize + 9, save_vsize + 14, save_vsize + 13 });

        meshSet.trilist.AddRange(new int[] { save_vsize + 10, save_vsize + 11, save_vsize + 15 });
        meshSet.trilist.AddRange(new int[] { save_vsize + 10, save_vsize + 15, save_vsize + 14 });

        /* uv
        */
        meshSet.uvlist.AddRange(auv);

        // bone weights
        if (boneIndex < 0) throw new SystemException("Unexpected!");
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[0]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[1]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[2]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[3]

        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[4]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[5]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[6]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[7] 

        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[8]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[9]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[10]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[11] 

        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[12]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[13]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[14]
        meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[15]
    }

    public enum REPEATMODE
    {
        NONE,
        REPEAT_X,
        REPEAT_Y,
        REPEAT_XY,
    }


    public class REPEATDATA
    {
        public Vector3[] vs;
        public Vector2[] uvs;
        REPEATDATA()
        {
            vs  = new Vector3[4];
            uvs = new Vector2[4];
        }

        public static REPEATDATA Create(Vector3 startPos,Vector3[] maxVS, Vector2 uvOffset, Vector2 atlasSize, Vector2[] atlasUvs)
        {
            /*
               maxVS[0]                              maxVS[1]
                +---------                           --+
                |                                      |
                |  StartPos & uvOffset
                |   +----------
                |   |       
                |   |

            */

            if (startPos.x < maxVS[0].x || startPos.y > maxVS[0].y) throw new SystemException("Unexpected!");
            if (uvOffset.x < 0 || uvOffset.x > atlasSize.x || uvOffset.y < 0 || uvOffset.y > atlasSize.y)  throw new SystemException("Unexpected!");
            
            REPEATDATA d = new REPEATDATA();
            d.vs[0] = startPos;
            d.vs[1] = startPos + (atlasSize.x - uvOffset.x) * Vector3.right;
            if (d.vs[1].x > maxVS[1].x)
            {
                d.vs[1] = startPos.y * Vector3.up + maxVS[1].x * Vector3.right;
            }
            d.vs[2] = startPos - atlasSize.y * Vector3.up;
            if (d.vs[2].y < maxVS[2].y)
            {
                d.vs[2] = startPos.x * Vector3.right + maxVS[2].y * Vector3.up;
            }
            d.vs[3] = d.vs[2].y * Vector3.up + d.vs[1].x * Vector3.right;

            float atlas_width = atlasUvs[1].x - atlasUvs[0].x;
            float atlas_height= atlasUvs[0].y - atlasUvs[2].y;

            d.uvs[0] = atlasUvs[0] + new Vector2( uvOffset.x / atlasSize.x * atlas_width, uvOffset.y / atlasSize.y * atlas_height);
            d.uvs[1] = d.uvs[0].y * Vector2.up + (d.uvs[0].x + ((d.vs[1].x-d.vs[0].x) / atlasSize.x * atlas_width) ) * Vector2.right;
            d.uvs[2] = (d.uvs[0].y - ((d.vs[0].y - d.vs[2].y) / atlasSize.y * atlas_height)) * Vector2.up + d.uvs[0].x * Vector2.right;
            d.uvs[3] = d.uvs[2].y * Vector2.up + d.uvs[1].x * Vector2.right;

            return d;
        }
    }

    public static void DrawAtlas_REPEAT(Vector2[] atlasUvs,Vector2 atlasSize ,Vector2 offset ,hgMesh.MeshSet meshSet, int boneIndex, Vector3[] vs, int colorIndex)
    {
        List<REPEATDATA> list = new List<REPEATDATA>();

        Vector3 startPos = vs[0];
        
        Vector2 uvOffset = Vector2.zero;
        {
            int ix = (int)offset.x % (int)atlasSize.x;
            int iy = (int)offset.y % (int)atlasSize.y;
            
            float x = ix > 0 ? atlasSize.x - ix : 0;
            float y = iy > 0 ? atlasSize.y - iy : 0;
            uvOffset = new Vector2(x, y); 
        }

        const int loopmax = 1000;
        for(var i = 0;i<=loopmax;i++)
        {
            if (i==loopmax) throw new SystemException("Unexpected! Too much Loop at DrawAtlas_REPEAT");
            REPEATDATA d = REPEATDATA.Create(startPos,vs,uvOffset,atlasSize,atlasUvs);
            list.Add(d);
            if (d.vs[3].x >= vs[3].x)
            {
                if (d.vs[3].y <= vs[3].y) break;
                startPos = d.vs[3].y * Vector3.up + vs[0].x * Vector3.right;
            }
            else
            {
                startPos = d.vs[1];
            }
        }

        if (meshSet==null) return;
        foreach(var d in list)
        {
            int save_vsize =  meshSet.vclist.Count;

            // Vertex
            meshSet.vlist.AddRange(d.vs);

            // Vcolor
            var ncolor = hglHtmlColor.IndexToVColor(colorIndex);
            ncolor.a = 17f / 64f - 1f / 128f;
            meshSet.vclist.AddRange(new Color[] { 
                ncolor, ncolor, ncolor, ncolor,    
            });

            // Triangle
            meshSet.trilist.AddRange(new int[] { save_vsize, save_vsize + 1, save_vsize + 3 });
            meshSet.trilist.AddRange(new int[] { save_vsize, save_vsize + 3, save_vsize + 2 });

            /* uv
                0--1     
                |  |
                2--3

            */
            meshSet.uvlist.AddRange(d.uvs);

            // bone weights
            if (boneIndex < 0) throw new SystemException("Unexpected!");
            meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[0]
            meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[1]
            meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[2]
            meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[3]
        }
    }
  

}
}