namespace GabrielClassAttendBot
{
    public partial class Student
    {
        public int _id { get; set; } //id
        public string _name { get; set; } //название
        public int _groupId { get; set; } //id группы

        public Student() //конструктор по умолчанию
        {
            _id = -1;
            _name = "";
            _groupId = -1;
        }

        public Student(int id, string name, int groupId) //настраиваемый конструктор
        {
            _id = id;
            _name = name;
            _groupId = groupId;
        }
    }
}
