using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FigSpec.SortingExpert
{
    public class JsonHelp
    {
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <returns></returns>
        public static T DeserializeObject<T>(string content)where T:class, new()
        {
            try
            {
                if (string.IsNullOrEmpty(content))
                {
                    return null;
                }
                return JsonConvert.DeserializeObject<T>(content);
            }
            catch (Exception ex)
            {
                //throw new Exception(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string SerializeObject<T>(T t) where T : class, new()
        {
            try
            {
                if (t == null)
                {
                    return string.Empty;
                }
                return JsonConvert.SerializeObject(t);
            }
            catch (Exception ex)
            {
                //throw new Exception(ex.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string SerializeObject(object o)
        {
            try
            {
                if (o == null)
                {
                    return string.Empty;
                }
                return JsonConvert.SerializeObject(o);
            }
            catch (Exception ex)
            {
                //throw new Exception(ex.Message);
                return string.Empty;
            }
        }
    }
}
