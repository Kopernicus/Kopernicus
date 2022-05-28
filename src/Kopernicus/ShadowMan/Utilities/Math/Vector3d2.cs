
using UnityEngine;

public class Vector3d2
{
    //Member varibles

    public double x, y, z;

    //Constructors

    public Vector3d2()
    {
        this.x = 0.0f;
        this.y = 0.0f;
        this.z = 0.0f;
    }

    public Vector3d2(double v)
    {
        this.x = v;
        this.y = v;
        this.z = v;
    }

    public Vector3d2(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3d2(Vector2d v, double z)
    {
        x = v.x;
        y = v.y;
        this.z = z;
    }

    public Vector3d2(Vector3d2 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public Vector3d2(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public Vector3d2(Vector2 v, double z)
    {
        x = v.x;
        y = v.y;
        this.z = z;
    }

    public Vector3d2(double[] v)
    {
        x = v[0];
        y = v[1];
        z = v[2];
    }

    //Operator overloads

    public static Vector3d2 operator +(Vector3d2 v1, Vector3d2 v2)
    {
        return new Vector3d2(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
    }

    public static Vector3d2 operator -(Vector3d2 v1, Vector3d2 v2)
    {
        return new Vector3d2(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
    }

    public static Vector3d2 operator *(Vector3d2 v1, Vector3d2 v2)
    {
        return new Vector3d2(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    }

    public static Vector3d2 operator *(Vector3d2 v, double s)
    {
        return new Vector3d2(v.x * s, v.y * s, v.z * s);
    }

    public static Vector3d2 operator *(double s, Vector3d2 v)
    {
        return new Vector3d2(v.x * s, v.y * s, v.z * s);
    }

    public static Vector3d2 operator /(Vector3d2 v, double s)
    {
        return new Vector3d2(v.x / s, v.y / s, v.z / s);
    }

    public static Vector3d2 operator /(Vector3d2 v1, Vector3d2 v2)
    {
        return new Vector3d2(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
    }

    //Functions

    public override string ToString()
    {
        return "(" + x + "," + y + "," + z + ")";
    }

    public double Magnitude()
    {
        return System.Math.Sqrt(x * x + y * y + z * z);
    }

    public double SqrMagnitude()
    {
        return (x * x + y * y + z * z);
    }

    public double Dot(Vector3d2 v)
    {
        return (x * v.x + y * v.y + z * v.z);
    }

    public void Normalize()
    {
        double invLength = 1.0 / System.Math.Sqrt(x*x + y*y + z*z);
        x *= invLength;
        y *= invLength;
        z *= invLength;
    }

    public Vector3d2 Normalized()
    {
        double invLength = 1.0 / System.Math.Sqrt(x*x + y*y + z*z);
        return new Vector3d2(x * invLength, y * invLength, z * invLength);
    }

    public Vector3d2 Normalized(double l)
    {
        double length = System.Math.Sqrt(x*x + y*y + z*z);
        double invLength = l / length;
        return new Vector3d2(x * invLength, y * invLength, z * invLength);
    }

    public Vector3d2 Normalized(ref double previousLength)
    {
        previousLength = System.Math.Sqrt(x * x + y * y + z * z);
        double invLength = 1.0 / previousLength;
        return new Vector3d2(x * invLength, y * invLength, z * invLength);
    }

    public Vector3d2 Cross(Vector3d2 v)
    {
        return new Vector3d2(y * v.z - z * v.y, z * v.x - x * v.z, x * v.y - y * v.x);
    }

    public Vector2d XY()
    {
        return new Vector2d(x, y);
    }

    public Vector3 ToVector3()
    {
        return new Vector3((float)x, (float)y, (float)z);
    }

    public static Vector3d2 UnitX()
    {
        return new Vector3d2(1, 0, 0);
    }
    public static Vector3d2 UnitY()
    {
        return new Vector3d2(0, 1, 0);
    }

    public static Vector3d2 UnitZ()
    {
        return new Vector3d2(0, 0, 1);
    }

    public static Vector3d2 Zero()
    {
        return new Vector3d2(0, 0, 0);
    }



}