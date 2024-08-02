# Тестовое задание FS

Приветствую, дорогой коллега, который проверяет данное тестовое задание!

Для его выполнения требовалась база данных. У моего компьютера в этом году юбилей, в октябре ему будет 10 лет, поэтому очередной установки СУБД он не выдержит, обидится, и не даст мне возможности программировать. Использовать урезанные in memory базы данных типа SQLite я тоже не стал. Обычно работа с ними очень быстро заканчивается на странице справки, где говорится о том, что искомая функция не поддерживается. Но у меня уже был установлен докер, и я решил, что базу данных буду запускать в нём. Я использую postgres.

Для удобства, это тестовое задание реализовано в формате проекта docker compose для Visual Studio. Задание состоит из трёх задач. Решение каждой задачи представляет собой веб сервис `Task<N>.Api`.

Предполагается, что решение будет просматриваться в `Debug` режиме, чтобы работал сваггер.

Для просмотра решения этого задания на вашем компьютере с ОС Windows должны быть установлены:
- Visual Studio 2022;
- Docker Desktop;
- **(Опционально)** Средство доступа к СУБД. Я использую DBeaver Community Edition.

Согласно настройкам в docker compose решение использует порты `5050`, `5432`, `8080` - `8085`.

Контейнеры можно отключать, закомментировав их конфигурацию перед запуском в файлах `docker-compose.yml` и `docker-compose.override.yml`. В этих же файлах можно поменять используемые порты. Для одной задачи достаточно её контейнера и контейнера с базой. Если у вас слабый компьютер, вы можете раскомментировать лимиты ресурсов в этих файлах.

В решение включен контейнер с pgadmin, для проверки запросов к базе. Он доступен по адресу: http://localhost:5050. Мне приходилось немного подождать, пока он загрузится при первом запуске. Контейнер находится в сети докера, поэтому для подключения можно использовать те же параметры, которые используют другие контейнеры: `Host=postgres;Database=testdb;Username=postgres;Password=postgres`.

Так же у вас есть возможность проверить сервисы отдельно, без докера. Для этого необходимо прописать строку доступа к СУБД postgres в файлах `appsettings.json` в поле `DbConnectionString`. **База данных не должна содержать таблиц**.

Приятного просмотра!

## Задача 1

### Условие

Необходимо реализовать веб-сервис на Asp.Net Core. Информация о запросах и ответов
методов должна логгироваться в БД.

#### Серверная часть

Разработать 2 метода API по технологии REST

##### 1 метод

Данный метод предназначен для сохранения данных в БД

Краткое описание:
* Данный метод получает на вход json вида
	```
	[
		{"1": "value1"},
		{"5": "value2"},
		{"10": "value32"},
		…
	]
	```
* Преобразует его к объекту вида:
	- code – код. Тип int.
	- value – значение. Тип string.
* Полученный массив необходимо отсортировать по полю code и сохранить в БД (в решении необходимо описать структуру таблицы).

В таблице должно быть 3 поля:
* порядковый номер
* код
* значение

Перед сохранением данных таблицу необходимо очистить.

##### 2 метод

Возвращает данные клиенту из таблицы в виде json.

Возвращаемые данные:
* порядковый номер
* код
* значение

Добавить возможность фильтрации возвращаемых данных.

### Решение

Описание API доступно при запущеном проекте по адресам:
* http://localhost:8080/swagger/index.html
* https://localhost:8081/swagger/index.html

Для метода сохранения данных я выбрал глагол `PUT`, потому, что метод идемпотентный. На одинаковых входных данных приводит к одинаковому результату.

Для метода поиска я выбрал метод `POST`, потому, что во первых `GET` не позволяет использовать структурированные параметры в body запроса, а так же мотод не идемпотентный - если данные в базе изменятся на одинаковых параметрах он будет выдавать разные ответы. Если оставить параметр поиска пустым - вернутся все данные из таблицы. Если оставить любое из полей параметров поиска `null` - оно игнорируется. В Swagger удобнее всего просто писать только значимые параметры. Например, чтобы найти первые 5 записей, где в поле `value` содержится слово *Привет* (регистр игнорируется), параметры поиска должны выглядеть так:

```
{
  "take": 5,
  "valueSubstring": "привет"
}
```

Приложение использует следующие таблицы в базе данных:
* `Records` - таблица с данными. В задаче не указана длина поля value, поэтому в базе это text - строка неограниченной длины.
* `Logs` - таблица с логами. Вас интересует значения, начинающиеся с `Request and Response` в поле `message`.

Я придерживался clean architecture, где во главе угла стоит бизнес логика. Это проект `Task1.Domain`. Он реализует всю бизнес логику приложения, и предоставляет интерфейсы для внешних слоёв. Он не ссылается на другие компоненты приложения.

