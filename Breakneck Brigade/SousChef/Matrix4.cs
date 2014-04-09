using System;

namespace SousChef
{
    /// <summary>
    /// A 4x4 homogenous matrix for storing 3D transformations.
    /// </summary>
    /// <author>
    /// Ryan George
    /// </author>
    public class Matrix4
    {
        /// <summary>
        /// Backing data structure for the matrix
        /// </summary>
        public float[,] backingArray    { get; private set; }
        /// <summary>
        /// Generated array for use in OpenGL. Updated on backing array mutation
        /// </summary>
        public float[]  glArray         { get; private set; }
        
        /// <summary>
        /// Initializes matrix to identity matrix.
        /// </summary>
        public Matrix4()
        {
            backingArray    = new float[4,4];
            glArray         = new float[16];
	        for (int i=0; i<4; ++i)
	        {
		        for (int j=0; j<4; ++j)
		        {
			        backingArray[i,j] = 0;
		        }
	        }
            UpdateGLArray();
        } 

        /// <summary>
        /// Initializes the matrix to the specified matrix with row-major parameter order.
        /// That is, the matrix will have the values:
        /// m00, m01, m02, m03
        /// m10, m11, m12, m13
        /// m20, m21, m22, m23
        /// m30, m31, m32, m33
        /// </summary>
        /// <param name="m00"></param>
        /// <param name="m01"></param>
        /// <param name="m02"></param>
        /// <param name="m03"></param>
        /// <param name="m10"></param>
        /// <param name="m11"></param>
        /// <param name="m12"></param>
        /// <param name="m13"></param>
        /// <param name="m20"></param>
        /// <param name="m21"></param>
        /// <param name="m22"></param>
        /// <param name="m23"></param>
        /// <param name="m30"></param>
        /// <param name="m31"></param>
        /// <param name="m32"></param>
        /// <param name="m33"></param>
        public Matrix4( float m00, float m01, float m02, float m03,
                        float m10, float m11, float m12, float m13,
                        float m20, float m21, float m22, float m23,
                        float m30, float m31, float m32, float m33 )
        {
            backingArray    = new float[4, 4];
            glArray         = new float[16];

            backingArray[0,0] = m00;
            backingArray[0,1] = m01;
            backingArray[0,2] = m02;
            backingArray[0,3] = m03;
            backingArray[1,0] = m10;
            backingArray[1,1] = m11;
            backingArray[1,2] = m12;
            backingArray[1,3] = m13;
            backingArray[2,0] = m20;
            backingArray[2,1] = m21;
            backingArray[2,2] = m22;
            backingArray[2,3] = m23;
            backingArray[3,0] = m30;
            backingArray[3,1] = m31;
            backingArray[3,2] = m32;
            backingArray[3,3] = m33;
            UpdateGLArray();
        }

        /// <summary>
        /// Initializes the matrix to the specified matrix with row-major parameter order.
        /// That is, the matrix will have the values:
        /// m00, m01, m02, m03
        /// m10, m11, m12, m13
        /// m20, m21, m22, m23
        /// m30, m31, m32, m33
        /// </summary>
        /// <param name="m00"></param>
        /// <param name="m01"></param>
        /// <param name="m02"></param>
        /// <param name="m03"></param>
        /// <param name="m10"></param>
        /// <param name="m11"></param>
        /// <param name="m12"></param>
        /// <param name="m13"></param>
        /// <param name="m20"></param>
        /// <param name="m21"></param>
        /// <param name="m22"></param>
        /// <param name="m23"></param>
        /// <param name="m30"></param>
        /// <param name="m31"></param>
        /// <param name="m32"></param>
        /// <param name="m33"></param>
        public Matrix4( double m00, double m01, double m02, double m03,
                        double m10, double m11, double m12, double m13,
                        double m20, double m21, double m22, double m23,
                        double m30, double m31, double m32, double m33 )
        {
            backingArray    = new float[4, 4];
            glArray         = new float[16];
            backingArray[0,0] = (float) m00;
            backingArray[0,1] = (float) m01;
            backingArray[0,2] = (float) m02;
            backingArray[0,3] = (float) m03;
            backingArray[1,0] = (float) m10;
            backingArray[1,1] = (float) m11;
            backingArray[1,2] = (float) m12;
            backingArray[1,3] = (float) m13;
            backingArray[2,0] = (float) m20;
            backingArray[2,1] = (float) m21;
            backingArray[2,2] = (float) m22;
            backingArray[2,3] = (float) m23;
            backingArray[3,0] = (float) m30;
            backingArray[3,1] = (float) m31;
            backingArray[3,2] = (float) m32;
            backingArray[3,3] = (float) m33;
            UpdateGLArray();
        }

