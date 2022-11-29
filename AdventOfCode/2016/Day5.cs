using ProblemsLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AdventOfCode._2016
{
    [Problem("2016-05-01", MethodName = nameof(ExecutePart1))]
    [Problem("2016-05-02", MethodName = nameof(ExecutePart2))]
    public class Day5
    {
        private static IEnumerable<byte[]> Generator(string arg)
        {
            var md5 = MD5.Create();
            uint i = 0;
            while (true)
            {
                string str = $"{arg}{i}";
                var bytes = Encoding.ASCII.GetBytes(str);
                var hash = md5.ComputeHash(bytes);
                if (hash[0] == 0 && hash[1] == 0 && (hash[2] & 0xF0) == 0)
                    yield return hash;
                ++i;
            }
        }
        
        [TestCase("abc", "18f47a30")]
        public static string ExecutePart1(string input)
        {
            var result = "";
            foreach(var hash in Generator(input).Take(8))
                result += (hash[2] & 0x0F).ToString("x");
            return result;
        }
        
        [TestCase("abc", "05ace8e3")]
        public static string ExecutePart2(string input)
        {
            var md5 = MD5.Create();
            char[] result = new char[8];
            int count = 0;
            foreach(var hash in Generator(input).Where(hash => (hash[2] & 0x0F) < 8))
            {
                var six = hash[2] & 0x0F;
                var seven = (hash[3] & 0xF0) >> 4;
                if (result[six] == 0)
                {
                    result[six] = seven.ToString("x")[0];
                    ++count;
                    if (count == result.Length)
                        break;
                }
            }
            return new string(result);
        }
    }
}
