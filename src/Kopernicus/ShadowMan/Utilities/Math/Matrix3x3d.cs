
using UnityEngine;

public class Matrix3x3d
{
    //Members varibles

    public double[,] m = new double[3,3];

    //Constructors

    public Matrix3x3d() { }

    public Matrix3x3d(double m00, double m01, double m02,
                      double m10, double m11, double m12,
                      double m20, double m21, double m22)
    {
        m[0, 0] = m00; m[0, 1] = m01; m[0, 2] = m02;
        m[1, 0] = m10; m[1, 1] = m11; m[1, 2] = m12;
        m[2, 0] = m20; m[2, 1] = m21; m[2, 2] = m22;

    }

    public Matrix3x3d(double[,] m)
    {
        System.Array.Copy(m, this.m, 9);
    }

    public Matrix3x3d(Matrix3x3d m)
    {
        System.Array.Copy(m.m, this.m, 9);
    }

    //Operator Overloads

    public static Matrix3x3d operator +(Matrix3x3d m1, Matrix3x3d m2)
    {
        Matrix3x3d kSum = new Matrix3x3d();
        for (int iRow = 0; iRow < 3; iRow++)
        {
            for (int iCol = 0; iCol < 3; iCol++)
            {
                kSum.m[iRow, iCol] = m1.m[iRow, iCol] + m2.m[iRow, iCol];
            }
        }
        return kSum;
    }

    public static Matrix3x3d operator -(Matrix3x3d m1, Matrix3x3d m2)
    {
        Matrix3x3d kSum = new Matrix3x3d();
        for (int iRow = 0; iRow < 3; iRow++)
        {
            for (int iCol = 0; iCol < 3; iCol++)
            {
                kSum.m[iRow, iCol] = m1.m[iRow, iCol] - m2.m[iRow, iCol];
            }
        }
        return kSum;
    }

    public static Matrix3x3d operator *(Matrix3x3d m1, Matrix3x3d m2)
    {
        Matrix3x3d kProd = new Matrix3x3d();
        for (int iRow = 0; iRow < 3; iRow++)
        {
            for (int iCol = 0; iCol < 3; iCol++)
            {
                kProd.m[iRow, iCol] = m1.m[iRow, 0] * m2.m[0, iCol] +
                                     m1.m[iRow, 1] * m2.m[1, iCol] +
                                     m1.m[iRow, 2] * m2.m[2, iCol];
            }
        }
        return kProd;
    }

    public static Vector3d2 operator *(Matrix3x3d m, Vector3d2 v)
    {
        Vector3d2 kProd = new Vector3d2();

        kProd.x = m.m[0, 0] * v.x + m.m[0, 1] * v.y + m.m[0, 2] * v.z;
        kProd.y = m.m[1, 0] * v.x + m.m[1, 1] * v.y + m.m[1, 2] * v.z;
        kProd.z = m.m[2, 0] * v.x + m.m[2, 1] * v.y + m.m[2, 2] * v.z;

        return kProd;
    }

    public static Matrix3x3d operator *(Matrix3x3d m, double s)
    {
        Matrix3x3d kProd = new Matrix3x3d();
        for (int iRow = 0; iRow < 3; iRow++)
        {
            for (int iCol = 0; iCol < 3; iCol++)
            {
                kProd.m[iRow, iCol] = m.m[iRow, iCol] * s;
            }
        }
        return kProd;
    }

    //Functions

    public override string ToString()
    {
        return m[0, 0] + "," + m[0, 1] + "," + m[0, 2] + "\n" +
                m[1, 0] + "," + m[1, 1] + "," + m[1, 2] + "\n" +
                m[2, 0] + "," + m[2, 1] + "," + m[2, 2];
    }

    public Matrix3x3d Transpose()
    {
        Matrix3x3d kTranspose = new Matrix3x3d();
        for (int iRow = 0; iRow < 3; iRow++)
        {
            for (int iCol = 0; iCol < 3; iCol++)
            {
                kTranspose.m[iRow, iCol] = m[iCol, iRow];
            }
        }
        return kTranspose;
    }

