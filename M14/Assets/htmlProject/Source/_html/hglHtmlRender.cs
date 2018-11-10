using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using hug_u;

public class hglHtmlRender : MonoBehaviour {

    public hglRender m_render;

#if XXX
    public Camera            m_camera;
    public hglHtmlRenderInfo m_renderInfo;
    public hglHtmlColor      m_htmlColor;
    public hglAnchor         m_anchor;
    public hglWindowInfo     m_winInfo;
    public BLOCK             m_root;
    public BLOCK             m_cur;
    public Transform         m_boneRoot;

    //public enum ALIGN
    //{
    //    NONE,
    //    LEFT,CENTER,RIGHT,
    //}

    //public enum VALIGN
    //{
    //    NONE,
    //    TOP,
    //    CENTER,
    //    BOTTOM
    //}

    public class  BASE
    {
        //public bool             disabled;

        /*
         +----------------- <- Margin Rect
         |    (margin)
         |  +-------------- <- Real Rect  (Affected frame and background color)
         |  |  (padding)  
         |  |  +----------- <- Pad  Rect  
         |  |  |               
        */
        public hglParser.Element xe;
        public hgRect            marginRect;     //  
        public hgRect            realRect  ;     //   
        public hgRect            padRect   ;     //  ... equal to Contents Rect
        public BLOCK             parent    ;

        public float[]           margin           ;  public float GetMargin(int i)  { return (margin!=null  && margin.Length==4  && !float.IsNaN(margin[i] )) ? margin[i]  : 0 ;     }
        public float[]           paddingwBorder   ;  public float GetPaddingBorder(int i) { return (paddingwBorder!=null && paddingwBorder.Length==4 && !float.IsNaN(paddingwBorder[i])) ? paddingwBorder[i] : 0 ;     }

        public hgRect            doneMarginRect;     //
        public hgRect            doneRealRect  ;     //
        public hgRect            donePadRect   ;     //
        public hgRect            doneContRect  ;     // No space Rect

        //public float             hieghtIfObjIsCrOnly;
    }

    public class  BLOCK : BASE
    {
        protected hglHtmlRender     htmlRender;
        public    ALIGN             align;
        public    VALIGN            valign;
        public    List<BASE>        list;
        protected INLINE            _curLine;

        public BLOCK(hglHtmlRender hr) {htmlRender = hr; _curLine=null;}

        public void AddCD(hgMesh.CD cd)
        {
            if (_curLine == null)
            {
                var nl = new INLINE();
                list.Add(nl);
                _curLine = nl;
            }
            _curLine.list.Add(cd);
        }

        public BLOCK CreateChildBlock(hglParser.Element e, float[] margin, float[] padding_w_border, ALIGN ialign, VALIGN ivalign, float width=0, float height=0)
        {
            BLOCK nb = new BLOCK(htmlRender) {  xe = e, parent = this};
            nb.marginRect = this.padRect;
            nb.realRect   = this.padRect.CalcRealRect(margin,ialign,width,height,htmlRender.m_winInfo.m_dispArea);   //this.padRect.CalcPadding(margin),
            nb.padRect    = nb.realRect.CalcPadding(padding_w_border);
            nb.margin     = margin;
            nb.paddingwBorder    = padding_w_border;
            nb.align      = ialign;
            nb.valign     = ivalign;
 			nb.list = new List<BASE>();  
            list.Add(nb);

            _curLine = null;
            return nb;
        }

        public BLOCK CreateTableBlock(hglParser.Element e, float vspace, float hspace, ALIGN ialign, float cellpadding=0, float width = 0, float height =0 )
        {
            TABLE table = new TABLE(htmlRender) { xe =e, parent = this};
            table.marginRect = this.padRect;
            //table.realRect   = this.padRect;
            //table.padRect    = this.padRect;
            float[] margin           = new float[] { vspace,hspace,vspace,hspace };
            float[] padding_w_border = new float[] {0,0,0,0};

            table.realRect   = this.padRect.CalcRealRect(margin, /*ialign*/ ALIGN.CENTER, width,height,htmlRender.m_winInfo.m_dispArea);  //<------------------ TEST
            table.padRect    = new hgRect(table.realRect);

            table.vspace = vspace;
            table.hspace = hspace;
            table.align  = ialign;
            table.width  = width;
            table.height = height;
            table.cellpadding = cellpadding;
            list.Add(table);

            return table;
        }
        public BLOCK CreateTableRow(hglParser.Element e,ALIGN ialign, float height)
        {
            TABLE.TR tr = new TABLE.TR(htmlRender) { xe =e, parent =this};
            {
               tr.list = new List<BASE>();
               tr.marginRect = new hgRect(0,0,htmlRender.m_winInfo.m_fixedWidth ,float.MaxValue);
               tr.realRect   = new hgRect(0,0,htmlRender.m_winInfo.m_fixedWidth ,float.MaxValue);
               tr.padRect    = new hgRect(0,0,htmlRender.m_winInfo.m_fixedWidth ,float.MaxValue);
               tr.align = ialign;
               tr.height = height;
            }

            TABLE table = (TABLE)this;
            table.trlist.Add(tr);

            return tr;
        }
        public BLOCK CreateTableData(hglParser.Element e,ALIGN ialign, int colSpan, int rowSpan)
        {
            TABLE table = (TABLE)parent;
            if (!e.thisStyle.HasKey(StyleKey.border_width) && !e.thisStyle.HasKey(StyleKey.border_color) &&  !e.thisStyle.HasKey(StyleKey.border_style))
            {
                e.thisStyle.Set(StyleKey.border,table.xe.thisStyle.Get(StyleKey.border));                    
            }
            //
            var border_width = e.thisStyle.GetFloat(StyleKey.border_width,float.NaN);
            //

            var padding_w_border = new float[]{ table.cellpadding + border_width, table.cellpadding + border_width, table.cellpadding + border_width, table.cellpadding + border_width  };
            
            TABLE.TD td = new TABLE.TD();
            td.block = new BLOCK(htmlRender) { xe=e ,parent=this};
            {
                td.block.list = new List<BASE>();
                var psuedoPadRect       = new hgRect(0,0,htmlRender.m_winInfo.m_fixedWidth ,float.MaxValue);
                td.block.marginRect     = psuedoPadRect;
                td.block.realRect       = psuedoPadRect;
                td.block.padRect        = psuedoPadRect.CalcPadding(padding_w_border);
                td.block.paddingwBorder = padding_w_border;
            }
            td.align = align;
            td.colspan = colSpan;
            td.rowspan = rowSpan;
           
            TABLE.TR tr = (TABLE.TR)this;
            tr.tdlist.Add(td);

            return td.block;
        }

        public override string ToString()
        {
            const int dispMax = 100;
            var all = GetAllCD();
            string s= "";
            if (xe!=null && !string.IsNullOrEmpty(xe.text)) s = xe.text + ">";
            int len = all.Length;

            if (len < dispMax)
            {
                foreach (var cd in all)
                {
                    s += cd.ToString();
                }
            }
            else
            {
                for (int i = 0; i < dispMax; i++)
                {
                    s += all[i].ToString();
                }
                s+="...";
            }
            return s;
        }

