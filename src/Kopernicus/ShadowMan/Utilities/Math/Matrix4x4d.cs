
using UnityEngine;

#pragma warning disable 660, 661

public class Matrix4x4d
{
    //Members varibles

    public double[,] m = new double[4,4];

    //Constructors

    public Matrix4x4d() { }

    public Matrix4x4d(double m00, double m01, double m02, double m03,
                      double m10, double m11, double m12, double m13,
                      double m20, double m21, double m22, double m23,
                      double m30, double m31, double m32, double m33)
    {
        m[0, 0] = m00; m[0, 1] = m01; m[0, 2] = m02; m[0, 3] = m03;
        m[1, 0] = m10; m[1, 1] = m11; m[1, 2] = m12; m[1, 3] = m13;
        m[2, 0] = m20; m[2, 1] = m21; m[2, 2] = m22; m[2, 3] = m23;
        m[3, 0] = m30; m[3, 1] = m31; m[3, 2] = m32; m[3, 3] = m33;

    }

    public Matrix4x4d(Matrix4x4 mat)
    {
        m[0, 0] = mat.m00; m[0, 1] = mat.m01; m[0, 2] = mat.m02; m[0, 3] = mat.m03;
        m[1, 0] = mat.m10; m[1, 1] = mat.m11; m[1, 2] = mat.m12; m[1, 3] = mat.m13;
        m[2, 0] = mat.m20; m[2, 1] = mat.m21; m[2, 2] = mat.m22; m[2, 3] = mat.m23;
        m[3, 0] = mat.m30; m[3, 1] = mat.m31; m[3, 2] = mat.m32; m[3, 3] = mat.m33;

    }

    public Matrix4x4d(double[,] m)
    {
        System.Array.Copy(m, this.m, 16);
    }

    public Matrix4x4d(Matrix4x4d m)
    {
        System.Array.Copy(m.m, this.m, 16);
    }

    //Operator Overloads

    public static Matrix4x4d operator +(Matrix4x4d m1, Matrix4x4d m2)
    {
        Matrix4x4d kSum = new Matrix4x4d();
        for (int iRow = 0; iRow < 4; iRow++)
        {
            for (int iCol = 0; iCol < 4; iCol++)
            {
                kSum.m[iRow, iCol] = m1.m[iRow, iCol] + m2.m[iRow, iCol];
            }
        }
        return kSum;
    }

    public static Matrix4x4d operator -(Matrix4x4d m1, Matrix4x4d m2)
    {
        Matrix4x4d kSum = new Matrix4x4d();
        for (int iRow = 0; iRow < 4; iRow++)
        {
            for (int iCol = 0; iCol < 4; iCol++)
            {
                kSum.m[iRow, iCol] = m1.m[iRow, iCol] - m2.m[iRow, iCol];
            }
        }
        return kSum;
    }

    public static Matrix4x4d operator *(Matrix4x4d m1, Matrix4x4d m2)
    {
        Matrix4x4d kProd = new Matrix4x4d();
        for (int iRow = 0; iRow < 4; iRow++)
        {
            for (int iCol = 0; iCol < 4; iCol++)
            {

                kProd.m[iRow, iCol] = m1.m[iRow, 0] * m2.m[0, iCol] +
                                     m1.m[iRow, 1] * m2.m[1, iCol] +
                                     m1.m[iRow, 2] * m2.m[2, iCol] +
                                     m1.m[iRow, 3] * m2.m[3, iCol];
            }
        }
        return kProd;
    }

    public static Vector3d2 operator *(Matrix4x4d m, Vector3d2 v)
    {
        Vector3d2 kProd = new Vector3d2();

        double fInvW = 1.0 / (m.m[3,0] * v.x + m.m[3,1] * v.y + m.m[3,2] * v.z + m.m[3,3]);

        kProd.x = (m.m[0, 0] * v.x + m.m[0, 1] * v.y + m.m[0, 2] * v.z + m.m[0, 3]) * fInvW;
        kProd.y = (m.m[1, 0] * v.x + m.m[1, 1] * v.y + m.m[1, 2] * v.z + m.m[1, 3]) * fInvW;
        kProd.z = (m.m[2, 0] * v.x + m.m[2, 1] * v.y + m.m[2, 2] * v.z + m.m[2, 3]) * fInvW;

        return kProd;
    }

