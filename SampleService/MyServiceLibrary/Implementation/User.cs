using System;
using MyServiceLibrary.Helpers;

namespace MyServiceLibrary.Implementation
{
    /// <summary>
    /// User entity
    /// </summary>
    [Serializable]
    public class User
    {
        /// <summary>
        /// User's identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User's first name
        /// </summary>
        [CheckDefaultValue]
        public string FirstName { get; set; }

        /// <summary>
        /// User's last name
        /// </summary>
        [CheckDefaultValue]
        public string LastName { get; set; }

        /// <summary>
        /// User's date of birth
        /// </summary>
        [CheckDefaultValue]
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        /// Operator compares users by value
        /// </summary>
        /// <param name="a">User a</param>
        /// <param name="b">User b</param>
        /// <returns>True if users are equal</returns>
        public static bool operator ==(User a, User b)
        {
            if (object.ReferenceEquals(a, b))
            {
                return true;
            }

            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return (a.FirstName == b.FirstName && a.LastName == b.LastName) && a.DateOfBirth.Equals(b.DateOfBirth);
        }

        /// <summary>
        /// Operator compares users by value
        /// </summary>
        /// <param name="x">User a</param>
        /// <param name="y">User b</param>
        /// <returns>True if users aren't equal</returns>
        public static bool operator !=(User x, User y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Method compares this and entered user by value
        /// </summary>
        /// <param name="obj">second user</param>
        /// <returns>True if users are equal</returns>
        public override bool Equals(object obj)
        {
            return obj is User && this == (User)obj;
        }

        /// <summary>
        /// Method returns unique hash number for current user
        /// </summary>
        /// <returns>Hash number</returns>
        public override int GetHashCode()
        {
            return (this.FirstName.GetHashCode() * this.LastName.Length) - this.DateOfBirth.GetHashCode();
        }

        /// <summary>
        /// Methods convert user to string and returns it
        /// </summary>
        /// <returns>result string</returns>
        public override string ToString()
        {
            return $"{this.LastName} {this.FirstName} {this.DateOfBirth.ToString("dd.MM.yyyy")}";
        }
    }
}