        public hgMesh.CD[] GetAllCD()
        {
            List<hgMesh.CD> tmplist = new List<hgMesh.CD>();
            if (list!=null) foreach (var line in list)
            {
                if (line is BLOCK)
                {
                    var cslist = ((BLOCK)line).GetAllCD();
                    tmplist.AddRange(cslist);
                }
                else if (line is INLINE)
                {
					var inline = (INLINE)line;
                    if (inline!=null && inline.list!=null) foreach (var c in inline.list)
                    {
                        tmplist.Add(c);
                    }
                }
            }
            return tmplist.ToArray();
        }

        //-- [FORMAT]--
        public class _FORMATLINE
        {
            public Vector3    leftOfBase; 
            public BASE       obj;
            public Vector3    localPosition;
            //public hgRect     rectCache;

            //public bool       isObjCrOnly;
        }
        //public xgRect _fotmatRect;

        // ############
        // # FORMAT 0 #
        public List<_FORMATLINE> _format0List;        
        public List<hgMesh.CD> _format0GetNewLine(Vector3 leftOfBase)
        {
            if (_format0List==null) _format0List = new List<_FORMATLINE>();
            var fline = new _FORMATLINE() { leftOfBase = leftOfBase, obj = new INLINE()};
            _format0List.Add(fline);
            return ((INLINE)fline.obj).list;
        }
        public void _format0AddBlock(BLOCK block)
        {
            if (_format0List==null) _format0List = new List<_FORMATLINE>();
            var fline = new _FORMATLINE() { obj = block};
            _format0List.Add(fline);
        }
        // # FORMAT 0 #
        // ############

        // ############
        // # FORMAT 1 #
        public List<hgMesh.CD_IMAGE> _format1FixImage;
        public List<_FORMATLINE> _format1List;      
        //public Vector3 realRectLocalPosition;
        public Vector3 padRectLocalPosition;
        public List<hgMesh.CD> _format1GetNewLine(Vector3 leftOfBase)
        {
            if (_format1List==null) _format1List = new List<_FORMATLINE>();
            var fline = new _FORMATLINE() { leftOfBase = leftOfBase, obj = new INLINE()};
            _format1List.Add(fline);
            return ((INLINE)fline.obj).list;
        }
        public void _format1AddBlock(BLOCK block) 
        {
            if (_format1List==null) _format1List = new List<_FORMATLINE>();
            var fline = new _FORMATLINE() { obj = block};
            _format1List.Add(fline);
        }
        public void _format1AddImage(hgMesh.CD_IMAGE cd)
        {
            if (_format1FixImage==null) _format1FixImage = new List<hgMesh.CD_IMAGE>();
            _format1FixImage.Add(cd);
        }
        // # FORMAT 1 #
        // ############
        
        public virtual void Format_1() // Combine inline elements
        {
            bool hasMeshable = false;
            Vector3 LeftObBase      = new Vector3( padRect._leftX,0,0);
            List<hgMesh.CD> nlist   = null;
            Action<BLOCK>   AddBlock = (block) => { _format0AddBlock(block); nlist = null; };
            Action<hgMesh.CD> Check = (cd)    => {
                nlist = nlist==null ? _format0GetNewLine(LeftObBase) : nlist;
                if (cd.isCR)
                {
                    if (nlist.Count==0)
                    {
                        var spaceCd = hgMesh.CD_CHAR.CreateSpaceChar(cd,xe);
                        nlist.Add(spaceCd);
                                                
                    }
                    nlist = null;
                }
                else if (cd.isHR)
                {
                    if (nlist.Count > 0)
                    {
                        nlist = _format0GetNewLine(LeftObBase);
                    }
                    nlist.Add(cd);
                    nlist = null; 
                }
                else
                {
                    nlist.Add(cd);
                }

                if ( !(cd is hgMesh.CD_IMAGE && ((hgMesh.CD_IMAGE)cd).src == hgMesh.CD_IMAGE.SOURCE.TEXTURE) )
                {
                    hasMeshable = true;
                }
            };

            for(int i = 0; i<list.Count;i++)
            {
                BASE o = list[i];
                if (o is BLOCK)
                {
                    BLOCK block = (BLOCK)o;
                    block.Format_1();
                    AddBlock(block);
                }
                else
                {
                    INLINE iline=(INLINE)o;
                    foreach (var cd in iline.list)
                    {
                        Check(cd);
                    }                   
                }
            }

            if (_format0List == null || hasMeshable == false )
            {
                var spaceCd = hgMesh.CD_CHAR.CreateSpaceChar(null,xe);
                if (spaceCd!=null) Check(spaceCd);
            }
        }

