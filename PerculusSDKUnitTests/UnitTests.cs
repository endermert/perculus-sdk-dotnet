using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PerculusSDK;
using System.Collections.Generic;

namespace PerculusSDKUnitTests
{
    [TestClass]
    public class UnitTests
    {
        const string PERCULUS_BASE_URI = "http://localhost";
        const string PERCULUS_API_USERNAME = "test";
        const string PERCULUS_API_PASSWORD = "1234";

        [TestMethod]
        public void AddRoom()
        {
            /*
            // You can use Rooms class to create a room.
            // But RoomService.Save is more convenient. You can look at RoomService.cs to see the implementation details.
            PerculusSDK.Rooms.RoomsSoapClient roomsSoapClient = new PerculusSDK.Rooms.RoomsSoapClient();
            PerculusSDK.Rooms.AuthHeader authHeader = new PerculusSDK.Rooms.AuthHeader() { Username = "<your-api-username>", Password = "<your-api-password>" };
            PerculusSDK.Rooms.WSResponse response = roomsSoapClient.AddRoom(authHeader, 0, "room name", "desc", DateTime.Now, 60, "elearning", "#cccccc", "tr-TR", 0, "", true, false, 0, 0, false, "", false, "default", "elearning", null, getModuleSettings());
            */

            PerculusSDK.RoomService roomService = new PerculusSDK.RoomService();
            roomService.Initialize(new Uri(PERCULUS_BASE_URI), PERCULUS_API_USERNAME, PERCULUS_API_PASSWORD);

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


            if (room == null)
            {
                // error
                Console.Out.WriteLine("error:" + roomService.GetLastError());
                Assert.IsTrue(false);
            }
            Console.Out.WriteLine("New room created:" + room.ROOMID);

            // add attendee
            if (room.ROOMID > 0)
            {
                var participant = new Participant();
                participant.FirstName = "test";
                participant.LastName = "user";
                participant.Role = Participant.PARTICIPANT_USER;
                participant.Email = "perculustestuser@perculus.com";
                
                
                // register participant in the room and get attendance URL to redirect
                string url = roomService.RegisterAndGetURL(participant, room.ROOMID);

                // if you don't want to redirect user instantly, you can get attendance code and store it in database
                // string attendanceCode = roomService.RegisterAttendee(participant, room.ROOMID);
                
                
                if (string.IsNullOrEmpty(url))
                {
                    Console.Out.WriteLine("error:" + roomService.GetLastError());
                    Assert.IsTrue(false);
                }
                Console.Out.WriteLine("Attendance url:" + url);
            }
        }

        private PerculusSDK.Rooms.ModuleSetting[] getModuleSettings()
        {
            List<PerculusSDK.Rooms.ModuleSetting> moduleSettings = new List<PerculusSDK.Rooms.ModuleSetting>();
            /* The following settings can be applied
            moduleSettings.Add(new ModuleSetting() { Value="1", key="allowPrivateMessaging", mname="Chat"});
            moduleSettings.Add(new ModuleSetting() { Value="1", key="allowMessaging", mname="Chat"});
            moduleSettings.Add(new ModuleSetting() { Value="1", key="showAllUsers", mname="Chat"});
            moduleSettings.Add(new ModuleSetting() { Value="1", key="allowStartOthersStream", mname="StreamList"});
            moduleSettings.Add(new ModuleSetting() { Value="1", key="allowResizeVideos", mname="StreamList"});
            moduleSettings.Add(new ModuleSetting() { Value="1", key="defaultVideoW", mname="StreamList"});
            moduleSettings.Add(new ModuleSetting() { Value="1", key="defaultVideoH", mname="StreamList"});
            moduleSettings.Add(new ModuleSetting() { Value="1", key="streamCount", mname="StreamList"});
            moduleSettings.Add(new ModuleSetting() { Value="1", key="cancelAEC", mname="StreamList"});
            moduleSettings.Add(new ModuleSetting() { Value="1", key="streamQuality", mname="StreamList"});
            // moduleSettings.Add(new ModuleSetting() { Value="2", key="extraTime", mname="Session"}); // this is reserved for 
            moduleSettings.Add(new ModuleSetting() { Value="5", key="warnBefore", mname="Session"});
            moduleSettings.Add(new ModuleSetting() { Value="1", key="showTimer", mname="Session"});
            moduleSettings.Add(new ModuleSetting() { Value="0x104ba9,0x104ba9", key="panelHeaderColors", mname="Session"});
            moduleSettings.Add(new ModuleSetting() { Value="0xffffff,0xcccccc", key="panelFooterColors", mname="Session"});
            moduleSettings.Add(new ModuleSetting() { Value="0xffffff", key="panelTitleColor", mname="Session"});
             * */
            return moduleSettings.ToArray();
        }
    }
}
