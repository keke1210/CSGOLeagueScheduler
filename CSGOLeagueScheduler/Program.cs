using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSGOLeagueScheduler
{
    // Problem :
    // X company is hosting a CS GO league with 16 teams participating and has these rules:
    // 1. The league will last exactly 15 weeks
    // 2. Each team will face each other team exactly once
    // 3. Each team will have a match every week
    // 4. Every week will have exactly 8 matches 

    // n => nr. of teams
    // k => weeks pre-scheduled
    // s => lines to be added by my algo (s = n - 1 - k where k > 0 and k < n - 1)

    // mps (matches per week) => n / 2 (2 because 2 is the number of teams participating in a match)

    class Program
    {
        // Ask if a team can have a rematch based on the initial Input
        // 15 weeks total
        private static readonly string[] preScheduledStrExpressions = new string[]
        {
            "1:16,2:15,3:14,4:13,5:12,6:11,7:10,8:9", //w1
            "2:9,5:7,12:15,3:6,8:10,4:14,1:11,13:16", //w2
            "1:3,7:12,14:15,4:10,8:16,9:13,2:6,5:11", //w3
            "7:13,5:8,11:14,3:9,4:15,6:12,2:16,1:10", //w4
            "7:16,3:15,13:14,5:10,9:11,8:12,4:6,1:2" //w5

            //"9:1,10:2,11:3,12:4,13:5,14:6,15:7,12:2",
            //"13:3,15:5,16:6,8:7,12:10,14:2,16:4,7:6",
            //"12:1,13:11,14:10,15:9,8:3,7:4,6:5,13:1",
            //"14:12,15:11,16:10,7:2,5:4,14:1,15:13,16:12",
            //"8:11,6:9,5:2,4:3,15:1,16:14,8:13,4:9",
            //"3:2,8:15,7:14,6:13,4:11,3:10,8:1,6:15",
            //"5:14,3:12,2:11,9:10,7:1,6:8,5:16,2:13",
            //"9:12,10:11,6:1,4:8,3:16,9:14,10:13,11:12",
            //"5:1,3:7,2:8,9:16,10:15,12:13,4:1,3:5",
            //"9:7,11:16,2:4,9:5,10:6,11:7,14:8,15:16"
        };

        private static readonly string[] preScheduledStrExpressions2 = new string[]
        {
            "1:2,3:4", //w1

            //"3:1,4:2",
            //"4:1,2:3"
        };

        static void Main(string[] args)
        {
            Console.WriteLine("Example 1:\n");

            int n = 16;
            Solve(n, n - 1, preScheduledStrExpressions);

            Console.WriteLine("\n\n Example 2:\n");

            // Another example
            int n2 = 4;
            Solve(n2, n2 - 1, preScheduledStrExpressions2);

            Console.ReadKey();
        }

        public static void Solve(int n, int totalNumberWeeks, string[] preScheduledWeekExpr)
        {
            var league = new League(n, totalNumberWeeks, preScheduledWeekExpr);
            league.GenerateSchedules();
            league.DisplaySchedules();

            // Only for testing
            var expr = league.GenerateStringExpression();
            Console.WriteLine("\nString expressions for all the schedules:");
            Console.WriteLine(expr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n">number of teams</param>
        /// <param name="totalNumberWeeks"></param>
        /// <param name="preScheduledWeekExpr"></param>
        /// <param name="k">number of prescheduled weeks</param>

        // Note: I used struct instead of record because I needed to override the default implementation of IEquatable
        public struct Match : IEquatable<Match>
        {
            public Match(int Team1, int Team2)
            {
                if (Team1 == Team2)
                    throw new ArgumentException("Can't have a match between the same team!");

                this.Team1 = Team1;
                this.Team2 = Team2;
            }
            public int Team1 { get; }
            public int Team2 { get; }
            public Match AwayMatch => new Match(Team2, Team1);

            public bool Equals(Match other)
            {
                return (this.Team1.Equals(other.Team1) && this.Team2.Equals(other.Team2))
                    || (this.Team1.Equals(other.Team2) && this.Team2.Equals(other.Team1));
            }
            public static bool operator ==(Match match1, Match match2)
            {
                return match1.Equals(match2);
            }
            public static bool operator !=(Match match1, Match match2)
            {
                return !match1.Equals(match2);
            }
            public override bool Equals(object obj)
            {
                return obj is Match && Equals((Match)obj);
            }
            public override int GetHashCode()
            {
                return HashCode.Combine(this.Team1, this.Team2);
            }
            public override string ToString()
            {
                return $"{Team1}\tvs\t{Team2}";
            }
        }

        public class Week
        {
            private readonly int _n;

            private Match[] _matches;
            public Match[] Matches => _matches;

            public Week(int numOfMatches)
            {
                _n = numOfMatches;
                _matches = new Match[numOfMatches];
            }

            public Week(string weekExpression, int numOfMatches)
            {
                _n = numOfMatches;
                this.SetMatches(weekExpression);
            }

            public Week(IEnumerable<Match> matches, int numOfMatches)
            {
                _n = numOfMatches;
                _matches = matches.ToArray();
            }

            private void SetMatches(string weekExpression)
            {
                _matches = weekExpression.Split(',').Select(x =>
                {
                    var match = x.Split(':');

                    if (match[0].IsEmptyNullOrString() || match[1].IsEmptyNullOrString())
                        throw new ArgumentNullException($"Bad Inputon match expression: {match}");

                    var t1 = Convert.ToInt32(match[0]);
                    var t2 = Convert.ToInt32(match[1]);

                    if (t1 <= 0 || t1 > _n || t2 <= 0 || t2 > _n)
                        throw new ArgumentException($"Bad input. Teams should be values from 1 to {_n}");

                    return new Match(Team1: t1, Team2: t2);
                }).ToArray();
            }
        }

        public class League
        {
            private readonly int _n;
            private readonly string[] _preScheduledStringExpressions;
            private readonly int _totalNrOfWeeks;
            private readonly int _k;
            private readonly int _numOfMatchesPerWeek;
            private readonly int[] _allTeamsIDs;

            private IList<Match> _preScheduledMatches;

            public Week[] Weeks { get; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="n">Number of teams</param>
            /// <param name="totalNrOfWeeks">total number of weeks for the tournament</param>
            /// <param name="preScheduledStringExpressions">pre defined input</param>
            public League(int n, int totalNrOfWeeks, string[] preScheduledStringExpressions)
            {
                if (n % 2 != 0)
                    throw new ArgumentException("Number of teams can't be odd!");

                _preScheduledStringExpressions = preScheduledStringExpressions;
                _n = n;
                _allTeamsIDs = Enumerable.Range(1, _n).ToArray();
                _k = preScheduledStringExpressions.Length;
                _totalNrOfWeeks = totalNrOfWeeks;
                _numOfMatchesPerWeek = _n / 2;
                Weeks = new Week[_totalNrOfWeeks];
                SetPreScheduledMatches();
                ReserveSchedules();
            }

            /// <summary>
            /// Generate the new schedules
            /// </summary>
            public void GenerateSchedules()
            {
                //Using round robin algorithm to generate all possible matches

                // teamIDs that are possible to be scheduled
                var toBeScheduled = new List<int>();
                var matchesThatCanBeScheduled = new List<Match>();

                toBeScheduled.AddRange(_allTeamsIDs.Skip(_numOfMatchesPerWeek).Take(_numOfMatchesPerWeek));
                toBeScheduled.AddRange(_allTeamsIDs.Skip(1).Take(_numOfMatchesPerWeek - 1).ToArray().Reverse());

                int toBeScheduledCount = toBeScheduled.Count;

                // here get all the posible schedules and add them to matchesThatCanBeScheduled List
                for (int week = 0; week < _totalNrOfWeeks; week++)
                {
                    int teamIdx = week % toBeScheduledCount;

                    matchesThatCanBeScheduled.Add(new Match(toBeScheduled[teamIdx], _allTeamsIDs[0]));

                    for (int idx = 1; idx < _numOfMatchesPerWeek; idx++)
                    {
                        int firstTeamIdx = (week + idx) % toBeScheduledCount;
                        int secondTeamIdx = (week + toBeScheduledCount - idx) % toBeScheduledCount;
                        matchesThatCanBeScheduled.Add(new Match(toBeScheduled[firstTeamIdx], toBeScheduled[secondTeamIdx]));
                    }
                }

                // from matchesThatCanBeScheduled we remove the matches that were pre-scheduled and get the remaining schedules
                var remainingMatches = matchesThatCanBeScheduled.Except(_preScheduledMatches).ToList();

                int matchCount = 0;
                int weekCount = _k - 1;
                foreach (var match in remainingMatches)
                {
                    if (matchCount % _numOfMatchesPerWeek == 0)
                    {
                        ++weekCount;
                        Weeks[weekCount] = new Week(_numOfMatchesPerWeek);
                        matchCount = 0;
                    }
                    Weeks[weekCount].Matches[matchCount] = match;

                    ++matchCount;
                }
            }

            /// <summary>
            /// Display all the schedules on the Console
            /// </summary>
            public void DisplaySchedules()
            {
                int matchCount = 0;
                int weekCount = 0;
                var allMatches = this.Weeks.SelectMany(x => x.Matches);
                foreach (var match in allMatches)
                {
                    if (match.Team1 == default || match.Team2 == default)
                        break;

                    if (matchCount % _numOfMatchesPerWeek == 0)
                    {
                        ++weekCount;
                        Console.WriteLine();
                        Console.WriteLine($"{new string('-', 14)} Week {weekCount} {new string('-', 15)}");
                    }
                    ++matchCount;
                    Console.WriteLine($"\t{match}");
                }
            }

            /// <summary>
            /// Only for testing
            /// </summary>
            /// <returns></returns>
            public string GenerateStringExpression()
            {
                StringBuilder sb = new StringBuilder();
                int matchCount = 0;

                var allMatches = this.Weeks.SelectMany(x => x.Matches).ToArray();
                foreach (var match in allMatches)
                {
                    if (match.Team1 == default || match.Team2 == default)
                        break;

                    if (matchCount % _numOfMatchesPerWeek == 0)
                    {
                        sb.AppendLine();
                    }

                    sb.Append(match.Team1);
                    sb.Append(':');
                    sb.Append(match.Team2);

                    //If it's not the last element, add the ','
                    if (allMatches[allMatches.Length - 1] != match)
                        sb.Append(',');

                    ++matchCount;
                }

                return sb.ToString();
            }

            private void SetPreScheduledMatches()
            {
                // the matches we get from input
                _preScheduledMatches = _preScheduledStringExpressions.SelectMany(x => x.Split(',').Select(matchExpr =>
                {
                    var match = matchExpr.Split(':');

                    if (match[0].IsEmptyNullOrWhitespace() || match[1].IsEmptyNullOrWhitespace())
                        throw new ArgumentNullException($"Bad Input on match expression: {{{matchExpr}}}");

                    var t1 = Convert.ToInt32(match[0]);
                    var t2 = Convert.ToInt32(match[1]);

                    if (t1 <= 0 || t1 > _n || t2 <= 0 || t2 > _n)
                        throw new ArgumentException($"Bad input. Teams should be values from 1 to {_n}");

                    return new Match(Team1: t1, Team2: t2);
                })).ToList();

                // Add into the collection the away matches that we get from input
                int alreadyScheduledMatchesCount = _preScheduledMatches.Count;
                for (var i = 0; i < alreadyScheduledMatchesCount; i++)
                    _preScheduledMatches.Add(_preScheduledMatches[i].AwayMatch);
            }

            private void ReserveSchedules()
            {
                // Set the matches we get from the input, to the Weeks collection
                // Example: Adds the first 5 weeks from input
                for (int i = 0; i < _preScheduledStringExpressions.Length; i++)
                {
                    Weeks[i] = new Week(_preScheduledStringExpressions[i], _n);
                }

                // Adds the rest of the weeks that are not scheduled
                for (int i = _k; i < _totalNrOfWeeks; i++)
                {
                    Weeks[i] = new Week(new Match[_numOfMatchesPerWeek], _n);
                }
            }
        }
    }

    public static class StringExtensions
    {
        public static bool IsEmptyNullOrWhitespace(this string str)
                => string.IsNullOrWhiteSpace(str) || str == string.Empty;
    }
}
