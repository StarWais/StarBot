using System;
using VkNet;
using VkNet.Enums;
using VkNet.Enums.SafetyEnums;
using VkNet.Enums.Filters;
using VkNet.Model.RequestParams;
using System.Threading;
using VkNet.Model;
using System.Text;
using System.Threading.Tasks;
using VkNet.Exception;
using System.Collections.Generic;
using System.Net;

namespace StarBot
{
    static class Program
    {
        static Random random = new Random();
        static VkApi vk = new VkApi();
        static int ConfID;
        static bool Busy = false;
        static bool NeedCaptcha = false;
        static string CaptchaMessage = String.Empty;
        static LongPollServerResponse server;
        static long CaptchaUserID = 0;
        static Func<string> code = () =>
        {
            Console.Write("Код авторизации: \nЕсли нет, то нажмите Enter");
            string value = Console.ReadLine();
            return value;
        };
        static string[] Commands =
{
            "привет",//0
            "иди нахуй",//1
            "сколько времени",//2
            "расписание",//3
            "как дела",//4
            "пролайкай стену",//5
            "пролайкай фотки",//6
            "скинь песню",//7
            "добавь в друзья",//8
            "вопрос"//9
        };
        static void Main(string[] args)
        {
            Console.Title = "StarBot v 1.0";
            Console.WriteLine("Начинаем авторизацию...");
            if (Auth())
            {
                long[] temp = new long[] { vk.UserId.Value };
                var users = vk.Users.Get(temp, ProfileFields.Sex | ProfileFields.FirstName | ProfileFields.LastName);
                User Bot = users[0];
                Console.WriteLine($"Авторизация прошла успешно. Добро пожаловать, {Bot.FirstName} {Bot.LastName}.");
                GetAndSetConfs();
                server = vk.Messages.GetLongPollServer();
                Timer WatchTimer = new Timer(new TimerCallback(CheckMessages), null, 0, 2000);
                Console.WriteLine($"Программа завершила свою работу");
                Console.ReadKey();
            }
        }
        static bool Auth()
        {
            try
            {
                vk.Authorize(new ApiAuthParams
                {
                    ApplicationId = 6150077,
                    Login = "",
                    Password = "",
                    Settings = Settings.All,
                    TwoFactorAuthorization = code
                });
                return true;
            }
            catch
            {
                Console.WriteLine("Введен неверный логин или пароль");
                return false;
            }
        }
        static async void CheckMessages(object state)
        {
            Console.WriteLine("Сканируем сообщения...");
            long BotID = vk.UserId.Value;
            try
            {
                var longpoll = await vk.Messages.GetLongPollHistoryAsync(new MessagesGetLongPollHistoryParams
                {
                    Ts = server.Ts,
                    Pts = server.Pts
                });
                var Messages = longpoll.Messages;
                Message last = new Message();
                foreach (var CurrentMessage in Messages)
                {
                    if (CurrentMessage.UserId != BotID && CurrentMessage.ChatId == ConfID && !CurrentMessage.ReadState.Equals(MessageReadState.Readed))
                    {
                        if(!Busy)
                        {
                            Command(Convert.ToInt32(CurrentMessage.UserId), CurrentMessage.Body, Convert.ToInt32(CurrentMessage.ChatId));
                            CurrentMessage.ReadState = MessageReadState.Readed;
                        }
                        else
                        {
                            SendToConf("Подожди, я пока занят", ConfID);
                            if (NeedCaptcha)
                            {
                                if (CurrentMessage.UserId == CaptchaUserID)
                                    CaptchaMessage = CurrentMessage.Body;
                                NeedCaptcha = false;
                                CaptchaMessage = String.Empty;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при мониторинге сообщений: {ex}");
            }
        }
        static async void Command(long checkedUserID, string Message, long ConfId)
        {
            var users = vk.Users.Get(new long[] { checkedUserID }, ProfileFields.Sex | ProfileFields.FirstName | ProfileFields.LastName);
            User Sender = users[0];
            string SenderName = $"{Sender.FirstName} {Sender.LastName}";
            bool Contains = false;
            string SendedMessage = String.Empty;
            int CmdId = 0;
            for (int i = 0; i < Commands.Length; i++)
            {
                if (Message.ToLower().Contains("бот") && Message.ToLower().Contains(Commands[i]))
                {
                    Contains = true;
                    CmdId = i;
                    Console.WriteLine($"{SenderName} -->> {Message}");
                    break;
                }
            }
            if (Contains)
            {
                switch (CmdId)
                {
                    case 0:
                        if (random.Next() % 2 == 0)
                        {
                            SendedMessage = $"Привет, {Sender.FirstName}";
                            SendToConf(SendedMessage, ConfId);
                            Console.WriteLine($"{SendedMessage} -->> {SenderName}");
                            break;
                        }
                        else
                        {
                            SendedMessage = $"Дарова, {Sender.FirstName}";
                            SendToConf(SendedMessage, ConfId);
                            Console.WriteLine($"{SendedMessage} -->> {SenderName}");
                            break;
                        }
                    case 1:
                        if (random.Next() % 2 == 0)
                        {
                            switch (Sender.Sex)
                            {
                                case Sex.Male:
                                    SendedMessage = $"{Sender.FirstName}, сам пошел нахуй";
                                    SendToConf(SendedMessage, ConfId);
                                    Console.WriteLine($"{SendedMessage} -->> {SenderName}");
                                    break;
                                case Sex.Female:
                                    SendedMessage = $"{Sender.FirstName}, сама пошла нахуй";
                                    SendToConf(SendedMessage, ConfId);
                                    Console.WriteLine($"{SendedMessage} -->> {SenderName}");
                                    break;
                            }
                            break;
                        }
                        else
                        {
                            SendedMessage = $"{Sender.FirstName}, нельзя просто так взять и послать бота, он может обидеться";
                            SendToConf(SendedMessage, ConfId);
                            Console.WriteLine($"{SendedMessage} -->> {SenderName}");
                            break;
                        }
                    case 2:
                        SendedMessage = $"Сейчас {DateTime.Now.Hour}:{DateTime.Now.Minute}";
                        SendToConf(SendedMessage, ConfId);
                        Console.WriteLine($"{SendedMessage} -->> {SenderName}");
                        break;
                    case 3:
                        Schedule.SavedUser = await Schedule.GetCurrentUser("Kudryvec_MV_17");
                        await Schedule.GetMySchedule();
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < Schedule.SavedSchedule.count; i++)
                        {
                            string day = String.Empty;
                            var s = DateTime.Parse(Schedule.SavedSchedule.days[i].date).DayOfWeek;
                            switch(s)
                            {
                                case DayOfWeek.Monday:
                                    day = "Понедельник";
                                    break;
                                case DayOfWeek.Tuesday:
                                    day = "Вторник";
                                    break;
                                case DayOfWeek.Wednesday:
                                    day = "Среда";
                                    break;
                                case DayOfWeek.Thursday:
                                    day = "Четверг";
                                    break;
                                case DayOfWeek.Friday:
                                    day = "Пятница";
                                    break;
                                case DayOfWeek.Saturday:
                                    day = "Суббота";
                                    break;
                            }
                            sb.Append($"{Schedule.SavedSchedule.days[i].date} ({day})" + "\n");
                            sb.Append(new string('*', 60) + "\n");
                            for (int j = 0; j < Schedule.SavedSchedule.days[i].lessons.Length; j++)
                            {
                                sb.Append($"{Schedule.SavedSchedule.days[i].lessons[j].timeStart} - {Schedule.SavedSchedule.days[i].lessons[j].timeEnd} | {Schedule.SavedSchedule.days[i].lessons[j].title} ({Schedule.SavedSchedule.days[i].lessons[j].type}). Кабинет: {Schedule.SavedSchedule.days[i].lessons[j].room}\n");
                            }
                            sb.Append(new string('*', 60) + "\n");
                        }
                        SendToConf(sb.ToString(), ConfId);
                        Console.WriteLine($"{sb.ToString()} -->> {SenderName}");
                        break;
                    case 4:
                        if (random.Next() % 2 == 0)
                        {
                            SendedMessage = $"Вроде неплохо";
                            SendToConf(SendedMessage, ConfId);
                            Console.WriteLine($"{SendedMessage} -->> {SenderName}");
                            break;
                        }
                        else
                        {
                            SendedMessage = $"На самом деле осозновать что ты всего-лишь бот грустно...";
                            SendToConf(SendedMessage, ConfId);
                            Console.WriteLine($"{SendedMessage} -->> {SenderName}");
                            break;
                        }
                    case 5:
                        Console.WriteLine($"Начинаем лайкать стену пользователя: {SenderName}");
                        Busy = true;
                        LikeWall(Sender.Id, ConfId, SenderName);
                        Busy = false;
                        break;
                    case 8:
                        Console.WriteLine($"Пробуем добавить в друзья пользователя: {SenderName}");
                        try
                        {
                            vk.Friends.Add(Sender.Id, follow:true);
                            Console.WriteLine($"Заявка отправлена пользователю {SenderName}");
                            SendToConf("Добавился", ConfId);
                        }
                        catch
                        {
                            Console.WriteLine($"Ошибка при отправке запроса пользователю {SenderName}");
                        }
                        break;
                    case 6:
                        Console.WriteLine($"Начинаем лайкать фото пользователя: {SenderName}");
                        Busy = true;
                        LikePhotos(Sender.Id, ConfId, SenderName);
                        Busy = false;
                        break;
                    case 7:

                        break;
                    case 9:
                        if (random.Next(1, 4) == 1)
                        {
                            SendedMessage = $"{Sender.FirstName}, да";
                            SendToConf(SendedMessage, ConfId);
                            Console.WriteLine($"{SendedMessage} -->> {SenderName}");
                            break;
                        }
                        else if (random.Next(1, 4) == 2)
                        {
                            SendedMessage = $"{Sender.FirstName}, нет";
                            SendToConf(SendedMessage, ConfId);
                            Console.WriteLine($"{SendedMessage} -->> {SenderName}");
                            break;
                        }
                        else if (random.Next(1, 4) == 3)
                        {
                            SendedMessage = $"{Sender.FirstName}, возможно";
                            SendToConf(SendedMessage, ConfId);
                            Console.WriteLine($"{SendedMessage} -->> {SenderName}");
                            break;
                        }
                        else
                        {
                            SendedMessage = $"{Sender.FirstName}, я затрудняюсь ответить";
                            SendToConf(SendedMessage, ConfId);
                            Console.WriteLine($"{SendedMessage} -->> {SenderName}");
                            break;
                        }
                }
            }
        }
        static void SendToConf(string Message, long ChatId)
        {
            vk.Messages.Send(new MessagesSendParams
            {
                ChatId = ChatId,
                Message = Message
            });
        }
        static void LikeWall(long LikedUser, long ChatId, string SenderName)
        {
            int GoodLikes = 0;
            int Errors = 0;
            var posts = vk.Wall.Get(new WallGetParams
            {
                OwnerId = LikedUser,
                Count = 100
            });
            for (int i = 0; i < posts.WallPosts.Count; i++)
            {
                var temp = posts.WallPosts[i].Id;
                try
                {
                    Thread.Sleep(2000);
                    GoodLikes++;
                    vk.Likes.Add(new LikesAddParams
                    {
                        OwnerId = LikedUser,
                        ItemId = long.Parse(temp.ToString()),
                        Type = LikeObjectType.Post,
                    });
                    Console.WriteLine($"Успех, {i}/{posts.WallPosts.Count}");
                }
                catch(Exception ex)
                {
                    if(!(ex is VkApiException))
                        break;
                    Errors++;
                    if (CheckCaptcha((VkApiException)ex, vk.Users.Get(new List<long>() { LikedUser })[0].FirstName))
                    {
                        NeedCaptcha = true;
                        var ff = ex as CaptchaNeededException;
                        try
                        {
                            vk.Likes.Add(new LikesAddParams
                            {
                                OwnerId = LikedUser,
                                ItemId = long.Parse(temp.ToString()),
                                Type = LikeObjectType.Post,
                                CaptchaKey = CaptchaMessage,
                                CaptchaSid = ff.Sid
                            });
                            GoodLikes++;
                            Errors--;
                        }
                        catch (Exception) { }
                        CaptchaMessage = String.Empty;
                        NeedCaptcha = false;
                    }
                    else
                        break;
                }
            }
            SendToConf($"Процесс завершен. Успешных лайков стены: {GoodLikes}. Ошибок: {Errors}", ChatId);
            Console.WriteLine($"Пользователь {SenderName} пролайкан. Успешных лайков стены: {GoodLikes}. Ошибок: {Errors}", ChatId);
        }
        static bool CheckCaptcha(VkApiException ex, string Name)
        {
            if (ex is CaptchaNeededException ff)
            {
                SendToConf($"Так-с, {Name} возникла капча, введи пожалуйста", ConfID);
                vk.Messages.Send(new MessagesSendParams
                {
                    ChatId = ConfID,
                    Message = ff.Img.ToString()
                });
                return true;
            }
            return false;
        }
        static void LikePhotos(long LikedUser, long ChatId, string SenderName)
        {
            int GoodLikes = 0;
            int Errors = 0;
            var photos = vk.Photo.Get(new PhotoGetParams
            {
                OwnerId = LikedUser,
                Count = 100,
                AlbumId = PhotoAlbumType.Profile
            });
            for (ulong i = 0; i < photos.TotalCount; i++)
            {
                var temp = photos[Convert.ToInt32(i)].Id;
                try
                {
                    Thread.Sleep(2000);
                    vk.Likes.Add(new LikesAddParams
                    {
                        OwnerId = LikedUser,
                        ItemId = long.Parse(temp.ToString()),
                        Type = LikeObjectType.Photo
                    });
                    GoodLikes++;
                }
                catch(Exception ex)
                {
                    if (!(ex is VkApiException))
                        break;
                    Errors++;
                    if (CheckCaptcha((VkApiException)ex, vk.Users.Get(new List<long>() { LikedUser })[0].FirstName))
                    {
                        NeedCaptcha = true;
                        var ff = ex as CaptchaNeededException;
                        try
                        {
                            vk.Likes.Add(new LikesAddParams
                            {
                                OwnerId = LikedUser,
                                ItemId = long.Parse(temp.ToString()),
                                Type = LikeObjectType.Photo
                            });
                            GoodLikes++;
                            Errors--;
                        }
                        catch (Exception) { }
                        CaptchaMessage = String.Empty;
                        NeedCaptcha = false;
                    }
                    else
                        break;
                }
            }
            SendToConf($"Процесс завершен. Успешных лайков фотографий: {GoodLikes}. Ошибок: {Errors}", ChatId);
            Console.WriteLine($"Пользователь {SenderName} пролайкан. Успешных лайков фотографий: {GoodLikes}. Ошибок: {Errors}");
        }
        static void GetAndSetConfs()
        {
            var Confs = vk.Messages.GetDialogs(new MessagesDialogsGetParams
            {
                Count = 200
            });
            foreach (var Conf in Confs.Messages)
            {
                if (!String.IsNullOrEmpty(Conf.Title))
                    Console.WriteLine($" * {Conf.Title} | ID: {Conf.ChatId?.ToString()}, Пользователей: {Conf.UsersCount}");
            }
            Console.Write("Введите ID отслеживаемой беседы: ");
            ConfID = int.Parse(Console.ReadLine());
        }
    }
}