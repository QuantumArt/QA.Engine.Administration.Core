namespace QA.Engine.Administration.Logger.Core.Models
{
    /// <summary>
    /// Поле лога
    /// </summary>
    public class JsonField
    {
        public JsonField(string name, string value)
        {
            Name = name;
            Value = value;
        }
        public JsonField(string name, JsonLayoutField layout)
        {
            Name = name;
            Layout = layout;
        }
        /// <summary>
        /// Имя поля
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Значение поля
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Значение поля - контейнер вложенных полей (объект json)
        /// </summary>
        public JsonLayoutField Layout { get; set; }
    }
}
