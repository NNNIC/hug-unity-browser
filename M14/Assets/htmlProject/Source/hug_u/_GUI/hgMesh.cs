using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using hug_u;

namespace hug_u {
public class hgMesh {

    [System.Serializable]
    public class MeshSet {
        public MeshSet() 
        {
            vlist   = new List<Vector3>();
            vclist  = new List<Color>();
            trilist = new List<int>();
            uvlist  = new List<Vector2>();
            bwlist  = new List<BoneWeight>();
            bnlist  = new List<Transform>();
            bplist  = new List<Matrix4x4>();
            imageList= new List<CD_IMAGE>();
            atlasList= new List<CD_IMAGE>();
            
        }
        public List<Vector3> vlist;
        public List<Color>   vclist;
        public List<int>     trilist;
        public List<Vector2> uvlist;
        public List<Transform>  bnlist;
        public List<BoneWeight> bwlist; 
        public List<Matrix4x4>  bplist;//bind poses
        public List<CD_IMAGE> imageList;
        public List<CD_IMAGE> atlasList;

        public Mesh Create()
        {
            Mesh mesh        = new Mesh();
            mesh.vertices    = vlist.ToArray();
            mesh.colors      = vclist.ToArray();
            mesh.triangles   = trilist.ToArray();
            mesh.uv          = uvlist.ToArray();
            mesh.boneWeights = bwlist.ToArray();
            mesh.bindposes   = bplist.ToArray();
            return mesh;
        }

        public void CreateImage(hgImageRender xgimage, Material mat)
        {
            foreach (var cd in imageList)
            {
                xgimage.Create(mat,cd);
            }
        }
        public void CreateAtlas(Func<int> getBoneIndex)
        {
            foreach (var cd in atlasList)
            {
                hgAtlasRender.Create(cd,getBoneIndex,this);
            }
        }
    }

    [System.Serializable]
    public class CD 
    {
        public Vector3  leftBase;

        public float    hspace;
        public float    vspace;

        // COMMON
        public float fontSize;
        public float fontBase;

        public int   colorIndex;
        public int   effectColorIndex;

        public Vector3[] inner_v;
        public Vector3[] outer_v;

        public bool      isCR;
        public bool      isHR;

        public hglParser.Element xe;

        // [ CHARACTER ATTRIBUTE] = START =
        protected const uint ITALICVAL   =1;        //0
        protected const uint BOLDVAL     =2;        //1
        protected const uint UNDERLINEVAL=4;        //2
        protected const uint STRIKEVAL   =8;        //3
        protected const uint EFFECTMASK  =16+32;    //4,5
        protected const uint SHADOWVAL   =16;       //4
        protected const uint OUTLINEVAL  =32;       //5
        protected uint _attribute;

        public bool italic
        {
            get { return ((_attribute & ITALICVAL) !=0); }
            set { _attribute = value ? ( _attribute | ITALICVAL ) : (_attribute &= ~ITALICVAL ); }
        }
        public bool bold
        {
            get { return ((_attribute & BOLDVAL) !=0); }
            set { _attribute = value ? ( _attribute | BOLDVAL ) : (_attribute &= ~BOLDVAL ); }
        }
        public bool underline
        {
            get { return ((_attribute & UNDERLINEVAL) !=0); }
            set { _attribute = value ? ( _attribute | UNDERLINEVAL ) : (_attribute &= ~UNDERLINEVAL ); }
        }
        public bool strike
        {
            get { return ((_attribute & STRIKEVAL) !=0); }
            set { _attribute = value ? ( _attribute | STRIKEVAL ) : (_attribute &= ~STRIKEVAL ); }
        }

        public enum Effect { NONE, SHADOW, OUTLINE }
        public Effect  effect
        {
            get {
                if ((_attribute & EFFECTMASK) == SHADOWVAL)  return Effect.SHADOW;
                if ((_attribute & EFFECTMASK) == OUTLINEVAL) return Effect.OUTLINE;
                return Effect.NONE;
            }
            set
            {
                _attribute &= ~EFFECTMASK;
                switch(value)
                {
                case Effect.SHADOW: _attribute |= SHADOWVAL;  return;
                case Effect.OUTLINE:_attribute |= OUTLINEVAL; return;
                }
                return;
            }
        }
        // [ CHARACTER ATTRIBUTE] = END =

