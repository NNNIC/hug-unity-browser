using UnityEngine;
using System.Collections;

public class hgUserMesh : MonoBehaviour {

    //function Start()
    //{
    //	var mf: MeshFilter = GetComponent(MeshFilter);
    //	//mf.mesh = CreateOneRectangle(5,5);
    //	//mf.mesh = CreatePlane_NxM(2,2);
    //	mf.mesh = CreatePlane_NxM_1by1UV(2,2,1.0,1.0,0.0,0.0);
    //	
    //}

    // AddComponent Mesh Fullter
    // AddComponent Mesh Renderer

    public static Mesh CreateOneRectangle(float width, float height)
    {
	    Vector3[] verts    = new Vector3[4];
	    Vector3[] normals  = new Vector3[4];
	    Vector2[] uv       = new Vector2[4];
	    int[]     tri      = new int[6];
	    /*
          v2    v3
           +----+
           |    |
           |    |
           +----+   
          v0    v1
        */
        float hw = width  / 2f;
        float hh = height / 2f;

   	    verts[0] = new Vector3(-hw, -hh, 0);
	    verts[1] = new Vector3(+hw, -hh, 0);
	    verts[2] = new Vector3(-hw, +hh, 0);
	    verts[3] = new Vector3(+hw, +hh, 0);

        for (int i = 0; i < normals.Length; i++) {
		    normals[i] = UnityEngine.Vector3.up;
	    }
	
	    uv[0] = new Vector2(0, 0);
	    uv[1] = new Vector2(1, 0);
	    uv[2] = new Vector2(0, 1);
	    uv[3] = new Vector2(1, 1);
	
	    tri[0] = 0;
	    tri[1] = 2;
	    tri[2] = 3;
	
	    tri[3] = 0;
	    tri[4] = 3;
	    tri[5] = 1;
	
	    Mesh mesh  = new Mesh();
	    mesh.vertices = verts;
	    mesh.triangles = tri;
	    mesh.uv = uv;
	    mesh.normals = normals;
	
	    return mesh;
    }

    public static GameObject CreateRectangleGameObject(Color color, string shaderstr)
    {
        GameObject obj = new GameObject();
        obj.AddComponent<MeshFilter>();
        obj.AddComponent<MeshRenderer>();
        MeshFilter mf   = obj.GetComponent<MeshFilter>();
        mf.mesh = CreateOneRectangle(1f,1f);
        mf.mesh.RecalculateBounds();
        Texture2D texture = new Texture2D( 1, 1,   TextureFormat.ARGB32, false); 
        texture.SetPixel(0,0,color);
        texture.Apply();

        Shader shader = Shader.Find(shaderstr);
        if (shader==null) 
        {
            shader = Shader.Find("Diffuse");
        }
        obj.renderer.material =  new Material(shader); 
        obj.renderer.material.mainTexture=texture;

        return obj;
    }
    public static GameObject CreateRectangleGameObjectInEditMode(Color color, string shaderstr)
    {
        GameObject obj = new GameObject();
        obj.AddComponent<MeshFilter>();
        obj.AddComponent<MeshRenderer>();
        MeshFilter mf   = obj.GetComponent<MeshFilter>();
        mf.sharedMesh = CreateOneRectangle(1f,1f);
        mf.sharedMesh.RecalculateBounds();
        Texture2D texture = new Texture2D( 1, 1,   TextureFormat.ARGB32, false); 
        texture.SetPixel(0,0,color);
        texture.Apply();

        Shader shader = Shader.Find(shaderstr);
        if (shader==null) 
        {
            shader = Shader.Find("Diffuse");
        }
        obj.renderer.sharedMaterial =  new Material(shader); 
        obj.renderer.sharedMaterial.mainTexture=texture;

        return obj;
    }
    public static GameObject CreateRectangleGameObjectInEditMode()
    {
        GameObject obj = new GameObject();
        obj.AddComponent<MeshFilter>();
        obj.AddComponent<MeshRenderer>();
        MeshFilter mf   = obj.GetComponent<MeshFilter>();
        mf.sharedMesh = CreateOneRectangle(1f,1f);

        Material mat = Resources.LoadAssetAtPath("Assets/Models/Materials/Default-Mat.mat",typeof(Material)) as Material;
        if (mat!=null)
        {
            obj.renderer.sharedMaterial =  mat; 
        }
        else 
        {
            Shader   shader = Shader.Find("Diffuse");
            obj.renderer.sharedMaterial =  new Material(shader); 
        }

        return obj;
    }

