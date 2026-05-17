using System;
using System.Collections.Generic;
using Npgsql;

namespace HairdresserSystem
{
    public class DatabaseAdapter
    {
        private string connectionString = "Server=localhost;Port=5432;Database=HairdresserStyle;User Id=postgres;Password=123;";

        /// <summary>
        /// Проверка подключения к базе данных
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public string GetConnectionString()
        {
            return connectionString;
        }


        // =====================================================
        // 1. РЕГИСТРАЦИЯ КЛИЕНТА
        // =====================================================
        public bool RegisterClient(string login, string password, string fullName, string phone, string email)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Проверяем уникальность логина
                        string checkLoginSql = "SELECT COUNT(*) FROM Users WHERE Login = @login";
                        using (NpgsqlCommand cmd = new NpgsqlCommand(checkLoginSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@login", login);
                            long count = (long)cmd.ExecuteScalar();
                            if (count > 0)
                            {
                                Console.WriteLine($"Логин '{login}' уже существует");
                                return false;
                            }
                        }

                        // Проверяем уникальность телефона
                        string checkPhoneSql = "SELECT COUNT(*) FROM Clients WHERE Phone = @phone";
                        using (NpgsqlCommand cmd = new NpgsqlCommand(checkPhoneSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@phone", phone);
                            long count = (long)cmd.ExecuteScalar();
                            if (count > 0)
                            {
                                Console.WriteLine($"Телефон '{phone}' уже существует");
                                return false;
                            }
                        }

                        // Добавляем в Users
                        string userSql = "INSERT INTO Users (Login, PasswordHash, Role) VALUES (@login, @pass, 'Client') RETURNING UserID";
                        int userId;
                        using (NpgsqlCommand cmd = new NpgsqlCommand(userSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@login", login);
                            cmd.Parameters.AddWithValue("@pass", password);
                            userId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // Добавляем в Clients
                        string clientSql = "INSERT INTO Clients (UserID, FullName, Phone, Email) VALUES (@userId, @name, @phone, @email)";
                        using (NpgsqlCommand cmd = new NpgsqlCommand(clientSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@userId", userId);
                            cmd.Parameters.AddWithValue("@name", fullName);
                            cmd.Parameters.AddWithValue("@phone", phone);
                            cmd.Parameters.AddWithValue("@email", email);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Ошибка в RegisterClient: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        // =====================================================
        // 2. АВТОРИЗАЦИЯ (Проверка логина/пароля, получение роли и ID)
        // =====================================================
        public (bool success, string role, int userId) Login(string login, string password)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT UserID, Role FROM Users WHERE Login = @login AND PasswordHash = @pass";
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@login", login);
                    cmd.Parameters.AddWithValue("@pass", password);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return (true, reader.GetString(1), reader.GetInt32(0));
                        }
                    }
                }
            }
            return (false, null, -1);
        }

        // =====================================================
        // 3. ПОЛУЧИТЬ СВОБОДНЫЕ СЛОТЫ (с учетом мастеров И рабочих мест)
        // =====================================================
        public List<FreeSlot> GetFreeSlots(int serviceId, DateTime date)
        {
            List<FreeSlot> freeSlots = new List<FreeSlot>();

            // 1. Сначала узнаем длительность услуги
            int duration = 0;
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string serviceSql = "SELECT DurationMinutes FROM Services WHERE ServiceID = @sid AND IsActive = TRUE";
                using (NpgsqlCommand cmd = new NpgsqlCommand(serviceSql, conn))
                {
                    cmd.Parameters.AddWithValue("@sid", serviceId);
                    var result = cmd.ExecuteScalar();
                    if (result == null) return freeSlots;
                    duration = Convert.ToInt32(result);
                }

                // 2. Находим всех мастеров, которые могут делать эту услугу
                string mastersSql = @"SELECT m.MasterID, m.FullName FROM Masters m
                                      JOIN MasterSpecializations ms ON m.MasterID = ms.MasterID
                                      WHERE ms.ServiceID = @sid AND m.IsActive = TRUE";

                List<MasterInfo> masters = new List<MasterInfo>();
                using (NpgsqlCommand cmd = new NpgsqlCommand(mastersSql, conn))
                {
                    cmd.Parameters.AddWithValue("@sid", serviceId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            masters.Add(new MasterInfo
                            {
                                MasterID = reader.GetInt32(0),
                                FullName = reader.GetString(1)
                            });
                        }
                    }
                }

                // 3. Для каждого мастера ищем свободные рабочие места и время
                foreach (var master in masters)
                {
                    // Получаем занятые слоты мастера
                    string busySql = @"SELECT StartDateTime FROM Appointments 
                                       WHERE MasterID = @mid AND Date(StartDateTime) = @date";
                    HashSet<DateTime> busyStartTimes = new HashSet<DateTime>();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(busySql, conn))
                    {
                        cmd.Parameters.AddWithValue("@mid", master.MasterID);
                        cmd.Parameters.AddWithValue("@date", date);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                busyStartTimes.Add(reader.GetDateTime(0));
                            }
                        }
                    }

