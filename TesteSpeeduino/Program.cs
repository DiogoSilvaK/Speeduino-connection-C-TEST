using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace TesteSpeeduino
{
    internal class Program
    {
        byte[] speedyResponse = new byte[100]; // O buffer de dados para os dados da Serial. Isso é mais longo do que o necessário, apenas por precaução
        byte[] byteNumber = new byte[2];  // ponteiro para o número uint8_t que estamos lendo atualmente
        int iat;   // para armazenar a temperatura do líquido de arrefecimento
        int clt;   // para armazenar a temperatura do líquido de arrefecimento
        int tps;
        int bat;
        int engStatus;
        int adv;
        ushort rpm;  // rpm e PW do speeduino
        float afr;
        float mapData;
        sbyte psi;
        float afrConv;
        byte[] cmdAdata = new byte[50];
        int rpmMax;
        double largRpmDash;
        int d, c, h;

        const int BYTES_TO_READ = 75;
        const int SERIAL_TIMEOUT = 300;
        float rps;
        bool sent = false;
        bool received = false;
        uint sendTimestamp;
        string portName = "COM4"; // Especifique o nome da porta COM desejada
        int baudRate = 115200;
        SerialPort serialPort = new SerialPort("COM4", 115200, Parity.None, 8 ,StopBits.One);

        static void Main(string[] args)
        {
            var rqd = new Program();
            rqd.RequestData();
            //Console.Write(rqd.rpm);
            Console.ReadKey(true);
        }

        void RequestData()
        {
 // Defina a taxa de transmissão apropriada

            try
            {
                
                serialPort.Open();

                Console.WriteLine($"Conexão serial aberta na porta {portName}.");
                Console.ReadKey();
                while (serialPort.IsOpen)
                {
                    
                    if (sent && serialPort.IsOpen)
                    {
                        //Console.WriteLine(serialPort.ReadChar());
                        if (serialPort.Read(speedyResponse, 0, BYTES_TO_READ) > 0)
                        {
                           // int bytesRead = serialPort.Read(speedyResponse, 0, BYTES_TO_READ);
                            /*for (int i = 0; i < 125; i++)
                            {
                                int dataValue = serialPort.ReadByte();
                                Console.WriteLine($"Data Value {i + 1}: {dataValue}");
                            }*/



                            if (sent)
                            {
                                
                                for (int i = 0; i < 74; i++)
                                {
                                    int value = speedyResponse[i];
                                    Console.WriteLine(value.ToString());
                                }
                                ProcessData();
                                received = true;
                                ClearRX();
                            }
                            else
                            {
                                ProcessData();
                                received = true;
                                rps = (float)(1000.0 / (DateTimeOffset.Now.ToUnixTimeMilliseconds() - sendTimestamp));
                            }
                            
                            sent = false;
                        }
                        else
                        {
                            serialPort.ReadExisting();
                        }

                    }
                    else if (!sent)
                    {
                        //Console.WriteLine(mapData);
                        //Console.WriteLine("TESTE");

                        serialPort.Write("A");
                        sent = true;
                        sendTimestamp = (uint)DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    }
                   // else if (sent && DateTimeOffset.Now.ToUnixTimeMilliseconds() - sendTimestamp > SERIAL_TIMEOUT)
                   // {
                   //     sent = false;
                  //  }
                }

                // Aqui você pode enviar e receber dados pela porta serial

                // Feche a porta serial quando terminar
                serialPort.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao abrir a porta serial: {ex.Message}");
            }
        }
        void ClearRX()
        {
            while (serialPort.BytesToRead > 0)
            {
              serialPort.ReadExisting();
            }
        }
        void ProcessData()
        {
            engStatus = speedyResponse[31];
            rpm = (ushort)((speedyResponse[15] << 8) | speedyResponse[14]);
            afr = speedyResponse[10];
            mapData = ((int)(speedyResponse[5] << 8) | speedyResponse[4]);
            
            clt = speedyResponse[7];
            clt = (byte)(clt - 40);
            afrConv =(float)(afr / 10.0);
            iat = speedyResponse[6];
            iat = (byte)(iat - 40);
            tps = speedyResponse[24];
            tps = (int)(tps / 2.0);
            bat = speedyResponse[9];
            adv = speedyResponse[23];

            Console.WriteLine(mapData + "MAP");
            Console.WriteLine(tps + " TPS ");
        }

    }
}
