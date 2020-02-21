using Priority_Queue;

namespace OnlineQualification
{
	public class Problem
	{
		public int DayCount { get; set; }
		public int[] ScorePerBook { get; set; }
		public Library[] Libraries { get; set; }
	}

	public class Library : FastPriorityQueueNode
	{
		public int ID { get; set; }
		public int SignUpTime { get; set; }
		public int ScanRate { get; set; }
		public int[] BookIDs { get; set; }
	}
}
