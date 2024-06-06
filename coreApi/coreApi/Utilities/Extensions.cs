using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace coreApi.Utilities.Extensions
{
    public static class Extensions
    {
        public static string CipherText = "North South GIS India Private Limited";

        #region Public Static Methods

        public static string GenerateRandomPassword(PasswordOptions opts = null)

        {

            if (opts == null) opts = new PasswordOptions()

            {
                RequiredLength = 8,
                RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = false,
                RequireUppercase = true
            };


            string[] randomChars = new[] {
        "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase
        "abcdefghijkmnopqrstuvwxyz",    // lowercase
        "0123456789",                   // digits
        "!@$?_-"                        // non-alphanumeric
    };

            CryptoRandom rand = new CryptoRandom();

            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count; i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count), rcs[rand.Next(0, rcs.Length)]);
            }
            return new string(chars.ToArray());
        }


        /// <summary>
        /// Get Distinct Records from given Input Data Table
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="Columns"></param>
        /// <returns></returns>
        public static DataTable GetDistinctRecords(this DataTable inputTable, string[] columns)
        {
            DataTable dtUniqRecords = new DataTable();
            dtUniqRecords = inputTable.DefaultView.ToTable(true, columns);
            return dtUniqRecords;
        }

        public static string GetFileSize(this string fileLocation)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = new FileInfo(fileLocation).Length;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            string result = String.Format("{0:0.##} {1}", len, sizes[order]);
            return result;
        }


        public static DateTime UnixTimeStampToDateTime(this double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static string XmlSerialize<T>(this T objectToSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

            StringWriter stringWriter = new StringWriter();
            XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);

            xmlWriter.Formatting = System.Xml.Formatting.Indented;
            xmlSerializer.Serialize(xmlWriter, objectToSerialize);

            return stringWriter.ToString();
        }


        public static string ToXML(this Object oObject)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlSerializer xmlSerializer = new XmlSerializer(oObject.GetType());
            using (MemoryStream xmlStream = new MemoryStream())
            {
                xmlSerializer.Serialize(xmlStream, oObject);
                xmlStream.Position = 0;
                xmlDoc.Load(xmlStream);
                return xmlDoc.InnerXml;
            }
        }
        public static string SerializeXML(this object dataToSerialize)
        {
            if (dataToSerialize == null) return null;

            using (StringWriter stringwriter = new System.IO.StringWriter())
            {
                var serializer = new XmlSerializer(dataToSerialize.GetType());
                serializer.Serialize(stringwriter, dataToSerialize);
                return stringwriter.ToString();
            }
        }




        /// <summary>
        /// Is Empty Checking
        /// </summary>
        /// <param name="dataSet">DataSet</param>
        /// <returns>return bool is dataset is empty</returns>
        public static bool IsEmpty(this DataSet dataSet)
        {
            return dataSet == null || dataSet.Tables.Count == 0 || !dataSet.Tables.Cast<DataTable>().Any(i => i.Rows.Count > 0);
        }

        /// <summary>
        /// To json.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The Json of any object.</returns>
        public static string ToJson(this object value)
        {
            if (value == null) return string.Empty;

            try
            {
                string json = JsonConvert.SerializeObject(value);
                return json;
            }
            catch
            {
                //log exception but dont throw one
                return string.Empty;
            }
        }

        public static T GetValue<T>(this DataRow row, string field)
        {
            if (!row.Table.Columns.Contains(field))
            {
                return default(T);
            }
            else
            {

                return row.IsNull(field) ? default(T) : (T)Convert.ChangeType(row[field].ToString(), typeof(T));

            }
        }

        //public static T FieldOrDefault<T>(this DataRow row, string columnName)
        //{
        //    return row.IsNull(columnName) ? default(T) : row.Field<T>(columnName);
        //}


        private static byte[] Encrypt(byte[] EncryptData, byte[] Key, byte[] IV)
        {
            MemoryStream objMemoryStream = new MemoryStream();
            TripleDES objTripleDES = TripleDES.Create();
            objTripleDES.Key = Key;
            objTripleDES.IV = IV;
            CryptoStream objCryptoStream = new CryptoStream(objMemoryStream, objTripleDES.CreateEncryptor(), CryptoStreamMode.Write);
            objCryptoStream.Write(EncryptData, 0, EncryptData.Length);
            objCryptoStream.Close();
            byte[] EncryptedData = objMemoryStream.ToArray();
            return EncryptedData;
        }

        public static string Encrypt(this string strPassword)
        {
            strPassword = strPassword.Replace(" ", "+");
            byte[] objByte = Encoding.Unicode.GetBytes(strPassword);
            PasswordDeriveBytes objPDB = new PasswordDeriveBytes(CipherText, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            byte[] objbyteEncryptedData = Encrypt(objByte, objPDB.GetBytes(16), objPDB.GetBytes(8));
            return Convert.ToBase64String(objbyteEncryptedData);
        }

        private static byte[] Decrypt(byte[] DecryptData, byte[] Key, byte[] IV)
        {
            MemoryStream objMemoryStream = new MemoryStream();
            TripleDES objTripleDES = TripleDES.Create();
            objTripleDES.Key = Key;
            objTripleDES.IV = IV;
            CryptoStream objCryptoStream = new CryptoStream(objMemoryStream, objTripleDES.CreateDecryptor(), CryptoStreamMode.Write);
            objCryptoStream.Write(DecryptData, 0, DecryptData.Length);
            objCryptoStream.Close();
            byte[] DecryptedData = objMemoryStream.ToArray();
            return DecryptedData;
        }

        public static string Decrypt(this string strPassword)
        {
            byte[] objByte = Convert.FromBase64String(strPassword);
            PasswordDeriveBytes objPDB = new PasswordDeriveBytes(CipherText, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            byte[] objDecryptedData = Decrypt(objByte, objPDB.GetBytes(16), objPDB.GetBytes(8));
            return Encoding.Unicode.GetString(objDecryptedData);
        }


        /// <summary>
        /// Checks string object's value to array of string values
        /// </summary>        
        /// <param name="stringValues">Array of string values to compare</param>
        /// <returns>Return true if any string value matches</returns>
        public static bool In(this string value, params string[] stringValues)
        {
            foreach (string otherValue in stringValues)
                if (string.Compare(value, otherValue) == 0)
                    return true;
            return false;
        }

        /// <summary>
        /// 	Determines whether the specified string is null or empty.
        /// </summary>
        /// <param name = "value">The string value to check.</param>
        public static bool IsNullOrEmpty(this string value)
        {
            return ((value == null) || (value.Length == 0));
        }

        /// <summary>
        /// Convert text's case to a title case
        /// </summary>
        /// <param name="value">Converting Value</param>
        /// <param name="culture">Culture</param>
        /// <returns>Converted Title Case string</returns>
        /// <remarks>UppperCase characters is the source string after the first of 
        /// each word are lowered, unless the word is exactly 2 characters</remarks>
        public static string ToTitleCase(this string value, CultureInfo culture)
        {
            return culture.TextInfo.ToTitleCase(value);
        }

        /// <summary>
        /// 	Formats the value with the parameters using string.Format.
        /// </summary>
        /// <param name = "value">The input string.</param>
        /// <param name = "parameters">The parameters.</param>
        /// <returns></returns>
        public static string FormatWith(this string value, params object[] parameters)
        {
            return string.Format(value, parameters);
        }

        /// <summary>
        /// 	Determines whether the comparison value strig is contained within the input value string
        /// </summary>
        /// <param name = "inputValue">The input value.</param>
        /// <param name = "comparisonValue">The comparison value.</param>
        /// <param name = "comparisonType">Type of the comparison to allow case sensitive or insensitive comparison.</param>
        /// <returns>
        /// 	<c>true</c> if input value contains the specified value, otherwise, <c>false</c>.
        /// </returns>
        public static bool Contains(this string inputValue, string comparisonValue, StringComparison comparisonType)
        {
            return (inputValue.IndexOf(comparisonValue, comparisonType) != -1);
        }

        /// <summary>
        /// 	Tests whether the contents of a string is a numeric value
        /// </summary>
        /// <param name = "value">String to check</param>
        /// <returns>
        /// 	Boolean indicating whether or not the string contents are numeric
        /// </returns>
        public static bool IsNumeric(this string value)
        {
            float output;
            return float.TryParse(value, out output);
        }

        /// <summary>
        /// Finds out if the specified string contains null, empty or consists only of white-space characters
        /// </summary>
        /// <param name="value">The input string</param>
        /// <returns>Boolean indicating whether or not the string is emptry or white space</returns>
        public static bool IsEmptyOrWhiteSpace(this string value)
        {
            return (value.IsNullOrEmpty() || value.All(t => char.IsWhiteSpace(t)));
        }

        /// <summary>
        /// Uppercase First Letter
        /// </summary>
        /// <param name="value">The string value to process</param>
        /// <returns>returns String with First Letter Uppercase</returns>
        public static string ToUpperFirstLetter(this string value)
        {
            if (value.IsEmptyOrWhiteSpace()) return string.Empty;
            char[] valueChars = value.ToCharArray();
            valueChars[0] = char.ToUpper(valueChars[0]);
            return new string(valueChars);
        }

        /// <summary>
        /// Returns the left part of the string.
        /// </summary>
        /// <param name="value">The original string.</param>
        /// <param name="characterCount">The character count to be returned.</param>
        /// <returns>The left part</returns>
        public static string Left(this string value, int characterCount)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (characterCount >= value.Length)
                throw new ArgumentOutOfRangeException("characterCount", characterCount, "characterCount must be less than length of string");
            return value.Substring(0, characterCount);
        }

        /// <summary>
        /// Returns the Right part of the string.
        /// </summary>
        /// <param name="value">The original string.</param>
        /// <param name="characterCount">The character count to be returned.</param>
        /// <returns>The right part</returns>
        public static string Right(this string value, int characterCount)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (characterCount >= value.Length)
                throw new ArgumentOutOfRangeException("characterCount", characterCount, "characterCount must be less than length of string");
            return value.Substring(value.Length - characterCount);
        }

        /// <summary>
        /// Truncates the string to a specified length and replace the truncated to a ...
        /// </summary>
        /// <param name="text">string that will be truncated</param>
        /// <param name="maxLength">total length of characters to maintain before the truncate happens</param>
        /// <returns>truncated string</returns>
        public static string Truncate(this string text, int maxLength)
        {
            // replaces the truncated string to a ...
            const string suffix = "...";
            string truncatedString = text;

            if (maxLength <= 0) return truncatedString;
            int strLength = maxLength - suffix.Length;

            if (strLength <= 0) return truncatedString;

            if (text == null || text.Length <= maxLength) return truncatedString;

            truncatedString = text.Substring(0, strLength);
            truncatedString = truncatedString.TrimEnd();
            truncatedString += suffix;
            return truncatedString;
        }

        public static double ToDouble(this string input, bool throwExceptionIfFailed = false)
        {
            double result;
            var valid = double.TryParse(input, NumberStyles.AllowDecimalPoint,
              new NumberFormatInfo { NumberDecimalSeparator = "." }, out result);
            if (!valid)
                if (throwExceptionIfFailed)
                    throw new FormatException(string.Format("'{0}' cannot be converted as double", input));
            return result;
        }

        /// <summary>
        /// Converts a string to an Int32 value
        /// </summary>
        /// <param name="text">The text to convert.</param>
        /// <returns>Returns integer value </returns>
        public static int ToInt32(this string text)
        {
            try
            {
                return Convert.ToInt32(text);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Converts a double value to an Int32 value
        /// </summary>
        /// <param name="doubleValue">The doubleValue to convert.</param>
        /// <returns>Returns integer value </returns>
        public static int ToInt32(this double? doubleValue)
        {
            try
            {
                return Convert.ToInt32(doubleValue);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Converts the object to datetime
        /// </summary>
        /// <param name="date">object to convert</param>
        /// <returns>Returns datetime object if convertable otherwise default datetime as 01-01-1900 00:00:00</returns>
        public static DateTime ToDateTime(this object date)
        {
            try
            {
                return Convert.ToDateTime(date);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Serializes the object
        /// </summary>
        /// <param name="obj">Object to Serialize</param>
        /// <returns>Serialized byte array</returns>
        public static byte[] Serialize(this object obj)
        {
            try
            {
                byte[] serialized;
                MemoryStream ms = new MemoryStream();
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(ms, obj);
                serialized = new byte[ms.Length];
                ms.Read(serialized, 0, (int)ms.Length);

                return serialized;
            }
            catch
            {
                return null;
            }

        }

        /// <summary>
        /// De-Serializes the object
        /// </summary>
        /// <typeparam name="T">Data Type</typeparam>
        /// <param name="serialized">Object to Serialize</param>
        /// <returns>returns Deserialized Object</returns>
        public static T Deserialize<T>(this byte[] serialized)
        {
            T deserialized;
            MemoryStream ms = new MemoryStream(serialized);
            BinaryFormatter deSerializer = new BinaryFormatter();
            deserialized = (T)deSerializer.Deserialize(ms); ;
            return deserialized;
        }

        /// <summary>
        /// Data Table to CSV Conversion
        /// </summary>
        /// <param name="table">Datatable to Converted</param>
        /// <param name="delimiter">delimiter</param>
        /// <param name="includeHeader">Header to be Include or Not.</param>
        /// <param name="fileNameWithPath">File Saving Location</param>
        public static void ToCSV(this DataTable table, string delimiter, bool includeHeader, string fileNameWithPath)
        {
            StringBuilder result = new StringBuilder();
            if (includeHeader)
            {
                foreach (DataColumn column in table.Columns)
                {
                    result.Append(column.ColumnName);
                    result.Append(delimiter);
                }
                result.Remove(--result.Length, 0);
                result.Append(Environment.NewLine);
            }
            foreach (DataRow row in table.Rows)
            {
                foreach (object item in row.ItemArray)
                {
                    if (item is System.DBNull)
                        result.Append(delimiter);
                    else
                    {
                        string itemAsString = item.ToString();
                        // Double up all embedded double quotes
                        itemAsString = itemAsString.Replace("\"", "\"\"");
                        // To keep things simple, always delimit with double-quotes
                        // so we don't have to determine in which cases they're necessary
                        // and which cases they're not.
                        itemAsString = "\"" + itemAsString + "\"";
                        result.Append(itemAsString + delimiter);
                    }
                }
                result.Remove(--result.Length, 0);
                result.Append(Environment.NewLine);
            }
            using (StreamWriter writer = new StreamWriter(fileNameWithPath, true))
            {
                writer.Write(result.ToString());
            }
        }

        public static DataTable LINQToDataTable<T>(this IEnumerable<T> varlist)
        {
            DataTable dtReturn = new DataTable();

            // column names 
            PropertyInfo[] oProps = null;

            if (varlist == null) return dtReturn;

            foreach (T rec in varlist)
            {
                // Use reflection to get property names, to create table, Only first time, others           will follow 
                if (oProps == null)
                {
                    oProps = ((Type)rec.GetType()).GetProperties();
                    foreach (PropertyInfo pi in oProps)
                    {
                        Type colType = pi.PropertyType;

                        if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                        {
                            colType = colType.GetGenericArguments()[0];
                        }

                        dtReturn.Columns.Add(new DataColumn(pi.Name, colType));
                    }
                }

                DataRow dr = dtReturn.NewRow();

                foreach (PropertyInfo pi in oProps)
                {
                    dr[pi.Name] = pi.GetValue(rec, null) == null ? DBNull.Value : pi.GetValue
                    (rec, null);
                }

                dtReturn.Rows.Add(dr);
            }
            return dtReturn;
        }

        // public static DataTable ToDataTable(System.Data.Linq.DataContext ctx, object query)
        // {
        //     if (query == null)
        //     {
        //         throw new ArgumentNullException("query");
        //     }

        //     IDbCommand cmd = ctx.GetCommand(query as IQueryable);
        //     SqlDataAdapter adapter = new SqlDataAdapter();
        //     adapter.SelectCommand = (SqlCommand)cmd;
        //     DataTable dt = new DataTable("sd");

        //     try
        //     {
        //         cmd.Connection.Open();
        //         adapter.FillSchema(dt, SchemaType.Source);
        //         adapter.Fill(dt);
        //     }
        //     finally
        //     {
        //         cmd.Connection.Close();
        //     }
        //     return dt;
        // }
        //public static DataTable ToDataTable(System.Data.Linq.DataContext ctx, object query)
        //{
        //    if (query == null)
        //    {
        //        throw new ArgumentNullException("query");
        //    }

        //    IDbCommand cmd = ctx.GetCommand(query as IQueryable);
        //    SqlDataAdapter adapter = new SqlDataAdapter();
        //    adapter.SelectCommand = (SqlCommand)cmd;
        //    DataTable dt = new DataTable("sd");

        //    try
        //    {
        //        cmd.Connection.Open();
        //        adapter.FillSchema(dt, SchemaType.Source);
        //        adapter.Fill(dt);
        //    }
        //    finally
        //    {
        //        cmd.Connection.Close();
        //    }
        //    return dt;
        //}

        //   public static DataTable AsDataTable<T>(this IEnumerable<T> list)
        //where T : class
        //   {
        //       DataTable dtOutput = new DataTable();

        //       //if the list is empty, return empty data table
        //       if (list.Count() == 0)
        //           return dtOutput;

        //       //get the list of  public properties and add them as columns to the
        //       //output table           
        //       PropertyInfo[] properties = list.FirstOrDefault().GetType().
        //           GetProperties(BindingFlags.Public | BindingFlags.Instance);
        //       foreach (PropertyInfo propertyInfo in properties)
        //           dtOutput.Columns.Add(propertyInfo.Name, propertyInfo.PropertyType);

        //       //populate rows
        //       DataRow dr;
        //       //iterate through all the objects in the list and add them
        //       //as rows to the table
        //       foreach (T t in list)
        //       {
        //           dr = dtOutput.NewRow();
        //           //iterate through all the properties of the current object
        //           //and set their values to data row
        //           foreach (PropertyInfo propertyInfo in properties)
        //           {
        //               dr[propertyInfo.Name] = propertyInfo.GetValue(t, null);
        //           }
        //           dtOutput.Rows.Add(dr);
        //       }
        //       return dtOutput;
        //   }


        public static DateTime? ToDateTimeNullable(this string value)
        {
            try
            {
                return Convert.ToDateTime(value);
            }
            catch
            {
                return (DateTime?)null;
            }
        }

        public static int? ToInt32Nullable(this string value)
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return (int?)null;
            }
        }

        /// <summary>
        /// Converts a string to an Int64 value
        /// </summary>
        /// <param name="text">The text to convert.</param>
        /// <returns>Returns integer value </returns>
        public static long ToInt64(this string text)
        {
            try
            {
                return Convert.ToInt64(text);
            }
            catch
            {
                return 0;
            }
        }

        public static long? ToInt64Nullable(this string value)
        {
            try
            {
                return Convert.ToInt64(value);
            }
            catch
            {
                return (long?)null;
            }
        }

        public static decimal? ToDecimalNullable(this string value)
        {
            try
            {
                return Convert.ToDecimal(value);
            }
            catch
            {
                return (decimal?)null;
            }
        }
        public static decimal ToDecimal(this string value)
        {
            try
            {
                return Convert.ToDecimal(value);
            }
            catch
            {
                return 0;
            }
        }
        public static short? ToInt16Nullable(this string value)
        {
            try
            {
                return Convert.ToInt16(value);
            }
            catch
            {
                return (short?)null;
            }
        }


        public static String ConvertToWords(this String requiredNumber)
        {
            String returnValue = "", wholeNo = requiredNumber, points = "", andStr = "", pointStr = "";
            String endString = "Only";
            try
            {
                int decimalPlace = requiredNumber.IndexOf(".");
                if (decimalPlace > 0)
                {
                    wholeNo = requiredNumber.Substring(0, decimalPlace);
                    points = requiredNumber.Substring(decimalPlace + 1);
                    if (Convert.ToInt32(points) > 0)
                    {
                        andStr = "and";// just to separate whole numbers from points/cents  
                        endString = "Paisa " + endString;//Cents  
                        pointStr = ConvertDecimals(points);
                    }
                }
                returnValue = String.Format("{0} {1}{2} {3}", ConvertWholeNumber(wholeNo).Trim(), andStr, pointStr, endString);
            }
            catch { }
            return returnValue;
        }
        public static string ToTitleCase(this string s)
        {
            //return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
        }

        #endregion

        #region Private Static Methods
        private static String ConvertWholeNumber(String Number)
        {
            string word = "";
            try
            {
                bool beginsZero = false;//tests for 0XX
                bool isDone = false;//test if already translated
                double dblAmt = (Convert.ToDouble(Number));
                //if ((dblAmt > 0) && number.StartsWith("0"))
                if (dblAmt > 0)
                {//test for zero or digit zero in a nuemric
                    beginsZero = Number.StartsWith("0");

                    int numDigits = Number.Length;
                    int pos = 0;//store digit grouping
                    String place = "";//digit grouping name:hundres,thousand,etc...
                    switch (numDigits)
                    {
                        case 1://ones' range

                            word = ones(Number);
                            isDone = true;
                            break;
                        case 2://tens' range
                            word = tens(Number);
                            isDone = true;
                            break;
                        case 3://hundreds' range
                            pos = (numDigits % 3) + 1;
                            place = " Hundred ";
                            break;
                        case 4://thousands' range
                        case 5:
                        case 6:
                            pos = (numDigits % 4) + 1;
                            place = " Thousand ";
                            break;
                        case 7://millions' range
                        case 8:
                        case 9:
                            pos = (numDigits % 7) + 1;
                            place = " Million ";
                            break;
                        case 10://Billions's range
                        case 11:
                        case 12:

                            pos = (numDigits % 10) + 1;
                            place = " Billion ";
                            break;
                        //add extra case options for anything above Billion...
                        default:
                            isDone = true;
                            break;
                    }
                    if (!isDone)
                    {//if transalation is not done, continue...(Recursion comes in now!!)
                        if (Number.Substring(0, pos) != "0" && Number.Substring(pos) != "0")
                        {
                            try
                            {
                                word = ConvertWholeNumber(Number.Substring(0, pos)) + place + ConvertWholeNumber(Number.Substring(pos));
                            }
                            catch { }
                        }
                        else
                        {
                            word = ConvertWholeNumber(Number.Substring(0, pos)) + ConvertWholeNumber(Number.Substring(pos));
                        }

                        //check for trailing zeros
                        //if (beginsZero) word = " and " + word.Trim();
                    }
                    //ignore digit grouping names
                    if (word.Trim().Equals(place.Trim())) word = "";
                }
            }
            catch { }
            return word.Trim();
        }

        private static String tens(String Number)
        {
            int _Number = Convert.ToInt32(Number);
            String name = null;
            switch (_Number)
            {
                case 10:
                    name = "Ten";
                    break;
                case 11:
                    name = "Eleven";
                    break;
                case 12:
                    name = "Twelve";
                    break;
                case 13:
                    name = "Thirteen";
                    break;
                case 14:
                    name = "Fourteen";
                    break;
                case 15:
                    name = "Fifteen";
                    break;
                case 16:
                    name = "Sixteen";
                    break;
                case 17:
                    name = "Seventeen";
                    break;
                case 18:
                    name = "Eighteen";
                    break;
                case 19:
                    name = "Nineteen";
                    break;
                case 20:
                    name = "Twenty";
                    break;
                case 30:
                    name = "Thirty";
                    break;
                case 40:
                    name = "Fourty";
                    break;
                case 50:
                    name = "Fifty";
                    break;
                case 60:
                    name = "Sixty";
                    break;
                case 70:
                    name = "Seventy";
                    break;
                case 80:
                    name = "Eighty";
                    break;
                case 90:
                    name = "Ninety";
                    break;
                default:
                    if (_Number > 0)
                    {
                        name = tens(Number.Substring(0, 1) + "0") + " " + ones(Number.Substring(1));
                    }
                    break;
            }
            return name;
        }
        private static String ones(String Number)
        {
            int _Number = Convert.ToInt32(Number);
            String name = "";
            switch (_Number)
            {

                case 1:
                    name = "One";
                    break;
                case 2:
                    name = "Two";
                    break;
                case 3:
                    name = "Three";
                    break;
                case 4:
                    name = "Four";
                    break;
                case 5:
                    name = "Five";
                    break;
                case 6:
                    name = "Six";
                    break;
                case 7:
                    name = "Seven";
                    break;
                case 8:
                    name = "Eight";
                    break;
                case 9:
                    name = "Nine";
                    break;
            }
            return name;
        }

        private static String ConvertDecimals(String number)
        {
            String cd = "", digit = "", engOne = "";
            for (int i = 0; i < number.Length; i++)
            {
                digit = number[i].ToString();
                if (digit.Equals("0"))
                {
                    engOne = "Zero";
                }
                else
                {
                    engOne = ones(digit);
                }
                cd += " " + engOne;
            }
            return cd;
        }
        #endregion
        /// <summary>
        /// Converts a string to an Int32 value
        /// </summary>
        /// <param name="text">The text to convert.</param>
        /// <returns>Returns integer value </returns>
    }
}
