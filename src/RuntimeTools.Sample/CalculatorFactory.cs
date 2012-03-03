namespace HardcoreDebugging.Sample
{
    public static class CalculatorFactory
    {
        public static ICalculator Create()
        {
        #if DEBUG
            return DynamicActivator<ICalculator, Calculator>.CreateInstance();
        #else
            return new Calculator();
        #endif
        }
    }
}