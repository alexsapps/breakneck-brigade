/*
 * SousChef Vector4 Class
 * An implementation of a 4D vector for use in homogenous matrix operations
 * 
 * Important Note: The vast majority of the operations do not do true 4D 
 * vector math, and omit the 4th coordinate. For example, dotting two Vector4s 
 * will not add the final 1*1 coordinate.
 * 
 * Adapted from code by Jurgen Schulze and Steve Rotenburg
 * Written by Ryan George
 */
using System;
namespace SousChef
{
    public class Vector4
    {
        float[] v;              //Backing array for the object

        /*
         * No arg constructor that initializes all dimensions to 0
         */
        public Vector4()
        {
            v = new float[4];
	        for (int i=0; i<4; ++i)
	        {
		        v[i] = 0;
	        }
        } 

        /*
         * Copy constructor
         */
        public Vector4(Vector4 other)
        {
            v = new float[4];
            v[0] = other[0];
            v[1] = other[1];
            v[2] = other[2];
            v[3] = other[3];
        }

        /*
         * Constructor that initializes all dimensions to their specified values
         */
        public Vector4(float x, float y, float z, float w)
        {
	        v[0] = x;
	        v[1] = y;
	        v[2] = z;
	        v[3] = w;
        }

        /*
         * Constructor that initializes all dimensions to their specified values
         */
        public Vector4(double x, double y, double z, double w)
        {
	        v[0] = (float) x;
	        v[1] = (float) y;
	        v[2] = (float) z;
	        v[3] = (float) w;
        }

        /*
         * Constructor that initializes only x, y, and z dimensions.
         * Initializes homogenous coordinate to 1, indicating a 3D vector
         */
        Vector4(float x, float y, float z)
        {
            v[0] = x;
            v[1] = y;
            v[2] = z;
            v[3] = 1;
        }

        /*
         * Constructor that initializes only x, y, and z dimensions.
         * Initializes homogenous coordinate to 1, indicating a 3D vector
         */
        Vector4(double x, double y, double z)
        {
            v[0] = (float) x;
            v[1] = (float) y;
            v[2] = (float) z;
            v[3] = 1;
        }

        /*
         * Support for accessing individual coordinates of the vector
         */
        public float this[int ind]
        {
            get {return v[ind];}
            set {v[ind] = value;}
        }
        
        public static Vector4 operator * (float scalar, Vector4 rhs)
        {
	        Vector4 retVect = new Vector4(rhs);
	        retVect[0] *= scalar;
	        retVect[1] *= scalar;
	        retVect[2] *= scalar;
	        return retVect;
        }

        public static Vector4 operator * (Vector4 lhs, float scalar)
        {
            return scalar*lhs;
        }

        public static float operator * (Vector4 lhs, Vector4 rhs)
        {
	        return  lhs[0]*rhs[0] +
                    lhs[1]*rhs[1] +
                    lhs[2]*rhs[2] +
                    lhs[3]*rhs[3];
        }

        public static Vector4 operator + (Vector4 lhs, Vector4 rhs)
        {
	        return new Vector4( lhs[0]+rhs[0],
					            lhs[1]+rhs[1],
                                lhs[2]+rhs[2]);	
        }

        public static Vector4 operator - (Vector4 lhs, Vector4 rhs)
        {
	        return new Vector4( lhs[0]-rhs[0],
					            lhs[1]-rhs[1],
					            lhs[2]-rhs[2]);
        }

        public void Set(float x, float y, float z, float w)
        {
	        v[0] = x;
	        v[1] = y;
	        v[2] = z;
	        v[3] = w;
        }
        
        public void Set(double x, double y, double z, double w)
        {
	        v[0] = (float) x;
	        v[1] = (float) y;
	        v[2] = (float) z;
	        v[3] = (float) w;
        }

        public float DotProduct(Vector4 other)
        {
	        return	v[0]*other[0]+
			        v[1]*other[1]+
			        v[2]*other[2];
        }

        public Vector4 CrossProduct(Vector4 other)
        {
            float x = (v[1]*other[2]) - (v[2]*other[1]);
            float y = (v[2]*other[0]) - (v[0]*other[2]);
            float z = (v[0]*other[1]) - (v[1]*other[0]);
            return new Vector4(x,y,z,0);
        }

        public void Negate()
        {
	        v[0]=-v[0];
	        v[1]=-v[1];
	        v[2]=-v[2];
	        v[3]=-v[3];
        }

        public void Dehomogenize()
        {
	        v[0] /= v[3];
	        v[1] /= v[3];
	        v[2] /= v[3];
	        v[3] /= v[3];
        }


        public float Magnitude()
        {
	        return (float) Math.Sqrt(   v[0]*v[0]+
				                        v[1]*v[1]+
				                        v[2]*v[2]);
        }


        public void Normalize()
        {
	        float mag = this.Magnitude();
	        v[0]=v[0]/mag;
	        v[1]=v[1]/mag;
	        v[2]=v[2]/mag;
            v[3]=v[3]/mag;
        }

        public void Scale(float scalar)
        {
	        v[0] *= scalar;
	        v[1] *= scalar;
	        v[2] *= scalar;
	        v[3] *= scalar;
        }

        public void Print()
        {
	        Console.Write(  "Vector4: " + "\n" + 
                            v[0]        + "\n" +
                            v[1]        + "\n" +
                            v[2]        + "\n" +
                            v[3]        + "\n" );
        }
    }
}