public class MathProblem
{
	public enum problem_type
	{
		add_9_and_12 = 0,
		add_0_and_2 = 1,
		add_7_and_8 = 2,
		add_12_and_13 = 3,
		add_2_and_9 = 4,
		add_11_and_11 = 5,
		add_4_and_7 = 6,
		add_8_and_11 = 7,
		mult_10_and_11 = 8,
		mult_1_and_5 = 9,
		mult_13_and_13 = 10,
		mult_5_and_13 = 11,
		mult_7_and_10 = 12,
		mult_3_and_4 = 13,
		mult_7_and_9 = 14,
		mult_6_and_11 = 15,
		sub_5_and_6 = 16,
		sub_0_and_12 = 17,
		sub_9_and_10 = 18,
		sub_2_and_6 = 19,
		sub_12_and_13 = 20,
		sub_7_and_8 = 21,
		sub_10_and_11 = 22,
		sub_1_and_3 = 23
	}

	private string header;

	private problem_type type;

	public byte shift;

	public void Unpack(Packet packet)
	{
	}

	public int Solve()
	{
		return 0;
	}
}
