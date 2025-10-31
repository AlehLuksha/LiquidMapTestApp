using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LiquidMapTestApp.Helpers
{
    /// <summary>
    /// The json helper.
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Converts the x ml to s json.
        /// </summary>
        /// <param name="xmlSource">The xml source.</param>
        /// <param name="removeSpecialCharacters">If true, remove special characters.</param>
        /// <returns>A string.</returns>
        public static string ConvertXMlToSJson(string xmlSource, bool removeSpecialCharacters)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlSource);

            string jsonString = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.None, false);

            // Парсинг JSON строки в JObject
            JObject jsonObject = JObject.Parse(jsonString);

            JObject processedObject = jsonObject;

            if (removeSpecialCharacters)
            {
                // Рекурсивно переименуем ключи, убрав '@' и ':'
                processedObject = RemoveSpecialCharacters(jsonObject);
            }

            // Вывод отредактированного JSON
            string processedJson = processedObject.ToString(Newtonsoft.Json.Formatting.Indented);

            return processedJson;
        }

        /// <summary>
        /// Removes the special characters.
        /// </summary>
        /// <param name="originalObject">The original object.</param>
        /// <returns>A JObject.</returns>
        public static  JObject RemoveSpecialCharacters(JObject originalObject)
        {
            JObject newObject = new JObject();

            foreach (var property in originalObject.Properties())
            {
                string newKey = property.Name.Replace("@", "").Replace(":", "");

                if (property.Value is JObject)
                {
                    // Рекурсивный вызов для вложенных объектов
                    newObject[newKey] = RemoveSpecialCharacters((JObject)property.Value);
                }
                else if (property.Value is JArray)
                {
                    // Обрабатываем массивы
                    JArray newArray = new JArray();
                    foreach (var item in (JArray)property.Value)
                    {
                        if (item is JObject)
                        {
                            newArray.Add(RemoveSpecialCharacters((JObject)item));
                        }
                        else
                        {
                            newArray.Add(item);
                        }
                    }
                    newObject[newKey] = newArray;
                }
                else
                {
                    // Обычные значения
                    newObject[newKey] = property.Value;
                }
            }

            return newObject;
        }

    }
}