        //
        public override string ToString()
        {
            if (isCR) return "\\n";
            if (isHR) return "<HR>";
            if (this is CD_CHAR) return  ((CD_CHAR)this).code >= ' ' ? ((CD_CHAR)this).code.ToString() : "\\x" + ((int)((CD_CHAR)this).code).ToString("x");
            if (this is CD_IMAGE)
            {
                var cdi = (CD_IMAGE)this;
                if (cdi.src == CD_IMAGE.SOURCE.TEXTURE) return "Texture:" + cdi.texture.ToString();
                return "Atlas:" + cdi.atlasName;
            }
            return this.ToString();
        }

        public static float GetVSpace(List<CD> list)
        {
            float vspace = -100;
            foreach(var cd in list) vspace = Mathf.Max(vspace,cd.vspace);
            return vspace;
        }
        public static float GetVSpace(List<CD> list, CD add)
        {
            var vspace = GetVSpace(list);
            vspace = Mathf.Max(vspace,add.vspace);
            return vspace;

        }
        public static hgRect CalcRect(List<CD> list)
        {
            hgRect r = new hgRect();
            for(int i = 0;i<list.Count;i++)
            {
                r.Sample(list[i].outer_v);
            }
            return r;
        }
        public static hgRect CalcRectwScan(List<CD> list, CD add,  Vector3 ileftBase) //for wordwrap
        {
            hgRect r = new hgRect();

            Vector3 leftBase =ileftBase;
            for(int i = 0;i<list.Count;i++)
            {
                CD.GetNextRightVertexUV(list,i,null,ref leftBase,-1 );  
                r.Sample(list[i].outer_v);
            }
            if (add!=null) {
                List<CD> tlist= new List<CD>();
                tlist.Add(add);
                CD.GetNextRightVertexUV(tlist,0,null,ref leftBase,-1 );    
	            r.Sample(add.outer_v);
            }

            return r;
        }
        public static void GetNextRightVertexUV(List<CD> list, int index, MeshSet meshSet, ref Vector3 leftOfBase , int boneIndex /*= -1*/ )
        {
            if (list[index] is CD_CHAR)
            {
                CD_CHAR.GetNextRightVertexUV(list, index, meshSet, ref leftOfBase, boneIndex);
            }
            else if (list[index] is CD_IMAGE)
            {
                //CD_IMAGE.GetNextRightVertexUV(list, index, meshSet, ref leftOfBase);
                Vector3 nextLeftOfBase;
                CD_IMAGE.GetNextRightVertexUV(list, index, meshSet, CD_IMAGE.POSMODE.LEFOFBASE,leftOfBase,out nextLeftOfBase, boneIndex );
                leftOfBase = nextLeftOfBase;
            }
            else
            { 
            }
        }
        public static string ToString(List<CD> list)
        {
            if (list==null||list.Count==0) return "none";
            string s = "";
            foreach(var i in list) s+=i.ToString();
            return s;
        }
    }

    public class CD_CHAR : CD
    {
        public static float s_italicAngle = 20;
		public static float s_bold_ratio  = 0.4f;	

        // MODE == CHAR
        public hgBMFontData  bmFont;
        public float    effectShadowParam;
        public float    effectOutlineParam;

               char  _code;
        public char  code
        {
            get { return _code;                                   }
            set { 
                if (value == '\n')   isCR = true; 
                if (value == '\xe1') isHR = true;
                _code = value;  
            }
        }
 

        public Vector2 effect_val;
        public CD_CHAR(hglParser.Element e) { isCR=false; colorIndex = -1; effectColorIndex = -1;  xe = e; _attribute = 0;}
    
