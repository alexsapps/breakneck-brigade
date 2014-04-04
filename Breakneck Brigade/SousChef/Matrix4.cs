using System;

namespace SousChef
{
    public class Matrix4
    {
        public float[,] backingArray    { get; private set; } //Backing data structure for the matrix
        public float[]  glArray         { get; private set; } //Generated array for use in OpenGL. Updated on backing array mutation
        /*
         * Initializes to identity matrix
         */
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

        /*
         * 
         */
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

        /*
         * 
         */
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

        public void Identity()
        {
            SetAll(1, 0, 0, 0,
                   0, 1, 0, 0,
                   0, 0, 1, 0,
                   0, 0, 0, 1);

        }

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

        public Matrix4 Rotate(Vector4 axis, float angle)
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

        /*
         * Full matrix invert for a 4x4 matrix
         */
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
        

        /*
         * A quick matrix inversion for homogenous matricies ONLY.
         * 
         * 1) transpose the 3x3 matrix from [0,0] to [2,2]
         * 2) negate the 4th column from index 0 to index 2
         */
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

        public void Print()
        {
	        Console.WriteLine("Matrix4: \n" +
                    backingArray[0,0] + ", " + backingArray[1,0] + ", " + backingArray[2,0] + ", " + backingArray[3,0] + "\n" +
			        backingArray[0,1] + ", " + backingArray[1,1] + ", " + backingArray[2,1] + ", " + backingArray[3,1] + "\n" +
			        backingArray[0,2] + ", " + backingArray[1,2] + ", " + backingArray[2,2] + ", " + backingArray[3,2] + "\n" +
			        backingArray[0,3] + ", " + backingArray[1,3] + ", " + backingArray[2,3] + ", " + backingArray[3,3] + "\n" );
        }

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