        public virtual bool Format_2(bool ShrinkWidth=false, hgRect curRect=null)
        {
            if (curRect == null ) 
            {
                curRect = padRect;
            }
            else
            {
                marginRect.MoveTop(curRect._topY);
                realRect.MoveTop(marginRect._topY - GetMargin(0));
                padRect.MoveTop(realRect._topY    - GetPaddingBorder(0));

                curRect = padRect;
            }

            //Debug.Log("Format_2:Start :" + this.ToString() + "> topY=" + curRect._topY);
            var baseRect = new hgRect(curRect);

            //float totalPadRectSize = 0;
            hglHtmlFormat.FRAMES frames = null;
            float maxX = 0; float maxW = 0;
            for(var loop = 0; loop<100;loop++)
            {
                if (loop == 99) throw new SystemException("Unexpected! Format Loop Limit Exceeded");

                //totalPadRectSize = 0;
                _format1List = null;
                //var yPos = curRect._topY;
                frames = new hglHtmlFormat.FRAMES();
                if (_format1FixImage != null) foreach (var cd in _format1FixImage)
                {
                    frames.AddImg(cd); 
                }
                frames.backFrame = new hgRect(curRect);

                maxX = 0; maxW=0;

                List<hgMesh.CD> nlist = null;

                if (_format0List!=null) foreach (var f in _format0List)
                {
                    if (f.obj is BLOCK)
                    {
                        var block = (BLOCK)f.obj;
                        //Debug.Log("call start:" +block.ToString() + ">frames.yPos="+frames.yPos);
                        if (block.Format_2(ShrinkWidth, frames.backFrame))
                        { 
						    _format1AddBlock(block);
                            frames.yPos = block.doneMarginRect._botY;
                            //Debug.Log("call end:" +block.ToString() + ">frames.yPos="+frames.yPos);

                            maxX = Mathf.Max(maxX,block.doneMarginRect._rightX);
                            maxW = Mathf.Max(maxW,block.doneMarginRect.width);

                        //totalPadRectSize = block.doneMarginRect.height;
                        }
                    }
                    else
                    {
                        INLINE il = (INLINE)f.obj;
                        var list = il.list;
                        int index = 0;
                        for(var loop2=0;loop2<1000;loop2++)
                        {
                            if (index >= list.Count) break;
                            Vector3 oleftOfBase;
                            ALIGN talign = ShrinkWidth  ? ALIGN.LEFT : align;
                            var rc = hglHtmlFormat.FormatLine(frames,list,ref index,talign,out nlist, out oleftOfBase);
                            if (rc == hglHtmlFormat.RESULTCODE.SUCCESS)
                            {
								var r = hgMesh.CD.CalcRectwScan(nlist,null,oleftOfBase);
                                frames.yPos = r._botY;
                                _format1GetNewLine(oleftOfBase).AddRange(nlist);
                                maxX = Mathf.Max(maxX,r._rightX);
                                maxW = Mathf.Max(maxW,r.width);
                                //f.rectCache = r;
                                continue;
                            }
                            else if (rc == hglHtmlFormat.RESULTCODE.FOUND_FIX_IMAGE)
                            {
                                var cdimg = (hgMesh.CD_IMAGE)list[index];
                                _format1AddImage(cdimg);
                                list.RemoveAt(index);
                                goto _LOOP1;
                            }
                            else // ERROR
                            {
                                nlist = list;
                                _format1GetNewLine(oleftOfBase).AddRange(list);
                                frames.yPos = hgMesh.CD.CalcRect(nlist)._botY;
                                break;
                            }
                        }
                    }
                }
                break;
            _LOOP1:;
            }

            // Calc total size of blocks
            if (frames!=null && curRect.IsInifinityHeight())
            {
                donePadRect = new hgRect(baseRect);
                donePadRect.SetBot(frames.yPos);
                if (ShrinkWidth) {
                    if (maxX!=0) donePadRect.max_v.x = maxX;
                }
                doneContRect = new hgRect(donePadRect);
                doneRealRect = new hgRect(donePadRect);
                doneRealRect.setTop(baseRect._topY + GetPaddingBorder(0) );
                doneRealRect.SetBot(frames.yPos - GetPaddingBorder(2));
                doneRealRect.setLeft(realRect._leftX);
                if (!ShrinkWidth) doneRealRect.setRight(realRect._rightX);
                
                doneMarginRect = new hgRect(doneRealRect);
                doneMarginRect.setTop(baseRect._topY + GetPaddingBorder(0) + GetMargin(0) );
                doneMarginRect.SetBot(frames.yPos - GetPaddingBorder(2) - GetMargin(2));
                doneMarginRect.setLeft(marginRect._leftX);
                if (!ShrinkWidth) doneMarginRect.setRight(marginRect._rightX);

            }
            else
            {
                doneContRect    = new hgRect(padRect);
                doneContRect.SetBot(frames.yPos);
                donePadRect     = padRect;
                doneRealRect    = new hgRect(donePadRect);
                doneRealRect.SetBot(donePadRect._botY - GetPaddingBorder(2));
                doneMarginRect = new hgRect(doneRealRect);
                doneMarginRect.SetBot(doneRealRect._botY - GetMargin(2));
            }

            if (xe!=null) xe.formatBase = this;

            return true;
        }
		
        public virtual void Format_3() // Vertical Centering
        {
            hgRect parentPadRect = hgRect.zero;
            if (parent == null || parent.doneMarginRect == null)
            {
                parentPadRect = new hgRect(0,0,htmlRender.m_winInfo.m_fixedWidth,htmlRender.m_winInfo.m_height);
            }
            else
            {
                parentPadRect = parent.donePadRect;
            }
	            
	        if (valign == VALIGN.CENTER)
	        {
	            float hv = parentPadRect.center.y - doneContRect.center.y;
	            Vector3 v = hv * Vector3.up;
	            if (v.sqrMagnitude!=0) Relocate(v);
	        }
#if XXX
	        if (valign == VALIGN.CENTER)
	        {
	            float hv = donePadRect.center.y - doneContRect.center.y; Debug.Log("hv="+hv);
	            Vector3 v = hv * Vector3.up;
	            if (v.sqrMagnitude!=0) Relocate(v);
	        }
#endif

            if (_format1List!=null) foreach (var f in _format1List)
            {
                if (f.obj is BLOCK)
                {
                    var block = (BLOCK)f.obj;
					block.Format_3();
                }
            }
        }

		public void Format_MBONE()
		{	
	        if (_format1List!=null) foreach (var f in _format1List)
            {
                if (f.obj is BLOCK)
                {
                    var block = (BLOCK)f.obj;
					block.Format_MBONE();
					f.localPosition = block.doneRealRect.center - donePadRect.center;
                    //Debug.LogWarning("f.localPosition="+f.localPosition);
                }
                else
                {
                    INLINE il = (INLINE)f.obj;
                    //f.leftOfBase = f.leftOfBase - new Vector3(donepadRect.center.x,donepadRect.center.y,0) ;
                    f.leftOfBase = f.leftOfBase - new Vector3(doneRealRect.center.x,doneRealRect.center.y,0) ;
                }
            }
            if (_format1FixImage!=null) foreach(var cd in _format1FixImage)
            {
                //cd.leftBase = cd.leftBase - new Vector3(donepadRect.center.x,donepadRect.center.y,0);
                cd.leftBase = cd.leftBase - new Vector3(doneRealRect.center.x,doneRealRect.center.y,0);
            }
            padRectLocalPosition = donePadRect.center - doneRealRect.center;
		}
         
        private Transform CreateBone(hgMesh.MeshSet ms, string nm, int index, Transform parent, Vector3 localPos, bool bCalcBindPose)
        {
            var bone = new GameObject(nm+index.ToString()).transform;
            bone.transform.parent = parent;
            bone.localRotation    = Quaternion.identity;
            bone.localScale       = Vector3.one;
            if (float.IsNaN(localPos.x)||float.IsNaN(localPos.y)||float.IsNaN(localPos.z)) 
				throw new SystemException("LocalPos Has NANs. Unexpected! : " + bone.name);
            bone.localPosition    = localPos;
            Matrix4x4 bp = Matrix4x4.identity;
            if (bCalcBindPose) bp = bone.worldToLocalMatrix * parent.localToWorldMatrix;
            ms.bnlist.Add(bone);
            ms.bplist.Add(bp);
            return bone;
        }

        public void Format_MBONE_FINAL(Vector2 topLeft,hgMesh.MeshSet ms, Transform parent)
        {
            int boneIndex = 0;
            Func<int> getBoneIndex = ()=>{ var i = boneIndex; boneIndex++; return i; };
            Format_A_FINAL_sub(topLeft,ms, parent,doneMarginRect.center, getBoneIndex);

            ms.CreateAtlas(getBoneIndex);

        }

