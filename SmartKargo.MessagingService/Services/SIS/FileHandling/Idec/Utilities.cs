using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace QidWorkerRole.SIS.FileHandling.Idec
{
    internal static class Utilities
    {
        /// <summary>
        /// Method which returns a positive or negative amount value depending on the sign parameter for Decimal operand values only
        /// </summary>
        /// <param name="sign"></param>
        /// <param name="operand"></param>
        /// <returns></returns>
        public static decimal GetActualValueForDecimal(string sign, string operand)
        {
            var operandValue = Convert.ToDecimal(operand);

            if (!string.IsNullOrEmpty(sign.Trim()))
            {
                const decimal decimalZero = 0;
                operandValue = sign.Equals(IdecConstants.AmountNegativeSign, StringComparison.OrdinalIgnoreCase) ? decimalZero - operandValue : decimalZero + operandValue;
            }

            return operandValue;
        }

        /// <summary>
        /// Method which returns a positive or negative amount value depending on the sign parameter for Double operand values only
        /// </summary>
        public static double GetActualValueForDouble(string sign, string operand)
        {
            var operandValue = Convert.ToDouble(operand);

            if (!string.IsNullOrEmpty(sign.Trim()))
            {
                const double doubleZero = 0;
                operandValue = sign.Equals(IdecConstants.AmountNegativeSign, StringComparison.OrdinalIgnoreCase) ? doubleZero - operandValue : doubleZero + operandValue;
            }

            return operandValue;
        }

        /// <summary>
        /// Method which returns a positive or negative amount value depending on the sign parameter for Double operand values only
        /// </summary>
        public static string GetSignValue(double? value)
        {
            var sign = string.Empty;

            if (value != 0 && value != null)
            {
                sign = value > 0 ? IdecConstants.AmountPlusSign : IdecConstants.AmountNegativeSign;
            }

            return sign;
        }

        /// <summary>
        /// Method which returns a positive or negative amount value depending on the sign parameter for decimal operand values only
        /// </summary>
        public static string GetSignValue(decimal value)
        {
            var sign = string.Empty;

            if (value != 0)
            {
                sign = value > 0 ? IdecConstants.AmountPlusSign : IdecConstants.AmountNegativeSign;
            }

            return sign;
        }

        /// <summary>
        /// Method returns Boolean equivalent string
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static string GetBooDisplaylValue(bool flag)
        {
            return flag ? IdecConstants.TrueValue : IdecConstants.FalseValue;
        }

        /// <summary>
        /// This method will divide the parent collection into sub collections having at max specified children
        /// </summary>
        public static IEnumerable<List<T>> GetDividedSubCollections<T>(List<T> parentCollection, int count)
        {
            var totalElements = parentCollection.Count;
            var elementProcessingIndex = 0;

            if (parentCollection.Count <= count)
            {
                yield return parentCollection;
            }
            else
            {
                do
                {
                    var childCollection = new List<T>();
                    for (var i = 0; i < count; i++)
                    {
                        if (elementProcessingIndex == totalElements)
                            break;

                        childCollection.Add(parentCollection[elementProcessingIndex]);
                        elementProcessingIndex++;
                    }

                    if (childCollection.Count > 0)
                    {
                        yield return childCollection;
                    }

                } while (elementProcessingIndex < totalElements);
            }
        }

        /// <summary>
        /// Returns period in the format 'YYMMPP' for BM/CM Records output writing
        /// </summary>
        /// <param name="month">month</param>
        /// <param name="year">year</param>
        /// <param name="periodNo">period no.</param>
        /// <returns></returns>
        public static string GetFormattedPeriod(int month, int year, int periodNo)
        {
            var yearStr = year.ToString();
            var monthStr = month.ToString();
            var periodStr = periodNo.ToString();

            // This is case when year is passed as 4 digit number as 2009 
            if (yearStr.Length == 4)
            {
                yearStr = yearStr.Substring(2, 2);
            }

            return string.Format("{0}{1}{2}", yearStr.PadLeft(2, '0'), monthStr.PadLeft(2, '0'), periodStr.PadLeft(2, '0'));
        }

        /// <summary>
        /// To get formatted FlightDate : 00MMDD for BM/CM Coupon records output writing
        /// </summary>
        public static string GetFomattedFlightDate(int? month, int? day)
        {
            var monthstr = string.Empty;
            var daystr = string.Empty;

            if (month.HasValue)
            {
                monthstr = month.Value.ToString();
            }

            if (day.HasValue)
            {
                daystr = day.Value.ToString();
            }

            return string.Format("00{0}{1}", monthstr.PadLeft(2, '0'), daystr.PadLeft(2, '0'));
        }

        public static bool IsWholeNumber(string number)
        {
            if (String.IsNullOrEmpty(number))
            {
                return false;
            }
            var pattern = new Regex(@"(^\d*\.?\d*[0-9]+\d*$)|(^[0-9]+\d*\.\d*$)");
            return pattern.IsMatch(number);
        }

        /// <summary>
        /// Gets the string Airline Code.
        /// </summary>
        /// <param name="airlineCode">The Airline Code.</param>
        /// <returns></returns>
        public static string GetMemberNumericCode(int airlineCode)
        {
            /* 3 digit code remain as is */
            if (airlineCode <= 999)
            {
                return airlineCode.ToString().PadLeft(3, '0');
            }

            /* In case of 4 digit numeric code, first 2 positions are converted to an Alpha value.
             * provided the first two positions are between 10 and 35 */
            if (airlineCode > 999 && airlineCode <= 3599)
            {
                var asciiValue = Convert.ToInt32(airlineCode.ToString().Substring(0, 2)) + 55;
                return string.Format("{0}{1}", Convert.ToChar(asciiValue), airlineCode.ToString().Substring(2, 2));
            }

            return airlineCode.ToString();
        }

        /// <summary>
        /// Gets the numeric Airline Code.
        /// </summary>
        /// <param name="airlineCode">The Airline Code.</param>
        /// <returns></returns>
        public static int GetNumericMemberCode(string airlineCode)
        {
            var index = 0;
            int value;

            if (IsWholeNumber(airlineCode))
            {
                return Convert.ToInt32(airlineCode);
            }

            var memberCodeAsciiChars = new byte[airlineCode.Length];
            Encoding.ASCII.GetBytes(airlineCode.ToUpper(), 0, airlineCode.Length, memberCodeAsciiChars, 0);
            foreach (var memberCodeAsciiValue in memberCodeAsciiChars)
            {
                if (memberCodeAsciiValue <= 90 && memberCodeAsciiValue >= 65)
                {
                    //To get A = 10, B=11
                    value = memberCodeAsciiValue - 55;
                    string toReplace = airlineCode.Substring(index, 1);
                    airlineCode = airlineCode.Replace(toReplace, value.ToString());
                }
                index++;
            }

            int numericMemberCode;
            int returnValue;
            if (Int32.TryParse(airlineCode, out numericMemberCode))
            {
                returnValue = numericMemberCode > 9999 ? 0 : numericMemberCode;
            }
            else
            {
                returnValue = 0;
            }
            return returnValue;
        }
    }
}