Perculus Software Development Kit
=================================
<a href="http://www.perculus.com" target="_blank">Perculus</a> is a virtual classroom system developed by <a href="http://www.advancity.com.tr" target="_blank">Advancity</a>.

Perculus SDK v0.1 is a class library that is built on top of Perculus Web Service API.
The programming language is C#.
Its aim is to provide some good samples about using Perculus APIs.

There are 3 Web Service API's as follows:
- Rooms API <em>(http://&lt;perculus domain&gt;/api/v2/rooms.asmx?WSDL)</em>
- Users API (http://&lt;perculus domain&gt;/api/v2/users.asmx?WSDL)
- Reports API (http://&lt;perculus domain&gt;/api/v2/reports.asmx?WSDL)

The API functions return a generic response (WSResponse). This response includes the following:
- success: A boolean value that denotes whether the operation is sucessful or not.
- code: If the operation is not successful, check the code which is an integer value.
- data: This is the data object that carries the operation result. If you call GetRoom(roomId) you'll get Room object in data. So, depending on the operation you need to cast data property to the appropriate type (this is how we do it in C#).
- xmlData: xmlData's type is XmlNode (C#). It basically carries xml data if the operation returns xml data with it. Some functions use data and some of them use xmlData. If a single object is returned, data is preferred whereas xmlData is preferred in case of returning a report result.

In every API call, you need to pass a AuthType object which is used to pass API credentials (username and password). 
In every method, you'll see that the first parameter is AuthHeader variable.

	PerculusSDK.Rooms.RoomsSoapClient roomsSoapClient = new PerculusSDK.Rooms.RoomsSoapClient(new BasicHttpBinding(), new EndpointAddress("http://<perculus domain>/api/v2/rooms.asmx"));
	PerculusSDK.Rooms.AuthHeader authHeader = new PerculusSDK.Rooms.AuthHeader() { Username = "<your-api-username>", Password = "<your-api-password>" };
	PerculusSDK.Rooms.WSResponse response = roomsSoapClient.AddRoom(authHeader, 0, "room name", "desc", DateTime.Now, 60, "elearning", "#cccccc", "tr-TR", 0, "", true, false, 0, 0, false, "", false, "default", "elearning", null, getModuleSettings());
   
You can also use RoomService class in the PerculusSDK to get a feeling that you are not using a web service at all. The following code is really easy to implement:

	PerculusSDK.RoomService roomService = new PerculusSDK.RoomService();
	roomService.Initialize(new Uri(PERCULUS_BASE_URI), "<your-api-username>", "<your-api-password>");
	
	var room = roomService.Save(new PerculusSDK.Rooms.Room()
	{
		BEGINDATE = DateTime.Now,
		SESSIONNAME = "Test Room",
		DESCRIPTION = "Sample Description",
		DURATION = 60,
		CATEGORY = "elearning",
		COLORCODE = "#cccccc", // gray background
		LANGUAGEFILE = "tr-TR" // english: en-US, turkish: tr-TR
	}, getModuleSettings());
   
Please look at UnitTests.cs in PerculusSDKUnitTests project to see the whole example.

   
The codes that web service methods may return are as follows:

 - NONE = 0
 - AUTHENTICATION_FAILURE=101
 - UNAUTHORIZED_IP_ADDRESS = 102
 - FUNCTION_NOT_ALLOWED = 103
 - SYSTEM_OFFLINE_LICENSE_CONFLICT = 104
 - NOT_AN_AUTHENTIC_REQUEST = 105
 - INVALID_PARAMETER = 106
 - MISSING_PARAMETER = 107
 - ERR_LICENSE_CONFLICT = 108
 - USERS_ADDED_PARTIALLY_IN_MASS_INSERT = 201
 - INVITATION_SENT_SOME_FAIL = 202
 - INVITATION_SEND_FAILED = 203
 - USER_NOT_FOUND = 204
 - USER_CREATE_ERROR = 205
 - USERNAME_IN_USE = 206
 - EMAIL_ADDRESS_IN_USE = 207
 - EMAIL_ADDRESS_NOT_FOUND = 208
 - ERROR_DURING_RESET_OR_MAIL_DISPATCH = 209
 - FAILED_OLD_PASSWORD_MISMATCH = 210
 - PASSWORD_FAILED_ON_COMPLEXITY_CHECK = 211
 - ERR_NOSESSION = 301
 - FAILED_TO_UPLOAD_FILE = 302
 - FAILED_TO_SEND_URL_TO_CAPTURE = 303
 - PRINTING_FAILED = 304
 - PRINTING_PENDING = 305
 - PRINTING_NOTSTARTED = 306
 - INVALID_OR_MISSING_MEDIASERVER_INFO = 307
 - REPLAY_FOLDER_ALREADY_EXISTS = 308
 - NO_ROOM_FOLDER = 309
 - SEC_CONFIG_ERROR = 310
 - SESSION_STILL_ACTIVE = 311
 - ERR_FAILED_TO_INITIALIZE_USERSESSION = 312
 - INSUFFICIENT_DISK_QUOTA = 313