        public int Format_A_FINAL_sub(Vector2 topLeft,hgMesh.MeshSet ms, Transform parent, Vector3 localPos, Func<int> getBoneIndex)
        {
            //RealRect
            var realRectboneIndex      = getBoneIndex();
            var objname = this.xe!=null && this.xe.text!=null ? this.xe.text + "." : "RR.";
            //objname  += " : " + localPos.ToString();
            var realRectBone           = CreateBone(ms, objname,realRectboneIndex,parent,localPos,true);
            var realbackRect = doneRealRect.CreateCenterPivotRect();
            realbackRect.Move(localPos);

            if (this.xe!=null) this.xe.bone1 = realRectBone;

            if (xe != null)
            {
                var atlasPart = xe.thisStyle.Get(StyleKey.background_atlas);
                if (!string.IsNullOrEmpty(atlasPart)) 
                {
                    if (xe.mode == hglParser.Mode.TAG && xe.text == "body")
                    {
                        var bgBoneIndex = getBoneIndex();
                        var tmpTopLeft  = parent.InverseTransformPoint(hglEtc.toVector3(topLeft));
                        var bgRect      = new hgRect(tmpTopLeft.x,tmpTopLeft.y, htmlRender.m_winInfo.m_fixedWidth, htmlRender.m_winInfo.m_fixedWidth * (float)Screen.height / (float)Screen.width );
                        //var pos         = parent.InverseTransformPoint(new Vector3(- (float)Screen.width / 2f,(float)Screen.height / 2f));
                        var bgBone      = CreateBone(ms, "Background",bgBoneIndex,parent, Vector3.zero  ,true );
                        htmlRender.DrawBG_AtlasREPEAT(atlasPart,bgRect,0f,Vector2.zero,xe.backcolorIndex,ms,bgBoneIndex);
                        htmlRender.m_camera.GetComponent<hglSlideControl>().m_background = bgBone;
                    }
                    else
                    { 
                        htmlRender.DrawBG_AtlasREPEAT(atlasPart,realbackRect,0f,Vector2.zero,xe.backcolorIndex,ms,realRectboneIndex);
                    }
                }
                if (xe.backcolorIndex >= 0)
                { 
                    if (xe.mode == hglParser.Mode.TAG && xe.text == "body" && htmlRender.m_renderInfo.m_bodyBackColorToCameraColor)
                    {
                        htmlRender.ChangeCameraBackColor(htmlRender.m_htmlColor.GetColor(xe.backcolorIndex));
                    }    
                    else
                    { 
                        htmlRender.DrawBG_COLORINDEX(realbackRect,0f,xe.backcolorIndex,ms,realRectboneIndex);
                    }
                }
                htmlRender.m_camera.GetComponent<hglSlideControl>().m_popup      =  htmlRender.m_winInfo.m_winType == hglWindowType.POPUP ? realRectBone : null;
                CreateFrame(realbackRect,ms,realRectboneIndex);
            }

            var contentsRectBoneIndex  = getBoneIndex();
            var contentsRectBone       = CreateBone(ms,"C.",contentsRectBoneIndex,realRectBone,padRectLocalPosition,true);

            if (this.xe!=null) this.xe.bone2 = contentsRectBone;

            if (_format1List!=null) foreach (var fl in _format1List)
            {
                if (fl.obj is BLOCK)
                {
                    var block = (BLOCK)fl.obj;
                    var blockRealRectBoneIndex = block.Format_A_FINAL_sub(topLeft,ms,contentsRectBone,fl.localPosition,getBoneIndex);
                    if (blockRealRectBoneIndex > 0)
                    { 
                        if (float.IsNaN(fl.localPosition.x)||float.IsNaN(fl.localPosition.y)||float.IsNaN(fl.localPosition.z)) throw new SystemException("fl.localPosition has NANs. Unexpected!");
                    }
                }
                else
                {
                    var iline = (INLINE)fl.obj;
                    var leftOfBase = fl.leftOfBase; //new Vector3(fl.leftOfBase.x,fl.leftOfBase.y + yPos,fl.leftOfBase.z);
                    for(int i=0; i<iline.list.Count;i++)
                    {
                        hgMesh.CD.GetNextRightVertexUV(iline.list,i,ms,ref leftOfBase,contentsRectBoneIndex);
                    }

                    CreateCharLines(iline.list,leftOfBase,ms,contentsRectBoneIndex);
                }
            }
            if (_format1FixImage != null) foreach (var cd in _format1FixImage)
            {
                var leftBase = cd.leftBase;
                List<hgMesh.CD> list = new List<hgMesh.CD>();
                list.Add(cd);
                hgMesh.CD.GetNextRightVertexUV(list,0,ms,ref leftBase,contentsRectBoneIndex);
            }

            //CreateFrame(ms,realRectboneIndex);
            return realRectboneIndex;
        }

        public void FORMAT_1BONE_FINAL(hgMesh.MeshSet ms, Transform parent)
        {
            int boneIndex = 0;
            var bone      = CreateBone(ms,"TEST.",boneIndex,parent,Vector3.zero,true);
            FORMAT_1BONE_FINAL_SUB(ms,boneIndex);
        }
        void FORMAT_1BONE_FINAL_SUB(hgMesh.MeshSet ms, int boneIndex)
        {
            htmlRender.DrawBG_COLOR(doneRealRect,0f,new Color(0,0,0,0),ms,boneIndex);

            if (_format1List!=null) foreach (var fl in _format1List)
            {
                if (fl.obj is BLOCK)
                {
                    var block = (BLOCK)fl.obj;
                    block.FORMAT_1BONE_FINAL_SUB(ms, boneIndex);
                }
                else
                {
                    var iline = (INLINE)fl.obj;
                    var leftOfBase = fl.leftOfBase;
                    for (int i = 0; i < iline.list.Count; i++)
                    {
                        hgMesh.CD.GetNextRightVertexUV(iline.list,i,ms,ref leftOfBase,boneIndex);
                    }
                }
            }
        }

        public void Format_ANCHOR()
        {
            List<hglAnchor.CDDATA> cdlist  = new List<hglAnchor.CDDATA>();
            List<BASE>             blklist = new List<BASE>();
            Format_ANCHOR_sub(cdlist,blklist);
            hglAnchor.CreateAnchor(blklist);
            hglAnchor.CreateAnchor(cdlist);
        }

