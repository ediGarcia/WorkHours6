using HelperExtensions;
using HelperMethods;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Reflection;
using WorkHours6.Models;

#pragma warning disable CS8602

namespace WorkHours6.Services;

/// <summary>
/// Service that manages the persisted data.
/// </summary>
public static class DataService
{
    private static readonly SqliteConnection Database;
    private static readonly TimeSpan FullDayTime = TimeSpan.FromHours(8);

    static DataService()
    {
        string databasePath = Path.Combine(DirectoryMethods.GetParentDirectory(Assembly.GetExecutingAssembly().Location), @"Resources\timesheet.s3db");

        if (File.Exists(databasePath))
        {
            Database = new($"DataSource={databasePath}");
            return;
        }

        string[] tableCommands =
        [
            "CREATE TABLE TimeEntries (Date DATE PRIMARY KEY NOT NULL, WorkedSeconds INTEGER NOT NULL DEFAULT 0, LastStartTime DATETIME, CreditedSeconds INTEGER NOT NULL DEFAULT 0, IsTimerEnabled INTEGER NOT NULL DEFAULT 0);",
            "CREATE TABLE BalanceSettings (Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, StartDate DATE NOT NULL, CalculateExtraHours INTEGER NOT NULL DEFAULT 0, TargetBalance INTEGER NOT NULL DEFAULT 0, TargetBalanceDeadline DATE);",
            $"INSERT INTO BalanceSettings (StartDate, CalculateExtraHours, TargetBalanceDeadline) VALUES ('{DateTimeMethods.GetFirstDayOfMonth():yyyy-MM-dd}', 1, '{DateTimeMethods.GetLastDayOfMonth():yyyy-MM-dd}');",
            "CREATE TABLE Version (Id INTEGER DEFAULT 1 NOT NULL);",
            "INSERT INTO Version (Id) VALUES (1);"
        ];

        using (Database = new($"DataSource={databasePath}"))
        {
            Database.Open();

            foreach (string commandText in tableCommands)
            {
                using SqliteCommand tableCommand = new(commandText, Database);
                tableCommand.ExecuteNonQuery();
            }
        }
    }

    #region Public Methods

    #region GetBalanceSettings
    /// <summary>
    /// Retrieves the balance settings from the database.
    /// </summary>
    /// <returns></returns>
    public static UserSettingsDatabaseEntry GetBalanceSettings()
    {
        using (Database)
        {
            Database.Open();

            using SqliteCommand command = new("SELECT * FROM BalanceSettings", Database);
            using SqliteDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
            reader.Read();

            return new()
            {
                Id = reader.GetInt32("Id"),
                BalanceStartDate = reader.GetDateTime("StartDate"),
                CalculateExtraHours = reader.GetBoolean("CalculateExtraHours"),
                TargetBalance = TimeSpan.FromSeconds(reader.GetInt32("TargetBalance")),
                TargetBalanceDeadline = reader.GetDateTime("TargetBalanceDeadline")
            };
        }
    }
    #endregion

    #region GetMonthReport
    /// <summary>
    /// Retrieves the data entries of every day of the specified month.
    /// </summary>
    /// <param name="month"></param>
    /// <param name="year"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static List<TimeDatabaseEntry> GetMonthReport(int month, int year)
    {
        #region Argument range validation.

        if (month is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(month),
                "The month value should be between 01 (jan) and 12 (dec).");

        if (year < 0)
            throw new ArgumentOutOfRangeException(nameof(year),
                "The year value should be greater than zero.");

        #endregion

        List<TimeDatabaseEntry> entries = GetTimeEntries(DateTimeMethods.GetFirstDayOfWeek(DateTimeMethods.GetFirstDayOfMonth(month, year)),
            DateTimeMethods.GetLastDayOfWeek(DateTimeMethods.GetLastDayOfMonth(month, year)));

        entries.ForEach(_ =>
            _.WorkedTime = _.Date == DateTime.Today ? TimeSpan.Zero : DateTimeMethods.RoundToMinutes(_.WorkedTime));
        return entries;
    }
    #endregion

    #region GetTimeEntries
    /// <summary>
    /// Retrieves the time entries for the selected period.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static List<TimeDatabaseEntry> GetTimeEntries(DateTime start, DateTime end)
    {
        if (end < start)
            throw new ArgumentException("The end date should not come before the start date.");

        using (Database)
        {
            Database.Open();

            using SqliteCommand command =
                new("SELECT * FROM TimeEntries WHERE Date BETWEEN @start AND @end ORDER BY Date", Database);
            command.Parameters.AddWithValue("@start", start);
            command.Parameters.AddWithValue("@end", end);

            using SqliteDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
            List<TimeDatabaseEntry> entries = new((int)(end - start).TotalDays);
            DateTime currentDateTime = start;

            while (reader.Read())
            {
                DateTime date = reader.GetDateTime("Date");

                for(; currentDateTime < date; currentDateTime = currentDateTime.AddDays(1))
                    entries.Add(CreateDatabaseEntry(currentDateTime));

                entries.Add(CreateDatabaseEntry(reader));
                currentDateTime = currentDateTime.AddDays(1);
            }

            while (currentDateTime <= end)
            {
                entries.Add(CreateDatabaseEntry(currentDateTime));
                currentDateTime = currentDateTime.AddDays(1);
            }

            return entries;
        }
    }
    #endregion

