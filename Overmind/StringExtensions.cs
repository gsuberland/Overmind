namespace Overmind
{
    static class StringExtensions
    {
        public static string ReplaceMany(this string subject, string[] find, string[] replace)
        {
            string result = subject;
            if (find.LongLength != replace.LongLength)
                throw new InvalidOperationException("Length of find and replace string arrays is not equal.");
            for (long i = 0; i < find.LongLength; i++)
            {
                result = result.Replace(find[i], replace[i]);
            }
            return result;
        }

        public static IEnumerable<string> ReplaceMany(this IEnumerable<string> subjects, string[] find, string[] replace)
        {
            return subjects.Select(s => s.ReplaceMany(find, replace));
        }
    }
}