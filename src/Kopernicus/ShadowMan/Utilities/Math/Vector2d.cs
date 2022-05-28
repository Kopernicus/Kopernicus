using UnityEngine;

public class Vector2d
{
    //Member varibles

    public double x, y;

    //Constructors

    public Vector2d()
    {
        this.x = 0.0f;
        this.y = 0.0f;
    }

    public Vector2d(double v)
    {
        this.x = v;
        this.y = v;
    }

    public Vector2d(double x, double y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector2d(Vector2d v)
    {
        x = v.x;
        y = v.y;
    }

    public Vector2d(double[] v)
    {
        x = v[0];
        y = v[1];
    }

    //Operator overloads

    public static Vector2d operator +(Vector2d v1, Vector2d v2)
    {
        return new Vector2d(v1.x + v2.x, v1.y + v2.y);
    }

    public static Vector2d operator -(Vector2d v1, Vector2d v2)
    {
        return new Vector2d(v1.x - v2.x, v1.y - v2.y);
    }

    public static Vector2d operator *(Vector2d v1, Vector2d v2)
    {
        return new Vector2d(v1.x * v2.x, v1.y * v2.y);
    }

    public static Vector2d operator *(Vector2d v, double s)
    {
        return new Vector2d(v.x * s, v.y * s);
    }

    public static Vector2d operator *(double s, Vector2d v)
    {
        return new Vector2d(v.x * s, v.y * s);
    }

    public static Vector2d operator /(Vector2d v, double s)
    {
        return new Vector2d(v.x / s, v.y / s);
    }

    public static Vector2d operator /(Vector2d v1, Vector2d v2)
    {
        return new Vector2d(v1.x / v2.x, v1.y / v2.y);
    }

    //Functions

    public override string ToString()
    {
        return "(" + x + "," + y + ")";
    }

    public double Magnitude()
    {
        return System.Math.Sqrt(x * x + y * y);
    }

    public double SqrMagnitude()
    {
        return (x * x + y * y);
    }

    public double Dot(Vector2d v)
    {
        return (x * v.x + y * v.y);
    }

    public void Normalize()
    {
        double invLength = 1.0 / System.Math.Sqrt(x*x + y*y);
        x *= invLength;
        y *= invLength;
    }

    public Vector2d Normalized()
    {
        double invLength = 1.0 / System.Math.Sqrt(x*x + y*y);
        return new Vector2d(x * invLength, y * invLength);
    }

    public Vector2d Normalized(double l)
    {
        double length = System.Math.Sqrt(x * x + y * y);
        double invLength = l / length;
        return new Vector2d(x * invLength, y * invLength);
    }

    public double Cross(Vector2d v)
    {
        return x * v.y - y * v.x;
    }

    static public double Cross(Vector2d u, Vector2d v)
    {
        return u.x * v.y - u.y * v.x;
    }

    public Vector2 ToVector2()
    {
        return new Vector2((float)x, (float)y);
    }

}


