        private void Format_ANCHOR_sub(List<hglAnchor.CDDATA> cdlist, List<BASE> blklist)
        {
            List<hgMesh.CD> cur = null;
            hglAnchor.Info anchor = null;
            if (_format1List!=null) foreach (var fl in _format1List)
            {
                if (fl.obj is BLOCK)
                {
                    var block = (BLOCK)fl.obj;
                    block.Format_ANCHOR_sub(cdlist,blklist);
                    if (block.xe != null && block.xe.baseBody != null)
                    {
                        var ainfo = ((hglBaseBody)block.xe.baseBody).ainfo;
                        if (ainfo != null)
                        {
                            blklist.Add(block);
                        }
                    }
                }
                else
                {
                    cur = null;
                    var iline = (INLINE)fl.obj;
                    for (int i = 0; i < iline.list.Count; i++)
                    {
                        var cd = iline.list[i];
                        if (cd is hgMesh.CD_IMAGE)
                        {
                            cur = null;
                            anchor = null;
                            if (cd.xe != null && cd.xe.GetAnchor() != null)
                            { 
                                cdlist.Add(new hglAnchor.CDDATA(){ clamp = new List<hgMesh.CD>(){cd} });
                            }
                        }
                        else if (cd is hgMesh.CD_CHAR)
                        {
                            var tanch = (cd.xe!=null) ? cd.xe.GetAnchor() : null;
                            if (tanch == null)
                            {
                                cur = null;
                                anchor = null;
                            }
                            else
                            {
                                if (tanch != anchor)
                                {
                                    cur=null;
                                }
                                anchor = tanch;
                                if (cur == null)
                                {
                                    var d = new hglAnchor.CDDATA();
                                    cdlist.Add(d);
                                    d.clamp = new List<hgMesh.CD>();
                                    cur = d.clamp;
                                }
                                cur.Add(cd);
                            }
                        }
                    }//end of for
                    if (_format1FixImage != null) foreach (var cd in _format1FixImage)
                    {
                        var tanch = (cd.xe!=null) ? cd.xe.GetAnchor() : null;
                        if (tanch != null)
                        {
                            var d = new hglAnchor.CDDATA();
                            d.clamp = new List<hgMesh.CD>(){cd};
                            cdlist.Add(d);
                        }
                    }
                }
            }
        }

        public void Format_onUpdate()
        {
            if (_format1List!=null) foreach (var fl in _format1List)
            {
                if (fl.obj is BLOCK)
                {
                    var block = (BLOCK)fl.obj;
                    block.Format_onUpdate();
                }
                else
                {
                    var iline = (INLINE)fl.obj;
                    for (int i = 0; i < iline.list.Count; i++)
                    {
                        var cd = iline.list[i];
                        if (cd is hgMesh.CD_IMAGE && cd.xe!=null && cd.xe.attrib.ContainsKey("onupdate"))
                        {
                            var cdi = (hgMesh.CD_IMAGE)cd;
                            if (cdi.bone != null)
                            {
                                var xmlScript = htmlRender.m_renderInfo.m_scriptMan;
                                xmlScript.SetUpdate((string)cd.xe.attrib["onupdate"],cdi.bone.gameObject);
                            }
                        }
                    }
                }
            }

            if (xe != null && xe.attrib.ContainsKey("onupdate"))
            {
                var bone = xe.FindBone();
                if (bone != null)
                {
                    var xmlScript = htmlRender.m_renderInfo.m_scriptMan;
                    xmlScript.SetUpdate((string)xe.attrib["onupdate"],xmlScript.gameObject);
                }
            }
        }

        public void Format_onClick()
        {
            if (_format1List!=null) foreach (var fl in _format1List)
            {
                if (fl.obj is BLOCK)
                {
                    var block = (BLOCK)fl.obj;
                    block.Format_onClick();
                }
                else
                {
                    var iline = (INLINE)fl.obj;
                    for (int i = 0; i < iline.list.Count; i++)
                    {
                        var cd = iline.list[i];
                        if (cd is hgMesh.CD_IMAGE && cd.xe!=null && cd.xe.attrib.ContainsKey("onclick"))
                        {
                            var cdi = (hgMesh.CD_IMAGE)cd;
                            if (cdi.bone != null)
                            {
                                var xmlScript = htmlRender.m_renderInfo.m_scriptMan;
                                // xmlScript.SetUpdate((string)cd.xe.attrib["onclick"],cdi.bone.gameObject);
                            }
                        }
                    }
                }
            }

            if (xe != null && xe.attrib.ContainsKey("onclick"))
            {
                var bone = xe.FindBone();
                if (bone != null)
                {
                    var xmlScript = htmlRender.m_renderInfo.m_scriptMan;
                    //xmlScript.SetUpdate((string)xe.attrib["onclick"],xmlScript.gameObject);
                }
            }
        }

        public void CreateCharLines(List<hgMesh.CD> list, Vector3 leftOfBase, hgMesh.MeshSet ms, int boneIndex) // Underline and Strike
        {
            List<List<hgMesh.CD>> underline_clamp = new List<List<hgMesh.CD>>();
            List<List<hgMesh.CD>> strike_clamp    = new List<List<hgMesh.CD>>();

            List<hgMesh.CD> ulist = null;
            List<hgMesh.CD> slist = null;
            foreach (var cd in list)
            {
                if (cd.underline)
                {
                    if (ulist != null)
                    {
                        if (ulist[0].xe == cd.xe)
                        {
                            ulist.Add(cd);
                            continue;
                        }
                        else
                        {
                            underline_clamp.Add(ulist);
                            ulist = null;
                        }
                    }
                    ulist = new List<hgMesh.CD>();
                    ulist.Add(cd);
                }
                else
                {
                    if (ulist != null)
                    {
                        underline_clamp.Add(ulist);
                        ulist = null;
                    }
                }
                if (cd.strike)
                {
                    if (slist != null)
                    {
                        if (slist[0].xe == cd.xe)
                        {
                            slist.Add(cd);
                            continue;
                        }
                        else
                        {
                            strike_clamp.Add(slist);
                            slist = null;
                        }
                    }
                    slist = new List<hgMesh.CD>();
                    slist.Add(cd);
                }
                else
                {
                    if (slist != null)
                    {
                        underline_clamp.Add(slist);
                        slist = null;
                    }
                }
            }
            if (ulist!=null) underline_clamp.Add(ulist);
            if (slist!=null) strike_clamp.Add(slist);

            // Render underlines
            foreach (var clamp in underline_clamp)
            {
                hgRect r = new hgRect();
                foreach(var cd in clamp) r.Sample(cd.outer_v);

                float borderwidth =1;// = clamp[0].fontSize    1;
                {
                    if (clamp[0] is hgMesh.CD_CHAR)
                    {
                        var cd = (hgMesh.CD_CHAR)clamp[0];
                        borderwidth = (cd.fontSize / 32) + 1;
                    }
                }
                var lb = clamp[0].leftBase;
                r.max_v = new Vector2(r.max_v.x, lb.y - 1);
                r.min_v = new Vector2(r.min_v.x, r.max_v.y - borderwidth);
                hgMesh.CD_IMAGE.DrawAtlas(r,0, htmlRender.m_renderInfo.m_atlasInfo.m_data, "white",clamp[0].colorIndex,ms,boneIndex,false);
            }
            // Render strike
            foreach (var clamp in strike_clamp)
            {
                hgRect r = new hgRect();
                foreach(var cd in clamp) r.Sample(cd.outer_v);

                float borderwidth = 1;
                var y = r.min_v.y + r.height / 2;            
                r.max_v = new Vector2(r.max_v.x, y);
                r.min_v = new Vector2(r.min_v.x, r.max_v.y - borderwidth);
                hgMesh.CD_IMAGE.DrawAtlas(r,0, htmlRender.m_renderInfo.m_atlasInfo.m_data, "white",clamp[0].colorIndex,ms,boneIndex,false);
            }

        }

