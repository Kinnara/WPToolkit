// ---------------------------------------------------------------------------
// <copyright file="QueryExtensions.cs" company="Microsoft">
//     (c) Copyright Microsoft Corporation.
//     This source is subject to the Microsoft Public License (Ms-PL).
//     Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
//     All other rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace Microsoft.Phone.Maps.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a class that extends map queries.
    /// </summary>
    public static class QueryExtensions
    {
        /// <summary>
        /// Get MapLocations as an asynchronous operation.
        /// </summary>
        /// <param name="geocodeQuery">The <see cref="GeocodeQuery"/> the operation is performed on.</param>
        /// <returns>
        /// Returns <see cref="Task&lt;TResult&gt;"/>.
        /// The task object representing the asynchronous operation. 
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Want to use concrete type for extension method")]
        public static Task<IList<MapLocation>> GetMapLocationsAsync(this GeocodeQuery geocodeQuery)
        {
            return QueryAsync<IList<MapLocation>>(geocodeQuery);
        }

        /// <summary>
        /// Get MapLocations as an asynchronous operation.
        /// </summary>
        /// <param name="reverseGeocodeQuery">The <see cref="ReverseGeocodeQuery"/> the operation is performed on.</param>
        /// <returns>
        /// Returns <see cref="Task&lt;TResult&gt;"/>.
        /// The task object representing the asynchronous operation. 
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Want to use concrete type for extension method")]
        public static Task<IList<MapLocation>> GetMapLocationsAsync(this ReverseGeocodeQuery reverseGeocodeQuery)
        {
            return QueryAsync<IList<MapLocation>>(reverseGeocodeQuery);
        }

        /// <summary>
        /// Get Route as an asynchronous operation.
        /// </summary>
        /// <param name="routeQuery">The <see cref="RouteQuery"/> the operation is performed on.</param>
        /// <returns>
        /// Returns <see cref="Task&lt;TResult&gt;"/>.
        /// The task object representing the asynchronous operation. 
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Want to use concrete type for extension method")]
        public static Task<Route> GetRouteAsync(this RouteQuery routeQuery)
        {
            return QueryAsync<Route>(routeQuery);
        }

        /// <summary>
        /// Generic async task based execution of the queries
        /// </summary>
        /// <param name="query">Query to be executed using a task</param>
        /// <returns>Task client can await on</returns>
        /// <typeparam name="TResult">Type of the result</typeparam>
        private static Task<TResult> QueryAsync<TResult>(this Query<TResult> query)
        {
            TaskCompletionSource<TResult> taskCompletionSource;
            EventHandler<QueryCompletedEventArgs<TResult>> queryCompletedHandler = null;

            taskCompletionSource = CreateSource<TResult>(query);

            // Prepare event handler in case it is executed.
            queryCompletedHandler = (sender, e) =>
            {
                // In the event handler, let's transfer the completion of the task.
                // NOTE: the last lamba is the one that will unregister the event handler
                TransferCompletion(
                                   taskCompletionSource,
                                   e, 
                                   () => e.Result,
                                   () => query.QueryCompleted -= queryCompletedHandler);
            };

            query.QueryCompleted += queryCompletedHandler;

            try
            {
                query.QueryAsync();
            }
            catch
            {
                // Event handler was not executed, clenaup the event handler
                query.QueryCompleted -= queryCompletedHandler;
                throw;
            }

            return taskCompletionSource.Task;
        }

        /// <summary>
        /// Creates the TaskCompletionSource for the given state
        /// </summary>
        /// <typeparam name="TResult">Type of the returned result</typeparam>
        /// <param name="state">State to be provided to the TaskCompletionSource</param>
        /// <returns>A Task Completion Source</returns>
        private static TaskCompletionSource<TResult> CreateSource<TResult>(object state)
        {
            return new TaskCompletionSource<TResult>(state, TaskCreationOptions.None);
        }

        /// <summary>
        /// Transfer the execution of the completion of the async task
        /// </summary>
        /// <typeparam name="TResult">Type of the return type</typeparam>
        /// <param name="tcs">Task completion source used in the task</param>
        /// <param name="e">Event args from the async operation</param>
        /// <param name="getResult">Function that will be executed only if result is used</param>
        /// <param name="unregisterHandler">Action to be executed to unregister the Query.QueryCompleted event handler</param>
        private static void TransferCompletion<TResult>(TaskCompletionSource<TResult> tcs, AsyncCompletedEventArgs e, Func<TResult> getResult, Action unregisterHandler)
        {
            if (e.UserState == tcs.Task.AsyncState)
            {
                if (unregisterHandler != null)
                {
                    unregisterHandler();
                }

                // Map the result of the operation
                if (e.Cancelled)
                {
                    tcs.TrySetCanceled();
                }
                else if (e.Error != null)
                {
                    tcs.TrySetException(e.Error);
                }
                else
                {
                    tcs.TrySetResult(getResult());
                }
            }
        }
    }
}
