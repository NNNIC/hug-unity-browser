using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using hug_u;

namespace hug_u {
public class hgImageRender {
    /*
        Management of Image Objects (* NOT Atlas)
    */

    [System.Serializable]
    public class DATA
    {
        public GameObject go;
        public hgRect     rect;
    }

    public List<DATA> m_list;

    public hgImageRender() {m_list = new List<DATA>();}
	
    public void Create( Material mat,hgMesh.CD_IMAGE cd)
    {
        if (m_list==null) m_list = new List<DATA>();

        int cur = cd.id;
        DATA dt = null;
        GameObject obj = null;
        MeshFilter mf  = null;

        hgRect r = new hgRect(cd.inner_v);

        if (cur < 0)
        { 
            dt = new DATA();
            m_list.Add(dt);
            cur = m_list.Count - 1;
			
			if (cd.texture==null) return; 
			
			obj = new GameObject(cd.texture.name);            
            obj.AddComponent<MeshFilter>();
            obj.AddComponent<MeshRenderer>();
            obj.renderer.material =  new Material(mat); 
            obj.renderer.material.mainTexture=cd.texture;
            mf  = obj.GetComponent<MeshFilter>();
            mf.mesh = hgUserMesh.CreateOneRectangle(r.width,r.height);
            mf.mesh.RecalculateBounds();
            obj.transform.parent = cd.bone;
            obj.transform.localScale    = Vector3.one;
            obj.transform.localPosition = new Vector3(r.center.x, r.center.y,0);

            cd.bone = obj.transform;

            if (cd.xe!=null) cd.xe.bone1 = obj.transform;
        }
        //else 
        //{
        //    dt = m_list[cur];
        //    obj = dt.go;
        //    mf  = obj.GetComponent<MeshFilter>();
        //    if (r.CheckSizeNotEqual(dt.rect))
        //    {
        //        mf.mesh = UserMesh.CreateOneRectangle(r.width,r.height);
        //        mf.mesh.RecalculateBounds();
        //    }
        //    if (obj.renderer.material.mainTexture != cd.texture)
        //    {
        //        obj.renderer.material.mainTexture=cd.texture;
        //    }
        //    if (r.CheckPosition(dt.rect))
        //    {
        //        obj.transform.localPosition = new Vector3(r.center.x, r.center.y,0);
        //    }
        //}
        //
        //dt.go = obj;
        //dt.rect = r;
        //
        //cd.id = cur;
    }
}
public class hgAtlasRender {
    /*
        Management of Atlas Objects 
    */


    public static void Create(hgMesh.CD_IMAGE cd,Func<int> getBoneIndex,hgMesh.MeshSet ms )
    {
        int boneIndex = getBoneIndex();

        hgRect r = new hgRect(cd.inner_v);
        Vector2[] atlas_r;
        hgAtlasInfoData.RECTPATTERN pattern;
        hgAtlasInfoData.StrechMode  mode;
        int[]     size;
        Vector2   perfectSize;
        cd.atlasInfo.GetRect(cd.atlasName,out perfectSize,out atlas_r,out pattern,out size, out mode);
        hgMeshAtlas.DrawAtlas_Sub4(pattern,atlas_r,size,perfectSize,ms,boneIndex,cd.inner_v,cd.colorIndex,mode,false);

        var abone = CreateBone(ms,cd.atlasName + ".",boneIndex,cd.bone,new Vector3(r.center.x,r.center.y,0),true);
        if (cd.xe!=null) cd.xe.bone1 = abone;
    }
    private static Transform CreateBone(hgMesh.MeshSet ms, string nm, int index, Transform parent, Vector3 localPos, bool bCalcBindPose)
    {
        var bone = new GameObject(nm+index.ToString()).transform;
        bone.transform.parent = parent;
        bone.localRotation    = Quaternion.identity;
        bone.localScale       = Vector3.one;
        if (float.IsNaN(localPos.x)||float.IsNaN(localPos.y)||float.IsNaN(localPos.z)) throw new SystemException("LocalPos Has NANs. Unexpected!");
        bone.localPosition    = localPos;
        Matrix4x4 bp = Matrix4x4.identity;
        if (bCalcBindPose) bp = bone.worldToLocalMatrix * parent.localToWorldMatrix;
        ms.bnlist.Add(bone);
        ms.bplist.Add(bp);
        return bone;
    }
}
}