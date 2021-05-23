using System;
using System.Collections.Generic;
using System.Text;

namespace TCache.Tests
{
    public class SomeDataKey : IEquatable<SomeDataKey>
    {
        public string firstName;
        public string lastName;
        public DateTime dateOfBirth;

        public bool Equals(SomeDataKey rhs)
        {
            if ((this.firstName == rhs.firstName) &&
                (this.lastName == rhs.lastName) &&
                    (this.dateOfBirth == rhs.dateOfBirth))
            {
                return true;
            }
            return false;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return this.Equals((SomeDataKey)obj);
        }

        // override object.GetHashCode
        public override int GetHashCode() => HashCode.Combine(firstName, lastName, dateOfBirth);
    }
}
