namespace RuntimeFactory.Sample
{
    public static class CalculatorFactory
    {
        public static ICalculator Create()
        {
        #if DEBUG
            return Factory<ICalculator, Calculator>.Create();
        #else
            return new Calculator();
        #endif
        }
    }
}