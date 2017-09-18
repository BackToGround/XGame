using UnityEngine;

public struct SgtVector3D
{
	public double x;
	public double y;
	public double z;

	public SgtVector3D(double newX, double newY, double newZ)
	{
		x = newX; y = newY; z = newZ;
	}

	public SgtVector3D(Vector3 v)
	{
		x = v.x; y = v.y; z = v.z;
	}

	public double sqrMagnitude
	{
		get
		{
			return x * x + y * y + z * z;
		}
	}

	public double magnitude
	{
		get
		{
			return System.Math.Sqrt(sqrMagnitude);
		}
	}

	public SgtVector3D normalized
	{
		get
		{
			var m = sqrMagnitude;

			if (m > 0.0)
			{
				return this / System.Math.Sqrt(m);
			}

			return this;
		}
	}

	public static SgtVector3D Cross(SgtVector3D a, SgtVector3D b)
	{
		return new SgtVector3D(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
	}

	public static SgtVector3D operator - (SgtVector3D a, SgtVector3D b)
	{
		return new SgtVector3D(a.x - b.x, a.y - b.y, a.z - b.z);
	}

	public static SgtVector3D operator + (SgtVector3D a, SgtVector3D b)
	{
		return new SgtVector3D(a.x + b.x, a.y + b.y, a.z + b.z);
	}

	public static SgtVector3D operator / (SgtVector3D a, long b)
	{
		return new SgtVector3D(a.x / b, a.y / b, a.z / b);
	}

	public static SgtVector3D operator / (SgtVector3D a, double b)
	{
		return new SgtVector3D(a.x / b, a.y / b, a.z / b);
	}

	public static SgtVector3D operator * (SgtVector3D a, long b)
	{
		return new SgtVector3D(a.x * b, a.y * b, a.z * b);
	}

	public static SgtVector3D operator * (SgtVector3D a, double b)
	{
		return new SgtVector3D(a.x * b, a.y * b, a.z * b);
	}

	public static SgtVector3D operator * (long a, SgtVector3D b)
	{
		return new SgtVector3D(b.x * a, b.y * a, b.z * a);
	}

	public static explicit operator Vector3 (SgtVector3D a)
	{
		return new Vector3((float)a.x, (float)a.y, (float)a.z);
	}
}