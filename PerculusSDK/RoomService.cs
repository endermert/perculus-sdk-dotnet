using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using PerculusSDK.Rooms;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace PerculusSDK
{
    /// <summary>
    /// RoomService is a wrapper class which includes helper methods to use Perculus Rooms API. 
    /// This class uses Rooms Web Service API of Perculus v2.8.
    /// </summary>
    public class RoomService
    {
        private Rooms.RoomsSoapClient _roomsApi = null;
        private Rooms.AuthHeader _authHeader = null;
        private Dictionary<string, string> _vcParameters = new Dictionary<string, string>();
        public Uri ApiUri
        {
            get
            {
                UriBuilder uriBuilder = new UriBuilder(PerculusBaseUri);
                uriBuilder.Path = "api/v2/rooms.asmx";
                return uriBuilder.Uri;
            }
        }
        public Uri PerculusBaseUri { get; set; }

        private string _lastError;

        private string getParameter(string key)
        {
            if (key == "AttendanceURLFormat")
            {
                UriBuilder uriBuilder = new UriBuilder(PerculusBaseUri);
                uriBuilder.Path = "perculus.aspx?c={0}";
                return uriBuilder.Uri.ToString();
            }
            else if (key == "DefaultLayout")
            {
                return "elearning";
            }
            return "";
        }

        private RoomsSoapClient getRoomsApi()
        {
            if (_roomsApi == null)
            {
                _roomsApi = new RoomsSoapClient(new BasicHttpBinding(), new EndpointAddress(ApiUri.ToString()));
            }

            return _roomsApi;
        }

        private AuthHeader getAuthHeader()
        {
            return _authHeader;
        }


        private Rooms.Room createOrUpdateRoomOnPerculus(Room room, params ModuleSetting[] moduleSettings)
        {
            WSResponse response = new WSResponse();

            var pws = getRoomsApi();

            if (room.ROOMID > 0)
            {
                response = pws.GetRoom(getAuthHeader(), room.ROOMID);
                if (response.success)
                {
                    room = (Room)response.data;
                }
                else
                {
                    _lastError = response.code.ToString();
                    return null;
                }
            }
            else
            {
                // new room
                // set defaults
                room.ADDDATE = DateTime.Now;

                room.PASSKEY = "";
                room.REQUIRESLOGIN = false; // pass key  olacağı için login olmasına gerek yok
                room.SEND_ICS = false;
                room.SEND_PASSKEY = false;
                room.FLASHFILE = ""; // standart sürüm
                room.FORMENROLL_ALLOW = false;
                room.FORMENROLL_CAPACITY = 0;
                room.FORMENROLL_TYPE = 0;
                room.INREPLAYMODE = false;
                room.STREAMCOUNT = 0;
                room.USERRIGHTSCHEMA = "default";
            }

            string permittedLayoutStr = "elearning"; //elearning,meeting
            string[] permittedLayouts = new string[] { };
            permittedLayouts = permittedLayoutStr.Split(',').Select(s => s.Trim()).ToArray();

            try
            {
                response = pws.AddRoom(getAuthHeader(), room.ROOMID
                                                    , room.SESSIONNAME, room.DESCRIPTION, room.BEGINDATE,
                                                    room.DURATION,
                                                    room.CATEGORY,
                                                    room.COLORCODE,
                                                    room.LANGUAGEFILE,
                                                    room.STREAMCOUNT, "", room.STARTACTIVE
                                                    , room.FORMENROLL_ALLOW, room.FORMENROLL_CAPACITY
                                                    , room.FORMENROLL_TYPE, room.SEND_PASSKEY
                                                    , room.PASSKEY, room.SEND_ICS, room.USERRIGHTSCHEMA, getParameter("DefaultLayout"),
                                                    permittedLayouts, moduleSettings);
                if (!response.success)
                {
                    _lastError = response.code.ToString();
                    return null;
                }

            }
            catch (Exception ex)
            {
                response.success = false;
                _lastError = ex.ToString();
                return null;
            }

            if (response.data != null)
            {
                room.ROOMID = ((Room)response.data).ROOMID;
            }
            else
            {
                _lastError = "Web service response data is null.";
                return null;
            }

            return room;
        }

        public bool Initialized { get; private set; }
        public bool Initialize(Uri perculusBaseUri, string apiUsername, string apiPassword)
        {
            if (Initialized)
            {
                _lastError = "Service is already initialized.";
                return false;
            }
            _authHeader = new AuthHeader()
            {
                Username = apiUsername,
                Password = apiPassword
            };

            PerculusBaseUri = perculusBaseUri;
            return true;
        }

        public string GetLastError()
        {
            return _lastError ?? "";
        }

        public Room Save(Room room, params ModuleSetting[] moduleSettings)
        {
            return createOrUpdateRoomOnPerculus(room, moduleSettings);
        }

        public bool Delete(int roomId)
        {
            var roomsApi = getRoomsApi();
            WSResponse response = roomsApi.Delete(getAuthHeader(), roomId);
            if (!response.success)
            {
                _lastError = response.code.ToString();
            }
            return response.success;
        }

        public string RegisterAttendee(Participant participant, int roomId)
        {
            var roomsApi = getRoomsApi();

            var response = roomsApi.RegisterExternalAttendee(getAuthHeader(), roomId, participant.FirstName, participant.LastName, participant.Email, participant.Role);
            if (response.success)
            {
                return response.data.ToString();
            }
            else
            {
                _lastError = response.code.ToString();
            }
            return "";
        }

        /// <summary>
        /// Returns empty string if there is an error.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="role"></param>
        /// <param name="virtualClassRemoteId"></param>
        /// <returns></returns>
        public string RegisterAndGetURL(Participant participant, int roomId)
        {
            string attendanceURL = "";
            var roomsApi = getRoomsApi();

            var response = roomsApi.RegisterExternalAttendee(getAuthHeader(), roomId, participant.FirstName, participant.LastName, participant.Email, participant.Role);
            if (response.success)
            {
                string attendCode = response.data.ToString();
                attendanceURL = string.Format(getParameter("AttendanceURLFormat"), attendCode);
            }
            else
            {
                _lastError = response.code.ToString();
            }
            return attendanceURL;
        }

        public bool RegisterAttendees(Participant[] participants, string participantRole, int roomId)
        {
            var roomsApi = getRoomsApi();

            System.Text.StringBuilder externalAttendeesString = new StringBuilder();
            for (int i = 0; i < participants.Length; i++)
            {
                var u = participants[i];
                externalAttendeesString.AppendFormat("{0};{1};{2};{3}", u.FirstName, u.LastName, u.Email, u.Role);
                if (i < participants.Length - 1) externalAttendeesString.Append("\\n");
            }
            var response = roomsApi.RegisterAttendees(getAuthHeader(), roomId, "", externalAttendeesString.ToString(), "");
            if (!response.success)
            {
                _lastError = response.code.ToString();
            }
            return response.success;
        }

        public bool UnRegisterAttendees(Participant[] users, int roomId)
        {
            throw new NotImplementedException();
        }

        public bool UnRegisterAttendee(Participant user, int roomId)
        {
            throw new NotImplementedException();
        }

        public bool AddFile(string localFilePath, int roomId)
        {
            var roomsApi = getRoomsApi();
            string fileId = null;
            int maxFileSize = 10000000; // bytes.. 10 MB
            System.IO.FileInfo fi = new System.IO.FileInfo(localFilePath);
            if (!fi.Exists)
            {
                _lastError = "File doesn't exist.";
            }
            else if (fi.Length > maxFileSize)
            {
                _lastError = "File size cannot be greater than " + maxFileSize + " bytes.";
            }
            else
            {
                string fileName = fi.Name;
                byte[] fileContent = new byte[fi.Length];
                System.IO.Stream str = fi.OpenRead();
                str.Read(fileContent, 0, Convert.ToInt32(fi.Length));
                string fileData = Convert.ToBase64String(fileContent);
                var result = roomsApi.PostFile(getAuthHeader(), roomId, 0, fileName, fileData);
                if (result.success && result.data != null) fileId = result.data.ToString();
                else _lastError = result.code.ToString();
                str.Close();
                str.Dispose();
            }

            return fileId != null;
        }

        public bool RemoveFile(string fileId)
        {
            throw new NotImplementedException();
        }

        public bool IsRegistered(Participant user, int roomId)
        {
            string email = user.Email;
            var roomsApi = getRoomsApi();
            var response = roomsApi.GetAttendCodeWithEmail(getAuthHeader(), roomId, email);
            return response.success;
        }

        public string[] GetRegisteredSessions(Participant user)
        {
            throw new NotImplementedException();
        }

        public string[] GetSupportedFileFormats()
        {
            return new string[] { ".doc", ".docx", ".ppt", ".pptx", ".pdf", ".txt", ".jpeg", ".jpg", ".png", ".gif", ".xls", ".xlsx", ".rtf" };
        }

        public string GetAttendanceURL(Participant participant, int roomId)
        {
            string attendanceURL = "";

            var roomsApi = getRoomsApi();
            var response = roomsApi.GetAttendCodeWithEmail(getAuthHeader(), roomId, participant.Email);
            if (response.success)
            {
                string attendCode = response.data as string;
                attendanceURL = string.Format(getParameter("AttendanceURLFormat"), attendCode);
            }
            return attendanceURL;
        }

        public bool PublishReplay(int roomId)
        {
            var roomsApi = getRoomsApi();
            var response = roomsApi.PackRoom(getAuthHeader(), roomId);

            if (!response.success)
            {
                _lastError = response.code.ToString();
            }
            return response.success;
        }

    }
}
