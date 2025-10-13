using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using v2.Models;
using Microsoft.EntityFrameworkCore;

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

        public static List<T> LoadDataFromFile<T>(string filePath)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                return new List<T>();
            }

            using (StreamReader reader = new StreamReader(filePath))
            {
                string json = reader.ReadToEnd();

                // Handle both { "1": {...} } and [ {...} ] formats
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
        }

        public static void ImportData(AppDbContext context)
        {
            // --- USERS ONLY ---
            var users = LoadDataFromFile<UserProfile>("data/users.json");
            int importedUsers = 0;

            foreach (var user in users)
            {
                user.Id = 0; // reset ID for EF Core
                user.CreatedAt = ToUtc(user.CreatedAt);

                // Only add the user if it doesn't already exist (e.g., by email)
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
            int imported = 0;

            foreach (var vehicle in vehicles)
            {
                vehicle.Id = 0;
                vehicle.CreatedAt = DataLoader.ToUtc(vehicle.CreatedAt);

                // Skip if LicensePlate is missing
                if (string.IsNullOrWhiteSpace(vehicle.LicensePlate))
                {
                    Console.WriteLine($"⚠ Skipping vehicle for user {vehicle.UserId}: missing LicensePlate");
                    continue;
                }

                // Skip if vehicle with same LicensePlate already exists
                bool exists = context.Vehicles.Any(v => v.LicensePlate == vehicle.LicensePlate);
                if (exists)
                {
                    continue;
                }

                context.Vehicles.Add(vehicle);
                imported++;
            }

            // Save all vehicles at once
            context.SaveChanges();
            Console.WriteLine($"✅ Imported {imported} vehicles");
            
            // --- RESERVATIONS ---
                var reservations = LoadDataFromFile<Reservation>("data/reservations.json");
                int importedReservations = 0;

                foreach (var res in reservations)
                {
                    res.Id = 0;
                    res.StartTime = ToUtc(res.StartTime);
                    res.EndTime = ToUtc(res.EndTime);
                    res.CreatedAt = ToUtc(res.CreatedAt);

                    // Map user
                    var user = context.Users.FirstOrDefault(u => u.Id.ToString() == res.UserId.ToString());
                    if (user == null) continue;
                    res.UserId = user.Id;

                    // Map vehicle
                    var vehicle = context.Vehicles.FirstOrDefault(v => v.Id.ToString() == res.VehicleId.ToString());
                    if (vehicle == null) continue;
                    res.VehicleId = vehicle.Id;

                    // Map parking lot
                    var parkingLot = context.ParkingLots.FirstOrDefault(p => p.Id.ToString() == res.ParkingLotId.ToString());
                    if (parkingLot == null) continue;
                    res.ParkingLotId = parkingLot.Id;

                    // Duplicate check: same User, Vehicle, ParkingLot, StartTime, EndTime
                    bool exists = context.Reservations.Any(r =>
                        r.UserId == res.UserId &&
                        r.VehicleId == res.VehicleId &&
                        r.ParkingLotId == res.ParkingLotId &&
                        r.StartTime == res.StartTime &&
                        r.EndTime == res.EndTime);

                    if (exists) continue;

                    context.Reservations.Add(res);
                    importedReservations++;
                }

                context.SaveChanges();
                Console.WriteLine($"✅ Imported {importedReservations} reservations");

                        
            // --- PARKING LOTS ---
            var parkingLots = LoadDataFromFile<ParkingLot>("data/parking-lots.json");
            int importedLots = 0;

            foreach (var lot in parkingLots)
            {
                // Preserve the JSON ID so reservations can link correctly
                // Do NOT reset lot.Id
                lot.CreatedAt = ToUtc(lot.CreatedAt);

                // Flatten coordinates object
                if (lot.Coordinates != null)
                {
                    lot.Latitude = lot.Coordinates.Lat;
                    lot.Longitude = lot.Coordinates.Lng;
                }

                // Skip if parking lot with same Address already exists
                bool exists = context.ParkingLots.Any(p => p.Address == lot.Address);
                if (exists)
                {
                    continue;
                }

                context.ParkingLots.Add(lot);
                importedLots++;
            }

            context.SaveChanges();
            Console.WriteLine($"✅ Imported {importedLots} parking lots");

            /*
            // --- PAYMENTS ---
            var payments = LoadDataFromFile<Payment>("data/payments.json");
            int paymentsImported = 0;

            foreach (var payment in payments)
            {
                payment.Id = 0; // reset for EF Core

                // Convert dates safely
                payment.CreatedAt = DataLoader.ToUtc(payment.CreatedAt);
                payment.Completed = DataLoader.ToUtc(payment.Completed);

                if (payment.TData != null)
                {
                    payment.TData.Date = DataLoader.ToUtc(payment.TData.Date);
                }

                // Skip if transaction already exists
                bool exists = context.Payments.Any(p => p.Transaction == payment.Transaction);
                if (exists)
                    continue;

                context.Payments.Add(payment);
                paymentsImported++;
            }
            

            // Save all payments at once
            context.SaveChanges();
            Console.WriteLine($"✅ Imported {paymentsImported} payments");
*/

           // --- PARKING SESSIONS ---
            string sessionsFolder = "pdata";
            int importedSessions = 0;

            if (Directory.Exists(sessionsFolder))
            {
                // Preload all users into a dictionary for mapping username -> UserId
                var usersDict = context.Users.AsNoTracking().ToDictionary(u => u.Username, u => u.Id);

                var sessionFiles = Directory.GetFiles(sessionsFolder, "p*-sessions.json");

                foreach (var file in sessionFiles)
                {
                    var sessions = LoadDataFromFile<ParkingSession>(file);

                    foreach (var s in sessions)
                    {
                        s.Id = 0; // reset for EF Core
                        s.Started = ToUtc(s.Started);
                        s.Stopped = ToUtc(s.Stopped);

                        // Optional: map user
                        int? userId = null;
                        if (!string.IsNullOrEmpty(s.Username) && usersDict.TryGetValue(s.Username, out var uid))
                        {
                            userId = uid;
                        }

                        // Skip duplicates by LicensePlate + Started time
                        bool exists = context.ParkingSessions.Any(ps =>
                            ps.LicensePlate == s.LicensePlate &&
                            ps.Started == s.Started
                        );

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
                        }
                    }

                    // Save after each file
                    context.SaveChanges();
                }
            }

            Console.WriteLine($"✅ Imported {importedSessions} parking sessions");
        }
    }
}
