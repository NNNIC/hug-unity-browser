using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using hug_u;

public class hglCalcTable2  {

	/*
	   - Base Table -

		[0,0][0,1][0,2][0,3] 
		[1,0][1,1][1,2][1,3] 
		[2,0][2,1][2,2][2,3] 
		[3,0][3,1][3,2][3,3]
		 TD   TD   TD   TD

	*/
#if OBSOLATED
		class BaseTable
	{
		Hashtable m_baseTable = null;
		int len_x; public int Get_len_x() {return len_x;}
		int len_y; public int Get_len_y() {return len_y;}

		public BaseTable()
		{
			m_baseTable = new Hashtable();
			len_x = len_y =0;
		}
		void MakePoint(int x, int y, object val=null)
		{
			string key = string.Format("[{0},{1}]",y,x);
			m_baseTable[key] = val;
		}
		public object GetPoint(int x, int y)
		{
			string key = string.Format("[{0},{1}]",y,x);
			if (!m_baseTable.ContainsKey(key)) 
				return null;
			return m_baseTable[key]; 
		}
		public void SetPoint(int x, int y, object val)
		{
			string key = string.Format("[{0},{1}]",y,x);
			if (!m_baseTable.ContainsKey(key))
				throw new SystemException("unexpected!");
			m_baseTable[key] = val;
		}
		public bool checkExist(int x,int y)
		{
			string key = string.Format("[{0},{1}]",y,x);
			return m_baseTable.ContainsKey(key);
		}
		public int GetMostLeft(int y)
		{
			if (len_x == 0) return 0;
			for (int i = 0; i < len_x; i++)
			{
				if (checkExist(i,y)==false) return i;
			}
			throw new SystemException("Unexpected!");
		}
		public void AddElement(int x, int y, object val)
		{
			if (x >= len_x)
			{
				for(int ix = len_x; ix<=x;ix++)
				for(int iy = 0; iy<len_y; iy++)
				{
					MakePoint(ix,iy);
				}
				len_x = x + 1;
			}
			if (y > len_y)
			{
				for(int iy = len_y; iy <= y; iy++)
				for(int ix = 0;ix<len_x;ix++)
				{
					MakePoint(ix,iy);
				}
				len_y = y+1;   
			}
			SetPoint(x,y,val);
		}

		public override string ToString()
		{
			List<string> keylist = new List<string>();
			foreach(var k in m_baseTable.Keys) keylist.Add((string)k);
			keylist.Sort();
			string dt = "";
			foreach (var k in keylist)
			{
				dt += k + "=";
				var val = (Element)m_baseTable[k];
				dt += val.ToString() + "\n";
			}
			return dt;
		}
	}

#endif
	class BaseTable
	{
		public int size_x;
		public int size_y;
		Element[,] m_baseTable;
		int max_x; public int Get_max_x() {return max_x;}
		int max_y; public int Get_max_y() {return max_y;}

		public BaseTable()
		{
			size_x = size_y = 10;
			m_baseTable = new Element[size_x,size_y];
			max_x = max_y =-1;

		}
		public void IncreaseTable(int newsizex,int newsizey )
		{
			if ((newsizex > size_x) || (newsizey > size_y))
			{ 
				newsizex = Mathf.Max(size_x, newsizex);
				newsizey = Mathf.Max(size_y, newsizey);

				Element[,] newtable = new Element[newsizex,newsizey];
				for(int x = 0; x < size_x; x++) for(int y = 0;y<size_y;y++) newtable[x,y] = m_baseTable[x,y];
				size_x = newsizex;
				size_y = newsizey;
				m_baseTable = newtable;
			}
		}

		public Element GetPoint(int x, int y)
		{
			return m_baseTable[x,y]; 
		}
		public void SetPoint(int x, int y, Element val)
		{
			if (x>=size_x || y>=size_y) throw new SystemException("ERROR: TABLE SETPOINT("+x+","+y+")");
			m_baseTable[x,y] = val;
		}
		public bool checkExist(int x,int y)
		{
			if (x>=size_x || y>=size_y) 
				throw new SystemException("ERROR: checkExist("+x+","+y+")");
			return (m_baseTable[x,y]!=null);
		}
		public int GetMostLeft(int y)
		{
			if (y>=size_y) 
				return 0;
			for (int i = 0; i < size_x; i++)
			{
				if (checkExist(i,y)==false) return i;
			}
			return size_x;
			//throw new SystemException("Unexpected!");
		}
		public void AddElement(int x, int y, Element val)
		{
			if (x>=size_x) IncreaseTable(x+10,y);
			if (y>=size_y) IncreaseTable(x,y+10);

			max_x = Mathf.Max(max_x,x);
			max_y = Mathf.Max(max_y,y);
			SetPoint(x,y,val);
		}