    public static Vector4d operator *(Matrix4x4d m, Vector4d v)
    {
        Vector4d kProd = new Vector4d();

        kProd.x = m.m[0, 0] * v.x + m.m[0, 1] * v.y + m.m[0, 2] * v.z + m.m[0, 3] * v.w;
        kProd.y = m.m[1, 0] * v.x + m.m[1, 1] * v.y + m.m[1, 2] * v.z + m.m[1, 3] * v.w;
        kProd.z = m.m[2, 0] * v.x + m.m[2, 1] * v.y + m.m[2, 2] * v.z + m.m[2, 3] * v.w;

        return kProd;
    }

    public static Matrix4x4d operator *(Matrix4x4d m, double s)
    {
        Matrix4x4d kProd = new Matrix4x4d();
        for (int iRow = 0; iRow < 4; iRow++)
        {
            for (int iCol = 0; iCol < 4; iCol++)
            {
                kProd.m[iRow, iCol] = m.m[iRow, iCol] * s;
            }
        }
        return kProd;
    }

    public static bool operator ==(Matrix4x4d m1, Matrix4x4d m2)
    {

        for (int iRow = 0; iRow < 4; iRow++)
        {
            for (int iCol = 0; iCol < 4; iCol++)
            {
                if (m1.m[iRow, iCol] != m2.m[iRow, iCol]) return false;
            }
        }

        return true;
    }

    public static bool operator !=(Matrix4x4d m1, Matrix4x4d m2)
    {
        for (int iRow = 0; iRow < 4; iRow++)
        {
            for (int iCol = 0; iCol < 4; iCol++)
            {
                if (m1.m[iRow, iCol] != m2.m[iRow, iCol]) return true;
            }
        }

        return false;
    }

    //Functions

    public override string ToString()
    {
        return m[0, 0] + "," + m[0, 1] + "," + m[0, 2] + "," + m[0, 3] + "\n" +
                m[1, 0] + "," + m[1, 1] + "," + m[1, 2] + "," + m[1, 3] + "\n" +
                m[2, 0] + "," + m[2, 1] + "," + m[2, 2] + "," + m[2, 3] + "\n" +
                m[3, 0] + "," + m[3, 1] + "," + m[3, 2] + "," + m[3, 3];
    }

    public Matrix4x4d Transpose()
    {
        Matrix4x4d kTranspose = new Matrix4x4d();
        for (int iRow = 0; iRow < 4; iRow++)
        {
            for (int iCol = 0; iCol < 4; iCol++)
            {
                kTranspose.m[iRow, iCol] = m[iCol, iRow];
            }
        }
        return kTranspose;
    }

    private double MINOR(int r0, int r1, int r2, int c0, int c1, int c2)
    {
        return m[r0, c0] * (m[r1, c1] * m[r2, c2] - m[r2, c1] * m[r1, c2]) -
               m[r0, c1] * (m[r1, c0] * m[r2, c2] - m[r2, c0] * m[r1, c2]) +
               m[r0, c2] * (m[r1, c0] * m[r2, c1] - m[r2, c0] * m[r1, c1]);
    }

    private double Determinant()
    {
        return (m[0, 0] * MINOR(1, 2, 3, 1, 2, 3) -
                m[0, 1] * MINOR(1, 2, 3, 0, 2, 3) +
                m[0, 2] * MINOR(1, 2, 3, 0, 1, 3) -
                m[0, 3] * MINOR(1, 2, 3, 0, 1, 2));
    }

    private Matrix4x4d Adjoint()
    {
        return new Matrix4x4d(
                MINOR(1, 2, 3, 1, 2, 3),
                -MINOR(0, 2, 3, 1, 2, 3),
                MINOR(0, 1, 3, 1, 2, 3),
                -MINOR(0, 1, 2, 1, 2, 3),

                -MINOR(1, 2, 3, 0, 2, 3),
                MINOR(0, 2, 3, 0, 2, 3),
                -MINOR(0, 1, 3, 0, 2, 3),
                MINOR(0, 1, 2, 0, 2, 3),

                MINOR(1, 2, 3, 0, 1, 3),
                -MINOR(0, 2, 3, 0, 1, 3),
                MINOR(0, 1, 3, 0, 1, 3),
                -MINOR(0, 1, 2, 0, 1, 3),

                -MINOR(1, 2, 3, 0, 1, 2),
                MINOR(0, 2, 3, 0, 1, 2),
                -MINOR(0, 1, 3, 0, 1, 2),
                MINOR(0, 1, 2, 0, 1, 2));
    }

