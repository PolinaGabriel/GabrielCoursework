namespace GabrielClassAttendBot
{
    public partial class Group
    {
        public int _id { get; set; } //id
        public string _name { get; set; } //название

        public Group() //конструктор по умолчанию
        {
            _id = -1;
            _name = "";
        }

        public Group(int id, string name) //настраиваемый конструктор
        {
            _id = id;
            _name = name;
        }
    }
}
