using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal static class OperatorUtil
    {
        public static bool IsLogical(Operator @operator)
        {
            switch (@operator)
            {
                case Operator.And:
                case Operator.Or:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsCompare(Operator @operator)
        {
            switch (@operator)
            {
                case Operator.Eq:
                case Operator.Ge:
                case Operator.Gt:
                case Operator.Le:
                case Operator.Lt:
                case Operator.Ne:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsArithmic(Operator @operator)
        {
            switch (@operator)
            {
                case Operator.Add:
                case Operator.Div:
                case Operator.Mod:
                case Operator.Mul:
                case Operator.Sub:
                    return true;

                default:
                    return false;
            }
        }

        internal static bool IsBinary(Operator @operator)
        {
            return @operator != Operator.Not;
        }
    }
}
