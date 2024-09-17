using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Exceptions;
using Update = Telegram.Bot.Types.Update;

namespace GabrielClassAttendBot
{
    public partial class Program()
    {
        public static int classType; //0 - группа, 10 - много студентов сразу, 1 - студент, 2 - отметка

        public static int groupId; //id активной группы

        public static string groupName; //имя активной группы

        public static string groupOldName; //старое имя переименованной группы

        public static int operationType; //0 - добавление, 1 - переименование, 2 - удаление

        public static int studentId; //id активного студента

        public static string studentOldName; //старое имя переименованного студента

        public static int attendId; //id активной отметки

        public static int attendCount; //счётчик отметки

        public static StreamWriter attendWriter; //для записи файла отметки

        public static StreamWriter logWriter; //для записи файла логов

        static async Task Main()
        {
            ITelegramBotClient _botClient = new TelegramBotClient("<token>");
            ReceiverOptions _receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[]
                {
                    UpdateType.Message,
                    UpdateType.CallbackQuery
                },
            };
            CancellationTokenSource cts = new CancellationTokenSource();
            _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);
            var me = await _botClient.GetMeAsync();
            Random rnd = new Random();
            logWriter = new StreamWriter("Asset/log" + rnd.Next(0, 100) + ".txt");
            Console.WriteLine(DateTime.Now + " Бот " + $"{me.FirstName} запущен");
            logWriter.WriteLine(DateTime.Now + " Бот " + $"{me.FirstName} запущен");
            await Task.Delay(-1);
        }

        public static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
            var ErrorMessage = error switch
            {
                ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => error.ToString()
            };
            Console.WriteLine(ErrorMessage);
            logWriter.WriteLine(DateTime.Now + " " + ErrorMessage);
            logWriter.Close();
            return Task.CompletedTask;
        }

        public static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        var message = update.Message;
                        var user = message.From;
                        Console.WriteLine(DateTime.Now + $" {user.FirstName} ({user.Id}) написал сообщение: {message.Text}");
                        logWriter.WriteLine(DateTime.Now + $" {user.FirstName} ({user.Id}) написал сообщение: {message.Text}");
                        var chat = message.Chat;
                        switch (message.Text)
                        {
                            case "/start": //начало работы
                            Groups:
                                Keyboard.GroupsKB();
                                await botClient.SendTextMessageAsync
                                (
                                    chat.Id,
                                    "Выберите группу:\n",
                                    replyMarkup: Keyboard.chooseGroup
                                );
                                await botClient.SendTextMessageAsync
                                (
                                    chat.Id,
                                    "Вы можете создать группу\n",
                                    replyMarkup: Keyboard.addGroup
                                );
                                return;

                            case "Создать группу": //создание группы
                            NewGroup:
                                classType = 0;
                                operationType = 0;
                                await botClient.SendTextMessageAsync
                                (
                                    chat.Id,
                                    "Введите название группы\n"
                                );
                                return;

                            case "Подтвердить создание группы": //подтверждение создания группы
                                Group g = new Group(DB.groups.Count, groupName);
                                DB.groups.Add(g);
                                await botClient.SendTextMessageAsync
                                (
                                    chat.Id,
                                    "Группа " + g._name + " создана\n",
                                    replyMarkup: Keyboard.backToGroups
                                );
                                return;

                            case "Придумать другое название": //переименование группы (если название повторилось)
                                if (operationType == 0)
                                {
                                    goto NewGroup;
                                }
                                else
                                {
                                    goto RenGroup;
                                }

                            case "Вернуться к выбору групп": //возвращение к выбору групп
                                goto Groups;

                            case "Создать отметку посещаемости": //создание отметки посещаемости
                                Attend a = new Attend();
                                a._id = DB.attends.Count;
                                attendWriter = new StreamWriter("Asset/attend" + a._id + ".txt");
                                a._dateTime = message.Date;
                                attendWriter.WriteLine(a._dateTime);
                                foreach (Group group in DB.groups)
                                {
                                    if (group._id == groupId)
                                    {
                                        a._groupName = group._name;
                                        attendWriter.WriteLine(a._groupName);
                                    }
                                }
                                foreach (Student student in DB.students)
                                {
                                    if (student._groupId == groupId)
                                    {
                                        a._students.Add(student._name);
                                    }
                                }
                                DB.attends.Add(a);
                                classType = 2;
                                attendId = a._id;
                                await botClient.SendTextMessageAsync
                                (
                                    chat.Id,
                                    "Введите название отметки (можно внести данные о преподавателе, предмете и т.д.)\n"
                                );
                                return;

                            case "Отметить присутствующих": //отметка присутствующих
                                Mark:
                                if (attendCount < DB.attends[attendId]._students.Count)
                                {
                                    await botClient.SendTextMessageAsync
                                    (
                                        chat.Id,
                                        DB.attends[attendId]._students[attendCount],
                                        replyMarkup: Keyboard.attend
                                    );
                                }
                                else
                                {
                                    attendCount = 0;
                                    attendWriter.Close();
                                    await botClient.SendTextMessageAsync
                                    (
                                        chat.Id,
                                        ShowAttend()
                                    );
                                    using (var stream = File.OpenRead("Asset/attend" + attendId + ".txt"))
                                    {
                                        Telegram.Bot.Types.InputFiles.InputOnlineFile file = new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream);
                                        file.FileName = "attend" + attendId + ".txt";
                                        var send = await botClient.SendDocumentAsync(chat.Id, file);
                                    }
                                    await botClient.SendTextMessageAsync
                                    (
                                        chat.Id,
                                        "Отметка создана\n",
                                        replyMarkup: Keyboard.backToMenue
                                    );
                                }
                                return;

                            case "Переименовать группу": //переименование группы
                            RenGroup:
                                classType = 0;
                                operationType = 1;
                                foreach (Group group in DB.groups)
                                {
                                    if (group._id == groupId)
                                    {
                                        groupOldName = group._name;
                                    }
                                }
                                await botClient.SendTextMessageAsync
                                (
                                    chat.Id,
                                    "Введите новое название группы\n"
                                );
                                return;

                            case "Подтвердить переименование группы": //подтверждение переименования группы
                                DB.groups[groupId]._name = groupName;
                                await botClient.SendTextMessageAsync
                                (
                                    chat.Id,
                                    "Группа " + groupOldName + " переименована в " + groupName + "\n",
                                    replyMarkup: Keyboard.backToMenue
                                );
                                return;

                            case "Вернуться в меню": //возвращение в меню
                                await botClient.SendTextMessageAsync
                                (
                                    chat.Id,
                                    "Выберите действие для группы " + DB.groups[groupId]._name + "\n",
                                    replyMarkup: Keyboard.menue
                                );
                                return;

                            case "Добавить студентов": //добавление нескольких студентов сразу
                                classType = 10;
                                operationType = -1;
                                await botClient.SendTextMessageAsync
                                (
                                    chat.Id,
                                    "Введите ФИО всех студентов группы, разделяя их запятыми\n"
                                );
                                return;

                            case "Добавить студента": //добавление студента
                                classType = 1;
                                operationType = 0;
                                await botClient.SendTextMessageAsync
                                (
                                    chat.Id,
                                    "Введите ФИО студента\n"
                                );
                                return;

                            case "Изменить ФИО студента": //изменение ФИО студента
                                classType = 1;
                                operationType = 1;
                                Keyboard.StudentsKB();
                                await botClient.SendTextMessageAsync
                                (
                                    chat.Id,
                                    "Выберите студента:\n",
                                    replyMarkup: Keyboard.chooseStudent
                                );
                                return;

                            case "Удалить студента": //удаление студента
                                operationType = 2;
                                Keyboard.StudentsKB();
                                await botClient.SendTextMessageAsync
                                (
                                    chat.Id,
                                    "Выберите студента:\n",
                                    replyMarkup: Keyboard.chooseStudent
                                );
                                return;
                                
                            case "Удалить группу": //удаление группы
                                DB.groups.Remove(DB.groups[groupId]);
                                foreach (Group group in DB.groups)
                                {
                                    group._id = DB.groups.IndexOf(group);
                                }
                                foreach (Student student in DB.students)
                                {
                                    if (student._groupId == groupId)
                                    {
                                        DB.students.Remove(student);
                                    }
                                }
                                foreach (Student student in DB.students)
                                {
                                    student._id = DB.students.IndexOf(student);
                                }
                                await botClient.SendTextMessageAsync
                                (
                                    chat.Id,
                                    "Группа удалена\n",
                                     replyMarkup: Keyboard.backToGroups
                                );
                                return;

                            case "Статистика":
                                await botClient.SendTextMessageAsync
                                (
                                    chat.Id,
                                    "Выберите, какую статистику хотите посмотреть\n",
                                    replyMarkup: Keyboard.statistics
                                );
                                return;

                            case "За всё время":
                                await botClient.SendTextMessageAsync
                                (
                                    chat.Id,
                                    ShowStatistics(),
                                    replyMarkup: Keyboard.backToMenue
                                );
                                return;

                            case "За текущий месяц":
                                await botClient.SendTextMessageAsync
                                (
                                    chat.Id,
                                    ShowThisMonthStatistics(),
                                    replyMarkup: Keyboard.backToMenue
                                );
                                return;

                            case "Назад": //возвращение к выбору группы
                                goto Groups;

                            case "+": //студент присутствует на занятии
                                DB.attends[attendId]._studentsQuantity++;
                                attendWriter.WriteLine(DB.attends[attendId]._students[attendCount]);
                                attendCount++;
                                goto Mark;

                            case "-": //студент отсутствует на занятии
                                DB.attends[attendId]._students[attendCount] += "   Н";
                                attendWriter.WriteLine(DB.attends[attendId]._students[attendCount]);
                                attendCount++;
                                goto Mark;

                            case "/stop":
                                await botClient.SendTextMessageAsync
                                (
                                    chat.Id,
                                    "Бот остановлен"
                                );
                                logWriter.Close();
                                await botClient.CloseAsync();
                                break;

                            default: //присвоение имени
                                switch (classType)
                                {
                                    case 0: //присвоение имени группе
                                        int i = 0;
                                        foreach (Group group in DB.groups)
                                        {
                                            if (message.Text == group._name)
                                            {
                                                i++;
                                            }
                                        }
                                        if (i == 0)
                                        {
                                            if (operationType == 0)
                                            {
                                                groupName = message.Text;
                                                await botClient.SendTextMessageAsync
                                                (
                                                    chat.Id,
                                                    "Подтвердите создание группы\n",
                                                    replyMarkup: Keyboard.finishAddGroup
                                                );
                                            }
                                            else
                                            {
                                                groupName = message.Text;
                                                await botClient.SendTextMessageAsync
                                                (
                                                    chat.Id,
                                                    "Подтвердите переименование группы\n",
                                                    replyMarkup: Keyboard.finishRenameGroup
                                                );
                                            }
                                        }
                                        else
                                        {
                                            await botClient.SendTextMessageAsync
                                            (
                                                chat.Id,
                                                "Группа с таким названием уже добавлена.\n",
                                                replyMarkup: Keyboard.differentName
                                            );
                                        }
                                        break;

                                    case 10: //создание нескольких студентов сразу
                                        string[] manyStudents = message.Text.Split(",");
                                        for (int j = 0; j < manyStudents.Length; j++)
                                        {
                                            if (manyStudents[j].Substring(manyStudents[j].Length - 1) == " ")
                                            {
                                                manyStudents[j] = manyStudents[j].Substring(0, manyStudents[j].Length - 1);
                                            }
                                            if (manyStudents[j].Substring(0, 1) == " ")
                                            {
                                                manyStudents[j] = manyStudents[j].Substring(1, manyStudents[j].Length - 1);
                                            }
                                        }
                                        foreach (string name in manyStudents)
                                        {
                                            Student s = new Student(DB.students.Count, name, groupId);
                                            DB.students.Add(s);
                                        }
                                        await botClient.SendTextMessageAsync
                                            (
                                                chat.Id,
                                                "Студенты добавлены\n",
                                                replyMarkup: Keyboard.backToMenue
                                            );
                                        break;

                                    case 1: //создание студента
                                        if (operationType == 0)
                                        {
                                            Student s = new Student(DB.students.Count, message.Text, groupId);
                                            DB.students.Add(s);
                                            await botClient.SendTextMessageAsync
                                            (
                                                chat.Id,
                                                "Студент " + s._name + " добавлен\n",
                                                replyMarkup: Keyboard.backToMenue
                                            );
                                        }
                                        else
                                        {
                                            DB.students[studentId]._name = message.Text;
                                            await botClient.SendTextMessageAsync
                                            (
                                                chat.Id,
                                                "ФИО студента " + studentOldName + " изменено на " + message.Text + "\n",
                                                replyMarkup: Keyboard.backToMenue
                                            );
                                        }
                                        break;

                                    case 2: //присвоение имени отметке
                                        DB.attends[attendId]._name = message.Text;
                                        attendWriter.WriteLine(DB.attends[attendId]._name);
                                        await botClient.SendTextMessageAsync
                                        (
                                            chat.Id,
                                            "Отметке присовено имя\n",
                                            replyMarkup: Keyboard.makeAttend
                                        );
                                        break;
                                }
                                return; 
                        }
                        return;

                    case UpdateType.CallbackQuery:
                        var callbackQuery = update.CallbackQuery;
                        user = callbackQuery.From;
                        Console.WriteLine(DateTime.Now + $" {user.FirstName} ({user.Id}) нажал на кнопку: {callbackQuery.Data}");
                        logWriter.WriteLine(DateTime.Now + $" {user.FirstName} ({user.Id}) нажал на кнопку: {callbackQuery.Data}");
                        chat = callbackQuery.Message.Chat;
                        if (callbackQuery.Data.Contains("g") == true) //выбор группы из списка
                        {
                            classType = 0;
                            groupId = Convert.ToInt32(callbackQuery.Data.Substring(1));
                            await botClient.SendTextMessageAsync
                            (
                                chat.Id,
                                "Выберите действие для группы " + DB.groups[groupId]._name + "\n",
                                replyMarkup: Keyboard.menue
                            );
                        }
                        else if (callbackQuery.Data.Contains("s") == true) //выбор студента из списка
                        {
                            classType = 1;
                            studentId = Convert.ToInt32(callbackQuery.Data.Substring(1));
                            studentOldName = DB.students[studentId]._name;
                            if (operationType == 2)
                            {
                                DB.students.Remove(DB.students[studentId]);
                                foreach (Student student in DB.students)
                                {
                                    student._id = DB.students.IndexOf(student);
                                }
                                await botClient.SendTextMessageAsync
                                (
                                    chat.Id,
                                    "Студент " + studentOldName + " удалён\n",
                                    replyMarkup: Keyboard.backToMenue
                                );
                            }
                            else
                            {
                                foreach (Student student in DB.students)
                                {
                                    if (student._id == studentId)
                                    {
                                        studentOldName = student._name;
                                    }
                                }
                                await botClient.SendTextMessageAsync
                                (
                                    chat.Id,
                                    "Введите новое ФИО студента\n"
                                );
                            }
                        }
                        return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static string ShowAttend() //вывод отметки посещаемости
        {
            string show = DB.attends[attendId]._name + "\n" + DB.attends[attendId]._dateTime + "\n" + DB.attends[attendId]._groupName + "\n" + "\n";
            foreach (string student in DB.attends[attendId]._students)
            {
                show += student + "\n";
            }
            return show;
        }

        public static string ShowStatistics() //вывод всей статистики
        {
            string show = "";
            if (DB.attends.Count(a => a._groupName == groupName) == 0)
            {
                show = "У группы " + groupName + " нет отметок";
            }
            else
            {
                foreach (Attend attend in DB.attends)
                {
                    if (attend._groupName == groupName)
                    {
                        show += attend._dateTime + " - " + attend._studentsQuantity + " чел." + "\n";
                    }
                }
            }
            return show;
        }

        public static string ShowThisMonthStatistics() //вывод статистики за текущий месяц
        {
            string show = "";
            if (DB.attends.Count(a => a._groupName == groupName && a._dateTime.Month == DateTime.Now.Month) == 0)
            {
                show = "У группы " + groupName + " нет отметок за текущий месяц";
            }
            else
            {
                foreach (Attend attend in DB.attends)
                {
                    if (attend._groupName == groupName && attend._dateTime.Month == DateTime.Now.Month)
                    {
                        show += attend._dateTime + " - " + attend._studentsQuantity + " чел." + "\n";
                    }
                }
            }
            return show;
        }
    }
}