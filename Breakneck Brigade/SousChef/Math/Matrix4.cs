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
        /// Generated array for use in OpenGL. Updated on backing array mutation
        /// </summary>
        public float[]  glArray         { get; private set; }
        
        /// <summary>
        /// Initializes matrix to identity matrix.
        /// </summary>
        public Matrix4()
        {
            glArray         = new float[16];
            this.Identity();
        } 

        /// <summary>
        /// Copy constructor. Copies the values and instanciates a new matrix from the values.
        /// </summary>
        /// <param name="toCopy"></param>
        public Matrix4(Matrix4 toCopy)
        {
            glArray    = new float[16];
            this[0, 0] = toCopy[0, 0];
            this[0, 1] = toCopy[0, 1];
            this[0, 2] = toCopy[0, 2];
            this[0, 3] = toCopy[0, 3];
            this[1, 0] = toCopy[1, 0];
            this[1, 1] = toCopy[1, 1];
            this[1, 2] = toCopy[1, 2];
            this[1, 3] = toCopy[1, 3];
            this[2, 0] = toCopy[2, 0];
            this[2, 1] = toCopy[2, 1];
            this[2, 2] = toCopy[2, 2];
            this[2, 3] = toCopy[2, 3];
            this[3, 0] = toCopy[3, 0];
            this[3, 1] = toCopy[3, 1];
            this[3, 2] = toCopy[3, 2];
            this[3, 3] = toCopy[3, 3];
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
            glArray         = new float[16];

            this[0,0] = m00;
            this[0,1] = m01;
            this[0,2] = m02;
            this[0,3] = m03;
            this[1,0] = m10;
            this[1,1] = m11;
            this[1,2] = m12;
            this[1,3] = m13;
            this[2,0] = m20;
            this[2,1] = m21;
            this[2,2] = m22;
            this[2,3] = m23;
            this[3,0] = m30;
            this[3,1] = m31;
            this[3,2] = m32;
            this[3,3] = m33;
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
            glArray         = new float[16];
            this[0,0] = (float) m00;
            this[0,1] = (float) m01;
            this[0,2] = (float) m02;
            this[0,3] = (float) m03;
            this[1,0] = (float) m10;
            this[1,1] = (float) m11;
            this[1,2] = (float) m12;
            this[1,3] = (float) m13;
            this[2,0] = (float) m20;
            this[2,1] = (float) m21;
            this[2,2] = (float) m22;
            this[2,3] = (float) m23;
            this[3,0] = (float) m30;
            this[3,1] = (float) m31;
            this[3,2] = (float) m32;
            this[3,3] = (float) m33;
        }

        /// <summary>
        /// Initializes a matrix that is transformed to position from the origin
        /// </summary>
        /// <param name="position"></param>
        public Matrix4(Vector4 position)
        {
            glArray         = new float[16];
            this.Identity();
            //this.TranslationMat(position[0], position[1], position[2]);
        }

        /// <summary>
        /// Quick method to set a value in the matrix.
        /// </summary>
        /// <param name="x">Column</param>
        /// <param name="y">Row</param>
        /// <returns></returns>
        public float this[int x, int y]
        {
            get { return glArray[4 * x + y]; }
            set { glArray[4 * x + y] = value; }
        }

        public void SetAll( float m00, float m10, float m20, float m30,
                            float m01, float m11, float m21, float m31,
                            float m02, float m12, float m22, float m32,
                            float m03, float m13, float m23, float m33 )
        {
            this[0,0] = m00;
            this[0,1] = m01;
            this[0,2] = m02;
            this[0,3] = m03;
            this[1,0] = m10;
            this[1,1] = m11;
            this[1,2] = m12;
            this[1,3] = m13;
            this[2,0] = m20;
            this[2,1] = m21;
            this[2,2] = m22;
            this[2,3] = m23;
            this[3,0] = m30; 
            this[3,1] = m31; 
            this[3,2] = m32; 
            this[3,3] = m33;
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
        /// <returns>A reference to the matrix SetAll was called on</returns>
        public Matrix4 SetAll( double m00, double m10, double m20, double m30,
                            double m01, double m11, double m21, double m31,
                            double m02, double m12, double m22, double m32,
                            double m03, double m13, double m23, double m33 )
        {
            this[0,0] = (float) m00;
            this[0,1] = (float) m01;
            this[0,2] = (float) m02;
            this[0,3] = (float) m03;
            this[1,0] = (float) m10;
            this[1,1] = (float) m11;
            this[1,2] = (float) m12;
            this[1,3] = (float) m13;
            this[2,0] = (float) m20;
            this[2,1] = (float) m21;
            this[2,2] = (float) m22;
            this[2,3] = (float) m23;
            this[3,0] = (float) m30; 
            this[3,1] = (float) m31; 
            this[3,2] = (float) m32; 
            this[3,3] = (float) m33;
            return this;
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
				        result[ii,jj] += this[kk,jj] * other[ii,kk];
			        }
		        }
	        }
    
            return result;
        }

        /// <summary>
        /// Multiplies this matrix by a 4x1 vector by inverting the vector to a 1x4 vector.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>A 1x4 vector which is the result of the multiplication. </returns>
        public Vector4 Multiply(Vector4 other)
        {
            Vector4 result = new Vector4();
            result.X += this[0, 0] * other.X;
            result.X += this[1, 0] * other.Y;
            result.X += this[2, 0] * other.Z;
            result.X += this[3, 0] * other.W;
            result.Y += this[0, 1] * other.X;
            result.Y += this[1, 1] * other.Y;
            result.Y += this[2, 1] * other.Z;
            result.Y += this[3, 1] * other.W;
            result.Z += this[0, 2] * other.X;
            result.Z += this[1, 2] * other.Y;
            result.Z += this[2, 2] * other.Z;
            result.Z += this[3, 2] * other.W;
            result.W += this[0, 3] * other.X;
            result.W += this[1, 3] * other.Y;
            result.W += this[2, 3] * other.Z;
            result.W += this[3, 3] * other.W;
            return result;
        }

        /// <summary>
        /// Matrix4 * Matrix4 operator override
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns>A reference to this matrix</returns>
        public static Matrix4 operator * (Matrix4 lhs, Matrix4 rhs)
        {
            return lhs.Multiply(rhs);
        }

        /// <summary>
        /// Matrix4 * Matrix4 operator override
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns>A reference to this matrix</returns>
        public static Vector4 operator *(Vector4 lhs, Matrix4 rhs)
        {
            return rhs.Multiply(lhs);
        }

        /// <summary>
        /// Matrix4 * Matrix4 operator override
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns>A reference to this matrix</returns>
        public static Vector4 operator *(Matrix4 lhs, Vector4 rhs)
        {
            return lhs.Multiply(rhs);
        }

        /// <summary>
        /// Makes the matrix homogenous by dividing every value by the homogenous coordinate (m[3][3])
        /// </summary>
        /// <returns>A reference to this matrix</returns>
        public Matrix4 Homogenize()
        {
	        for(int ii = 0; ii < 4; ii++)
	        {
		        for(int jj = 0; jj < 4; jj++)
		        {
			        this[ii,jj] /= this[3,3];
		        }
	        }
            return this;
        }

        /// <summary>
        /// Sets the instance of this matrix to have the values of the identity matrix.
        /// </summary>
        /// <return>A reference to this matrix</return>
        public Matrix4 Identity()
        {
            SetAll(1, 0, 0, 0,
                   0, 1, 0, 0,
                   0, 0, 1, 0,
                   0, 0, 0, 1);
            return this;
        }

        /// <summary>
        /// Creates a rotation around the world X axis
        /// </summary>
        /// <param name="angle">The angle to rotate around in radians</param>
        /// <returns>A reference to this matrix</returns>
        public Matrix4 RotateX(float angle)
        {
	        this[0,0] = 1;
	        this[0,1] = 0;
	        this[0,2] = 0;
	        this[0,3] = 0;
	        this[1,0] = 0;
	        this[1,1] = (float) Math.Cos(angle);
	        this[1,2] = (float) Math.Sin(angle);
	        this[1,3] = 0;
	        this[2,0] = 0;
	        this[2,1] = (float) - Math.Sin(angle);
	        this[2,2] = (float) Math.Cos(angle);
	        this[2,3] = 0;
	        this[3,0] = 0;
	        this[3,1] = 0;
	        this[3,2] = 0;
	        this[3,3] = 1;
            return this;
        }

        /// <summary>
        /// Creates a rotation around the world Y axis
        /// </summary>
        /// <param name="angle">The angle to rotate around in radians</param>
        /// <returns>A reference to this Matrix</returns>
        public Matrix4 RotateY(float angle)
        {
	        this[0,0] = (float) Math.Cos(angle);
	        this[0,1] = 0;
	        this[0,2] = (float) -Math.Sin(angle);
	        this[0,3] = 0;
	        this[1,0] = 0;
	        this[1,1] = 1;
	        this[1,2] = 0;
	        this[1,3] = 0;
	        this[2,0] = (float) Math.Sin(angle);
	        this[2,1] = 0;
	        this[2,2] = (float) Math.Cos(angle);
	        this[2,3] = 0;
	        this[3,0] = 0;
	        this[3,1] = 0;
	        this[3,2] = 0;
	        this[3,3] = 1;
            return this;
        }

        /// <summary>
        /// Creates a rotation around the world Z axis
        /// </summary>
        /// <param name="angle">The angle to rotate around in radians</param>
        /// <returns>A reference to this matrix</returns>
        public Matrix4 RotateZ(float angle)
        {
	        this[0,0] = (float) Math.Cos(angle);
	        this[0,1] = (float) Math.Sin(angle);
	        this[0,2] = 0;
	        this[0,3] = 0;
	        this[1,0] = (float) -Math.Sin(angle);
	        this[1,1] = (float) Math.Cos(angle);
	        this[1,2] = 0;
	        this[1,3] = 0;
	        this[2,0] = 0;
	        this[2,1] = 0;
	        this[2,2] = 1;
	        this[2,3] = 0;
	        this[3,0] = 0;
	        this[3,1] = 0;
	        this[3,2] = 0;
	        this[3,3] = 1;
            return this;
        }

        /// <summary>
        /// Creates a rotation around the world X axis
        /// </summary>
        /// <param name="angle">The angle to rotate around in degrees</param>
        /// <returns>A reference to this matrix</returns>
        public Matrix4 RotateXDeg(float angle)
        {
	        angle *= MathConstants.DEG2RAD;
            return RotateX(angle);
        }

        /// <summary>
        /// Creates a rotation around the world Y axis
        /// </summary>
        /// <param name="angle">The angle to rotate around in degrees</param>
        /// <returns>A reference to this matrix</returns>
        public Matrix4 RotateYDeg(float angle)
        {
            angle *= MathConstants.DEG2RAD;
            return RotateY(angle);
        }

        /// <summary>
        /// Creates a rotation around the world Z axis
        /// </summary>
        /// <param name="angle">The angle to rotate around in degrees</param>
        /// <returns>A reference to this matrix</returns>
        public Matrix4 RotateZDeg(float angle)
        {
            angle *= MathConstants.DEG2RAD;
            return RotateZ(angle);
        }


        /// <summary>
        /// Creates a rotation around an arbitrary axis
        /// </summary>
        /// <param name="axis">The axis to rate around as a normalized vector</param>
        /// <param name="angle">The angle to rotate around this axis in radians</param>
        /// <returns>A reference to this matrix</returns>
        public Matrix4 Rotate(Vector4 axis, float angle)
        {
            float cTheta = (float)Math.Cos(angle);
            float sTheta = (float)Math.Sin(angle);
            float ax = axis.X;
            float ay = axis.Y;
            float az = axis.Z;

            this[0, 0] = (float)(Math.Pow(ax, 2) + cTheta*(1-Math.Pow(ax, 2)));
            this[0, 1] = ax*ay*(1-cTheta) + az*sTheta;
            this[0, 2] = ax*az*(1-cTheta) - ay*sTheta;
            this[0, 3] = 0;
            this[1, 0] = ax*ay*(1-cTheta) - az * sTheta;
            this[1, 1] = (float)(Math.Pow(ay, 2) + cTheta * (1 - Math.Pow(ay, 2)));
            this[1, 2] = ay*az*(1-cTheta) + ax*sTheta;
            this[1, 3] = 0;
            this[2, 0] = ax*az*(1-cTheta) + ay*sTheta;
            this[2, 1] = ay*az*(1-cTheta)-ax*sTheta;
            this[2, 2] = (float)(Math.Pow(az, 2) + cTheta*(1-Math.Pow(az, 2)));
            this[2, 3] = 0;
            this[3, 0] = 0;
            this[3, 1] = 0;
            this[3, 2] = 0;
            this[3, 3] = 1;

            return this;
        }

        /// <summary>
        /// Creates a rotation around an arbitrary axis
        /// </summary>
        /// <param name="axis">The axis to rate around as a normalized vector</param>
        /// <param name="angle">The angle to rotate around this axis in degrees</param>
        /// <returns>A reference to this matrix</returns>
        public Matrix4 RotateDeg(Vector4 axis, float angle)
        {
            angle = MathConstants.DEG2RAD * MathConstants.INVERSE_PI;
	        return Rotate(axis, angle);
        }

        /// <summary>
        /// Turns this matrix into a scaling matrix, scaling each dimension by
        /// the corresponding paremeter.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns>A reference to this matrix</returns>
        public Matrix4 ScalingMat(float x, float y, float z)
        {
	        this[0,0] = x;
	        this[0,1] = 0;
	        this[0,2] = 0;
	        this[0,3] = 0;
	        this[1,0] = 0;
	        this[1,1] = y;
	        this[1,2] = 0;
	        this[1,3] = 0;
	        this[2,0] = 0;
	        this[2,1] = 0;
	        this[2,2] = z;
	        this[2,3] = 0;
	        this[3,0] = 0;
	        this[3,1] = 0;
	        this[3,2] = 0;
	        this[3,3] = 1;
            
            return this;

        }
        /// <summary>
        /// Turns this matrix into a translation matrix, translating the
        /// model by the arguments in each specified dimension.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Matrix4 TranslationMat(float x, float y, float z)
        {
	        this[0,0] = 1;
	        this[0,1] = 0;
	        this[0,2] = 0;
	        this[0,3] = 0;
	        this[1,0] = 0;
	        this[1,1] = 1;
	        this[1,2] = 0;
	        this[1,3] = 0;
	        this[2,0] = 0;
	        this[2,1] = 0;
	        this[2,2] = 1;
	        this[2,3] = 0;
	        this[3,0] = x;
	        this[3,1] = y;
	        this[3,2] = z;
	        this[3,3] = 1;

            return this;
        }

        /// <summary>
        /// Generates an rotation matrix about the object's X axis.
        /// </summary>
        /// <param name="angle">Angle in radians</param>
        /// <returns>The rotation matrix</returns>
        public static Matrix4 MakeRotateX(float angle) { return (new Matrix4()).RotateX(angle); }
        /// <summary>
        /// Generates an rotation matrix about the object's Y axis.
        /// </summary>
        /// <param name="angle">Angle in radians</param>
        /// <returns>The rotation matrix</returns>
        public static Matrix4 MakeRotateY(float angle) { return (new Matrix4()).RotateY(angle); }
        /// <summary>
        /// Generates an rotation matrix about the object's Z axis.
        /// </summary>
        /// <param name="angle">Angle in radians</param>
        /// <returns>The rotation matrix</returns>
        public static Matrix4 MakeRotateZ(float angle) { return (new Matrix4()).RotateZ(angle); }

        /// <summary>
        /// Generates an rotation matrix about the object's X axis.
        /// </summary>
        /// <param name="angle">Angle in degrees</param>
        /// <returns>The rotation matrix</returns>
        public static Matrix4 MakeRotateXDeg(float angle) { return (new Matrix4()).RotateXDeg(angle); }
        /// <summary>
        /// Generates an rotation matrix about the object's Y axis.
        /// </summary>
        /// <param name="angle">Angle in degrees</param>
        /// <returns>The rotation matrix</returns>
        public static Matrix4 MakeRotateYDeg(float angle) { return (new Matrix4()).RotateYDeg(angle); }
        /// <summary>
        /// Generates an rotation matrix about the object's Z axis.
        /// </summary>
        /// <param name="angle">Angle in degrees</param>
        /// <returns>The rotation matrix</returns>
        public static Matrix4 MakeRotateZDeg(float angle) { return (new Matrix4()).RotateZDeg(angle); }

        /// <summary>
        /// Generates an rotation matrix about an arbitrary object axis.
        /// </summary>
        /// <param name="axis">The axis of rotation</param>
        /// <param name="angle">Angle in radians</param>
        /// <returns>The rotation matrix</returns>
        public static Matrix4 MakeRotate(Vector4 axis, float angle) { return (new Matrix4()).Rotate(axis, angle); }

        /// <summary>
        /// Generates an rotation matrix about an arbitrary object axis.
        /// </summary>
        /// <param name="axis">The axis of rotation</param>
        /// <param name="angle">Angle in degrees</param>
        /// <returns>The rotation matrix</returns>
        public static Matrix4 MakeRotateDeg(Vector4 axis, float angle) { return (new Matrix4()).RotateDeg(axis, angle); }

        /// <summary>
        /// Generates a scaling matrix, scaling each dimension by
        /// the corresponding paremeter.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns>A reference to this matrix</returns>
        public static Matrix4 MakeScalingMat(float x, float y, float z) { return (new Matrix4()).ScalingMat(x, y, z); }

        /// <summary>
        /// Generates a translation matrix, translating the
        /// coordinate by the arguments in each specified dimension.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public static Matrix4 MakeTranslationMat(float x, float y, float z) { return (new Matrix4()).TranslationMat(x, y, z); }



        /// <summary>
        /// Transposes the matrix (that is, reflects across the down-right diagonal)
        /// </summary>
        /// <returns>A reference to this matrix</returns>
        public Matrix4 Transpose()
        {
	        float temp;
	        for(int ii = 3; ii >= 0; ii--)
	        {
		        for(int jj = ii-1; jj >=0; jj--)
		        {
			        temp = this[ii,jj];
			        this[ii,jj] = this[jj,ii];
			        this[jj,ii] = temp;
		        }
	        }
            return this;
        }

        /// <summary>
        /// A fully calculated matrix inverse. Substantially more expensive than the FastInverse. Should
        /// only be used when FastInverse fails to invert the matrix.
        /// </summary>
        /// <returns>A reference to this matrix</returns>
        public Matrix4 Invert()
        {
            float det = this[0,0]*this[1,1]*this[2,2]*this[3,3]+this[0,0]*this[1,2]*this[2,3]*this[3,1]+this[0,0]*this[1,3]*this[2,1]*this[3,2]
            +this[0,1]*this[1,0]*this[2,3]*this[3,2]+this[0,1]*this[1,2]*this[2,0]*this[3,3]+this[0,1]*this[1,3]*this[2,2]*this[3,0]
            +this[0,2]*this[1,0]*this[2,1]*this[3,3]+this[0,2]*this[1,1]*this[2,3]*this[3,0]+this[0,2]*this[1,3]*this[2,0]*this[3,1]
            +this[0,3]*this[1,0]*this[2,2]*this[3,1]+this[0,3]*this[1,1]*this[2,0]*this[3,2]+this[0,3]*this[1,2]*this[2,1]*this[3,0]
            -this[0,0]*this[1,1]*this[2,3]*this[3,2]-this[0,0]*this[1,2]*this[2,1]*this[3,3]-this[0,0]*this[1,3]*this[2,2]*this[3,1]
            -this[0,1]*this[1,0]*this[2,2]*this[3,3]-this[0,1]*this[1,2]*this[2,3]*this[3,0]-this[0,1]*this[1,3]*this[2,0]*this[3,2]
            -this[0,2]*this[1,0]*this[2,3]*this[3,1]-this[0,2]*this[1,1]*this[2,0]*this[3,3]-this[0,2]*this[1,3]*this[2,1]*this[3,0]
            -this[0,3]*this[1,0]*this[2,1]*this[3,2]-this[0,3]*this[1,1]*this[2,2]*this[3,0]-this[0,3]*this[1,2]*this[2,0]*this[3,1];
            float[,] b = new float[4,4];
            b[0,0] = this[1,1]*this[2,2]*this[3,3]+this[1,2]*this[2,3]*this[3,1]+this[1,3]*this[2,1]*this[3,2]-this[1,1]*this[2,3]*this[3,2]-this[1,2]*this[2,1]*this[3,3]-this[1,3]*this[2,2]*this[3,1];
            b[0,1] = this[0,1]*this[2,3]*this[3,2]+this[0,2]*this[2,1]*this[3,3]+this[0,3]*this[2,2]*this[3,1]-this[0,1]*this[2,2]*this[3,3]-this[0,2]*this[2,3]*this[3,1]-this[0,3]*this[2,1]*this[3,2];
            b[0,2] = this[0,1]*this[1,2]*this[3,3]+this[0,2]*this[1,3]*this[3,1]+this[0,3]*this[1,1]*this[3,2]-this[0,1]*this[1,3]*this[3,2]-this[0,2]*this[1,1]*this[3,3]-this[0,3]*this[1,2]*this[3,1];
            b[0,3] = this[0,1]*this[1,3]*this[2,2]+this[0,2]*this[1,1]*this[2,3]+this[0,3]*this[1,2]*this[2,1]-this[0,1]*this[1,2]*this[2,3]-this[0,2]*this[1,3]*this[2,1]-this[0,3]*this[1,1]*this[2,2];
            b[1,0] = this[1,0]*this[2,3]*this[3,2]+this[1,2]*this[2,0]*this[3,3]+this[1,3]*this[2,2]*this[3,0]-this[1,0]*this[2,2]*this[3,3]-this[1,2]*this[2,3]*this[3,0]-this[1,3]*this[2,0]*this[3,2];
            b[1,1] = this[0,0]*this[2,2]*this[3,3]+this[0,2]*this[2,3]*this[3,0]+this[0,3]*this[2,0]*this[3,2]-this[0,0]*this[2,3]*this[3,2]-this[0,2]*this[2,0]*this[3,3]-this[0,3]*this[2,2]*this[3,0];
            b[1,2] = this[0,0]*this[1,3]*this[3,2]+this[0,2]*this[1,0]*this[3,3]+this[0,3]*this[1,2]*this[3,0]-this[0,0]*this[1,2]*this[3,3]-this[0,2]*this[1,3]*this[3,0]-this[0,3]*this[1,0]*this[3,2];
            b[1,3] = this[0,0]*this[1,2]*this[2,3]+this[0,2]*this[1,3]*this[2,0]+this[0,3]*this[1,0]*this[2,2]-this[0,0]*this[1,3]*this[2,2]-this[0,2]*this[1,0]*this[2,3]-this[0,3]*this[1,2]*this[2,0];
            b[2,0] = this[1,0]*this[2,1]*this[3,3]+this[1,1]*this[2,3]*this[3,0]+this[1,3]*this[2,0]*this[3,1]-this[1,0]*this[2,3]*this[3,1]-this[1,1]*this[2,0]*this[3,3]-this[1,3]*this[2,1]*this[3,0];
            b[2,1] = this[0,0]*this[2,3]*this[3,1]+this[0,1]*this[2,0]*this[3,3]+this[0,3]*this[2,1]*this[3,0]-this[0,0]*this[2,1]*this[3,3]-this[0,1]*this[2,3]*this[3,0]-this[0,3]*this[2,0]*this[3,1];
            b[2,2] = this[0,0]*this[1,1]*this[3,3]+this[0,1]*this[1,3]*this[3,0]+this[0,3]*this[1,0]*this[3,1]-this[0,0]*this[1,3]*this[3,1]-this[0,1]*this[1,0]*this[3,3]-this[0,3]*this[1,1]*this[3,0];
            b[2,3] = this[0,0]*this[1,3]*this[2,1]+this[0,1]*this[1,0]*this[2,3]+this[0,3]*this[1,1]*this[2,0]-this[0,0]*this[1,1]*this[2,3]-this[0,1]*this[1,3]*this[2,0]-this[0,3]*this[1,0]*this[2,1];
            b[3,0] = this[1,0]*this[2,2]*this[3,1]+this[1,1]*this[2,0]*this[3,2]+this[1,2]*this[2,1]*this[3,0]-this[1,0]*this[2,1]*this[3,2]-this[1,1]*this[2,2]*this[3,0]-this[1,2]*this[2,0]*this[3,1];
            b[3,1] = this[0,0]*this[2,1]*this[3,2]+this[0,1]*this[2,2]*this[3,0]+this[0,2]*this[2,0]*this[3,1]-this[0,0]*this[2,2]*this[3,1]-this[0,1]*this[2,0]*this[3,2]-this[0,2]*this[2,1]*this[3,0];
            b[3,2] = this[0,0]*this[1,2]*this[3,1]+this[0,1]*this[1,0]*this[3,2]+this[0,2]*this[1,1]*this[3,0]-this[0,0]*this[1,1]*this[3,2]-this[0,1]*this[1,2]*this[3,0]-this[0,2]*this[1,0]*this[3,1];
            b[3,3] = this[0,0]*this[1,1]*this[2,2]+this[0,1]*this[1,2]*this[2,0]+this[0,2]*this[1,0]*this[2,1]-this[0,0]*this[1,2]*this[2,1]-this[0,1]*this[1,0]*this[2,2]-this[0,2]*this[1,1]*this[2,0];
            for(int i=0;i<4;i++)
                for(int j=0;j<4;j++)
                    this[i,j] = b[i,j]/det;
            return this;
        }
        

        /// <summary>
        /// A quick matrix inversion. This will only work if the matrix is actually homogenous.
        /// </summary>
        /// <returns>A reference to this matrix</returns>
        public Matrix4 FastInvert()
        {
            this[0,0] = this[0,0];
	        this[0,1] = this[1,0];
	        this[0,2] = this[2,0];

	        this[1,0] = this[0,1];
	        this[1,1] = this[1,1];
	        this[1,2] = this[2,1];

	        this[2,0] = this[0,2];
            this[2,1] = this[1,2];
            this[2,2] = this[2,2];

	        this[3,0] = -this[3,0];
	        this[3,1] = -this[3,1];
	        this[3,2] = -this[3,2];

            return this;
        }

      
        ///<summary>
        /// Multiplies all values in the matrix by this scalar (including the homogenous coordinate)
        /// </summary>
        /// <param name="scalar"></param>
        /// <returns>A reference to this Matrix4</returns>
        public Matrix4 Scale(float scalar)
        {
	        for(int ii = 0; ii < 4; ii++)
	        {
		        for(int jj = 0; jj < 4; jj++)
		        {
			        this[ii,jj] *= scalar;
		        }
	        }
            return this;
        }
        /// <summary>
        /// Prints the values in the matrix to the console.
        /// </summary>
        public void Print()
        {
	        Console.WriteLine("Matrix4: \n" +
                    this[0,0] + ", " + this[1,0] + ", " + this[2,0] + ", " + this[3,0] + "\n" +
			        this[0,1] + ", " + this[1,1] + ", " + this[2,1] + ", " + this[3,1] + "\n" +
			        this[0,2] + ", " + this[1,2] + ", " + this[2,2] + ", " + this[3,2] + "\n" +
			        this[0,3] + ", " + this[1,3] + ", " + this[2,3] + ", " + this[3,3] + "\n" );
        }

        /// <summary>
        /// Sets the matrix to be the inverse of the Hermite matrix, commonly used for animation easing.
        /// </summary>
        /// <returns>A reference to this matrix</returns>
        public Matrix4 HermiteInverse()
        {
	        this[0,0] = 2;
	        this[0,1] = -3;
	        this[0,2] = 0;
	        this[0,3] = 1;

	        this[1,0] = -2;
	        this[1,1] = 3;
	        this[1,2] = 0;
	        this[1,3] = 0;

	        this[2,0] = 1;
	        this[2,1] = -2;
	        this[2,2] = 1;
	        this[2,3] = 0;

	        this[3,0] = 1;
	        this[3,1] = -1;
	        this[3,2] = 0;
	        this[3,3] = 0;

            return this;
        }

        /// <summary>
        /// Copies the rotation compoment of the matrix (the square defined by 0,0 to 2,2)
        /// </summary>
        /// <param name="mat"></param>
        /// <returns>A reference to this matrix</returns>
        public Matrix4 CopyRot(Matrix4 mat)
        {
            this[0,0] = mat[0, 0];
            this[0,1] = mat[0, 1];
            this[0,2] = mat[0, 2];
            this[1,0] = mat[1, 0];
            this[1,1] = mat[1, 1];
            this[1,2] = mat[1, 2];
            this[2,0] = mat[2, 0];
            this[2,1] = mat[2, 1];
            this[2,2] = mat[2, 2];

            return this;
        }
    }
}
