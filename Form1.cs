using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using ChatServer;
using System.Net.Sockets;
using System.Text.Json;
using System.IO;



namespace Server
{
    public delegate void Delegate_message(string iText);
    public partial class Form1 : Form
    {
        private string hod;
        

        class s_data
        {
            public string cord_x { get; set; }
            public string cord_y { get; set; }
            public string locked { get; set; }
        }

        class msg
        {
            public string id { get; set; }
            public string mes { get; set; }
        }

        static ServerObject server; // сервер
        static Thread listenThread; // потока для прослушивания

        static string userName;
        private string host = "127.0.0.1";
        private const int port = 8888;
        static TcpClient client;
        static NetworkStream stream;


        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                server = new ServerObject();
                
                listenThread = new Thread(new ThreadStart(server.Listen));
                listenThread.IsBackground = true;                
                listenThread.Start(); //старт потока
            }
            catch (Exception ex)
            {
                server.Disconnect();               
                MessageBox.Show(ex.Message);
            }

            button1.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            userName = textBox1.Text;
            client = new TcpClient();

            if (textBox3.Text == "ip адрес")
            {
                host = "127.0.0.1";
            }
            else
            {
                host = textBox3.Text;
            }
            try
            {
                client.Connect(host, port); //подключение клиента
                stream = client.GetStream(); // получаем поток

                string message = userName;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);

                // запускаем новый поток для получения данных
                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.IsBackground = true;
                receiveThread.Start(); //старт потока
                listBox1.Items.Add("Добро пожаловать:" + userName);
                clear_inactive_desk(true);
                