    public Matrix4x4d Inverse()
    {
        return Adjoint() * (1.0f / Determinant());
    }

    public Vector4d GetColumn(int iCol)
    {
        return new Vector4d(m[0, iCol], m[1, iCol], m[2, iCol], m[3, iCol]);
    }

    public void SetColumn(int iCol, Vector4d v)
    {
        m[0, iCol] = v.x;
        m[1, iCol] = v.y;
        m[2, iCol] = v.z;
        m[3, iCol] = v.w;
    }

    public Vector4d GetRow(int iRow)
    {
        return new Vector4d(m[iRow, 0], m[iRow, 1], m[iRow, 2], m[iRow, 3]);
    }

    public void SetRow(int iRow, Vector4d v)
    {
        m[iRow, 0] = v.x;
        m[iRow, 1] = v.y;
        m[iRow, 2] = v.z;
        m[iRow, 3] = v.w;
    }

    public Matrix4x4 ToMatrix4x4()
    {
        Matrix4x4 mat = new Matrix4x4();

        mat.m00 = (float)m[0, 0]; mat.m01 = (float)m[0, 1]; mat.m02 = (float)m[0, 2]; mat.m03 = (float)m[0, 3];
        mat.m10 = (float)m[1, 0]; mat.m11 = (float)m[1, 1]; mat.m12 = (float)m[1, 2]; mat.m13 = (float)m[1, 3];
        mat.m20 = (float)m[2, 0]; mat.m21 = (float)m[2, 1]; mat.m22 = (float)m[2, 2]; mat.m23 = (float)m[2, 3];
        mat.m30 = (float)m[3, 0]; mat.m31 = (float)m[3, 1]; mat.m32 = (float)m[3, 2]; mat.m33 = (float)m[3, 3];

        return mat;
    }

    public Matrix3x3d ToMatrix3x3d()
    {
        Matrix3x3d mat = new Matrix3x3d();

        mat.m[0, 0] = m[0, 0]; mat.m[0, 1] = m[0, 1]; mat.m[0, 2] = m[0, 2];
        mat.m[1, 0] = m[1, 0]; mat.m[1, 1] = m[1, 1]; mat.m[1, 2] = m[1, 2];
        mat.m[2, 0] = m[2, 0]; mat.m[2, 1] = m[2, 1]; mat.m[2, 2] = m[2, 2];

        return mat;
    }

    //Static Function

    static public Matrix4x4d ToMatrix4x4d(Matrix4x4 matf)
    {
        Matrix4x4d mat = new Matrix4x4d();

        mat.m[0, 0] = matf.m00; mat.m[0, 1] = matf.m01; mat.m[0, 2] = matf.m02; mat.m[0, 3] = matf.m03;
        mat.m[1, 0] = matf.m10; mat.m[1, 1] = matf.m11; mat.m[1, 2] = matf.m12; mat.m[1, 3] = matf.m13;
        mat.m[2, 0] = matf.m20; mat.m[2, 1] = matf.m21; mat.m[2, 2] = matf.m22; mat.m[2, 3] = matf.m23;
        mat.m[3, 0] = matf.m30; mat.m[3, 1] = matf.m31; mat.m[3, 2] = matf.m32; mat.m[3, 3] = matf.m33;

        return mat;
    }

    static public Matrix4x4d Translate(Vector3d2 v)
    {
        return new Matrix4x4d(1, 0, 0, v.x,
                                  0, 1, 0, v.y,
                                  0, 0, 1, v.z,
                                  0, 0, 0, 1);
    }

    static public Matrix4x4d Translate(Vector3 v)
    {
        return new Matrix4x4d(1, 0, 0, v.x,
                                0, 1, 0, v.y,
                                  0, 0, 1, v.z,
                                  0, 0, 0, 1);
    }