                    // Получаем занятые рабочие места
                    string busyWorkplaceSql = @"SELECT WorkplaceID, StartDateTime FROM Appointments 
                                                WHERE Date(StartDateTime) = @date";
                    Dictionary<int, HashSet<DateTime>> busyWorkplaces = new Dictionary<int, HashSet<DateTime>>();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(busyWorkplaceSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@date", date);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int wpId = reader.GetInt32(0);
                                DateTime start = reader.GetDateTime(1);
                                if (!busyWorkplaces.ContainsKey(wpId))
                                    busyWorkplaces[wpId] = new HashSet<DateTime>();
                                busyWorkplaces[wpId].Add(start);
                            }
                        }
                    }

                    // Получаем список всех рабочих мест
                    string workplacesSql = "SELECT WorkplaceID, Title FROM Workplaces";
                    List<WorkplaceInfo> workplaces = new List<WorkplaceInfo>();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(workplacesSql, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                workplaces.Add(new WorkplaceInfo
                                {
                                    WorkplaceID = reader.GetInt32(0),
                                    Title = reader.GetString(1)
                                });
                            }
                        }
                    }

                    // Генерируем слоты с 9:00 до 21:00
                    DateTime startOfDay = date.Date.AddHours(9);
                    DateTime endOfDay = date.Date.AddHours(21);

                    for (DateTime slotStart = startOfDay; slotStart.AddMinutes(duration) <= endOfDay; slotStart = slotStart.AddMinutes(30))
                    {
                        // Проверяем, свободен ли мастер
                        if (busyStartTimes.Contains(slotStart))
                            continue;

                        // Ищем свободное рабочее место
                        foreach (var workplace in workplaces)
                        {
                            if (!busyWorkplaces.ContainsKey(workplace.WorkplaceID) ||
                                !busyWorkplaces[workplace.WorkplaceID].Contains(slotStart))
                            {
                                freeSlots.Add(new FreeSlot
                                {
                                    MasterID = master.MasterID,
                                    MasterName = master.FullName,
                                    WorkplaceID = workplace.WorkplaceID,
                                    WorkplaceTitle = workplace.Title,
                                    StartTime = slotStart,
                                    EndTime = slotStart.AddMinutes(duration)
                                });
                                break; // Нашли место - хватит
                            }
                        }
                    }
                }
            }
            return freeSlots;
        }

        // =====================================================
        // 4. СОЗДАТЬ ЗАПИСЬ
        // =====================================================
        public bool MakeAppointment(int clientId, int masterId, int workplaceId, int serviceId, DateTime startTime)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // Получаем длительность услуги
                string durationSql = "SELECT DurationMinutes FROM Services WHERE ServiceID = @sid";
                int duration;
                using (NpgsqlCommand cmd = new NpgsqlCommand(durationSql, conn))
                {
                    cmd.Parameters.AddWithValue("@sid", serviceId);
                    duration = Convert.ToInt32(cmd.ExecuteScalar());
                }

                DateTime endTime = startTime.AddMinutes(duration);

                // Проверяем, не занят ли мастер
                string checkMasterSql = "SELECT COUNT(*) FROM Appointments WHERE MasterID = @mid AND StartDateTime = @start";
                using (NpgsqlCommand cmd = new NpgsqlCommand(checkMasterSql, conn))
                {
                    cmd.Parameters.AddWithValue("@mid", masterId);
                    cmd.Parameters.AddWithValue("@start", startTime);
                    if ((long)cmd.ExecuteScalar() > 0)
                        return false;
                }

                // Проверяем, не занято ли рабочее место
                string checkWorkplaceSql = "SELECT COUNT(*) FROM Appointments WHERE WorkplaceID = @wid AND StartDateTime = @start";
                using (NpgsqlCommand cmd = new NpgsqlCommand(checkWorkplaceSql, conn))
                {
                    cmd.Parameters.AddWithValue("@wid", workplaceId);
                    cmd.Parameters.AddWithValue("@start", startTime);
                    if ((long)cmd.ExecuteScalar() > 0)
                        return false;
                }

                // Создаем запись
                string insertSql = @"INSERT INTO Appointments 
                                    (StartDateTime, EndDateTime, ClientID, MasterID, WorkplaceID, ServiceID, Status)
                                    VALUES (@start, @end, @cid, @mid, @wid, @sid, 'Scheduled')";

                using (NpgsqlCommand cmd = new NpgsqlCommand(insertSql, conn))
                {
                    cmd.Parameters.AddWithValue("@start", startTime);
                    cmd.Parameters.AddWithValue("@end", endTime);
                    cmd.Parameters.AddWithValue("@cid", clientId);
                    cmd.Parameters.AddWithValue("@mid", masterId);
                    cmd.Parameters.AddWithValue("@wid", workplaceId);
                    cmd.Parameters.AddWithValue("@sid", serviceId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // =====================================================
        // 5. ПОЛУЧИТЬ ЗАПИСИ КЛИЕНТА (возвращает список)
        // =====================================================
        public List<(int id, DateTime start, DateTime end, string service, string master, string workplace, string status)>
            GetClientAppointments(int clientId)
        {
            var appointments = new List<(int, DateTime, DateTime, string, string, string, string)>();

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"SELECT a.AppointmentID, a.StartDateTime, a.EndDateTime, 
                              s.Title, m.FullName, w.Title, a.Status
                       FROM Appointments a
                       JOIN Services s ON a.ServiceID = s.ServiceID
                       JOIN Masters m ON a.MasterID = m.MasterID
                       JOIN Workplaces w ON a.WorkplaceID = w.WorkplaceID
                       WHERE a.ClientID = @cid
                       ORDER BY a.StartDateTime DESC";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@cid", clientId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            appointments.Add((
                                reader.GetInt32(0),     // id
                                reader.GetDateTime(1),  // start
                                reader.GetDateTime(2),  // end
                                reader.GetString(3),    // service
                                reader.GetString(4),    // master
                                reader.GetString(5),    // workplace
                                reader.GetString(6)     // status
                            ));
                        }
                    }
                }
            }
            return appointments;
        }

        // =====================================================
        // 6. ОТМЕНИТЬ ЗАПИСЬ
        // =====================================================
        public bool CancelAppointment(int appointmentId)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = "UPDATE Appointments SET Status = 'Canceled' WHERE AppointmentID = @aid";
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@aid", appointmentId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
        // =====================================================
        // 6. ПРОСМОТРЕТЬ РАСПИСАНИЕ МАСТЕРА (возвращает список)
        // =====================================================
        public List<(int id, DateTime start, DateTime end, string clientName, string service, string workplace, string status)>
            GetMasterSchedule(int masterId, DateTime date)
        {
            var schedule = new List<(int, DateTime, DateTime, string, string, string, string)>();

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"SELECT a.AppointmentID, a.StartDateTime, a.EndDateTime,
                              c.FullName, s.Title, w.Title, a.Status
                       FROM Appointments a
                       JOIN Clients c ON a.ClientID = c.ClientID
                       JOIN Services s ON a.ServiceID = s.ServiceID
                       JOIN Workplaces w ON a.WorkplaceID = w.WorkplaceID
                       WHERE a.MasterID = @mid AND DATE(a.StartDateTime) = @date
                       ORDER BY a.StartDateTime";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@mid", masterId);
                    cmd.Parameters.AddWithValue("@date", date);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            schedule.Add((
                                reader.GetInt32(0),     // id
                                reader.GetDateTime(1),  // start
                                reader.GetDateTime(2),  // end
                                reader.GetString(3),    // clientName
                                reader.GetString(4),    // service
                                reader.GetString(5),    // workplace
                                reader.GetString(6)     // status
                            ));
                        }
                    }
                }
            }
            return schedule;
        }

        // =====================================================
        // 7. ПРОСМОТРЕТЬ ОБЩЕЕ РАСПИСАНИЕ (АДМИН) (возвращает список)
        // =====================================================
        public List<(DateTime start, DateTime end, string masterName, string clientName, string service, string workplace, string status)>
            GetGeneralSchedule(DateTime date)
        {
            var schedule = new List<(DateTime, DateTime, string, string, string, string, string)>();

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"SELECT a.StartDateTime, a.EndDateTime,
                              m.FullName, c.FullName, s.Title, w.Title, a.Status
                       FROM Appointments a
                       JOIN Masters m ON a.MasterID = m.MasterID
                       JOIN Clients c ON a.ClientID = c.ClientID
                       JOIN Services s ON a.ServiceID = s.ServiceID
                       JOIN Workplaces w ON a.WorkplaceID = w.WorkplaceID
                       WHERE DATE(a.StartDateTime) = @date
                       ORDER BY a.StartDateTime";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@date", date);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            schedule.Add((
                                reader.GetDateTime(0),  // start
                                reader.GetDateTime(1),  // end
                                reader.GetString(2),    // masterName
                                reader.GetString(3),    // clientName
                                reader.GetString(4),    // service
                                reader.GetString(5),    // workplace
                                reader.GetString(6)     // status
                            ));
                        }
                    }
                }
            }
            return schedule;
        }

        // =====================================================
        // 8. СОЗДАТЬ ЗАПИСЬ ДЛЯ КЛИЕНТА (АДМИН) (п. 1.8)
        // =====================================================
        public (bool success, int clientId) FindOrCreateClient(string phone, string fullName = null)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // Сначала ищем клиента по телефону
                string findSql = @"SELECT c.ClientID, c.FullName 
                           FROM Clients c 
                           JOIN Users u ON c.UserID = u.UserID 
                           WHERE c.Phone = @phone";

                using (NpgsqlCommand cmd = new NpgsqlCommand(findSql, conn))
                {
                    cmd.Parameters.AddWithValue("@phone", phone);
                    var result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        return (true, Convert.ToInt32(result));
                    }
                }

                // Если не нашли и передано имя - создаем нового
                if (!string.IsNullOrEmpty(fullName))
                {
                    string login = $"client_{DateTime.Now.Ticks}";
                    string email = $"{login}@temp.ru";

                    bool regSuccess = RegisterClient(login, "temp123", fullName, phone, email);
                    if (regSuccess)
                    {
                        // Получаем ID нового клиента
                        using (NpgsqlCommand cmd = new NpgsqlCommand(findSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@phone", phone);
                            var newId = cmd.ExecuteScalar();
                            if (newId != null)
                                return (true, Convert.ToInt32(newId));
                        }
                    }
                }

                return (false, -1);
            }
        }

        // Админская версия создания записи (с поиском клиента)
        public bool AdminMakeAppointment(string clientPhone, string clientName, int masterId,
                                          int workplaceId, int serviceId, DateTime startTime)
        {
            // Находим или создаем клиента
            var (found, clientId) = FindOrCreateClient(clientPhone, clientName);
            if (!found)
            {
                Console.WriteLine("Клиент не найден и не может быть создан");
                return false;
            }

            // Создаем запись
            return MakeAppointment(clientId, masterId, workplaceId, serviceId, startTime);
        }

        // =====================================================
        // 9. ЗАВЕРШИТЬ ВИЗИТ (п. 1.9)
        // =====================================================
        public bool CompleteVisit(int appointmentId)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = "UPDATE Appointments SET Status = 'Completed' WHERE AppointmentID = @aid";
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@aid", appointmentId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // =====================================================
        // 10. ДОБАВИТЬ МАСТЕРА (п. 1.11)
        // =====================================================
        public bool AddMaster(string login, string password, string fullName, string phone, List<int> serviceIds = null)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Шаг 1: Проверяем, не существует ли уже такой логин
                        string checkLoginSql = "SELECT COUNT(*) FROM Users WHERE Login = @login";
                        using (NpgsqlCommand cmd = new NpgsqlCommand(checkLoginSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@login", login);
                            long count = (long)cmd.ExecuteScalar();
                            if (count > 0)
                            {
                                Console.WriteLine($"Логин '{login}' уже существует");
                                return false;
                            }
                        }

                        // Шаг 2: Проверяем, не существует ли уже такой телефон
                        string checkPhoneSql = "SELECT COUNT(*) FROM Masters WHERE Phone = @phone";
                        using (NpgsqlCommand cmd = new NpgsqlCommand(checkPhoneSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@phone", phone);
                            long count = (long)cmd.ExecuteScalar();
                            if (count > 0)
                            {
                                Console.WriteLine($"Телефон '{phone}' уже существует");
                                return false;
                            }
                        }

                        // Шаг 3: Добавляем в Users
                        string userSql = "INSERT INTO Users (Login, PasswordHash, Role) VALUES (@login, @pass, 'Master') RETURNING UserID";
                        int userId;
                        using (NpgsqlCommand cmd = new NpgsqlCommand(userSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@login", login);
                            cmd.Parameters.AddWithValue("@pass", password);
                            userId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // Шаг 4: Добавляем в Masters
                        string masterSql = "INSERT INTO Masters (UserID, FullName, Phone, IsActive) VALUES (@uid, @name, @phone, TRUE) RETURNING MasterID";
                        int masterId;
                        using (NpgsqlCommand cmd = new NpgsqlCommand(masterSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@uid", userId);
                            cmd.Parameters.AddWithValue("@name", fullName);
                            cmd.Parameters.AddWithValue("@phone", phone);
                            masterId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // Шаг 5: Добавляем специализации (если указаны)
                        if (serviceIds != null && serviceIds.Count > 0)
                        {
                            foreach (int serviceId in serviceIds)
                            {
                                string specSql = "INSERT INTO MasterSpecializations (MasterID, ServiceID) VALUES (@mid, @sid)";
                                using (NpgsqlCommand cmd = new NpgsqlCommand(specSql, conn))
                                {
                                    cmd.Parameters.AddWithValue("@mid", masterId);
                                    cmd.Parameters.AddWithValue("@sid", serviceId);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Ошибка в AddMaster: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        // =====================================================
        // ПОЛУЧИТЬ ВСЕХ МАСТЕРОВ
        // =====================================================
        public List<(int MasterID, string FullName, string Phone, bool IsActive)> GetAllMasters()
        {
            var masters = new List<(int, string, string, bool)>();
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT MasterID, FullName, Phone, IsActive FROM Masters ORDER BY FullName";
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        masters.Add((
                            reader.GetInt32(0),
                            reader.GetString(1),
                            reader.GetString(2),
                            reader.GetBoolean(3)
                        ));
                    }
                }
            }
            return masters;
        }

        // =====================================================
        // 11. ДЕАКТИВАЦИЯ МАСТЕРА (п. 1.12)
        // =====================================================
        public bool DeactivateMaster(int masterId)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // Проверяем, есть ли активные записи в будущем
                string checkSql = "SELECT COUNT(*) FROM Appointments WHERE MasterID = @mid AND Status = 'Scheduled' AND StartDateTime > NOW()";
                using (NpgsqlCommand cmd = new NpgsqlCommand(checkSql, conn))
                {
                    cmd.Parameters.AddWithValue("@mid", masterId);
                    long futureAppointments = (long)cmd.ExecuteScalar();

                    if (futureAppointments > 0)
                    {
                        Console.WriteLine($"Невозможно деактивировать мастера: у него {futureAppointments} будущих записей");
                        return false;
                    }
                }

                // Деактивируем мастера
                string sql = "UPDATE Masters SET IsActive = FALSE WHERE MasterID = @mid";
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@mid", masterId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // =====================================================
        // 12. УПРАВЛЕНИЕ СПЕЦИАЛИЗАЦИЕЙ МАСТЕРА (п. 1.13)
        // =====================================================
        public bool UpdateMasterSpecializations(int masterId, List<int> serviceIds)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Удаляем старые специализации
                        string deleteSql = "DELETE FROM MasterSpecializations WHERE MasterID = @mid";
                        using (NpgsqlCommand cmd = new NpgsqlCommand(deleteSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@mid", masterId);
                            cmd.ExecuteNonQuery();
                        }

                        // Добавляем новые
                        foreach (int serviceId in serviceIds)
                        {
                            string insertSql = "INSERT INTO MasterSpecializations (MasterID, ServiceID) VALUES (@mid, @sid)";
                            using (NpgsqlCommand cmd = new NpgsqlCommand(insertSql, conn))
                            {
                                cmd.Parameters.AddWithValue("@mid", masterId);
                                cmd.Parameters.AddWithValue("@sid", serviceId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public List<ServiceInfo> GetMasterSpecializations(int masterId)
        {
            List<ServiceInfo> services = new List<ServiceInfo>();

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"SELECT s.ServiceID, s.Title, s.Price, s.DurationMinutes
                       FROM Services s
                       JOIN MasterSpecializations ms ON s.ServiceID = ms.ServiceID
                       WHERE ms.MasterID = @mid AND s.IsActive = TRUE";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@mid", masterId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            services.Add(new ServiceInfo
                            {
                                ServiceID = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                Price = reader.GetDecimal(2),
                                DurationMinutes = reader.GetInt32(3)
                            });
                        }
                    }
                }
            }
            return services;
        }

        // =====================================================
        // 13. УПРАВЛЕНИЕ УСЛУГАМИ (п. 1.14)
        // =====================================================
        public bool AddService(string title, decimal price, int durationMinutes)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = "INSERT INTO Services (Title, Price, DurationMinutes, IsActive) VALUES (@title, @price, @dur, TRUE)";
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@dur", durationMinutes);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool UpdateService(int serviceId, string title, decimal price, int durationMinutes)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = "UPDATE Services SET Title = @title, Price = @price, DurationMinutes = @dur WHERE ServiceID = @sid";
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@sid", serviceId);
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@dur", durationMinutes);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteService(int serviceId)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                // Проверяем, есть ли будущие записи на эту услугу
                string checkSql = @"SELECT COUNT(*) FROM Appointments a 
                            WHERE a.ServiceID = @sid 
                            AND a.Status = 'Scheduled' 
                            AND a.StartDateTime > NOW()";
                using (NpgsqlCommand cmd = new NpgsqlCommand(checkSql, conn))
                {
                    cmd.Parameters.AddWithValue("@sid", serviceId);
                    long futureCount = (long)cmd.ExecuteScalar();

                    if (futureCount > 0)
                    {
                        Console.WriteLine($"Невозможно удалить услугу: есть {futureCount} будущих записей");
                        return false;
                    }
                }

                // Меняем статус на архивный (не удаляем физически)
                string sql = "UPDATE Services SET IsActive = FALSE WHERE ServiceID = @sid";
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@sid", serviceId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public List<ServiceInfo> GetAllServices(bool onlyActive = true)
        {
            List<ServiceInfo> services = new List<ServiceInfo>();

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string sql = onlyActive ?
                    "SELECT ServiceID, Title, Price, DurationMinutes FROM Services WHERE IsActive = TRUE ORDER BY Title" :
                    "SELECT ServiceID, Title, Price, DurationMinutes, IsActive FROM Services ORDER BY Title";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            services.Add(new ServiceInfo
                            {
                                ServiceID = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                Price = reader.GetDecimal(2),
                                DurationMinutes = reader.GetInt32(3)
                            });
                        }
                    }
                }
            }
            return services;
        }

        // =====================================================
        // ДОПОЛНИТЕЛЬНЫЕ ВСПОМОГАТЕЛЬНЫЕ КЛАССЫ
        // =====================================================

        public class ServiceInfo
        {
            public int ServiceID { get; set; }
            public string Title { get; set; }
            public decimal Price { get; set; }
            public int DurationMinutes { get; set; }

            public override string ToString()
            {
                return $"{Title} - {Price} руб. ({DurationMinutes} мин)";
            }
        }
    }

    // =====================================================
    // ВСПОМОГАТЕЛЬНЫЕ КЛАССЫ
    // =====================================================

    public class FreeSlot
    {
        public int MasterID { get; set; }
        public string MasterName { get; set; }
        public int WorkplaceID { get; set; }
        public string WorkplaceTitle { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public override string ToString()
        {
            return $"{StartTime:HH:mm} - {EndTime:HH:mm} | Мастер: {MasterName} | Место: {WorkplaceTitle}";
        }
    }

    public class MasterInfo
    {
        public int MasterID { get; set; }
        public string FullName { get; set; }
    }

    public class WorkplaceInfo
    {
        public int WorkplaceID { get; set; }
        public string Title { get; set; }
    }
}