        /// <summary>
        /// Quick method to set a value in the matrix.
        /// </summary>
        /// <param name="x">Column</param>
        /// <param name="y">Row</param>
        /// <returns></returns>
        public float this[int x, int y]
        {
            get { return backingArray[x, y];                    }
            set { backingArray[x, y] = value; UpdateGLArray();  }
        }

        public void SetAll( float m00, float m10, float m20, float m30,
                            float m01, float m11, float m21, float m31,
                            float m02, float m12, float m22, float m32,
                            float m03, float m13, float m23, float m33 )
        {
            backingArray[0,0] = m00;
            backingArray[0,1] = m01;
            backingArray[0,2] = m02;
            backingArray[0,3] = m03;
            backingArray[1,0] = m10;
            backingArray[1,1] = m11;
            backingArray[1,2] = m12;
            backingArray[1,3] = m13;
            backingArray[2,0] = m20;
            backingArray[2,1] = m21;
            backingArray[2,2] = m22;
            backingArray[2,3] = m23;
            backingArray[3,0] = m30; 
            backingArray[3,1] = m31; 
            backingArray[3,2] = m32; 
            backingArray[3,3] = m33;
            UpdateGLArray();
        }

        /// <summary>
        /// Sets all values in the matrix in row-major parameter order.
        /// 
        /// That is, the matrix will have the values:
        /// m00, m01, m02, m03
        /// m10, m11, m12, m13
        /// m20, m21, m22, m23
        /// m30, m31, m32, m33 
        /// </summary>
        /// <param name="m00"></param>
        /// <param name="m10"></param>
        /// <param name="m20"></param>
        /// <param name="m30"></param>
        /// <param name="m01"></param>
        /// <param name="m11"></param>
        /// <param name="m21"></param>
        /// <param name="m31"></param>
        /// <param name="m02"></param>
        /// <param name="m12"></param>
        /// <param name="m22"></param>
        /// <param name="m32"></param>
        /// <param name="m03"></param>
        /// <param name="m13"></param>
        /// <param name="m23"></param>
        /// <param name="m33"></param>
        public void SetAll( double m00, double m10, double m20, double m30,
                            double m01, double m11, double m21, double m31,
                            double m02, double m12, double m22, double m32,
                            double m03, double m13, double m23, double m33 )
        {
            backingArray[0,0] = (float) m00;
            backingArray[0,1] = (float) m01;
            backingArray[0,2] = (float) m02;
            backingArray[0,3] = (float) m03;
            backingArray[1,0] = (float) m10;
            backingArray[1,1] = (float) m11;
            backingArray[1,2] = (float) m12;
            backingArray[1,3] = (float) m13;
            backingArray[2,0] = (float) m20;
            backingArray[2,1] = (float) m21;
            backingArray[2,2] = (float) m22;
            backingArray[2,3] = (float) m23;
            backingArray[3,0] = (float) m30; 
            backingArray[3,1] = (float) m31; 
            backingArray[3,2] = (float) m32; 
            backingArray[3,3] = (float) m33;
            UpdateGLArray();
        }

        /// <summary>
        /// Multiplies this matrix by another matrix.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>The result of the matrix multiplication</returns>
        public Matrix4 Multiply(Matrix4 other)
        {
	        Matrix4 result = new Matrix4(   0, 0, 0, 0,
                                            0, 0, 0, 0,
                                            0, 0, 0, 0,
                                            0, 0, 0, 0);
	        for(int ii = 0; ii < 4; ii++)
	        {
		        for(int jj = 0; jj < 4; jj++)
		        {
			        for(int kk = 0; kk < 4; kk++)
			        {
				        result[ii,jj] += backingArray[kk,jj] * other[ii,kk];
			        }
		        }
	        }
    
            return result;
        }

        /// <summary>
        /// Makes the matrix homogenous by dividing every value by the homogenous coordinate (m[3][3])
        /// </summary>
        public void Homogenize()
        {
	        for(int ii = 0; ii < 4; ii++)
	        {
		        for(int jj = 0; jj < 4; jj++)
		        {
			        backingArray[ii,jj] /= backingArray[3,3];
		        }
	        }
        }

        /// <summary>
        /// Multiplies this matrix by a 4x1 vector by inverting the vector to a 1x4 vector.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>A 1x4 vector which is the result of the multiplication. </returns>
        public Vector4 Multiply(Vector4 other)
        {
            Vector4 result = new Vector4();
	        for(int ii = 0; ii < 4; ii++)
	        {
		        for(int jj = 0; jj < 4 ; jj++)
		        {
			        result[ii] += backingArray[jj,ii] * other[jj];
		        }
	        }
	        return result;
        }

