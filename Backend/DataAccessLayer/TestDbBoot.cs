using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("ConsoleTest")]
[assembly: InternalsVisibleTo("Frontend")]
namespace IntroSE.Kanban.Backend.DataAccessLayer
{
    internal static class TestDbBoot
    {
        public static void EnsureSchemaExists()
        {
            using (var connection = new SQLiteConnection("Data Source=kanban.db;Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                            CREATE TABLE IF NOT EXISTS Boards (
                                Id INTEGER PRIMARY KEY,
                                Name TEXT NOT NULL,
                                OwnerEmail TEXT NOT NULL
                            );
                            
                            CREATE TABLE IF NOT EXISTS BoardMembers (
                                BoardID INTEGER NOT NULL,
                                Email TEXT NOT NULL,
                                PRIMARY KEY (BoardID, Email),
                                FOREIGN KEY (BoardID) REFERENCES Boards(Id) ON DELETE CASCADE,
                                FOREIGN KEY (Email) REFERENCES Users(Email) ON DELETE CASCADE
                            );                        
    
                            CREATE TABLE IF NOT EXISTS Columns (
                                BoardID INTEGER NOT NULL,
                                Ordinal INTEGER NOT NULL,
                                TasksLimit INTEGER NOT NULL,
                                PRIMARY KEY (BoardID, Ordinal),
                                FOREIGN KEY (BoardID) REFERENCES Boards(Id) ON DELETE CASCADE
                            );

                            CREATE TABLE IF NOT EXISTS Tasks (
                                BoardID INTEGER NOT NULL,
                                Ordinal INTEGER NOT NULL,
                                TaskId INTEGER NOT NULL,
                                AssigneeEmail TEXT NOT NULL,
                                Title TEXT NOT NULL,
                                Description TEXT,
                                DueDate TEXT NOT NULL,
                                CreationTime TEXT NOT NULL,
                                PRIMARY KEY (BoardID, Ordinal, TaskId),
                                FOREIGN KEY (BoardID, Ordinal) REFERENCES Columns(BoardID, Ordinal) ON DELETE CASCADE
                            );

                            CREATE TABLE IF NOT EXISTS Users (
                                Email TEXT PRIMARY KEY,
                                Password TEXT NOT NULL
                            );
                        ";
                    command.ExecuteNonQuery();
                }
            }
            Console.WriteLine("DB path: " + Path.GetFullPath("kanban.db"));

        }

    }


}
