using Continuum.Core.InstallActions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Continuum.Core
{
    public class ModInstallActionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(ModInstallAction).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);

            string actionType = (string)jObject["Action"] ?? string.Empty;

            ModInstallAction item = null;
            if (actionType.Equals("MoveFile", StringComparison.InvariantCultureIgnoreCase))
                item = new MoveFileAction();
            else if (actionType.Equals("MoveFiles", StringComparison.InvariantCultureIgnoreCase))
                item = new MoveFilesAction();
            else if (actionType.Equals("DeleteFiles", StringComparison.InvariantCultureIgnoreCase))
                item = new DeleteFilesAction();
            else if (actionType.Equals("ReplaceFile", StringComparison.InvariantCultureIgnoreCase))
                item = new ReplaceFileAction();
            else if (actionType.Equals("ReplaceFiles", StringComparison.InvariantCultureIgnoreCase))
                item = new ReplaceFilesAction();
            else if (actionType.Equals("CopyFile", StringComparison.InvariantCultureIgnoreCase))
                item = new CopyFileAction();
            else if (actionType.Equals("CopyFiles", StringComparison.InvariantCultureIgnoreCase))
                item = new CopyFilesAction();
            else if (actionType.Equals("WriteToFile", StringComparison.InvariantCultureIgnoreCase))
                item = new WriteToFileAction();
            else if (actionType.Equals("QuickBMSExtract", StringComparison.InvariantCultureIgnoreCase))
                item = new QuickBMSExtractAction();
            else if (actionType.Equals("UnluacDecompile", StringComparison.InvariantCultureIgnoreCase))
                item = new UnluacDecompileAction();
            else if (actionType.Equals("ZipFiles", StringComparison.InvariantCultureIgnoreCase))
                item = new ZipFilesAction();
            else if (actionType.Equals("ZipDirectory", StringComparison.InvariantCultureIgnoreCase))
                item = new ZipDirectoryAction();
            else if (actionType.Equals("UnzipFile", StringComparison.InvariantCultureIgnoreCase))
                item = new UnzipFileAction();
            else if (actionType.Equals("UnzipFiles", StringComparison.InvariantCultureIgnoreCase))
                item = new UnzipFilesAction();
            else
                item = new GenericInstallAction();

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
