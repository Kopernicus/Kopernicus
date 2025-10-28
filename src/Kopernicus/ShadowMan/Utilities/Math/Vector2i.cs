using UnityEngine;
using System.Collections;

public class Vector2i
{
    public int x, y;

    public Vector2i() { }

    public Vector2i(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector2i(Vector2 v)
    {
        this.x = (int)v.x;
        this.y = (int)v.y;
    }

}
