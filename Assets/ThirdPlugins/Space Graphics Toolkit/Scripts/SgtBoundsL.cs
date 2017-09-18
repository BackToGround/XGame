[System.Serializable]
public struct SgtBoundsL
{
	public long minX;
	public long minY;
	public long minZ;
	public long maxX;
	public long maxY;
	public long maxZ;

	public SgtBoundsL Double
	{
		get
		{
			return new SgtBoundsL(minX * 2, minY * 2, minZ * 2, maxX * 2, maxY * 2, maxZ * 2);
		}
	}

	public long SizeX
	{
		get
		{
			return maxX - minX;
		}
	}

	public long SizeY
	{
		get
		{
			return maxY - minY;
		}
	}

	public long SizeZ
	{
		get
		{
			return maxZ - minZ;
		}
	}

	public SgtRectL RectZY
	{
		get
		{
			return new SgtRectL(minZ, minY, maxZ, maxY);
		}
	}

	public SgtRectL RectXZ
	{
		get
		{
			return new SgtRectL(minX, minZ, maxX, maxZ);
		}
	}

	public SgtRectL RectXY
	{
		get
		{
			return new SgtRectL(minX, minY, maxX, maxY);
		}
	}

	public SgtBoundsL(long x, long y, long z, long size)
	{
		minX = x - size;
		minY = y - size;
		minZ = z - size;
		maxX = x + size;
		maxY = y + size;
		maxZ = z + size;
	}

	public SgtBoundsL(long newMinX, long newMinY, long newMinZ, long newMaxX, long newMaxY, long newMaxZ)
	{
		minX = newMinX;
		minY = newMinY;
		minZ = newMinZ;
		maxX = newMaxX;
		maxY = newMaxY;
		maxZ = newMaxZ;
	}

	public void ClampTo(SgtBoundsL other)
	{
		if (minX < other.minX) minX = other.minX; else if (minX > other.maxX) minX = other.maxX;
		if (minY < other.minY) minY = other.minY; else if (minY > other.maxY) minY = other.maxY;
		if (minZ < other.minZ) minZ = other.minZ; else if (minZ > other.maxZ) minZ = other.maxZ;
		if (maxX < other.minX) maxX = other.minX; else if (maxX > other.maxX) maxX = other.maxX;
		if (maxY < other.minY) maxY = other.minY; else if (maxY > other.maxY) maxY = other.maxY;
		if (maxZ < other.minZ) maxZ = other.minZ; else if (maxZ > other.maxZ) maxZ = other.maxZ;
	}

	public bool Contains(SgtVector3L xyz)
	{
		return xyz.x >= minX && xyz.x < maxX && xyz.y >= minY && xyz.y < maxY && xyz.z >= minZ && xyz.z < maxZ;
	}

	public bool Contains(long x, long y, long z)
	{
		return x >= minX && x < maxX && y >= minY && y < maxY && z >= minZ && z < maxZ;
	}

	public bool IsInsideX(long x)
	{
		return x >= minX && x < maxX;
	}

	public bool IsInsideY(long y)
	{
		return y >= minY && y < maxY;
	}

	public bool IsInsideZ(long z)
	{
		return z >= minZ && z < maxZ;
	}

	public void Clear()
	{
		minX = maxX = minY = maxY = minZ = maxZ = 0;
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public static bool operator == (SgtBoundsL a, SgtBoundsL b)
	{
		return a.minX == b.minX && a.minY == b.minY && a.minZ == b.minZ && a.maxX == b.maxX && a.maxY == b.maxY && a.maxZ == b.maxZ;
	}

	public static bool operator != (SgtBoundsL a, SgtBoundsL b)
	{
		return a.minX != b.minX || a.minY != b.minY || a.minZ != b.minZ || a.maxX != b.maxX || a.maxY != b.maxY || a.maxZ != b.maxZ;
	}

	public override string ToString()
	{
		return "(" + minX + ", " + minY + ", " + minZ + " : " + maxX + ", " + maxY + ", " + maxZ + ")";
	}
}