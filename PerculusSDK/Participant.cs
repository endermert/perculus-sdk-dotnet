using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerculusSDK
{
    public class Participant
    {
        #region roles
        public const string PARTICIPANT_USER = "u";
        public const string PARTICIPANT_ADMIN = "a";
        public const string PARTICIPANT_INSTRUCTOR = "e";
        #endregion

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
    }
}