        public static void GetNextRightVertexUV(List<CD> list, int index, MeshSet meshSet, ref Vector3 leftOfBase , int boneIndex /*= -1*/ )
        {
            Vector3[] outer_v = null;
            Vector3[] inner_v = null;

            CD base_cd = (list!=null && index < list.Count) ? list[index] : null;
            if (base_cd == null) throw new SystemException("ERROR UNEXPECTED");
			
            CD_CHAR cd =  (base_cd is CD_CHAR) ? (CD_CHAR)base_cd : null;
			if (cd==null) return;

            CD base_firstCd = index-1 >=0 ? list[index-1] : null; 
            CD_CHAR firstCd =  (base_firstCd!=null)&&(base_firstCd is CD_CHAR) ? (CD_CHAR)base_firstCd : null;

            int size,_base, width,height,xoffset,yoffset,xadvance,chnl;
            Vector2[] uvs;
            if (cd.bmFont.GetInfo(cd.code, (firstCd!=null ?  firstCd.code : '\x0' ), out size,out _base ,out uvs, out width, out height, out xoffset, out yoffset, out xadvance, out chnl))
            {
                if (meshSet != null)
                { 
                    Vector3[] tmp_innerv;
                    Vector3[] tmp_localv;
                    switch(cd.effect)
                    {
                        case CD_CHAR.Effect.SHADOW:
                        {   //shadow
                            int x = (int)cd.effect_val.x;  int y = (int)cd.effect_val.y;
                            CreateOneFontMesh(cd, meshSet, leftOfBase, size, _base, width, height, xoffset + x, yoffset + y, xadvance, chnl, uvs,out tmp_innerv, out tmp_localv, true, boneIndex);
                        }                                                     
                        break;                                               
                        case CD_CHAR.Effect.OUTLINE:                                
                        {   //outline                                       
                            int w = (int)cd.effect_val.x;  int y = w;  var quality = (int)cd.effect_val.y;
                            for (int ix = -w; ix <= w; ix++) for (int iy = -y; iy <= y; iy++)
                            {
                                if (quality != 0)
                                {
                                    if (  (ix >= -w+1 && ix <= w-1) 
                                          &&
                                          (iy >= -y+1 && iy <= y-1)
                                    ) continue;
                                }
                                else
                                { 
                                    if (  (ix >= -w+1 && ix <= w-1) 
                                          ||
                                          (iy >= -y+1 && iy <= y-1)
                                    ) continue;
                                }
                                CreateOneFontMesh(cd, meshSet, leftOfBase, size, _base, width, height, xoffset + ix, yoffset + iy, xadvance, chnl, uvs,out tmp_innerv, out tmp_localv, true, boneIndex);
                            }
                        }
                        break;
                    }
                }
                cd.leftBase = leftOfBase;
                leftOfBase = CreateOneFontMesh(cd, meshSet, leftOfBase, size, _base, width, height, xoffset, yoffset, xadvance, chnl, uvs, out inner_v, out outer_v, false,boneIndex);
            }
            cd.inner_v = inner_v;
            cd.outer_v = outer_v;
        }

        private static Vector3 CreateOneFontMesh(CD icd, MeshSet meshSet, Vector3 leftOfBase, int size, int _base ,int width, int height, int xoffset, int yoffset, int xadvance, int chnl, Vector2[] uvs, out Vector3[] inner_v, out Vector3[] localv, bool isEffect, int boneIndex/*=-1*/)
        {
            CD_CHAR cd = (CD_CHAR)icd;
            int save_vsize = meshSet!=null ?  meshSet.vlist.Count : 0;
            float factor = (float)cd.fontSize / (float)size;
            float bold_v = cd.bold ? width * s_bold_ratio * factor : 0;
            float italic_v = cd.italic ? Mathf.Cos((90 - s_italicAngle) * Mathf.Deg2Rad) * height * factor : 0;

            localv = CreateOneMesh_sub(cd, meshSet, ref leftOfBase, _base, size, width, height, xoffset, yoffset, xadvance, factor,  bold_v, italic_v, out inner_v, boneIndex);

            if (meshSet != null)
            { 
                // VColor
                var ncolor = isEffect && cd.effectColorIndex>=0 ? hglHtmlColor.IndexToVColor(cd.effectColorIndex) :  hglHtmlColor.IndexToVColor(cd.colorIndex);
                //if (color2!=null) ncolor = (Color)color2;

                switch(chnl)  // use alpha for chnl
                {
                case 1: ncolor.a = 0;    break;
                case 2: ncolor.a = 1f/64f-1f/128f; break;
                case 4: ncolor.a = 2f/64f-1f/128f; break;
                case 8: ncolor.a = 3f/64f-1f/128f; break;
                }
                meshSet.vclist.AddRange(new Color[] { ncolor, ncolor, ncolor, ncolor });

                // Triangle
                meshSet.trilist.AddRange(new int[] { save_vsize, save_vsize + 1, save_vsize + 3 });
                meshSet.trilist.AddRange(new int[] { save_vsize, save_vsize + 3, save_vsize + 2 });

                // uv
                meshSet.uvlist.AddRange(uvs);

                // bone weights
                if (boneIndex < 0) throw new SystemException("Unexpected!");
                meshSet.bwlist.Add(new BoneWeight(){ boneIndex0 = boneIndex, weight0 = 1}); // v[0]
                meshSet.bwlist.Add(new BoneWeight(){ boneIndex0 = boneIndex, weight0 = 1}); // v[1]
                meshSet.bwlist.Add(new BoneWeight(){ boneIndex0 = boneIndex, weight0 = 1}); // v[2]
                meshSet.bwlist.Add(new BoneWeight(){ boneIndex0 = boneIndex, weight0 = 1}); // v[3]
            }
            // next left of base
            return leftOfBase;
        }

