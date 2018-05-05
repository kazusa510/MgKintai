using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Kintai.Models.Session
{
    public static class SessionExtention
    {
        public static void SetObject<T>(this ISession session, string key, T obj) where T : class
        {
            if (obj == null) return;
            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);
                stream.Seek(0, SeekOrigin.Begin);
                session.Set(key, stream.ToArray());
            }
        }

        public static T GetObject<T>(this ISession session, string key) where T : class
        {
            if (session.TryGetValue(key, out byte[] value))
            {
                var formatter = new BinaryFormatter();
                using (var stream = new MemoryStream())
                {
                    stream.Write(value, 0, value.Length);
                    stream.Seek(0, SeekOrigin.Begin);
                    return formatter.Deserialize(stream) as T;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
