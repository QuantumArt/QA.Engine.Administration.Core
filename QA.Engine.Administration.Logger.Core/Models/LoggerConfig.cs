using NLog.Layouts;
using System;
using System.Collections.Generic;
using System.Text;

namespace QA.Engine.Administration.Logger.Core.Models
{
    /// <summary>
    /// Конфигурация лога
    /// </summary>
    public class LoggerConfig
    {
        /// <summary>
        /// Дополнительные поля для лога. Например:
        /// new []
        ///   {
        ///     new JsonField("field_1",
        ///       new JsonLayoutField(new []
        ///       {
        ///         new JsonField("field_1_1", "value_1_1"),
        ///         new JsonField("field_1_2",
        ///           new JsonLayoutField(new []
        ///           {
        ///             new JsonField("field_1_2_1", "value_1_2_1"),
        ///           })),
        ///       })
        ///     ),
        ///     new JsonField("field_2", "value_2")
        ///   }
        /// </summary>
        public IEnumerable<JsonField> LogFields { get; set; }
        /// <summary>
        /// Можна задать через appsettings.json по пути "JsonLogging:LogLevel".
        /// Default=Info
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Info;

        public bool IgnoreMicrosoftLogs { get; set; } = true;
        public bool IgnoreSystemNetLogs { get; set; } = true;

        public string ServiceName { get; set; } = "admin.webapp";
        public string ServiceVersion { get; set; } = "0.0.0";

        public bool UseFileLogger { get; set; } = false;
        public string LogFilePath { get; set; } = "c:\\temp\\logs\\${iis-site-name}";

        /// <summary>
        /// Дефолтные поля лога
        /// </summary>
        /// <returns></returns>
        internal static List<JsonAttribute> GetDefualtLayout()
        {
            return new List<JsonAttribute>
            {
                new JsonAttribute("@timestamp", "${date:format=o:universalTime=true}"),
                new JsonAttribute("eid", "${event-properties:item=EventId_Id}"),
                new JsonAttribute("level", "${uppercase:${level}}"),
                new JsonAttribute("logger", "${logger}"),
                new JsonAttribute("message", "${message}"),
                new JsonAttribute("emitter", new JsonLayout
                {
                    Attributes =
                    {
                        new JsonAttribute("appname", $"${{var:{nameof(ServiceName)}}}"),
                        new JsonAttribute("version", $"${{var:{nameof(ServiceVersion)}}}"),
                    }
                }, false),
                new JsonAttribute("exeption", "${exception:format=tostring}"),
            };
        }
    }
}