        /// <summary>
        /// Sets the instance of this matrix to have the values of the identity matrix.
        /// </summary>
        public void Identity()
        {
            SetAll(1, 0, 0, 0,
                   0, 1, 0, 0,
                   0, 0, 1, 0,
                   0, 0, 0, 1);

        }

        /// <summary>
        /// Creates a rotation around the world X axis
        /// </summary>
        /// <param name="angle">The angle to rotate around in radians</param>
        public void RotateX(float angle)
        {
	        backingArray[0,0] = 1;
	        backingArray[0,1] = 0;
	        backingArray[0,2] = 0;
	        backingArray[0,3] = 0;
	        backingArray[1,0] = 0;
	        backingArray[1,1] = (float) Math.Cos(angle);
	        backingArray[1,2] = (float) Math.Sin(angle);
	        backingArray[1,3] = 0;
	        backingArray[2,0] = 0;
	        backingArray[2,1] = (float) - Math.Sin(angle);
	        backingArray[2,2] = (float) Math.Cos(angle);
	        backingArray[2,3] = 0;
	        backingArray[3,0] = 0;
	        backingArray[3,1] = 0;
	        backingArray[3,2] = 0;
	        backingArray[3,3] = 1;
        }

        /// <summary>
        /// Creates a rotation around the world Y axis
        /// </summary>
        /// <param name="angle">The angle to rotate around in radians</param>
        public void RotateY(float angle)
        {
	        backingArray[0,0] = (float) Math.Cos(angle);
	        backingArray[0,1] = 0;
	        backingArray[0,2] = (float) -Math.Sin(angle);
	        backingArray[0,3] = 0;
	        backingArray[1,0] = 0;
	        backingArray[1,1] = 1;
	        backingArray[1,2] = 0;
	        backingArray[1,3] = 0;
	        backingArray[2,0] = (float) Math.Sin(angle);
	        backingArray[2,1] = 0;
	        backingArray[2,2] = (float) Math.Cos(angle);
	        backingArray[2,3] = 0;
	        backingArray[3,0] = 0;
	        backingArray[3,1] = 0;
	        backingArray[3,2] = 0;
	        backingArray[3,3] = 1;
        }

        /// <summary>
        /// Creates a rotation around the world Z axis
        /// </summary>
        /// <param name="angle">The angle to rotate around in radians</param>
        public void RotateZ(float angle)
        {
	        backingArray[0,0] = (float) Math.Cos(angle);
	        backingArray[0,1] = (float) Math.Sin(angle);
	        backingArray[0,2] = 0;
	        backingArray[0,3] = 0;
	        backingArray[1,0] = (float) -Math.Sin(angle);
	        backingArray[1,1] = (float) Math.Cos(angle);
	        backingArray[1,2] = 0;
	        backingArray[1,3] = 0;
	        backingArray[2,0] = 0;
	        backingArray[2,1] = 0;
	        backingArray[2,2] = 1;
	        backingArray[2,3] = 0;
	        backingArray[3,0] = 0;
	        backingArray[3,1] = 0;
	        backingArray[3,2] = 0;
	        backingArray[3,3] = 1;
        }

        /// <summary>
        /// Creates a rotation around the world X axis
        /// </summary>
        /// <param name="angle">The angle to rotate around in degrees</param>
        public void RotateXDeg(float angle)
        {
	        angle = (angle/180.0f) * MathConstants.INVERSE_PI;

	        backingArray[0,0] = 1;
	        backingArray[0,1] = 0;
	        backingArray[0,2] = 0;
	        backingArray[0,3] = 0;
	        backingArray[1,0] = 0;
	        backingArray[1,1] = (float) Math.Cos(angle);
	        backingArray[1,2] = (float) Math.Sin(angle);
	        backingArray[1,3] = 0;
	        backingArray[2,0] = 0;
	        backingArray[2,1] = (float) - Math.Sin(angle);
	        backingArray[2,2] = (float) Math.Cos(angle);
	        backingArray[2,3] = 0;
	        backingArray[3,0] = 0;
	        backingArray[3,1] = 0;
	        backingArray[3,2] = 0;
	        backingArray[3,3] = 1;
        }