        public void CreateFrame(hgRect r,hgMesh.MeshSet ms,  int boneIndex)
        {
            var w = xe.thisStyle.GetFloat(StyleKey.border_width,float.NaN);
            if (float.IsNaN(w) || w <=0) return;
            var iw = (int)w;
            int[] edgeSize = new int[]{iw,iw,iw,iw};
            var c = xe.thisStyle.GetColorErrorNull(StyleKey.border_color);
            if (c == null)
            {
                c = xe.thisStyle.GetColorErrorNull(StyleKey.color);
            }
            if (c != null)
            {
                xe.frameColorIndex = htmlRender.m_htmlColor.GetNewIndex((Color)c);               
            }
            var style = xe.thisStyle.Get(StyleKey.border_style);
            if (style=="none" || style=="hidden" ) return;
            string atlasParts = "frame";
            if (style == "groove" || style == "ridge" || style == "ridge" || style == "inset" || style == "outset") //ref http://tohoho-web.com/css/prop/border-style.htm
            {
                atlasParts = "frame-3d";
            }

            hgMesh.CD_IMAGE.DrawAtlasFrame(r ,0, htmlRender.m_renderInfo.m_atlasInfo.m_data, atlasParts,xe.frameColorIndex,ms,boneIndex,edgeSize);
        }

        //--- for table ---
        public void Relocate(Vector3 v)
        {
            if (_format1List != null) foreach (var f in _format1List)
            {
                if (f.obj is BLOCK)
                {
                    var block = (BLOCK)f.obj;
                    block.Relocate(v);
                }
                else
                {
                    INLINE il = (INLINE)f.obj;
                    f.leftOfBase += v;
                }
            }
            if (_format1FixImage != null) foreach (var cd in _format1FixImage)
            {
                cd.leftBase += v;
            }

            if (doneContRect!=null) doneContRect.Move(v);
            doneMarginRect.Move(v);
            doneRealRect.Move(v);
            donePadRect.Move(v);
        }

        public void ReAlign()
        {
            if (_format1List != null) foreach (var f in _format1List)
            {
                if (f.obj is BLOCK)
                {
                    var block = (BLOCK)f.obj;
                    block.ReAlign();
                }
                else
                {
                    INLINE il = (INLINE)f.obj;
                    if (xe.thisStyle.Get(StyleKey.text_align) == "left")
                    { 
                        f.leftOfBase.x = GetPaddingBorder(3) + doneMarginRect._leftX;
                    }
                }
            }
        }
    }

    public class TABLE : BLOCK
    {
        public TABLE(hglHtmlRender render) : base(render) {
            trlist = new List<TR>();
        }
        public float vspace;
        public float hspace;
        public float width;
        public float height;
        public float cellpadding;

        public class TR : BLOCK
        {
            public int               rowNum;
            public float             height;
            public List<TD>          tdlist;
            public TR(hglHtmlRender render) : base(render) {
                tdlist = new List<TD>();
            }
            public override void Format_1()
            {
                for (int i = 0; i < tdlist.Count; i++)
                { 
                    tdlist[i].rowNum = rowNum;
                    tdlist[i].colNum = i;
					tdlist[i].block.Format_1();
                }
            }
        }

        public class TD
        {
            public int    rowNum;
            public int    colNum;
            public BLOCK  block;
            public ALIGN  align;
            public int    colspan;
            public int    rowspan;
            public hgRect padRect;
            public hgRect padding_w_padRect;

            public void CreatePadWpaddingRect(float cellpadding)
            {
                padding_w_padRect = new hgRect(padRect);
                padding_w_padRect.min_v -= new Vector2(cellpadding,cellpadding);
                padding_w_padRect.max_v += new Vector2(cellpadding,cellpadding);
            }

            public override string ToString()
            {
                return "colspan="+colspan +",rowspan="+rowspan+",padRect="+padRect;
            }
        }

        public List<TR> trlist;

        public override void Format_1()
        {
            for(int i = 0; i<trlist.Count; i++)
            {
                trlist[i].rowNum = i;
                trlist[i].Format_1();
            }
        }
        public override bool Format_2(bool ShrinkWidth = false, hgRect curRect = null)
        {
            foreach (var tr in trlist)
            {
                foreach (var td in tr.tdlist)
                {
                    if (td.block.Format_2(true))
                    { 
                        td.padRect = td.block.donePadRect;
                    }
                }
            }

            hglCalcTable2 calcTable = new hglCalcTable2(this);

            hgRect tableRealRect, tableMarginRect;
            calcTable.Format(/*this,*/align,curRect,out tableRealRect,out tableMarginRect);
            
            foreach (var tr in trlist)
            {
                foreach (var td in tr.tdlist)
                {
                    _format1AddBlock(td.block);
                }
            }

            donePadRect = tableRealRect; 
            doneRealRect = new hgRect(tableRealRect);
            doneMarginRect = new hgRect(doneRealRect);
            doneMarginRect.setTop(doneRealRect._topY + vspace);

            return true;
        }
        
    }
    public class INLINE : BASE
    {
        public bool  isFixed;
        public float vspace;
        public List<hgMesh.CD> list;
        public INLINE() { list = new List<hgMesh.CD>(); vspace = 0; isFixed = false;}

        public override string ToString()
        {
            if (list == null || list.Count==0) return "(nolist)"; 
            string d="";

            foreach (var cd in list)
            {
                d += cd.ToString();
            }
            return d;
        }
    }

    // ############################
    // # METHODS FOR hglTags_Body #
    public void CreateRootBlock( float width, float height)
    {
        m_root = new BLOCK(this) {
            xe = null,
            marginRect = new hgRect(0,0,width,height),
			realRect   = new hgRect(0,0,width,height),
            padRect    = new hgRect(0,0,width,height),
            parent     = null,

            list = new List<BASE>(),
        };
        m_cur = m_root;
    }
    