        private static Vector3[] CreateOneMesh_sub(CD cd, MeshSet meshSet, ref Vector3 leftOfBase, int _base, int size, int width, int height, int xoffset, int yoffset, int xadvance, float factor, float bold_v, float italic_v, out Vector3[] inner_v, int boneIndex)
        {
            /*           f = fontsize / size
                 w       h = fontsize;
                         w = xadvance * f
              +-----+
                0-1 | <-yoffset            [0] = ( xoff    ,   -yoff          ) * f
             hgt| | |h                     [1] = ( xoff+wdh,   -yoff          ) * f
                2-3 |                      [2] = ( xoff    ,   -yoff-hgt      ) * f
              +-----+                      [4] = ( xoff+wdt,   -yoff-hgt      ) * f
                ^
                xoffset                 
            */

            var topOfLeft = leftOfBase + _base * factor * Vector3.up;

            // Vertex
            Vector3[] vs = new Vector3[4];
            vs[0] = new Vector3(xoffset, -yoffset, 0) * factor + topOfLeft + italic_v * Vector3.right;
            vs[1] = new Vector3(xoffset + width, -yoffset, 0) * factor + topOfLeft + italic_v * Vector3.right + bold_v * Vector3.right;
            vs[2] = new Vector3(xoffset, -yoffset - height, 0) * factor + topOfLeft;
            vs[3] = new Vector3(xoffset + width, -yoffset - height, 0) * factor + topOfLeft + bold_v * Vector3.right;

            inner_v = vs;

                        // next left of base
            leftOfBase += ((xoffset + xadvance) * factor + bold_v + cd.hspace) * Vector3.right;

            if (meshSet!=null) meshSet.vlist.AddRange(vs);

            /*
                0     1               0 => topOfLeft
                +-----+               1 => [0] + ((xoffset + xadvance) * factor + bold_v + italic_v) * Vector3.right;
                |     |               2 => topOfLeft - size * Vector3,up;
                |     |               3 => [2] + ((xoffset + xadvance) * factor + bold_v) * Vector3.right;
                |     |  <-- base   
                +-----+
                2     3
            */

            Vector3[] outer_v = new Vector3[4];
            
            var outTopOfLeft = yoffset>=0 ? topOfLeft : topOfLeft - yoffset * factor * Vector3.up;
            outer_v[0] = outTopOfLeft;
            outer_v[1] = outer_v[0] + ((xoffset + xadvance) * factor + bold_v + italic_v) * Vector3.right;
            outer_v[2] = topOfLeft - (size * factor) * Vector3.up;
            outer_v[3] = outer_v[2] + ((xoffset + xadvance) * factor + bold_v) * Vector3.right;

            return outer_v;
        }

        public static CD_CHAR CreateSpaceChar(CD referCd,hglParser.Element e)
        {
            if (referCd != null)
            {
                CD_CHAR refcd = (CD_CHAR)referCd;
                hgMesh.CD_CHAR cd = new hgMesh.CD_CHAR(e);
                cd.bmFont = refcd.bmFont;
                cd.code = ' ';
                cd.fontSize = refcd.fontSize;
                cd.vspace = refcd.vspace;
                cd.hspace = refcd.hspace;
                cd.colorIndex = 0;
                return cd;
            }
            else if (e!=null)
            {
                hgMesh.CD_CHAR cd = new hgMesh.CD_CHAR(e);
                cd.bmFont = e.GetTags().m_htmlRender.m_renderInfo.m_bmFont.m_bmFonrData;
                cd.code = ' ';
                cd.fontSize = e.thisStyle.GetFloat(StyleKey.font_size,float.NaN);
                cd.vspace = 0;
                cd.hspace = 0;
                cd.colorIndex = 0;
                return cd;
            }
            return null;

        }
    }