                //SendMessage();
            }
            catch (Exception ex)
            {
                listBox1.Items.Add(ex.Message);
                MessageBox.Show(ex.Message);
            }
            finally
            {
              //  Disconnect();
            }
        }
        //отправка сообщения
        public void SendMessage(string msg)
        {
                string message = Console.ReadLine();
                byte[] data = Encoding.Unicode.GetBytes(msg);
                stream.Write(data, 0, data.Length);     
        }
        // получение сообщений
        public void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    Console.WriteLine(message);
                    BeginInvoke(new Delegate_message(JSON_parser_Receive), message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Disconnect();
                }
            }
        }

        static void Disconnect()
        {
            if (stream != null)
                stream.Close();//отключение потока
            if (client != null)
                client.Close();//отключение клиента
            Environment.Exit(0); //завершение процесса
        }


        public void JSON_parser_Receive(string messages_t)
        {
            var personObject = JsonSerializer.Deserialize<s_data>(messages_t);
            var mesobj = JsonSerializer.Deserialize<msg>(messages_t);
            if ((personObject.cord_x != null) & (personObject.cord_y != null))
            {
                listBox1.Items.Add("cord_x: " + personObject.cord_x + " cord_y: " + personObject.cord_y);
                xod_pro(personObject.cord_x, personObject.cord_y);
            }
            if (mesobj.id != null)
            {
                listBox1.Items.Add(mesobj.id + ": " + mesobj.mes);
                //SendMessage("{\"mesg\":[{\"id\":\"" + textBox1.Text + "\",\"mes\":\"" + textBox2.Text + "\"}]}");
            }
            if (personObject.locked != null)
            {     
               listBox1.Items.Add(personObject.locked);
                if (personObject.locked == "x")
                { 
                    pictureBox11.Image = Properties.Resources.x as Bitmap;
                    hod = "x";
                }
                if (personObject.locked == "0")
                {
                    pictureBox11.Image = Properties.Resources._0 as Bitmap;
                    hod = "0";
                }
            }
        }

        public void xod_pro(string pic, string ox)
        {      
                if (ox == "x")
                {
                    ((PictureBox)this.Controls[pic]).BackgroundImage = Properties.Resources.x as Bitmap;
                
                }
                if (ox == "0")
                {
                ((PictureBox)this.Controls[pic]).BackgroundImage = Properties.Resources._0 as Bitmap;
            }
            
        }

        private void button3_Click(object sender, EventArgs e) {
           // SendMessage(textBox2.Text);
            SendMessage("{\"id\":\"" + textBox1.Text + "\",\"mes\":\"" + textBox2.Text + "\"}");
        }
        //реакция движения мыши
        private void pictureBox2_MouseMove_1(object sender, MouseEventArgs e)
        {
            if (hod == "x") ((PictureBox)sender).Image = Properties.Resources.x as Bitmap;
            if (hod == "0") ((PictureBox)sender).Image = Properties.Resources._0 as Bitmap;

        }
        //реакция на покидание пикчабокса
        private void pictureBox2_MouseLeave_1(object sender, EventArgs e)
        {
            ((PictureBox)sender).Image = null;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (server == null) //проверка существования объекта
            {
                MessageBox.Show("только сервер может начать игру");
            }
            else
            {
                if (server.count() < 2)
                {
                    MessageBox.Show("Не подходящие число игроков: " + Convert.ToString(server.count()) + ". Необходимо 2а игрока.");
                }
                else {
                    //Создание объекта для генерации чисел
                    Random rnd = new Random();
                    //Получить очередное (в данном случае - первое) случайное число
                    int value = rnd.Next(0, 10);
                    if (value <= 5)
                    {
                        hod = "x";
                        pictureBox11.BackgroundImage = Properties.Resources.x as Bitmap;
                        SendMessage("{\"locked\":\"0\",\"id\":\"" + textBox1.Text + "\",\"mes\":\"Вы играете 0\"}");
                        
                    }
                    else {
                        hod = "0";
                        pictureBox11.BackgroundImage = Properties.Resources._0 as Bitmap;
                        SendMessage("{\"locked\":\"x\",\"id\":\"" + textBox1.Text + "\",\"mes\":\"Вы играете x\"}");
                    }
                }
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            pictureBox11.Image = Properties.Resources._0 as Bitmap;
            hod = "0";
        }
        //реакция моус клик всех пикчабоксов
        private void hod_select(object sender, MouseEventArgs e)
        {
            
            if (hod == "x") ((PictureBox)sender).BackgroundImage = Properties.Resources.x as Bitmap;
            if (hod == "0") ((PictureBox)sender).BackgroundImage = Properties.Resources._0 as Bitmap;
            ((PictureBox)sender).Enabled = false;
            if (hod == "x")
            {
                var jsonPerson = "{\"cord_x\":\"" + Convert.ToString(((PictureBox)sender).Name) + "\",\"cord_y\":\"" + hod + "\",\"mesg\":[{\"id\":\"\",\"mes\":\"\"}]}";
                SendMessage(jsonPerson);
                //hod = "0";
            } else
            {
                var jsonPerson = "{\"cord_x\":\"" + Convert.ToString(((PictureBox)sender).Name) + "\",\"cord_y\":\"" + hod + "\",\"mesg\":[{\"id\":\"\",\"mes\":\"\"}]}";
                SendMessage(jsonPerson);
               // hod = "x";
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            clear_inactive_desk(true);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            clear_inactive_desk(false);
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text == "Имя") {
                textBox1.Text = "";
                textBox1.ForeColor = Color.Black;
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (textBox1.Text == "") {
                textBox1.Text = "Имя";
                textBox1.ForeColor = Color.Silver;
            }
        }

        //отчистка доски + (блокировка/разблокировка) доски
        public void clear_inactive_desk(Boolean x)
        {
            foreach (PictureBox c in this.Controls.OfType<PictureBox>())
                if (c is PictureBox)
                {
                    c.Enabled = x;
                    c.BackgroundImage = Properties.Resources.trans as Bitmap;
                    c.Image = Properties.Resources.trans as Bitmap;
                }
            pictureBox1.BackgroundImage = Properties.Resources.desk1 as Bitmap;
        }
    }
}