		public void Normalize() // Check element, if it isn't cohrent, adjust it.
		{
			for (int y = 0; y <= max_y; y++) for (int x = 0; x <= max_x; x++)
				{
					var elm = m_baseTable[x,y];
					if (elm==null)
					{
						var ne  = new Element();
						ne.mode = Element.Mode.ORIGINAL;
						ne.td   = null;
						ne.x    = x;
						ne.y    = y;
						ne.spanLastCol = x;
						ne.spanLastRow = y;
						m_baseTable[x,y] = ne;
					}
				}
		}


		public override string ToString()
		{
			string dt = "";
			for (int y = 0; y < size_y; y++) for (int x = 0; x < size_x; x++)
			{
				var val = m_baseTable[x,y];
				if (val==null) continue;
				dt += string.Format("[{0},{1}]={2}\n",x,y,val.ToString() );
			}
			return dt;
		}
	}

	class Element
	{
		public enum Mode { ORIGINAL, SPAN  }
		public Mode    mode;
		public Element original;
		public int x,y;
		public int spanLastCol;
		public int spanLastRow;
		public hglRender.TABLE.TD td;
		public hgRect doneRealRect;

		public override string ToString()
		{
			if (mode == Mode.ORIGINAL)
			{
				return td!=null ? td.ToString() : "(null)";
			}
			else
			{
				return "{SPAN}";
			}
		}
	}
	public class LENVAL
	{ 
		public float len;
		public static float GetWidth(LENVAL[] lv,  int start, int end)
		{
			float sum = 0;
			for(int i = start; i <= end; i++)
			{
				if (lv[i].len<0) return -1;
				sum += lv[i].len;
			}
			return sum;
		}

		public override string ToString()
		{
			return len.ToString();
		}
	}
	
	/*
		ref : http://tohoho-web.com/html/table.htm


				 |<------table width------->|
		  +---------------------------------------+
		  |                                       |
	  --- |      +==========================+     |
	   ^  |    ->|<- border width           |     |
	   |  |      |                          |     |
	   |  .      .                          .     .
table  |  .      .                          .     .
height |  .      .                          .     .
	   |  |      |                          |     |
	   v  |      |       v                  |<--->| hspace
	  --- |      +==========================+     |
		  |                                       |
		  +---------------------------------------+
						 ^
						 |
					   vspace

		  +--------+--------+
		  |        |        |
		  |  cell  |  cell__|__
		  |        |        | 
		  +--------+--------+--
							^
							cellpadding

	*/

	BaseTable m_baseTable;
	hglRender.TABLE m_table;

	public hglCalcTable2( hglRender.TABLE table)
	{
		m_baseTable = new BaseTable();
		m_table = table;
	}

	LENVAL[] m_xLenList;
	LENVAL[] m_yLenList;