        /// <summary>
        /// Creates a rotation around the world Y axis
        /// </summary>
        /// <param name="angle">The angle to rotate around in degrees</param>
        public void RotateYDeg(float angle)
        {
            angle = (angle/180.0f) * MathConstants.INVERSE_PI;

	        backingArray[0,0] = (float) Math.Cos(angle);
	        backingArray[0,1] = 0;
	        backingArray[0,2] = (float) -Math.Sin(angle);
	        backingArray[0,3] = 0;
	        backingArray[1,0] = 0;
	        backingArray[1,1] = 1;
	        backingArray[1,2] = 0;
	        backingArray[1,3] = 0;
	        backingArray[2,0] = (float) Math.Sin(angle);
	        backingArray[2,1] = 0;
	        backingArray[2,2] = (float) Math.Cos(angle);
	        backingArray[2,3] = 0;
	        backingArray[3,0] = 0;
	        backingArray[3,1] = 0;
	        backingArray[3,2] = 0;
	        backingArray[3,3] = 1;
        }

        /// <summary>
        /// Creates a rotation around the world Z axis
        /// </summary>
        /// <param name="angle">The angle to rotate around in degrees</param>
        public void RotateZDeg(float angle)
        {
            angle = (float) (angle/180.0) * MathConstants.INVERSE_PI;

	        backingArray[0,0] = (float) Math.Cos(angle);
	        backingArray[0,1] = (float) Math.Sin(angle);
	        backingArray[0,2] = 0;
	        backingArray[0,3] = 0;
	        backingArray[1,0] = (float) -Math.Sin(angle);
	        backingArray[1,1] = (float) Math.Cos(angle);
	        backingArray[1,2] = 0;
	        backingArray[1,3] = 0;
	        backingArray[2,0] = 0;
	        backingArray[2,1] = 0;
	        backingArray[2,2] = 1;
	        backingArray[2,3] = 0;
	        backingArray[3,0] = 0;
	        backingArray[3,1] = 0;
	        backingArray[3,2] = 0;
	        backingArray[3,3] = 1;
        }


        /// <summary>
        /// Creates a rotation around an arbitrary axis
        /// </summary>
        /// <param name="axis">The axis to rate around as a normalized vector</param>
        /// <param name="angle">The angle to rotate around this axis in radians</param>
        /// <returns></returns>
        public Matrix4 Rotate(Vector4 axis, float angle)
        {
            float cTheta = (float)Math.Cos(angle);
            float sTheta = (float)Math.Sin(angle);
            float ax = axis[0];
            float ay = axis[1];
            float az = axis[2];

            float[,] t = new float[4, 4];

            t[0, 0] = (float)(Math.Pow(ax, 2) + cTheta*(1-Math.Pow(ax, 2)));
            t[0, 1] = ax*ay*(1-cTheta) + az*sTheta;
            t[0, 2] = ax*az*(1-cTheta) - ay*sTheta;
            t[0, 3] = 0;
            t[1, 0] = ax*ay*(1-cTheta) - az * sTheta;
            t[1, 1] = (float)(Math.Pow(ay, 2) + cTheta * (1 - Math.Pow(ay, 2)));
            t[1, 2] = ay*az*(1-cTheta) + ax*sTheta;
            t[1, 3] = 0;
            t[2, 0] = ax*az*(1-cTheta) + ay*sTheta;
            t[2, 1] = ay*az*(1-cTheta)-ax*sTheta;
            t[2, 2] = (float)(Math.Pow(az, 2) + cTheta*(1-Math.Pow(az, 2)));
            t[2, 3] = 0;
            t[3, 0] = 0;
            t[3, 1] = 0;
            t[3, 2] = 0;
            t[3, 3] = 1;

            return new Matrix4(t[0, 0], t[0, 1], t[0, 2], t[0, 3],
                                t[1, 0], t[1, 1], t[1, 2], t[1, 3],
                                t[2, 0], t[2, 1], t[2, 2], t[2, 3],
                                t[3, 0], t[3, 1], t[3, 2], t[3, 3]);
        }

