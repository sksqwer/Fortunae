/*
The MIT License

Copyright (c) 2015 Martin Pichlmair

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

/* Unity has their own implementation by now but I'll keep this in order to
   maintain compatibility with older projects.
 */

using System;
using UnityEngine;
using Serializable = System.SerializableAttribute;
namespace GB
{
    [System.Serializable]
    public struct SerializableVector2
    {
        [SerializeField]
        public int x;
        [SerializeField]
        public int y;

        public SerializableVector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public SerializableVector2(SerializableVector2 vec)
        {
            this.x = vec.x;
            this.y = vec.y;
        }
        public SerializableVector2(float x, float y)
        {
            this.x = (int)x;
            this.y = (int)y;
        }

        public static SerializableVector2 one = new SerializableVector2(1, 1);
        public static SerializableVector2 right = new SerializableVector2(1, 0);
        public static SerializableVector2 up = new SerializableVector2(0, 1);
        public static SerializableVector2 zero = new SerializableVector2(0, 0);

        public int magnitude
        {
            get { return (int)Mathf.Sqrt((float)(x * x + y * y)); }
        }
        public int sqrMagnitude
        {
            get { return x * x + y * y; }
        }
        public int this[int i]
        {
            get { return i == 0 ? x : y; }
            set
            {
                if (i == 0) { x = value; return; }
                if (i == 1) { y = value; return; }
                throw new System.ArgumentOutOfRangeException("i", "Index out of range (0,1)");
            }
        }
        public SerializableVector2 normalized
        {
            get { return new SerializableVector2(this / magnitude); }
        }

        public void Normalize()
        {
            SerializableVector2 n = this.normalized;
            x = n.x; y = n.y;
        }
        public void Set(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public override string ToString()
        {
            return x + "/" + y;
        }

        public static float Angle(SerializableVector2 a, SerializableVector2 b)
        {
            return Vector2.Angle(a, b);
        }
        public static SerializableVector2 ClampMagnitude(SerializableVector2 a, float magnitude)
        {
            int dist = a.magnitude;
            return new SerializableVector2(a * (int)magnitude / dist);
        }
        public static float Distance(SerializableVector2 a, SerializableVector2 b)
        {
            return (a - b).magnitude;
        }
        public static float Dot(SerializableVector2 a, SerializableVector2 b)
        {
            return a.x * b.x + a.y * b.y;
        }
        public static SerializableVector2 Lerp(SerializableVector2 a, SerializableVector2 b, float t)
        {
            t = Mathf.Clamp01(t);
            return new SerializableVector2(a.x + (int)(t * (float)b.x), a.y + (int)(t * (float)b.y));
        }
        public static SerializableVector2 Max(SerializableVector2 a, SerializableVector2 b)
        {
            return new SerializableVector2(Math.Max(a.x, b.x), Math.Max(a.y, b.y));
        }
        public static SerializableVector2 Min(SerializableVector2 a, SerializableVector2 b)
        {
            return new SerializableVector2(Math.Min(a.x, b.x), Math.Min(a.y, b.y));
        }
        public static SerializableVector2 MoveTowards(SerializableVector2 a, SerializableVector2 b, float maxDistanceDelta)
        {
            return a + SerializableVector2.ClampMagnitude(b - a, maxDistanceDelta);
        }
        public static SerializableVector2 Scale(SerializableVector2 a, SerializableVector2 b)
        {
            return new SerializableVector2(a.x * b.x, a.y * b.y);
        }

        public static SerializableVector2 operator -(SerializableVector2 a)
        {
            return new SerializableVector2(-a.x, -a.y);
        }
        public static SerializableVector2 operator -(SerializableVector2 a, SerializableVector2 b)
        {
            return new SerializableVector2(a.x - b.x, a.y - b.y);
        }
        public static bool operator !=(SerializableVector2 a, SerializableVector2 b)
        {
            return (a.x != b.x || a.y != b.y);
        }
        public static SerializableVector2 operator *(SerializableVector2 a, int n)
        {
            return new SerializableVector2(a.x * n, a.y * n);
        }
        public static SerializableVector2 operator /(SerializableVector2 a, int n)
        {
            return new SerializableVector2(a.x / n, a.y / n);
        }
        public static SerializableVector2 operator +(SerializableVector2 a, SerializableVector2 b)
        {
            return new SerializableVector2(a.x + b.x, a.y + b.y);
        }
        public static bool operator ==(SerializableVector2 a, SerializableVector2 b)
        {
            return (a.x == b.x && a.y == b.y);
        }
        public static implicit operator Vector2(SerializableVector2 x)
        {
            return new Vector2((float)x.x, (float)x.y);
        }
        public static implicit operator Vector3(SerializableVector2 x)
        {
            return new Vector3((float)x.x, (float)x.y, 0f);
        }
        public static implicit operator string(SerializableVector2 x)
        {
            return x.ToString();
        }

        // code from http://msdn.microsoft.com/en-us/library/system.object.gethashcode.aspx
        public override bool Equals(System.Object obj)
        {
            if (!(obj is SerializableVector2)) return false;
            SerializableVector2 p = (SerializableVector2)obj;
            return x == p.x & y == p.y;
        }
        public override int GetHashCode()
        {
            return ShiftAndWrap(x.GetHashCode(), 2) ^ y.GetHashCode();
        }
        private int ShiftAndWrap(int value, int positions)
        {
            positions = positions & 0x1F;
            uint number = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
            uint wrapped = number >> (32 - positions);
            return BitConverter.ToInt32(BitConverter.GetBytes((number << positions) | wrapped), 0);
        }

    }

