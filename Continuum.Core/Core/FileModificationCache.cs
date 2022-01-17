using Continuum.Core.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Continuum.Core
{
	[System.Serializable]
	public class FileModificationCache
	{
		public struct Tuple
		{
			public string file;
			public FileModification modification;
		}

		[JsonIgnore] bool cacheUpToDate = false;
		[JsonIgnore] Dictionary<string, FileModificationType> quickLookup = new Dictionary<string, FileModificationType>();
		[JsonProperty] Dictionary<string, List<FileModification>> modificationList = new Dictionary<string, List<FileModification>>();

		public FileModificationCache()
		{

		}

		public FileModificationCache(FileModificationCache oldCache)
		{
			if (oldCache == null)
				return;

			// Re-initialize the quick lookup cache
			foreach (var kvp in oldCache.modificationList)
			{
				foreach (var modification in kvp.Value)
				{
					AddModification(kvp.Key, modification);
				}
			}
		}

		/// <summary>
		/// Since this can be deserialized, we need to regenerate the cache potentially at any time
		/// </summary>
		private void RegenerateCache()
		{
			if (cacheUpToDate)
				return;

			foreach (var item in modificationList)
			{
				quickLookup[item.Key] = FileModificationType.None;

				foreach (var modification in item.Value)
					quickLookup[item.Key] |= modification.Type;
			}

			cacheUpToDate = true;
		}

		public void AddModification(string file, FileModification modification)
		{
			RegenerateCache();

			if (!quickLookup.ContainsKey(file))
			{
				quickLookup.Add(file, modification.Type);
				modificationList.Add(file, new List<FileModification>() { modification });
			}
			else
			{
				quickLookup[file] |= modification.Type;
				modificationList[file].Add(modification);
			}
		}

		public bool HasAnyModifications(string file)
		{
			RegenerateCache();

			if (quickLookup.TryGetValue(file, out FileModificationType modificationType))
				return modificationType != FileModificationType.None;

			return false;
		}

		public bool HasModification(string file, FileModificationType type)
		{
			RegenerateCache();

			if (quickLookup.TryGetValue(file, out FileModificationType modificationType))
				return (type & modificationType) == type;

			return false;
		}

		public bool HasModification(string file, FileModificationType type, out FileModification modification)
		{
			RegenerateCache();

			if (quickLookup.TryGetValue(file, out FileModificationType modificationType))
			{
				modification = modificationList[file].LastOrDefault(m => m.Type == type);
				return (type & modificationType) == type;
			}

			modification = new FileModification();
			return false;
		}

		public IEnumerable<Tuple> GetModifications(string modID = null, FileModificationType? type = null, bool? reserved = null)
		{
			foreach (var kvp in modificationList)
			{
				foreach (var modification in kvp.Value)
				{
					if ((type == null || (type.Value & modification.Type) == modification.Type) && (reserved == null || modification.ReservedFile == reserved.Value) && (modID == null || modification.ModID.Equals(modID, System.StringComparison.InvariantCultureIgnoreCase)))
						yield return new Tuple() { file = kvp.Key, modification = modification };
				}
			}
		}
	}
}

