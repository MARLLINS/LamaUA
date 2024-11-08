using System.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace LamaUA
{
    class LamaBot
    {
        static Dictionary<long, UserData> userData = new Dictionary<long, UserData>();

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            string? token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN", EnvironmentVariableTarget.User);

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("You need to provide the Telegram bot token in the environment variables!");
                return;
            }

            var bot = new TelegramBotClient(token);

            using var cts = new CancellationTokenSource();

            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                cancellationToken: cts.Token
            );

            var me = await bot.GetMe();
            Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");

            Console.ReadLine();
            cts.Cancel();
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;

            if (message == null || message.From == null || message.Text == null)
                return;

            long userId = message.From.Id;

            if (message.Text.StartsWith("/lama") || message.Text == Constants.STRING_MY_LAMA)
            {
                await ShowUserLamaProfile(botClient, userId, cancellationToken);
            }
            else if (message.Text.StartsWith("/start"))
            {
                if (!userData.ContainsKey(userId))
                {
                    var data = new UserData(isUserCalledCommandStart: true);
                    userData[userId] = data;

                    await botClient.SendMessage(
                        message.Chat.Id,
                        "Введите имя ламы",
                        cancellationToken: cancellationToken
                    );
                    return;
                }

                await botClient.SendMessage(
                    message.Chat.Id,
                    "Вы уже назвали свою ламу",
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                if (userData.TryGetValue(userId, out UserData? data) && data.isUserCalledCommandStart)
                {
                    string lamaName = message.Text;
                    data.userLamaData.name = lamaName;
                    data.isUserCalledCommandStart = false;
                    userData[userId] = data;

                    if (message?.Chat?.Id != null)
                    {
                        await botClient.SendMessage(
                            chatId: message.Chat.Id,
                            text: $"Вы установили своей ламе имя {lamaName}",
                            cancellationToken: cancellationToken
                        );
                    }
                }
                else
                {
                    await botClient.SendMessage(
                        message.Chat.Id,
                        $"Ты написал: {message.Text}",
                        cancellationToken: cancellationToken
                    );
                }
            }
        }

        private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }

        private static async Task ShowUserLamaProfile(ITelegramBotClient botClient, long userId, CancellationToken cancellationToken)
        {
            var filePath = "../../../images/lama.jpg";
            using var stream = System.IO.File.OpenRead(filePath);
            var inputFile = new InputFileStream(stream, "lama.jpg");

            if (userData.TryGetValue(userId, out UserData? data))
            {
                UserLamaData lamaData = data.userLamaData;

                await botClient.SendPhoto(
                    chatId: userId,
                    photo: inputFile,
                    caption: $"Ваша лама: {(string.IsNullOrEmpty(lamaData.name) ? "Имя не установлено" : lamaData.name)}\n" +
                             $"Здоровье: {lamaData.health}\n" +
                             $"Голод: {lamaData.hunger}",
                    cancellationToken: cancellationToken,
                    replyMarkup: new ReplyKeyboardMarkup(new[]
                    {
                        new KeyboardButton(Constants.STRING_MY_LAMA)
                    })
                    {
                        ResizeKeyboard = true
                    }
                );
            }
            else
            {
                await botClient.SendMessage(userId, "Лама не найдена!", cancellationToken: cancellationToken);
            }
        }
    }
}