    #region GetTimeEntry
    /// <summary>
    /// Retrieves the time data from the database.
    /// </summary>
    /// <param name="targetDate"></param>
    /// <returns></returns>
    public static TimeDatabaseEntry GetTimeEntry(DateTime targetDate)
    {
        using (Database)
        {
            Database.Open();

            using SqliteCommand command = new("SELECT * FROM TimeEntries WHERE Date=@Date", Database);
            command.Parameters.AddWithValue("@Date", targetDate.Date);

            using SqliteDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
            return reader.Read() ? CreateDatabaseEntry(reader) : CreateDatabaseEntry(targetDate.Date);
        }
    }
    #endregion

    #region SaveBalanceSettings

    /// <summary>
    /// Saves the balance settings into the database.
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="targetBalance"></param>
    /// <param name="targetBalanceDeadline"></param>
    /// <param name="calculateExtraHours"></param>
    public static void SaveBalanceSettings(DateTime startDate, TimeSpan targetBalance, DateTime targetBalanceDeadline, bool calculateExtraHours)
    {
        using (Database)
        {
            Database.Open();

            using SqliteCommand command = new("UPDATE BalanceSettings SET StartDate=@StartDate, TargetBalance=@TargetBalance, TargetBalanceDeadline=@TargetBalanceDeadline, CalculateExtraHours=@CalculateExtraHours", Database);
            command.Parameters.AddWithValue("@StartDate", startDate);
            command.Parameters.AddWithValue("@TargetBalance", targetBalance.TotalSeconds);
            command.Parameters.AddWithValue("@TargetBalanceDeadline", targetBalanceDeadline);
            command.Parameters.AddWithValue("@CalculateExtraHours", calculateExtraHours);
            command.ExecuteNonQuery();
        }
    }
    #endregion

    #region SaveTimeData*

    #region SaveTimeData(IList<DatabaseEntry>)
    /// <summary>
    /// Persists the specified time data into the database.
    /// </summary>
    /// <param name="entries"></param>
    public static void SaveTimeData(IList<TimeDatabaseEntry> entries) =>
        entries.ForEach(_ =>
            SaveTimeData(_.Date, _.WorkedTime, _.CreditedHours, _.LastStartTime, false));
    #endregion

    #region SaveTimeData(DateTime, TimeSpan, TimeSpan, DateTime?, bool)
    /// <summary>
    /// Persists the specified time data to the database.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="workedHours"></param>
    /// <param name="creditedHours"></param>
    /// <param name="lastStartTime"></param>
    /// <param name="isTimerRunning"></param>
    public static void SaveTimeData(DateTime date, TimeSpan workedHours, TimeSpan creditedHours, DateTime? lastStartTime, bool isTimerRunning)
    {
        if (workedHours == TimeSpan.Zero
            && (creditedHours == TimeSpan.Zero || creditedHours == FullDayTime && date.IsWeekend())
            && !isTimerRunning)
        {
            using (Database)
            {
                Database.Open();

                using SqliteCommand command = new("DELETE FROM TimeEntries WHERE Date=@Date", Database);
                command.Parameters.AddWithValue("@Date", date);
                command.ExecuteNonQuery();
                return;
            }
        }

        double workedSeconds = Math.Round(workedHours.TotalSeconds);
        double creditedSeconds = Math.Round(creditedHours.TotalSeconds);

        using (Database)
        {
            Database.Open();

            using (SqliteCommand command =
                   new(
                       "UPDATE TimeEntries SET WorkedSeconds=@WorkedSeconds, LastStartTime=@LastStartTime, CreditedSeconds=@CreditedSeconds, IsTimerEnabled=@IsTimerEnabled WHERE Date=@Date", Database))
            {
                command.Parameters.AddWithValue("@Date", date);
                command.Parameters.AddWithValue("@WorkedSeconds", workedSeconds);

                if (lastStartTime.HasValue)
                    command.Parameters.AddWithValue("@LastStartTime", lastStartTime);
                else
                    command.Parameters.AddWithValue("@LastStartTime", DBNull.Value);

                command.Parameters.AddWithValue("@CreditedSeconds", creditedSeconds);
                command.Parameters.AddWithValue("@IsTimerEnabled", isTimerRunning);

                if (command.ExecuteNonQuery() == 0)
                {
                    command.CommandText =
                        "INSERT INTO TimeEntries(Date, WorkedSeconds, LastStartTime, CreditedSeconds, IsTimerEnabled) VALUES (@Date, @WorkedSeconds, @LastStartTime, @CreditedSeconds, @IsTimerEnabled)";
                    command.ExecuteNonQuery();
                }
            }
        }
    }
    #endregion

    #endregion

    #endregion

    #region Private Methods

    #region CreateDatabaseEntry*

    #region CreateDatabaseEntry(DateTime)
    /// <summary>
    /// Creates an empty <see cref="TimeDatabaseEntry"/> instance from the specified date.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    private static TimeDatabaseEntry CreateDatabaseEntry(DateTime date) =>
        new(date) { CreditedHours = date.IsWeekend() ? FullDayTime : TimeSpan.Zero };
    #endregion

    #region CreateDatabaseEntry(DbDataReader)
    /// <summary>
    /// Creates a <see cref="TimeDatabaseEntry"/> instance from the specified <see cref="SqliteDataReader"/> data.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static TimeDatabaseEntry CreateDatabaseEntry(DbDataReader reader) =>
        new(reader.GetDateTime("Date"))
        {
            WorkedTime = TimeSpan.FromSeconds(reader.GetInt32("WorkedSeconds")),
            LastStartTime = reader.IsDBNull("LastStartTime") ? null : reader.GetDateTime("LastStartTime"),
            CreditedHours = TimeSpan.FromSeconds(reader.GetInt32("CreditedSeconds")),
            IsTimerEnabled = reader.GetBoolean("IsTimerEnabled")
        };
    #endregion

    #endregion

    #endregion
}
