public struct HsvColor
{
	public double H;

	public double S;

	public double V;

	public float normalizedH => (float)H / 360f;

	public float normalizedS => (float)S;

	public float normalizedV => (float)V;

	public override string ToString()
	{
		return "{" + H.ToString("f2") + "," + S.ToString("f2") + "," + V.ToString("f2") + "}";
	}
}
