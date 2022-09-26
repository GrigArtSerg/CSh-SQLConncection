using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace CVD_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Введите IP сервера базы данных: ");
            String ConnSer = "server = ";
            ConnSer += Console.ReadLine() + ";"; // 127.0.0.1;

            Console.Write("Введите порт базы данных: ");
            String ConnPort = "port = ";
            ConnPort += Console.ReadLine() + ";";// 3306;

            Console.Write("Введите имя пользователя базы данных: ");
            String ConnUser = " user = ";
            ConnUser += Console.ReadLine() + ";";// root;

            Console.Write("Введите пароль базы данных: ");
            String ConnPass = "password = ";
            ConnPass += Console.ReadLine() + ";";// password;

            Console.Write("Введите имя базы данных: ");
            String ConnDB = "database = ";
            ConnDB += Console.ReadLine() + ";";// new_schema;
            
            
            String ConnectTM = "server = 127.0.0.1;" +
                                "port = 3306; user = root;" +
                                "password = password;" +
                                "database = new_schema;";
            
            //String Connect = ConnSer + ConnPort + ConnUser + ConnPass + ConnDB;
            
            
            MySqlConnection Conn = new MySqlConnection(ConnectTM);
            try
            {
                Conn.Open();
                int DepCount = 0;

                string Departments = $"select id from department";
                MySqlCommand CMD_Dep = new MySqlCommand(Departments, Conn);
                MySqlDataReader Read_Dep = CMD_Dep.ExecuteReader();

                while (Read_Dep.Read())
                {
                    DepCount++;
                }
                Read_Dep.Close();

                while (true)
                {
                    Console.WriteLine("\nSalary - запрос суммарной зарплаты в разрезе департамента");
                    Console.WriteLine("Max - запрос департамента с максимальной зарплатой");
                    Console.WriteLine("Chiefs - вывод ЗП руководителей (по убыванию)");
                    Console.WriteLine("Введите команду:");

                    string input = Console.ReadLine().ToLower();

                    switch (input)
                    {
                        case "salary":
                            Console.Write("Введите номер департамента: ");
                            string Dep = Console.ReadLine().ToLower();
                            int DepNum = 0;

                            if (Dep == "all")
                            {
                                Console.WriteLine($"Департамент 1 без руководителей: {SalaryByDepartments(Conn, 1, false)}");
                                Console.WriteLine($"Департамент 1 c руководителем: {SalaryByDepartments(Conn, 1, true)}");

                                Console.WriteLine($"Департамент 2 без руководителей: {SalaryByDepartments(Conn, 2, false)}");
                                Console.WriteLine($"Департамент 2 с руководителем: {SalaryByDepartments(Conn, 2, true)}");

                                Console.WriteLine($"Департамент 3 без руководителей: {SalaryByDepartments(Conn, 3, false)}");
                                Console.WriteLine($"Департамент 3 с руководителем: {SalaryByDepartments(Conn, 3, true)}");

                                break;
                            }
                            else
                            {
                                try
                                {
                                    DepNum = Convert.ToInt32(Dep);
                                }
                                catch(Exception exc)
                                {
                                    Console.WriteLine("Неверная команда");
                                    break;
                                }
                            }

                            if (DepNum <= DepCount)
                            {
                                Console.Write("Нужен ли руководитель? Y - да, N - нет ");
                                string IsCh = Console.ReadLine().ToLower();

                                bool IsChief = false;

                                if (IsCh == "y") IsChief = true;
                                else if (IsCh != "n") Console.WriteLine("Неверная команда");
                                
                                Console.WriteLine($"\nДепартамент {DepNum} без руководителя: {SalaryByDepartments(Conn, DepNum, IsChief)}");
                            }
                            else Console.WriteLine($"Департамент не существует. Всего {DepCount} департаментов");

                            break;

                        case "max":
                            Console.WriteLine($"Максимальная зарплата у сотрудника в {MaxSalaryDep(Conn)} департаменте");
                            break;
                        
                        case "chiefs":
                            ChifSalary(Conn, DepCount);
                            break;
                        
                        default:
                            Console.WriteLine("Неверная команда");
                            break;
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Warning 1: {ex.Message}");
            }
            finally
            {
                Conn.Close();
            }
        }

        static int SalaryByDepartments(MySqlConnection Conn, int DepartmentNumber, bool IsChief)
        {
            int Salary = 0;

            try
            {
                if (Conn.State == System.Data.ConnectionState.Closed) Conn.Open();

                //Console.WriteLine("Соединение установлено");

                string SQL_Employee = $"select salary from employee where department_id = {DepartmentNumber}";
                MySqlCommand CMD_Employee = new MySqlCommand(SQL_Employee, Conn);
                MySqlDataReader Read_Employee = CMD_Employee.ExecuteReader();

                while (Read_Employee.Read())
                {
                    Salary += Read_Employee.GetInt32("salary");
                }
                Read_Employee.Close();

                if (IsChief) Salary += FindChief(Conn, DepartmentNumber);
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Warning 2: {ex.Message}");
            }
            finally
            {
                Conn.Close();
            }
            return Salary;
        }

        static int FindChief(MySqlConnection Conn, int DepartmentNumber)
        {
            string Chief = $"select chief_id from employee where department_id = {Convert.ToString(DepartmentNumber)}";
            MySqlCommand CMD_Chief = new MySqlCommand(Chief, Conn);
            MySqlDataReader Read_Chief = CMD_Chief.ExecuteReader();
            Read_Chief.Read();

            string ChiefSalary = $"select salary from employee where id = {Read_Chief.GetString("chief_id")}";
            MySqlCommand CMD_ChiefSalary = new MySqlCommand(ChiefSalary, Conn);
            Read_Chief.Close();
          
            MySqlDataReader Read_ChiefSalary = CMD_ChiefSalary.ExecuteReader();
            Read_ChiefSalary.Read();
            return Read_ChiefSalary.GetInt32("salary");
        }

        static int MaxSalaryDep(MySqlConnection Conn)
        {
            int Max = 0, MaxDep = 0;

            try
            {
                if (Conn.State == System.Data.ConnectionState.Closed) Conn.Open();

                string EmpSalary = "select department_id, salary from employee where chief_id != 0";
                MySqlCommand CMD_EmpSalary = new MySqlCommand(EmpSalary, Conn);
                MySqlDataReader Read_EmpSalary = CMD_EmpSalary.ExecuteReader();
                //Read_EmpSalary.Read();
                while (Read_EmpSalary.Read())
                {
                    if (Read_EmpSalary.GetInt32("salary") > Max)
                    {
                        Max = Read_EmpSalary.GetInt32("salary");
                        MaxDep = Read_EmpSalary.GetInt32("department_id");
                    }
                    //else Console.WriteLine(Read_EmpSalary.GetInt32("salary"));
                }
                Read_EmpSalary.Close();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Warning 3: {ex.Message}");
            }
            finally
            {
                Conn.Close();
            }
            return MaxDep;
        }

        static void ChifSalary(MySqlConnection Conn, int DepCount)
        {
            try
            {
                if (Conn.State == System.Data.ConnectionState.Closed) Conn.Open();

                List<int> Chiefs = new List<int> { };

                string ChiefsList = "select distinct chief_id from employee where chief_id != 0";
                MySqlCommand CMD_ChiefsList = new MySqlCommand(ChiefsList, Conn);
                MySqlDataReader Read_ChiefsList = CMD_ChiefsList.ExecuteReader();

                while (Read_ChiefsList.Read())
                {
                    Chiefs.Add(Read_ChiefsList.GetInt32("chief_id"));
                }

                Read_ChiefsList.Close();

                for (int i = 0; i < Chiefs.Count; i++)
                {
                    string ChiefSalary = $"select salary from employee where id = {Chiefs[i]}";
                    MySqlCommand CMD_ChiefSalary = new MySqlCommand(ChiefSalary, Conn);
                    MySqlDataReader Read_ChiefSalary = CMD_ChiefSalary.ExecuteReader();

                    while (Read_ChiefSalary.Read()) Chiefs[i] = Read_ChiefSalary.GetInt32("salary");
                    Read_ChiefSalary.Close();
                }

                Chiefs.Sort();

                for (int i = Chiefs.Count - 1; i >= 0; i--)
                {
                    Console.WriteLine(Chiefs[i]);
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Warning 4: {ex.Message}");
            }
            finally
            {
                Conn.Close();
            }
        }
    }
}