    public class CD_IMAGE : CD
    {
        // MODE == IMAGE

        /*
              width
            +------+
            |      |  height
   yoff->   +------+
          
    Base  o-----------
            ^
            xoff
            
        */

        public enum SOURCE { ATLAS, TEXTURE }
        public SOURCE src;  

        //for ATLAS
        public hgAtlasInfoData atlasInfo;
        public string      atlasName;
        public bool        isBonedAtlas;

        //for Texture
        public Texture     texture;
        public int         id;   // for cache

        // For both
        public float x_offset, y_offset;
        public float width, height;
        public float x_advance, y_advance;
        public Transform   bone;
        
        public enum ALIGN {NONE,LEFT,RIGHT}
        public ALIGN align;

        public CD_IMAGE(hglParser.Element e) {id=-1;  isCR = false;  colorIndex = -1; effectColorIndex = -1;  xe=e; }

        public enum POSMODE
        {
            LEFOFBASE,
            TOPLEFT,
            TOPRIGHT,
        }

        public static void GetNextRightVertexUV(List<CD> list, int index, MeshSet meshSet, POSMODE mode, Vector3 pos, out Vector3 nextLeftOfBase, int boneIndex/* = -1*/ )
        {
            nextLeftOfBase = Vector3.zero;
            CD_IMAGE cd = (CD_IMAGE)list[index];

            //int save_vsize = (meshSet!=null) ? meshSet.vclist.Count : 0;
            float width   = cd.width;
            float height  = cd.height;

            if (cd.width == 0 || cd.height == 0)
            {
                if (cd.src == SOURCE.ATLAS)
                {
                    cd.atlasInfo.GetPerfectSize(cd.atlasName, out width, out height);
                }
                else
                {
                    width  = cd.texture.width;
                    height = cd.texture.height;
                }
            }
 

            /* 
                    ^---------->
            yAdvance|
                        width 
                       +----+
                       |    | height
                 yoff->+----+
                    
              Base  o----------> 
                       ^       ^
                      xoff     xAdvance
            */

            switch(mode)
            {
            case POSMODE.LEFOFBASE: nextLeftOfBase = pos;break;
            case POSMODE.TOPLEFT:   nextLeftOfBase = pos + ( -cd.y_advance) * Vector3.up;  break;
            case POSMODE.TOPRIGHT:  nextLeftOfBase = pos - ( -cd.y_advance) * Vector3.up + (-cd.x_advance) * Vector3.right;break;
            }
            cd.leftBase = nextLeftOfBase;
            Vector3 imageTopLeft =  nextLeftOfBase + new Vector3(cd.x_offset, cd.y_offset,0) + height * Vector3.up;

            // Vertex
            Vector3[] vs = new Vector3[4];
            vs[0] = imageTopLeft;
            vs[1] = vs[0] + width * Vector3.right;
            vs[2] = imageTopLeft + height * Vector3.down;
            vs[3] = vs[2] + width * Vector3.right;
            
            cd.inner_v = vs;

            // next left of base
            nextLeftOfBase += (cd.x_offset + width + cd.hspace) * Vector3.right; 

            /* outer_v 
            
            */
            Vector3[] outer_v = new Vector3[4];
            outer_v[0]  = cd.leftBase + cd.y_advance * Vector3.up;
            outer_v[1]  = outer_v[0]  + cd.x_advance * Vector3.right;
            outer_v[2]  = cd.leftBase;
            outer_v[3]  = outer_v[2]  + cd.x_advance * Vector3.right;
            cd.outer_v = outer_v;            

            //
            if (meshSet != null)
            {
                if (cd.src == SOURCE.ATLAS)
                {
                    if (cd.isBonedAtlas)
                    {
                        cd.bone = meshSet.bnlist[boneIndex];
                        meshSet.atlasList.Add(cd);
                    }
                    else
                    { 
                        int[]     size;
                        Vector2[] r;
                        hgAtlasInfoData.RECTPATTERN pattern;
                        hgAtlasInfoData.StrechMode  smode;
                        Vector2 perfectSize;
                        cd.atlasInfo.GetRect(cd.atlasName,out perfectSize, out r, out pattern,out size, out smode);
                        hgMeshAtlas.DrawAtlas_Sub4(pattern, r,size,perfectSize, meshSet, boneIndex, vs, cd.colorIndex,smode,false);
                    }
                }
                else
                { // SOURCE.IMAGE
                    cd.bone = meshSet.bnlist[boneIndex];
                    meshSet.imageList.Add(cd);
                }
            }
        }

