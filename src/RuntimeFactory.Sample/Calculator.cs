namespace RuntimeFactory.Sample
{
    internal class Calculator : ICalculator
    {
        public int Sum(int a, int b)
        {
            return a + b + 55;
        }
    }
}