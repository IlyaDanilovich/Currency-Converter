namespace Converter
{
    class ConverterCore
    {
        public double Convert(double amount, double fromRate, double toRate)
        {
            return fromRate / toRate * amount;
        }
    }
}
