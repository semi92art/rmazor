#### Коды ошибок
В коде клиента константы кодов ошибок хранятся в классе `ServerErrorCodes`.
| Код  | Сообщение                                   | В каком случае может возникнуть                              |
| :--- | :------------------------------------------ | :----------------------------------------------------------- |
| 1    | AccountEntity не найден по DeviceId         | При поиске аккаунта по Id устройства                         |
| 2    | Неверные логин или пароль                   | При попытке входа по логину и паролю                         |
| 3    | Entity с таким AccountId и GameId не найден | В случае, если в таблице DataFieldValue нет совпадений по полям AccountId и GameId |
| 4    | Запрос составлен неправильно                | В случае неправильного составления запроса к WebAPI          |
| 5    | Валидация базы данных завершилась неудачей  | При перезаписи полей любой из таблиц БД                      |
| 6    | Account с таким Name уже существует         | При регистрации аккаутна по логину и паролю                  |
| 7    | Account с таким DeviceId уже существует     | При регистрации гостевого аккаунта по Id устройства          |



### Команды на боевом сервере
- Пул образа из DockerHub + запуск: `/home/artem/Documents/refresh_clapi1.sh`

### DotNet EF Core
#### Команды
- Создание миграции: `dotnet ef migrations add MigrationName`
- Обновление БД по миграции: `dotnet ef database update MigrationName`
- Обновление утилит dotnet: `dotnet tool update --global dotnet-ef`

### Docker
#### Команды
- Показать все запущенные контейнеры: `sudo docker container ls -a`
- Запустить контейнер clickersapi: `sudo docker run --rm -p 7000:80 drago1/clickersapi`
- Загрузить контейнер с хаба: `sudo docker pull drago1/clickersapi`
 - Остановить контейнер: `sudo docker stop [id контейнера]`
#### Полезные ссылки
- Просмотр контейнеров: [stackoverflow 1](https://stackoverflow.com/questions/16840409/how-to-list-containers-in-docker) 
- 
### Другое
#### Полезные команды

- Системный мониторинг: `glances`
- Убить процесс через PID: `sudo kill -9 [PID]`
- Запустить процесс в бэкграунде, который будет выполнять файл каждые 60 секунд: `setsid forever watch -n 60 [путь к файлу]`
- Отобразить информацию о процессе, запущенном в бэкграунде, выполняющем файл: `ps -auxwww --sort=start_time | grep -i [имя файла]`

- Сброс настроек сети: `sudo iptables -F`

- Проверка статуса файрвола: `sudo ufw status`

- ##### Удаление пользователя

  1. Ищем все процессы, запущенные пользователем через `ps -aux | grep [имя пользователя]`
  2. Удостоверяемся, что не запущены процессы, влияющие на работу системы и убиваем их через `sudo pkill -u [имя пользователя]`
  3. Еще раз проверяем, остались ли запущенные пользователем процессы с помощью команды из п.1. Если таковые остались, убиваем каждый через `sudo kill -9 [PID процесса]`
  4. Удаляем пользователя с помощью команды `sudo deluser [имя пользователя]`
#### Полезные ссылки
- Посмотреть порты: [askubuntu](https://askubuntu.com/questions/538208/how-to-check-opened-closed-ports-on-my-computer)
- Настройка rdp: [youtube](<https://www.youtube.com/watch?v=a0p0y1bN8Tw>)
- Установка dokuwiki: [ссылка 1](https://www.linuxcloudvps.com/blog/how-to-install-dokuwiki-on-ubuntu-18-04/)
- Устранение проблемы "Enter password to unlock your login keyring": [ссылка 1](http://wiki.onsever.ru/linux/ubuntu_zaprashivaet_parol_-_enter_password_to_unlock_your_login_keyring), [ссылка 2](https://askubuntu.com/questions/495957/how-to-disable-the-unlock-your-keyring-popup)