    public void CreateChildBlock(hglParser.Element e, bool bMoveToChild=true)
    {
		float[] margin = null;
		float[] padding = null;	
        float   height  = 0;
        float   width   = 0;
        ALIGN   align   = ALIGN.NONE;
        VALIGN  valign  = VALIGN.NONE;
		{
 			margin  = e.thisStyle.GetMargin();
        	padding = e.thisStyle.GetPadding_w_border();
            height  = e.thisStyle.GetFloat_allowPercent(StyleKey.height,0);
            width   = e.thisStyle.GetFloat_allowPercent(StyleKey.width,0);
            var als = e.thisStyle.Get(StyleKey.text_align);
            switch (als)
            {
            case "center" : align = ALIGN.CENTER; break;
            case "right"  : align = ALIGN.RIGHT;  break;
            }            
            if (align == ALIGN.NONE)
            {
                if (margin[1]== (float)(-0xa) && margin[3]== (float)(-0xa))  // means auto!
                {
                    align = ALIGN.CENTER;
                }
            }
            if (margin[0] == (float)(-0xa) && margin[2] == (float)(-0xa))
            {
               valign = VALIGN.CENTER;
            }
            else valign = VALIGN.TOP;

            for(int i = 0; i<margin.Length;i++) margin[i] = margin[i]>=0 ? margin[i] : 0;
		}

        var child =  m_cur.CreateChildBlock(e,margin,padding,align,valign,width,height);

        if (bMoveToChild)
        {
            m_cur = child;
        }
    }
    public void CreateTableBlock(hglParser.Element e, ALIGN align, float cellpadding, float hspace, float vspace, float width, float height  )
    {                                                 
        m_cur = m_cur.CreateTableBlock(e,vspace,hspace,align,cellpadding,width,height);
    }
    public void CreateTableRow(hglParser.Element e, ALIGN align, float height)
    {
        m_cur = m_cur.CreateTableRow(e,align,height);
    }
    public void CreateTableData(hglParser.Element e, ALIGN align, int colspan, int rowspan)
    {
        m_cur = m_cur.CreateTableData(e,align,colspan,rowspan);
    }
    public void MoveToParent()
    {
        m_cur = m_cur.parent;
    }

    public enum TextMode
    {
        RAW,
        NORMALIZE
    }

    private void SetupCdCommon(hglParser.Element e, hgMesh.CD cd)
    {
        cd.xe = e;
    }

    public void WriteText(hglParser.Element e,string text, TextMode mode)
    {        
        string src = text;
        if (mode == TextMode.NORMALIZE)
        {
            src = hglEtc.NormalizeText(src);
            src = hglEtc.decodeTextToDisplay(src);
        }
        else
        {
            src = hglEtc.DeleteUnsuportedChar(src);
            src = hglEtc.decodeTextToDisplay(src);
        }
		if (string.IsNullOrEmpty(src)) return;
		
        var fontSize = e.thisStyle.GetFloat(StyleKey.font_size,float.NaN);
        var hspace   = e.thisStyle.GetFloat(StyleKey.letter_spacing,float.NaN);  hspace = float.IsNaN(hspace) ? 0 : hspace;
        var color    = e.thisStyle.GetColor(StyleKey.color);
        var linehit  = e.thisStyle.GetFloat(StyleKey.line_height,float.NaN);
        bool italic  = false;
        {
            var i = e.thisStyle.Get(StyleKey.font_style);
            if (!string.IsNullOrEmpty(i) && 
                ( i == "italic" || i == "oblique")
               )
            {
                italic = true;
            }
        }
        bool bold = false;
        {
            var i = e.thisStyle.Get(StyleKey.font_weight);
            if (!string.IsNullOrEmpty(i) && i.Contains("bold")) bold = true;
        }
        bool underline = false;
        bool strike    = false;
        {
            var i = e.thisStyle.Get(StyleKey.text_decoration);
            if (!string.IsNullOrEmpty(i) && i == "underline")
            {
                underline =true;
            }
            if (!string.IsNullOrEmpty(i) && i == "line-through")
            {
                strike =true;
            }
        }
        hgMesh.CD_CHAR.Effect effect     = hgMesh.CD_CHAR.Effect.NONE;
        Vector2               effect_val = Vector2.one;
        {
            float x,y; Color col;
            if (hglConverter.GetTextOutLineStyle(e.thisStyle.Get(StyleKey.text_outline), out x, out y, out col))
            { 
                effect = hgMesh.CD_CHAR.Effect.OUTLINE;
                e.parent.effectColorIndex = m_htmlColor.GetNewIndex(col);
                effect_val = new Vector2(x,y);
                
            }
            else if (hglConverter.GetTextShadowStyle(e.thisStyle.Get(StyleKey.text_shadow), out x, out y, out col))
            { 
                effect = hgMesh.CD_CHAR.Effect.SHADOW;
                e.parent.effectColorIndex = m_htmlColor.GetNewIndex(col);
                effect_val = new Vector2(x,y);
            }
        }

        float vspace = 0;
        if (!float.IsNaN(linehit) )
        {
            vspace = linehit;
        }

        m_htmlColor.SetColor(e.parent.colorIndex,color);


        for (int i = 0; i < src.Length; i++)
        {
            hgMesh.CD_CHAR cd = new hgMesh.CD_CHAR(e);
            SetupCdCommon(e,(hgMesh.CD)cd);  SetupCdCommon(e,(hgMesh.CD)cd);
			cd.bmFont           = m_renderInfo.m_bmFont.m_bmFonrData;
            cd.code             = src[i];
            cd.fontSize         = fontSize;
            cd.vspace           = vspace;
            cd.hspace           = hspace;
            cd.italic           = italic;
            cd.bold             = bold;
            cd.underline        = underline;
            cd.strike           = strike;
            cd.effect           = effect;
			cd.colorIndex       = e.parent.colorIndex;
            cd.effectColorIndex = e.parent.effectColorIndex;
            cd.effect_val       = effect_val;
            
            m_cur.AddCD(cd);        
        }
    }

    public void WriteCR(hglParser.Element e)
    {
        WriteText(e,"\n",TextMode.RAW);
    }

    public void DrawHR(hglParser.Element e)
    {
        var width  = hglEtc.GetFloat("width" , e.attrib);  hglEtc.ifNanSetZero(ref width);
        var height = hglEtc.GetFloat("height", e.attrib);  hglEtc.ifNanSetZero(ref height);
        var vspace = hglEtc.GetFloat("vspace", e.attrib);  hglEtc.ifNanSetZero(ref vspace);
        var hspace = hglEtc.GetFloat("hspace", e.attrib);  hglEtc.ifNanSetZero(ref hspace);
        
        var alignStr  = (string)e.attrib["align"]; 

        var cd = new hgMesh.CD_IMAGE(e);  SetupCdCommon(e,(hgMesh.CD)cd);

        cd.src = hgMesh.CD_IMAGE.SOURCE.ATLAS;
        cd.atlasInfo = m_renderInfo.m_atlasInfo.m_data;
        cd.atlasName = "HR";
        cd.isBonedAtlas = m_renderInfo.m_bonedAtlas;
 
        cd.width  =  width==0  ? 32 : width;
        cd.height =  height==0 ? 4  : height; 
        cd.align  = hgMesh.CD_IMAGE.ALIGN.NONE;

        cd.x_offset  = 0;
        cd.x_advance = 32;

        cd.y_offset  = 15;
        cd.y_advance = 32;

		m_cur.AddCD(cd); 	       
    }

