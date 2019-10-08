using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Globalization;

namespace PIDD_windows_form_Csharp
{
    public partial class PanelFondo : Form
    {   //al ejecutar el programa cargo la interface COM MLApp e inicializo variables globales y establezco período de muestre en 0,5seg
        MLApp.MLApp matlab = new MLApp.MLApp();
        Color c1 = Color.FromArgb(0, 80, 200);
        Color c2 = Color.FromArgb(26, 32, 40);
        bool encendido = false;
        bool modoauto = true;
        double manualov = 0;
        double ov = 0;
        double sp;
        double pv = 0;
        double error = 0;
        double erroranterior = 0;
        double erroranterioranterior = 0;
        double I = 0;
        double P = 0;
        double D = 0;
        double Ki = 0;
        double Kp = 0;
        double Kd = 0;
        double T = 0.5;
        

        public PanelFondo()
        {
            //al correr el programa abro el modelo en simulink ubicado en C:/matlabfunctions/pidtest.slx y seteo ov en 2
            InitializeComponent();
            MLApp.MLApp matlab = new MLApp.MLApp();
            matlab.Execute(@"cd C:\matlabfunctions");
            matlab.Execute("open_system('pidtest')");
            matlab.Execute("set_param('pidtest/Constant', 'Value', '2')");
            btnauto.BackColor = c1;
           
        }

        //Muestreo crea un hilo en background y espera 0,5 hasta finalizar su ejecución
        private static void Muestreo()
        {
            Thread.Sleep(500);
        }
        

        // Parseo cadena de caracteres para su posterior conversión a double
        private static string RemoveNonNumberDigitsAndCharacters(string text)
        {
            var numericChars = "0123456789,.+-eE".ToCharArray();
            string aux = text.Replace(".", ",");
            return new String(aux.Where(c => numericChars.Any(n => n == c)).ToArray());
        }
        private void Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Btncerrar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Btnmaximizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            Btnmaximizar.Visible = false;
            Btnrestaurar.Visible = true;
        }

