public struct SgtRectL
{
	public long minX;
	public long minY;
	public long maxX;
	public long maxY;

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

	public void ClampTo(SgtRectL other)
	{
		if (minX < other.minX) minX = other.minX; else if (minX > other.maxX) minX = other.maxX;
		if (minY < other.minY) minY = other.minY; else if (minY > other.maxY) minY = other.maxY;
		if (maxX < other.minX) maxX = other.minX; else if (maxX > other.maxX) maxX = other.maxX;
		if (maxY < other.minY) maxY = other.minY; else if (maxY > other.maxY) maxY = other.maxY;
	}

	public SgtRectL GetExpanded(long amount)
	{
		return new SgtRectL(minX - amount, minY - amount, maxX + amount, maxY + amount);
	}

	public SgtRectL(long newMinX, long newMinY, long newMaxX, long newMaxY)
	{
		minX = newMinX; minY = newMinY; maxX = newMaxX; maxY = newMaxY;
	}

	public bool Contains(long x, long y)
	{
		return x >= minX && x < maxX && y >= minY && y < maxY;
	}

	public void Clear()
	{
		minX = minY = maxX = maxY = 0;
	}

	public void SwapX()
	{
		var t = minX;

		minX = -maxX;
		maxX = -t;
	}

	public void SwapY()
	{
		var t = minY;

		minY = -maxY;
		maxY = -t;
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public static bool operator == (SgtRectL a, SgtRectL b)
	{
		return a.minX == b.minX && a.minY == b.minY && a.maxX == b.maxX && a.maxY == b.maxY;
	}

	public static bool operator != (SgtRectL a, SgtRectL b)
	{
		return a.minX != b.minX || a.minY != b.minY || a.maxX != b.maxX || a.maxY != b.maxY;
	}

	public override string ToString()
	{
		return "(" + minX + ", " + minY + " : " + maxX + ", " + maxY + ")";
	}
}