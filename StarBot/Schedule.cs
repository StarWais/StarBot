using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StarBot
{
    public class Schedule
    {
        public struct Table
        {
            public Int32 count { get; set; }

            public Day[] days { get; set; }
        }

        public struct Day
        {
            public Int32 num { get; set; }

            public Int32 count { get; set; }

            public String date { get; set; }

            public Lesson[] lessons { get; set; }
        }

        public struct Lesson
        {
            public String timeStart { get; set; }

            public String timeEnd { get; set; }

            public LessonTeacher teacher { get; set; }

            public String type { get; set; }

            public String title { get; set; }

            public String address { get; set; }

            public String room { get; set; }

            public Subgroup subgroup { get; set; }
        }

        public struct Subgroup
        {
            public Int32 id { get; set; }

            public String type { get; set; }
        }

        public struct LessonTeacher
        {
            public String id { get; set; }

            public String fullname { get; set; }

            public String post { get; set; }
        }

        public struct Account
        {
            public String id { get; set; }

            public String fullname { get; set; }

            public String grouptitle { get; set; }

            public String nzach { get; set; }

            public String studenttype { get; set; }

            public String k_sgryp { get; set; }

            public String kvidstud { get; set; }
        }
        public struct Faculty
        {
            public FacultyItems[] items { get; set; }
        }
        public struct FacultyItems
        {
            public Int32 id { get; set; }
            public String title { get; set; }
        }
        public struct Department
        {
            public DepartmentItems[] items { get; set; }
        }
        public struct DepartmentItems
        {
            public Int32 id { get; set; }
            public String title { get; set; }
        }
        public struct Group
        {
            public GroupItems[] items { get; set; }
        }
        public struct GroupItems
        {
            public Int32 id { get; set; }
            public String title { get; set; }

        }
        public struct Course
        {
            public Int32 id { get; set; }
            public String title { get; set; }
        }
        public struct TeacherItems
        {
            public Int32 id { get; set; }
            public String fullname { get; set; }
        }
        public struct Teacher
        {
            public TeacherItems[] items { get; set; }
        }
        public static async Task<String> Request(String URI)
        {
            try
            {
                WebRequest Request = WebRequest.CreateHttp(URI);

                Request.Credentials = CredentialCache.DefaultCredentials;

                WebResponse Response = Request.GetResponse();

                Stream DataStream = Response.GetResponseStream();

                StreamReader DataStreamReader = new StreamReader(DataStream);

                String Data = await DataStreamReader.ReadToEndAsync();

                DataStreamReader.Close();
                Response.Close();

                return Data;
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }
        public static async Task GetMySchedule()
        {
            String Info = await Request($"http://api.grsu.by/1.x/app1/getGroupSchedule?studentId={SavedUser.id}&dateStart={getStartOfWeek().ToShortDateString()}&dateEnd={getStartOfWeek().AddDays(7).ToShortDateString()}&lang=ru_RU");
            SavedSchedule = JsonConvert.DeserializeObject<Table>(Info);
        }
        public static async Task<Account> GetCurrentUser(string Login)
        {
            String Base = await Request("http://api.grsu.by/1.x/app1/getStudent?login=" + Login);
            Account User = JsonConvert.DeserializeObject<Account>(Base);
            return User;
        }
        public static async Task GetGroups(int departmentId, int facultyId, int course)
        {
            String Info = await Request($"http://api.grsu.by/1.x/app1/getGroups?departmentId={departmentId}&facultyId={facultyId}&course={course}");
            Groups = JsonConvert.DeserializeObject<Group>(Info);
        }
        public static async Task GetGroupSchedule(int groupId)
        {
            String Info = await Request($"http://api.grsu.by/1.x/app1/getGroupSchedule?groupId={groupId}&dateStart={getStartOfWeek().ToShortDateString()}&dateEnd={getStartOfWeek().AddDays(7).ToShortDateString()}&lang=ru_RU");
            SavedSchedule = JsonConvert.DeserializeObject<Table>(Info);
        }
        public static async Task GetTeacherSchecdule(int teacherId)
        {
            String Info = await Request($"http://api.grsu.by/1.x/app1/getTeacherSchedule?teacherId={teacherId}&dateStart={getStartOfWeek().ToShortDateString()}&dateEnd={getStartOfWeek().AddDays(7).ToShortDateString()}");
            SavedSchedule = JsonConvert.DeserializeObject<Table>(Info);
        }
        public static async Task GetTeachers()
        {
            String Info = await Request("http://api.grsu.by/1.x/app2/getTeachers");
            Teachers = JsonConvert.DeserializeObject<Teacher>(Info);
        }
        public static Table SavedSchedule { get; set; }
        public static Account SavedUser { get; set; }
        public static Faculty Faculties { get; set; }
        public static Department Departments { get; set; }
        public static Teacher Teachers { get; set; }
        public static Group Groups { get; set; }
        public static DateTime getStartOfWeek()
        {
            DateTime now = DateTime.Now;
            int dayOfWeek = (int)now.DayOfWeek;
            dayOfWeek--;
            if (dayOfWeek < 0)
            {
                dayOfWeek = 6;
            }
            return now.AddDays(-1 * (double)dayOfWeek);
        }
    }
}
