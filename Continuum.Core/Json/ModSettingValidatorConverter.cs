using Continuum.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Continuum.Core
{
    public class ModSettingValidatorConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(ModSettingValidator).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);

            string actionType = (string)jObject["ValidationType"] ?? string.Empty;

            ModSettingValidator item = null;
            if (actionType.Equals("PathExists", StringComparison.InvariantCultureIgnoreCase))
                item = new PathExistsValidator();
            else if (actionType.Equals("Regex", StringComparison.InvariantCultureIgnoreCase))
                item = new RegexValidator();
            else if (actionType.Equals("MandatoryField", StringComparison.InvariantCultureIgnoreCase))
                item = new MandatoryFieldValidator();
            else
                item = new GenericModSettingValidator();

            serializer.Populate(jObject.CreateReader(), item);

            return item;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer,
            object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
