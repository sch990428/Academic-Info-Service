using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using static AcademicInfo.Controllers.SectionResponse;
using System.Reflection.Metadata;

namespace AcademicInfo.Controllers {
    public class SectionRequest {
        private string AccessToken;
    }
    public class SectionResponse {
        public int SectionId { get; set; } // 강좌 번호

        public int Year { get; set; } // 개설 년도

        public string Semester { get; set; } // 개설 학기

        public string Name { get; set; } // 교과목명

        public string Instructor { get; set; } // 담당교수

        public DayOfWeek Day { get; set; } // 강의요일

        public int StartTime { get; set; } // 강의 시작시간

        public int EndTime { get; set; } // 강의 종료시간

        public string BuildingId { get; set; } // 강의실
    }
    public class ClassRoom {
        public int room_id { get; set; }
        public string course_id { get; set; } // ABC12345
        public int section_id { get; set; } // 0810
        public string semester { get; set; } // SPRING, SUMMER, FALL, WINTER         
        public int year { get; set; } // 2023
        public string title { get; set; } // 캡스톤디자인
    }

    public class TimeSlot {
        public DayOfWeek day_of_week { get; set; }
        public int start_time { get; set; }
        public int end_time { get; set; }
    }

    public class ClassRoomDetail : ClassRoom {
        public List<TimeSlot> timeSlots { get; set; }
        public string instructor { get; set; }
        public string buildingId { get; set; }
        public int color { get; set; } = -1;
    }

    [ApiController]
    [Route("[controller]")]
    public class SectionController : ControllerBase
    {

        private readonly ILogger<SectionController> _logger;

