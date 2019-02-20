using System;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.IO;

/*
 * Author: Livolickie | https://github.com/livolickie/
 * Discord: https://discord.gg/ER3DT29
 * VK: https://vk.com/having_team
*/
namespace NetSonic
{
    //Класс должен быть статическим
    public static class Class1
    {
        //Наш буфер для записи и чтения данных
        static Buffer buffer = new Buffer();

        //Атрибут, нужный для того, чтобы эту функцию экпортировать
        [DllExport("GetSeiral_DiskDrive", CallingConvention.Cdecl)]
        unsafe public static double GetSerial_DiskDrive(byte * address)
        {
            //Нужен для извлечения коллекции по параметру дисков
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            //Смещаем в нашей области памяти позицию записи на начало
            buffer.SeekStart();
            //Перебираем полученную коллекцию из запроса
            foreach (var wmi_HD in searcher.Get())
            {
                //Здесь проверяется наличие у накопителя данных серийного номера, в противном случае он не будет добавлен в наш буфер
                if (wmi_HD["SerialNumber"] != null)
                    //Здесь мы добавляем серийный номер в нашу область памяти, убирая при это лишние пробелы через LINQ
                    buffer.WriteString(new string(wmi_HD["SerialNumber"].ToString().ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray())); 
            }
            int count = buffer.Tell(); //Сколько байт данных мы записали
            buffer.SeekStart(); //Смещаем позицию в буфере на начало
            for (int i = 0; i < count; i++) //Копируем данные из нашего буфера в указатель, который хранит адрес на буфер Game Maker'a
                address[i] = buffer.ReadByte();
            count = searcher.Get().Count;
            searcher.Dispose(); //Освобождаем память от объекта по извлечению коллекции
            return count; //Возвращаем количество найденных внешних накопителей с серийными номерами
        }
    }

    //Класс для обертки над MemoryStream
    public class Buffer
    {
        MemoryStream memory;
        BinaryWriter writer;
        BinaryReader reader;

        public Buffer()
        {
            memory = new MemoryStream();
            writer = new BinaryWriter(memory);
            reader = new BinaryReader(memory);
        }

        public void SeekStart()
        {
            memory.Seek(0, SeekOrigin.Begin);
        }

        public int Tell()
        {
            return (int)memory.Position;
        }

        public byte ReadByte()
        {
            return reader.ReadByte();
        }

        public void WriteString(string val)
        {
            for (int i = 0; i < val.Length; i++)
                writer.Write(val[i]);
            writer.Write('\0');
        }
    }
}