    public static Mesh CreatePlane_MxN(int m, int n)
    {
	    if (m<=0) Debug.LogError("n should be more than 0"); 
	    if (n<=0) Debug.LogError("m should be more than 0"); 

	    /*  
		    ex) 2 x 2

		       6 - 7 - 8
	 	    y  | / | / |		
	        ,  3 - 4 - 5	
		    n  | / | / |	
		    ^  0 - 1 - 2 
		    | 
	        + -> m ,x 
	    */
	    int i  ;//General
	    int j  ;//General
	    int k  ;//General
	
	    int mx    = m+1;
	    int nx    = n+1;
	
	    //float mf  = m;
	    //float nf  = n;
	
	    Vector3[] vers      = new Vector3[mx * nx];
	    Vector3[] noms      = new Vector3[mx * nx]; 
	    Vector2[] uv        = new Vector2[mx * nx];
	    float rect_w        =  1.0f / (float)m;
	    float rect_h        =  1.0f / (float)n;
			
	    for(j=0;j<nx;j++) for(i=0;i<mx;i++) 
	    {
		    vers[j*mx + i] = new Vector3( i, j, 0.0f);
		    noms[j*mx + i] = UnityEngine.Vector3.up;
		    uv  [j*mx + i] = new Vector2( rect_w * i, rect_h * j);	
	    }
	
	    int[] tris   = new int[3 * 2 * n * m];
	    /*
	       j*m -  j*m+1  -  j*m+2 - j*m+m-1
	        :       :         :       :
	        |       |         |       |   
		    m  -   m+1   -   m+2  - m+m-1
		    |       |         |       |
		    0  -    1    -    2    - m-1
		
		    |
		    V
	   
	      (j+1)*mx+i - (j+1)*mx+i+1
		     |      /      |                     
		    j*mx+i   -   j*mx+i+1 
						
	    */
	    k=0;
	    for(j=0;j<n;j++) for(i=0;i<m;i++) 
	    {
		    /*
		    v2 -  v3   
	        |  /  |
	        v0 -  v1		

		    tri1 = v0 , v2 , v3
		    tri2 = v0 , v3 , v1
		    */
	
		    int v0  = j*mx+i;
		    int v1  = j*mx+i+1;
		    int v2  = (j+1)*mx+i;
		    int v3  = (j+1)*mx+i+1; 
		
		    tris[k++] = v0;
		    tris[k++] = v2;
		    tris[k++] = v3;

		    tris[k++] = v0;
		    tris[k++] = v3;
		    tris[k++] = v1;
	    }

	    Mesh me  = new Mesh();
	    me.vertices   = vers;
	    me.normals    = noms;
	    me.triangles  = tris;
	    me.uv         = uv; 
	
	    return me;
    }