    public void DrawImage(hglParser.Element e)
    {
        var width  = hglEtc.GetFloat("width" , e.attrib,0f);
        var height = hglEtc.GetFloat("height", e.attrib,0f);

        float mtop=0,mright=0,mbot=0,mleft=0; // Margin : TOP/RIGHT/BOTTOM/LEFT
        {
            var vspace = hglEtc.GetFloat("vspace", e.attrib,0f);
            if (vspace > 0) { mtop = vspace; mbot = vspace;}
            var hspace = hglEtc.GetFloat("hspace", e.attrib,0f);
            if (hspace > 0) { mright=hspace; mleft=hspace; }
        }
        if (e.thisStyle.HasKey(StyleKey.width))
        {
            width = e.thisStyle.GetFloat(StyleKey.width,float.NaN);
        }
        if (e.thisStyle.HasKey(StyleKey.height))
        {
            height = e.thisStyle.GetFloat(StyleKey.height,float.NaN);
        }
        if (e.thisStyle.HasKey(StyleKey.margin_top))
        {
            mtop = e.thisStyle.GetFloat(StyleKey.margin_top,float.NaN);
        }
        if (e.thisStyle.HasKey(StyleKey.margin_right))
        {
            mright = e.thisStyle.GetFloat(StyleKey.margin_right,float.NaN);
        }
        if (e.thisStyle.HasKey(StyleKey.margin_bottom))
        {
            mbot  = e.thisStyle.GetFloat(StyleKey.margin_bottom,float.NaN);
        }
        if (e.thisStyle.HasKey(StyleKey.margin_left))
        {
            mleft = e.thisStyle.GetFloat(StyleKey.margin_left,float.NaN);
        }
        var alignStr  = (string)e.attrib["align"]; 
   
        
        hgMesh.CD_IMAGE.ALIGN align = hgMesh.CD_IMAGE.ALIGN.NONE;
        switch(alignStr)
        {
        case "left" : align = hgMesh.CD_IMAGE.ALIGN.LEFT; break;
        case "right": align = hgMesh.CD_IMAGE.ALIGN.RIGHT; break;
        }
        
        var cd = new hgMesh.CD_IMAGE(e); SetupCdCommon(e,(hgMesh.CD)cd);

        var src       = (string)e.attrib["src"];
        var atlasName = (string)e.attrib["atlas"];
        if (atlasName != null)
        { 
            cd.src = hgMesh.CD_IMAGE.SOURCE.ATLAS;
            cd.atlasInfo = m_renderInfo.m_atlasInfo.m_data;
            cd.atlasName = atlasName;
            cd.isBonedAtlas = m_renderInfo.m_bonedAtlas;
        }
        else if (src != null)
        {
            cd.src = hgMesh.CD_IMAGE.SOURCE.TEXTURE;
            cd.texture = ((hglBaseBody)e.baseBody).hglTags.m_resman.GetTexture(m_winInfo.CreateFullPath(src));
        }
        else
        {
            return;
        }
        cd.width  = width;
        cd.height = height; 
        cd.align  = align;

        cd.x_offset  = mleft;
        cd.x_advance = width + mleft + mright;

        cd.y_offset  = mbot;
        cd.y_advance = height + mtop + mbot;

		m_cur.AddCD(cd);  	
    }

    public void ChangeCameraBackColor(Color color)
    {
        m_camera.backgroundColor = color;
    }
    // # METHODS FOR hglTags_Body #
    // ############################

    public void DrawBG_COLOR(hgRect r,float z,Color col,hgMesh.MeshSet ms,int boneIndex)
    {
        var colorIndex = m_htmlColor.GetNewIndex(col);
        var atlasPart = string.IsNullOrEmpty(m_renderInfo.m_backboardAtlas) ? "white" : m_renderInfo.m_backboardAtlas;
        hgMesh.CD_IMAGE.DrawAtlas(r,z,m_renderInfo.m_atlasInfo.m_data,atlasPart,colorIndex,ms,boneIndex,false);
    }
     
    public void DrawBG_COLORINDEX(hgRect r,float z,int colorIndex,hgMesh.MeshSet ms,int boneIndex)
    {
        var atlasPart = string.IsNullOrEmpty(m_renderInfo.m_backboardAtlas) ? "white" : m_renderInfo.m_backboardAtlas;
        hgMesh.CD_IMAGE.DrawAtlas(r,z,m_renderInfo.m_atlasInfo.m_data,atlasPart,colorIndex,ms,boneIndex,false);
    }
    public void DrawBG_AtlasREPEAT(string atlasPart, hgRect r, float z, Vector2 offset,int colorIndex,hgMesh.MeshSet ms, int boneIndex )
    {
        hgMesh.CD_IMAGE.DrawAtlasREPEAT(r,z,offset,m_renderInfo.m_atlasInfo.m_data,atlasPart,colorIndex,ms,boneIndex);
    }

    public void Format(hgMesh.MeshSet ms )
    {
        m_root.Format_1();
        m_root.Format_2();  
        m_root.Format_3();
        
        if (m_renderInfo.m_1bone)
        {
            m_root.FORMAT_1BONE_FINAL(ms,transform);
        }
        else
        {
		    m_root.Format_MBONE();
            transform.localPosition = m_renderInfo.m_renderTopLeft;
            m_root.Format_MBONE_FINAL(m_renderInfo.m_renderTopLeft,ms,transform); //Debug.Log(">>>>>>>>>>>>>> " + m_renderInfo.m_renderTopLeft);
            //m_root.Format_MBONE_FINAL(new Vector2(-640,0),ms,transform);
        }
    }

    public void OutputRendering()
    {
        hgMesh.MeshSet ms = new hgMesh.MeshSet();
    
        Format(ms);

        var render = GetComponent<SkinnedMeshRenderer>();
        render.sharedMesh           = ms.Create();
        render.bones                = ms.bnlist.ToArray();
        render.material             = m_renderInfo.m_fontMaterial;
        render.material.renderQueue = m_winInfo.GetRenderingOrder();

        var xgImage = new hgImageRender();
        ms.CreateImage(xgImage,m_renderInfo.m_imgMaterial);
        
        m_htmlColor.ApplyAll();
        renderer.material.SetTexture("_PalTex",m_htmlColor.GetTexture());

        m_root.Format_ANCHOR();

        m_root.Format_onUpdate();

    }
#endif

    public void Init()
    {
        m_render = new hglRender();
        m_render.m_renderInfo = (hglHtmlRenderInfo)hgca.FindAscendantComponent(gameObject,typeof(hglHtmlRenderInfo));

        //m_renderInfo = (hglHtmlRenderInfo)hgca.FindAscendantComponent(gameObject,typeof(hglHtmlRenderInfo));
        gameObject.AddComponent<SkinnedMeshRenderer>();
        m_render.m_winInfo = (hglWindowInfo)hgca.FindAscendantComponent(gameObject,typeof(hglWindowInfo));

        m_render.m_htmlColor = new hglHtmlColor();
        m_render.m_anchor    = new hglAnchor();
    }

    void LateUpdate()
    {
        m_render.m_htmlColor.Apply();
    }

}
