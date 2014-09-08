using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace NHibernate.OData
{
    [DebuggerStepThrough]
    internal static class Require
    {
        [AssertionMethod]
        public static void NotNull([AssertionCondition(AssertionConditionType.IS_NOT_NULL)] object param, [InvokerParameterName] string paramName)
        {
            if (param == null)
                throw new ArgumentNullException(paramName);
        }

        [AssertionMethod]
        public static void NotEmpty([AssertionCondition(AssertionConditionType.IS_NOT_NULL)] string param, [InvokerParameterName] string paramName)
        {
            if (String.IsNullOrEmpty(param))
                throw new ArgumentException("Value cannot be null or empty.", paramName);
        }

        [AssertionMethod]
        public static void ValidEnum<T>(T param, [InvokerParameterName] string paramName)
        {
            if (!Enum.IsDefined(typeof(T), param))
                throw new ArgumentOutOfRangeException(paramName);
        }

        [AssertionMethod]
        public static void That([AssertionCondition(AssertionConditionType.IS_TRUE)] bool condition, string message)
        {
            if (!condition)
                throw new ArgumentException(message);
        }

        [AssertionMethod]
        public static void That([AssertionCondition(AssertionConditionType.IS_TRUE)] bool condition, string message, string paramName)
        {
            if (!condition)
                throw new ArgumentException(message, paramName);
        }
    }
}