    private double Determinant()
    {
        double fCofactor00 = m[1,1] * m[2,2] - m[1,2] * m[2,1];
        double fCofactor10 = m[1,2] * m[2,0] - m[1,0] * m[2,2];
        double fCofactor20 = m[1,0] * m[2,1] - m[1,1] * m[2,0];

        double fDet = m[0,0] * fCofactor00 + m[0,1] * fCofactor10 + m[0,2] * fCofactor20;

        return fDet;
    }

    //public bool Inverse(ref Matrix3x3d mInv, double tolerance = 1e-06)
    public bool Inverse(ref Matrix3x3d mInv, double tolerance)
    {
        // Invert a 3x3 using cofactors.  This is about 8 times faster than
        // the Numerical Recipes code which uses Gaussian elimination.
        mInv.m[0, 0] = m[1, 1] * m[2, 2] - m[1, 2] * m[2, 1];
        mInv.m[0, 1] = m[0, 2] * m[2, 1] - m[0, 1] * m[2, 2];
        mInv.m[0, 2] = m[0, 1] * m[1, 2] - m[0, 2] * m[1, 1];
        mInv.m[1, 0] = m[1, 2] * m[2, 0] - m[1, 0] * m[2, 2];
        mInv.m[1, 1] = m[0, 0] * m[2, 2] - m[0, 2] * m[2, 0];
        mInv.m[1, 2] = m[0, 2] * m[1, 0] - m[0, 0] * m[1, 2];
        mInv.m[2, 0] = m[1, 0] * m[2, 1] - m[1, 1] * m[2, 0];
        mInv.m[2, 1] = m[0, 1] * m[2, 0] - m[0, 0] * m[2, 1];
        mInv.m[2, 2] = m[0, 0] * m[1, 1] - m[0, 1] * m[1, 0];

        double fDet = m[0,0] * mInv.m[0,0] + m[0,1] * mInv.m[1,0] + m[0,2] * mInv.m[2,0];

        if (System.Math.Abs(fDet) <= tolerance)
        {
            return false;
        }

        double fInvDet = 1.0 / fDet;

        for (int iRow = 0; iRow < 3; iRow++)
        {
            for (int iCol = 0; iCol < 3; iCol++)
                mInv.m[iRow, iCol] *= fInvDet;
        }

        return true;
    }

    //public Matrix3x3d Inverse(double tolerance = 1e-06)
    public Matrix3x3d Inverse(double tolerance)
    {
        Matrix3x3d kInverse = new Matrix3x3d();
        Inverse(ref kInverse, tolerance);
        return kInverse;
    }

    public Vector3d2 GetColumn(int iCol)
    {
        return new Vector3d2(m[0, iCol], m[1, iCol], m[2, iCol]);
    }

    public void SetColumn(int iCol, Vector3d2 v)
    {
        m[0, iCol] = v.x;
        m[1, iCol] = v.y;
        m[2, iCol] = v.z;
    }

    public Vector3d2 GetRow(int iRow)
    {
        return new Vector3d2(m[iRow, 0], m[iRow, 1], m[iRow, 2]);
    }

    public void SetRow(int iRow, Vector3d2 v)
    {
        m[iRow, 0] = v.x;
        m[iRow, 1] = v.y;
        m[iRow, 2] = v.z;
    }

    public Matrix4x4d ToMatrix4x4d()
    {
        return new Matrix4x4d(m[0, 0], m[0, 1], m[0, 2], 0.0,
                              m[1, 0], m[1, 1], m[1, 2], 0.0,
                              m[2, 0], m[2, 1], m[2, 2], 0.0,
                              0.0, 0.0, 0.0, 0.0);
    }

    public Matrix4x4 ToMatrix4x4()
    {
        Matrix4x4 mat = new Matrix4x4();

        mat.m00 = (float)m[0, 0]; mat.m01 = (float)m[0, 1]; mat.m02 = (float)m[0, 2];
        mat.m10 = (float)m[1, 0]; mat.m11 = (float)m[1, 1]; mat.m12 = (float)m[1, 2];
        mat.m20 = (float)m[2, 0]; mat.m21 = (float)m[2, 1]; mat.m22 = (float)m[2, 2];

        return mat;
    }

    static public Matrix3x3d Identity()
    {
        return new Matrix3x3d(1, 0, 0, 0, 1, 0, 0, 0, 1);
    }

}

