        /// <summary>
        /// Creates a rotation around an arbitrary axis
        /// </summary>
        /// <param name="axis">The axis to rate around as a normalized vector</param>
        /// <param name="angle">The angle to rotate around this axis in degrees</param>
        /// <returns></returns>
        public Matrix4 RotateDeg(Vector4 axis, float angle)
        {
	        angle = (float) (angle/180.0) * MathConstants.INVERSE_PI;
	        float cTheta = (float) Math.Cos(angle);
	        float sTheta = (float) Math.Sin(angle);
	        float ax = axis[0];
	        float ay = axis[1];
	        float az = axis[2];

            float[,] t = new float[4,4];

	        t[0,0] = (float) (Math.Pow(ax,2) + cTheta*(1-Math.Pow(ax,2)));
	        t[0,1] = ax*ay*(1-cTheta) + az*sTheta;
	        t[0,2] = ax*az*(1-cTheta) - ay*sTheta;
	        t[0,3] = 0;
	        t[1,0] = ax*ay*(1-cTheta) - az * sTheta;
            t[1,1] = (float) (Math.Pow(ay, 2) + cTheta * (1 - Math.Pow(ay, 2)));
	        t[1,2] = ay*az*(1-cTheta) + ax*sTheta;
	        t[1,3] = 0;
	        t[2,0] = ax*az*(1-cTheta) + ay*sTheta;
	        t[2,1] = ay*az*(1-cTheta)-ax*sTheta;
	        t[2,2] = (float) (Math.Pow(az,2) + cTheta*(1-Math.Pow(az,2)));
	        t[2,3] = 0;
	        t[3,0] = 0;
	        t[3,1] = 0;
            t[3,2] = 0;
	        t[3,3] = 1;
    
            return new Matrix4( t[0,0], t[0,1], t[0,2], t[0,3],
                                t[1,0], t[1,1], t[1,2], t[1,3],
                                t[2,0], t[2,1], t[2,2], t[2,3],
                                t[3,0], t[3,1], t[3,2], t[3,3]);
        }

        /// <summary>
        /// Turns this matrix into a scaling matrix, scaling each dimension by
        /// the corresponding paremeter.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void ScalingMat(float x, float y, float z)
        {
	        backingArray[0,0] = x;
	        backingArray[0,1] = 0;
	        backingArray[0,2] = 0;
	        backingArray[0,3] = 0;
	        backingArray[1,0] = 0;
	        backingArray[1,1] = y;
	        backingArray[1,2] = 0;
	        backingArray[1,3] = 0;
	        backingArray[2,0] = 0;
	        backingArray[2,1] = 0;
	        backingArray[2,2] = z;
	        backingArray[2,3] = 0;
	        backingArray[3,0] = 0;
	        backingArray[3,1] = 0;
	        backingArray[3,2] = 0;
	        backingArray[3,3] = 1;
        }
        /// <summary>
        /// Turns this matrix into a translation matrix, translating the
        /// model by the arguments in each specified dimension.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void TranslationMat(float x, float y, float z)
        {
	        backingArray[0,0] = 1;
	        backingArray[0,1] = 0;
	        backingArray[0,2] = 0;
	        backingArray[0,3] = 0;
	        backingArray[1,0] = 0;
	        backingArray[1,1] = 1;
	        backingArray[1,2] = 0;
	        backingArray[1,3] = 0;
	        backingArray[2,0] = 0;
	        backingArray[2,1] = 0;
	        backingArray[2,2] = 1;
	        backingArray[2,3] = 0;
	        backingArray[3,0] = x;
	        backingArray[3,1] = y;
	        backingArray[3,2] = z;
	        backingArray[3,3] = 1;
        }

        /// <summary>
        /// Transposes the matrix (that is, reflects across the down-right diagonal)
        /// </summary>
        public void Transpose()
        {
	        float temp;
	        for(int ii = 3; ii >= 0; ii--)
	        {
		        for(int jj = ii-1; jj >=0; jj--)
		        {
			        temp = backingArray[ii,jj];
			        backingArray[ii,jj] = backingArray[jj,ii];
			        backingArray[jj,ii] = temp;
		        }
	        }
        }