	public void Format(/*hglHtmlRender.BASE tableBlock,*/ ALIGN align, hgRect curRect, out hgRect doneRealRect, out hgRect doneMarginRect )
	{
		foreach (var tr in m_table.trlist)
		{
			foreach (var td in tr.tdlist)
			{
				td.CreatePadWpaddingRect(m_table.cellpadding + m_table.xe.thisStyle.GetFloat(StyleKey.border_width,float.NaN));
				SetTD(td);
			}
		}

		int validWidth  = m_baseTable.Get_max_x()+1;
		int validHeight = m_baseTable.Get_max_y()+1;

		m_baseTable.Normalize();

		m_xLenList = new LENVAL[validWidth];   for(int i=0;i<m_xLenList.Length;i++) m_xLenList[i] = new LENVAL() {  len = -1 };  
		m_yLenList = new LENVAL[validHeight];  for(int i=0;i<m_yLenList.Length;i++) m_yLenList[i] = new LENVAL() {  len = -1 };

		bool bNeedUpdate = true;
		int loopIndex = 0;
		while(bNeedUpdate)
		{
			if (loopIndex++ > 100) { Debug.LogError("Too much Loop");  break;}
			bNeedUpdate = false;

			for(var y = 0;y<validHeight;y++)
			{
				for(var x = 0;x<validWidth;x++)
				{
					float w,h;
					if (GetWH(x,y,out w, out h))
					{
						if ( w>0 && w>m_xLenList[x].len)
						{
							bNeedUpdate = true;
							m_xLenList[x].len = w;
						}
						if ( h>0 && h>m_yLenList[y].len)
						{
							bNeedUpdate = true;
							m_yLenList[y].len = h;
						} 
					}
				}
			}
		}
		//Relocate

		var tmpDoneRealRect = new hgRect();
		for(int y = 0; y < validHeight; y++)
		{
			for(int x = 0; x < validWidth; x++)
			{
				var dt = m_baseTable.GetPoint(x,y);
				if (dt.mode == Element.Mode.ORIGINAL)
				{
					if (dt.td==null) continue;
					var w = LENVAL.GetWidth(m_xLenList,x,dt.spanLastCol);
					var h = LENVAL.GetWidth(m_yLenList,y,dt.spanLastRow);

					var lx = x>0 ? LENVAL.GetWidth(m_xLenList,0,x-1) : 0;
					var ly = y>0 ? LENVAL.GetWidth(m_yLenList,0,y-1) : 0;

					tmpDoneRealRect.Sample(new hgRect(lx,-ly,w,h));
 
					Vector2 v = Vector2.zero;

					{
						Vector2 center = new Vector2(lx + w / 2, -(ly + h / 2)); //Debug.LogWarning(dt.td.block.ToString() + ">" + center + " Aligh=" + align);
						v = center - dt.td.padding_w_padRect.center;
					}
					dt.td.block.Relocate(v);

					//Enlarge td.block.doneRealRect to fit.
					dt.td.block.doneRealRect.ChangWidth(w);
					dt.td.block.doneRealRect.ChangHeight(h);
					dt.td.block.doneMarginRect = new hgRect(dt.td.block.doneRealRect);
				}
			}
		}

		Debug.Log("tmpDoneRealRect = " + tmpDoneRealRect );

		float reltopY = curRect._topY;
		float reltopX = curRect._leftX;
		if (m_table.align == ALIGN.CENTER)
		{
			reltopX = curRect._leftX + curRect.width / 2 - tmpDoneRealRect.width / 2;
		}
		else if (m_table.align == ALIGN.RIGHT)
		{
			reltopX = curRect._rightX - tmpDoneRealRect.width;
		}

		Vector3 v2 = new Vector3(reltopX,reltopY,0) - hglEtc.toVector3(tmpDoneRealRect.topLeft);
		if (v2!=Vector3.zero)  foreach (var tr in m_table.trlist)
		{
			foreach (var td in tr.tdlist)
			{
				td.block.Relocate(v2);
			}
		}
		tmpDoneRealRect.Move(v2);

		//Fix align
		foreach (var tr in m_table.trlist)
		{
			foreach (var td in tr.tdlist)
			{
				td.block.ReAlign(); 
			}
		}

		// For centering
		if (align == ALIGN.CENTER)
		{
			var v3 = new Vector3( m_table.padRect.center.x - tmpDoneRealRect.center.x,0,0);
			tmpDoneRealRect.Move(v3);
			foreach (var tr in m_table.trlist)
			{
				foreach (var td in tr.tdlist)
				{
					td.block.Relocate(v3);
				}
			}
		} 

		doneRealRect   = tmpDoneRealRect;
		doneMarginRect = new hgRect(doneRealRect);
		doneMarginRect.max_v  += new Vector2( m_table.hspace, m_table.vspace ) ;
		doneMarginRect.min_v  -= new Vector2( m_table.hspace, m_table.vspace ) ;

	}
	bool GetWH(int x, int y, out float w, out float h)
	{
		w = -1; // means Unknown
		h = -1; // means Unknown

		var elm = m_baseTable.GetPoint(x,y);
		if (elm == null)
		{ 
			w = 0; h = 0;
			return true;
		}
		//    throw new SystemException("ERROR: TABLE("+x+","+y+")" );
		if (elm.mode == Element.Mode.ORIGINAL) 
		{
			if (elm.spanLastCol == x)
			{
				w = (elm.td!=null) ? elm.td.padding_w_padRect.width : 0;
			}
			if (elm.spanLastRow == y)
			{
				h = (elm.td!=null) ? elm.td.padding_w_padRect.height : 0;
			}
			return true;
		}
		else if (elm.mode == Element.Mode.SPAN)
		{
			var basedt =  elm.original;
			
			if (basedt.spanLastCol == x)
			{
				var lw = LENVAL.GetWidth(m_xLenList,basedt.x, basedt.spanLastCol -1 );
				if (lw >= 0)
				{
					w = basedt.td.padding_w_padRect.width - lw;
				}
			}
			if (basedt.spanLastRow == y)
			{
				var lw = LENVAL.GetWidth(m_yLenList,basedt.y, basedt.spanLastRow -1 );
				if (lw >= 0)
				{
					h = basedt.td.padding_w_padRect.height - lw;
				}
			}
			return true;
		}
		return false;
	}
	private void SetTD(hglRender.TABLE.TD td)
	{
		var ty = td.rowNum;
		var x  = m_baseTable.GetMostLeft(ty);
		var colspan = td.colspan > 1 ? td.colspan : 1;
		var rowspan = td.rowspan > 1 ? td.rowspan : 1;
		Element org = null;
		for (int ix = 0; ix < colspan; ix++) for (int iy = 0; iy < rowspan; iy++)
		{ 
			var elm = new Element();
			if (ix == 0 && iy == 0)
			{
				elm.mode = Element.Mode.ORIGINAL;
				elm.td = td;
				elm.x  = x;
				elm.y  = ty;
				elm.spanLastCol = x  + colspan - 1;
				elm.spanLastRow = ty + rowspan - 1;
				org = elm;
			}
			else
			{
				elm.mode = Element.Mode.SPAN;
				elm.original = org;
				elm.x = elm.y = -1;
			}
 
			m_baseTable.AddElement(x + ix ,ty + iy,elm);
		}
		
	}

