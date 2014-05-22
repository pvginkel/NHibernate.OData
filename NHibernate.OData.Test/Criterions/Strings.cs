using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Criterion;
using NHibernate.Exceptions;
using NHibernate.OData.Test.Domain;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Criterions
{
    [TestFixture]
    internal class Strings : DomainTestFixture
    {
        [Test]
        public void Concat()
        {
            Verify<Parent>("concat(Name) eq 'Parent 1'", q => q.Where(p => p.Int32 == 1));
            Verify<Parent>("concat(Name, 'x') eq 'Parent 1x'", q => q.Where(p => p.Int32 == 1));
            Verify<Parent>("concat(Name, LengthString) eq 'Parent 1A'", q => q.Where(p => p.Int32 == 1));
        }

        [Test]
        public void SubString()
        {
            Verify<Parent>("substring(LengthString, 3) eq 'CDE'", q => q.Where(p => p.Int32 == 5));
            Verify<Parent>("substring(LengthString, 3, 3) eq 'CDE'", q => q.Where(p => p.Int32 >= 5));
            Verify<Parent>("substring(LengthString, Int32) eq 'C'", q => q.Where(p => p.Int32 == 3));
        }

        [Test]
        [ExpectedException(typeof(GenericADOException))] // SQLite does not support locate
        public void IndexOf()
        {
            Verify<Parent>("indexof(LengthString, 'E') eq 5", q => q.Where(p => p.Int32 >= 5));
            VerifyThrows<Parent>("indexof(LengthString, Name)");
        }

        [Test]
        public void SubStringOf()
        {
            Verify<Parent>("substringof('CDE', LengthString)", q => q.Where(p => p.LengthString.IsLike("CDE", MatchMode.Anywhere)));
            VerifyThrows<Parent>("substringof(Name, LengthString)");
        }

        [Test]
        public void StartsWith()
        {
            Verify<Parent>("startswith(LengthString, 'ABCDE')", q => q.Where(p => p.LengthString.IsLike("ABCDE", MatchMode.Start)));
            VerifyThrows<Parent>("startswith(LengthString, Name)");
        }

        [Test]
        public void EndsWith()
        {
            Verify<Parent>("endswith(LengthString, 'CDE')", q => q.Where(p => p.LengthString.IsLike("CDE", MatchMode.End)));
            VerifyThrows<Parent>("endswith(LengthString, Name)");
        }

        [Test]
        public void SubStringOfAndEndsWith()
        {
            Verify<Parent>("substringof('CDE', LengthString) and endswith(LengthString, 'G')", q => q.Where(p => p.LengthString.IsLike("CDE", MatchMode.Anywhere) && p.LengthString.IsLike("G", MatchMode.End)));
        }

        [Test]
        public void Length()
        {
            Verify<Parent>("length(LengthString) gt 5", q => q.Where(p => p.Int32 > 5));
        }

        [Test]
        public void Replace()
        {
            Verify<Parent>("replace(LengthString, 'A', 'X') eq 'XB'", q => q.Where(p => p.Int32 == 2));
        }

        [Test]
        public void ToUpper()
        {
            Verify<Parent>("toupper(Name) eq 'PARENT 1'", q => q.Where(p => p.Int32 == 1));
            VerifyEmpty<Parent>("toupper(Name) eq 'parent 1'");
        }

        [Test]
        public void ToLower()
        {
            Verify<Parent>("tolower(Name) eq 'parent 1'", q => q.Where(p => p.Int32 == 1));
            VerifyEmpty<Parent>("tolower(Name) eq 'PARENT 1'");
        }

        [Test]
        public void Trim()
        {
            Session.Save(new Parent
            {
                Name = " Leading space",
                Int32 = 21
            });

            Session.Save(new Parent
            {
                Name = "Trailing space ",
                Int32 = 22
            });

            Verify<Parent>("trim(Name) eq 'Leading space'", q => q.Where(p => p.Int32 == 21));
            Verify<Parent>("trim(Name) eq 'Trailing space'", q => q.Where(p => p.Int32 == 22));
        }
    }
}