        /// <summary>
        /// A fully calculated matrix inverse. Substantially more expensive than the FastInverse. Should
        /// only be used when FastInverse fails to invert the matrix.
        /// </summary>
        public void Invert()
        {
            float det = backingArray[0,0]*backingArray[1,1]*backingArray[2,2]*backingArray[3,3]+backingArray[0,0]*backingArray[1,2]*backingArray[2,3]*backingArray[3,1]+backingArray[0,0]*backingArray[1,3]*backingArray[2,1]*backingArray[3,2]
            +backingArray[0,1]*backingArray[1,0]*backingArray[2,3]*backingArray[3,2]+backingArray[0,1]*backingArray[1,2]*backingArray[2,0]*backingArray[3,3]+backingArray[0,1]*backingArray[1,3]*backingArray[2,2]*backingArray[3,0]
            +backingArray[0,2]*backingArray[1,0]*backingArray[2,1]*backingArray[3,3]+backingArray[0,2]*backingArray[1,1]*backingArray[2,3]*backingArray[3,0]+backingArray[0,2]*backingArray[1,3]*backingArray[2,0]*backingArray[3,1]
            +backingArray[0,3]*backingArray[1,0]*backingArray[2,2]*backingArray[3,1]+backingArray[0,3]*backingArray[1,1]*backingArray[2,0]*backingArray[3,2]+backingArray[0,3]*backingArray[1,2]*backingArray[2,1]*backingArray[3,0]
            -backingArray[0,0]*backingArray[1,1]*backingArray[2,3]*backingArray[3,2]-backingArray[0,0]*backingArray[1,2]*backingArray[2,1]*backingArray[3,3]-backingArray[0,0]*backingArray[1,3]*backingArray[2,2]*backingArray[3,1]
            -backingArray[0,1]*backingArray[1,0]*backingArray[2,2]*backingArray[3,3]-backingArray[0,1]*backingArray[1,2]*backingArray[2,3]*backingArray[3,0]-backingArray[0,1]*backingArray[1,3]*backingArray[2,0]*backingArray[3,2]
            -backingArray[0,2]*backingArray[1,0]*backingArray[2,3]*backingArray[3,1]-backingArray[0,2]*backingArray[1,1]*backingArray[2,0]*backingArray[3,3]-backingArray[0,2]*backingArray[1,3]*backingArray[2,1]*backingArray[3,0]
            -backingArray[0,3]*backingArray[1,0]*backingArray[2,1]*backingArray[3,2]-backingArray[0,3]*backingArray[1,1]*backingArray[2,2]*backingArray[3,0]-backingArray[0,3]*backingArray[1,2]*backingArray[2,0]*backingArray[3,1];
            float[,] b = new float[4,4];
            b[0,0] = backingArray[1,1]*backingArray[2,2]*backingArray[3,3]+backingArray[1,2]*backingArray[2,3]*backingArray[3,1]+backingArray[1,3]*backingArray[2,1]*backingArray[3,2]-backingArray[1,1]*backingArray[2,3]*backingArray[3,2]-backingArray[1,2]*backingArray[2,1]*backingArray[3,3]-backingArray[1,3]*backingArray[2,2]*backingArray[3,1];
            b[0,1] = backingArray[0,1]*backingArray[2,3]*backingArray[3,2]+backingArray[0,2]*backingArray[2,1]*backingArray[3,3]+backingArray[0,3]*backingArray[2,2]*backingArray[3,1]-backingArray[0,1]*backingArray[2,2]*backingArray[3,3]-backingArray[0,2]*backingArray[2,3]*backingArray[3,1]-backingArray[0,3]*backingArray[2,1]*backingArray[3,2];
            b[0,2] = backingArray[0,1]*backingArray[1,2]*backingArray[3,3]+backingArray[0,2]*backingArray[1,3]*backingArray[3,1]+backingArray[0,3]*backingArray[1,1]*backingArray[3,2]-backingArray[0,1]*backingArray[1,3]*backingArray[3,2]-backingArray[0,2]*backingArray[1,1]*backingArray[3,3]-backingArray[0,3]*backingArray[1,2]*backingArray[3,1];
            b[0,3] = backingArray[0,1]*backingArray[1,3]*backingArray[2,2]+backingArray[0,2]*backingArray[1,1]*backingArray[2,3]+backingArray[0,3]*backingArray[1,2]*backingArray[2,1]-backingArray[0,1]*backingArray[1,2]*backingArray[2,3]-backingArray[0,2]*backingArray[1,3]*backingArray[2,1]-backingArray[0,3]*backingArray[1,1]*backingArray[2,2];
            b[1,0] = backingArray[1,0]*backingArray[2,3]*backingArray[3,2]+backingArray[1,2]*backingArray[2,0]*backingArray[3,3]+backingArray[1,3]*backingArray[2,2]*backingArray[3,0]-backingArray[1,0]*backingArray[2,2]*backingArray[3,3]-backingArray[1,2]*backingArray[2,3]*backingArray[3,0]-backingArray[1,3]*backingArray[2,0]*backingArray[3,2];
            b[1,1] = backingArray[0,0]*backingArray[2,2]*backingArray[3,3]+backingArray[0,2]*backingArray[2,3]*backingArray[3,0]+backingArray[0,3]*backingArray[2,0]*backingArray[3,2]-backingArray[0,0]*backingArray[2,3]*backingArray[3,2]-backingArray[0,2]*backingArray[2,0]*backingArray[3,3]-backingArray[0,3]*backingArray[2,2]*backingArray[3,0];
            b[1,2] = backingArray[0,0]*backingArray[1,3]*backingArray[3,2]+backingArray[0,2]*backingArray[1,0]*backingArray[3,3]+backingArray[0,3]*backingArray[1,2]*backingArray[3,0]-backingArray[0,0]*backingArray[1,2]*backingArray[3,3]-backingArray[0,2]*backingArray[1,3]*backingArray[3,0]-backingArray[0,3]*backingArray[1,0]*backingArray[3,2];
            b[1,3] = backingArray[0,0]*backingArray[1,2]*backingArray[2,3]+backingArray[0,2]*backingArray[1,3]*backingArray[2,0]+backingArray[0,3]*backingArray[1,0]*backingArray[2,2]-backingArray[0,0]*backingArray[1,3]*backingArray[2,2]-backingArray[0,2]*backingArray[1,0]*backingArray[2,3]-backingArray[0,3]*backingArray[1,2]*backingArray[2,0];
            b[2,0] = backingArray[1,0]*backingArray[2,1]*backingArray[3,3]+backingArray[1,1]*backingArray[2,3]*backingArray[3,0]+backingArray[1,3]*backingArray[2,0]*backingArray[3,1]-backingArray[1,0]*backingArray[2,3]*backingArray[3,1]-backingArray[1,1]*backingArray[2,0]*backingArray[3,3]-backingArray[1,3]*backingArray[2,1]*backingArray[3,0];
            b[2,1] = backingArray[0,0]*backingArray[2,3]*backingArray[3,1]+backingArray[0,1]*backingArray[2,0]*backingArray[3,3]+backingArray[0,3]*backingArray[2,1]*backingArray[3,0]-backingArray[0,0]*backingArray[2,1]*backingArray[3,3]-backingArray[0,1]*backingArray[2,3]*backingArray[3,0]-backingArray[0,3]*backingArray[2,0]*backingArray[3,1];
            b[2,2] = backingArray[0,0]*backingArray[1,1]*backingArray[3,3]+backingArray[0,1]*backingArray[1,3]*backingArray[3,0]+backingArray[0,3]*backingArray[1,0]*backingArray[3,1]-backingArray[0,0]*backingArray[1,3]*backingArray[3,1]-backingArray[0,1]*backingArray[1,0]*backingArray[3,3]-backingArray[0,3]*backingArray[1,1]*backingArray[3,0];
            b[2,3] = backingArray[0,0]*backingArray[1,3]*backingArray[2,1]+backingArray[0,1]*backingArray[1,0]*backingArray[2,3]+backingArray[0,3]*backingArray[1,1]*backingArray[2,0]-backingArray[0,0]*backingArray[1,1]*backingArray[2,3]-backingArray[0,1]*backingArray[1,3]*backingArray[2,0]-backingArray[0,3]*backingArray[1,0]*backingArray[2,1];
            b[3,0] = backingArray[1,0]*backingArray[2,2]*backingArray[3,1]+backingArray[1,1]*backingArray[2,0]*backingArray[3,2]+backingArray[1,2]*backingArray[2,1]*backingArray[3,0]-backingArray[1,0]*backingArray[2,1]*backingArray[3,2]-backingArray[1,1]*backingArray[2,2]*backingArray[3,0]-backingArray[1,2]*backingArray[2,0]*backingArray[3,1];
            b[3,1] = backingArray[0,0]*backingArray[2,1]*backingArray[3,2]+backingArray[0,1]*backingArray[2,2]*backingArray[3,0]+backingArray[0,2]*backingArray[2,0]*backingArray[3,1]-backingArray[0,0]*backingArray[2,2]*backingArray[3,1]-backingArray[0,1]*backingArray[2,0]*backingArray[3,2]-backingArray[0,2]*backingArray[2,1]*backingArray[3,0];
            b[3,2] = backingArray[0,0]*backingArray[1,2]*backingArray[3,1]+backingArray[0,1]*backingArray[1,0]*backingArray[3,2]+backingArray[0,2]*backingArray[1,1]*backingArray[3,0]-backingArray[0,0]*backingArray[1,1]*backingArray[3,2]-backingArray[0,1]*backingArray[1,2]*backingArray[3,0]-backingArray[0,2]*backingArray[1,0]*backingArray[3,1];
            b[3,3] = backingArray[0,0]*backingArray[1,1]*backingArray[2,2]+backingArray[0,1]*backingArray[1,2]*backingArray[2,0]+backingArray[0,2]*backingArray[1,0]*backingArray[2,1]-backingArray[0,0]*backingArray[1,2]*backingArray[2,1]-backingArray[0,1]*backingArray[1,0]*backingArray[2,2]-backingArray[0,2]*backingArray[1,1]*backingArray[2,0];
            for(int i=0;i<4;i++)
                for(int j=0;j<4;j++)
                    backingArray[i,j] = b[i,j]/det;
        }
        

