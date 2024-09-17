using Telegram.Bot.Types.ReplyMarkups;

namespace GabrielClassAttendBot
{
    public class Keyboard : Program
    {
        public static List<InlineKeyboardButton[]> groupsKB = new List<InlineKeyboardButton[]>(); //массив для клаиватуры с группами
        public static void GroupsKB() //заполенине массива для клавиатуры с группами
        {
            groupsKB.Clear();
            foreach (Group group in DB.groups)
            {
                groupsKB.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(group._name, "g" + Convert.ToString(group._id)) });
            }
        }
        public static InlineKeyboardMarkup chooseGroup = new InlineKeyboardMarkup //клавиатура с группами
        (
            groupsKB
        );

        public static ReplyKeyboardMarkup addGroup = new ReplyKeyboardMarkup //клавиатура для создания группы
        (
            new KeyboardButton[]
            {
                new KeyboardButton("Создать группу")
            }
        )
        { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup backToGroups = new ReplyKeyboardMarkup //клавиатура для взвращения к выбору групп
        (
            new KeyboardButton("Вернуться к выбору групп")
        )
        { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup makeAttend = new ReplyKeyboardMarkup //клавиатура для создания отметки посещаемости
        (
            new KeyboardButton("Отметить присутствующих")
        )
        { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup attend = new ReplyKeyboardMarkup //клавиатура для отметки студента
        (
            new KeyboardButton[]
            {
                new KeyboardButton("+"),
                new KeyboardButton("-")
            }
        )
        { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup backToMenue = new ReplyKeyboardMarkup //клавиатура для возвращения в меню группы
        (
            new KeyboardButton("Вернуться в меню")
        )
        { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup menue = new ReplyKeyboardMarkup //меню группы
        (
            new List<KeyboardButton[]>()
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Создать отметку посещаемости"),
                    new KeyboardButton("Добавить студентов")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Статистика"),
                    new KeyboardButton("Добавить студента")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Переименовать группу"),
                    new KeyboardButton("Изменить ФИО студента")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Удалить группу"),
                    new KeyboardButton("Удалить студента")
                },
                new KeyboardButton[]
                {
                    new KeyboardButton("Назад")
                }
            }
        )
        { ResizeKeyboard = true };

        public static List<InlineKeyboardButton[]> studentsKB = new List<InlineKeyboardButton[]>(); //массив для клавиатуры со студентами
        public static void StudentsKB() //заполнение массива для клавиатуры со cтудентами
        {
            studentsKB.Clear();
            foreach (Student student in DB.students)
            {
                if (student._groupId == groupId)
                {
                    studentsKB.Add(new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(student._name, "s" + Convert.ToString(student._id)) });
                }
            }
        }
        public static InlineKeyboardMarkup chooseStudent = new InlineKeyboardMarkup //клавиатура со студентами
        (
            studentsKB
        );

        public static ReplyKeyboardMarkup finishAddGroup = new ReplyKeyboardMarkup //клавиатура для подтверждения создания группы
        (
            new KeyboardButton("Подтвердить создание группы")
        )
        { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup finishRenameGroup = new ReplyKeyboardMarkup //клавиатура для подтверждения переименования группы
        (
            new KeyboardButton("Подтвердить переименование группы")
        )
        { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup differentName = new ReplyKeyboardMarkup //клавиатура для выбора нового имени группы в случае повтора
        (
            new KeyboardButton("Придумать другое название")
        )
        { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup statistics = new ReplyKeyboardMarkup //клавиатура для выбора статистики
        (
            new KeyboardButton[]
            {
                new KeyboardButton("За текущий месяц"),
                new KeyboardButton("За всё время")
            }
        )
        { ResizeKeyboard = true };
    }
}