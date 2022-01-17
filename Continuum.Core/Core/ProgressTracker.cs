using System;

namespace Continuum.Core
{
	public class ProgressTracker
	{
		public float PercentageComplete => totalActions == 0 ? 0 : totalActions / (float)completedActions;
		public int TotalActions => totalActions;
		public int CompletedActions => completedActions;
		public string Context => context;

		int totalActions;
		int completedActions;
		string context;

		object lockObj = new Object();

		public void ResetState(int totalActions)
		{
			this.totalActions = totalActions;
			this.completedActions = 0;
			this.context = "Loading...";
		}

		public void UpdateContext(string context)
		{
			this.context = context;
		}

		public void CompleteAction()
		{
			lock (lockObj)
			{
				if (completedActions >= totalActions)
				{
					throw new Exception("Cannot complete more actions than there are actions available");
				}

				completedActions = completedActions + 1;
			}
		}
	}
}
