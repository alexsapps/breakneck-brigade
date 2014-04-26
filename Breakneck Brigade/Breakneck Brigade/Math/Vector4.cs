using System;
namespace Breakneck_Brigade
{
    /// <summary>
    /// An implementation of a 4D vector for use in homogenous matrix operations
    /// 
    /// Important Note: The vast majority of the operations do not do true 4D 
    /// vector math, and omit the 4th coordinate. For example, dotting two Vector4s 
    /// will not add the final 1*1 coordinate.
    /// </summary>
    /// <author>
    /// Ryan George
    /// </author>
    public class Vector4
    {
        public float X { get { return this[0]; } set { this[0] = value; } }
        public float Y { get { return this[1]; } set { this[1] = value; } }
        public float Z { get { return this[2]; } set { this[2] = value; } }
        public float W { get { return this[3]; } set { this[3] = value; } }

        const int size = 4;

        float[] v = new float[size];              //Backing array for the object

       /// <summary>
       /// Initializes all dimensions of the vector to 0.
       /// </summary>
        public Vector4()
        {
            
        } 

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="other"></param>
        public Vector4(Vector4 other)
        {
            Array.Copy(other.v, this.v, size);
        }

        /// <summary>
        /// Sets the dimension of the vector to the specified values
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="w"></param>
        public Vector4(float x, float y, float z, float w)
        {
	        v[0] = x;
	        v[1] = y;
	        v[2] = z;
	        v[3] = w;
        }

        /// <summary>
        /// Sets the dimension of the vector to the specified values
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="w"></param>
        public Vector4(double x, double y, double z, double w)
        {
	        v[0] = (float) x;
	        v[1] = (float) y;
	        v[2] = (float) z;
	        v[3] = (float) w;
        }

        /// <summary>
        /// Constructor that initializes only x, y, and z dimensions.
        /// Initializes homogenous coordinate to 1, indicating a 3D vector
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Vector4(float x, float y, float z)
        {
            v[0] = x;
            v[1] = y;
            v[2] = z;
            v[3] = 1;
        }

        /// <summary>
        /// Constructor that initializes only x, y, and z dimensions.
        /// Initializes homogenous coordinate to 1, indicating a 3D vector
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Vector4(double x, double y, double z)
        {
            v[0] = (float) x;
            v[1] = (float) y;
            v[2] = (float) z;
            v[3] = 1;
        }

        /// <summary>
        /// Gets the specified dimension of the vector.
        /// x = 0
        /// y = 1
        /// z = 2
        /// w = 3
        /// </summary>
        /// <param name="ind"></param>
        /// <returns></returns>
        public float this[int ind]
        {
            get { return v[ind]; }
            set { v[ind] = value; }
        }
        
        /// <summary>
        /// Scalar multiplication.
        /// </summary>
        /// <param name="scalar"></param>
        /// <param name="rhs"></param>
        /// <returns>The scaled vector</returns>
        public static Vector4 operator * (float scalar, Vector4 rhs)
        {
	        Vector4 retVect = new Vector4(rhs);
	        retVect[0] *= scalar;
	        retVect[1] *= scalar;
	        retVect[2] *= scalar;
	        return retVect;
        }

        /// <summary>
        /// Scalar multiplication.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="scalar"></param>
        /// <returns>The scaled vector</returns>
        public static Vector4 operator * (Vector4 lhs, float scalar)
        {
            return scalar * lhs;
        }

        /// <summary>
        /// Vector multiplication.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns>A scalar that represents the result of multiplying the two vectors</returns>
        public static float operator * (Vector4 lhs, Vector4 rhs)
        {
	        return  lhs[0]*rhs[0] +
                    lhs[1]*rhs[1] +
                    lhs[2]*rhs[2] +
                    lhs[3]*rhs[3];
        }