    public static class Vector2Extensions
    {
        public static SerializableVector2 SerializableVector2(this Vector2 vector2)
        {
            return new SerializableVector2(Mathf.RoundToInt(vector2.x), Mathf.RoundToInt(vector2.y));
        }

        public static Vector2 IncrementToward(this Vector2 n, Vector2 target, float a)
        {
            float dist = (target - n).magnitude;
            if (dist < Mathf.Epsilon) return target;
            return n + Vector2.ClampMagnitude(target - n, Mathf.Min(dist, a));
        }

        public static Vector2 FlipY(this Vector2 n)
        {
            float x = n.x;
            n.x = -n.y;
            n.y = x;
            return n;
        }

        public static Vector3 Vector3XY(this Vector2 vector2, float z = 0f)
        {
            return new Vector3(vector2.x, vector2.y, z);
        }
        public static Vector3 Vector3XZ(this Vector2 vector2)
        {
            return new Vector3(vector2.x, 0f, vector2.y);
        }
    }
    public static class Vector3Extensions
    {
        public static SerializableVector2 SerializableVector2(this Vector2 vector2)
        {
            return new SerializableVector2(Mathf.RoundToInt(vector2.x), Mathf.RoundToInt(vector2.y));
        }
        public static Vector2 Vector2(this SerializableVector2 SerializableVector2)
        {
            return new Vector2(SerializableVector2.x, SerializableVector2.y);
        }
        public static Vector2 Vector2XY(this Vector3 vector3)
        {
            return new Vector2(vector3.x, vector3.y);
        }
        public static Vector2 Vector2XZ(this Vector3 vector3)
        {
            return new Vector2(vector3.x, vector3.z);
        }
        public static Vector2 Vector2YZ(this Vector3 vector3)
        {
            return new Vector2(vector3.y, vector3.z);
        }
        public static SerializableVector2 SerializableVector2XY(this Vector3 vector3)
        {
            return new SerializableVector2(Mathf.RoundToInt(vector3.x), Mathf.RoundToInt(vector3.y));
        }
        public static SerializableVector2 SerializableVector2XZ(this Vector3 vector3)
        {
            return new SerializableVector2(Mathf.RoundToInt(vector3.x), Mathf.RoundToInt(vector3.z));
        }
        public static SerializableVector2 SerializableVector2YZ(this Vector3 vector3)
        {
            return new SerializableVector2(Mathf.RoundToInt(vector3.y), Mathf.RoundToInt(vector3.z));
        }

        public static Vector2 IncrementToward(this Vector3 n, Vector3 target, float a)
        {
            float dist = (target - n).magnitude;
            if (dist < Mathf.Epsilon) return target;
            return n + Vector3.ClampMagnitude(target - n, Mathf.Min(dist, a));
        }

    }

}
/* 

one		Shorthand for writing Vector2(1, 1).
right	Shorthand for writing Vector2(1, 0).
up		Shorthand for writing Vector2(0, 1).
zero	Shorthand for writing Vector2(0, 0).

Variables

magnitude		Returns the length of this vector (Read Only).
normalized		Returns this vector with a magnitude of 1 (Read Only).
sqrMagnitude	Returns the squared length of this vector (Read Only).
this[int]		Access the x or y component using [0] or [1] respectively.
x				X component of the vector.
y				Y component of the vector.

Constructors

Vector2	Constructs a new vector with given x, y components.

Functions

Normalize		Makes this vector have a magnitude of 1.
Set				Set x and y components of an existing Vector2.
ToString		Returns a nicely formatted string for this vector.

Static Functions

Angle			Returns the angle in degrees between from and to.
ClampMagnitude	Returns a copy of vector with its magnitude clamped to maxLength.
Distance		Returns the distance between a and b.
Dot				Dot Product of two vectors.
Lerp			Linearly interpolates between two vectors.
Max				Returns a vector that is made from the largest components of two vectors.
Min				Returns a vector that is made from the smallest components of two vectors.
MoveTowards		Moves a point current towards target.
Scale			Multiplies two vectors component-wise.

Operators

operator -	Subtracts one vector from another.
operator !=	Returns true if vectors different.
operator *	Multiplies a vector by a number.
operator /	Divides a vector by a number.
operator +	Adds two vectors.
operator ==	Returns true if the vectors are equal.
Vector2	Converts a Vector3 to a Vector2.
Vector3	Converts a Vector2 to a Vector3.
 */