    public static Mesh CreateTextPlane_MxN(int m, int n, float xlen, float ylen, float xpad, float ypad, Rect uvrect)
    {
	    int i ; // General
	    int j ; // General
	
	    /*
		    n
		    ^
		    |
	       10---11 15---16
		    |   |   |   |
		    |   |   |   |
		    8---9  13---14
		                    <- ypad
		    2---3   6---7   
		    |   |   |   |
		    |   |   |   |
		    0---1   4---5   -> m
		          ^
		          |
		          xpad
		      
		    trianlge = [0,2,3] [0,3,1]      
	    */
	
	    int max_vers  = n * m * 4;
	    int max_tris  = n * m * 2 * 3;
	
	    Vector3[] vertices     = new Vector3[max_vers];
	    Vector3[] normals      = new Vector3[max_vers];
	    Vector2[] uvs          = new Vector2[max_vers];
	    int[]     triangls     = new int    [max_tris];
	
	    for(j = 0;j<n;j++) for(i = 0;i<m;i++)
	    {
		    int v_idx0     = (j * m  + i) * 4;
		    int v_idx1     = v_idx0 + 1;
		    int v_idx2     = v_idx0 + 2;
		    int v_idx3     = v_idx0 + 3;
		
		    Vector3 v_v0   = new Vector3(
									    xlen * i + (xpad * (i-1)),
									    ylen * j + (ypad * (j-1)),
								        0.0f
								    );
		    Vector3 v_v1     = v_v0 + new Vector3(xlen,0,0);
		    Vector3 v_v2     = v_v0 + new Vector3(0,ylen,0);
		    Vector3 v_v3     = v_v0 + new Vector3(xlen,ylen,0);
		
		    vertices[v_idx0] = v_v0;
		    vertices[v_idx1] = v_v1;
		    vertices[v_idx2] = v_v2;
		    vertices[v_idx3] = v_v3;
		
		    normals[v_idx0]  = Vector3.up;
		    normals[v_idx1]  = Vector3.up;
		    normals[v_idx2]  = Vector3.up;
		    normals[v_idx3]  = Vector3.up;
		

		    /*
			    v2  v3        s2    s3
			 
			    v0  v1        s0    s1       
		    */

		    uvs[v_idx0]      = new Vector2(uvrect.x              ,uvrect.y);
		    uvs[v_idx1]      = new Vector2(uvrect.x+uvrect.width ,uvrect.y);
		    uvs[v_idx2]      = new Vector2(uvrect.x              ,uvrect.y+uvrect.height);
		    uvs[v_idx3]      = new Vector2(uvrect.x+uvrect.width ,uvrect.y+uvrect.height);
		
		    int t_index      = (j * m + i) * 3 * 2;
		
		    triangls[t_index++] = v_idx0;		
		    triangls[t_index++] = v_idx3;		
		    triangls[t_index++] = v_idx1;
		
		    triangls[t_index++] = v_idx0;		
		    triangls[t_index++] = v_idx2;		
		    triangls[t_index++] = v_idx3;		
				
	    }

	    Mesh me       = new Mesh();
	    me.vertices   = vertices;
	    me.normals    = normals;
	    me.triangles  = triangls;
	    me.uv         = uvs; 
	    return me;
    }


    public class plane_MxN_UV
    {
	    private MeshFilter m_mf ;
	    private int m_m ;
	    private int m_n ;

        public plane_MxN_UV(MeshFilter mf)
	    {
		    m_mf = mf;
	    }


        public void Create(int m, int n, float xlen, float ylen, float xpad, float ypad, Rect uvrect)
	    {
		    m_m = m;
		    m_n = n;
		    Mesh mesh = hgUserMesh.CreateTextPlane_MxN(m,n,xlen,ylen,xpad,ypad,uvrect);
		    m_mf.mesh = null;
		    m_mf.mesh = mesh;

		    return;
	    }

        public void UpdateIdentical(Rect uvrect)
	    {
		    int i ; // General
		    int j ; // General
		
		     Vector2[] uvs = m_mf.mesh.uv;
		
		    for(j=0;j<m_n;j++) for(i=0;i<m_m;i++)
		    {
			    //var index = j * m_m + i;
			
			    Rect r     = uvrect;
			
			    int v_idx0  = (j * m_m  + i) * 4;
			    int v_idx1  = v_idx0+1;
			    int v_idx2  = v_idx0+2;
			    int v_idx3  = v_idx0+3;
			
			    /*
				    v2  v3        s2    s3
				 
				    v0  v1        s0    s1       
			    */
			
			    uvs[v_idx0] = /*s0*/ new Vector2(r.x        ,r.y         );
			    uvs[v_idx1] = /*s1*/ new Vector2(r.x+r.width,r.y         );
			    uvs[v_idx2] = /*s2*/ new Vector2(r.x        ,r.y+r.height);
			    uvs[v_idx3] = /*s3*/ new Vector2(r.x+r.width,r.y+r.height);
		    }
		
		    m_mf.mesh.uv = null;
		    m_mf.mesh.uv = uvs;
		    uvs = null;
	    }

