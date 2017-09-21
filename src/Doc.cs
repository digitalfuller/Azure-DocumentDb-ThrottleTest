namespace Azure.DocumentDb.ThrottleTest
{
    internal class Doc
    {
        public int N;

        public string X;

        public static Doc BigDoc(int n, int e)
        {
            var x = "x";
            while (e-- > 0) x = x + x;
            return new Doc {N = n, X = x};
        }
    }
}