    static public Matrix4x4d Scale(Vector3d2 v)
    {
        return new Matrix4x4d(v.x, 0, 0, 0,
                                  0, v.y, 0, 0,
                                  0, 0, v.z, 0,
                                  0, 0, 0, 1);
    }

    static public Matrix4x4d Scale(Vector3 v)
    {
        return new Matrix4x4d(v.x, 0, 0, 0,
                                  0, v.y, 0, 0,
                                  0, 0, v.z, 0,
                                  0, 0, 0, 1);
    }

    static public Matrix4x4d RotateX(double angle)
    {
        double ca = System.Math.Cos(angle * System.Math.PI / 180.0);
        double sa = System.Math.Sin(angle * System.Math.PI / 180.0);

        return new Matrix4x4d(1, 0, 0, 0,
                                  0, ca, -sa, 0,
                                  0, sa, ca, 0,
                                  0, 0, 0, 1);
    }

    static public Matrix4x4d RotateY(double angle)
    {
        double ca = System.Math.Cos(angle * System.Math.PI / 180.0);
        double sa = System.Math.Sin(angle * System.Math.PI / 180.0);

        return new Matrix4x4d(ca, 0, sa, 0,
                                  0, 1, 0, 0,
                                  -sa, 0, ca, 0,
                                  0, 0, 0, 1);
    }

    static public Matrix4x4d RotateZ(double angle)
    {
        double ca = System.Math.Cos(angle * System.Math.PI / 180.0);
        double sa = System.Math.Sin(angle * System.Math.PI / 180.0);

        return new Matrix4x4d(ca, -sa, 0, 0,
                                  sa, ca, 0, 0,
                                  0, 0, 1, 0,
                                  0, 0, 0, 1);
    }

    static public Matrix4x4d Rotate(Vector3 rotation)
    {
        Quat x = new Quat(new Vector3d2(1,0,0), rotation.x * MathUtility.Deg2Rad );
        Quat y = new Quat(new Vector3d2(0,1,0), rotation.y * MathUtility.Deg2Rad );
        Quat z = new Quat(new Vector3d2(0,0,1), rotation.z * MathUtility.Deg2Rad );

        return (z * y * x).ToMatrix4x4d();
    }

    static public Matrix4x4d Rotate(Vector3d2 rotation)
    {
        Quat x = new Quat(new Vector3d2(1,0,0), rotation.x * MathUtility.Deg2Rad );
        Quat y = new Quat(new Vector3d2(0,1,0), rotation.y * MathUtility.Deg2Rad );
        Quat z = new Quat(new Vector3d2(0,0,1), rotation.z * MathUtility.Deg2Rad );

        return (z * y * x).ToMatrix4x4d();
    }

    static public Matrix4x4d Perspective(double fovy, double aspect, double zNear, double zFar)
    {
        double f = 1.0 / System.Math.Tan((fovy * System.Math.PI / 180.0) / 2.0);
        return new Matrix4x4d(f / aspect, 0, 0, 0,
                                  0, f, 0, 0,
                                  0, 0, (zFar + zNear) / (zNear - zFar), (2.0 * zFar * zNear) / (zNear - zFar),
                                  0, 0, -1, 0);
    }

    static public Matrix4x4d Ortho(double xRight, double xLeft, double yTop, double yBottom, double zNear, double zFar)
    {
        double tx, ty, tz;
        tx = -(xRight + xLeft) / (xRight - xLeft);
        ty = -(yTop + yBottom) / (yTop - yBottom);
        tz = -(zFar + zNear) / (zFar - zNear);
        return new Matrix4x4d(2.0 / (xRight - xLeft), 0, 0, tx,
                                   0, 2.0 / (yTop - yBottom), 0, ty,
                                   0, 0, -2.0 / (zFar - zNear), tz,
                                   0, 0, 0, 1);
    }

    static public Matrix4x4d Identity()
    {
        return new Matrix4x4d(1, 0, 0, 0,
                                0, 1, 0, 0,
                                0, 0, 1, 0,
                                0, 0, 0, 1);
    }

}

























