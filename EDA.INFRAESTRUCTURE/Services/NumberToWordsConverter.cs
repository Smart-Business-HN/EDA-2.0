namespace EDA.INFRAESTRUCTURE.Services
{
    public static class NumberToWordsConverter
    {
        private static readonly string[] Unidades =
        {
            "", "UN", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE",
            "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISEIS", "DIECISIETE",
            "DIECIOCHO", "DIECINUEVE", "VEINTE", "VEINTIUN", "VEINTIDOS", "VEINTITRES",
            "VEINTICUATRO", "VEINTICINCO", "VEINTISEIS", "VEINTISIETE", "VEINTIOCHO", "VEINTINUEVE"
        };

        private static readonly string[] Decenas =
        {
            "", "", "", "TREINTA", "CUARENTA", "CINCUENTA", "SESENTA", "SETENTA", "OCHENTA", "NOVENTA"
        };

        private static readonly string[] Centenas =
        {
            "", "CIENTO", "DOSCIENTOS", "TRESCIENTOS", "CUATROCIENTOS", "QUINIENTOS",
            "SEISCIENTOS", "SETECIENTOS", "OCHOCIENTOS", "NOVECIENTOS"
        };

        /// <summary>
        /// Convierte un número decimal a palabras en español con formato para facturas hondureñas.
        /// Ejemplo: 1234.56 -> "MIL DOSCIENTOS TREINTA Y CUATRO LEMPIRAS CON 56/100"
        /// </summary>
        public static string ConvertToWords(decimal number, string currency = "LEMPIRAS")
        {
            if (number < 0)
                return "MENOS " + ConvertToWords(Math.Abs(number), currency);

            if (number == 0)
                return $"CERO {currency} CON 00/100";

            long integerPart = (long)Math.Truncate(number);
            int decimalPart = (int)Math.Round((number - integerPart) * 100);

            string words = ConvertIntegerToWords(integerPart);

            // Manejar caso especial de "UN" -> "UNO" para números que terminan en 1
            if (integerPart == 1)
                words = "UN";

            return $"{words} {currency} CON {decimalPart:D2}/100";
        }

        private static string ConvertIntegerToWords(long number)
        {
            if (number == 0) return "CERO";
            if (number < 0) return "MENOS " + ConvertIntegerToWords(Math.Abs(number));

            string words = "";

            // Millones
            if (number >= 1000000)
            {
                long millions = number / 1000000;
                if (millions == 1)
                    words += "UN MILLON ";
                else
                    words += ConvertIntegerToWords(millions) + " MILLONES ";
                number %= 1000000;
            }

            // Miles
            if (number >= 1000)
            {
                long thousands = number / 1000;
                if (thousands == 1)
                    words += "MIL ";
                else
                    words += ConvertIntegerToWords(thousands) + " MIL ";
                number %= 1000;
            }

            // Centenas
            if (number >= 100)
            {
                if (number == 100)
                {
                    words += "CIEN ";
                    number = 0;
                }
                else
                {
                    words += Centenas[number / 100] + " ";
                    number %= 100;
                }
            }

            // Decenas y unidades
            if (number > 0)
            {
                if (number < 30)
                {
                    words += Unidades[number];
                }
                else
                {
                    words += Decenas[number / 10];
                    if (number % 10 > 0)
                        words += " Y " + Unidades[number % 10];
                }
            }

            return words.Trim();
        }
    }
}