        private void Btnrestaurar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            Btnrestaurar.Visible = false;
            Btnmaximizar.Visible = true;

        }

        private void Btnminimizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;

        }

        public void Chart2_Click(object sender, EventArgs e)
        {
            
        }

        private void Chart1_Click(object sender, EventArgs e)
        {
            this.chartoutput.Series["Series1"].Points.Clear();
        }

        

        private void Panel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private async void Button2_Click(object sender, EventArgs e)
        {
            //Ejecución del controlador
            double K1, K2,incr;
            double outputpid = 0;
            double outputpidanterior = 0;
            button2.BackColor = c1;
            button3.BackColor = c2;
            matlab.Execute("set_param('pidtest','SimulationCommand','start')");
            //continuo iterando mientras encendido =true
            encendido = true;
            while (encendido)
            {//si cambio encendido a false, termino ciclo
                if (!encendido){
                    break;
                }
                if (modoauto)
                {
                    //ejecución en modo automático. Calculo constantes de int y deriv, el error, el incremento y output   
                    K1 = Ki * T / 2;
                    K2 = Kd / T;
                    error = sp - pv;


                    P = Kp * (error-erroranterior);
                    I = K1 * (error + erroranterior);
                    D = K2 * (error - 2 * erroranterior + erroranterioranterior);

                    incr = P + I + D;
                    //guardo estados presentes que serán los estados previos de la próxima iteración
                    outputpid = outputpidanterior + incr;
                    //imprimo por pantallas resultados para realizar análisis o seguimiento.
                    Console.WriteLine("error = " + error);
                    Console.WriteLine("erroranterior = " + erroranterior);
                    Console.WriteLine("erroranterioranterior = " + erroranterioranterior);
                    Console.WriteLine("sp = " + sp);
                    Console.WriteLine("incr = " + incr);
                    Console.WriteLine("outputpidanterior = " + outputpidanterior);
                    Console.WriteLine("outputpid = " + outputpid);
                    outputpidanterior = outputpid;
                    
                    erroranterioranterior = erroranterior;
                    erroranterior = error;

                    ov = outputpid;
                    //envío ov a matlab
                    string j = ov.ToString();
                    string aux = j.Replace(",", ".");
                    string k = "set_param('pidtest/Constant', 'Value', '" +aux+ "')";
                    matlab.Execute(k);

                    //espero un período de muestreo
                    Task muestrear = new Task(Muestreo);
                    muestrear.Start();
                    await muestrear;
                    //valido si el flag encendido continúa en true, sino salgo del ciclo
                    if(!encendido){
                        break;
                    }

                    //las siguientes variables no tienen sentido técnico, solo están para realizar una asignación y así esperar a obtener el resultado de la operación en matlab para continuar procesando
                    //selecciono el objeto scope y obtengo el handler en modo runtimeobject para realizar posteriores lecturas
                    string tututut = matlab.Execute("set_param('pidtest/Scope','Selected','on')");
                    string tururur = matlab.Execute("blockhandler1=get_param(gcbh,'RuntimeObject')");
                    //leo el valor de entrada del scope que será PV y realizo tratamiento de cadena de caracteres para convertir pv en double
                    string handlervalue = matlab.Execute("blockhandler1.InputPort(1).Data");
                    Console.WriteLine("valor pv pre procesado :" +handlervalue);
                    string stringAfterChar = handlervalue.Substring(handlervalue.IndexOf("=") + 2);

                    string pvstring = RemoveNonNumberDigitsAndCharacters(stringAfterChar);
                    pv = Convert.ToDouble(pvstring);
                    Console.WriteLine("set OV -> = " + k);
                    Console.WriteLine("pv = " + pv);
                    Console.WriteLine("ov = " + ov);
                    
                    //grafico  sp vs pv
                    this.chartoutput.Series["Series1"].Points.AddXY("PV", pv);
                    this.chartoutput.Series["Series2"].Points.AddXY("SP", sp);
                    //grafico sp vs ov
                    this.chartSPPV.Series["Series1"].Points.Clear();
                    this.chartSPPV.Series["Series1"].Points.AddXY("SP", sp);
                    this.chartSPPV.Series["Series1"].Points.AddXY("OV", ov);
                }
                //si el modo es manual
                if (!modoauto) 
                {
                    //calculo el algoritmo pid, pero esta no será ov, ov será manualov
                    K1 = Ki * T / 2;
                    K2 = Kd / T;
                    error = sp - pv;


                    P = Kp * (error - erroranterior);
                    I = K1 * (error + erroranterior);
                    D = K2 * (error - 2 * erroranterior + erroranterioranterior);

                    incr = P + I + D;
                    outputpid = outputpidanterior + incr;
                    outputpidanterior = outputpid;

                    erroranterioranterior = erroranterior;
                    erroranterior = error;
                    //almaceno salida del controlador en ov
                    ov = outputpid;

                    //envío manualov al modelo
                    string j = manualov.ToString();
                    string aux = j.Replace(",", ".");
                    string k = "set_param('pidtest/Constant', 'Value', '" + aux + "')";
                    matlab.Execute(k);
                    //espero un período de muestreo
                    Task muestrear = new Task(Muestreo);
                    muestrear.Start(); await muestrear;
                    //valido si el flag encendido continúa en true, sino salgo del ciclo
                    if (!encendido){
                        break;
                    }
                    //las siguientes variables no tienen sentido técnico, solo están para realizar una asignación y así esperar a obtener el resultado de la operación en matlab para continuar procesando
                    //selecciono el objeto scope y obtengo el handler en modo runtimeobject para realizar posteriores lecturas

                    string tututut = matlab.Execute("set_param('pidtest/Scope','Selected','on')");
                    string tururur = matlab.Execute("blockhandler1=get_param(gcbh,'RuntimeObject')");
                    //leo el valor de entrada del scope que será PV y realizo tratamiento de cadena de caracteres para convertir pv en double

                    string handlervalue = matlab.Execute("blockhandler1.InputPort(1).Data");
                    Console.WriteLine("valor pv pre procesado :" + handlervalue);
                    string stringAfterChar = handlervalue.Substring(handlervalue.IndexOf("=") + 2);

                    string pvstring = RemoveNonNumberDigitsAndCharacters(stringAfterChar);
                    pv = Convert.ToDouble(pvstring);
                    Console.WriteLine("set OV -> = " + k);
                    Console.WriteLine("pv = " + pv);
                    Console.WriteLine("ov = " + ov);
                    Console.WriteLine("manual ov = " + manualov);
                    //grafico sp vs pv
                    this.chartoutput.Series["Series1"].Points.AddXY("PV", pv);
                    this.chartoutput.Series["Series2"].Points.AddXY("SP", sp);
                    //grafico sp vs ov
                    this.chartSPPV.Series["Series1"].Points.Clear();
                    this.chartSPPV.Series["Series1"].Points.AddXY("SP", sp);
                    this.chartSPPV.Series["Series1"].Points.AddXY("OV", manualov);
                }
            }

        }

        private void Button3_Click(object sender, EventArgs e)
        {   //detengo ejecución e inicializo parámetros
            button2.BackColor = c2;
            button3.BackColor = c1;
            encendido = false;
            matlab.Execute("set_param('pidtest','SimulationCommand','stop')");
            manualov = 0;
            ov = 0;
            pv = 0;
            error = 0;
            erroranterior = 0;
            erroranterioranterior = 0;
        }

        private void Btnauto_Click(object sender, EventArgs e)
        {
            btnauto.BackColor = c1;
            btnmanual.BackColor = c2;
            modoauto = true;

        }

        private void Btnmanual_Click(object sender, EventArgs e)
        {
            btnauto.BackColor = c2;
            btnmanual.BackColor = c1;
            modoauto = false;
        }

        private void Button5_Click(object sender, EventArgs e)
        {

        }

        private void TextBoxOV_TextChanged(object sender, EventArgs e)
        {
            manualov = Double.Parse(textBoxOV.Text);
        }

        private void TextBoxSP_KeyPress(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;
            if (ch == 6)
            {
                sp = Double.Parse(textBoxSP.Text);
            }
            if(!Char.IsDigit(ch) && ch != 8 && ch != 88 && ch != '.' && ch != ',')
            {
                e.Handled = true;
            }
        }

        private void TextBoxKP_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void TextBoxKI_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void TextBoxKD_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void TextBoxOV_KeyPress(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;
            if(!Char.IsDigit(ch) && ch != 8 && ch != 88 && ch != '.' && ch != ',')
            {
                e.Handled = true;
            }
        }

        private void TextBoxKP_KeyPress(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;


            if(!Char.IsDigit(ch) && ch != 8 && ch != 88 && ch != '.' && ch != ',')
            {
                e.Handled = true;
            }
        }

        private void TextBoxKI_KeyPress(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;
            if(!Char.IsDigit(ch) && ch != 8 && ch != 88 && ch != '.' && ch != ',')
            {
                e.Handled = true;
            }
        }

        private void TextBoxKD_KeyPress(object sender, KeyPressEventArgs e)
        {
            char ch = e.KeyChar;
            if (!Char.IsDigit(ch) && ch != 8 && ch != 88 && ch != '.' && ch != ',')
            {
                e.Handled = true;
            }
        }

        private void Btnsave_Click(object sender, EventArgs e)
        {
            sp = Double.Parse(textBoxSP.Text);
            manualov = Double.Parse(textBoxOV.Text);
            Kp = Double.Parse(textBoxKP.Text);
            Ki = Double.Parse(textBoxKI.Text);
            Kd = Double.Parse(textBoxKD.Text);
        }

        private void Btnovup_Click(object sender, EventArgs e)
        {
            manualov += 1;
        }

        private void Btnovdown_Click(object sender, EventArgs e)
        {
            manualov -= 1;
        }
    }
}
