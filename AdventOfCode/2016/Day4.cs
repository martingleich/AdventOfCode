using AdventOfCode.Utils;
using ProblemsLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdventOfCode._2016
{
    [Problem("2016-04-01", MethodName = nameof(ExecutePart1))]
    [Problem("2016-04-02", MethodName = nameof(ExecutePart2))]
    public class Day4
    {
        private record class Room(char[] Letters, int SectorId, char[]? Checksum)
        {
            public static readonly Parser<Room> Parser = Utils.Parser.MakeRegexParser(new Regex(@"^((?:[a-z]+-)+)(\d+)(?:\[([a-z]+)\])?"), m =>
            {
                var letters = m.Groups[1].Value.ToCharArray();
                var section = int.Parse(m.Groups[2].ValueSpan);
                var checksum = m.Groups[3].Success ? m.Groups[3].Value.ToCharArray() : null;
                return new Room(letters, section, checksum);
            });

            private static Func<char, char> DecryptChar(int shift) => c => c switch
                {
                    '-' => ' ',
                    _ => (char)('a' + ((c - 'a') + shift) % ('z' - 'a' + 1))
                };
            public string RealName => new (Letters.Select(DecryptChar(SectorId)).ToArray());
        }
        private static IEnumerable<Room> GetRealRooms(string input) => input
            .SplitLines()
            .Select(Room.Parser.Parse)
            .Where(IsRealRoom);

        [TestCase(@"
aaaaa-bbb-z-y-x-123[abxyz]
a-b-c-e-d-f-g-h-987[abcde]
not-a-real-room-404[oarel]
totally-real-room-200[decoy]", 123+987+404)]
        public static int ExecutePart1(string input) => GetRealRooms(input).Sum(k => k.SectorId);

        [TestCase("qzmt-zixmtkozy-ivhz-343", "very encrypted name")]
        private static string Decrypt(string input) => Room.Parser.Parse(input).RealName;
        public static int ExecutePart2(string input) => GetRealRooms(input)
            .Single(x => x.RealName.Contains("north") && x.RealName.Contains("storage"))
            .SectorId;

        private static bool IsRealRoom(Room arg) => arg.Checksum == null ||
            Utilities.Histogram(arg.Letters.Where(l => l != '-'))
            .OrderByDescending(x => x.Value).ThenBy(v => v.Key)
            .Take(5)
            .Select(v => v.Key)
            .SequenceEqual(arg.Checksum);
    }
}
