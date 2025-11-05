using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using v2.Models;
using System;
using System.Globalization;

namespace v2.Data
{
    public class DataLoader
    {
        public static DateTime ToUtc(DateTime dateTime)
        {
            return dateTime.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                : dateTime.ToUniversalTime();
        }
        public class CustomDateTimeConverter : JsonConverter<DateTime>
        {
            private readonly string[] formats = {
            "dd-MM-yyyy HH:mm:ss",
            "dd-MM-yyyy HH:mm:ssffffff", // handles extra digits
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-dd HH:mm:ss",
            "dd/MM/yyyy HH:mm:ss"
        };

            // ✅ Correct method signature for newer Newtonsoft.Json
            public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.String)
                {
                    string str = (string)reader.Value;
                    if (DateTime.TryParseExact(str, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt))
                        return dt;
                }
                return DateTime.MinValue; // fallback for invalid/malformed dates
            }

            public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString("yyyy-MM-ddTHH:mm:ss"));
            }
        }

        // Load small JSON files fully into memory
        public static List<T> LoadDataFromFile<T>(string filePath)
        {
            var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                return new List<T>();
            }

            using var reader = new StreamReader(filePath);
            string json = reader.ReadToEnd();

            if (json.TrimStart().StartsWith("{"))
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, T>>(json, settings);
                return dict != null ? new List<T>(dict.Values) : new List<T>();
            }
            else
            {
                return JsonConvert.DeserializeObject<List<T>>(json, settings) ?? new List<T>();
            }
        }

        // Stream large JSON files (arrays or object dictionaries)
        public static IEnumerable<T> StreamDataFromFile<T>(string filePath)
        {
            if (!File.Exists(filePath)) yield break;

            using var sr = new StreamReader(filePath);
            using var reader = new JsonTextReader(sr);
            var serializer = new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };

            if (!reader.Read()) yield break;

            if (reader.TokenType == JsonToken.StartArray)
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        var item = serializer.Deserialize<T>(reader);
                        if (item != null) yield return item;
                    }
                }
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        reader.Read();
                        var item = serializer.Deserialize<T>(reader);
                        if (item != null) yield return item;
                    }
                    else if (reader.TokenType == JsonToken.EndObject) break;
                }
            }
        }

        // Stream reservations safely and fix string IDs
        public static IEnumerable<Reservation> StreamReservationsFromFile(string filePath)
        {
            if (!File.Exists(filePath)) yield break;

            using var sr = new StreamReader(filePath);
            using var reader = new JsonTextReader(sr);
            var jToken = JToken.ReadFrom(reader);

            if (jToken is JObject objDict)
            {
                foreach (var prop in objDict.Properties())
                {
                    var res = prop.Value.ToObject<Reservation>();
                    if (res != null)
                    {
                        if (res.UserId == 0 && int.TryParse(prop.Value["user_id"]?.ToString(), out int userId))
                            res.UserId = userId;
                        if (res.VehicleId == 0 && int.TryParse(prop.Value["vehicle_id"]?.ToString(), out int vehicleId))
                            res.VehicleId = vehicleId;
                        if (res.ParkingLotId == 0 && int.TryParse(prop.Value["parking_lot_id"]?.ToString(), out int lotId))
                            res.ParkingLotId = lotId;
                        yield return res;
                    }
                }
            }
            else if (jToken is JArray arr)
            {
                foreach (var item in arr)
                {
                    var res = item.ToObject<Reservation>();
                    if (res != null)
                    {
                        if (res.UserId == 0 && int.TryParse(item["user_id"]?.ToString(), out int userId))
                            res.UserId = userId;
                        if (res.VehicleId == 0 && int.TryParse(item["vehicle_id"]?.ToString(), out int vehicleId))
                            res.VehicleId = vehicleId;
                        if (res.ParkingLotId == 0 && int.TryParse(item["parking_lot_id"]?.ToString(), out int lotId))
                            res.ParkingLotId = lotId;
                        yield return res;
                    }
                }
            }
        }

        public static void ImportData(AppDbContext context)
        {
            // --- USERS ---
            var users = LoadDataFromFile<UserProfile>("data/users.json");
            int importedUsers = 0;
            foreach (var user in users)
            {
                user.Id = 0; // reset ID for EF Core
                user.CreatedAt = ToUtc(user.CreatedAt);

                if (!context.Users.Any(u => u.Email == user.Email))
                {
                    context.Users.Add(user);
                    importedUsers++;
                }
            }
            context.SaveChanges();
            Console.WriteLine($"✅ Imported {importedUsers} users");

            // --- VEHICLES ---
            var vehicles = LoadDataFromFile<Vehicle>("data/vehicles.json");
            int importedVehicles = 0;
            foreach (var vehicle in vehicles)
            {
                vehicle.Id = 0;
                vehicle.CreatedAt = ToUtc(vehicle.CreatedAt);

                if (string.IsNullOrWhiteSpace(vehicle.LicensePlate)) continue;
                if (context.Vehicles.Any(v => v.LicensePlate == vehicle.LicensePlate)) continue;

                context.Vehicles.Add(vehicle);
                importedVehicles++;
            }
            context.SaveChanges();
            Console.WriteLine($"✅ Imported {importedVehicles} vehicles");

            // --- PARKING LOTS ---
            var parkingLots = LoadDataFromFile<ParkingLot>("data/parking-lots.json");
            int importedLots = 0;
            foreach (var lot in parkingLots)
            {
                lot.CreatedAt = ToUtc(lot.CreatedAt);

                if (lot.Coordinates != null)
                {
                    lot.Latitude = lot.Coordinates.Lat;
                    lot.Longitude = lot.Coordinates.Lng;
                }

                if (context.ParkingLots.Any(p => p.Address == lot.Address)) continue;

                context.ParkingLots.Add(lot);
                importedLots++;
            }
            context.SaveChanges();
            Console.WriteLine($"✅ Imported {importedLots} parking lots");

            // --- RESERVATIONS ---
            var reservations = LoadDataFromFile<Reservation>("data/reservations.json");
            int importedReservations = 0;
            foreach (var res in reservations)
            {
                res.Id = 0;
                res.StartTime = ToUtc(res.StartTime);
                res.EndTime = ToUtc(res.EndTime);
                res.CreatedAt = ToUtc(res.CreatedAt);

                var user = context.Users.FirstOrDefault(u => u.Id.ToString() == res.UserId.ToString());
                if (user == null) continue;
                res.UserId = user.Id;

                var vehicle = context.Vehicles.FirstOrDefault(v => v.Id.ToString() == res.VehicleId.ToString());
                if (vehicle == null) continue;
                res.VehicleId = vehicle.Id;

                var parkingLot = context.ParkingLots.FirstOrDefault(p => p.Id.ToString() == res.ParkingLotId.ToString());
                if (parkingLot == null) continue;
                res.ParkingLotId = parkingLot.Id;

                bool exists = context.Reservations.Any(r =>
                    r.UserId == res.UserId &&
                    r.VehicleId == res.VehicleId &&
                    r.ParkingLotId == res.ParkingLotId &&
                    r.StartTime == res.StartTime &&
                    r.EndTime == res.EndTime
                );

                if (exists) continue;

                context.Reservations.Add(res);
                importedReservations++;
            }
            context.SaveChanges();
            Console.WriteLine($"✅ Imported {importedReservations} reservations");

            // --- PAYMENTS ---
            int importedPayments = 0;
            const int paymentBatchSize = 500;
            int pendingPayments = 0;
            bool origDetectPayments = context.ChangeTracker.AutoDetectChangesEnabled;
            context.ChangeTracker.AutoDetectChangesEnabled = false;

            try
            {
                using var sr = new StreamReader("data/payments.json");
                using var reader = new JsonTextReader(sr);
                var serializer = new JsonSerializer
                {
                    NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
                };
                serializer.Converters.Add(new CustomDateTimeConverter());

                if (!reader.Read()) return; // empty file
                if (reader.TokenType != JsonToken.StartArray) throw new Exception("Expected JSON array");

                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        var payment = serializer.Deserialize<Payment>(reader);
                        if (payment == null) continue;

                        // Ensure CreatedAt, Completed, and TData.Date are valid UTC
                        payment.CreatedAt = payment.CreatedAt == DateTime.MinValue ? DateTime.UtcNow : ToUtc(payment.CreatedAt);
                        payment.Completed = payment.Completed == DateTime.MinValue ? DateTime.UtcNow : ToUtc(payment.Completed);
                        if (payment.TData != null)
                            payment.TData.Date = ToUtc(payment.TData.Date);

                        // Skip duplicates
                        if (context.Payments.Any(p => p.Transaction == payment.Transaction)) continue;

                        context.Payments.Add(payment);
                        importedPayments++;
                        pendingPayments++;

                        if (pendingPayments >= paymentBatchSize)
                        {
                            context.SaveChanges();
                            pendingPayments = 0;
                        }
                    }
                }

                if (pendingPayments > 0) context.SaveChanges();
            }
            finally
            {
                context.ChangeTracker.AutoDetectChangesEnabled = origDetectPayments;
            }

            Console.WriteLine($"✅ Imported {importedPayments} payments");

            // --- PARKING SESSIONS ---
            int importedSessions = 0;
            string baseDir = AppContext.BaseDirectory;
            string? sessionsFolder = null;

            var dir = new DirectoryInfo(baseDir);
            for (int depth = 0; depth < 25 && dir != null; depth++, dir = dir.Parent)
            {
                var candidate = Path.Combine(dir.FullName, "data", "pdata");
                if (Directory.Exists(candidate))
                {
                    sessionsFolder = candidate;
                    break;
                }
            }

            if (sessionsFolder != null)
            {
                const int batchSize = 1000;
                int pending = 0;
                bool origDetect = context.ChangeTracker.AutoDetectChangesEnabled;
                context.ChangeTracker.AutoDetectChangesEnabled = false;

                try
                {
                    for (int i = 1; i <= 1500; i++)
                    {
                        string filePath = Path.Combine(sessionsFolder, $"p{i}-sessions.json");
                        if (!File.Exists(filePath)) continue;

                        var sessions = LoadDataFromFile<ParkingSession>(filePath);
                        if (sessions == null || sessions.Count == 0) continue;

                        foreach (var s in sessions)
                        {
                            s.Id = 0;
                            s.Started = ToUtc(s.Started);
                            s.Stopped = ToUtc(s.Stopped);

                            if (string.IsNullOrWhiteSpace(s.LicensePlate)) continue;

                            bool exists = context.ParkingSessions.AsNoTracking()
                                .Any(ps => ps.LicensePlate == s.LicensePlate && ps.Started == s.Started);

                            if (!exists)
                            {
                                context.ParkingSessions.Add(new ParkingSession
                                {
                                    Id = 0,
                                    ParkingLotId = s.ParkingLotId,
                                    LicensePlate = s.LicensePlate,
                                    Username = s.Username,
                                    Started = s.Started,
                                    Stopped = s.Stopped,
                                    DurationMinutes = s.DurationMinutes,
                                    Cost = s.Cost,
                                    PaymentStatus = s.PaymentStatus
                                });
                                importedSessions++;
                                pending++;
                                if (pending >= batchSize)
                                {
                                    context.SaveChanges();
                                    pending = 0;
                                }
                            }
                        }

                        if (pending > 0)
                        {
                            context.SaveChanges();
                            pending = 0;
                        }
                    }
                }
                finally
                {
                    context.ChangeTracker.AutoDetectChangesEnabled = origDetect;
                }
            }
            Console.WriteLine($"✅ Imported {importedSessions} parking sessions");
        }
    }
}
//hello helllo