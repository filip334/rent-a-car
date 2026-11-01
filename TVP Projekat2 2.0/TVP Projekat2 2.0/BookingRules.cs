using System;
namespace TVP_Projekat2_2._0
{
    public static class BookingRules
    {
        public static bool Overlaps(DateTime start, DateTime end, DateTime otherStart, DateTime otherEnd)
        {
            return start < otherEnd && otherStart < end;
        }
        public static int CalculatePrice(int hourlyRate, DateTime start, DateTime end)
        {
            if (hourlyRate <= 0 || end - start < TimeSpan.FromHours(1))
                throw new ArgumentException("Cena mora biti pozitivna, a najam trajati najmanje jedan sat.");
            return checked(hourlyRate * (int)Math.Ceiling((end - start).TotalHours));
        }
    }
}
