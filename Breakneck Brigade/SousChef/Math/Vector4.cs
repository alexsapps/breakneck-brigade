using System;
namespace SousChef
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
        public float X;
        public float Y;
        public float Z;
        public float W;

        const int size = 4;

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
            this.X = other.X;
            this.Y = other.Y;
            this.Z = other.Z;
            this.W = other.W;
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
	        this.X = x;
	        this.Y = y;
	        this.Z = z;
	        this.W = w;
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
	        this.X = (float) x;
	        this.Y = (float) y;
	        this.Z = (float) z;
	        this.W = (float) w;
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
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = 1;
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
            this.X = (float) x;
            this.Y = (float) y;
            this.Z = (float) z;
            this.W = 1;
        }

        public Vector4(SousChef.Coordinate coordinate) : this(coordinate.x, coordinate.y, coordinate.z) { }
        
        /// <summary>
        /// Scalar multiplication.
        /// </summary>
        /// <param name="scalar"></param>
        /// <param name="rhs"></param>
        /// <returns>The scaled vector</returns>
        public static Vector4 operator * (float scalar, Vector4 rhs)
        {
	        Vector4 retVect = new Vector4(rhs);
	        retVect.X *= scalar;
	        retVect.Y *= scalar;
	        retVect.Z *= scalar;
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
	        return  lhs.X*rhs.X +
                    lhs.Y*rhs.Y +
                    lhs.Z*rhs.Z +
                    lhs.W*rhs.W;
        }

        /// <summary>
        /// Vector addition
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns>The Vector4 that is the result of the two vectors added together. This does NOT add the homogenous coordinate.</returns>
        public static Vector4 operator + (Vector4 lhs, Vector4 rhs)
        {
	        return new Vector4( lhs.X+rhs.X,
					            lhs.Y+rhs.Y,
                                lhs.Z+rhs.Z);	
        }

        /// <summary>
        /// Vector subtraction
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns>The Vector4 that is the result of lhs-rhs. This does NOT add the homogenous coordinate.</returns>
        public static Vector4 operator - (Vector4 lhs, Vector4 rhs)
        {
	        return new Vector4( lhs.X-rhs.X,
					            lhs.Y-rhs.Y,
					            lhs.Z-rhs.Z);
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
	        this.X = x;
	        this.Y = y;
	        this.Z = z;
	        this.W = w;
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
	        this.X = (float) x;
	        this.Y = (float) y;
	        this.Z = (float) z;
	        this.W = (float) w;
        }

        /// <summary>
        /// Sets all dimensions of the vector, preserving the value of the homogenous coordinate
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Set(double x, double y, double z)
        {
            this.Set(x, y, z, this.W);
        }

        /// <summary>
        /// Sets all dimensions of the vector, preserving the value of the homogenous coordinate
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void Set(float x, float y, float z)
        {
            this.Set(x, y, z, this.W);
        }

        /// <summary>
        /// Dots this vector and another vector, disregarding the homogenous coordinate.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>The dot product of this vector, disregarding the homogenous coordinate.</returns>
        public float DotProduct(Vector4 other)
        {
	        return	this.X*other.X+
			        this.Y*other.Y+
			        this.Z*other.Z;
        }

        /// <summary>
        /// Crosses this vector and another vector, disregarding the homogenous coordinate.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>The cross product of the vector, disregarding the homogenous coordinate.</returns>
        public Vector4 CrossProduct(Vector4 other)
        {
            float x = (this.Y*other.Z) - (this.Z*other.Y);
            float y = (this.Z*other.X) - (this.X*other.Z);
            float z = (this.X*other.Y) - (this.Y*other.X);
            return new Vector4(x,y,z,0);
        }

        /// <summary>
        /// Gets the magnitude of the 3D vector, disregarding the homogenous coordinate.
        /// </summary>
        /// <returns>The magnitude of the 3D vector, disregarding the homogenous coordinate.</returns>
        public float Magnitude()
        {
            return (float)Math.Sqrt(this.X*this.X+
				                    this.Y*this.Y+
				                    this.Z*this.Z);
        }

        /// <summary>
        /// Negates this vector, disregarding the homogeonous coordinate
        /// </summary>
        public void Negate()
        {
	        this.X=-this.X;
	        this.Y=-this.Y;
	        this.Z=-this.Z;
        }

        /// <summary>
        /// Dehomogenizes the vector (divides every dimension of the vector by the homogenous coordinate)
        /// </summary>
        public void Dehomogenize()
        {
	        this.X /= this.W;
	        this.Y /= this.W;
	        this.Z /= this.W;
	        this.W /= this.W;
        }
        /// <summary>
        /// Normalizes the vector, skipping the homogenous coordinate.
        /// </summary>
        public void Normalize()
        {
	        float mag = this.Magnitude();
	        this.X=this.X/mag;
	        this.Y=this.Y/mag;
	        this.Z=this.Z/mag;
        }

        /// <summary>
        /// Scales the vector, skipping the homogenous coordinate.
        /// </summary>
        /// <param name="scalar"></param>
        public void Scale(float scalar)
        {
	        this.X *= scalar;
	        this.Y *= scalar;
	        this.Z *= scalar;
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
                this.X + "," +
                this.Y + "," +
                this.Z + "," +
                this.W;
        }
    }
}