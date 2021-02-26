﻿using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace DAM2.Core.Shared.Extensions
{
    public static class JsonExtensions
    {
        public static JObject ReplacePath<T>(this JToken root, string path, T newValue)
        {
            if (root == null || path == null)
                throw new ArgumentNullException();

            foreach (var value in root.SelectTokens(path).ToList())
            {
                if (value == root)
                    root = JToken.FromObject(newValue);
                else
                    value.Replace(JToken.FromObject(newValue));
            }

            return (JObject)root;
        }

        public static string ReplacePath<T>(string jsonString, string path, T newValue)
        {
            return JToken.Parse(jsonString).ReplacePath(path, newValue).ToString();
        }
    }
}