        /// <summary>
        /// A quick matrix inversion. This will only work if the matrix is actually homogenous.
        /// </summary>
        public void FastInvert()
        {
            backingArray[0,0] = backingArray[0,0];
	        backingArray[0,1] = backingArray[1,0];
	        backingArray[0,2] = backingArray[2,0];

	        backingArray[1,0] = backingArray[0,1];
	        backingArray[1,1] = backingArray[1,1];
	        backingArray[1,2] = backingArray[2,1];

	        backingArray[2,0] = backingArray[0,2];
            backingArray[2,1] = backingArray[1,2];
            backingArray[2,2] = backingArray[2,2];

	        backingArray[3,0] = -backingArray[3,0];
	        backingArray[3,1] = -backingArray[3,1];
	        backingArray[3,2] = -backingArray[3,2];
        }

        /// <summary>
        /// Multiplies all values in the matrix by this scalar (including the homogenous coordinate)
        /// </summary>
        /// <param name="scalar"></param>
        public void Scale(float scalar)
        {
	        for(int ii = 0; ii < 4; ii++)
	        {
		        for(int jj = 0; jj < 4; jj++)
		        {
			        backingArray[ii,jj] *= scalar;
		        }
	        }
        }
        /// <summary>
        /// Prints the values in the matrix to the console.
        /// </summary>
        public void Print()
        {
	        Console.WriteLine("Matrix4: \n" +
                    backingArray[0,0] + ", " + backingArray[1,0] + ", " + backingArray[2,0] + ", " + backingArray[3,0] + "\n" +
			        backingArray[0,1] + ", " + backingArray[1,1] + ", " + backingArray[2,1] + ", " + backingArray[3,1] + "\n" +
			        backingArray[0,2] + ", " + backingArray[1,2] + ", " + backingArray[2,2] + ", " + backingArray[3,2] + "\n" +
			        backingArray[0,3] + ", " + backingArray[1,3] + ", " + backingArray[2,3] + ", " + backingArray[3,3] + "\n" );
        }