	public override string ToString()
	{
		return m_baseTable.ToString();
	}


#if OBSOLATED
	public string align;
	public float cellpadding;
	public float borderwidth;
	public float tableheight;
	public float tablewidth;
	public float hspace;
	public float vspace;

	public void SetTR(hglParser.Element xe)
	{
		m_cur_y = m_cur_y < 0 ? 0 : m_cur_y+1;
		m_cur_x = 0;
		Element e = new Element() { mode = Element.Mode.TR,xe = xe, calcSize = Vector2.zero };
		m_baseTable.AddElement(m_cur_x,m_cur_y,e);
		m_cur_x++;
	}
	public void SetTD(hglParser.Element xe, int colspan, int rowspan )
	{
		if (m_cur_y<0 || m_cur_x <= 0) throw new SystemException("Unexpected!");
		Element e = new Element() { mode = Element.Mode.ORIGINAL, xe =xe};
		while(m_baseTable.GetPoint(m_cur_x,m_cur_y)!=null) m_cur_x++;
		m_baseTable.AddElement(m_cur_x,m_cur_y,e);
		if (colspan > 1) for (int ix = 1; ix < colspan; ix++)
		{
			if (rowspan > 1) for (int iy = 1; iy < rowspan; iy++)
			{
				Element se = new Element() { mode = Element.Mode.SPAN, original = e};
				m_baseTable.AddElement(m_cur_x + ix,m_cur_y+iy,se);
			}
		}
		if (colspan >1) m_cur_x += colspan-1;
	}

	public void Format()
	{
		/*
		
	   w[0] [1] [2]  
		+--+--+---+ 
		|  |  |   | h[0]
		+--+--+---+
		|  |  |   | h[1]
		+--+--+---+
		|  |  |   | h[2]
		+--+--+---+

		Note: Left is TR info.So, w[0]=0!
		*/

		float[] w = new float[m_baseTable.Get_max_x()]; for(var i=0;i<w.Length;i++) w[i]=0f;
		float[] h = new float[m_baseTable.Get_max_y()]; for(var i=0;i<w.Length;i++) h[i]=0f;






	}

#endif

	
}
