﻿using System;
using System.Runtime.CompilerServices;
using Allure.Commons;
using NUnit.Framework.Internal;

namespace NUnit.Allure.Core
{
    internal class SetUpTearDownHelper
    {
        public string CustomName { get; set; }
        public string MethodName { get; set; }
        public long StartTime { get; set; }
        public long EndTime { get; set; }
        public Exception Exception { get; set; }

        public override string ToString()
        {
            return MethodName;
        }
    }

    public static class AllureExtensions
    {
        internal static long ToUnixTimeMilliseconds(this DateTimeOffset dateTimeOffset)
        {
            return (long) dateTimeOffset.UtcDateTime.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        /// <summary>
        /// Ability to measure time of execution of [SetUp], [TearDown], [OneTimeSetUp] NUnit attributes for Allure reports
        /// </summary>
        public static void WrapSetUpTearDownParams(Action action, string customName = "",
            [CallerMemberName] string callerName = "")
        {
            var setUpTearDownHelper = new SetUpTearDownHelper {MethodName = callerName};
            if (!string.IsNullOrEmpty(customName)) setUpTearDownHelper.CustomName = customName;
            try
            {
                setUpTearDownHelper.StartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                action.Invoke();
            }
            catch (Exception e)
            {
                setUpTearDownHelper.Exception = e;
                throw;
            }
            finally
            {
                setUpTearDownHelper.EndTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                TestExecutionContext.CurrentContext.CurrentTest.Properties.Add(callerName, setUpTearDownHelper);
            }
        }

        /// <summary>
        /// Wraps method into AllureStep.
        /// </summary>
        public static void WrapInStep(this AllureLifecycle lifecycle, Action action, string stepName = "", [CallerMemberName] string callerName = "")
        {
            if (string.IsNullOrEmpty(stepName)) stepName = callerName;

            var id = Guid.NewGuid().ToString();
            var stepResult = new StepResult {name = stepName};
            try
            {
                lifecycle.StartStep(id, stepResult);
                action.Invoke();
                lifecycle.StopStep(step => stepResult.status = Status.passed);
            }
            catch (Exception e)
            {
                lifecycle.StopStep(step =>
                {
                    step.statusDetails = new StatusDetails
                    {
                        message = e.Message,
                        trace = e.StackTrace
                    };
                    step.status = AllureNUnitHelper.GetNUnitStatus();
                });
                throw;
            }
        }
    }
}