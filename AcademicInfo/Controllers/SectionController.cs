using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Text.Json.Serialization;

namespace AcademicInfo.Controllers
{
    public class SectionRequest
    {
        private string AccessToken;
    }
    public class SectionResponse
    {
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
        public async Task<IActionResult> POST()
        {
            //받은 AccessToken을 인증하는 부분을 추가할 예정

            //Console.WriteLine("요청받음");
            var connectionString = "Host=\r\nacademic-info-db.postgres.database.azure.com\r\n;Username=classhub;Password=ch55361!;Database=AcademicInfo";
            var connection = new NpgsqlConnection(connectionString);

            var query = "SELECT\r\n    Section.section_id AS sectionid,\r\n    Section.year,\r\n    Section.semester,\r\n    Course.title AS name,\r\n    Instructor.name AS instructor,\r\n    Time_slot.day_of_week AS day,\r\n    Time_slot.start_time AS startTime,\r\n    Time_slot.end_time AS endTime,\r\n    Section.room_id AS buildingid\r\nFROM\r\n    Takes\r\n    JOIN Section ON Takes.course_id = Section.course_id\r\n        AND Takes.section_id = Section.section_id\r\n        AND Takes.semester = Section.semester\r\n        AND Takes.year = Section.year\r\n    JOIN Course ON Takes.course_id = Course.course_id\r\n    JOIN Teaches ON Takes.course_id = Teaches.course_id\r\n        AND Takes.section_id = Teaches.section_id\r\n        AND Takes.semester = Teaches.semester\r\n        AND Takes.year = Teaches.year\r\n    JOIN Instructor ON Teaches.instructor_id = Instructor.ID\r\n    JOIN Time_slot ON Section.section_time_slot_id = Time_slot.section_time_slot_id\r\nWHERE\r\n    Takes.student_id = 3;";
            var results = connection.Query<SectionResponse>(query);

            connection.Dispose();

            return Ok(results);
        }
    };
}