using Microsoft.AspNetCore.Components;
using System.Collections.Specialized;
using System.Web;
using System;
using System.Threading.Tasks;

namespace Continuum.GUI.Extensions
{
    public static class AsyncTaskExtensions
    {
        public static Task InvokeSafe(this Func<Task> taskDelegate)
        {
            if (taskDelegate == null)
                return Task.CompletedTask;

            return taskDelegate.Invoke();
        }

        public static Task InvokeSafe<T>(this Func<T, Task> taskDelegate, T parameter)
        {
            if (taskDelegate == null)
                return Task.CompletedTask;

            return taskDelegate.Invoke(parameter);
        }

        public static async Task RunWithMinimumDelay(this Task task, int milliseconDelay)
        {
            var delayTask = Task.Delay(milliseconDelay); // Fire and forget

            await task;
            await delayTask;
        }
    }
}