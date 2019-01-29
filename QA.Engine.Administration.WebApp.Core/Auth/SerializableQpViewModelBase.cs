using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace QA.Engine.Administration.WebApp.Core.Auth
{
    /// <summary>
    /// Базовый класс модели представления при использовании Qp
    /// </summary>
    [DataContract]
    public class SerializableQpViewModelBase
    {
        /// <summary>
        /// Идентификатор хоста
        /// </summary>
        [DataMember]
        public string HostId { get; set; }

        /// <summary>
        /// Идентификатор бэкенда
        /// </summary>
        [DataMember]
        public string BackendSid { get; set; }

        /// <summary>
        /// Код заказчика
        /// </summary>
        [DataMember]
        public string CustomerCode { get; set; }

        /// <summary>
        /// Ключ
        /// </summary>
        [IgnoreDataMember]
        public string QpKey
        {
            get
            {
                return (CustomerCode ?? string.Empty) + "_" + (SiteId ?? string.Empty);
            }
        }

        /// <summary>
        /// Идентификатор сайта
        /// </summary>
        [DataMember]
        public string SiteId { get; set; }

        /// <summary>
        /// Сериализация JSON в объект
        /// </summary>
        /// <param name="json">Строка для десериализации</param>
        public static SerializableQpViewModelBase FromJsonString(string json)
        {
            using (var strm = new MemoryStream())
            {
                using (var sw = new StreamWriter(strm))
                {
                    var serializer = new DataContractJsonSerializer(typeof(SerializableQpViewModelBase));
                    sw.Write(json);
                    sw.Flush();
                    strm.Position = 0;
                    var obj = serializer.ReadObject(strm);
                    return (SerializableQpViewModelBase)obj;
                }
            }
        }

        public static SerializableQpViewModelBase FromJsonString(Stream stream)
        {
            string data = null;
            using (var sr = new StreamReader(stream))
                data = sr.ReadToEnd();

            using (var strm = new MemoryStream())
            {
                using (var sw = new StreamWriter(strm))
                {
                    var serializer = new DataContractJsonSerializer(typeof(SerializableQpViewModelBase));
                    sw.Write(data);
                    sw.Flush();
                    strm.Position = 0;
                    var obj = serializer.ReadObject(strm);
                    return (SerializableQpViewModelBase)obj;
                }
            }
        }
    }
}
