using Priority_Queue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OnlineQualification
{
	class Program
	{
		static void Main(string[] args)
		{
			string[] fileNames = new[]
			{
				"a_example",
				"b_read_on",
				"c_incunabula",
				"d_tough_choices",
				"e_so_many_books",
				"f_libraries_of_the_world"
			};

			foreach (string fileName in fileNames)
			{
				Console.Write(fileName + "...");

				var problem = Read($@"..\..\..\..\input\{fileName}.txt");
				var solution = Solve(problem, out int score);

				using (var output = File.CreateText($@"..\..\..\..\output\{fileName}.txt"))
				{
					output.WriteLine(solution.SignUps.Count);
					foreach (var library in solution.SignUps)
					{
						output.WriteLine($"{library.LibraryID} {library.BookIDs.Count}");
						output.WriteLine(string.Join(" ", library.BookIDs));
					}
				}

				Console.WriteLine("\b\b\b\t" + score);
			}

			Console.WriteLine();
			Console.WriteLine("Done. Press Enter to terminate.");
			Console.ReadLine();
		}

		private static Problem Read(string filePath)
		{
			using (var input = File.OpenText(filePath))
			{
				int[] numbers = input.ReadLine().Split(' ').Select(int.Parse).ToArray();
				var problem = new Problem
				{
					Libraries = new Library[numbers[1]],
					DayCount = numbers[2]
				};
				problem.ScorePerBook = input.ReadLine().Split(' ').Select(int.Parse).ToArray();
				for (int i = 0; i < problem.Libraries.Length; i++)
				{
					numbers = input.ReadLine().Split(' ').Select(int.Parse).ToArray();
					problem.Libraries[i] = new Library
					{
						ID = i,
						SignUpTime = numbers[1],
						ScanRate = numbers[2]
					};
					int[] bookIDs = input.ReadLine().Split(' ').Select(int.Parse).ToArray();
					problem.Libraries[i].BookIDs = bookIDs.OrderBy(id => -problem.ScorePerBook[id]).ToArray();
				}
				return problem;
			}
		}

		private static State Solve(Problem problem, out int totalScore)
		{
			int timeLeft = problem.DayCount;
			totalScore = 0;
			var state = new State();
			var bookIDsAdded = new HashSet<int>();

			var libraryQueue = new FastPriorityQueue<Library>(problem.Libraries.Length);
			var idsOfLibrariesToSignUp = new HashSet<int>();
			foreach (var library in problem.Libraries)
			{
				float score = GetPotentialScore(problem, library, bookIDsAdded, timeLeft);
				if (score > 0)
				{
					libraryQueue.Enqueue(library, -score);
					idsOfLibrariesToSignUp.Add(library.ID);
				}
			}

			while (libraryQueue.Count != 0)
			{
				var library = libraryQueue.Dequeue();
				idsOfLibrariesToSignUp.Remove(library.ID);

				if (library.SignUpTime < timeLeft)
				{
					var signUp = new SignUp
					{
						LibraryID = library.ID,
					};
					AddBooks(problem, signUp, timeLeft, bookIDsAdded, out int scoreForLibrary);
					if (scoreForLibrary > 0)
					{
						state.SignUps.Add(signUp);
						timeLeft -= library.SignUpTime;
						totalScore += scoreForLibrary;

						foreach (var libraryToSignUp in idsOfLibrariesToSignUp.Select(id => problem.Libraries[id]).ToArray())
						{
							float score = GetPotentialScore(problem, libraryToSignUp, bookIDsAdded, timeLeft);
							if (score == 0)
							{
								libraryQueue.Remove(libraryToSignUp);
								idsOfLibrariesToSignUp.Remove(libraryToSignUp.ID);
							}
							else
							{
								libraryQueue.UpdatePriority(libraryToSignUp, -score);
							}
						}
					}
				}
			}
			return state;
		}

		private static void AddBooks(Problem problem, SignUp signUp, int timeLeft, HashSet<int> bookIDsAdded, out int score)
		{
			var library = problem.Libraries[signUp.LibraryID];
			foreach (int bookIDToAdd in GetBookIDsToAdd(problem, library, timeLeft, bookIDsAdded, out score))
			{
				signUp.BookIDs.Add(bookIDToAdd);
				bookIDsAdded.Add(bookIDToAdd);
			}
		}

		private static float GetPotentialScore(Problem problem, Library library, HashSet<int> bookIDsAdded, int timeLeft)
		{
			int timeLeftAfterSignUp = timeLeft - library.SignUpTime;
			return GetScoreForBookIDsToAdd(problem, library, timeLeftAfterSignUp, bookIDsAdded);
		}

		private static int[] GetBookIDsToAdd(Problem problem, Library library, int timeLeft, HashSet<int> bookIDsAdded, out int score)
		{
			score = 0;
			var bookIDsAddedForSignUp = new List<int>();

			int maximumBooksToAdd = Math.Min(timeLeft * library.ScanRate, library.BookIDs.Length);
			foreach (int bookID in library.BookIDs)
			{
				if (!bookIDsAdded.Contains(bookID))
				{
					bookIDsAddedForSignUp.Add(bookID);
					score += problem.ScorePerBook[bookID];

					if (bookIDsAddedForSignUp.Count >= maximumBooksToAdd)
						break;
				}
			}

			return bookIDsAddedForSignUp.ToArray();
		}

		private static float GetScoreForBookIDsToAdd(Problem problem, Library library, int timeLeft, HashSet<int> bookIDsAdded)
		{
			int score = 0;
			int booksAddedCount = 0;
			int maximumBooksToAdd = Math.Min(timeLeft * library.ScanRate, library.BookIDs.Length);
			foreach (int bookID in library.BookIDs)
			{
				if (!bookIDsAdded.Contains(bookID))
				{
					booksAddedCount++;
					score += problem.ScorePerBook[bookID];

					if (booksAddedCount >= maximumBooksToAdd)
						break;
				}
			}

			return score / (float)library.SignUpTime;
		}
	}
}