        /// <summary>
        /// Vector addition
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns>The Vector4 that is the result of the two vectors added together. This does NOT add the homogenous coordinate.</returns>
        public static Vector4 operator + (Vector4 lhs, Vector4 rhs)
        {
	        return new Vector4( lhs[0]+rhs[0],
					            lhs[1]+rhs[1],
                                lhs[2]+rhs[2]);	
        }

        /// <summary>
        /// Vector subtraction
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns>The Vector4 that is the result of lhs-rhs. This does NOT add the homogenous coordinate.</returns>
        public static Vector4 operator - (Vector4 lhs, Vector4 rhs)
        {
	        return new Vector4( lhs[0]-rhs[0],
					            lhs[1]-rhs[1],
					            lhs[2]-rhs[2]);
        }

        /// <summary>
        /// Sets all dimensions of the vector.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="w"></param>
        public void Set(float x, float y, float z, float w)
        {
	        v[0] = x;
	        v[1] = y;
	        v[2] = z;
	        v[3] = w;
        }
        
        /// <summary>
        /// Sets all dimensions of the vector.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="w"></param>
        public void Set(double x, double y, double z, double w)
        {
	        v[0] = (float) x;
	        v[1] = (float) y;
	        v[2] = (float) z;
	        v[3] = (float) w;
        }

        /// <summary>
        /// Dots this vector and another vector, disregarding the homogenous coordinate.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>The dot product of this vector, disregarding the homogenous coordinate.</returns>
        public float DotProduct(Vector4 other)
        {
	        return	v[0]*other[0]+
			        v[1]*other[1]+
			        v[2]*other[2];
        }

        /// <summary>
        /// Crosses this vector and another vector, disregarding the homogenous coordinate.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>The cross product of the vector, disregarding the homogenous coordinate.</returns>
        public Vector4 CrossProduct(Vector4 other)
        {
            float x = (v[1]*other[2]) - (v[2]*other[1]);
            float y = (v[2]*other[0]) - (v[0]*other[2]);
            float z = (v[0]*other[1]) - (v[1]*other[0]);
            return new Vector4(x,y,z,0);
        }

        /// <summary>
        /// Gets the magnitude of the 3D vector, disregarding the homogenous coordinate.
        /// </summary>
        /// <returns>The magnitude of the 3D vector, disregarding the homogenous coordinate.</returns>
        public float Magnitude()
        {
            return (float)Math.Sqrt(v[0]*v[0]+
				                    v[1]*v[1]+
				                    v[2]*v[2]);
        }

        /// <summary>
        /// Negates this vector, disregarding the homogeonous coordinate
        /// </summary>
        public void Negate()
        {
	        v[0]=-v[0];
	        v[1]=-v[1];
	        v[2]=-v[2];
        }

        /// <summary>
        /// Dehomogenizes the vector (divides every dimension of the vector by the homogenous coordinate)
        /// </summary>
        public void Dehomogenize()
        {
	        v[0] /= v[3];
	        v[1] /= v[3];
	        v[2] /= v[3];
	        v[3] /= v[3];
        }
        /// <summary>
        /// Normalizes the vector, skipping the homogenous coordinate.
        /// </summary>
        public void Normalize()
        {
	        float mag = this.Magnitude();
	        v[0]=v[0]/mag;
	        v[1]=v[1]/mag;
	        v[2]=v[2]/mag;
        }

        /// <summary>
        /// Scales the vector, skipping the homogenous coordinate.
        /// </summary>
        /// <param name="scalar"></param>
        public void Scale(float scalar)
        {
	        v[0] *= scalar;
	        v[1] *= scalar;
	        v[2] *= scalar;
        }

        /// <summary>
        /// Prints the vector to the console.
        /// </summary>
        public void Print()
        {
	        Console.WriteLine(ToString());
        }

        /// <summary>
        /// Get's the string representation of this Vector4
        /// </summary>
        public override string ToString()
        {
            return "Vector4: " +
                v[0] + "," +
                v[1] + "," +
                v[2] + "," +
                v[3];
        }
    }
}