        /// <summary>
        /// Sets the matrix to be the inverse of the Hermite matrix, commonly used for animation easing.
        /// </summary>
        public void HermiteInverse()
        {
	        backingArray[0,0] = 2;
	        backingArray[0,1] = -3;
	        backingArray[0,2] = 0;
	        backingArray[0,3] = 1;

	        backingArray[1,0] = -2;
	        backingArray[1,1] = 3;
	        backingArray[1,2] = 0;
	        backingArray[1,3] = 0;

	        backingArray[2,0] = 1;
	        backingArray[2,1] = -2;
	        backingArray[2,2] = 1;
	        backingArray[2,3] = 0;

	        backingArray[3,0] = 1;
	        backingArray[3,1] = -1;
	        backingArray[3,2] = 0;
	        backingArray[3,3] = 0;
        }

        /// <summary>
        /// Copies the rotation compoment of the matrix (the square defined by 0,0 to 2,2)
        /// </summary>
        /// <param name="mat"></param>
        public void CopyRot(Matrix4 mat)
        {
            backingArray[0,0] = mat[0, 0];
            backingArray[0,1] = mat[0, 1];
            backingArray[0,2] = mat[0, 2];
            backingArray[1,0] = mat[1, 0];
            backingArray[1,1] = mat[1, 1];
            backingArray[1,2] = mat[1, 2];
            backingArray[2,0] = mat[2, 0];
            backingArray[2,1] = mat[2, 1];
            backingArray[2,2] = mat[2, 2];
        }

        /// <summary>
        /// Updates the OpenGL array
        /// </summary>
        private void UpdateGLArray()
        {
            glArray[0]  = backingArray[0, 0];
            glArray[1]  = backingArray[0, 1];
            glArray[2]  = backingArray[0, 2];
            glArray[3]  = backingArray[0, 3];
            glArray[4]  = backingArray[1, 0];
            glArray[5]  = backingArray[1, 1];
            glArray[6]  = backingArray[1, 2];
            glArray[7]  = backingArray[1, 3];
            glArray[8]  = backingArray[2, 0];
            glArray[9]  = backingArray[2, 1];
            glArray[10] = backingArray[2, 2];
            glArray[11] = backingArray[2, 3];
            glArray[12] = backingArray[3, 0];
            glArray[13] = backingArray[3, 1];
            glArray[14] = backingArray[3, 2];
            glArray[15] = backingArray[3, 3];
        }
    }
}