        public void UpdateIndividual(Rect[] uvrects)
	    {
		    int i ; // General
		    int j ; // General
		
		    Vector2[] uvs = m_mf.mesh.uv;
		
		    for(j=0;j<m_n;j++) for(i=0;i<m_m;i++)
		    {
			    var index = j * m_m + i;
			    if (index >= uvrects.Length) break;
			
			    Rect r    = uvrects[index];
			
			    int v_idx0 = (j * m_m  + i) * 4;
			    int v_idx1 = v_idx0+1;
			    int v_idx2 = v_idx0+2;
			    int v_idx3 = v_idx0+3;
			
			    /*
				    v2  v3        s2    s3
				 
				    v0  v1        s0    s1       
			    */
			
			    uvs[v_idx0] = /*s0*/new Vector2(r.x        ,r.y         );
			    uvs[v_idx1] = /*s1*/new Vector2(r.x+r.width,r.y         );
			    uvs[v_idx2] = /*s2*/new Vector2(r.x        ,r.y+r.height);
			    uvs[v_idx3] = /*s3*/new Vector2(r.x+r.width,r.y+r.height);
		    }
		    m_mf.mesh.uv = null;
		    m_mf.mesh.uv = uvs;
		    uvs = null;		
	    }	
    };

};




//class plane_NxM_UV
//{
//	private var m_n : int;
//	private var m_m : int;
//
//	function Create(n : int, m : int, xlen : float, ylen : float, xpad : float, ypad : float, uvrect : Rect) : Mesh
//	{
//		m_n = n;
//		m_m = m;
//	
//		/*
//			i.e. 1 x 1
//			
//		   12---13-14--15
//	  ypad->|    | |   | 
//			8----9-10--11
//	  0.0-->|    | |   |
//			4----5-6---7
//			|    | |   |
//			|    | |   |
//			|    | |   |
//			0----1-2---3
//		          ^   ^
//		          |   |
//		         0.0  xpad
//		
//		*/
//		var i : int ;//General
//		var j : int ;//General
//		var k : int ;//General
//	
//		var mesh : Mesh = UserMesh.CreatePlane_NxM( n * 3 , m * 3);
//		var uvs  : Vector2[] = mesh.uv;
//		var	vers : Vector3[] = mesh.vertices;
//	
//		var v0 : Vector3 = Vector3(0,0,0);
//		var v1 : Vector3 = Vector3(xlen,0,0);
//		var v2 : Vector3 = Vector3(xlen,0,0);
//		var v3 : Vector3 = Vector3(xlen+xpad,0,0);
//		
//		var v4 : Vector3 = v0 + Vector3(0,ylen,0);
//		var v5 : Vector3 = v1 + Vector3(0,ylen,0);
//		var v6 : Vector3 = v2 + Vector3(0,ylen,0);
//		var v7 : Vector3 = v3 + Vector3(0,ylen,0);
//		
//		var v8  : Vector3 = v0 + Vector3(0,ylen,0);
//		var v9  : Vector3 = v1 + Vector3(0,ylen,0);
//		var v10 : Vector3 = v2 + Vector3(0,ylen,0);
//		var v11 : Vector3 = v3 + Vector3(0,ylen,0);
//	
//		var v12 : Vector3 = v0 + Vector3(0,ylen+ypad,0);
//		var v13 : Vector3 = v1 + Vector3(0,ylen+ypad,0);
//		var v14 : Vector3 = v2 + Vector3(0,ylen+ypad,0);
//		var v15 : Vector3 = v3 + Vector3(0,ylen+ypad,0);
//		
//		/*
//		    s2   s3
//			
//			s0   s1
//			
//			
//			t0  = Vector2(1.0,0.0);  //<-- Transparent point of UV. Use it for padding spaces.
//		*/
//		var t0 : Vector2 = Vector2(255.0/256.0,0.0);
//	
//		var s0 : Vector2 = Vector2(uvrect.x              ,  uvrect.y + uvrect.height);
//		var s1 : Vector2 = Vector2(uvrect.x+uvrect.width ,  uvrect.y + uvrect.height);
//		var s2 : Vector2 = Vector2(uvrect.x              ,  uvrect.y);
//		var s3 : Vector2 = Vector2(uvrect.x+uvrect.width ,  uvrect.y);	
//				
//		// adjust
//		var nx : int = n * 3 + 1;
//		for(j=0;j<m;j++) for(i=0;i<n;i++)
//		{
//			/*
//			   * Index 
//				r2=(j*3+2)*nx+i*3   
//				r1=(j*3+1)*nx+i*3   
//				r0= j*3   *nx+i*3   
//				
//				r2 - r2+1  r2+2 r2+3
//				 |     |    |
//			    r1   r1+1  r1+2 r1+3
//				 |     |    |
//				 |     |    |
//				r0   r0+1  r0+2 r0+3
//			*/
//			var r3 : int =(j*3+3)*nx+i*3;   
//			var r2 : int =(j*3+2)*nx+i*3;   
//			var r1 : int =(j*3+1)*nx+i*3;   
//			var r0 : int = j*3   *nx+i*3;
//			
//			vers[r0+1] = vers[r0] + v1;
//			vers[r0+2] = vers[r0] + v2;
//			vers[r0+3] = vers[r0] + v3;
//			
//			vers[r1+0] = vers[r0] + v4; 
//			vers[r1+1] = vers[r0] + v5;
//			vers[r1+2] = vers[r0] + v6;
//			vers[r1+3] = vers[r0] + v7;
//				
//			vers[r2+0] = vers[r0] + v8; 
//			vers[r2+1] = vers[r0] + v9;
//			vers[r2+2] = vers[r0] + v10;
//			vers[r2+3] = vers[r0] + v11;
//			
//			vers[r3+0] = vers[r0] + v12; 
//			vers[r3+1] = vers[r0] + v13;
//			vers[r3+2] = vers[r0] + v14;
//			vers[r3+3] = vers[r0] + v15;
//			
//			uvs[r0+0]  = s0;
//			uvs[r0+1]  = s1;
//			uvs[r0+2]  = t0;
//			uvs[r0+3]  = t0;
//			
//			uvs[r1+0]  = s2; 
//			uvs[r1+1]  = s3;
//			uvs[r1+2]  = t0;
//			uvs[r1+3]  = t0;
//				
//			uvs[r2+0]  = t0; 
//			uvs[r2+1]  = t0; 
//			uvs[r2+2]  = t0; 
//			uvs[r2+3]  = t0; 
//	
//			uvs[r3+0]  = Vector2(0,0); 
//			uvs[r3+1]  = Vector2(0,0); 
//			uvs[r3+2]  = Vector2(0,0); 
//			uvs[r3+3]  = Vector2(0,0); 
//		}
//		
//		mesh.uv = uvs;
//		mesh.vertices = vers;
//		
//		return mesh;
//	}
//	
//	function UpdateAllSame(mf : MeshFilter ,  uvrect : Rect)
//	{
//		/*
//			i.e. 1 x 1
//			
//		   12---13-14--15
//	  ypad->|    | |   | 
//			8----9-10--11
//	  0.0-->|    | |   |
//			4----5-6---7
//			|    | |   |
//			|    | |   |
//			|    | |   |
//			0----1-2---3
//		          ^   ^
//		          |   |
//		         0.0  xpad
//		
//		*/
//		var i : int ;//General
//		var j : int ;//General
//		var k : int ;//General
//		
//		/*
//		    s2   s3
//			
//			s0   s1
//		*/
//		var s0 : Vector2 = Vector2(uvrect.x              ,  uvrect.y + uvrect.height);
//		var s1 : Vector2 = Vector2(uvrect.x+uvrect.width ,  uvrect.y + uvrect.height);
//		var s2 : Vector2 = Vector2(uvrect.x              ,  uvrect.y);
//		var s3 : Vector2 = Vector2(uvrect.x+uvrect.width ,  uvrect.y);	
//				
//		// adjust
//		var nx : int = m_n * 3 + 1;
//		
//		
//		var mesh : Mesh     = mf.mesh;
//		var uvs : Vector2[] = mesh.uv;
//		
//		for(j=0;j<m_m;j++) for(i=0;i<m_n;i++)
//		{
//			/*
//			   * Index 
//				r2=(j*3+2)*nx+i*3   
//				r1=(j*3+1)*nx+i*3   
//				r0= j*3   *nx+i*3   
//				
//				r2 - r2+1  r2+2 r2+3
//				 |     |    |
//			    r1   r1+1  r1+2 r1+3
//				 |     |    |
//				 |     |    |
//				r0   r0+1  r0+2 r0+3
//			*/
//			var r1 : int =(j*3+1)*nx+i*3;   
//			var r0 : int = j*3   *nx+i*3;
//
//		/*			
//			uvs[r0+0]  = s0;
//			uvs[r0+1]  = s1;
//
//			uvs[r1+0]  = s2; 
//			uvs[r1+1]  = s3;
//		*/
//			uvs[r0+0]  = s2;
//			uvs[r0+1]  = s3;
//
//			uvs[r1+0]  = s0; 
//			uvs[r1+1]  = s1;
//		}
//		mf.mesh = null;
//		mesh.uv = null;
//		mesh.uv = uvs;
//		mf.mesh = mesh;
//		uvs     = null;
//	}
//	
//	function Update(mf : MeshFilter, uvrects : Rect[])
//	{
//		/*
//			i.e. 1 x 1
//			
//		   12---13-14--15
//	  ypad->|    | |   | 
//			8----9-10--11
//	  0.0-->|    | |   |
//			4----5-6---7
//			|    | |   |
//			|    | |   |
//			|    | |   |
//			0----1-2---3
//		          ^   ^
//		          |   |
//		         0.0  xpad
//		
//		*/
//		var i : int ;//General
//		var j : int ;//General
//		var k : int ;//General
//		
//				
//		// adjust
//		var nx : int = m_n * 3 + 1;
//		
//		var uvs : Vector2[] = mf.mesh.uv;
//		
//		k=0;		
//		for(j=0;j<m_m;j++) for(i=0;i<m_n;i++)
//		{
//			if (k>=uvrects.Length) break;
//			/*
//			   * Index 
//				r2=(j*3+2)*nx+i*3   
//				r1=(j*3+1)*nx+i*3   
//				r0= j*3   *nx+i*3   
//				
//				r2 - r2+1  r2+2 r2+3
//				 |     |    |
//			    r1   r1+1  r1+2 r1+3
//				 |     |    |
//				 |     |    |
//				r0   r0+1  r0+2 r0+3
//			*/
//			/*
//			    s2   s3
//				
//				s0   s1
//			*/
//			var uvrect : Rect = uvrects[k];
//			k++;
//			
//			var s0 : Vector2 = Vector2(uvrect.x              ,  uvrect.y + uvrect.height);
//			var s1 : Vector2 = Vector2(uvrect.x+uvrect.width ,  uvrect.y + uvrect.height);
//			var s2 : Vector2 = Vector2(uvrect.x              ,  uvrect.y);
//			var s3 : Vector2 = Vector2(uvrect.x+uvrect.width ,  uvrect.y);	
//			
//			var r1 : int =(j*3+1)*nx+i*3;   
//			var r0 : int = j*3   *nx+i*3;
//			
//		/*			
//			uvs[r0+0]  = s0;
//			uvs[r0+1]  = s1;
//
//			uvs[r1+0]  = s2; 
//			uvs[r1+1]  = s3;
//		*/
//			uvs[r0+0]  = s2;
//			uvs[r0+1]  = s3;
//
//			uvs[r1+0]  = s0; 
//			uvs[r1+1]  = s1;
//		}
//		
//		mf.mesh.uv = null;
//		mf.mesh.uv = uvs;
//		uvs  = null;
//	}
////////////////////////////////////};