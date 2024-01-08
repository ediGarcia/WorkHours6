using HelperExtensions;
using HelperMethods;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
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
    #region Public Methods

    #region GetBalanceSettings
    /// <summary>
    /// Retrieves the balance settings from the database.
    /// </summary>
    /// <returns></returns>
    public static UserSettingsDatabaseEntry GetBalanceSettings()
    {
        using SqliteCommand command = CreateCommand();
        command.CommandText = "SELECT * FROM BalanceSettings";

        using SqliteDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);

        return reader.Read()
            ? new()
            {
                Id = reader.GetInt32("Id"),
                BalanceStartDate = reader.GetDateTime("StartDate"),
                CalculateExtraHours = reader.GetBoolean("CalculateExtraHours"),
                TargetBalance = TimeSpan.FromSeconds(reader.GetInt32("TargetBalance")),
                TargetBalanceDeadline = reader.GetDateTime("TargetBalanceDeadline")
            }
            : new();
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

        using SqliteCommand command = CreateCommand();

        command.CommandText = "SELECT * FROM TimeEntries WHERE Date BETWEEN @start AND @end ORDER BY Date";
        command.Parameters.AddWithValue("@start", start);
        command.Parameters.AddWithValue("@end", end);

        using SqliteDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
        List<TimeDatabaseEntry> entries = new((int)(end - start).TotalDays);
        DateTime currentDateTime = start;

        while (reader.Read())
        {
            DateTime date = reader.GetDateTime("Date");

            while (date > currentDateTime)
            {
                entries.Add(new(currentDateTime));
                currentDateTime = currentDateTime.AddDays(1);
            }

            entries.Add(CreateDatabaseEntry(reader));
            currentDateTime = currentDateTime.AddDays(1);
        }

        while (currentDateTime <= end)
        {
            entries.Add(new(currentDateTime));
            currentDateTime = currentDateTime.AddDays(1);
        }

        return entries;
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
        using SqliteCommand command = CreateCommand();
        command.CommandText = "SELECT * FROM TimeEntries WHERE Date=@Date";
        command.Parameters.AddWithValue("@Date", targetDate.Date);

        using SqliteDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
        return reader.Read() ? CreateDatabaseEntry(reader) : new(targetDate.Date);
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
        int updatedRows;

        using (SqliteCommand command = CreateCommand())
        {
            command.CommandText =
                "UPDATE BalanceSettings SET StartDate=@StartDate, TargetBalance=@TargetBalance, TargetBalanceDeadline=@TargetBalanceDeadline, CalculateExtraHours=@CalculateExtraHours";
            command.Parameters.AddWithValue("@StartDate", startDate);
            command.Parameters.AddWithValue("@TargetBalance", targetBalance.TotalSeconds);
            command.Parameters.AddWithValue("@TargetBalanceDeadline", targetBalanceDeadline);
            command.Parameters.AddWithValue("@CalculateExtraHours", calculateExtraHours);
            updatedRows = command.ExecuteNonQuery();
            command.Connection.Close();
        }

        if (updatedRows == 0)
        {
            using SqliteCommand command = CreateCommand();
            command.CommandText =
                "INSERT INTO BalanceSettings(StartDate,TargetBalance,TargetBalanceDeadline,CalculateExtraHours) VALUES (@StartDate,@TargetBalance,@TargetBalanceDeadline,@CalculateExtraHours)";
            command.Parameters.AddWithValue("@StartDate", startDate);
            command.Parameters.AddWithValue("@TargetBalance", targetBalance.TotalSeconds);
            command.Parameters.AddWithValue("@TargetBalanceDeadline", targetBalanceDeadline);
            command.Parameters.AddWithValue("@CalculateExtraHours", calculateExtraHours);
            command.ExecuteNonQuery();
            command.Connection.Close();
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
        entries.ForEach(_ => SaveTimeData(_.Date, _.WorkedTime, _.CreditedHours, false));
    #endregion

    #region SaveTimeData(DateTime, TimeSpan, TimeSpan, bool)
    /// <summary>
    /// Persists the specified time data to the database.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="workedHours"></param>
    /// <param name="creditedHours"></param>
    /// <param name="isTimerRunning"></param>
    public static void SaveTimeData(DateTime date, TimeSpan workedHours, TimeSpan creditedHours, bool isTimerRunning)
    {
        if (workedHours == TimeSpan.Zero && creditedHours == TimeSpan.Zero && !isTimerRunning)
        {
            using SqliteCommand command = CreateCommand();
            command.CommandText = "DELETE FROM TimeEntries WHERE Date=@Date";
            command.Parameters.AddWithValue("@Date", date);

            command.ExecuteNonQuery();
            command.Connection.Close();
            return;
        }

        int updatedRows;
        double workedSeconds = Math.Round(workedHours.TotalSeconds);
        double creditedSeconds = Math.Round(creditedHours.TotalSeconds);
        object lastStartTime = isTimerRunning ? DateTimeMethods.RoundToSeconds(DateTime.Now) : DBNull.Value;

        using (SqliteCommand command = CreateCommand())
        {
            command.CommandText =
                "UPDATE TimeEntries SET WorkedSeconds=@WorkedSeconds, LastStartTime=@LastStartTime, CreditedSeconds=@CreditedSeconds WHERE Date=@Date";
            command.Parameters.AddWithValue("@Date", date);
            command.Parameters.AddWithValue("@WorkedSeconds", workedSeconds);
            command.Parameters.AddWithValue("@LastStartTime", lastStartTime);
            command.Parameters.AddWithValue("@CreditedSeconds", creditedSeconds);
            updatedRows = command.ExecuteNonQuery();
            command.Connection.Close();
        }

        if (updatedRows == 0)
        {
            using SqliteCommand command = CreateCommand();
            command.CommandText =
                "INSERT INTO TimeEntries(Date,WorkedSeconds,LastStartTime,CreditedSeconds) VALUES (@Date,@WorkedSeconds,@LastStartTime,@CreditedSeconds)";
            command.Parameters.AddWithValue("@Date", date);
            command.Parameters.AddWithValue("@WorkedSeconds", workedSeconds);
            command.Parameters.AddWithValue("@LastStartTime", lastStartTime);
            command.Parameters.AddWithValue("@CreditedSeconds", creditedSeconds);
            command.ExecuteNonQuery();
            command.Connection.Close();
        }
    }
    #endregion

    #endregion

    #endregion

    #region Private Methods

    #region CreateConnection
    /// <summary>
    /// Creates and opens a database connection.
    /// </summary>
    /// <returns></returns>
    private static SqliteCommand CreateCommand()
    {
        string databasePath = Path.Combine(
            SystemMethods.GetParentDirectory(Assembly.GetExecutingAssembly().Location),
            @"Resources\timesheet.s3db");

        SqliteConnection connection = new($"DataSource={databasePath}");
        connection.Open();
        return connection.CreateCommand();
    }
    #endregion

    #region CreateDatabaseEntry
    /// <summary>
    /// Creates a <see cref="TimeDatabaseEntry"/> instance from the specified <see cref="SqliteDataReader"/> data.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    private static TimeDatabaseEntry CreateDatabaseEntry(SqliteDataReader reader) =>
        new(reader.GetDateTime("Date"))
        {
            Id = reader.GetInt32("Id"),
            WorkedTime = TimeSpan.FromSeconds(reader.GetInt32("WorkedSeconds")),
            LastStartTime = reader["LastStartTime"] is DBNull ? null : reader.GetDateTime("LastStartTime"),
            CreditedHours = TimeSpan.FromSeconds(reader.GetInt32("CreditedSeconds"))
        };
    #endregion

    #endregion
}
