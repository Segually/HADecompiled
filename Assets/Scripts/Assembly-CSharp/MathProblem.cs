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
		header = packet.GetString();
		type = (problem_type)packet.GetByte();
		shift = packet.GetByte();
	}

	public int Solve()
	{
		int left;
		int right;
		switch (type)
		{
			case (problem_type)0: left = 9; right = 12; break;
			case (problem_type)1: left = 0; right = 2; break;
			case (problem_type)2: left = 7; right = 8; break;
			case (problem_type)3: left = 12; right = 13; break;
			case (problem_type)4: left = 2; right = 9; break;
			case (problem_type)5: left = 11; right = 11; break;
			case (problem_type)6: left = 4; right = 7; break;
			case (problem_type)7: left = 8; right = 11; break;
			case (problem_type)8: left = 10; right = 11; break;
			case (problem_type)9: left = 1; right = 5; break;
			case (problem_type)10: left = 13; right = 13; break;
			case (problem_type)11: left = 5; right = 13; break;
			case (problem_type)12: left = 7; right = 10; break;
			case (problem_type)13: left = 3; right = 4; break;
			case (problem_type)14: left = 7; right = 9; break;
			case (problem_type)15: left = 6; right = 11; break;
			case (problem_type)16: left = 5; right = 6; break;
			case (problem_type)17: left = 0; right = 12; break;
			case (problem_type)18: left = 9; right = 10; break;
			case (problem_type)19: left = 2; right = 6; break;
			case (problem_type)20: left = 12; right = 13; break;
			case (problem_type)21: left = 7; right = 8; break;
			case (problem_type)22: left = 10; right = 11; break;
			case (problem_type)23: left = 1; right = 3; break;
			default: return -1;
		}
		left = (left + shift) % 14;
		right = (right + shift) % 14;
		if ((int)type < 8) return left + right;
		if ((int)type < 16) return left * right;
		return left - right;
	}
}
