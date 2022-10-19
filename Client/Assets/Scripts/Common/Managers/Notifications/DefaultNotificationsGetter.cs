using System;
using System.Collections.Generic;
using Common.Enums;

namespace Common.Managers.Notifications
{
    public static class DefaultNotificationsGetter
    {
        public static List<NotificationInfoEx> GetNotifications()
        {
            var result = new List<NotificationInfoEx>
            {
                new NotificationInfoEx
                {
                    Title = new Dictionary<ELanguage, string>
                    {
                        {ELanguage.English,  "🔥🔥🔥 Hurry up!"},  
                        {ELanguage.Russian,  "🔥🔥🔥 Поторопись!"},  
                        {ELanguage.German,   "🔥🔥🔥 Beeil dich!"},  
                        {ELanguage.Spanish,  "🔥🔥🔥 ¡Apresúrate!"},  
                        {ELanguage.Portugal, "🔥🔥🔥 Se apresse!"},  
                        {ELanguage.Korean,   "🔥🔥🔥 서둘러!"},  
                        {ELanguage.Japanese, "🔥🔥🔥 急げ！"},  
                    },
                    Body = new Dictionary<ELanguage, string>
                    {
                        {ELanguage.English,  "🔥🔥🔥 Next maze is waiting for you!\n Colorize get a reward! 🏆🏆🏆"},
                        {ELanguage.Russian,  "🔥🔥🔥 Следующий лабиринт ждет тебя!\n Раскрась и получи награду! 🏆🏆🏆"},
                        {ELanguage.German,   "🔥🔥🔥 Das nächste Labyrinth wartet auf dich!\n Vervollständigen Sie und erhalten Sie eine Belohnung! 🏆🏆🏆"},
                        {ELanguage.Spanish,  "🔥🔥🔥 ¡El siguiente laberinto te está esperando!\n ¡Coloríelo y obtenga una recompensa! 🏆🏆🏆"},
                        {ELanguage.Portugal, "🔥🔥🔥 O próximo labirinto está esperando por você!\n Colorize e receba uma recompensa! 🏆🏆🏆"},
                        {ELanguage.Korean,   "🔥🔥🔥 다음 미로가 당신을 기다리고 있습니다!\n 색칠하고 보상을 받으십시오! 🏆🏆🏆"},
                        {ELanguage.Japanese, "🔥🔥🔥 次の迷路があなたを待っています\n それを着色して報酬を得てください！ 🏆🏆🏆"},
                    },
                    Span = TimeSpan.FromDays(1)
                },
                new NotificationInfoEx
                {
                    Title = new Dictionary<ELanguage, string>
                    {
                        {ELanguage.English,  "🎨🎨🎨 What a good day to colorize new maze!"},  
                        {ELanguage.Russian,  "🎨🎨🎨 Какой хороший день, чтобы раскрасить новый лабиринт!"},  
                        {ELanguage.German,   "🎨🎨🎨 Was für ein guter Tag, um neues Labyrinth zu fördern!"},  
                        {ELanguage.Spanish,  "🎨🎨🎨 ¡Qué buen día para colorear el nuevo laberinto!"},  
                        {ELanguage.Portugal, "🎨🎨🎨 Que bom dia para colorizar o novo labirinto!"},  
                        {ELanguage.Korean,   "🎨🎨🎨 새로운 미로를 채색하기에 좋은 날!"},  
                        {ELanguage.Japanese, "🎨🎨🎨 新しい迷路を着色するのになんて良い日でしょう！"},  
                    },
                    Body = new Dictionary<ELanguage, string>
                    {
                        {ELanguage.English,  "🔥🔥🔥 Solve the next maze puzzle!\n Don't give up and you'll find out what happens next! 🔥🔥🔥"},
                        {ELanguage.Russian,  "🔥🔥🔥 Разгадай следующую головоломку-лабиринт!\n Не сдавайтесь, и ты узнаете, что будет дальше! 🔥🔥🔥"},
                        {ELanguage.German,   "🔥🔥🔥 Lösen Sie das nächste Labyrinth -Puzzle! Gib nicht auf und du wirst herausfinden, was als nächstes passiert! 🔥🔥🔥"},
                        {ELanguage.Spanish,  "🔥🔥🔥 ¡Resuelve el próximo rompecabezas de laberinto!\n ¡No te rindas y descubrirás qué pasa después! 🔥🔥🔥"},
                        {ELanguage.Portugal, "🔥🔥🔥 Resolva o próximo quebra -cabeça do labirinto!\n Não desista e você descobrirá o que acontece a seguir! 🔥🔥🔥"},
                        {ELanguage.Korean,   "🔥🔥🔥 다음 미로 퍼즐을 해결하십시오!\n 포기하지 말고 다음에 무슨 일이 일어나는지 알게 될 것입니다! 🔥🔥🔥"},
                        {ELanguage.Japanese, "🔥🔥🔥 次の迷路パズルを解決してください！\n あきらめないでください、そしてあなたは次に何が起こるかを知るでしょう！ 🔥🔥🔥"},
                    },
                    Span = TimeSpan.FromDays(3)
                },
                new NotificationInfoEx
                {
                    Title = new Dictionary<ELanguage, string>
                    {
                        {ELanguage.English,  "🔥🔥🔥 Come back! Next maze is waiting for you!"},  
                        {ELanguage.Russian,  "🔥🔥🔥 Вернись! Следующий лабиринт ждет тебя!"},  
                        {ELanguage.German,   "🔥🔥🔥 Komm zurück! Das nächste Labyrinth wartet auf dich!"},  
                        {ELanguage.Spanish,  "🔥🔥🔥 ¡Regresar! ¡El siguiente laberinto te está esperando!"},  
                        {ELanguage.Portugal, "🔥🔥🔥 Volte! O próximo labirinto está esperando por você!"},  
                        {ELanguage.Korean,   "🔥🔥🔥 돌아와! 다음 미로가 당신을 기다리고 있습니다!"},  
                        {ELanguage.Japanese, "🔥🔥🔥 戻ってくる！次の迷路があなたを待っています！"},  
                    },
                    Body = new Dictionary<ELanguage, string>
                    {
                        {ELanguage.English,  "🔥🔥🔥 Next maze puzzle will blow your mind!\n Don't delay! Colorize it! 🎨🎨🎨"},
                        {ELanguage.Russian,  "🔥🔥🔥 Следующая головоломка-лабиринт взорвёт твой мозг!\n Не откладывай! Раскрась его! 🎨🎨🎨"},
                        {ELanguage.German,   "🔥🔥🔥 Das nächste Labyrinth -Puzzle wird dich umhauen!\n Verzögerung nicht! Färbe es! 🎨🎨🎨"},
                        {ELanguage.Spanish,  "🔥🔥🔥 ¡El próximo rompecabezas del laberinto te sorprenderá!\n ¡No se demore! ¡Colorealo! 🎨🎨🎨"},
                        {ELanguage.Portugal, "🔥🔥🔥 Próximo Puzzle do Maze vai explodir sua mente\n! Não demore! Pinte-o!"},
                        {ELanguage.Korean,   "🔥🔥🔥 다음 미로 퍼즐은 당신의 마음을 날려 버릴 것입니다!\n 지체하지 마십시오! 색을 입으세요! 🎨🎨🎨"},
                        {ELanguage.Japanese, "🔥🔥🔥 次の迷路パズルはあなたの心を吹き飛ばします！\n 遅らせないでください！色！ 🎨🎨🎨"},
                    },
                    Span = TimeSpan.FromDays(5)
                },
            };
            return result;
        }
    }
}