        public SectionController(ILogger<SectionController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> POST([FromQuery] int student_id)
        {
            var connectionString = "Host=\r\nacademic-info-db.postgres.database.azure.com\r\n;Username=classhub;Password=ch55361!;Database=AcademicInfo";
            var connection = new NpgsqlConnection(connectionString);

            //해당 id가 학생의 계정에 존재한다면
            var student_exist_query = "SELECT COUNT(*) FROM student WHERE id = @id";
            var professor_exist_query = "SELECT COUNT(*) FROM instructor WHERE id = @id";
            var parameters = new DynamicParameters();
            parameters.Add("id", student_id);

            if (connection.ExecuteScalar<int>(student_exist_query, parameters) == 1) {

                var query = "SELECT\r\n    Section.section_id AS sectionid,\r\n    Section.year,\r\n    Section.semester,\r\n    Course.title AS name,\r\n    Instructor.name AS instructor,\r\n    Time_slot.day_of_week AS day,\r\n    Time_slot.start_time AS startTime,\r\n    Time_slot.end_time AS endTime,\r\n    Section.room_id AS buildingid\r\nFROM\r\n    Takes\r\n    JOIN Section ON Takes.course_id = Section.course_id\r\n        AND Takes.section_id = Section.section_id\r\n        AND Takes.semester = Section.semester\r\n        AND Takes.year = Section.year\r\n    JOIN Course ON Takes.course_id = Course.course_id\r\n    JOIN Teaches ON Takes.course_id = Teaches.course_id\r\n        AND Takes.section_id = Teaches.section_id\r\n        AND Takes.semester = Teaches.semester\r\n        AND Takes.year = Teaches.year\r\n    JOIN Instructor ON Teaches.instructor_id = Instructor.ID\r\n    JOIN Time_slot ON Section.section_time_slot_id = Time_slot.section_time_slot_id\r\nWHERE\r\n    Takes.student_id = @student_id;";
                var parameters2 = new DynamicParameters();
                parameters2.Add("student_id", student_id);

                var results = connection.Query<SectionResponse>(query, parameters2);

                return Ok(results);

            } else if (connection.ExecuteScalar<int>(professor_exist_query, parameters) == 1) {

                //해당 id가 학생이 아니라면 교수를 검사
                var query = "SELECT Section.section_id AS number, Section.year, Section.semester, Course.title AS name, Instructor.name AS instructor, Time_slot.day_of_week AS day, Time_slot.start_time AS startTime, Time_slot.end_time AS endTime, Section.room_id AS buildingid\r\nFROM teaches \r\nJOIN Section ON teaches.course_id = Section.course_id AND teaches.section_id = Section.section_id AND teaches.semester = Section.semester AND teaches.year = Section.year\r\nJOIN Course ON teaches.course_id = Course.course_id\r\nJOIN instructor ON teaches.instructor_id = instructor.id\r\nJOIN Time_slot ON Section.section_time_slot_id = Time_slot.section_time_slot_id\r\nWHERE teaches.instructor_id = @instructor_id";
                var parameters2 = new DynamicParameters();
                parameters2.Add("instructor_id", student_id);

                var results = connection.Query<SectionResponse>(query, parameters2);

                return Ok(results);

            }

            //일단은 반환하게 한다
            var query3 = "SELECT\r\n    Section.section_id AS sectionid,\r\n    Section.year,\r\n    Section.semester,\r\n    Course.title AS name,\r\n    Instructor.name AS instructor,\r\n    Time_slot.day_of_week AS day,\r\n    Time_slot.start_time AS startTime,\r\n    Time_slot.end_time AS endTime,\r\n    Section.room_id AS buildingid\r\nFROM\r\n    Takes\r\n    JOIN Section ON Takes.course_id = Section.course_id\r\n        AND Takes.section_id = Section.section_id\r\n        AND Takes.semester = Section.semester\r\n        AND Takes.year = Section.year\r\n    JOIN Course ON Takes.course_id = Course.course_id\r\n    JOIN Teaches ON Takes.course_id = Teaches.course_id\r\n        AND Takes.section_id = Teaches.section_id\r\n        AND Takes.semester = Teaches.semester\r\n        AND Takes.year = Teaches.year\r\n    JOIN Instructor ON Teaches.instructor_id = Instructor.ID\r\n    JOIN Time_slot ON Section.section_time_slot_id = Time_slot.section_time_slot_id\r\nWHERE\r\n    Takes.student_id = @student_id;";
            var parameters3 = new DynamicParameters();
            parameters3.Add("student_id", student_id);

            var results3 = connection.Query<SectionResponse>(query3, parameters3);

            return Ok(results3);
        }

        [HttpGet("/ClassRoomDetail")]
        public IActionResult ClassRoomDetail([FromQuery] string course_id, int section_id, string semester, int year, string accessToken) {
            var connectionString = "Host=\r\nacademic-info-db.postgres.database.azure.com\r\n;Username=classhub;Password=ch55361!;Database=AcademicInfo";
            var connection = new NpgsqlConnection(connectionString);

            int semester_int = -1;
            if (semester == "Spring") {
                semester_int = 1;
            } else if (semester == "Summer") {
                semester_int = 2;
            } else if (semester == "Fall") {
                semester_int = 3;
            } else if (semester == "Winter") {
                semester_int = 4;
            }

            var query = "SELECT Instructor.name AS instructor, Section.room_id AS BuildingId\r\nFROM Section, Teaches, Instructor\r\nWHERE Teaches.section_id = Section.section_id AND Teaches.instructor_id = Instructor.id\r\nAND Section.course_id = @course_id AND Section.section_id = @section_id AND Section.semester = @semester AND Section.year = @year";
            var parameters = new DynamicParameters();
            parameters.Add("course_id", course_id);
            parameters.Add("section_id", section_id);
            parameters.Add("semester", semester_int);
            parameters.Add("year", year);

            var result = connection.QuerySingle<ClassRoomDetail>(query, parameters);

            var query2 = "SELECT *\r\nFROM section, section_time_slot, time_slot\r\nWHERE section.section_time_slot_id = time_slot.section_time_slot_id AND time_slot.section_time_slot_id = section_time_slot.section_time_slot_id\r\nAND Section.course_id = @course_id AND Section.section_id = @section_id AND Section.semester = @semester AND Section.year = @year";
            var time_slot_list = connection.Query<TimeSlot>(query2, parameters);

            result.timeSlots = new List<TimeSlot>();

            foreach (var slot in time_slot_list) {
                result.timeSlots.Add(slot);
            }

            return Ok(result);
        }
    };
}