using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace lab2v7
{
    public abstract class Equipment
    {
        public int Id = 0;
        public string Name { get; }
        public string SerialNumber { get; }
        public string RegistrationDate { get; }
        public string LastMaintenanceDate { get; }
        
        public Equipment(string name, string serialNumber, string registrationDate, string lastMaintenanceDate)
        {
            Name = name;
            SerialNumber = serialNumber;
            RegistrationDate = registrationDate;
            LastMaintenanceDate = lastMaintenanceDate;
        }
    }

    class DecomEquipmentByTime : Equipment
    {
        public string DecomissedDate { get; }

        public DecomEquipmentByTime(string name, string serialNumber, string registrationDate, string lastMaintenanceDate, string decomissedDate) : base(name, serialNumber, registrationDate, lastMaintenanceDate)
        {
            DecomissedDate = decomissedDate;
        }
    }

    class DecomEquipmentByReason : Equipment
    {
        public string DecomissedReason { get; }
        public string DecomissedDate { get; }

        public DecomEquipmentByReason(string name, string serialNumber, string registrationDate, string lastMaintenanceDate, string decomissedReason, string decomissedDate) : base(name, serialNumber, registrationDate, lastMaintenanceDate)
        {
            DecomissedDate = decomissedDate;
            DecomissedReason = decomissedReason;
        }
    }

    public interface IDataSource
    {
        Equipment Save(Equipment record);
        Equipment Get(int id);
        bool Delete(int id);
        List<Equipment> GetAll();
    }

    class MemoryDataSource : IDataSource
    {
        private List<Equipment> records = new List<Equipment>();
        
        public Equipment Save(Equipment record)
        {
            if (record.Id == 0)
            {
                record.Id = records.Count + 1;
                records.Add(record);
            }
            else
            {
                records[record.Id - 1] = record;
            }
            
            return records[record.Id - 1];
        }

        public Equipment Get(int id)
        {
            return records.Find(item => item.Id == id);
        }

        public bool Delete(int id)
        {
            return records.Remove(records.Find(item => item.Id == id));
        }

        public List<Equipment> GetAll()
        {
            return records;
        }
    }

    class FileDataSource : IDataSource
    {
        private string _signature = "VotyakovIvanSergeevich";
        private string _path;
        private int maxId = 0;

        public FileDataSource(string filePath)
        {
            _path = filePath;
            var reader = new BinaryReader(File.Open(_path, FileMode.Open));
            var signature = reader.ReadString();
            if (signature != _signature) throw new Exception("Неправильная сигнатура файла");
            while (reader.PeekChar() != -1)
            {
                reader.ReadBoolean();
                maxId = reader.ReadInt32();
                var type = reader.ReadByte();
                switch (type)
                {
                    case 0:
                        reader.ReadString();
                        reader.ReadString();
                        reader.ReadString();
                        reader.ReadString();
                        reader.ReadString();
                        break;
                    case 1:
                        reader.ReadString();
                        reader.ReadString();
                        reader.ReadString();
                        reader.ReadString();
                        reader.ReadString();
                        reader.ReadString();
                        break;
                }
            }
            reader.Close();
        }
        
        public Equipment Save(Equipment record)
        {
            if (record.Id == 0)
            {
                var writer = new BinaryWriter(File.Open(_path, FileMode.OpenOrCreate));
                maxId++;
                record.Id = maxId;
                writer.Seek(0, SeekOrigin.End);
                writer.Write(false);
                writer.Write(record.Id);
                /*Console.Clear();
                Console.WriteLine(TypeDescriptor.GetClassName(record));
                Console.ReadLine();*/
                switch (TypeDescriptor.GetClassName(record))
                {
                    case "lab2v7.DecomEquipmentByTime":
                        writer.Write((byte)0);
                        writer.Write(record.Name);
                        writer.Write(record.SerialNumber);
                        writer.Write(record.RegistrationDate);
                        writer.Write(record.LastMaintenanceDate);
                        writer.Write((record as DecomEquipmentByTime).DecomissedDate);
                        break;
                    case "lab2v7.DecomEquipmentByReason":
                        writer.Write((byte)1);
                        writer.Write(record.Name);
                        writer.Write(record.SerialNumber);
                        writer.Write(record.RegistrationDate);
                        writer.Write(record.LastMaintenanceDate);
                        writer.Write((record as DecomEquipmentByReason).DecomissedDate);
                        writer.Write((record as DecomEquipmentByReason).DecomissedReason);
                        break;
                }
                writer.Close();
            }
            else
            {
                var reader = new BinaryReader(File.Open(_path, FileMode.Open));
                reader.ReadString();
                int offset = _signature.Length + 1;
                while (true)
                {
                    reader.ReadBoolean();
                    var id = reader.ReadInt32();
                    if (id == record.Id)
                    {
                        var type = reader.ReadByte();
                        reader.Close();
                        var writer = new BinaryWriter(File.Open(_path, FileMode.OpenOrCreate));
                        writer.Seek(offset, SeekOrigin.Begin);
                        switch (type)
                        {
                            case 0:
                                writer.Write(false);
                                writer.Write(record.Id);
                                writer.Write((byte)0);
                                writer.Write(record.Name);
                                writer.Write(record.SerialNumber);
                                writer.Write(record.RegistrationDate);
                                writer.Write(record.LastMaintenanceDate);
                                writer.Write((record as DecomEquipmentByTime).DecomissedDate);
                                break;
                            case 1:
                                writer.Write(false);
                                writer.Write(record.Id);
                                writer.Write((byte)1);
                                writer.Write(record.Name);
                                writer.Write(record.SerialNumber);
                                writer.Write(record.RegistrationDate);
                                writer.Write(record.LastMaintenanceDate);
                                writer.Write((record as DecomEquipmentByReason).DecomissedDate);
                                writer.Write((record as DecomEquipmentByReason).DecomissedReason);
                                break;
                        }
                        writer.Close();
                        break;
                    }

                    var type1 = reader.ReadByte();
                    var a = reader.ReadString();
                    var b = reader.ReadString();
                    var c = reader.ReadString();
                    var d = reader.ReadString();
                    var e = reader.ReadString();
                    switch (type1)
                    {
                        case 0:
                            offset = offset + 1 + 4 + a.Length + 1 + b.Length + 1 + c.Length + 1 + d.Length + 1 + e.Length + 1;
                            break;
                        case 1:
                            var f = reader.ReadString();
                            offset = offset + 1 + 4 + a.Length + 1 + b.Length + 1 + c.Length + 1 + d.Length + 1 + e.Length + 1 + f.Length + 1;
                            break;
                    }
                    
                }
                reader.Close();
            }
            
            return record;
        }

        public Equipment Get(int id)
        {
            var reader = new BinaryReader(File.Open(_path, FileMode.Open));
            reader.ReadString();
            while (true)
            {
                reader.ReadBoolean();
                var _id = reader.ReadInt32();
                var type = reader.ReadByte();
                if (_id == id)
                {
                    var a = reader.ReadString();
                    var b = reader.ReadString();
                    var c = reader.ReadString();
                    var d = reader.ReadString();
                    var e = reader.ReadString();
                    switch (type)
                    {
                        case 0:
                            var rec = new DecomEquipmentByTime(a, b,c,d,e);
                            rec.Id = _id;
                            reader.Close();
                            return rec;
                            break;
                        case 1:
                            var f = reader.ReadString();
                            var rec2 = new DecomEquipmentByReason(a, b,c,d,e,f);
                            rec2.Id = _id;
                            reader.Close();
                            return rec2;
                            break;
                    }
                }
                reader.ReadString();
                reader.ReadString();
                reader.ReadString();
                reader.ReadString();
                reader.ReadString();
                if(type ==1) reader.ReadString();
            }
        }

        public bool Delete(int id)
        {
            var reader = new BinaryReader(File.Open(_path, FileMode.Open));
            reader.ReadString();
            int offset = _signature.Length + 1;
            while (true)
            {
                reader.ReadBoolean();
                var _id = reader.ReadInt32();
                var type = reader.ReadByte();
                if (_id == id)
                {
                    reader.Close();
                    var writer = new BinaryWriter(File.Open(_path, FileMode.OpenOrCreate));
                    writer.Seek(offset, SeekOrigin.Begin);
                    writer.Write(true);
                    writer.Close();
                    break;
                }
                if (_id == maxId)
                {
                    reader.Close();
                    break;
                }

                var a = reader.ReadString();
                var b = reader.ReadString();
                var c = reader.ReadString();
                var d = reader.ReadString();
                var e = reader.ReadString();
                switch (type)
                {
                    case 0:
                        offset = offset + 1 + 4 + a.Length + 1 + b.Length + 1 + c.Length + 1 + d.Length + 1 + e.Length + 1;
                        break;
                    case 1:
                        var f = reader.ReadString();
                        offset = offset + 1 + 4 + a.Length + 1 + b.Length + 1 + c.Length + 1 + d.Length + 1 + e.Length + 1 + f.Length + 1;
                        break;
                }
            }
            return true;
        }

        public List<Equipment> GetAll()
        {
            var list = new List<Equipment>();
            var reader = new BinaryReader(File.Open(_path, FileMode.Open));
            var _id = 0;
            reader.ReadString();
            while (true)
            {
                if (_id == maxId)
                {
                    reader.Close();
                    return list;
                }
                var flag = reader.ReadBoolean();
                /*Console.Clear();
                Console.WriteLine(flag);
                Console.ReadLine();*/
                _id = reader.ReadInt32();
                var type = reader.ReadByte();
                var _name = reader.ReadString();
                var _serialNumber = reader.ReadString();
                var _date1 = reader.ReadString();
                var _date2 = reader.ReadString();
                var _date3 = reader.ReadString();
                if (!flag)
                {
                    switch (type)
                    {
                        case 0:
                            var rec = new DecomEquipmentByTime(_name, _serialNumber, _date1, _date2, _date3);
                            rec.Id = _id;
                            list.Add(rec);
                            break;
                        case 1:
                            var _reason = reader.ReadString();
                            var rec1 = new DecomEquipmentByReason(_name, _serialNumber, _date1, _date2, _date3, _reason);
                            rec1.Id = _id;
                            list.Add(rec1);
                            break;
                    }
                }
            }
        }
    }

    class BusinessLogic
    {
        private IDataSource _dataSource;
        private string _datePattern = @"\d\d\d\d/\d\d/\d\d", _serialNumberPattern = @"\d\d-\d\d\d";

        public BusinessLogic(IDataSource dataSource)
        {
            _dataSource = dataSource;
        }
        
        public Equipment SaveRecord(Equipment record)
        {
            if (Regex.Match(record.SerialNumber, _serialNumberPattern).Success &&
                Regex.Match(record.RegistrationDate, _datePattern).Success &&
                Regex.Match(record.LastMaintenanceDate, _datePattern).Success)

            {
                return _dataSource.Save(record);
            }
            else
            {
                throw new Exception("Неверный ввод данных, повторите ввод (дата в формате: ГГГГ/ММ/ДД, серийный номер в формате: 99-999)");
            }
        }

        public Equipment UpdateRecord(Equipment record)
        {
            if (Regex.Match(record.SerialNumber, _serialNumberPattern).Success &&
                Regex.Match(record.RegistrationDate, _datePattern).Success &&
                Regex.Match(record.LastMaintenanceDate, _datePattern).Success)
            {
                return _dataSource.Save(record);
            }
            else
            {
                throw new Exception("Неверный ввод данных, повторите ввод (дата в формате: ГГГГ/ММ/ДД, серийный номер в формате: 99-999)");
            }
        }

        public bool DeleteRecord(int id)
        {
            return _dataSource.Delete(id);
        }

        public List<Equipment> GetAllRecords()
        {
            return _dataSource.GetAll().OrderBy(x => x.Name).ThenBy(x => x.SerialNumber).ToList();
        }

        public Equipment GetRecord(int id)
        {
            return _dataSource.Get(id);
        }

        public void CreateReport(string path, int ageFrom, int ageTo)
        {
            var kvStorage = new Dictionary<int, int>();
            var records = _dataSource.GetAll();
            var currentDate = DateTime.Today;
            
            foreach (var record in records)
            {
                var year = Convert.ToInt32(record.RegistrationDate.Substring(0, 4));
                var month = Convert.ToInt32(record.RegistrationDate.Substring(5, 2));
                var day = Convert.ToInt32(record.RegistrationDate.Substring(8, 2));
                var date = new DateTime(year, month, day);

                var dif = ((currentDate.Year - date.Year) * 12) + currentDate.Month - date.Month;
                if (dif < ageFrom || dif > ageTo) continue;
                if (kvStorage.ContainsKey(dif)) kvStorage[dif]++;
                else kvStorage.Add(dif, 1);
            }
            
            using (StreamWriter writer = new StreamWriter(path, false))
            {
                foreach (var kvPair in kvStorage)
                {
                    writer.WriteLine($"{kvPair.Key} month: {kvPair.Value} pieces of equipment");
                }
            }
        }
    }
    
    internal class Program
    {
        public static void Main(string[] args)
        {
            //Записывает сигнатуру
            var writer = new BinaryWriter(File.Open("data.dat", FileMode.OpenOrCreate));
            writer.Write("VotyakovIvanSergeevich");
            writer.Close();
            
            BusinessLogic businessLogic = null;

            string message = null;
            while (true)
            {
                Console.Clear();
                if(!string.IsNullOrEmpty(message)) Console.WriteLine(message);
                Console.WriteLine("Введите путь до файла, с которым хотите работать");
                Console.Write("> ");
                var path = Console.ReadLine();

                try
                {
                    var fds = new FileDataSource(path);
                    businessLogic = new BusinessLogic(fds);
                    break;
                }
                catch (Exception e)
                {
                    message = e.Message;
                }
            }

            string[] MenuItems = { "Добавить запись", "Посмотреть записи", "Изменить запись", "Удалить запись", "Создать отчет", "Выйти" };
            string[] RecordTypes = { "Оборудование списанное по времени", "Оборудование списанное по другой причине" };
            int MenuItemNumber = 0;

            bool isWork = true;
            while (isWork)
            {
                Console.Clear();
                
                Console.WriteLine("Выберите действие (стрелки вверх и вниз, Enter)");
                for (int i = 0; i < MenuItems.Length; i++)
                {
                    if (MenuItemNumber == i)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("> ");
                        Console.WriteLine(MenuItems[i]);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else
                    {
                        Console.Write("> ");
                        Console.WriteLine(MenuItems[i]);
                    }
                }
                
                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        if (MenuItemNumber == 0) MenuItemNumber = MenuItems.Length - 1;
                        else MenuItemNumber--;
                        break;
                    case ConsoleKey.DownArrow:
                        if (MenuItemNumber == MenuItems.Length - 1) MenuItemNumber = 0;
                        else MenuItemNumber++;
                        break;
                    case ConsoleKey.Enter:
                        switch (MenuItems[MenuItemNumber])
                        {
                            case "Добавить запись":
                                MenuItemNumber = 0;
                                bool SaveDone = false;
                                
                                while (!SaveDone)
                                {
                                    Console.Clear();
                                    Console.WriteLine("Выберите вид записи (стрелки вверх и вниз, Enter)");
                                    
                                    for (int i = 0; i < RecordTypes.Length; i++)
                                    {
                                        if (MenuItemNumber == i)
                                        {
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.Write("> ");
                                            Console.WriteLine(RecordTypes[i]);
                                            Console.ForegroundColor = ConsoleColor.White;
                                        }
                                        else
                                        {
                                            Console.Write("> ");
                                            Console.WriteLine(RecordTypes[i]);
                                        }
                                    }
                                    
                                    key = Console.ReadKey(true).Key;
                                    switch (key)
                                    {
                                        case ConsoleKey.UpArrow:
                                            if (MenuItemNumber == 0) MenuItemNumber = RecordTypes.Length - 1;
                                            else MenuItemNumber--;
                                            break;
                                        case ConsoleKey.DownArrow:
                                            if (MenuItemNumber == RecordTypes.Length - 1) MenuItemNumber = 0;
                                            else MenuItemNumber++;
                                            break;
                                        case ConsoleKey.Enter:
                                            switch (RecordTypes[MenuItemNumber])
                                            {
                                                case "Оборудование списанное по времени":
                                                    string mes = null;
                                                    
                                                    while (true)
                                                    {
                                                        Console.Clear();
                                                        if (mes != null) Console.WriteLine(mes);
                                                        Console.WriteLine("Название");
                                                        Console.Write("> ");
                                                        var name = Console.ReadLine();
                                                        Console.WriteLine("Серийный номер");
                                                        Console.Write("> ");
                                                        var serialNumber = Console.ReadLine();
                                                        Console.WriteLine("Дата регистрации");
                                                        Console.Write("> ");
                                                        var regDate = Console.ReadLine();
                                                        Console.WriteLine("Дата последнего обслуживания");
                                                        Console.Write("> ");
                                                        var lastMaintenance = Console.ReadLine();
                                                        Console.WriteLine("Дата списания");
                                                        Console.Write("> ");
                                                        var decomDate = Console.ReadLine();

                                                        var record = new DecomEquipmentByTime(name, serialNumber,
                                                            regDate, lastMaintenance, decomDate);

                                                        try
                                                        {
                                                            businessLogic.SaveRecord(record);
                                                            SaveDone = true;
                                                            MenuItemNumber = 0;
                                                            break;
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            mes = e.Message;
                                                        }
                                                    }
                                                    break;
                                                case "Оборудование списанное по другой причине":
                                                    string mess = null;
                                                    
                                                    while (true)
                                                    {
                                                        Console.Clear();
                                                        if (mess != null) Console.WriteLine(mess);
                                                        Console.WriteLine("Название");
                                                        Console.Write("> ");
                                                        var name = Console.ReadLine();
                                                        Console.WriteLine("Серийный номер");
                                                        Console.Write("> ");
                                                        var serialNumber = Console.ReadLine();
                                                        Console.WriteLine("Дата регистрации");
                                                        Console.Write("> ");
                                                        var regDate = Console.ReadLine();
                                                        Console.WriteLine("Дата последнего обслуживания");
                                                        Console.Write("> ");
                                                        var lastMaintenance = Console.ReadLine();
                                                        Console.WriteLine("Дата списания");
                                                        Console.Write("> ");
                                                        var decomDate = Console.ReadLine();
                                                        Console.WriteLine("Причина списания");
                                                        Console.Write("> ");
                                                        var decomReason = Console.ReadLine();

                                                        var record = new DecomEquipmentByReason(name, serialNumber,
                                                            regDate, lastMaintenance, decomDate, decomReason);

                                                        try
                                                        {
                                                            businessLogic.SaveRecord(record);
                                                            SaveDone = true;
                                                            MenuItemNumber = 0;
                                                            break;
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            mess = e.Message;
                                                        }
                                                    }
                                                    break;
                                            }
                                            break;
                                    }
                                }
                                break;
                            case "Посмотреть записи":
                                Console.Clear();
                                var records = businessLogic.GetAllRecords();
                                Console.WriteLine($"Всего записей: {records.Count} (нажмите Enter для выхода)");
                                foreach (var record in records)
                                {
                                    var recordClass = TypeDescriptor.GetClassName(record).Split('.').Last();
                                    Console.Write($"id: {record.Id}   | ");
                                    switch (recordClass)
                                    {
                                        case "DecomEquipmentByReason":
                                            Console.Write($"Оборудование списаное по другой причине | ");
                                            break;
                                        case "DecomEquipmentByTime":
                                            Console.Write($"Оборудование списаное по времени        | ");
                                            break;
                                    }
                                    Console.Write($"{record.Name   } | ");
                                    Console.Write($"{record.SerialNumber} | ");
                                    Console.Write($"{record.RegistrationDate} | ");
                                    Console.Write($"{record.LastMaintenanceDate} |");
                                    Console.WriteLine();
                                }

                                Console.ReadLine();
                                break;
                            case "Изменить запись":
                                bool updateDone = false;
                                string mes2 = null;
                                while (!updateDone)
                                {
                                    Console.Clear();
                                    Console.WriteLine("Введите идентификатор записи, для выхода введите 0");
                                    if (mes2 != null) Console.WriteLine(mes2);
                                    Console.Write("> ");
                                    var id = Convert.ToInt32(Console.ReadLine());
                                    
                                    if(id == 0) break;

                                    var record = businessLogic.GetRecord(id);
                                    var recordClass = TypeDescriptor.GetClassName(record).Split('.').Last();
                                    if (record == null) mes2 = "Неверный идентификатор";
                                    else
                                    {
                                        mes2 = null;
                                        while (!updateDone)
                                        {
                                            Console.Clear();
                                            if (mes2 != null) Console.WriteLine(mes2);
                                            Console.WriteLine("Название");
                                            Console.Write("> ");
                                            var name = Console.ReadLine();
                                            Console.WriteLine("Серийный номер");
                                            Console.Write("> ");
                                            var serialNumber = Console.ReadLine();
                                            Console.WriteLine("Дата регистрации");
                                            Console.Write("> ");
                                            var regDate = Console.ReadLine();
                                            Console.WriteLine("Дата последнего обслуживания");
                                            Console.Write("> ");
                                            var lastMaintenance = Console.ReadLine();
                                            Console.WriteLine("Дата списания");
                                            Console.Write("> ");
                                            var decomDate = Console.ReadLine();
                                            switch (recordClass)
                                            {
                                                case "DecomEquipmentByReason":
                                                    Console.WriteLine("Причина списания");
                                                    Console.Write("> ");
                                                    var decomReason = Console.ReadLine();

                                                    var updatedRecBR = new DecomEquipmentByReason(name, serialNumber, regDate, lastMaintenance, decomDate, decomReason);
                                                    updatedRecBR.Id = record.Id;

                                                    try
                                                    {
                                                        businessLogic.UpdateRecord(updatedRecBR);
                                                        updateDone = true;
                                                    }
                                                    catch(Exception e)
                                                    {
                                                        mes2 = e.Message;
                                                    }
                                                    break;
                                                case "DecomEquipmentByTime":
                                                    var updatedRecBT = new DecomEquipmentByTime(name, serialNumber, regDate, lastMaintenance, decomDate);
                                                    updatedRecBT.Id = record.Id;
                                                    
                                                    try
                                                    {
                                                        businessLogic.UpdateRecord(updatedRecBT);
                                                        updateDone = true;
                                                    }
                                                    catch(Exception e)
                                                    {
                                                        mes2 = e.Message;
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                }
                                break;
                            case "Удалить запись":
                                string mes3 = null;
                                bool delDone = false;
                                while (!delDone)
                                {
                                    Console.Clear();
                                    Console.WriteLine("Введите идентификатор записи, для выхода введите 0");
                                    if (mes3 != null) Console.WriteLine(mes3);
                                    Console.Write("> ");
                                    var idDel = Convert.ToInt32(Console.ReadLine());
                                    
                                    if(idDel == 0) break;
                                    Console.Clear();
                                    Console.WriteLine($"Хотите удалить запись с id: {idDel}? (1 - да, 0 - нет)");
                                    Console.Write("> ");
                                    switch (Console.ReadLine())
                                    {
                                        case "0":
                                            delDone = true;
                                            break;
                                        case "1":
                                            if (businessLogic.DeleteRecord(idDel))
                                            {
                                                Console.Clear();
                                                Console.WriteLine("Запись успешно удалена. Нажмите Enter для выхода");
                                                Console.ReadLine();
                                                delDone = true;
                                            }
                                            else
                                            {
                                                mes3 = "Неверный идентификатор";
                                            }
                                            break;
                                    }
                                }
                                break;
                            case "Создать отчет":
                                Console.Clear();
                                Console.WriteLine("Введите путь до файла");
                                Console.Write("> ");
                                var path = Console.ReadLine();
                                Console.WriteLine("Введите возраст от");
                                Console.Write("> ");
                                var ageFrom = Convert.ToInt32(Console.ReadLine());
                                Console.WriteLine("Введите возраст до");
                                Console.Write("> ");
                                var ageTo = Convert.ToInt32(Console.ReadLine());
                                businessLogic.CreateReport(path, ageFrom, ageTo);
                                break;
                            case "Выйти":
                                isWork = false;
                                Console.Clear();
                                Console.WriteLine("Выход");
                                break;
                        }
                        break;
                }
            }
        }
    }
}