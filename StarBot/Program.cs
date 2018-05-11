using System;
using VkNet;
using VkNet.Utils;
using VkNet.Enums;
using VkNet.Enums.SafetyEnums;
using VkNet.Enums.Filters;
using VkNet.Model.RequestParams;
using System.Threading;
using VkNet.Model;

namespace StarBot
{
    class Program
    {
        static Random random = new Random();
        static VkApi vk = new VkApi();
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
                User Bot = vk.Users.Get(vk.UserId.Value);
                Console.WriteLine($"Авторизация прошла успешно. Добро пожаловать, {Bot.FirstName} {Bot.LastName}.");
                //var Friends = GetFriends();
                GetAndSetConfs(out int ConfID);
                CheckMessages(Bot.Id, ConfID);
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
                    Login = "375296901425",
                    Password = "knopkagovna1488",
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
        static VkCollection<User> GetFriends()
        {
            VkCollection<User> Friends = vk.Friends.Get(new FriendsGetParams
            {
                UserId = vk.UserId,
                Fields = ProfileFields.FirstName | ProfileFields.LastName,
                Order = FriendsOrder.Name
            });
            return Friends;
        }
        static void CheckMessages(long CheckedUserID, int ConfID)
        {
            Console.WriteLine("Сканируем сообщения...");
            long BotID = vk.UserId.Value;
            bool Enabled = true;
            Message CurrentMessage = null;
            Message LastMessage = null;
            bool IsFirst = true;
            while (Enabled)
            {
                Thread.Sleep(1000);
                try
                {
                    if (IsFirst)
                    {
                        LastMessage = vk.Messages.Get(new MessagesGetParams
                        {
                            Count = 1
                        }).Messages[0];
                        IsFirst = false;
                    }
                    else
                    {
                        CurrentMessage = vk.Messages.Get(new MessagesGetParams
                        {
                            Count = 1,
                            Out = MessageType.Received
                        }).Messages[0];

                        if (CurrentMessage.UserId != BotID && CurrentMessage.Date != LastMessage.Date && CurrentMessage.ChatId == ConfID)
                        {
                            Command(Convert.ToInt32(CurrentMessage.UserId), CurrentMessage.Body, Convert.ToInt32(CurrentMessage.ChatId));
                        }
                        LastMessage = CurrentMessage;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при мониторинге сообщений: {ex}");
                    Enabled = false;
                }
            }
        }
        static void Command(long checkedUserID, string Message, long ConfId)
        {
            User Sender = vk.Users.Get(checkedUserID, ProfileFields.Sex | ProfileFields.FirstName | ProfileFields.LastName);
            string SenderName = $"{Sender.FirstName} {Sender.LastName}";
            bool Contains = false;
            string SendedMessage;
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
                    case 5:
                        Console.WriteLine($"Начинаем лайкать стену пользователя: {SenderName}");
                        LikeWall(Sender.Id, ConfId, SenderName);
                        break;
                    case 8:
                        Console.WriteLine($"Пробуем добавить в друзья пользователя: {SenderName}");
                        try
                        {
                            vk.Friends.Add(Sender.Id);
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
                        LikePhotos(Sender.Id, ConfId, SenderName);
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
                try
                {
                    var temp = posts.WallPosts[i].Id;
                    Thread.Sleep(2000);
                    vk.Likes.Add(new LikesAddParams
                    {
                        OwnerId = LikedUser,
                        ItemId = long.Parse(temp.ToString()),
                        Type = LikeObjectType.Post
                    });
                    GoodLikes++;
                    Console.WriteLine($"Успех, {i}/{posts.WallPosts.Count}");
                }
                catch(Exception ex)
                {
                    Errors++;
                    Console.WriteLine($"ОШИБКА, {i}/{posts.WallPosts.Count}");
                    if (ex is VkNet.Exception.CaptchaNeededException)
                        break;
                    continue;
                }
            }
            SendToConf($"Процесс завершен. Успешных лайков стены: {GoodLikes}. Ошибок: {Errors}", ChatId);
            Console.WriteLine($"Пользователь {SenderName} пролайкан. Успешных лайков стены: {GoodLikes}. Ошибок: {Errors}", ChatId);
            GoodLikes = 0;
            Errors = 0;
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
                try
                {
                    var temp = photos[Convert.ToInt32(i)].Id;
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
                    Errors++;
                    if (ex is VkNet.Exception.CaptchaNeededException)
                        break;
                    continue;
                }
            }
            SendToConf($"Процесс завершен. Успешных лайков фотографий: {GoodLikes}. Ошибок: {Errors}", ChatId);
            Console.WriteLine($"Пользователь {SenderName} пролайкан. Успешных лайков фотографий: {GoodLikes}. Ошибок: {Errors}");
            GoodLikes = 0;
            Errors = 0;
        }
        static void GetAndSetConfs(out int ConfID)
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