        public static void DrawAtlas_Sub(Rect atlasRect, MeshSet meshSet, int boneIndex, Vector3[] vs, int colorIndex)
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
            var r = atlasRect;//cd.atlasInfo.GetRect(cd.atlasName);
            var uv_left = r.xMin;
            var uv_right = r.xMin + r.width;
            var uv_top = r.yMax;
            var uv_bot = r.yMax - r.height;

            Vector2[] uvs = new Vector2[4];
            uvs[0] = new Vector2(uv_left, uv_top);
            uvs[1] = new Vector2(uv_right, uv_top);
            uvs[2] = new Vector2(uv_left, uv_bot);
            uvs[3] = new Vector2(uv_right, uv_bot);
            meshSet.uvlist.AddRange(uvs);

            // bone weights
            if (boneIndex < 0) throw new SystemException("Unexpected!");
            meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[0]
            meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[1]
            meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[2]
            meshSet.bwlist.Add(new BoneWeight() { boneIndex0 = boneIndex, weight0 = 1 }); // v[3]
        }
        public static void DrawAtlas_Sub2(Vector2[] atlasRect, MeshSet meshSet, int boneIndex, Vector3[] vs, int colorIndex)
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
        public static void DrawAtlas_Sub3(hgAtlasInfoData.RECTPATTERN pattern, Vector2[] atlasRect, MeshSet meshSet, int boneIndex, Vector3[] vs, int colorIndex)
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

        public static void DrawAtlas(hgRect r, float z, hgAtlasInfoData atlasInfo, string atlasPartName, int colorIndex, MeshSet meshSet, int boneIndex, bool frameOnly /*= false*/)
        {
            Vector3[] vs = r.GetVector3Positions(z);
            Vector2[] atlasUvs;
            int[]     size;
            hgAtlasInfoData.RECTPATTERN pattern;
            hgAtlasInfoData.StrechMode  strechmode;
            Vector2   perfectSize;

            atlasInfo.GetRect(atlasPartName, out perfectSize, out atlasUvs, out pattern, out size, out strechmode);
            hgMeshAtlas.DrawAtlas_Sub4(pattern,atlasUvs,size,perfectSize,meshSet,boneIndex,vs,colorIndex,strechmode,frameOnly);
        }
        public static void DrawAtlasFrame(hgRect r, float z, hgAtlasInfoData atlasInfo, string atlasPartName, int colorIndex, MeshSet meshSet, int boneIndex, int[] edgeSize)
        {
            Vector3[] vs = r.GetVector3Positions(z);
            Vector2[] atlasUvs;
            hgAtlasInfoData.RECTPATTERN pattern;
            Vector2   perfectSize;

            atlasInfo.GetRectForceEdgeSize(atlasPartName, edgeSize, out perfectSize, out atlasUvs, out pattern);
            hgMeshAtlas.DrawAtlas_Sub4(pattern,atlasUvs,edgeSize,perfectSize,meshSet,boneIndex,vs,colorIndex,hgAtlasInfoData.StrechMode.STRECH_CENTER,true);
        }

        public static void DrawAtlasREPEAT(hgRect r, float z, Vector2 offset, hgAtlasInfoData atlasInfo, string atlasPartName, int colorIndex, MeshSet meshSet, int boneIndex)
        {
            Vector3[] vs = r.GetVector3Positions(z);
            Vector2[] atlasUvs;
            Vector2   perfectSize;

            atlasInfo.GetRectOneMesh(atlasPartName, out perfectSize, out atlasUvs);
            hgMeshAtlas.DrawAtlas_REPEAT(atlasUvs,perfectSize,offset, meshSet,boneIndex,vs,colorIndex);
        }
    }
 
    public MeshSet CreateMeshSet(Vector3 ileftTop, List<CD> list)
    {
        Vector3 leftTop = ileftTop;

        MeshSet ms = new MeshSet();
        for(int i=0;i<list.Count;i++)
        {
            CD.GetNextRightVertexUV(list,i,ms, ref leftTop,-1);
        }
        return ms;
    }

}
}