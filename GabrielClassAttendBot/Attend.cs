namespace GabrielClassAttendBot
{
    public partial class Attend
    {
        public int _id { get; set; } //id
        public string _name { get; set; } //заголовок
        public DateTime _dateTime { get; set; } //дата и время
        public string _groupName { get; set; } //название группы
        public int _studentsQuantity { get; set; } //количество присутствующих на занятии
        public List<string> _students = new List<string>(); //список всех студентов с отметками
    }
}
