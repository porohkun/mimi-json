﻿using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MimiJson
{

    public class JSONObject : IEnumerable<KeyValuePair<string, JsonValue>>
    {
        private readonly IDictionary<string, JsonValue> values = new Dictionary<string, JsonValue>();

        public static string Tab = "    ";

        #region constructors

        public JSONObject()
        {
            
        }

        public JSONObject(IEnumerable<JOPair> args):this()
        {
            foreach (var arg in args)
                Add(arg.Key, arg.Value);
        }

        public JSONObject(params JOPair[] args):this((IEnumerable<JOPair>)args)
        {
        }

        #endregion

        public bool ContainsKey(string key)
        {
            return values.ContainsKey(key);
        }

        public ICollection<string> Keys { get { return values.Keys; } }
        public ICollection<JsonValue> Values { get { return values.Values; } }

        public void Add(string key, JsonValue value)
        {
            values[key] = value;
        }

        public void Add(KeyValuePair<string, JsonValue> pair)
        {
            values[pair.Key] = pair.Value;
        }

        public void Remove(string key)
        {
            if (values.ContainsKey(key))
            {
                values.Remove(key);
            }
        }

        public void Clear()
        {
            values.Clear();
        }

        public JsonValue GetValue(string key)
        {
            JsonValue value;
            if (values.TryGetValue(key, out value))
                if (value.Type == JsonValueType.Refer)
                    return value.Refer;
            return value;
        }

        public JsonValue this[string key]
        {
            get { return GetValue(key); }
            set { values[key] = value; }
        }

        public int Count
        {
            get { return values.Count; }
        }

        public static JSONObject Parse(string jsonString, ref int position)
        {
            if (string.IsNullOrEmpty(jsonString))
                return null;
            
            JSONObject obj;
            
            Parser.SkipWhitespace(jsonString, ref position);

            if (Parser.CheckObjectStart(jsonString, position))
            {
                obj = new JSONObject();
                position++;
                Parser.SkipWhitespace(jsonString, ref position);
                
                while (!Parser.CheckObjectEnd(jsonString, position))
                {
                    if (position >= jsonString.Length)
                        return obj;
                    int keyStart = position;
                    string key = Parser.ParseString(jsonString, ref position);
                    //if (key == null)
                    //    return Fail("key string", position);
                    
                    position++;
                    int keyEnd = position;
                    Parser.SkipWhitespace(jsonString, ref position);
                    
                    if (Parser.CheckKeyValueSeparator(jsonString, position))
                    {
                        position++;
                        Parser.SkipWhitespace(jsonString, ref position);
                        
                        JsonValue value = JsonValue.Parse(jsonString, ref position);
                        obj.Add(key, value);
                        value.KeyPos = new JsonPos(keyStart, keyEnd);
                        
                    }
                    if (Parser.CheckValueSeparator(jsonString,position))
                    {
                        position++;
                        Parser.SkipWhitespace(jsonString, ref position);
                        
                    }
                }
                return obj;
            }

            JSONLogger.Error("Unexpected end of string");
            return null;
        }
        
        public override string ToString()
        {
            return ToString(0, true);
        }

        public string ToString(bool formatted)
        {
            return ToString(0, formatted);
        }

        public string ToString(int tabs, bool formatted)
        {
            var stringBuilder = new StringBuilder();
            string ptab = "";
            for (int i = 0; i < tabs; i++) ptab += JSONObject.Tab;
            string tab = "";
            for (int i = 0; i <= tabs; i++) tab += JSONObject.Tab;
            stringBuilder.Append(formatted ? "{\r\n" : "{");

            foreach (var pair in values)
            {
                stringBuilder.Append(tab);
                stringBuilder.Append("\"");
                stringBuilder.Append(pair.Key);
                stringBuilder.Append("\"");
                stringBuilder.Append(formatted ? ": " : ":");
                stringBuilder.Append(pair.Value.ToString(formatted ? (tabs + 1) : -1, formatted));
                stringBuilder.Append(formatted ? ",\r\n" : ",");
            }
            if (values.Count > 0)
            {
                stringBuilder.Remove(stringBuilder.Length - (formatted ? 3 : 1), 1);
            }
            stringBuilder.Append(ptab);
            stringBuilder.Append('}');
            return stringBuilder.ToString();
        }

        public IEnumerator<KeyValuePair<string, JsonValue>> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        public JSONObject Clone()
        {
            JSONObject result = new JSONObject();
            foreach (var keyValuePair in values)
            {
                result.Add(keyValuePair.Key, keyValuePair.Value.Clone());
            }
            return result;
        }
    }
}