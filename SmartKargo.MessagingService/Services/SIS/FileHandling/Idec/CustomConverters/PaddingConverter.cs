using FileHelpers;

namespace QidWorkerRole.SIS.FileHandling.Idec.CustomConverters
{
    /// <summary>
    /// this converter class pads passed padding character of an object of passed length
    /// </summary>
    internal class PaddingConverter : ConverterBase
    {
        readonly int _totalLength;
        readonly char _paddingCharacter;
        readonly int _direction;    //0 : RJ ZF i.e. Left padding and 1 : LJ BF i.e. Right side padding


        public PaddingConverter(int length, char paddingChar, int direction)
        {
            _totalLength = length;
            _paddingCharacter = paddingChar;
            _direction = direction;
        }

        public override object StringToField(string from)
        {
            return from;
        }

        public override string FieldToString(object fieldValue)
        {
            if (fieldValue == null)
                fieldValue = "0";

            if (_direction == IdecConstants.RJZF_PaddingConverter)// RJ ZF
                return fieldValue.ToString().PadLeft(_totalLength, _paddingCharacter);

            //LJ BF
            return fieldValue.ToString().PadRight(_totalLength, _paddingCharacter);
        }
    }

    /// <summary>
    /// converts in the format yyMMdd
    /// </summary>
    internal class DateFormatConverter : ConverterBase
    {
        public override object StringToField(string from)
        {
            return from;
        }

        public override string FieldToString(object fieldValue)
        {
            const string defaultDateFormat = "000000";

            if (fieldValue != null && fieldValue.ToString() != string.Empty)
            {
                // added to handle the case of invalid date value like 000000
                try
                {
                    DateTime date = Convert.ToDateTime(fieldValue);
                    return date.ToString(IdecConstants.DateFormat);
                }
                catch (Exception)
                {
                    return fieldValue.ToString();
                }
            }
            return defaultDateFormat;
        }
    }

    /// <summary>
    /// This converter class will remove point from double value and pad specified number of decimal to right side and
    /// pad left side with a padding character
    /// ex: if value - 802 , fieldLength = 11 , decimal place = 2 and padding char ='0' then it will return 00000080200
    /// if value - 802.1 , fieldLength = 11 , decimal place = 2 and padding char ='0' then it will return 00000080210
    /// </summary>
    /// 
    internal class DoubleNumberConverter : ConverterBase
    {
        readonly int _fieldLength;
        readonly int _decimalPlaceValue;
        readonly char _paddingCharacter;

        public DoubleNumberConverter(int length, int decimalplaces, char padchar)
        {
            _fieldLength = length;
            _decimalPlaceValue = decimalplaces;
            _paddingCharacter = padchar;
        }

        public override object StringToField(string from)
        {
            if (string.IsNullOrWhiteSpace(from))
            {
                return from;
            }
            else
            {
                var fromDoubleValue = from.Substring(0, ((_fieldLength - _decimalPlaceValue))) + "." + from.Substring((_fieldLength - _decimalPlaceValue), _decimalPlaceValue);

                return fromDoubleValue;
            }
        }

        public override string FieldToString(object fieldValue)
        {
            if (fieldValue == null)
                fieldValue = "0";

            //Note : if string IsNullOrEmpty then initialize it to "0" so that it will be converted to zero field
            if (string.IsNullOrEmpty(fieldValue.ToString()))
                fieldValue = "0";
            // Make value +VE
            if (Convert.ToDouble(fieldValue) < 0)
            {
                fieldValue = -Convert.ToDouble(fieldValue);
            }

            //Logic : divide string into fractionNumberString and wholeNumberString
            //for 802.1 - fractionNumberString = 1 and wholeNumberString = 802
            //then pad fractionNumberString by padding char to given length 
            string fractionNumberString = string.Empty;
            string[] stringArray = fieldValue.ToString().Split('.');

            string wholeNumberString = stringArray[0];
            if (stringArray.Length > 1)
            {
                //Logic :For no 0.712340 if decimal place value is 5 then it should return 71234
                //But for no 0.1 if decimal place value is 2 then it should return only 1
                if (_decimalPlaceValue < stringArray[1].Length)
                    fractionNumberString = stringArray[1].Substring(0, _decimalPlaceValue);
                else
                    fractionNumberString = stringArray[1];
            }

            return string.Concat(wholeNumberString, fractionNumberString.PadRight(_decimalPlaceValue, _paddingCharacter)).PadLeft(_fieldLength, _paddingCharacter);

        }
    }
}