На проект с бизнес логикой ссылается слой доступа к данным `Task1.DataAccess`. Изначально он содержал модели уровня DA и контекст Entiy Framework, но из-за сложностей с использованием множества контекстов (для каждой задачи) в одной БД пришлось вынести все модели в один контекст. Так появился проект `Common`. Иначе метод `Database.EnsureCreated()` не создаёт таблицы.

Проект `Task1.Api` представляет собой веб приложение, которое собирает всё воедино и предоставляет API с требуемыми в задаче методами. Так же этот проект отвественнен за создание БД. Он это делает до логирования, потому, что если Serilog создаст таблицу `Logs` раньше, `EnsureCreated()` откажется создавать свои таблицы. По этой причне в `docker-compose.yml` проект зависит от контейнера с базой данных, а от него зависят 2 другие задачи. Тем не менее, если запускать другие задачи отдельно, они смогут создать нужные таблицы. Настройки логгера скопированы из инструкций на официальном сайте Serilog.

## Задача 2

Даны таблицы:

```
Clients - клиенты
(
Id bigint, -- Id клиента
ClientName nvarchar(200) -- Наименование клиента
);
```

```
ClientContacts - контакты клиентов
(
Id bigint, -- Id контакта
ClientId bigint, -- Id клиента
ContactType nvarchar(255), -- тип контакта
ContactValue nvarchar(255) -- значение контакта
);
```

1. Написать запрос, который возвращает наименование клиентов и кол-во
контактов клиентов
2. Написать запрос, который возвращает список клиентов, у которых есть более 2
контактов

### Решение

Для генерации тестовых данных реализован метод API, который нужно вызвать из интерфейса Swagger:
* http://localhost:8082/swagger/index.html
* https://localhost:8083/swagger/index.html

Где `numberOfClients` количество клиентов, `numberOfContacts` количество контактов, которые случайно распределятся между клиентами, `randomSeed` - опциональный параметр, если вам нужны повторяемые результаты. Я пробовал до миллиона клиентов, и десятков тысяч контактов.

В этой задаче мы используем таблицы `Clients` и `Contacts`. Ошибся в наименовании, переделывать не стал, потому, что смысл сохраняется.

1.
```
SELECT CL."ClientName", COUNT(CO."ContactId")
FROM public."Contacts" AS CO
RIGHT JOIN public."Clients" AS CL ON CO."ClientId" = CL."ClientId"
GROUP BY CL."ClientId";
```

2.
```
SELECT CL."ClientName", COUNT(CO."ContactId")
FROM public."Contacts" AS CO
JOIN public."Clients" AS CL ON CO."ClientId" = CL."ClientId"
GROUP BY CL."ClientId"
HAVING COUNT(CO."ContactId") > 2;
```

## Задача 3 (опционально)

Дана таблица:

```
Dates - клиенты
(
Id bigint,
Dt date
);
```

Написать запрос, который возвращает интервалы для одинаковых Id.

### Решение

Аналогично предыдущей задаче, необходимо сгенерировать данные с помощью вызова API:
* http://localhost:8084/swagger/index.html
* https://localhost:8085/swagger/index.html

Где `numberOfClients` число клиентов, `numberOfDates` - количество отметок времени, которое случайно распределяется между клиентами, `randomSeed` - опциональный параметр если вам нужны повторяемые результаты. Так же пробовал до миллиона отметок времени для 10 клиентов.

В решении используется таблица `Dates`, где:
* `Id` - суррогатный ключ, который не используется в решении и нужен только чтобы упростить настройку Entity Framework.
* `ClientId` - идентификатор клиента. Таблица с клиентами отсуствует, для упрощения модели, но теоретически это внешний ключ.
* `Date` - отметка времени.

В задании не сказано, что таблица (без учёта суррогатного ключа) находится во второй нормальной форме. Учитывая, что возникновение у одного пользователя двух событий в один день весьма вероятно, представляю 2 решения для событий с повторами и без.

1. Для таблицы без повторов, или повторы не важны:

```
SELECT *
FROM (
	SELECT "ClientId", "Date", LEAD("Date")
	OVER (
		PARTITION BY "ClientId"
		ORDER BY "Date") AS "EndTime"
	FROM public."Dates"
	)
WHERE "EndTime" IS NOT NULL;
```

2. Для табиц с повторами:

```
SELECT "ClientId", "Date", "EndTime"
FROM (
	SELECT "ClientId", "Date",
		LEAD("Date") OVER
			(
			PARTITION BY "ClientId"
			ORDER BY "Date"
			) AS "EndTime"
	FROM (
		SELECT DISTINCT "ClientId", "Date"
		FROM public."Dates"
		)
	)
WHERE "EndTime" IS NOT NULL;
```