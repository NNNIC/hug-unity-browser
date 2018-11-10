using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class hglHtmlColor {

    const int SIZE = 32;

    List<Color> m_list;
    
    Texture2D   m_tex; public Texture2D GetTexture() {return m_tex;}

    bool m_needUpdate;

    public hglHtmlColor() { 
        m_list = new List<Color>(); 
        m_tex  = new Texture2D(SIZE,SIZE,TextureFormat.ARGB32,false);
        for (var x = 0; x < SIZE; x++) for (var y = 0; y < SIZE; y++)
        {
            m_tex.SetPixel(x,y,Color.white);
        }
        m_tex.filterMode = FilterMode.Point;
        m_tex.Apply();
    }

    public int GetNewIndex(Color col)
    {
        m_list.Add(col);
        m_needUpdate = true;
        return m_list.Count-1;
    }

    public int CountIndex()
    {
        return m_list.Count;
    }

    public void SetColor(int index, Color col)
    {
        if (index<0 || index>=m_list.Count) return;
        m_list[index] = col;
        var x = index % SIZE;
        var y = index / SIZE;
        m_tex.SetPixel(x,y,col);
        m_needUpdate = true;
    }

    public Color GetColor(int index)
    {
        if (index <0 || index >= m_list.Count ) return Color.black;
        return m_list[index];
    }

    public void ApplyAll()
    {
        for (var x = 0; x < SIZE; x++) for (var y = 0; y < SIZE; y++)
        {
            var index = y * SIZE + x;
            Color col = index < m_list.Count ? m_list[index] : Color.white;
            m_tex.SetPixel(x,y,col);
        }
        m_tex.Apply();
        m_needUpdate = false;
    }
    public void Apply()
    {
        if (!m_needUpdate) return;
        m_tex.Apply();
        m_needUpdate = false;
    }


    public static Color IndexToVColor(int index)
    {
        if (index < 0 && index > SIZE*SIZE ) return Color.white;
        float x = (float)(index % SIZE) / (float)SIZE + 1f / (float)(SIZE*2);
        float y = (float)(index / SIZE) / (float)SIZE + 1f / (float)(SIZE*2);

        return new Color(x,y,0);
    }

    
}
