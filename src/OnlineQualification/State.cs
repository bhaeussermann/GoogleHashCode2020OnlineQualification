using System.Collections.Generic;

namespace OnlineQualification
{
	public class State
	{
		public List<SignUp> SignUps { get; } = new List<SignUp>();
	}

	public class SignUp
	{
		public int LibraryID { get; set; }
		public List<int> BookIDs { get; } = new List<int>();
	}
}
